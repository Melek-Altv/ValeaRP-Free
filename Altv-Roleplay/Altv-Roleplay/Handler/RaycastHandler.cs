using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Services;
using Altv_Roleplay.Utils;

namespace Altv_Roleplay.Handler
{
    class RaycastHandler : IScript
    {
        public static List<VehicleModel> ForbiddenSeatbelt = new List<VehicleModel>()
        {
            VehicleModel.Bmx,VehicleModel.Cruiser,VehicleModel.Fixter, VehicleModel.Scorcher, VehicleModel.TriBike, VehicleModel.TriBike2, VehicleModel.TriBike3, VehicleModel.Bf400,
            VehicleModel.Akuma, VehicleModel.Avarus, VehicleModel.Bagger, VehicleModel.Bati, VehicleModel.Bati2, VehicleModel.CarbonRs, VehicleModel.Chimera, VehicleModel.Cliffhanger,
            VehicleModel.Daemon, VehicleModel.Daemon2, VehicleModel.Defiler, VehicleModel.Diablous, VehicleModel.Diablous2, VehicleModel.Double, VehicleModel.Enduro, VehicleModel.Esskey,
            VehicleModel.Faggio, VehicleModel.Faggio2, VehicleModel.Faggio3, VehicleModel.Fcr, VehicleModel.Fcr2, VehicleModel.Gargoyle, VehicleModel.Hakuchou, VehicleModel.Hexer,
            VehicleModel.Hakuchou2, VehicleModel.Innovation, VehicleModel.Lectro, VehicleModel.Manchez, VehicleModel.Manchez2, VehicleModel.Nemesis, VehicleModel.Nightblade, VehicleModel.Oppressor,
            VehicleModel.Oppressor2, VehicleModel.Pcj, VehicleModel.Ratbike, VehicleModel.Rrocket, VehicleModel.Ruffian, VehicleModel.Sanchez, VehicleModel.Sanchez2, VehicleModel.Sanctus,
            VehicleModel.Shotaro, VehicleModel.Sovereign, VehicleModel.Stryder, VehicleModel.Thrust, VehicleModel.Vader, VehicleModel.Vindicator, VehicleModel.Vortex, VehicleModel.Wolfsbane,
            VehicleModel.Zombiea, VehicleModel.Zombieb,

       };

        [AsyncClientEvent("Server:InteractionMenu:GetMenuVehicleItems")]
        public static void GetMenuVehicleItems(IPlayer player, string type, IVehicle veh)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (type != "vehicleIn" && type != "vehicleOut") return;
                if (veh == null || !veh.Exists || player == null || !player.Exists) return;
                long vehID = veh.GetVehicleId();
                int charId = (int)player.GetCharacterMetaId();
                bool vehTrunkIsOpen = veh.GetVehicleTrunkState(); //false = zu || true = offen
                if (charId <= 0 || vehID <= 0) return;
                var interactHTML = ""; 
                interactHTML += "<li><p id='InteractionMenu-SelectedTitle'>Schließen</p></li><li class='interactitem' data-action='close' data-actionstring='Schließen'><img src='../utils/img/cancel.png'></li>";
                if (type == "vehicleOut")
                {
                    interactHTML += "<li class='interactitem' id='InteractionMenu-vehtoggleLock' data-action='vehtoggleLock' data-actionstring='Fahrzeug auf/abschließen'><img src='../utils/img/vehlock.png'></li>";
                    interactHTML += "<li class='interactitem' id='InteractionMenu-vehFuelVehicle' data-action='vehFuelVehicle' data-actionstring='Fahrzeug tanken'><img src='../utils/img/vehfuel.png'></li>";

                    if (ServerFactions.IsCharacterInAnyFaction(charId) && ServerFactions.IsCharacterInFactionDuty(charId) && ServerFactions.GetCharacterFactionId(charId) == 2 || ServerFactions.IsCharacterInFactionDuty(charId) && ServerFactions.GetCharacterFactionId(charId) == 1)
                    {
                        interactHTML += "<li class='interactitem' id='InteractionMenu-vehInformationVehicle' data-action='vehInformationVehicle' data-actionstring='Fahrzeug Informationen'><img src='../utils/img/vehicle-information.png'></li>";
                    }

                    if (!ServerVehicles.GetVehicleLockState(veh) && ServerVehicles.GetVehicleType(veh) != 2)
                    {
                        interactHTML += "<li class='interactitem' id='InteractionMenu-vehOpenCloseTrunk' data-action='vehOpenCloseTrunk' data-actionstring='Kofferraum öffnen/schließen'><img src='../utils/img/trunk.png'></li>";
                        interactHTML += "<li class='interactitem' id='InteractionMenu-toggleVehHood' data-action='toggleVehHood' data-actionstring='Motorhaube öffnen/schließen'><img src='../utils/img/motorhaube.png'></li>";
                    }

                    if (vehTrunkIsOpen && ServerVehicles.GetVehicleType(veh) != 2)
                    {
                        interactHTML += "<li class='interactitem' id='InteractionMenu-vehViewTrunkContent' data-action='vehViewTrunkContent' data-actionstring='Kofferraum ansehen'><img src='../utils/img/viewtrunk.png'></li>";
                    }

                    if(CharactersInventory.ExistCharacterItem(charId, "Reparaturkit", "inventory") || CharactersInventory.ExistCharacterItem(charId, "Reparaturkit", "backpack"))
                    {
                        interactHTML += "<li class='interactitem' id='InteractionMenu-vehRepair' data-action='vehRepair' data-actionstring='Fahrzeug reparieren'><img src='../utils/img/repair.png'></li>";
                    }

                    if (ServerFactions.IsCharacterInAnyFaction(charId) && ServerFactions.IsCharacterInFactionDuty(charId) && ServerFactions.GetCharacterFactionId(charId) == 4)
                    {
                        interactHTML += "<li class='interactitem' id='InteractionMenu-towVehicle' data-action='vehTow' data-actionstring='Fahrzeug verwahren'><img src='../utils/img/towvehicle.png'></li>";
                        if (veh.Position.IsInRange(Constants.Positions.AutoClubLosSantos_TuneVehPosition, 35f) && veh.GetVehicleId() > 0)
                        {
                            interactHTML += "<li class='interactitem' id='InteractionMenu-vehTuning' data-action='vehTuning' data-actionstring='Fahrzeug modifizieren'><img src='../utils/img/vehTuning.png'></li>";
                        }
                    }
                }
                else if (type == "vehicleIn")
                {
                    interactHTML += "<li class='interactitem' id='InteractionMenu-vehtoggleLock' data-action='vehtoggleLock' data-actionstring='Fahrzeug auf/abschließen'><img src='../utils/img/vehlock.png'></li>";
                    interactHTML += "<li class='interactitem' id='InteractionMenu-vehtoggleEngine' data-action='vehtoggleEngine' data-actionstring='Motor an/ausmachen'><img src='../utils/img/vehengine.png'></li>";
                    interactHTML += "<li class='interactitem' id='InteractionMenu-toggleSeatbelt' data-action='toggleSeatbelt' data-actionstring='Anschnallen'><img src='../utils/img/viewglovebox.png'></li>";

                    if (player.IsInVehicle && (player.Seat == 1 || player.Seat == 2) && ServerVehicles.GetVehicleType(veh) != 2)
                    {
                        interactHTML += "<li class='interactitem' id='InteractionMenu-vehViewGloveboxContent' data-action='vehViewGloveboxContent' data-actionstring='Handschuhfach ansehen'><img src='../utils/img/viewglovebox.png'></li>";
                    }

                    if (ServerVehicles.GetVehicleOwner(veh) == charId && ServerVehicles.GetVehicleType(veh) != 2)
                    {
                        interactHTML += "<li class='interactitem' id='InteractionMenu-vehchangeowner' data-action='vehchangeowner' data-actionstring='Fahrzeug überschreiben'><img src='../utils/img/fahrzeugC.png'></li>";
                    }

                    if (player.IsInVehicle && !ForbiddenSeatbelt.ToList().Contains((VehicleModel)player.Vehicle.Model) && ServerAllVehicles.GetVehicleClass((long)player.Vehicle.Model) != 7)
                    {
                        interactHTML += "<li class='interactitem' id='InteractionMenu-toggleSeatbelt' data-action='toggleSeatbelt' data-actionstring='Anschnallen'><img src='../utils/img/viewglovebox.png'></li>";
                    }
                }

                player.EmitLocked("Client:RaycastMenu:SetMenuItems", type, interactHTML);
                stopwatch.Stop();
                if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charId} - GetMenuVehicleItems benötigte {stopwatch.Elapsed.Milliseconds}ms");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:InteractionMenu:GetMenuPlayerItems")]
        public static void GetMenuPlayerItems(IPlayer player, string type, IPlayer targetPlayer)
        {
            try
            {
                if (player == null || !player.Exists || type != "player" || targetPlayer == null || !targetPlayer.Exists) return;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                int charId = User.GetPlayerOnline(player);
                int targetCharId = User.GetPlayerOnline(targetPlayer);
                if (charId <= 0) return;
                var interactHTML = "";
                interactHTML += "<li><p id='InteractionMenu-SelectedTitle'>Schließen</p></li><li class='interactitem' data-action='close' data-actionstring='Schließen'><img src='../utils/img/cancel.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-playersupportId' data-action='playersupportId' data-actionstring='Support ID anzeigen'><img src='../utils/img/playersupportid.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-playergiveItem' data-action='playergiveItem' data-actionstring='Gegenstand geben'><img src='../utils/img/order.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-showIdCard' data-action='showIdCard' data-actionstring='Ausweis zeigen'><img src='../utils/img/inventory/Ausweis.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-showLicenses' data-action='showLicenses' data-actionstring='Lizenzen zeigen'><img src='../utils/img/inventory/license.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-playerGiveTakeRopeCuffs' data-action='playerGiveTakeRopeCuffs' data-actionstring='Spieler fesseln/entfesseln'><img src='../utils/img/inventory/Seil.png'></li>";

                if (CharactersInventory.ExistCharacterItem(charId, "Handschellen", "inventory") || CharactersInventory.ExistCharacterItem(charId, "Handschellen", "backpack") || CharactersInventory.ExistCharacterItem(charId, "Handschellenschluessel", "schluessel") || CharactersInventory.ExistCharacterItem(charId, "Handschellenschluessel", "backpack"))
                {
                    interactHTML += "<li class='interactitem' id='InteractionMenu-playerGiveTakeHandcuffs' data-action='playerGiveTakeHandcuffs' data-actionstring='Handschellen an/ablegen'><img src='../utils/img/inventory/Handschellen.png'></li>";
                }

                if(targetPlayer.HasPlayerHandcuffs() || targetPlayer.HasPlayerRopeCuffs())
                {
                    interactHTML += "<li class='interactitem' id='InteractionMenu-playerSearchInventory' data-action='playerSearchInventory' data-actionstring='Spieler durchsuchen'><img src='../utils/img/searchbag.png'></li>";
                }

                if (Characters.GetCharacterStabilisiert(targetCharId) == false && Characters.GetCharacterUnconscious(targetCharId) == true && CharactersInventory.ExistCharacterItem(charId, "EHK", "inventory"))
                {
                    interactHTML += "<li class='interactitem' id='InteractionMenu-playerstabilisieren' data-action='playerstabilisieren' data-actionstring='Spieler stabilisieren'><img src='../utils/img/stabilisieren.png'></li>";
                }

                if (ServerCompanys.IsCharacterInAnyServerCompany(charId))
                {
                    interactHTML += "<li class='interactitem' id='InteractionMenu-playerGiveBill' data-action='playergiveCompanyBill' data-actionstring='Rechnung ausstellen (Unternehmen)'><img src='../utils/img/bill.png'></li>";
                }

                if (ServerFactions.IsCharacterInAnyFaction(charId) && ServerFactions.IsCharacterInFactionDuty(charId))
                {
                    interactHTML += "<li class='interactitem' id='InteractionMenu-playerGiveBill' data-action='playergiveFactionBill' data-actionstring='Rechnung ausstellen (Fraktion)'><img src='../utils/img/bill.png'></li>";

                    if ((ServerFactions.GetCharacterFactionId(charId) == 2 || ServerFactions.GetCharacterFactionId(charId) == 12) && player.Position.IsInRange(Constants.Positions.Arrest_Position, 5f) && targetPlayer.Position.IsInRange(Constants.Positions.Arrest_Position, 5f))
                    {
                        interactHTML += "<li class='interactitem' id='InteractionMenu-playerJail' data-action='playerJail' data-actionstring='Spieler inhaftieren'><img src='../utils/img/jail.png'></li>";
                    }

                    if (ServerFactions.GetCharacterFactionId(charId) == 3 && Characters.IsCharacterUnconscious((int)targetPlayer.GetCharacterMetaId())) {
                        interactHTML += "<li class='interactitem' id='InteractionMenu-playerRevive' data-action='playerRevive' data-actionstring='Spieler wiederbeleben'><img src='../utils/img/revive.png'></li>";
                    }

                    if(ServerFactions.GetCharacterFactionId(charId) == 3 && targetPlayer.Health < 200)
                    {
                        interactHTML += "<li class='interactitem' id='InteractionMenu-HealPlayer' data-action='healPlayer' data-actionstring='Spieler heilen'><img src='../utils/img/inventory/Verbandskasten.png'></li>";
                    }
                }

                if(ServerFactions.IsCharacterInAnyFaction(charId) && ServerFactions.IsCharacterInFactionDuty(charId) && ServerFactions.GetCharacterFactionId(charId) == 5)
                {
                    interactHTML += "<li class='interactitem' id='InteractionMenu-playerGiveLicense' data-action='playerGiveLicense' data-actionstring='Lizenz ausstellen (Fahrschule)'><img src='../utils/img/drivingschool.png'></li>";
                }

                player.EmitLocked("Client:RaycastMenu:SetMenuItems", type, interactHTML);
                stopwatch.Stop();
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:showVehicleInformations")]
        public static void ShowVehicleInformation(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                ClassicVehicle veh = (ClassicVehicle)Alt.GetAllVehicles().ToList().Where(x => player.Position.IsInRange(x.Position, 3f)).OrderBy(x => player.Position.Distance(x.Position)).FirstOrDefault();
                if (veh == null || !veh.Exists || veh.VehicleId <= 0) return;
                string name = Characters.GetCharacterName(ServerVehicles.GetVehicleOwnerById(veh.VehicleId));
                HUDHandler.SendNotification(player, 2, 15000, $"Fahrzeughalter: {name}, <br>Fahrzeugkennzeichen: {ServerVehicles.GetVehiclePlateByOwner(veh.VehicleId)}");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:showJailMenuHUD")]
        public static void ShowJailHUD(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                player.EmitLocked("Client:Jail:showJailMenuHUD");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [AsyncClientEvent("Server:Raycast:jailPlayer")]
        public static void jailPlayer(ClassicPlayer player, int jailTime)
        {

            try
            {
                ClassicPlayer targetPlayer = (ClassicPlayer)Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && x.Position.IsInRange(player.Position, 2f) && x != player);
                if (player == null || !player.Exists || player.CharacterId <= 0 || targetPlayer == null || !targetPlayer.Exists || targetPlayer.CharacterId <= 0 || !ServerFactions.IsCharacterInAnyFaction(player.CharacterId) || !ServerFactions.IsCharacterInFactionDuty(player.CharacterId) || (ServerFactions.GetCharacterFactionId(player.CharacterId) != 2 && ServerFactions.GetCharacterFactionId(player.CharacterId) != 12) || Characters.IsCharacterInJail(targetPlayer.CharacterId)) return;
                HUDHandler.SendNotification(player, 1, 7500, $"Du sitzt nun für {jailTime} Minuten im Gefängnis.");
                Characters.SetCharacterJailTime(targetPlayer.CharacterId, true, jailTime);
                CharactersWanteds.RemoveCharacterWanteds(targetPlayer.CharacterId);
                targetPlayer.Position = new Position(-559.411f, -132.75165f, 33.744995f);
                if (Characters.GetCharacterGender(targetPlayer.CharacterId) == false)
                {
                    targetPlayer.SetClothes(11, 5, 0, 0);
                    targetPlayer.SetClothes(3, 5, 0, 0);
                    targetPlayer.SetClothes(4, 5, 7, 0);
                    targetPlayer.SetClothes(6, 7, 0, 0);
                    targetPlayer.SetClothes(8, 1, 88, 0);
                }
                else
                {
                    targetPlayer.SetClothes(11, 247, 0, 0);
                    targetPlayer.SetClothes(4, 66, 6, 0);
                    targetPlayer.SetClothes(3, 4, 0, 0);
                    targetPlayer.SetClothes(8, 3, 0, 0);
                    targetPlayer.SetClothes(6, 60, 9, 0);
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:showChangeOwnerHUD")]
        public static void ShowChangeOwnerHUD(ClassicPlayer player, ClassicVehicle veh)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || veh == null || !veh.Exists || veh.VehicleId <= 0 || ServerVehicles.GetVehicleOwner(veh) != player.CharacterId) return;
                player.EmitLocked("Client:Vehicle:showChangeOwnerHUD", ServerAllVehicles.GetVehicleNameOnHash(veh.Model));
                Alt.Log($"Fahrzeugmodell› Hash› {veh.Model}");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [AsyncClientEvent("Server:Vehicle:changeVehOwner")]
        public static void ChangeVehOwner(ClassicPlayer player, string targetname)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || targetname == null || player.Vehicle == null || !player.Vehicle.Exists) return;
                ClassicVehicle veh = (ClassicVehicle)player.Vehicle;
                if (ServerVehicles.GetVehicleType(veh) == 4) return;
                var plate = ServerVehicles.GetVehiclePlateByOwner(veh.GetVehicleId());
                targetname = targetname.Replace("_", " ");
                var targetId = Characters.GetCharacterIdFromCharName(targetname);
                ClassicPlayer targetPlayer = (ClassicPlayer)Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId == Characters.GetCharacterIdFromCharName(targetname));
                if (targetPlayer == null || !targetPlayer.Exists)
                {
                    HUDHandler.SendNotification(player, 3, 2500, "Die eingegebene Spieler-ID ist nicht online.");
                    return;
                }

                if (!targetPlayer.Position.IsInRange(player.Position, 13f))
                {
                    HUDHandler.SendNotification(player, 3, 2500, "Der ausgewählte Spieler ist nicht im Umfeld.");
                    return;
                }

                if (veh == null || !veh.Exists || veh.VehicleId <= 0 || ServerVehicles.GetVehicleOwner(veh) != player.CharacterId)
                {
                    HUDHandler.SendNotification(player, 4, 2500, "Nicht dein Fahrzeug! / Fahrzeug nicht erfasst!");
                    return;
                }

                models.Server_Vehicles dbVeh = Model.ServerVehicles.ServerVehicles_.FirstOrDefault(x => x.id == veh.VehicleId);
                if (dbVeh == null) return;

                CharactersInventory.RemoveCharacterItem(player.CharacterId, $"Fahrzeugschluessel {plate}", "schluessel");
                CharactersInventory.AddCharacterItem(targetId, $"Fahrzeugschluessel {plate}", 2, "schluessel");
                HUDHandler.SendNotification(player, 2, 2500, "Du hast das Fahrzeug erfolgreich überschrieben.");
                HUDHandler.SendNotification(targetPlayer, 2, 2500, $"{Characters.GetCharacterName(player.CharacterId)} hat dir sein Fahrzeug überschrieben.");

                dbVeh.charid = targetId;
                using (var db = new models.gtaContext())
                {
                    db.Server_Vehicles.Update(dbVeh);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [AsyncClientEvent("Server:Raycast:OpenVehicleFuelMenu")]
        public static void OpenVehicleFuelMenu(IPlayer player, IVehicle veh)
        {
            try
            {
                if (player == null || !player.Exists || veh == null || !veh.Exists) return;
                int charId = User.GetPlayerOnline(player);
                long vehID = veh.GetVehicleId();
                if (charId <= 0 || vehID <= 0 || player.IsInVehicle) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return;
                }
                if (!player.Position.IsInRange(veh.Position, 5f)) { 
                    HUDHandler.SendNotification(player, 4, 5000, "Du hast dich zu weit vom Fahrzeug entfernt."); 
                    return;
                }
                if(ServerVehicles.GetVehicleEngineState(veh)) { 
                    HUDHandler.SendNotification(player, 4, 5000, "Der Motor vom Fahrzeug muss ausgeschaltet sein."); 
                    return; 
                }
                var fuelSpot = ServerFuelStations.ServerFuelStationSpots_.FirstOrDefault(x => veh.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2.5f));
                if (fuelSpot == null) { 
                    HUDHandler.SendNotification(player, 4, 5000, "Das Fahrzeug befindet sich an keiner Tankstelle. [FEHLERCODE: 001]"); 
                    return; 
                }
                int fuelStationId = ServerFuelStations.GetFuelSpotParentStation(fuelSpot.id);
                if(fuelStationId == 0) { 
                    HUDHandler.SendNotification(player, 4, 5000, "Ein unerwarteter Fehler ist aufgetreten. [FEHLERCODE: 002]"); 
                    return; 
                }
                int availableLiter = ServerFuelStations.GetFuelStationAvailableLiters(fuelStationId);
                if(availableLiter < 1) { 
                    HUDHandler.SendNotification(player, 4, 5000, "Diese Tankstelle hat keinen Treibstoff mehr auf Lager."); 
                    return; 
                }
                var fuelArray = ServerFuelStations.GetFuelStationAvailableFuel(fuelStationId);
                string stationName = ServerFuelStations.GetFuelStationName(fuelStationId);
                string ownerName = ServerFuelStations.GetFuelStationOwnerName(ServerFuelStations.GetFuelStationOwnerId(fuelStationId));
                var maxFuel = ServerVehicles.GetVehicleFuelLimitOnHash(veh.Model);
                var curFuel = Convert.ToInt32(ServerVehicles.GetVehicleFuel(veh));
                maxFuel -= curFuel;

                player.EmitLocked("Client:FuelStation:OpenCEF", fuelStationId, stationName, ownerName, maxFuel, availableLiter, fuelArray, vehID);
            } 
            catch(Exception e) 
            { 
                Alt.Log($"{e}"); 
            }
        }

        [AsyncClientEvent("Server:Raycast:LockVehicle")]
        public static void LockVehicle(IPlayer player, IVehicle veh)
        {
            if (player == null || !player.Exists || veh == null || !veh.Exists) return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int charId = User.GetPlayerOnline(player);
            long vehID = veh.GetVehicleId();
            string vehPlate = ServerVehicles.GetVehiclePlateByOwner(vehID);
            if (charId <= 0 || vehID <= 0) return;
            if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                return; 
            }
            if (!player.Position.IsInRange(veh.Position, 8f)) { 
                HUDHandler.SendNotification(player, 4, 5000, $"Du hast dich zu weit vom Fahrzeug entfernt."); 
                return; 
            }
            if (ServerVehicles.GetVehicleFactionId(veh) == 0 && ServerVehicles.GetVehicleType(veh) == 0 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel " + vehPlate, "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast keinen Schlüssel für dieses Fahrzeug."); return; }
            else if (ServerVehicles.GetVehicleFactionId(veh) != 0 && vehPlate.Contains(ServerFactions.GetFactionShortName(ServerVehicles.GetVehicleFactionId(veh))))
            {
                string factionPlate = ServerFactions.GetFactionShortName(ServerVehicles.GetVehicleFactionId(veh));
                if (!CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel " + factionPlate, "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast keinen Schlüssel für dieses Fahrzeug ({factionPlate})."); return; }
            }
            else if (ServerVehicles.GetVehicleType(veh) == 1)
            {
                if (ServerVehicles.GetVehicleFactionId(veh) == 0) return;
                string factionPlate = ServerFactions.GetFactionShortName(ServerVehicles.GetVehicleFactionId(veh));
                if (!vehPlate.Contains(factionPlate)) return;
                if (!CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel " + factionPlate, "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast keinen Schlüssel für dieses Fahrzeug ({factionPlate})."); return; }
            }
            else if (ServerVehicles.GetVehicleType(veh) == 2 && ServerVehicles.GetVehicleOwner(veh) != charId) { HUDHandler.SendNotification(player, 3, 5000, "Du hast keinen Schlüssel."); return; }
            else if (ServerVehicles.GetVehicleFactionId(veh) != 0 && !vehPlate.Contains(ServerFactions.GetFactionShortName(ServerVehicles.GetVehicleFactionId(veh))) && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel " + vehPlate, "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast keinen Schlüssel für dieses Fahrzeug."); return; }

            bool LockState = ServerVehicles.GetVehicleLockState(veh);
            ServerVehicles.SetVehicleLockState(veh, !LockState);    //Sound
            if (LockState == true)
            {
                InventoryHandler.InventoryAnimation(player, "carlock", 0);
                HUDHandler.SendNotification(player, 2, 2000, "Du hast das Fahrzeug aufgeschlossen.");
                foreach (var client in Alt.GetAllPlayers())
                {
                    if (client.Position.IsInRange(player.Position, 5f))
                    {
                        Global.mGlobal.VirtualAPI.TriggerClientEventSafe(client, "Client:Vehicles:lockUpdate", veh);
                        Global.mGlobal.VirtualAPI.TriggerClientEventSafe(client, "playHowl2d", "./audio/carlock.mp3");
                    }
                    else
                    {
                        Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "Client:Vehicles:lockUpdate", veh);
                        Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "playHowl2d", "./audio/carlock.mp3");
                    }
                }
            }
            else
            {
                InventoryHandler.InventoryAnimation(player, "carlock", 0);
                HUDHandler.SendNotification(player, 4, 2000, "Du hast das Fahrzeug abgeschlossen.");
                Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "Client:Vehicles:ToggleDoorState", veh, 5, false);
                veh.SetVehicleTrunkState(false);
                foreach (var client in Alt.GetAllPlayers())
                {
                    if (client.Position.IsInRange(player.Position, 5f))
                    {
                        Global.mGlobal.VirtualAPI.TriggerClientEventSafe(client, "Client:Vehicles:lockUpdate", veh);
                        Global.mGlobal.VirtualAPI.TriggerClientEventSafe(client, "playHowl2d", "./audio/carlock.mp3");
                    }
                    else
                    {
                        Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "Client:Vehicles:lockUpdate", veh);
                        Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "playHowl2d", "./audio/carlock.mp3");
                    }
                }
            }
            stopwatch.Stop();
        }

        [AsyncClientEvent("Server:Raycast:ToggleVehicleEngine")]
        public static void ToggleVehicleEngine(IPlayer player, IVehicle veh)
        {
            if (player == null || !player.Exists || veh == null || !veh.Exists) return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int charId = User.GetPlayerOnline(player);
            long vehID = veh.GetVehicleId();
            string vehPlate = ServerVehicles.GetVehiclePlateByOwner(vehID);
            if (charId <= 0 || vehID <= 0 || player.Seat != 1) return;
            if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                return; 
            }
            if (!player.Position.IsInRange(veh.Position, 8f)) { 
                HUDHandler.SendNotification(player, 4, 5000, $"Du hast dich zu weit vom Fahrzeug entfernt."); 
                return; 
            }
            if (ServerVehicles.GetVehicleFactionId(veh) == 0 && ServerVehicles.GetVehicleType(veh) == 0 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel " + vehPlate, "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast keinen Schlüssel für dieses Fahrzeug."); return; }
            else if (ServerVehicles.GetVehicleFactionId(veh) != 0 && vehPlate.Contains(ServerFactions.GetFactionShortName(ServerVehicles.GetVehicleFactionId(veh))))
            {
                string factionPlate = ServerFactions.GetFactionShortName(ServerVehicles.GetVehicleFactionId(veh));
                if (!CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel " + factionPlate, "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast keinen Schlüssel für dieses Fahrzeug ({factionPlate})."); return; }
            }
            else if (ServerVehicles.GetVehicleType(veh) == 1)
            {
                if (ServerVehicles.GetVehicleFactionId(veh) == 0) return;
                string factionPlate = ServerFactions.GetFactionShortName(ServerVehicles.GetVehicleFactionId(veh));
                if (!vehPlate.Contains(factionPlate)) return;
                if (!CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel " + factionPlate, "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast keinen Schlüssel für dieses Fahrzeug ({factionPlate})."); return; }
            }
            else if (ServerVehicles.GetVehicleType(veh) == 2 && ServerVehicles.GetVehicleOwner(veh) != charId) { HUDHandler.SendNotification(player, 3, 5000, "Du hast keinen Schlüssel."); return; }
            else if (ServerVehicles.GetVehicleFactionId(veh) != 0 && !vehPlate.Contains(ServerFactions.GetFactionShortName(ServerVehicles.GetVehicleFactionId(veh))) && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel " + vehPlate, "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast keinen Schlüssel für dieses Fahrzeug."); return; }

            bool engineState = ServerVehicles.GetVehicleEngineState(veh);
            if (engineState == false && !ServerVehicles.IsVehicleEngineHealthy(veh)) { HUDHandler.SendNotification(player, 3, 5000, "Dieses Fahrzeug hat einen Motorschaden."); return; }
            if (engineState == false && ServerVehicles.GetVehicleFuel(veh) <= 0) { HUDHandler.SendNotification(player, 3, 5000, "Dieses Fahrzeug hat keinen Treibstoff mehr."); return; }
            ServerVehicles.SetVehicleEngineState(veh, !engineState);
            if (engineState == true) { HUDHandler.SendNotification(player, 4, 2500, "Du hast den Motor ausgeschaltet."); }
            else { HUDHandler.SendNotification(player, 2, 2500, "Du hast den Motor eingeschaltet."); }
            stopwatch.Stop();
        }

        [AsyncClientEvent("Server:Raycast:toggleVehicleHood")]
        public static void ToggleVehicleHood(ClassicPlayer player, ClassicVehicle vehicle)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || vehicle == null || !vehicle.Exists || vehicle.VehicleId <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                if (!player.Position.IsInRange(vehicle.Position, 5f)) { 
                    HUDHandler.SendNotification(player, 4, 5000, $"Du hast dich zu weit vom Fahrzeug entfernt."); 
                    return; 
                }
                if (ServerVehicles.GetVehicleLockState(vehicle)) { HUDHandler.SendNotification(player, 4, 5000, "Das Fahrzeug ist abgeschlossen."); return; }
                bool isHoodOpened = vehicle.GetVehicleHoodState();
                if (!isHoodOpened)
                {
                    vehicle.SetVehicleHoodState(true);
                    Alt.EmitAllClients("Client:Vehicles:ToggleDoorState", vehicle, 4, true);
                    HUDHandler.SendNotification(player, 2, 1000, "Motorhaube geöffnet.");
                    return;
                }
                else
                {
                    vehicle.SetVehicleHoodState(false);
                    Alt.EmitAllClients("Client:Vehicles:ToggleDoorState", vehicle, 4, false);
                    HUDHandler.SendNotification(player, 2, 1000, "Motorhaube geschlossen.");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:toggleSeatbelt")] //Sound
        public static void ToggleSeatbelt(ClassicPlayer player, ClassicVehicle vehicle)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || vehicle == null || !vehicle.Exists || vehicle.VehicleId <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }

                if (!player.isSeatbelt)
                {
                    player.isSeatbelt = true;
                    player.EmitLocked("Client:Seatbelt:ToggleSeatbelt", false);
                    Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "playHowl2d", "./audio/buckle.ogg");
                    HUDHandler.SendNotification(player, 1, 7000, "Du hast dich angeschnallt!");
                    return;
                }
                else if (player.isSeatbelt)
                {
                    player.isSeatbelt = false;
                    player.EmitLocked("Client:Seatbelt:ToggleSeatbelt", true);
                    Global.mGlobal.VirtualAPI.TriggerClientEventSafe(player, "playHowl2d", "./audio/unbuckle.ogg");
                    HUDHandler.SendNotification(player, 4, 7000, "Du hast dich abgeschnallt!");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:OpenCloseVehicleTrunk")]
        public static void OpenCloseVehicleTrunk(IPlayer player, IVehicle veh)
        {
            try
            {
                if (player == null || !player.Exists || veh == null || !veh.Exists) return;
                int charId = User.GetPlayerOnline(player);
                long vehID = veh.GetVehicleId();
                string vehPlate = veh.NumberplateText;
                if (charId <= 0 || vehID <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                if (!player.Position.IsInRange(veh.Position, 5f)) { 
                    HUDHandler.SendNotification(player, 4, 5000, $"Du hast dich zu weit vom Fahrzeug entfernt."); 
                    return; 
                }
                if(ServerVehicles.GetVehicleLockState(veh)) { 
                    HUDHandler.SendNotification(player, 4, 5000, "Das Fahrzeug ist abgeschlossen."); 
                    return; 
                }
                bool isTrunkOpened = veh.GetVehicleTrunkState(); //false = Zu | True = offen
                if(!isTrunkOpened)
                {
                    veh.SetVehicleTrunkState(true);
                    Alt.EmitAllClients("Client:Vehicles:ToggleDoorState", veh, 5, true);
                    HUDHandler.SendNotification(player, 2, 2000, "Du hast den Kofferraum geöffnet.");
                    return;
                } 
                else
                {
                    veh.SetVehicleTrunkState(false);
                    Alt.EmitAllClients("Client:Vehicles:ToggleDoorState", veh, 5, false);
                    HUDHandler.SendNotification(player, 2, 2000, "Du hast den Kofferraum geschlossen");
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:ViewVehicleTrunk")]
        public static void ViewVehicleTrunk(ClassicPlayer player, ClassicVehicle veh)
        {
            try
            {
                if (player == null || !player.Exists || veh == null || !veh.Exists || player.CharacterId <= 0 || veh.VehicleId <= 0) return;
                if (player.IsInVehicle) return;
                if (!player.Position.IsInRange(veh.Position, 5f)) { 
                    HUDHandler.SendNotification(player, 4, 7000, $"Du hast dich zu weit vom Fahrzeug entfernt."); 
                    return; 
                }
                if (ServerVehicles.GetVehicleLockState(veh)) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Das Fahrzeug ist abgeschlossen."); 
                    return; 
                }
                bool isTrunkOpened = veh.GetVehicleTrunkState(); //false = Zu | True = offen
                if(!isTrunkOpened) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Fehler: Der Kofferraum ist zu."); 
                    return; 
                }
                var characterInvArray = CharactersInventory.GetCharacterInventory(player.CharacterId); //Inventar Items
                var vehicleTrunkArray = ServerVehicles.GetVehicleTrunkItems(veh.VehicleId, false); //Kofferraum Items
                var curVehWeight = ServerVehicles.GetVehicleVehicleTrunkWeight(veh.VehicleId, false);
                var maxVehWeight = ServerVehicles.GetVehicleTrunkCapacityOnHash(veh.Model);
                player.EmitLocked("Client:VehicleTrunk:openCEF", player.CharacterId, veh.VehicleId, "trunk", characterInvArray, vehicleTrunkArray, curVehWeight, maxVehWeight); //trunk oder glovebox
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:ViewVehicleGlovebox")]
        public static void ViewVehicleGlovebox(ClassicPlayer player, ClassicVehicle veh)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || veh == null || !veh.Exists || veh.VehicleId <= 0) return;
                if (!player.IsInVehicle) return;
                if (player.Seat != 1 && player.Seat != 2) return;
                var characterInvArray = CharactersInventory.GetCharacterInventory(player.CharacterId); //Inventar Items
                var vehicleGloveboxArray = ServerVehicles.GetVehicleTrunkItems(veh.VehicleId, true); //Handschuhfach Items
                var curVehWeight = ServerVehicles.GetVehicleVehicleTrunkWeight(veh.VehicleId, true);
                var maxVehWeight = 5f;
                player.EmitLocked("Client:VehicleTrunk:openCEF", player.CharacterId, veh.VehicleId, "glovebox", characterInvArray, vehicleGloveboxArray, curVehWeight, maxVehWeight); //trunk oder glovebox
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:showPlayerSupportId")]
        public static void ShowPlayerSupportId(ClassicPlayer player, ClassicPlayer targetPlayer)
        {
            if (player == null || !player.Exists || targetPlayer == null || !targetPlayer.Exists) return;
            int targetCharId = User.GetPlayerCharId(targetPlayer);
            if (targetCharId == 0) return;
            HUDHandler.SendNotification(player, 1, 7000, $"Die Ausweis-ID des Spielers lautet: {targetCharId}");
        }

        [AsyncClientEvent("Server:Raycast:givePlayerItemRequest")]
        public static void GivePlayerItemRequest(IPlayer player, IPlayer targetPlayer)
        {
            if (player == null || !player.Exists || targetPlayer == null || !targetPlayer.Exists) return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int charId = User.GetPlayerOnline(player);
            int targetCharId = User.GetPlayerOnline(targetPlayer);
            if (charId <= 0 || targetCharId <= 0) return;
            if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
            player.EmitLocked("Client:Inventory:CreateInventory", CharactersInventory.GetCharacterInventory(User.GetPlayerOnline(player)), Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(User.GetPlayerOnline(player))), targetCharId);
            stopwatch.Stop();
        }

        [AsyncClientEvent("Server:Raycast:OpenGivePlayerBillCEF")]
        public static void OpenGivePlayerBillCEF(IPlayer player, IPlayer targetPlayer, string type) //Types:  faction | company
        {
            try
            {
                if (player == null || !player.Exists || targetPlayer == null || !targetPlayer.Exists) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                if (type != "faction" && type != "company") return;
                int charId = User.GetPlayerOnline(player);
                int targetCharId = User.GetPlayerOnline(targetPlayer);
                if (charId <= 0 || targetCharId <= 0) return;
                if(type == "faction")
                {
                    if(!ServerFactions.IsCharacterInAnyFaction(charId)) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Du bist in keiner Fraktion.");
                        return; 
                    }
                    if(!ServerFactions.IsCharacterInFactionDuty(charId)) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Du bist nicht im Dienst."); 
                        return; 
                    }
                    int factionId = ServerFactions.GetCharacterFactionId(charId);
                    if (factionId <= 0) return;
                    player.EmitLocked("Client:GivePlayerBill:openCEF", "faction", targetCharId);
                }
                else if(type == "company")
                {
                    if(!ServerCompanys.IsCharacterInAnyServerCompany(charId)) { 
                        HUDHandler.SendNotification(player, 4, 7000, "Du bist in keinem Unternehmen.");
                        return; 
                    }
                    int companyId = ServerCompanys.GetCharacterServerCompanyId(charId);
                    if (companyId <= 0) return;
                    player.EmitLocked("Client:GivePlayerBill:openCEF", "company", targetCharId);
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:openGivePlayerLicenseCEF")]
        public static void OpenGivePlayerLicenseCEF(IPlayer player, IPlayer targetPlayer)
        {
            try
            {
                if (player == null || !player.Exists || targetPlayer == null || !targetPlayer.Exists) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                int charId = User.GetPlayerOnline(player);
                int targetCharId = User.GetPlayerOnline(targetPlayer);
                if (charId <= 0 || targetCharId <= 0) return;
                if(!ServerFactions.IsCharacterInAnyFaction(charId)) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Fehler: Du bist in keiner Fraktion.");
                    return; 
                }
                if(!ServerFactions.IsCharacterInFactionDuty(charId)) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Fehler: Du bist nicht im Dienst."); 
                    return; 
                }
                if(ServerFactions.GetCharacterFactionId(charId) != 5) {
                    HUDHandler.SendNotification(player, 3, 7000, "Fehler: Du bist kein Teil der Fahrschule."); 
                    return; 
                }
                var licArray = CharactersLicenses.GetCharacterLicenses(targetCharId);
                player.EmitLocked("Client:GivePlayerLicense:openCEF", targetCharId, licArray);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:PlayerBill:giveBill")]
        public static void PlayerBillGiveBill(IPlayer player, string type, string reason, int targetCharId, int moneyAmount) //Types:  faction | company
        {
            try
            {
                if (player == null || !player.Exists || targetCharId <= 0 || moneyAmount <= 0 || reason == null || reason == "") return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                if (type != "faction" && type != "company") return;
                int charId = User.GetPlayerOnline(player);
                if (charId == 0) return;
                var targetPlayer = Alt.GetAllPlayers().ToList().FirstOrDefault(x => x.GetCharacterMetaId() == (ulong)targetCharId);
                if (targetPlayer == null || !targetPlayer.Exists) return;
                int factionCompanyId = 0;
                string factionCompanyName = "None";
                if(type == "faction")
                {
                    if(!ServerFactions.IsCharacterInAnyFaction(charId)) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Du bist in keiner Fraktion"); 
                        return; 
                    }
                    if(!ServerFactions.IsCharacterInFactionDuty(charId)) {
                        HUDHandler.SendNotification(player, 3, 7000, "Du bist nicht im Dienst.");
                        return; 
                    }
                    factionCompanyId = ServerFactions.GetCharacterFactionId(charId);
                    factionCompanyName = ServerFactions.GetFactionFullName(factionCompanyId);
                } else if(type == "company")
                {
                    if(!ServerCompanys.IsCharacterInAnyServerCompany(charId)) {
                        HUDHandler.SendNotification(player, 3, 7000, "Du bist in keinem Unternehmen."); 
                        return; 
                    }
                    factionCompanyId = ServerCompanys.GetCharacterServerCompanyId(charId);
                    factionCompanyName = ServerCompanys.GetServerCompanyName(factionCompanyId);
                }
                if (factionCompanyId <= 0 || factionCompanyName == "None" || factionCompanyName == "") return;
                targetPlayer.EmitLocked("Client:RecievePlayerBill:openCEF", type, factionCompanyId, moneyAmount, reason, factionCompanyName, charId);
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }        

        [AsyncClientEvent("Server:PlayerBill:BillAction")]
        public static void PlayerBillAction(IPlayer player, string action, string type, int factionCompanyId, int moneyAmount, string reason, int givenBillOwnerCharId)
        {
            try
            {
                if (player == null || !player.Exists || action == "" || type == "" || factionCompanyId <= 0 || moneyAmount <= 0 || reason == "" || givenBillOwnerCharId <= 0) return;
                if (type != "faction" && type != "company") return;
                if (action != "bar" && action != "bank" && action != "decline") return;
                int targetCharId = User.GetPlayerOnline(player);
                if (targetCharId <= 0) return;
                var givenBillPlayer = Alt.GetAllPlayers().ToList().FirstOrDefault(x => x.GetCharacterMetaId() == (ulong)givenBillOwnerCharId);
                if (givenBillPlayer == null || !givenBillPlayer.Exists) return;
                string factionCompanyName = "None";
                if(type == "faction") { factionCompanyName = ServerFactions.GetFactionFullName(factionCompanyId); }
                else if(type == "company") { factionCompanyName = ServerCompanys.GetServerCompanyName(factionCompanyId); }
                if (factionCompanyName == "None" || factionCompanyName == "" || factionCompanyName == "Zivilist") return;
                DateTime dateTime = DateTime.Now;
                if (action == "bar")
                {
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?");
                        return; 
                    }
                    if (!CharactersInventory.ExistCharacterItem(targetCharId, "Bargeld", "brieftasche")) { 
                        HUDHandler.SendNotification(player, 3, 7000, $"Du hast nicht genügend Bargeld dabei ({moneyAmount}$)."); 
                        HUDHandler.SendNotification(givenBillPlayer, 3, 7000, "Die Person hat nicht genügend Bargeld dabei."); 
                        return; 
                    }
                    if(CharactersInventory.GetCharacterItemAmount(targetCharId, "Bargeld", "brieftasche") < moneyAmount) { 
                        HUDHandler.SendNotification(player, 3, 7000, $"Du hast nicht genügend Bargeld dabei ({moneyAmount}$)."); 
                        HUDHandler.SendNotification(givenBillPlayer, 3, 7000, "Die Person hat nicht genügend Bargeld dabei."); 
                        return;
                    }
                    CharactersInventory.RemoveCharacterItemAmount(targetCharId, "Bargeld", moneyAmount, "brieftasche");
                } else if(action == "bank")
                {
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) {
                        HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?");
                        return;
                    }
                    if (!CharactersBank.HasCharacterBankMainKonto(targetCharId)) {
                        HUDHandler.SendNotification(player, 3, 7000, "Du hast noch kein Hauptkonto in der Bank gesetzt."); 
                        HUDHandler.SendNotification(givenBillPlayer, 3, 7000, "Die Person hat noch kein Hauptkonto gesetzt."); 
                        return; 
                    }
                    int accountNumber = CharactersBank.GetCharacterBankMainKonto(targetCharId);
                    if (accountNumber <= 0) return;
                    if(CharactersBank.GetBankAccountLockStatus(accountNumber)) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Dein Hauptkonto ist aktuell gesperrt."); 
                        HUDHandler.SendNotification(givenBillPlayer, 3, 7000, "Das Hauptkonto der Person ist gesperrt.");
                        return;
                    }
                    if(CharactersBank.GetBankAccountMoney(accountNumber) < moneyAmount) {
                        HUDHandler.SendNotification(player, 3, 7000, $"Dein Bankkonto ist nicht ausreichend gedeckt ({moneyAmount}$)."); 
                        HUDHandler.SendNotification(givenBillPlayer, 3, 7000, "Die Person hat nicht genügend Geld auf ihrem Bankkonto.");
                        return;
                    }
                    CharactersBank.SetBankAccountMoney(accountNumber, CharactersBank.GetBankAccountMoney(accountNumber) - moneyAmount);
                    ServerBankPapers.CreateNewBankPaper(accountNumber, dateTime.ToString("dd.MM.yyyy"), dateTime.ToString("HH.mm"), "Ausgehende Überweisung", $"{factionCompanyName}", $"Rechnungskartenzahlung", $"-{moneyAmount}$", "Online Banking");
                } else if(action == "decline")
                {
                    HUDHandler.SendNotification(givenBillPlayer, 3, 7000, $"Die Person hat die Rechnung i.H.v. {moneyAmount}$ abgelehnt.");
                    HUDHandler.SendNotification(player, 3, 7000, $"Du hast die Rechnung i.H.v. {moneyAmount}$ abgelehnt.");
                    return;
                }

                if(type == "faction")
                {
                    ServerFactions.SetFactionBankMoney(factionCompanyId, ServerFactions.GetFactionBankMoney(factionCompanyId) + moneyAmount);
                    LoggingService.NewFactionLog(factionCompanyId, targetCharId, givenBillOwnerCharId, "bill", $"{Characters.GetCharacterName(targetCharId)} hat die Rechnung von {Characters.GetCharacterName(givenBillOwnerCharId)} i.H.v. {moneyAmount}$ erfolgreich bezahlt ({action}).");
                } else if(type == "company")
                {
                    ServerCompanys.SetServerCompanyMoney(factionCompanyId, ServerCompanys.GetServerCompanyMoney(factionCompanyId) + moneyAmount);
                    LoggingService.NewCompanyLog(factionCompanyId, targetCharId, givenBillOwnerCharId, "bill", $"{Characters.GetCharacterName(targetCharId)} hat die Rechnung von {Characters.GetCharacterName(givenBillOwnerCharId)} i.H.v. {moneyAmount}$ erfolgreich bezahlt ({action}).");
                }

                HUDHandler.SendNotification(player, 2, 7000, $"Du hast die Rechnung i.H.v. {moneyAmount}$ bezahlt (Zahlungsart: {action}).");
                HUDHandler.SendNotification(givenBillPlayer, 2, 7000, $"Die Person hat die Rechnung i.H.v. {moneyAmount}$ bezahlt (Zahlungsart: {action}).");
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:GiveTakeHandcuffs")]
        public static void GiveTakeHandcuffs(IPlayer player, IPlayer targetPlayer)
        {
            try
            {
                if (player == null || targetPlayer == null || !player.Exists || !targetPlayer.Exists) return;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if(!player.Position.IsInRange(targetPlayer.Position, 3f)) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Du bist zu weit entfernt."); 
                    return; 
                }
                int charId = User.GetPlayerOnline(player);
                int targetCharId = User.GetPlayerOnline(targetPlayer);
                if (charId <= 0 || targetCharId <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?");
                    return;
                }
                bool hasTargetHandcuffs = targetPlayer.HasPlayerHandcuffs();
                bool hasTargetRopeCuffs = targetPlayer.HasPlayerRopeCuffs();

                if(hasTargetRopeCuffs) { HUDHandler.SendNotification(player, 3, 7000, "Fehler: Der Spieler ist mit einem Seil gefesselt.");
                    return;
                }

                if (hasTargetHandcuffs)
                {
                    //TargetPlayer hat Handschellen.
                    if (!CharactersInventory.ExistCharacterItem(charId, "Handschellenschluessel", "schluessel")) {
                        HUDHandler.SendNotification(player, 3, 7000, "Fehler: Du hast keinen Schlüssel.");
                        return;
                    }
                    InventoryHandler.StopAnimation(targetPlayer, "mp_arresting", "sprint");
                    targetPlayer.SetPlayerIsCuffed("handcuffs", false);
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, "Dir wurden die Handschellen abgenommen.");

                    float itemWeight = ServerItems.GetItemWeight("Handschellen");
                    float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                    float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                    if (invWeight + itemWeight > 15f && backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId))) { 
                        HUDHandler.SendNotification(player, 3, 7000, $"Du hast nicht genug Platz in deinen Taschen."); 
                        return; 
                    }
                    if (invWeight + itemWeight <= 15f)
                    {
                        HUDHandler.SendNotification(player, 2, 7000, $"Handschellen abgenommen.");
                        CharactersInventory.AddCharacterItem(charId, "Handschellen", 1, "inventory");
                        stopwatch.Stop();
                        return;
                    }

                    if (Characters.GetCharacterBackpack(charId) != "None" && backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
                    {
                        HUDHandler.SendNotification(player, 2, 7000, $"Handschellen abgenommen.");
                        CharactersInventory.AddCharacterItem(charId, "Handschellen", 1, "backpack"); 
                        stopwatch.Stop();
                        return;
                    }
                    return;
                }
                else
                {
                    //TargetPlayer hat keine Handschellen.
                    if(!CharactersInventory.ExistCharacterItem(charId, "Handschellen", "inventory") && !CharactersInventory.ExistCharacterItem(charId, "Handschellen", "backpack")) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Fehler: Du hast keine Handschellen."); 
                        return; 
                    }
                    if(CharactersInventory.ExistCharacterItem(charId, "Handschellen", "inventory") && CharactersInventory.GetCharacterItemAmount(charId, "Handschellen", "inventory") > 0) {
                        CharactersInventory.RemoveCharacterItemAmount(charId, "Handschellen", 1, "inventory");
                    }
                    else if(CharactersInventory.ExistCharacterItem(charId, "Handschellen", "backpack") && CharactersInventory.GetCharacterItemAmount(charId, "Handschellen", "backpack") > 0) { 
                        CharactersInventory.RemoveCharacterItemAmount(charId, "Handschellen", 1, "backpack"); 
                    }
                    InventoryHandler.InventoryAnimation(targetPlayer, "handcuffs", -1);
                    targetPlayer.SetPlayerIsCuffed("handcuffs", true);
                    targetPlayer.GiveWeapon(WeaponModel.Fist, 0, true);
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, "Dir wurden Handschellen angelegt.");
                    HUDHandler.SendNotification(player, 2, 7000, "Handschellen angelegt.");
                    stopwatch.Stop();
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:GiveTakeRopeCuffs")]
        public static void GiveTakeRopeCuffs(IPlayer player, IPlayer targetPlayer)
        {
            try
            {
                if (player == null || targetPlayer == null || !player.Exists || !targetPlayer.Exists) return;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (!player.Position.IsInRange(targetPlayer.Position, 3f)) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Du bist zu weit entfernt.");
                    return; 
                }
                int charId = User.GetPlayerOnline(player);
                int targetCharId = User.GetPlayerOnline(targetPlayer);
                if (charId <= 0 || targetCharId <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) {
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?");
                    return; 
                }
                bool hasTargetHandCuffs = targetPlayer.HasPlayerHandcuffs();
                bool hasTargetRopeCuffs = targetPlayer.HasPlayerRopeCuffs();

                if(hasTargetHandCuffs) { HUDHandler.SendNotification(player, 3, 7000, "Fehler: Der Spieler hat Handschellen an."); 
                    return; 
                }

                if (hasTargetRopeCuffs)
                {
                    //TargetPlayer hat Seilfesseln.
                    InventoryHandler.StopAnimation(targetPlayer, "mp_arresting", "sprint");
                    targetPlayer.SetPlayerIsCuffed("ropecuffs", false);
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, "Dir wurden die Seilfesseln abgenommen.");

                    float itemWeight = ServerItems.GetItemWeight("Seil");
                    float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                    float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                    if (invWeight + itemWeight > 15f && backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId))) { 
                        HUDHandler.SendNotification(player, 3, 7000, $"Du hast nicht genug Platz in deinen Taschen."); 
                        return;
                    }
                    if (invWeight + itemWeight <= 15f)
                    {
                        HUDHandler.SendNotification(player, 2, 7000, $"Seil abgenommen.");
                        CharactersInventory.AddCharacterItem(charId, "Seil", 1, "inventory"); 
                        stopwatch.Stop();
                        return;
                    }

                    if (Characters.GetCharacterBackpack(charId) != "None" && backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
                    {
                        HUDHandler.SendNotification(player, 2, 7000, $"Seil abgenommen.");
                        CharactersInventory.AddCharacterItem(charId, "Seil", 1, "backpack");
                        stopwatch.Stop();
                        return;
                    }
                    return;
                }
                else
                {
                    //TargetPlayer hat keine Seilfesseln.
                    if (!CharactersInventory.ExistCharacterItem(charId, "Seil", "inventory") && !CharactersInventory.ExistCharacterItem(charId, "Seil", "backpack")) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Fehler: Du hast kein Seil dabei."); 
                        return; 
                    }
                    if (CharactersInventory.ExistCharacterItem(charId, "Seil", "inventory") && CharactersInventory.GetCharacterItemAmount(charId, "Seil", "inventory") > 0) { 
                        CharactersInventory.RemoveCharacterItemAmount(charId, "Seil", 1, "inventory"); 
                    }
                    else if (CharactersInventory.ExistCharacterItem(charId, "Seil", "backpack") && CharactersInventory.GetCharacterItemAmount(charId, "Seil", "backpack") > 0) {
                        CharactersInventory.RemoveCharacterItemAmount(charId, "Seil", 1, "backpack"); 
                    }
                    InventoryHandler.InventoryAnimation(targetPlayer, "handcuffs", -1);
                    targetPlayer.SetPlayerIsCuffed("ropecuffs", true);
                    targetPlayer.GiveWeapon(WeaponModel.Fist, 0, true);
                    HUDHandler.SendNotification(targetPlayer, 1, 7000, "Du wurdest mit einem Seil gefesselt.");
                    HUDHandler.SendNotification(player, 2, 7000, "Seil angelegt.");
                    stopwatch.Stop();
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:SearchPlayerInventory")]
        public static void SearchPlayerInventory(IPlayer player, IPlayer targetPlayer)
        {
            try
            {
                if (player == null || targetPlayer == null || !player.Exists || !targetPlayer.Exists) return;
                if (!player.Position.IsInRange(targetPlayer.Position, 3f)) {
                    HUDHandler.SendNotification(player, 3, 7000, "Du bist zu weit entfernt.");
                    return; 
                }
                int charId = User.GetPlayerOnline(player);
                int targetCharId = User.GetPlayerOnline(targetPlayer);
                if (charId <= 0 || targetCharId <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) {
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                if (!targetPlayer.HasPlayerHandcuffs() && !targetPlayer.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Dieser Spieler ist nicht gefesselt.");
                    return; 
                }
                var targetInvArray = CharactersInventory.GetCharacterInventory(targetCharId);
                player.EmitLocked("Client:PlayerSearch:openCEF", targetCharId, targetInvArray);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:PlayerStabilisieren")]
        public static async void PlayerStabilisieren(IPlayer player, IPlayer targetPlayer)
        {
            try
            {
                if (player == null || targetPlayer == null || !player.Exists || !targetPlayer.Exists) return;
                if (!player.Position.IsInRange(targetPlayer.Position, 3f)) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Du bist zu weit entfernt."); 
                    return; 
                }
                int charId = User.GetPlayerOnline(player);
                int targetCharId = User.GetPlayerOnline(targetPlayer);
                if (charId <= 0 || targetCharId <= 0) return;
                if (Characters.GetCharacterStabilisiert(targetCharId) == true) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Der Spieler ist bereits stabilisiert!"); 
                    return; 
                }
                if (Characters.GetCharacterisDead(targetCharId) == true) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Der Spieler ist bereits verstorben.");
                    return; 
                }
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                var bewusstlos = Characters.GetCharacterUnconsciousTime(targetCharId);
                CharactersInventory.RemoveCharacterItemAmount(charId, "EHK", 1, "inventory");
                InventoryHandler.InventoryAnimation(player, "stabilisieren", 15000);
                await Task.Delay(15000);
                HUDHandler.SendNotification(player, 1, 7000, "Du hast jemanden stabilisiert!");
                Characters.SetCharacterUnconscious(targetCharId, true, bewusstlos + 2);
                Characters.IsCharacterStabilisiert(targetCharId, true);

            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:showLicenses")]
        public static void ShowLicenses(ClassicPlayer player, ClassicPlayer targetPlayer)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || targetPlayer == null || !targetPlayer.Exists || targetPlayer.CharacterId <= 0) return;
                player.Emit("Client:License:openLicenseOverview", Characters.GetCharacterName(player.CharacterId), CharactersLicenses.HasCharacterLicense(player.CharacterId, "pkw"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "lkw"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "bike"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "boat"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "fly"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "helicopter"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "passengertransport"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "weaponlicense"));
                targetPlayer.Emit("Client:License:openLicenseOverview", Characters.GetCharacterName(player.CharacterId), CharactersLicenses.HasCharacterLicense(player.CharacterId, "pkw"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "lkw"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "bike"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "boat"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "fly"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "helicopter"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "passengertransport"), CharactersLicenses.HasCharacterLicense(player.CharacterId, "weaponlicense"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:showIdcard")]
        public static void ShowIdCard(IPlayer player, IPlayer targetPlayer)
        {
            try
            {
                if (player == null || targetPlayer == null || !player.Exists || !targetPlayer.Exists) return;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                int charId = (int)player.GetCharacterMetaId();
                int targetId = (int)targetPlayer.GetCharacterMetaId();
                if (charId <= 0 || targetId <= 0) return;
                if (Characters.GetCharacterAccState(charId) <= 0) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                    HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                    return; 
                }
                var data = "[]";
                if(ServerFactions.IsCharacterInAnyFaction(charId) && ServerFactions.IsCharacterInFactionDuty(charId))
                {
                    data = Characters.GetCharacterFactionInformations(charId);
                    if (data == null || data == "[]") return;
                    player.EmitLocked("Client:IdentityCard:showIdentityCard", "faction", data);
                    targetPlayer.EmitLocked("Client:IdentityCard:showIdentityCard", "faction", data);
                    stopwatch.Stop();
                    return;
                }
                data = Characters.GetCharacterInformations(charId);
                if (data == null || data == "[]") return;
                player.EmitLocked("Client:IdentityCard:showIdentityCard", "perso", data);
                targetPlayer.EmitLocked("Client:IdentityCard:showIdentityCard", "perso", data);
                InventoryHandler.InventoryAnimation(player, "give", 0);
                stopwatch.Stop();
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
    }
}
