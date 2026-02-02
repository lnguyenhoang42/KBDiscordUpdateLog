using System.Text;
using System.Text.Json;
using DSharpPlus.Entities;

namespace KBDiscordUpdateLog;

public class CompiledUserMessage(DiscordUser user, List<string> messages)
{
    public DiscordUser User { get; init; } = user;
    public List<string> Messages { get; set; } = messages;

    public string CompileMessages()
    {
        return Messages.Aggregate("", (current, message) => current + $"\n- {message}");
    }
}
public class CompiledPublishInfo(DiscordChannel publishChannel, List<CompiledUserMessage> cUserMessage, DiscordUser? userAuthor)
{
    private DiscordChannel PublishChannel { get; init; } = publishChannel;
    public List<CompiledUserMessage> CompiledUserMessage { get; set; } = cUserMessage;

    public async Task PublishLog(string logName)
    {
        Console.WriteLine($"Publishing log {logName}");
        var compiledEmbed = new DiscordEmbedBuilder()
            .WithAuthor(userAuthor?.Username,null,userAuthor?.AvatarUrl)
            .WithColor(userAuthor?.BannerColor ?? DiscordColor.Black)
            .WithTitle(logName)
            .WithTimestamp(DateTime.UtcNow);
        foreach (var compiledUserMessage in CompiledUserMessage)
        {
            compiledEmbed.AddField(compiledUserMessage.User.Username, compiledUserMessage.CompileMessages());
        }
        await PublishChannel.SendMessageAsync(compiledEmbed);
    }
}
public class Config
{
    public string? Version { get; init; }
    /// <summary>
    ///     Bot token
    /// </summary>
    public string? Token { get; init; }
    /// <summary>
    ///     User's ID as operators
    /// </summary>
    public string[]? OperatorIds { get; init; }
    /// <summary>
    ///     How many past messages can the bot get starting from recent message
    /// </summary>
    public int MessageLogLimit { get; init; }
    /// <summary>
    ///     Channel to log text message and compile
    /// </summary>
    public Dictionary<string, string>[]? LogChannel { get; init; }

    public void UpdateConfig()
    {
        SerializeToCfg(this);
    }

    private static JsonSerializerOptions _defaultSerializer = new()
    {
        WriteIndented = true
    };
    public static long GetTimestamp()
    {
        return DateTime.UtcNow.ToFileTimeUtc();
    }
    public static Config Gen()
    {
        var kbConfig = new Config
        {
            Version = PrimaryInfo.ConfigVersion,
            Token = "",
            OperatorIds = [],
            MessageLogLimit = 100,
            LogChannel = [
                new Dictionary<string, string> {
                ["ChannelLog"] = "",
                ["ChannelPublish"] = "",
                ["RecentTimeStamp"] = GetTimestamp().ToString()
            }
            ]
        };

        return kbConfig;
    }
    public static async Task<Config?> GetCfg(string path = "Config\\config.json")
    {
        return JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(path));
    }

    public static void SerializeToCfg(Config config, string path = "Config\\config.json")
    {
        var options = _defaultSerializer;
        var configJson = File.Open(path, FileMode.OpenOrCreate);
        var jsonString = JsonSerializer.Serialize(config, options);
        configJson.Write(Encoding.UTF8.GetBytes(jsonString));
        configJson.Close();
    }

    // Generate config.json
    public static void GenCfgJson(string path = "Config\\config.json")
    {
        if (File.Exists(path)) return;
        var options = _defaultSerializer;
        var configJson = File.Create(path);
        var newGenCfg = Config.Gen();
        var jsonString = JsonSerializer.Serialize(newGenCfg, options);
        configJson.Write(Encoding.UTF8.GetBytes(jsonString));
        configJson.Close();
    }
    
    // Replace old with new config.json
    public static async Task<Config?> Replace(Config? kbCfg)
    {
        Console.WriteLine(
            $"Old config: {kbCfg?.Version} replaced with new config: {PrimaryInfo.ConfigVersion}");
        File.Move("Config\\config.json", $"Config\\{kbCfg?.Version}-config.json");
        GenCfgJson();
        return await GetCfg();
    }
}