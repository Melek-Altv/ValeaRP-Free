using System;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Handler;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;

namespace Altv_Roleplay.Factions.DMV
{
    class Functions : IScript
    {

        [AsyncClientEvent("Server:Tablet:DMVAppSearchLicense")]
        public static void DMVAppSearchLicense(IPlayer player, string targetCharname)
        {
            try
            {
                if (player == null || !player.Exists || targetCharname == "") return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 4, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                if (!ServerFactions.IsCharacterInAnyFaction(charId)) { 
                    HUDHandler.SendNotification(player, 4, 5000, "Fehler: Du bist in keiner Fraktion."); 
                    return; 
                }
                if (ServerFactions.GetCharacterFactionId(charId) != 5 && ServerFactions.GetCharacterFactionId(charId) != 1) { 
                    HUDHandler.SendNotification(player, 4, 5000, "Du bist kein Fahrlehrer.");
                    return; 
                }
                if (!ServerFactions.IsCharacterInFactionDuty(charId)) { 
                    HUDHandler.SendNotification(player, 4, 5000, "Fehler: Du bist nicht im Dienst."); 
                    return; 
                }
                if (!Characters.ExistCharacterName(targetCharname)) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Fehler: Der eingegebene Name wurde nicht gefunden."); 
                    return; 
                }
                int targetCharId = Characters.GetCharacterIdFromCharName(targetCharname);
                if (targetCharId <= 0) return;
                string charName = Characters.GetCharacterName(targetCharId);
                string licArray = CharactersLicenses.GetCharacterLicenses(targetCharId);
                player.EmitLocked("Client:Tablet:SetLSPDAppLicenseSearchData", charName, licArray);
                HUDHandler.SendNotification(player, 2, 1500, $"Lizenzabfrage durchgeführt: {charName}.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
    }
}
