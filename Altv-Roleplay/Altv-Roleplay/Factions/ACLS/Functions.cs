﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Handler;
using Altv_Roleplay.Model;
using Altv_Roleplay.Services;
using Altv_Roleplay.Utils;

namespace Altv_Roleplay.Factions.ACLS
{
    class Functions : IScript
    {
        [AsyncClientEvent("Server:Raycast:RepairVehicle")]
        public async void RepairVehicle(IPlayer player, IVehicle vehicle)
        {
            try
            {
                if (player == null || !player.Exists || vehicle == null || !vehicle.Exists) return;
                int charId = (int)player.GetCharacterMetaId();
                if (charId <= 0 || player.HasPlayerRopeCuffs() || player.HasPlayerHandcuffs() || player.IsPlayerUnconscious()) return;
                if (!CharactersInventory.ExistCharacterItem(charId, "Reparaturkit", "inventory") && !CharactersInventory.ExistCharacterItem(charId, "Reparaturkit", "backpack")) { 
                    HUDHandler.SendNotification(player, 4, 2000, "Du besitzt kein Reparaturkit."); 
                    return; 
                }
                if (CharactersInventory.ExistCharacterItem(charId, "Reparaturkit", "inventory")) CharactersInventory.RemoveCharacterItemAmount(charId, "Reparaturkit", 1, "inventory");
                else if (CharactersInventory.ExistCharacterItem(charId, "Reparaturkit", "backpack")) CharactersInventory.RemoveCharacterItemAmount(charId, "Reparaturkit", 1, "backpack");
                InventoryHandler.InventoryAnimation(player, "repair", 24000); //Animation
                Extensions.createProgress(player, 240);
                await Task.Delay(24000); //warten bis repair
                ServerVehicles.SetVehicleEngineHealthy(vehicle, true);
                Alt.EmitAllClients("Client:Utilities:repairVehicle", vehicle);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Raycast:towVehicle")]
        public static async void TowVehicle(IPlayer player, IVehicle vehicle)
        {
            try
            {
                if (player == null || !player.Exists || vehicle == null || !vehicle.Exists) return;
                int charId = (int)player.GetCharacterMetaId();
                int vehId = (int)vehicle.GetVehicleId();
                if (charId <= 0 || player.HasPlayerRopeCuffs() || player.HasPlayerHandcuffs() || player.IsPlayerUnconscious() || !ServerFactions.IsCharacterInAnyFaction(charId) || vehId <= 0) return;
                if (ServerFactions.GetCharacterFactionId(charId) != 4) return;
                Extensions.createProgress(player, 600);
                player.EmitLocked("Client:Animation:PlayScenario", "WORLD_HUMAN_WELDING", 60000);
                await Task.Delay(60000);
                int vehClass = ServerAllVehicles.GetVehicleClass(vehicle.Model);
                switch(vehClass)
                {
                    case 0: //Fahrzeuge
                        ServerVehicles.SetVehicleInGarage(vehicle, true, 30, false);
                        break;
                    case 1: //Boote
                        break;
                    case 2: //Flugzeuge
                        break;
                    case 3: //Helikopter
                        ServerVehicles.SetVehicleInGarage(vehicle, true, 29, false);
                        break;
                }
                ServerFactions.SetFactionBankMoney(4, ServerFactions.GetFactionBankMoney(4) + 1500); //ToDo: Anpassen
                HUDHandler.SendNotification(player, 2, 2000, "Fahrzeug erfolgreich verwahrt.");
                LoggingService.NewFactionLog(4, charId, vehId, "towVehicle", $"{Characters.GetCharacterName(charId)} hat das Fahrzeug mit der ID {vehId} abgeschleppt.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        int[] modPrices = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        [AsyncClientEvent("Server:Raycast:tuneVehicle")]
        public void OpenTuningMenu(IPlayer player, IVehicle veh)
        {
            try
            {
                if (player == null || !player.Exists || veh == null || !veh.Exists) return;
                int charId = (int)player.GetCharacterMetaId();
                int vehId = (int)veh.GetVehicleId();
                if (charId <= 0 || vehId <= 0) return;
                veh.ModKit = 1;

                var mod = ServerVehicles.ServerVehiclesMod_.FirstOrDefault(x => x.vehId == (int)veh.GetVehicleId());
                if (mod != null)
                {
                    int[] possibleMods = { };
                    int[] installedMods = { };

                    for (var i = 0; i < 85; i++)
                    {
                        Array.Resize(ref possibleMods, i);
                        Array.Resize(ref installedMods, i);
                    }

                    for (var i = 0; i < 49; i++)
                    {
                        possibleMods[i] = veh.GetModsCount((byte)i);
                        installedMods[i] = veh.GetMod((byte)i);
                    }

                    possibleMods[49] = 15;
                    installedMods[49] = 0;

                    possibleMods[50] = 11;
                    installedMods[50] = mod.wheel_type;

                    possibleMods[51] = 160;
                    installedMods[51] = mod.wheels;

                    possibleMods[52] = 160;
                    installedMods[52] = mod.wheelcolor;

                    possibleMods[53] = 3;
                    installedMods[53] = mod.window_tint;

                    possibleMods[54] = 4;
                    installedMods[54] = mod.plate_color;

                    possibleMods[55] = 5;
                    installedMods[55] = mod.colorPrimaryType;

                    possibleMods[56] = 0;
                    installedMods[56] = mod.colorPrimary_r;

                    possibleMods[57] = 0;
                    installedMods[57] = mod.colorPrimary_g;

                    possibleMods[58] = 0;
                    installedMods[58] = mod.colorPrimary_b;

                    possibleMods[59] = 5;
                    installedMods[59] = mod.colorSecondaryType;

                    possibleMods[60] = 0;
                    installedMods[60] = mod.colorSecondary_r;

                    possibleMods[61] = 0;
                    installedMods[61] = mod.colorSecondary_g;

                    possibleMods[62] = 0;
                    installedMods[62] = mod.colorSecondary_b;

                    possibleMods[63] = 160;
                    installedMods[63] = mod.colorPearl;

                    possibleMods[64] = 100;
                    installedMods[64] = 0;

                    for (var i = 0; i < 16; i++)
                    {
                        Array.Resize(ref possibleMods, possibleMods.Length + 1);
                        possibleMods[possibleMods.GetUpperBound(0)] = 1;

                        Array.Resize(ref installedMods, installedMods.Length + 1);
                        installedMods[installedMods.GetUpperBound(0)] = Convert.ToInt32(veh.IsExtraOn((byte)i));
                    }

                    possibleMods[80] = 160;
                    installedMods[80] = mod.interior_color;

                    possibleMods[81] = 1;
                    if (Convert.ToBoolean(mod.neon)) installedMods[81] = 1;
                    else installedMods[81] = 0;

                    possibleMods[82] = 0;
                    installedMods[82] = mod.neon_r;

                    possibleMods[83] = 0;
                    installedMods[83] = mod.neon_g;

                    possibleMods[84] = 0;
                    installedMods[84] = mod.neon_b;

                    possibleMods[85] = 0;
                    installedMods[85] = mod.smoke_r;

                    possibleMods[86] = 0;
                    installedMods[86] = mod.smoke_g;

                    possibleMods[87] = 0;
                    installedMods[87] = mod.smoke_b;

                    possibleMods[88] = 12;
                    installedMods[88] = mod.headlightColor;

                    player.Emit("Client:Tuningmenu:OpenMenu", veh, possibleMods, installedMods, modPrices);
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }


        [AsyncClientEvent("Server:Tuning:resetToNormal")]
        public static void ResetTuningToNormal(IPlayer player, IVehicle vehicle)
        {
            try
            {
                if (player == null || !player.Exists || vehicle == null || !vehicle.Exists) return;
                if (player.GetCharacterMetaId() <= 0 || vehicle.GetVehicleId() <= 0) return;
                ServerVehicles.SetVehicleModsCorrectly(vehicle);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }


        [AsyncClientEvent("Server:Tuningmenu:EquipTuneItem")]
        public void BuyTuningPart(IPlayer player, IVehicle vehicle, int type, int index)
        {
            try
            {
                if (player == null || !player.Exists || vehicle == null || !vehicle.Exists) return;
                if (player.GetCharacterMetaId() <= 0 || vehicle.GetVehicleId() <= 0) return;

                int charId = User.GetPlayerOnline(player);
                int price = modPrices[type];

                ServerVehicles.InstallBoughtMod(vehicle, type, index);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }


        [AsyncClientEvent("Server:Tuningmenu:EquipRGBTuneItem")]
        public static void BuyRGBTuningPart(IPlayer player, IVehicle vehicle, int type, int colorR, int colorG, int colorB, int paintType)
        {
            try
            {
                if (player == null || !player.Exists || vehicle == null || !vehicle.Exists) return;
                if (player.GetCharacterMetaId() <= 0 || vehicle.GetVehicleId() <= 0) return;

                if (type == 100) ServerVehicles.InstallBoughtMod(vehicle, 55, paintType);
                else if (type == 200) ServerVehicles.InstallBoughtMod(vehicle, 59, paintType);

                ServerVehicles.InstallBoughtModRgb(vehicle, type, colorR, colorG, colorB);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
    }
}


