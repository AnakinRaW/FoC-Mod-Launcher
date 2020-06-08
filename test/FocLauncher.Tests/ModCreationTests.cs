using System.IO;
using FocLauncher.Game;
using FocLauncher.Mods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FocLauncher.Tests
{
    [TestClass]
    public class ModCreationTests
    {
        private static readonly string TestScenariosPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\TestScenarios"));
        private IGame _game;

        [TestInitialize]
        public void CreateGame()
        {
            _game = new Foc(new DirectoryInfo(Path.Combine(TestScenariosPath, "TwoMods")), GameType.Disk);
        }

        [TestMethod]
        public void ModCreation()
        {
            var path = Path.Combine(_game.Directory.FullName, "Mods\\ModA");
            var mod = ModFactory.CreateMod(_game, ModType.Default, path, false);
            Assert.IsNotNull(mod);
            Assert.AreEqual(ModType.Default, mod.Type);
            Assert.IsInstanceOfType(mod, typeof(Mod));
            Assert.AreEqual(path, ((Mod)mod).Directory.FullName);
        }

        [TestMethod]
        public void ModNotExists()
        {
            var path = Path.Combine(_game.Directory.FullName, "Mods\\ModC");
            Assert.ThrowsException<PetroglyphModException>(() =>
                ModFactory.CreateMod(_game, ModType.Default, path, false));
        }
    }
}