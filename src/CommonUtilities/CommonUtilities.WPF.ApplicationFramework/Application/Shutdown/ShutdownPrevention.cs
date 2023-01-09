namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;

public sealed class ShutdownPrevention
{
    public string ReasonId { get; }

    public ShutdownPrevention(string reasonId)
    {
        ReasonId = reasonId;
    }
}