using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.models;
using Altv_Roleplay.Services;
using Altv_Roleplay.Utils;
using Newtonsoft.Json;

namespace Altv_Roleplay.Handler
{
    class ShopHandler : IScript
    {

        #region Shops
        [AsyncClientEvent("Server:Shop:buyItem")]
        public static void BuyShopItem(IPlayer player, int shopId, int amount, string itemname)
        {
            if (player == null || !player.Exists || shopId <= 0 || amount <= 0 || itemname == "") return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                return; 
            }
            if (!player.Position.IsInRange(ServerShops.GetShopPosition(shopId), 3f)) { 
                HUDHandler.SendNotification(player, 3, 7000, $"Du bist zu weit vom Shop entfernt.");
                return; 
            }
            int charId = User.GetPlayerOnline(player);
            if (charId == 0) return;
            if (ServerShops.GetShopNeededLicense(shopId) != "None" && !Characters.HasCharacterPermission(charId, ServerShops.GetShopNeededLicense(shopId))) { 
                HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf."); 
                return; 
            }
            float itemWeight = ServerItems.GetItemWeight(itemname) * amount;
            float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
            float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
            int itemPrice = ServerShopsItems.GetShopItemPrice(shopId, itemname) * amount;
            int shopFaction = ServerShops.GetShopFaction(shopId);
            if (ServerShopsItems.GetShopItemAmount(shopId, itemname) < amount) {
                HUDHandler.SendNotification(player, 3, 7000, $"Soviele Gegenstände hat der Shop nicht auf Lager."); 
                return; 
            }
            if (invWeight + itemWeight > 15f && backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId))) { 
                HUDHandler.SendNotification(player, 3, 7000, $"Du hast nicht genug Platz in deinen Taschen."); 
                return; 
            }
            if (shopFaction > 0 && shopFaction != 0)
            {
                if (!ServerFactions.IsCharacterInAnyFaction(charId)) { HUDHandler.SendNotification(player, 3, 7000, "Du hast hier keinen Zugriff drauf [CODE1-2]."); 
                    return; 
                }
                if (ServerFactions.GetCharacterFactionId(charId) != shopFaction) { 
                    HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf (Gefordert: {shopFaction} - Deine: {ServerFactions.GetCharacterFactionId(charId)}."); 
                    return; 
                }
                if (ServerFactions.GetFactionBankMoney(shopFaction) < itemPrice) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Die Frakton hat nicht genügend Geld auf dem Fraktionskonto."); 
                    return; 
                }
                ServerFactions.SetFactionBankMoney(shopFaction, ServerFactions.GetFactionBankMoney(shopFaction) - itemPrice);
                LoggingService.NewFactionLog(shopFaction, charId, 0, "shop", $"{Characters.GetCharacterName(charId)} hat {itemname} ({amount}x) für {itemPrice}$ erworben.");
            }
            else
            {
                if (!CharactersInventory.ExistCharacterItem(charId, "Bargeld", "brieftasche") || CharactersInventory.GetCharacterItemAmount(charId, "Bargeld", "brieftasche") < itemPrice)
                {
                    HUDHandler.SendNotification(player, 3, 7000, "Du hast nicht genügend Geld dabei.");
                    return;
                }
                CharactersInventory.RemoveCharacterItemAmount(charId, "Bargeld", itemPrice, "brieftasche");
            }

            if (itemname.Contains("Fahrzeugschluessel"))
            {
                CharactersInventory.AddCharacterItem(charId, itemname, amount, "schluessel");
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemname} für {itemPrice} gekauft.");
                stopwatch.Stop();
                return;
            }

            if (itemname.Contains("Generalschluessel"))
            {
                CharactersInventory.AddCharacterItem(charId, itemname, amount, "schluessel");
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemname} für {itemPrice} gekauft.");
                stopwatch.Stop();
                return;
            }

            if (itemname.Contains("Handschellenschluessel"))
            {
                CharactersInventory.AddCharacterItem(charId, itemname, amount, "schluessel");
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemname} für {itemPrice} gekauft.");
                stopwatch.Stop();
                return;
            }

            if (itemname.Contains("Hausschluessel"))
            {
                CharactersInventory.AddCharacterItem(charId, itemname, amount, "schluessel");
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemname} für {itemPrice} gekauft.");
                stopwatch.Stop();
                return;
            }

            if (invWeight + itemWeight <= 15f)
            {
                CharactersInventory.AddCharacterItem(charId, itemname, amount, "inventory");
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemname} für {itemPrice} gekauft.");
                stopwatch.Stop();
                return;
            }

            if (Characters.GetCharacterBackpack(charId) != "None" && backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
            {
                CharactersInventory.AddCharacterItem(charId, itemname, amount, "backpack");
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemname} für {itemPrice} gekauft.");
                stopwatch.Stop();
                return;
            }
        }

        public static async void RobShopAsync(ClassicPlayer player, int shopId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || shopId <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                if (!CharactersInventory.ExistCharacterItem(player.CharacterId, "Schweissgerät", "inventory")) return;
                if (player.isRobbingAShop)
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Du raubst bereits einen Shop aus.");
                    return;
                }

                if (ServerFactions.GetFactionDutyMemberCount(2) < 2)
                {
                    HUDHandler.SendNotification(player, 3, 7000, "Es sind nicht genügend Beamte im Dienst (2).");
                    return;
                }

                foreach (var inni in ServerShops.ServerShops_.Where(x => x.shopId == shopId))
                {
                    if (DateTime.Now.Subtract(inni.date).TotalHours <= 1 && inni.isRobbed == true)
                    {
                        HUDHandler.SendNotification(player, 3, 7000, "Dieser Shop wurde erst vor kurzem ausgeraubt.");
                        return;
                    }
                    if (DateTime.Now.Subtract(inni.date).TotalHours >= 1)
                    {
                        inni.isRobbed = false;
                    }
                    if (ServerShops.IsShopRobbedNow(shopId))
                    {
                        HUDHandler.SendNotification(player, 3, 7000, "Dieser Shop wird bereits ausgeraubt.");
                        return;
                    }

                    inni.isRobbed = true;
                    inni.date = DateTime.Now;
                }

                ServerFactions.AddNewFactionDispatch(0, 2, $"Aktiver Shopraub", player.Position);

                foreach (var p in Alt.GetAllPlayers().Where(x => x != null && x.Exists && x.GetCharacterMetaId() > 0).ToList())
                {
                    if (!ServerFactions.IsCharacterInAnyFaction((int)p.GetCharacterMetaId()) || !ServerFactions.IsCharacterInFactionDuty((int)p.GetCharacterMetaId()) || ServerFactions.GetCharacterFactionId((int)p.GetCharacterMetaId()) != 2 && ServerFactions.GetCharacterFactionId((int)p.GetCharacterMetaId()) != 12) continue;
                    HUDHandler.SendNotification(p, 1, 9500, "Ein stiller Alarm wurde ausgelöst.");
                }

                ServerShops.SetShopRobbedNow(shopId, true);
                player.isRobbingAShop = true;
                HUDHandler.SendNotification(player, 1, 180000, "Du raubst nun diesen Laden aus!");
                Extensions.createProgress(player, 1800);
                player.EmitLocked("Client:Inventory:PlayAnimation", "anim@amb@clubhouse@tutorial@bkr_tut_ig3@", "machinic_loop_mechandplayer", 180000, 1, false);
                await Task.Delay(180000);
                ServerShops.SetShopRobbedNow(shopId, false);
                if (player == null || !player.Exists) return;
                player.isRobbingAShop = false;
                player.EmitLocked("Client:Inventory:StopAnimation");
                int amount = new Random().Next(800, 2345);
                int amount2 = new Random().Next(1, 6);
                var nameam = ServerShops.ServerShops_.FirstOrDefault(x => x.shopId == shopId);
                if (nameam.name.Contains("Ammunation")) {

                        CharactersInventory.AddCharacterItem(player.CharacterId, "Pistolen Magazin", amount2, "inventory");
                        HUDHandler.SendNotification(player, 2, 7000, $"Shop ausgeraubt, Du erhälst {amount2} Pistolen Magazine.");
                } else if (!nameam.name.Contains("Ammunation")) {

                    CharactersInventory.AddCharacterItem(player.CharacterId, "Bargeld", amount, "brieftasche");
                    HUDHandler.SendNotification(player, 2, 7000, $"Shop ausgeraubt, Du erhälst ${amount}.");
                }
                player.EmitLocked("Client:HUD:setCefStatus", false);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        internal static void OpenShop(IPlayer player, Server_Shops shopPos)
        {
            try
            {
                if (player == null || !player.Exists) return;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;

                if (shopPos.faction > 0 && shopPos.faction != 0)
                {
                    if (!ServerFactions.IsCharacterInAnyFaction(charId)) { HUDHandler.SendNotification(player, 3, 2500, "Kein Zugriff [1]"); return; }
                    if (ServerFactions.GetCharacterFactionId(charId) != shopPos.faction) {
                        HUDHandler.SendNotification(player, 3, 7000, $"Kein Zugriff [{shopPos.faction} - {ServerFactions.GetCharacterFactionId(charId)}]"); 
                        return; 
                    }
                }

                if (shopPos.neededLicense != "None" && !Characters.HasCharacterPermission(charId, shopPos.neededLicense)) {
                    HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf.");
                    stopwatch.Stop();
                    return;
                }

                if (shopPos.isOnlySelling == false) {
                    Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "Client:Shop:shopCEFCreateCEF", ServerShopsItems.GetShopShopItems(shopPos.shopId), shopPos.shopId, shopPos.isOnlySelling);
                    stopwatch.Stop();
                    return;
                }
                else if (shopPos.isOnlySelling == true) {
                    Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "Client:Shop:shopCEFCreateCEF", ServerShopsItems.GetShopSellItems(charId, shopPos.shopId), shopPos.shopId, shopPos.isOnlySelling);
                    stopwatch.Stop();
                    return;
                }
                stopwatch.Stop();
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Shop:sellItem")]
        public static void SellShopItem(IPlayer player, int shopId, int amount, string itemname)
        {
            if (player == null || !player.Exists || shopId <= 0 || amount <= 0 || itemname == "") return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?");
                return; 
            }
            if (!player.Position.IsInRange(ServerShops.GetShopPosition(shopId), 3f)) { 
                HUDHandler.SendNotification(player, 3, 7000, "Du bist zu weit entfernt."); 
                return; 
            }
            int charId = User.GetPlayerOnline(player);
            if (charId == 0) return;
            if (ServerShops.GetShopNeededLicense(shopId) != "None" && !Characters.HasCharacterPermission(charId, ServerShops.GetShopNeededLicense(shopId))) { 
                HUDHandler.SendNotification(player, 3, 7000, "Du hast hier keinen Zugriff drauf.");
                return; 
            }
            if (!CharactersInventory.ExistCharacterItem(charId, itemname, "inventory") && !CharactersInventory.ExistCharacterItem(charId, itemname, "backpack")) { 
                HUDHandler.SendNotification(player, 3, 7000, "Diesen Gegenstand besitzt du nicht."); 
                return; 
            }
            int itemSellPrice = ServerShopsItems.GetShopItemPrice(shopId, itemname); //Verkaufpreis pro Item
            int invItemAmount = CharactersInventory.GetCharacterItemAmount(charId, itemname, "inventory"); //Anzahl an Items im Inventar
            int backpackItemAmount = CharactersInventory.GetCharacterItemAmount(charId, itemname, "backpack"); //Anzahl an Items im Rucksack
            if (invItemAmount + backpackItemAmount < amount) {
                HUDHandler.SendNotification(player, 3, 7000, "Soviele Gegenstände hast du nicht zum Verkauf dabei.");
                return; 
            }


            var removeFromInventory = Math.Min(amount, invItemAmount);
            if (removeFromInventory > 0)
            {
                CharactersInventory.RemoveCharacterItemAmount(charId, itemname, removeFromInventory, "inventory");
            }

            var itemsLeft = amount - removeFromInventory;
            if (itemsLeft > 0)
            {
                CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemsLeft, "backpack");
            }

            HUDHandler.SendNotification(player, 2, 7000, $"Du hast {amount}x {itemname} für {itemSellPrice * amount}$ verkauft.");
            CharactersInventory.AddCharacterItem(charId, "Bargeld", amount * itemSellPrice, "brieftasche");
            stopwatch.Stop();
        }
        #endregion

        #region VehicleShop

        internal static void OpenVehicleShop(IPlayer player, string shopname, int shopId)
        {
            if (player == null || !player.Exists || shopId <= 0) return;
            var array = ServerVehicleShops.GetVehicleShopItems(shopId);
            player.EmitLocked("Client:VehicleShop:OpenCEF", shopId, shopname, array);
        }

        internal static void SellVehicle(IPlayer player)
        {
            IVehicle veh = player.Vehicle;
            ClassicPlayer p = player as ClassicPlayer;
            if (ServerVehicles.GetVehicleType(veh) == 4) return;
            if (player == null && !player.Exists && player.GetCharacterMetaId() == 0 && Characters.GetCharacterAccountId((int)player.GetCharacterMetaId()) == 0 && player.IsInVehicle == false) return;
            if (veh == null && !veh.Exists) return;
            var hash = ServerVehicles.GetVehicleHashById((int)veh.GetVehicleId());
            ulong fHash = Convert.ToUInt64(hash);
            int price = ServerVehicleShops.GetVehicleShopPrice2(hash) / 100 * 70;
            if (p == null || ServerVehicles.GetVehicleOwner(veh) != p.CharacterId) { 
                HUDHandler.SendNotification(player, 4, 7500, $"Dies ist nicht dein Fahrzeug!"); 
                return; 
            }
            CharactersInventory.AddCharacterItem((int)player.GetCharacterMetaId(), "Bargeld", price, "inventory");
            HUDHandler.SendNotification(player, 2, 7500, $"Du hast das Fahrzeug {ServerVehicles.GetVehicleNameOnHash(hash)} verkauft für: <br> Summe: {price}$ <br>");
            CharactersInventory.RemoveCharacterItem2((int)player.GetCharacterMetaId(), $"Fahrzeugschluessel {veh.NumberplateText}");
            ServerVehicles.RemoveVehiclePermanently(veh);
            Alt.RemoveVehicle(veh);
        }

        [AsyncClientEvent("Server:VehicleShop:BuyVehicle")]
        public void BuyVehicle(IPlayer player, int shopid, string hash)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (player == null || !player.Exists || shopid <= 0 || hash == "") return;
                long fHash = Convert.ToInt64(hash);
                int charId = User.GetPlayerOnline(player);
                if (charId == 0 || fHash == 0) return;
                int Price = ServerVehicleShops.GetVehicleShopPrice(shopid, fHash);
                bool PlaceFree = true;
                Position ParkOut = ServerVehicleShops.GetVehicleShopOutPosition(shopid);
                Rotation RotOut = ServerVehicleShops.GetVehicleShopOutRotation(shopid);
                foreach (IVehicle veh in Alt.GetAllVehicles().ToList()) { if (veh.Position.IsInRange(ParkOut, 2f)) { PlaceFree = false; break; } }
                if (!PlaceFree) { HUDHandler.SendNotification(player, 3, 7000, $"Der Ausladepunkt ist belegt.");
                    return; 
                }
                int rnd = new Random().Next(100000, 999999);
                if (ServerVehicles.ExistServerVehiclePlate($"NL{rnd}")) { BuyVehicle(player, shopid, hash); return; }
                if (shopid == 1 || shopid == 2)
                {
                    if(ServerFactions.GetCharacterFactionId(charId) == 2 && ServerFactions.GetCharacterFactionRank(charId) <= ServerFactions.GetFactionMaxRankCount(2) && ServerFactions.GetCharacterFactionRank(charId) <= 13) { 
                        return; 
                    }
                    var factionmoney = ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId));
                    if (factionmoney < Price) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Ihre Fraktion hat nicht genügend Geld!");
                        return; 
                    }
                    ServerFactions.SetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId), (ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId)) - Price));
                    int vehClass = ServerAllVehicles.GetVehicleClass(fHash);
                    if (vehClass == 0) { ServerVehicles.CreateVehicle(fHash, charId, 0, 2, false, 11, ParkOut, RotOut, $"LSPD", 255, 255, 255, false); }
                    else if (vehClass == 3) { ServerVehicles.CreateVehicle(fHash, charId, 0, 2, false, 28, ParkOut, RotOut, $"LSPD", 255, 255, 255, false); }
                    CharactersInventory.AddCharacterItem(charId, $"Fahrzeugschluessel LSPD", 2, "schluessel");
                    HUDHandler.SendNotification(player, 1, 7000, "Die Kosten für dieses Fahrzeug wurden von Ihrer Fraktionskasser übernommen.");
                }
                else if (shopid == 3 || shopid == 4)
                {
                    if (ServerFactions.GetCharacterFactionId(charId) == 3 && ServerFactions.GetCharacterFactionRank(charId) <= ServerFactions.GetFactionMaxRankCount(3) && ServerFactions.GetCharacterFactionRank(charId) <= 11) { 
                        return; 
                    }
                    var factionmoney = ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId));
                    if (factionmoney < Price) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Ihre Fraktion hat nicht genügend Geld!"); 
                        return; 
                    }
                    ServerFactions.SetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId), (ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId)) - Price));
                    int vehClass = ServerAllVehicles.GetVehicleClass(fHash);
                    if (vehClass == 0) { ServerVehicles.CreateVehicle(fHash, charId, 0, 3, false, 11, ParkOut, RotOut, $"LSMD", 255, 255, 255, false); }
                    else if (vehClass == 3) { ServerVehicles.CreateVehicle(fHash, charId, 0, 3, false, 28, ParkOut, RotOut, $"LSMD", 255, 255, 255, false); }
                    CharactersInventory.AddCharacterItem(charId, $"Fahrzeugschluessel LSMD", 2, "schluessel");
                    HUDHandler.SendNotification(player, 1, 7000, "Die Kosten für dieses Fahrzeug wurden von Ihrer Fraktionskasser übernommen.");
                }
                else if (shopid == 5)
                {
                    if (ServerFactions.GetCharacterFactionId(charId) == 4 && ServerFactions.GetCharacterFactionRank(charId) <= ServerFactions.GetFactionMaxRankCount(4) && ServerFactions.GetCharacterFactionRank(charId) <= 11) { 
                        return; 
                    }
                    var factionmoney = ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId));
                    if (factionmoney < Price) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Ihre Fraktion hat nicht genügend Geld!");
                        return; 
                    }
                    ServerFactions.SetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId), (ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId)) - Price));
                    int vehClass = ServerAllVehicles.GetVehicleClass(fHash);
                    if (vehClass == 0) { ServerVehicles.CreateVehicle(fHash, charId, 0, 4, false, 11, ParkOut, RotOut, $"BNY", 255, 255, 255, false); }
                    else if (vehClass == 3) { ServerVehicles.CreateVehicle(fHash, charId, 0, 4, false, 28, ParkOut, RotOut, $"BNY", 255, 255, 255, false); }
                    CharactersInventory.AddCharacterItem(charId, $"Fahrzeugschluessel BNY", 2, "schluessel");
                    HUDHandler.SendNotification(player, 1, 7000, "Die Kosten für dieses Fahrzeug wurden von Ihrer Fraktionskasser übernommen.");
                }
                else if (shopid == 6)
                {
                    if (ServerFactions.GetCharacterFactionId(charId) == 5 && ServerFactions.GetCharacterFactionRank(charId) <= ServerFactions.GetFactionMaxRankCount(5) && ServerFactions.GetCharacterFactionRank(charId) <= 11) { 
                        return; 
                    }
                    var factionmoney = ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId));
                    if (factionmoney < Price) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Ihre Fraktion hat nicht genügend Geld!"); 
                        return; 
                    }
                    ServerFactions.SetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId), (ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId)) - Price));
                    int vehClass = ServerAllVehicles.GetVehicleClass(fHash);
                    if (vehClass == 0) { ServerVehicles.CreateVehicle(fHash, charId, 0, 5, false, 11, ParkOut, RotOut, $"DMV", 255, 255, 255, false); }
                    else if (vehClass == 3) { ServerVehicles.CreateVehicle(fHash, charId, 0, 5, false, 28, ParkOut, RotOut, $"DMV", 255, 255, 255, false); }
                    CharactersInventory.AddCharacterItem(charId, $"Fahrzeugschluessel DMV", 2, "schluessel");
                    HUDHandler.SendNotification(player, 1, 7000, "Die Kosten für dieses Fahrzeug wurden von Ihrer Fraktionskasser übernommen.");
                }
                else if (shopid == 7)
                {
                    if(ServerFactions.GetCharacterFactionId(charId) == 6 && ServerFactions.GetCharacterFactionRank(charId) <= ServerFactions.GetFactionMaxRankCount(6) && ServerFactions.GetCharacterFactionRank(charId) <= 11) {
                        return; 
                    }
                    var factionmoney = ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId));
                    if (factionmoney < Price) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Ihre Fraktion hat nicht genügend Geld!"); 
                        return; 
                    }
                    ServerFactions.SetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId), (ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId)) - Price));
                    int vehClass = ServerAllVehicles.GetVehicleClass(fHash);
                    if (vehClass == 0) { ServerVehicles.CreateVehicle(fHash, charId, 0, 6, false, 11, ParkOut, RotOut, $"VUC", 255, 255, 255, false); }
                    else if (vehClass == 3) { ServerVehicles.CreateVehicle(fHash, charId, 0, 6, false, 28, ParkOut, RotOut, $"VUC", 255, 255, 255, false); }
                    CharactersInventory.AddCharacterItem(charId, $"Fahrzeugschluessel VUC", 2, "schluessel");
                    HUDHandler.SendNotification(player, 1, 7000, "Die Kosten für dieses Fahrzeug wurden von Ihrer Fraktionskasser übernommen.");
                }
                else if (shopid == 8 || shopid == 9)
                {
                    if(ServerFactions.GetCharacterFactionId(charId) == 1 && ServerFactions.GetCharacterFactionRank(charId) <= ServerFactions.GetFactionMaxRankCount(1) && ServerFactions.GetCharacterFactionRank(charId) <= 11) {
                        return; 
                    }
                    var factionmoney = ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId));
                    if (factionmoney < Price) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Ihre Fraktion hat nicht genügend Geld!"); 
                        return; 
                    }
                    ServerFactions.SetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId), (ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId)) - Price));
                    int vehClass = ServerAllVehicles.GetVehicleClass(fHash);
                    if (vehClass == 0) { ServerVehicles.CreateVehicle(fHash, charId, 0, 1, false, 11, ParkOut, RotOut, $"DOJ", 255, 255, 255, false); }
                    else if (vehClass == 3) { ServerVehicles.CreateVehicle(fHash, charId, 0, 1, false, 28, ParkOut, RotOut, $"DOJ", 255, 255, 255, false); }
                    CharactersInventory.AddCharacterItem(charId, $"Fahrzeugschluessel DOJ", 2, "schluessel");
                    HUDHandler.SendNotification(player, 1, 7000, "Die Kosten für dieses Fahrzeug wurden von Ihrer Fraktionskasser übernommen.");
                }
                else
                {
                    if (!CharactersInventory.ExistCharacterItem(charId, "Bargeld", "brieftasche") || CharactersInventory.GetCharacterItemAmount(charId, "Bargeld", "brieftasche") < Price) { 
                        HUDHandler.SendNotification(player, 4, 7000, $"Du hast nicht genügend Bargeld dabei ({Price}$)."); 
                        return; 
                    }
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Bargeld", Price, "brieftasche");
                    int vehClass = ServerAllVehicles.GetVehicleClass(fHash);
                    if (vehClass == 0) { ServerVehicles.CreateVehicle(fHash, charId, 0, 0, false, 11, ParkOut, RotOut, $"NL{rnd}", 255, 255, 255, false); }
                    else if (vehClass == 3) { ServerVehicles.CreateVehicle(fHash, charId, 0, 0, false, 28, ParkOut, RotOut, $"NL{rnd}", 255, 255, 255, false); }
                    CharactersInventory.AddCharacterItem(charId, $"Fahrzeugschluessel NL{rnd}", 2, "schluessel");
                    CharactersInventory.AddCharacterItem(charId, $"Kaufvertrag {DateTime.Now.ToString("dd.MM.yyyy")}", 1, "inventory");
                    HUDHandler.SendNotification(player, 2, 7000, $"Fahrzeug erfolgreich gekauft.");
                }
                if (!CharactersTablet.HasCharacterTutorialEntryFinished(charId, "buyVehicle"))
                {
                    CharactersTablet.SetCharacterTutorialEntryState(charId, "buyVehicle", true);
                    HUDHandler.SendNotification(player, 1, 2500, "Erfolg freigeschaltet: Mobilität");
                }
                stopwatch.Stop();
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:VehicleShop:TestVehicle")]
        public static void TestVehicle(IPlayer player, int shopid, string hash)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (player == null || !player.Exists || shopid <= 0 || hash == "") return;
                if (player.GetPlayerCurrentTestVehicle() != "None")
                {
                    HUDHandler.SendNotification(player, 1, 7000, "Sie können nur 1 Fahrzeug gleichzeitig Testen.");
                    return;
                }
                long fHash = Convert.ToInt64(hash);
                string vehName = ServerVehicles.GetVehicleNameOnHash(fHash);
                int charId = User.GetPlayerOnline(player);
                if (charId == 0 || fHash == 0) return;
                bool PlaceFree = true;
                Position ParkOut = ServerVehicleShops.GetVehicleShopOutPosition(shopid);
                Rotation RotOut = ServerVehicleShops.GetVehicleShopOutRotation(shopid);
                foreach (IVehicle veh in Alt.GetAllVehicles().ToList()) { if (veh.Position.IsInRange(ParkOut, 2f)) { PlaceFree = false; break; } }
                if (!PlaceFree)
                {
                    HUDHandler.SendNotification(player, 3, 7000, $"Der Ausladepunkt ist belegt.");
                    return;
                }
                if (shopid == 1 || shopid == 2 || shopid == 3 || shopid == 4 || shopid == 5 || shopid == 6 || shopid == 7 || shopid == 8 || shopid == 9)
                {
                    return;
                }
                else
                {
                    ServerVehicles.CreateVehicle(fHash, charId, 4, 0, false, 0, ParkOut, RotOut, $"TEST-{charId}", 255, 255, 255, false);
                    player.SetPlayerCurrentTestVehicle($"{vehName}");
                    HUDHandler.SendNotification(player, 2, 7000, $"Sie haben dieses Fahrzeug 10 Minuten zum Testen.");
                }
                stopwatch.Stop();
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
        #endregion

        #region Clothes Shop
        public static void OpenClothesShop(ClassicPlayer player, int id)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || !ServerClothesShops.ExistClothesShop(id)) return;
                int gender = Convert.ToInt32(Characters.GetCharacterGender(player.CharacterId));
                var shopItems = ServerClothesShops.ServerClothesShopsItems_.ToList().Where(x => x.shopId == id && (x.gender == gender || x.gender == 2)).Select(x => new
                {
                    itemName = x.clothesName,
                    x.itemPrice,
                    clothesType = x.type,
                    clothesDraw = x.draw,
                    clothesTex = x.tex,
                }).ToList();

                var itemCount = (int)shopItems.Count;
                var iterations = Math.Floor((decimal)(itemCount / 30));
                var rest = itemCount % 30;
                for (var i = 0; i < iterations; i++)
                {
                    var skip = i * 30;
                    player.EmitLocked("Client:ClothesShop:sendItemsToClient", JsonConvert.SerializeObject(shopItems.Skip(skip).Take(30).ToList()));
                }
                if (rest != 0) player.EmitLocked("Client:ClothesShop:sendItemsToClient", JsonConvert.SerializeObject(shopItems.Skip((int)iterations * 30).ToList()));

                var torsoItems = ServerClothes.ServerClothes_.ToList().Where(x => x.faction == 0 && x.gender == gender && (x.type == "Torso" || x.type == "Undershirt")).Select(x => new
                {
                    itemName = x.clothesName,
                    itemPrice = 0,
                    clothesType = x.type,
                    clothesDraw = x.draw,
                    clothesTex = x.texture,
                }).ToList();

                var torsoCount = (int)torsoItems.Count;
                var torsoIterations = Math.Floor((decimal)(torsoCount / 30));
                var torsoRest = torsoCount % 30;
                for (var i = 0; i < torsoIterations; i++)
                {
                    var torsoSkip = i * 30;
                    player.EmitLocked("Client:ClothesShop:sendItemsToClient", JsonConvert.SerializeObject(torsoItems.Skip(torsoSkip).Take(30).ToList()));
                }
                if (rest != 0) player.EmitLocked("Client:ClothesShop:sendItemsToClient", JsonConvert.SerializeObject(torsoItems.Skip((int)torsoIterations * 30).ToList()));

                player.EmitLocked("Client:ClothesShop:createCEF", id);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:ClothesShop:buyItem")]
        public static void BuyClothesShopItem(ClassicPlayer player, int shopId, int amount, string clothesName)
        {
            try
            {
                var ClothesTypeGerman = "";
                if (player == null || !player.Exists || player.CharacterId <= 0 || !ServerClothesShops.ExistClothesShop(shopId) || amount <= 0 || !ServerClothes.ExistClothes(clothesName)) return;
                int price = ServerClothesShops.GetClothesPrice(shopId, clothesName) * amount;
                if (!CharactersInventory.ExistCharacterItem(player.CharacterId, "Bargeld", "brieftasche") || CharactersInventory.GetCharacterItemAmount(player.CharacterId, "Bargeld", "brieftasche") < price)
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Du hast nicht genügend Bargeld dabei!");
                    return;
                }
                if (CharactersClothes.ExistCharacterClothes(player.CharacterId, clothesName))
                {
                    HUDHandler.SendNotification(player, 3, 7000, "Dieses Kleidungsstück besitzt du bereits.");
                    return;
                }

                if (ServerClothes.GetClothesType(clothesName) == "Torso")
                {
                    Characters.SwitchCharacterClothes(player, "Torso", clothesName);
                    ClothesTypeGerman = "Oberkörper";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Mask")
                {
                    Characters.SwitchCharacterClothes(player, "Mask", clothesName);
                    ClothesTypeGerman = "Maske";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Leg")
                {
                    Characters.SwitchCharacterClothes(player, "Leg", clothesName);
                    ClothesTypeGerman = "Hose";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Feet")
                {
                    Characters.SwitchCharacterClothes(player, "Feet", clothesName);
                    ClothesTypeGerman = "Schuhe";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Necklace")
                {
                    Characters.SwitchCharacterClothes(player, "Necklace", clothesName);
                    ClothesTypeGerman = "Nackenbekleidung";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Undershirt")
                {
                    Characters.SwitchCharacterClothes(player, "Undershirt", clothesName);
                    ClothesTypeGerman = "Unterbekleidung";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Armor")
                {
                    Characters.SwitchCharacterClothes(player, "Armor", clothesName);
                    ClothesTypeGerman = "Panzerung";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Decal")
                {
                    Characters.SwitchCharacterClothes(player, "Decal", clothesName);
                    ClothesTypeGerman = "Abzeichen";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Top")
                {
                    Characters.SwitchCharacterClothes(player, "Top", clothesName);
                    ClothesTypeGerman = "Oberbekleidung";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Hat")
                {
                    Characters.SwitchCharacterClothes(player, "Hat", clothesName);
                    ClothesTypeGerman = "Hut";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Glass")
                {
                    Characters.SwitchCharacterClothes(player, "Glass", clothesName);
                    ClothesTypeGerman = "Brille";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Earring")
                {
                    Characters.SwitchCharacterClothes(player, "Earring", clothesName);
                    ClothesTypeGerman = "Ohrenring";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Watch")
                {
                    Characters.SwitchCharacterClothes(player, "Watch", clothesName);
                    ClothesTypeGerman = "Uhr";
                }
                else if (ServerClothes.GetClothesType(clothesName) == "Bracelet")
                {
                    Characters.SwitchCharacterClothes(player, "Bracelet", clothesName);
                    ClothesTypeGerman = "Armband";
                }
                else
                {
                    ClothesTypeGerman = "Unbekannt";
                }

               
                CharactersInventory.RemoveCharacterItemAmount(player.CharacterId, "Bargeld", price, "brieftasche");
                CharactersClothes.CreateCharacterOwnedClothes(player.CharacterId, clothesName);
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast dir ein Kleidungsstück von der Kategorie {ClothesTypeGerman} gekauft.");
                Characters.SwitchCharacterClothes(player, ServerClothes.GetClothesType(clothesName), clothesName);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
        #endregion

        #region Tattoo Shop
        internal static void OpenTattooShop(ClassicPlayer player, Server_Tattoo_Shops tattooShop)
        {
            if (player == null || !player.Exists || player.CharacterId <= 0 || tattooShop == null) return;
            int gender = Convert.ToInt32(Characters.GetCharacterGender(player.CharacterId));
            player.Emit("Client:TattooShop:openShop", gender, tattooShop.id, CharactersTattoos.GetAccountOwnTattoos(player.CharacterId));
        }

        [AsyncClientEvent("Server:TattooShop:buyTattoo")]
        public static void ClientEvent_buyTattoo(ClassicPlayer player, int shopId, int tattooId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || shopId <= 0 || tattooId <= 0 || !ServerTattoos.ExistTattoo(tattooId) || CharactersTattoos.ExistAccountTattoo(player.CharacterId, tattooId) || !ServerTattooShops.ExistTattooShop(shopId)) return;
                int price = ServerTattoos.GetTattooPrice(tattooId);
                if (!CharactersInventory.ExistCharacterItem(player.CharacterId, "Bargeld", "inventory") || CharactersInventory.GetCharacterItemAmount(player.CharacterId, "Bargeld", "inventory") < price)
                {
                    HUDHandler.SendNotification(player, 4, 5000, $"Fehler: Du hast nicht genügend Geld dabei ({price}$).");
                    return;
                }
                CharactersInventory.RemoveCharacterItemAmount(player.CharacterId, "Bargeld", price, "inventory");
                ServerTattooShops.SetTattooShopBankMoney(shopId, ServerTattooShops.GetTattooShopBank(shopId) + price);
                CharactersTattoos.CreateNewEntry(player.CharacterId, tattooId);
                HUDHandler.SendNotification(player, 2, 1500, $"Du hast das Tattoo '{ServerTattoos.GetTattooName(tattooId)}' für {price}$ gekauft.");
                player.updateTattoos();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        [AsyncClientEvent("Server:TattooShop:deleteTattoo")]
        public static void ClientEvent_deleteTattoo(ClassicPlayer player, int tattooId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || tattooId <= 0 || !CharactersTattoos.ExistAccountTattoo(player.CharacterId, tattooId)) return;
                CharactersTattoos.RemoveAccountTattoo(player.CharacterId, tattooId);
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast das Tattoo '{ServerTattoos.GetTattooName(tattooId)}' erfolgreich entfernen lassen.");
                player.updateTattoos();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }
        #endregion
    }
}
