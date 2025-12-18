using System.Text.Json;

namespace ForagerSite.DataContainer;

public class GeoPolyRecord
{
    public string Kind { get; set; } = "";   // State / County / Place
    public string Id { get; set; } = "";     // StateFP | STATEFP|COUNTYFP | STATEFP|PLACEFP
    public string NAME { get; set; } = "";

    public string? STATEFP { get; set; }
    public string? COUNTYFP { get; set; }
    public string? PLACEFP { get; set; }

    public string? STATENAME { get; set; }
    public string? COUNTYNAME { get; set; }

    public string Display { get; set; } = "";

    // We keep the whole GeoJSON Feature so JS can draw it exactly.
    public JsonElement Feature { get; set; }

    // Derived bounds (fast filtering + quick fit fallback)
    public double West { get; set; }
    public double South { get; set; }
    public double East { get; set; }
    public double North { get; set; }
}
