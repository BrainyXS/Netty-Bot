using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBot.DataAccess.Contract;
using DiscordBot.DataAccess.Contract.ReactionRoles;
using DiscordBot.Framework.Contract.Modularity;

namespace DiscordBot.Modules.ReactionRoles;

public class ReactionRoleCommands : CommandModuleBase, IGuildModule
{
    private readonly IModuleDataAccess _dataAccess;
    private readonly ReactionRoleManager _manager;
    private readonly IReactionRoleBusinessLogic _businessLogic;

    public ReactionRoleCommands(IModuleDataAccess dataAccess, ReactionRoleManager manager,
        IReactionRoleBusinessLogic businessLogic) : base(dataAccess)
    {
        _dataAccess = dataAccess;
        _manager = manager;
        _businessLogic = businessLogic;
    }

    public override string ModuleUniqueIdentifier => "REACTION_ROLE";

    public override async Task<bool> CanExecuteAsync(ulong id, SocketCommandContext socketCommandContext)
    {
        await RequirePermissionAsync(socketCommandContext, GuildPermission.Administrator);
        return await IsEnabled(id);
    }

    public override Task ExecuteAsync(ICommandContext context)
    {
        return ExecuteCommandsAsync(context);
    }

    [Command("registerReactionRole")]
    public async Task RegisterReactionRole(ICommandContext context)
    {
        var prefix = await _dataAccess.GetServerPrefixAsync(context.Guild.Id);
        await RequireArg(context, 3, $"Syntaxfehler: {prefix}registerReactionRole [Emote] [Rollen-Id] [Nachricht]");
        var emote = GetEmote(await RequireString(context));
        var roleId = await RequireUlongAsync(context, 2);

        var role = context.Guild.GetRole(roleId);
        if (role == null)
        {
            await context.Channel.SendMessageAsync($"Keine Rolle mit der ID '{roleId}' gefunden");
            return;
        }

        var content = await RequireReminderArg(context, 3);
        var message = await context.Channel.SendMessageAsync(content);
        await message.AddReactionAsync(emote);
        var reactionRole = new ReactionRole
        {
            Emote = emote,
            Id = 0,
            ChannelId = message.Channel.Id,
            GuildId = context.Guild.Id,
            MessageId = message.Id,
            RoleId = role.Id
        };
        _manager.ReactionRoles.Add(reactionRole);
        await _businessLogic.SaveReactionRoleAsync(reactionRole);
        await context.Message.DeleteAsync();
    }

    private IEmote GetEmote(string emote)
    {
        try
        {
            return Emote.Parse(emote);
        }
        catch (Exception)
        {
            return new Emoji(emote);
        }
    }
}