using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Altv_Roleplay.Factories;
using Altv_Roleplay.models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Altv_Roleplay.Handler
{
    public partial class LifeinvaderAd
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public int charId { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public DateTime timestamp { get; set; }
    }

    class LifeinvaderHandler : IScript
    {
        public static List<LifeinvaderAd> Ads_ = new List<LifeinvaderAd>();
        public static readonly Position lifeinvaderPos = new Position(-1082.3473f, -247.83296f, 37.75537f);

        public static void Load()
        {
            EntityStreamer.MarkerStreamer.Create(EntityStreamer.MarkerTypes.MarkerTypeVerticalCylinder, new System.Numerics.Vector3(lifeinvaderPos.X, lifeinvaderPos.Y, lifeinvaderPos.Z - 1), new System.Numerics.Vector3(0.5f, 0.5f, 0.5f), color: new Rgba(150, 50, 50, 150), streamRange: 30);

            using (var db = new gtaContext())
            {
                foreach (var entry in db.LifeinvaderAd.ToList().Where(x => DateTime.Now.Subtract(Convert.ToDateTime(x.timestamp)).TotalHours >= 48))
                    db.LifeinvaderAd.Remove(entry);
                db.SaveChanges();

                Ads_ = new List<LifeinvaderAd>(db.LifeinvaderAd);

                Alt.Log($"{Ads_.Count} Lifeinvader-Werbungen wurden geladen.");
            }
        }

        [AsyncClientEvent("Server:Lifeinvader:createAd")]
        public static void CreateAd(ClassicPlayer player, string title, string text)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(text)) return;
                if (Model.CharactersInventory.GetCharacterItemAmount(player.CharacterId, "Bargeld", "brieftasche") < 30) { 
                    HUDHandler.SendNotification(player, 4, 7000, "Du hast nicht genügend Geld (500$)."); 
                    return; 
                }
                Model.CharactersInventory.RemoveCharacterItemAmount(player.CharacterId, "Bargeld", 30, "brieftasche");
                AddAd(new LifeinvaderAd { charId = player.CharacterId, text = text, timestamp = DateTime.Now, title = title });
                foreach (var p in Alt.GetAllPlayers().ToList().Cast<ClassicPlayer>().Where(x => x != null && x.Exists && x.CharacterId > 0))
                    HUDHandler.SendNotification(p, 1, 7000, "Eine neue Lifeinvader Werbung wurde geschaltet. Sehe sie dir in der Tablet Lifeinvader App an.");
                TabletHandler.RefreshTabletData(player, false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        public static string GetAds()
        {
            var ads = Ads_.ToList().Select(x => new
            {
                x.id,
                x.text,
                x.title,
                name = Model.Characters.GetCharacterName(x.charId),
                number = Model.Characters.GetCharacterPhonenumber(x.charId),
                x.timestamp,
            }).OrderByDescending(x => x.timestamp);

            return Newtonsoft.Json.JsonConvert.SerializeObject(ads);
        }

        public static void RemoveAd(int id)
        {
            try
            {
                LifeinvaderAd ad = Ads_.FirstOrDefault(x => x.id == id);
                if (ad == null) return;
                Ads_.Remove(ad);
                using (var db = new gtaContext())
                {
                    db.LifeinvaderAd.Remove(ad);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        public static void AddAd(LifeinvaderAd ad)
        {
            try
            {
                if (ad == null) return;
                Ads_.Add(ad);
                using (var db = new gtaContext())
                {
                    db.LifeinvaderAd.Add(ad);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }
    }
}