using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BMC.Rebrickable.Api.Models.Responses;


namespace BMC.Rebrickable.Api
{
    /// <summary>
    ///
    /// Complete strongly-typed client for the Rebrickable REST API v3.
    ///
    /// Handles authentication, pagination, rate limiting, and serialisation.
    /// This is a pure HTTP-level service with no database dependencies — higher-level
    /// consumers compose it with EF/database access as needed.
    ///
    /// Usage:
    ///     var client = new RebrickableApiClient("your-api-key", msg => Console.WriteLine(msg));
    ///     var colors = await client.GetColorsAsync();
    ///
    /// </summary>
    public class RebrickableApiClient : IDisposable
    {
        private const string BaseUrl = "https://rebrickable.com/api/v3";
        private const int DefaultPageSize = 1000;
        private const int ThrottleDelayMs = 200;

        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;
        private readonly Action<string> _log;

        private readonly JsonSerializerOptions _jsonOptions;


        /// <summary>
        /// Current Rebrickable API rate limit state, updated after every request.
        /// </summary>
        public RateLimitInfo RateLimit { get; private set; } = new RateLimitInfo();


        /// <summary>
        /// Creates a new RebrickableApiClient.
        /// </summary>
        /// <param name="apiKey">Rebrickable API key (the key itself, without the 'key ' prefix).</param>
        /// <param name="log">Optional logging callback.</param>
        /// <param name="httpClient">Optional HttpClient for dependency injection. If null, a new one is created.</param>
        public RebrickableApiClient(string apiKey, Action<string> log = null, HttpClient httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key is required.", nameof(apiKey));

            _log = log ?? (_ => { });

            if (httpClient != null)
            {
                _httpClient = httpClient;
                _ownsHttpClient = false;
            }
            else
            {
                _httpClient = new HttpClient();
                _ownsHttpClient = true;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BMC-Rebrickable-Api/1.0");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }


        #region LEGO Catalog — Colors

        /// <summary>Get a paginated list of all colours.</summary>
        public Task<PagedResponse<RebrickableColor>> GetColorsAsync(int? page = null, int? pageSize = null, string ordering = null)
        {
            string url = BuildUrl("/lego/colors/", page, pageSize, ordering);
            return GetAsync<PagedResponse<RebrickableColor>>(url);
        }


        /// <summary>Get all colours across all pages.</summary>
        public Task<List<RebrickableColor>> GetAllColorsAsync(string ordering = null)
        {
            return GetAllPagesAsync<RebrickableColor>("/lego/colors/", ordering);
        }


        /// <summary>Get details about a specific colour.</summary>
        public Task<RebrickableColor> GetColorAsync(int id)
        {
            return GetAsync<RebrickableColor>($"{BaseUrl}/lego/colors/{id}/");
        }

        #endregion


        #region LEGO Catalog — Elements

        /// <summary>Get details about a specific element (part+colour combination).</summary>
        public Task<RebrickableElement> GetElementAsync(string elementId)
        {
            return GetAsync<RebrickableElement>($"{BaseUrl}/lego/elements/{Uri.EscapeDataString(elementId)}/");
        }

        #endregion


        #region LEGO Catalog — Minifigs

        /// <summary>Get a paginated list of minifigs.</summary>
        public Task<PagedResponse<RebrickableMinifig>> GetMinifigsAsync(
            string search = null, int? minParts = null, int? maxParts = null,
            string inSetNum = null, string inThemeId = null,
            string ordering = null, int? page = null, int? pageSize = null)
        {
            var extra = new Dictionary<string, string>();
            if (search != null) extra["search"] = search;
            if (minParts.HasValue) extra["min_parts"] = minParts.Value.ToString();
            if (maxParts.HasValue) extra["max_parts"] = maxParts.Value.ToString();
            if (inSetNum != null) extra["in_set_num"] = inSetNum;
            if (inThemeId != null) extra["in_theme_id"] = inThemeId;

            string url = BuildUrl("/lego/minifigs/", page, pageSize, ordering, extra);
            return GetAsync<PagedResponse<RebrickableMinifig>>(url);
        }


        /// <summary>Get details for a specific minifig.</summary>
        public Task<RebrickableMinifig> GetMinifigAsync(string figNum)
        {
            return GetAsync<RebrickableMinifig>($"{BaseUrl}/lego/minifigs/{Uri.EscapeDataString(figNum)}/");
        }


        /// <summary>Get a paginated list of parts in a minifig.</summary>
        public Task<PagedResponse<RebrickableInventoryPart>> GetMinifigPartsAsync(
            string figNum, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/lego/minifigs/{Uri.EscapeDataString(figNum)}/parts/", page, pageSize);
            return GetAsync<PagedResponse<RebrickableInventoryPart>>(url);
        }


        /// <summary>Get a paginated list of sets a minifig has appeared in.</summary>
        public Task<PagedResponse<RebrickableSet>> GetMinifigSetsAsync(
            string figNum, string ordering = null, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/lego/minifigs/{Uri.EscapeDataString(figNum)}/sets/", page, pageSize, ordering);
            return GetAsync<PagedResponse<RebrickableSet>>(url);
        }

        #endregion


        #region LEGO Catalog — Part Categories

        /// <summary>Get a paginated list of all part categories.</summary>
        public Task<PagedResponse<RebrickablePartCategory>> GetPartCategoriesAsync(
            string ordering = null, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl("/lego/part_categories/", page, pageSize, ordering);
            return GetAsync<PagedResponse<RebrickablePartCategory>>(url);
        }


        /// <summary>Get all part categories across all pages.</summary>
        public Task<List<RebrickablePartCategory>> GetAllPartCategoriesAsync(string ordering = null)
        {
            return GetAllPagesAsync<RebrickablePartCategory>("/lego/part_categories/", ordering);
        }


        /// <summary>Get details about a specific part category.</summary>
        public Task<RebrickablePartCategory> GetPartCategoryAsync(int id)
        {
            return GetAsync<RebrickablePartCategory>($"{BaseUrl}/lego/part_categories/{id}/");
        }

        #endregion


        #region LEGO Catalog — Parts

        /// <summary>Get a paginated list of parts with optional filters.</summary>
        public Task<PagedResponse<RebrickablePart>> GetPartsAsync(
            string search = null, string partNum = null, string partNums = null,
            string partCatId = null, string colorId = null,
            string bricklinkId = null, string brickowlId = null,
            string legoId = null, string ldrawId = null,
            string ordering = null, int? page = null, int? pageSize = null)
        {
            var extra = new Dictionary<string, string>();
            if (search != null) extra["search"] = search;
            if (partNum != null) extra["part_num"] = partNum;
            if (partNums != null) extra["part_nums"] = partNums;
            if (partCatId != null) extra["part_cat_id"] = partCatId;
            if (colorId != null) extra["color_id"] = colorId;
            if (bricklinkId != null) extra["bricklink_id"] = bricklinkId;
            if (brickowlId != null) extra["brickowl_id"] = brickowlId;
            if (legoId != null) extra["lego_id"] = legoId;
            if (ldrawId != null) extra["ldraw_id"] = ldrawId;

            string url = BuildUrl("/lego/parts/", page, pageSize, ordering, extra);
            return GetAsync<PagedResponse<RebrickablePart>>(url);
        }


        /// <summary>Get details about a specific part.</summary>
        public Task<RebrickablePart> GetPartAsync(string partNum)
        {
            return GetAsync<RebrickablePart>($"{BaseUrl}/lego/parts/{Uri.EscapeDataString(partNum)}/");
        }


        /// <summary>Get a paginated list of all colours a part has appeared in.</summary>
        public Task<PagedResponse<RebrickablePartColor>> GetPartColorsAsync(
            string partNum, string ordering = null, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/lego/parts/{Uri.EscapeDataString(partNum)}/colors/", page, pageSize, ordering);
            return GetAsync<PagedResponse<RebrickablePartColor>>(url);
        }


        /// <summary>Get details about a specific part/colour combination.</summary>
        public Task<RebrickablePartColor> GetPartColorAsync(string partNum, int colorId)
        {
            return GetAsync<RebrickablePartColor>($"{BaseUrl}/lego/parts/{Uri.EscapeDataString(partNum)}/colors/{colorId}/");
        }


        /// <summary>Get a paginated list of sets a part/colour combination has appeared in.</summary>
        public Task<PagedResponse<RebrickableSet>> GetPartColorSetsAsync(
            string partNum, int colorId, string ordering = null, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/lego/parts/{Uri.EscapeDataString(partNum)}/colors/{colorId}/sets/", page, pageSize, ordering);
            return GetAsync<PagedResponse<RebrickableSet>>(url);
        }

        #endregion


        #region LEGO Catalog — Sets

        /// <summary>Get a paginated list of sets with optional filters.</summary>
        public Task<PagedResponse<RebrickableSet>> GetSetsAsync(
            string search = null, string themeId = null,
            int? minYear = null, int? maxYear = null,
            int? minParts = null, int? maxParts = null,
            string ordering = null, int? page = null, int? pageSize = null)
        {
            var extra = new Dictionary<string, string>();
            if (search != null) extra["search"] = search;
            if (themeId != null) extra["theme_id"] = themeId;
            if (minYear.HasValue) extra["min_year"] = minYear.Value.ToString();
            if (maxYear.HasValue) extra["max_year"] = maxYear.Value.ToString();
            if (minParts.HasValue) extra["min_parts"] = minParts.Value.ToString();
            if (maxParts.HasValue) extra["max_parts"] = maxParts.Value.ToString();

            string url = BuildUrl("/lego/sets/", page, pageSize, ordering, extra);
            return GetAsync<PagedResponse<RebrickableSet>>(url);
        }


        /// <summary>Get details for a specific set.</summary>
        public Task<RebrickableSet> GetSetAsync(string setNum)
        {
            return GetAsync<RebrickableSet>($"{BaseUrl}/lego/sets/{Uri.EscapeDataString(setNum)}/");
        }


        /// <summary>Get a paginated list of alternate builds for a set.</summary>
        public Task<PagedResponse<RebrickableAlternateBuild>> GetSetAlternatesAsync(
            string setNum, string ordering = null, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/lego/sets/{Uri.EscapeDataString(setNum)}/alternates/", page, pageSize, ordering);
            return GetAsync<PagedResponse<RebrickableAlternateBuild>>(url);
        }


        /// <summary>Get a paginated list of minifigs in a set.</summary>
        public Task<PagedResponse<RebrickableInventoryMinifig>> GetSetMinifigsAsync(
            string setNum, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/lego/sets/{Uri.EscapeDataString(setNum)}/minifigs/", page, pageSize);
            return GetAsync<PagedResponse<RebrickableInventoryMinifig>>(url);
        }


        /// <summary>Get a paginated list of parts in a set.</summary>
        public Task<PagedResponse<RebrickableInventoryPart>> GetSetPartsAsync(
            string setNum, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/lego/sets/{Uri.EscapeDataString(setNum)}/parts/", page, pageSize);
            return GetAsync<PagedResponse<RebrickableInventoryPart>>(url);
        }


        /// <summary>Get all parts in a set across all pages.</summary>
        public Task<List<RebrickableInventoryPart>> GetAllSetPartsAsync(string setNum)
        {
            return GetAllPagesAsync<RebrickableInventoryPart>($"/lego/sets/{Uri.EscapeDataString(setNum)}/parts/");
        }


        /// <summary>Get a paginated list of sub-sets in a set.</summary>
        public Task<PagedResponse<RebrickableInventorySet>> GetSetSetsAsync(
            string setNum, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/lego/sets/{Uri.EscapeDataString(setNum)}/sets/", page, pageSize);
            return GetAsync<PagedResponse<RebrickableInventorySet>>(url);
        }

        #endregion


        #region LEGO Catalog — Themes

        /// <summary>Get a paginated list of all themes.</summary>
        public Task<PagedResponse<RebrickableTheme>> GetThemesAsync(
            string ordering = null, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl("/lego/themes/", page, pageSize, ordering);
            return GetAsync<PagedResponse<RebrickableTheme>>(url);
        }


        /// <summary>Get all themes across all pages.</summary>
        public Task<List<RebrickableTheme>> GetAllThemesAsync(string ordering = null)
        {
            return GetAllPagesAsync<RebrickableTheme>("/lego/themes/", ordering);
        }


        /// <summary>Get details about a specific theme.</summary>
        public Task<RebrickableTheme> GetThemeAsync(int id)
        {
            return GetAsync<RebrickableTheme>($"{BaseUrl}/lego/themes/{id}/");
        }

        #endregion


        #region User — Token and Profile

        /// <summary>
        /// Obtain a user token by providing Rebrickable credentials.
        /// The returned token is used for all subsequent user-level API calls.
        /// </summary>
        public async Task<RebrickableUserToken> GetUserTokenAsync(string username, string password)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            return await PostFormAsync<RebrickableUserToken>($"{BaseUrl}/users/_token/", content);
        }


        /// <summary>Get user profile details.</summary>
        public Task<RebrickableUserProfile> GetUserProfileAsync(string userToken)
        {
            return GetAsync<RebrickableUserProfile>($"{BaseUrl}/users/{userToken}/profile/");
        }

        #endregion


        #region User — Sets (collection-wide)

        /// <summary>Get a paginated list of all sets in the user's collection.</summary>
        public Task<PagedResponse<RebrickableUserSet>> GetUserSetsAsync(
            string userToken, string search = null, string setNum = null,
            string themeId = null, int? minYear = null, int? maxYear = null,
            int? minParts = null, int? maxParts = null,
            string ordering = null, int? page = null, int? pageSize = null)
        {
            var extra = new Dictionary<string, string>();
            if (search != null) extra["search"] = search;
            if (setNum != null) extra["set_num"] = setNum;
            if (themeId != null) extra["theme_id"] = themeId;
            if (minYear.HasValue) extra["min_year"] = minYear.Value.ToString();
            if (maxYear.HasValue) extra["max_year"] = maxYear.Value.ToString();
            if (minParts.HasValue) extra["min_parts"] = minParts.Value.ToString();
            if (maxParts.HasValue) extra["max_parts"] = maxParts.Value.ToString();

            string url = BuildUrl($"/users/{userToken}/sets/", page, pageSize, ordering, extra);
            return GetAsync<PagedResponse<RebrickableUserSet>>(url);
        }


        /// <summary>Get all sets in the user's collection across all pages.</summary>
        public Task<List<RebrickableUserSet>> GetAllUserSetsAsync(string userToken, string ordering = null)
        {
            return GetAllPagesAsync<RebrickableUserSet>($"/users/{userToken}/sets/", ordering);
        }


        /// <summary>Get details about a specific set in the user's collection.</summary>
        public Task<RebrickableUserSet> GetUserSetAsync(string userToken, string setNum)
        {
            return GetAsync<RebrickableUserSet>($"{BaseUrl}/users/{userToken}/sets/{Uri.EscapeDataString(setNum)}/");
        }


        /// <summary>Add a set to the user's collection.</summary>
        public async Task<RebrickableUserSet> AddUserSetAsync(string userToken, string setNum, int quantity = 1, bool includeSpares = true)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("set_num", setNum),
                new KeyValuePair<string, string>("quantity", quantity.ToString()),
                new KeyValuePair<string, string>("include_spares", includeSpares ? "True" : "False")
            });

            return await PostFormAsync<RebrickableUserSet>($"{BaseUrl}/users/{userToken}/sets/", content);
        }


        /// <summary>Update a set's quantity in the user's collection. Pass quantity 0 to delete.</summary>
        public async Task<RebrickableUserSet> UpdateUserSetAsync(string userToken, string setNum, int quantity)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("quantity", quantity.ToString())
            });

            return await PutFormAsync<RebrickableUserSet>($"{BaseUrl}/users/{userToken}/sets/{Uri.EscapeDataString(setNum)}/", content);
        }


        /// <summary>Delete a set from all the user's set lists.</summary>
        public Task DeleteUserSetAsync(string userToken, string setNum)
        {
            return DeleteAsync($"{BaseUrl}/users/{userToken}/sets/{Uri.EscapeDataString(setNum)}/");
        }


        /// <summary>Sync the user's full set collection to a provided list.</summary>
        public async Task SyncUserSetsAsync(string userToken, List<object> sets)
        {
            string json = JsonSerializer.Serialize(sets, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await PostAsync($"{BaseUrl}/users/{userToken}/sets/sync/", content);
        }

        #endregion


        #region User — Set Lists

        /// <summary>Get a paginated list of the user's set lists.</summary>
        public Task<PagedResponse<RebrickableUserSetList>> GetUserSetListsAsync(
            string userToken, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/users/{userToken}/setlists/", page, pageSize);
            return GetAsync<PagedResponse<RebrickableUserSetList>>(url);
        }


        /// <summary>Get all set lists across all pages.</summary>
        public Task<List<RebrickableUserSetList>> GetAllUserSetListsAsync(string userToken)
        {
            return GetAllPagesAsync<RebrickableUserSetList>($"/users/{userToken}/setlists/");
        }


        /// <summary>Get details about a specific set list.</summary>
        public Task<RebrickableUserSetList> GetUserSetListAsync(string userToken, int listId)
        {
            return GetAsync<RebrickableUserSetList>($"{BaseUrl}/users/{userToken}/setlists/{listId}/");
        }


        /// <summary>Create a new set list.</summary>
        public async Task<RebrickableUserSetList> CreateUserSetListAsync(string userToken, string name, bool isBuildable = false)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("is_buildable", isBuildable ? "True" : "False")
            });

            return await PostFormAsync<RebrickableUserSetList>($"{BaseUrl}/users/{userToken}/setlists/", content);
        }


        /// <summary>Update a set list.</summary>
        public async Task<RebrickableUserSetList> UpdateUserSetListAsync(string userToken, int listId, string name, bool isBuildable = false)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("is_buildable", isBuildable ? "True" : "False")
            });

            return await PutFormAsync<RebrickableUserSetList>($"{BaseUrl}/users/{userToken}/setlists/{listId}/", content);
        }


        /// <summary>Delete a set list and all its sets.</summary>
        public Task DeleteUserSetListAsync(string userToken, int listId)
        {
            return DeleteAsync($"{BaseUrl}/users/{userToken}/setlists/{listId}/");
        }


        /// <summary>Get a paginated list of sets in a set list.</summary>
        public Task<PagedResponse<RebrickableUserSet>> GetUserSetListSetsAsync(
            string userToken, int listId, string ordering = null, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/users/{userToken}/setlists/{listId}/sets/", page, pageSize, ordering);
            return GetAsync<PagedResponse<RebrickableUserSet>>(url);
        }


        /// <summary>Add a set to a specific set list.</summary>
        public async Task<RebrickableUserSet> AddUserSetListSetAsync(
            string userToken, int listId, string setNum, int quantity = 1, bool includeSpares = true)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("set_num", setNum),
                new KeyValuePair<string, string>("quantity", quantity.ToString()),
                new KeyValuePair<string, string>("include_spares", includeSpares ? "True" : "False")
            });

            return await PostFormAsync<RebrickableUserSet>($"{BaseUrl}/users/{userToken}/setlists/{listId}/sets/", content);
        }


        /// <summary>Update a set in a specific set list.</summary>
        public async Task<RebrickableUserSet> UpdateUserSetListSetAsync(
            string userToken, int listId, string setNum, int? quantity = null, bool? includeSpares = null)
        {
            var pairs = new List<KeyValuePair<string, string>>();
            if (quantity.HasValue) pairs.Add(new KeyValuePair<string, string>("quantity", quantity.Value.ToString()));
            if (includeSpares.HasValue) pairs.Add(new KeyValuePair<string, string>("include_spares", includeSpares.Value ? "True" : "False"));

            var content = new FormUrlEncodedContent(pairs);
            return await PatchFormAsync<RebrickableUserSet>($"{BaseUrl}/users/{userToken}/setlists/{listId}/sets/{Uri.EscapeDataString(setNum)}/", content);
        }


        /// <summary>Remove a set from a specific set list.</summary>
        public Task DeleteUserSetListSetAsync(string userToken, int listId, string setNum)
        {
            return DeleteAsync($"{BaseUrl}/users/{userToken}/setlists/{listId}/sets/{Uri.EscapeDataString(setNum)}/");
        }


        /// <summary>Get all sets in a set list across all pages.</summary>
        public Task<List<RebrickableUserSet>> GetAllUserSetListSetsAsync(string userToken, int listId)
        {
            return GetAllPagesAsync<RebrickableUserSet>($"/users/{userToken}/setlists/{listId}/sets/");
        }

        #endregion


        #region User — Part Lists

        /// <summary>Get a paginated list of the user's part lists.</summary>
        public Task<PagedResponse<RebrickableUserPartList>> GetUserPartListsAsync(
            string userToken, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/users/{userToken}/partlists/", page, pageSize);
            return GetAsync<PagedResponse<RebrickableUserPartList>>(url);
        }


        /// <summary>Get all part lists across all pages.</summary>
        public Task<List<RebrickableUserPartList>> GetAllUserPartListsAsync(string userToken)
        {
            return GetAllPagesAsync<RebrickableUserPartList>($"/users/{userToken}/partlists/");
        }


        /// <summary>Get details about a specific part list.</summary>
        public Task<RebrickableUserPartList> GetUserPartListAsync(string userToken, int listId)
        {
            return GetAsync<RebrickableUserPartList>($"{BaseUrl}/users/{userToken}/partlists/{listId}/");
        }


        /// <summary>Create a new part list.</summary>
        public async Task<RebrickableUserPartList> CreateUserPartListAsync(string userToken, string name, bool isBuildable = false)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("is_buildable", isBuildable ? "True" : "False")
            });

            return await PostFormAsync<RebrickableUserPartList>($"{BaseUrl}/users/{userToken}/partlists/", content);
        }


        /// <summary>Update a part list.</summary>
        public async Task<RebrickableUserPartList> UpdateUserPartListAsync(string userToken, int listId, string name, bool isBuildable = false)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("is_buildable", isBuildable ? "True" : "False")
            });

            return await PutFormAsync<RebrickableUserPartList>($"{BaseUrl}/users/{userToken}/partlists/{listId}/", content);
        }


        /// <summary>Delete a part list and all its parts.</summary>
        public Task DeleteUserPartListAsync(string userToken, int listId)
        {
            return DeleteAsync($"{BaseUrl}/users/{userToken}/partlists/{listId}/");
        }


        /// <summary>Get a paginated list of parts in a part list.</summary>
        public Task<PagedResponse<RebrickableUserPartListPart>> GetUserPartListPartsAsync(
            string userToken, int listId, string ordering = null, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/users/{userToken}/partlists/{listId}/parts/", page, pageSize, ordering);
            return GetAsync<PagedResponse<RebrickableUserPartListPart>>(url);
        }


        /// <summary>Add a part to a part list.</summary>
        public async Task<RebrickableUserPartListPart> AddUserPartListPartAsync(
            string userToken, int listId, string partNum, int colorId, int quantity)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("part_num", partNum),
                new KeyValuePair<string, string>("color_id", colorId.ToString()),
                new KeyValuePair<string, string>("quantity", quantity.ToString())
            });

            return await PostFormAsync<RebrickableUserPartListPart>($"{BaseUrl}/users/{userToken}/partlists/{listId}/parts/", content);
        }


        /// <summary>Update a part's quantity in a part list.</summary>
        public async Task<RebrickableUserPartListPart> UpdateUserPartListPartAsync(
            string userToken, int listId, string partNum, int colorId, int quantity)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("quantity", quantity.ToString())
            });

            return await PutFormAsync<RebrickableUserPartListPart>(
                $"{BaseUrl}/users/{userToken}/partlists/{listId}/parts/{Uri.EscapeDataString(partNum)}/{colorId}/", content);
        }


        /// <summary>Remove a part from a part list.</summary>
        public Task DeleteUserPartListPartAsync(string userToken, int listId, string partNum, int colorId)
        {
            return DeleteAsync($"{BaseUrl}/users/{userToken}/partlists/{listId}/parts/{Uri.EscapeDataString(partNum)}/{colorId}/");
        }


        /// <summary>Get all parts in a part list across all pages.</summary>
        public Task<List<RebrickableUserPartListPart>> GetAllUserPartListPartsAsync(string userToken, int listId)
        {
            return GetAllPagesAsync<RebrickableUserPartListPart>($"/users/{userToken}/partlists/{listId}/parts/");
        }

        #endregion


        #region User — Lost Parts

        /// <summary>Get a paginated list of the user's lost parts.</summary>
        public Task<PagedResponse<RebrickableUserLostPart>> GetUserLostPartsAsync(
            string userToken, string ordering = null, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl($"/users/{userToken}/lost_parts/", page, pageSize, ordering);
            return GetAsync<PagedResponse<RebrickableUserLostPart>>(url);
        }


        /// <summary>Add a lost part.</summary>
        public async Task<RebrickableUserLostPart> AddUserLostPartAsync(string userToken, int invPartId, int lostQuantity = 1)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("inv_part_id", invPartId.ToString()),
                new KeyValuePair<string, string>("lost_quantity", lostQuantity.ToString())
            });

            return await PostFormAsync<RebrickableUserLostPart>($"{BaseUrl}/users/{userToken}/lost_parts/", content);
        }


        /// <summary>Delete a lost part entry.</summary>
        public Task DeleteUserLostPartAsync(string userToken, int id)
        {
            return DeleteAsync($"{BaseUrl}/users/{userToken}/lost_parts/{id}/");
        }


        /// <summary>Get all lost parts across all pages.</summary>
        public Task<List<RebrickableUserLostPart>> GetAllUserLostPartsAsync(string userToken)
        {
            return GetAllPagesAsync<RebrickableUserLostPart>($"/users/{userToken}/lost_parts/");
        }

        #endregion


        #region User — Aggregate Queries

        /// <summary>Get a paginated list of all parts in all the user's part lists.</summary>
        public Task<PagedResponse<RebrickableUserPartListPart>> GetUserPartsAsync(
            string userToken, string search = null, string partNum = null,
            int? partCatId = null, int? colorId = null,
            string ordering = null, int? page = null, int? pageSize = null)
        {
            var extra = new Dictionary<string, string>();
            if (search != null) extra["search"] = search;
            if (partNum != null) extra["part_num"] = partNum;
            if (partCatId.HasValue) extra["part_cat_id"] = partCatId.Value.ToString();
            if (colorId.HasValue) extra["color_id"] = colorId.Value.ToString();

            string url = BuildUrl($"/users/{userToken}/parts/", page, pageSize, ordering, extra);
            return GetAsync<PagedResponse<RebrickableUserPartListPart>>(url);
        }


        /// <summary>Get all parts from all part lists and sets combined (WARNING: resource intensive).</summary>
        public Task<PagedResponse<RebrickableUserPartListPart>> GetUserAllPartsAsync(
            string userToken, string partNum = null, int? partCatId = null, int? colorId = null,
            int? page = null, int? pageSize = null)
        {
            var extra = new Dictionary<string, string>();
            if (partNum != null) extra["part_num"] = partNum;
            if (partCatId.HasValue) extra["part_cat_id"] = partCatId.Value.ToString();
            if (colorId.HasValue) extra["color_id"] = colorId.Value.ToString();

            string url = BuildUrl($"/users/{userToken}/allparts/", page, pageSize, null, extra);
            return GetAsync<PagedResponse<RebrickableUserPartListPart>>(url);
        }


        /// <summary>Get a paginated list of all minifigs in the user's sets.</summary>
        public Task<PagedResponse<RebrickableUserMinifig>> GetUserMinifigsAsync(
            string userToken, string search = null, string figSetNum = null,
            string ordering = null, int? page = null, int? pageSize = null)
        {
            var extra = new Dictionary<string, string>();
            if (search != null) extra["search"] = search;
            if (figSetNum != null) extra["fig_set_num"] = figSetNum;

            string url = BuildUrl($"/users/{userToken}/minifigs/", page, pageSize, ordering, extra);
            return GetAsync<PagedResponse<RebrickableUserMinifig>>(url);
        }


        /// <summary>Check how many parts the user needs to build a specific set.</summary>
        public Task<RebrickableUserBuild> GetUserBuildAsync(string userToken, string setNum)
        {
            return GetAsync<RebrickableUserBuild>($"{BaseUrl}/users/{userToken}/build/{Uri.EscapeDataString(setNum)}/");
        }

        #endregion


        #region User — Badges

        /// <summary>Get a paginated list of all available badges.</summary>
        public Task<PagedResponse<RebrickableBadge>> GetBadgesAsync(
            string ordering = null, int? page = null, int? pageSize = null)
        {
            string url = BuildUrl("/users/badges/", page, pageSize, ordering);
            return GetAsync<PagedResponse<RebrickableBadge>>(url);
        }


        /// <summary>Get details about a specific badge.</summary>
        public Task<RebrickableBadge> GetBadgeAsync(int id)
        {
            return GetAsync<RebrickableBadge>($"{BaseUrl}/users/badges/{id}/");
        }

        #endregion


        #region Internal HTTP Helpers

        /// <summary>
        /// Automatically pages through all results for a paginated endpoint.
        /// Returns the full combined results list.
        /// </summary>
        private async Task<List<T>> GetAllPagesAsync<T>(string relativePath, string ordering = null)
        {
            var allResults = new List<T>();
            int page = 1;

            while (true)
            {
                string url = BuildUrl(relativePath, page, DefaultPageSize, ordering);
                var response = await GetAsync<PagedResponse<T>>(url);

                allResults.AddRange(response.Results);

                if (string.IsNullOrEmpty(response.Next))
                    break;

                page++;
            }

            return allResults;
        }


        /// <summary>
        /// Performs a GET request, handles rate limiting, and deserialises the response.
        /// </summary>
        private async Task<T> GetAsync<T>(string url)
        {
            await Task.Delay(ThrottleDelayMs);

            int retries = 0;
            while (true)
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync(url))
                {
                    UpdateRateLimitInfo(response);

                    if (response.StatusCode == (HttpStatusCode)429)
                    {
                        int waitSeconds = GetRetryAfterSeconds(response, retries);
                        _log($"  ⏳ Rate limited — waiting {waitSeconds}s before retry...");
                        await Task.Delay(waitSeconds * 1000);
                        retries++;
                        continue;
                    }

                    string body = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new RebrickableApiException(
                            response.StatusCode,
                            response.ReasonPhrase,
                            body);
                    }

                    return JsonSerializer.Deserialize<T>(body, _jsonOptions);
                }
            }
        }


        /// <summary>
        /// Performs a POST request with form content.
        /// </summary>
        private async Task<T> PostFormAsync<T>(string url, FormUrlEncodedContent content)
        {
            await Task.Delay(ThrottleDelayMs);

            using (HttpResponseMessage response = await _httpClient.PostAsync(url, content))
            {
                UpdateRateLimitInfo(response);
                string body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new RebrickableApiException(
                        response.StatusCode,
                        response.ReasonPhrase,
                        body);
                }

                return JsonSerializer.Deserialize<T>(body, _jsonOptions);
            }
        }


        /// <summary>
        /// Performs a POST request with arbitrary content (e.g. JSON body).
        /// </summary>
        private async Task PostAsync(string url, HttpContent content)
        {
            await Task.Delay(ThrottleDelayMs);

            using (HttpResponseMessage response = await _httpClient.PostAsync(url, content))
            {
                UpdateRateLimitInfo(response);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();
                    throw new RebrickableApiException(
                        response.StatusCode,
                        response.ReasonPhrase,
                        body);
                }
            }
        }


        /// <summary>
        /// Performs a PUT request with form content.
        /// </summary>
        private async Task<T> PutFormAsync<T>(string url, FormUrlEncodedContent content)
        {
            await Task.Delay(ThrottleDelayMs);

            using (HttpResponseMessage response = await _httpClient.PutAsync(url, content))
            {
                UpdateRateLimitInfo(response);
                string body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new RebrickableApiException(
                        response.StatusCode,
                        response.ReasonPhrase,
                        body);
                }

                return JsonSerializer.Deserialize<T>(body, _jsonOptions);
            }
        }


        /// <summary>
        /// Performs a PATCH request with form content.
        /// </summary>
        private async Task<T> PatchFormAsync<T>(string url, FormUrlEncodedContent content)
        {
            await Task.Delay(ThrottleDelayMs);

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = content
            };

            using (HttpResponseMessage response = await _httpClient.SendAsync(request))
            {
                UpdateRateLimitInfo(response);
                string body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new RebrickableApiException(
                        response.StatusCode,
                        response.ReasonPhrase,
                        body);
                }

                return JsonSerializer.Deserialize<T>(body, _jsonOptions);
            }
        }


        /// <summary>
        /// Performs a DELETE request.
        /// </summary>
        private async Task DeleteAsync(string url)
        {
            await Task.Delay(ThrottleDelayMs);

            using (HttpResponseMessage response = await _httpClient.DeleteAsync(url))
            {
                UpdateRateLimitInfo(response);

                if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                {
                    string body = await response.Content.ReadAsStringAsync();
                    throw new RebrickableApiException(
                        response.StatusCode,
                        response.ReasonPhrase,
                        body);
                }
            }
        }

        #endregion


        #region URL Building

        /// <summary>
        /// Builds a full API URL with optional pagination and extra query parameters.
        /// </summary>
        private string BuildUrl(string relativePath, int? page = null, int? pageSize = null,
            string ordering = null, Dictionary<string, string> extra = null)
        {
            var sb = new StringBuilder(BaseUrl);
            sb.Append(relativePath);

            char separator = '?';

            if (page.HasValue)
            {
                sb.Append(separator).Append("page=").Append(page.Value);
                separator = '&';
            }

            if (pageSize.HasValue)
            {
                sb.Append(separator).Append("page_size=").Append(pageSize.Value);
                separator = '&';
            }
            else
            {
                // Default to max page size for efficiency
                sb.Append(separator).Append("page_size=").Append(DefaultPageSize);
                separator = '&';
            }

            if (!string.IsNullOrEmpty(ordering))
            {
                sb.Append(separator).Append("ordering=").Append(Uri.EscapeDataString(ordering));
                separator = '&';
            }

            if (extra != null)
            {
                foreach (var kvp in extra)
                {
                    sb.Append(separator).Append(kvp.Key).Append('=').Append(Uri.EscapeDataString(kvp.Value));
                    separator = '&';
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Extracts the Retry-After header value, or uses exponential backoff as fallback.
        /// </summary>
        private int GetRetryAfterSeconds(HttpResponseMessage response, int retryCount)
        {
            if (response.Headers.RetryAfter != null)
            {
                if (response.Headers.RetryAfter.Delta.HasValue)
                    return (int)response.Headers.RetryAfter.Delta.Value.TotalSeconds + 1;
            }

            // Exponential backoff: 2, 4, 8, 16... seconds
            return (int)Math.Pow(2, retryCount + 1);
        }

        /// <summary>
        /// Reads X-RateLimit-* headers from the response and updates the public RateLimit property.
        /// </summary>
        private void UpdateRateLimitInfo(HttpResponseMessage response)
        {
            try
            {
                if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues))
                {
                    if (int.TryParse(System.Linq.Enumerable.FirstOrDefault(remainingValues), out int remaining))
                        RateLimit.Remaining = remaining;
                }

                if (response.Headers.TryGetValues("X-RateLimit-Limit", out var limitValues))
                {
                    if (int.TryParse(System.Linq.Enumerable.FirstOrDefault(limitValues), out int limit))
                        RateLimit.Limit = limit;
                }

                if (response.Headers.TryGetValues("X-RateLimit-Reset", out var resetValues))
                {
                    if (int.TryParse(System.Linq.Enumerable.FirstOrDefault(resetValues), out int reset))
                        RateLimit.ResetSeconds = reset;
                }

                RateLimit.LastUpdated = DateTime.UtcNow;
            }
            catch
            {
                // Never let rate-limit parsing break actual API calls
            }
        }

        #endregion


        public void Dispose()
        {
            if (_ownsHttpClient)
                _httpClient?.Dispose();
        }
    }


    /// <summary>
    /// Tracks Rebrickable API rate limit state from response headers.
    /// </summary>
    public class RateLimitInfo
    {
        public int Remaining { get; set; } = -1;
        public int Limit { get; set; } = -1;
        public int ResetSeconds { get; set; }
        public DateTime LastUpdated { get; set; }

        /// <summary>True if we've received at least one rate limit header.</summary>
        public bool HasData => Remaining >= 0 && Limit > 0;

        /// <summary>Percentage of rate limit remaining (0-100).</summary>
        public int PercentRemaining => Limit > 0 ? (int)((Remaining / (double)Limit) * 100) : 100;
    }
}
