using AltV.Net.Elements.Entities;
using System.Collections.Generic;
using AltV.Net.Data;
using AltV.Net;
using System;
using Altv_Roleplay.models;
using Altv_Roleplay.Handler;
using System.Linq;

namespace Altv_Roleplay.Model
{
    class ServerDoors
    {
        public static List<Server_Doors> ServerDoors_ = new List<Server_Doors>();
        public static List<IColShape> ServerDoorsColshapes_ = new List<IColShape>();
        public static List<IColShape> ServerDoorsLockColshapes_ = new List<IColShape>();
        public static void CreateNewDoor(IPlayer client, Position pos, ulong hash)
        {
            if (client == null || !client.Exists) return;
            var ServerDoorData = new Server_Doors
            {
                name = "None",
                posX = pos.X,
                posY = pos.Y,
                posZ = pos.Z,
                hash = hash,
                state = true,
                doorKey = "None",
                doorKey2 = "None",
                type = "Door",
                lockPosX = pos.X,
                lockPosY = pos.Y,
                lockPosZ = pos.Z,
                pairs = "None"
            };

            try
            {
                ServerDoors_.Add(ServerDoorData);
                using (gtaContext db = new gtaContext())
                {
                    db.Server_Doors.Add(ServerDoorData);
                    db.SaveChanges();
                }

                HUDHandler.SendNotification(client, 2, 5000, $"Tür an der Entity Position erstellt {pos}");

            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static string GetDoorPair(int id)
        {
            try
            {
                if (id <= 0) return null;
                var doors = ServerDoors_.FirstOrDefault(x => x.id == id);
                if (doors != null) return doors.pairs;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return null;
        }
    }
}