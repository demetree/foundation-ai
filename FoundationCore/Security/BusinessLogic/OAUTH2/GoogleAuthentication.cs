using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Foundation.Security
{
    public class GoogleAuthentication
    {
        //
        // https://developers.google.com/identity/protocols/oauth2/web-server
        //
        //
        // The discovery URL that really should be used instead of these hard coded endpoints is https://accounts.google.com/.well-known/openid-configuration
        //
        public class GoogleToken
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
        }

        public class GoogleUser
        {
            public string sub { get; set; }
            public string name { get; set; }
            public string given_name { get; set; }
            public string family_name { get; set; }
            public string link { get; set; }
            public string email { get; set; }
            public string picture { get; set; }
            public string gender { get; set; }
            public string locale { get; set; }

            public byte[] pictureData { get; set; }
        }

        public class GoogleConfiguration
        {
            public string issuer { get; set; }
            public string authorization_endpoint { get; set; }
            public string device_authorization_endpointa { get; set; }
            public string token_endpoint { get; set; }
            public string userinfo_endpoint { get; set; }
            public string revocation_endpoint { get; set; }
            public string jwks_uri { get; set; }
            public List<string> response_types_supported { get; set; }
            public List<string> subject_types_supported { get; set; }
            public List<string> id_token_signing_alg_values_supported { get; set; }
            public List<string> scopes_token_signing_alg_values_supported { get; set; }
            public List<string> token_endpoint_auth_methods_supported { get; set; }
            public List<string> claims_supported { get; set; }
            public List<string> code_challenge_methods_supported { get; set; }
            public List<string> grant_types_supported { get; set; }
        }


        public static GoogleConfiguration GetGoogleConfiguration()
        {
            // I'm pretty sure that this won't change, so no sense making this a config option.
            string configURL = "https://accounts.google.com/.well-known/openid-configuration";

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.Expect100Continue = true;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, configURL);

                    using (var response = client.Send(request))
                    {
                        response.EnsureSuccessStatusCode();

                        string responseFromServer;

                        using (var receiveStream = response.Content.ReadAsStream())
                        {
                            using (StreamReader sr = new StreamReader(receiveStream))
                            {
                                responseFromServer = sr.ReadToEnd().Trim();
                                //
                                // Force the close after we're done
                                //
                                sr.Close();
                            }

                            // force cleanup
                            receiveStream.Close();
                        }

                        if (responseFromServer != null && responseFromServer.Length > 0)
                        {
                            try
                            {
                                GoogleConfiguration googleConfig = JsonSerializer.Deserialize<GoogleConfiguration>(responseFromServer);

                                if (googleConfig != null)
                                {
                                    return googleConfig;
                                }
                                else
                                {
                                    return null;
                                }

                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error caught during interpreting JSON results.  Response received is " + responseFromServer, ex);
                            }
                        }
                        else
                        {
                            throw new Exception("No response was received from request for Google discovery document.");
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                string result = null;

                using (var reader = new StreamReader(wex.Response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }

                throw new Exception("Error Response content is " + result, wex);
            }
            catch (Exception ex)
            {
                throw new Exception("Caught error trying to authenticate with Google", ex);
            }
        }


        private static void getGoogleIdentifiers(out string googleAppClientId, out string googleAppClientSecret, out string googleRedirectionURL)
        {
            // Examples from POC
            //googleAppClientId = "973793997217-fpo2l6a6hjfahm2tmuodoapf4ajvgblh.apps.googleusercontent.com";
            //googleAppClientSecret = "tOPjjL_Yhmlve88xKhHeMTLH";  
            //googleRedirectionURL = "http://localhost:23531/GoogleLogin";  

            googleAppClientId = Foundation.Configuration.GetStringConfigurationSetting("GoogleAppClientId");
            googleAppClientSecret = Foundation.Configuration.GetStringConfigurationSetting("GoogleAppClientSecret");
            googleRedirectionURL = Foundation.Configuration.GetStringConfigurationSetting("GoogleRedirectionURL");

            return;
        }


        public static GoogleToken GetGoogleToken(string code, string googleTokenURL)
        {
            string googleAppClientId;
            string googleAppClientSecret;
            string googleRedirectionURL;

            getGoogleIdentifiers(out googleAppClientId, out googleAppClientSecret, out googleRedirectionURL);

            // moved to param
            //string googleTokenURL = "https://accounts.google.com/o/oauth2/token";

            string postString = "grant_type=authorization_code&code=" + code + "&client_id=" + googleAppClientId + "&client_secret=" + HttpUtility.UrlEncode(googleAppClientSecret) + "&redirect_uri=" + googleRedirectionURL + "";

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.Expect100Continue = true;

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(googleTokenURL);
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.Method = "POST";
            //request.KeepAlive = false;

            UTF8Encoding utfenc = new UTF8Encoding();
            byte[] bytes = utfenc.GetBytes(postString);
            //Stream outputstream = null;

            //try
            //{
            //    request.ContentLength = bytes.Length;
            //    outputstream = request.GetRequestStream();
            //    outputstream.Write(bytes, 0, bytes.Length);
            //}
            //catch(Exception)
            //{
            //    throw;
            //}

            try
            {

                using (HttpClient client = new HttpClient())
                {
                    string responseFromServer = null;

                    var task = Task.Run(() => client.PostAsync(postString, new ByteArrayContent(bytes)));
                    task.Wait();

                    using (var response = task.Result)
                    {
                        response.EnsureSuccessStatusCode();

                        using (var receiveStream = response.Content.ReadAsStream())
                        {
                            using (StreamReader sr = new StreamReader(receiveStream))
                            {
                                responseFromServer = sr.ReadToEnd().Trim();
                                //
                                // Force the close after we're done
                                //
                                sr.Close();
                            }

                            // force cleanup
                            receiveStream.Close();
                        }
                    }


                    if (responseFromServer != null && responseFromServer.Length > 0)
                    {
                        try
                        {
                            GoogleToken googleToken = JsonSerializer.Deserialize<GoogleToken>(responseFromServer);

                            return googleToken;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error caught during interpreting JSON results.  Response received is " + responseFromServer, ex);
                        }
                    }
                    else
                    {
                        throw new Exception("No response was received from Google from authentication request.");
                    }
                }
            }
            catch (WebException wex)
            {
                string result = null;

                using (var reader = new StreamReader(wex.Response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }

                throw new Exception("Error Response content is " + result, wex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error caught during getting audit Google token string.  Details are: " + ex.ToString());
                throw;
            }
        }


        public static GoogleUser GetGoogleUserProfile(GoogleToken token, string googleUserInfoURL)
        {
            string url = googleUserInfoURL + "?alt=json&access_token=" + HttpUtility.UrlEncode(token.access_token);

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.Expect100Continue = true;

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.Credentials = CredentialCache.DefaultCredentials;

            //request.AllowWriteStreamBuffering = true;
            //request.ContentType = "text/xml";
            //request.Method = "GET";
            //request.KeepAlive = false;
            //request.ProtocolVersion = HttpVersion.Version10;
            //request.ServicePoint.ConnectionLimit = 1;            

            try
            {
                //using (WebResponse response = request.GetResponse())
                //{
                //    string responseFromServer;

                //    using (var receiveStream = response.GetResponseStream())
                //    {
                //        using (StreamReader sr = new StreamReader(receiveStream))
                //        {

                using (HttpClient client = new HttpClient())
                {
                    string responseFromServer;

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                    using (var response = client.Send(request))
                    {
                        response.EnsureSuccessStatusCode();


                        using (var receiveStream = response.Content.ReadAsStream())
                        {
                            using (StreamReader sr = new StreamReader(receiveStream))
                            {
                                responseFromServer = sr.ReadToEnd().Trim();

                                //
                                // Force the close after we're done
                                //
                                //sr.Close();
                            }
                        }

                        // force cleanup
                        //receiveStream.Close();
                    }


                    //
                    // More forced cleanup
                    //
                    //response.Close();

                    if (responseFromServer != null && responseFromServer.Length > 0)
                    {
                        try
                        {
                            GoogleUser googleUser = JsonSerializer.Deserialize<GoogleUser>(responseFromServer);

                            if (string.IsNullOrEmpty(googleUser.picture) == false)
                            {

                                //using (var webClient = new WebClient()) { 
                                //    googleUser.pictureData = webClient.DownloadData(googleUser.picture);
                                //}

                                string downloadURL = googleUser.picture;

                                //HttpResponseMessage response = await client.GetAsync(downloadUrl);

                                var task = Task.Run(() => client.GetAsync(downloadURL));
                                task.Wait();

                                using (var response = task.Result)
                                {
                                    using (Stream streamToReadFrom = response.Content.ReadAsStream())
                                    {
                                        using (MemoryStream memoryStream = new MemoryStream())
                                        {
                                            streamToReadFrom.CopyTo(memoryStream);
                                            googleUser.pictureData = memoryStream.ToArray();
                                        }
                                    }
                                }
                            }

                            return googleUser;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error caught during interpreting JSON results.  Response received is " + responseFromServer, ex);
                        }
                    }
                    else
                    {
                        throw new Exception("No response was received from Google from authentication request.");
                    }
                }
            }
            catch (WebException wex)
            {
                string result = null;

                using (var reader = new StreamReader(wex.Response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }

                throw new Exception("Error Response content is " + result, wex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error caught during getting Google user profile.  Details are: " + ex.ToString());
                throw;
            }
        }

        public static string GetInitialGoogleAuthenticationRedirectionURL()
        {
            string googleAppClientId;
            string googleAppClientSecret;
            string googleRedirectionURL;

            getGoogleIdentifiers(out googleAppClientId, out googleAppClientSecret, out googleRedirectionURL);

            GoogleConfiguration config = GetGoogleConfiguration();
            return config.authorization_endpoint + "?scope=profile%20https://www.googleapis.com/auth/userinfo.email&include_granted_scopes=true&redirect_uri=" + googleRedirectionURL + "&response_type=code&client_id=" + googleAppClientId + "";


            //return "https://accounts.google.com/o/oauth2/v2/auth?scope=profile%20https://www.googleapis.com/auth/userinfo.email&include_granted_scopes=true&redirect_uri=" + googleRedirectionURL + "&response_type=code&client_id=" + googleAppClientId + "";  
        }
    }
}
