using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Foundation
{
    public class Network
    {
        public static string MakeJSONWebRequest(string url, string method = "GET")
        {
            //
            // This is needed to allow TLS 1.2.  Without this, no connection allowed.  See https://stackoverflow.com/questions/28453353/http-post-error-an-existing-connection-was-forcibly-closed-by-the-remote-host#35015311
            //
            //System.Net.ServicePointManager.SecurityProtocol = System.Net.ServicePointManager.SecurityProtocol | System.Net.SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowWriteStreamBuffering = true;
            request.ContentType = "text/html";          // to get JSON
            request.Method = method;
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version11;
            request.ServicePoint.ConnectionLimit = 1;
            string responseFromServer;

            try
            {
                using (WebResponse resp = request.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        responseFromServer = sr.ReadToEnd().Trim();
                    }
                }

                return responseFromServer;
            }
            catch (Exception)
            {
                //throw;

                return null;
            }
        }

        public static byte[] MakeWebRequest(string url, string method = "GET")
        {
            //
            // This is needed to allow TLS 1.2.  Without this, no connection allowed.  See https://stackoverflow.com/questions/28453353/http-post-error-an-existing-connection-was-forcibly-closed-by-the-remote-host#35015311
            //
            //System.Net.ServicePointManager.SecurityProtocol = System.Net.ServicePointManager.SecurityProtocol | System.Net.SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowWriteStreamBuffering = true;
            request.Method = method;
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version11;
            request.ServicePoint.ConnectionLimit = 1;


            byte[] responseFromServer;

            using (WebResponse resp = request.GetResponse())
            {
                MemoryStream ms = new MemoryStream();
                resp.GetResponseStream().CopyTo(ms);

                responseFromServer = ms.ToArray();
            }


            return responseFromServer;
        }

        public static async Task<byte[]> MakeWebRequestAsync(string url, string method = "GET")
        {
            //
            // This is needed to allow TLS 1.2.  Without this, no connection allowed.  See https://stackoverflow.com/questions/28453353/http-post-error-an-existing-connection-was-forcibly-closed-by-the-remote-host#35015311
            //
            //System.Net.ServicePointManager.SecurityProtocol = System.Net.ServicePointManager.SecurityProtocol | System.Net.SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowWriteStreamBuffering = true;
            request.Method = method;
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version11;
            request.ServicePoint.ConnectionLimit = 1;


            byte[] responseFromServer;

            try
            {
                using (WebResponse resp = await request.GetResponseAsync())
                {
                    MemoryStream ms = new MemoryStream();
                    resp.GetResponseStream().CopyTo(ms);

                    responseFromServer = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error during web request.  Details are: " + ex.ToString());
                throw;
            }

            return responseFromServer;
        }

    }
}
