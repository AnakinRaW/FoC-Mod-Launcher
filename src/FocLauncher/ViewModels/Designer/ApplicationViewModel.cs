using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace FocLauncher.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class ApplicationViewModel : IApplicationViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? CloseDialogRequest;
    public WindowState MinMaxState { get; set; }
    public bool RightToLeft { get; set; }
    public string Title { get; set; }
    public bool IsFullScreen { get; set; }
    public bool IsResizable { get; set; }
    public bool HasMinimizeButton { get; set; }
    public bool HasMaximizeButton { get; set; }
    public bool IsGripVisible { get; set; }
    public TaskBarIconProgressState ProgressState { get; set; }
    public IStatusBarViewModel StatusBar { get; }
    public ILauncherViewModel CurrentViewModel { get; set; } = new MainPageViewModel();

    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public void CloseDialog()
    {
        throw new NotImplementedException();
    }

    public void OnClosing(CancelEventArgs e)
    {
        throw new NotImplementedException();
    }
}