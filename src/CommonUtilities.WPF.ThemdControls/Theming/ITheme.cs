using System;

namespace Sklavenwalker.CommonUtilities.Wpf.Theming;

public interface ITheme : IEquatable<ITheme>
{
    /// <summary>
    /// The ID of the theme.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The localized displayed text
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Gets the resource URI.
    /// </summary>
    /// <returns>The URI</returns>
    Uri ResourceUri { get; }
}