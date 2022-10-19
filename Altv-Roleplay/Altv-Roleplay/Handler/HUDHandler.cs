using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;
using System;
using System.Linq;

namespace Altv_Roleplay.Handler
{
    class HUDHandler : IScript
    {

        public static void CreateHUDBrowser(IPlayer client)
        {
            if (client == null || !client.Exists) return;
            client.EmitLocked("Client:HUD:CreateCEF", Characters.GetCharacterHunger(User.GetPlayerOnline(client)), Characters.GetCharacterThirst(User.GetPlayerOnline(client)));
            int currPoliceMembers = ServerFactions.ServerFactionMembers_.ToList().Where(x => x.IsOnline == true && x.factionId == 2 && x.isDuty == true).Count();
            client.EmitLocked("Client:Stars:CreateCEF", currPoliceMembers);
        }

        [ScriptEvent(ScriptEventType.PlayerEnterVehicle)]
        public static void OnPlayerEnterVehicle_Handler(IVehicle vehicle, IPlayer client, byte seat)
        {
            try
            {
                if (client == null || !client.Exists) return;
                client.EmitLocked("Client:HUD:updateHUDPosInVeh", true, ServerVehicles.GetVehicleFuel(vehicle), ServerAllVehicles.GetVehicleMaxFuel(ServerVehicles.GetVehicleHashById((int)vehicle.GetVehicleId())), ServerVehicles.GetVehicleKM(vehicle));
                client.EmitLocked("Client:HUD:GetDistanceForVehicleKM");
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [ScriptEvent(ScriptEventType.PlayerLeaveVehicle)]
        public static void OnPlayerLeaveVehicle_Handler(IVehicle vehicle, IPlayer client, byte seat)
        {
            try
            {
                if (client == null || !client.Exists) return;
                client.EmitLocked("Client:HUD:updateHUDPosInVeh", false, 0, 0, 0);
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static void SendInformationToVehicleHUD(IPlayer player) {
            if (player == null || !player.Exists) return;
            IVehicle Veh = player.Vehicle;
            if (!Veh.Exists) return;
            long vehID = Veh.GetVehicleId();
            if (vehID == 0) return;
            player.EmitLocked("Client:HUD:SetPlayerHUDVehicleInfos", ServerVehicles.GetVehicleFuel(Veh), ServerAllVehicles.GetVehicleMaxFuel(ServerVehicles.GetVehicleHashById((int)vehID)), ServerVehicles.GetVehicleKM(Veh));  
        }

        public static void SendNotification(IPlayer client, int type, int duration, string msg, int delay = 0) //1 Info | 2 Success | 3 Warning | 4 Error
        {
            try
            {
                if (client == null || !client.Exists) return;
                client.EmitLocked("Client:HUD:sendNotification", type, duration, msg, delay);
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Vehicle:UpdateVehicleKM")]
        public static void UpdateVehicleKM(IPlayer player, float km)
        {
            try
            {
                if (player == null || !player.Exists || km <= 0) return;
                if (!player.IsInVehicle || player.Vehicle == null) return;
                float fKM = km / 1000;
                fKM = fKM + ServerVehicles.GetVehicleKM(player.Vehicle);
                ServerVehicles.SetVehicleKM(player.Vehicle, fKM);
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }
    }
}
