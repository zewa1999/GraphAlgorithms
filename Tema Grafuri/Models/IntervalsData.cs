using Newtonsoft.Json;

public class IntervalsData
{
    [JsonProperty("Interval")]
    public string Interval { get; set; } = string.Empty;
}

public class IntervalsRoot
{
    [JsonProperty("Intervals")]
    public List<IntervalsData> Intervals { get; set; } = new();
}