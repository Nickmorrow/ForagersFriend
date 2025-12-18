using ForagerSite.DataContainer;
using System.Text.Json;

namespace ForagerSite.Services;

public class LocationIndexService
{
    private readonly IWebHostEnvironment _env;

    private readonly List<GeoPolyRecord> _all = new();

    private readonly Dictionary<string, List<GeoPolyRecord>> _prefix3 =
        new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string, string> _stateFpToStateName = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _countyKeyToCountyName = new(StringComparer.OrdinalIgnoreCase);

    public LocationIndexService(IWebHostEnvironment env) => _env = env;

    public async Task InitializeAsync()
    {
        if (_all.Count > 0) return;

        await LoadGeoFileAsync("geo/states_poly.json", "State");
        await LoadGeoFileAsync("geo/counties_poly.json", "County");
        await LoadGeoFileAsync("geo/places_poly.json", "Place");

        BuildLookups();
        HydrateAdminNames();
        BuildDisplayStrings();
        AssignIds();
        BuildPrefixIndex();
    }

    // ----------------------------
    // Search API used by MapSearch
    // ----------------------------
    public List<GeoPolyRecord> Search(string query, int limit = 8)
    {
        query = Normalize(query);
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new();

        var key = query.Length >= 3 ? query[..3] : query;
        var candidates = _prefix3.TryGetValue(key, out var bucket) ? bucket : _all;

        int KindRank(string kind) => kind switch
        {
            "State" => 0,
            "County" => 1,
            "Place" => 2,
            _ => 99
        };

        return candidates
            .Select(r => new { r, d = Normalize(r.Display) })
            .Where(x => x.d.Contains(query))
            .OrderBy(x => x.d.StartsWith(query) ? 0 : 1)
            .ThenBy(x => KindRank(x.r.Kind))
            .ThenBy(x => x.r.Display)
            .Take(limit)
            .Select(x => x.r)
            .ToList();
    }

    // ----------------------------
    // Boundary lookup used by API
    // ----------------------------
    public GeoPolyRecord? GetByKindAndId(string kind, string id)
        => _all.FirstOrDefault(r =>
            r.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase) &&
            r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    // ----------------------------
    // Loading + parsing
    // ----------------------------
    private async Task LoadGeoFileAsync(string rel, string kind)
    {
        var path = Path.Combine(_env.WebRootPath, rel.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
            throw new FileNotFoundException($"Missing geo json file: {path}");

        var raw = await File.ReadAllTextAsync(path);
        var json = raw.TrimStart('\uFEFF', '\u200B', ' ', '\t', '\r', '\n');

        foreach (var feature in EnumerateFeatures(json))
        {
            var props = feature.GetProperty("properties");

            var rec = new GeoPolyRecord
            {
                Kind = kind,
                Feature = feature.Clone(), // important: store a clone (doc will be disposed)
                NAME = props.TryGetProperty("NAME", out var n) ? (n.GetString() ?? "") : "",

                STATEFP = props.TryGetProperty("STATEFP", out var st) ? st.GetString() : null,
                COUNTYFP = props.TryGetProperty("COUNTYFP", out var co) ? co.GetString() : null,
                PLACEFP = props.TryGetProperty("PLACEFP", out var pl) ? pl.GetString() : null,
            };

            // derive bounds from geometry (fast + works for Polygon/MultiPolygon)
            (rec.West, rec.South, rec.East, rec.North) = ComputeBounds(feature.GetProperty("geometry"));

            _all.Add(rec);
        }
    }

    private static IEnumerable<JsonElement> EnumerateFeatures(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // FeatureCollection
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("type", out var t) &&
            t.GetString() == "FeatureCollection" &&
            root.TryGetProperty("features", out var features) &&
            features.ValueKind == JsonValueKind.Array)
        {
            foreach (var f in features.EnumerateArray())
                yield return f;
            yield break;
        }

        // Single Feature
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("type", out var t2) &&
            t2.GetString() == "Feature")
        {
            yield return root;
            yield break;
        }

        // Array of Features
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var f in root.EnumerateArray())
                yield return f;
        }
    }

    private static (double west, double south, double east, double north) ComputeBounds(JsonElement geometry)
    {
        // GeoJSON coords are [lng, lat]
        double west = double.PositiveInfinity;
        double south = double.PositiveInfinity;
        double east = double.NegativeInfinity;
        double north = double.NegativeInfinity;

        if (!geometry.TryGetProperty("coordinates", out var coords))
            return (0, 0, 0, 0);

        void VisitCoordArray(JsonElement el)
        {
            // A position is [lng,lat]
            if (el.ValueKind == JsonValueKind.Array && el.GetArrayLength() == 2 &&
                el[0].ValueKind == JsonValueKind.Number && el[1].ValueKind == JsonValueKind.Number)
            {
                var lng = el[0].GetDouble();
                var lat = el[1].GetDouble();

                if (lng < west) west = lng;
                if (lng > east) east = lng;
                if (lat < south) south = lat;
                if (lat > north) north = lat;
                return;
            }

            if (el.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in el.EnumerateArray())
                    VisitCoordArray(child);
            }
        }

        VisitCoordArray(coords);

        if (double.IsInfinity(west)) return (0, 0, 0, 0);
        return (west, south, east, north);
    }

    // ----------------------------
    // Admin name hydration + display formatting
    // ----------------------------
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
                _stateFpToStateName.TryGetValue(r.STATEFP, out var st))
                r.STATENAME = st;

            // Places sometimes do NOT have COUNTYFP; if yours does, we’ll fill COUNTYNAME.
            if (r.Kind == "Place" &&
                !string.IsNullOrWhiteSpace(r.STATEFP) &&
                !string.IsNullOrWhiteSpace(r.COUNTYFP))
            {
                var key = $"{r.STATEFP}|{r.COUNTYFP}";
                if (_countyKeyToCountyName.TryGetValue(key, out var cty))
                    r.COUNTYNAME = cty;
            }
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

    private static string FormatPlace(GeoPolyRecord r)
    {
        // Detroit, Wayne County, Michigan
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(r.NAME)) parts.Add(r.NAME);
        if (!string.IsNullOrWhiteSpace(r.COUNTYNAME)) parts.Add($"{r.COUNTYNAME} County");
        if (!string.IsNullOrWhiteSpace(r.STATENAME)) parts.Add(r.STATENAME);
        return string.Join(", ", parts);
    }

    private void AssignIds()
    {
        foreach (var r in _all)
        {
            r.Id = r.Kind switch
            {
                "State" => r.STATEFP ?? r.NAME,
                "County" => $"{r.STATEFP}|{r.COUNTYFP}",
                "Place" => $"{r.STATEFP}|{r.PLACEFP}",
                _ => r.NAME
            };
        }
    }

    private static string Normalize(string s) =>
        string.Join(' ', (s ?? "")
            .Trim()
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private void BuildPrefixIndex()
    {
        _prefix3.Clear();

        foreach (var r in _all)
        {
            var n = Normalize(r.Display);
            var key = n.Length >= 3 ? n[..3] : n;

            if (!_prefix3.TryGetValue(key, out var bucket))
                _prefix3[key] = bucket = new();

            bucket.Add(r);
        }
    }
}
