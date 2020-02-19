using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncherHost.Update.UpdateCatalog;
using FocLauncherHost.Updater;
using FocLauncherHost.Updater.Component;
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
            _updateManager = new TestUpdateManager(new Product(), @"C:\Users\Anakin\OneDrive\launcherUpdate.xml");
        }

        [TestMethod]
        public async Task NotLocallyExistingDependency()
        {
            var dependency = new Dependency();
            dependency.Name = "NotExisting.dll";
            dependency.Version = "1.0.0.0";
            dependency.Destination = ApplicationBasePath;
            dependency.Origin = "https://example.com";

            var component = DependencyHelper.DependencyToComponent(dependency);

            const ComponentAction expected = ComponentAction.Update;
            await _updateManager.CalculateComponentStatusAsync(component);

            Assert.AreEqual(expected, component.RequiredAction);
        }

        [TestMethod]
        public async Task NotLocallyExistingDependency2()
        {
            var dependency = new Dependency();
            dependency.Name = "NotExisting.dll";
            dependency.Version = "1.0.0.0";
            dependency.Destination = ApplicationBasePath;
            dependency.Origin = "https://example.com";

            var component = DependencyHelper.DependencyToComponent(dependency);

            const ComponentAction expected = ComponentAction.Update;
            await _updateManager.CalculateComponentStatusAsync(component);

            Assert.AreEqual(expected, component.RequiredAction);
        }

        [TestMethod]
        public async Task VersionLower()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "0.0.0.9";
            dependency.Destination = ApplicationBasePath;
            dependency.Origin = "https://example.com";

            var component = DependencyHelper.DependencyToComponent(dependency);

            const ComponentAction expected = ComponentAction.Update;
            await _updateManager.CalculateComponentStatusAsync(component);

            Assert.AreEqual(expected, component.RequiredAction);
        }

        [TestMethod]
        public async Task VersionHigher()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "2.0.0.0";
            dependency.Destination = ApplicationBasePath;
            dependency.Origin = "https://example.com";

            var component = DependencyHelper.DependencyToComponent(dependency);

            const ComponentAction expected = ComponentAction.Update;
            await _updateManager.CalculateComponentStatusAsync(component);

            Assert.AreEqual(expected, component.RequiredAction);
        }

        [TestMethod]
        public async Task VersionEqual()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "1.0.0.0";
            dependency.Destination = ApplicationBasePath;
            dependency.Origin = "https://example.com";

            var component = DependencyHelper.DependencyToComponent(dependency);

            const ComponentAction expected = ComponentAction.Keep;
            await _updateManager.CalculateComponentStatusAsync(component);

            Assert.AreEqual(expected, component.RequiredAction);
        }

        [TestMethod]
        public async Task VersionEqualShaEqual()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "1.0.0.0";
            dependency.Destination = ApplicationBasePath;
            dependency.Sha2 = UpdaterUtilities.GetFileHash(Path.Combine(ApplicationBasePath, dependency.Name), HashType.Sha2);
            dependency.Origin = "https://example.com";

            var component = DependencyHelper.DependencyToComponent(dependency);

            const ComponentAction expected = ComponentAction.Keep;
            await _updateManager.CalculateComponentStatusAsync(component);

            Assert.AreEqual(expected, component.RequiredAction);
        }

        [TestMethod]
        public async Task VersionEqualShaNull()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "1.0.0.0";
            dependency.Sha2 = null;
            dependency.Destination = ApplicationBasePath;
            dependency.Origin = "https://example.com";

            var component = DependencyHelper.DependencyToComponent(dependency);

            const ComponentAction expected = ComponentAction.Keep;
            await _updateManager.CalculateComponentStatusAsync(component);

            Assert.AreEqual(expected, component.RequiredAction);
        }

        [TestMethod]
        public async Task VersionEqualShaNotEqual()
        {
            var dependency = new Dependency();
            dependency.Name = "FocLauncher.dll";
            dependency.Version = "1.0.0.0";
            dependency.Destination = ApplicationBasePath;
            dependency.Sha2 = UpdaterUtilities.HexToArray("d32b568cd1b96d459e7291ebf4b25d007f275c9f13149beeb782fac0716613f8");
            dependency.Origin = "https://example.com";

            var component = DependencyHelper.DependencyToComponent(dependency);

            const ComponentAction expected = ComponentAction.Update;
            await _updateManager.CalculateComponentStatusAsync(component);

            Assert.AreEqual(expected, component.RequiredAction);
        }

        internal class TestUpdateManager : UpdateManager
        {
            public TestUpdateManager(IProductInfo productInfo, string versionMetadataPath) : base(productInfo, versionMetadataPath)
            {
            }

            protected override Task<IEnumerable<IComponent>> GetCatalogComponentsAsync(Stream catalogStream, CancellationToken token)
            {
                return Task.FromResult(Enumerable.Empty<IComponent>());
            }

            protected override Task<bool> ValidateCatalogStreamAsync(Stream inputStream)
            {
                return Task.FromResult(true);
            }
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
