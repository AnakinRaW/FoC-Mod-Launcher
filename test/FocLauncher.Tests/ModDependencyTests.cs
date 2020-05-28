using System.IO;
using FocLauncher.Game;
using FocLauncher.ModInfo;
using FocLauncher.Mods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FocLauncher.Tests
{
    [TestClass]
    public class ModDependencyTests
    {
        private static readonly string TestScenariosPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\TestScenarios"));
        private IGame _game;

        [TestInitialize]
        public void CreateGame()
        {
            _game = new Foc(new DirectoryInfo(Path.Combine(TestScenariosPath, "TwoMods")), GameType.Disk);
        }

        [TestMethod]
        public void TestCycleDetection()
        {
            var modA = ModFactory.CreateMod(_game, ModType.Default, Path.Combine(_game.Directory.FullName, "Mods\\ModA"), false);
            var modB = ModFactory.CreateMod(_game, ModType.Default, Path.Combine(_game.Directory.FullName, "Mods\\ModB"), false);

            var modInfoA = new ModInfoData();
            var modInfoB = new ModInfoData();

            // Setup cycle
            modInfoA.Dependencies.Add(modB);
            modInfoB.Dependencies.Add(modA);

            _game.AddMod(modA);
            _game.AddMod(modB);

            ((ModBase)modA).SetModInfo(modInfoA);
            ((ModBase)modB).SetModInfo(modInfoB);

            Assert.ThrowsException<PetroglyphModException>(() =>
                modA.ResolveDependencies(ModDependencyResolveStrategy.FromExistingModsRecursive));
        }

        [TestMethod]
        public void Test1()
        {
            var modA = ModFactory.CreateMod(_game, ModType.Default, Path.Combine(_game.Directory.FullName, "Mods\\ModA"), false);
            var modB = ModFactory.CreateMod(_game, ModType.Default, Path.Combine(_game.Directory.FullName, "Mods\\ModB"), false);

            var modInfoA = new ModInfoData();
            var modInfoB = new ModInfoData();

            modInfoA.Dependencies.Add(modB);

            _game.AddMod(modA);
            _game.AddMod(modB);

            ((ModBase)modA).SetModInfo(modInfoA);
            ((ModBase)modB).SetModInfo(modInfoB);

            modA.ResolveDependencies(ModDependencyResolveStrategy.FromExistingModsRecursive);

            Assert.AreSame(1, modInfoA.Dependencies.Count);
            Assert.AreSame(1, modA.ExpectedDependencies);
            Assert.IsTrue(modA.DependenciesResolved);
            Assert.IsTrue(modA.HasDependencies);
            Assert.AreSame(modA.ExpectedDependencies, modA.Dependencies);
        }
    }
}
