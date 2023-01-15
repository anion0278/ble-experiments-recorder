using Microsoft.Win32;
using System.Diagnostics;

namespace BleRecorder.DataAccess.FileStorage;

public interface IFileSystemManager
{
    string ReadAll(string path);
    void WriteAll(string path, string data);
    string GetFileDir(string filePath);
    bool FileExists(string path);
}

public class FileSystemManager : IFileSystemManager
{
    public string ReadAll(string path)
    {
        return File.ReadAllText(path);
    }

    public void WriteAll(string path, string data)
    {
        File.WriteAllText(path, data);
    }

    public string GetFileDir(string filePath)
    {
        return Path.GetDirectoryName(filePath);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }
}
