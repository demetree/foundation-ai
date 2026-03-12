using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System;


namespace Foundation
{
    /// <summary>
    /// 
    /// Extension method for serving SPA static files with proper cache-busting headers.
    ///
    /// Angular's production build hashes JS and CSS filenames (e.g., main.ABC123.js), so browsers treat 
    /// each new build as a new resource.  By setting Cache-Control: max-age=31536000 (1 year) on these 
    /// hashed assets, browsers cache them aggressively and never re-download unchanged bundles.
    ///
    /// However, index.html itself is NOT hashed — it's the entry point that references the hashed bundles.
    /// If index.html is cached, the browser will serve old bundle references even after a new deployment.
    /// Setting Cache-Control: no-cache on index.html forces the browser to always check for a fresh copy.
    ///
    /// </summary>
    public static class StaticFileCacheBustingExtensions
    {
        private const int ONE_YEAR_IN_SECONDS = 31536000;

        /// <summary>
        /// Replaces app.UseStaticFiles() with cache-aware static file serving.
        ///
        ///   - Hashed assets (.js, .css, .woff, .woff2): Cache-Control: public, max-age=31536000, immutable
        ///   - Entry point (index.html):                  Cache-Control: no-cache, no-store, must-revalidate
        ///   - Everything else:                            Default (no explicit Cache-Control header)
        ///
        /// </summary>
        public static WebApplication UseStaticFilesWithCacheBusting(this WebApplication app)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    string path = ctx.Context.Request.Path.Value ?? "";

                    if (IsHashedAsset(path) == true)
                    {
                        //
                        // Hashed JS/CSS/font bundles — safe to cache forever because the hash changes on every build.
                        // "immutable" tells browsers not to even revalidate.
                        //
                        ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={ONE_YEAR_IN_SECONDS}, immutable");
                    }
                    else if (IsEntryPoint(path) == true)
                    {
                        //
                        // index.html — must always be fetched fresh so the browser gets the latest script/link tags
                        // pointing to the new hashed bundles.
                        //
                        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
                        ctx.Context.Response.Headers.Append("Expires", "0");
                    }

                    // Everything else (images, JSON, etc.) uses default caching behaviour.
                }
            });

            return app;
        }


        /// <summary>
        /// Returns true for file types that Angular hashes in production builds.
        /// </summary>
        private static bool IsHashedAsset(string path)
        {
            return path.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".woff", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Returns true for the SPA entry point file.
        /// </summary>
        private static bool IsEntryPoint(string path)
        {
            return path.EndsWith("index.html", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
