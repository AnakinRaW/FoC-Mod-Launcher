using System;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Theming;

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

    /// <summary>
    /// Allows to add resources to the theme.
    /// </summary>
    /// <returns>A <see cref="ResourceDictionary"/> with resources
    /// or null if no custom resources are required for this theme.</returns>
    ResourceDictionary? CustomResources();
}