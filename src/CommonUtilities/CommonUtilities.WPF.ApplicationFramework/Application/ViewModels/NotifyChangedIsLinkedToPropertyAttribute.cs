using System;
using System.Collections.Generic;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

[AttributeUsage(AttributeTargets.Property)]
public class NotifyChangedIsLinkedToPropertyAttribute : Attribute
{
    public ICollection<string> LinkedProperties { get; }

    public NotifyChangedIsLinkedToPropertyAttribute(params string[] linkedProperties)
    {
        LinkedProperties = linkedProperties;
    }
}