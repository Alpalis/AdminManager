using Alpalis.AdminManager.API;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Players.Life.Events;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events
{
    public class CancelDamage : IEventListener<UnturnedPlayerDamagingEvent>
    {
        private readonly IGodSystem m_GodSystem;

        public CancelDamage(
            IGodSystem godSystem)
        {
            m_GodSystem = godSystem;
        }

        [EventListener(Priority = EventListenerPriority.Normal)]
        public async Task HandleEventAsync(object? sender, UnturnedPlayerDamagingEvent @event)
        {
            if (!m_GodSystem.IsInGodMode(@event.Player.SteamId)) return;
            @event.IsCancelled = true;
        }
    }
}
