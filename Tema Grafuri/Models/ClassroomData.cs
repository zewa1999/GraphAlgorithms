using Newtonsoft.Json;

public class ClassroomData
{
    [JsonProperty("ClassroomName")]
    public string ClassroomName { get; set; } = string.Empty;

    [JsonProperty("ClassroomType")]
    public string ClassroomType { get; set; } = string.Empty;

    [JsonProperty("ClassroomCapacity")]
    public int ClassroomCapacity { get; set; }
}

public class ClassroomRoot
{
    [JsonProperty("Classrooms")]
    public List<ClassroomData> Classrooms { get; set; } = new();
}