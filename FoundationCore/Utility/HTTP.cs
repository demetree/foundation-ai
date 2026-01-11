using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Foundation.HTTPUtility
{
    public static class HTTP
    {
        public async static Task<string> SaveHttpContentToTempFileAsync(string route, HttpContent httpContent)
        {
            if (httpContent == null)
            {
                throw new NullReferenceException("HttpContent cannot be null.");
            }

            using var contentStream = await httpContent.ReadAsStreamAsync();
            var guid = Guid.NewGuid();

            string extension = ".bin";
            var contentType = httpContent.Headers.ContentType;
            if (contentType != null)
            {
                var mediaType = contentType.MediaType;
                var extensionMap = new Dictionary<string, string>
            {
                {"text/plain", ".txt"},
                {"application/json", ".json"},
                {"application/xml", ".xml"},
                {"image/jpeg", ".jpg"},
                {"image/png", ".png"}
            };
                if (extensionMap.ContainsKey(mediaType))
                {
                    extension = extensionMap[mediaType];
                }
            }

            var httpContentTempPath = Path.Combine(Path.GetTempPath(), "HTTPContent");

            System.IO.Directory.CreateDirectory(httpContentTempPath);

            var dataFilePath = Path.Combine(httpContentTempPath, guid.ToString() + extension);
            using var fileStream = File.Create(dataFilePath);
            contentStream.CopyTo(fileStream);

            var routeFilePath = Path.Combine(httpContentTempPath, guid.ToString() + ".route");

            using (StreamWriter outputFile = new StreamWriter(routeFilePath))
            {
                outputFile.WriteLine(route);
            }

            return dataFilePath;
        }
    }
}
