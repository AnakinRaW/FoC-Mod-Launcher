using System.Diagnostics;
using System.Windows.Navigation;

namespace FocLauncher.Views.Dialogs;

public partial class AboutDialog
{
    public AboutDialog()
    {
        InitializeComponent();
    }

    private void OpenLicenseSite(object sender, RequestNavigateEventArgs e)
    {
        var ps = new ProcessStartInfo(e.Uri.AbsoluteUri)
        {
            UseShellExecute = true,
            Verb = "open"
        };
        Process.Start(ps);
        e.Handled = true;
    }
}