using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.models;
using Altv_Roleplay.Utils;

namespace Altv_Roleplay.Handler
{
    class InventoryHandler : IScript
    {
        [AsyncClientEvent("Server:Inventory:RequestInventoryItems")]
        public static void RequestInventoryItems(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists) return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                if (!CharactersTablet.HasCharacterTutorialEntryFinished(charId, "openInventory"))
                {
                    CharactersTablet.SetCharacterTutorialEntryState(charId, "openInventory", true);
                    HUDHandler.SendNotification(player, 1, 7000, "Erfolg freigeschaltet: Inventar öffnen.");
                }
                string invArray = CharactersInventory.GetCharacterInventory(User.GetPlayerOnline(player));
                player.EmitLocked("Client:Inventory:AddInventoryItems", invArray, Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(User.GetPlayerOnline(player))), 0);
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Inventory:closeCEF")]
        public static void CloseInventoryCEF(IPlayer player)
        {
            if (player == null || !player.Exists) return;
            player.EmitLocked("Client:Inventory:closeCEF");
        }

        [AsyncClientEvent("Server:Inventory:switchItemToDifferentInv")]
        public static void SwitchItemToDifferentInv(ClassicPlayer player, string itemname, int itemAmount, string fromContainer, string toContainer)
        {
            try
            {
                if (player == null || !player.Exists || itemname == "" || itemAmount <= 0 || fromContainer == "" || toContainer == "" || User.GetPlayerOnline(player) == 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                int charId = player.CharacterId;
                string normalName = ServerItems.ReturnNormalItemName(itemname);
                float itemWeight = ServerItems.GetItemWeight(itemname) * itemAmount;
                float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                if (!CharactersInventory.ExistCharacterItem(charId, itemname, fromContainer)) return;

                if (toContainer == "inventory") { if (invWeight + itemWeight > 15f) { 
                        HUDHandler.SendNotification(player, 3, 7000, $"Soviel Platz hast du im Inventar nicht.");
                        return; 
                    } 
                }
                else if (toContainer == "backpack") { if (backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId))) { 
                        HUDHandler.SendNotification(player, 3, 7000, $"Soviel Platz hast du in deinen Taschen / deinem Rucksack nicht."); 
                        return; 
                    } 
                }

                if (CharactersInventory.GetCharacterItemAmount(charId, itemname, fromContainer) < itemAmount) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Die angegebene Item-Anzahl ist größer als die Anzahl der Items die du mit dir trägst."); 
                    return; 
                }
                if (itemname == "Rucksack" || itemname == "Tasche" || normalName == "Ausweis" || normalName == "Bargeld" || normalName == "Smartphone" || normalName == "EC Karte" || normalName == "Fahrzeugschluessel") { HUDHandler.SendNotification(player, 3, 5000, "Diesen Gegenstand kannst du nicht in deinen Rucksack / deine Tache legen."); return; }
                CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                CharactersInventory.AddCharacterItem(charId, itemname, itemAmount, toContainer);
                HUDHandler.SendNotification(player, 2, 7000, $"Der Gegenstand {itemname} ({itemAmount}x) wurde erfolgreich vom ({fromContainer}) in ({toContainer}) verschoben.");
                RequestInventoryItems(player);
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Inventory:UseItem")]
        public static async void UseItem(ClassicPlayer player, string itemname, int itemAmount, string fromContainer)
        {
            try
            {
                string ECData = null,
                    CarKeyData = null,
                    Kaufvertrag = null;
                if (player == null || !player.Exists || itemname == "" || itemAmount <= 0 || fromContainer == "" || User.GetPlayerOnline(player) == 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                if (ServerItems.IsItemUseable(ServerItems.ReturnNormalItemName(itemname)) == false) { 
                    HUDHandler.SendNotification(player, 4, 7000, $"Dieser Gegenstand ist nicht benutzbar ({itemname})!"); 
                    return; 
                }
                int charId = player.CharacterId;

                float itemWeight = ServerItems.GetItemWeight(itemname) * itemAmount;
                float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                if (charId <= 0 || CharactersInventory.ExistCharacterItem(charId, itemname, fromContainer) == false) return;
                if (CharactersInventory.GetCharacterItemAmount(charId, itemname, fromContainer) < itemAmount) { 
                    HUDHandler.SendNotification(player, 4, 7000, $"Die angegeben zu nutzende Anzahl ist nicht vorhanden ({itemname})!"); 
                    return; 
                }
                if (itemname.Contains("EC Karte")) { string[] SplittedItemName = itemname.Split(' '); ECData = itemname.Replace("EC Karte ", ""); itemname = "EC Karte"; }
                else if (itemname.Contains("Fahrzeugschluessel")) { string[] SplittedItemName = itemname.Split(' '); CarKeyData = itemname.Replace("Fahrzeugschluessel ", ""); itemname = "Autoschluessel"; }
                else if (itemname.Contains("Kaufvertrag")) { string[] SplittedItemName = itemname.Split(' '); Kaufvertrag = itemname.Replace("Kaufvertrag ", ""); itemname = "Kaufvertrag"; }

                if (ServerItems.IsItemDesire(itemname))
                {
                    CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                    Characters.SetCharacterHunger(charId, Characters.GetCharacterHunger(charId) + ServerItems.GetItemDesireFood(itemname) * itemAmount);
                    Characters.SetCharacterThirst(charId, Characters.GetCharacterThirst(charId) + ServerItems.GetItemDesireDrink(itemname) * itemAmount);
                    player.EmitLocked("Client:HUD:UpdateDesire", Characters.GetCharacterHunger(charId), Characters.GetCharacterThirst(charId), Characters.GetCharacterHealth(charId), Characters.GetCharacterArmor(charId)); //HUD updaten
                }

                else if (itemname == "Beamtenschutzweste")
                {
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Beamtenschutzweste", 1, fromContainer);
                    Characters.SetCharacterArmor(charId, 100);
                    player.Armor = 100;
                    if (Characters.GetCharacterGender(charId)) player.SetClothes(9, 17, 2, 0);
                    else player.SetClothes(9, 15, 2, 0);
                }

                //Schweissgerät
                else if (itemname == "Schweissgerät")
                {
                    var robATM = ServerATM.ServerATM_.ToList().FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                    if (robATM != null && !player.IsInVehicle)
                    {
                        #pragma warning disable CS4014
                        ATMRobHandler.RobATM((ClassicPlayer)player, robATM);
                        #pragma warning restore CS4014
                        return;
                    }

                    //Break up Vehicles Sheytan
                    ClassicVehicle veh = (ClassicVehicle)Alt.GetAllVehicles().ToList().FirstOrDefault(x => x != null && x.Exists && ((ClassicVehicle)x).VehicleId > 0 && player.Position.IsInRange(x.Position, 5f));
                    if (veh != null && veh.Exists && player.Vehicle != veh)
                    {
                        VehicleHandler.BreakVehicle(player, veh);
                        return;
                    }
                }

                else if (itemname == "Schutzweste")
                {
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Schutzweste", 1, fromContainer);
                    Characters.SetCharacterArmor(charId, 100);
                    player.Armor = 100;
                    if (Characters.GetCharacterGender(charId)) player.SetClothes(9, 17, 2, 0);
                    else player.SetClothes(9, 15, 2, 0);
                }

                else if (itemname == "Rucksack" || itemname == "Tasche")
                {
                    if (fromContainer == "backpack") { 
                        HUDHandler.SendNotification(player, 3, 7000, "Kleidungen & Taschen können nicht aus dem Rucksack aus benutzt werden."); 
                        return; 
                    }
                    if (Characters.GetCharacterBackpack(charId) == "Rucksack")
                    {
                        if (itemname == "Rucksack")
                        {
                            if (CharactersInventory.GetCharacterBackpackItemCount(charId) == 0)
                            {
                                Characters.SetCharacterBackpack(player, "None");
                                HUDHandler.SendNotification(player, 2, 7000, "Du hast deinen Rucksack ausgezogen.");
                            }
                            else { HUDHandler.SendNotification(player, 4, 7000, "Du hast zuviele Sachen im Rucksack, du kannst deinen Rucksack nicht ablegen."); }
                        }
                        else
                        {
                            HUDHandler.SendNotification(player, 3, 7000, "Du hast bereits eine Tasche angelegt, lege diese vorher ab um deinen Rucksack anzulegen.");
                        }
                    }

                    else if (Characters.GetCharacterBackpack(charId) == "Tasche")
                    {
                        if (itemname == "Tasche")
                        {
                            if (CharactersInventory.GetCharacterBackpackItemCount(charId) == 0)
                            {
                                Characters.SetCharacterBackpack(player, "None");
                                HUDHandler.SendNotification(player, 2, 7000, "Du hast deine Tasche ausgezogen.");
                            }
                            else { HUDHandler.SendNotification(player, 4, 7000, "Du hast zuviele Sachen in deiner Tasche, du kannst deine Tasche nicht ablegen."); }
                        }
                        else
                        {
                            HUDHandler.SendNotification(player, 3, 7000, "Du hast bereits einen Rucksack angelegt, lege diesen vorher ab um deine Tasche anzulegen.");
                        }
                    }

                    else if (Characters.GetCharacterBackpack(charId) == "None")
                    {
                        Characters.SetCharacterBackpack(player, itemname);
                        if (itemname == "Rucksack")
                        {
                            HUDHandler.SendNotification(player, 2, 7000, "Du hast deinen Rucksack angezogen");
                        } else
                        {
                            HUDHandler.SendNotification(player, 2, 7000, "Du hast deine Tasche angezogen.");
                        }
                    }
                }

                else if (itemname == "EC Karte")
                {
                    var atmPos = ServerATM.ServerATM_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 1f));
                    if (atmPos == null || player.IsInVehicle) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Du bist an keinem ATM oder sitzt in einem Auto."); 
                        return; 
                    }
                    int usingAccountNumber = Convert.ToInt32(ECData);
                    if (CharactersBank.GetBankAccountLockStatus(usingAccountNumber)) { if (CharactersInventory.ExistCharacterItem(charId, "EC Karte " + usingAccountNumber, "brieftasche")) { 
                            CharactersInventory.RemoveCharacterItemAmount(charId, "EC Karte " + usingAccountNumber, 1, "brieftasche"); 
                        } 
                        HUDHandler.SendNotification(player, 3, 7000, $"Ihre EC Karte wurde einzogen da diese gesperrt ist."); 
                        return; 
                    }
                    player.EmitLocked("Client:ATM:BankATMcreateCEF", CharactersBank.GetBankAccountPIN(usingAccountNumber), usingAccountNumber, atmPos.zoneName);
                }

                else if (ServerItems.GetItemType(itemname) == "weapon")
                {
                    if (itemname.Contains("Munitionsbox"))
                    {
                        string wName = itemname.Replace(" Munitionsbox", "");
                        CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                        CharactersInventory.AddCharacterItem(charId, $"{wName} Magazin", itemAmount, fromContainer);
                    }
                    else if (itemname.Contains("Magazin")) { WeaponHandler.EquipCharacterWeapon(player, "Ammo", itemname, 30 * itemAmount, fromContainer); }
                    else { WeaponHandler.EquipCharacterWeapon(player, "Weapon", itemname, 0, fromContainer); }
                }

                //Brecheisen
                else if (itemname == "Brecheisen")
                {
                    var house = ServerHouses.ServerHouses_.FirstOrDefault(x => x.ownerId > 0 && x.isLocked && ((ClassicColshape)x.entranceShape).IsInRange((ClassicPlayer)player));
                    if (house != null)
                    {
                        HouseHandler.BreakIntoHouse(player, house.id);
                        return;
                    }
                }

                //Hausschloss
                else if (itemname == "Hausschloss")
                {
                    var house = ServerHouses.ServerHouses_.FirstOrDefault(x => x.ownerId == charId && ((ClassicColshape)x.entranceShape).IsInRange((ClassicPlayer)player));
                    if (house != null)
                    {
                        CharactersInventory.RemoveCharacterItemAmount(charId, "Hausschloss", 1, fromContainer);
                        HouseHandler.ChangeKeys(player, house.id);
                        return;
                    }
                }

                //Verbandskasten
                else if (itemname == "Verbandskasten")
                {
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Verbandskasten", 1, fromContainer);
                    Characters.SetCharacterHealth(charId, 200);
                    player.Health = 200;
                    HUDHandler.SendNotification(player, 4, 7000, $"Du hast den {itemname} Genutzt!");
                }

                //Schmerztabletten
                else if (itemname == "Schmerztabletten")
                {
                    int amou1 = 10;
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Schmerztabletten", 1, fromContainer);
                    Characters.SetCharacterHealth(charId, player.Health + amou1);
                    player.Health = (ushort)(player.Health + amou1);
                    HUDHandler.SendNotification(player, 4, 7000, $"Du hast die {itemname} Genutzt!");
                }

                //Wundsalbe
                else if (itemname == "Wundsalbe")
                {
                    int amou1 = 7;
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Wundsalbe", 1, fromContainer);
                    Characters.SetCharacterHealth(charId, player.Health + amou1);
                    player.Health = (ushort)(player.Health + amou1);
                    HUDHandler.SendNotification(player, 4, 7000, $"Du hast die {itemname} Genutzt!");
                }

                //Pflaster
                else if (itemname == "Pflaster")
                {
                    int amou1 = 5;
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Pflaster", 1, fromContainer);
                    Characters.SetCharacterHealth(charId, player.Health + amou1);
                    player.Health = (ushort)(player.Health + amou1);
                    HUDHandler.SendNotification(player, 4, 7000, $"Du hast das {itemname} Genutzt!");
                }

                //Kühlpack
                else if (itemname == "Kühlpack")
                {
                    int amou1 = 2;
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Kühlpack", 1, fromContainer);
                    Characters.SetCharacterHealth(charId, player.Health + amou1);
                    player.Health = (ushort)(player.Health + amou1);
                    HUDHandler.SendNotification(player, 4, 7000, $"Du hast das {itemname} Genutzt!");
                }

                //Desinfektionsmittel
                else if (itemname == "Desinfektionsmittel")
                {
                    int amou1 = 3;
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Desinfektionsmittel", 1, fromContainer);
                    Characters.SetCharacterHealth(charId, player.Health + amou1);
                    player.Health = (ushort)(player.Health + amou1);
                    HUDHandler.SendNotification(player, 4, 7000, $"Du hast das {itemname} Genutzt deine wunde ist jetzt Sauber!");
                }

                //Dr.Elefant
                else if (itemname == "Dr.Elefant")
                {
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Dr.Elefant", 1, fromContainer);
                    HUDHandler.SendNotification(player, 4, 7000, "Du hast denn Dr.Elefant vernichtet!");
                }

                //Dr.Teddy
                else if (itemname == "Dr.Teddy")
                {
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Dr.Teddy", 1, fromContainer);
                    HUDHandler.SendNotification(player, 4, 7000, "Du hast denn Dr.Teddy vernichtet!");
                }

                //Joint
                else if (itemname == "Joint")
                {
                    int amou1 = 10 * 1;
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Joint", 1, fromContainer);
                    Characters.SetCharacterArmor(charId, player.Armor + amou1);
                    player.Armor = (ushort)(player.Armor + amou1);
                    player.EmitLocked("Client:Animation:PlayScenario", "WORLD_HUMAN_SMOKING_POT", 20000);
                    player.EmitLocked("Client:Inventory:PlayEffect", "DeathFailFranklinIn", 90000);
                    HUDHandler.SendNotification(player, 4, 7000, $"Du hast den {itemname} Genutzt!");
                }

                //Benzinkanister
                else if (itemname == "Benzinkanister" && player.IsInVehicle && player.Vehicle.Exists)
                {
                    if (ServerVehicles.GetVehicleFuel(player.Vehicle) >= ServerVehicles.GetVehicleFuelLimitOnHash(player.Vehicle.Model)) { 
                        HUDHandler.SendNotification(player, 4, 2000, "Der Tank ist bereits voll."); 
                        return; 
                    }
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Benzinkanister", 1, fromContainer);
                    ServerVehicles.SetVehicleFuel(player.Vehicle, ServerVehicles.GetVehicleFuel(player.Vehicle) + 15.0f);
                    HUDHandler.SendNotification(player, 2, 7000, "Du hast das Fahrzeug erfolgreich aufgetankt.");
                }

                //Muschel
                else if (itemname == "Muschel")
                {
                    var itemName = "Leere Muschel";
                    int rnd = new Random().Next(1, 100);
                    if (rnd <= 15) { itemName = "Perle1"; }//15%
                    if (rnd <= 85) { itemName = "Leere Muschel"; }//98%

                    CharactersInventory.RemoveCharacterItemAmount(charId, "Muschel", 1, fromContainer);
                    CharactersInventory.AddCharacterItem(charId, itemName, 1, "inventory");
                    HUDHandler.SendNotification(player, 2, 7000, $"Glückwunsch, du hast hast aus der Muschel eine {itemName} bekommen!");
                }

                //Benzinkanister
                else if (itemname == "Benzinkanister")
                {
                    foreach (IVehicle veh in Alt.GetAllVehicles())
                    {
                        if (player.Position.IsInRange(veh.Position, 5f))
                        {

                            if ((ServerVehicles.GetVehicleFuel(veh) + 15.0f) >= ServerVehicles.GetVehicleFuelLimitOnHash(veh.Model))
                            {
                                HUDHandler.SendNotification(player, 4, 7000, "Das Fahrzeug ist zu voll zum Tanken");
                                return;
                            }
                            else if (player != null)
                            {
                                ulong vehID = (ulong)veh.GetVehicleId();
                                if (charId <= 0 || vehID <= 0 || player.IsInVehicle) { 
                                    HUDHandler.SendNotification(player, 4, 7000, "Das Fahrzeug ist zu voll zum Tanken"); 
                                    return; 
                                }
                                var newfuel = ServerVehicles.GetVehicleFuel(veh) + 15.0f;
                                ServerVehicles.SetVehicleFuel(veh, newfuel);
                                CharactersInventory.RemoveCharacterItem(charId, "Benzinkanister", fromContainer);
                                HUDHandler.SendNotification(player, 3, 10000, "Fahrzeug wird befüllt, bitte warten.");
                                await Task.Delay(10000);
                                HUDHandler.SendNotification(player, 2, 7000, "Fahrzeug erfolgreich getankt");
                                CharactersInventory.AddCharacterItem(charId, "Leerer Benzinkanister", 1, "inventory");
                            }
                        }
                        else if (veh == null)
                        {
                            HUDHandler.SendNotification(player, 4, 7000, "Du bist nicht in der nähe eines Fahrzeuges.");
                            return;
                        }
                    }
                }

                //Leerer Benzinkanister
                else if (itemname == "Leerer Benzinkanister")
                {
                    var fuelSpot = ServerFuelStations.ServerFuelStationSpots_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2.5f));
                    if (fuelSpot == null) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Benzinkanister können nur an einer Tanke befüllt werden"); 
                        return; 
                    }
                    int fuelStationId = ServerFuelStations.GetFuelSpotParentStation(fuelSpot.id);
                    if (fuelStationId == 0) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Ein unerwarteter Fehler ist aufgetreten. [FEHLERCODE: 002]"); 
                        return; 
                    }
                    int availableLiter = ServerFuelStations.GetFuelStationAvailableLiters(fuelStationId);
                    if (availableLiter < 1) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Diese Tankstelle hat keinen Treibstoff mehr auf Lager."); 
                        return; 
                    }
                    if (CharactersInventory.ExistCharacterItem(charId, "Benzinkanister", "inventory")) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Du hast bereits einen vollen Benzinkanister."); 
                        return; 
                    }
                    if (invWeight + itemWeight > 15f && backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId))) {
                        HUDHandler.SendNotification(player, 3, 7000, $"Du hast nicht genug Platz in deinen Taschen."); 
                        return; 
                    }

                    else
                    {
                        if (invWeight + itemWeight <= 15f)
                        {
                            HUDHandler.SendNotification(player, 2, 10000, "Benzinkanister wird befüllt, bitte warten.");
                            CharactersInventory.RemoveCharacterItem(charId, "Leerer Benzinkanister", fromContainer);
                            await Task.Delay(10000);
                            CharactersInventory.RemoveCharacterItemAmount(charId, "Bargeld", 125, "brieftasche"); //15$ Benzin * 15L Benzin = 125$
                            CharactersInventory.AddCharacterItem(charId, "Benzinkanister", 1, "inventory");
                            HUDHandler.SendNotification(player, 2, 10000, "Benzinkanister ist nun voll.");
                        }
                        else
                        {
                            HUDHandler.SendNotification(player, 2, 10000, "Benzinkanister wird befüllt, bitte warten.");
                            CharactersInventory.RemoveCharacterItem(charId, "Leerer Benzinkanister", fromContainer);
                            await Task.Delay(10000);
                            CharactersInventory.RemoveCharacterItemAmount(charId, "Bargeld", 125, "brieftasche"); //15$ Benzin * 15L Benzin = 125$
                            CharactersInventory.AddCharacterItem(charId, "Benzinkanister", 1, "backpack");
                            HUDHandler.SendNotification(player, 2, 10000, "Benzinkanister ist nun voll.");
                        }
                    }
                }

                else if (itemname == "Pedalo")
                {
                    HUDHandler.SendNotification(player, 1, 7000, "Bruder muss los..");
                    player.EmitLocked("Client:Ragdoll:SetPedToRagdoll", true, 0); //Ragdoll setzen
                    player.EmitLocked("Client:Ragdoll:SetPedToRagdoll", false, 0); //Ragdoll setzen
                }

                else if (itemname == "Kokain")
                {
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Kokain", 1, fromContainer);
                    HUDHandler.SendNotification(player, 2, 2000, "Du hast Koks gezogen du bist nun 15 Minuten effektiver.");
                    player.EmitLocked("Client:Inventory:PlayEffect", "DMT_flight", 900000);
                    Characters.SetCharacterFastFarm(charId, true, 15);

                }

                else if (itemname == "Einreisegeschenk")
                {
                    CharactersInventory.RemoveCharacterItem(charId, "Einreisegeschenk", fromContainer);
                    int rnd = new Random().Next(2000, 7000);
                    CharactersInventory.AddCharacterItem(charId, "Bargeld", rnd, "brieftasche");
                    HUDHandler.SendNotification(player, 2, 7000, $"Glückwunsch, du hast hast aus dem Geschenk {rnd}$ bekommen!");
                }

                // Nagelband
                else if (itemname == "Nagelband")
                {
                    Nagelband nagelBand = NagelbandHandler.NagelbandList_.ToList().FirstOrDefault(x => x.prop != null && x.colshape != null && x.colshape.Position.IsInRange(player.Position, 2f));
                    if (nagelBand != null) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Hier liegt bereits ein Nagelband in der Nähe."); 
                        return; 
                    }
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Nagelband", 1, fromContainer);
                    NagelbandHandler.CreateNagelband(player);
                    HUDHandler.SendNotification(player, 2, 7000, "Nagelband platziert.");
                }

                else if (itemname == "Blumentopf")
                {
                    if (!CharactersInventory.ExistCharacterItem(charId, "Weedsamen", "inventory") && !CharactersInventory.ExistCharacterItem(charId, "Weedsamen", "backpack")) { HUDHandler.SendNotification(player, 4, 7000, "Du hast keine Weedsamen dabei."); return; }
                    if (CharactersInventory.ExistCharacterItem(charId, "Weedsamen", "inventory")) CharactersInventory.RemoveCharacterItemAmount(charId, "Weedsamen", 1, "inventory");
                    else if (CharactersInventory.ExistCharacterItem(charId, "Weedsamen", "backpack")) CharactersInventory.RemoveCharacterItemAmount(charId, "Weedsamen", 1, "backpack");
                    if (CharactersInventory.ExistCharacterItem(charId, "Blumentopf", "inventory")) CharactersInventory.RemoveCharacterItemAmount(charId, "Blumentopf", 1, "inventory");
                    else if (CharactersInventory.ExistCharacterItem(charId, "Blumentopf", "backpack")) CharactersInventory.RemoveCharacterItemAmount(charId, "Blumentopf", 1, "backpack");
                    WeedPlantHandler.PlaceNewWeedpot(player);
                }

                else if (itemname == "Pflanzenwasser")
                {
                    WeedPlantHandler.FillNearestPotWithWater(player);
                }

                else if (itemname == "Dünger")
                {
                    if (!CharactersInventory.ExistCharacterItem(charId, "Dünger", "inventory") && !CharactersInventory.ExistCharacterItem(charId, "Dünger", "backpack")) { HUDHandler.SendNotification(player, 4, 7000, "Du hast keinen Dünger dabei."); return; }
                    if (CharactersInventory.ExistCharacterItem(charId, "Dünger", "inventory")) CharactersInventory.RemoveCharacterItemAmount(charId, "Dünger", 1, "inventory");
                    else if (CharactersInventory.ExistCharacterItem(charId, "Dünger", "backpack")) CharactersInventory.RemoveCharacterItemAmount(charId, "Dünger", 1, "backpack");
                    WeedPlantHandler.FertilizeNearestPot(player);
                }

                else if (itemname == "Smartphone")
                {
                    if (Characters.IsCharacterPhoneEquipped(charId))
                    {
                        player.EmitLocked("Client:Smartphone:equipPhone", false, Characters.GetCharacterPhonenumber(charId), Characters.IsCharacterPhoneFlyModeEnabled(charId), Characters.GetCharacterPhoneWallpaper(charId));
                        HUDHandler.SendNotification(player, 2, 7000, "Smartphone ausgeschaltet.");
                        Alt.Emit("Server:Smartphone:leaveRadioFrequence", player);
                    }
                    else
                    {
                        player.EmitLocked("Client:Smartphone:equipPhone", true, Characters.GetCharacterPhonenumber(charId), Characters.IsCharacterPhoneFlyModeEnabled(charId), Characters.GetCharacterPhoneWallpaper(charId));
                        HUDHandler.SendNotification(player, 4, 7000, "Smartphone eingeschaltet.");
                    }
                    Characters.SetCharacterPhoneEquipped(charId, !Characters.IsCharacterPhoneEquipped(charId));
                    SmartphoneHandler.RequestLSPDIntranet((ClassicPlayer)player);
                }

                else if (ServerItems.GetItemType(itemname) == "clothes")
                {
                    if (ServerItems.GetClothesItemType(itemname) == "n") return;
                    Characters.SwitchCharacterClothesItem(player, itemname, ServerItems.GetClothesItemType(itemname));
                }

                else
                {
                    Console.WriteLine(itemname);
                }
                RequestInventoryItems(player);
                if (ServerItems.hasItemAnimation(ServerItems.ReturnNormalItemName(itemname))) { InventoryAnimation(player, ServerItems.GetItemAnimationName(ServerItems.ReturnNormalItemName(itemname)), 0); }
                if (ServerItems.GetAttachmentId(itemname) != 0)
                {
                    Attachment_DB attachmentDB = AttachmentHandler.AttachmentsDB_.ToList().FirstOrDefault(x => x.id == ServerItems.GetAttachmentId(itemname));
                    if (attachmentDB == null) return;
                    #pragma warning disable CS4014
                    AttachmentHandler.AddAttachment(player, new Attachment_Model { id = player.CharacterId, bone = attachmentDB.bone, model = attachmentDB.model, posX = attachmentDB.pos.X, posY = attachmentDB.pos.Y, posZ = attachmentDB.pos.Z, rotX = attachmentDB.rot.X, rotY = attachmentDB.rot.Y, rotZ = attachmentDB.rot.Z }, attachmentDB.removeAfterSeconds);
                    #pragma warning restore CS4014
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Inventory:DropItem")]
        public static void DropItem(ClassicPlayer player, string itemname, int itemAmount, string fromContainer)
        {
            try
            {
                if (player == null || !player.Exists || itemname == "" || itemAmount <= 0 || fromContainer == "" || User.GetPlayerOnline(player) == 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                string normalItemName = ServerItems.ReturnNormalItemName(itemname);
                if (ServerItems.IsItemDroppable(itemname) == false) { 
                    HUDHandler.SendNotification(player, 4, 7000, $"Diesen Gegenstand kannst du nicht wegwerfen ({itemname})."); 
                    return; 
                }
                int charId = player.CharacterId;
                if (charId <= 0 || CharactersInventory.ExistCharacterItem(charId, itemname, fromContainer) == false) return;
                if (CharactersInventory.GetCharacterItemAmount(charId, itemname, fromContainer) < itemAmount) { 
                    HUDHandler.SendNotification(player, 4, 5000, $"Die angegebene wegzuwerfende Anzahl ist nicht vorhanden ({itemname}).");
                    return; 
                }
                if (itemname == "Smartphone")
                {
                    if (Characters.IsCharacterPhoneEquipped(charId)) { HUDHandler.SendNotification(player, 3, 7000, "Du musst dein Handy erst ausschalten / ablegen."); return; }
                }

                else if (itemname == "Rucksack")
                {
                    if (Characters.GetCharacterBackpack(charId) == "Rucksack")
                    {
                        if (CharactersInventory.GetCharacterItemAmount(charId, "Rucksack", "inventory") == itemAmount)
                        {
                            HUDHandler.SendNotification(player, 3, 7000, "Du musst deinen Rucksack erst ablegen, bevor du diesen wegwerfen kannst.");
                            return;
                        }
                        else
                        {
                            CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                            InventoryAnimation(player, "drop", 0);
                            HUDHandler.SendNotification(player, 2, 7000, $"Der Gegenstand {itemname} ({itemAmount}) wurde erfolgreich weggeworfen ({fromContainer}).");
                            return;
                        }
                    }
                    else
                    {
                        CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                        InventoryAnimation(player, "drop", 0);
                        HUDHandler.SendNotification(player, 2, 7000, $"Der Gegenstand {itemname} ({itemAmount}) wurde erfolgreich weggeworfen ({fromContainer}).");
                        return;
                    }
                }

                else if (itemname == "Tasche")
                {
                    if (Characters.GetCharacterBackpack(charId) == "Tasche")
                    {
                        if (CharactersInventory.GetCharacterItemAmount(charId, "Tasche", "inventory") == itemAmount)
                        {
                            HUDHandler.SendNotification(player, 3, 7000, "Du musst zuerst deine Tasche ablegen, bevor du diese wegwerfen kannst.");
                            return;
                        }
                        else
                        {
                            CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                            InventoryAnimation(player, "drop", 0);
                            HUDHandler.SendNotification(player, 2, 7000, $"Der Gegenstand {itemname} ({itemAmount}) wurde erfolgreich weggeworfen ({fromContainer}).");
                            RequestInventoryItems(player);
                            return;
                        }
                    }

                    else
                    {
                        CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                        InventoryAnimation(player, "drop", 0);
                        HUDHandler.SendNotification(player, 2, 7000, $"Der Gegenstand {itemname} ({itemAmount}) wurde erfolgreich weggeworfen ({fromContainer}).");
                        RequestInventoryItems(player);
                        return;
                    }
                }

                else if (ServerItems.GetItemType(itemname) == "weapon")
                {
                    if ((string)Characters.GetCharacterWeapon(player, "PrimaryWeapon") == normalItemName || (string)Characters.GetCharacterWeapon(player, "SecondaryWeapon") == normalItemName || (string)Characters.GetCharacterWeapon(player, "SecondaryWeapon2") == normalItemName || (string)Characters.GetCharacterWeapon(player, "FistWeapon") == normalItemName)
                    {
                        if (CharactersInventory.GetCharacterItemAmount(charId, normalItemName, fromContainer) == itemAmount) {
                            HUDHandler.SendNotification(player, 3, 7000, "Du musst zuerst deine Waffe ablegen."); 
                            return; 
                        }
                    }
                }
                CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                InventoryAnimation(player, "drop", 0);
                HUDHandler.SendNotification(player, 2, 7000, $"Der Gegenstand {itemname} ({itemAmount}) wurde erfolgreich weggeworfen ({fromContainer}).");
                RequestInventoryItems(player);
            }

            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Inventory:GiveItem")]
        public static void GiveItem(ClassicPlayer player, string itemname, int itemAmount, string fromContainer)
        {
            try
            {
                IPlayer targethit = NearestPlayer(player, player.Position, 5f);
                IPlayer targetPlayer = targethit;

                int targetPlayerId = (int)targetPlayer.GetCharacterMetaId();
                if (targetPlayer == player || targethit == null) return;
                if (player == null || !player.Exists || itemname == "" || itemAmount <= 0 || fromContainer == "" || targetPlayerId == 0) return;
                if (targetPlayer == player)
                {
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Du bekommst von { Characters.GetCharacterName(User.GetPlayerOnline(player))} { itemAmount}x {itemname}.");
                    HUDHandler.SendNotification(player, 2, 7000, $"Du gibst { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemname}.");
                    return;
                }
                player.EmitLocked("Client:Inventory:closeCEF");
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                if (!ServerItems.IsItemGiveable(itemname)) { 
                    HUDHandler.SendNotification(player, 4, 7000, $"Diesen Gegenstand kannst du nicht weggeben ({itemname})."); 
                    return; 
                }
                int charId = player.CharacterId;
                if (charId <= 0 || !CharactersInventory.ExistCharacterItem(charId, itemname, fromContainer)) return;
                if (CharactersInventory.GetCharacterItemAmount(charId, itemname, fromContainer) < itemAmount) { 
                    HUDHandler.SendNotification(player, 4, 7000, $"Die angegebene Anzahl ist nicht vorhanden ({itemname})."); 
                    return; 
                }
                if (CharactersInventory.IsItemActive(player, itemname)) { 
                    HUDHandler.SendNotification(player, 4, 7000, $"Ausgerüstete Gegenstände können nicht abgegeben werden.");
                    return; 
                }
                float itemWeight = ServerItems.GetItemWeight(itemname) * itemAmount;
                float invWeight = CharactersInventory.GetCharacterItemWeight(targetPlayerId, "inventory");
                float backpackWeight = CharactersInventory.GetCharacterItemWeight(targetPlayerId, "backpack");
                if (targetPlayer == null) return;
                if (!player.Position.IsInRange(targetPlayer.Position, 5f)) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Die Person ist zu weit entfernt."); 
                    return; 
                }
                if (invWeight + itemWeight > 15f && backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(targetPlayerId))) { 
                    HUDHandler.SendNotification(player, 3, 7000, $"Der Spieler hat nicht genug Platz in seinen Taschen."); 
                    return;
                }

                if (itemname.Contains("Fahrzeugschluessel"))
                {
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Du bekommst von { Characters.GetCharacterName(User.GetPlayerOnline(player))} { itemAmount}x {itemname}.");
                    HUDHandler.SendNotification(player, 2, 7000, $"Du gibst { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemname}.");
                    CharactersInventory.AddCharacterItem(targetPlayerId, itemname, itemAmount, "schluessel");
                    CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                    InventoryAnimation(player, "give", 0);
                    return;
                }

                if (itemname.Contains("Generalschluessel"))
                {
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Du bekommst von { Characters.GetCharacterName(User.GetPlayerOnline(player))} { itemAmount}x {itemname}.");
                    HUDHandler.SendNotification(player, 2, 7000, $"Du gibst { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemname}.");
                    CharactersInventory.AddCharacterItem(targetPlayerId, itemname, itemAmount, "schluessel");
                    CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                    InventoryAnimation(player, "give", 0);
                    return;
                }

                if (itemname.Contains("Handschellenschluessel"))
                {
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Du bekommst von { Characters.GetCharacterName(User.GetPlayerOnline(player))} { itemAmount}x {itemname}.");
                    HUDHandler.SendNotification(player, 2, 7000, $"Du gibst { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemname}.");
                    CharactersInventory.AddCharacterItem(targetPlayerId, itemname, itemAmount, "schluessel");
                    CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                    InventoryAnimation(player, "give", 0);
                    return;
                }

                if (itemname.Contains("Hausschluessel"))
                {
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Du bekommst von { Characters.GetCharacterName(User.GetPlayerOnline(player))} { itemAmount}x {itemname}.");
                    HUDHandler.SendNotification(player, 2, 7000, $"Du gibst { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemname}."); 
                    CharactersInventory.AddCharacterItem(targetPlayerId, itemname, itemAmount, "schluessel");
                    CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                    InventoryAnimation(player, "give", 0);
                    return;
                }

                if (itemname.Contains("Bargeld"))
                {
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Du bekommst von { Characters.GetCharacterName(User.GetPlayerOnline(player))} { itemAmount}x {itemname}.");
                    HUDHandler.SendNotification(player, 2, 7000, $"Du gibst { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemname}.");
                    CharactersInventory.AddCharacterItem(targetPlayerId, itemname, itemAmount, "brieftasche");
                    CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                    InventoryAnimation(player, "give", 0);
                    return;
                }

                if (invWeight + itemWeight <= 15f)
                {

                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Du bekommst von { Characters.GetCharacterName(User.GetPlayerOnline(player))} { itemAmount}x {itemname}.");
                    HUDHandler.SendNotification(player, 2, 7000, $"Du gibst { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemname}.");
                    CharactersInventory.AddCharacterItem(targetPlayerId, itemname, itemAmount, "inventory");
                    CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                    InventoryAnimation(player, "give", 0);
                    return;
                }

                if (Characters.GetCharacterBackpack(targetPlayerId) != "None" && backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(targetPlayerId)))
                {
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Du bekommst von { Characters.GetCharacterName(User.GetPlayerOnline(player))} { itemAmount}x {itemname}.");
                    HUDHandler.SendNotification(player, 2, 7000, $"Du gibst { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemname}.");
                    CharactersInventory.AddCharacterItem(targetPlayerId, itemname, itemAmount, "backpack");
                    CharactersInventory.RemoveCharacterItemAmount(charId, itemname, itemAmount, fromContainer);
                    InventoryAnimation(player, "give", 0);
                    return;
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:PlayerSearch:TakeItem")]
        public static void PlayerSearchTakeItem(ClassicPlayer player, int givenTargetCharId, string itemName, string itemLocation, int itemAmount)
        {
            try
            {
                if (player == null || !player.Exists || givenTargetCharId <= 0 || itemName == "" || itemAmount <= 0 || itemLocation == "") return;
                int charId = player.CharacterId;
                if (charId <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                var targetPlayer = Alt.GetAllPlayers().ToList().FirstOrDefault(x => x.GetCharacterMetaId() == (ulong)givenTargetCharId);
                int targetCharId = (int)targetPlayer.GetCharacterMetaId();
                if (targetCharId != givenTargetCharId) return;
                if (targetPlayer == null || !targetPlayer.Exists) return;
                if(!player.Position.IsInRange(targetPlayer.Position, 3f)) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Fehler: Du bist zuweit vom Spieler entfernt."); 
                    return; 
                }
                if(!targetPlayer.HasPlayerHandcuffs() && !targetPlayer.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Der Spieler ist nicht gefesselt."); 
                    return; 
                }
                if(!ServerItems.IsItemDroppable(itemName) || !ServerItems.IsItemGiveable(itemName)) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Fehler: Diesen Gegenstand kannst du nicht entfernen."); 
                    return; 
                }
                if(!CharactersInventory.ExistCharacterItem(targetCharId, itemName, itemLocation)) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Dieser Gegenstand existiert nicht mehr.");
                    return; 
                }
                if(CharactersInventory.IsItemActive(targetPlayer, itemName)) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Ausgerüstete Gegenstände können nicht entwendet werden."); 
                    return; 
                }
                if(CharactersInventory.GetCharacterItemAmount(targetCharId, itemName, itemLocation) < itemAmount) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Fehler: Soviele Gegenstände hat der Spieler davon nicht."); 
                    return; 
                }
                float itemWeight = ServerItems.GetItemWeight(itemName) * itemAmount;
                float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                if (invWeight + itemWeight > 15f && backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId))) { 
                    HUDHandler.SendNotification(player, 3, 7000, $"Du hast nicht genug Platz in deinen Taschen."); 
                    return; 
                }
                CharactersInventory.RemoveCharacterItemAmount(targetCharId, itemName, itemAmount, itemLocation);
                
                if (itemName.Contains("Fahrzeugschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast der Person { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemName} entwendet."); 
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Dir wurde von { Characters.GetCharacterName(User.GetPlayerOnline(player))} {itemAmount}x {itemName} aus dem Schlüsselbund entwendet.");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "schluessel");
                    return;
                }
                if (itemName.Contains("Handschellenschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast der Person { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemName} entwendet.");
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Dir wurde von { Characters.GetCharacterName(User.GetPlayerOnline(player))} { itemAmount}x {itemName} aus dem Schlüsselbund entwendet.");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "schluessel");
                    return;
                }
                if (itemName.Contains("Generalschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast der Person { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemName} entwendet.");
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Dir wurde von { Characters.GetCharacterName(User.GetPlayerOnline(player))} { itemAmount}x {itemName} aus dem Schlüsselbund entwendet.");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "schluessel");
                    return;
                }
                if (itemName.Contains("Hausschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast der Person { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemName} entwendet.");
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Dir wurde von { Characters.GetCharacterName(User.GetPlayerOnline(player))} { itemAmount}x {itemName} aus dem Schlüsselbund entwendet.");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "schluessel");
                    return;
                }
                if (itemName.Contains("Bargeld"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast der Person { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemName} entwendet."); 
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Dir wurde von { Characters.GetCharacterName(User.GetPlayerOnline(player))} {itemAmount}x {itemName} aus der Brieftasche entwendet.");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "brieftasche");
                    return;
                }

                if (invWeight + itemWeight <= 15f || itemWeight == 0f)
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast der Person {Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemName} entwendet.");
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Dir wurde von { Characters.GetCharacterName(User.GetPlayerOnline(player))} {itemAmount}x {itemName} aus der Brieftasche entwendet.");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "inventory");
                    return;
                } 
                else if (Characters.GetCharacterBackpack(charId) != "None" && backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast der Person { Characters.GetCharacterName(User.GetPlayerOnline(targetPlayer))} {itemAmount}x {itemName} entwendet.");
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, $"Dir wurde von { Characters.GetCharacterName(User.GetPlayerOnline(player))} {itemAmount}x {itemName} aus der Inventar entwendet.");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "backpack");
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        internal static void InventoryAnimation(IPlayer player, string Animation, int duration)
        {
            if (player == null || !player.Exists || player.IsInVehicle || Animation == "") return;

            else if (Animation == "eat") player.EmitLocked("Client:Inventory:PlayAnimation", "amb@code_human_wander_eating_donut@male@idle_a", "idle_c", 3500, 49, false);
            else if (Animation == "drink") player.EmitLocked("Client:Inventory:PlayAnimation", "amb@world_human_drinking@beer@male@idle_a", "idle_c", 3500, 49, false);
            else if (Animation == "carlock") player.EmitLocked("Client:Inventory:PlayAnimation", "anim@mp_player_intmenu@key_fob@", "fob_click", 500, 1, false);
            else if (Animation == "drop") player.EmitLocked("Client:Inventory:PlayAnimation", "anim@narcotics@trash", "drop_front", 500, 1, false);
            else if (Animation == "give") player.EmitLocked("Client:Inventory:PlayAnimation", "anim@narcotics@trash", "drop_front", 500, 1, false);
            else if (Animation == "farmPickup") player.EmitLocked("Client:Inventory:PlayAnimation", "pickup_object", "pickup_low", duration, 1, false);
            else if (Animation == "handcuffs") player.EmitLocked("Client:Inventory:PlayAnimation", "mp_arresting", "sprint", -1, 49, false);
            else if (Animation == "revive") player.EmitLocked("Client:Inventory:PlayAnimation", "missheistfbi3b_ig8_2", "cpr_loop_paramedic", duration, 1, false);
            else if (Animation == "weste") player.EmitLocked("Client:Inventory:PlayAnimation", "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01", 3000, 1, false);
            else if (Animation == "Kokain") player.EmitLocked("Client:Inventory:PlayAnimation", "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01", 2000, 49, false);
            else if (Animation == "verband") player.EmitLocked("Client:Inventory:PlayAnimation", "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01", 3000, 1, false);
            else if (Animation == "repair") player.EmitLocked("Client:Inventory:PlayAnimation", "mini@repair", "fixing_a_ped", duration, 1, false);
            else if (Animation == "breakUp") player.EmitLocked("Client:Inventory:PlayAnimation", "anim@amb@clubhouse@tutorial@bkr_tut_ig3@", "machinic_loop_mechandplayer", duration, 49, false);
            else if (Animation == "joint") player.EmitLocked("Client:Inventory:PlayAnimation", "timetable@gardener@smoking_joint", "smoke_idle", 5500, 49, false);
            else if (Animation == "stabilisieren") player.EmitLocked("Client:Inventory:PlayAnimation", "amb@medic@standing@tendtodead@idle_a", "idle_a", 15000, 1, false);
            else if (Animation == "zigaretten") player.EmitLocked("Client:Animation:PlayScenario", "WORLD_HUMAN_SMOKING", 0);
        }

        internal static void StopAnimation(IPlayer player, string animDict, string animName)
        {
            if (player == null || !player.Exists) return;
            player.EmitLocked("Client:Inventory:StopAnimation", animDict, animName);
        }

        public static void Aservatenkammer(IPlayer player)
        {
            int charId = User.GetPlayerOnline(player);
            foreach (Characters_Inventory item in CharactersInventory.CharactersInventory_.ToList().Where(x => x.charId == charId && ServerItems.GetItemType(x.itemName) == "illegal"))
            {
                if (item.itemLocation == "inventory")
                {
                    CharactersInventory.RemoveCharacterItemAmount(charId, item.itemName, item.itemAmount, "inventory");
                }
                else if (item.itemLocation == "backpack")
                {
                    CharactersInventory.RemoveCharacterItemAmount(charId, item.itemName, item.itemAmount, "backpack");
                }
                HUDHandler.SendNotification(player, 1, 5000, $"Du hast {item.itemName} in die Aservatenkammer gelegt.");
            }
            return;
        }

        #pragma warning disable CS8632
        public static IPlayer? NearestPlayer(IPlayer player, Position pos, float distance)
        #pragma warning restore CS8632
        {
            Dictionary<float, IPlayer> vBf = new Dictionary<float, IPlayer>();
            foreach (IPlayer vs in Alt.GetAllPlayers().Where(x => x != player)) if (vs.Position.Distance(pos) <= distance) vBf.Add(vs.Position.Distance(pos), vs);
            if (vBf.Count > 0) return vBf[vBf.Keys.Min<float>()];
            else return null;
        }
    }
}