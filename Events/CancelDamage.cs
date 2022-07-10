using Alpalis.AdminManager.API;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Players.Life.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events
{
    public class CancelDamage : IEventListener<UnturnedPlayerDamagingEvent>
    {
        #region Member Variables
        private readonly IGodSystem m_GodSystem;
        #endregion Member Variables

        #region Class Constructor
        public CancelDamage(
            IGodSystem godSystem)
        {
            m_GodSystem = godSystem;
        }
        #endregion Class Constructor

        [EventListener(Priority = EventListenerPriority.Normal)]
        public async Task HandleEventAsync(object? sender, UnturnedPlayerDamagingEvent @event)
        {
            if (!m_GodSystem.IsInGodMode(@event.Player.SteamId)) return;
            @event.IsCancelled = true;
        }
    }
}
