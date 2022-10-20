using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Data;
using Altv_Roleplay.Model;
using Altv_Roleplay.Handler;
using System;
using System.Timers;
using Altv_Roleplay.Utils;
using System.Linq;
using AltV.Net.Async;
using Altv_Roleplay.Factories;

namespace Altv_Roleplay
{
    public class Main : AsyncResource
    {
        public override IEntityFactory<IPlayer> GetPlayerFactory()
        {
            return new AccountsFactory();
        }

        public override IBaseObjectFactory<IColShape> GetColShapeFactory()
        {
            return new ColshapeFactory();
        }

        public override IEntityFactory<IVehicle> GetVehicleFactory()
        {
            return new VehicleFactory();
        }

        public override void OnStart()
        {
            AltV.Net.EntitySync.AltEntitySync.Init(7, (threadId) => 200, (threadId) => false,
                (threadCount, repository) => new AltV.Net.EntitySync.ServerEvent.ServerEventNetworkLayer(threadCount, repository),
                (entity, threadCount) => entity.Type,
                (entityId, entityType, threadCount) => entityType,
                (threadId) =>
                {
                    return threadId switch
                    {
                        // Marker
                        0 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 75, 10_000, 10_000, 64),
                        // Text
                        1 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 75, 10_000, 10_000, 32),
                        // Props
                        2 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 100, 10_000, 10_000, 1500),
                        // Help Text
                        3 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 100, 10_000, 10_000, 1),
                        // Blips
                        4 => new EntityStreamer.GlobalEntity(),
                        // Dynamic Blip
                        5 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 175, 10_000, 10_000, 200),
                        // Ped
                        6 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 175, 10_000, 10_000, 64),
                        _ => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 175, 10_000, 10_000, 115),
                    };
                },
            new AltV.Net.EntitySync.IdProvider());

            //Datenbank laden
            Database.DatabaseHandler.ResetDatabaseOnlineState();
            Database.DatabaseHandler.LoadAllPlayers();
            Database.DatabaseHandler.LoadAllPlayerCharacters();
            Database.DatabaseHandler.LoadAllCharacterClothes();
            Database.DatabaseHandler.LoadAllServerNanojobs();
            Database.DatabaseHandler.LoadAllCharacterSkins();
            Database.DatabaseHandler.LoadAllCharacterBankAccounts();
            Database.DatabaseHandler.LoadAllCharacterLastPositions();
            Database.DatabaseHandler.LoadAllCharacterInventorys();
            Database.DatabaseHandler.LoadAllCharacterLicenses();
            Database.DatabaseHandler.LoadAllCharacterPermissions();
            Database.DatabaseHandler.LoadAllCharacterMinijobData();
            Database.DatabaseHandler.LoadAllCharacterPhoneChats();
            Database.DatabaseHandler.LoadAllCharacterWanteds();
            Database.DatabaseHandler.LoadAllServerBlips();
            Database.DatabaseHandler.LoadAllServerMarkers();
            Database.DatabaseHandler.LoadAllServerVehiclesGlobal();
            Database.DatabaseHandler.LoadAllServerAnimations();
            Database.DatabaseHandler.LoadAllServerATMs();
            Database.DatabaseHandler.LoadAllServerBanks();
            Database.DatabaseHandler.LoadAllServerBankPapers();
            Database.DatabaseHandler.LoadAllServerItems();
            Database.DatabaseHandler.LoadAllServerPeds();
            Database.DatabaseHandler.LoadAllClothesShops();
            Database.DatabaseHandler.LoadAllServerShops();
            Database.DatabaseHandler.LoadAllServerShopItems();
            Database.DatabaseHandler.LoadAllServerBarbers();
            Database.DatabaseHandler.LoadAllServerTeleports();
            Database.DatabaseHandler.LoadAllGarages();
            Database.DatabaseHandler.LoadAllGarageSlots();
            Database.DatabaseHandler.LoadAllVehicleMods();
            Database.DatabaseHandler.LoadAllVehicles();
            Database.DatabaseHandler.LoadAllVehicleTrunkItems();
            Database.DatabaseHandler.LoadAllVehicleShops();
            Database.DatabaseHandler.LoadAllVehicleShopItems();
            Database.DatabaseHandler.LoadAllServerFarmingSpots();
            Database.DatabaseHandler.LoadAllServerFarmingProducers();
            Database.DatabaseHandler.LoadAllServerJobs();
            Database.DatabaseHandler.LoadAllServerLicenses();
            Database.DatabaseHandler.LoadAllServerFuelStations();
            Database.DatabaseHandler.LoadALlServerFuelStationSpots();
            Database.DatabaseHandler.LoadAllServerTabletAppData();
            Database.DatabaseHandler.LoadAllCharactersTabletApps();
            Database.DatabaseHandler.LoadAllCharactersTabletTutorialEntrys();
            Database.DatabaseHandler.LoadAllServerTabletEvents();
            Database.DatabaseHandler.LoadAllServerTabletNotes();
            Database.DatabaseHandler.LoadAllServerCompanys();
            Database.DatabaseHandler.LoadAllServerCompanyMember();
            Database.DatabaseHandler.LoadAllServerFactions();
            Database.DatabaseHandler.LoadAllServerFactionRanks();
            Database.DatabaseHandler.LoadAllServerFactionMembers();
            Database.DatabaseHandler.LoadAllServerFactionStorageItems();
            Database.DatabaseHandler.LoadAllServerDoors();
            Database.DatabaseHandler.LoadAllServerHotels();
            Database.DatabaseHandler.LoadAllServerHouses();
            Database.DatabaseHandler.LoadAllServerMinijobBusdriverRoutes();
            Database.DatabaseHandler.LoadAllServerMinijobBusdriverRouteSpots();
            Database.DatabaseHandler.LoadAllServerMinijobGarbageSpots();
            Database.DatabaseHandler.LoadAllServerLogsFaction();
            Database.DatabaseHandler.LoadAllServerLogsCompany();
            Database.DatabaseHandler.LoadAllTattooStuff();
            Database.DatabaseHandler.LoadAllServerMinijoblkwRoutes();
            Database.DatabaseHandler.LoadAllServerMinijoblkwRouteSpots();
            Database.DatabaseHandler.LoadAllServerFactionSkins();

            WeedPlantHandler.LoadAllWeedPots();
            AutomatHandler.LoadAllAutomaten();
            WeatherHandler.GetRealWeatherType();
            WaschstraßenHandler.Load();
            LifeinvaderHandler.Load();
            CraftingHandler.Load();
            AttachmentHandler.Load();
            AnruflistenHandler.Load();

            Minijobs.Müllmann.Main.Initialize();
            Minijobs.Busfahrer.Main.Initialize();
            Minijobs.lkw.Main.Initialize();

            Alt.OnColShape += ColAction;
            Alt.OnClient<IPlayer, string>("Server:Utilities:BanMe", banme);

            ServerPeds.ServerPeds_.Add(new models.Server_Peds { model = "s_m_m_autoshop_02", posX = Constants.Positions.Schluesseldienst_Ped.X, posY = Constants.Positions.Schluesseldienst_Ped.Y, posZ = Constants.Positions.Schluesseldienst_Ped.Z - 1, rotation = Constants.Positions.Schluesseldienst_PedRot });
            ServerBlips.ServerMarkers_.Add(new models.Server_Markers { alpha = 255, bobUpAndDown = false, red = 150, blue = 55, green = 55, type = 1, scaleX = 1, scaleY = 1, scaleZ = 1, posX = Constants.Positions.Schluesseldienst_Pos.X, posY = Constants.Positions.Schluesseldienst_Pos.Y, posZ = Constants.Positions.Schluesseldienst_Pos.Z - 1 });

            System.Timers.Timer checkTimer = new System.Timers.Timer();
            checkTimer.Elapsed += new ElapsedEventHandler(TimerHandler.OnCheckTimer);
            checkTimer.Interval += 15000;
            checkTimer.Enabled = true;

            System.Timers.Timer entityTimer = new System.Timers.Timer();
            entityTimer.Elapsed += new ElapsedEventHandler(TimerHandler.OnEntityTimer);
            entityTimer.Interval += 60000;
            entityTimer.Enabled = true;

            System.Timers.Timer desireTimer = new System.Timers.Timer();
            desireTimer.Elapsed += new ElapsedEventHandler(TimerHandler.OnDesireTimer);
            //desireTimer.Interval += 150000;
            desireTimer.Interval += 110000;
            desireTimer.Enabled = true;

            System.Timers.Timer ApartmentTimer = new System.Timers.Timer();
            ApartmentTimer.Elapsed += new ElapsedEventHandler(TimerHandler.ApartmentTimer);
            ApartmentTimer.Interval += 60000 * 5;
            ApartmentTimer.Enabled = true;

            System.Timers.Timer anticheatTimer = new System.Timers.Timer();
            anticheatTimer.Elapsed += new ElapsedEventHandler(AntiCheatHandler.OnanticheatTimer);
            anticheatTimer.Interval += 250;
            anticheatTimer.Enabled = true;
            
            System.Timers.Timer vehicleTimer = new System.Timers.Timer();
            vehicleTimer.Elapsed += new ElapsedEventHandler(TimerHandler.VehicleTimer);
            vehicleTimer.Interval += 1000; //Kilometerstand alle 0,5s aktualisieren
            vehicleTimer.Enabled = true;

            System.Timers.Timer AtmRobTimer = new System.Timers.Timer();
            AtmRobTimer.Elapsed += new ElapsedEventHandler(TimerHandler.AtmRobTimer);
            AtmRobTimer.Interval += 3600000; //AtmRobs alle 60m resetten
            AtmRobTimer.Enabled = true;

            System.Timers.Timer fuelTimer = new System.Timers.Timer();
            fuelTimer.Elapsed += new ElapsedEventHandler(TimerHandler.FuelTimer);
            fuelTimer.Interval += 7500; //0,02 Liter alle 7,5sek
            fuelTimer.Enabled = true;

            System.Timers.Timer checkMinijobTimer = new System.Timers.Timer();
            checkMinijobTimer.Elapsed += new ElapsedEventHandler(TimerHandler.OnCheckMinijobTimer);
            checkMinijobTimer.Enabled = true;

            System.Timers.Timer hudTimer = new System.Timers.Timer();
            hudTimer.Elapsed += new ElapsedEventHandler(TimerHandler.OnHUDTimer);
            hudTimer.Interval += 250;
            hudTimer.Enabled = true;

            System.Timers.Timer testVehicleTimer = new System.Timers.Timer();
            testVehicleTimer.Elapsed += new ElapsedEventHandler(TimerHandler.VehicleTestTimer);
            testVehicleTimer.Interval += 55000;
            testVehicleTimer.Enabled = true;


            Log.OutputLog($"|--------------- [Anti-cheat {AntiCheat.AnticheatConfig.version}] ---------------|", ConsoleColor.DarkYellow);

            if (AntiCheat.AnticheatConfig.autoheal)
                Log.OutputLog("|------------ Autoheal detection [True] -----------|", ConsoleColor.Green);
            else Log.OutputLog("|----------- Autoheal detection [False] -----------|", ConsoleColor.Red);

            if (AntiCheat.AnticheatConfig.teleport)
                Log.OutputLog("|------------ Teleport detection [True] -----------|", ConsoleColor.Green);
            else Log.OutputLog("|----------- Teleport detection [False] -----------|", ConsoleColor.Red);

            #pragma warning disable CS0162
            if (AntiCheat.AnticheatConfig.death)
                Log.OutputLog("|-------- Death weapon detection [True] -----------|", ConsoleColor.Green);
            else Log.OutputLog("|------- Death weapon detection [False] -----------|", ConsoleColor.Red);
            #pragma warning restore CS0162

            #pragma warning disable CS0162
            if (AntiCheat.AnticheatConfig.death)
                Log.OutputLog("|------- Damage weapon detection [True] -----------|", ConsoleColor.Green);
            else Log.OutputLog("|------ Damage weapon detection [False] -----------|", ConsoleColor.Red);
            #pragma warning restore CS0162

            Log.OutputLog("|---------------- [Anti-cheat Loaded] -------------|", ConsoleColor.DarkYellow);
        }

        private void banme(IPlayer player, string msg)
        {
            try
            {
                if (player == null || !player.Exists || player.AdminLevel() != 0) return;
                int charId = User.GetPlayerOnline(player);
                player.Kick("");
                if (charId <= 0) return;
                User.SetPlayerBanned(Characters.GetCharacterAccountId(charId), true, $"Grund: {msg}");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        private void ColAction(IColShape colShape, IEntity targetEntity, bool state)
        {
            if (colShape == null) return;
            if (!colShape.Exists) return;
            IPlayer client = targetEntity as IPlayer;
            if (client == null || !client.Exists) return;
            string colshapeName = colShape.GetColShapeName();
            long colshapeId = colShape.GetColShapeId();

            if (colshapeName == "Cardealer" && state == true)
            {
                long vehprice = colShape.GetColshapeCarDealerVehPrice();
                string vehname = colShape.GetColshapeCarDealerVehName();
                HUDHandler.SendNotification(client, 1, 2500, $"Name: {vehname}<br>Preis: {vehprice}$");
                return;
            }
            else if (colshapeName == "Nagelband" && state && client.Vehicle != null && client.Vehicle.Exists)
            {
                for (byte i = 1; i <= client.Vehicle.WheelsCount; i++)
                    if (!client.Vehicle.IsWheelBurst(i)) client.Vehicle.SetWheelBurst(i, true);
            }
            else if (colshapeName == "DoorShape" && state)
            {
                var doorData = ServerDoors.ServerDoors_.FirstOrDefault(x => x.id == (int)colshapeId);
                if (doorData == null) return;
                client.EmitLocked("Client:DoorManager:ManageDoor", doorData.hash, new Position(doorData.posX, doorData.posY, doorData.posZ), (bool)doorData.state);
            }
        }

        private void tptoWaypoint(ClassicPlayer player, float x, float y, float z) //ToDo: entfernen
        {
            if (player == null) return;
            player.Position = new Position(x, y, z);
            player.LastPosition = new Position(x, y, z);//by ValeaRP
        }     

        public override void OnStop()
        {
            foreach (var player in Alt.GetAllPlayers().Where(p => p != null && p.Exists)) player.Kick("Server wird heruntergefahren...");
            Alt.Log("Server ist gestoppt.");
        }
    }
}
