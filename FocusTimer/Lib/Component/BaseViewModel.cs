namespace FocusTimer.Lib.Component;

public abstract class BaseViewModel : BaseModel
{
    public virtual void OnInitialize()
    {
    }

    public virtual void OnWindowLoaded()
    {
    }

    // constructor-passe fields -> constructor -> lifecycle callbacks -> fields -> properties -> methods
}