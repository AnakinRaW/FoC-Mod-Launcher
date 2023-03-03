using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnakinRaW.AppUpaterFramework.Interaction;

public enum InteractionStatus
{
    Ok,
    Cancel,
    Retry,
    Abort
}

public class SupportedInteractions : IEnumerable<SupportedInteractionState>
{
    private readonly Dictionary<InteractionStatus, SupportedInteractionState> _supportedInteractions;

    internal SupportedInteractions(IEnumerable<SupportedInteractionState> supportedInteractions)
    {
        _supportedInteractions = supportedInteractions.ToDictionary(state => state.Status, state => state);
    }

    internal SupportedInteractions(params SupportedInteractionState[] supportedInteractions)
    {
        _supportedInteractions = supportedInteractions.ToDictionary(state => state.Status, state => state);
    }

    public bool TryGet(InteractionStatus status, out SupportedInteractionState state)
    {
        return _supportedInteractions.TryGetValue(status, out state);
    }

    public bool Contains(InteractionStatus status)
    {
        return _supportedInteractions.ContainsKey(status);
    }

    public IEnumerator<SupportedInteractionState> GetEnumerator()
    {
        return _supportedInteractions.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

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