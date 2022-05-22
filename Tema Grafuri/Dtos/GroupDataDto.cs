using AutoMapper;

namespace Tema_Grafuri.Dtos;

public class GroupDataDto
{
    public string GroupName { get; set; } = string.Empty;
    public int GroupSize { get; set; }
    public List<string> GroupComponents { get; set; } = new();
}