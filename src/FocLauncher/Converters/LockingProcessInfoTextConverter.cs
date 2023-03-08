using System.Globalization;
using AnakinRaW.AppUpdaterFramework.FileLocking.Interaction;
using AnakinRaW.CommonUtilities.Wpf.Converters;

namespace FocLauncher.Converters;

internal class LockingProcessInfoTextConverter : ValueConverter<ILockingProcess, string>
{
    protected override string? Convert(ILockingProcess value, object? parameter, CultureInfo culture)
    {
        return $"{value.ProcessName}.exe\t[{value.ProcessId}]";
    }
}