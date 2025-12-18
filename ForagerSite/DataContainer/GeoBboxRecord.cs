namespace ForagerSite.DataContainer
{
    public class GeoBboxRecord
    {
        public string Kind { get; set; } = "";           // "State" | "County" | "Place"

        public string NAME { get; set; } = "";

        public string? STATEFP { get; set; }             // states/counties/places
        public string? STUSPS { get; set; }              // states only (optional)
        public string? COUNTYFP { get; set; }            // counties only
        public string? COUNTYNAME { get; set; }          // places only (we will fill this)
        public string? STATENAME { get; set; }           // counties/places (we will fill this)

        public double west { get; set; }
        public double south { get; set; }
        public double east { get; set; }
        public double north { get; set; }

        // computed
        public string Display { get; set; } = "";
    }

}
