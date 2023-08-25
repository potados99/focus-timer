using FocusTimer.Features.Timer.Slot;
using FocusTimer.Lib.Utility;
using log4net;
using log4net.Core;

namespace FocusTimer.Lib.Component;

public abstract class BaseViewModel : BaseModel
{
    protected ILog L => this.GetLogger();
    
    public virtual void OnInitialize()
    {
    }

    public virtual void OnLoaded()
    {
    }

    // constructor-passe fields -> constructor -> lifecycle callbacks -> fields -> properties -> methods
}