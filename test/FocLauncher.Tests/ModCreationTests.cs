using System.IO;
using EawModinfo.Spec;
using FocLauncher.Game;
using FocLauncher.Mods;
using Xunit;

namespace FocLauncher.Tests
{
    public class ModCreationTests
    {
        private static readonly string TestScenariosPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\TestScenarios"));
        private IGame _game;

        public ModCreationTests()
        {
            _game = new Foc(new DirectoryInfo(Path.Combine(TestScenariosPath, "TwoMods")), GameType.Disk);
        }

        [Fact]
        public void ModCreation()
        {
            var path = Path.Combine(_game.Directory.FullName, "Mods\\ModA");
            var mod = ModFactory.CreateMod(_game, ModType.Default, path, false);
            Assert.NotNull(mod);
            Assert.Equal(ModType.Default, mod.Type);
            Assert.IsType<Mod>(mod);
            Assert.Equal(path, ((Mod)mod).Directory.FullName);
        }

        [Fact]
        public void ModNotExists()
        {
            var path = Path.Combine(_game.Directory.FullName, "Mods\\ModC");
            Assert.Throws<PetroglyphModException>(() =>
                ModFactory.CreateMod(_game, ModType.Default, path, false));
        }
    }
}