using System;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.ViewModels.Dialogs;

namespace FocLauncher.Update.ViewModels.Dialogs;

internal abstract class UpdateImageDialog : DialogViewModel, IImageDialogViewModel
{
    public abstract ImageKey Image { get; }

    protected UpdateImageDialog(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        HasDialogFrame = true;
        IsResizable = false;
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        Title = "Launcher Update";
    }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}