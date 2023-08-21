using System.Windows;
using System.Windows.Controls;

namespace FocusTimer.Lib.Utility;

public class BindableMenuItem
{
    public delegate void ClickHandler(object sender, RoutedEventArgs e);
    public delegate void CheckHandler(bool isChecked);

    public event ClickHandler OnClick;
    public event CheckHandler OnCheck;

    public bool IsCheckable { get; set; }

    private bool _IsChecked;
    public bool IsChecked
    {
        get { return _IsChecked; }
        set
        {
            _IsChecked = value;
            OnCheck?.Invoke(value);
        }
    }

    public Image? Icon { get; set; }
    public string Header { get; set; }
    public BindableMenuItem[] Children { get; set; }
}