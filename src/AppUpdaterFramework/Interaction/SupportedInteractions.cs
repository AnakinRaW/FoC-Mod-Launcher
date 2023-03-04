using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnakinRaW.AppUpdaterFramework.Interaction;

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