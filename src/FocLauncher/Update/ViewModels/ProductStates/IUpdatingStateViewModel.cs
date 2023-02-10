using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.Update.ViewModels.ProductStates;

public interface IUpdatingStateViewModel : IProductStateViewModel
{

}

public class UpdatingStateViewModel : ViewModelBase, IUpdatingStateViewModel
{
    public UpdatingStateViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}