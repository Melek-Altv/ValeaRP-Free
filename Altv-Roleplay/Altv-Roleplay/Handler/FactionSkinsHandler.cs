using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using System;
using System.Linq;

namespace Altv_Roleplay.Handler
{
    internal class FactionSkinsHandler : IScript
    {
        public static void OpenFactionSkinsMenu(ClassicPlayer player)
        {
            if (player == null || !player.Exists) return;
            player.Emit("Client:FrakSkinBrowser:Load");
        }

        [AsyncClientEvent("Server:FrakSkinBrowser:cefIsReady")]
        public static void LoadFactionSkins(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists) return;
                int charId = User.GetPlayerOnline(player);
                int factionId = ServerFactions.GetCharacterFactionId(charId);
                int playerRank = ServerFactions.GetCharacterFactionRank(charId);
                int gender = 0;
                if (Characters.GetCharacterGender(charId))
                {
                    gender = 1;
                }
                var factionSkins = ServerFactions.FactionSkins_.Where(x => x.faction == factionId && x.rank <= playerRank && x.gender == gender);
                int zero = 0;
                string html = $"<button onclick='callskin({zero})'>Aus dem Dienst gehen</button>";
                foreach (var skin in factionSkins)
                {
                    html += $"<button onclick='callskin({skin.id})'>{skin.clothesName}</button>";
                }
                player.EmitLocked("Client:FrakSkinBrowser:SetSkins", html);
            }
            catch (Exception ex)
            {
                Alt.Log(ex.ToString());
            }
        }

        [AsyncClientEvent("Server:FrakSkinBrowser:setSkin")]
        public static void SetFactionSkins(ClassicPlayer player, int id)
        {
            try
            {
                if (player == null || !player.Exists) return;
                int charId = User.GetPlayerOnline(player);
                var skin = ServerFactions.FactionSkins_.FirstOrDefault(x => x.id == id);
                player.EmitLocked("Server:FrakSkinBrowser:Close");
                if (skin == null && id != 0)
                {
                    return;
                }
                if (id != 0)
                {
                    player.SetClothes(0, (ushort)skin.hat, (byte)skin.hattex, 0);                       //  Hat
                    player.SetClothes(1, (ushort)skin.glasses, (byte)skin.glassestex, 0);               //  Sonnenbrille
                    player.SetClothes(11, (ushort)skin.top, (byte)skin.toptex, 0);                      //  Oberbekleidung
                    player.SetClothes(3, (ushort)skin.torso, 0, 0);                                     //  Körper
                    player.SetClothes(8, (ushort)skin.undershirt, (byte)skin.undershirttex, 0);         //  Unterbekleidung
                    player.SetClothes(4, (ushort)skin.leg, (byte)skin.legtex, 0);                       //  Hose 
                    player.SetClothes(6, (ushort)skin.shose, (byte)skin.shosetex, 0);                   //  Schuhe
                    player.SetClothes(10, (ushort)skin.decal, (byte)skin.decaltex, 0);                  //  Decal
                }
                else
                {
                    Characters.SetCharacterCorrectClothes(player);
                }

            }
            catch (Exception ex)
            {
                Alt.Log(ex.ToString());
            }
        }
        

    }
}
