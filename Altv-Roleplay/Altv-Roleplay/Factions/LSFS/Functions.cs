﻿using System;
using System.Globalization;
using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Handler;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;

namespace Altv_Roleplay.Factions.LSFS
{
    class Functions : IScript
    {
        [AsyncClientEvent("Server:GivePlayerLicense:GiveLicense")]
        public static void GiveLicense(IPlayer player, int targetCharId, string licShort)
        {
            try
            {
                if (player == null || !player.Exists || targetCharId <= 0 || licShort == "") return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) {
                    HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das gefesselt machen?"); 
                    return;
                }
                if (!ServerFactions.IsCharacterInAnyFaction(charId)) {
                    HUDHandler.SendNotification(player, 3, 5000, "Du bist in keiner Fraktion."); 
                    return;
                }
                if (!ServerFactions.IsCharacterInFactionDuty(charId)) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Du bist nicht im Dienst.");
                    return;
                }
                if (ServerFactions.GetCharacterFactionId(charId) != 5) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Du bist kein Angehöriger der Fahrschule.");
                    return; 
                }
                var targetPlayer = Alt.GetAllPlayers().ToList().FirstOrDefault(x => x.GetCharacterMetaId() == (ulong)targetCharId);
                if (targetPlayer == null || !targetPlayer.Exists) return;
                if (targetCharId != (int)targetPlayer.GetCharacterMetaId()) { 
                    return; 
                }
                if (!player.Position.IsInRange(targetPlayer.Position, 5f)) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Du bist zu weit entfernt."); 
                    return;
                }
                if (!CharactersLicenses.ExistServerLicense(licShort)) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Ein unerwarteter Fehler ist aufgetreten.");
                    return; 
                }
                if (CharactersLicenses.HasCharacterLicense(targetCharId, licShort)) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Der Spieler hat diese Lizenz bereits.");
                    return;
                }
                if (!CharactersBank.HasCharacterBankMainKonto(targetCharId)) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Der Spieler besitzt kein Hauptkonto."); 
                    return; 
                }
                int accNumber = CharactersBank.GetCharacterBankMainKonto(targetCharId);
                int licPrice = CharactersLicenses.GetLicensePrice(licShort);
                if (CharactersBank.GetBankAccountLockStatus(accNumber)) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Das Hauptkonto des Spielers ist gesperrt."); 
                    return; 
                }
                CharactersBank.SetBankAccountMoney(accNumber, CharactersBank.GetBankAccountMoney(accNumber) - licPrice);
                ServerBankPapers.CreateNewBankPaper(accNumber, DateTime.Now.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")), DateTime.Now.ToString("t", CultureInfo.CreateSpecificCulture("de-DE")), "Ausgehende Überweisung", "Fahrschule", $"Lizenzkauf: {CharactersLicenses.GetFullLicenseName(licShort)}", $"-{licPrice}$", "Bankeinzug");
                CharactersLicenses.SetCharacterLicense(targetCharId, licShort, true);
                Characters.AddCharacterPermission(targetCharId, licShort);
                ServerFactions.SetFactionBankMoney(5, ServerFactions.GetFactionBankMoney(5) + licPrice);
                HUDHandler.SendNotification(player, 2, 2000, $"Sie haben dem Spieler {Characters.GetCharacterName(targetCharId)} die Lizenz '{CharactersLicenses.GetFullLicenseName(licShort)}' für eine Gebühr i.H.v. {licPrice}$ ausgestellt.");
                HUDHandler.SendNotification(targetPlayer, 2, 2000, $"Ihnen wurde die Lizenz '{CharactersLicenses.GetFullLicenseName(licShort)}' für eine Gebühr i.H.v. {licPrice}$ ausgestellt, diese wurde von Ihrem Hauptkonto abgebucht.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
    }
}
