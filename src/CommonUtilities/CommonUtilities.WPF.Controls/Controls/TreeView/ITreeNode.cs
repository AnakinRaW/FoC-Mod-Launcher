using System.Collections;
using System.Collections.Generic;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public interface ITreeNode
{
    bool IsExpanded { get; set; }

    bool IsExpandable { get; }

    object? Item { get; }

    IEnumerable? ItemsSource { get; }

    bool AreChildNodesRealized { get; }

    bool IsVisible { get; }

    ITreeNode? Parent { get; }

    IEnumerable<ITreeNode> ChildNodes { get; }

    IEnumerable<ITreeNode> DescendantNodes { get; }

    void RealizeChildNodes();
}