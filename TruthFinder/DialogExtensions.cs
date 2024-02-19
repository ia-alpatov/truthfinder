using System.ComponentModel;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs;
using TruthFinder.ViewModels;

namespace TruthFinder;

public static class DialogExtensions
{
    public static async Task<SelectModelViewModel> ShowModelChooser(this IDialogService dialog, INotifyPropertyChanged ownerViewModel)
    {
        var viewModel = dialog.CreateViewModel<SelectModelViewModel>();
        var result = await dialog.ShowDialogAsync(ownerViewModel, viewModel).ConfigureAwait(true);
        return viewModel;
    }
}