using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mebster.Myodam.UI.WPF.Views.Resouces;

public abstract class RestrictedInputTextBox : TextBox
{
    protected virtual Regex AllowedRegexPattern { get; set; }

    public RestrictedInputTextBox()
    {
        DataObject.AddPastingHandler(this, PastingHandler);
        PreviewKeyDown += OnPreviewKeyDown;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsSpaceAllowed() && e.Key == Key.Space) //handling space character
            e.Handled = true;
    }

    private bool IsSpaceAllowed()
    {
        return IsTextAllowed(" ");
    }

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        if (!IsTextAllowed(e.Text))
            e.Handled = true;
        base.OnPreviewTextInput(e);
    }

    private bool IsTextAllowed(string text)
    {
        return AllowedRegexPattern.IsMatch(text);
    }

    protected virtual void PastingHandler(object sender, DataObjectPastingEventArgs e) // handles also drag-drop-paste
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            if (!IsSpaceAllowed() && !string.IsNullOrWhiteSpace(text)) // handles unwanted space characters when copy-pasting
            {
                text = text.Trim();
            }
            if (!IsTextAllowed(text)) e.CancelCommand();
        }
        else e.CancelCommand();
    }
}


public class NumericPositiveDoubleTextBox : RestrictedInputTextBox
{
    protected override Regex AllowedRegexPattern { get; set; } = new(@"^[0-9]+(?:\.[0-9]+)?$");
}
