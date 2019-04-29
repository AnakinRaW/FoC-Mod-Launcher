using System;

namespace FocLauncher.Core.AssemblyHelper
{
    internal class AssemblyLoader
    {
        internal static void LoadEmbeddedAssemblies()
        {
            EmbeddedAssembly.Load("FocLauncher.Core.Resources.HtmlAgilityPack.dll", "HtmlAgilityPack.dll");
            EmbeddedAssembly.Load("FocLauncher.Core.Resources.Newtonsoft.Json.dll", "Newtonsoft.Json.dll");
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
    }
}
