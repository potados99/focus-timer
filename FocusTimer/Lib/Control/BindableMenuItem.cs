using System.Windows;
using System.Windows.Controls;

namespace FocusTimer.Lib.Control;

/// <summary>
/// XAML 바인딩에 사용할 수 있는 MenuItem입니다.
/// 체크와 클릭 이벤트를 처리할 수 있습니다.
/// </summary>
public class BindableMenuItem
{
    public delegate void ClickHandler(object sender, RoutedEventArgs e);
    public delegate void CheckHandler(bool isChecked);

    public event ClickHandler? OnClick;
    public event CheckHandler? OnCheck;

    public bool IsCheckable { get; set; }

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            _isChecked = value;
            OnCheck?.Invoke(value);
        }
    }

    public Image? Icon { get; set; }
    public string Header { get; set; }
    public BindableMenuItem[] Children { get; set; }
}