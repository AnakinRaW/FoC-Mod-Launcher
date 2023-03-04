using System;

namespace AnakinRaW.AppUpdaterFramework.Interaction;

public struct SupportedInteractionState : IEquatable<SupportedInteractionState>
{
    public InteractionStatus Status { get; }

    public string? Description { get; }

    public SupportedInteractionState(InteractionStatus status, string? description)
    {
        Description = description;
        Status = status;
    }
    
    public bool Equals(SupportedInteractionState other)
    {
        return Status == other.Status;
    }

    public override bool Equals(object? obj)
    {
        return obj is SupportedInteractionState other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (int)Status;
    }
}