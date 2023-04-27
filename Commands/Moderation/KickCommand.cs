using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands
{
    [Command("kick")]
    [CommandSyntax("<player> [reason]")]
    public class KickCommand : UnturnedCommand
    {
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;

        public KickCommand(
            IAdminSystem adminSystem,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_AdminSystem = adminSystem;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 2 || Context.Parameters.Length < 1)
                throw new CommandWrongUsageException(Context);
            if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["kick_command:prefix"],
                    m_StringLocalizer["kick_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["kick_command:prefix"] : "",
                    m_StringLocalizer["kick_command:error_player"]));
            string reason;
            if (!Context.Parameters.TryGet(1, out reason!))
            {
                if (Context.Actor is UnturnedUser user)
                {
                    SteamPlayer sPlayer = user.Player.SteamPlayer;
                    reason = m_StringLocalizer["kick_command:default_reason:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = user.SteamId
                    }];
                }
                else
                    reason = m_StringLocalizer["kick_command:default_reason:console"];
            }
            await UniTask.SwitchToMainThread();
            Provider.kick(targetUser.SteamId, reason);

            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            PrintAsync(string.Format("{0}{1}",
                Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["kick_command:prefix"] : "",
                m_StringLocalizer["kick_command:succeed",new {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetUser.SteamId
                }]));
        }
    }
}
