using AltV.Net;
using AltV.Net.Async;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.models;
using Altv_Roleplay.Utils;
using System;
using System.Threading.Tasks;

namespace Altv_Roleplay.Handler
{
    class ATMRobHandler : IScript
    {
        internal static async Task RobATM(ClassicPlayer player, Server_ATM robATM)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || robATM == null) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 7000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                if (player.IsPlayerUsingCrowbar())
                {
                    player.EmitLocked("Client:Inventory:StopAnimation");
                    player.SetPlayerUsingCrowbar(false);
                    HUDHandler.SendNotification(player, 2, 7000, "Du hast den Aufbruch abgebrochen.");
                    return;
                }
                else
                {
                    //Aufbrechen
                    if (robATM.isRobbed == true)
                    {
                        HUDHandler.SendNotification(player, 3, 7000, "Dieser Automat wurde vor kurzem erst aufgebrochen.");
                        return;
                    }

                    if (ServerFactions.GetFactionDutyMemberCount(2) < 4)
                    {
                        HUDHandler.SendNotification(player, 3, 7000, "Es sind weniger als 4 Polizisten im Staat.");
                        return;
                    }

                    if (!CharactersInventory.ExistCharacterItem(player.CharacterId, "Schweissgerät", "inventory"))
                    {
                        HUDHandler.SendNotification(player, 3, 7000, "Du hast kein Schweißgerät dabei um den ATM aufzubrechen.");
                        return;
                    }

                    ServerFactions.AddNewFactionDispatch(0, 2, $"ATM Raub in Gange", new AltV.Net.Data.Position(robATM.posX, robATM.posY, robATM.posZ));
                    DiscordLog.SendEmbed("atmrob", "ATM Rob Log", $"Der Spieler { Characters.GetCharacterName(User.GetPlayerOnline(player))} hat den ATM {robATM.id} ausgeraubt.");
                    robATM.lastRobbed = DateTime.Now;
                    robATM.isRobbed = true;
                    int duration = 180000;
                    robATM.isRobbed = true;
                    int rndmMoney = new Random().Next(2250, 8000);
                    player.SetPlayerUsingCrowbar(true);
                    player.EmitLocked("Client:Animation:PlayScenario", "WORLD_HUMAN_WELDING", duration);
                    Extensions.createProgress(player, 1800);
                    await Task.Delay(duration);
                    robATM.isRobbed = true;
                    if (player == null || !player.Exists) return;
                    if (!player.Position.IsInRange(new AltV.Net.Data.Position(robATM.posX, robATM.posY, robATM.posZ), 5f))
                    {
                        HUDHandler.SendNotification(player, 3, 7000, "Aufbrechen abgebrochen, du bist zu weit entfernt."); player.SetPlayerUsingCrowbar(false);
                        return;
                    }
                    if (!player.IsPlayerUsingCrowbar()) return;

                    HUDHandler.SendNotification(player, 2, 7000, $"ATM aufgebrochen, du hast {rndmMoney}$ erbeutet - verschwinde..");
                    player.SetPlayerUsingCrowbar(false);
                    player.EmitLocked("Client:Inventory:StopAnimation");
                    CharactersInventory.AddCharacterItem(player.CharacterId, "Bargeld", rndmMoney, "brieftasche");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }
    }
}