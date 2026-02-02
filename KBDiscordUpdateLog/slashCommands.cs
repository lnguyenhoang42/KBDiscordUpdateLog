using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace KBDiscordUpdateLog;

public class SlashCommands : ApplicationCommandModule
{
    private static async Task<bool> IsUserOperator(InteractionContext ctx)
    {
        var kbCfg = await Config.GetCfg();
        var matchOperator = kbCfg?.OperatorIds?.First(s => s.Contains(ctx.User.Id.ToString()));
        if (matchOperator is null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are not an operator.").AsEphemeral());
        }
        return matchOperator is not null;
    }
    //Commands
    [SlashCommand("log", "Compile text message in log channel to update channel")]
    public static async Task StatusCommand(InteractionContext ctx,
        [Option("name", "Name of the log")] string logTitle)
    {
        if (!await IsUserOperator(ctx)) return;

        var channelLoggedReturn = "";
        var kbCfg = await Config.GetCfg();
        var index = 0;
        if (kbCfg?.LogChannel != null && ctx.Client != null)
            foreach (var compiledData in kbCfg.LogChannel)
            {
                var logChannelId = compiledData["ChannelLog"];
                var publishChannelId = compiledData["ChannelPublish"];
                var recentTimeStamp = long.Parse(compiledData["RecentTimeStamp"]);
                if (logChannelId is "" || publishChannelId is "") continue;
                var logChannel = await ctx.Client.GetChannelAsync(ulong.Parse(logChannelId));
                var publishChannel = await ctx.Client.GetChannelAsync(ulong.Parse(publishChannelId));
                if (logChannel is null || publishChannel is null) continue;
                var compiledPublishInfo = new CompiledPublishInfo(publishChannel, [], ctx.User);
                var lastMessages = await logChannel.GetMessagesAsync(kbCfg.MessageLogLimit);
                var timeSortedMessages = lastMessages.ToList();
                timeSortedMessages.Sort((x, y) => x.Timestamp.CompareTo(y.Timestamp));
                foreach (var discordMessage in timeSortedMessages)
                {
                    if (discordMessage.Timestamp.UtcDateTime.ToFileTimeUtc() < recentTimeStamp) continue;
                    var cUserMessage =
                        compiledPublishInfo.CompiledUserMessage.FirstOrDefault(compiledUserMessage => compiledUserMessage.User.Id == discordMessage.Author.Id);
                    if (cUserMessage == null)
                    {
                        cUserMessage = new CompiledUserMessage(discordMessage.Author, [discordMessage.Content]);
                        compiledPublishInfo.CompiledUserMessage.Add(cUserMessage);
                    }
                    else
                    {
                        cUserMessage.Messages.Add(discordMessage.Content);
                    }
                    await discordMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,":thumbsup:"));
                }
                if (compiledPublishInfo.CompiledUserMessage.Count < 1) continue;
                channelLoggedReturn += $"\n<#{logChannelId}> -> <#{publishChannelId}>";
                await compiledPublishInfo.PublishLog(logTitle);
                kbCfg.LogChannel[index]["RecentTimeStamp"] = Config.GetTimestamp().ToString();
                kbCfg.UpdateConfig();
                index++;
            }
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Update log from channel(s):{channelLoggedReturn}").AsEphemeral());
    }
}