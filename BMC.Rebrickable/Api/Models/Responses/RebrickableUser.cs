using System.Text.Json.Serialization;


namespace BMC.Rebrickable.Api.Models.Responses
{
    /// <summary>
    /// User token response from /users/_token/.
    /// </summary>
    public class RebrickableUserToken
    {
        [JsonPropertyName("user_token")]
        public string UserToken { get; set; }
    }


    /// <summary>
    /// User profile from /users/{token}/profile/.
    /// </summary>
    public class RebrickableUserProfile
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("last_activity")]
        public string LastActivity { get; set; }

        [JsonPropertyName("last_ip")]
        public string LastIp { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("avatar_img")]
        public string AvatarImg { get; set; }

        [JsonPropertyName("lego")]
        public RebrickableUserLegoStats Lego { get; set; }
    }


    /// <summary>
    /// LEGO collection stats within the user profile.
    /// </summary>
    public class RebrickableUserLegoStats
    {
        [JsonPropertyName("num_sets")]
        public int NumSets { get; set; }

        [JsonPropertyName("num_set_lists")]
        public int NumSetLists { get; set; }

        [JsonPropertyName("num_parts")]
        public int NumParts { get; set; }

        [JsonPropertyName("num_part_lists")]
        public int NumPartLists { get; set; }

        [JsonPropertyName("num_minifigs")]
        public int NumMinifigs { get; set; }

        [JsonPropertyName("num_lost_parts")]
        public int NumLostParts { get; set; }
    }


    /// <summary>
    /// A set in the user's collection from /users/{token}/sets/.
    /// </summary>
    public class RebrickableUserSet
    {
        [JsonPropertyName("set_num")]
        public string SetNum { get; set; }

        [JsonPropertyName("set_name")]
        public string SetName { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("include_spares")]
        public bool IncludeSpares { get; set; }

        [JsonPropertyName("list_id")]
        public int? ListId { get; set; }

        [JsonPropertyName("set")]
        public RebrickableSet Set { get; set; }
    }


    /// <summary>
    /// User's set list from /users/{token}/setlists/.
    /// </summary>
    public class RebrickableUserSetList
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("is_buildable")]
        public bool IsBuildable { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("num_sets")]
        public int NumSets { get; set; }
    }


    /// <summary>
    /// User's part list from /users/{token}/partlists/.
    /// </summary>
    public class RebrickableUserPartList
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("is_buildable")]
        public bool IsBuildable { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("num_parts")]
        public int NumParts { get; set; }
    }


    /// <summary>
    /// A part in a user's part list.
    /// </summary>
    public class RebrickableUserPartListPart
    {
        [JsonPropertyName("part")]
        public RebrickablePart Part { get; set; }

        [JsonPropertyName("color")]
        public RebrickableColor Color { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }


    /// <summary>
    /// A user's lost part from /users/{token}/lost_parts/.
    /// </summary>
    public class RebrickableUserLostPart
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("inv_part_id")]
        public int InvPartId { get; set; }

        [JsonPropertyName("lost_quantity")]
        public int LostQuantity { get; set; }

        [JsonPropertyName("part")]
        public RebrickablePart Part { get; set; }

        [JsonPropertyName("color")]
        public RebrickableColor Color { get; set; }
    }


    /// <summary>
    /// Build match result from /users/{token}/build/{set_num}/.
    /// </summary>
    public class RebrickableUserBuild
    {
        [JsonPropertyName("total_parts")]
        public int TotalParts { get; set; }

        [JsonPropertyName("num_have")]
        public int NumHave { get; set; }

        [JsonPropertyName("num_need")]
        public int NumNeed { get; set; }

        [JsonPropertyName("pct_have")]
        public double PctHave { get; set; }
    }


    /// <summary>
    /// Badge from /users/badges/.
    /// </summary>
    public class RebrickableBadge
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("descr")]
        public string Description { get; set; }
    }


    /// <summary>
    /// A user's minifig from /users/{token}/minifigs/.
    /// </summary>
    public class RebrickableUserMinifig
    {
        [JsonPropertyName("set_num")]
        public string SetNum { get; set; }

        [JsonPropertyName("set_name")]
        public string SetName { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("set_img_url")]
        public string SetImgUrl { get; set; }
    }
}
