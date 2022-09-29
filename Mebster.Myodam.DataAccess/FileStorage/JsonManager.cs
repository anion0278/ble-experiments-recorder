using Mebster.Myodam.Models.TestSubject;
using System.Text.Json;

namespace Mebster.Myodam.DataAccess.FileStorage;


public interface IJsonManager
{
    T? Deserialize<T>(string textData);
    string Serialize<T>(T data);
}

public class JsonManager : IJsonManager
{
    public T? Deserialize<T>(string textData)
    {
        return JsonSerializer.Deserialize<T>(textData);
    }

    public string Serialize<T>(T data)
    {
        return JsonSerializer.Serialize<T>(data, new JsonSerializerOptions { WriteIndented = true });
    }

}