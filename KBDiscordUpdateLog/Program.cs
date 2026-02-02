// See https://aka.ms/new-console-template for more information

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace KBDiscordUpdateLog;

internal abstract class Program
{
    private static async Task Main()
    {
        if (!Directory.Exists("Config"))
        {
            Directory.CreateDirectory("Config");
        }
        Config.GenCfgJson();
        
        var kbCfg = await Config.GetCfg();;

        if (kbCfg?.Version is not PrimaryInfo.ConfigVersion)
        {
            kbCfg = await Config.Replace(kbCfg);
        }

        if (kbCfg is null or { Token: "" or null })
        {
            Console.WriteLine("Token or config is empty");
            await Task.Delay(-1);
            return;
        }
        
        var client = new DiscordClient(new DiscordConfiguration
        {
            Token = kbCfg.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
        });
        Console.WriteLine("Initializing DiscordClient");
        var slash = client.UseSlashCommands();
        // Commands
        slash.RegisterCommands<SlashCommands>();
        Console.WriteLine("Updating DiscordClient status");
        await client.ConnectAsync(new DiscordActivity()
        {
            Name = "/log",
        }, UserStatus.Online);
        
        await Task.Delay(-1);
        Console.WriteLine("Disconnecting DiscordClient");
    }
}