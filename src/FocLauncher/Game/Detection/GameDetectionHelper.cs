﻿using System;
using NLog;

namespace FocLauncher.Game.Detection
{
    public static class GameDetectionHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static GameDetection GetGameInstallations(GameDetectionOptions detectionOptions = GameDetectionOptions.Default)
        {
            switch (detectionOptions)
            {
                case GameDetectionOptions.Default:
                    var registryResult= new RegistryGameDetector().DetectGames();

                    // TODO: Check whether it's OK an error exist when we find an installation from local path
                    if (registryResult.IsError)
                        return registryResult;

                    var localResult = new LocalGameDetector().DetectGames();
                    if (localResult.IsError)
                    {
                        Logger.Trace("No Foc installation found in current directory. Returning the registry result instead.");
                        return registryResult;
                    }
                    return localResult;
                case GameDetectionOptions.LocalOnly:
                    return new LocalGameDetector().DetectGames();
                case GameDetectionOptions.RegistryOnly:
                    return new RegistryGameDetector().DetectGames();
                default:
                    throw new ArgumentOutOfRangeException(nameof(detectionOptions), detectionOptions, null);
            }
        }
    }
}
