using System;
using System.Windows.Controls;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Lib.Component;

public abstract class BaseUserControl<T> : UserControl where T : BaseViewModel
{
    protected T ViewModel;

    protected BaseUserControl()
    {
        this.GetLogger().Info("Control을 초기화합니다.");

        Loaded += (_, _) =>
        {
            ViewModel = DataContext as T ?? throw new InvalidOperationException("No ViewModel given as a DataContext");
            ViewModel.OnLoaded();
        };
        
        OnInitialize();
        
        this.GetLogger().Info("Control이 초기화되었습니다.");
    }

    protected abstract void OnInitialize();
}