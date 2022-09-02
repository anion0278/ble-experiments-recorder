using Microsoft.Win32;
using System.Diagnostics;

namespace Mebster.Myodam.DataAccess.FileStorage;

public interface IFileSystemManager
{
    string ReadAll(string path);
    void WriteAll(string path, string data);
    bool SaveSingleFileDialog(string predefinedName, out string? selectedFileName);
    bool FileExists(string path);
    void OpenOrShowDir(string dirName);
    string GetFileDir(string filePath);
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

    public void OpenOrShowDir(string dirName)
    {
        var psi = new ProcessStartInfo()
        {
            FileName = dirName,
            Verb = "open", // utilizes opened folder if available
            UseShellExecute = true
        };
        Process.Start(psi);
    }

    public string GetFileDir(string filePath)
    {
        return Path.GetDirectoryName(filePath);
    }

    public bool SaveSingleFileDialog(string predefinedName, out string? selectedFileName)
    {
        selectedFileName = null;
        var saveFileDialog = new SaveFileDialog
        {
            Title = "Export data",
            FileName = predefinedName,
            Filter = "Excel Document|*.xlsx",
        };
        bool? dialogResult = saveFileDialog.ShowDialog();
        if (dialogResult != true) return false;

        selectedFileName = saveFileDialog.FileName;
        return true;
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }
}
