namespace FocusTimer.Lib.Component;

public abstract class BaseViewModel : BaseModel
{
    public abstract void OnInitialize();
    
    // constructor-passe fields -> constructor -> lifecycle callbacks -> fields -> properties -> methods
}