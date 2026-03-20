//
// DavXmlBuilder.cs
//
// Utility class for building RFC 4918 WebDAV XML responses using System.Xml.Linq.
// No third-party XML dependencies — only the built-in XDocument/XElement/XNamespace.
//
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Scheduler.WebDAV.Services;

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


        /// <summary>
        /// Builds the supportedlock property element for PROPFIND responses.
        /// Advertises exclusive and shared write locks.
        /// </summary>
        public static XElement SupportedLockProperty()
        {
            return new XElement(DAV + "supportedlock",
                new XElement(DAV + "lockentry",
                    new XElement(DAV + "lockscope",
                        new XElement(DAV + "exclusive")),
                    new XElement(DAV + "locktype",
                        new XElement(DAV + "write"))),
                new XElement(DAV + "lockentry",
                    new XElement(DAV + "lockscope",
                        new XElement(DAV + "shared")),
                    new XElement(DAV + "locktype",
                        new XElement(DAV + "write"))));
        }


        /// <summary>
        /// Builds a complete DAV:prop response containing lockdiscovery for a LOCK response.
        /// </summary>
        public static XDocument LockDiscoveryResponse(WebDavLock lockInfo)
        {
            XElement activeLock = BuildActiveLockElement(lockInfo);

            XElement propEl = new XElement(DAV + "prop",
                new XElement(DAV + "lockdiscovery", activeLock));

            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                propEl);
        }


        /// <summary>
        /// Builds a lockdiscovery property element from a list of active locks.
        /// Used in PROPFIND responses to show current locks on a resource.
        /// </summary>
        public static XElement LockDiscoveryProperty(IEnumerable<WebDavLock> activeLocks)
        {
            XElement lockDiscovery = new XElement(DAV + "lockdiscovery");

            foreach (WebDavLock lockInfo in activeLocks)
            {
                lockDiscovery.Add(BuildActiveLockElement(lockInfo));
            }

            return lockDiscovery;
        }


        /// <summary>
        /// Builds a single DAV:activelock element for a lock.
        /// </summary>
        private static XElement BuildActiveLockElement(WebDavLock lockInfo)
        {
            XElement scopeEl = lockInfo.LockScope == "shared"
                ? new XElement(DAV + "lockscope", new XElement(DAV + "shared"))
                : new XElement(DAV + "lockscope", new XElement(DAV + "exclusive"));

            int remainingSeconds = Math.Max(0,
                (int)(lockInfo.ExpiresAt - DateTime.UtcNow).TotalSeconds);

            return new XElement(DAV + "activelock",
                scopeEl,
                new XElement(DAV + "locktype",
                    new XElement(DAV + "write")),
                new XElement(DAV + "depth",
                    lockInfo.Depth == -1 ? "infinity" : lockInfo.Depth.ToString()),
                new XElement(DAV + "owner",
                    new XElement(DAV + "href", lockInfo.Owner)),
                new XElement(DAV + "timeout",
                    $"Second-{remainingSeconds}"),
                new XElement(DAV + "locktoken",
                    new XElement(DAV + "href", lockInfo.LockToken)),
                new XElement(DAV + "lockroot",
                    new XElement(DAV + "href", "/")));
        }
    }
}
