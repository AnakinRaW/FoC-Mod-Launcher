using System;

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
    }
}