## Setting up config
Make sure to get [.Net 10](https://aka.ms/dotnet-core-applaunch?framework=Microsoft.NETCore.App&framework_version=10.0.0&arch=x64&rid=win-x64&os=win10)

First is to extract the zip into a folder, go into the folder and copy it path/address as text

Open up Command Prompt and type 'cd ' and then Ctrl + V to paste the address and press Enter

Simply type in 'KBDiscordUpdateLog.exe' and then press Enter to run it

A 'Config' folder should appear as the prompt saying 'Token or config is empty'

Open up 'config.json' in any text editor:
 - `Token` is the bot token you need to input from discord developer portal of the chosen bot(might be too lengthy to explain this process).
 - `ChannelLog` the channel where text message will be recorded and to be publish.
 - `ChannelPublish` the channel where the publish will happen.
 - `MessageLogLimit` how many messages should the bot check starting from most recent message
```json
{
  "ChannelLog": "",
  "ChannelPublish": "",
  "RecentTimeStamp": "0"
},
```
More log channels can be added by simply adding more of the above into the LogChannel, Reminder to have a ',' at the end of every array.

## Message logging
Any new message since bot first start up will be logged

![img.png](img.png)

use /log to log messages into an embed

![img1.png](img1.png)

Note will be publish like below

![img2.png](img2.png)