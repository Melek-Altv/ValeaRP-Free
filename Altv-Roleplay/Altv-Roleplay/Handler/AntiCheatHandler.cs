using AltV.Net;
using AltV.Net.Data;
using Altv_Roleplay.Factories;
using System;
using System.Threading.Tasks;
using System.Linq;
using AltV.Net.Enums;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;
using AltV.Net.Elements.Entities;
using System.Timers;
using AltV.Net.Async.Elements.Refs;

namespace Altv_Roleplay.Handler
{
    public class AntiCheatHandler : IScript
    {
        [ScriptEvent(ScriptEventType.WeaponDamage)]
        public static async Task WeaponDamageEvent(ClassicPlayer player, ClassicPlayer target, uint weapon, ushort dmg, Position offset, BodyPart bodypart)
        {
            try
            {
                if (player == null || !player.Exists || target == null || !target.Exists) return;
                WeaponModel weaponModel = (WeaponModel)weapon;
                if (weaponModel == WeaponModel.Fist) return;
                if (Enum.IsDefined(typeof(AntiCheat.forbiddenWeapons), (Utils.AntiCheat.forbiddenWeapons)weaponModel))
                {
                    if (AntiCheat.AnticheatConfig.death)
                    {
                        User.SetPlayerBanned(player, true, $"Blacklisted Weaponkill: {weaponModel}");
                        player.Kick("");
                        foreach (IPlayer p in Alt.GetAllPlayers().ToList().Where(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId > 0 && x.AdminLevel() > 0))
                        {
                            HUDHandler.SendNotification(player, 4, 7000, $"{Characters.GetCharacterName(player.CharacterId)} wurde gebannt: Waffenhack[2] - {weaponModel}");
                        }
                        return;
                    }
#pragma warning disable CS0162 // Unerreichbarer Code wurde entdeckt.
                    if (bodypart == BodyPart.Head)
#pragma warning restore CS0162 // Unerreichbarer Code wurde entdeckt.
                    {
                        int adminlevel = target.AdminLevel();
                        if (adminlevel > 2)
                        {
                            if (target.headhits > 3 && !target.IsUnconscious) DiscordLog.SendEmbed("adminmenu", "Anticheat Logs", $"Der Spieler {Characters.GetCharacterName((int)player.GetCharacterMetaId())} könnte ein Cheater sein! | Autoheal");
                            target.headhits = target.headhits + 1;
                            await Task.Delay(1000);
                            if (target.IsUnconscious) target.headhits = target.headhits - 2;
                            await Task.Delay(4000);
                            target.headhits = target.headhits - 1;
                            if (target.headhits < 0) target.headhits = 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        internal static void OnanticheatTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!AntiCheat.AnticheatConfig.teleport) return;
                foreach (ClassicPlayer player in Alt.GetAllPlayers().ToList())
                {
                    if (player == null) continue;
                    using var pRef = new AsyncPlayerRef(player);
                    if (!pRef.Exists) return;
                    if (player.Exists && User.GetPlayerOnline(player) != 0)
                    {
                        if (AntiCheat.AnticheatConfig.teleport) CheckTeleport(player);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.OutputLog($"[Exception] {ex}", ConsoleColor.Red);
            }
        }

        #region Teleport
        public static void CheckTeleport(ClassicPlayer player)
        {
            if (player.IsInVehicle)
            {
                if (player.Position.Distance(player.LastPosition) > AntiCheat.AnticheatConfig.TELEPORT_KICK_VEHICLE)
                {
                    Log.OutputLog($"[{AntiCheat.AnticheatConfig.anticheatName} {AntiCheat.AnticheatConfig.version}] Teleport detected by {Characters.GetCharacterName(player.CharacterId)}");
                    User.SetPlayerBanned(player.Id, true, $"[{AntiCheat.AnticheatConfig.anticheatName} {AntiCheat.AnticheatConfig.version}] Teleport detected");
                    player.Kick($"Dieser Benutzeraccount wurde gebannt! | [{AntiCheat.AnticheatConfig.anticheatName} {AntiCheat.AnticheatConfig.version}] Teleport");
                    foreach (IPlayer p in Alt.GetAllPlayers().ToList().Where(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId > 0 && x.AdminLevel() > 0))
                    {
                        HUDHandler.SendNotification(p, 3, 7000, $"{Characters.GetCharacterName(player.CharacterId)} wurde von [{AntiCheat.AnticheatConfig.anticheatName} {AntiCheat.AnticheatConfig.version}] gesperrt!");
                    }
                }
            }

            else if (player.IsUnconscious || player.AdminLevel() > 1) return;
            if (player.Position.Z < 150)
            {
                if (player.Position.Distance(player.LastPosition) > AntiCheat.AnticheatConfig.TELEPORT_KICK_FOOT)
                {
                    Log.OutputLog($"[{AntiCheat.AnticheatConfig.anticheatName} {AntiCheat.AnticheatConfig.version}] Teleportdetected by {Characters.GetCharacterName(player.CharacterId)}");
                    User.SetPlayerBanned(player.Id, true, $"[{AntiCheat.AnticheatConfig.anticheatName} {AntiCheat.AnticheatConfig.version}] Teleport detected");
                    player.Kick($"Dieser Benutzeraccount wurde gebannt! | [{AntiCheat.AnticheatConfig.anticheatName} {AntiCheat.AnticheatConfig.version}] Teleport");
                    foreach (IPlayer p in Alt.GetAllPlayers().ToList().Where(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId > 0 && x.AdminLevel() > 0))
                    {
                        HUDHandler.SendNotification(p, 3, 7000, $"{Characters.GetCharacterName(player.CharacterId)} wurde von [{AntiCheat.AnticheatConfig.anticheatName} {AntiCheat.AnticheatConfig.version}] gesperrt!");
                    }
                }
            }
            player.LastPosition = player.Position;
        }
        #endregion
    }
}
