using Microsoft.Win32;
using System.Diagnostics;

namespace Mebster.Myodam.UI.WPF.Views.Resouces;

public interface IDialogHelpers
{
    bool SaveSingleFileDialog(string predefinedName, out string? selectedFileName);
    void OpenOrShowDir(string dirName);
}

public class DialogHelpers : IDialogHelpers
{
    public bool SaveSingleFileDialog(string predefinedName, out string? selectedFileName)
    {
        selectedFileName = null;
        var saveFileDialog = new SaveFileDialog
        {
            Title = "Export data",
            FileName = predefinedName,
            Filter = "Excel Document|*.xlsx", // TODO Builder/Factory
        };
        bool? dialogResult = saveFileDialog.ShowDialog();
        if (dialogResult != true) return false;

        selectedFileName = saveFileDialog.FileName;
        return true;
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
}