using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Handler;
using Altv_Roleplay.Model;
using Altv_Roleplay.models;
using Altv_Roleplay.Utils;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Altv_Roleplay.Minijobs.lkw
{
    public class Main : IScript
    {
        public static ClassicColshape startJobShape = (ClassicColshape)Alt.CreateColShapeSphere(Constants.Positions.Minijob_lkw_StartPos, 2f);

        public static void Initialize()
        {
            Alt.Log("Lade Minijob: Lastwagenfahrer...");
            Alt.OnColShape += ColshapeEnterExitHandler;
            Alt.OnPlayerEnterVehicle += PlayerEnterVehicle;
            Alt.OnPlayerLeaveVehicle += PlayerExitVehicle;

            var data = new Server_Peds { model = "cs_tom", posX = startJobShape.Position.X, posY = startJobShape.Position.Y, posZ = startJobShape.Position.Z - 1, rotation = 1.488093614578257f };
            ServerPeds.ServerPeds_.Add(data);
            var markerData = new Server_Markers { type = 39, posX = Constants.Positions.Minijob_lkw_VehOutPos.X, posY = Constants.Positions.Minijob_lkw_VehOutPos.Y, posZ = Constants.Positions.Minijob_lkw_VehOutPos.Z + 1, alpha = 150, bobUpAndDown = true, scaleX = 1, scaleY = 1, scaleZ = 1, red = 212, green = 0, blue = 0 };
            ServerBlips.ServerMarkers_.Add(markerData);
            var markerData2 = new Server_Markers { type = 39, posX = Constants.Positions.Minijob_lkw_VehOutPos2.X, posY = Constants.Positions.Minijob_lkw_VehOutPos2.Y, posZ = Constants.Positions.Minijob_lkw_VehOutPos2.Z + 1, alpha = 150, bobUpAndDown = true, scaleX = 1, scaleY = 1, scaleZ = 1, red = 212, green = 0, blue = 0 };
            ServerBlips.ServerMarkers_.Add(markerData2);
            Alt.Log ("Minijob: Lastwagenfahrer geladen...");

            startJobShape.Radius = 2f;
        }

        private static async void PlayerExitVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            try
            {
                if (player == null || !player.Exists) return;
                if (vehicle == null || !vehicle.Exists) return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                if (ServerVehicles.GetVehicleType(vehicle) != 2) return;
                if (ServerVehicles.GetVehicleOwner(vehicle) != charId) return;
                if (player.GetPlayerCurrentMinijob() != "Lkwfahrer") return;
                if (player.GetPlayerCurrentMinijobStep() != "DRIVE_BACK_TO_START") return;
                if (!vehicle.Position.IsInRange(Constants.Positions.Minijob_lkw_VehOutPos, 8f)) return;
                player.EmitLocked("Client:Minijob:RemoveJobMarker");
                foreach (var veh in Alt.GetAllVehicles().Cast<ClassicVehicle>().Where(x => x != null && x.Exists && x.NumberplateText == $"LKW-{charId}").ToList())
                {
                    if (veh == null || !veh.Exists) continue;
                    ServerVehicles.RemoveVehiclePermanently(veh);
                    await Task.Delay(2000);
                    veh.Remove();
                }
                foreach (var veh2 in Alt.GetAllVehicles().Cast<ClassicVehicle>().Where(x => x != null && x.Exists && x.NumberplateText == $"ANH-{charId}").ToList())
                {
                    if (veh2 == null || !veh2.Exists) continue;
                    ServerVehicles.RemoveVehiclePermanently(veh2);
                    await Task.Delay(2000);
                    veh2.Remove();
                }
                int givenEXP = Model.GetRouteGivenEXP((int)player.GetPlayerCurrentMinijobRouteId());
                int givenMoney = Model.GetRouteGivenMoney((int)player.GetPlayerCurrentMinijobRouteId());
                player.SetPlayerCurrentMinijob("None");
                player.SetPlayerCurrentMinijobStep("None");
                player.SetPlayerCurrentMinijobActionCount(0);
                player.SetPlayerCurrentMinijobRouteId(0);
                CharactersMinijobs.IncreaseCharacterMinijobEXP(charId, "Lkwfahrer", givenEXP);
                if (!CharactersBank.HasCharacterBankMainKonto(charId)) { 
                    HUDHandler.SendNotification(player, 3, 7000, $"Dein Gehalt i.H.v. {givenMoney}$ konnte nicht überwiesen werden da du kein Hauptkonto hast. Du hast aber {givenEXP}EXP erhalten (du hast nun: {CharactersMinijobs.GetCharacterMinijobEXP(charId, "Lkwfahrer")}EXP).");
                    return; 
                }
                int accNumber = CharactersBank.GetCharacterBankMainKonto(charId);
                if (accNumber <= 0) return;
                CharactersBank.SetBankAccountMoney(accNumber, CharactersBank.GetBankAccountMoney(accNumber) + givenMoney);
                ServerBankPapers.CreateNewBankPaper(accNumber, DateTime.Now.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")), DateTime.Now.ToString("t", CultureInfo.CreateSpecificCulture("de-DE")), "Eingehende Überweisung", "Los Santos Transit", "Minijob Gehalt", $"+{givenMoney}$", "Online Banking");
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast den Minijob erfolgreich abgeschlossen. Dein Gehalt i.H.v. {givenMoney}$ wurde dir auf dein Hauptkonto überwiesen. Du hast {givenEXP} erhalten (deine EXP: {CharactersMinijobs.GetCharacterMinijobEXP(charId, "Lkwfahrer")})");
                return;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        private static void PlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            try
            {
                if (player == null || vehicle == null || !player.Exists || !vehicle.Exists) return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                if (ServerVehicles.GetVehicleType(vehicle) != 2) return;
                if (ServerVehicles.GetVehicleOwner(vehicle) != charId) return;
                if (player.GetPlayerCurrentMinijob() != "Lkwfahrer") return;
                if (player.GetPlayerCurrentMinijobStep() == "None") return;
                if(player.GetPlayerCurrentMinijobStep() == "FirstStepInVehicle")
                {
                   var spot = Model.GetCharacterMinijobNextSpot(player);
                   if (spot == null) return;
                   HUDHandler.SendNotification(player, 1, 7000, "Fahre zur ersten Haltestelle und warte dort 10 Sekunden.");
                   player.SetPlayerCurrentMinijobStep("DRIVE_TO_NEXT_STATION");
                   player.EmitLocked("Client:Minijob:CreateJobMarker", "Minijob: Haltestelle", 3, 80, 30, spot.posX, spot.posY, spot.posZ, false);
                   return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        private static void ColshapeEnterExitHandler(IColShape colShape, IEntity targetEntity, bool state)
        {
            try
            {
                if (colShape == null) return;
                if (!colShape.Exists) return;
                IPlayer client = targetEntity as IPlayer;
                if (client == null || !client.Exists) return;
                int charId = User.GetPlayerOnline(client);
                if (charId <= 0) return;
                if (colShape == startJobShape && state)
                {
                    if (client.GetPlayerCurrentMinijob() == "Lkwfahrer") {
                        HUDHandler.SendNotification(client, 1, 7000, "Drücke E um den LKW Minijob zu beenden."); 
                    }
                    else if (client.GetPlayerCurrentMinijob() == "None") { 
                        HUDHandler.SendNotification(client, 1, 7000, "Drücke E um den LKW Minijob zu starten."); 
                    }
                    else if (client.GetPlayerCurrentMinijob() != "None") { 
                        HUDHandler.SendNotification(client, 3, 7000, "Du bist bereits in einem Minijob."); 
                    }
                    return;
                }

                if (client.GetPlayerCurrentMinijob() != "Lkwfahrer") return;
                if (client.GetPlayerCurrentMinijobRouteId() <= 0) return;
                if (client.GetPlayerCurrentMinijobActionCount() <= 0) return;
                if (client.GetPlayerCurrentMinijobStep() == "DRIVE_TO_NEXT_STATION" && state && client.IsInVehicle)
                {
                    var spot = Model.GetCharacterMinijobNextSpot(client);
                    if (spot == null) return;
                    if (colShape != spot.destinationColshape) return;
                    client.EmitLocked("Client:Minijob:RemoveJobMarkerWithFreeze", 10000);
                    int maxSpots = Model.GetMinijobMaxRouteSpots((int)client.GetPlayerCurrentMinijobRouteId());
                    if((int)client.GetPlayerCurrentMinijobActionCount() < maxSpots)
                    {
                       //neuer Punkt
                       client.SetPlayerCurrentMinijobActionCount(client.GetPlayerCurrentMinijobActionCount() + 1);
                       var newSpot = Model.GetCharacterMinijobNextSpot(client);
                       if (newSpot == null) return;
                       client.SetPlayerCurrentMinijobStep("DRIVE_TO_NEXT_STATION");
                       client.EmitLocked("Client:Minijob:CreateJobMarker", "Minijob: Haltestelle", 3, 80, 30, newSpot.posX, newSpot.posY, newSpot.posZ, false);
                       HUDHandler.SendNotification(client, 2, 10000, "An Haltestelle angekommen, warte 10 Sekunden und fahre anschließend zur nächsten Haltestelle.");
                       Alt.Log($"Aktueller Spot || Route: {newSpot.routeId} || SpotID: {newSpot.spotId}");
                       return;
                    }
                    else if((int)client.GetPlayerCurrentMinijobActionCount() >= maxSpots)
                    {
                        //zurueck zum Depot
                        HUDHandler.SendNotification(client, 2, 10000, "An Haltestelle angekommen, warte 10 Sekunden und fahre den LKW anschließend zurück zum Depot und stelle ihn dort ab, wo du ihn bekommen hast.");
                        client.SetPlayerCurrentMinijobStep("DRIVE_BACK_TO_START");
                        client.EmitLocked("Client:Minijob:CreateJobMarker", "Minijob: LKW Abgabe", 3, 515, 30, Constants.Positions.Minijob_lkw_VehOutPos.X, Constants.Positions.Minijob_lkw_VehOutPos.Y, Constants.Positions.Minijob_lkw_VehOutPos.Z, false);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        internal static void TryStartMinijob(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || !((ClassicColshape)startJobShape).IsInRange((ClassicPlayer)player)) return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                if (player.GetPlayerCurrentMinijob() == "Lkwfahrer")
                {
                    //Job abbrechen
                    foreach (var veh in Alt.GetAllVehicles().Cast<ClassicVehicle>().Where(x => x != null && x.Exists && x.NumberplateText == $"LKW-{charId}").ToList())
                    {
                        if (veh == null || !veh.Exists) continue;
                        ServerVehicles.RemoveVehiclePermanently(veh);
                        veh.Remove();
                    }
                    foreach (var veh in Alt.GetAllVehicles().Cast<ClassicVehicle>().Where(x => x != null && x.Exists && x.NumberplateText == $"ANH-{charId}").ToList())
                    {
                        if (veh == null || !veh.Exists) continue;
                        ServerVehicles.RemoveVehiclePermanently(veh);
                        veh.Remove();
                    }
                    HUDHandler.SendNotification(player, 2, 7000, "Du hast den Minijob: Lkwfahrer beendet.");
                    player.SetPlayerCurrentMinijob("None");
                    player.SetPlayerCurrentMinijobRouteId(0);
                    player.SetPlayerCurrentMinijobStep("None");
                    player.SetPlayerCurrentMinijobActionCount(0);
                    player.EmitLocked("Client:Minijob:RemoveJobMarker");
                    return;
                }
                else if (player.GetPlayerCurrentMinijob() == "None")
                {
                    //Levelauswahl anzeigen
                    if (!CharactersMinijobs.ExistCharacterMinijobEntry(charId, "Lkwfahrer"))
                    {
                        CharactersMinijobs.CreateCharacterMinijobEntry(charId, "Lkwfahrer");
                    }
                    var availableRoutes = Model.GetAvailableRoutes(charId);
                    if (availableRoutes == "[]") return;
                    player.EmitLocked("Client:MinijobLKWdriver:openCEF", availableRoutes);
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Minijoblkw:StartJob")]
        public static void StartMiniJob(IPlayer player, int routeId)
        {
            try
            {
                if (player == null || !player.Exists || routeId <= 0) return;
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                if (player.GetPlayerCurrentMinijob() != "None") return;
                if (!Model.ExistRoute(routeId)) return;
                if (CharactersMinijobs.GetCharacterMinijobEXP(charId, "Lkwfahrer") < Model.GetRouteNeededEXP(routeId)) { 
                    HUDHandler.SendNotification(player, 3, 7000, $"Du hast nicht die benötigen EXP für diese Linie ({Model.GetRouteNeededEXP(routeId)}EXP - du hast {CharactersMinijobs.GetCharacterMinijobEXP(charId, "Lkwfahrer")}EXP).");
                    return;
                }
                foreach (var veh in Alt.Core.GetVehicles().ToList())
                {
                    if (veh == null || !veh.Exists) continue;
                    if (veh.Position.IsInRange(Constants.Positions.Minijob_lkw_VehOutPos, 8f)) {
                        HUDHandler.SendNotification(player, 3, 7000, "Der Ausparkpunkt ist blockiert."); 
                        return;
                    }
                }
                ServerVehicles.CreateVehicle((long)Model.GetRouteVehicleHash(routeId), charId, 2, 0, false, 0, Constants.Positions.Minijob_lkw_VehOutPos, Constants.Positions.Minijob_lkw_VehOutRot, $"LKW-{charId}", 132, 132, 132, false);
                ServerVehicles.CreateVehicle((long)Model.GetRouteVehicleHash2(routeId), charId, 2, 0, false, 0, new Position(Constants.Positions.Minijob_lkw_VehOutPos2.X, Constants.Positions.Minijob_lkw_VehOutPos2.Y, Constants.Positions.Minijob_lkw_VehOutPos2.Z + 3), Constants.Positions.Minijob_lkw_VehOutRot2, $"ANH-{charId}", 132, 132, 132, false);
                player.SetPlayerCurrentMinijob("Lkwfahrer");
                player.SetPlayerCurrentMinijobStep("FirstStepInVehicle");
                player.SetPlayerCurrentMinijobRouteId(routeId);
                player.SetPlayerCurrentMinijobActionCount(1);
                HUDHandler.SendNotification(player, 1, 7000, "Du hast den Minijob begonnen. Wir haben dir einen LKW am Tor ausgeparkt, steige ein, und koppel den Anhänger an.");
                return;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
    }
}
