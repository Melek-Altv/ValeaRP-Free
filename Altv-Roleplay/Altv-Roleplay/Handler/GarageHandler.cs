using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.models;
using Altv_Roleplay.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Altv_Roleplay.Handler
{
    class GarageHandler : IScript
    {
        internal static void OpenGarageCEF(ClassicPlayer player, int garageId, bool isHouseGarage)
        {
            try
            {
                if (player == null || !player.Exists || garageId == 0) return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                var charFaction = ServerFactions.GetCharacterFactionId(charId);
                if (isHouseGarage)
                {
                    // Hier dient die garageId als houseId.
                    var houseInfo = ServerHouses.ServerHouses_.ToList().FirstOrDefault(x => x.id == garageId);
                    if (houseInfo == null || !player.Position.IsInRange(houseInfo.garagePos, 5f)) return;
                    if (houseInfo.ownerId != player.CharacterId && !ServerHouses.IsCharacterRentedInHouse(player.CharacterId, houseInfo.id)) {
                        HUDHandler.SendNotification(player, 3, 7000, "Dieses Haus gehört nicht dir und / oder du bist nicht eingemietet."); 
                        return; 
                    }
                    var inString = GetGarageParkInString(player, player.CharacterId, garageId, false, "Zivilist", charFaction, true);
                    var outString = GetGarageParkOutString(player, garageId, player.CharacterId, false, "Zivilist", true);
                    Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "Client:Garage:OpenGarage", garageId, $"Hausgarage {houseInfo.id}", inString, outString, isHouseGarage);
                }
                else
                {
                    var garageInfo = ServerGarages.ServerGarages_.FirstOrDefault(x => x.id == garageId);
                    if (garageInfo == null) return;
                    if (!player.Position.IsInRange(new Position(garageInfo.posX, garageInfo.posY, garageInfo.posZ), 2f)) return;
                    var factionCut = ServerFactions.GetFactionShortName(charFaction);
                    bool charFactionDuty = ServerFactions.IsCharacterInFactionDuty(charId);
                    var inString = "";
                    var outString = "";
                    var garageName = "";
                    //0 Auto | 1 Boot | 2 Flugzeug | 3 Heli
                    if (garageInfo.type == 0) { garageName = $"Fahrzeuggarage: {garageInfo.name}"; }
                    else if (garageInfo.type == 1) { garageName = $"Bootsgarage: {garageInfo.name}"; }
                    else if (garageInfo.type == 2) { garageName = $"Flugzeuggarage: {garageInfo.name}"; }
                    else if (garageInfo.type == 3) { garageName = $"Heligarage: {garageInfo.name}"; }

                    if (garageInfo.name.Contains("Fraktion"))
                    {
                        if (charFaction <= 0) { HUDHandler.SendNotification(player, 4, 7000, $"Keine Berechtigung."); return; }
                        var gFactionCut = garageInfo.name.Split(" ")[1]; //Fraktion LSPD Mission Row  <- Beispiel
                        if (gFactionCut != factionCut) { HUDHandler.SendNotification(player, 4, 7000, $"Keine Berechtigung [002]."); return; }
                        inString = GetGarageParkInString(player, charId, garageId, true, factionCut, charFaction, false);
                        outString = GetGarageParkOutString(player, garageId, charId, true, factionCut, false);
                        player.EmitLocked("Client:Garage:OpenGarage", garageId, garageName, inString, outString, isHouseGarage);
                        return;
                    }

                    inString = GetGarageParkInString(player, charId, garageId, false, "Zivilist", charFaction, false); //Array von Fahrzeugen die um die Garage rum zum Einparken stehen
                    outString = GetGarageParkOutString(player, garageId, charId, false, "Zivilist", false);
                    Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "Client:Garage:OpenGarage", garageId, garageName, inString, outString, isHouseGarage);
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static string GetGarageParkInString(ClassicPlayer player, int charId, int garageId, bool isFaction, string factionShort, int factionId, bool isHouseGarage)
        {
            if (player == null || !player.Exists || garageId == 0 || charId == 0) return "undefined";
            List<IVehicle> vehicles = null;
            dynamic array = new JArray() as dynamic;
            dynamic entry = new JObject();
            if (isHouseGarage)
            {
                List<ClassicVehicle> vehs = Alt.GetAllVehicles().ToList().Cast<ClassicVehicle>().Where(x => x != null && x.Exists && x.VehicleId > 0 && x.Position.IsInRange(player.Position, 50f)).ToList();
                foreach (var veh in vehs)
                {
                    if (!CharactersInventory.ExistCharacterItem(player.CharacterId, $"Fahrzeugschluessel {veh.NumberplateText}", "schluessel") && ServerVehicles.GetVehicleOwner(veh) != player.CharacterId) continue;
                    entry = new JObject();
                    entry.vehid = veh.VehicleId;
                    entry.plate = ServerVehicles.GetVehiclePlateByOwner(veh.GetVehicleId());
                    entry.hash = veh.Model;
                    entry.name = ServerVehicles.GetVehicleNameOnHash(veh.Model);
                    array.Add(entry);
                }
            }
            else
            {
                if (isFaction == false) { vehicles = Alt.GetAllVehicles().Where(x => x != null && x.Exists && x.HasVehicleId() && x.GetVehicleId() > 0 && x.Position.IsInRange(player.Position, 50f)).ToList(); }
                else if (isFaction == true) { vehicles = Alt.GetAllVehicles().Where(x => x != null && x.Exists && x.HasVehicleId() && x.GetVehicleId() > 0 && x.Position.IsInRange(player.Position, 50f) && ServerVehicles.GetVehicleFactionId(x) == factionId && x.NumberplateText.Contains(factionShort)).ToList(); }
                int garageType = ServerGarages.GetGarageType(garageId);
                if (garageType == -1) return "undefined";
                foreach (var veh in vehicles)
                {
                    bool hasKey = false,
                        isOwner = ServerVehicles.GetVehicleOwner(veh) == charId;
                    if (isFaction) hasKey = CharactersInventory.ExistCharacterItem(charId, $"Fahrzeugschluessel {factionShort}", "schluessel");
                    else if (!isFaction) hasKey = CharactersInventory.ExistCharacterItem(charId, $"Fahrzeugschluessel {veh.NumberplateText}", "schluessel");
                    if (!isOwner && !hasKey) continue;
                    entry = new JObject();
                    entry.vehid = veh.GetVehicleId();
                    entry.plate = ServerVehicles.GetVehiclePlateByOwner(veh.GetVehicleId());
                    entry.hash = veh.Model;
                    entry.name = ServerVehicles.GetVehicleNameOnHash(veh.Model);
                    array.Add(entry);
                }
            }
            return array.ToString();
        }

        public static string GetGarageParkOutString(ClassicPlayer player, int garageId, int charId, bool isFaction, string factionShort, bool isHouseGarage)
        {
            try
            {
                if (player == null || !player.Exists || garageId == 0 || charId == 0) return "undefined";
                List<Server_Vehicles> inGarageVehs = null;
                dynamic array = new JArray() as dynamic;
                dynamic entry = new JObject();
                if (isHouseGarage)
                {
                    inGarageVehs = ServerVehicles.ServerVehicles_.Where(x => x.isInHouseGarage && x.garageId == garageId).ToList();
                    foreach (var veh in inGarageVehs)
                    {
                        bool hasKey = CharactersInventory.ExistCharacterItem(player.CharacterId, $"Fahrzeugschluessel {veh.plate}", "schluessel"),
                            isOwner = veh.charid == player.CharacterId;
                        if (!hasKey && !isOwner) continue;
                        entry = new JObject();
                        entry.vehid = veh.id;
                        entry.plate = veh.plate;
                        entry.hash = veh.hash;
                        entry.name = ServerAllVehicles.GetVehicleNameOnHash(veh.hash);
                        array.Add(entry);
                    }
                }
                else
                {
                    if (isFaction == false) { inGarageVehs = ServerVehicles.ServerVehicles_.Where(x => x.isInGarage == true && x.garageId == garageId && x.isInHouseGarage == false).ToList(); }
                    else if (isFaction == true) { inGarageVehs = ServerVehicles.ServerVehicles_.Where(x => x.isInGarage == true && x.garageId == garageId && x.isInHouseGarage == false && x.plate.Contains(factionShort)).ToList(); }

                    foreach (var vehicle in inGarageVehs)
                    {
                        bool hasKey = false;
                        if (isFaction == false) { hasKey = CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel " + vehicle.plate, "schluessel"); }
                        else if (isFaction == true) { hasKey = CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel " + factionShort, "schluessel"); }
                        bool isOwner = vehicle.charid == charId;
                        if (!hasKey && !isOwner) continue;

                        entry = new JObject();
                        entry.vehid = vehicle.id;
                        entry.plate = vehicle.plate;
                        entry.hash = vehicle.hash;
                        entry.name = ServerAllVehicles.GetVehicleNameOnHash(vehicle.hash);
                        array.Add(entry);
                    }
                }
                return array.ToString();
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return "[]";
        }

        [AsyncClientEvent("Server:Garage:DoAction")]
        public static async Task DoGarageAction(IPlayer player, int garageid, string action, int vehID, bool isHouseGarage)
        {
            try
            {
                if (player == null || !player.Exists || action == "" || vehID <= 0 || garageid <= 0) return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                var vehicle = Alt.GetAllVehicles().ToList().FirstOrDefault(x => x.GetVehicleId() == (long)vehID);
                if (action == "storage")
                {
                    if (garageid == 30 || garageid == 29) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Hier kannst du keine Fahrzeuge einparken!"); 
                        return; 
                    }
                    //Fahrzeug einparken
                    if (vehicle == null) return;
                    if (!vehicle.Position.IsInRange(player.Position, 50f)) return;
                    if (ServerVehicles.GetVehicleType(vehicle) == 4) return;
                    if (vehicle.BodyHealth < 990) { 
                        HUDHandler.SendNotification(player, 4, 7000, $"Das Fahrzeug konnte nicht eingeparkt werden, da es beschädigt ist!<br>Lasse es zuvor von einem Mechaniker Reparieren."); 
                        return; 
                    }
                    if (isHouseGarage)
                    {
                        var houseInfo = ServerHouses.ServerHouses_.ToList().FirstOrDefault(x => x.id == garageid);
                        if (houseInfo == null) return;
                        if (houseInfo.ownerId != charId && !ServerHouses.IsCharacterRentedInHouse(charId, houseInfo.id)) { 
                            HUDHandler.SendNotification(player, 4, 7000, "Dieses Haus gehört dir nicht oder du bist nicht eingemietet."); 
                            return; 
                        }
                        int vehCount = ServerVehicles.ServerVehicles_.ToList().Where(x => x.garageId == garageid && x.isInHouseGarage).Count();
                        if (vehCount >= houseInfo.garageSlots) { HUDHandler.SendNotification(player, 4, 7000, $"Dieses Haus hat kein Platz mehr. Hier passen nur {houseInfo.garageSlots} Fahrzeuge rein.");
                            return; 
                        }
                        ServerVehicles.SetVehicleInGarage(vehicle, false, garageid, true);
                    }
                    else ServerVehicles.SetVehicleInGarage(vehicle, true, garageid, false);
                }
                else if (action == "take")
                {
                    //Fahrzeug ausparken
                    if (isHouseGarage)
                    {
                        var houseInfo = ServerHouses.ServerHouses_.ToList().FirstOrDefault(x => x.id == garageid);
                        if (houseInfo == null) return;
                        if (vehicle != null) { 
                            HUDHandler.SendNotification(player, 4, 7000, $"Ein unerwarteter Fehler ist aufgetreten. [GARAGE-008]"); 
                            return; 
                        }
                        var finalVeh = ServerVehicles.ServerVehicles_.FirstOrDefault(v => v.id == vehID);
                        if (finalVeh == null) { 
                            HUDHandler.SendNotification(player, 4, 7000, $"Ein unerwarteter Fehler ist aufgetreten. [GARAGE-009]"); 
                            return; 
                        }
                        var altVeh = await AltAsync.Do(() => Alt.CreateVehicle((uint)finalVeh.hash, houseInfo.garageVehPos, houseInfo.garageVehRot));
                        altVeh.LockState = VehicleLockState.Locked;
                        altVeh.EngineOn = false;
                        if (finalVeh.plate.StartsWith("NL") && !finalVeh.plate.StartsWith("LSPD") && !finalVeh.plate.StartsWith("LSMD") && !finalVeh.plate.StartsWith("BNY") && !finalVeh.plate.StartsWith("DMV") && !finalVeh.plate.StartsWith("VUC") && !finalVeh.plate.StartsWith("DOJ"))
                        {
                            altVeh.NumberplateText = "__";
                        }
                        else
                        {
                            altVeh.NumberplateText = finalVeh.plate;
                        }
                        altVeh.SetVehicleId((long)finalVeh.id);
                        altVeh.SetVehicleTrunkState(false);
                        ServerVehicles.SetVehicleModsCorrectly(altVeh);
                        ServerVehicles.SetVehicleInGarage(altVeh, false, garageid, false);
                    }
                    else
                    {
                        if (garageid == 30 || garageid == 29) 
                        {
                            int price = 350;
                            if (!CharactersInventory.ExistCharacterItem(charId, "Bargeld", "brieftasche") || CharactersInventory.GetCharacterItemAmount(charId, "Bargeld", "brieftasche") < price)
                            {
                                HUDHandler.SendNotification(player, 3, 7000, "Du hast nicht genügend Geld dabei.");
                                return;
                            }
                            CharactersInventory.RemoveCharacterItemAmount(charId, "Bargeld", price, "brieftasche");
                            HUDHandler.SendNotification(player, 1, 7000, $"Dir wurden ${price} Gebühren abgezogen.");
                        }
                        Position outPos = new Position(0, 0, 0);
                        int curPid = 1;
                        bool slotAreFree = true;
                        foreach (var x in ServerGarages.ServerGarageSlots_.Where(x => x.garageId == garageid))
                        {
                            foreach (var veh in Alt.GetAllVehicles().ToList())
                            {
                                if (veh.Position.IsInRange(ServerGarages.GetGarageSlotPosition(garageid, curPid), 2f))
                                {
                                    slotAreFree = false;
                                    curPid++;
                                    break;
                                }
                                else { slotAreFree = true; }
                            }
                            if (slotAreFree) break;
                        }
                        if (!slotAreFree) { 
                            HUDHandler.SendNotification(player, 4, 7000, $"Es ist kein Parkplatz mehr frei."); 
                            return; 
                        }
                        if (vehicle != null) { 
                            HUDHandler.SendNotification(player, 4, 7000, $"Ein unerwarteter Fehler ist aufgetreten. [GARAGE-002]"); 
                            return; 
                        }
                        var finalVeh = ServerVehicles.ServerVehicles_.FirstOrDefault(v => v.id == vehID);
                        if (finalVeh == null) { 
                            HUDHandler.SendNotification(player, 4, 7000, $"Ein unerwarteter Fehler ist aufgetreten. [GARAGE-001]"); 
                            return; 
                        }
                        var altVeh = await AltAsync.Do(() => Alt.CreateVehicle((uint)finalVeh.hash, ServerGarages.GetGarageSlotPosition(garageid, curPid), (ServerGarages.GetGarageSlotRotation(garageid, curPid))));
                        altVeh.LockState = VehicleLockState.Locked;
                        altVeh.EngineOn = false;
                        if (finalVeh.plate.StartsWith("NL") && !finalVeh.plate.StartsWith("LSPD") && !finalVeh.plate.StartsWith("LSMD") && !finalVeh.plate.StartsWith("BNY") && !finalVeh.plate.StartsWith("DMV") && !finalVeh.plate.StartsWith("VUC") && !finalVeh.plate.StartsWith("DOJ"))
                        {
                            altVeh.NumberplateText = "__";
                        }
                        else
                        {
                            altVeh.NumberplateText = finalVeh.plate;
                        }
                        altVeh.SetVehicleId((long)finalVeh.id);
                        altVeh.SetVehicleTrunkState(false);
                        ServerVehicles.SetVehicleModsCorrectly(altVeh);
                        ServerVehicles.SetVehicleInGarage(altVeh, false, garageid, false);
                    }
                }

                if (!CharactersTablet.HasCharacterTutorialEntryFinished(charId, "useGarage"))
                {
                    CharactersTablet.SetCharacterTutorialEntryState(charId, "useGarage", true);
                    HUDHandler.SendNotification(player, 1, 7000, "Erfolg freigeschaltet: Keine Schäden");
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
    }
}
