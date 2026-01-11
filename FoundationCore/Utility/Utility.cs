using Foundation.Auditor;
using Foundation.Security.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using static Foundation.StringUtility;

namespace Foundation
{
    public class SMSSendCounterSingleton
    {
        // The Singleton's constructor should always be private to prevent
        // direct construction calls with the `new` operator.
        private SMSSendCounterSingleton() { }

        // The Singleton's instance is stored in a static field. There there are
        // multiple ways to initialize this field, all of them have various pros
        // and cons. In this example we'll show the simplest of these ways,
        // which, however, doesn't work really well in multithreaded program.
        private static SMSSendCounterSingleton _instance;

        private int sendCounter;

        // This is the static method that controls the access to the singleton
        // instance. On the first run, it creates a singleton object and places
        // it into the static field. On subsequent runs, it returns the client
        // existing object stored in the static field.
        public static SMSSendCounterSingleton GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SMSSendCounterSingleton();
                _instance.sendCounter = 0;
            }
            return _instance;
        }

        // Finally, any singleton should define some business logic, which can
        // be executed on its instance.
        public void incrementSendCounter()
        {
            // ...
            sendCounter++;
        }

        public int getSendCounter()
        {
            return sendCounter;
        }
    }


    public static partial class Utility
    {
        public static string GetRootPath()
        {
            string rootPath = Configuration.GetStringConfigurationSetting("RootPath");

            if (rootPath == null || rootPath.Length == 0)
            {
                // don't put on a trailing slash if the root path is blank. 
                rootPath = "";
            }
            else if (rootPath.Length > 0 && rootPath.StartsWith("/") == false)
            {
                // put on a trailing slash so that it is easy to just insert this as a section of the path in .cshtml and .js files driven on the view bag data.
                rootPath = rootPath.Trim();
                rootPath += "/";
            }

            return rootPath;
        }



        public static void SendMail(List<String> toAddresses, string subject, string body, bool isBodyHTML = false, string attachmentFilePath = null, List<String> ccAddresses = null, List<String> bccAddresses = null, SecurityUser user = null)
        {
            if (toAddresses != null && toAddresses.Count > 0)
            {
                for (int i = 0; i < toAddresses.Count; i++)
                {
                    var addr = new System.Net.Mail.MailAddress(toAddresses[i].Trim());
                    if (addr.Address != toAddresses[i].Trim())
                    {
                        throw new Exception("The to address of " + toAddresses[i] + " provided could not be used.");
                    }
                }
            }
            else
            {
                throw new Exception("At least one to address is required");
            }


            if (ccAddresses != null)
            {
                for (int i = 0; i < ccAddresses.Count; i++)
                {
                    var addr = new System.Net.Mail.MailAddress(ccAddresses[i].Trim());
                    if (addr.Address != ccAddresses[i].Trim())
                    {
                        throw new Exception("The cc address of " + ccAddresses[i] + " provided could not be used.");
                    }
                }
            }

            if (bccAddresses != null)
            {
                for (int i = 0; i < bccAddresses.Count; i++)
                {
                    var addr = new System.Net.Mail.MailAddress(bccAddresses[i].Trim());
                    if (addr.Address != bccAddresses[i].Trim())
                    {
                        throw new Exception("The bcc address of " + bccAddresses[i] + " provided could not be used.");
                    }
                }
            }


            if (subject == null || subject.Trim().Length == 0)
            {
                throw new Exception("A subject must be provided");
            }

            if (body == null || body.Trim().Length == 0)
            {
                throw new Exception("A body must be provided");
            }

            try
            {
                string connectionString = Configuration.GetStringConfigurationSetting("RootMailConnectionStringPath");

                if (connectionString.Length > 0)
                {
                    Dictionary<String, String> mailSettings = StringToDictionary(connectionString);

                    string server = "";
                    string fromAddress = "";
                    int port = 25;
                    bool enableSSL = false;

                    if (mailSettings.ContainsKey("server") == true)
                    {
                        server = mailSettings["server"];

                        SmtpClient mailClient = new SmtpClient(server);

                        mailClient.ServicePoint.MaxIdleTime = 0;
                        mailClient.ServicePoint.ConnectionLimit = 1;


                        MailMessage mail = new MailMessage();

                        if (mailSettings.ContainsKey("fromAddress") == true)
                        {
                            fromAddress = mailSettings["fromAddress"];
                        }
                        else
                        {
                            fromAddress = "doNotReply@doNotReply.com";
                        }

                        mail.From = new MailAddress(fromAddress);

                        for (int i = 0; i < toAddresses.Count; i++)
                        {
                            mail.To.Add(toAddresses[i].Trim());
                        }


                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = isBodyHTML;

                        if (ccAddresses != null && ccAddresses.Count > 0)
                        {
                            for (int i = 0; i < ccAddresses.Count; i++)
                            {
                                mail.CC.Add(ccAddresses[i].Trim());
                            }
                        }

                        if (bccAddresses != null && bccAddresses.Count > 0)
                        {
                            for (int i = 0; i < bccAddresses.Count; i++)
                            {
                                mail.Bcc.Add(bccAddresses[i].Trim());
                            }
                        }

                        if (attachmentFilePath != null && attachmentFilePath.Length > 0)
                        {
                            Attachment attachment = new Attachment(attachmentFilePath);
                            mail.Attachments.Add(attachment);
                        }


                        if (mailSettings.ContainsKey("port") == true)
                        {
                            string portString = mailSettings["port"];
                            int.TryParse(portString, out port);
                        }

                        mailClient.Port = port;

                        if (mailSettings.ContainsKey("user") == true &&
                            mailSettings.ContainsKey("password") == true)
                        {
                            string mailUser = mailSettings["user"];
                            string password = mailSettings["password"];
                            mailClient.Credentials = new System.Net.NetworkCredential(mailUser, password);
                        }


                        if (mailSettings.ContainsKey("enableSSL") == true)
                        {
                            string SSLString = mailSettings["enableSSL"];

                            if (SSLString != null && SSLString.Trim().ToUpper() == "TRUE")
                            {
                                enableSSL = true;
                            }
                            else if (SSLString != null && SSLString.Trim().ToUpper() == "FALSE")
                            {
                                enableSSL = false;
                            }
                            else
                            {
                                bool.TryParse(SSLString, out enableSSL);
                            }
                        }

                        mailClient.EnableSsl = enableSSL;


                        try
                        {
                            mailClient.Send(mail);

                            CreateExternalCommunicationForEmail(toAddresses, ccAddresses, bccAddresses, subject, body, true, null, user, null);

                        }
                        catch (Exception mailEx)
                        {
                            CreateExternalCommunicationForEmail(toAddresses, ccAddresses, bccAddresses, subject, body, false, null, user, mailEx);
                        }

                        //
                        // Clean up the mail client
                        //
                        mailClient.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static void SendMail(string to, string subject, string body, bool isBodyHTML = false, string attachmentFilePath = null, string cc = null, string bcc = null, SecurityUser user = null)
        {
            //
            // Multiple email addresses must be semi colon delimited
            //
            // First, make sure we can use the to address.
            //
            List<String> toAddresses;
            List<String> ccAddresses;
            List<String> bccAddresses;

            if (to != null && to.Length > 0)
            {
                toAddresses = to.Split(';').ToList();
            }
            else
            {
                toAddresses = new List<String>();
            }

            if (cc != null && cc.Length > 0)
            {
                ccAddresses = cc.Split(';').ToList();
            }
            else
            {
                ccAddresses = new List<String>();
            }

            if (bcc != null && bcc.Length > 0)
            {
                bccAddresses = bcc.Split(';').ToList();
            }
            else
            {
                bccAddresses = new List<String>();
            }

            SendMail(toAddresses, subject, body, isBodyHTML, attachmentFilePath, ccAddresses, bccAddresses, user);
        }


        public static bool SystemHasSMSConfiguration()
        {
            string connectionString = Configuration.GetStringConfigurationSetting("NexmoConnectionString");

            if (connectionString != null && connectionString.Trim().Length > 0)
            {
                Dictionary<String, String> smsSettings = StringToDictionary(connectionString);

                if (smsSettings.ContainsKey("secret") == true &&
                    smsSettings.ContainsKey("key") == true &&
                    smsSettings.ContainsKey("from") == true)
                {
                    string secret = smsSettings["secret"];
                    string key = smsSettings["key"];
                    string from = smsSettings["from"];

                    if (from != null && from.Trim().Length > 0
                        && key != null && key.Trim().Length > 0
                        && secret != null && secret.Trim().Length > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        public class smsResultData
        {
            public string numberSentFrom { get; set; }
            public string responseMessage { get; set; }
        }

        //
        // Use this method to have some rudimentary retry on common errors.  No logging here though, so bring this up higher if that is needed.
        //
        public static smsResultData SendSMSWithRetry(string to, string message, SecurityUser user = null)
        {
            int failureCounter = 0;
            bool success = false;

            smsResultData output = null;

            while (success == false)
            {
                try
                {
                    output = SendSMS(to, message, user);
                    success = true;
                }
                catch (Exception ex)
                {
                    if (failureCounter >= 3)
                    {
                        // hard exit after 3 failures
                        throw;
                    }

                    failureCounter++;
                    //CreateAuditEvent(AuditEngine.AuditType.Error, "Error caught sending text message to cell phone number " + textDetails.phoneNumber + ".  Will retry up to 3 times.  This is attempt " + failureCounter);

                    Random rnd = new Random();

                    //
                    // Wait between 1 and 3 seconds randonly between attempts
                    //
                    if (ex.Message.ToUpper().Contains("THROUGHPUT") == true)
                    {
                        // wait a bit before trying again because of the throughput error
                        int randomMillisecondsToWait = rnd.Next(1000, 3000);
                        Thread.Sleep(randomMillisecondsToWait);
                    }
                    else if (ex.Message.ToUpper().Contains("QUOTA EXCEEDED - REJECTED") == true)
                    {
                        //
                        // we're out of credits in our vendor account.  Wait 60 seconds for the auto top up at the vendor side to try and fix this
                        //
                        Thread.Sleep(60000);
                    }
                    else
                    {
                        //  don't bother with retries unless it's a throughput error.  Most likely errors at this point are unreachable number, or a number that can't be interpreted by Nexmo
                        throw;
                    }
                }
            }

            return output;
        }


        public static smsResultData SendSMS(string to, string message, SecurityUser user = null)
        {
            smsResultData output = null;

            if (message == null || message.Trim().Length == 0)
            {
                throw new Exception("Message cannot be null or empty when sending SMS");
            }

            if (to == null || to.Trim().Length == 0)
            {
                throw new Exception("To cannot be null or empty when sending SMS");
            }

            // remove any hyphens or periods pluses
            to = to.Replace("-", "");
            to = to.Replace(".", "");
            to = to.Replace("+", "");
            to = to.Replace(" ", "");
            to = to.Replace("  ", "");
            to = to.Replace(" ", "");   // differnt space?  check chr later
            to = to.Replace("(", "");
            to = to.Replace(")", "");

            if ((to.Length == 10 || to.Length == 11) == false)
            {
                throw new Exception("To number of " + to + " is an invalid length.  10 or 11 numbers are expected.");
            }


            long toAsLong;
            if (long.TryParse(to, out toAsLong) == false)
            {
                throw new Exception("To value of " + to + " is not a number");
            }

            // add a 1 to the front of the number if the length is 10
            if (to.Length == 10)
            {
                to = "1" + to;
            }

            //
            // This uses Nexmo.  It returns the JSON nexmo returns.
            //
            string connectionString = Configuration.GetStringConfigurationSetting("NexmoConnectionString");


            if (connectionString.Length > 0)
            {
                Dictionary<String, String> smsSettings = StringToDictionary(connectionString);

                if (smsSettings.ContainsKey("secret") == true &&
                    smsSettings.ContainsKey("key") == true &&
                    smsSettings.ContainsKey("from") == true)
                {
                    string secret = smsSettings["secret"];
                    string key = smsSettings["key"];
                    string from = smsSettings["from"];

                    //
                    // Pull one number out of the from list, and send form that, in round robin fashion
                    //
                    if (from.Contains(";") == true)
                    {
                        SMSSendCounterSingleton instance = SMSSendCounterSingleton.GetInstance();

                        string[] fromNumbers = from.Split(';');

                        int sendCount = instance.getSendCounter();

                        int remainder = 0;

                        if (sendCount >= fromNumbers.Length)
                        {
                            remainder = sendCount % fromNumbers.Length;
                        }
                        else
                        {
                            remainder = sendCount;
                        }

                        //
                        // Use the number from the remainder index position
                        //
                        if (remainder >= 0 && remainder < fromNumbers.Length)
                        {
                            from = fromNumbers[remainder];
                        }
                        else
                        {
                            from = fromNumbers[0];
                        }
                    }

                    string server = "https://rest.nexmo.com/sms/json";

                    //
                    // Turns out I don't need to include user and password in the call.  Those are needed for web based account management though.
                    //
                    string urlForSendingSMS = server + "?api_key=" + WebUtility.UrlEncode‌​(key) +
                                                    "&api_secret=" + WebUtility.UrlEncode‌​(secret) +
                                                    "&to=" + WebUtility.UrlEncode‌​(to) +
                                                    "&text=" + WebUtility.UrlEncode‌​(message);

                    if (from != null)
                    {
                        urlForSendingSMS = urlForSendingSMS + "&from=" + WebUtility.UrlEncode‌​(from);
                    }

                    //
                    // This is needed to allow TLS 1.2.  Without this, no connection allowed.  See https://stackoverflow.com/questions/28453353/http-post-error-an-existing-connection-was-forcibly-closed-by-the-remote-host#35015311
                    //
                    // Hit issues iwth closed connections - trying this https://stackoverflow.com/questions/21446489/unable-to-read-data-from-the-transport-connection-an-existing-connection-was-fo
                    //
                    //System.Net.ServicePointManager.SecurityProtocol = System.Net.ServicePointManager.SecurityProtocol | System.Net.SecurityProtocolType.Tls12;
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                    System.Net.ServicePointManager.Expect100Continue = true;

                    /*
                    var request = (HttpWebRequest)WebRequest.Create(urlForSendingSMS);
                    request.AllowWriteStreamBuffering = true;
                    request.ContentType = "text/xml";
                    request.Method = "GET";
                    request.KeepAlive = false;
                    request.ProtocolVersion = HttpVersion.Version10;
                    request.ServicePoint.ConnectionLimit = 1;
                    */


                    string responseFromServer = null;

                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlForSendingSMS);

                            using (var response = client.Send(request))
                            {
                                response.EnsureSuccessStatusCode();

                                using (var receiveStream = response.Content.ReadAsStream())
                                {
                                    using (StreamReader sr = new StreamReader(receiveStream))
                                    {
                                        responseFromServer = sr.ReadToEnd().Trim();

                                        output = new smsResultData();
                                        output.numberSentFrom = from;
                                        output.responseMessage = responseFromServer;

                                        SMSSendCounterSingleton instance = SMSSendCounterSingleton.GetInstance();

                                        instance.incrementSendCounter();

                                        //
                                        // Force the close after we're done
                                        //
                                        sr.Close();
                                    }

                                    // force cleanup
                                    receiveStream.Close();
                                }


                                //
                                // determine if the send was successful
                                //
                                if (responseFromServer != null && responseFromServer.Length > 0)
                                {
                                    Dictionary<string, object> responseObject;

                                    try
                                    {
                                        responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseFromServer);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("Error caught during interpreting JSON results from SMS Provider.  Response received is " + responseFromServer, ex);
                                    }

                                    string status = "";
                                    int messageCount = 0;

                                    if (responseObject.ContainsKey("message-count") == true)
                                    {
                                        if (int.TryParse((string)responseObject["message-count"], out messageCount) == false)
                                        {
                                            throw new Exception("Unable to get integer message count from response of " + responseFromServer);
                                        }

                                        if (messageCount > 0)
                                        {
                                            Newtonsoft.Json.Linq.JArray responseMessages = (Newtonsoft.Json.Linq.JArray)responseObject["messages"];

                                            status = responseMessages[0]["status"].ToString();

                                            //
                                            // Look for a status of 0 in the first message to ascertain success of the batch.
                                            //
                                            if (status == "0")
                                            {
                                                CreateExternalCommunicationForSMS(to, message, true, responseFromServer, user, null);
                                            }
                                            else
                                            {
                                                string toFromResponse = responseMessages[0]["to"].ToString();
                                                string errorText = responseMessages[0]["error-text"].ToString();

                                                string errorMessage = "Could not send text message to '" + toFromResponse + "'.  Text message service reported failure with message '" + errorText + "'";

                                                throw new Exception(errorMessage);
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Unexpected message-count in JSON object.  Expecting greater than 0.  Response from server is: " + responseFromServer);
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Unable to find 'message-count' property in JSON result.  Response received is " + responseFromServer);
                                    }
                                }
                                else
                                {
                                    throw new Exception("No response was received from SMS provider");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //
                            // Record the unsuccessful sent attempt.  This assumes that all non success pathways will throw an exception that will and here.
                            //
                            CreateExternalCommunicationForSMS(to, message, false, responseFromServer, user, ex);

                            throw;
                        }
                    }
                }
            }

            return output;
        }

        /*
 * This returns a value between 0 and 1.  1 means 100% positive that user is not a robot, and 0 is that it is a robot.  It is a range of trust, so the consumer nneds
 * to do what they will with the input.
 * 
 * */
        public static double RecaptchaVerify(string responseToken, string userIPAddress = null)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.ServicePointManager.SecurityProtocol | System.Net.SecurityProtocolType.Tls12;

            try
            {
                string recaptchaKeysString = Configuration.GetStringConfigurationSetting("RecaptchaKeys");


                if (recaptchaKeysString.Length > 0)
                {
                    Dictionary<String, String> recaptchaKeys = StringToDictionary(recaptchaKeysString);
                    /*
                     * 
                     * 
secret	Required. The shared key between your site and reCAPTCHA.
response	Required. The user response token provided by the reCAPTCHA client-side integration on your site.
remoteip	Optional. The user's IP address.
                     * */

                    string secret = recaptchaKeys["secret"];

                    var recaptchaVerifyURL = "https://www.google.com/recaptcha/api/siteverify?secret=" + WebUtility.UrlEncode(secret) + "&response=" + WebUtility.UrlEncode(responseToken);

                    if (userIPAddress != null)
                    {
                        recaptchaVerifyURL += "&remoteip=" + WebUtility.UrlEncode(userIPAddress);
                    }

                    //var request = (HttpWebRequest)WebRequest.Create(recaptchaVerifyURL);
                    //request.AllowWriteStreamBuffering = true;
                    //request.ContentType = "text/xml";
                    //request.Method = "POST";
                    //request.KeepAlive = false;
                    //request.ContentLength = 0;
                    //request.ProtocolVersion = HttpVersion.Version10;
                    //request.ServicePoint.ConnectionLimit = 1;
                    string responseFromServer;

                    try
                    {
                        //using (WebResponse resp = request.GetResponse())

                        using (HttpClient client = new HttpClient())
                        {

                            //using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            //{
                            //    responseFromServer = sr.ReadToEnd().Trim();

                            //    sr.Close();
                            //}

                            //resp.Close();


                            var task = Task.Run(() => client.PostAsync(recaptchaVerifyURL, null));
                            task.Wait();

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
                        }


                        if (responseFromServer != null && responseFromServer.Length > 0)
                        {
                            Dictionary<string, object> responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseFromServer);

                            bool success = (bool)responseObject["success"];

                            if (success == true)
                            {
                                double score = (double)responseObject["score"];

                                return score;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        responseFromServer = ex.Message + "<br />" + ex.InnerException.Message + "<br /><br />";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error caught during getting Recaptcha verify string.  Details are: " + ex.ToString());
                throw;
            }

            return 0;
        }





        //
        // Make this public so that other functions can call it in other wrapper business logic functions that aren't necessarily running with web context base classes
        //

        //
        // As simple as possible a logging function to use for ad-hoc logging needs
        //
        public static void CreateAuditEvent(string message, Exception ex = null)
        {
            if (ex == null)
            {
                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, message, true);
            }
            else
            {
                CreateAuditEvent(AuditEngine.AuditType.Error, message, false, null, null, null, null, ex);
            }
        }


        public static void CreateAuditEvent(AuditEngine.AuditType auditType, string message, Boolean success, SecurityUser user = null, string session = null, string module = null, string entity = null, Exception ex = null)
        {
            string environmentName = Configuration.GetStringConfigurationSetting("EnvironmentName");

            List<string> errors = null;

            if (ex != null)
            {
                errors = new List<string>();

                Exception subEx = ex;

                //
                // First put in the entire error written as a string
                //
                errors.Add(ex.ToString());


                while (subEx != null)
                {
                    errors.Add(subEx.Message + " - " + subEx.ToString());
                    subEx = subEx.InnerException;
                }
            }

            DateTime startTime = DateTime.UtcNow;
            DateTime stopTime = DateTime.UtcNow;


            AuditEngine a = Foundation.Auditor.AuditEngine.Instance;
            a.CreateAuditEvent(startTime,
                stopTime,
                success,
                AuditEngine.AuditAccessType.APIRequest,
                auditType,
                user != null ? user.accountName : "System",
                session,
                null,
                null,
                module,
                entity,
                null,
                environmentName,
                null,
                System.Threading.Thread.CurrentThread.ManagedThreadId,
                message,
                null,
                null,
                errors);

        }

        private static void CreateExternalCommunicationForEmail(List<String> toAddresses, List<String> ccAddresses, List<String> bccAddresses, string subject, string body, bool completedSuccessfully, string responseFromServer, SecurityUser user, Exception ex)
        {
            // string environmentName = ConfigurationManager.AppSettings["EnvironmentName"];

            AuditEngine a = Foundation.Auditor.AuditEngine.Instance;

            try
            {
                a.CreateExternalCommunication(DateTime.UtcNow, completedSuccessfully, AuditEngine.ExternalCommunicationType.Email, user != null ? user.accountName : null, toAddresses, ccAddresses, bccAddresses, subject, body, responseFromServer, ex);
            }
            catch (Exception)
            {
                return;
            }
        }

        private static void CreateExternalCommunicationForSMS(string phoneNumber, string message, bool completedSuccessfully, string responseFromServer, SecurityUser user, Exception ex)
        {
            try
            {
                List<string> recipient = new List<string>();

                recipient.Add(phoneNumber);

                Foundation.Auditor.AuditEngine.Instance.CreateExternalCommunication(DateTime.UtcNow, completedSuccessfully, AuditEngine.ExternalCommunicationType.SMS, user != null ? user.accountName : null, recipient, null, null, null, message, responseFromServer, ex);
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
