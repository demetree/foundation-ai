// ============================================================================
//
// CertificateInspector.cs — TLS certificate inspector.
//
// Connects to a host via TLS, retrieves the server certificate, and returns
// detailed information including subject, issuer, validity, chain status,
// SANs, and key properties.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Foundation.Networking.Locksmith.Configuration;

namespace Foundation.Networking.Locksmith.Services
{
    /// <summary>
    ///
    /// Inspects TLS certificates by connecting to remote hosts and
    /// extracting certificate details.
    ///
    /// </summary>
    public class CertificateInspector
    {
        private readonly LocksmithConfiguration _config;


        public CertificateInspector(LocksmithConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Inspects the TLS certificate on the specified host and port.
        /// </summary>
        public async Task<CertificateInfo> InspectAsync(string host, int port = 443, CancellationToken cancellationToken = default)
        {
            CertificateInfo info = new CertificateInfo
            {
                Host = host,
                Port = port,
                InspectedAtUtc = DateTime.UtcNow
            };

            X509Certificate2 serverCert = null;
            X509Chain chain = null;
            SslPolicyErrors policyErrors = SslPolicyErrors.None;

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    //
                    // Connect with timeout
                    //
                    Task connectTask = client.ConnectAsync(host, port);
                    Task timeoutTask = Task.Delay(_config.InspectTimeoutMs, cancellationToken);
                    Task completedTask = await Task.WhenAny(connectTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        info.Success = false;
                        info.ErrorMessage = "Connection timed out";
                        return info;
                    }

                    if (connectTask.IsFaulted == true)
                    {
                        info.Success = false;
                        info.ErrorMessage = connectTask.Exception?.InnerException?.Message ?? "Connection failed";
                        return info;
                    }

                    //
                    // Perform TLS handshake and capture the certificate
                    //
                    using (SslStream sslStream = new SslStream(
                        client.GetStream(),
                        false,
                        (sender, certificate, certChain, errors) =>
                        {
                            //
                            // Capture the cert and chain for inspection
                            //
                            if (certificate != null)
                            {
                                serverCert = new X509Certificate2(certificate);
                            }

                            chain = certChain;
                            policyErrors = errors;

                            //
                            // Accept all certs — we're inspecting, not validating for connectivity
                            //
                            return true;
                        }))
                    {
                        SslClientAuthenticationOptions authOptions = new SslClientAuthenticationOptions
                        {
                            TargetHost = host
                        };

                        await sslStream.AuthenticateAsClientAsync(authOptions, cancellationToken);

                        //
                        // Capture TLS version
                        //
                        info.TlsVersion = sslStream.SslProtocol.ToString();
                    }
                }

                if (serverCert == null)
                {
                    info.Success = false;
                    info.ErrorMessage = "No certificate received from server";
                    return info;
                }

                //
                // ── Extract certificate details ──────────────────────────
                //
                info.Success = true;
                info.Subject = serverCert.Subject;
                info.Issuer = serverCert.Issuer;
                info.SerialNumber = serverCert.SerialNumber;
                info.Thumbprint = serverCert.Thumbprint;
                info.SignatureAlgorithm = serverCert.SignatureAlgorithm.FriendlyName ?? string.Empty;

                //
                // Extract the common name from the subject
                //
                info.CommonName = ExtractCommonName(serverCert.Subject);

                //
                // Public key info
                //
                if (serverCert.PublicKey != null)
                {
                    AsymmetricAlgorithm key = serverCert.PublicKey.GetRSAPublicKey() as AsymmetricAlgorithm
                        ?? serverCert.PublicKey.GetECDsaPublicKey() as AsymmetricAlgorithm;

                    if (key != null)
                    {
                        info.PublicKeyInfo = key.SignatureAlgorithm + " " + key.KeySize + " bits";
                    }
                }

                //
                // Validity dates
                //
                info.NotBeforeUtc = serverCert.NotBefore.ToUniversalTime();
                info.NotAfterUtc = serverCert.NotAfter.ToUniversalTime();
                info.DaysUntilExpiry = (int)(info.NotAfterUtc - DateTime.UtcNow).TotalDays;
                info.IsExpired = DateTime.UtcNow > info.NotAfterUtc;

                //
                // Subject Alternative Names
                //
                info.SubjectAlternativeNames = ExtractSans(serverCert);

                //
                // Chain validation
                //
                ValidateChain(serverCert, info);
            }
            catch (Exception ex)
            {
                info.Success = false;
                info.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
            }
            finally
            {
                serverCert?.Dispose();
            }

            return info;
        }


        // ── Helpers ───────────────────────────────────────────────────────


        /// <summary>
        /// Extracts the common name (CN) from a certificate subject string.
        /// </summary>
        private static string ExtractCommonName(string subject)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return string.Empty;
            }

            //
            // Subject format: "CN=example.com, O=Example Inc, ..."
            //
            string[] parts = subject.Split(',');

            foreach (string part in parts)
            {
                string trimmed = part.Trim();

                if (trimmed.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring(3).Trim();
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// Extracts Subject Alternative Names from the certificate.
        /// </summary>
        private static List<string> ExtractSans(X509Certificate2 cert)
        {
            List<string> sans = new List<string>();

            foreach (X509Extension ext in cert.Extensions)
            {
                //
                // OID 2.5.29.17 is Subject Alternative Name
                //
                if (ext.Oid?.Value == "2.5.29.17")
                {
                    AsnEncodedData asnData = new AsnEncodedData(ext.Oid, ext.RawData);
                    string formatted = asnData.Format(true);

                    if (string.IsNullOrWhiteSpace(formatted) == false)
                    {
                        string[] lines = formatted.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string line in lines)
                        {
                            string trimmed = line.Trim();

                            //
                            // Format is typically "DNS Name=example.com"
                            //
                            if (trimmed.StartsWith("DNS Name=", StringComparison.OrdinalIgnoreCase))
                            {
                                sans.Add(trimmed.Substring(9).Trim());
                            }
                            else if (trimmed.Contains("="))
                            {
                                sans.Add(trimmed);
                            }
                        }
                    }

                    break;
                }
            }

            return sans;
        }


        /// <summary>
        /// Validates the certificate chain and populates chain status info.
        /// </summary>
        private static void ValidateChain(X509Certificate2 cert, CertificateInfo info)
        {
            using (X509Chain validationChain = new X509Chain())
            {
                validationChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                validationChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

                bool isValid = validationChain.Build(cert);

                info.ChainIsValid = isValid;
                info.ChainLength = validationChain.ChainElements.Count;

                if (isValid == false)
                {
                    foreach (X509ChainStatus status in validationChain.ChainStatus)
                    {
                        info.ChainStatusMessages.Add(status.StatusInformation);
                    }
                }
            }
        }
    }
}
