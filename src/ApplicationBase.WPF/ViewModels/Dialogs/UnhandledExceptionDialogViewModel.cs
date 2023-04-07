using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnakinRaW.ApplicationBase.Imaging;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Controls;
using AnakinRaW.CommonUtilities.Wpf.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.ApplicationBase.ViewModels.Dialogs;

public partial class UnhandledExceptionDialogViewModel : ModalWindowViewModel, IUnhandledExceptionDialogViewModel
{
    private readonly string? _repositoryUrl;

    [ObservableProperty]
    private Exception _exception = null!;

    public string WindowCaption { get; }

    public string Header => "Oh no, something went wrong!";

    public string HandlerDescription => "If the problem keeps appearing you can file an issue on GitHub.com";

    public ICommand? Handler { get; }

    public ImageSource? HandlerIcon { get; }
    
    public string HandlerName => "Create _Issue...";

    public UnhandledExceptionDialogViewModel(Exception exception, IServiceProvider serviceProvider)
    {
        Requires.NotNull(exception, nameof(exception));
        Exception = exception;
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        IsResizable = false;
        IsCloseButtonEnabled = false;
        var env = serviceProvider.GetRequiredService<IApplicationEnvironment>();
        WindowCaption = env.ApplicationName;

        if (env.RepositoryUrl != null)
        {
            _repositoryUrl = env.RepositoryUrl.ToUri().AbsolutePath;
            Handler = new DelegateCommand(OnCreateIssue);
            if (env.RepositoryUrl.Host.Equals("github", StringComparison.OrdinalIgnoreCase))
                HandlerIcon = new BitmapImage(ImageCatalog.GithubDefinition.Source);
        }

       
    }

    private void OnCreateIssue()
    {
        if (string.IsNullOrEmpty(_repositoryUrl))
            return;
        Process.Start(_repositoryUrl!);
    }
}