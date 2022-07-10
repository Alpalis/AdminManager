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
    public class ReputationCommand
    {
        #region Command Parameters
        [Command("reputation")]
        [CommandAlias("rep")]
        [CommandDescription("Command to manage your or somebody's reputation.")]
        [CommandSyntax("<get/add/set/take>")]
        #endregion Command Parameters
        public class Reputation : UnturnedCommand
        {
            #region Class Constructor
            public Reputation(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }
        }

        #region Add
        #region Command Parameters
        [Command("add")]
        [CommandSyntax("<amount> [player]")]
        [CommandDescription("Command to add your or somebody's reputation.")]
        [RegisterCommandPermission("other", Description = "Allows to add reputation of other people.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(Reputation))]
        #endregion Command Parameters
        public class AddUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public AddUnturned(
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
                         config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                         m_StringLocalizer["reputation_command:error_adminmode"]));
                if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                        m_StringLocalizer["reputation_command:add:error_amount"]));
                if (Context.Parameters.Count == 1)
                {
                    await UniTask.SwitchToMainThread();
                    if (user.Player.Player.skills.reputation + amount > int.MaxValue)
                        throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                         m_StringLocalizer["reputation_command:add:error_amount"]));
                    user.Player.Player.skills.askRep((int)amount);
                    PrintAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                        m_StringLocalizer["reputation_command:add:succeed:yourself",
                        new { Amount = amount, NewReputation = user.Player.Player.skills.reputation }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                        m_StringLocalizer["reputation_command:error_player"]));
                await UniTask.SwitchToMainThread();
                if (targetUser.Player.Player.skills.reputation + amount > int.MaxValue)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                     m_StringLocalizer["reputation_command:add:error_amount"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                targetSPlayer.player.skills.askRep((int)amount);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                    m_StringLocalizer["reputation_command:add:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity,
                        Amount = amount,
                        NewReputation = targetSPlayer.player.skills.reputation
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                    m_StringLocalizer["reputation_command:add:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity,
                        Amount = amount,
                        NewReputation = targetSPlayer.player.skills.reputation
                    }]));
            }
        }

        #region Command Parameters
        [Command("add")]
        [CommandSyntax("<amount> <player>")]
        [CommandDescription("Command to add your or somebody's reputation.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(Reputation))]
        #endregion Command Parameters
        public class AddConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public AddConsole(
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
                if (Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:add:error_amount"]);
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:error_player"]);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                await UniTask.SwitchToMainThread();
                if (targetSPlayer.player.skills.reputation + amount > int.MaxValue)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:add:error_amount"]);
                targetSPlayer.player.skills.askRep((int)amount);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["experience_command:prefix"] : "",
                    m_StringLocalizer["reputation_command:add:succeed:somebody:console", new
                    {
                        Amount = amount,
                        NewReputation = targetSPlayer.player.skills.reputation
                    }]));
                PrintAsync(m_StringLocalizer["reputation_command:add:succeed:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    ID = targetIdentity,
                    Amount = amount,
                    NewReputation = targetSPlayer.player.skills.reputation
                }]);
            }
        }
        #endregion Add

        #region Set
        #region Command Parameters
        [Command("set")]
        [CommandSyntax("<amount> [player]")]
        [CommandDescription("Command to set your or somebody's reputation.")]
        [RegisterCommandPermission("other", Description = "Allows to set reputation of other people.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(Reputation))]
        #endregion Command Parameters
        public class SetUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public SetUnturned(
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
                         config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                         m_StringLocalizer["reputation_command:error_adminmode"]));
                if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out int amount))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                        m_StringLocalizer["reputation_command:set:error_amount"]));
                if (Context.Parameters.Count == 1)
                {
                    await UniTask.SwitchToMainThread();
                    user.Player.Player.skills.askRep(amount - user.Player.Player.skills.reputation);
                    PrintAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                        m_StringLocalizer["reputation_command:set:succeed:yourself",
                        new { Reputation = user.Player.Player.skills.reputation }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                        m_StringLocalizer["reputation_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                await UniTask.SwitchToMainThread();
                user.Player.Player.skills.askRep(amount - user.Player.Player.skills.reputation);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                    m_StringLocalizer["reputation_command:set:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity,
                        Reputation = targetSPlayer.player.skills.reputation
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                    m_StringLocalizer["reputation_command:set:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity,
                        Reputation = targetSPlayer.player.skills.reputation
                    }]));
            }
        }

        #region Command Parameters
        [Command("set")]
        [CommandSyntax("<amount> <player>")]
        [CommandDescription("Command to set your or somebody's reputation.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(Reputation))]
        #endregion Command Parameters
        public class SetConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public SetConsole(
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
                if (Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out int amount))
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:set:error_amount"]);
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:error_player"]);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                await UniTask.SwitchToMainThread();
                targetSPlayer.player.skills.modRep(amount);
                targetSPlayer.player.skills.askRep(0);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                    m_StringLocalizer["reputation_command:set:succeed:somebody:console", new
                    {
                        Reputation = targetSPlayer.player.skills.reputation
                    }]));
                PrintAsync(m_StringLocalizer["reputation_command:set:succeed:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    ID = targetIdentity,
                    Reputation = targetSPlayer.player.skills.reputation
                }]);
            }
        }
        #endregion Set

        #region Take
        #region Command Parameters
        [Command("take")]
        [CommandSyntax("<amount> [player]")]
        [CommandDescription("Command to take your or somebody's reputation.")]
        [RegisterCommandPermission("other", Description = "Allows to take reputation of other people.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(Reputation))]
        #endregion Command Parameters
        public class TakeUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public TakeUnturned(
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
                         config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                         m_StringLocalizer["reputation_command:error_adminmode"]));
                if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                        m_StringLocalizer["reputation_command:take:error_amount"]));
                if (Context.Parameters.Count == 1)
                {
                    await UniTask.SwitchToMainThread();
                    if ((int)user.Player.Player.skills.experience - amount < int.MinValue)
                        throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                         m_StringLocalizer["reputation_command:take:error_amount"]));
                    user.Player.Player.skills.askRep(-(int)amount);
                    PrintAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                        m_StringLocalizer["reputation_command:take:succeed:yourself",
                        new { Amount = amount, NewReputation = user.Player.Player.skills.reputation }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                        m_StringLocalizer["reputation_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                await UniTask.SwitchToMainThread();
                if ((int)targetSPlayer.player.skills.experience - amount < int.MinValue)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                     m_StringLocalizer["reputation_command:take:error_amount"]));
                targetSPlayer.player.skills.ServerModifyExperience(-(int)amount);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                    m_StringLocalizer["reputation_command:take:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity,
                        Amount = amount,
                        NewReputation = targetSPlayer.player.skills.reputation
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                    m_StringLocalizer["reputation_command:take:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity,
                        Amount = amount,
                        NewReputation = targetSPlayer.player.skills.reputation
                    }]));
            }
        }

        #region Command Parameters
        [Command("take")]
        [CommandSyntax("<amount> <player>")]
        [CommandDescription("Command to take your or somebody's reputation.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(Reputation))]
        #endregion Command Parameters
        public class TakeConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public TakeConsole(
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
                if (Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:take:error_amount"]);
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:error_player"]);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                await UniTask.SwitchToMainThread();
                if ((int)targetSPlayer.player.skills.experience - amount < int.MinValue)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:take:error_amount"]);
                targetSPlayer.player.skills.askRep(-(int)amount);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                    m_StringLocalizer["reputation_command:take:succeed:somebody:console", new
                    {
                        Amount = amount,
                        NewReputation = targetSPlayer.player.skills.reputation
                    }]));
                PrintAsync(m_StringLocalizer["reputation_command:take:succeed:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    ID = targetIdentity,
                    Amount = amount,
                    NewReputation = targetSPlayer.player.skills.reputation
                }]);
            }
        }
        #endregion Take  

        #region Get
        #region Command Parameters
        [Command("get")]
        [CommandSyntax("[player]")]
        [CommandDescription("Command to get your or somebody's reputation.")]
        [RegisterCommandPermission("other", Description = "Allows to get reputation of other people.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(Reputation))]
        #endregion Command Parameters
        public class GetUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public GetUnturned(
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
                if (Context.Parameters.Length == 0)
                {
                    PrintAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                        m_StringLocalizer["reputation_command:get:yourself", new { Reputation = user.Player.Player.skills.reputation }]));
                    return;
                }
                else if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                         m_StringLocalizer["reputation_command:error_adminmode"]));
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                        m_StringLocalizer["reputation_command:error_player"]));
                SteamPlayer sPlayer = targetUser.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                PrintAsync(string.Format("{0}{1}", config.MessagePrefix ? m_StringLocalizer["reputation_command:prefix"] : "",
                    m_StringLocalizer["reputation_command:get:somebody", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity,
                        Reputation = sPlayer.player.skills.reputation
                    }]));
            }
        }

        #region Command Parameters
        [Command("get")]
        [CommandSyntax("<player>")]
        [CommandDescription("Command to get somebody's reputation.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(Reputation))]
        #endregion Command Parameters
        public class GetConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            #endregion Member Variables

            #region Class Constructor
            public GetConsole(
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:error_player"]);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                PrintAsync(m_StringLocalizer["reputation_command:get:somebody", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    ID = identity,
                    Reputation = sPlayer.player.skills.reputation
                }]);
            }
        }
        #endregion Get
    }
}
