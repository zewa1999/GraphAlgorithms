using Newtonsoft.Json;

namespace Tema_Grafuri.Dtos;

public class ClassroomDataDto
{
    public string ClassroomName { get; set; } = string.Empty;
    public string ClassroomType { get; set; } = string.Empty;
    public int ClassroomCapacity { get; set; }
}