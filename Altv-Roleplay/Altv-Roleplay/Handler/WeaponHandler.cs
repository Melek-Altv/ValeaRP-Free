using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Altv_Roleplay.Model;
using System;

namespace Altv_Roleplay.Handler
{
    class WeaponHandler : IScript
    {
        public static void EquipCharacterWeapon(IPlayer player, string type, string wName, int amount, string fromContainer)
        {
            try
            {
                int charId = User.GetPlayerOnline(player);
                string wType = "None";
                string normalWName = "None";
                string ammoWName = "None";
                WeaponModel wHash = 0;

                switch (wName)
                {
                    // Mit Munition
                    case "Gefechtspistole":
                    case "Gefechtspistole Magazin":
                        wType = "Secondary";
                        normalWName = "Gefechtspistole";
                        ammoWName = "Gefechtspistole";
                        wHash = (WeaponModel)0x5EF9FEC4;
                        break;
                    case "SMG":
                    case "SMG Magazin":
                        wType = "Secondary";
                        normalWName = "SMG";
                        ammoWName = "SMG";
                        wHash = (WeaponModel)0x2BE6766B;
                        break;
                    case "PDW":
                    case "PDW Magazin":
                        wType = "Secondary";
                        normalWName = "PDW";
                        ammoWName = "PDW";
                        wHash = (WeaponModel)0xA3D4D34;
                        break;
                    case "AssaultSMG":
                    case "AssaultSMG Magazin":
                        wType = "Secondary";
                        normalWName = "AssaultSMG";
                        ammoWName = "AssaultSMG";
                        wHash = (WeaponModel)0xEFE7E2DF;
                        break;
                    case "Micro SMG":
                    case "Micro SMG Magazin":
                        wType = "Secondary";
                        normalWName = "Micro SMG";
                        ammoWName = "Micro SMG";
                        wHash = (WeaponModel)0x13532244;
                        break;
                    case "CombatMGMk2":
                    case "CombatMGMk2 Magazin":
                        wType = "Secondary";
                        normalWName = "CombatMGMk2";
                        ammoWName = "CombatMGMk2";
                        wHash = (WeaponModel)0xDBBD7280;
                        break;
                    case "Gusenberg":
                    case "Gusenberg Magazin":
                        wType = "Primary";
                        normalWName = "Gusenberg";
                        ammoWName = "Gusenberg";
                        wHash = (WeaponModel)0x61012683;
                        break;
                    case "Heavy Pistol":
                    case "Heavy Pistol Magazin":
                        wType = "Secondary";
                        normalWName = "Heavy Pistol";
                        ammoWName = "Heavy Pistol";
                        wHash = (WeaponModel)0xD205520E;
                        break;
                    case "Pistol50":
                    case "Pistol50 Magazin":
                        wType = "Secondary";
                        normalWName = "Pistol50";
                        ammoWName = "Pistol50";
                        wHash = (WeaponModel)0x99AEEB3B;
                        break;
                    case "APPistole":
                    case "APPistole Magazin":
                        wType = "Secondary";
                        normalWName = "APPistole";
                        ammoWName = "APPistole";
                        wHash = (WeaponModel)0x22D8FE39;
                        break;
                    case "Pistole":
                    case "Pistole Magazin":
                        wType = "Secondary";
                        normalWName = "Pistole";
                        ammoWName = "Pistole";
                        wHash = (WeaponModel)0x1B06D571;
                        break;
                    case "SpecialCarbine":
                    case "SpecialCarbine Magazin":
                        wType = "Primary";
                        normalWName = "SpecialCarbine";
                        ammoWName = "SpecialCarbine";
                        wHash = (WeaponModel)0xC0A3098D;
                        break;
                    case "Assault Rifle":
                    case "Assault Rifle Magazin":
                        wType = "Primary";
                        normalWName = "Assault Rifle";
                        ammoWName = "Assault Rifle";
                        wHash = (WeaponModel)0xBFEFFF6D;
                        break;
                    case "CarabineRifle":
                    case "CarabineRifle Magazin":
                        wType = "Primary";
                        normalWName = "CarabineRifle";
                        ammoWName = "CarabineRifle";
                        wHash = (WeaponModel)0x83BF0278;
                        break;
                    case "Bullpup Gewehr":
                    case "Bullpup Gewehr Magazin":
                        wType = "Primary";
                        normalWName = "Bullpup Gewehr";
                        ammoWName = "Bullpup Gewehr";
                        wHash = (WeaponModel)0x7F229F94;
                        break;
                    case "SawnoffShotgun":
                    case "SawnoffShotgun Magazin":
                        wType = "Primary";
                        normalWName = "SawnoffShotgun";
                        ammoWName = "SawnoffShotgun";
                        wHash = (WeaponModel)0x7846A318;
                        break;
                    case "Compact Rifle":
                    case "Compact Rifle Magazin":
                        wType = "Primary";
                        normalWName = "Compact Rifle";
                        ammoWName = "Compact Rifle";
                        wHash = (WeaponModel)0x624FE830;
                        break;
                    case "HeavyShotgun":
                    case "HeavyShotgun Magazin":
                        wType = "Primary";
                        normalWName = "HeavyShotgun";
                        ammoWName = "HeavyShotgun";
                        wHash = (WeaponModel)0x3AABBBAA;
                        break;
                    case "PumpShotgun":
                    case "PumpShotgun Magazin":
                        wType = "Primary";
                        normalWName = "PumpShotgun";
                        ammoWName = "PumpShotgun";
                        wHash = (WeaponModel)0x1D073A89;
                        break;
                    case "Sniper":
                    case "Sniper Magazin":
                        wType = "Primary";
                        normalWName = "Sniper";
                        ammoWName = "Sniper";
                        wHash = (WeaponModel)0x5FC3C11;
                        break;
                    // Hand
                    case "Schlagstock":
                        wType = "Fist";
                        normalWName = "Schlagstock";
                        wHash = (WeaponModel)0x678B81B1;
                        break;
                    case "Messer":
                        wType = "Fist";
                        normalWName = "Messer";
                        wHash = (WeaponModel)0x99B507EA;
                        break;
                    case "Baseballschlaeger":
                        wType = "Fist";
                        normalWName = "Baseballschlaeger";
                        wHash = (WeaponModel)0x958A4A8F;
                        break;
                    case "Dolch":
                        wType = "Fist";
                        normalWName = "Dolch";
                        wHash = (WeaponModel)0x92A27487;
                        break;
                    case "Hammer":
                        wType = "Fist";
                        normalWName = "Hammer";
                        wHash = (WeaponModel)0x4E875F73;
                        break;
                    case "BattleAxe":
                        wType = "Fist";
                        normalWName = "BattleAxe";
                        wHash = (WeaponModel)0xF9DCBF2D;
                        break;
                    case "Machete":
                        wType = "Fist";
                        normalWName = "Machete";
                        wHash = (WeaponModel)0xDD5DF8D9;
                        break;
                    case "Schlagring":
                        wType = "Fist";
                        normalWName = "Schlagring";
                        wHash = (WeaponModel)0xD8DF3C3C;
                        break;
                    case "GolfClub":
                        wType = "Fist";
                        normalWName = "GolfClub";
                        wHash = (WeaponModel)0x440E4788;
                        break;
                    case "Tazer":
                        wType = "Secondary";
                        normalWName = "Tazer";
                        wHash = (WeaponModel)0x3656C8C1;
                        break;
                    case "Klappmesser":
                        wType = "Fist";
                        normalWName = "Klappmesser";
                        wHash = (WeaponModel)0xDFE37640;
                        break;
                    case "Feuerlöscher":
                        wType = "Secondary";
                        normalWName = "Feuerlöscher";
                        wHash = (WeaponModel)0x60EC506;
                        break;
                    case "Billard Kö":
                        wType = "Fist";
                        normalWName = "Billard Kö";
                        wHash = (WeaponModel)0x94117305;
                        break;
                }

                if (type == "Weapon")
                {
                    if (wType == "Primary")
                    {
                        string primWeapon = (string)Characters.GetCharacterWeapon(player, "PrimaryWeapon");
                        if (primWeapon == "None")
                        {
                            player.GiveWeapon(wHash, 0, true);
                            player.Emit("Client:Weapon:SetWeaponAmmo", (uint)wHash, 0);
                            Characters.SetCharacterWeapon(player, "PrimaryWeapon", wName);
                            Characters.SetCharacterWeapon(player, "PrimaryAmmo", 0);
                            SetWeaponComponents(player, wName);
                            HUDHandler.SendNotification(player, 2, 7000, $"{wName} erfolgreich ausgerüstet.");
                            return;
                        }
                        else if (primWeapon == wName)
                        {
                            int wAmmoAmount = (int)Characters.GetCharacterWeapon(player, "PrimaryAmmo");
                            float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                            float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                            float bigWeight = invWeight + backpackWeight;
                            float itemWeight = ServerItems.GetItemWeight($"{ammoWName} Magazin");
                            float multiWeight = itemWeight * wAmmoAmount;
                            float finalWeight = bigWeight + multiWeight;
                            float helpWeight = 15f + Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId));

                            if (invWeight + multiWeight > 15f && backpackWeight + multiWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
                            {
                                HUDHandler.SendNotification(player, 4, 7000, "Nicht genügend Platz.");
                                return;
                            }

                            if (wAmmoAmount >= 1 && ammoWName != "None" && finalWeight <= helpWeight) player.Emit("Client:Weapon:GetWeaponAmmo", (uint)wHash, ammoWName);

                            if (finalWeight <= helpWeight)
                            {
                                HUDHandler.SendNotification(player, 2, 7000, $"{wName} erfolgreich abgelegt.");
                                Characters.SetCharacterWeapon(player, "PrimaryWeapon", "None");
                                Characters.SetCharacterWeapon(player, "PrimaryAmmo", 0);
                                player.RemoveWeapon(wHash);
                            }
                        }
                        else
                        {
                            HUDHandler.SendNotification(player, 3, 7000, "Du musst zuerst deine Hauptwaffe ablegen bevor du eine neue anlegen kannst.");
                        }
                    }
                    else if (wType == "Fist")
                    {
                        string fistWeapon = (string)Characters.GetCharacterWeapon(player, "FistWeapon");
                        if (fistWeapon == "None")
                        {
                            player.GiveWeapon(wHash, 0, false);
                            player.Emit("Client:Weapon:SetWeaponAmmo", (uint)wHash, 0);
                            Characters.SetCharacterWeapon(player, "FistWeapon", wName);
                            Characters.SetCharacterWeapon(player, "FistWeaponAmmo", 0);
                            HUDHandler.SendNotification(player, 2, 2000, $"{wName} erfolgreich ausgerüstet.");
                        }
                        else if (fistWeapon == wName)
                        {
                            float curWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory") + CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                            float maxWeight = 15f + Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId));
                            if (curWeight < maxWeight)
                            {
                                Characters.SetCharacterWeapon(player, "FistWeapon", "None"); Characters.SetCharacterWeapon(player, "FistWeaponAmmo", 0); player.RemoveWeapon(wHash);
                                HUDHandler.SendNotification(player, 2, 7000, $"{wName} erfolgreich abgelegt.");
                            }
                            else { HUDHandler.SendNotification(player, 4, 7000, "Du hast nicht genügend Platz."); }
                        }
                        else
                        {
                            HUDHandler.SendNotification(player, 3, 5000, "Du musst zuerst deine Schlagwaffe ablegen bevor du eine neue anlegen kannst.");
                        }
                    }
                    else if (wType == "Secondary")
                    {
                        string secondaryWeapon = (string)Characters.GetCharacterWeapon(player, "SecondaryWeapon");
                        string secondaryWeapon2 = (string)Characters.GetCharacterWeapon(player, "SecondaryWeapon2");

                        if (secondaryWeapon == "None")
                        {
                            if (secondaryWeapon2 == wName)
                            {
                                int ammoAmount = (int)Characters.GetCharacterWeapon(player, "SecondaryAmmo2");
                                float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                                float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                                float bigWeight = invWeight + backpackWeight;
                                float itemWeight = ServerItems.GetItemWeight($"{ammoWName} Magazin");
                                float multiWeight = itemWeight * ammoAmount;
                                float finalWeight = bigWeight + multiWeight;
                                float helpWeight = 15f + Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId));

                                if (invWeight + multiWeight > 15f && backpackWeight + multiWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
                                {
                                    HUDHandler.SendNotification(player, 4, 7000, "Nicht genügend Platz.");
                                    return;
                                }

                                if (ammoAmount >= 1 && ammoWName != "None" && finalWeight <= helpWeight) player.Emit("Client:Weapon:GetWeaponAmmo", (uint)wHash, ammoWName);

                                if (finalWeight <= helpWeight)
                                {
                                    HUDHandler.SendNotification(player, 2, 7000, $"{wName} erfolgreich abgelegt.");
                                    Characters.SetCharacterWeapon(player, "SecondaryWeapon2", "None");
                                    Characters.SetCharacterWeapon(player, "SecondaryAmmo2", "None");
                                    player.RemoveWeapon(wHash);
                                }
                            }
                            else
                            {
                                player.GiveWeapon(wHash, 0, true);
                                player.Emit("Client:Weapon:SetWeaponAmmo", (uint)wHash, 0);
                                Characters.SetCharacterWeapon(player, "SecondaryWeapon", wName);
                                Characters.SetCharacterWeapon(player, "SecondaryAmmo", 0);
                                SetWeaponComponents(player, wName);
                                HUDHandler.SendNotification(player, 2, 7000, $"{wName} erfolgreich ausgerüstet.");
                            }
                        }
                        else if (secondaryWeapon == wName)
                        {
                            int ammoAmount = (int)Characters.GetCharacterWeapon(player, "SecondaryAmmo");
                            float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                            float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                            float bigWeight = invWeight + backpackWeight;
                            float itemWeight = ServerItems.GetItemWeight($"{ammoWName} Magazin");
                            float multiWeight = itemWeight * ammoAmount;
                            float finalWeight = bigWeight + multiWeight;
                            float helpWeight = 15f + Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId));

                            if (invWeight + multiWeight > 15f && backpackWeight + multiWeight > Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
                            {
                                HUDHandler.SendNotification(player, 4, 7000, "Nicht genügend Platz.");
                                return;
                            }

                            if (ammoAmount >= 1 && ammoWName != "None" && finalWeight <= helpWeight) player.Emit("Client:Weapon:GetWeaponAmmo", (uint)wHash, ammoWName);

                            if (finalWeight <= helpWeight)
                            {
                                HUDHandler.SendNotification(player, 2, 7000, $"{wName} erfolgreich abgelegt.");
                                Characters.SetCharacterWeapon(player, "SecondaryWeapon", "None");
                                Characters.SetCharacterWeapon(player, "SecondaryAmmo", 0);
                                player.RemoveWeapon(wHash);
                            }
                        }
                        else
                        {
                            if (secondaryWeapon2 == "None")
                            {
                                player.GiveWeapon(wHash, 0, true);
                                player.Emit("Client:Weapon:SetWeaponAmmo", (uint)wHash, 0);
                                Characters.SetCharacterWeapon(player, "SecondaryWeapon2", wName);
                                Characters.SetCharacterWeapon(player, "SecondaryAmmo2", 0);
                                SetWeaponComponents(player, wName);
                                HUDHandler.SendNotification(player, 2, 7000, $"{wName} erfolgreich ausgerüstet.");
                            }
                            else if (secondaryWeapon2 == wName)
                            {
                                int ammoAmount = (int)Characters.GetCharacterWeapon(player, "SecondaryAmmo2");
                                float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                                float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                                float bigWeight = invWeight + backpackWeight;
                                float itemWeight = ServerItems.GetItemWeight($"{ammoWName} Magazin");
                                float multiWeight = itemWeight * ammoAmount;
                                float finalWeight = bigWeight + multiWeight;
                                float helpWeight = 15f + Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId));

                                if (ammoAmount >= 1 && ammoWName != "None" && finalWeight <= helpWeight) player.Emit("Client:Weapon:GetWeaponAmmo", (uint)wHash, ammoWName);

                                if (finalWeight <= helpWeight)
                                {
                                    HUDHandler.SendNotification(player, 2, 7000, $"{wName} erfolgreich abgelegt.");
                                    Characters.SetCharacterWeapon(player, "SecondaryWeapon2", "None");
                                    Characters.SetCharacterWeapon(player, "SecondaryAmmo2", 0);
                                    player.RemoveWeapon(wHash);
                                }
                            }
                            else
                            {
                                HUDHandler.SendNotification(player, 3, 7000, "Du musst zuerst deine Sekundärwaffe ablegen bevor du eine neue anlegen kannst.");
                            }
                        }
                    }
                }
                else if (type == "Ammo")
                {
                    if (wType == "Primary")
                    {
                        string primaryWeapon = (string)Characters.GetCharacterWeapon(player, "PrimaryWeapon");
                        if (primaryWeapon == "None")
                        {
                            HUDHandler.SendNotification(player, 3, 7000, "Du hast keine Primärwaffe angelegt.");
                        }
                        else if (primaryWeapon == normalWName)
                        {
                            int newAmmo = (int)Characters.GetCharacterWeapon(player, "PrimaryAmmo") + 30;
                            player.Emit("Client:Weapon:SetWeaponAmmo", (uint)wHash, newAmmo);
                            Characters.SetCharacterWeapon(player, "PrimaryAmmo", newAmmo);
                            HUDHandler.SendNotification(player, 2, 7000, $"Du hast {wName} in deine Waffe geladen.");

                            if (CharactersInventory.ExistCharacterItem(charId, $"{wName}", fromContainer))
                            {
                                CharactersInventory.RemoveCharacterItemAmount(charId, $"{wName}", 1, fromContainer);
                            }
                        }
                        else
                        {
                            HUDHandler.SendNotification(player, 3, 7000, "Die Munitionen passen nicht in deine Waffe.");
                        }
                    }
                    else if (wType == "Secondary")
                    {
                        string secondaryWeapon = (string)Characters.GetCharacterWeapon(player, "SecondaryWeapon");
                        if (secondaryWeapon == "None")
                        {
                            HUDHandler.SendNotification(player, 4, 7000, "Du hast keine Sekundärwaffe angelegt.");
                        }
                        else if (secondaryWeapon == normalWName)
                        {
                            int newAmmo = (int)Characters.GetCharacterWeapon(player, "SecondaryAmmo") + 30;
                            player.Emit("Client:Weapon:SetWeaponAmmo", (uint)wHash, newAmmo);
                            Characters.SetCharacterWeapon(player, "SecondaryAmmo", newAmmo);
                            HUDHandler.SendNotification(player, 2, 7000, $"Du hast {wName} in deine Waffe geladen.");

                            if (CharactersInventory.ExistCharacterItem(charId, $"{wName}", fromContainer))
                            {
                                CharactersInventory.RemoveCharacterItemAmount(charId, $"{wName}", 1, fromContainer);
                            }
                        }
                        else
                        {
                            string secondary2Weapon = (string)Characters.GetCharacterWeapon(player, "SecondaryWeapon2");
                            if (secondary2Weapon == "None")
                            {
                                HUDHandler.SendNotification(player, 4, 7000, "Du hast keine Sekundärwaffe angelegt.");
                            }
                            else if (secondary2Weapon == normalWName)
                            {
                                int newAmmo = (int)Characters.GetCharacterWeapon(player, "SecondaryAmmo2") + 30;
                                player.Emit("Client:Weapon:SetWeaponAmmo", (uint)wHash, newAmmo);
                                Characters.SetCharacterWeapon(player, "SecondaryAmmo2", newAmmo);
                                HUDHandler.SendNotification(player, 2, 7000, $"Du hast {wName} in deine Waffe geladen.");

                                if (CharactersInventory.ExistCharacterItem(charId, $"{wName}", fromContainer))
                                {
                                    CharactersInventory.RemoveCharacterItemAmount(charId, $"{wName}", 1, fromContainer);
                                }
                            }
                            else
                            {
                                HUDHandler.SendNotification(player, 4, 7000, "Die Munitionen passen nicht in deine Waffe.");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static void SetWeaponComponents(IPlayer player, string wName)
        {
            if (player == null || !player.Exists) return;
            switch (wName)
            {
                case "PDW":
                    player.AddWeaponComponent(WeaponModel.CombatPDW, 0x7BC4CDDC); //Flashlight
                    player.AddWeaponComponent(WeaponModel.CombatPDW, 0xAA2C45B4); //Scope
                    player.AddWeaponComponent(WeaponModel.CombatPDW, 0xC164F53); //Grip
                    player.AddWeaponComponent(WeaponModel.CombatPDW, 0x4317F19E); //Magazin
                    break;
                case "MkII Pistole":
                    player.AddWeaponComponent(WeaponModel.PistolMkII, 0x43FD595B); //Flashlight
                    player.AddWeaponComponent(WeaponModel.PistolMkII, 0x21E34793); //Mündungsbremse
                    break;
            }
        }

        public static WeaponModel GetWeaponModelByName(string wName)
        {
            WeaponModel wHash = 0;
            switch (wName)
            {
                case "Gefechtspistole": wHash = WeaponModel.CombatPistol; break;
                case "SMG": wHash = WeaponModel.SMG; break;
                case "PDW": wHash = WeaponModel.CombatPDW; break;
                case "AssaultSMG": wHash = WeaponModel.AssaultSMG; break;
                case "Micro SMG": wHash = WeaponModel.MicroSMG; break;
                case "CombatMGMk2": wHash = WeaponModel.CombatMGMkII; break;
                case "Gusenberg": wHash = WeaponModel.GusenbergSweeper; break;
                case "Heavy Pistol": wHash = WeaponModel.HeavyPistol; break;
                case "Pistol50": wHash = WeaponModel.Pistol50; break;
                case "APPistole": wHash = WeaponModel.APPistol; break;
                case "Pistole": wHash = WeaponModel.Pistol; break;
                case "SpecialCarbine": wHash = WeaponModel.SpecialCarbine; break;
                case "Assault Rifle": wHash = WeaponModel.AssaultRifle; break;
                case "CarabineRifle": wHash = WeaponModel.CarbineRifle; break;
                case "Bullpup Gewehr": wHash = WeaponModel.BullpupRifle; break;
                case "SawnoffShotgun": wHash = WeaponModel.SawedOffShotgun; break;
                case "Compact Rifle": wHash = WeaponModel.CompactRifle; break;
                case "HeavyShotgun": wHash = WeaponModel.HeavyShotgun; break;
                case "PumpShotgun": wHash = WeaponModel.PumpShotgun; break;
                case "Sniper": wHash = WeaponModel.SniperRifle; break;
                case "Schlagstock": wHash = WeaponModel.Nightstick; break;
                case "Messer": wHash = WeaponModel.Knife; break;
                case "Baseballschlaeger": wHash = WeaponModel.BaseballBat; break;
                case "Dolch": wHash = WeaponModel.AntiqueCavalryDagger; break;
                case "Hammer": wHash = WeaponModel.Hammer; break;
                case "BattleAxe": wHash = WeaponModel.BattleAxe; break;
                case "Machete": wHash = WeaponModel.Machete; break;
                case "Schlagring": wHash = WeaponModel.BrassKnuckles; break;
                case "GolfClub": wHash = WeaponModel.GolfClub; break;
                case "Tazer": wHash = WeaponModel.StunGun; break;
                case "Klappmesser": wHash = WeaponModel.Switchblade; break;
                case "Feuerlöscher": wHash = WeaponModel.FireExtinguisher; break;       // Icon Change
                case "Billard Kö": wHash = WeaponModel.PoolCue; break;

            }
            return wHash;
        }

        [AsyncClientEvent("Server:Weapon:SendWeaponAmmo")]
        public static void SetWeaponAmmo(IPlayer player, string name, int ammo)
        {
            try
            {
                int charId = User.GetPlayerOnline(player);

                float invWeight = CharactersInventory.GetCharacterItemWeight(charId, "inventory");
                float backpackWeight = CharactersInventory.GetCharacterItemWeight(charId, "backpack");
                float itemWeight = ServerItems.GetItemWeight($"{name} Magazin");
                float multiWeight = itemWeight * ammo;

                int magazin = ammo / 30;
                int kugeln = ammo - magazin * 30;
                if (invWeight + multiWeight <= 15f)
                {
                    if (kugeln <= 0)
                    {
                        CharactersInventory.AddCharacterItem(charId, $"{name} Magazin", magazin, "inventory");
                    }
                    else
                    {
                        CharactersInventory.AddCharacterItem(charId, $"{name} Kugeln", kugeln, "inventory");
                        CharactersInventory.AddCharacterItem(charId, $"{name} Magazin", magazin, "inventory");
                    }
                }
                else if (backpackWeight + multiWeight <= Characters.GetCharacterBackpackSize(Characters.GetCharacterBackpack(charId)))
                {
                    if (kugeln <= 0)
                    {
                        CharactersInventory.AddCharacterItem(charId, $"{name} Magazin", magazin, "backpack");
                    }
                    else
                    {
                        CharactersInventory.AddCharacterItem(charId, $"{name} Kugeln", kugeln, "backpack");
                        CharactersInventory.AddCharacterItem(charId, $"{name} Magazin", magazin, "backpack");
                    }
                }
                InventoryHandler.RequestInventoryItems(player);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Weapon:UpdateAmmo")]
        public static void UpdateWeaponAmmo(IPlayer player, uint hash, int ammo)
        {
            try
            {
                string wType = "None";
                string normalWName = "None";

                switch (hash)
                {
                    case 1593441988:
                        wType = "Secondary";
                        normalWName = "Gefechtspistole";
                        break;
                    case 736523883:
                        wType = "Secondary";
                        normalWName = "SMG";
                        break;
                    case 171789620:
                        wType = "Secondary";
                        normalWName = "PDW";
                        break;
                    case 4024951519:
                        wType = "Secondary";
                        normalWName = "AssaultSMG";
                        break;
                    case 324215364:
                        wType = "Secondary";
                        normalWName = "Micro SMG";
                        break;
                    case 3686625920:
                        wType = "Secondary";
                        normalWName = "CombatMGMk2";
                        break;
                    case 3523564046:
                        wType = "Secondary";
                        normalWName = "Heavy Pistol";
                        break;
                    case 2578377531:
                        wType = "Secondary";
                        normalWName = "Pistol50";
                        break;
                    case 584646201:
                        wType = "Secondary";
                        normalWName = "APPistole";
                        break;
                    case 453432689:
                        wType = "Secondary";
                        normalWName = "Pistole";
                        break;
                    case 3231910285:
                        wType = "Primary";
                        normalWName = "SpecialCarbine";
                        break;
                    case 3220176749:
                        wType = "Primary";
                        normalWName = "Assault Rifle";
                        break;
                    case 2210333304:
                        wType = "Primary";
                        normalWName = "CarabineRifle";
                        break;
                    case 2132975508:
                        wType = "Primary";
                        normalWName = "Bullpup Gewehr";
                        break;
                    case 2017895192:
                        wType = "Primary";
                        normalWName = "SawnoffShotgun";
                        break;
                    case 1649403952:
                        wType = "Primary";
                        normalWName = "Compact Rifle";
                        break;
                    case 984333226:
                        wType = "Primary";
                        normalWName = "HeavyShotgun";
                        break;
                    case 487013001:
                        wType = "Primary";
                        normalWName = "PumpShotgun";
                        break;
                    case 100416529:
                        wType = "Primary";
                        normalWName = "Sniper";
                        break;
                    case 911657153:
                        wType = "Secondary";
                        normalWName = "Tazer";
                        break;
                }

                if (wType == "Primary") Characters.SetCharacterWeapon(player, "PrimaryAmmo", ammo);
                else if (wType == "Fist") Characters.SetCharacterWeapon(player, "FistWeaponAmmo", ammo);
                else if (normalWName == (string)Characters.GetCharacterWeapon(player, "SecondaryWeapon")) Characters.SetCharacterWeapon(player, "SecondaryAmmo", ammo);
                else if (wType != "None") Characters.SetCharacterWeapon(player, "SecondaryAmmo2", ammo);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
    }
}
