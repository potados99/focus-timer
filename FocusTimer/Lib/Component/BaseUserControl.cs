using System;
using System.Windows.Controls;

namespace FocusTimer.Lib.Component;

public abstract class BaseUserControl<T> : UserControl where T : BaseViewModel
{
    protected T ViewModel;

    protected BaseUserControl()
    {
        Loaded += (_, _) =>
        {
            ViewModel = DataContext as T ?? throw new InvalidOperationException("No ViewModel given as a DataContext");
            ViewModel.OnLoaded();
        };
        
        OnInitialize();
    }

    protected abstract void OnInitialize();
}