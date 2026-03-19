//
// DavXmlBuilder.cs
//
// Utility class for building RFC 4918 WebDAV XML responses using System.Xml.Linq.
// No third-party XML dependencies — only the built-in XDocument/XElement/XNamespace.
//
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Scheduler.WebDAV.Xml
{
    /// <summary>
    /// Builds DAV: namespace XML responses for WebDAV operations.
    /// </summary>
    public static class DavXmlBuilder
    {
        /// <summary>
        /// The DAV: XML namespace (RFC 4918).
        /// </summary>
        public static readonly XNamespace DAV = "DAV:";


        /// <summary>
        /// Builds a complete DAV:multistatus XML document from a list of response elements.
        /// </summary>
        public static XDocument MultiStatus(IEnumerable<XElement> responses)
        {
            XElement multistatus = new XElement(DAV + "multistatus",
                new XAttribute(XNamespace.Xmlns + "D", DAV.NamespaceName));

            foreach (XElement response in responses)
            {
                multistatus.Add(response);
            }

            return new XDocument(new XDeclaration("1.0", "utf-8", null), multistatus);
        }


        /// <summary>
        /// Builds a DAV:response element for a collection (folder).
        /// </summary>
        public static XElement CollectionResponse(string href, string displayName, DateTime lastModified, DateTime? created = null)
        {
            XElement propElement = new XElement(DAV + "prop",
                new XElement(DAV + "displayname", displayName),
                new XElement(DAV + "resourcetype",
                    new XElement(DAV + "collection")),
                new XElement(DAV + "getlastmodified", FormatHttpDate(lastModified)));

            if (created.HasValue)
            {
                propElement.Add(new XElement(DAV + "creationdate", FormatIso8601(created.Value)));
            }

            return new XElement(DAV + "response",
                new XElement(DAV + "href", href),
                new XElement(DAV + "propstat",
                    propElement,
                    new XElement(DAV + "status", "HTTP/1.1 200 OK")));
        }


        /// <summary>
        /// Builds a DAV:response element for a file (non-collection resource).
        /// </summary>
        public static XElement FileResponse(
            string href,
            string displayName,
            long contentLength,
            string contentType,
            DateTime lastModified,
            DateTime? created = null,
            string etag = null)
        {
            XElement propElement = new XElement(DAV + "prop",
                new XElement(DAV + "displayname", displayName),
                new XElement(DAV + "resourcetype"),  // Empty = non-collection
                new XElement(DAV + "getcontentlength", contentLength.ToString()),
                new XElement(DAV + "getcontenttype", contentType ?? "application/octet-stream"),
                new XElement(DAV + "getlastmodified", FormatHttpDate(lastModified)));

            if (created.HasValue)
            {
                propElement.Add(new XElement(DAV + "creationdate", FormatIso8601(created.Value)));
            }

            if (!string.IsNullOrEmpty(etag))
            {
                propElement.Add(new XElement(DAV + "getetag", $"\"{etag}\""));
            }

            return new XElement(DAV + "response",
                new XElement(DAV + "href", href),
                new XElement(DAV + "propstat",
                    propElement,
                    new XElement(DAV + "status", "HTTP/1.1 200 OK")));
        }


        /// <summary>
        /// Formats a DateTime as an HTTP-date (RFC 7231) for DAV:getlastmodified.
        /// Example: "Wed, 19 Mar 2026 18:00:00 GMT"
        /// </summary>
        public static string FormatHttpDate(DateTime utcDateTime)
        {
            return utcDateTime.ToUniversalTime().ToString("R");
        }


        /// <summary>
        /// Formats a DateTime as ISO 8601 for DAV:creationdate.
        /// Example: "2026-03-19T18:00:00Z"
        /// </summary>
        public static string FormatIso8601(DateTime utcDateTime)
        {
            return utcDateTime.ToUniversalTime().ToString("o");
        }
    }
}
