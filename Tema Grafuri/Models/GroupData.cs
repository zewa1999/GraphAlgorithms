using AutoMapper;
using Newtonsoft.Json;
using Tema_Grafuri.Dtos;

public class GroupData : ITypeConverter<GroupData, GroupDataDto>
{
    [JsonProperty("GroupName")]
    public string GroupName { get; set; } = string.Empty;

    [JsonProperty("GroupSize")]
    public int GroupSize { get; set; }

    [JsonProperty("GroupComponents")]
    public string GroupComponents { get; set; } = string.Empty;

    public GroupDataDto Convert(GroupData source, GroupDataDto destination, ResolutionContext context)
    {
        destination = destination ?? new GroupDataDto();
        destination.GroupName = source.GroupName;
        destination.GroupSize = source.GroupSize;
        destination.GroupComponents = source.GroupComponents.Split(' ').ToList();
        return destination;
    }
}

public class GroupDataRoot
{
    [JsonProperty("Groups")]
    public List<GroupData> Groups { get; set; } = new();
}