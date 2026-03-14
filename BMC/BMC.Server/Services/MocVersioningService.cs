using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Foundation.BMC.Database;

namespace Foundation.BMC.Services
{
    /// <summary>
    ///
    /// MOCHub versioning service — core logic for version control, diffing, and forking.
    ///
    /// Provides:
    ///   - CreateSnapshotAsync: Generates an MPD snapshot from the project's current state
    ///     and stores it as a MocVersion row (a "commit").
    ///   - ComputeDiffSummaryAsync: Compares two version snapshots and returns a structured
    ///     change summary (added/removed/modified bricks).
    ///   - ForkMocAsync: Clones a project and its MOC listing, creating new PublishedMoc +
    ///     MocFork records + initial MocVersion snapshot.
    ///   - GenerateSlug: Creates a URL-friendly slug from a MOC name.
    ///
    /// AI-developed code — initial implementation March 2026
    ///
    /// </summary>
    public class MocVersioningService
    {
        private readonly BMCContext _context;
        private readonly ModelExportService _exportService;
        private readonly ILogger<MocVersioningService> _logger;


        public MocVersioningService(
            BMCContext context,
            ModelExportService exportService,
            ILogger<MocVersioningService> logger)
        {
            _context = context;
            _exportService = exportService;
            _logger = logger;
        }


        #region DTOs

        /// <summary>
        /// Result of a version snapshot creation.
        /// </summary>
        public class SnapshotResult
        {
            public int MocVersionId { get; set; }
            public int VersionNumber { get; set; }
            public int? PartCount { get; set; }
            public int? AddedParts { get; set; }
            public int? RemovedParts { get; set; }
            public int? ModifiedParts { get; set; }
        }


        /// <summary>
        /// Summary of differences between two versions.
        /// </summary>
        public class DiffSummary
        {
            public int FromVersion { get; set; }
            public int ToVersion { get; set; }
            public int AddedCount { get; set; }
            public int RemovedCount { get; set; }
            public int ModifiedCount { get; set; }
            public List<DiffEntry> Entries { get; set; } = new List<DiffEntry>();
        }


        /// <summary>
        /// A single entry in the diff — one brick added, removed, or modified.
        /// </summary>
        public class DiffEntry
        {
            public string ChangeType { get; set; }     // "Added", "Removed", "Modified"
            public string PartId { get; set; }          // LDraw part ID
            public int ColourCode { get; set; }
            public string Position { get; set; }        // "x y z" formatted position
            public string Details { get; set; }         // Human-readable description
        }


        /// <summary>
        /// Result of a fork operation.
        /// </summary>
        public class ForkResult
        {
            public int NewProjectId { get; set; }
            public int NewPublishedMocId { get; set; }
            public int MocForkId { get; set; }
            public string Slug { get; set; }
        }

        #endregion


        /// <summary>
        ///
        /// Create a version snapshot (commit) of the published MOC's current state.
        ///
        /// Generates the MPD text via ModelExportService, calculates part count deltas
        /// against the previous version, and stores as a new MocVersion row.
        ///
        /// </summary>
        public async Task<SnapshotResult> CreateSnapshotAsync(
            int publishedMocId,
            string commitMessage,
            Guid authorTenantGuid,
            CancellationToken cancellationToken = default)
        {
            //
            // Load the published MOC to get the project ID
            //
            PublishedMoc moc = await _context.PublishedMocs
                .Where(pm => pm.id == publishedMocId && pm.active == true && pm.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (moc == null)
            {
                throw new InvalidOperationException($"Published MOC {publishedMocId} not found.");
            }

            //
            // Generate the MPD snapshot from the current project state
            //
            string mpdSnapshot = await _exportService.GenerateViewerMpdAsync(
                moc.projectId,
                moc.tenantGuid,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(mpdSnapshot))
            {
                throw new InvalidOperationException("Project has no data to snapshot.");
            }

            //
            // Determine the next version number
            //
            int nextVersion = await _context.MocVersions
                .Where(mv => mv.publishedMocId == publishedMocId && mv.active == true)
                .MaxAsync(mv => (int?)mv.versionNumber, cancellationToken) ?? 0;
            nextVersion++;

            //
            // Count parts in the current snapshot (count LDraw type 1 lines)
            //
            int currentPartCount = CountPartsInMpd(mpdSnapshot);

            //
            // Calculate deltas against the previous version
            //
            int? addedParts = null;
            int? removedParts = null;
            int? modifiedParts = null;

            if (nextVersion > 1)
            {
                MocVersion previousVersion = await _context.MocVersions
                    .Where(mv => mv.publishedMocId == publishedMocId
                              && mv.versionNumber == nextVersion - 1
                              && mv.active == true)
                    .FirstOrDefaultAsync(cancellationToken);

                if (previousVersion != null)
                {
                    DiffSummary diff = ComputeDiff(previousVersion.mpdSnapshot, mpdSnapshot);
                    addedParts = diff.AddedCount;
                    removedParts = diff.RemovedCount;
                    modifiedParts = diff.ModifiedCount;
                }
            }

            //
            // Create the version record
            //
            MocVersion version = new MocVersion
            {
                publishedMocId = publishedMocId,
                tenantGuid = moc.tenantGuid,
                versionNumber = nextVersion,
                commitMessage = commitMessage,
                mpdSnapshot = mpdSnapshot,
                partCount = currentPartCount,
                addedPartCount = addedParts,
                removedPartCount = removedParts,
                modifiedPartCount = modifiedParts,
                snapshotDate = DateTime.UtcNow,
                authorTenantGuid = authorTenantGuid,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.MocVersions.Add(version);

            //
            // Update the MOC's cached part count
            //
            moc.partCount = currentPartCount;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "MOCHub: Created version {Version} for MOC {MocId} — {PartCount} parts",
                nextVersion, publishedMocId, currentPartCount);

            return new SnapshotResult
            {
                MocVersionId = version.id,
                VersionNumber = nextVersion,
                PartCount = currentPartCount,
                AddedParts = addedParts,
                RemovedParts = removedParts,
                ModifiedParts = modifiedParts
            };
        }


        /// <summary>
        ///
        /// Compute a diff summary between two version snapshots.
        ///
        /// Parses the LDraw type 1 lines from each snapshot and compares them.
        /// A "part" is identified by its LDraw type 1 line (colour, position, rotation, part ID).
        ///
        /// </summary>
        public async Task<DiffSummary> ComputeDiffSummaryAsync(
            int publishedMocId,
            int fromVersionNumber,
            int toVersionNumber,
            CancellationToken cancellationToken = default)
        {
            MocVersion fromVersion = await _context.MocVersions
                .Where(mv => mv.publishedMocId == publishedMocId && mv.versionNumber == fromVersionNumber && mv.active == true)
                .FirstOrDefaultAsync(cancellationToken);

            MocVersion toVersion = await _context.MocVersions
                .Where(mv => mv.publishedMocId == publishedMocId && mv.versionNumber == toVersionNumber && mv.active == true)
                .FirstOrDefaultAsync(cancellationToken);

            if (fromVersion == null || toVersion == null)
            {
                throw new InvalidOperationException("One or both versions not found.");
            }

            return ComputeDiff(fromVersion.mpdSnapshot, toVersion.mpdSnapshot);
        }


        /// <summary>
        ///
        /// Fork a published MOC into the current user's account.
        ///
        /// Creates a new Project (deep clone of the source), a new PublishedMoc record,
        /// a MocFork lineage record, and an initial MocVersion snapshot.
        /// Also increments the source MOC's fork count.
        ///
        /// </summary>
        public async Task<ForkResult> ForkMocAsync(
            int sourceMocId,
            Guid targetTenantGuid,
            CancellationToken cancellationToken = default)
        {
            //
            // Load the source MOC and verify it allows forking
            //
            PublishedMoc sourceMoc = await _context.PublishedMocs
                .Where(pm => pm.id == sourceMocId && pm.active == true && pm.deleted == false)
                .Include(pm => pm.project)
                .FirstOrDefaultAsync(cancellationToken);

            if (sourceMoc == null)
            {
                throw new InvalidOperationException($"Source MOC {sourceMocId} not found.");
            }

            if (sourceMoc.allowForking == false)
            {
                throw new InvalidOperationException("This MOC does not allow forking.");
            }

            //
            // Clone the project
            //
            Project sourceProject = sourceMoc.project;

            Project newProject = new Project
            {
                tenantGuid = targetTenantGuid,
                name = sourceProject.name + " (Fork)",
                description = sourceProject.description,
                partCount = sourceProject.partCount,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync(cancellationToken);

            //
            // Clone placed bricks
            //
            List<PlacedBrick> sourceBricks = await _context.PlacedBricks
                .Where(pb => pb.projectId == sourceProject.id
                          && pb.tenantGuid == sourceMoc.tenantGuid
                          && pb.active == true
                          && pb.deleted == false)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (PlacedBrick brick in sourceBricks)
            {
                PlacedBrick clone = new PlacedBrick
                {
                    projectId = newProject.id,
                    tenantGuid = targetTenantGuid,
                    brickPartId = brick.brickPartId,
                    brickColourId = brick.brickColourId,
                    positionX = brick.positionX,
                    positionY = brick.positionY,
                    positionZ = brick.positionZ,
                    rotationX = brick.rotationX,
                    rotationY = brick.rotationY,
                    rotationZ = brick.rotationZ,
                    rotationW = brick.rotationW,
                    buildStepNumber = brick.buildStepNumber,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.PlacedBricks.Add(clone);
            }

            //
            // Clone the ModelDocument (source file data) if present
            //
            ModelDocument sourceDoc = await _context.ModelDocuments
                .Where(md => md.projectId == sourceProject.id
                          && md.tenantGuid == sourceMoc.tenantGuid
                          && md.active == true
                          && md.deleted == false)
                .OrderByDescending(md => md.id)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (sourceDoc != null)
            {
                ModelDocument cloneDoc = new ModelDocument
                {
                    projectId = newProject.id,
                    tenantGuid = targetTenantGuid,
                    sourceFormat = sourceDoc.sourceFormat,
                    sourceFileName = sourceDoc.sourceFileName,
                    sourceFileData = sourceDoc.sourceFileData,
                    studioVersion = sourceDoc.studioVersion,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.ModelDocuments.Add(cloneDoc);
            }

            await _context.SaveChangesAsync(cancellationToken);

            //
            // Clone Build Manuals
            //
            List<BuildManual> sourceManuals = await _context.BuildManuals
                .Where(bm => bm.projectId == sourceProject.id
                          && bm.tenantGuid == sourceMoc.tenantGuid
                          && bm.active == true
                          && bm.deleted == false)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (BuildManual manual in sourceManuals)
            {
                BuildManual clonedManual = new BuildManual
                {
                    projectId = newProject.id,
                    tenantGuid = targetTenantGuid,
                    name = manual.name,
                    description = manual.description,
                    pageWidthMm = manual.pageWidthMm,
                    pageHeightMm = manual.pageHeightMm,
                    isPublished = manual.isPublished,
                    objectGuid = Guid.NewGuid(),
                    versionNumber = 1,
                    active = true,
                    deleted = false
                };

                _context.BuildManuals.Add(clonedManual);
                await _context.SaveChangesAsync(cancellationToken);

                // Clone Pages for this manual
                List<BuildManualPage> sourcePages = await _context.BuildManualPages
                    .Where(bmp => bmp.buildManualId == manual.id
                               && bmp.tenantGuid == sourceMoc.tenantGuid
                               && bmp.active == true
                               && bmp.deleted == false)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                foreach (BuildManualPage page in sourcePages)
                {
                    BuildManualPage clonedPage = new BuildManualPage
                    {
                        buildManualId = clonedManual.id,
                        tenantGuid = targetTenantGuid,
                        pageNum = page.pageNum,
                        title = page.title,
                        notes = page.notes,
                        backgroundTheme = page.backgroundTheme,
                        layoutPreset = page.layoutPreset,
                        objectGuid = Guid.NewGuid(),
                        versionNumber = 1,
                        active = true,
                        deleted = false
                    };

                    _context.BuildManualPages.Add(clonedPage);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Clone Steps for this page
                    List<BuildManualStep> sourceSteps = await _context.BuildManualSteps
                        .Where(bms => bms.buildManualPageId == page.id
                                   && bms.tenantGuid == sourceMoc.tenantGuid
                                   && bms.active == true
                                   && bms.deleted == false)
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);

                    foreach (BuildManualStep step in sourceSteps)
                    {
                        BuildManualStep clonedStep = new BuildManualStep
                        {
                            buildManualPageId = clonedPage.id,
                            tenantGuid = targetTenantGuid,
                            stepNumber = step.stepNumber,
                            cameraPositionX = step.cameraPositionX,
                            cameraPositionY = step.cameraPositionY,
                            cameraPositionZ = step.cameraPositionZ,
                            cameraTargetX = step.cameraTargetX,
                            cameraTargetY = step.cameraTargetY,
                            cameraTargetZ = step.cameraTargetZ,
                            cameraZoom = step.cameraZoom,
                            showExplodedView = step.showExplodedView,
                            explodedDistance = step.explodedDistance,
                            renderImagePath = step.renderImagePath,
                            pliImagePath = step.pliImagePath,
                            fadeStepEnabled = step.fadeStepEnabled,
                            objectGuid = Guid.NewGuid(),
                            versionNumber = 1,
                            active = true,
                            deleted = false
                        };

                        _context.BuildManualSteps.Add(clonedStep);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            //
            // Create the new PublishedMoc (the fork)
            //
            string slug = GenerateSlug(sourceMoc.name);

            // Ensure slug uniqueness within the target tenant
            string baseSlug = slug;
            int slugSuffix = 1;
            while (await _context.PublishedMocs.AnyAsync(
                pm => pm.tenantGuid == targetTenantGuid && pm.slug == slug && pm.active == true,
                cancellationToken))
            {
                slug = $"{baseSlug}-{slugSuffix}";
                slugSuffix++;
            }

            PublishedMoc forkedMoc = new PublishedMoc
            {
                projectId = newProject.id,
                tenantGuid = targetTenantGuid,
                name = sourceMoc.name,
                description = sourceMoc.description,
                tags = sourceMoc.tags,
                visibility = "Public",
                allowForking = true,
                forkedFromMocId = sourceMocId,
                licenseName = sourceMoc.licenseName,
                readmeMarkdown = sourceMoc.readmeMarkdown,
                slug = slug,
                isPublished = true,
                publishedDate = DateTime.UtcNow,
                partCount = sourceMoc.partCount,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.PublishedMocs.Add(forkedMoc);
            await _context.SaveChangesAsync(cancellationToken);

            //
            // Get the latest version of the source for fork lineage
            //
            MocVersion latestSourceVersion = await _context.MocVersions
                .Where(mv => mv.publishedMocId == sourceMocId && mv.active == true)
                .OrderByDescending(mv => mv.versionNumber)
                .FirstOrDefaultAsync(cancellationToken);

            //
            // Create the fork lineage record
            //
            MocFork fork = new MocFork
            {
                forkedMocId = forkedMoc.id,
                sourceMocId = sourceMocId,
                mocVersionId = latestSourceVersion?.id,
                forkerTenantGuid = targetTenantGuid,
                forkedDate = DateTime.UtcNow,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.MocForks.Add(fork);

            //
            // Increment the source MOC's fork count
            //
            sourceMoc.forkCount = sourceMoc.forkCount + 1;

            await _context.SaveChangesAsync(cancellationToken);

            //
            // Create an initial version snapshot for the fork
            //
            SnapshotResult snapshot = await CreateSnapshotAsync(
                forkedMoc.id,
                $"Initial fork from {sourceMoc.name}",
                targetTenantGuid,
                cancellationToken);

            _logger.LogInformation(
                "MOCHub: Forked MOC {SourceId} → {ForkId} (project {ProjectId}), slug='{Slug}'",
                sourceMocId, forkedMoc.id, newProject.id, slug);

            return new ForkResult
            {
                NewProjectId = newProject.id,
                NewPublishedMocId = forkedMoc.id,
                MocForkId = fork.id,
                Slug = slug
            };
        }


        /// <summary>
        ///
        /// Get the raw MPD snapshot text for a specific version.
        ///
        /// </summary>
        public async Task<string> GetVersionMpdAsync(
            int publishedMocId,
            int versionNumber,
            CancellationToken cancellationToken = default)
        {
            string mpdSnapshot = await _context.MocVersions
                .Where(mv => mv.publishedMocId == publishedMocId
                          && mv.versionNumber == versionNumber
                          && mv.active == true)
                .Select(mv => mv.mpdSnapshot)
                .FirstOrDefaultAsync(cancellationToken);

            return mpdSnapshot;
        }


        /// <summary>
        ///
        /// Generate a diff MPD — a single LDraw MPD file that visualises the differences
        /// between two version snapshots.
        ///
        /// How it works:
        ///   1. Parse type 1 lines (brick placements) from both snapshots
        ///   2. Classify each line as Added, Removed, or Unchanged
        ///   3. Emit a unified MPD where:
        ///       - Unchanged bricks keep their original LDraw colour code
        ///       - Added bricks are recoloured to LDraw colour 2 (bright green)
        ///       - Removed bricks are recoloured to LDraw colour 36 (translucent red)
        ///   4. Merge submodel definitions from both snapshots so all geometry resolves
        ///
        /// The output can be loaded directly by LDrawLoader to render a 3D diff view.
        ///
        /// </summary>
        public async Task<string> GenerateDiffMpdAsync(
            int publishedMocId,
            int fromVersionNumber,
            int toVersionNumber,
            CancellationToken cancellationToken = default)
        {
            //
            // Constants for diff highlight colours
            //
            const string COLOUR_ADDED = "2";         // Bright green
            const string COLOUR_REMOVED = "36";       // Translucent red

            //
            // Load both version snapshots
            //
            MocVersion fromVersion = await _context.MocVersions
                .Where(mv => mv.publishedMocId == publishedMocId
                          && mv.versionNumber == fromVersionNumber
                          && mv.active == true)
                .FirstOrDefaultAsync(cancellationToken);

            MocVersion toVersion = await _context.MocVersions
                .Where(mv => mv.publishedMocId == publishedMocId
                          && mv.versionNumber == toVersionNumber
                          && mv.active == true)
                .FirstOrDefaultAsync(cancellationToken);

            if (fromVersion == null || toVersion == null)
            {
                throw new InvalidOperationException("One or both versions not found.");
            }

            string fromMpd = fromVersion.mpdSnapshot ?? "";
            string toMpd = toVersion.mpdSnapshot ?? "";

            //
            // Extract type 1 lines (brick placements) from both snapshots.
            // A type 1 line has the format:
            //   1 <colour> <x> <y> <z> <a> <b> <c> <d> <e> <f> <g> <h> <i> <part>
            //
            HashSet<string> fromLines = ExtractPartLines(fromMpd);
            HashSet<string> toLines = ExtractPartLines(toMpd);

            //
            // Classify each line
            //
            List<string> unchangedLines = new List<string>();
            List<string> addedLines = new List<string>();
            List<string> removedLines = new List<string>();

            //
            // Lines in both snapshots → unchanged (keep original colour)
            // Lines only in 'to' → added (recolour to green)
            // Lines only in 'from' → removed (recolour to red)
            //
            foreach (string line in toLines)
            {
                if (fromLines.Contains(line))
                {
                    unchangedLines.Add(line);
                }
                else
                {
                    addedLines.Add(ReplacePartLineColour(line, COLOUR_ADDED));
                }
            }

            foreach (string line in fromLines)
            {
                if (toLines.Contains(line) == false)
                {
                    removedLines.Add(ReplacePartLineColour(line, COLOUR_REMOVED));
                }
            }

            //
            // Merge submodel definitions from both snapshots so that all part
            // geometry referenced by type 1 lines can resolve.  If both snapshots
            // define the same submodel, the 'to' version takes precedence.
            //
            Dictionary<string, string> fromSubmodels = ExtractSubmodels(fromMpd);
            Dictionary<string, string> toSubmodels = ExtractSubmodels(toMpd);

            Dictionary<string, string> mergedSubmodels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, string> kvp in fromSubmodels)
            {
                mergedSubmodels[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<string, string> kvp in toSubmodels)
            {
                //
                // 'to' version takes precedence over 'from'
                //
                mergedSubmodels[kvp.Key] = kvp.Value;
            }

            //
            // Build the unified diff MPD
            //
            StringBuilder diffMpd = new StringBuilder();

            //
            // Header
            //
            diffMpd.AppendLine("0 FILE DiffView.ldr");
            diffMpd.AppendLine("0 Diff visualisation — green = added, red = removed");
            diffMpd.AppendLine($"0 Comparing v{fromVersionNumber} → v{toVersionNumber}");
            diffMpd.AppendLine();

            //
            // Unchanged bricks first (original colours)
            //
            if (unchangedLines.Count > 0)
            {
                diffMpd.AppendLine("0 // Unchanged bricks");

                foreach (string line in unchangedLines)
                {
                    diffMpd.AppendLine(line);
                }

                diffMpd.AppendLine();
            }

            //
            // Added bricks (green)
            //
            if (addedLines.Count > 0)
            {
                diffMpd.AppendLine("0 // Added bricks (green)");

                foreach (string line in addedLines)
                {
                    diffMpd.AppendLine(line);
                }

                diffMpd.AppendLine();
            }

            //
            // Removed bricks (translucent red)
            //
            if (removedLines.Count > 0)
            {
                diffMpd.AppendLine("0 // Removed bricks (translucent red)");

                foreach (string line in removedLines)
                {
                    diffMpd.AppendLine(line);
                }

                diffMpd.AppendLine();
            }

            diffMpd.AppendLine("0 NOFILE");

            //
            // Append all merged submodel definitions so geometry resolves
            //
            foreach (KeyValuePair<string, string> kvp in mergedSubmodels)
            {
                diffMpd.AppendLine($"0 FILE {kvp.Key}");
                diffMpd.AppendLine(kvp.Value);
                diffMpd.AppendLine("0 NOFILE");
            }

            _logger.LogInformation(
                "MOCHub: Generated diff MPD for MOC {MocId} v{From}→v{To}: {Unchanged} unchanged, {Added} added, {Removed} removed",
                publishedMocId, fromVersionNumber, toVersionNumber,
                unchangedLines.Count, addedLines.Count, removedLines.Count);

            string result = diffMpd.ToString();

            return result;
        }


        #region Helpers


        /// <summary>
        /// Generate a URL-friendly slug from a MOC name.
        /// Converts to lowercase, replaces spaces with hyphens, removes special characters.
        /// </summary>
        public static string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "untitled-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            string slug = name.ToLowerInvariant();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            if (string.IsNullOrEmpty(slug))
            {
                slug = "untitled-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            // Truncate to 90 chars to leave room for uniqueness suffixes
            if (slug.Length > 90)
            {
                slug = slug.Substring(0, 90).TrimEnd('-');
            }

            return slug;
        }


        /// <summary>
        /// Count the number of LDraw type 1 (part reference) lines in an MPD string.
        /// Type 1 lines start with "1 " and represent placed parts.
        /// </summary>
        private static int CountPartsInMpd(string mpd)
        {
            if (string.IsNullOrEmpty(mpd)) return 0;

            int count = 0;
            foreach (string line in mpd.Split('\n'))
            {
                string trimmed = line.TrimStart();
                if (trimmed.StartsWith("1 "))
                {
                    count++;
                }
            }
            return count;
        }


        /// <summary>
        /// Compute a diff between two MPD snapshots by comparing LDraw type 1 lines.
        /// </summary>
        private static DiffSummary ComputeDiff(string fromMpd, string toMpd)
        {
            HashSet<string> fromLines = ExtractPartLines(fromMpd);
            HashSet<string> toLines = ExtractPartLines(toMpd);

            List<DiffEntry> entries = new List<DiffEntry>();

            // Added: in 'to' but not in 'from'
            foreach (string line in toLines)
            {
                if (!fromLines.Contains(line))
                {
                    entries.Add(ParseDiffEntry("Added", line));
                }
            }

            // Removed: in 'from' but not in 'to'
            foreach (string line in fromLines)
            {
                if (!toLines.Contains(line))
                {
                    entries.Add(ParseDiffEntry("Removed", line));
                }
            }

            return new DiffSummary
            {
                AddedCount = entries.Count(e => e.ChangeType == "Added"),
                RemovedCount = entries.Count(e => e.ChangeType == "Removed"),
                ModifiedCount = 0,  // Future: detect moved/recoloured bricks
                Entries = entries
            };
        }


        /// <summary>
        /// Extract all LDraw type 1 lines from an MPD string.
        /// </summary>
        private static HashSet<string> ExtractPartLines(string mpd)
        {
            HashSet<string> lines = new HashSet<string>();
            if (string.IsNullOrEmpty(mpd)) return lines;

            foreach (string line in mpd.Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("1 "))
                {
                    lines.Add(trimmed);
                }
            }
            return lines;
        }


        /// <summary>
        /// Parse a type 1 LDraw line into a DiffEntry.
        /// Format: 1 <colour> <x> <y> <z> <a> <b> <c> <d> <e> <f> <g> <h> <i> <part>
        /// </summary>
        private static DiffEntry ParseDiffEntry(string changeType, string line)
        {
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string partId = parts.Length > 14 ? parts[14] : parts.Length > 1 ? parts[^1] : "unknown";
            int colourCode = 0;
            string position = "0 0 0";

            if (parts.Length > 4)
            {
                int.TryParse(parts[1], out colourCode);
                position = $"{parts[2]} {parts[3]} {parts[4]}";
            }

            return new DiffEntry
            {
                ChangeType = changeType,
                PartId = partId,
                ColourCode = colourCode,
                Position = position,
                Details = $"{changeType} {partId} (colour {colourCode}) at {position}"
            };
        }


        /// <summary>
        ///
        /// Extract all submodel file blocks from an MPD string.
        ///
        /// LDraw MPD files use "0 FILE filename" / "0 NOFILE" markers to delimit
        /// embedded submodel definitions.  This method extracts them into a dictionary
        /// keyed by the filename (case-insensitive) so they can be merged across
        /// version snapshots.
        ///
        /// Skips the first file block (the root model) since we rebuild that in the diff MPD.
        ///
        /// </summary>
        private static Dictionary<string, string> ExtractSubmodels(string mpd)
        {
            Dictionary<string, string> submodels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(mpd))
            {
                return submodels;
            }

            string[] lines = mpd.Split('\n');
            string currentFileName = null;
            List<string> currentContent = new List<string>();
            bool isFirstFile = true;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                if (trimmed.StartsWith("0 FILE "))
                {
                    //
                    // Save the previous file block (if any) — skip the root model
                    //
                    if (currentFileName != null && isFirstFile == false)
                    {
                        submodels[currentFileName] = string.Join("\n", currentContent);
                    }

                    if (currentFileName != null && isFirstFile == true)
                    {
                        isFirstFile = false;
                    }

                    currentFileName = trimmed.Substring(7).Trim();
                    currentContent = new List<string>();
                    continue;
                }

                if (trimmed == "0 NOFILE")
                {
                    if (currentFileName != null && isFirstFile == false)
                    {
                        submodels[currentFileName] = string.Join("\n", currentContent);
                    }

                    if (isFirstFile == true)
                    {
                        isFirstFile = false;
                    }

                    currentFileName = null;
                    currentContent = new List<string>();
                    continue;
                }

                if (currentFileName != null)
                {
                    currentContent.Add(line.TrimEnd('\r'));
                }
            }

            //
            // Save the last file block if it wasn't closed with NOFILE
            //
            if (currentFileName != null && isFirstFile == false)
            {
                submodels[currentFileName] = string.Join("\n", currentContent);
            }

            return submodels;
        }


        /// <summary>
        ///
        /// Replace the colour code in a type 1 LDraw line.
        ///
        /// Type 1 format: 1 <colour> <x> <y> <z> <a> <b> <c> <d> <e> <f> <g> <h> <i> <part>
        /// This swaps the second token (colour) with the given replacement colour code.
        ///
        /// </summary>
        private static string ReplacePartLineColour(string line, string newColour)
        {
            string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 3)
            {
                return line;
            }

            //
            // Token[0] = "1", Token[1] = colour code — replace it
            //
            tokens[1] = newColour;

            string result = string.Join(" ", tokens);

            return result;
        }

        #endregion
    }
}
