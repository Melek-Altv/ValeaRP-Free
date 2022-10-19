using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Resources.Chat.Api;
using Altv_Roleplay.Factories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Threading.Tasks;

namespace Altv_Roleplay.Handler
{
    public partial class Attachment_DB
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public string model { get; set; }
        public int bone { get; set; }
        public Vector3 pos { get; set; }
        public Vector3 rot { get; set; }
        public int removeAfterSeconds { get; set; }
    }

    public partial class Attachment_Model
    {
        public int id { get; set; } 
        public string model { get; set; }
        public int bone { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }
        public float rotX { get; set; }
        public float rotY { get; set; }
        public float rotZ { get; set; }
    }

    class AttachmentHandler : IScript
    {
        public static List<Attachment_DB> AttachmentsDB_ = new List<Attachment_DB>();

        public static void Load()
        {
            using (var db = new models.gtaContext())
            {
                AttachmentsDB_ = new List<Attachment_DB>(db.Attachment_DB);
                Alt.Log($"{AttachmentsDB_.Count} Attachments wurden geladen.");

            }
        }

        [Command("proptest")]
        public static void CMDTest(ClassicPlayer player, int t)
        {
            if (t == 1)
#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                AddAttachment(player, new Attachment_Model { id = player.CharacterId, bone = 28422, model = "h4_prop_h4_box_ammo_01a", posX = 0.31f, posY = 0.03f, posZ = 0.05f, rotX = 0f, rotY = -100f, rotZ = -50f }, 0);
#pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
            else if (t == 2)
                RemoveAttachment(player);
        }

        public static void RemoveAttachment(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || player.attachment == null) return;
                player.DeleteStreamSyncedMetaData("player_attachment");
                player.attachment = null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        public static async Task AddAttachment(ClassicPlayer player, Attachment_Model attachModel, int removeAfterSeconds)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || attachModel == null || attachModel.id <= 0) return;
                if (player.attachment != null) RemoveAttachment(player);
                player.attachment = attachModel;
                player.SetStreamSyncedMetaData("player_attachment", System.Text.Json.JsonSerializer.Serialize(attachModel));
                if (removeAfterSeconds <= 0) return;
                await Task.Delay(removeAfterSeconds);
                RemoveAttachment(player);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        [AsyncClientEvent("Server:Phone:Attachment")]
        public static async Task PhoneAttachment(ClassicPlayer player, bool isAttachment)
        {
            if (player == null || !player.Exists || player.CharacterId <= 0) return;
            if (isAttachment) await AddAttachment(player, new Attachment_Model { 
                bone = 28422 , model = "prop_phone_ing", posX = 0, posY = 0, posZ = 0, rotX = 0, rotY = 0, rotZ = 0, id = player.CharacterId
            }, 0);
            else RemoveAttachment(player);
        }

        [AsyncClientEvent("Server:Tablet:Attachment")]
        public static async Task TabletAttachment(ClassicPlayer player, bool isAttachment)
        {
            if (player == null || !player.Exists || player.CharacterId <= 0) return;
            if (isAttachment) await AddAttachment(player, new Attachment_Model { 
                bone = 28422, model = "prop_cs_tablet", posX = 0, posY = 0, posZ = 0, rotX = 0, rotY = 0, rotZ = 0, id = player.CharacterId 
            }, 0);
            else RemoveAttachment(player);
        }

        public static async Task AngelAttachment(ClassicPlayer player, bool isAttachment)
        {
            if (player == null || !player.Exists || player.CharacterId <= 0) return;
            if (isAttachment) await AddAttachment(player, new Attachment_Model { 
                bone = 60309, model = "prop_fishing_rod_01", posX = 0, posY = 0, posZ = 0, rotX = 0, rotY = 0, rotZ = 0, id = player.CharacterId 
            }, 0);
            else RemoveAttachment(player);
        }
    }
}
