using System.ComponentModel;

namespace FocusTimer.Library.Control.Base;

/// <summary>
/// 바인딩 가능한 속성들을 가지는 모델입니다.
/// </summary>
public class BaseModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}