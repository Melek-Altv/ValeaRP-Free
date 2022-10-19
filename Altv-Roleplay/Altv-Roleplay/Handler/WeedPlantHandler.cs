using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;
using Altv_Roleplay.models;
using System.Timers;
using System.Numerics;

namespace Altv_Roleplay.Handler
{
    public partial class WeedPot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public int remainingMinutes { get; set; }
        public int dimension { get; set; }
        public int water { get; set; }
        public bool hasFertilizer { get; set; }
        public Position position { get; set; }
        public int state { get; set; } // 1 = Benötigt Wasser, 2 = Wächst, 3 = Erntebereit, 4 = tot.

        [NotMapped]
        public EntityStreamer.Prop prop { get; set; } = null;

        [NotMapped]
        public EntityStreamer.PlayerLabel textLabel { get; set; } = null;
    }

    public class WeedPlantHandler : IScript
    {
        public static List<WeedPot> WeedPots_ = new List<WeedPot>();
        public static readonly string smallObject = "bkr_prop_weed_01_small_01a";
        public static readonly string midObject = "bkr_prop_weed_med_01a";
        public static readonly string bigObject = "bkr_prop_weed_lrg_01a";

        public static void LoadAllWeedPots()
        {
            using (var db = new gtaContext())
            {
                WeedPots_ = new List<WeedPot>(db.WeedPot);
                Alt.Log($"{WeedPots_.Count} Weedpots geladen..");
            }

            foreach (WeedPot pot in WeedPots_)
            {
                switch (pot.state)
                {
                    case 4:
                        pot.prop = EntityStreamer.PropStreamer.Create(smallObject, pot.position, new Position(), pot.dimension, frozen: true);
                        pot.textLabel = EntityStreamer.TextLabelStreamer.Create($"Weed Pflanze\nStatus: zerstört.\nDrücke E zum entfernen.", new Position(pot.position.X, pot.position.Y, pot.position.Z + 1), pot.dimension, streamRange: 1);
                        break;
                    case 3:
                        pot.prop = EntityStreamer.PropStreamer.Create(bigObject, new Position(pot.position.X, pot.position.Y, pot.position.Z - 2.5f), new Position(), pot.dimension, frozen: true);
                        pot.textLabel = EntityStreamer.TextLabelStreamer.Create($"Weed Pflanze\nStatus: erntebereit.\nDrücke E zum ernten.", new Position(pot.position.X, pot.position.Y, pot.position.Z + 1), pot.dimension, streamRange: 1);
                        break;
                    case 2:
                        pot.prop = EntityStreamer.PropStreamer.Create(midObject, new Position(pot.position.X, pot.position.Y, pot.position.Z - 2.5f), new Position(), pot.dimension, frozen: true);
                        pot.textLabel = EntityStreamer.TextLabelStreamer.Create($"Weed Pflanze\nStatus: Wachstum.\nWasser: {pot.water}%\nDünger: Ja\nWachsdauer: {pot.remainingMinutes}", new Position(pot.position.X, pot.position.Y, pot.position.Z + 1), pot.dimension, streamRange: 1);
                        break;
                    case 1:
                        pot.prop = EntityStreamer.PropStreamer.Create(smallObject, pot.position, new Position(), pot.dimension, frozen: true);
                        pot.textLabel = EntityStreamer.TextLabelStreamer.Create($"Weed Pflanze\nStatus: benötigt Wasser.\nWasser: {pot.water}%", new Position(pot.position.X, pot.position.Y, pot.position.Z + 1), pot.dimension, streamRange: 1);
                        break;
                }
            }

            System.Timers.Timer weedPotTimer = new System.Timers.Timer();
            weedPotTimer.Interval = 2000;
            weedPotTimer.Elapsed += WeedPotTimerElapsed;
            weedPotTimer.Enabled = true;
        }

        private static void WeedPotTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (WeedPot pot in WeedPots_.Where(x => x.state == 2 && x.remainingMinutes > 0))
                {
                    lock (pot)
                    {
                        pot.water -= 2;
                        pot.remainingMinutes -= 1;
                        UpdatePotLabelAndObject(pot);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void UpdatePotLabelAndObject(WeedPot pot)
        {
            if (pot == null) return;
            lock (pot)
            {
                if (pot.water >= 100 || pot.water < 0)
                {
                    pot.state = 4;
                    pot.prop.Delete();
                    pot.prop = EntityStreamer.PropStreamer.Create(smallObject, pot.position, new Position(), pot.dimension, frozen: true);
                    pot.textLabel.SetText($"Weed Pflanze\nStatus: zerstört.\nDrücke E zum entfernen.");
                    return;
                }

                if (pot.remainingMinutes <= 0)
                {
                    pot.state = 3;
                    pot.prop.Delete();
                    pot.prop = EntityStreamer.PropStreamer.Create(bigObject, new Position(pot.position.X, pot.position.Y, pot.position.Z - 2.5f), new Position(), pot.dimension, frozen: true);
                    pot.textLabel.SetText($"Weed Pflanze\nStatus: erntebereit.\nDrücke E zum ernten.");
                    return;
                }

                string hasFertilizer = pot.hasFertilizer == true ? "Ja" : "Nein";
                pot.textLabel.SetText($"Weed Pflanze\nStatus: Wachstum.\nWasser: {pot.water}%\nDünger: {hasFertilizer}\nWachsdauer: {pot.remainingMinutes}");
            }

            using (var db = new gtaContext())
            {
                db.WeedPot.Update(pot);
                db.SaveChanges();
            }
        }

        internal static void HarvestPot(IPlayer player, WeedPot weedPot)
        {
            if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0 || weedPot == null) return;
            int amount = new Random().Next(3, 5);
            if (weedPot.hasFertilizer) amount = new Random().Next(4, 8);
            float invWeight = CharactersInventory.GetCharacterItemWeight(User.GetPlayerOnline(player), "inventory");
            float itemWeight1 = ServerItems.GetItemWeight("Knolle") * amount;
            float itemWeight2 = ServerItems.GetItemWeight("Blumentopf");
            float backpackWeight = CharactersInventory.GetCharacterItemWeight(User.GetPlayerOnline(player), "backpack");
            weedPot.prop.Delete();
            weedPot.textLabel.Delete();
            WeedPots_.Remove(weedPot);

            if (invWeight + itemWeight1 + itemWeight2 <= 15f)
            {
                CharactersInventory.AddCharacterItem(User.GetPlayerOnline(player), "Knolle", amount, "inventory");
                CharactersInventory.AddCharacterItem(User.GetPlayerOnline(player), "Blumentopf", 1, "inventory");
            }
            else if (Characters.GetCharacterBackpack(User.GetPlayerOnline(player)) != "None" && backpackWeight + itemWeight1 + itemWeight2 <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(User.GetPlayerOnline(player))))
            {
                CharactersInventory.AddCharacterItem(User.GetPlayerOnline(player), "Knolle", amount, "backpack");
                CharactersInventory.AddCharacterItem(User.GetPlayerOnline(player), "Blumentopf", 1, "backpack");
            }
            else
            {
                HUDHandler.SendNotification(player, 3, 7000, $"Du hast nicht genug Platz in deinen Taschen.");
                return;
            }

            HUDHandler.SendNotification(player, 2, 7000, $"Du hast die Pflanze geerntet und {amount} Knollen erhalten.");
            using (var db = new gtaContext())
            {
                db.WeedPot.Remove(weedPot);
                db.SaveChanges();
            }
        }

        internal static void RemoveOldPot(IPlayer player, WeedPot weedPot)
        {
            if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0 || weedPot == null) return;
            float invWeight = CharactersInventory.GetCharacterItemWeight(User.GetPlayerOnline(player), "inventory");
            float itemWeight = ServerItems.GetItemWeight("Blumentopf");
            float backpackWeight = CharactersInventory.GetCharacterItemWeight(User.GetPlayerOnline(player), "backpack");
            weedPot.prop.Delete();
            weedPot.textLabel.Delete();
            WeedPots_.Remove(weedPot);

            if (invWeight + itemWeight <= 15f) CharactersInventory.AddCharacterItem(User.GetPlayerOnline(player), "Blumentopf", 1, "inventory");
            else if (backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(User.GetPlayerOnline(player)))) CharactersInventory.AddCharacterItem(User.GetPlayerOnline(player), "Blumentopf", 1, "backpack");
            else if (Characters.GetCharacterBackpack(User.GetPlayerOnline(player)) != "None" && backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(User.GetPlayerOnline(player)))) CharactersInventory.AddCharacterItem(User.GetPlayerOnline(player), "Blumentopf", 1, "backpack");

            HUDHandler.SendNotification(player, 2, 2500, $"Du hast die kaputte Pflanze entfernt.");
            using (var db = new gtaContext())
            {
                db.WeedPot.Remove(weedPot);
                db.SaveChanges();
            }
        }

        public static void PlaceNewWeedpot(IPlayer player)
        {
            if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0) return;
            Vector3 forwardVector = ((ClassicPlayer)player).getForwardVector();
            WeedPot pot = new WeedPot
            {
                dimension = player.Dimension,
                position = new Position(player.Position.X + forwardVector.X, player.Position.Y + forwardVector.Y, player.Position.Z - 1 + forwardVector.Z),
                remainingMinutes = 60,
                state = 1,
                water = 0,
                hasFertilizer = false,
                prop = EntityStreamer.PropStreamer.Create(smallObject, new Position(player.Position.X + forwardVector.X, player.Position.Y + forwardVector.Y, player.Position.Z - 1 + forwardVector.Z), new Position(), player.Dimension, frozen: true),
                textLabel = EntityStreamer.TextLabelStreamer.Create($"Weed Pflanze\nStatus: benötigt Wasser.\nWasser: 0%", new Position(player.Position.X + forwardVector.X, player.Position.Y + forwardVector.Y, player.Position.Z + forwardVector.Z), player.Dimension, streamRange: 1),
            };

            WeedPots_.Add(pot);
            using (var db = new gtaContext())
            {
                db.WeedPot.Add(pot);
                db.SaveChanges();
            }

            HUDHandler.SendNotification(player, 2, 10000, "Du hast eine Weedpflanze angepflanzt. Gieße diese mit Wasser, damit der Wachsvorgang beginnt. Achte auf den Wasserstand, geht dieser auf 0, trocknet deine Pflanze aus.");
        }

        public static void FillNearestPotWithWater(IPlayer player)
        {
            if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0) return;
            WeedPot pot = WeedPots_.FirstOrDefault(x => player.Position.IsInRange(x.position, 2f));
            if (pot == null || pot.state == 3 || pot.state == 4) return;
            if (CharactersInventory.ExistCharacterItem(User.GetPlayerOnline(player), "Pflanzenwasser", "inventory")) CharactersInventory.RemoveCharacterItemAmount(User.GetPlayerOnline(player), "Pflanzenwasser", 1, "inventory");
            else if (CharactersInventory.ExistCharacterItem(User.GetPlayerOnline(player), "Pflanzenwasser", "backpack")) CharactersInventory.RemoveCharacterItemAmount(User.GetPlayerOnline(player), "Pflanzenwasser", 1, "backpack");
            string hasFertilizer = pot.hasFertilizer == true ? "Ja" : "Nein";
            lock (pot)
            {
                pot.water += 20;
                if (pot.water > 100)
                {
                    pot.state = 4;
                    pot.prop.Delete();
                    pot.prop = EntityStreamer.PropStreamer.Create(smallObject, pot.position, new Position(), pot.dimension, frozen: true);
                    pot.textLabel.SetText($"Weed Pflanze\nStatus: zerstört.\nDrücke E zum entfernen.");
                }
                else if (pot.water <= 100 && pot.state == 1)
                {
                    pot.state = 2;
                    pot.prop.Delete();
                    pot.prop = EntityStreamer.PropStreamer.Create(midObject, new Position(pot.position.X, pot.position.Y, pot.position.Z - 2.5f), new Position(), pot.dimension, frozen: true);
                    pot.textLabel.SetText($"Weed Pflanze\nStatus: Wachstum.\nWasser: {pot.water}%\nDünger: {hasFertilizer}\nWachsdauer: {pot.remainingMinutes}");
                    HUDHandler.SendNotification(player, 2, 5000, "Die Pflanze fängst nun an zu wachsen. Die Dauer beträgt 60 Minuten, achte regelmäßig darauf diese zu gießen. Mit Dünger kannst du die Wachstumsdauer verkürzen.");
                    return;
                }
                else if (pot.water <= 100 && pot.state == 2)
                {
                    pot.textLabel.SetText($"Weed Pflanze\nStatus: Wachstum.\nWasser: {pot.water}%\nDünger: {hasFertilizer}\nWachsdauer: {pot.remainingMinutes}");
                    return;
                }
            }
            using (var db = new gtaContext())
            {
                db.WeedPot.Update(pot);
                db.SaveChanges();
            }
        }

        public static void FertilizeNearestPot(IPlayer player)
        {
            if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0) return;
            WeedPot pot = WeedPots_.FirstOrDefault(x => player.Position.IsInRange(x.position, 2f));
            if (pot == null || pot.state != 2 || pot.hasFertilizer || pot.remainingMinutes < 30) return;
            pot.hasFertilizer = true;
            pot.remainingMinutes -= 15;
            string hasFertilizer = pot.hasFertilizer == true ? "Ja" : "Nein";
            pot.textLabel.SetText($"Weed Pflanze\nStatus: Wachstum.\nWasser: {pot.water}%\nDünger: {hasFertilizer}\nWachsdauer: {pot.remainingMinutes}");
        }
    }
}