using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EawModinfo.Spec;
using EawModinfo.Spec.Steam;
using FocLauncher.Game;
using FocLauncher.Mods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Versioning;

namespace FocLauncher.Tests
{
    public class ModDependencyTest
    {
    }


    [TestClass]
    public class ModDependencyTests
    {
        private static readonly string TestScenariosPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\TestScenarios"));
        private IGame _game;
        private IMod _modA;
        private IMod _modB;
        private IMod _modC;
        private IMod _modE;
        private IMod _modD;

        [TestInitialize]
        public void CreateGame()
        {
            _game = new Foc(new DirectoryInfo(Path.Combine(TestScenariosPath, "FiveMods")), GameType.Disk);
            _modA = ModFactory.CreateMod(_game, ModType.Default, Path.Combine(_game.Directory.FullName, "Mods\\ModA"), false);
            _modB = ModFactory.CreateMod(_game, ModType.Default, Path.Combine(_game.Directory.FullName, "Mods\\ModB"), false);
            _modC = ModFactory.CreateMod(_game, ModType.Default, Path.Combine(_game.Directory.FullName, "Mods\\ModC"), false);
            _modD = ModFactory.CreateMod(_game, ModType.Default, Path.Combine(_game.Directory.FullName, "Mods\\ModD"), false);
            _modE = ModFactory.CreateMod(_game, ModType.Default, Path.Combine(_game.Directory.FullName, "Mods\\ModE"), false);
            _game.AddMod(_modA);
            _game.AddMod(_modB);
            _game.AddMod(_modC);
            _game.AddMod(_modD);
            _game.AddMod(_modE);
        }

        [TestMethod]
        public void SingleDependency()
        {
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
            });

            Assert.AreEqual(1, _modA.ExpectedDependencies);
            Assert.IsTrue(_modA.DependenciesResolved);
            Assert.IsTrue(_modA.HasDependencies);
            Assert.AreEqual(_modA.ExpectedDependencies, _modA.Dependencies.Count);
        }
        
        [TestMethod]
        public void TestNoCycle()
        {
            // A : B, C
            // B : D
            // C : E
            // Expected list: (A,) B, C, D, E
            var expected = new List<IMod> { _modA, _modB, _modC, _modD, _modE };
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                a.Dependencies.Add(_modC);
                b.Dependencies.Add(_modD);
                c.Dependencies.Add(_modE);
            });
            
            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            CollectionAssert.AreEqual(expected, mods.ToList());
            AssertRecursive(expected);
        }

        [TestMethod]
        public void TestNoCycle2()
        {


            // A : C, B
            // B : D
            // C : E
            // Expected list: (A,) C, B, E, D
            var expected = new List<IMod> { _modA, _modC, _modB, _modE, _modD };
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modC);
                a.Dependencies.Add(_modB);
                b.Dependencies.Add(_modD);
                c.Dependencies.Add(_modE);
            });

            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            CollectionAssert.AreEqual(expected, mods.ToList());
            AssertRecursive(expected);
        }

        [TestMethod]
        public void TestNoCycle3()
        {
            // A : B, C
            // B : D
            // C : D
            // D : E
            // Actual List: (A,) B, C, D, D, E, E
            // Reduced Expected list: (A,) B, C, D, E
            var expected = new List<IMod> { _modA, _modB, _modC, _modD, _modE };
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                a.Dependencies.Add(_modC);
                b.Dependencies.Add(_modD);
                c.Dependencies.Add(_modD);
                d.Dependencies.Add(_modE);
            });


            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            CollectionAssert.AreEqual(expected, mods.ToList());
            AssertRecursive(expected);
        }


        [TestMethod]
        public void TestNoCycle4()
        {
            // A : B, C, D
            // B : E
            // C : E
            // D : E
            // Actual List: (A,) B, C, D, E, E, E
            // Reduced Expected list: (A,) B, C, D, E
            var expected = new List<IMod> { _modA, _modB, _modC, _modD, _modE };
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                a.Dependencies.Add(_modC);
                a.Dependencies.Add(_modD);
                b.Dependencies.Add(_modE);
                c.Dependencies.Add(_modE);
                d.Dependencies.Add(_modE);
            });

            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            CollectionAssert.AreEqual(expected, mods.ToList());
            AssertRecursive(expected);
        }

        [TestMethod]
        public void TestNoCycle5()
        {
            // A : B, C
            // B : E
            // C : D
            // E : D
            // Actual List: (A,) B, C, E, D, D
            // Reduced Expected list: (A,) B, C, E, D
            var expected = new List<IMod> { _modA, _modB, _modC, _modE, _modD };
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                a.Dependencies.Add(_modC);
                b.Dependencies.Add(_modE);
                c.Dependencies.Add(_modD);
                e.Dependencies.Add(_modD);
            });
            
            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            CollectionAssert.AreEqual(expected, mods.ToList());
            AssertRecursive(expected);
        }

        [TestMethod]
        public void TestCycleSelf()
        {
            // A : A
            // Cycle!
            Assert.ThrowsException<PetroglyphModException>(() =>
            {
                SetModInfo((a, b, c, d, e) =>
                {
                    a.Dependencies.Add(_modA);
                });
            });
            Assert.ThrowsException<PetroglyphModException>(() => AssertRecursive(null));
        }

        [TestMethod]
        public void TestCycle()
        {
            // A : B
            // B : A
            // Cycle!
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                b.Dependencies.Add(_modA);
            });
            
            var resolver = new ModDependencyTraverser(_modA);
            Assert.ThrowsException<PetroglyphModException>(() => resolver.Traverse());
            Assert.ThrowsException<PetroglyphModException>(() => AssertRecursive(null));
        }

        [TestMethod]
        public void TestCycleComlex()
        {
            // A : B
            // B : C, D
            // D : E
            // E : A
            // Cycle!
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                b.Dependencies.Add(_modC);
                b.Dependencies.Add(_modD);
                d.Dependencies.Add(_modE);
                e.Dependencies.Add(_modA);
            });

            var resolver = new ModDependencyTraverser(_modA);
            Assert.ThrowsException<PetroglyphModException>(() => resolver.Traverse());
            Assert.ThrowsException<PetroglyphModException>(() => AssertRecursive(null));
        }

        private void SetModInfo(Action<IModIdentity, IModIdentity, IModIdentity, IModIdentity, IModIdentity> setAction)
        {
            var modInfoA = new MyModinfo();
            var modInfoB = new MyModinfo();
            var modInfoC = new MyModinfo();
            var modInfoD = new MyModinfo();
            var modInfoE = new MyModinfo();

            setAction(modInfoA, modInfoB, modInfoC, modInfoD, modInfoE);

            ((ModBase)_modA).SetModInfo(modInfoA);
            ((ModBase)_modB).SetModInfo(modInfoB);
            ((ModBase)_modC).SetModInfo(modInfoC);
            ((ModBase)_modD).SetModInfo(modInfoD);
            ((ModBase)_modE).SetModInfo(modInfoE);

            _modA.ResolveDependencies(ModDependencyResolveStrategy.FromExistingMods);
            _modB.ResolveDependencies(ModDependencyResolveStrategy.FromExistingMods);
            _modC.ResolveDependencies(ModDependencyResolveStrategy.FromExistingMods);
            _modD.ResolveDependencies(ModDependencyResolveStrategy.FromExistingMods);
            _modE.ResolveDependencies(ModDependencyResolveStrategy.FromExistingMods);

        }

        private void ResetDependencies()
        {
            ((ModBase)_modA).ResetDependencies();
            ((ModBase)_modB).ResetDependencies();
            ((ModBase)_modC).ResetDependencies();
            ((ModBase)_modD).ResetDependencies();
            ((ModBase)_modE).ResetDependencies();
        }

        private void AssertRecursive(ICollection expected)
        {
            ResetDependencies();
            Assert.IsTrue(_modA.ResolveDependencies(ModDependencyResolveStrategy.FromExistingModsRecursive));
            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            CollectionAssert.AreEqual(expected, mods.ToList());
        }

        private class MyModinfo : IModinfo
        {
            public bool Equals(IModIdentity other)
            {
                throw new NotImplementedException();
            }

            public string Name { get; }
            public SemanticVersion? Version { get; }
            public IList<IModReference> Dependencies { get; }
            public string Summary { get; }
            public string Icon { get; }
            public IDictionary<string, object> Custom { get; }
            public ISteamData? SteamData { get; }
            public IEnumerable<ILanguageInfo> Languages { get; }

            public MyModinfo()
            {
                Dependencies = new List<IModReference>();
            }
        }
    }
}
