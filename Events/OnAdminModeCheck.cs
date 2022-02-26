using Alpalis.AdminManager.API;
using Alpalis.UtilityServices.Events;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using Steamworks;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events
{
    public class OnAdminModeCheck : IEventListener<AdminModeEvent>
    {
        #region Member Variables
        private readonly IAdminSystem m_AdminSystem;
        #endregion Member Variables

        #region Class Constructor
        public OnAdminModeCheck(
            IAdminSystem adminSystem)
        {
            m_AdminSystem = adminSystem;
        }
        #endregion Class Constructor

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
