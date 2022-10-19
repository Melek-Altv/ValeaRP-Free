using AltV.Net;
using AltV.Net.Async;
using Altv_Roleplay.Factories;
using Altv_Roleplay.models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Altv_Roleplay.Handler
{
    public partial class Server_Calls
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public int charId { get; set; }
        public int number { get; set; }
        public string result { get; set; } // accepted (green), declined (red)
        public string timestamp { get; set; }
    }

    public class AnruflistenHandler : IScript
    {
        public static List<Server_Calls> ServerCalls_ = new List<Server_Calls>();

        public static void Load()
        {
            using (var db = new gtaContext())
            {
                ServerCalls_ = new List<Server_Calls>(db.Server_Calls);
                Alt.Log($"{ServerCalls_.Count} Anruflisten-Verläufe geladen.");
            }
        }

        public static void AddCallHistory(int charId, int number,string result, string timestamp)
        {
            try
            {
                if (charId <= 0) return;
                Server_Calls call = new Server_Calls
                {
                    charId = charId,
                    number = number,
                    result = result,
                    timestamp = timestamp
                };

                using (var db = new gtaContext())
                {
                    db.Server_Calls.Add(call);
                    db.SaveChanges();
                }
                ServerCalls_.Add(call);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:requestAnrufliste")]
        public static void RefreshList(ClassicPlayer player)
        {
            if (player == null || !player.Exists || player.CharacterId <= 0) return;
            var items = ServerCalls_.ToList().Where(x => x.charId == player.CharacterId).Select(x => new
            {
                x.id,
                x.number,
                x.result,
                x.timestamp,
            }).OrderByDescending(x => x.id).TakeLast(20).ToList();

            player.Emit("Client:Smartphone:setAnrufliste", Newtonsoft.Json.JsonConvert.SerializeObject(items));
        }
    }
}
