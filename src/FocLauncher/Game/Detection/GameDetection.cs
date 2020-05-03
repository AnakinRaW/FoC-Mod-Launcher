﻿using System.IO;
using Microsoft;

namespace FocLauncher.Game.Detection
{
    public class GameDetection
    {
        public static GameDetection NotSettedUp => new GameDetection(DetectionResult.NotSettedUp);
        public static GameDetection NotInstalled => new GameDetection(DetectionResult.NotInstalled);

        public GameType FocType { get; }

        // Setter required, because EA was too dump to fix registry values when they adapted the game for their Origin client.
        public FileInfo? FocExe { get; internal set; }

        public FileInfo? EawExe { get; }

        public DetectionResult Result { get; }

        private GameDetection(DetectionResult result)
        {
            Result = result;
            FocType = GameType.Undefined;
        }

        public GameDetection(FileInfo eawExe, FileInfo focExe)
        {
            Requires.NotNull(eawExe, nameof(eawExe));
            Requires.NotNull(focExe, nameof(focExe));
            Result = DetectionResult.Installed;
            EawExe = eawExe;
            FocExe = focExe;
            FocType = GameTypeHelper.GetGameType(this);
        }

        public bool IsError => Result != DetectionResult.Installed || FocExe == null || EawExe == null;

        public override string ToString()
        {
            return $"Game Detection Object: Result:{Result}; FoC-Type:{FocType}; FoC:'{FocExe?.FullName}'; EaW:'{EawExe?.FullName}'";
        }
    }
}
