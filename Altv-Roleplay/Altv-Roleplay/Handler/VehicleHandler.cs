using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;
using Newtonsoft.Json;

namespace Altv_Roleplay.Handler
{
    class VehicleHandler : IScript
    {
        [AsyncClientEvent("Server:VehicleTrunk:StorageItem")]
        public static void VehicleTrunkStorageItem(ClassicPlayer player, int vehId, int charId, string itemName, int itemAmount, string fromContainer, string type)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (player == null || !player.Exists || vehId <= 0 || charId <= 0 || itemName == "" || itemAmount <= 0 || fromContainer == "none" || fromContainer == "") return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs())
                {
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?");
                    return;
                }
                if (type != "trunk" && type != "glovebox") return;
                int cCharId = player.CharacterId;
                if (cCharId != charId) return;
                var targetVehicle = Alt.GetAllVehicles().ToList().FirstOrDefault(x => x.GetVehicleId() == (long)vehId);
                if (targetVehicle == null || !targetVehicle.Exists) return;
                if (!player.Position.IsInRange(targetVehicle.Position, 5f))
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du bist zu weit entfernt.");
                    return;
                }
                if (type == "trunk")
                {
                    if (player.IsInVehicle)
                    {
                        HUDHandler.SendNotification(player, 3, 7000, "Wie willst du von Innen an den Kofferraum kommen?");
                        return;
                    }
                    if (!targetVehicle.GetVehicleTrunkState())
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Fehler: Der Kofferraum ist nicht geöffnet.");
                        return;
                    }
                }
                else if (type == "glovebox")
                {
                    if (!player.IsInVehicle)
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du bist in keinem Fahrzeug.");
                        return;
                    }
                }
                if (!CharactersInventory.ExistCharacterItem(charId, itemName, fromContainer))
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Diesen Gegenstand besitzt du nicht.");
                    return;
                }
                if (CharactersInventory.GetCharacterItemAmount(charId, itemName, fromContainer) < itemAmount)
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du hast nicht genügend Gegenstände davon dabei.");
                    return;
                }
                if (CharactersInventory.IsItemActive(player, itemName))
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Ausgerüstete Gegenstände können nicht umgelagert werden.");
                    return;
                }
                float itemWeight = ServerItems.GetItemWeight(itemName) * itemAmount;
                float curVehWeight = 0f;
                float maxVehWeight = 0f;

                if (type == "trunk")
                {
                    curVehWeight = ServerVehicles.GetVehicleVehicleTrunkWeight(vehId, false);
                    maxVehWeight = ServerVehicles.GetVehicleTrunkCapacityOnHash(targetVehicle.Model);
                }
                else if (type == "glovebox")
                {
                    curVehWeight = ServerVehicles.GetVehicleVehicleTrunkWeight(vehId, true);
                    maxVehWeight = 5f;
                }

                if (curVehWeight + itemWeight > maxVehWeight)
                {
                    HUDHandler.SendNotification(player, 3, 7000, $"Fehler: Soviel passt hier nicht rein (Aktuell: {curVehWeight} |  Maximum: {maxVehWeight}).");
                    return;
                }
                CharactersInventory.RemoveCharacterItemAmount(charId, itemName, itemAmount, fromContainer);

                if (type == "trunk")
                {
                    DiscordLog.SendEmbed("kofferraum", "Kofferraum Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {itemAmount}x {itemName} in den Kofferraum gelegt.");
                    ServerVehicles.AddVehicleTrunkItem(vehId, itemName, itemAmount, false);
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast den Gegenstand '{itemName} ({itemAmount}x)' in den Kofferraum gelegt.");
                    stopwatch.Stop();
                    return;
                }
                else if (type == "glovebox")
                {
                    DiscordLog.SendEmbed("handschuhfach", "Handschuhfach Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {itemAmount}x {itemName} in den Handschuhfach gelegt.");
                    ServerVehicles.AddVehicleTrunkItem(vehId, itemName, itemAmount, true);
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast den Gegenstand '{itemName} ({itemAmount}x)' in das Handschuhfach gelegt.");
                    stopwatch.Stop();
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:VehicleTrunk:TakeItem")]
        public static void VehicleTrunkTakeItem(ClassicPlayer player, int vehId, int charId, string itemName, int itemAmount, string type)
        {
            try
            {
                if (player == null || !player.Exists || vehId <= 0 || charId <= 0 || itemName == "" || itemAmount <= 0) return;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs())
                {
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?");
                    return;
                }
                if (type != "trunk" && type != "glovebox") return;
                int cCharId = player.CharacterId;
                if (cCharId != charId) return;
                var targetVehicle = Alt.GetAllVehicles().ToList().FirstOrDefault(x => x.GetVehicleId() == (long)vehId);
                if (targetVehicle == null || !targetVehicle.Exists) return;
                if (!player.Position.IsInRange(targetVehicle.Position, 5f))
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du bist zu weit entfernt.");
                    return;
                }
                if (type == "trunk")
                {
                    if (player.IsInVehicle)
                    {
                        HUDHandler.SendNotification(player, 3, 7000, "Wie willst du von Innen an den Kofferraum kommen?");
                        return;
                    }
                    if (!targetVehicle.GetVehicleTrunkState())
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Fehler: Der Kofferraum ist nicht geöffnet.");
                        return;
                    }
                    if (!ServerVehicles.ExistVehicleTrunkItem(vehId, itemName, false))
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Fehler: Der Gegenstand existiert hier nicht.");
                        return;
                    }
                    if (ServerVehicles.GetVehicleTrunkItemAmount(vehId, itemName, false) < itemAmount)
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Fehler: Soviele Gegenstände sind nicht im Fahrzeug.");
                        return;
                    }
                }
                else if (type == "glovebox")
                {
                    if (!player.IsInVehicle)
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du bist in keinem Fahrzeug.");
                        return;
                    }
                    if (!ServerVehicles.ExistVehicleTrunkItem(vehId, itemName, true))
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Fehler: Der Gegenstand existiert hier nicht.");
                        return;
                    }
                    if (ServerVehicles.GetVehicleTrunkItemAmount(vehId, itemName, true) < itemAmount)
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Fehler: Soviele Gegenstände sind nicht im Fahrzeug.");
                        return;
                    }
                }
                float itemWeight = ServerItems.GetItemWeight(itemName) * itemAmount;
                float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                if (invWeight + itemWeight > 15f && backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
                {
                    HUDHandler.SendNotification(player, 3, 7000, $"Du hast nicht genug Platz in deinen Taschen.");
                    return;
                }

                if (type == "trunk")
                {
                    DiscordLog.SendEmbed("kofferraum", "Kofferraum Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {itemAmount}x {itemName} aus dem Kofferraum genommen.");
                    ServerVehicles.RemoveVehicleTrunkItemAmount(vehId, itemName, itemAmount, false);
                }
                else if (type == "glovebox")
                {
                    DiscordLog.SendEmbed("handschuhfach", "Handschuhfach Log", $"{ Characters.GetCharacterName(User.GetPlayerOnline(player))} hat {itemAmount}x {itemName} aus dem Handschuhfach genommen.");
                    ServerVehicles.RemoveVehicleTrunkItemAmount(vehId, itemName, itemAmount, true);
                }
                if (itemName.Contains("Fahrzeugschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {itemName} ({itemAmount}x) aus dem Fahrzeug genommen (Lagerort: Schlüsselbund).");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "schluessel");
                    stopwatch.Stop();
                    return;
                }
                if (itemName.Contains("Generalschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {itemName} ({itemAmount}x) aus dem Fahrzeug genommen (Lagerort: Schlüsselbund).");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "schluessel");
                    stopwatch.Stop();
                    return;
                }
                if (itemName.Contains("Handschellenschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {itemName} ({itemAmount}x) aus dem Fahrzeug genommen (Lagerort: Schlüsselbund).");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "schluessel");
                    stopwatch.Stop();
                    return;
                }
                if (itemName.Contains("Hausschluessel"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {itemName} ({itemAmount}x) aus dem Fahrzeug genommen (Lagerort: Schlüsselbund).");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "schluessel");
                    stopwatch.Stop();
                    return;
                }
                if (itemName.Contains("Bargeld"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {itemName} ({itemAmount}x) aus dem Fahrzeug genommen (Lagerort: Brieftasche).");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "brieftasche");
                    stopwatch.Stop();
                    return;
                }
                if (itemName.Contains("EC Karte"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {itemName} ({itemAmount}x) aus dem Fahrzeug genommen (Lagerort: Brieftasche).");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "brieftasche");
                    stopwatch.Stop();
                    return;
                }
                if (itemName.Contains("Ausweis"))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {itemName} ({itemAmount}x) aus dem Fahrzeug genommen (Lagerort: Brieftasche).");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "brieftasche");
                    stopwatch.Stop();
                    return;
                }
                if (invWeight + itemWeight <= 15f)
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {itemName} ({itemAmount}x) aus dem Fahrzeug genommen (Lagerort: Inventar).");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "inventory");
                    stopwatch.Stop();
                    return;
                }
                if (Characters.GetCharacterBackpack(charId) != "None" && backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
                {
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast {itemName} ({itemAmount}x) aus dem Fahrzeug genommen (Lagerort: Rucksack / Tasche).");
                    CharactersInventory.AddCharacterItem(charId, itemName, itemAmount, "backpack");
                    stopwatch.Stop();
                    return;
                }

            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        internal static void OpenLicensingCEF(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists) return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                if (!player.Position.IsInRange(Constants.Positions.VehicleLicensing_Position, 3f))
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du hast dich zu weit entfernt.");
                    return;
                }

                var vehicleList = Alt.GetAllVehicles().Where(x => x.GetVehicleId() > 0 && x.Position.IsInRange(Constants.Positions.VehicleLicensing_VehPosition, 10f) && ServerVehicles.GetVehicleOwner(x) == charId).Select(x => new
                {
                    vehId = x.GetVehicleId(),
                    ownerId = ServerVehicles.GetVehicleOwner(x),
                    vehName = ServerVehicles.GetVehicleNameOnHash(x.Model),
                    vehPlate = ServerVehicles.GetVehiclePlateByOwner(x.GetVehicleId()),
                }).ToList();

                if (vehicleList.Count <= 0)
                {
                    HUDHandler.SendNotification(player, 3, 7000, "Keines deiner Fahrzeuge steht vor dem Anmeldegebäude (an der roten Fahrzeugmarkierung).");
                    return;
                }
                if (ServerVehicles.GetVehicleType2(ServerVehicles.GetVehicleID(charId)) == 2) return;
                player.EmitLocked("Client:VehicleLicensing:openCEF", JsonConvert.SerializeObject(vehicleList));
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:VehicleLicensing:LicensingAction")]
        public void LicensingAction(IPlayer player, string action, int vehId, string vehPlate, string newPlate)
        {
            try
            {
                if (player == null || !player.Exists || vehId <= 0 || vehPlate == "") return;
                if (action != "anmelden" && action != "abmelden") return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                IVehicle veh = Alt.GetAllVehicles().ToList().FirstOrDefault(x => x.GetVehicleId() == (long)vehId);
                if (ServerVehicles.GetVehicleType(veh) == 4) return;
                if (ServerVehicles.GetVehicleFactionId(veh) >= 1)
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Du kannst bei einem Fraktionsfahrzeug keine Änderungen vornehmen, komme mit deinem Privat Fahrzeug wieder.");
                    return;
                }
                var plate = ServerVehicles.GetVehiclePlateByOwner(vehId);
                if (veh == null || !veh.Exists)
                {
                    HUDHandler.SendNotification(player, 4, 5000, "Fehler: Ein unerwarteter Fehler ist aufgetreten.");
                    return;
                }
                if (ServerVehicles.GetVehicleOwner(veh) != charId)
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Dieses Fahrzeug gehört nicht dir.");
                    return;
                }
                if (!veh.Position.IsInRange(Constants.Positions.VehicleLicensing_VehPosition, 10f))
                {
                    HUDHandler.SendNotification(player, 4, 5000, "Fehler: Das Fahrzeug ist nicht am Zulassungspunkt.");
                    return;
                }
                if (!ServerVehicles.GetVehicleLockState(veh))
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Das Fahrzeug muss zugeschlossen sein.");
                    return;
                }

                if (action == "anmelden")
                {
                    var notAllowedStrings = new[] { "LSPD", "DOJ", "LSFD", "BNY", "DMV", "FIB", "DMV-", "LSPD-", "DOJ-", "LSFD-", "BNY-", "FIB-", "NL", "EL-", "MM-", "PL-", "SWAT", "S.W.A.T", "SWAT-", "NOOSE", "N.O.O.S.E", " ", "-", "_", ".", ",", "$" };
                    newPlate = newPlate.Replace(" ", "");
                    if (ServerVehicles.ExistServerVehiclePlate(newPlate))
                    {
                        HUDHandler.SendNotification(player, 3, 7000, "Fehler: Dieses Nummernschild ist bereits vorhanden.");
                        return;
                    }
                    bool stringIsValid = Regex.IsMatch(newPlate, @"[a-zA-Z0-9-]$");
                    bool validPlate = false;
                    if (stringIsValid) validPlate = true;
                    for (var i = 0; i < notAllowedStrings.Length; i++)
                    {
                        if (newPlate.Contains(notAllowedStrings[i])) { validPlate = false; break; }
                    }
                    if (!validPlate)
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Fehler: Das Wunschnummernschild enthält unzulässige Zeichen.");
                        return;
                    }
                    if (!CharactersInventory.ExistCharacterItem(charId, "Bargeld", "brieftasche"))
                    {
                        HUDHandler.SendNotification(player, 3, 7000, "Fehler: Du hast kein Bargeld dabei (120$).");
                        return;
                    }
                    if (CharactersInventory.GetCharacterItemAmount(charId, "Bargeld", "brieftasche") < 120)
                    {
                        HUDHandler.SendNotification(player, 3, 7000, "Fehler: Du hast nicht genügend Bargeld dabei (120$).");
                        return;
                    }
                    CharactersInventory.RemoveCharacterItemAmount(charId, "Bargeld", 120, "brieftasche");
                    CharactersInventory.RenameCharactersItemName($"Fahrzeugschluessel {plate}", $"Fahrzeugschluessel {newPlate}");
                    ServerVehicles.SetServerVehiclePlate(vehId, newPlate);
                    veh.NumberplateText = newPlate;
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast das Kennzeichen von dem Fahrzeug '{ServerVehicles.GetVehicleNameOnHash(veh.Model)}' auf {newPlate} geändert (Gebühr: 120$).");
                    return;
                }
                else if (action == "abmelden")
                {
                    int rnd = new Random().Next(100000, 999999);
                    if (ServerVehicles.ExistServerVehiclePlate($"NL{rnd}")) { LicensingAction(player, "abmelden", vehId, vehPlate, newPlate); return; }
                    CharactersInventory.RenameCharactersItemName($"Fahrzeugschluessel {plate}", $"Fahrzeugschluessel NL{rnd}");
                    ServerVehicles.SetServerVehiclePlate(vehId, $"NL{rnd}");
                    veh.NumberplateText = "__";
                    HUDHandler.SendNotification(player, 2, 7000, $"Du hast das Fahrzeug '{ServerVehicles.GetVehicleNameOnHash(veh.Model)}' mit dem Kennzeichen '{plate}' abgemeldet.");
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        internal static void OpenChangeKeyCEF(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                if (!player.Position.IsInRange(Constants.Positions.Schluesseldienst_Pos, 3f))
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Du hast dich zu weit entfernt.");
                    return;
                }
                var vehicleList = Alt.GetAllVehicles().Where(x => x.GetVehicleId() > 0 && x.Position.IsInRange(Constants.Positions.Schluesseldienst_Pos, 10f) && ServerVehicles.GetVehicleOwner(x) == player.CharacterId).Select(x => new
                {
                    vehId = x.GetVehicleId(),
                    vehName = ServerVehicles.GetVehicleNameOnHash(x.Model),
                    vehPlate = ServerVehicles.GetVehiclePlateByOwner(x.GetVehicleId()),
                }).ToList();

                var houseList = Model.ServerHouses.ServerHouses_.Where(x => x.ownerId == player.CharacterId).Select(x => new
                {
                    x.id,
                    x.street,
                }).ToList();

                if (vehicleList.Count <= 0 && houseList.Count <= 0)
                {
                    HUDHandler.SendNotification(player, 3, 7000, "Keines deiner Fahrzeuge steht in der Nähe und du hast keine Häuser.");
                    return;
                }

                player.Emit("Client:ChangeKey:openCEF", JsonConvert.SerializeObject(vehicleList), JsonConvert.SerializeObject(houseList));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [AsyncClientEvent("Server:ChangeKeyCEF:changeVehKey")]
        public static void ChangeVehKey(ClassicPlayer player, int vehId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                models.Server_Vehicles veh = ServerVehicles.ServerVehicles_.ToList().FirstOrDefault(x => x.id == vehId);
                if (veh == null || veh.charid != player.CharacterId) return;
                CharactersInventory.AddCharacterItem(player.CharacterId, $"Fahrzeugschluessel {veh.plate}", 1, "schluessel");
                HUDHandler.SendNotification(player, 2, 7000, "Schlüssel erfolgreich nachgemacht.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [AsyncClientEvent("Server:ChangeKeyCEF:changeHouseKey")]
        public static void ChangeHouseKey(ClassicPlayer player, int houseId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                models.Server_Houses house = ServerHouses.ServerHouses_.ToList().FirstOrDefault(x => x.id == houseId);
                if (house == null || house.ownerId != player.CharacterId) return;
                CharactersInventory.AddCharacterItem(player.CharacterId, $"Hausschluessel {house.id}", 1, "schluessel");
                HUDHandler.SendNotification(player, 2, 7000, "Schlüssel erfolgreich nachgemacht.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //BreakUp
        internal static async void BreakVehicle(ClassicPlayer player, ClassicVehicle veh)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || veh == null || !veh.Exists || veh.VehicleId <= 0) return;
                if (!ServerVehicles.GetVehicleLockState(veh))
                {
                    HUDHandler.SendNotification(player, 2, 7000, "Das Fahrzeug ist bereits offen.");
                    return;
                }
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs())
                {
                    HUDHandler.SendNotification(player, 4, 7000, "Wie willst du das mit Handschellen/Fesseln machen?");
                    return;
                }
                if (player.IsPlayerUsingCrowbar())
                {
                    player.EmitLocked("Client:Inventory:StopAnimation");
                    player.SetPlayerUsingCrowbar(false);
                    HUDHandler.SendNotification(player, 4, 7000, "Du hast den Aufbruch abgebrochen.");
                    return;
                }
                else
                {
                    // Aufbrechen
                    int duration = 120000;
                    ClassicPlayer vehOwner = (ClassicPlayer)Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId == ServerVehicles.GetVehicleOwner(veh));
                    if (vehOwner != null && vehOwner.Exists)
                    {
                        // Nachricht + GPS
                        HUDHandler.SendNotification(vehOwner, 3, duration, $"Dein Fahrzeug {ServerAllVehicles.GetVehicleNameOnHash(veh.Model)} wird aufgebrochen, Standort wurde dir im GPS markiert.");
                        vehOwner.EmitLocked("Server:GPS:setWaypoint", veh.Position.X, veh.Position.Y);
                    }

                    player.GiveWeapon(WeaponModel.Crowbar, 1, true);
                    player.SetPlayerUsingCrowbar(true);
                    // Animation
                    player.EmitLocked("Client:Animation:PlayScenario", "WORLD_HUMAN_WELDING", duration);
                    HUDHandler.SendNotification(player, 1, duration, "Du brichst das Auto auf..");
                    Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "playHowl2d", "./audio/caralarm.ogg");
                    DiscordLog.SendEmbed("breakvehicle", "BreakVehicle Log", $"Der Spieler { Characters.GetCharacterName(User.GetPlayerOnline(player))} hat das Fahrzeug {veh.GetVehicleId()} ausgeraubt.");
                    await Task.Delay(duration);
                    if (player == null || !player.Exists || veh == null || !veh.Exists) return;
                    if (!player.Position.IsInRange(veh.Position, 4f))
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Aufbrechen abgebrochen, du bist zu weit entfernt."); player.SetPlayerUsingCrowbar(false); player.RemoveWeapon(WeaponModel.Crowbar);
                        return;
                    }
                    if (!player.IsPlayerUsingCrowbar()) return;
                    player.RemoveWeapon(WeaponModel.Crowbar);
                    ServerVehicles.SetVehicleLockState(veh, false);
                    veh.LockState = VehicleLockState.Unlocked;
                    HUDHandler.SendNotification(player, 1, 7000, "Auto aufgebrochen, verschwinde oder mach weiter..");
                    player.SetPlayerUsingCrowbar(false);
                    player.EmitLocked("Client:Inventory:StopAnimation");
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static void Autopilot(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                ClassicVehicle veh = (ClassicVehicle)player.Vehicle;
                long vehID = veh.GetVehicleId();
                foreach (models.Server_Vehicles veh2 in ServerVehicles.ServerVehicles_.ToList().Where(x => x.plate == ServerVehicles.GetVehiclePlateByOwner(vehID)))
                {
                    if (veh == null || veh2.charid != player.CharacterId) return;
                    var vehhash = ServerVehicles.GetVehicleHashById(veh2.id);
                    models.Server_All_Vehicles veh3 = ServerAllVehicles.ServerAllVehicles_.ToList().FirstOrDefault(x => x.hash == vehhash);
                    if (veh3.fuelType == "Strom")
                    {
                        player.EmitLocked("Autopilot:autopilot", vehhash);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}