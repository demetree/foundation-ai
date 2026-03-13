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
            sourceMoc.forkCount = (sourceMoc.forkCount ?? 0) + 1;

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

        #endregion
    }
}
