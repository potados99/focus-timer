using System.Windows;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace FocusTimer.Lib.Component;

public abstract class BaseWindow<T> : Window where T : BaseViewModel
{
    protected readonly T ViewModel = App.Provider.GetService<T>()!;

    protected BaseWindow()
    {
        DataContext = ViewModel;
        OnInitialize();
        ViewModel.OnInitialize();
    }

    protected abstract void OnInitialize();
}