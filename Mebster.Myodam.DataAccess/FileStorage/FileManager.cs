using Microsoft.Win32;

namespace Mebster.Myodam.DataAccess.FileStorage;

public interface IFileSystemManager
{
    string ReadAll(string path);
    void WriteAll(string path, string data);
    bool SaveSingleFileDialog(string predefinedName, out string? selectedFileName);
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
