using System;

namespace FocLauncher.AssemblyHelper
{
    class AssemblyLoader
    {
        internal static void LoadAssemblies()
        {
            EmbeddedAssembly.Load("FocLauncher.Resources.HtmlAgilityPack.dll", "HtmlAgilityPack.dll");
            EmbeddedAssembly.Load("FocLauncher.Resources.Newtonsoft.Json.dll", "Newtonsoft.Json.dll");
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
    }
}
