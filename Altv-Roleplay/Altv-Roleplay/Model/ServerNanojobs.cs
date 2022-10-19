using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Handler;
using Altv_Roleplay.models;
using System;
using System.Collections.Generic;

namespace Altv_Roleplay.Model
{
    class ServerNanojobs
    {
        public static List<Server_nanojobs> Servernanojobs_ = new List<Server_nanojobs>();

        public static void CreateNewBank(IPlayer client, Position pos, string jobName)
        {
            if (client == null || !client.Exists) return;
            var ServerNanojobData = new Server_nanojobs
            {
                posX = pos.X,
                posY = pos.Y,
                posZ = pos.Z,
                jobName = jobName
            };

            try
            {
                Servernanojobs_.Add(ServerNanojobData);
                using (gtaContext db = new gtaContext())
                {
                    db.server_nanojobs.Add(ServerNanojobData);
                    db.SaveChanges();
                }

                HUDHandler.SendNotification(client, 2, 5000, $"Bank in der Zone ({ServerNanojobData.jobName}) an deiner Position erstellt.");

                foreach (IPlayer player in Alt.GetAllPlayers())
                {
                    if (player == null || !player.Exists) return;
                    player.EmitLocked("Client:ServerBlips:AddNewBlip", $"{ServerNanojobData.jobName}", 2, 1, true, 605, pos.X, pos.Y, pos.Z);
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }



    }
}
