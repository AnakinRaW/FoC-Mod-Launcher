// TODO: Make sure you build this as Release when you ship the theme with your mod

using System;
// TODO: Make sure the FocLauncher.Theming.dll is a reference of this project in order to get the necessary type information.
// You do not need to include the FocLauncher.Theming.dll with your mod!
using FocLauncher.Theming;

//TODO: The namespace needs to be the same as your generated .dll file. By default the .dll files has the same name as the project (In this case it's SampleTheme)
namespace SampleTheme
{
    // TODO: The class MUST be called Theme and MUST be derived from AbstractTheme
    public class Theme : AbstractTheme
    {
        /// <summary>
        /// This name will get displayed in the theme selection dialog
        /// </summary>
        public override string Name => "Sample Theme";

        public override Uri GetResourceUri()
        {
            //TODO: Make sure you add the correct namespace here, which in this case is SampleTheme. The / in from is required
            //TODO: You can choose any file name for the .xaml file but I suggest you use the template I give you here
            return new Uri("/SampleTheme;component/Theme.xaml", UriKind.Relative);
        }
    }
}
