using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
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
        [Command("reputation")]
        [CommandAlias("rep")]
        [CommandDescription("Manages your or somebody's reputation.")]
        [CommandSyntax("<get/add/set/take>")]
        public class Root : UnturnedCommand
        {
            public Root(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }
        }

        [Command("add")]
        [CommandSyntax("<amount> [player]")]
        [CommandDescription("Adds your or somebody's reputation.")]
        [RegisterCommandPermission("other", Description = "Allows to add reputation of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(Root))]
        public class AddUnturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public AddUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["reputation_command:prefix"],
                         m_StringLocalizer["reputation_command:error_adminmode"]));
                if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["reputation_command:prefix"],
                        m_StringLocalizer["reputation_command:add:error_amount"]));
                if (Context.Parameters.Count == 1)
                {
                    await UniTask.SwitchToMainThread();
                    if (user.Player.Player.skills.reputation + amount > int.MaxValue)
                        throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["reputation_command:prefix"],
                         m_StringLocalizer["reputation_command:add:error_amount"]));
                    user.Player.Player.skills.askRep((int)amount);
                    PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["reputation_command:prefix"],
                        m_StringLocalizer["reputation_command:add:succeed:yourself",
                        new { Amount = amount, NewReputation = user.Player.Player.skills.reputation }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["reputation_command:prefix"],
                        m_StringLocalizer["reputation_command:error_player"]));
                await UniTask.SwitchToMainThread();
                if (targetUser.Player.Player.skills.reputation + amount > int.MaxValue)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["reputation_command:prefix"],
                     m_StringLocalizer["reputation_command:add:error_amount"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                targetSPlayer.player.skills.askRep((int)amount);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["reputation_command:prefix"],
                    m_StringLocalizer["reputation_command:add:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        Amount = amount,
                        NewReputation = targetSPlayer.player.skills.reputation
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["reputation_command:prefix"],
                    m_StringLocalizer["reputation_command:add:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        Amount = amount,
                        NewReputation = targetSPlayer.player.skills.reputation
                    }]));
            }
        }

        [Command("add")]
        [CommandSyntax("<amount> <player>")]
        [CommandDescription("Adds your or somebody's reputation.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(Root))]
        public class AddConsole : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;

            public AddConsole(
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:add:error_amount"]);
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:error_player"]);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                if (targetSPlayer.player.skills.reputation + amount > int.MaxValue)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:add:error_amount"]);
                targetSPlayer.player.skills.askRep((int)amount);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
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
                    Amount = amount,
                    NewReputation = targetSPlayer.player.skills.reputation
                }]);
            }
        }

        [Command("set")]
        [CommandSyntax("<amount> [player]")]
        [CommandDescription("Sets your or somebody's reputation.")]
        [RegisterCommandPermission("other", Description = "Allows to set reputation of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(Root))]
        public class SetUnturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public SetUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["reputation_command:prefix"],
                         m_StringLocalizer["reputation_command:error_adminmode"]));
                if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out int amount))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["reputation_command:prefix"],
                        m_StringLocalizer["reputation_command:set:error_amount"]));
                if (Context.Parameters.Count == 1)
                {
                    await UniTask.SwitchToMainThread();
                    user.Player.Player.skills.askRep(amount - user.Player.Player.skills.reputation);
                    PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["reputation_command:prefix"],
                        m_StringLocalizer["reputation_command:set:succeed:yourself",
                        new { Reputation = user.Player.Player.skills.reputation }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["reputation_command:prefix"],
                        m_StringLocalizer["reputation_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                user.Player.Player.skills.askRep(amount - user.Player.Player.skills.reputation);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["reputation_command:prefix"],
                    m_StringLocalizer["reputation_command:set:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        Reputation = targetSPlayer.player.skills.reputation
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["reputation_command:prefix"],
                    m_StringLocalizer["reputation_command:set:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        Reputation = targetSPlayer.player.skills.reputation
                    }]));
            }
        }

        [Command("set")]
        [CommandSyntax("<amount> <player>")]
        [CommandDescription("Sets your or somebody's reputation.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(Root))]
        public class SetConsole : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;

            public SetConsole(
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out int amount))
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:set:error_amount"]);
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:error_player"]);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                targetSPlayer.player.skills.modRep(amount);
                targetSPlayer.player.skills.askRep(0);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["reputation_command:prefix"],
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
                    Reputation = targetSPlayer.player.skills.reputation
                }]);
            }
        }

        [Command("take")]
        [CommandSyntax("<amount> [player]")]
        [CommandDescription("Takes your or somebody's reputation.")]
        [RegisterCommandPermission("other", Description = "Allows to take reputation of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(Root))]
        public class TakeUnturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public TakeUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["reputation_command:prefix"],
                         m_StringLocalizer["reputation_command:error_adminmode"]));
                if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["reputation_command:prefix"],
                        m_StringLocalizer["reputation_command:take:error_amount"]));
                if (Context.Parameters.Count == 1)
                {
                    await UniTask.SwitchToMainThread();
                    if ((int)user.Player.Player.skills.experience - amount < int.MinValue)
                        throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["reputation_command:prefix"],
                         m_StringLocalizer["reputation_command:take:error_amount"]));
                    user.Player.Player.skills.askRep(-(int)amount);
                    PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["reputation_command:prefix"],
                        m_StringLocalizer["reputation_command:take:succeed:yourself",
                        new { Amount = amount, NewReputation = user.Player.Player.skills.reputation }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["reputation_command:prefix"],
                        m_StringLocalizer["reputation_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                if ((int)targetSPlayer.player.skills.experience - amount < int.MinValue)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["reputation_command:prefix"],
                     m_StringLocalizer["reputation_command:take:error_amount"]));
                targetSPlayer.player.skills.askRep(-(int)amount);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["reputation_command:prefix"],
                    m_StringLocalizer["reputation_command:take:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        Amount = amount,
                        NewReputation = targetSPlayer.player.skills.reputation
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["reputation_command:prefix"],
                    m_StringLocalizer["reputation_command:take:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        Amount = amount,
                        NewReputation = targetSPlayer.player.skills.reputation
                    }]));
            }
        }

        [Command("take")]
        [CommandSyntax("<amount> <player>")]
        [CommandDescription("Takes your or somebody's reputation.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(Root))]
        public class TakeConsole : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;

            public TakeConsole(
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:take:error_amount"]);
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:error_player"]);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                if ((int)targetSPlayer.player.skills.experience - amount < int.MinValue)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:take:error_amount"]);
                targetSPlayer.player.skills.askRep(-(int)amount);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["reputation_command:prefix"],
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
                    Amount = amount,
                    NewReputation = targetSPlayer.player.skills.reputation
                }]);
            }
        }

        [Command("get")]
        [CommandSyntax("[player]")]
        [CommandDescription("Gets your or somebody's reputation.")]
        [RegisterCommandPermission("other", Description = "Allows to get reputation of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(Root))]
        public class GetUnturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public GetUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (Context.Parameters.Length == 0)
                {
                    PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["reputation_command:prefix"],
                        m_StringLocalizer["reputation_command:get:yourself", new { Reputation = user.Player.Player.skills.reputation }]));
                    return;
                }
                else if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["reputation_command:prefix"],
                         m_StringLocalizer["reputation_command:error_adminmode"]));
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["reputation_command:prefix"],
                        m_StringLocalizer["reputation_command:error_player"]));
                SteamPlayer sPlayer = targetUser.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                PrintAsync(string.Format("{0}{1}", m_StringLocalizer["reputation_command:prefix"],
                    m_StringLocalizer["reputation_command:get:somebody", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        Reputation = sPlayer.player.skills.reputation
                    }]));
            }
        }

        [Command("get")]
        [CommandSyntax("<player>")]
        [CommandDescription("Gets somebody's reputation.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(Root))]
        public class GetConsole : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;

            public GetConsole(
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(m_StringLocalizer["reputation_command:error_player"]);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                PrintAsync(m_StringLocalizer["reputation_command:get:somebody", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    Reputation = sPlayer.player.skills.reputation
                }]);
            }
        }
    }
}
