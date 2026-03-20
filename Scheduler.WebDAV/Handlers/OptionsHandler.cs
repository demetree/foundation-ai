//
// OptionsHandler.cs
//
// Responds to OPTIONS requests with WebDAV DAV:1,2 compliance headers.
// This tells clients what methods and capabilities the server supports.
//
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Scheduler.WebDAV.Handlers
{
    public static class OptionsHandler
    {
        public static Task HandleAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.Headers["DAV"] = "1, 2";
            context.Response.Headers["Allow"] = "OPTIONS, PROPFIND, PROPPATCH, GET, HEAD, PUT, DELETE, MKCOL, MOVE, COPY, LOCK, UNLOCK";
            context.Response.Headers["MS-Author-Via"] = "DAV";
            context.Response.ContentLength = 0;

            return Task.CompletedTask;
        }
    }
}
