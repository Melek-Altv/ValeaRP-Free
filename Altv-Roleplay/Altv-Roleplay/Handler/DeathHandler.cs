using System;
using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;

namespace Altv_Roleplay.Handler
{
    class DeathHandler : IScript
    {
        [ScriptEvent(ScriptEventType.PlayerDead)]
        public static void OnPlayerDeath(ClassicPlayer player, IEntity killer, uint weapon)
        {
            try
            {
                if (player == null || !player.Exists) return;
                int charId = (int)player.GetCharacterMetaId();
                if (charId <= 0) return;
                if (Characters.IsCharacterUnconscious(charId)) return;
                if (Characters.IsCharacterInJail(charId))
                {
                    lock (player)
                    {
                        player.Spawn(new Position(1691.4594f, 2565.7056f, 45.556763f), 0);
                        player.Position = new Position(1691.4594f, 2565.7056f, 45.556763f);
                        player.LastPosition = new Position(1691.4594f, 2565.7056f, 45.556763f);
                    }
                    return;
                }

                OpenDeathscreen(player);
                Characters.SetCharacterUnconscious(charId, true, 10); // Von 15 auf 10 geändert.
                ServerFactions.AddNewFactionDispatch(0, 3, $"Eine Verletzte Person wurde gemeldet", player.Position);

                Alt.Emit("Server:Smartphone:leaveRadioFrequence", player);
                Alt.Emit("SaltyChat:SetPlayerAlive", player, false);

                SmartphoneHandler.DenyCall(player);

                ClassicPlayer killerPlayer = (ClassicPlayer)killer;
                if (killerPlayer == null || !killerPlayer.Exists) return;
                WeaponModel weaponModel = (WeaponModel)weapon;
                DiscordLog.SendEmbed("death", "Death - Log", $"{Characters.GetCharacterName(killerPlayer.CharacterId)} ({killerPlayer.CharacterId}) hat {Characters.GetCharacterName(player.CharacterId)} ({player.CharacterId}) getötet. Mit: {weaponModel}");
                if (weaponModel == WeaponModel.Fist) return;
                foreach (IPlayer p in Alt.GetAllPlayers().ToList().Where(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId > 0 && x.AdminLevel() > 0))
                {
                    HUDHandler.SendNotification(p, 4, 15000, $"{Characters.GetCharacterName(killerPlayer.CharacterId)} ({killerPlayer.CharacterId}) hat {Characters.GetCharacterName(player.CharacterId)} ({player.CharacterId}) getötet. Mit: {weaponModel}");
                }
                if (Enum.IsDefined(typeof(AntiCheat.forbiddenWeapons), (Utils.AntiCheat.forbiddenWeapons)weaponModel))
                {
                    if (AntiCheat.AnticheatConfig.death)
                    {
                        User.SetPlayerBanned(killerPlayer, true, $"Waffen Hack[2]: {weaponModel}");
                        killerPlayer.Kick("");
                        player.Health = 200;
                        foreach (IPlayer p in Alt.GetAllPlayers().ToList().Where(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId > 0 && x.AdminLevel() > 0))
                        {
                            HUDHandler.SendNotification(player, 4, 7000, $"{Characters.GetCharacterName(killerPlayer.CharacterId)} wurde gebannt: Waffenhack[2] - {weaponModel}");
                        }
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        internal static void OpenDeathscreen(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists) return;
                int charId = (int)player.GetCharacterMetaId();
                if (charId <= 0) return;
                Position pos = new Position(player.Position.X, player.Position.Y, player.Position.Z + 1);
                player.Spawn(pos);
                player.EmitLocked("Client:Ragdoll:SetPedToRagdoll", true, 0); //Ragdoll setzen
                player.EmitLocked("Client:Deathscreen:openCEF"); // Deathscreen öffnen
                player.SetPlayerIsUnconscious(true);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        internal static void CloseDeathscreen(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists) return;
                int charId = (int)player.GetCharacterMetaId();
                if (charId <= 0) return;
                player.EmitLocked("Client:Deathscreen:closeCEF");
                player.SetPlayerIsUnconscious(false);
                player.SetPlayerIsFastFarm(false);
                player.EmitLocked("Client:Ragdoll:SetPedToRagdoll", false, 2000);
                Characters.SetCharacterUnconscious(charId, false, 0);
                Characters.IsCharacterStabilisiert(charId, false);
                Characters.SetCharacterFastFarm(charId, false, 0);
                player.EmitLocked("Client:Inventory:StopEffect", "DrugsMichaelAliensFight");

                foreach (var item in CharactersInventory.CharactersInventory_.ToList().Where(x => x.charId == charId))
                {
                    if (item.itemName.Contains("EC Karte") || item.itemName.Contains("Ausweis") || item.itemName.Contains("Fahrzeugschluessel") || item.itemName.Contains("Generalschluessel") || item.itemName.Contains("Bargeld") || item.itemName.Contains("Bennys Clubkarte") || item.itemName.Contains("Einreisegeschenk") || item.itemName.Contains("Kaufvertrag") || ServerItems.GetItemType(ServerItems.ReturnNormalItemName(item.itemName)) == "clothes") continue;
                    CharactersInventory.RemoveCharacterItem(charId, item.itemName, item.itemLocation);
                }

                Characters.SetCharacterWeapon(player, "PrimaryWeapon", "None");
                Characters.SetCharacterWeapon(player, "PrimaryAmmo", 0);
                Characters.SetCharacterWeapon(player, "SecondaryWeapon2", "None");
                Characters.SetCharacterWeapon(player, "SecondaryWeapon", "None");
                Characters.SetCharacterWeapon(player, "SecondaryAmmo2", 0);
                Characters.SetCharacterWeapon(player, "SecondaryAmmo", 0);
                Characters.SetCharacterWeapon(player, "FistWeapon", "None");
                Characters.SetCharacterWeapon(player, "FistWeaponAmmo", 0);
                player.EmitLocked("Client:Smartphone:equipPhone", false, Characters.GetCharacterPhonenumber(charId), Characters.IsCharacterPhoneFlyModeEnabled(charId), Characters.GetCharacterPhoneWallpaper(charId));
                Characters.SetCharacterPhoneEquipped(charId, false);
                player.RemoveAllWeapons();
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
        internal static void Revive(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists) return;
                int charId = (int)player.GetCharacterMetaId();
                if (charId <= 0) return;
                player.EmitLocked("Client:Deathscreen:closeCEF");
                player.SetPlayerIsUnconscious(false);
                player.EmitLocked("Client:Ragdoll:SetPedToRagdoll", false, 2000);
                Characters.SetCharacterUnconscious(charId, false, 0);
                Characters.IsCharacterStabilisiert(charId, false);
                ServerFactions.SetFactionBankMoney(3, ServerFactions.GetFactionBankMoney(3) + 1500); //ToDo: Preis anpassen
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
    }
}
