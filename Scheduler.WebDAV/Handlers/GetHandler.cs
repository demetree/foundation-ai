//
// GetHandler.cs
//
// Handles GET requests — downloads a file's binary content.
//
// When a browser navigates to the root or a folder, returns a clean
// informative HTML page instead of a confusing 405 or XML dump.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation.Scheduler.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Server.Services;
using Scheduler.WebDAV.Services;

namespace Scheduler.WebDAV.Handlers
{
    public static class GetHandler
    {
        public static async Task HandleAsync(HttpContext context, WebDavContext davContext)
        {
            IFileStorageService fileService = context.RequestServices.GetRequiredService<IFileStorageService>();

            string path = context.Request.Path.Value ?? "/";

            //
            // Resolve path to folder + document name
            //
            PathResolver.ResolvedPath resolved = await PathResolver.ResolveAsync(
                path, davContext.TenantGuid, fileService, context.RequestAborted);


            //
            // If it resolved to a collection (folder or root), check if a browser is asking.
            // Browsers send "Accept: text/html" — WebDAV clients do not.
            //
            if (resolved.IsCollection && resolved.DocumentName == null)
            {
                string acceptHeader = context.Request.Headers["Accept"].FirstOrDefault() ?? "";

                if (acceptHeader.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                {
                    await WriteBrowserLandingPageAsync(context, davContext);
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return;
            }

            //
            // Find the document
            //
            string fileName = resolved.DocumentName;

            if (fileName == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            List<Document> docsInFolder = await fileService.GetDocumentsInFolderAsync(
                resolved.FolderId, davContext.TenantGuid, context.RequestAborted);

            Document docMeta = docsInFolder.FirstOrDefault(d =>
                string.Equals(d.fileName, fileName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(d.name, fileName, StringComparison.OrdinalIgnoreCase));

            if (docMeta == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            //
            // Fetch the full document (including binary)
            //
            Document doc = await fileService.GetDocumentByIdAsync(
                docMeta.id, davContext.TenantGuid, context.RequestAborted);

            if (doc == null || doc.fileDataData == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }


            //
            // Write the response
            //
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = doc.mimeType ?? doc.fileDataMimeType ?? "application/octet-stream";
            context.Response.ContentLength = doc.fileDataData.Length;
            context.Response.Headers["ETag"] = $"\"{doc.objectGuid:N}\"";

            context.Response.Headers["Last-Modified"] = doc.uploadedDate.ToUniversalTime().ToString("R");

            await context.Response.Body.WriteAsync(doc.fileDataData, 0, doc.fileDataData.Length, context.RequestAborted);
        }


        /// <summary>
        /// Returns a clean informative HTML page for browsers that navigate to the server.
        /// </summary>
        private static async Task WriteBrowserLandingPageAsync(HttpContext context, WebDavContext davContext)
        {
            string host = context.Request.Host.Value;
            string userName = davContext.User?.accountName ?? "your username";

            string html = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>Scheduler WebDAV File Server</title>
    <style>
        *  {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
            background: linear-gradient(135deg, #0f0c29 0%, #302b63 50%, #24243e 100%);
            color: #e0e0e0;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 2rem;
        }}
        .card {{
            background: rgba(255, 255, 255, 0.06);
            backdrop-filter: blur(20px);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 16px;
            padding: 3rem;
            max-width: 640px;
            width: 100%;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
        }}
        h1 {{
            font-size: 1.8rem;
            font-weight: 700;
            margin-bottom: 0.5rem;
            background: linear-gradient(135deg, #667eea, #764ba2);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }}
        .subtitle {{
            color: #9ca3af;
            font-size: 0.95rem;
            margin-bottom: 2rem;
        }}
        .status {{
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            background: rgba(52, 211, 153, 0.1);
            border: 1px solid rgba(52, 211, 153, 0.3);
            color: #34d399;
            padding: 0.35rem 0.85rem;
            border-radius: 999px;
            font-size: 0.8rem;
            font-weight: 600;
            margin-bottom: 2rem;
        }}
        .status::before {{
            content: '';
            width: 8px;
            height: 8px;
            background: #34d399;
            border-radius: 50%;
            animation: pulse 2s infinite;
        }}
        @keyframes pulse {{
            0%, 100% {{ opacity: 1; }}
            50% {{ opacity: 0.4; }}
        }}
        h2 {{
            font-size: 1rem;
            font-weight: 600;
            color: #c4b5fd;
            margin: 1.5rem 0 0.75rem;
            text-transform: uppercase;
            letter-spacing: 0.05em;
        }}
        .method {{
            background: rgba(255, 255, 255, 0.04);
            border: 1px solid rgba(255, 255, 255, 0.08);
            border-radius: 10px;
            padding: 1rem 1.25rem;
            margin-bottom: 0.75rem;
        }}
        .method h3 {{
            font-size: 0.9rem;
            font-weight: 600;
            color: #e5e7eb;
            margin-bottom: 0.4rem;
        }}
        .method p {{
            font-size: 0.82rem;
            color: #9ca3af;
            line-height: 1.5;
        }}
        code {{
            background: rgba(139, 92, 246, 0.15);
            color: #c4b5fd;
            padding: 0.15rem 0.4rem;
            border-radius: 4px;
            font-size: 0.82rem;
            font-family: 'SF Mono', 'Fira Code', 'Cascadia Code', monospace;
        }}
        .user-info {{
            font-size: 0.82rem;
            color: #9ca3af;
            margin-top: 2rem;
            padding-top: 1.25rem;
            border-top: 1px solid rgba(255, 255, 255, 0.06);
        }}
        .user-info strong {{ color: #c4b5fd; }}
    </style>
</head>
<body>
    <div class=""card"">
        <h1>&#128193; Scheduler WebDAV</h1>
        <p class=""subtitle"">File Server</p>
        <div class=""status"">Online</div>

        <p style=""font-size: 0.88rem; color: #d1d5db; line-height: 1.6; margin-bottom: 0.5rem;"">
            This is a WebDAV file server.  Connect using a WebDAV client to browse, upload, and manage your files.
        </p>

        <h2>How to Connect</h2>

        <div class=""method"">
            <h3>Windows Explorer</h3>
            <p>Right-click <strong>This PC</strong> &rarr; <strong>Map network drive</strong><br>
               Folder: <code>\\{host}\DavWWWRoot</code><br>
               Or: <code>net use Z: http://{host}/ /user:{userName}</code></p>
        </div>

        <div class=""method"">
            <h3>macOS Finder</h3>
            <p><strong>Go</strong> &rarr; <strong>Connect to Server</strong> (&#8984;K)<br>
               Server address: <code>http://{host}/</code></p>
        </div>

        <div class=""method"">
            <h3>Linux (GNOME Files)</h3>
            <p><strong>Other Locations</strong> &rarr; <strong>Connect to Server</strong><br>
               <code>dav://{host}/</code></p>
        </div>

        <div class=""method"">
            <h3>Cyberduck / WinSCP / Other Clients</h3>
            <p>Protocol: <strong>WebDAV (HTTP)</strong><br>
               Server: <code>{host}</code><br>
               Username &amp; password: your Scheduler credentials</p>
        </div>

        <div class=""user-info"">
            Authenticated as <strong>{userName}</strong>
        </div>
    </div>
</body>
</html>";

            byte[] bytes = Encoding.UTF8.GetBytes(html);

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength = bytes.Length;

            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length, context.RequestAborted);
        }
    }
}
