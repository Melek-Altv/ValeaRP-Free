using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Services;
using System;
using System.Globalization;

namespace Altv_Roleplay.Handler
{
    class BankHandler : IScript
    {
        [AsyncClientEvent("Server:Bank:CreateNewBankAccount")]
        public static void CreateNewBankAccount(IPlayer player, string zoneName)
        {
            if (player == null || !player.Exists || zoneName == "") return;
            if (CharactersBank.GetCharacterBankAccountCount(player) >= 2) { HUDHandler.SendNotification(player, 4, 7000, "Du kannst nur zwei Bankkonten gleichzeitig haben."); 
                return; 
            }
            int rndAccNumber = new Random().Next(1000000, 999999999);
            int rndPin = new Random().Next(0000, 9999);
            int charid = User.GetPlayerOnline(player);
            if (charid == 0) return;
            if(CharactersBank.ExistBankAccountNumber(rndAccNumber)) { HUDHandler.SendNotification(player, 3, 7000, "Es ist ein Fehler aufgetreten. Bitte versuchen Sie es erneut."); 
                return; 
            }
            CharactersBank.CreateBankAccount(charid, rndAccNumber, rndPin, zoneName);
            HUDHandler.SendNotification(player, 2, 12000, $"Sie haben erfolgreich ein Konto erstellt. Ihre Kontonummer lautet ({rndAccNumber}) - ihr PIN: ({rndPin}).");
            CharactersInventory.AddCharacterItem(charid, "EC Karte " + rndAccNumber, 1, "brieftasche");
            if(!CharactersTablet.HasCharacterTutorialEntryFinished(charid, "createBankAccount"))
            {
                CharactersTablet.SetCharacterTutorialEntryState(charid, "createBankAccount", true);
                HUDHandler.SendNotification(player, 1, 4500, "Erfolg freigeschaltet: Dein erstes Konto");
            }
        }

        [AsyncClientEvent("Server:Bank:BankAccountAction")]
        public static void BankAccountAction(IPlayer player, string action, string accountNumberStr)
        {
            if (player == null || !player.Exists || action == "" || accountNumberStr == "") return;
            int accountNumber = Int32.Parse(accountNumberStr);
            int charid = User.GetPlayerOnline(player);
            if (accountNumber == 0 || charid == 0) return;
            if (action == "generatepin")
            {
                int rndPin = new Random().Next(0000, 9999);
                CharactersBank.ChangeBankAccountPIN(accountNumber, rndPin);
                HUDHandler.SendNotification(player, 2, 7000, $"Sie haben Ihren PIN erfolgreich geändert, neuer PIN: {rndPin}.");
            } else if(action == "lock")
            {
                CharactersBank.ChangeBankAccountLockStatus(accountNumber);
                if(CharactersBank.GetBankAccountLockStatus(accountNumber)) { 
                    HUDHandler.SendNotification(player, 2, 7000, $"Sie haben Ihr Konto mit der Kontonummer ({accountNumber}) erfolgreich gesperrt."); 
                }
                else { HUDHandler.SendNotification(player, 2, 7000, $"Sie haben Ihr Konto mit der Kontonummre ({accountNumber}) erfolgreich entsperrt."); 
                }
                CharactersBank.ResetBankAccountPINTrys(accountNumber);
            } else if(action == "setMain")
            {
                if(accountNumber == CharactersBank.GetCharacterBankMainKonto(charid)) { HUDHandler.SendNotification(player, 3, 7000, "Dieses Konto ist bereits dein Hauptkonto."); return; }
                if (CharactersBank.GetCharacterBankMainKonto(charid) != 0) CharactersBank.SetCharacterBankMainKonto(CharactersBank.GetCharacterBankMainKonto(charid));
                CharactersBank.SetCharacterBankMainKonto(accountNumber);
                HUDHandler.SendNotification(player, 2, 5000, $"Sie haben das Konto mit der Kontonummer ({accountNumber}) als Hauptkonto gesetzt.");
            } else if(action == "copycard")
            {
                if(CharactersBank.GetBankAccountLockStatus(accountNumber)) { HUDHandler.SendNotification(player, 3, 7000, "Ihr Konto ist gesperrt, entsperren Sie Ihr Konto bevor Sie eine neue Karte erhalten können."); return; }
                if(CharactersInventory.GetCharacterItemAmount(charid, "EC Karte " + accountNumber, "brieftasche") >= 3) { 
                    HUDHandler.SendNotification(player, 4, 5000, "Sie haben bereits zu viele Karten beantragt."); 
                    return; 
                }
                CharactersInventory.AddCharacterItem(charid, "EC Karte " + accountNumber, 1, "brieftasche");
                HUDHandler.SendNotification(player, 2, 7000, $"Sie haben erfolgreich eine Kartenkopie für das Konto mit der Kontonummer ({accountNumber}) erhalten.");
            }
        }

        [AsyncClientEvent("Server:ATM:requestBankData")]
        public static void RequestATMBankData(ClassicPlayer player, int accountNumber)
        {
            if (player == null || !player.Exists || player.CharacterId <= 0) return;
            player.EmitLocked("Client:ATM:BankATMSetRequestedData", CharactersBank.GetBankAccountMoney(accountNumber), ServerBankPapers.GetBankAccountBankPaper(player, accountNumber));
        }

        [AsyncClientEvent("Server:ATM:WithdrawMoney")]
        public static void WithdrawATMMoney(IPlayer player, int accountNumber, int amount, string zoneName)
        {
            if (player == null || !player.Exists || accountNumber == 0 || amount < 1) return;
            int charid = User.GetPlayerOnline(player);
            if (charid == 0) return;
            if (CharactersBank.GetBankAccountLockStatus(accountNumber)) { HUDHandler.SendNotification(player, 3, 7000, $"Diese EC Karte ist gesperrt und kann nicht weiter benutzt werden."); 
                if(CharactersInventory.ExistCharacterItem(charid, "EC Karte " + accountNumber, "brieftasche")) { 
                    CharactersInventory.RemoveCharacterItemAmount(charid, "EC Karte " + accountNumber, 1, "brieftasche"); 
                } 
                return; 
            }
            if (CharactersBank.GetBankAccountMoney(accountNumber) < amount) { 
                HUDHandler.SendNotification(player, 3, 7000, $"Ihr Konto ist für diese Summe nicht ausreichend gedeckt."); 
                return; 
            }

            DateTime dateTime = DateTime.Now;
            CharactersBank.SetBankAccountMoney(accountNumber, (CharactersBank.GetBankAccountMoney(accountNumber) - amount)); //Geld vom Konto abziehen
            CharactersInventory.AddCharacterItem(charid, "Bargeld", amount, "brieftasche"); //Spieler Geld geben
            ServerBankPapers.CreateNewBankPaper(accountNumber, dateTime.ToString("dd.MM.yyyy"), dateTime.ToString("HH.mm"), "Auszahlung", "None", "None", $"-{amount}$", zoneName);
            HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}$ von deinem Bankkonto abgehoben.");
            DiscordLog.SendEmbed("bank", "Bank LOG", $"{Characters.GetCharacterName(charid)} hat {amount}$ von seinem Bankkonto abgehoben.");
        }

        [AsyncClientEvent("Server:ATM:TryPin")]
        public static void TryATMPin(IPlayer player, string action, int accountNumber)
        {
            if (player == null || !player.Exists) return;
            int charid = User.GetPlayerOnline(player);
            if (charid == 0) return;
            if(action == "reset") { CharactersBank.ResetBankAccountPINTrys(accountNumber); }
            else if(action == "increase") {
                CharactersBank.SetBankAccountPinTrys(accountNumber, (CharactersBank.GetBankAccountPinTrys(accountNumber) + 1));
                if(CharactersBank.GetBankAccountPinTrys(accountNumber) >= 3)
                {
                    player.EmitLocked("Client:ATM:BankATMdestroyCEFBrowser");
                    HUDHandler.SendNotification(player, 3, 7000, $"Sie haben die Geheimzahl zu oft falsch eingegeben, Ihre Karte wurde gesperrt und eingezogen.");
                    CharactersBank.ChangeBankAccountLockStatus(accountNumber);
                    CharactersInventory.RemoveCharacterItemAmount(charid, "EC Karte " + accountNumber, 1, "brieftasche");
                } else
                {
                    HUDHandler.SendNotification(player, 3, 7000, $"Sie haben Ihre Geheimzahl falsch eingegeben.");
                }
            }
        }

        [AsyncClientEvent("Server:ATM:DepositMoney")]
        public static void DepositATMMoney(IPlayer player, int accountNumber, int amount, string zoneName)
        {
            if (player == null || !player.Exists || accountNumber == 0 || amount < 1) return;
            int charid = User.GetPlayerOnline(player);
            if (charid == 0) return;
            if (CharactersBank.GetBankAccountLockStatus(accountNumber)) { HUDHandler.SendNotification(player, 3, 5000, $"Diese EC Karte ist gesperrt und kann nicht weiter benutzt werden."); 
                if (CharactersInventory.ExistCharacterItem(charid, "EC Karte " + accountNumber, "brieftasche")) { 
                    CharactersInventory.RemoveCharacterItemAmount(charid, "EC Karte " + accountNumber, 1, "brieftasche"); 
                } 
                return; 
            }
            if(!CharactersInventory.ExistCharacterItem(charid, "Bargeld", "brieftasche") || CharactersInventory.GetCharacterItemAmount(charid, "Bargeld", "brieftasche") < amount) { 
                HUDHandler.SendNotification(player, 3, 7000, $"Du hast nicht genug Bargeld in deiner brieftasche dabei ({amount}$)."); 
                return; 
            }
            DateTime dateTime = DateTime.Now;
            CharactersBank.SetBankAccountMoney(accountNumber, (CharactersBank.GetBankAccountMoney(accountNumber) + amount)); //Geld aufs Konto packen
            CharactersInventory.RemoveCharacterItemAmount(charid, "Bargeld", amount, "brieftasche"); //Spieler Geld entfernen
            ServerBankPapers.CreateNewBankPaper(accountNumber, dateTime.ToString("dd.MM.yyyy"), dateTime.ToString("HH.mm"), "Einzahlung", "None", "None", $"+{amount}$", zoneName);
            HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}$ auf dein Bankkonto eingezahlt.");
            DiscordLog.SendEmbed("bank", "Bank LOG", $"{Characters.GetCharacterName(charid)} hat {amount}$ auf sein Bankkonto eingezahlt.");
        }

        [AsyncClientEvent("Server:ATM:TransferMoney")]
        public static void TransferATMMoney(IPlayer player, int accountNumber, int targetNumber, int amount, string zoneName)
        {
            if (player == null || !player.Exists || accountNumber == 0 || targetNumber == 0 || amount < 1) return;
            int charid = User.GetPlayerOnline(player);
            if (charid == 0) return;
            if (CharactersBank.GetBankAccountLockStatus(accountNumber)) { HUDHandler.SendNotification(player, 3, 5000, $"Diese EC Karte ist gesperrt und kann nicht weiter benutzt werden."); 
                if (CharactersInventory.ExistCharacterItem(charid, "EC Karte " + accountNumber, "brieftasche")) { 
                    CharactersInventory.RemoveCharacterItemAmount(charid, "EC Karte " + accountNumber, 1, "brieftasche"); 
                } 
                return; 
            }
            if(accountNumber == targetNumber) { 
                HUDHandler.SendNotification(player, 3, 5000, $"Sie können sich selber kein Geld überweisen."); 
                return; 
            }
            if(CharactersBank.GetBankAccountMoney(accountNumber) < amount) { 
                HUDHandler.SendNotification(player, 3, 5000, $"Ihr Bankkonto ist für diese Transaktion nicht ausreichend gedeckt ({amount}$)."); 
                return; 
            }
            CharactersBank.SetBankAccountMoney(accountNumber, (CharactersBank.GetBankAccountMoney(accountNumber) - amount)); //Geld vom Konto abziehen
            CharactersBank.SetBankAccountMoney(targetNumber, (CharactersBank.GetBankAccountMoney(targetNumber) + amount)); //Geld aufs Zielkonto addieren

            string Date = DateTime.Now.ToString("d", CultureInfo.CreateSpecificCulture("de-DE"));
            string Time = DateTime.Now.ToString("t", CultureInfo.CreateSpecificCulture("de-DE"));
            ServerBankPapers.CreateNewBankPaper(accountNumber, Date, Time, "Ausgehende Überweisung", $"{targetNumber}", "None", $"-{amount}$", zoneName);
            ServerBankPapers.CreateNewBankPaper(targetNumber, Date, Time, "Eingehende Überweisung", $"{accountNumber}", "None", $"+{amount}$", zoneName);
            HUDHandler.SendNotification(player, 2, 5000, $"Sie haben erfolgreich {amount}$ an das Konto mit der Kontonummer {targetNumber} überwiesen.");
        }

        [AsyncClientEvent("Server:FactionBank:DepositMoney")]
        public static void DepositFactionMoney(IPlayer player, string type, int factionId, int moneyAmount) //Type: faction | company
        {
            try
            {
                if (player == null || !player.Exists || factionId <= 0 || moneyAmount < 1 || type == "") return;
                if (type != "faction" && type != "company") return;
                int charid = User.GetPlayerOnline(player);
                if (charid == 0) return;

                if(type == "faction")
                {
                    if (!ServerFactions.IsCharacterInAnyFaction(charid)) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Du bist in keiner Fraktion und hast keine Berechtigung dazu."); 
                        return; 
                    }
                    if (factionId != ServerFactions.GetCharacterFactionId(charid)) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Ein unerwarteter Fehler ist aufgetreten. [FACTIONBANK-001]"); 
                        return; 
                    }
                    if (ServerFactions.GetCharacterFactionRank(charid) != ServerFactions.GetFactionMaxRankCount(factionId) && ServerFactions.GetCharacterFactionRank(charid) != ServerFactions.GetFactionMaxRankCount(factionId) - 1) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Du hast nicht den benötigten Rang um auf das Fraktionskonto zuzugreifen."); 
                        return; 
                    }
                } else if(type == "company")
                {
                    if(!ServerCompanys.IsCharacterInAnyServerCompany(charid)) { HUDHandler.SendNotification(player, 4, 7000, "Du bist in keinem Unternehmen und hast keine Berechtigung dazu."); return; }
                    if(factionId != ServerCompanys.GetCharacterServerCompanyId(charid)) { HUDHandler.SendNotification(player, 4, 7000, "Ein unerwarteter Fehler ist aufgetreten. [FACTIONBANK-0001]"); return; }
                    if(ServerCompanys.GetCharacterServerCompanyRank(charid) < 1) { HUDHandler.SendNotification(player, 3, 7000, "Du hast nicht den benötigten Rang um auf das Unternehmenskonto zuzugreifen."); return; }
                }
               
                if (!CharactersInventory.ExistCharacterItem(charid, "Bargeld", "brieftasche") || CharactersInventory.GetCharacterItemAmount(charid, "Bargeld", "brieftasche") < moneyAmount) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Du hast nicht genügend Bargeld zum Einzahlen dabei."); 
                    return; 
                }
                CharactersInventory.RemoveCharacterItemAmount(charid, "Bargeld", moneyAmount, "brieftasche");

                if(type == "faction")
                {
                    DiscordLog.SendEmbed("fraktionsbank", "Fraktionsbank LOG", $"{Characters.GetCharacterName(charid)} hat {moneyAmount}$ auf das Fraktionskonto (ID: {factionId}) eingezahlt.");
                    ServerFactions.SetFactionBankMoney(factionId, (ServerFactions.GetFactionBankMoney(factionId) + moneyAmount));
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast erfolgreich {moneyAmount}$ auf das Fraktionskonto eingezahlt.");
                    LoggingService.NewFactionLog(factionId, charid, 0, "bank", $"{Characters.GetCharacterName(charid)} ({charid}) hat {moneyAmount}$ auf das Fraktionskonto eingezahlt.");
                    return;
                } else if(type =="company")
                {
                    DiscordLog.SendEmbed("companybank", "Unternehmenskonto LOG", $"{Characters.GetCharacterName(charid)} hat {moneyAmount}$ auf das Unternehmenskonto (ID: {factionId}) eingezahlt.");
                    ServerCompanys.SetServerCompanyMoney(factionId, (ServerCompanys.GetServerCompanyMoney(factionId) + moneyAmount));
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast erfolgreich {moneyAmount}$ auf das Unternehmenskonto eingezahlt.");
                    LoggingService.NewCompanyLog(factionId, charid, 0, "bank", $"{Characters.GetCharacterName(charid)} ({charid}) hat {moneyAmount}$ auf das Unternehmenskonto eingezahlt.");
                    return;
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:FactionBank:WithdrawMoney")]
        public static void WithdrawFactionMoney(IPlayer player, string type, int factionId, int moneyAmount) //Type: faction | company
        {
            try
            {
                if (player == null || !player.Exists || factionId <= 0 || moneyAmount < 1 || type == "") return;
                if (type != "faction" && type != "company") return;
                int charid = User.GetPlayerOnline(player);
                if (charid == 0) return;

                if(type == "faction")
                {
                    if (!ServerFactions.IsCharacterInAnyFaction(charid)) {
                        HUDHandler.SendNotification(player, 4, 7000, "Du bist in keiner Fraktion und hast keine Berechtigung dazu."); 
                        return; 
                    }
                    if (factionId != ServerFactions.GetCharacterFactionId(charid)) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Ein unerwarteter Fehler ist aufgetreten. [FACTIONBANK-001]");
                        return; 
                    }
                    if (ServerFactions.GetCharacterFactionRank(charid) != ServerFactions.GetFactionMaxRankCount(factionId) && ServerFactions.GetCharacterFactionRank(charid) != ServerFactions.GetFactionMaxRankCount(factionId) - 1) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Du hast nicht den benötigten Rang um auf das Fraktionskonto zuzugreifen."); 
                        return; 
                    }
                    if (ServerFactions.GetFactionBankMoney(factionId) < moneyAmount) { HUDHandler.SendNotification(player, 3, 7000, "Soviel Geld ist auf dem Fraktionskonto nicht vorhanden."); return; }
                    ServerFactions.SetFactionBankMoney(factionId, ServerFactions.GetFactionBankMoney(factionId) - moneyAmount);
                    CharactersInventory.AddCharacterItem(charid, "Bargeld", moneyAmount, "brieftasche"); 
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast erfolgreich {moneyAmount}$ vom Fraktionskonto abgebucht.");
                    DiscordLog.SendEmbed("frakbank", "Fraktionsbank LOG", $"{Characters.GetCharacterName(charid)} hat {moneyAmount}$ vom Fraktionskonto (ID: {factionId}) abgebucht.");
                    LoggingService.NewFactionLog(factionId, charid, 0, "bank", $"{Characters.GetCharacterName(charid)} ({charid}) hat {moneyAmount}$ vom Fraktionskonto abgebucht.");
                    return;
                } 
                else if(type == "company")
                {
                    if(!ServerCompanys.IsCharacterInAnyServerCompany(charid)) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Du bist in keinem Unternehmen und hast keine Berechtigung dazu."); 
                        return; 
                    }
                    if (factionId != ServerCompanys.GetCharacterServerCompanyId(charid)) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Ein unerwarteter Fehler ist aufgetreten. [FACTIONBANK-0001]"); 
                        return; 
                    }
                    if(ServerCompanys.GetCharacterServerCompanyRank(charid) < 1) {
                        HUDHandler.SendNotification(player, 3, 7000, "Du hast nicht den benötigten Rang um auf das Unternehmenskonto zuzugreifen."); 
                        return; 
                    }
                    if(ServerCompanys.GetServerCompanyMoney(factionId) < moneyAmount) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Soviel Geld ist auf dem Unternehmenskonto nicht vorhanden."); 
                        return; 
                    }
                    ServerCompanys.SetServerCompanyMoney(factionId, ServerCompanys.GetServerCompanyMoney(factionId) - moneyAmount);
                    CharactersInventory.AddCharacterItem(charid, "Bargeld", moneyAmount, "brieftasche");
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast erfolgreich {moneyAmount}$ vom Unternehmenskonto abgebucht.");
                    DiscordLog.SendEmbed("companybank", "Unternehmenskonto Log", $"{ Characters.GetCharacterName(charid)} hat {moneyAmount}$ vom Unternehmenskonto (ID: {factionId}) abgebucht.");
                    LoggingService.NewCompanyLog(factionId, charid, 0, "bank", $"{ Characters.GetCharacterName(charid)} ({charid}) hat {moneyAmount}$ vom Unternehmenskonto abgebucht.");
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
