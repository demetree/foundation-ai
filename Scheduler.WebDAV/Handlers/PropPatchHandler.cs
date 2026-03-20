//
// PropPatchHandler.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// No-op PROPPATCH handler — accepts any property update request and returns
// 207 Multi-Status indicating success without actually modifying anything.
//
// This satisfies clients (macOS Finder, LibreOffice) that send PROPPATCH after
// PUT to set properties like getlastmodified.
//
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Scheduler.WebDAV.Services;
using Scheduler.WebDAV.Xml;

namespace Scheduler.WebDAV.Handlers
{
    public static class PropPatchHandler
    {
        private static readonly XNamespace DAV = "DAV:";

        public static async Task HandleAsync(HttpContext context, WebDavContext davContext)
        {
            string path = context.Request.Path.Value ?? "/";

            //
            // Parse the request body to extract property names being set
            //
            List<string> propNames = new List<string>();

            if (context.Request.ContentLength > 0)
            {
                try
                {
                    using MemoryStream ms = new MemoryStream();
                    await context.Request.Body.CopyToAsync(ms, context.RequestAborted);
                    ms.Position = 0;

                    XDocument doc = await XDocument.LoadAsync(ms, LoadOptions.None, context.RequestAborted);

                    // Walk <propertyupdate><set><prop>...</prop></set></propertyupdate>
                    foreach (XElement setEl in doc.Descendants(DAV + "set"))
                    {
                        foreach (XElement propEl in setEl.Descendants(DAV + "prop"))
                        {
                            foreach (XElement child in propEl.Elements())
                            {
                                propNames.Add(child.Name.LocalName);
                            }
                        }
                    }
                }
                catch
                {
                    // Malformed XML — just return success for empty property list
                }
            }


            //
            // Build a 207 response indicating all properties were "set" successfully
            //
            XElement propstatEl = new XElement(DAV + "propstat",
                new XElement(DAV + "status", "HTTP/1.1 200 OK"));

            if (propNames.Count > 0)
            {
                XElement propEl = new XElement(DAV + "prop");
                foreach (string name in propNames)
                {
                    propEl.Add(new XElement(DAV + name));
                }
                propstatEl.AddFirst(propEl);
            }

            XElement responseEl = new XElement(DAV + "response",
                new XElement(DAV + "href", path),
                propstatEl);

            XDocument responseDoc = DavXmlBuilder.MultiStatus(new[] { responseEl });


            context.Response.StatusCode = 207;
            context.Response.ContentType = "application/xml; charset=utf-8";

            using MemoryStream responseMs = new MemoryStream();
            responseDoc.Save(responseMs);
            context.Response.ContentLength = responseMs.Length;
            responseMs.Position = 0;
            await responseMs.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
    }
}
