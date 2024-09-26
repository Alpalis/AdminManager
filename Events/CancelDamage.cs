using Alpalis.AdminManager.API;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Players.Life.Events;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events;

public sealed class CancelDamage(
    IGodSystem godSystem) : IEventListener<UnturnedPlayerDamagingEvent>
{
    private readonly IGodSystem m_GodSystem = godSystem;

    [EventListener(Priority = EventListenerPriority.Normal)]
    public Task HandleEventAsync(object? sender, UnturnedPlayerDamagingEvent @event)
    {
        if (!m_GodSystem.IsInGodMode(@event.Player.SteamId)) return Task.CompletedTask;
        @event.IsCancelled = true;
        return Task.CompletedTask;
    }
}
