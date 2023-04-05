using System;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace AnakinRaW.ApplicationBase.ViewModels.Dialogs;

internal abstract class UpdateImageDialog : DialogViewModel, IImageDialogViewModel
{
    public abstract ImageKey Image { get; }

    protected UpdateImageDialog(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        HasDialogFrame = true;
        IsResizable = false;
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        Title = "Application Update";
    }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}