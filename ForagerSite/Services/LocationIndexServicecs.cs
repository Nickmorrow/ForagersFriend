using ForagerSite.DataContainer;
using System.Text.Json;

namespace ForagerSite.Services;

public class LocationIndexService
{
    private readonly IWebHostEnvironment _env;

    private readonly List<GeoBboxRecord> _all = new();

    // simple prefix index: key = first 3 chars of normalized name
    private readonly Dictionary<string, List<GeoBboxRecord>> _prefix3 = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _stateFpToStateName = new();
    private Dictionary<string, string> _countyKeyToCountyName = new(); // key: STATEFP|COUNTYFP

    public LocationIndexService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task InitializeAsync()
    {
        if (_all.Count > 0) return;

        await LoadFileAsync("geo/states_bbox.json", "State");
        await LoadFileAsync("geo/counties_bbox.json", "County");
        await LoadFileAsync("geo/places_bbox.json", "Place");

        BuildLookups();
        HydrateAdminNames();
        BuildDisplayStrings();
        BuildPrefixIndex();
    }
    private void BuildLookups()
    {
        _stateFpToStateName = _all
            .Where(r => r.Kind == "State" && !string.IsNullOrWhiteSpace(r.STATEFP))
            .GroupBy(r => r.STATEFP!)
            .ToDictionary(g => g.Key, g => g.First().NAME, StringComparer.OrdinalIgnoreCase);

        _countyKeyToCountyName = _all
            .Where(r => r.Kind == "County" &&
                        !string.IsNullOrWhiteSpace(r.STATEFP) &&
                        !string.IsNullOrWhiteSpace(r.COUNTYFP))
            .GroupBy(r => $"{r.STATEFP}|{r.COUNTYFP}")
            .ToDictionary(g => g.Key, g => g.First().NAME, StringComparer.OrdinalIgnoreCase);
    }

    private void HydrateAdminNames()
    {
        foreach (var r in _all)
        {
            if (!string.IsNullOrWhiteSpace(r.STATEFP) &&
                _stateFpToStateName.TryGetValue(r.STATEFP, out var stName))
            {
                r.STATENAME = stName;
            }

            // Places: fill county name if we have COUNTYFP
            if (r.Kind == "Place" &&
                !string.IsNullOrWhiteSpace(r.STATEFP) &&
                !string.IsNullOrWhiteSpace(r.COUNTYFP))
            {
                var key = $"{r.STATEFP}|{r.COUNTYFP}";
                if (_countyKeyToCountyName.TryGetValue(key, out var countyName))
                    r.COUNTYNAME = countyName;
            }
        }
    }

    private async Task LoadFileAsync(string relativeWwwrootPath, string kind)
    {
        var path = Path.Combine(_env.WebRootPath, relativeWwwrootPath.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(path))
            throw new FileNotFoundException($"Missing geo json file: {path}");

        var raw = await File.ReadAllTextAsync(path);

        // strip BOM + whitespace
        var json = raw.TrimStart('\uFEFF', '\u200B', ' ', '\t', '\r', '\n');

        if (json.Length == 0)
            throw new Exception($"Geo file is empty: {path}");

        // ✅ add this temporarily
        var head = json.Substring(0, Math.Min(50, json.Length));
        Console.WriteLine($"[Geo] {relativeWwwrootPath} starts with: {head}");

        var list = JsonSerializer.Deserialize<List<GeoBboxRecord>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? new();

        foreach (var r in list)
        {
            r.Kind = kind;
            _all.Add(r);
        }
    }
    private void BuildDisplayStrings()
    {
        foreach (var r in _all)
        {
            r.Display = r.Kind switch
            {
                "State" => r.NAME,

                "County" => !string.IsNullOrWhiteSpace(r.STATENAME)
                    ? $"{r.NAME} County, {r.STATENAME}"
                    : $"{r.NAME} County",

                "Place" => FormatPlace(r),

                _ => r.NAME
            };
        }
    }

    private static string FormatPlace(GeoBboxRecord r)
    {
        // Detroit, Wayne County, Michigan
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(r.NAME))
            parts.Add(r.NAME);

        if (!string.IsNullOrWhiteSpace(r.COUNTYNAME))
            parts.Add($"{r.COUNTYNAME} County");

        if (!string.IsNullOrWhiteSpace(r.STATENAME))
            parts.Add(r.STATENAME);

        return string.Join(", ", parts);
    }


    private static string Normalize(string s)
        => string.Join(' ', (s ?? "")
            .Trim()
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private void BuildPrefixIndex()
    {
        _prefix3.Clear();

        foreach (var r in _all)
        {
            var n = Normalize(r.NAME);
            var key = n.Length >= 3 ? n[..3] : n;

            if (!_prefix3.TryGetValue(key, out var bucket))
            {
                bucket = new List<GeoBboxRecord>();
                _prefix3[key] = bucket;
            }

            bucket.Add(r);
        }
    }

    public List<GeoBboxRecord> Search(string query, int limit = 8)
    {
        query = Normalize(query);
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new();

        var key = query.Length >= 3 ? query[..3] : query;
        var candidates = _prefix3.TryGetValue(key, out var bucket) ? bucket : _all;

        // ranking: starts-with NAME, then contains NAME, prefer higher-level areas first
        int KindRank(string kind) => kind switch
        {
            "State" => 0,
            "County" => 1,
            "Place" => 2,
            _ => 99
        };

        return candidates
            .Select(r => new
            {
                r,
                n = Normalize(r.NAME)
            })
            .Where(x => x.n.Contains(query))
            .OrderBy(x => x.n.StartsWith(query) ? 0 : 1)
            .ThenBy(x => KindRank(x.r.Kind))
            .ThenBy(x => x.r.NAME)
            .Take(limit)
            .Select(x => x.r)
            .ToList();
    }
}
