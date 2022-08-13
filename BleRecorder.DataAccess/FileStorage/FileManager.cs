namespace BleRecorder.DataAccess.FileStorage;

public interface IFileManager
{
    string ReadAll(string path);
    void WriteAll(string path, string data);
    bool FileExists(string path);
}

public class FileManager : IFileManager
{
    public string ReadAll(string path)
    {
        return File.ReadAllText(path);
    }

    public void WriteAll(string path, string data)
    {
        File.WriteAllText(path, data);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }
}
