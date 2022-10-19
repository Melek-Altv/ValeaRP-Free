using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;
using Altv_Roleplay.models;

using AltV.Net.Async;
using AltV.Net.Resources.Chat.Api;

namespace Altv_Roleplay.Handler
{
    public partial class Automat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public int automatenId { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }

    }
    class AutomatHandler : IScript
    {
        public static List<Automat> Automat_ = new List<Automat>();

        [Command("createautomat")]
        public static void AutomatenCMD(ClassicPlayer player, int automatenid)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() < 5)
            {
                HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte.");
                return;
            }
            Automat automat = new Automat
            {
                automatenId = automatenid,
                posX = player.Position.X,
                posY = player.Position.Y,
                posZ = player.Position.Z,
            };

            using (var db = new gtaContext())
            {
                db.Automat.Add(automat);
                db.SaveChanges();
            }
            Automat_.Add(automat);
            HUDHandler.SendNotification(player, 2, 1500, "Automat erstellt.");
        }

        #region OpenMenu
        public static void OpenMenu(IPlayer player, int automatenId)
        {
            if (player == null || !player.Exists) return;
            if (player.IsInVehicle) return;

            switch (automatenId)
            {
                case 0:
                    player.EmitLocked("Client:BeanMachine:Open");
                    break;
                case 1:
                    player.EmitLocked("Client:eCola:Open");
                    break;
                case 2:
                    player.EmitLocked("Client:Raineinside:Open");
                    break;
                case 3:
                    player.EmitLocked("Client:Raineoutside:Open");
                    break;
                case 4:
                    player.EmitLocked("Client:Sprunk:Open");
                    break;
                case 5:
                    player.EmitLocked("Client:Candybox:Open");
                    break;
            }
        }
        #endregion

        #region Load
        public static void LoadAllAutomaten()
        {
            using (var db = new gtaContext())
            {
                Automat_ = new List<Automat>(db.Automat);
                Alt.Log($"{Automat_.Count} Automaten geladen..");
            }
        }
        #endregion

        #region Take
        [AsyncClientEvent("Server:Take")]
        public static void Take(ClassicPlayer player, string machine, string item, int price)
        {
            if (player != null && player.Exists && item != "" && machine != "" && price > 0)
            {
                float itemWeight = ServerItems.GetItemWeight(item) * 1;
                float curBackpackWeight = CharactersInventory.GetCharacterItemWeight(player.CharacterId, "inventory");
                if (curBackpackWeight + itemWeight > 15)
                {
                    HUDHandler.SendNotification(player, 4, 7000, $"Du hast nicht genug Platz.");
                    return;
                }
                if (!CharactersInventory.ExistCharacterItem(player.CharacterId, "Bargeld", "brieftasche"))
                {
                    HUDHandler.SendNotification(player, 4, 5000, $"Du hast kein Bargeld dabei.");
                    return;
                }
                CharactersInventory.RemoveCharacterItemAmount(player.CharacterId, "Bargeld", price, "brieftasche");
                CharactersInventory.AddCharacterItem(player.CharacterId, item, 1, "inventory");
                HUDHandler.SendNotification(player, 2, 5000, $"Du hast {price}$ für {item} bezahlt");
            }

        }
        #endregion
    }
}
