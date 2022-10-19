using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Resources.Chat.Api;
using Altv_Roleplay.Factories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Altv_Roleplay.Handler
{
    public partial class Nagelband
    {
        public int id { get; set; } // Unique ID
        public EntityStreamer.Prop prop { get; set; }
        public ClassicColshape colshape { get; set; }
    }

    class NagelbandHandler : IScript
    {
        public static List<Nagelband> NagelbandList_ = new List<Nagelband>();
        public static float newRadius = 0f;

        [Command("nagelband")]
        public static void Cmd(ClassicPlayer player, string lel)
        {
            newRadius = Convert.ToSingle(lel);
            HUDHandler.SendNotification(player, 2, 7000, $"Radius auf {newRadius} gesetzt.");
        }

        public static void CreateNagelband(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                int nextId = 0;
                if (NagelbandList_.Count <= 0) nextId = 1;
                else nextId = NagelbandList_.ToList().OrderBy(x => x.id).Last().id + 1;
                DegreeRotation rot = (DegreeRotation)player.Rotation;
                var nagelband = new Nagelband
                {
                    id = nextId,
                    prop = EntityStreamer.PropStreamer.Create("p_ld_stinger_s", new Position(player.Position.X, player.Position.Y, player.Position.Z - 1f), new System.Numerics.Vector3(0, 0, rot.Yaw), player.Dimension, frozen: true),
                    colshape = (ClassicColshape)Alt.CreateColShapeSphere(player.Position, 5f)
                };
                nagelband.colshape.Radius = 5f;
                nagelband.colshape.ColshapeName = "Nagelband";
                NagelbandList_.Add(nagelband);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        public static void DeleteNagelband(int id)
        {
            Nagelband nagelband = NagelbandList_.FirstOrDefault(x => x.id == id);
            if (nagelband == null) return;
            if (nagelband.prop != null) nagelband.prop.Delete();
            if (nagelband.colshape != null) nagelband.colshape.Remove();
            NagelbandList_.Remove(nagelband);
        }
    }
}
