
using System;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;

namespace Altv_Roleplay.Handler
{
    class KeyHandler : IScript
    {
        [AsyncClientEvent("Server:KeyHandler:PressE")]
        [Obsolete]
        public static void PressE(IPlayer player)
        {
            lock (player)
            {
                if (player == null || !player.Exists) return;
                int charId = User.GetPlayerOnline(player);
                if (charId == 0) return;

                ClassicColshape farmCol = (ClassicColshape)ServerFarmingSpots.ServerFarmingSpotsColshapes_.FirstOrDefault(x => ((ClassicColshape)x).IsInRange((ClassicPlayer)player));
                if (farmCol != null && !player.IsInVehicle)
                {
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                        return; 
                    }
                    if (player.GetPlayerFarmingActionMeta() != "None") return;
                    var farmColData = ServerFarmingSpots.ServerFarmingSpots_.FirstOrDefault(x => x.id == (int)farmCol.GetColShapeId());

                    if (farmColData != null)
                    {
                        if (farmColData.neededItemToFarm != "None")
                        {
                            if (!CharactersInventory.ExistCharacterItem(charId, farmColData.neededItemToFarm, "inventory") && !CharactersInventory.ExistCharacterItem(charId, farmColData.neededItemToFarm, "backpack")) { 
                                HUDHandler.SendNotification(player, 3, 7000, $"Zum Farmen benötigst du: {farmColData.neededItemToFarm}."); 
                                return; 
                            }
                        }
                        player.SetPlayerFarmingActionMeta("farm");
                        FarmingHandler.FarmFieldAction(player, farmColData.itemName, farmColData.itemMinAmount, farmColData.itemMaxAmount, farmColData.animation, farmColData.duration);
                        return;
                    }
                }

                ClassicColshape farmProducerCol = (ClassicColshape)ServerFarmingSpots.ServerFarmingProducerColshapes_.FirstOrDefault(x => ((ClassicColshape)x).IsInRange((ClassicPlayer)player));
                if (farmProducerCol != null && !player.IsInVehicle)
                {
                    if (player.GetPlayerFarmingActionMeta() != "None") { HUDHandler.SendNotification(player, 3, 7000, $"Warte einen Moment."); return; }
                    var farmColData = ServerFarmingSpots.ServerFarmingProducer_.FirstOrDefault(x => x.id == (int)farmProducerCol.GetColShapeId());
                    if (farmColData != null)
                    {
                        FarmingHandler.ProduceItem(player, farmColData.neededItem, farmColData.producedItem, farmColData.neededItemAmount, farmColData.producedItemAmount, farmColData.duration);
                        return;
                    }
                }

                if (player.Position.IsInRange(Constants.Positions.Schluesseldienst_Pos, 3f) && !player.IsInVehicle)
                {
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                        HUDHandler.SendNotification(player, 3, 57000000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                        return; 
                    }
                    VehicleHandler.OpenChangeKeyCEF((ClassicPlayer)player);
                    return;
                }

                if(((ClassicColshape)Minijobs.Müllmann.Main.startJobShape).IsInRange((ClassicPlayer)player))
                {
                    Minijobs.Müllmann.Main.StartMinijob(player);
                    return;
                }

                if(((ClassicColshape)Minijobs.Busfahrer.Main.startJobShape).IsInRange((ClassicPlayer)player))
                {
                    Minijobs.Busfahrer.Main.TryStartMinijob(player);
                    return;
                }

                if (((ClassicColshape)Minijobs.lkw.Main.startJobShape).IsInRange((ClassicPlayer)player))
                {
                    Minijobs.lkw.Main.TryStartMinijob(player);
                    return;
                }

                Position waschstraßePos = WaschstraßenHandler.Positions_.ToList().FirstOrDefault(x => player.Position.IsInRange(x, 3f));
                if (waschstraßePos != new Position(0, 0, 0) && player.Vehicle != null)
                {
                    WaschstraßenHandler.ClearVehicleDirt((ClassicPlayer)player, waschstraßePos);
                    return;
                }

                if (player.Position.IsInRange(LifeinvaderHandler.lifeinvaderPos, 2f) && !player.IsInVehicle)
                {
                    player.Emit("Client:Lifeinvader:openCEF");
                    return;
                }

                Nagelband nagelband = NagelbandHandler.NagelbandList_.ToList().FirstOrDefault(x => x.colshape != null && x.colshape.Position.IsInRange(player.Position, 2.5f));
                if (nagelband != null && !player.IsInVehicle)
                {
                    if (!CharactersInventory.ExistCharacterItem(charId, "Werkzeugkasten", "inventory") && !CharactersInventory.ExistCharacterItem(charId, "Werkzeugkasten", "backpack")) { 
                        HUDHandler.SendNotification(player, 3, 2500, "Du hast keinen Werkzeugkasten dabei."); 
                        return; 
                    }
                    NagelbandHandler.DeleteNagelband(nagelband.id);
                    CharactersInventory.AddCharacterItem(charId, "Nagelband", 1, "inventory");
                    HUDHandler.SendNotification(player, 2, 7000, "Nagelband erfolgreich abgebaut..");
                    return;
                }

                var craftingStation = CraftingHandler.ServerCraftingStations_.ToList().FirstOrDefault(x => x.pos.IsInRange(player.Position, 2f));
                if (craftingStation != null && !player.IsInVehicle)
                {
                    if (((ClassicPlayer)player).IsUsingCrafter == true) return;
                    player.Emit("Client:CrafingStation:open", craftingStation.id, CraftingHandler.GetCraftingRecipes(craftingStation.id));
                    return;
                }

                var houseEntrance = ServerHouses.ServerHouses_.FirstOrDefault(x => ((ClassicColshape)x.entranceShape).IsInRange((ClassicPlayer)player));
                if (houseEntrance != null && player.Dimension == 0)
                {
                    var renter = ServerHouses.GetHouseRenters(houseEntrance.id, charId);
                    Alt.Log($"{renter}");
                    if (houseEntrance.ownerId != charId && renter != charId)
                    {
                        HouseHandler.OpenEntranceCEF(player, houseEntrance.id);
                        return;
                    }
                    else if (ServerHouses.IsHouseLocked(houseEntrance.id) == true && houseEntrance.ownerId != 0)
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Dieses Haus ist zugeschlossen!");
                        return;
                    }
                    else if (ServerHouses.IsHouseLocked(houseEntrance.id) == false && houseEntrance.ownerId != 0)
                    {
                        HouseHandler.EnterHouse(player, houseEntrance.id);
                        return;
                    }
                }

                var hotelPos = ServerHotels.ServerHotels_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                if (hotelPos != null && !player.IsInVehicle)
                {
                    HotelHandler.OpenCEF(player, hotelPos);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.Aservatenkammer, 2f) && ServerFactions.IsCharacterInAnyFaction(charId) && ServerFactions.IsCharacterInFactionDuty(charId) && ServerFactions.GetCharacterFactionId(charId) == 2)
                {
                    InventoryHandler.Aservatenkammer(player);
                    return;
                }

                if (player.Dimension >= 5000)
                {
                    int houseInteriorCount = ServerHouses.GetMaxInteriorsCount();
                    for (var i = 1; i <= houseInteriorCount; i++)
                    {
                        if (i > houseInteriorCount || i <= 0) continue;
                        if ((player.Dimension >= 5000 && player.Dimension < 10000) && player.Position.IsInRange(ServerHouses.GetInteriorExitPosition(i), 2f))
                        {
                            //Apartment Leave
                            HotelHandler.LeaveHotel(player);
                            return;
                        }
                        else if ((player.Dimension >= 5000 && player.Dimension < 10000) && player.Position.IsInRange(ServerHouses.GetInteriorStoragePosition(i), 2f))
                        {
                            //Apartment Storage
                            HotelHandler.OpenStorage(player);
                            return;
                        }
                        else if (player.Dimension >= 10000 && player.Position.IsInRange(ServerHouses.GetInteriorExitPosition(i), 2f))
                        {
                            //House Leave
                            HouseHandler.LeaveHouse(player, i);
                            return;
                        }
                        else if (player.Dimension >= 10000 && player.Position.IsInRange(ServerHouses.GetInteriorStoragePosition(i), 2f))
                        {
                            //House Storage
                            HouseHandler.OpenStorage(player);
                            return;
                        }
                        else if (player.Dimension >= 10000 && player.Position.IsInRange(ServerHouses.GetInteriorManagePosition(i), 2f))
                        {
                            //Hausverwaltung
                            HouseHandler.OpenManageCEF(player);
                            return;
                        }
                    }
                }

                var teleportsPos = ServerItems.ServerTeleports_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 1.5f));
                if (teleportsPos != null && !player.IsInVehicle)
                {
                    player.Position = new Position(teleportsPos.targetX, teleportsPos.targetY, teleportsPos.targetZ + 0.5f);
                    return;
                }

                var shopPos = ServerShops.ServerShops_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 3f));
                if (shopPos != null && !player.IsInVehicle)
                {
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                    ShopHandler.OpenShop(player, shopPos);
                    return;
                }

                var shopRob = ServerShops.ServerShops_.FirstOrDefault(x => player.Position.IsInRange(x.robPos, 3f));
                if (shopRob != null && !player.IsInVehicle)
                {
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                    ShopHandler.RobShopAsync((ClassicPlayer)player, shopRob.shopId);
                    return;
                }

                var houseGarage = ServerHouses.ServerHouses_.ToList().FirstOrDefault(x => x.garageSlots > 0 && player.Position.IsInRange(x.garagePos, 2f));
                if (houseGarage != null && !player.IsInVehicle)
                {
                    GarageHandler.OpenGarageCEF((ClassicPlayer)player, houseGarage.id, true);
                    return;
                }

                var garagePos = ServerGarages.ServerGarages_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                if (garagePos != null && !player.IsInVehicle)
                {
                    GarageHandler.OpenGarageCEF((ClassicPlayer)player, garagePos.id, false);
                    return;
                }

                var clothesStoragePos = ServerClothesStorages.ServerClothesStorages_.ToList().FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 1f));
                if(clothesStoragePos != null && !player.IsInVehicle)
                {
                    ServerClothesStorages.RequestClothesStorage((ClassicPlayer)player, clothesStoragePos.id);
                    return;
                }

                var clothesShopPos = ServerClothesShops.ServerClothesShops_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                if(clothesShopPos != null && !player.IsInVehicle)
                {
                    ShopHandler.OpenClothesShop((ClassicPlayer)player, clothesShopPos.id);
                    return;
                }

                var nanojobsPos = ServerNanojobs.Servernanojobs_.ToList().Where(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 1f) && x.jobName == "Angeln");
                if (nanojobsPos != null && !player.IsInVehicle)
                {
                    NanojobsHandler.AngelnAction((ClassicPlayer)player, charId);
                }

                var vehicleShopPos = ServerVehicleShops.ServerVehicleShops_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.pedX, x.pedY, x.pedZ), 2f));
                if (vehicleShopPos != null && !player.IsInVehicle)
                {
                    if (vehicleShopPos.neededLicense != "None" && !Characters.HasCharacterPermission(charId, vehicleShopPos.neededLicense)) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf."); return; }
                    if (vehicleShopPos.id == 1 && ServerFactions.GetCharacterFactionId(charId) != 2) { HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf."); return; }   // Polizeifahrzeugshop
                    if (vehicleShopPos.id == 2 && ServerFactions.GetCharacterFactionId(charId) != 2) { HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf."); return; }   // Polizeihelikoptershop
                    if (vehicleShopPos.id == 3 && ServerFactions.GetCharacterFactionId(charId) != 3) { HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf."); return; }   // Medicfahrzeugshop
                    if (vehicleShopPos.id == 4 && ServerFactions.GetCharacterFactionId(charId) != 3) { HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf."); return; }   // Medichelikoptershop
                    if (vehicleShopPos.id == 5 && ServerFactions.GetCharacterFactionId(charId) != 4) { HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf."); return; }   // Mechanikerfahrzeugshop
                    if (vehicleShopPos.id == 6 && ServerFactions.GetCharacterFactionId(charId) != 5) { HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf."); return; }   // Fahrschulshop
                    if (vehicleShopPos.id == 7 && ServerFactions.GetCharacterFactionId(charId) != 6) { HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf."); return; }   // Vanilla Unicorn Shop
                    if (vehicleShopPos.id == 8 && ServerFactions.GetCharacterFactionId(charId) != 1) { HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf."); return; }   // DOJ SHOP
                    if (vehicleShopPos.id == 9 && ServerFactions.GetCharacterFactionId(charId) != 1) { HUDHandler.SendNotification(player, 3, 7000, $"Du hast hier keinen Zugriff drauf."); return; }   // DOJ HELI Shop
                    ShopHandler.OpenVehicleShop(player, vehicleShopPos.name, vehicleShopPos.id);
                    return;
                }

                var bankPos = ServerBanks.ServerBanks_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 1f));
                if (bankPos != null && !player.IsInVehicle)
                {
                    if (bankPos.zoneName == "Maze Bank Fraktion")
                    {
                        if(!ServerFactions.IsCharacterInAnyFaction(charId)) return;
                        if(ServerFactions.GetCharacterFactionRank(charId) != ServerFactions.GetFactionMaxRankCount(ServerFactions.GetCharacterFactionId(charId)) && ServerFactions.GetCharacterFactionRank(charId) != ServerFactions.GetFactionMaxRankCount(ServerFactions.GetCharacterFactionId(charId)) - 1) { return; }
                        player.EmitLocked("Client:FactionBank:createCEF", "faction", ServerFactions.GetCharacterFactionId(charId), ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId)));
                        return;
                    }
                    else if(bankPos.zoneName == "Maze Bank Company")
                    {
                        if (!ServerCompanys.IsCharacterInAnyServerCompany(charId)) return;
                        if(ServerCompanys.GetCharacterServerCompanyRank(charId) != 1 && ServerCompanys.GetCharacterServerCompanyRank(charId) != 2) { 
                            HUDHandler.SendNotification(player, 3, 7000, "Du hast kein Unternehmen auf welches du zugreifen kannst."); 
                            return; 
                        }
                        player.EmitLocked("Client:FactionBank:createCEF", "company", ServerCompanys.GetCharacterServerCompanyId(charId), ServerCompanys.GetServerCompanyMoney(ServerCompanys.GetCharacterServerCompanyId(charId)));
                        return;
                    }
                    else
                    {
                        var bankArray = CharactersBank.GetCharacterBankAccounts(charId);
                        player.EmitLocked("Client:Bank:createBankAccountManageForm", bankArray, bankPos.zoneName);
                        return;
                    }
                }             

                var barberPos = ServerBarbers.ServerBarbers_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                if (barberPos != null && !player.IsInVehicle)
                {
                    player.EmitLocked("Client:Barber:barberCreateCEF", Characters.GetCharacterHeadOverlay1(charId), Characters.GetCharacterHeadOverlay2(charId), Characters.GetCharacterHeadOverlay3(charId));
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.Surgery, 2f) && !player.IsInVehicle)
                {
                    player.EmitLocked("Client:Surgery:surgeryCreateCEF", Characters.GetCharacterHeadOverlay1(charId), Characters.GetCharacterHeadOverlay2(charId), Characters.GetCharacterHeadOverlay3(charId), Characters.GetCharacterFaceFeatures(charId));
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.NameChange, 2f) && !player.IsInVehicle)
                {
                    player.EmitLocked("Client:Townhall:showChangePlayerNameHUD", Characters.GetCharacterName(charId));
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.EinreiseNPC, 2f) && !player.IsInVehicle)
                {
                    foreach (var inni in User.Player.Where(x => x.Online == charId))
                    {
                        if (DateTime.Now.Subtract(inni.Einreisedate).TotalMinutes <= 5 && inni.EinreiseisUsed == true)
                        {
                            if (inni.EinreiseIsNotifySended == false)
                            {
                                HUDHandler.SendNotification(player, 3, 7000, "Du kannst nur alle 5 Minuten die Klingel benutzen.");
                            }
                            inni.EinreiseIsNotifySended = true;
                            return;
                        }
                        if (DateTime.Now.Subtract(inni.Einreisedate).TotalMinutes >= 5)
                        {
                            inni.EinreiseisUsed = false;
                            inni.EinreiseIsNotifySended = false;
                        }
                        inni.EinreiseisUsed = true;
                        inni.Einreisedate = DateTime.Now;
                    }
                    if (Characters.GetAdminMember() == false)
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Es befindet sich derzeit kein Einreisebeamte im Dienst, es wurde weitergeleitet.");
                        DiscordLog.SendEmbed("einreiseamt", "Einreiselog", Characters.GetCharacterName((int)player.GetCharacterMetaId()) + "wartet im Einreiseamt auf einen Beamten");
                    }
                    else
                    {
                        HUDHandler.SendNotification(player, 1, 7000, "Das Einreiseamt wurde informiert, bitte um Geduld.");
                        foreach (var admin in Alt.GetAllPlayers().Where(x => x != null && x.Exists && x.AdminLevel() > 0 && x.AdminLevel() < 10))
                        {
                            HUDHandler.SendNotification(admin, 1, 10000, $"Der Bürger {Characters.GetCharacterName(charId)} mit der AccountID ({Characters.GetCharacterAccountId(charId)}) wartet im Einreiseamt.");
                        }
                    }
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.VehicleLicensing_Position, 3f))
                {
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                        HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                        return; 
                    }
                    VehicleHandler.OpenLicensingCEF(player);
                    return;
                }

                if (ServerFactions.IsCharacterInAnyFaction(charId))
                {
                    int factionId = ServerFactions.GetCharacterFactionId(charId);
                    var factionSkinPos = ServerFactions.ServerFactionPositions_.FirstOrDefault(x => x.factionId == factionId && x.posType == "skin" && player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 1.5f));
                    if (factionSkinPos != null && !player.IsInVehicle)
                    {
                        if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                        bool isDuty = ServerFactions.IsCharacterInFactionDuty(charId);
                        if (isDuty)
                        {
                            FactionSkinsHandler.OpenFactionSkinsMenu((ClassicPlayer)player);
                            return;
                        }
                        else
                        {
                            HUDHandler.SendNotification(player, 4, 5000, "Du bist nicht zum Dienst angemeldet");
                            return;
                        }
                    }

                    var factionDutyPos = ServerFactions.ServerFactionPositions_.FirstOrDefault(x => x.factionId == factionId && x.posType == "duty" && player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                    if (factionDutyPos != null && !player.IsInVehicle)
                    {
                        bool isDuty = ServerFactions.IsCharacterInFactionDuty(charId);
                        ServerFactions.SetCharacterInFactionDuty(charId, !isDuty);
                        if (isDuty)
                        {
                            HUDHandler.SendNotification(player, 4, 7000, "Du hast dich erfolgreich vom Dienst abgemeldet.");
                        }
                        else
                        {
                            HUDHandler.SendNotification(player, 2, 7000, "Du hast dich erfolgreich zum Dienst angemeldet.");
                        }
                        if (factionId == 2 || factionId == 12) SmartphoneHandler.RequestLSPDIntranet((ClassicPlayer)player);
                        return;
                    }

                    var factionStoragePos = ServerFactions.ServerFactionPositions_.FirstOrDefault(x => x.factionId == factionId && x.posType == "storage" && player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                    if(factionStoragePos != null && !player.IsInVehicle)
                    {
                        if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                            HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                            return;
                        }
                        bool isDuty = ServerFactions.IsCharacterInFactionDuty(charId);
                        if(isDuty)
                        {
                            var factionStorageContent = ServerFactions.GetServerFactionStorageItems(factionId, charId); //Fraktionsspind Items
                            var CharacterInvArray = CharactersInventory.GetCharacterInventory(charId); //Spieler Inventar
                            player.EmitLocked("Client:FactionStorage:openCEF", charId, factionId, "faction", CharacterInvArray, factionStorageContent);
                            return;
                        }
                    }

                    var factionServicePhonePos = ServerFactions.ServerFactionPositions_.ToList().FirstOrDefault(x => x.factionId == factionId && x.posType == "servicephone" && player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                    if (factionServicePhonePos != null && !player.IsInVehicle && ServerFactions.IsCharacterInFactionDuty(charId))
                    {
                        int activeLeitstelle = ServerFactions.GetCurrentServicePhoneOwner(factionId);

                        if (activeLeitstelle <= 0)
                        {
                            ServerFactions.UpdateCurrentServicePhoneOwner(factionId, charId);
                            ServerFactions.sendMsg(factionId, $"{Characters.GetCharacterName(charId)} hat das Leitstellentelefon deiner Fraktion übernommen.");
                            return;
                        }
                        if (activeLeitstelle != charId)
                        {
                            HUDHandler.SendNotification(player, 2, 7000, $"Die Leitstelle ist aktuell vom Mitarbeiter {Characters.GetCharacterName(activeLeitstelle)} besetzt.");
                            return;
                        }
                        if (activeLeitstelle == charId)
                        {
                            ServerFactions.UpdateCurrentServicePhoneOwner(factionId, 0);
                            ServerFactions.sendMsg(factionId, $"{Characters.GetCharacterName(charId)} hat das Leitstellentelefon deiner Fraktion abgelegt.");
                            return;
                        }
                    }
                }

                if (player.Position.IsInRange(Constants.Positions.Jobcenter_Position, 2.5f) && !Characters.IsCharacterCrimeFlagged(charId) && !player.IsInVehicle) //Arbeitsamt
                {
                    TownhallHandler.CreateJobcenterBrowser(player);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.TownhallHouseSelector, 2.5f))
                {
                    TownhallHandler.OpenHouseSelector(player);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.IdentityCardApply, 2.5f) && Characters.GetCharacterAccState(charId) == 0 && !player.IsInVehicle) //Rathaus IdentityCardApply
                {
                    TownhallHandler.TryCreateIdentityCardApplyForm(player);
                    return;
                }

                var tattooShop = ServerTattooShops.ServerTattooShops_.ToList().FirstOrDefault(x => x.owner != 0 && player.Position.IsInRange(new Position(x.pedX, x.pedY, x.pedZ), 2.5f));
                if (tattooShop != null && !player.IsInVehicle)
                {
                    ShopHandler.OpenTattooShop((ClassicPlayer)player, tattooShop);
                    return;
                }

                WeedPot weedPot = WeedPlantHandler.WeedPots_.FirstOrDefault(x => player.Position.IsInRange(x.position, 2f) && x.state == 3);
                if (weedPot != null && !player.IsInVehicle)
                {
                    WeedPlantHandler.HarvestPot(player, weedPot);
                    return;
                }

                WeedPot weedPot2 = WeedPlantHandler.WeedPots_.FirstOrDefault(x => player.Position.IsInRange(x.position, 2f) && x.state == 4);
                if (weedPot2 != null && !player.IsInVehicle)
                {
                    WeedPlantHandler.RemoveOldPot(player, weedPot2);
                    return;
                }

                var automat = AutomatHandler.Automat_.ToList().FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 0.5f));
                if (automat != null && !player.IsInVehicle)
                {
                    AutomatHandler.OpenMenu(player, automat.automatenId);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.SellVehicle, 2.5f))
                {
                    if (player.IsInVehicle)
                    {
                        if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                        ShopHandler.SellVehicle(player);
                        return;
                    }
                }

                if (player.Position.IsInRange(Constants.Positions.SellVehicle2, 2.5f))
                {
                    if (player.IsInVehicle)
                    {
                        if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                        ShopHandler.SellVehicle(player);
                        return;
                    }
                }
            }
        }

        [AsyncClientEvent("Server:KeyHandler:PressU")]
        public static void PressU(IPlayer player)
        {
            try
            {
                lock (player)
                {
                    if (player == null || !player.Exists) return;
                    int charId = User.GetPlayerOnline(player);
                    if (charId <= 0) return;
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }

                    ClassicColshape serverDoorLockCol = (ClassicColshape)ServerDoors.ServerDoorsLockColshapes_.FirstOrDefault(x => ((ClassicColshape)x).IsInRange((ClassicPlayer)player));
                    if (serverDoorLockCol != null)
                    {
                        var doorColData = ServerDoors.ServerDoors_.FirstOrDefault(x => x.id == (int)serverDoorLockCol.GetColShapeId());
                        if (doorColData != null && doorColData.pairs == "None")
                        {
                            string doorKey = doorColData.doorKey;
                            string doorKey2 = doorColData.doorKey2;
                            if (doorKey == null || doorKey2 == null) return;
                            if (!CharactersInventory.ExistCharacterItem(charId, doorKey, "schluessel") && !CharactersInventory.ExistCharacterItem(charId, doorKey2, "schluessel")) return;

                            if (!doorColData.state) { HUDHandler.SendNotification(player, 4, 1500, "Tür abgeschlossen."); }
                            else { HUDHandler.SendNotification(player, 2, 1500, "Tür aufgeschlossen."); }
                            doorColData.state = !doorColData.state;
                            Alt.EmitAllClients("Client:DoorManager:ManageDoor", doorColData.hash, new Position(doorColData.posX, doorColData.posY, doorColData.posZ), (bool)doorColData.state);
                            return;
                        }
                        else if (doorColData.pairs != "None")
                        {
                            var doorColData2 = ServerDoors.ServerDoors_.FirstOrDefault(x => x.name == ServerDoors.GetDoorPair(doorColData.id));
                            if (doorColData2 == null) return;
                            if (doorColData.pairs == null) return;
                            string doorKey = doorColData.doorKey;
                            string doorKey2 = doorColData.doorKey2;
                            if (doorKey == null || doorKey2 == null) return;
                            if (doorColData.pairs == doorColData2.name)
                            {
                                if (!CharactersInventory.ExistCharacterItem(charId, doorKey, "schluessel") && !CharactersInventory.ExistCharacterItem(charId, doorKey2, "schluessel")) return;
                                if (!doorColData.state && !doorColData2.state) { HUDHandler.SendNotification(player, 4, 7000, "Tür abgeschlossen."); }
                                else { HUDHandler.SendNotification(player, 2, 7000, "Tür aufgeschlossen."); }
                                doorColData2.state = !doorColData2.state;
                                doorColData.state = !doorColData.state;
                                Alt.EmitAllClients("Client:DoorManager:ManageDoor", doorColData.hash, new Position(doorColData.posX, doorColData.posY, doorColData.posZ), (bool)doorColData.state);
                                Alt.EmitAllClients("Client:DoorManager:ManageDoor", doorColData2.hash, new Position(doorColData2.posX, doorColData2.posY, doorColData2.posZ), (bool)doorColData2.state);
                            }
                            return;
                        }
                    }

                    if(player.Dimension >= 5000)
                    {
                        int houseInteriorCount = ServerHouses.GetMaxInteriorsCount();
                        for(var i = 1; i <= houseInteriorCount; i++)
                        {
                            if(player.Dimension >= 5000 && player.Dimension < 10000 && player.Position.IsInRange(ServerHouses.GetInteriorExitPosition(i), 2f))
                            {
                                //Hotel abschließen / aufschließen
                                if (player.Dimension - 5000 <= 0) continue;
                                int apartmentId = player.Dimension - 5000;
                                int hotelId = ServerHotels.GetHotelIdByApartmentId(apartmentId);
                                if (hotelId <= 0 || apartmentId <= 0) continue;
                                if(!ServerHotels.ExistHotelApartment(hotelId, apartmentId)) { 
                                    HUDHandler.SendNotification(player, 3, 7000, "Ein unerwarteter Fehler ist aufgetreten [HOTEL-001]."); 
                                    return; 
                                }
                                if (ServerHotels.GetApartmentOwner(hotelId, apartmentId) != charId) { 
                                    HUDHandler.SendNotification(player, 3, 7000, "Du hast keinen Schlüssel."); 
                                    return; 
                                }
                                HotelHandler.LockHotel(player, hotelId, apartmentId);
                                return;
                            }
                            else if(player.Dimension >= 10000 && player.Position.IsInRange(ServerHouses.GetInteriorExitPosition(i), 2f))
                            {
                                //Haus abschließen / aufschließen
                                if (player.Dimension - 10000 <= 0) continue;
                                int houseId = player.Dimension - 10000;
                                if (houseId <= 0) continue;
                                if(!ServerHouses.ExistHouse(houseId)) {
                                    HUDHandler.SendNotification(player, 3, 7000, "Ein unerwarteter Fehler ist aufgetreten [HOUSE-001]."); 
                                    return; 
                                }
                                bool hasKey = CharactersInventory.ExistCharacterItem(charId, $"Hausschluessel {houseId}", "schluessel");
                                if (!hasKey && !ServerHouses.IsCharacterRentedInHouse(charId, houseId)) {
                                    HUDHandler.SendNotification(player, 3, 7000, "Dieses Haus gehört nicht dir und / oder du bist nicht eingemietet."); 
                                    return; 
                                }
                                HouseHandler.LockHouse(player, houseId);
                                return;
                            }
                        }
                    }

                    var houseEntrance = ServerHouses.ServerHouses_.FirstOrDefault(x => ((ClassicColshape)x.entranceShape).IsInRange((ClassicPlayer)player));
                    if (houseEntrance != null)
                    {
                        HouseHandler.LockHouse(player, houseEntrance.id);
                    }
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:KeyHandler:PressO")]
        public static void PressO(IPlayer player)
        {
            try
            {
                lock (player)
                {
                    if (player == null || !player.Exists) return;
                    int charId = User.GetPlayerOnline(player);
                    if (charId <= 0) return;
                    if (player.Vehicle != null)
                    {
                        VehicleHandler.Autopilot((ClassicPlayer)player);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:KeyHandler:CreateNewDoor")]
        public static void CreateNewDoor(IPlayer client, ulong hash, Vector3 pos)
        {
            ServerDoors.CreateNewDoor(client, pos, hash);
        }

    }
}
