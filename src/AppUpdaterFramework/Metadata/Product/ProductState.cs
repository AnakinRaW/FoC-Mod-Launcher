using System;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Product;

[Flags]
public enum ProductState
{
    Installed = 0,
    RestartRequired = 1,
    ElevationRequired = 2
}