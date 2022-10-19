using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Altv_Roleplay.Handler
{
    class WaschstraßenHandler : IScript
    {
        public static List<Position> Positions_ = new List<Position>() {
            new Position(28.140661f,-1392.2505f,28.656372f),
            new Position(170.4f,-1718.1362f,28.58899f),
            new Position(-700.2725f,-934.4835f,18.310669f),
        };

        public static void Load()
        {
            foreach (Position pos in Positions_.ToList())
            {
                EntityStreamer.BlipStreamer.CreateStaticBlip("Waschstraße", 0, 0.5f, true, 100, pos, 0);
                EntityStreamer.MarkerStreamer.Create(EntityStreamer.MarkerTypes.MarkerTypeVerticalCylinder, pos, new System.Numerics.Vector3(2), color: new Rgba(210, 0, 255, 1), dimension: 0);
           }

            Alt.Log($"{Positions_.Count()} Waschstraßen wurden geladen.");
        }

        [Obsolete]
        public static async void ClearVehicleDirt(ClassicPlayer player, Position pos)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || player.Vehicle == null || !player.Position.IsInRange(pos, 5f)) return;
                if (CharactersInventory.GetCharacterItemAmount(player.CharacterId, "Bargeld", "brieftasche") < 50) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Du hast keine 50$ dabei."); 
                    return; 
                }
                CharactersInventory.RemoveCharacterItemAmount(player.CharacterId, "Bargeld", 50, "brieftasche");
                ServerVehicles.SetVehicleEngineState(player.Vehicle, false);
                ((ClassicVehicle)player.Vehicle).inWash = true;
                HUDHandler.SendNotification(player, 1, 7000, "Dein Auto wird gewaschen..");
                await Task.Delay(7500);
                if (player == null || !player.Exists || player.Vehicle == null || !player.Position.IsInRange(pos, 5f)) return;
                ((ClassicVehicle)player.Vehicle).inWash = false;
                HUDHandler.SendNotification(player, 2, 7000, "Dein Auto wurde gereinigt.");
                await player.Vehicle.SetDirtLevelAsync(0);
                player.Vehicle.NetworkOwner.Emit("Client:Utilities:cleanVehicle", player.Vehicle); // Workaround
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }
    }
}
