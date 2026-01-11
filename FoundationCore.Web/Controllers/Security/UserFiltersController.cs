using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Controllers.WebAPI
{
    [Authorize]
    public class UserFiltersController : SecureWebAPIController
    {
        private const string USER_FILTERS = "UserFilters";
        private const string USER_LAST_PAGE = "UserLastPage";

        public UserFiltersController() : base("Security", "UserFilters")
        {
            return;
        }

        [HttpGet]
        [Route("api/UserFilters/GetUserFilters")]
        public async Task<IActionResult> GetUserFilters(CancellationToken cancellationToken = default)
        {
            try
            {
                JsonNode data = await Foundation.Security.UserSettings.GetObjectSettingAsync(USER_FILTERS, await GetSecurityUserAsync(), cancellationToken);

                //
                // If filters exist in the user's settings, serialize the JsonNode directly back to JSON
                // and return it as the response body with the correct content type.
                //
                if (data != null)
                {
                    string jsonResponse = data.ToJsonString();

                    //
                    // Return the JSON string as the response body with proper encoding and content type.
                    // StringContent is used for explicit control over encoding and media type.
                    //
                    return Ok(new StringContent(jsonResponse, Encoding.UTF8, "application/json"));
                }
                else
                {
                    //
                    // No filters stored – return a 200 OK with an empty body
                    //
                    // This indicates success but no data
                    //
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new StringContent("Caught error getting user Settings.  Message is: " + ex.Message, System.Text.Encoding.UTF8, "text/plain"));
            }
        }


        [HttpPost]
        [Route("api/UserFilters/SaveUserFilters")]
        public async Task<IActionResult> SaveUserFilters(dynamic settings)
        {
            try
            {
                await Foundation.Security.UserSettings.SetObjectSettingAsync(USER_FILTERS, settings, await GetSecurityUserAsync());

                return Ok(new StringContent("{ \"Saved\": 1 }", System.Text.Encoding.UTF8, "application/JSON"));
            }
            catch (Exception ex)
            {
                return BadRequest(new StringContent("Caught error saving user Settings.  Message is: " + ex.Message, System.Text.Encoding.UTF8, "text/plain"));
            }
        }


        [HttpPost]
        [Route("api/UserFilters/ClearUserFilters")]
        public async Task<IActionResult> ClearUserFilters()
        {
            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                // Write null to the setting to clear it
                await Foundation.Security.UserSettings.SetObjectSettingAsync(USER_FILTERS, null, securityUser);

                // also blow away the last page
                await Foundation.Security.UserSettings.SetStringSettingAsync(USER_LAST_PAGE, "", securityUser);

                return Ok(new StringContent("{ \"Saved\": 1 }", System.Text.Encoding.UTF8, "application/JSON"));
            }
            catch (Exception ex)
            {
                return BadRequest(new StringContent("Caught error clearing user settings.  Message is: " + ex.Message, System.Text.Encoding.UTF8, "text/plain"));
            }
        }



        [HttpGet]
        [Route("api/UserFilters/GetUserLastPage")]
        public async Task<IActionResult> GetUserLastPage()
        {
            try
            {
                string page = await Foundation.Security.UserSettings.GetStringSettingAsync(USER_LAST_PAGE, await GetSecurityUserAsync());

                if (page != null)
                {
                    return Ok(new StringContent("{ \"page\": \"" + page.Replace("\"", "\"\"") + "\"}", System.Text.Encoding.UTF8, "application/JSON"));
                }
                else
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new StringContent("Caught error user last page.  Message is: " + ex.Message, System.Text.Encoding.UTF8, "text/plain"));
            }
        }

        [HttpPost]
        [Route("api/UserFilters/SaveUserLastPage")]
        public async Task<IActionResult> SaveUserLastPageAsync(string page)
        {
            try
            {
                await Foundation.Security.UserSettings.SetStringSettingAsync(USER_LAST_PAGE, page, GetSecurityUser());

                return Ok(new StringContent("{ \"Saved\": 1 }", System.Text.Encoding.UTF8, "application/JSON"));
            }
            catch (Exception ex)
            {
                return BadRequest(new StringContent("Caught error saving user last page.  Message is: " + ex.Message, System.Text.Encoding.UTF8, "text/plain"));
            }
        }
    }
}