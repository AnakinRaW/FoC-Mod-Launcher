using System;
using System.Collections.Generic;
using System.Linq;
using FocLauncher.Controls;
using FocLauncher.Game;
using FocLauncher.Mods;

namespace FocLauncher
{
    internal class LauncherSession : IInvocationController
    {
        private readonly ILauncherWindowModel _mainWindowModel;
        private readonly IGame _game;

        public event EventHandler<IPetroglyhGameableObject> Started;
        public event EventHandler<IPetroglyhGameableObject> StartFailed;

        internal LauncherSession(ILauncherWindowModel mainWindowModel, IGame game)
        {
            _mainWindowModel = mainWindowModel;
            _game = game;
        }

        public bool Invoke(IEnumerable<object> items)
        {
            var itemsList = items.ToList();

            if (itemsList.Count != 1)
                return false;

            var item = itemsList[0];
            IPetroglyhGameableObject gameableObject;
            switch (item)
            {
                case IPetroglyhGameableObject petroglyhGameableObject:
                    gameableObject = petroglyhGameableObject;
                    break;
                case LauncherListItemModel model:
                    gameableObject = model.GameObject;
                    break;
                default:
                    return false;
            }

            return RunGame(gameableObject);
        }

        private bool RunGame(IPetroglyhGameableObject gameableObject)
        {
            if (_game is null)
                return false;

            var args = new GameCommandArguments();
            if (gameableObject is IMod mod)
                args.Mod = mod;
            args.IgnoreAsserts = _mainWindowModel.IgnoreAsserts;
            args.NoArtProcess = _mainWindowModel.NoArtProcess;
            args.Windowed = _mainWindowModel.Windowed;


            bool started;
            if (_mainWindowModel.UseDebugBuild && _game is IDebugable debugable)
                started = debugable.DebugGame(args, gameableObject.IconFile);
            else
                started = _game.PlayGame(args, gameableObject.IconFile);

            if (started)
                Started?.Invoke(this, gameableObject);
            else
                StartFailed?.Invoke(this, gameableObject);

            return started;
        }
    }
}