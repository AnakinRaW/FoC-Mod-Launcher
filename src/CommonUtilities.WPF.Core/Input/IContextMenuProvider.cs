using System.Windows.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Input;

/// <summary>
/// A Provider to get a specific context menu
/// </summary>
public interface IContextMenuProvider
{
    /// <summary>
    /// Gets a <see cref="ContextMenu"/> by a given condition or <see langword="null"/> if not context menu was found.
    /// </summary>
    /// <param name="data">Option do determinate which context menu to return</param>
    /// <returns>Returns the context menu</returns>
    ContextMenu? Provide(object data);
}

public interface IContextMenuProvider<in T> : IContextMenuProvider
{
    /// <summary>
    /// Gets a <see cref="ContextMenu"/> by a given condition or <see langword="null"/> if not context menu was found.
    /// </summary>
    /// <param name="data">Option do determinate which context menu to return</param>
    /// <returns>Returns the context menu</returns>
    ContextMenu? Provide(T data);
}

