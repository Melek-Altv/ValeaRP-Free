using System;
using Discord.Webhook;
using Discord.Webhook.HookRequest;

namespace Altv_Roleplay.Handler
{
    class DiscordLog
    {
        internal static void SendEmbed(string type, string nickname, string text)
        {
            DiscordWebhook hook = new DiscordWebhook();

            switch (type)
            {
                case "adminmenu":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "report":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "support":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "frakbank":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "bank":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "companybank":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "kofferraum":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "handschuhfach":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "hotelStorage":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "housestorage":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "factionstorage":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "ooc":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "death":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "Einreiselog":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "giveitem":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "atmrob":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                case "breakvehicle":
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
                default:
                    hook.HookUrl = "DEINDISCORDWEBHOOK";
                    break;
            }

            if (hook.HookUrl == "YOUR_WEBHOOK") return; //Hier YOUR_WEBHOOK nicht ersetzen

            DiscordHookBuilder builder = DiscordHookBuilder.Create(Nickname: nickname, AvatarUrl: "https://cdn.discordapp.com/attachments/804702429152804866/952001545732513882/sa-logo.jpg?width=519&height=519");

            DiscordEmbed embed = new DiscordEmbed(
                            Title: "Sa-Development - Logs",
                            Description: text,
                            Color: 0xf54242,
                            FooterText: "Sa-Development - Logs",
                            FooterIconUrl: "https://cdn.discordapp.com/attachments/804702429152804866/952001545732513882/sa-logo.jpg?width=519&height=519");
            builder.Embeds.Add(embed);

            DiscordHook HookMessage = builder.Build();
            hook.Hook(HookMessage);
        }
    }
}
