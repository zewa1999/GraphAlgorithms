using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace Tema_Grafuri;

public static class JsonDeserializer
{
    public static T? ToJson<T>(string jsonName) where T : new()
    {
        try
        {
            var path = Directory.GetCurrentDirectory() + @"\Data\" + jsonName;
            var json = File.ReadAllText(path);
            T? myDeserializedClass = JsonConvert.DeserializeObject<T>(json);

            return myDeserializedClass;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return new();
    }
}