using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.Resources.Chat.Api;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.models;
using Altv_Roleplay.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Altv_Roleplay.Handler
{
    public class Commands : IScript
    {

        [Command("einreise")]
        public static void EinreiseCMD(IPlayer player, string name)
        {
            try
            {
                if (player == null || !player.Exists || name == null || player.AdminLevel() <= 1 || player.GetCharacterMetaId() <= 0) return;
                if (!player.HasData("isEinreisedienst")) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Du bist nicht im Einreisedienst!"); 
                    return; 
                }
                name = name.Replace("_", " ");
                int charId = Characters.GetCharacterIdFromCharName(name);
                if (!User.ExistPlayerByCharacterId(charId)) { 
                    HUDHandler.SendNotification(player, 4, 7000, $"Diese ID existiert nicht {charId}"); 
                    return; 
                }
                if (User.IsPlayerICWhitelisted(charId)) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Der Spieler hat bereits ein Visum."); 
                    return; 
                }
                ClassicPlayer targetP = (ClassicPlayer)Alt.Core.GetPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId == charId);

                float x = (float)-1045.2131f;
                float y = (float)-2750.8748f;
                float z = (float)21.360474f;
                targetP.Position = new Position(x, y, z);

                HUDHandler.SendNotification(targetP, 1, 7000, $"Sie haben ihr Visum erhalten!");
                HUDHandler.SendNotification(player, 1, 7000, $"Sie haben dem Spieler {name} erfolgreich ein Visum ausgestellt!");
                User.SetPlayerICWhitelisted(charId, true);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("rein")]    // Um Ins Einreise Amt zu kommen
        public static void ReinCMD(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.AdminLevel() <= 1 || player.GetCharacterMetaId() <= 0) return;
                if (!player.HasData("isEinreisedienst")) { HUDHandler.SendNotification(player, 4, 7000, "Du bist nicht im Einreisedienst!"); return; }
                int charid = User.GetPlayerOnline(player);

                float x = (float)-1065.5209f;
                float y = (float)-2798.0044f;
                float z = (float)27.695923f;
                player.Position = new Position(x, y, z);

                HUDHandler.SendNotification(player, 1, 7000, $"Sie sind nun im Einreiseamt!");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("raus")]    // Um aus dem Einreise Amt rauszukommen
        public static void RausCMD(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.AdminLevel() <= 1 || player.GetCharacterMetaId() <= 0) return;
                if (!player.HasData("isEinreisedienst")) { HUDHandler.SendNotification(player, 4, 7000, "Du bist nicht im Einreisedienst!"); return; }

                float x = (float)-1043.1692f;
                float y = (float)-2746.6418f;
                float z = (float)21.343628f;
                player.Position = new Position(x, y, z);

                HUDHandler.SendNotification(player, 1, 7000, $"Sie sind nun nicht mehr im Einreise Amt!");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("einreisedienst", true)]
        public static void EinreisedienstCMD(IPlayer player)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() <= 1) { HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); return; }

            if (player.HasData("isEinreisedienst"))
            {
                player.DeleteData("isEinreisedienst");
                Characters.SetCharacterCorrectClothes(player);
                HUDHandler.SendNotification(player, 1, 7000, $"Du befindest dich nun im nicht mehr im Einreisedienst!");
            }
            else
            {
                player.SetData("isEinreisedienst", true);
                if (!Characters.GetCharacterGender((int)player.GetCharacterMetaId()))
                {
                    //Männlich
                    player.SetClothes(1, 0, 0, 0);
                    player.SetClothes(4, 24, 0, 0);
                    player.SetClothes(6, 10, 0, 0);
                    player.SetClothes(3, 4, 0, 0);
                    player.SetClothes(11, 322, 0, 0);
                    player.SetClothes(8, 15, 0, 0);
                }
                else
                {
                    //Weiblich
                    player.SetClothes(1, 0, 0, 0);
                    player.SetClothes(11, 357, 0, 0);
                    player.SetClothes(4, 8, 0, 0);
                    player.SetClothes(3, 0, 0, 0);
                    player.SetClothes(8, 3, 0, 0);
                    player.SetClothes(6, 0, 0, 0);
                }
                HUDHandler.SendNotification(player, 1, 7000, $"Du befindest dich nun im Einreisedienst!");
            }
        }

        //Jail Time anzeigen
        [Command("jailtime")]
        public static void JailtimeCMD(ClassicPlayer player)
        {
            if (player == null || !player.Exists || player.CharacterId <= 0 || !Characters.IsCharacterInJail(player.CharacterId)) return;
            HUDHandler.SendNotification(player, 1, 7000, $"Du bist noch für { Characters.GetCharacterJailTime(player.CharacterId)} Minuten im Gefängnis.");
        }

        //OoC Messages
        [Command("ooc", true)]
        public static void OocCMD(IPlayer player, string msg)
        {
            if (player == null || !player.Exists) return;
            if (msg == null) { HUDHandler.SendNotification(player, 4, 7000, "Wie wäre es mit einer Nachricht? (Fehlt)"); return; }
            if (msg.Contains('<') || msg.Contains('>'))
            {
                HUDHandler.SendNotification(player, 4, 7000, "Ungültige Eingabe! (good try)");
                DiscordLog.SendEmbed("ooc", "OOC-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] hat rce probiert: {msg} (blocked)");
                return;
            }

            DiscordLog.SendEmbed("ooc", "OOC-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] Nachricht: {msg}");

            foreach (var client in Alt.GetAllPlayers())
            {
                var name = Characters.GetCharacterName((int)player.GetCharacterMetaId());
                if (client == null || !client.Exists) continue;
                var range = 5; //Change OOC Range!!
                if (client.Position.Distance(player.Position) <= range)
                {
                    HUDHandler.SendNotification((IPlayer)client, 2, 7000, $"[{(int)player.GetCharacterMetaId()}] {name}: \n " + msg);
                }
            }
        }

        // Jemanden Melden
        [Command("support", true)]
        public static void SupportCMD(IPlayer player, string msg)
        {
            try
            {
                if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0) return;
                DiscordLog.SendEmbed("support", "Support", $" { Characters.GetCharacterName((int)player.GetCharacterMetaId())}[{(int)player.GetCharacterMetaId()}]  {msg}");
                if (msg.Contains('<') || msg.Contains('>'))
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Ungültige Eingabe! (good try)");
                    DiscordLog.SendEmbed("support", "support-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] hat rce probiert: {msg} (blocked)");
                    return;
                }
                foreach (var admin in Alt.GetAllPlayers().Where(x => x != null && x.Exists && x.AdminLevel() >= 1))
                {
                    HUDHandler.SendNotification(admin, 4, 15000, $"[SUPPORT] { Characters.GetCharacterName(User.GetPlayerOnline(player))} (ID: {User.GetPlayerOnline(player)}) benötigt Support: {msg}");
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Jemanden Reporten 
        [Command("report", true)]
        public static void ReportCMD(IPlayer player, string msg)
        {
            if (player == null || !player.Exists) return;
            if (msg.Contains('<') || msg.Contains('>'))
            {
                HUDHandler.SendNotification(player, 4, 7000, "Ungültige Eingabe! (good try)");
                DiscordLog.SendEmbed("support", "Report-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] hat rce probiert: {msg} (blocked)");
                return;
            }

            List<string> playerList = new List<string>();
            foreach (var client in Alt.GetAllPlayers().Cast<ClassicPlayer>())
            {
                var name = Characters.GetCharacterName((int)client.GetCharacterMetaId());
                float dist = client.Position.Distance(player.Position);
                if (dist <= 100.0)
                {
                    playerList.Add($"{name}({(int)client.GetCharacterMetaId()}) - {Math.Round((decimal)dist, 2)}m\n");
                }
            }

            string final = $"[REPORT] { Characters.GetCharacterName((int)player.GetCharacterMetaId())} (ID: {(int)player.GetCharacterMetaId()}) benötigt Support:\n{msg}\nSpieler in der Nähe: \n";

            foreach (var p in playerList)
            {
                final += p;
            }

            DiscordLog.SendEmbed("report", "Report", final);
            foreach (var client in Alt.GetAllPlayers().Where(x => x != null && x.Exists && x.AdminLevel() >= 1))
            {
                if (client == null || !client.Exists) continue;
                HUDHandler.SendNotification((IPlayer)client, 4, 10000, final);
            }
        }

        // Spieler Kicken
        [Command("kick")]
        public static void CmdKICK(IPlayer player, int charId)
        {
            try
            {
                if (player == null || !player.Exists || charId <= 0 || player.AdminLevel() <= 2) return;
                var targetP = Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && User.GetPlayerOnline(x) == charId);
                if (targetP == null) return;
                targetP.Kick("");
                HUDHandler.SendNotification(player, 4, 7000, $"Spieler mit Char-ID {charId} Erfolgreich gekickt.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Spieler bannen
        [Command("ban")]
        public static void CmdBAn(IPlayer player, int charId, string reason)
        {
            try
            {
                if (player == null || !player.Exists || charId <= 0 || player.AdminLevel() <= 2 || reason == "") return;
                int accountId = Characters.GetCharacterAccountId(charId);
                User.SetPlayerBanned(accountId, true, reason);
                HUDHandler.SendNotification(player, 1, 7000, $"Spieler mit ID {accountId} Erfolgreich gebannt.");
                var targetP = Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && User.GetPlayerAccountId(x) == accountId);
                if (targetP != null) targetP.Kick($"{reason}");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Spieler Entbannen
        [Command("unban")]
        public static void CMD_Unban(ClassicPlayer player, int accId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || accId <= 0 || player.AdminLevel() <= 2) return;
                User.SetPlayerBanned(accId, false, "");
                HUDHandler.SendNotification(player, 4, 7000, $"Spieler mit ID {accId} Erfolgreich entbannt.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Announce Schreiben 
        [Command("announce", true)]
        public static void AnnounceCMD(IPlayer player, string msg)
        {
            try
            {
                if (player == null || !player.Exists) return;
                if (player.AdminLevel() <= 3) { HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); return; }
                foreach (var client in Alt.GetAllPlayers())
                {
                    if (client == null || !client.Exists) continue;
                    HUDHandler.SendNotification(client, 4, 20000, msg);
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Auto Spawnen
        [Command("car")]
        [Obsolete]
        public static void HeyCMD(ClassicPlayer player, string model)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() <= 9) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return; 
            }
            if (!player.HasData("isAduty")) { 
                HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                return; 
            }
            if (player.Vehicle != null && player.Vehicle.Exists) player.Vehicle.Remove();
            uint vehHash = Alt.Hash(model);
            if (Enum.IsDefined(typeof(AntiCheat_Vehicles.ForbiddenVehicles), (Utils.AntiCheat_Vehicles.ForbiddenVehicles)vehHash))
            {
                HUDHandler.SendNotification(player, 4, 7000, "Die Art Fahrzeuge können nicht gespawnt werden");
                return;
            }
            IVehicle veh = Alt.CreateVehicle(model, new Position(player.Position.X + 2f, player.Position.Y, player.Position.Z), player.Rotation);
            player.LastPosition = player.Position;
            veh.EngineOn = true;
            veh.LockState = VehicleLockState.Unlocked;
            veh.SetNumberplateTextAsync("SA-Market");
            veh.PrimaryColor = 44;
            veh.SecondaryColor = 44;
            veh.WheelColor = 44;
            veh.PearlColor = 3;
            player.EmitLocked("Client:HUD:setIntoVehicle", veh);
            player.LastPosition = player.Position;
        }

        // Admin Geben
        [Command("setadmin")]
        public static void CMD_Giveadmin(IPlayer player, int accId, int adminLevel)
        {
            if (player.AdminLevel() <= 1) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return; 
            }
            if (!player.HasData("isAduty")) { 
                HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                return; 
            }
            try
            {
                if (player == null || !player.Exists) return;
                User.SetPlayerAdminLevel(accId, adminLevel);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Lizenz Geben
        [Command("license")]
        public static void LicenseCMD(IPlayer player, int charId, string licenseshort)
        {
            if (licenseshort == null || !player.Exists) return;
            if (player.AdminLevel() <= 9) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return; 
            }
            if (!player.HasData("isAduty")) { 
                HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                return; 
            }
            if (charId <= 0) return;
            Characters.AddCharacterPermission(charId, licenseshort);
            CharactersLicenses.SetCharacterLicense(charId, licenseshort, true);
            HUDHandler.SendNotification(player, 2, 3500, $"Du hast die License {licenseshort} vergeben an: {charId}");
        }

        // Bankautomat setzen
        [Command("setAtm")]
        public static void SetATMCMD(IPlayer player, string name)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() <= 10) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return; 
            }
            if (!player.HasData("isAduty")) { 
                HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                return; 
            }
            ulong charId = player.GetCharacterMetaId();
            if (charId <= 0) return;
            ServerATM.CreateNewATM(player, 0, player.Position, name);
            HUDHandler.SendNotification(player, 2, 7000, $"ATM erfolgreich gesetzt: {name}");
        }

        //SetShopRobPos
        [Command("SetShopRobPos")]
        public static void SetShopRobPosCMD(IPlayer player, int shopId)
        {
            try
            {
                if (player.AdminLevel() <= 10) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte.");
                    return; 
                }
                if (!player.HasData("isAduty")) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                    return; 
                }
                if (shopId <= 0) return;
                var shop = ServerShops.ServerShops_.FirstOrDefault(x => x.shopId == shopId);
                if (shop != null)
                {
                    shop.robPos = new Vector3(player.Position.X, player.Position.Y, player.Position.Z);
                    using (gtaContext db = new gtaContext())
                    {
                        db.Server_Shops.Update(shop);
                        db.SaveChanges();
                    }
                    HUDHandler.SendNotification(player, 2, 7000, $"Shop Raub Position gesetzt Shop ID:{shopId}.");
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }


        // Telepotieren
        [Command("tppos")]
        public static void TpPosCMD(ClassicPlayer player, float X, float Y, float Z)
        {
            try
            {
                if (player == null || !player.Exists) return;
                if (player.AdminLevel() <= 9) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                    return; 
                }
                if (!player.HasData("isAduty")) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                    return; 
                }
                lock (player)
                {
                    if (player == null || !player.Exists) return;
                    player.Position = new Position(X, Y, Z);
                    player.LastPosition = new Position(X, Y, Z);
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Autos Entfernen
        [Command("dv")]
        public static void CMDDeleteVehicle(IPlayer player, float range = 2)
        {
            if (player.AdminLevel() <= 9) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte.");
                return; 
            }
            if (!player.HasData("isAduty")) { 
                HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                return; 
            }
            foreach (IVehicle veh in Alt.GetAllVehicles())
            {
                if (veh.Position.Distance(player.Position) <= range)
                {
                    Alt.RemoveVehicle(veh);
                }
            }
        }

        // Autos Einparken Mit ID
        [Command("parkvehicle")]
        public static void CMD_parkVehicleById(IPlayer player, int vehId)
        {
            try
            {
                if (player == null || !player.Exists || player.AdminLevel() <= 2 || vehId <= 0) return;
                var vehicle = Alt.Core.GetVehicles().ToList().FirstOrDefault(x => x != null && x.Exists && x.HasVehicleId() && (int)x.GetVehicleId() == vehId);
                if (vehicle == null) return;
                int vehClass = ServerAllVehicles.GetVehicleClass(vehicle.Model);
                switch (vehClass)
                {
                    case 0:
                        ServerVehicles.SetVehicleInGarage(vehicle, true, 30, false);
                        HUDHandler.SendNotification(player, 4, 5000, $"Fahrzeug {vehId} in Garage {ServerGarages.GetGarageName(1)} eingeparkt.");
                        break;
                    case 3:
                        ServerVehicles.SetVehicleInGarage(vehicle, true, 29, false);
                        HUDHandler.SendNotification(player, 4, 5000, $"Fahrzeug {vehId} in Garage {ServerGarages.GetGarageName(12)} eingeparkt.");
                        break;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Autos Einparken Mit ID
        [Command("carid")]
        public static void CMD_getVehicleID(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.AdminLevel() <= 2) return;
                string msg = "Liste aller Fahrzeuge:<br>";
                foreach (ClassicVehicle vehicle in Alt.GetAllVehicles().Where(x => x != null && x.Exists && ((ClassicVehicle)x).VehicleId > 0 && player.Position.IsInRange(x.Position, 5f)))
                {
                    if (vehicle == null) return;
                    msg += $"{ServerVehicles.GetVehiclePlateByOwner(vehicle.GetVehicleId())} ({vehicle.GetVehicleId()})<br>";
                }
                HUDHandler.SendNotification(player, 1, 12000, msg);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Spieler Wiederbeleben
        [Command("revive")]
        public static void ReviveTargetCMD(IPlayer player, int targetId)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() <= 2) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return; 
            }
            string charName = Characters.GetCharacterName(targetId);
            if (!Characters.ExistCharacterName(charName)) return;
            var tp = Alt.GetAllPlayers().FirstOrDefault(x => x != null && x.Exists && x.GetCharacterMetaId() == (ulong)targetId);
            if (tp != null)
            {
                tp.Health = 200;
                DeathHandler.Revive(tp);
                Alt.Emit("SaltyChat:SetPlayerAlive", tp, true);
                HUDHandler.SendNotification(player, 4, 7000, $"Du hast den Spieler {charName} wiederbelebt.");
                return;
            }
            HUDHandler.SendNotification(player, 4, 7000, $"Der Spieler {charName} ist nicht online.");
        }

        // Job Geben
        [Command("setjob")]
        public static void FactionCMD(IPlayer player, int charId, int id) //todo = , int factionrank)
        {
            try
            {
                if (player == null || !player.Exists || player.GetCharacterMetaId() <= 0) return;
                if (player.AdminLevel() <= 5) { HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); return; }
                if (ServerFactions.IsCharacterInAnyFaction(charId))
                {
                    ServerFactions.RemoveServerFactionMember(ServerFactions.GetCharacterFactionId(charId), charId);
                }

                ServerFactions.CreateServerFactionMember(id, charId, ServerFactions.GetFactionMaxRankCount(id), charId);
                HUDHandler.SendNotification(player, 4, 7000, $"Done.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Items Geben
        [Command("giveitem")]
        public static void GiveItemCMD(IPlayer player, int charId, string itemName, int itemAmount)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() <= 3) { HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); return; }
            itemName = itemName.Replace("_", " ");
            if (!ServerItems.ExistItem(ServerItems.ReturnNormalItemName(itemName))) { 
                HUDHandler.SendNotification(player, 4, 7000, $"Itemname nicht gefunden: {itemName}"); 
                return; 
            }
            foreach (IPlayer players in Alt.GetAllPlayers().ToList().Where(x => x != null && x.Exists && User.GetPlayerOnline(x) == charId))
            {
                if (charId <= 0) return;
                if (itemName.Contains("Fahrzeugschluessel"))
                {
                    CharactersInventory.AddCharacterItem((int)charId, itemName, itemAmount, "schluessel");
                    HUDHandler.SendNotification(player, 2, 7000, $"Gegenstand '{itemName}' ({itemAmount}x) erhalten.");
                    DiscordLog.SendEmbed("giveitem", "GiveItem Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {Characters.GetCharacterName(User.GetPlayerOnline(players))} {itemAmount}x {itemName} gegeben.");
                    return;
                }
                if (itemName.Contains("Generalschluessel"))
                {
                    CharactersInventory.AddCharacterItem((int)charId, itemName, itemAmount, "schluessel");
                    HUDHandler.SendNotification(player, 2, 7000, $"Gegenstand '{itemName}' ({itemAmount}x) erhalten.");
                    DiscordLog.SendEmbed("giveitem", "GiveItem Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {Characters.GetCharacterName(User.GetPlayerOnline(players))} {itemAmount}x {itemName} gegeben.");
                    return;
                }
                if (itemName.Contains("Hausschluessel"))
                {
                    CharactersInventory.AddCharacterItem((int)charId, itemName, itemAmount, "schluessel");
                    HUDHandler.SendNotification(player, 2, 7000, $"Gegenstand '{itemName}' ({itemAmount}x) erhalten.");
                    DiscordLog.SendEmbed("giveitem", "GiveItem Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {Characters.GetCharacterName(User.GetPlayerOnline(players))} {itemAmount}x {itemName} gegeben.");
                    return;
                }
                if (itemName.Contains("Handschellenschluessel"))
                {
                    CharactersInventory.AddCharacterItem((int)charId, itemName, itemAmount, "schluessel");
                    HUDHandler.SendNotification(player, 2, 7000, $"Gegenstand '{itemName}' ({itemAmount}x) erhalten.");
                    DiscordLog.SendEmbed("giveitem", "GiveItem Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {Characters.GetCharacterName(User.GetPlayerOnline(players))} {itemAmount}x {itemName} gegeben.");
                    return;
                }
                if (itemName.Contains("Bargeld"))
                {
                    CharactersInventory.AddCharacterItem((int)charId, itemName, itemAmount, "brieftasche");
                    HUDHandler.SendNotification(player, 2, 7000, $"Gegenstand '{itemName}' ({itemAmount}x) erhalten.");
                    DiscordLog.SendEmbed("giveitem", "GiveItem Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {Characters.GetCharacterName(User.GetPlayerOnline(players))} {itemAmount}x {itemName} gegeben.");
                    return;
                }
                if (itemName.Contains("EC Karte"))
                {
                    CharactersInventory.AddCharacterItem((int)charId, itemName, itemAmount, "brieftasche");
                    HUDHandler.SendNotification(player, 2, 7000, $"Gegenstand '{itemName}' ({itemAmount}x) erhalten.");
                    DiscordLog.SendEmbed("giveitem", "GiveItem Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {Characters.GetCharacterName(User.GetPlayerOnline(players))} {itemAmount}x {itemName} gegeben.");
                    return;
                }
                if (itemName.Contains("Ausweis"))
                {
                    CharactersInventory.AddCharacterItem((int)charId, itemName, itemAmount, "brieftasche");
                    HUDHandler.SendNotification(player, 2, 7000, $"Gegenstand '{itemName}' ({itemAmount}x) erhalten.");
                    DiscordLog.SendEmbed("giveitem", "GiveItem Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {Characters.GetCharacterName(User.GetPlayerOnline(players))} {itemAmount}x {itemName} gegeben.");
                    return;
                }
                else
                {
                    CharactersInventory.AddCharacterItem((int)charId, itemName, itemAmount, "inventory");
                    HUDHandler.SendNotification(player, 2, 7000, $"Gegenstand '{itemName}' ({itemAmount}x) erhalten.");
                    DiscordLog.SendEmbed("giveitem", "GiveItem Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {Characters.GetCharacterName(User.GetPlayerOnline(players))} {itemAmount}x {itemName} gegeben.");
                    return;
                }
            }
        }

        //Unjail player
        [Command("unjail")]
        public static void Unjail_CMD(ClassicPlayer player, int charId)
        {
            if (charId != 0 || !Characters.IsCharacterInJail(charId)) return;
            if (player.AdminLevel() <= 1) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return; 
            }
            var jailtime = Characters.GetCharacterJailTime(charId);
            foreach (IPlayer players in Alt.GetAllPlayers().ToList().Where(x => x != null && x.Exists && User.GetPlayerOnline(x) == charId))
            {
                Characters.SetCharacterJailTime(charId, false, 0);
                Characters.SetCharacterCorrectClothes(players);
                players.Position = new Position(-582.9099f, -146.74286f, 38.22705f);
            }
            foreach (ClassicPlayer playerss in Alt.GetAllPlayers().Where(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId == charId))
            {
                playerss.LastPosition = new Position(-582.9099f, -146.74286f, 38.22705f);
            }
            HUDHandler.SendNotification(player, 1, 7000, $"Du hast {Characters.GetCharacterName(charId)} vom Gefängniss entlassen. Resthaftzeit war {jailtime} Minuten.");
        }

        // Sich zu einen Spieler Telepotieren
        [Command("goto", false)]
        public static void GotoCMD(ClassicPlayer player, int targetId)
        {
            if (player.AdminLevel() <= 2) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return; 
            }
            try
            {
                if (player == null || !player.Exists) return;
                if (targetId <= 0 || targetId.ToString().Length <= 0)
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Benutzung: /goto charId");
                    return;
                }
                string targetCharName = Characters.GetCharacterName(targetId);
                if (targetCharName.Length <= 0)
                {
                    HUDHandler.SendNotification(player, 3, 7000, $"Warnung: Die angegebene Character-ID wurde nicht gefunden ({targetId}).");
                    return;
                }
                if (!Characters.ExistCharacterName(targetCharName))
                {
                    HUDHandler.SendNotification(player, 3, 7000, $"Warnung: Der angegebene Charaktername wurde nicht gefunden ({targetCharName} - ID: {targetId}).");
                    return;
                }
                var targetPlayer = Alt.GetAllPlayers().FirstOrDefault(x => x != null && x.Exists && x.GetCharacterMetaId() == (ulong)targetId);
                if (targetPlayer == null || !targetPlayer.Exists) { HUDHandler.SendNotification(player, 4, 7000, "Fehler: Spieler ist nicht online."); return; }
                HUDHandler.SendNotification(targetPlayer, 1, 7000, $"{Characters.GetCharacterName((int)player.GetCharacterMetaId())} hat sich zu dir teleportiert.");
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast dich zu dem Spieler {Characters.GetCharacterName((int)targetPlayer.GetCharacterMetaId())} teleportiert.");
                lock (player)
                {
                    if (player == null || !player.Exists) return;
                    player.Position = targetPlayer.Position + new Position(0, 0, 1);
                    player.LastPosition = targetPlayer.Position;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Spieler zu sich Telepotieren
        [Command("gethere", false)]
        public static void GetHereCMD(ClassicPlayer player, int targetId)
        {
            if (player.AdminLevel() <= 2) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte.");
                return; 
            }
            try
            {
                if (player == null || !player.Exists) return;
                if (targetId <= 0 || targetId.ToString().Length <= 0)
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Benutzung: /gethere charId");
                    return;
                }
                string targetCharName = Characters.GetCharacterName(targetId);
                if (targetCharName.Length <= 0)
                {
                    HUDHandler.SendNotification(player, 3, 7000, $"Warnung: Die angegebene Character-ID wurde nicht gefunden ({targetId}).");
                    return;
                }
                if (!Characters.ExistCharacterName(targetCharName))
                {
                    HUDHandler.SendNotification(player, 3, 7000, $"Warnung: Der angegebene Charaktername wurde nicht gefunden ({targetCharName} - ID: {targetId}).");
                    return;
                }
                var targetPlayer = (ClassicPlayer)Alt.GetAllPlayers().FirstOrDefault(x => x != null && x.Exists && x.GetCharacterMetaId() == (ulong)targetId);
                if (targetPlayer == null || !targetPlayer.Exists) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Spieler ist nicht online."); 
                    return; 
                }
                HUDHandler.SendNotification(targetPlayer, 1, 7000, $"{ Characters.GetCharacterName((int)player.GetCharacterMetaId())} hat dich zu Ihm teleportiert.");
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast den Spieler { Characters.GetCharacterName((int)targetPlayer.GetCharacterMetaId())} zu dir teleportiert.");
                lock (targetPlayer)
                {
                    targetPlayer.Position = player.Position + new Position(0, 0, 1);
                    targetPlayer.LastPosition = player.Position;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Positionen Speichern
        [Command("pos")]
        public static void PosCMD(IPlayer player, string coordName)
        {
            if (player.AdminLevel() <= 9) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return; 
            }
            if (!player.HasData("isAduty")) { 
                HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                return; 
            }
            if (coordName == null)
            {
                HUDHandler.SendNotification(player, 4, 7000, "Kein Namen angegeben!");
                return;
            }
            if (player.AdminLevel() <= 0)
            {
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte.");
                return;
            }

            Position playerPosGet = player.Position;
            Rotation playerRotGet = player.Rotation;

            HUDHandler.SendNotification(player, 4, 12500, $"{coordName}: {playerPosGet.ToString()} - {playerRotGet.ToString()}");
            StreamWriter coordsFile;
            if (!File.Exists("SavedCoords.txt"))
            {
                coordsFile = new StreamWriter("SavedCoords.txt");
            }
            else
            {
                coordsFile = File.AppendText("SavedCoords.txt");
            }
            HUDHandler.SendNotification(player, 4, 8000, "Die SavedCoords.txt datei wurde überarbeitet!");
            coordsFile.WriteLine("| " + coordName + " | " + "Saved Coordenates: " + playerPosGet.ToString() + " Saved Rotation: " + playerRotGet.ToString());
            coordsFile.Close();
        }

        //tphouse
        [Command("tph")]
        public static void TphouseCMD(IPlayer player, int houseid)
        {
            try
            {
                if (player.AdminLevel() <= 10) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                    return; 
                }
                if (!player.HasData("isAduty")) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                    return; 
                }
                if (houseid <= 0) return;
                var house = ServerHouses.ServerHouses_.FirstOrDefault(x => x.id == houseid);
                if (house != null)
                {
                    player.Position = new Position(house.entranceX, house.entranceY, house.entranceZ);
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast dich zum Haus mit der ID:{houseid} erfolgreich teleportiert.");
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        //houseprice
        [Command("hp")]
        public static void HpCMD(IPlayer player, int houseid, int price)
        {
            try
            {
                if (player.AdminLevel() <= 10) {
                    HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte.");
                    return; 
                }
                if (!player.HasData("isAduty")) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                    return; 
                }
                if (houseid <= 0) return;
                var house = ServerHouses.ServerHouses_.FirstOrDefault(x => x.id == houseid);
                if (house != null)
                {
                    house.price = price;
                    using (gtaContext db = new gtaContext())
                    {
                        db.Server_Houses.Update(house);
                        db.SaveChanges();
                    }
                    HUDHandler.SendNotification(player, 2, 7000, $"Das Haus mit der ID: {houseid} kostet jetzt {price}$.");
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        //SetHouse Garage
        [Command("setg")]
        public static void CMD_sHGarage(IPlayer player, int houseId, int slots)
        {
            try
            {
                if (player.AdminLevel() <= 11) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                    return; 
                }
                if (!player.HasData("isAduty")) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                    return; 
                }
                if (houseId <= 0) return;
                var house = ServerHouses.ServerHouses_.FirstOrDefault(x => x.id == houseId);
                if (house != null)
                {
                    house.garagePos = new Vector3(player.Position.X, player.Position.Y, player.Position.Z);
                    house.garageSlots = slots;
                    using (gtaContext db = new gtaContext())
                    {
                        db.Server_Houses.Update(house);
                        db.SaveChanges();
                    }
                    HUDHandler.SendNotification(player, 2, 7000, "Haus - Garagenverwaltungspunkt gesetzt.");
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        //SetHouse GarageOut
        [Command("seta")]
        public static void CMD_sHGarageOut(IPlayer player, int houseId)
        {
            try
            {
                if (player.AdminLevel() <= 11) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                    return; 
                }
                if (!player.HasData("isAduty")) {
                    HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst.");
                    return; 
                }
                if (houseId <= 0) return;
                foreach (IVehicle veh in Alt.GetAllVehicles())
                {
                    if (veh.Position.Distance(player.Position) <= 5 && player.IsInVehicle)
                    {
                        var house = ServerHouses.ServerHouses_.FirstOrDefault(x => x.id == houseId);
                        if (house != null)
                        {
                            house.garageVehPos = new Vector3(veh.Position.X, veh.Position.Y, veh.Position.Z);
                            house.garageVehRot = new Vector3(veh.Rotation.Roll, veh.Rotation.Pitch, veh.Rotation.Yaw);
                            using (gtaContext db = new gtaContext())
                            {
                                db.Server_Houses.Update(house);
                                db.SaveChanges();
                            }

                            HUDHandler.SendNotification(player, 2, 7000, "Haus - HouseGarageParkOut gesetzt.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Auto Hash Speichern
        [Command("Hash")]
        public static void HashCMD(IPlayer player, string car)
        {
            if (player.AdminLevel() <= 9) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return; 
            }
            if (!player.HasData("isAduty")) { 
                HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                return; 
            }
            if (car == null)
            {
                HUDHandler.SendNotification(player, 4, 7000, "Kein Namen angegeben!");
                return;
            }
            if (player.AdminLevel() <= 0)
            {
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte.");
                return;
            }
            uint hashedCar = Alt.Hash(car);
            HUDHandler.SendNotification(player, 2, 20000, $"Fahrzeugname: {car} || Fahrzeughash: {hashedCar}");
            StreamWriter hashFile;
            if (!File.Exists("SavedHash.txt"))
            {
                hashFile = new StreamWriter("SavedHash.txt");
            }
            else
            {
                hashFile = File.AppendText("SavedHash.txt");
            }
            HUDHandler.SendNotification(player, 4, 8000, "Die SavedHash.txt datei wurde überarbeitet!");
            hashFile.WriteLine("| " + car + " | " + "Saved hash: " + hashedCar);
            hashFile.Close();
        }

        // Spieler aus der Dimension Holen      /setdim PLAYERID 0
        [Command("setdim")]
        public static void SetDimensionCMD(IPlayer player, int charId, int dimension)
        {
            if (player == null || !player.Exists || charId == 0) return;
            if (player.AdminLevel() <= 2) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return; 
            }
            var targetPlayer = Alt.GetAllPlayers().FirstOrDefault(x => (int)x.GetCharacterMetaId() == charId);
            if (targetPlayer != null)
            {
                targetPlayer.Dimension = dimension;
                targetPlayer.SetStreamSyncedMetaData("dimension", targetPlayer.Dimension);
                Alt.Emit("SaltyChat:PlayerChangeDimension", targetPlayer);
                HUDHandler.SendNotification(targetPlayer, 4, 8000, $"Der Spieler {Characters.GetCharacterName(User.GetPlayerOnline(player))} Hat Dich In Die Dimension: {dimension} Gesetzt");
                HUDHandler.SendNotification(player, 2, 8000, $"Der Spieler {Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} Ist Jetzt In Der Dimension: {dimension}");
            }
        }

        // Admin Anzug
        [Command("am", true)]
        public static void AdutyCMD(IPlayer player)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() <= 6) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return; 
            }
            if (player.HasData("isAduty"))
            {
                player.DeleteData("isAduty");
                if (ServerFactions.IsCharacterInAnyFaction((int)player.GetCharacterMetaId()) && ServerFactions.IsCharacterInFactionDuty((int)player.GetCharacterMetaId()))
                {
                    ServerFactions.SetCharactersFactionClothes((ClassicPlayer)player, (int)player.GetCharacterMetaId());
                    HUDHandler.SendNotification(player, 4, 7000, $"Du bist nun nicht mehr als Admin unterwegs. (Commands Deaktiviert)");
                }
                else
                {
                    Characters.SetCharacterCorrectClothes(player);
                    HUDHandler.SendNotification(player, 4, 7000, $"Du bist nun nicht mehr als Admin unterwegs. (Commands Deaktiviert)");
                }
            }
            else
            {
                player.SetData("isAduty", true);
                int componentColor = 0;

                switch (player.AdminLevel())
                {
                    case 2: //Supporter
                        componentColor = 11;
                        break;
                    case 3: //Moderator
                        componentColor = 10;
                        break;
                    case 4: //Administrator
                        componentColor = 8;
                        break;
                    case 5: //Fraktionleitung
                        componentColor = 7;
                        break;
                    case 6: //Communitymanagement
                        componentColor = 6;
                        break;
                    case 7: //Teamleitung
                        componentColor = 5;
                        break;
                    case 8: //
                        componentColor = 4;
                        break;
                    case 9: //
                        componentColor = 3;
                        break;
                    case 10: //Stv. Projektleitung
                        componentColor = 2;
                        break;
                    case 11: //Projektleitung
                        componentColor = 0;
                        break;
                }

                if (!Characters.GetCharacterGender((int)player.GetCharacterMetaId()))
                {
                    //MÃ¤nnlich
                    player.SetClothes(1, 135, (byte)componentColor, 0);
                    player.SetClothes(4, 114, (byte)componentColor, 0);
                    player.SetClothes(6, 78, (byte)componentColor, 0);
                    player.SetClothes(3, 3, (byte)componentColor, 0);
                    player.SetClothes(11, 287, (byte)componentColor, 0);
                }
                else
                {
                    //Weiblich
                    player.SetClothes(1, 135, (byte)componentColor, 0);
                    player.SetClothes(11, 300, (byte)componentColor, 0);
                    player.SetClothes(4, 121, (byte)componentColor, 0);
                    player.SetClothes(3, 8, (byte)componentColor, 0);
                    player.SetClothes(6, 82, (byte)componentColor, 0);
                    player.SetClothes(2, 0, 0, 2);
                }
                HUDHandler.SendNotification(player, 4, 7000, $"Du bist nun als Admin unterwegs. (Commands aktiviert)");
            }
        }

        [Command("settshopowner")]
        public static void CMD_tattooshopowner(IPlayer player, int shopId, int ownerid)
        {
            try
            {
                if (player.AdminLevel() <= 11) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                    return; 
                }
                if (!player.HasData("isAduty")) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst."); 
                    return; 
                }
                if (shopId <= 0) return;
                var tattooshop = ServerTattooShops.ServerTattooShops_.FirstOrDefault(x => x.id == shopId);
                if (tattooshop != null)
                {
                    tattooshop.owner = ownerid;
                    using (gtaContext db = new gtaContext())
                    {
                        db.Server_Tattoo_Shops.Update(tattooshop);
                        db.SaveChanges();
                    }
                    HUDHandler.SendNotification(player, 2, 10000, $"Neuen Besitzer {Characters.GetCharacterName(ownerid)} für den Tattooladen {ServerTattooShops.GetTattooShopName(shopId)} gesetzt.");
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        // Clothestorage setzen
        [Command("setCS")]
        public static void SetClothesStorageCMD(IPlayer player, int faction)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() <= 10) { 
                HUDHandler.SendNotification(player, 4, 7000, "Keine Rechte."); 
                return;
            }
            if (!player.HasData("isAduty")) { 
                HUDHandler.SendNotification(player, 4, 7000, "Nicht im (/am) Admindienst.");
                return; 
            }
            ulong charId = player.GetCharacterMetaId();
            if (charId <= 0) return;
            ServerClothesStorages.CreateNewClotheStorage(player, player.Position, faction);
            HUDHandler.SendNotification(player, 2, 7000, $"Clothe Storage erfolgreich an der Position {player.Position} gesetzt.");
        }

        // Scenario
        [Command("play")]
        public static void PlayCMD(ClassicPlayer player, string Scenario)
        {
            if (player == null || !player.Exists || Scenario == "Null" || player.AdminLevel() <= 4) return;
            player.EmitLocked("Client:Animation:PlayScenario", Scenario, 0);
        }

        [Command("forceduty")]
        public static void forceDuty(ClassicPlayer player)
        {
            if (player == null || !player.Exists || player.AdminLevel() < 9 || !ServerFactions.IsCharacterInAnyFaction((int)player.GetCharacterMetaId())) return;
            ServerFactions.SetCharacterInFactionDuty((int)player.GetCharacterMetaId(), true);
        }

        [Command("setMaxCharacters")]
        public static void SetMaxCharactersCMD(ClassicPlayer player, int AccId, int MaxAmount)
        {
            if (player.Exists == false || player.AdminLevel() <= 11 || AccId == 0 || MaxAmount == 0) return;
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            var AccDatas = User.Player.ToList().FirstOrDefault(x => x.playerid == AccId);
            if (AccDatas != null)
            {
                AccDatas.MaxCharacters = MaxAmount;
                using (var db = new gtaContext())
                {
                    db.Accounts.Update(AccDatas);
                    db.SaveChanges();
                }
                HUDHandler.SendNotification(player, 4, 5555, "Done setMaxCharacters");
            }
            else return;
        }

        // setPed 1 hc_gunman
        [Command("setPed")]
        public static void SetPed(ClassicPlayer player, int CharacterID, string PedHash)
        {
            if (!player.Exists || CharacterID <= 0 || player.AdminLevel() <= 8) return;
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            ClassicPlayer tplayer = (ClassicPlayer)Alt.GetAllPlayers().ToList().FirstOrDefault(x => ((ClassicPlayer)x).CharacterId == CharacterID);
            var Chardatas = Characters.PlayerCharacters.ToList().FirstOrDefault(x => x.charId == CharacterID);
            if (Chardatas != null)
            {
                if (PedHash == "None")
                {
                    if (tplayer != null)
                    {
                        if (!Characters.GetCharacterGender((int)player.GetCharacterMetaId())) tplayer.Model = Alt.Hash("mp_m_freemode_01");
                        else tplayer.Model = Alt.Hash("mp_f_freemode_01");
                        Characters.SetCharacterSkin(tplayer);
                        Characters.SetCharacterCorrectClothes(tplayer, true);
                    }
                    Chardatas.isPed = false;
                    Chardatas.PedHash = 0;
                    using (var db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(Chardatas);
                        db.SaveChanges();
                    }
                    HUDHandler.SendNotification(player, 4, 5555, "Done PED - RESET");
                    return;
                }
                else
                {
                    if (tplayer != null)
                    {
                        tplayer.Model = Alt.Hash(PedHash);
                    }
                    Chardatas.isPed = true;
                    Chardatas.PedHash = Alt.Hash(PedHash);
                    using (var db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(Chardatas);
                        db.SaveChanges();
                    }
                    HUDHandler.SendNotification(player, 4, 5555, "Done PED - SET");
                }
            }
            else return;
        }
    }
}
