using AnakinRaW.CommonUtilities.Wpf.Controls;

namespace FocLauncher.Controls;

public partial class ThemedModalWindow
{
    public ThemedModalWindow(IModalWindowViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}