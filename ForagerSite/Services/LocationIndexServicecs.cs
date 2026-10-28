using ForagerSite.DataContainer;
using System.Text.Json;

namespace ForagerSite.Services;

public class LocationIndexService
{
    private readonly IWebHostEnvironment _env;

    private readonly List<GeoPolyRecord> _all = new();
    private readonly Dictionary<string, List<GeoPolyRecord>> _prefix3 = new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string, string> _stateFpToStateName = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _countyKeyToCountyName = new(StringComparer.OrdinalIgnoreCase);

    public LocationIndexService(IWebHostEnvironment env) => _env = env;

    public async Task InitializeAsync()
    {
        if (_all.Count > 0) return;

        await LoadGeoFileAsync("geo/geoBoundaries-USA-ADM0.geojson");
        await LoadGeoFileAsync("geo/geoBoundaries-USA-ADM1.geojson");
        await LoadGeoFileAsync("geo/geoBoundaries-USA-ADM2.geojson");

        BuildStateLookup();
        AssignStateForCounties();
        BuildLookups();
        HydrateAdminNames();
        BuildDisplayStrings();
        AssignIds();
        BuildPrefixIndex();
    }

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
            "Country" => 3,
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

    public GeoPolyRecord? GetByKindAndId(string kind, string id)
        => _all.FirstOrDefault(r =>
            r.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase) &&
            r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    private async Task LoadGeoFileAsync(string rel)
    {
        var path = Path.Combine(_env.WebRootPath, rel.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
            throw new FileNotFoundException($"Missing geo json file: {path}");

        var raw = await File.ReadAllTextAsync(path);
        var json = raw.TrimStart('\uFEFF', '\u200B', ' ', '\t', '\r', '\n');

        foreach (var feature in EnumerateFeatures(json))
        {
            var props = feature.GetProperty("properties");
            var kind = GetKind(props);
            if (kind is null)
                continue;

            var name = GetPropertyString(props, "shapeName");
            var stateCode = NormalizeStateCode(GetPropertyString(props, "shapeISO"));

            var rec = new GeoPolyRecord
            {
                Kind = kind,
                Feature = feature.Clone(),
                NAME = name,
                STATEFP = kind switch
                {
                    "Country" => "US",
                    "State" => stateCode,
                    _ => null
                },
                COUNTYFP = kind == "County" ? GetCountyKey(props, name, stateCode) : null,
                PLACEFP = null,
            };

            (rec.West, rec.South, rec.East, rec.North) = ComputeBounds(feature.GetProperty("geometry"));
            _all.Add(rec);
        }
    }

    private static string? GetKind(JsonElement props)
    {
        var shapeType = GetPropertyString(props, "shapeType");
        return shapeType switch
        {
            "ADM0" => "Country",
            "ADM1" => "State",
            "ADM2" => "County",
            _ => null
        };
    }

    private static string GetCountyKey(JsonElement props, string fallbackName, string stateCode)
    {
        var id = GetPropertyString(props, "shapeID");
        if (!string.IsNullOrWhiteSpace(id))
            return id;

        return !string.IsNullOrWhiteSpace(stateCode)
            ? $"{stateCode}|{Normalize(fallbackName)}"
            : Normalize(fallbackName);
    }

    private static string GetPropertyString(JsonElement properties, string name)
        => properties.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static string NormalizeStateCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Trim();
        var hyphenIndex = normalized.LastIndexOf('-');
        if (hyphenIndex >= 0 && hyphenIndex < normalized.Length - 1)
            return normalized[(hyphenIndex + 1)..].Trim();

        return normalized.Replace(" ", string.Empty).Replace("_", string.Empty);
    }

    private static IEnumerable<JsonElement> EnumerateFeatures(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

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

        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("type", out var t2) &&
            t2.GetString() == "Feature")
        {
            yield return root;
            yield break;
        }

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var f in root.EnumerateArray())
                yield return f;
        }
    }

    private static (double west, double south, double east, double north) ComputeBounds(JsonElement geometry)
    {
        double west = double.PositiveInfinity;
        double south = double.PositiveInfinity;
        double east = double.NegativeInfinity;
        double north = double.NegativeInfinity;

        if (!geometry.TryGetProperty("coordinates", out var coords))
            return (0, 0, 0, 0);

        void VisitCoordArray(JsonElement el)
        {
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

        if (double.IsInfinity(west))
            return (0, 0, 0, 0);

        return (west, south, east, north);
    }

    private void BuildStateLookup()
    {
        _stateFpToStateName = _all
            .Where(r => r.Kind == "State" && !string.IsNullOrWhiteSpace(r.STATEFP))
            .GroupBy(r => r.STATEFP!)
            .ToDictionary(g => g.Key, g => g.First().NAME, StringComparer.OrdinalIgnoreCase);
    }

    //private void AssignStateForCounties()
    //{
    //    var states = _all.Where(r => r.Kind == "State").ToList();

    //    foreach (var county in _all.Where(r => r.Kind == "County"))
    //    {
    //        var countyVertices = EnumerateGeometryPoints(county.Feature.GetProperty("geometry"));
    //        if (countyVertices.Count == 0)
    //        {
    //            var centerLng = (county.West + county.East) / 2d;
    //            var centerLat = (county.South + county.North) / 2d;
    //            var fallback = states
    //                .FirstOrDefault(s => GeometryContainsPoint(s.Feature.GetProperty("geometry"), centerLng, centerLat));

    //            if (fallback is null)
    //                continue;

    //            county.STATEFP = fallback.STATEFP;
    //            county.STATENAME = fallback.NAME;
    //            continue;
    //        }

    //        var bestState = states
    //            .Select(state => new
    //            {
    //                State = state,
    //                MatchCount = countyVertices.Count(p => GeometryContainsPoint(state.Feature.GetProperty("geometry"), p.lng, p.lat))
    //            })
    //            .OrderByDescending(x => x.MatchCount)
    //            .ThenByDescending(x => x.State.NAME.Length)
    //            .FirstOrDefault();

    //        if (bestState is null || bestState.MatchCount == 0)
    //            continue;

    //        county.STATEFP = bestState.State.STATEFP;
    //        county.STATENAME = bestState.State.NAME;
    //    }
    //}


    private void AssignStateForCounties()
    {
        var states = _all.Where(r => r.Kind == "State").ToList();
        var counties = _all.Where(r => r.Kind == "County").ToList();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        int processed = 0;
        int unmatched = 0;

        Console.WriteLine($"Assigning {counties.Count} counties to states...");

        foreach (var county in counties)
        {
            var countyVertices = EnumerateGeometryPoints(
                county.Feature.GetProperty("geometry"));

            // Only consider states whose bounds overlap the county.
            var candidateStates = states.Where(s =>
                s.West <= county.East &&
                s.East >= county.West &&
                s.South <= county.North &&
                s.North >= county.South
            ).ToList();

            if (candidateStates.Count == 0)
            {
                unmatched++;
                processed++;
                continue;
            }

            // Sample a maximum of approximately 32 vertices.
            // Avoid performing calculations on every polygon vertex.
            const int maxSamples = 32;

            var step = Math.Max(1,
                (int)Math.Ceiling(countyVertices.Count / (double)maxSamples));

            var sampledPoints = countyVertices
                .Where((p, i) => i % step == 0)
                .Take(maxSamples)
                .ToList();

            // Also test the center of the county's bounding box.
            sampledPoints.Add((
                (county.West + county.East) / 2d,
                (county.South + county.North) / 2d
            ));

            var bestState = candidateStates
                .Select(state => new
                {
                    State = state,
                    MatchCount = sampledPoints.Count(p =>
                        GeometryContainsPoint(
                            state.Feature.GetProperty("geometry"),
                            p.Item1,
                            p.Item2))
                })
                .OrderByDescending(x => x.MatchCount)
                .FirstOrDefault();

            if (bestState != null && bestState.MatchCount > 0)
            {
                county.STATEFP = bestState.State.STATEFP;
                county.STATENAME = bestState.State.NAME;
            }
            else
            {
                unmatched++;
                Console.WriteLine($"Unmatched county: {county.NAME}");
            }

            processed++;

            if (processed % 100 == 0)
            {
                Console.WriteLine(
                    $"Processed {processed}/{counties.Count} " +
                    $"in {stopwatch.Elapsed.TotalSeconds:F1}s");
            }
        }

        stopwatch.Stop();

        Console.WriteLine(
            $"County assignment completed in " +
            $"{stopwatch.Elapsed.TotalSeconds:F1}s");

        Console.WriteLine($"Unmatched counties: {unmatched}");
    }


    private static List<(double lng, double lat)> EnumerateGeometryPoints(JsonElement geometry)
    {
        var points = new List<(double lng, double lat)>();

        void Visit(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Array)
                return;

            if (element.GetArrayLength() == 2 &&
                element[0].ValueKind == JsonValueKind.Number &&
                element[1].ValueKind == JsonValueKind.Number)
            {
                points.Add((element[0].GetDouble(), element[1].GetDouble()));
                return;
            }

            foreach (var child in element.EnumerateArray())
                Visit(child);
        }

        Visit(geometry.GetProperty("coordinates"));
        return points;
    }

    private static bool GeometryContainsPoint(JsonElement geometry, double lng, double lat)
    {
        if (!geometry.TryGetProperty("type", out var typeEl))
            return false;

        var type = typeEl.GetString();
        if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
            return ContainsPointInPolygon(geometry.GetProperty("coordinates"), lng, lat);

        if (string.Equals(type, "MultiPolygon", StringComparison.OrdinalIgnoreCase))
        {
            var polygons = geometry.GetProperty("coordinates");
            foreach (var polygon in polygons.EnumerateArray())
            {
                if (ContainsPointInPolygon(polygon, lng, lat))
                    return true;
            }
        }

        return false;
    }

    private static bool ContainsPointInPolygon(JsonElement polygonCoordinates, double lng, double lat)
    {
        foreach (var ring in polygonCoordinates.EnumerateArray())
        {
            var points = new List<(double x, double y)>();
            foreach (var point in ring.EnumerateArray())
            {
                if (point.ValueKind == JsonValueKind.Array && point.GetArrayLength() >= 2)
                {
                    points.Add((point[0].GetDouble(), point[1].GetDouble()));
                }
            }

            if (points.Count > 2 && IsPointInRing(points, lng, lat))
                return true;
        }

        return false;
    }

    private static bool IsPointInRing(List<(double x, double y)> ring, double lng, double lat)
    {
        var inside = false;
        var j = ring.Count - 1;
        for (var i = 0; i < ring.Count; i++)
        {
            var (xi, yi) = ring[i];
            var (xj, yj) = ring[j];

            var denominator = yj - yi;
            var intersects = (yi > lat) != (yj > lat) &&
                Math.Abs(denominator) > 0.0000001 &&
                (lng < (((xj - xi) * (lat - yi) / denominator) + xi));

            if (intersects)
                inside = !inside;

            j = i;
        }

        return inside;
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
                _stateFpToStateName.TryGetValue(r.STATEFP, out var st))
                r.STATENAME = st;

            if (r.Kind == "County" &&
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
                "Country" => r.NAME,
                "State" => r.NAME,
                "County" => FormatCounty(r),
                "Place" => FormatPlace(r),
                _ => r.NAME
            };
        }
    }

    private static string FormatCounty(GeoPolyRecord r)
    {
        var name = string.IsNullOrWhiteSpace(r.NAME) ? "County" : r.NAME;
        if (!name.Contains("County", StringComparison.OrdinalIgnoreCase))
            name = $"{name} County";

        return !string.IsNullOrWhiteSpace(r.STATENAME)
            ? $"{name}, {r.STATENAME}"
            : name;
    }

    private static string FormatPlace(GeoPolyRecord r)
    {
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
                "Country" => r.STATEFP ?? r.NAME,
                "State" => r.STATEFP ?? r.NAME,
                "County" => !string.IsNullOrWhiteSpace(r.STATEFP) && !string.IsNullOrWhiteSpace(r.COUNTYFP)
                    ? $"{r.STATEFP}|{r.COUNTYFP}"
                    : r.NAME,
                "Place" => $"{r.STATEFP}|{r.PLACEFP}",
                _ => r.NAME
            };
        }
    }

    private static string Normalize(string s) =>
        string.Join(' ', (s ?? string.Empty)
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
