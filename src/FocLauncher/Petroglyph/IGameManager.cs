using PetroGlyph.Games.EawFoc.Clients;

namespace FocLauncher.Petroglyph;

internal interface IGameManager
{
    IGameClient GameClient { get; }
}