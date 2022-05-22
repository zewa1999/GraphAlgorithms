namespace Tema_Grafuri.Dtos;

public class TeachersDataDto
{
    public string Teacher { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Hours { get; set; } = string.Empty;
    public List<string> Groups { get; set; } = new();
}