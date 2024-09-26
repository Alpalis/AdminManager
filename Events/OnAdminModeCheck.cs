using Alpalis.AdminManager.API;
using Alpalis.UtilityServices.API.Events;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using Steamworks;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events;

public sealed class OnAdminModeCheck(
    IAdminSystem adminSystem) : IEventListener<AdminModeEvent>
{
    private readonly IAdminSystem m_AdminSystem = adminSystem;

    [EventListener(Priority = EventListenerPriority.Normal)]
    public Task HandleEventAsync(object? sender, AdminModeEvent @event)
    {
        if (@event.Actor != null)
            @event.IsInAdminMode = m_AdminSystem.IsInAdminMode(@event.Actor!);
        if (@event.SteamID != null)
            @event.IsInAdminMode = m_AdminSystem.IsInAdminMode((CSteamID)@event.SteamID);
        return Task.CompletedTask;
    }
}
