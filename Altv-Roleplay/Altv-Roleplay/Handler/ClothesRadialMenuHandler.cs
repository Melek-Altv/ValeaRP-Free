using System;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;

namespace Altv_Roleplay.Handler
{
    class ClothesRadialMenuHandler : IScript
    {
        [AsyncClientEvent("Server:ClothesRadial:GetClothesRadialItems")]
        public static void GetAnimationItems(IPlayer player)
        {
            try
            {
                var interactHTML = "";
                interactHTML += "<li><p id='InteractionMenu-SelectedTitle'>Schließen</p></li><li class='interactitem' data-action='close' data-actionstring='Schließen'><img src='../utils/img/cancel.png'></li>";

                interactHTML += "<li class='interactitem' id='InteractionMenu-maske' data-action='maske' data-actionstring='Maske ausziehen'><img src='../utils/img/Maske.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-hut' data-action='hut' data-actionstring='Hut ausziehen'><img src='../utils/img/witch-hat.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-brille' data-action='brille' data-actionstring='Brille ausziehen'><img src='../utils/img/sun-glasses.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-tshirt' data-action='tshirt' data-actionstring='T-Shirt ausziehen'><img src='../utils/img/shirt.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-unterhemd' data-action='unterhemd' data-actionstring='Unterhemd ausziehen'><img src='../utils/img/undershirt.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-hose' data-action='hose' data-actionstring='Hose ausziehen'><img src='../utils/img/jeans.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-schuhe' data-action='schuhe' data-actionstring='Schuhe ausziehen'><img src='../utils/img/shoes.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-kette' data-action='kette' data-actionstring='Kette ausziehen'><img src='../utils/img/necklace.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-watch' data-action='watch' data-actionstring='Uhr ausziehen'><img src='../utils/img/watch.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-ears' data-action='ears' data-actionstring='Ohrring ausziehen'><img src='../utils/img/ears.png'></li>";
                interactHTML += "<li class='interactitem' id='InteractionMenu-armor' data-action='armor' data-actionstring='Schutzweste ausziehen'><img src='../utils/img/armor.png'></li>";

                player.EmitLocked("Client:ClothesRadial:SetMenuItems", interactHTML);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:ClothesRadial:SetNormalSkin")]
        public static void SetNormalSkin(IPlayer player, string action)
        {
            if (player == null || !player.Exists) return;
            int charid = User.GetPlayerOnline(player);
            int type = 0;
            string ClothesType = "Cloth";
            string TypeText = "none";
            if (charid == 0) return;

            if (action == "maske")
            {
                if (!player.HasData("HasMaskOn"))
                {
                    if (!Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(1, 0, 0, 0);
                    }
                    else if (Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(1, 0, 0, 0);
                    }
                    player.SetData("HasMaskOn", true);
                    return;
                }
                type = 1;
                TypeText = "Mask";
                player.DeleteData("HasMaskOn");
            }
            else if (action == "hut")
            {
                if (!player.HasData("HasHatOn"))
                {
                    if (!Characters.GetCharacterGender(charid))
                    {
                        player.SetProps(0, 11, 0);
                    }
                    else if (Characters.GetCharacterGender(charid))
                    {
                        player.SetProps(0, 57, 0);
                    }
                    player.SetData("HasHatOn", true);
                    return;
                }
                type = 0;
                TypeText = "Hat";
                ClothesType = "Prop";
                player.DeleteData("HasHatOn");
            }
            else if (action == "brille")
            {
                if (!player.HasData("HasGlassesOn"))
                {
                    if (!Characters.GetCharacterGender(charid))
                    {
                        player.SetProps(1, 0, 0);
                    }
                    else if (Characters.GetCharacterGender(charid))
                    {
                        player.SetProps(1, 5, 0);
                    }
                    player.SetData("HasGlassesOn", true);
                    return;
                }
                type = 1;
                TypeText = "Glass";
                ClothesType = "Prop";
                player.DeleteData("HasGlassesOn");
            }
            else if (action == "tshirt")
            {
                if (!player.HasData("HasShirtOn"))
                {
                    if (!Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(11, 15, 0, 0);
                        player.SetClothes(3, 15, 0, 0);
                    }
                    else if (Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(11, 18, 0, 0);
                        player.SetClothes(3, 15, 0, 0);
                    }
                    player.SetData("HasShirtOn", true);
                    return;
                }
                type = 11;
                TypeText = "Top";
                player.DeleteData("HasShirtOn");
                Characters.SetCharacterCorrectTorso(player, ServerClothes.GetClothesDraw(Characters.GetCharacterClothes(charid, "Top")));
            }
            else if (action == "unterhemd")
            {
                if (!player.HasData("HasUndershirtOn"))
                {
                    if (!Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(8, 15, 0, 0);
                    }
                    else if (Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(8, 14, 0, 0);
                    }
                    player.SetData("HasUndershirtOn", true);
                    return;
                }
                type = 8;
                TypeText = "Undershirt";
                player.DeleteData("HasUndershirtOn");
            }
            else if (action == "hose")
            {
                if (!player.HasData("HasPantsOn"))
                {
                    if (!Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(4, 21, 0, 0);
                    }
                    else if (Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(4, 17, 0, 0);
                    }
                    player.SetData("HasPantsOn", true);
                    return;
                }
                type = 4;
                TypeText = "Leg";
                player.DeleteData("HasPantsOn");
            }
            else if (action == "schuhe")
            {
                if (!player.HasData("HasShoesOn"))
                {
                    if (!Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(6, 34, 0, 0);
                    }
                    else if (Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(6, 35, 0, 0);
                    }
                    player.SetData("HasShoesOn", true);
                    return;
                }
                type = 6;
                TypeText = "Feet";
                player.DeleteData("HasShoesOn");
            }
            else if (action == "kette")
            {
                if (!player.HasData("HasNecklaceOn"))
                {
                    if (!Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(7, 0, 0, 0);
                    }
                    else if (Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(7, 0, 0, 0);
                    }
                    player.SetData("HasNecklaceOn", true);
                    return;
                }
                type = 7;
                TypeText = "Necklace";
                player.DeleteData("HasNecklaceOn");
            }
            else if (action == "armor")
            {
                if (!player.HasData("HasArmorOn"))
                {
                    if (!Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(9, 0, 0, 0);
                    }
                    else if (Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(9, 0, 0, 0);
                    }
                    player.SetData("HasArmorOn", true);
                    return;
                }
                type = 9;
                TypeText = "Armor";
                player.DeleteData("HasArmorOn");
            }
            else if (action == "watch")
            {
                if (!player.HasData("HasWatchOn"))
                {
                    if (!Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(6, 2, 0, 0);
                    }
                    else if (Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(6, 1, 0, 0);
                    }
                    player.SetData("HasWatchOn", true);
                    return;
                }
                type = 6;
                TypeText = "Watch";
                ClothesType = "Prop";
                player.DeleteData("HasWatchOn");
            }
            else if (action == "ears")
            {
                if (!player.HasData("HasEarsOn"))
                {
                    if (!Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(2, 3, 0, 0);
                    }
                    else if (Characters.GetCharacterGender(charid))
                    {
                        player.SetClothes(2, 22, 0, 0);
                    }
                    player.SetData("HasEarsOn", true);
                    return;
                }
                type = 2;
                TypeText = "Earring";
                ClothesType = "Prop";
                player.DeleteData("HasEarsOn");
            }


            if (TypeText == "none") return;
            if (ClothesType == "Prop")
            {
                int text = ServerClothes.GetClothesTexture(Characters.GetCharacterClothes(charid, TypeText));
                if (ServerClothes.GetClothesTexture(Characters.GetCharacterClothes(charid, TypeText)) == 0 && TypeText == "Earring")
                {
                    text = 120;
                    player.SetProps((byte)type, (ushort)ServerClothes.GetClothesDraw(Characters.GetCharacterClothes(charid, TypeText)), (byte)text);
                    return;
                }
                player.SetProps((byte)type, (ushort)ServerClothes.GetClothesDraw(Characters.GetCharacterClothes(charid, TypeText)), (byte)text);
                return;
            }
            if (ServerFactions.IsCharacterInAnyFaction(charid) && ServerFactions.IsCharacterInFactionDuty(charid))
            {
                ServerFactions.SetCharactersFactionClothes((ClassicPlayer)player, charid);
                player.DeleteData("HasEarsOn");
                player.DeleteData("HasWatchOn");
                player.DeleteData("HasArmorOn");
                player.DeleteData("HasNecklaceOn");
                player.DeleteData("HasShoesOn");
                player.DeleteData("HasPantsOn");
                player.DeleteData("HasUndershirtOn");
                player.DeleteData("HasGlassesOn");
                player.DeleteData("HasShirtOn");
                player.DeleteData("HasHatOn");
                player.DeleteData("HasMaskOn");
            }
            else
            {
                player.SetClothes((byte)type, (ushort)ServerClothes.GetClothesDraw(Characters.GetCharacterClothes(charid, TypeText)), (byte)ServerClothes.GetClothesTexture(Characters.GetCharacterClothes(charid, TypeText)), 0);
            }
        }
    }
}
