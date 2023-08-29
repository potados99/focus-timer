namespace FocusTimer.Library.Control.Base;

/// <summary>
/// 라이프사이클 이벤트를 가지는 모델입니다.
/// </summary>
public abstract class BaseViewModel : BaseModel
{
    public virtual void OnInitialize()
    {
    }

    public virtual void OnLoaded()
    {
    }
}