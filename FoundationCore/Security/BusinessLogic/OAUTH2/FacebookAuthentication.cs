using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Web;

namespace Foundation.Security
{
    public class FacebookAuthentication
    {
        //
        // The discovery URL that really should be used instead of these hard coded endpoints is https://accounts.facebook.com/.well-known/openid-configuration
        //
        public class FacebookToken
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
        }

        public class FacebookUser
        {
            public string id { get; set; }
            public string name { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string link { get; set; }
            public string email { get; set; }
            public string picture { get; set; }
            public string gender { get; set; }
            public string locale { get; set; }

            public byte[] pictureData { get; set; }
        }

        public class FacebookConfiguration
        {
            public string issuer { get; set; }
            public string authorization_endpoint { get; set; }
            public string jwks_uri { get; set; }
            public List<string> response_types_supported { get; set; }
            public List<string> subject_types_supported { get; set; }
            public List<string> id_token_signing_alg_values_supported { get; set; }
            public List<string> claims_supported { get; set; }
        }


        public static FacebookConfiguration GetFacebookConfiguration()
        {
            string configURL = "https://accounts.facebook.com/.well-known/openid-configuration";

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
                                FacebookConfiguration facebookConfig = JsonSerializer.Deserialize<FacebookConfiguration>(responseFromServer);

                                if (facebookConfig != null)
                                {
                                    return facebookConfig;
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
                            throw new Exception("No response was received from request for Facebook discovery document.");
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
                System.Diagnostics.Debug.WriteLine("Error caught during getting Facebook configuration.  Details are: " + ex.ToString());
                throw;
            }
        }

        private static void getFacebookIdentifiers(out string facebookAppClientId, out string facebookAppClientSecret, out string facebookRedirectionURL)
        {
            facebookAppClientId = Foundation.Configuration.GetStringConfigurationSetting("FacebookAppClientId");
            facebookAppClientSecret = Foundation.Configuration.GetStringConfigurationSetting("FacebookAppClientSecret");
            facebookRedirectionURL = Foundation.Configuration.GetStringConfigurationSetting("FacebookRedirectionURL");

            return;
        }


        public static FacebookToken GetFacebookToken(string code)
        {
            string facebookAppClientId;
            string facebookAppClientSecret;
            string facebookRedirectionURL;

            getFacebookIdentifiers(out facebookAppClientId, out facebookAppClientSecret, out facebookRedirectionURL);

            string facebookTokenURL = "https://graph.facebook.com/v10.0/oauth/access_token?client_id=" + facebookAppClientId + "&redirect_uri=" + facebookRedirectionURL + "&client_secret=" + HttpUtility.UrlEncode(facebookAppClientSecret) + "&code=" + HttpUtility.UrlEncode(code);

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.Expect100Continue = true;

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(facebookTokenURL);
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.Method = "GET";
            //request.KeepAlive = false;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, facebookTokenURL);

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
                                FacebookToken facebookToken = JsonSerializer.Deserialize<FacebookToken>(responseFromServer);

                                return facebookToken;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error caught during interpreting JSON results.  Response received is " + responseFromServer, ex);
                            }
                        }
                        else
                        {
                            throw new Exception("No response was received from Facebook from authentication request.");
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
            catch (Exception)
            {
                throw;
            }
        }


        public static FacebookUser GetFacebookUserProfile(FacebookToken token)
        {
            string url = "https://graph.facebook.com/me?fields=id,name,first_name,email,last_name&access_token=" + HttpUtility.UrlEncode(token.access_token);


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

                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

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
                            FacebookUser facebookUser = JsonSerializer.Deserialize<FacebookUser>(responseFromServer);

                            string pictureURL = "https://graph.facebook.com/v10.0/" + facebookUser.id + "/picture?access_token=" + HttpUtility.UrlEncode(token.access_token);

                            //HttpWebRequest pictureRequest = (HttpWebRequest)WebRequest.Create(pictureURL);

                            //pictureRequest.PreAuthenticate = true;
                            //pictureRequest.Headers.Add("Authorization", "Bearer " + token.access_token);
                            //pictureRequest.AllowWriteStreamBuffering = false;
                            //pictureRequest.Method = "GET";
                            //pictureRequest.ContentType = "image/*";
                            //pictureRequest.KeepAlive = false;
                            //pictureRequest.ProtocolVersion = HttpVersion.Version11;
                            //pictureRequest.ServicePoint.ConnectionLimit = 1;

                            //using (WebResponse pictureResponse = pictureRequest.GetResponse())

                            try
                            {
                                //TODO - THIS NEEDS THE BEARER TOKEN HEADER ADDED...
                                HttpRequestMessage pictureRequest = new HttpRequestMessage(HttpMethod.Get, pictureURL);

                                using (var pictureResponse = client.Send(request))
                                {
                                    pictureResponse.EnsureSuccessStatusCode();

                                    using (var pictureReceiveStream = response.Content.ReadAsStream())
                                    {
                                        using (StreamReader sr = new StreamReader(pictureReceiveStream))
                                        {
                                            using (var memStream = new MemoryStream())
                                            {
                                                pictureReceiveStream.CopyTo(memStream);
                                                facebookUser.pictureData = memStream.ToArray();
                                            }
                                            // force cleanup
                                            pictureReceiveStream.Close();
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {

                                // don't do anything if the attempt to get the picture fails.  If there is no 'consumer picture' this will throw an error.
                                //
                                // Not sure it's worth logging the failure here.
                                //
                            }

                            return facebookUser;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error caught during Facebook authentication", ex);
            }

            return null;
        }

        public static string GetInitialFacebookAuthenticationRedirectionURL()
        {
            string facebookAppClientId;
            string facebookAppClientSecret;
            string facebookRedirectionURL;

            getFacebookIdentifiers(out facebookAppClientId, out facebookAppClientSecret, out facebookRedirectionURL);

            FacebookConfiguration config = GetFacebookConfiguration();

            return config.authorization_endpoint + "?redirect_uri=" + facebookRedirectionURL + "&client_id=" + facebookAppClientId + "&scope=email,public_profile";

            //return "https://www.facebook.com/v10.0/dialog/oauth?redirect_uri=" + facebookRedirectionURL + "&client_id=" + facebookAppClientId + "&scope=email,public_profile";
        }
    }
}
