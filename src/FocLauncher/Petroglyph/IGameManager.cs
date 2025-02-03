using PG.StarWarsGame.Infrastructure.Clients;

namespace FocLauncher.Petroglyph;

internal interface IGameManager
{
    IGameClient GameClient { get; }
}