using Alpalis.AdminManager.API;
using Alpalis.UtilityServices.API.Events;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using Steamworks;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events
{
    public class OnAdminModeCheck : IEventListener<AdminModeEvent>
    {
        private readonly IAdminSystem m_AdminSystem;

        public OnAdminModeCheck(
            IAdminSystem adminSystem)
        {
            m_AdminSystem = adminSystem;
        }

        [EventListener(Priority = EventListenerPriority.Normal)]
        public async Task HandleEventAsync(object? sender, AdminModeEvent @event)
        {
            if (@event.Actor != null)
                @event.IsInAdminMode = m_AdminSystem.IsInAdminMode(@event.Actor!);
            if (@event.SteamID != null)
                @event.IsInAdminMode = m_AdminSystem.IsInAdminMode((CSteamID)@event.SteamID);
        }
    }
}
