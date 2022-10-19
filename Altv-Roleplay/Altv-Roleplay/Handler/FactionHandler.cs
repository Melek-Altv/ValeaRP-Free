using System;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Model;
using Altv_Roleplay.Services;
using Altv_Roleplay.Utils;

namespace Altv_Roleplay.Handler
{
    class FactionHandler : IScript
    {
        [AsyncClientEvent("Server:FactionStorage:StorageItem")]
        public static void FactionStorageStorageItem(IPlayer player, int factionId, int charId, string itemName, int amount, string fromContainer)
        {
            try
            {
                if (player == null || !player.Exists || factionId <= 0 || charId <= 0 || itemName == "" || itemName == "undefined" || amount <= 0 || fromContainer == "none" || fromContainer == "") return;
                int cCharId = User.GetPlayerOnline(player);
                if (cCharId != charId) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                if (!ServerFactions.IsCharacterInAnyFaction(charId)) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du bist in keiner Fraktion.");
                    return; 
                }
                int cFactionId = ServerFactions.GetCharacterFactionId(charId);
                if (cFactionId != factionId) { 
                    return; 
                }
                if (!ServerFactions.IsCharacterInFactionDuty(charId)) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du bist nicht im Dienst."); 
                    return; 
                }
                if (!CharactersInventory.ExistCharacterItem(charId, itemName, fromContainer)) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Diesen Gegenstand besitzt du nicht."); 
                    return; 
                }
                if (CharactersInventory.GetCharacterItemAmount(charId, itemName, fromContainer) < amount) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du hast nicht genügend Gegenstände davon dabei."); 
                    return; 
                }
                if (CharactersInventory.IsItemActive(player, itemName)) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Ausgerüstete Gegenstände können nicht umgelagert werden."); 
                    return; 
                }
                CharactersInventory.RemoveCharacterItemAmount(charId, itemName, amount, fromContainer);
                ServerFactions.AddServerFactionStorageItem(factionId, charId, itemName, amount);
                LoggingService.NewFactionLog(factionId, charId, 0, "storage", $"{ Characters.GetCharacterName(charId)} ({charId}) hat den Gegenstand '{itemName} ({amount}x)' in seinen Spind gelegt.");
                DiscordLog.SendEmbed("factionstorage", "FactionStorage Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {amount}x {itemName}  in seinen Spind gelegt.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:FactionStorage:TakeItem")]
        public static void FactionStorageTakeItem(IPlayer player, int factionId, int charId, string itemName, int amount)
        {
            try
            {
                if (player == null || !player.Exists || factionId <= 0 || charId <= 0 || amount <= 0 || itemName == "" || itemName == "undefined") return;
                int cCharId = User.GetPlayerOnline(player);
                if (cCharId != charId) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                if (!ServerFactions.IsCharacterInAnyFaction(charId)) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du bist in keiner Fraktion."); 
                    return; 
                }
                int cFactionId = ServerFactions.GetCharacterFactionId(charId);
                if (cFactionId != factionId) return;
                if (!ServerFactions.IsCharacterInFactionDuty(charId)) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du bist nicht im Dienst."); 
                    return; 
                }
                if (!ServerFactions.ExistServerFactionStorageItem(factionId, charId, itemName)) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Der Gegenstand existiert im Spind nicht."); 
                    return; 
                }
                if (ServerFactions.GetServerFactionStorageItemAmount(factionId, charId, itemName) < amount) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Soviele Gegenstände sind nicht im Spind."); 
                    return; 
                }
                float itemWeight = ServerItems.GetItemWeight(itemName) * amount;
                float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack"); 
                if (invWeight + itemWeight > 15f && backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId))) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast nicht genug Platz in deinen Taschen."); return; }
                ServerFactions.RemoveServerFactionStorageItemAmount(factionId, charId, itemName, amount);

                LoggingService.NewFactionLog(factionId, charId, 0, "storage", $"{Characters.GetCharacterName(charId)} ({charId}) hat den Gegenstand '{itemName} ({amount}x)' aus seinem Spind entnommen.");
                DiscordLog.SendEmbed("factionstorage", "FactionStorage Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {amount}x {itemName}  aus seinem Spind entnommen.");

                if (itemName.Contains("Fahrzeugschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemName} aus deinem Spind genommen.");
                    CharactersInventory.AddCharacterItem(charId, itemName, amount, "schluessel");
                    return;
                }
                if (itemName.Contains("Handschellenschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemName} aus deinem Spind genommen.");
                    CharactersInventory.AddCharacterItem(charId, itemName, amount, "schluessel");
                    return;
                }
                if (itemName.Contains("Generalschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemName} aus deinem Spind genommen.");
                    CharactersInventory.AddCharacterItem(charId, itemName, amount, "schluessel");
                    return;
                }
                if (itemName.Contains("Hausschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemName} aus deinem Spind genommen.");
                    CharactersInventory.AddCharacterItem(charId, itemName, amount, "schluessel");
                    return;
                }
                if (itemName.Contains("Bargeld"))
                {

                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemName} aus deinem Spind genommen.");
                    CharactersInventory.AddCharacterItem(charId, itemName, amount, "brieftasche");
                    return;
                }

                if (invWeight + itemWeight <= 15f)
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemName} aus deinem Spind genommen.");
                    CharactersInventory.AddCharacterItem(charId, itemName, amount, "inventory");
                    return;
                }

                if (Characters.GetCharacterBackpack(charId) != "None" && backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemName} aus deinem Spind genommen.");
                    CharactersInventory.AddCharacterItem(charId, itemName, amount, "backpack");
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
    }
}
