using Sklavenwalker.ProductUpdater.Services;
using System;
using FocLauncher.Services;
using FocLauncher.Threading;
using FocLauncher.Update.ProductMetadata;
using FocLauncher.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Update;

public partial class UpdateWindow
{
    private readonly IServiceProvider _serviceProvider;

    public UpdateWindow(IServiceProvider sp)
    {
        _serviceProvider = sp;
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        SearchForUpdate();
    }

    public async void SearchForUpdate()
    {
        try
        {
            var s = _serviceProvider.GetRequiredService<IProductUpdateProviderService>();
            var lps = new LauncherProductService(_serviceProvider);

            var result = await s.CheckForUpdates(lps.GetCurrentInstance());
        }
        catch (Exception e)
        {
            var ds = _serviceProvider.GetRequiredService<IQueuedDialogService>();
            const string header = "Unable to get online Update Information";
            ds.ShowDialog(new ErrorMessageDialogViewModel(header, e.Message, _serviceProvider)).Forget();
        }
    }
}