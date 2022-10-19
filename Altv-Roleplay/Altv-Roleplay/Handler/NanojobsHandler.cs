using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Altv_Roleplay.Handler
{
    class NanojobsHandler : IScript
    {
        public static async void AngelnAction (ClassicPlayer player, int charid)
        {
            var nanojobsPos = ServerNanojobs.Servernanojobs_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
            if (nanojobsPos == null) return;
            if (player.isNanonjobUsing == true) return;
            var itemName = "Barsch";
            int charId = (int)player.GetCharacterMetaId();
            if (charid == 0 || !player.Exists || player == null) return;
            if (!CharactersInventory.ExistCharacterItem(charId, "Angel", "inventory")) { 
                HUDHandler.SendNotification(player, 4, 7000, "Du hast keine Angel dabei!"); 
                return; 
            }

            //Item Seltenheit + Items
            int rnd11 = new Random().Next(1, 100);
            if (rnd11 <= 25) { itemName = "Barsch"; }//25%
            if (rnd11 <= 25) { itemName = "Forelle"; }//25%
            if (rnd11 <= 25) { itemName = "Muschel"; }//25%
            if (rnd11 <= 80 && rnd11 > 75) { itemName = "Karpfen"; }//5%
            if (rnd11 <= 82 && rnd11 > 80) { itemName = "Makrele"; }//2%
            if (rnd11 <= 90 && rnd11 > 82) { itemName = "Dorsch"; }//8%
            if (rnd11 <= 95 && rnd11 > 90) { itemName = "Zander"; }//5%
            if (rnd11 <= 100 && rnd11 > 95) { itemName = "Hecht"; }//5%

            int rndItemAmount = new Random().Next(1, 6);
            if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { 
                HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); 
                return; 
            }
            await AttachmentHandler.AngelAttachment(player, true);
            int duration = 15000;
            HUDHandler.SendNotification(player, 2, duration, "Du angelst nun.");
            Extensions.createProgress(player, 150);
            player.EmitLocked("Client:Inventory:PlayAnimation", "amb@world_human_stand_fishing@idle_a", "idle_a", duration, 49, false);

            player.isNanonjobUsing = true;
            await Task.Delay(duration + 1200);
            player.isNanonjobUsing = false;

            //Doppelte Menge aufsammeln
            if (Characters.IsCharacterFastFarm(charid)) rndItemAmount += 1;
            await AttachmentHandler.AngelAttachment(player, false);
            float itemWeight = ServerItems.GetItemWeight(itemName) * rndItemAmount;
            float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
            float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
            if (invWeight + itemWeight > 15f && backpackWeight + itemWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId))) { 
                HUDHandler.SendNotification(player, 3, 7000, $"Deine Taschen sind voll."); 
                return; 
            }

            if (invWeight + itemWeight <= 15f)
            {
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast {itemName} ({rndItemAmount}x) gesammelt (Lagerort: Inventar).");
                CharactersInventory.AddCharacterItem(charId, itemName, rndItemAmount, "inventory");
                return;
            }

            if (Characters.GetCharacterBackpack(charId) != "None" && backpackWeight + itemWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
            {
                HUDHandler.SendNotification(player, 2, 7000, $"Du hast {itemName} ({rndItemAmount}x) gesammelt (Lagerort: Rucksack / Tasche).");
                CharactersInventory.AddCharacterItem(charId, itemName, rndItemAmount, "backpack");
                return;
            }

        }
    }
}
