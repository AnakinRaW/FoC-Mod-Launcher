using System;
using System.IO;
using System.Threading.Tasks;
using FocLauncherHost.Updater;
using FocLauncherHost.Updater.MetadataModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FocLauncher.Updater.Tests
{
    [TestClass]
    public class UpdaterDependencyTests
    {
        private static readonly string ApplicationBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FoC Launcher");
        private UpdateManager _updateManager;

        [TestInitialize]
        public void SetUpdateManager()
        {
            Environment.SetEnvironmentVariable(LauncherConstants.ApplicationBaseVariable, ApplicationBasePath, EnvironmentVariableTarget.Process);
            _updateManager = new UpdateManager(new Product(), @"C:\Users\Anakin\OneDrive\launcherUpdate.xml");
        }

        [TestMethod]
        public async Task NotLocallyExistingDependency()
        {
            var dependency = new Dependency();
            dependency.Name = "NotExisting.dll";
            dependency.Version = "1.0.0.0";
            dependency.InstallLocation = InstallLocation.AppData;

            const DependencyAction expected = DependencyAction.Update;
            var result = await _updateManager.CheckDependencyAsync(dependency);

            Assert.AreEqual(expected, result.RequiredAction);
        }

        [TestMethod]
        public async Task NotLocallyExistingDependency2()
        {
            var dependency = new Dependency();
            dependency.Name = "NotExisting.dll";
            dependency.Version = "1.0.0.0";
            dependency.InstallLocation = InstallLocation.Current;

            const DependencyAction expected = DependencyAction.Update;
            var result = await _updateManager.CheckDependencyAsync(dependency);

            Assert.AreEqual(expected, result.RequiredAction);
        }

        [TestMethod]
        public async Task VersionLower()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "0.0.0.9";
            dependency.InstallLocation = InstallLocation.AppData;

            const DependencyAction expected = DependencyAction.Keep;
            var result = await _updateManager.CheckDependencyAsync(dependency);

            Assert.AreEqual(expected, result.RequiredAction);
        }

        [TestMethod]
        public async Task VersionHigher()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "2.0.0.0";
            dependency.InstallLocation = InstallLocation.AppData;

            const DependencyAction expected = DependencyAction.Update;
            var result = await _updateManager.CheckDependencyAsync(dependency);

            Assert.AreEqual(expected, result.RequiredAction);
        }

        [TestMethod]
        public async Task VersionEqual()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "1.0.0.0";
            dependency.InstallLocation = InstallLocation.AppData;

            const DependencyAction expected = DependencyAction.Keep;
            var result = await _updateManager.CheckDependencyAsync(dependency);

            Assert.AreEqual(expected, result.RequiredAction);
        }

        [TestMethod]
        public async Task VersionEqualShaEqual()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "1.0.0.0";
            dependency.InstallLocation = InstallLocation.AppData;
            dependency.Sha2 = UpdaterUtilities.GetSha2(Path.Combine(ApplicationBasePath, dependency.Name));

            const DependencyAction expected = DependencyAction.Keep;
            var result = await _updateManager.CheckDependencyAsync(dependency);

            Assert.AreEqual(expected, result.RequiredAction);
        }

        [TestMethod]
        public async Task VersionEqualShaNull()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "1.0.0.0";
            dependency.Sha2 = null;
            dependency.InstallLocation = InstallLocation.AppData;

            const DependencyAction expected = DependencyAction.Keep;
            var result = await _updateManager.CheckDependencyAsync(dependency);

            Assert.AreEqual(expected, result.RequiredAction);
        }

        [TestMethod]
        public async Task VersionEqualShaNotEqual()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "1.0.0.0";
            dependency.InstallLocation = InstallLocation.AppData;
            dependency.Sha2 = UpdaterUtilities.HexToArray("d32b568cd1b96d459e7291ebf4b25d007f275c9f13149beeb782fac0716613f8");

            const DependencyAction expected = DependencyAction.Update;
            var result = await _updateManager.CheckDependencyAsync(dependency);

            Assert.AreEqual(expected, result.RequiredAction);
        }

        internal class Product : IProductInfo
        {
            public string Name { get; } = "FoC-Launcher";
            public string Author { get; }
            public string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FoC Launcher");
            public string CurrentLocation => GetType().Assembly.Location;
        }
    }
}
