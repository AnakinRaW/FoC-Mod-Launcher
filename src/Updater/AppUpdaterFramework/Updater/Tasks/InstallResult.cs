﻿namespace AnakinRaW.AppUpdaterFramework.Updater.Tasks;

public enum InstallResult
{
    Success,
    SuccessRestartRequired,
    Failure,
    FailureElevationRequired,
    Cancel
}