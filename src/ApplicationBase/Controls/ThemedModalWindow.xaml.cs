using AnakinRaW.CommonUtilities.Wpf.Controls;

namespace AnakinRaW.ApplicationBase.Controls;

public partial class ThemedModalWindow
{
    public ThemedModalWindow(IModalWindowViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}