using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Model;
using System;
using System.Timers;
using Altv_Roleplay.Utils;
using Altv_Roleplay.Factories;
using System.Linq;
using System.Globalization;
using Altv_Roleplay.Services;
using AltV.Net.Elements.Refs;
using System.Diagnostics;
using AltV.Net.Async;

namespace Altv_Roleplay.Handler
{
    class TimerHandler
    {
        public static void OnCheckTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                foreach (ClassicPlayer player in Alt.GetAllPlayers().ToList())
                {
                    if (player == null) continue;
                    using (var playerReference = new PlayerRef(player))
                    {
                        if (!playerReference.Exists) return;
                        if (player == null || !player.Exists) continue;
                        lock (player)
                        {
                            if (player == null || !player.Exists) continue;
                            if (player.Dimension != 10000 && ((ClassicPlayer)player).accountId == 0) player.kickWithMessage("Fehler #1339 erkannt");
                            if (player.Dimension == 0) { if (User.GetPlayerOnline(player) <= 0 || User.GetPlayerSocialclubIdbyAccId(User.GetPlayerAccountId(player)) != player.SocialClubId || User.GetPlayerHardwareIdbyAccId(User.GetPlayerAccountId(player)) != player.HardwareIdHash) player.kickWithMessage("Fehler #1338 erkannt"); }
                        }
                    }
                }
                stopwatch.Stop();
            }
            catch(Exception ex)
            {
                Alt.Log($"{ex}");
            }
        }

        public static void OnEntityTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                WeatherHandler.GetRealWeatherType();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                foreach (ClassicPlayer player in Alt.GetAllPlayers().ToList())
                {
                    if (player == null) continue;
                    using (var playerReference = new PlayerRef(player))
                    {
                        if (!playerReference.Exists) return;
                        if (player == null || !player.Exists) continue;
                        lock (player)
                        {
                            if (player == null || !player.Exists) continue;
                            int charId = User.GetPlayerOnline(player);
                            if (charId > 0)
                            {
                                Characters.SetCharacterLastPosition(charId, player.Position, player.Dimension);
                                if (User.IsPlayerBanned(player)) { player.kickWithMessage($"Du bist gebannt. (Grund: {User.GetPlayerBanReason(player)})."); }
                                Characters.SetCharacterHealth(charId, player.Health);
                                Characters.SetCharacterArmor(charId, player.Armor);
                                if (!WeatherHandler.isNotDifferentWeather) WeatherHandler.SetRealWeather(player);
                                if(player.IsInVehicle) { player.EmitLocked("Client:HUD:GetDistanceForVehicleKM"); HUDHandler.SendInformationToVehicleHUD(player); }
                                Characters.IncreaseCharacterPaydayAndTaxTime(charId);

                                if (Characters.IsCharacterUnconscious(charId))
                                {
                                    int unconsciousTime = Characters.GetCharacterUnconsciousTime(charId);
                                    if (unconsciousTime > 0) { Characters.SetCharacterUnconscious(charId, true, unconsciousTime - 1); }
                                    else if (unconsciousTime <= 0)
                                    {
                                        Characters.SetCharacterUnconscious(charId, false, 0);
                                        Characters.IsCharacterStabilisiert(charId, false);
                                        DeathHandler.CloseDeathscreen(player);
                                        if (!User.IsPlayerICWhitelisted(charId))
                                        {
                                            lock (player)
                                            {
                                                if (player == null) continue;
                                                player.Spawn(new Position(-1128.5406f, -2783.2615f, 27.695923f), 0);
                                                player.LastPosition = new Position(-1128.5406f, -2783.2615f, 27.695923f);
                                            }
                                        }
                                        else
                                        {
                                            lock (player)
                                            {
                                                if (player == null) continue;
                                                player.Spawn(new Position(355.54285f, -596.33405f, 28.75768f), 0);
                                                player.LastPosition = new Position(355.54285f, -596.33405f, 28.75768f);
                                            }
                                        }
                                        player.Health = player.MaxHealth;
                                    }
                                }

                                if (Characters.IsCharacterFastFarm(charId))
                                {
                                    int fastFarmTime = Characters.GetCharacterFastFarmTime(charId);
                                    if (fastFarmTime > 0) Characters.SetCharacterFastFarm(charId, true, fastFarmTime - 1);
                                    else if (fastFarmTime <= 0) Characters.SetCharacterFastFarm(charId, false, 0);
                                }

                                if(Characters.IsCharacterInJail(charId))
                                {
                                    int jailTime = Characters.GetCharacterJailTime(charId);
                                    if (jailTime > 0) Characters.SetCharacterJailTime(charId, true, jailTime - 1);
                                    else if(jailTime <= 0)
                                    {
                                        if (CharactersWanteds.HasCharacterWanteds(charId))
                                        {
                                            int jailTimes = CharactersWanteds.GetCharacterWantedFinalJailTime(charId);
                                            int jailPrice = CharactersWanteds.GetCharacterWantedFinalJailPrice(charId);
                                            if (CharactersBank.HasCharacterBankMainKonto(charId))
                                            {
                                                int accNumber = CharactersBank.GetCharacterBankMainKonto(charId);
                                                int bankMoney = CharactersBank.GetBankAccountMoney(accNumber);
                                                CharactersBank.SetBankAccountMoney(accNumber, bankMoney - jailPrice);
                                                HUDHandler.SendNotification(player, 1, 7000, $"Durch deine Inhaftierung wurden dir {jailPrice}$ vom Konto abgezogen.");
                                            }
                                            HUDHandler.SendNotification(player, 1, 7000, $"Du sitzt nun für {jailTimes} Minuten im Gefängnis.");
                                            Characters.SetCharacterJailTime(charId, true, jailTimes);
                                            CharactersWanteds.RemoveCharacterWanteds(charId);
                                            player.Position = new Position(-559.411f, -132.75165f, 33.744995f);
                                            player.LastPosition = new Position(-559.411f, -132.75165f, 33.744995f);
                                            if (Characters.GetCharacterGender(charId) == false)
                                            {
                                                player.SetClothes(11, 5, 0, 0);
                                                player.SetClothes(3, 5, 0, 0);
                                                player.SetClothes(4, 5, 7, 0);
                                                player.SetClothes(6, 7, 0, 0);
                                                player.SetClothes(8, 1, 88, 0);
                                            }
                                            else
                                            {
                                                player.SetClothes(11, 247, 0, 0);
                                                player.SetClothes(4, 66, 6, 0);
                                                player.SetClothes(3, 4, 0, 0);
                                                player.SetClothes(8, 3, 0, 0);
                                                player.SetClothes(6, 60, 9, 0);
                                            }
                                        }
                                        else
                                        {
                                            Characters.SetCharacterJailTime(charId, false, 0);
                                            Characters.SetCharacterCorrectClothes(player);
                                            player.Position = new Position(-582.9099f, -146.74286f, 38.22705f);
                                            player.LastPosition = new Position(-582.9099f, -146.74286f, 38.22705f);
                                            HUDHandler.SendNotification(player, 1, 7000, "Du wurdest aus dem Gefängnis entlassen.");
                                        }
                                    }
                                }

                                if (Characters.GetCharacterPaydayTime(charId) >= 60)
                                {
                                    Characters.IncreaseCharacterPlayTimeHours(charId);
                                    Characters.ResetCharacterPaydayTime(charId);
                                    if (CharactersBank.HasCharacterBankMainKonto(charId))
                                    {
                                        int accountNumber = CharactersBank.GetCharacterBankMainKonto(charId);
                                        if (!ServerFactions.IsCharacterInAnyFaction(charId) || ServerFactions.GetCharacterFactionId(charId) == 0)
                                        {
                                            CharactersBank.SetBankAccountMoney(accountNumber, CharactersBank.GetBankAccountMoney(accountNumber) + 350); //350$ Stütze
                                            ServerBankPapers.CreateNewBankPaper(accountNumber, DateTime.Now.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")), DateTime.Now.ToString("t", CultureInfo.CreateSpecificCulture("de-DE")), "Eingehende Überweisung", "Staat", "Arbeitslosengeld", "+350$", "Unbekannt");
                                        }

                                        if (!Characters.IsCharacterCrimeFlagged(charId) && Characters.GetCharacterJob(charId) != "None" && DateTime.Now.Subtract(Convert.ToDateTime(Characters.GetCharacterLastJobPaycheck(charId))).TotalHours >= 12 && !ServerFactions.IsCharacterInAnyFaction(charId))
                                        {
                                            if (Characters.GetCharacterJobHourCounter(charId) >= ServerJobs.GetJobNeededHours(Characters.GetCharacterJob(charId)) - 1)
                                            {
                                                int jobCheck = ServerJobs.GetJobPaycheck(Characters.GetCharacterJob(charId));
                                                Characters.SetCharacterLastJobPaycheck(charId, DateTime.Now);
                                                Characters.ResetCharacterJobHourCounter(charId);
                                                CharactersBank.SetBankAccountMoney(accountNumber, CharactersBank.GetBankAccountMoney(accountNumber) + jobCheck);
                                                ServerBankPapers.CreateNewBankPaper(accountNumber, DateTime.Now.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")), DateTime.Now.ToString("t", CultureInfo.CreateSpecificCulture("de-DE")), "Eingehende Überweisung", "Arbeitsamt", $"Gehalt: {Characters.GetCharacterJob(charId)}", $"+{jobCheck}$", "Unbekannt");
                                                HUDHandler.SendNotification(player, 1, 7000, $"Gehalt erhalten (Beruf: {Characters.GetCharacterJob(charId)} | Gehalt: {jobCheck}$)");
                                            }
                                            else { Characters.IncreaseCharacterJobHourCounter(charId); }
                                        }

                                        if (ServerFactions.IsCharacterInAnyFaction(charId) && ServerFactions.IsCharacterInFactionDuty(charId))
                                        {
                                            int factionid = ServerFactions.GetCharacterFactionId(charId);
                                            int factionPayCheck = ServerFactions.GetFactionRankPaycheck(factionid, ServerFactions.GetCharacterFactionRank(charId));
                                            if (ServerFactions.GetFactionBankMoney(factionid) >= factionPayCheck)
                                            {
                                                ServerFactions.SetFactionBankMoney(factionid, ServerFactions.GetFactionBankMoney(factionid) - factionPayCheck);
                                                CharactersBank.SetBankAccountMoney(accountNumber, CharactersBank.GetBankAccountMoney(accountNumber) + factionPayCheck);
                                                HUDHandler.SendNotification(player, 1, 7000, $"Du hast deinen Lohn i.H.v. {factionPayCheck}$ erhalten ({ServerFactions.GetFactionRankName(factionid, ServerFactions.GetCharacterFactionRank(charId))})");
                                                ServerBankPapers.CreateNewBankPaper(accountNumber, DateTime.Now.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")), DateTime.Now.ToString("t", CultureInfo.CreateSpecificCulture("de-DE")), "Eingehende Überweisung", $"{ServerFactions.GetFactionFullName(factionid)}", $"Gehalt: {ServerFactions.GetFactionRankName(factionid, ServerFactions.GetCharacterFactionRank(charId))}", $"+{factionPayCheck}$", "Dauerauftrag");
                                                LoggingService.NewFactionLog(factionid, charId, 0, "paycheck", $"{Characters.GetCharacterName(charId)} hat seinen Lohn i.H.v. {factionPayCheck}$ erhalten ({ServerFactions.GetFactionRankName(factionid, ServerFactions.GetCharacterFactionRank(charId))}).");
                                            }
                                            else
                                            {
                                                HUDHandler.SendNotification(player, 3, 7000, $"Deine Fraktion hat nicht genügend Geld um dich zu bezahlen ({factionPayCheck}$).");
                                            }
                                        }
                                    }
                                    else { 
                                        HUDHandler.SendNotification(player, 3, 7000, $"Dein Einkommen konnte nicht überwiesen werden da du kein Hauptkonto hast."); 
                                    }
                                }

                                if (Characters.GetCharacterTaxTime(charId) >= 360)
                                {
                                    Characters.ResetCharacterTaxTime(charId);
                                    if (CharactersBank.HasCharacterBankMainKonto(charId))
                                    {
                                        var playerVehicles = ServerVehicles.ServerVehicles_.Where(x => x.id > 0 && x.charid == charId && !x.plate.StartsWith("NL"));
                                        int taxMoney = 0;
                                        foreach (var i in playerVehicles)
                                        {
                                            if (i.plate.StartsWith("LSPD") || i.plate.StartsWith("LSMD") || i.plate.StartsWith("BNY") || i.plate.StartsWith("DOJ") || i.plate.StartsWith("DMV")) return;
                                            if (i.plate.StartsWith("NL")) continue;
                                            taxMoney += ServerAllVehicles.GetVehicleTaxes(i.hash);
                                        }

                                        if (playerVehicles != null && taxMoney > 0)
                                        {
                                            int accountNumber = CharactersBank.GetCharacterBankMainKonto(charId);
                                            if (CharactersBank.GetBankAccountMoney(accountNumber) < taxMoney) { 
                                                HUDHandler.SendNotification(player, 3, 7000, $"Deine Fahrzeugsteuern konnten nicht abgebucht werden ({taxMoney}$)");
                                            }
                                            else
                                            {
                                                CharactersBank.SetBankAccountMoney(accountNumber, CharactersBank.GetBankAccountMoney(accountNumber) - taxMoney);
                                                ServerBankPapers.CreateNewBankPaper(accountNumber, DateTime.Now.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")), DateTime.Now.ToString("t", CultureInfo.CreateSpecificCulture("de-DE")), "Ausgehende Überweisung", "Zulassungsamt", $"Fahrzeugsteuer", $"-{taxMoney}$", "Bankeinzug");
                                                HUDHandler.SendNotification(player, 1, 7000, $"Du hast deine Fahrzeugsteuern i.H.v. {taxMoney}$ bezahlt.");
                                            }
                                        }
                                    }
                                    else { HUDHandler.SendNotification(player, 3, 7000, $"Du hast kein Hauptkonto"); }
                                }
                            }
                        }
                    }
                }
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                Alt.Log($"{ex}");
            }
        }

        internal static void ApartmentTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (var hotelApartment in ServerHotels.ServerHotelsApartments_.Where(x => x.ownerId > 0))
                {
                    if (hotelApartment == null) continue;
                    if(DateTime.Now.Subtract(Convert.ToDateTime(hotelApartment.lastRent)).TotalHours >= hotelApartment.maxRentHours)
                    {
                        int oldOwnerId = hotelApartment.ownerId;
                        ServerHotels.SetApartmentOwner(hotelApartment.hotelId, hotelApartment.id, 0);
                        ServerHotels.ClearApartmentInventory(hotelApartment.id);
                        foreach(IPlayer players in Alt.GetAllPlayers().ToList().Where(x => x != null && x.Exists && User.GetPlayerOnline(x) == oldOwnerId))
                        {
                            HUDHandler.SendNotification(players, 1, 7000, "Deine Mietdauer im Hotel ist ausgelaufen, dein Zimmer wurde gekündigt");
                        }
                    }
                }
            }
            catch(Exception ex) { Alt.Log($"{ex}"); }
        }

        internal static void OnDesireTimer(object sender, ElapsedEventArgs e)
        {
            foreach (IPlayer player in Alt.GetAllPlayers().ToList())
            {
                if (player == null) continue;
                using (var pRef = new PlayerRef(player))
                {
                    if (!pRef.Exists) return;
                    lock (player)
                    {
                        if (player.Exists && User.GetPlayerOnline(player) != 0)
                        {
                            int charId = User.GetPlayerOnline(player);
                            int random = new Random().Next(1, 1);
                            if (Characters.GetCharacterHunger(User.GetPlayerOnline(player)) > 0)
                            {
                                Characters.SetCharacterHunger(charId, (Characters.GetCharacterHunger(charId) - random));
                                if (Characters.GetCharacterHunger(charId) < 0) { Characters.SetCharacterHunger(charId, 0); }
                            }
                            else
                            {
                                player.Health = (ushort)(player.Health - 3);
                                Characters.SetCharacterHealth(charId, player.Health);
                                HUDHandler.SendNotification(player, 1, 7000, $"Du hast Hunger.");
                            }

                            if (Characters.GetCharacterThirst(User.GetPlayerOnline(player)) > 0)
                            {
                                Characters.SetCharacterThirst(charId, (Characters.GetCharacterThirst(charId) - random));
                                if (Characters.GetCharacterThirst(charId) < 0) { Characters.SetCharacterThirst(charId, 0); }
                            }
                            else
                            {
                                player.Health = (ushort)(player.Health - 5);
                                Characters.SetCharacterHealth(charId, player.Health);
                                HUDHandler.SendNotification(player, 1, 7000, $"Du hast Durst.");
                            }
                            player.EmitLocked("Client:HUD:UpdateDesire", Characters.GetCharacterHunger(charId), Characters.GetCharacterThirst(charId), Characters.GetCharacterHealth(charId), Characters.GetCharacterArmor(charId)); //HUD updaten
                        }
                    }
                }
            }
        }

        internal static void VehicleTimer(object sender, ElapsedEventArgs e)
        {
            foreach (IVehicle vehicle in Alt.GetAllVehicles().Where(x => x != null && x.Exists && x.GetVehicleId() > 0))
            {
                ServerVehicles.UpdateVehicleLastPosition(vehicle);
            }
            foreach (ClassicPlayer player in Alt.GetAllPlayers().Where(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId > 0))
            {
                if (player.IsInVehicle)
                {
                    HUDHandler.SendInformationToVehicleHUD(player);
                }
            }
        }

        internal static void AtmRobTimer(object sender, ElapsedEventArgs e)
        {
            foreach (var robATM in ServerATM.ServerATM_.ToList().Where(x => x.isRobbed == true))
            {
                robATM.isRobbed = false;
            }
        }

        internal static void FuelTimer(object sender, ElapsedEventArgs e)
        {
            foreach (IVehicle Veh in Alt.GetAllVehicles().ToList())
            {
                if (Veh == null || !Veh.Exists) { continue; }
                using (var vRef = new VehicleRef(Veh))
                {
                    if (!vRef.Exists) continue;
                    lock (Veh)
                    {
                        if (Veh == null || !Veh.Exists) continue;
                        long vehID = Veh.GetVehicleId();
                        if (vehID <= 0) { continue; }
                        ServerVehicles.SaveVehiclePositionAndStates(Veh);
                        if (Veh.EngineOn == true) { ServerVehicles.SetVehicleFuel(Veh, ServerVehicles.GetVehicleFuel(Veh) - 0.04f); }
                    }
                }
            }
        }

        internal static void OnCheckMinijobTimer(object sendr, ElapsedEventArgs e)
        {
            foreach (IPlayer player in Alt.GetAllPlayers().ToList())
            {
                int charId = User.GetPlayerOnline(player);
                // Bus Minijob
                foreach (var veh in Alt.GetAllVehicles().Cast<ClassicVehicle>().Where(x => x != null && x.Exists && x.NumberplateText == $"BUS-{charId}").ToList())
                {
                    if (!player.Position.IsInRange(veh.Position, 80f))
                    {
                        ServerVehicles.RemoveVehiclePermanently(veh);
                        HUDHandler.SendNotification(player, 4, 5000, "Du hast dich zu weit vom Fahrzeug entfernt, der Minijob wurde beendet!");
                        player.SetPlayerCurrentMinijob("None");
                        player.SetPlayerCurrentMinijobRouteId(0);
                        player.SetPlayerCurrentMinijobStep("None");
                        player.SetPlayerCurrentMinijobActionCount(0);
                        player.EmitLocked("Client:Minijob:RemoveJobMarker");
                        veh.Remove();
                        return;
                    }
                }
                //Muelmann Job
                foreach (var veh in Alt.GetAllVehicles().Cast<ClassicVehicle>().Where(x => x != null && x.Exists && x.NumberplateText == $"MM-{charId}").ToList())
                {
                    if (!player.Position.IsInRange(veh.Position, 80f))
                    {
                        ServerVehicles.RemoveVehiclePermanently(veh);
                        HUDHandler.SendNotification(player, 4, 5000, "Du hast dich zu weit vom Fahrzeug entfernt, der Minijob wurde beendet!");
                        player.SetPlayerCurrentMinijob("None");
                        player.SetPlayerCurrentMinijobRouteId(0);
                        player.SetPlayerCurrentMinijobStep("None");
                        player.SetPlayerCurrentMinijobActionCount(0);
                        player.EmitLocked("Client:Minijob:RemoveJobMarker");
                        veh.Remove();
                        return;
                    }
                }
                // LKW Minijob
                foreach (var veh in Alt.GetAllVehicles().Cast<ClassicVehicle>().Where(x => x != null && x.Exists && x.NumberplateText == $"LKW-{charId}").ToList())
                {
                    if (!player.Position.IsInRange(veh.Position, 80f))
                    {
                        ServerVehicles.RemoveVehiclePermanently(veh);
                        HUDHandler.SendNotification(player, 4, 5000, "Du hast dich zu weit vom Fahrzeug entfernt, der Minijob wurde beendet!");
                        player.SetPlayerCurrentMinijob("None");
                        player.SetPlayerCurrentMinijobRouteId(0);
                        player.SetPlayerCurrentMinijobStep("None");
                        player.SetPlayerCurrentMinijobActionCount(0);
                        player.EmitLocked("Client:Minijob:RemoveJobMarker");
                        veh.Remove();
                        return;
                    }
                }
                // LKW Minijob
                foreach (var veh in Alt.GetAllVehicles().Cast<ClassicVehicle>().Where(x => x != null && x.Exists && x.NumberplateText == $"ANH-{charId}").ToList())
                {
                    if (!player.Position.IsInRange(veh.Position, 80f))
                    {
                        ServerVehicles.RemoveVehiclePermanently(veh);
                        player.SetPlayerCurrentMinijob("None");
                        player.SetPlayerCurrentMinijobRouteId(0);
                        player.SetPlayerCurrentMinijobStep("None");
                        player.SetPlayerCurrentMinijobActionCount(0);
                        player.EmitLocked("Client:Minijob:RemoveJobMarker");
                        veh.Remove();
                        return;
                    }
                }
            }
        }

        public static void OnHUDTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();
                foreach (IPlayer player in Alt.GetAllPlayers().ToList())
                {
                    if (player == null) continue;
                    using (var playerReference = new PlayerRef(player))
                    {
                        if (!playerReference.Exists) return;
                        if (player == null || !player.Exists) continue;
                        lock (player)
                        {
                            if (player == null || !player.Exists) continue;
                            int charId2 = User.GetPlayerOnline(player);
                            if (charId2 > 0)
                            {
                                Characters.SetCharacterHealth(charId2, player.Health);
                                Characters.SetCharacterArmor(charId2, player.Armor);
                            }

                        }
                        int charId = User.GetPlayerOnline(player);
                        player.EmitLocked("Client:HUD:UpdateDesire", Characters.GetCharacterHunger(charId), Characters.GetCharacterThirst(charId), Characters.GetCharacterHealth(charId), Characters.GetCharacterArmor(charId)); //HUD updaten

                        int currPoliceMembers = ServerFactions.ServerFactionMembers_.ToList().Where(x => x.IsOnline == true && x.factionId == 2 && x.isDuty == true).Count();
                        player.EmitLocked("Client:Stars:UpdateStars", currPoliceMembers);
                    }
                }
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                Alt.Log($"{ex}");
            }
        }

        internal static void VehicleTestTimer(object sendr, ElapsedEventArgs e)
        {
            foreach (IPlayer player in Alt.GetAllPlayers().ToList())
            {
                int charId = User.GetPlayerOnline(player);
                var vehicle = ServerVehicles.ServerVehicles_.Where(x => x.id > 0 && x.charid == (int)player.GetCharacterMetaId() && x.plate == $"TEST-{charId}");
                if (vehicle != null)
                {
                    foreach (var i in vehicle)
                    {
                        var AblaufTime = i.buyDate + new TimeSpan(0, 0, 10, 0);
                        var NotifyTime = AblaufTime - new TimeSpan(0, 0, 2, 0);
                        var NotifyTime2 = NotifyTime.ToString("HH:mm");
                        var DeleteTime = AblaufTime.ToString("HH:mm");
                        var Time = DateTime.Now.ToString("HH:mm");
                        if (Time == NotifyTime2)
                        {
                            HUDHandler.SendNotification(player, 1, 7000, "Deine Testfahrt ist in 2 Minuten beendet.");
                            return;
                        }
                        else if (Time == DeleteTime)
                        {
                            foreach (var veh in Alt.GetAllVehicles().Cast<ClassicVehicle>().Where(x => x != null && x.Exists && x.NumberplateText == $"TEST-{charId}").ToList())
                            {
                                ServerVehicles.RemoveVehiclePermanently(veh);
                                HUDHandler.SendNotification(player, 4, 5000, "Die Testzeit ist jetzt vorbei.");
                                player.SetPlayerCurrentTestVehicle("None");
                                veh.Remove();
                                return;
                            }
                            return;
                        }
                    }
                }
            }
        }
    }
}
