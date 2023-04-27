using Alpalis.AdminManager.API;
using Alpalis.UtilityServices.Helpers;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alpalis.AdminManager.Commands.Moderation
{
    [Command("ban")]
    [CommandSyntax("<player/player's steamID> [reason] [time]")]
    [CommandDescription("Allows to ban players.")]
    public class BanCommand : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IAdminSystem m_AdminSystem;

        public BanCommand(
            IStringLocalizer stringLocalizer,
            IAdminSystem adminSystem,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_AdminSystem = adminSystem;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1 || Context.Parameters.Length > 3)
                throw new CommandWrongUsageException(Context);
            if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["ban_command:prefix"],
                    m_StringLocalizer["ban_command:error_adminmode"]));
            ulong steamID = 0;
            if ((!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null) && !Context.Parameters.TryGet(0, out steamID))
                throw new UserFriendlyException(string.Format("{0}{1}",
                    Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["ban_command:prefix"] : "",
                    m_StringLocalizer["ban_command:error_player"]));
            CSteamID formatedSteamID = new(steamID);
            if (targetUser == null && !formatedSteamID.IsValid())
                throw new UserFriendlyException(string.Format("{0}{1}",
                    Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["ban_command:prefix"] : "",
                    m_StringLocalizer["ban_command:error_steamid"]));
            string reason;
            if (!Context.Parameters.TryGet(1, out reason!))
            {
                if (Context.Actor is UnturnedUser user)
                {
                    SteamPlayer sPlayer = user.Player.SteamPlayer;
                    reason = m_StringLocalizer["ban_command:default_reason:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = user.SteamId
                    }];
                }
                else
                    reason = m_StringLocalizer["ban_command:default_reason:console"];
            }
            if (!Context.Parameters.TryGet(2, out uint duration))
                duration = uint.MaxValue;
            await UniTask.SwitchToMainThread();
            if (targetUser == null)
            {
                Provider.requestBanPlayer(Context.Actor is UnturnedUser user1 ? user1.SteamId : CSteamID.Nil, formatedSteamID, 0, null, reason, duration);
                PrintAsync(string.Format("{0}{1}",
                    Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["ban_command:prefix"] : "",
                    m_StringLocalizer["ban_command:succeed:steamid", new
                    {
                        SteamID = formatedSteamID
                    }]));
                return;
            }
            uint ip = targetUser.Player.SteamPlayer.getIPv4AddressOrZero();
            IEnumerable<byte[]> hwids = targetUser.Player.SteamPlayer.playerID.GetHwids();
            Provider.requestBanPlayer(Context.Actor is UnturnedUser user2 ? user2.SteamId : CSteamID.Nil, targetUser.SteamId, ip, hwids, reason, duration);
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            PrintAsync(string.Format("{0}{1}",
                Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["ban_command:prefix"] : "",
                m_StringLocalizer["ban_command:succeed:player", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetUser.SteamId,
                    IP = IPAddressHelper.GetIPAddressFromUInt(ip),
                    HWIDs = string.Join(", ", hwids.ToList().Select(x => Hash.toString(x)))
                }]));
        }
    }
}
