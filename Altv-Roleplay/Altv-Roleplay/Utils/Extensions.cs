
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Handler;
using Altv_Roleplay.Model;
using Altv_Roleplay.models;
using System;
using System.Linq;
using System.Numerics;

namespace Altv_Roleplay.Utils
{
    public static partial class Extensions
    {
        public static bool IsInRange(this Position currentPosition, Position otherPosition, float distance)
            => currentPosition.Distance(otherPosition) <= distance;

        public static void kickWithMessage(this IPlayer player, string reason) {
            HUDHandler.SendNotification(player, 3, 250000, $"Du wurdest vom Server gekickt. Grund: {reason}");
            player.Kick(reason);
        }

        public static void updateTattoos(this ClassicPlayer player)
        {
            if (player == null || !player.Exists || player.CharacterId <= 0) return;
            player.Emit("Client:Utilities:setTattoos", Model.CharactersTattoos.GetAccountTattoos(player.CharacterId));
        }

        public static void createProgress(IPlayer player, int time)
        {
            player.EmitLocked("Client:Progressbar:start", time);
        }

        public static bool HasVehicleId(this IVehicle vehicle)
        {
            var myVehicle = (ClassicVehicle)vehicle;
            if (myVehicle == null || !myVehicle.Exists) return false;
            return myVehicle.VehicleId != 0;
        }

        public static void SetVehicleHoodState(this ClassicVehicle vehicle, bool isOpen)
        {
            if (vehicle == null || !vehicle.Exists) return;
            vehicle.Hoodstate = isOpen;
        }

        public static bool GetVehicleHoodState(this ClassicVehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists) return false;
            return vehicle.Hoodstate;
        }

        public static void SetVehicleId(this IVehicle vehicle, long vehicleId)
        {
            var myVehicle = (ClassicVehicle)vehicle;
            if (myVehicle == null || !myVehicle.Exists) return;
            myVehicle.VehicleId = (int)vehicleId;
        }

        public static void SetVehicleTrunkState(this IVehicle vehicle, bool isOpen)
        {
            var myVehicle = (ClassicVehicle)vehicle;
            if (myVehicle == null || !myVehicle.Exists) return;
            myVehicle.Trunkstate = isOpen;
        }

        public static bool GetVehicleTrunkState(this IVehicle vehicle)
        {
            var myVehicle = (ClassicVehicle)vehicle;
            if (myVehicle == null || !myVehicle.Exists) return false;
            return myVehicle.Trunkstate;
        }

        public static long GetVehicleId(this IVehicle vehicle)
        {
            var myVehicle = (ClassicVehicle)vehicle;
            if (myVehicle == null || !myVehicle.Exists) return 0;
            return (long)myVehicle.VehicleId;
        }

        public static ulong GetCharacterMetaId(this IPlayer player)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return 0;
            return (ulong)myPlayer.CharacterId;
        }

        public static void SetCharacterMetaId(this IPlayer player, ulong id)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return;
            myPlayer.CharacterId = (int)id;
        }

        public static void SetColShapeName(this IColShape cols, string name)
        {
            var myColshape = (ClassicColshape)cols;
            if (myColshape == null || !myColshape.Exists) return;
            myColshape.ColshapeName = name;
        }

        public static long GetColshapeCarDealerVehPrice(this IColShape cols)
        {
            var myColshape = (ClassicColshape)cols;
            if (myColshape == null || !myColshape.Exists) return 0;
            return myColshape.CarDealerVehPrice;
        }

        public static string GetColshapeCarDealerVehName(this IColShape cols)
        {
            var myColshape = (ClassicColshape)cols;
            if (myColshape == null || !myColshape.Exists) return "";
            return myColshape.CarDealerVehName;
        }

        public static string GetColShapeName(this IColShape cols)
        {
            var myColshape = (ClassicColshape)cols;
            if (myColshape == null || !myColshape.Exists) return "None";
            return myColshape.ColshapeName;
        }

        public static void SetColShapeId(this IColShape cols, long id)
        {
            var myColshape = (ClassicColshape)cols;
            if (myColshape == null || !myColshape.Exists) return;
            myColshape.ColshapeId = (int)id;
        }

        public static long GetColShapeId(this IColShape cols)
        {
            var myColshape = (ClassicColshape)cols;
            if (myColshape == null || !myColshape.Exists) return 0;
            return (long)myColshape.ColshapeId;
        }

        public static string GetPlayerFarmingActionMeta(this IPlayer player)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return "None";
            return myPlayer.FarmingAction;
        }

        public static void SetPlayerFarmingActionMeta(this IPlayer player, string meta)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return;
            myPlayer.FarmingAction = meta;
        }

        public static void SetPlayerIsCuffed(this IPlayer player, string cuffType, bool isCuffed)
        {
            if (player == null || !player.Exists) return;
            if (cuffType == "handcuffs")
            {
                AltAsync.Do(() => player.SetSyncedMetaData("HasHandcuffs", isCuffed));
            } else if(cuffType == "ropecuffs")
            {
                AltAsync.Do(() => player.SetSyncedMetaData("HasRopeCuffs", isCuffed));
            }
        }

        public static void SetPlayerUsingCrowbar(this IPlayer player, bool isUsing)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return;
            myPlayer.IsUsingCrowbar = isUsing;
        }

        public static void SetPlayerIsUnconscious(this IPlayer player, bool isUnconscious)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return;
            myPlayer.IsUnconscious = isUnconscious;
        }

        public static bool IsPlayerUnconscious(this IPlayer player)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return false;
            return myPlayer.IsUnconscious;
        }

        public static void SetPlayerIsFastFarm(this IPlayer player, bool isFastFarm)
        {
            try
            {
                if (player == null || !player.Exists) return;
                var playerDb = Characters.PlayerCharacters.ToList().FirstOrDefault(x => x.charId == (int)player.GetCharacterMetaId());
                if (playerDb == null) return;
                playerDb.isFastFarm = isFastFarm;
                using (var db = new gtaContext())
                {
                    db.AccountsCharacters.Update(playerDb);
                    db.SaveChanges();
                }
            }
            catch (Exception e) { Alt.Log($"{e}"); }
        }

        public static bool IsPlayerFastFarm(this IPlayer player)
        {
            if (player == null || !player.Exists) return false;
            var playerDb = Characters.PlayerCharacters.ToList().FirstOrDefault(x => x.charId == (int)player.GetCharacterMetaId());
            return playerDb == null ? false : Convert.ToBoolean(playerDb.isFastFarm);
        }

        public static bool HasPlayerHandcuffs(this IPlayer player)
        {
            if (player == null || !player.Exists) return false;
            return player.GetSyncedMetaData("HasHandcuffs", out bool handCuffs) ? handCuffs : false;
        }

        public static bool IsPlayerUsingCrowbar(this IPlayer player)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return false;
            return myPlayer.IsUsingCrowbar;
        }

        public static bool HasPlayerRopeCuffs(this IPlayer player)
        {
            if (player == null || !player.Exists) return false;
            return player.GetSyncedMetaData("HasRopeCuffs", out bool RopeCuffs) ? RopeCuffs : false;
        }

        public static int AdminLevel(this IPlayer player)
        {
            if (player == null || !player.Exists) return 0;
            var playerDb = User.Player.FirstOrDefault(p => p.socialClub == player.SocialClubId && player.GetCharacterMetaId() == (ulong)p.Online);
            return playerDb == null ? 0 : Convert.ToInt32(playerDb.adminLevel);
        }
        public static void SetPlayerCurrentMinijob(this IPlayer player, string minijobName)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return;
            myPlayer.CurrentMinijob = minijobName;
        }

        public static string GetPlayerCurrentMinijob(this IPlayer player)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return "None";
            return myPlayer.CurrentMinijob;
        }

        public static void SetPlayerCurrentMinijobStep(this IPlayer player, string minijobStep)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return;
            myPlayer.CurrentMinijobStep = minijobStep;
        }

        public static string GetPlayerCurrentMinijobStep(this IPlayer player)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return "None";
            return myPlayer.CurrentMinijobStep;
        }

        public static void SetPlayerCurrentMinijobActionCount(this IPlayer player, long count)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return;
            myPlayer.CurrentMinijobActionCount = (int)count;
        }

        public static long GetPlayerCurrentMinijobActionCount(this IPlayer player)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return 0;
            return (long)myPlayer.CurrentMinijobActionCount;
        }
        public static void SetPlayerCurrentMinijobRouteId(this IPlayer player, long routeId)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return;
            myPlayer.CurrentMinijobRouteId = (int)routeId;
        }

        public static long GetPlayerCurrentMinijobRouteId(this IPlayer player)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return 0;
            return (long)myPlayer.CurrentMinijobRouteId;
        }

        public static Position getPositionInBackOfPosition(this Position pos, float rotation, float distance)
        {
            Position position = pos;
            float rot = rotation;
            var radius = rot * Math.PI / 180;
            Position newPos = new Position(position.X + (float)(distance * Math.Sin(-radius)), position.Y + (float)(distance * Math.Cos(-radius)), position.Z);
            return newPos;
        }
        public static Vector3 getForwardVector(this ClassicPlayer player)
        {
            Vector3 rotation = player.Rotation;
            float z = -rotation.Z;
            float x = rotation.X;
            double num = Math.Abs(Math.Cos(x));
            return new Vector3
            {
                X = (float)(Math.Sin(z) * num),
                Y = (float)(Math.Cos(z) * num),
                Z = (float)Math.Sin(x)
            };
        }

        public static void SetPlayerCurrentTestVehicle(this IPlayer player, string TestVehicle)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return;
            myPlayer.CurrentTestVehicle = TestVehicle;
        }

        public static string GetPlayerCurrentTestVehicle(this IPlayer player)
        {
            var myPlayer = (ClassicPlayer)player;
            if (myPlayer == null || !myPlayer.Exists) return "None";
            return myPlayer.CurrentTestVehicle;
        }
    }
}