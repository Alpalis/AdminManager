﻿using Alpalis.AdminManager.API;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Players.Movement.Events;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events
{
    public class UpdateFlyHeight : IEventListener<UnturnedPlayerStanceUpdatedEvent>
    {
        #region Member Variables
        private readonly IFlySystem m_FlySystem;
        #endregion Member Variables

        #region Class Constructor
        public UpdateFlyHeight(
            IFlySystem flySystem)
        {
            m_FlySystem = flySystem;
        }
        #endregion Class Constructor

        [EventListener(Priority = EventListenerPriority.Normal)]
        public async Task HandleEventAsync(object? sender, UnturnedPlayerStanceUpdatedEvent @event)
        {
            if (!m_FlySystem.IsInFlyMode(@event.Player.SteamId)) return;
            switch(@event.Player.Stance)
            {
                case "prone":
                    m_FlySystem.FlyDown(@event.Player.SteamPlayer);
                    break;
                case "stand":
                    m_FlySystem.FlyIdle(@event.Player.SteamPlayer);
                    break;
                case "crouch":
                    m_FlySystem.FlyUp(@event.Player.SteamPlayer);
                    break;
            }
        }
    }
}
