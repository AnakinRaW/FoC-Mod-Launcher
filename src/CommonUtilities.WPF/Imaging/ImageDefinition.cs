using System;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

public struct ImageDefinition
{
    public ImageMoniker Moniker;
    public Uri Source;
    public ImageFileKind Kind;
    public bool CanTheme;
}