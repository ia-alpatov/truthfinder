using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using HanumanInstitute.MvvmDialogs.Avalonia;
using TruthFinder.ViewModels;
using TruthFinder.Views;

namespace TruthFinder;


public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        Register<MainWindowViewModel, MainWindow>();
        Register<SelectModelViewModel, SelectModelWindow>();
    }
}