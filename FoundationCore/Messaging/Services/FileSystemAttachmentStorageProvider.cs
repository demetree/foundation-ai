using System;
using System.IO;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    ///
    /// File-system-based implementation of IAttachmentStorageProvider.
    ///
    /// Stores attachments as files on disk under a configurable root directory, organized by:
    ///     {rootPath}/{tenantGuid}/{attachmentGuid}/{originalFileName}
    ///
    /// Thread-safe for concurrent uploads — each attachment gets a unique GUID directory,
    /// so there are no collisions even for files with the same name.
    ///
    /// AI-developed as part of Foundation.Messaging refactoring, March 2026.
    ///
    /// </summary>
    public class FileSystemAttachmentStorageProvider : IAttachmentStorageProvider
    {
        private readonly string _rootPath;


        /// <summary>
        /// Creates a new file system storage provider.
        /// </summary>
        /// <param name="rootPath">
        /// The root directory under which all attachments will be stored.
        /// This directory will be created if it does not exist.
        /// </param>
        public FileSystemAttachmentStorageProvider(string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath) == true)
            {
                throw new ArgumentNullException(nameof(rootPath), "Root path for attachment storage is required.");
            }

            _rootPath = rootPath;

            //
            // Ensure the root directory exists.
            //
            if (Directory.Exists(_rootPath) == false)
            {
                Directory.CreateDirectory(_rootPath);
            }
        }


        /// <summary>
        /// Stores a file to disk under {rootPath}/{tenantGuid}/{attachmentGuid}/{fileName}.
        /// </summary>
        public async Task<AttachmentStorageResult> StoreAsync(Guid tenantGuid, string fileName, string mimeType, Stream content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Guid attachmentGuid = Guid.NewGuid();

            string tenantDirectory = Path.Combine(_rootPath, tenantGuid.ToString());
            string attachmentDirectory = Path.Combine(tenantDirectory, attachmentGuid.ToString());

            //
            // Sanitize the file name to prevent path traversal attacks.
            //
            string sanitizedFileName = Path.GetFileName(fileName);

            if (string.IsNullOrWhiteSpace(sanitizedFileName) == true)
            {
                sanitizedFileName = "attachment";
            }

            string filePath = Path.Combine(attachmentDirectory, sanitizedFileName);

            //
            // Create the directory structure if it doesn't exist.
            //
            Directory.CreateDirectory(attachmentDirectory);


            //
            // Write the content to disk.
            //
            long contentSize = 0;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true))
            {
                await content.CopyToAsync(fileStream);
                contentSize = fileStream.Length;
            }


            return new AttachmentStorageResult
            {
                storageGuid = attachmentGuid,
                contentSize = contentSize,
                storagePath = filePath,
            };
        }


        /// <summary>
        /// Retrieves a file from disk and returns it as a readable stream.
        /// Returns null if the attachment directory or file does not exist.
        /// </summary>
        public Task<Stream> RetrieveAsync(Guid tenantGuid, Guid attachmentGuid)
        {
            string attachmentDirectory = Path.Combine(_rootPath, tenantGuid.ToString(), attachmentGuid.ToString());

            if (Directory.Exists(attachmentDirectory) == false)
            {
                return Task.FromResult<Stream>(null);
            }

            //
            // The attachment directory contains exactly one file — the original file.
            //
            string[] files = Directory.GetFiles(attachmentDirectory);

            if (files.Length == 0)
            {
                return Task.FromResult<Stream>(null);
            }

            Stream stream = new FileStream(files[0], FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);

            return Task.FromResult(stream);
        }


        /// <summary>
        /// Deletes an attachment by removing its directory (and the file within it).
        /// </summary>
        public Task<bool> DeleteAsync(Guid tenantGuid, Guid attachmentGuid)
        {
            string attachmentDirectory = Path.Combine(_rootPath, tenantGuid.ToString(), attachmentGuid.ToString());

            if (Directory.Exists(attachmentDirectory) == false)
            {
                return Task.FromResult(false);
            }

            Directory.Delete(attachmentDirectory, recursive: true);

            return Task.FromResult(true);
        }


        /// <summary>
        /// Checks whether an attachment exists on disk.
        /// </summary>
        public Task<bool> ExistsAsync(Guid tenantGuid, Guid attachmentGuid)
        {
            string attachmentDirectory = Path.Combine(_rootPath, tenantGuid.ToString(), attachmentGuid.ToString());

            if (Directory.Exists(attachmentDirectory) == false)
            {
                return Task.FromResult(false);
            }

            string[] files = Directory.GetFiles(attachmentDirectory);

            return Task.FromResult(files.Length > 0);
        }
    }
}
