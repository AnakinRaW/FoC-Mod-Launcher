using System;
using System.Windows.Input;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Input;

namespace FocLauncher.Update.Commands;

internal class UpdateCommand : CommandDefinition
{
    public override ImageKey Image => default;
    public override string Text { get; }
    public override ICommand Command => new DelegateCommand(() => {});
    public override string? Tooltip => null;

    public UpdateCommand(IUpdateCatalog updateCatalog, IServiceProvider serviceProvider, bool isRepair)
    {
        Text = isRepair ? "Repair" : "Update";
    }
}