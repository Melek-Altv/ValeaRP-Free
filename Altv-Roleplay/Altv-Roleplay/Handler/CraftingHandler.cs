using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Altv_Roleplay.Factories;
using Altv_Roleplay.models;
using Altv_Roleplay.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Altv_Roleplay.Handler
{
    #region models
    public partial class Server_Crafting_Stations
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public Position pos { get; set; }
        public float heading { get; set; }
        public string pedModel { get; set; }
        public bool isBlipVisible { get; set; }
        public string comment { get; set; }
    }

    public partial class Server_Crafting_Recipes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public List<int> craftingStations { get; set; }
        public List<Server_Crafting_Recipes_ItemGroup> neededItems { get; set; }
        public Server_Crafting_Recipes_ItemGroup endItem { get; set; }
        public int duration { get; set; } // Verarbeitungszeit per Item
    }

    public partial class Server_Crafting_Recipes_ItemGroup
    {
        public string itemName { get; set; }
        public int itemAmount { get; set; }
    }
    #endregion
    class CraftingHandler : IScript
    {
        public static List<Server_Crafting_Stations> ServerCraftingStations_ = new List<Server_Crafting_Stations>();
        public static List<Server_Crafting_Recipes> ServerCraftingRecipes_ = new List<Server_Crafting_Recipes>();

        public static void Load()
        {
            using (var db = new gtaContext())
            {
                ServerCraftingStations_ = new List<Server_Crafting_Stations>(db.Server_Crafting_Stations);
                ServerCraftingRecipes_ = new List<Server_Crafting_Recipes>(db.Server_Crafting_Recipes);
            }

            foreach(var station in ServerCraftingStations_.ToList())
            {
                EntityStreamer.PedStreamer.Create(station.pedModel, station.pos, new System.Numerics.Vector3(0, 0, station.heading), 0);
                if (!station.isBlipVisible) continue;
                EntityStreamer.BlipStreamer.CreateStaticBlip($"Crafting: {station.comment}", 0, 0.7f, true, 566, station.pos, 0);
            }

            Alt.Log($"{ServerCraftingStations_.Count} Crafting-Stationen wurden geladen.");
            Alt.Log($"{ServerCraftingRecipes_.Count} Crafting-Rezepte wurden geladen.");
        }
        
        public static string GetCraftingRecipes(int craftingStationId)
        {
            var items = ServerCraftingRecipes_.ToList().Where(x => x.craftingStations.Contains(craftingStationId)).Select(x => new
            {
                x.id,
                x.endItem,
                endPic = Model.ServerItems.ReturnItemPicSRC(x.endItem.itemName),
                neededItems = x.neededItems.ToList().Select(y => new
                {
                    y.itemName,
                    y.itemAmount,
                    pic = Model.ServerItems.ReturnItemPicSRC(y.itemName),
                }),
            }).OrderBy(x => x.endItem.itemName).ToList();
            return System.Text.Json.JsonSerializer.Serialize(items);
        }

        [AsyncClientEvent("Server:CraftingStation:produceItem")]
        public static async Task ProduceItem(ClassicPlayer player, int stationId, int recipeId, int amount)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || stationId <= 0 || recipeId <= 0 || amount <= 0) return;
                Server_Crafting_Recipes recipe = ServerCraftingRecipes_.ToList().FirstOrDefault(x => x.id == recipeId);
                if (recipe == null) return;
                foreach(Server_Crafting_Recipes_ItemGroup neededItem in recipe.neededItems)
                {
                    if (Model.CharactersInventory.GetCharacterItemAmount(player.CharacterId, neededItem.itemName, "inventory") >= (neededItem.itemAmount * amount)) continue;
                    HUDHandler.SendNotification(player, 3, 7000, "Du hast nicht genügend Gegenstände dabei.");
                    return;
                }
                HUDHandler.SendNotification(player, 1, recipe.duration * amount, "Du stellst etwas her...");
                player.IsUsingCrafter = true;
                await Task.Delay(recipe.duration * amount);
                player.IsUsingCrafter = false;
                Server_Crafting_Stations station = ServerCraftingStations_.ToList().FirstOrDefault(x => x.id == stationId);
                if (station == null || player == null || !player.Exists || !player.Position.IsInRange(station.pos, 10f)) return;
                foreach (Server_Crafting_Recipes_ItemGroup neededItem in recipe.neededItems)
                {
                    if (Model.CharactersInventory.GetCharacterItemAmount(player.CharacterId, neededItem.itemName, "inventory") >= (neededItem.itemAmount * amount)) continue;
                    HUDHandler.SendNotification(player, 3, 7000, "Du hast nicht genügend Gegenstände dabei.");
                    return;
                }
                foreach (Server_Crafting_Recipes_ItemGroup neededItem in recipe.neededItems) Model.CharactersInventory.RemoveCharacterItemAmount(player.CharacterId, neededItem.itemName, neededItem.itemAmount * amount, "inventory");
                Model.CharactersInventory.AddCharacterItem(player.CharacterId, recipe.endItem.itemName, recipe.endItem.itemAmount * amount, "inventory");
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast {recipe.endItem.itemAmount * amount}x {recipe.endItem.itemName} hergestellt.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }
    }
}
