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
    /* For tech specs on the Microsoft side, go here https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow or here https://docs.microsoft.com/en-us/graph/auth-v2-user  */

    public class MicrosoftAuthentication
    {
        public const string SCOPE = "openid%20profile%20email%20https%3A%2F%2Fgraph.microsoft.com%2Fuser.read";     // Scope requested is openid and profile and user.read
        public const string CODE_KEY = "ThisIsntRand0mButItNeedsToBe43CharactersL0ng";

        public class MicrosoftToken
        {
            public string scope { get; set; }
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public int ext_expires_in { get; set; }
            public string refresh_token { get; set; }
        }

        public class MicrosoftUser
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


        public class MicrosoftConfiguration
        {
            public string token_endpoint { get; set; }
            public List<string> token_endpoint_auth_methods_supported { get; set; }

            public string jwks_uri { get; set; }
            public List<string> response_modes_supported { get; set; }

            public List<string> subject_types_supported { get; set; }

            public List<string> id_token_signing_alg_values_supported { get; set; }
            public List<string> response_types_supported { get; set; }


            public List<string> scopes_supported { get; set; }

            public string issuer { get; set; }


            public bool microsoft_multi_refresh_token { get; set; }
            public string authorization_endpoint { get; set; }
            public string device_authorization_endpoint { get; set; }

            public bool http_logout_supported { get; set; }
            public bool frontchannel_logout_supported { get; set; }

            public string end_session_endpoint { get; set; }

            public string revocation_endpoint { get; set; }


            public List<string> claims_supported { get; set; }
            public string check_session_iframe { get; set; }
            public string userinfo_endpoint { get; set; }


            public string tenant_region_scope { get; set; }

            public string cloud_instance_name { get; set; }
            public string cloud_graph_host_name { get; set; }
            public string msgraph_host { get; set; }
            public string rbac_url { get; set; }

        }


        public static MicrosoftConfiguration GetMicrosoftConfiguration()
        {
            // I'm pretty sure that this won't change, so no sense making this a config option.
            string configURL = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.Expect100Continue = true;

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(configURL);
            //request.Method = "GET";
            //request.KeepAlive = false;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var task = Task.Run(() => client.GetAsync(configURL));
                    task.Wait();

                    string responseFromServer = null;


                    using (var response = task.Result)
                    {
                        using (Stream streamToReadFrom = response.Content.ReadAsStream())
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                streamToReadFrom.CopyTo(memoryStream);

                                responseFromServer = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                            }
                        }
                    }

                    if (responseFromServer != null && responseFromServer.Length > 0)
                    {
                        try
                        {
                            MicrosoftConfiguration microsoftConfig = JsonSerializer.Deserialize<MicrosoftConfiguration>(responseFromServer);

                            if (microsoftConfig != null)
                            {
                                return microsoftConfig;
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
                        throw new Exception("No response was received from request for Microsoft discovery document.");
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
                throw new Exception("Caught error trying to authenticate with Microsoft", ex);
            }
        }


        private static void getMicrosoftIdentifiers(out string microsoftAppClientId,
                                                    out string microsoftAppTenantId,
                                                    out string microsoftAppClientSecret,
                                                    out string microsoftRedirectionURL)
        {
            microsoftAppClientId = Foundation.Configuration.GetStringConfigurationSetting("MicrosoftAppClientId");

            microsoftAppTenantId = Foundation.Configuration.GetStringConfigurationSetting("MicrosoftAppTenantId");
            if (string.IsNullOrEmpty(microsoftAppTenantId) == true)
            {
                microsoftAppTenantId = "common";
            }

            microsoftAppClientSecret = Foundation.Configuration.GetStringConfigurationSetting("MicrosoftAppClientSecret");
            microsoftRedirectionURL = Foundation.Configuration.GetStringConfigurationSetting("MicrosoftRedirectionURL");

            return;
        }


        public static MicrosoftToken GetMicrosoftToken(string code, string tokenURL)
        {
            string microsoftAppClientId;
            string microsoftAppTenantId;
            string microsoftAppClientSecret;
            string microsoftRedirectionURL;

            getMicrosoftIdentifiers(out microsoftAppClientId, out microsoftAppTenantId, out microsoftAppClientSecret, out microsoftRedirectionURL);


            //
            // https://login.microsoftonline.com
            // /{tenant}/oauth2/v2.0/token
            //
            //string microsoftTokenURL = "https://login.microsoftonline.com/" + microsoftAppTenantId + "/oauth2/v2.0/token";

            string postString = @"client_id=" + microsoftAppClientId + "&scope=" + SCOPE + "&code=" + code + "&redirect_uri=" + microsoftRedirectionURL + "&grant_type=authorization_code&code_verifier=" + CODE_KEY + "&client_secret=" + HttpUtility.UrlEncode(microsoftAppClientSecret);


            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.Expect100Continue = true;

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(tokenURL);
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
            //catch (Exception ex)
            //{
            //    throw new Exception("Caught error trying to authenticate with Microsoft", ex);
            //}

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var task = Task.Run(() => client.PostAsync(tokenURL, new ByteArrayContent(bytes)));
                    task.Wait();

                    using (var response = task.Result)
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
                                MicrosoftToken microsoftToken = JsonSerializer.Deserialize<MicrosoftToken>(responseFromServer);

                                return microsoftToken;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error caught during interpreting JSON results.  Response received is " + responseFromServer, ex);
                            }
                        }
                        else
                        {
                            throw new Exception("No response was received from Microsoft from authentication request.");
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
                System.Diagnostics.Debug.WriteLine("Error caught during getting Microsoft token.  Details are: " + ex.ToString());
                throw;
            }
        }


        public static MicrosoftUser GetMicrosoftUserProfile(MicrosoftToken token, string userInfoURL)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.Expect100Continue = true;

            //// Add the scope to the user info URL
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(userInfoURL);

            //request.PreAuthenticate = true;
            //request.Headers.Add("Authorization", "Bearer " + token.access_token);
            //request.Accept = "application/json";
            //request.AllowWriteStreamBuffering = true;
            //request.ContentType = "text/xml";
            //request.Method = "GET";
            //request.KeepAlive = false;
            //request.ProtocolVersion = HttpVersion.Version11;
            //request.ServicePoint.ConnectionLimit = 1;

            try
            {
                //using (WebResponse response = request.GetResponse())
                using (HttpClient client = new HttpClient())
                {
                    // ToDo - get auth headers onto this 
                    var task = Task.Run(() => client.GetAsync(userInfoURL));
                    task.Wait();


                    string responseFromServer;

                    using (var response = task.Result)
                    {
                        using (Stream streamToReadFrom = response.Content.ReadAsStream())
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                streamToReadFrom.CopyTo(memoryStream);

                                responseFromServer = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                            }
                        }
                    }

                    //using (var receiveStream = response.GetResponseStream())
                    //{
                    //    using (StreamReader sr = new StreamReader(receiveStream))
                    //    {
                    //        responseFromServer = sr.ReadToEnd().Trim();

                    //        //
                    //        // Force the close after we're done
                    //        //
                    //        sr.Close();
                    //    }

                    //    // force cleanup
                    //    receiveStream.Close();
                    //}

                    ////
                    //// More forced cleanup
                    ////
                    //response.Close();

                    if (responseFromServer != null && responseFromServer.Length > 0)
                    {
                        try
                        {
                            MicrosoftUser microsoftUser = JsonSerializer.Deserialize<MicrosoftUser>(responseFromServer);

                            if (string.IsNullOrEmpty(microsoftUser.picture) == false)
                            {
                                try
                                {
                                    //HttpWebRequest pictureRequest = (HttpWebRequest)WebRequest.Create(microsoftUser.picture);

                                    //pictureRequest.PreAuthenticate = true;
                                    //pictureRequest.Headers.Add("Authorization", "Bearer " + token.access_token);
                                    //pictureRequest.AllowWriteStreamBuffering = false;
                                    //pictureRequest.Method = "GET";
                                    //pictureRequest.ContentType = "image/*";
                                    //pictureRequest.KeepAlive = false;
                                    //pictureRequest.ProtocolVersion = HttpVersion.Version11;
                                    //pictureRequest.ServicePoint.ConnectionLimit = 1;

                                    /*
                                    using (WebResponse pictureResponse = pictureRequest.GetResponse())
                                    {
                                        using (var pictureReceiveStream = pictureResponse.GetResponseStream())
                                        {

                                            using (var memstream = new MemoryStream())
                                            {
                                                pictureReceiveStream.CopyTo(memstream);
                                                microsoftUser.pictureData = memstream.ToArray();
                                            }
                                            // force cleanup
                                            pictureReceiveStream.Close();
                                        }

                                        pictureResponse.Close();
                                    }
                                    */

                                    var pictureTask = Task.Run(() => client.GetAsync(microsoftUser.picture));
                                    pictureTask.Wait();


                                }
                                catch (WebException wex)
                                {
                                    string result = null;

                                    using (var reader = new StreamReader(wex.Response.GetResponseStream()))
                                    {
                                        result = reader.ReadToEnd();
                                    }

                                    // don't do anything if the attempt to get the picture fails.  If there is no 'consumer picture' this will throw an error.
                                    //
                                    // Not sure it's workth logging the failure here.
                                    //
                                }
                                catch (Exception)
                                {
                                    // do nothing there.
                                }
                            }

                            return microsoftUser;
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
                            throw new Exception("Error caught during interpreting JSON results.  Response received is " + responseFromServer, ex);
                        }
                    }
                    else
                    {
                        throw new Exception("No response was received from Microsoft from authentication request.");
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
                throw new Exception("Caught error trying to authenticate with Microsoft", ex);
            }
        }


        public static string GetInitialMicrosoftAuthenticationRedirectionURL(string state)
        {
            string microsoftAppClientId;
            string microsoftAppClientSecret;
            string microsoftTenantId;
            string microsoftRedirectionURL;

            getMicrosoftIdentifiers(out microsoftAppClientId, out microsoftTenantId, out microsoftAppClientSecret, out microsoftRedirectionURL);

            MicrosoftConfiguration config = GetMicrosoftConfiguration();
            //
            //return "https://login.microsoftonline.com/" + microsoftTenantId + "/oauth2/v2.0/authorize?client_id=" + microsoftAppClientId + "&response_mode=query&response_type=code&scope=" + SCOPE + "&state=" + state + "&code_challenge=" + CODE_KEY + "&redirect_uri=" + microsoftRedirectionURL;
            //

            return config.authorization_endpoint + "?client_id=" + microsoftAppClientId + "&response_mode=query&response_type=code&scope=" + SCOPE + "&state=" + state + "&code_challenge=" + CODE_KEY + "&redirect_uri=" + microsoftRedirectionURL;
            //return config.v2_authorization_endpoint + "?client_id=" + microsoftAppClientId + "&response_mode=query&response_type=code&scope=" + SCOPE + "&state=" + state + "&code_challenge=" + CODE_KEY + "&redirect_uri=" + microsoftRedirectionURL;
        }
    }
}
