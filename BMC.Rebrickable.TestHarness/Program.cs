using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BMC.Rebrickable.Api;
using BMC.Rebrickable.Api.Models.Responses;


namespace BMC.Rebrickable.TestHarness
{
    /// <summary>
    ///
    /// Interactive CLI test harness for the Rebrickable API v3 client.
    ///
    /// Usage:
    ///     dotnet run -- --key YOUR_API_KEY
    ///     dotnet run                          (reads from REBRICKABLE_API_KEY env var)
    ///
    /// </summary>
    class Program
    {
        private static RebrickableApiClient _client;
        private static string _userToken;


        static async Task<int> Main(string[] args)
        {
            string apiKey = GetApiKey(args);
            if (apiKey == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: No API key provided.");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("  dotnet run -- --key YOUR_API_KEY");
                Console.WriteLine("  set REBRICKABLE_API_KEY=YOUR_API_KEY && dotnet run");
                return 1;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("║     Rebrickable API v3 — Interactive Test Harness  ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            _client = new RebrickableApiClient(apiKey, msg => {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(msg);
                Console.ResetColor();
            });

            Console.WriteLine("Type 'help' for available commands, 'exit' to quit.");
            Console.WriteLine();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("rebrickable> ");
                Console.ResetColor();

                string input = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input))
                    continue;

                string[] parts = ParseCommandLine(input);
                string command = parts[0].ToLowerInvariant();
                string[] cmdArgs = parts.Length > 1
                    ? parts.Skip(1).ToArray()
                    : Array.Empty<string>();

                try
                {
                    bool shouldExit = await HandleCommand(command, cmdArgs);
                    if (shouldExit) break;
                }
                catch (RebrickableApiException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"API Error {(int)ex.StatusCode}: {ex.ResponseBody}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }

            _client.Dispose();
            return 0;
        }


        static async Task<bool> HandleCommand(string command, string[] args)
        {
            switch (command)
            {
                case "help":
                    PrintHelp();
                    return false;

                case "exit":
                case "quit":
                case "q":
                    Console.WriteLine("Goodbye!");
                    return true;


                // ═══════════════════════════════════════
                // LEGO Catalog
                // ═══════════════════════════════════════

                case "colors":
                case "colours":
                    await HandleColors(args);
                    return false;

                case "color":
                case "colour":
                    await HandleColor(args);
                    return false;

                case "themes":
                    await HandleThemes(args);
                    return false;

                case "theme":
                    await HandleTheme(args);
                    return false;

                case "categories":
                    await HandleCategories(args);
                    return false;

                case "category":
                    await HandleCategory(args);
                    return false;

                case "parts":
                    await HandleParts(args);
                    return false;

                case "part":
                    await HandlePart(args);
                    return false;

                case "part-colors":
                case "part-colours":
                    await HandlePartColors(args);
                    return false;

                case "part-color":
                case "part-colour":
                    await HandlePartColor(args);
                    return false;

                case "part-color-sets":
                case "part-colour-sets":
                    await HandlePartColorSets(args);
                    return false;

                case "sets":
                    await HandleSets(args);
                    return false;

                case "set":
                    await HandleSet(args);
                    return false;

                case "set-parts":
                    await HandleSetParts(args);
                    return false;

                case "set-minifigs":
                    await HandleSetMinifigs(args);
                    return false;

                case "set-sets":
                    await HandleSetSets(args);
                    return false;

                case "set-alternates":
                    await HandleSetAlternates(args);
                    return false;

                case "minifigs":
                    await HandleMinifigs(args);
                    return false;

                case "minifig":
                    await HandleMinifig(args);
                    return false;

                case "minifig-parts":
                    await HandleMinifigParts(args);
                    return false;

                case "minifig-sets":
                    await HandleMinifigSets(args);
                    return false;

                case "element":
                    await HandleElement(args);
                    return false;


                // ═══════════════════════════════════════
                // User
                // ═══════════════════════════════════════

                case "login":
                    await HandleLogin(args);
                    return false;

                case "profile":
                    await HandleProfile();
                    return false;

                case "my-sets":
                    await HandleMySets(args);
                    return false;

                case "my-set":
                    await HandleMySet(args);
                    return false;

                case "add-set":
                    await HandleAddSet(args);
                    return false;

                case "update-set":
                    await HandleUpdateSet(args);
                    return false;

                case "remove-set":
                    await HandleRemoveSet(args);
                    return false;

                case "sync-sets":
                    await HandleSyncSets(args);
                    return false;

                case "my-setlists":
                    await HandleMySetLists();
                    return false;

                case "setlist":
                    await HandleSetList(args);
                    return false;

                case "create-setlist":
                    await HandleCreateSetList(args);
                    return false;

                case "update-setlist":
                    await HandleUpdateSetList(args);
                    return false;

                case "delete-setlist":
                    await HandleDeleteSetList(args);
                    return false;

                case "patch-setlist":
                    await HandlePatchSetList(args);
                    return false;

                case "setlist-sets":
                    await HandleSetListSets(args);
                    return false;

                case "add-setlist-set":
                    await HandleAddSetListSet(args);
                    return false;

                case "update-setlist-set":
                    await HandleUpdateSetListSet(args);
                    return false;

                case "delete-setlist-set":
                    await HandleDeleteSetListSet(args);
                    return false;

                case "setlist-set":
                    await HandleGetSetListSet(args);
                    return false;

                case "patch-setlist-set":
                    await HandlePatchSetListSet(args);
                    return false;

                case "my-partlists":
                    await HandleMyPartLists();
                    return false;

                case "partlist":
                    await HandlePartList(args);
                    return false;

                case "create-partlist":
                    await HandleCreatePartList(args);
                    return false;

                case "update-partlist":
                    await HandleUpdatePartList(args);
                    return false;

                case "delete-partlist":
                    await HandleDeletePartList(args);
                    return false;

                case "patch-partlist":
                    await HandlePatchPartList(args);
                    return false;

                case "partlist-parts":
                    await HandlePartListParts(args);
                    return false;

                case "add-partlist-part":
                    await HandleAddPartListPart(args);
                    return false;

                case "update-partlist-part":
                    await HandleUpdatePartListPart(args);
                    return false;

                case "delete-partlist-part":
                    await HandleDeletePartListPart(args);
                    return false;

                case "partlist-part":
                    await HandleGetPartListPart(args);
                    return false;

                case "my-parts":
                    await HandleMyParts(args);
                    return false;

                case "my-allparts":
                    await HandleMyAllParts(args);
                    return false;

                case "my-lost-parts":
                    await HandleMyLostParts(args);
                    return false;

                case "add-lost-part":
                    await HandleAddLostPart(args);
                    return false;

                case "delete-lost-part":
                    await HandleDeleteLostPart(args);
                    return false;

                case "my-minifigs":
                    await HandleMyMinifigs(args);
                    return false;

                case "build":
                    await HandleBuild(args);
                    return false;

                case "badges":
                    await HandleBadges(args);
                    return false;

                case "badge":
                    await HandleBadge(args);
                    return false;

                case "rate-limit":
                    HandleRateLimit();
                    return false;

                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                    Console.ResetColor();
                    return false;
            }
        }


        #region Catalog Command Handlers

        static async Task HandleColors(string[] args)
        {
            int page = GetIntArg(args, 0, 1);
            var result = await _client.GetColorsAsync(page: page, pageSize: 20);
            PrintHeader($"Colors (page {page}, {result.Count} total)");
            foreach (var c in result.Results)
            {
                Console.WriteLine($"  {c.Id,4}  #{c.Rgb}  {c.Name}{(c.IsTrans ? " (transparent)" : "")}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleColor(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: color <id>"); return; }
            int id = int.Parse(args[0]);
            var c = await _client.GetColorAsync(id);
            PrintHeader($"Color {c.Id}");
            Console.WriteLine($"  Name:        {c.Name}");
            Console.WriteLine($"  RGB:         #{c.Rgb}");
            Console.WriteLine($"  Transparent: {c.IsTrans}");
        }


        static async Task HandleThemes(string[] args)
        {
            int page = GetIntArg(args, 0, 1);
            var result = await _client.GetThemesAsync(page: page, pageSize: 50);
            PrintHeader($"Themes (page {page}, {result.Count} total)");
            foreach (var t in result.Results)
            {
                string parent = t.ParentId.HasValue ? $" (parent: {t.ParentId})" : "";
                Console.WriteLine($"  {t.Id,4}  {t.Name}{parent}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleTheme(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: theme <id>"); return; }
            int id = int.Parse(args[0]);
            var t = await _client.GetThemeAsync(id);
            PrintHeader($"Theme {t.Id}");
            Console.WriteLine($"  Name:      {t.Name}");
            Console.WriteLine($"  Parent ID: {(t.ParentId.HasValue ? t.ParentId.Value.ToString() : "(none)")}");
        }


        static async Task HandleCategories(string[] args)
        {
            int page = GetIntArg(args, 0, 1);
            var result = await _client.GetPartCategoriesAsync(page: page, pageSize: 100);
            PrintHeader($"Part Categories (page {page}, {result.Count} total)");
            foreach (var c in result.Results)
            {
                Console.WriteLine($"  {c.Id,4}  {c.Name} ({c.PartCount} parts)");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleCategory(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: category <id>"); return; }
            int id = int.Parse(args[0]);
            var c = await _client.GetPartCategoryAsync(id);
            PrintHeader($"Part Category {c.Id}");
            Console.WriteLine($"  Name:       {c.Name}");
            Console.WriteLine($"  Part Count: {c.PartCount}");
        }


        static async Task HandleParts(string[] args)
        {
            string search = GetNamedArg(args, "--search");
            string catId = GetNamedArg(args, "--cat");
            string ldrawId = GetNamedArg(args, "--ldraw");
            string partNum = GetNamedArg(args, "--part");
            string bricklinkId = GetNamedArg(args, "--bricklink");
            string legoId = GetNamedArg(args, "--lego");
            int page = GetIntArg(args, "--page", 1);

            var result = await _client.GetPartsAsync(
                search: search, partCatId: catId, ldrawId: ldrawId,
                partNum: partNum, bricklinkId: bricklinkId, legoId: legoId,
                page: page, pageSize: 20);

            PrintHeader($"Parts (page {page}, {result.Count} total)");
            foreach (var p in result.Results)
            {
                Console.WriteLine($"  {p.PartNum,-15} Cat:{p.PartCatId,-4} {p.Name}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandlePart(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: part <part_num>"); return; }
            var p = await _client.GetPartAsync(args[0]);
            PrintHeader($"Part {p.PartNum}");
            Console.WriteLine($"  Name:        {p.Name}");
            Console.WriteLine($"  Category:    {p.PartCatId}");
            Console.WriteLine($"  URL:         {p.PartUrl}");
            Console.WriteLine($"  Image:       {p.PartImgUrl}");
            if (p.PrintOf != null) Console.WriteLine($"  Print of:    {p.PrintOf}");
            if (p.ExternalIds != null)
            {
                if (p.ExternalIds.BrickLink != null && p.ExternalIds.BrickLink.Count > 0)
                    Console.WriteLine($"  BrickLink:   {string.Join(", ", p.ExternalIds.BrickLink)}");
                if (p.ExternalIds.LDraw != null && p.ExternalIds.LDraw.Count > 0)
                    Console.WriteLine($"  LDraw:       {string.Join(", ", p.ExternalIds.LDraw)}");
                if (p.ExternalIds.Lego != null && p.ExternalIds.Lego.Count > 0)
                    Console.WriteLine($"  LEGO:        {string.Join(", ", p.ExternalIds.Lego)}");
                if (p.ExternalIds.BrickOwl != null && p.ExternalIds.BrickOwl.Count > 0)
                    Console.WriteLine($"  BrickOwl:    {string.Join(", ", p.ExternalIds.BrickOwl)}");
            }
        }


        static async Task HandlePartColors(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: part-colors <part_num>"); return; }
            var result = await _client.GetPartColorsAsync(args[0]);
            PrintHeader($"Colors for part {args[0]} ({result.Count} total)");
            foreach (var pc in result.Results)
            {
                string elements = pc.Elements != null && pc.Elements.Count > 0
                    ? $" Elements: {string.Join(", ", pc.Elements)}" : "";
                Console.WriteLine($"  Color {pc.ColorId,4} {pc.ColorName,-25} ({pc.NumSets} sets){elements}");
            }
        }


        static async Task HandleSets(string[] args)
        {
            string search = GetNamedArg(args, "--search");
            string themeId = GetNamedArg(args, "--theme");
            int? minYear = GetIntNamedArg(args, "--min-year");
            int? maxYear = GetIntNamedArg(args, "--max-year");
            int? minParts = GetIntNamedArg(args, "--min-parts");
            int? maxParts = GetIntNamedArg(args, "--max-parts");
            int page = GetIntArg(args, "--page", 1);

            var result = await _client.GetSetsAsync(
                search: search, themeId: themeId,
                minYear: minYear, maxYear: maxYear,
                minParts: minParts, maxParts: maxParts,
                page: page, pageSize: 20);

            PrintHeader($"Sets (page {page}, {result.Count} total)");
            foreach (var s in result.Results)
            {
                Console.WriteLine($"  {s.SetNum,-12} {s.Year}  {s.Name} ({s.NumParts} parts)");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleSet(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: set <set_num>"); return; }
            var s = await _client.GetSetAsync(args[0]);
            PrintHeader($"Set {s.SetNum}");
            Console.WriteLine($"  Name:      {s.Name}");
            Console.WriteLine($"  Year:      {s.Year}");
            Console.WriteLine($"  Theme ID:  {s.ThemeId}");
            Console.WriteLine($"  Parts:     {s.NumParts}");
            Console.WriteLine($"  Modified:  {s.LastModifiedDt ?? "(unknown)"}");
            Console.WriteLine($"  Image:     {s.SetImgUrl}");
            Console.WriteLine($"  URL:       {s.SetUrl}");
        }


        static async Task HandleSetParts(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: set-parts <set_num>"); return; }
            int page = GetIntArg(args, "--page", 1);
            var result = await _client.GetSetPartsAsync(args[0], page: page, pageSize: 50);
            PrintHeader($"Parts in {args[0]} (page {page}, {result.Count} total)");
            foreach (var ip in result.Results)
            {
                string spare = ip.IsSpare ? " [spare]" : "";
                string color = ip.Color != null ? $" ({ip.Color.Name})" : "";
                string partName = ip.Part != null ? ip.Part.Name : "(unknown)";
                string partNum = ip.Part != null ? ip.Part.PartNum : "?";
                string elemId = !string.IsNullOrEmpty(ip.ElementId) ? $" Elem:{ip.ElementId}" : "";
                Console.WriteLine($"  {ip.Quantity,3}x {partNum,-15}{color} {partName}{spare}{elemId}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleSetMinifigs(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: set-minifigs <set_num>"); return; }
            var result = await _client.GetSetMinifigsAsync(args[0]);
            PrintHeader($"Minifigs in {args[0]} ({result.Count} total)");
            foreach (var m in result.Results)
            {
                Console.WriteLine($"  {m.Quantity,3}x {m.SetNum,-15} {m.SetName}");
            }
        }


        static async Task HandleSetSets(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: set-sets <set_num>"); return; }
            var result = await _client.GetSetSetsAsync(args[0]);
            PrintHeader($"Sub-sets in {args[0]} ({result.Count} total)");
            foreach (var s in result.Results)
            {
                Console.WriteLine($"  {s.Quantity,3}x {s.SetNum,-15} {s.SetName}");
            }
        }


        static async Task HandleMinifigs(string[] args)
        {
            string search = GetNamedArg(args, "--search");
            int? minParts = GetIntNamedArg(args, "--min-parts");
            int? maxParts = GetIntNamedArg(args, "--max-parts");
            string inSetNum = GetNamedArg(args, "--in-set");
            string inThemeId = GetNamedArg(args, "--in-theme");
            int page = GetIntArg(args, "--page", 1);

            var result = await _client.GetMinifigsAsync(
                search: search, minParts: minParts, maxParts: maxParts,
                inSetNum: inSetNum, inThemeId: inThemeId,
                page: page, pageSize: 20);
            PrintHeader($"Minifigs (page {page}, {result.Count} total)");
            foreach (var m in result.Results)
            {
                Console.WriteLine($"  {m.SetNum,-15} {m.Name} ({m.NumParts} parts)");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleMinifig(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: minifig <fig_num>"); return; }
            var m = await _client.GetMinifigAsync(args[0]);
            PrintHeader($"Minifig {m.SetNum}");
            Console.WriteLine($"  Name:   {m.Name}");
            Console.WriteLine($"  Parts:  {m.NumParts}");
            Console.WriteLine($"  Image:  {m.SetImgUrl}");
            Console.WriteLine($"  URL:    {m.SetUrl}");
        }


        static async Task HandleElement(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: element <element_id>"); return; }
            var e = await _client.GetElementAsync(args[0]);
            PrintHeader($"Element {e.ElementId}");
            Console.WriteLine($"  Part:      {e.Part?.PartNum} — {e.Part?.Name}");
            Console.WriteLine($"  Color:     {e.Color?.Name} (#{e.Color?.Rgb})");
            Console.WriteLine($"  Design ID: {e.DesignId ?? "(none)"}");
            Console.WriteLine($"  Image:     {e.ElementImgUrl}");
        }


        static async Task HandlePartColor(string[] args)
        {
            if (args.Length < 2) { Console.WriteLine("Usage: part-color <part_num> <color_id>"); return; }
            var pc = await _client.GetPartColorAsync(args[0], int.Parse(args[1]));
            PrintHeader($"Part {args[0]} in Color {args[1]}");
            Console.WriteLine($"  Color:    {pc.ColorId} {pc.ColorName}");
            Console.WriteLine($"  In Sets:  {pc.NumSets}");
            Console.WriteLine($"  Image:    {pc.PartImgUrl}");
            if (pc.Elements != null && pc.Elements.Count > 0)
                Console.WriteLine($"  Elements: {string.Join(", ", pc.Elements)}");
        }


        static async Task HandlePartColorSets(string[] args)
        {
            if (args.Length < 2) { Console.WriteLine("Usage: part-color-sets <part_num> <color_id>"); return; }
            int page = GetIntArg(args, "--page", 1);
            var result = await _client.GetPartColorSetsAsync(args[0], int.Parse(args[1]), page: page, pageSize: 20);
            PrintHeader($"Sets with {args[0]} in color {args[1]} (page {page}, {result.Count} total)");
            foreach (var s in result.Results)
            {
                Console.WriteLine($"  {s.SetNum,-12} {s.Year}  {s.Name} ({s.NumParts} parts)");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleSetAlternates(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: set-alternates <set_num>"); return; }
            var result = await _client.GetSetAlternatesAsync(args[0]);
            PrintHeader($"Alternate builds for {args[0]} ({result.Count} total)");
            foreach (var a in result.Results)
            {
                Console.WriteLine($"  {a.SetNum,-12} {a.Name}");
                if (!string.IsNullOrEmpty(a.DesignerName))
                    Console.WriteLine($"               Designer: {a.DesignerName}");
            }
        }


        static async Task HandleMinifigParts(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: minifig-parts <fig_num>"); return; }
            int page = GetIntArg(args, "--page", 1);
            var result = await _client.GetMinifigPartsAsync(args[0], page: page, pageSize: 50);
            PrintHeader($"Parts in {args[0]} (page {page}, {result.Count} total)");
            foreach (var ip in result.Results)
            {
                string color = ip.Color != null ? $" ({ip.Color.Name})" : "";
                string partName = ip.Part != null ? ip.Part.Name : "(unknown)";
                string partNum = ip.Part != null ? ip.Part.PartNum : "?";
                Console.WriteLine($"  {ip.Quantity,3}x {partNum,-15}{color} {partName}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleMinifigSets(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: minifig-sets <fig_num>"); return; }
            int page = GetIntArg(args, "--page", 1);
            var result = await _client.GetMinifigSetsAsync(args[0], page: page, pageSize: 20);
            PrintHeader($"Sets containing {args[0]} (page {page}, {result.Count} total)");
            foreach (var s in result.Results)
            {
                Console.WriteLine($"  {s.SetNum,-12} {s.Year}  {s.Name} ({s.NumParts} parts)");
            }
            PrintPaginationHint(result);
        }

        #endregion


        #region User Command Handlers

        static async Task HandleLogin(string[] args)
        {
            if (args.Length < 2) { Console.WriteLine("Usage: login <username> <password>"); return; }
            var result = await _client.GetUserTokenAsync(args[0], args[1]);
            _userToken = result.UserToken;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Logged in! Token: {_userToken.Substring(0, 8)}...");
            Console.ResetColor();
        }


        static async Task HandleProfile()
        {
            if (!RequireLogin()) return;
            var p = await _client.GetUserProfileAsync(_userToken);
            PrintHeader("User Profile");
            Console.WriteLine($"  Username:      {p.Username}");
            Console.WriteLine($"  User ID:       {p.UserId}");
            Console.WriteLine($"  Location:      {p.Location}");
            Console.WriteLine($"  Last Activity: {p.LastActivity}");
            if (p.Lego != null)
            {
                Console.WriteLine($"  Sets:          {p.Lego.NumSets}");
                Console.WriteLine($"  Set Lists:     {p.Lego.NumSetLists}");
                Console.WriteLine($"  Parts:         {p.Lego.NumParts}");
                Console.WriteLine($"  Part Lists:    {p.Lego.NumPartLists}");
                Console.WriteLine($"  Minifigs:      {p.Lego.NumMinifigs}");
                Console.WriteLine($"  Lost Parts:    {p.Lego.NumLostParts}");
            }
        }


        static async Task HandleMySets(string[] args)
        {
            if (!RequireLogin()) return;
            string search = GetNamedArg(args, "--search");
            string setNum = GetNamedArg(args, "--set");
            string themeId = GetNamedArg(args, "--theme");
            int? minYear = GetIntNamedArg(args, "--min-year");
            int? maxYear = GetIntNamedArg(args, "--max-year");
            int? minParts = GetIntNamedArg(args, "--min-parts");
            int? maxParts = GetIntNamedArg(args, "--max-parts");
            int page = GetIntArg(args, "--page", 1);

            var result = await _client.GetUserSetsAsync(_userToken,
                search: search, setNum: setNum, themeId: themeId,
                minYear: minYear, maxYear: maxYear,
                minParts: minParts, maxParts: maxParts,
                page: page, pageSize: 20);
            PrintHeader($"My Sets (page {page}, {result.Count} total)");
            foreach (var s in result.Results)
            {
                string setInfo = s.Set != null ? $"{s.Set.Year} ({s.Set.NumParts} parts)" : "";
                Console.WriteLine($"  {s.Quantity,2}x {s.SetNum,-12} {s.SetName}  {setInfo}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleMySet(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: my-set <set_num>"); return; }
            var s = await _client.GetUserSetAsync(_userToken, args[0]);
            PrintHeader($"My Set {s.SetNum}");
            Console.WriteLine($"  Name:           {s.SetName}");
            Console.WriteLine($"  Quantity:       {s.Quantity}");
            Console.WriteLine($"  Include Spares: {s.IncludeSpares}");
            if (s.Set != null)
            {
                Console.WriteLine($"  Year:           {s.Set.Year}");
                Console.WriteLine($"  Parts:          {s.Set.NumParts}");
                Console.WriteLine($"  Theme ID:       {s.Set.ThemeId}");
            }
        }


        static async Task HandleAddSet(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: add-set <set_num> [quantity] [--no-spares]"); return; }
            int qty = GetIntArg(args, 1, 1);
            bool includeSpares = !HasFlag(args, "--no-spares");
            var result = await _client.AddUserSetAsync(_userToken, args[0], qty, includeSpares: includeSpares);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Added {result.Quantity}x {result.SetNum}{(includeSpares ? "" : " (no spares)")}");
            Console.ResetColor();
        }


        static async Task HandleUpdateSet(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 2) { Console.WriteLine("Usage: update-set <set_num> <quantity>"); return; }
            var result = await _client.UpdateUserSetAsync(_userToken, args[0], int.Parse(args[1]));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Updated {result.SetNum} → quantity {result.Quantity}");
            Console.ResetColor();
        }


        static async Task HandleRemoveSet(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: remove-set <set_num>"); return; }
            await _client.DeleteUserSetAsync(_userToken, args[0]);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Removed {args[0]}");
            Console.ResetColor();
        }


        static async Task HandleSyncSets(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: sync-sets <set_num:qty> [set_num:qty] ... [--no-spares]"); return; }

            bool includeSpares = !HasFlag(args, "--no-spares");

            // Build the list of set objects expected by the API
            var sets = new List<object>();
            foreach (var arg in args)
            {
                if (arg.StartsWith("--")) continue;
                var parts = arg.Split(':');
                string setNum = parts[0];
                int qty = parts.Length > 1 ? int.Parse(parts[1]) : 1;
                sets.Add(new { set_num = setNum, quantity = qty, include_spares = includeSpares ? "True" : "False" });
            }

            await _client.SyncUserSetsAsync(_userToken, sets);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Synced {sets.Count} sets");
            Console.ResetColor();
        }


        static async Task HandleMySetLists()
        {
            if (!RequireLogin()) return;
            var result = await _client.GetUserSetListsAsync(_userToken);
            PrintHeader($"My Set Lists ({result.Count} total)");
            foreach (var sl in result.Results)
            {
                string buildable = sl.IsBuildable ? " [buildable]" : "";
                Console.WriteLine($"  [{sl.Id}] {sl.Name} ({sl.NumSets} sets){buildable}");
            }
        }


        static async Task HandleSetList(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: setlist <id>"); return; }
            var sl = await _client.GetUserSetListAsync(_userToken, int.Parse(args[0]));
            PrintHeader($"Set List {sl.Id}");
            Console.WriteLine($"  Name:      {sl.Name}");
            Console.WriteLine($"  Sets:      {sl.NumSets}");
            Console.WriteLine($"  Buildable: {sl.IsBuildable}");
        }


        static async Task HandleCreateSetList(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: create-setlist <name> [--buildable]"); return; }
            bool buildable = HasFlag(args, "--buildable");
            var result = await _client.CreateUserSetListAsync(_userToken, args[0], buildable);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Created set list [{result.Id}] {result.Name}");
            Console.ResetColor();
        }


        static async Task HandleUpdateSetList(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 2) { Console.WriteLine("Usage: update-setlist <id> <name> [--buildable]"); return; }
            bool buildable = HasFlag(args, "--buildable");
            var result = await _client.UpdateUserSetListAsync(_userToken, int.Parse(args[0]), args[1], buildable);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Updated set list [{result.Id}] → {result.Name}");
            Console.ResetColor();
        }


        static async Task HandleDeleteSetList(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: delete-setlist <id>"); return; }
            await _client.DeleteUserSetListAsync(_userToken, int.Parse(args[0]));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Deleted set list {args[0]}");
            Console.ResetColor();
        }


        static async Task HandlePatchSetList(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: patch-setlist <id> [--name X] [--buildable]"); return; }
            string name = GetNamedArg(args, "--name");
            bool? buildable = HasFlag(args, "--buildable") ? true : (bool?)null;
            var result = await _client.PatchUserSetListAsync(_userToken, int.Parse(args[0]), name, buildable);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Patched set list [{result.Id}] → {result.Name}");
            Console.ResetColor();
        }


        static async Task HandleSetListSets(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: setlist-sets <list_id> [--page N]"); return; }
            int page = GetIntArg(args, "--page", 1);
            var result = await _client.GetUserSetListSetsAsync(_userToken, int.Parse(args[0]), page: page, pageSize: 20);
            PrintHeader($"Sets in list {args[0]} (page {page}, {result.Count} total)");
            foreach (var s in result.Results)
            {
                string setInfo = s.Set != null ? $"{s.Set.Year} ({s.Set.NumParts} parts)" : "";
                Console.WriteLine($"  {s.Quantity,2}x {s.SetNum,-12} {s.SetName}  {setInfo}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleAddSetListSet(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 2) { Console.WriteLine("Usage: add-setlist-set <list_id> <set_num> [quantity]"); return; }
            int qty = GetIntArg(args, 2, 1);
            var result = await _client.AddUserSetListSetAsync(_userToken, int.Parse(args[0]), args[1], qty);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Added {result.Quantity}x {result.SetNum} to list {args[0]}");
            Console.ResetColor();
        }


        static async Task HandleUpdateSetListSet(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 3) { Console.WriteLine("Usage: update-setlist-set <list_id> <set_num> <quantity>"); return; }
            var result = await _client.UpdateUserSetListSetAsync(_userToken, int.Parse(args[0]), args[1], int.Parse(args[2]));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Updated {result.SetNum} in list {args[0]} → quantity {result.Quantity}");
            Console.ResetColor();
        }


        static async Task HandleDeleteSetListSet(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 2) { Console.WriteLine("Usage: delete-setlist-set <list_id> <set_num>"); return; }
            await _client.DeleteUserSetListSetAsync(_userToken, int.Parse(args[0]), args[1]);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Removed {args[1]} from list {args[0]}");
            Console.ResetColor();
        }


        static async Task HandleGetSetListSet(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 2) { Console.WriteLine("Usage: setlist-set <list_id> <set_num>"); return; }
            var s = await _client.GetUserSetListSetAsync(_userToken, int.Parse(args[0]), args[1]);
            PrintHeader($"Set {s.SetNum} in list {args[0]}");
            Console.WriteLine($"  Name:           {s.SetName}");
            Console.WriteLine($"  Quantity:       {s.Quantity}");
            Console.WriteLine($"  Include Spares: {s.IncludeSpares}");
            if (s.Set != null)
            {
                Console.WriteLine($"  Year:           {s.Set.Year}");
                Console.WriteLine($"  Parts:          {s.Set.NumParts}");
            }
        }


        static async Task HandlePatchSetListSet(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 2) { Console.WriteLine("Usage: patch-setlist-set <list_id> <set_num> [--qty N] [--no-spares]"); return; }
            int? qty = GetIntNamedArg(args, "--qty");
            bool? includeSpares = HasFlag(args, "--no-spares") ? false : (bool?)null;
            var result = await _client.PatchUserSetListSetAsync(_userToken, int.Parse(args[0]), args[1], qty, includeSpares);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Patched {result.SetNum} in list {args[0]} → qty {result.Quantity}");
            Console.ResetColor();
        }


        static async Task HandleMyPartLists()
        {
            if (!RequireLogin()) return;
            var result = await _client.GetUserPartListsAsync(_userToken);
            PrintHeader($"My Part Lists ({result.Count} total)");
            foreach (var pl in result.Results)
            {
                string buildable = pl.IsBuildable ? " [buildable]" : "";
                Console.WriteLine($"  [{pl.Id}] {pl.Name} ({pl.NumParts} parts){buildable}");
            }
        }


        static async Task HandlePartList(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: partlist <id>"); return; }
            var pl = await _client.GetUserPartListAsync(_userToken, int.Parse(args[0]));
            PrintHeader($"Part List {pl.Id}");
            Console.WriteLine($"  Name:      {pl.Name}");
            Console.WriteLine($"  Parts:     {pl.NumParts}");
            Console.WriteLine($"  Buildable: {pl.IsBuildable}");
        }


        static async Task HandleCreatePartList(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: create-partlist <name> [--buildable]"); return; }
            bool buildable = HasFlag(args, "--buildable");
            var result = await _client.CreateUserPartListAsync(_userToken, args[0], buildable);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Created part list [{result.Id}] {result.Name}");
            Console.ResetColor();
        }


        static async Task HandleUpdatePartList(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 2) { Console.WriteLine("Usage: update-partlist <id> <name> [--buildable]"); return; }
            bool buildable = HasFlag(args, "--buildable");
            var result = await _client.UpdateUserPartListAsync(_userToken, int.Parse(args[0]), args[1], buildable);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Updated part list [{result.Id}] → {result.Name}");
            Console.ResetColor();
        }


        static async Task HandleDeletePartList(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: delete-partlist <id>"); return; }
            await _client.DeleteUserPartListAsync(_userToken, int.Parse(args[0]));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Deleted part list {args[0]}");
            Console.ResetColor();
        }


        static async Task HandlePatchPartList(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: patch-partlist <id> [--name X] [--buildable]"); return; }
            string name = GetNamedArg(args, "--name");
            bool? buildable = HasFlag(args, "--buildable") ? true : (bool?)null;
            var result = await _client.PatchUserPartListAsync(_userToken, int.Parse(args[0]), name, buildable);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Patched part list [{result.Id}] → {result.Name}");
            Console.ResetColor();
        }


        static async Task HandlePartListParts(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: partlist-parts <list_id> [--page N]"); return; }
            int page = GetIntArg(args, "--page", 1);
            var result = await _client.GetUserPartListPartsAsync(_userToken, int.Parse(args[0]), page: page, pageSize: 20);
            PrintHeader($"Parts in list {args[0]} (page {page}, {result.Count} total)");
            foreach (var p in result.Results)
            {
                string partName = p.Part != null ? $"{p.Part.PartNum} {p.Part.Name}" : "(unknown)";
                string color = p.Color != null ? $" ({p.Color.Name})" : "";
                Console.WriteLine($"  {p.Quantity,4}x {partName}{color}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleAddPartListPart(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 4) { Console.WriteLine("Usage: add-partlist-part <list_id> <part_num> <color_id> <quantity>"); return; }
            var result = await _client.AddUserPartListPartAsync(
                _userToken, int.Parse(args[0]), args[1], int.Parse(args[2]), int.Parse(args[3]));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Added {result.Quantity}x {result.Part?.PartNum} (color {result.Color?.Name}) to list {args[0]}");
            Console.ResetColor();
        }


        static async Task HandleUpdatePartListPart(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 4) { Console.WriteLine("Usage: update-partlist-part <list_id> <part_num> <color_id> <quantity>"); return; }
            var result = await _client.UpdateUserPartListPartAsync(
                _userToken, int.Parse(args[0]), args[1], int.Parse(args[2]), int.Parse(args[3]));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Updated to {result.Quantity}x {result.Part?.PartNum} (color {result.Color?.Name}) in list {args[0]}");
            Console.ResetColor();
        }


        static async Task HandleDeletePartListPart(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 3) { Console.WriteLine("Usage: delete-partlist-part <list_id> <part_num> <color_id>"); return; }
            await _client.DeleteUserPartListPartAsync(
                _userToken, int.Parse(args[0]), args[1], int.Parse(args[2]));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Removed {args[1]} (color {args[2]}) from list {args[0]}");
            Console.ResetColor();
        }


        static async Task HandleGetPartListPart(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 3) { Console.WriteLine("Usage: partlist-part <list_id> <part_num> <color_id>"); return; }
            var p = await _client.GetUserPartListPartAsync(_userToken, int.Parse(args[0]), args[1], int.Parse(args[2]));
            PrintHeader($"Part {args[1]} (color {args[2]}) in list {args[0]}");
            if (p.Part != null)
            {
                Console.WriteLine($"  Part:     {p.Part.PartNum} — {p.Part.Name}");
            }
            if (p.Color != null)
            {
                Console.WriteLine($"  Color:    {p.Color.Name} (#{p.Color.Rgb})");
            }
            Console.WriteLine($"  Quantity: {p.Quantity}");
        }


        static async Task HandleMyParts(string[] args)
        {
            if (!RequireLogin()) return;
            string search = GetNamedArg(args, "--search");
            string partNum = GetNamedArg(args, "--part");
            int? catId = GetIntNamedArg(args, "--cat");
            int? colorId = GetIntNamedArg(args, "--color");
            int page = GetIntArg(args, "--page", 1);

            var result = await _client.GetUserPartsAsync(_userToken,
                search: search, partNum: partNum,
                partCatId: catId, colorId: colorId,
                page: page, pageSize: 20);

            PrintHeader($"My Parts (page {page}, {result.Count} total)");
            foreach (var p in result.Results)
            {
                string partName = p.Part != null ? $"{p.Part.PartNum} {p.Part.Name}" : "(unknown)";
                string color = p.Color != null ? $" ({p.Color.Name})" : "";
                Console.WriteLine($"  {p.Quantity,4}x {partName}{color}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleMyAllParts(string[] args)
        {
            if (!RequireLogin()) return;
            string partNum = GetNamedArg(args, "--part");
            int? catId = GetIntNamedArg(args, "--cat");
            int? colorId = GetIntNamedArg(args, "--color");
            int page = GetIntArg(args, "--page", 1);

            var result = await _client.GetUserAllPartsAsync(_userToken,
                partNum: partNum, partCatId: catId, colorId: colorId,
                page: page, pageSize: 20);

            PrintHeader($"All My Parts — sets + lists (page {page}, {result.Count} total)");
            foreach (var p in result.Results)
            {
                string partName = p.Part != null ? $"{p.Part.PartNum} {p.Part.Name}" : "(unknown)";
                string color = p.Color != null ? $" ({p.Color.Name})" : "";
                Console.WriteLine($"  {p.Quantity,4}x {partName}{color}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleMyLostParts(string[] args)
        {
            if (!RequireLogin()) return;
            int page = GetIntArg(args, "--page", 1);
            var result = await _client.GetUserLostPartsAsync(_userToken, page: page, pageSize: 20);
            PrintHeader($"My Lost Parts (page {page}, {result.Count} total)");
            foreach (var lp in result.Results)
            {
                string partName = lp.Part != null ? $"{lp.Part.PartNum} {lp.Part.Name}" : $"inv_part_id={lp.InvPartId}";
                string color = lp.Color != null ? $" ({lp.Color.Name})" : "";
                Console.WriteLine($"  [{lp.Id}] {lp.LostQuantity,3}x {partName}{color}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleAddLostPart(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: add-lost-part <inv_part_id> [quantity]"); return; }
            int qty = GetIntArg(args, 1, 1);
            var result = await _client.AddUserLostPartAsync(_userToken, int.Parse(args[0]), qty);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Added lost part (ID: {result.Id}, quantity: {result.LostQuantity})");
            Console.ResetColor();
        }


        static async Task HandleDeleteLostPart(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: delete-lost-part <id>"); return; }
            await _client.DeleteUserLostPartAsync(_userToken, int.Parse(args[0]));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Deleted lost part {args[0]}");
            Console.ResetColor();
        }


        static async Task HandleMyMinifigs(string[] args)
        {
            if (!RequireLogin()) return;
            string search = GetNamedArg(args, "--search");
            int page = GetIntArg(args, "--page", 1);

            var result = await _client.GetUserMinifigsAsync(_userToken, search: search, page: page, pageSize: 20);
            PrintHeader($"My Minifigs (page {page}, {result.Count} total)");
            foreach (var m in result.Results)
            {
                Console.WriteLine($"  {m.Quantity,2}x {m.SetNum,-15} {m.SetName}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleBuild(string[] args)
        {
            if (!RequireLogin()) return;
            if (args.Length < 1) { Console.WriteLine("Usage: build <set_num>"); return; }
            var b = await _client.GetUserBuildAsync(_userToken, args[0]);
            PrintHeader($"Build Match for {args[0]}");
            Console.WriteLine($"  Total Parts: {b.TotalParts}");
            Console.WriteLine($"  You Have:    {b.NumHave}");
            Console.WriteLine($"  You Need:    {b.NumNeed}");

            // Colour the percentage based on match quality
            Console.Write("  Match:       ");
            if (b.PctHave >= 90) Console.ForegroundColor = ConsoleColor.Green;
            else if (b.PctHave >= 50) Console.ForegroundColor = ConsoleColor.Yellow;
            else Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{b.PctHave:F1}%");
            Console.ResetColor();
        }


        static async Task HandleBadges(string[] args)
        {
            int page = GetIntArg(args, 0, 1);
            var result = await _client.GetBadgesAsync(page: page, pageSize: 20);
            PrintHeader($"Badges (page {page}, {result.Count} total)");
            foreach (var b in result.Results)
            {
                Console.WriteLine($"  [{b.Id}] {b.Name} (Level {b.Level}) — {b.Description}");
            }
            PrintPaginationHint(result);
        }


        static async Task HandleBadge(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: badge <id>"); return; }
            var b = await _client.GetBadgeAsync(int.Parse(args[0]));
            PrintHeader($"Badge {b.Id}");
            Console.WriteLine($"  Name:        {b.Name}");
            Console.WriteLine($"  Code:        {b.Code}");
            Console.WriteLine($"  Level:       {b.Level}");
            Console.WriteLine($"  Description: {b.Description}");
        }

        #endregion


        #region General Commands

        static void HandleRateLimit()
        {
            var rl = _client.RateLimit;
            PrintHeader("API Rate Limit");
            if (!rl.HasData)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  No rate limit data yet — make an API call first.");
                Console.ResetColor();
                return;
            }
            Console.WriteLine($"  Remaining: {rl.Remaining} / {rl.Limit}");
            Console.WriteLine($"  Used:      {rl.PercentRemaining:F0}% remaining");
            Console.WriteLine($"  Resets in: {rl.ResetSeconds}s");
            Console.WriteLine($"  Last seen: {rl.LastUpdated:HH:mm:ss}");
        }

        #endregion


        #region Helpers

        static bool RequireLogin()
        {
            if (_userToken != null) return true;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Not logged in. Use 'login <username> <password>' first.");
            Console.ResetColor();
            return false;
        }


        static void PrintHeader(string text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"── {text} ──");
            Console.ResetColor();
        }


        static void PrintPaginationHint<T>(PagedResponse<T> response)
        {
            if (!string.IsNullOrEmpty(response.Next))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  ... more results available (use --page N)");
                Console.ResetColor();
            }
        }


        static void PrintHelp()
        {
            PrintHeader("LEGO Catalog Commands");
            Console.WriteLine("  colors [page]                          List colours");
            Console.WriteLine("  color <id>                             Get colour details");
            Console.WriteLine("  themes [page]                          List themes");
            Console.WriteLine("  theme <id>                             Get theme details");
            Console.WriteLine("  categories [page]                      List part categories");
            Console.WriteLine("  category <id>                          Get category details");
            Console.WriteLine("  parts [--search X] [--cat N] [--part X] [--ldraw X] [--bricklink X] [--lego X] [--page N]");
            Console.WriteLine("                                         Search parts");
            Console.WriteLine("  part <part_num>                        Get part details + external IDs");
            Console.WriteLine("  part-colors <part_num>                 Colours a part appears in");
            Console.WriteLine("  part-color <part_num> <color_id>       Part/colour combo details");
            Console.WriteLine("  part-color-sets <part_num> <color_id>  Sets with this part/colour");
            Console.WriteLine("  sets [--search X] [--theme N] [--min-year N] [--max-year N] [--min-parts N]");
            Console.WriteLine("       [--max-parts N] [--page N]        Search sets");
            Console.WriteLine("  set <set_num>                          Get set details");
            Console.WriteLine("  set-parts <set_num> [--page N]         List parts in a set");
            Console.WriteLine("  set-minifigs <set_num>                 List minifigs in a set");
            Console.WriteLine("  set-sets <set_num>                     List sub-sets in a set");
            Console.WriteLine("  set-alternates <set_num>               List alternate builds");
            Console.WriteLine("  minifigs [--search X] [--min-parts N] [--max-parts N] [--in-set X]");
            Console.WriteLine("           [--in-theme N] [--page N]     Search minifigs");
            Console.WriteLine("  minifig <fig_num>                      Get minifig details");
            Console.WriteLine("  minifig-parts <fig_num> [--page N]     List parts in a minifig");
            Console.WriteLine("  minifig-sets <fig_num> [--page N]      Sets containing a minifig");
            Console.WriteLine("  element <element_id>                   Get element details");
            Console.WriteLine();

            PrintHeader("User Collection (requires login)");
            Console.WriteLine("  login <username> <password>            Get user token");
            Console.WriteLine("  profile                                Show user profile");
            Console.WriteLine();
            Console.WriteLine("  my-sets [--search X] [--set X] [--theme N] [--min-year N] [--max-year N]");
            Console.WriteLine("          [--min-parts N] [--max-parts N] [--page N]");
            Console.WriteLine("                                         List owned sets");
            Console.WriteLine("  my-set <set_num>                       Get details for an owned set");
            Console.WriteLine("  add-set <set_num> [qty] [--no-spares]  Add set to collection");
            Console.WriteLine("  update-set <set_num> <quantity>         Update set quantity");
            Console.WriteLine("  remove-set <set_num>                   Remove set from collection");
            Console.WriteLine("  sync-sets <set:qty> [set:qty] ...       Sync sets to exact list");
            Console.WriteLine();
            Console.WriteLine("  my-setlists                            List set lists");
            Console.WriteLine("  setlist <id>                           Get set list details");
            Console.WriteLine("  create-setlist <name> [--buildable]     Create set list");
            Console.WriteLine("  update-setlist <id> <name> [--buildable]  Update set list");
            Console.WriteLine("  delete-setlist <id>                    Delete set list");
            Console.WriteLine("  patch-setlist <id> [--name X] [--buildable]  Patch set list");
            Console.WriteLine("  setlist-sets <id> [--page N]           List sets in a set list");
            Console.WriteLine("  setlist-set <id> <set_num>             Get set details in list");
            Console.WriteLine("  add-setlist-set <id> <set_num> [qty]   Add set to a list");
            Console.WriteLine("  update-setlist-set <id> <set> <qty>    Update set in a list");
            Console.WriteLine("  patch-setlist-set <id> <set> [--qty N] [--no-spares]  Patch set");
            Console.WriteLine("  delete-setlist-set <id> <set_num>      Remove set from a list");
            Console.WriteLine();
            Console.WriteLine("  my-partlists                           List part lists");
            Console.WriteLine("  partlist <id>                          Get part list details");
            Console.WriteLine("  create-partlist <name> [--buildable]    Create part list");
            Console.WriteLine("  update-partlist <id> <name> [--buildable]  Update part list");
            Console.WriteLine("  delete-partlist <id>                   Delete part list");
            Console.WriteLine("  patch-partlist <id> [--name X] [--buildable]  Patch part list");
            Console.WriteLine("  partlist-parts <id> [--page N]         List parts in a part list");
            Console.WriteLine("  partlist-part <id> <part> <color>       Get part details in list");
            Console.WriteLine("  add-partlist-part <id> <part> <color> <qty>  Add part to list");
            Console.WriteLine("  update-partlist-part <id> <part> <color> <qty>  Update part qty");
            Console.WriteLine("  delete-partlist-part <id> <part> <color>  Remove part from list");
            Console.WriteLine();
            Console.WriteLine("  my-parts [--search X] [--part X] [--cat N] [--color N] [--page N]");
            Console.WriteLine("                                         Parts from part lists");
            Console.WriteLine("  my-allparts [--part X] [--cat N] [--color N] [--page N]");
            Console.WriteLine("                                         ALL parts (sets + lists)");
            Console.WriteLine("  my-lost-parts [--page N]               List lost parts");
            Console.WriteLine("  add-lost-part <inv_part_id> [qty]      Add a lost part");
            Console.WriteLine("  delete-lost-part <id>                  Delete a lost part");
            Console.WriteLine("  my-minifigs [--search X] [--page N]    Minifigs from owned sets");
            Console.WriteLine("  build <set_num>                        Check build match %");
            Console.WriteLine("  badges [page]                          List badges");
            Console.WriteLine("  badge <id>                             Get badge details");
            Console.WriteLine();

            PrintHeader("General");
            Console.WriteLine("  rate-limit                             Show API rate limit status");
            Console.WriteLine("  help                                   Show this help");
            Console.WriteLine("  exit / quit / q                        Exit the harness");
        }


        static string GetApiKey(string[] args)
        {
            // Check --key argument
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--key")
                    return args[i + 1];
            }

            // Check environment variable
            return Environment.GetEnvironmentVariable("REBRICKABLE_API_KEY");
        }


        /// <summary>
        /// Get a positional int argument, or default value.
        /// </summary>
        static int GetIntArg(string[] args, int position, int defaultValue)
        {
            if (args.Length > position && int.TryParse(args[position], out int val))
                return val;
            return defaultValue;
        }


        /// <summary>
        /// Get a named int argument (e.g. --page 2), or default value.
        /// </summary>
        static int GetIntArg(string[] args, string name, int defaultValue)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase) && int.TryParse(args[i + 1], out int val))
                    return val;
            }
            return defaultValue;
        }


        /// <summary>
        /// Get a named string argument (e.g. --search "brick").
        /// </summary>
        static string GetNamedArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }


        /// <summary>
        /// Get a named nullable int argument.
        /// </summary>
        static int? GetIntNamedArg(string[] args, string name)
        {
            string val = GetNamedArg(args, name);
            if (val != null && int.TryParse(val, out int result))
                return result;
            return null;
        }


        /// <summary>
        /// Check whether a boolean flag is present (e.g. --no-spares, --buildable).
        /// </summary>
        static bool HasFlag(string[] args, string flag)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(flag, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Parses a command line respecting quoted strings.
        /// </summary>
        static string[] ParseCommandLine(string input)
        {
            var result = new List<string>();
            bool inQuote = false;
            var current = new System.Text.StringBuilder();

            foreach (char c in input)
            {
                if (c == '"')
                {
                    inQuote = !inQuote;
                }
                else if (c == ' ' && !inQuote)
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
                result.Add(current.ToString());

            return result.ToArray();
        }

        #endregion
    }
}
