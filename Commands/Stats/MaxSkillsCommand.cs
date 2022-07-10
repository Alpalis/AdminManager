using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Core.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;

namespace Alpalis.AdminManager.Commands
{
    public class MaxSkillsCommand
    {
        #region Command Parameters
        [Command("maxskills")]
        [CommandSyntax("[player]")]
        [CommandDescription("Command to max your or somebody's skills.")]
        [RegisterCommandPermission("other", Description = "Allows to max skills of other people.")]
        [CommandActor(typeof(UnturnedUser))]
        #endregion Command Parameters
        public class MaxSkillsUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public MaxSkillsUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["maxskills_command:prefix"] : "",
                         m_StringLocalizer["maxskills_command:error_adminmode"]));
                if (Context.Parameters.Count == 0)
                {
                    await UniTask.SwitchToMainThread();
                    user.Player.Player.skills.ServerUnlockAllSkills();
                    PrintAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["maxskills_command:prefix"] : "",
                        m_StringLocalizer["maxskills_command:succeed:yourself"]));
                    return;
                }
                else if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["maxskills_command:prefix"] : "",
                        m_StringLocalizer["maxskills_command:error_player"]));
                await UniTask.SwitchToMainThread();
                targetUser.Player.Player.skills.ServerUnlockAllSkills();
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["maxskills_command:prefix"] : "",
                    m_StringLocalizer["maxskills_command:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity,
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["maxskills_command:prefix"] : "",
                    m_StringLocalizer["maxskills_command:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity,
                    }]));
            }

            #region Command Parameters
            [Command("maxskills")]
            [CommandSyntax("<player>")]
            [CommandDescription("Command to max somebody's skills.")]
            [CommandActor(typeof(ConsoleActor))]
            #endregion Command Parameters
            public class MaxSkillsConsole : UnturnedCommand
            {
                #region Member Variables
                private readonly IStringLocalizer m_StringLocalizer;
                private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
                private readonly IConfigurationManager m_ConfigurationManager;
                private readonly Main m_Plugin;
                #endregion Member Variables

                #region Class Constructor
                public MaxSkillsConsole(
                    IStringLocalizer stringLocalizer,
                    IIdentityManagerImplementation identityManagerImplementation,
                    IConfigurationManager configurationManager,
                    IPluginAccessor<Main> plugin,
                    IServiceProvider serviceProvider) : base(serviceProvider)
                {
                    m_StringLocalizer = stringLocalizer;
                    m_IdentityManagerImplementation = identityManagerImplementation;
                    m_ConfigurationManager = configurationManager;
                    m_Plugin = plugin.Instance!;
                }
                #endregion Class Constructor


                protected override async UniTask OnExecuteAsync()
                {
                    Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                    if (Context.Parameters.Count != 1)
                        throw new CommandWrongUsageException(Context);
                    if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                        throw new UserFriendlyException(m_StringLocalizer["maxskills_command:error_player"]);
                    await UniTask.SwitchToMainThread();
                    targetUser.Player.Player.skills.ServerUnlockAllSkills();
                    SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                    CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                    ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                    targetUser.PrintMessageAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["maxskills_command:prefix"] : "",
                        m_StringLocalizer["maxskills_command:succeed:somebody:console"]));
                    PrintAsync(m_StringLocalizer["maxskills_command:succeed:somebody:executor", new
                        {
                            PlayerName = targetSPlayer.playerID.playerName,
                            CharacterName = targetSPlayer.playerID.characterName,
                            NickName = targetSPlayer.playerID.nickName,
                            SteamID = targetSteamID,
                            ID = targetIdentity,
                        }]);
                }
            }
        }
    }
}
