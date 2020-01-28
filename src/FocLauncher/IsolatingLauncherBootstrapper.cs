﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FocLauncher
{
    public class IsolatingLauncherBootstrapper : MarshalByRefObject, IDisposable
    {
        public IsolatingLauncherBootstrapper()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        public void StartLauncherApplication()
        {
            var app = new LauncherApp();
            app.Run();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var fields = args.Name.Split(',');
            var name = fields[0];
            var culture = fields[2];

            if (name.EndsWith(".resources") && !culture.EndsWith("neutral"))
                return null;

            var files = Directory.EnumerateFiles(LauncherConstants.ApplicationBasePath, "*.dll", SearchOption.TopDirectoryOnly);
            var dll = files.FirstOrDefault(x => $"{name}.dll".Equals(Path.GetFileName(x)));
            return dll == null ? null : Assembly.LoadFile(dll);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!(e.ExceptionObject is Exception exception))
                return;
            if (exception is LauncherException)
                return;

            var message = $"{exception.Message} ({exception.GetType().Name})";
            var wrappedException = new LauncherException(message, exception);
            wrappedException.Data.Add(LauncherException.LauncherDataModelKey, LauncherDataModel.Instance?.GetDebugInfo());
            throw wrappedException;
        }
    }
}