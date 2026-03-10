using Foundation.Scheduler.Database;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.OAuth2PlatformClient;
using Intuit.Ipp.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Alias to disambiguate from Foundation.Scheduler.Database.Invoice
using QbInvoice = Intuit.Ipp.Data.Invoice;

// Traditional helper class for QuickBooks Online authentication & sync
// This follows your existing code style: clear, explicit, no heavy modern patterns
public class QuickBooksIntegrator
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;          // e.g., "https://yourapp.com/qbo/callback"
    private readonly string _environment;          // "sandbox" or "production"
    private readonly string _accessToken;          // Store securely per tenant/company
    private readonly string _refreshToken;
    private readonly string _realmId;              // Company ID from QuickBooks

    public QuickBooksIntegrator(
        string clientId,
        string clientSecret,
        string redirectUri,
        string environment,
        string accessToken,
        string refreshToken,
        string realmId)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _redirectUri = redirectUri;
        _environment = environment;
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        _realmId = realmId;
    }

    // Get OAuth authorization URL (call this to initiate user login flow)
    public string GetAuthorizationUrl(string state = null)
    {
        OAuth2Client oauthClient = new OAuth2Client(_clientId, _clientSecret, _redirectUri, _environment);

        List<string> scopes = new List<string>() { "com.intuit.quickbooks.accounting" }; // Add openid/profile/email if needed
        return oauthClient.GetAuthorizationURL(scopes, state);
    }

    // Exchange code for tokens after redirect callback
    public async Task<(string AccessToken, string RefreshToken, long ExpiresAt)> GetTokensFromCodeAsync(string code)
    {
        var oauthClient = new OAuth2Client(_clientId, _clientSecret, _redirectUri, _environment);
        var tokenResponse = await oauthClient.GetBearerTokenAsync(code);

        return (
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            tokenResponse.AccessTokenExpiresIn
        );
    }

    // Refresh access token when expired
    public async Task<(string NewAccessToken, string NewRefreshToken, DateTimeOffset NewExpiresAt)> RefreshAccessTokenAsync(string currentRefreshToken)
    {
        var oauthClient = new OAuth2Client(_clientId, _clientSecret, _redirectUri, _environment);

        var tokenResponse = await oauthClient.RefreshTokenAsync(currentRefreshToken);

        return (
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(tokenResponse.AccessTokenExpiresIn)
        );
    }

    // Create a ServiceContext for API calls
    private ServiceContext GetServiceContext()
    {
        var oauthValidator = new OAuth2RequestValidator(_accessToken);
        var context = new ServiceContext(_realmId, IntuitServicesType.QBO, oauthValidator);
        context.IppConfiguration.BaseUrl.Qbo = _environment == "sandbox"
            ? "https://sandbox-quickbooks.api.intuit.com/"
            : "https://quickbooks.api.intuit.com/";

        return context;
    }


    public async Task<string> SyncGiftAsInvoiceAsync(Gift gift)
    {
        if (gift == null)
        {
            throw new ArgumentNullException(nameof(gift));
        }

        if (string.IsNullOrEmpty(gift.constituent?.externalId))
        {
            throw new InvalidOperationException("Constituent must have a valid QuickBooks ExternalId");
        }

        ServiceContext context = GetServiceContext();
        DataService dataService = new DataService(context);

        QbInvoice invoice = new QbInvoice
        {
            DocNumber = $"SCH-GIFT-{gift.id}",
            TxnDate = gift.receivedDate,
            DueDate = gift.receivedDate.AddDays(30),
            CustomerRef = new ReferenceType { Value = gift.constituent.externalId.ToString() },
            Line = new[]
            {
            new Line
            {
                Amount = gift.amount,
                DetailType = LineDetailTypeEnum.SalesItemLineDetail,
                AnyIntuitObject = new SalesItemLineDetail
                {
                    ItemRef = new ReferenceType { Value = "1" } // Your QBO Donation Item ID
                }
            }
        }
        };

        var tcs = new TaskCompletionSource<QbInvoice>();

        // Attach the callback
        dataService.OnAddAsyncCompleted += (sender, e) =>
        {
            if (e.Error != null)
            {
                tcs.TrySetException(e.Error);
            }
            else
            {
                tcs.TrySetResult((QbInvoice)e.Entity);
            }
        };

        // Start the async operation
        dataService.AddAsync(invoice); // null = no user state

        // Await the TaskCompletionSource
        var addedInvoice = await tcs.Task;

        // Store QBO ID back
        // gift.ExternalId = addedInvoice.Id;
        // Save to DB...

        throw new NotImplementedException("Finish teh gift updating code here and the itemref above");

        //return addedInvoice.Id;
    }


    //
    // Sync a Gift/Pledge as a QuickBooks Invoice (traditional style)
    //
    public string SyncGiftAsInvoice(Gift gift)
    {
        if (gift == null)
        {
            throw new ArgumentNullException(nameof(gift));
        }

        if (string.IsNullOrEmpty(gift.constituent?.externalId))
        {
            throw new InvalidOperationException("Constituent must have a valid QuickBooks ExternalId");
        }

        ServiceContext context = GetServiceContext();
        DataService dataService = new DataService(context);

        QbInvoice invoice = new QbInvoice
        {
            DocNumber = $"SCH-GIFT-{gift.id}",
            TxnDate = gift.receivedDate,
            DueDate = gift.receivedDate.AddDays(30), // Adjust as needed
            CustomerRef = new ReferenceType { Value = gift.constituent.externalId.ToString() }, // Assume Constituent has QBO Customer ID stored
            Line = new[]
            {
                new Line
                {
                    Amount = gift.amount,
                    DetailType = LineDetailTypeEnum.SalesItemLineDetail,
                    AnyIntuitObject = new SalesItemLineDetail
                    {
                        ItemRef = new ReferenceType { Value = "1" } // Replace with your QBO Donation Item ID
                    }
                }
            }
        };

        try
        {
            QbInvoice addedInvoice = dataService.Add(invoice);

            // Store QBO ID back in your Gift for future reference
            // gift.ExternalId = addedInvoice.Id;
            // Save to DB...
            throw new NotImplementedException("Finish teh gift updating code here and the itemref above");

            //return addedInvoice.Id;
        }
        catch (Exception ex)
        {
            // Log and rethrow or handle gracefully
            Console.WriteLine($"QuickBooks sync failed: {ex.Message}");
            throw; // Or wrap in custom exception
        }
    }
}
