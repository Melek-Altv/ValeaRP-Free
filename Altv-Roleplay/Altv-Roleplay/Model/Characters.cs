using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Handler;
using Altv_Roleplay.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Altv_Roleplay.Model
{
    class Characters : IScript
    {
        public static List<AccountsCharacters> PlayerCharacters = new List<AccountsCharacters>();
        public static List<Characters_Skin> CharactersSkin = new List<Characters_Skin>();
        public static List<Characters_LastPos> CharactersLastPos = new List<Characters_LastPos>();
        public static List<Characters_Permissions> CharactersPermissions = new List<Characters_Permissions>();

        public static void CreatePlayerCharacter(IPlayer client, string charname, string birthdate, bool gender, string facefeaturesarray, string headblendsdataarray, string headoverlaysarray1, string headoverlaysarray2, string headoverlaysarray3)
        {
            try
            {
                if (client == null || !client.Exists) return;
                var CharData = new AccountsCharacters
                {
                    accountId = User.GetPlayerAccountId(client),
                    charname = charname,
                    death = false,
                    accState = 0,
                    whitelisted = false,
                    firstJoin = true,
                    firstSpawnPlace = "unset",
                    firstJoinTimestamp = DateTime.Now,
                    gender = gender,
                    birthdate = birthdate,
                    birthplace = "None",
                    health = 100,
                    armor = 0,
                    hunger = 100,
                    thirst = 100,
                    address = "Kein Wohnsitz",
                    phonenumber = 0,
                    isCrime = false,
                    paydayTime = 0,
                    taxTime = 0,
                    job = "None",
                    jobHourCounter = 0,
                    lastJobPaycheck = DateTime.Now,
                    weapon_Primary = "None",
                    weapon_Primary_Ammo = 0,
                    weapon_Secondary = "None",
                    weapon_Secondary_Ammo = 0,
                    weapon_Secondary2 = "None",
                    weapon_Secondary2_Ammo = 0,
                    weapon_Fist = "None",
                    weapon_Fist_Ammo = 0,
                    isUnconscious = false,
                    unconsciousTime = 0,
                    isFastFarm = false,
                    fastFarmTime = 0,
                    lastLogin = DateTime.Now,
                    isPhoneEquipped = false,
                    playtimeHours = 0,
                    isInJail = false,
                    jailTime = 0
                };
                PlayerCharacters.Add(CharData);

                using (gtaContext db = new gtaContext())
                {
                    db.AccountsCharacters.Add(CharData);
                    db.SaveChanges();
                }

                GenerateCharacterPhonenumber(client, CharData.charId);

                CreateCharacterSkin(charname, facefeaturesarray, headblendsdataarray, headoverlaysarray1, headoverlaysarray2, headoverlaysarray3);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static void CreateCharacterSkin(string charname, string facefeaturesarray, string headblendsdataarray, string headoverlaysarray1, string headoverlaysarray2, string headoverlaysarray3)
        {
            int charId = GetCharacterIdFromCharName(charname);
            var CharSkinData = new Characters_Skin
            {
                charId = charId,
                facefeatures = facefeaturesarray,
                headblendsdata = headblendsdataarray,
                headoverlays1 = headoverlaysarray1,
                headoverlays2 = headoverlaysarray2,
                headoverlays3 = headoverlaysarray3,
                clothesTop = "None",
                clothesTorso = "None",
                clothesLeg = "None",
                clothesFeet = "None",
                clothesHat = "None",
                clothesGlass = "None",
                clothesEarring = "None",
                clothesNecklace = "None",
                clothesMask = "None",
                clothesArmor = "None",
                clothesUndershirt = "None",
                clothesBracelet = "None",
                clothesWatch = "None",
                clothesBag = "None",
                clothesDecal = "None"
            };

            CharactersLicenses.CreateCharacterLicensesEntry(charId, false, false, false, false, false, false, false, false);
            CharactersTablet.CreateCharacterTabletAppEntry(charId, false, false, false, false, false, false, false, false);
            CharactersTablet.CreateCharacterTabletTutorialAppEntry(charId);

            try
            {
                CharactersSkin.Add(CharSkinData);
                using (gtaContext db = new gtaContext())
                {
                    db.Characters_Skin.Add(CharSkinData);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static void SetCharacterCurrentFunkFrequence(int charId, string frequence)
        {
            try
            {
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars == null) return;
                chars.currentFunkFrequence = frequence;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static bool IsCharacterInJail(int charId)
        {
            try
            {
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.isInJail;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return false;
        }

        public static int GetCharacterJailTime(int charId)
        {
            try
            {
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.jailTime;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return 0;
        }

        public static void SetCharacterJailTime(int charId, bool isInJail, int jt)
        {
            try
            {
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars == null) return;
                if (jt < 0) jt = 0;
                chars.isInJail = isInJail;
                chars.jailTime = jt;
                using (var db = new gtaContext())
                {
                    db.AccountsCharacters.Update(chars);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static string GetCharacterCurrentFunkFrequence(int charId)
        {
            try
            {
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.currentFunkFrequence;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return null;
        }

        public static int GetCharacterAccountId(int charId)
        {
            try
            {
                if (charId <= 0) return 0;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.accountId;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return 0;
        }

        public static int GetCharacterCurrentlyRecieveCaller(int charId)
        {
            try
            {
                var chars = PlayerCharacters.ToList().FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.CurrentlyRecieveCaller;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return 0;
        }

        public static void SetCharacterCurrentlyRecieveCallState(int charId, int CurrentlyRecieveCaller)
        {
            try
            {
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) chars.CurrentlyRecieveCaller = CurrentlyRecieveCaller;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static int GetCharacterPhoneTargetNumber(int charId)
        {
            try
            {
                var chars = PlayerCharacters.ToList().FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.targetNumber;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return 0;
        }

        public static void SetCharacterTargetPhoneNumber(int charId, int targetNumber)
        {
            try
            {
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if(chars != null) chars.targetNumber = targetNumber;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static bool IsCharacterPhoneFlyModeEnabled(int charId)
        {
            try
            {
                var chars = PlayerCharacters.ToList().FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.isPhoneFlyModeActivated;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return false;
        }

        public static void SetCharacterPhoneFlyModeEnabled(int charId, bool isEnabled)
        {
            try
            {
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) chars.isPhoneFlyModeActivated = isEnabled;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static bool IsCharacterPhoneEquipped(int charId)
        {
            try
            {
                var chars = PlayerCharacters.ToList().FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.isPhoneEquipped;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return false;
        }

        public static void SetCharacterPhoneEquipped(int charId, bool isPhoneEquipped)
        {
            try
            {
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars == null) return;
                chars.isPhoneEquipped = isPhoneEquipped;
                using (var db = new gtaContext())
                {
                    db.AccountsCharacters.Update(chars);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static void SetCharacterHeadOverlays(int charid, string headoverlayarray1, string headoverlayarray2, string headoverlayarray3)
        {
            if (charid == 0 || headoverlayarray1.Length == 0 || headoverlayarray2.Length == 0 || headoverlayarray3.Length == 0) return;
            var chars = CharactersSkin.FirstOrDefault(x => x.charId == charid);
            if (chars != null)
            {
                chars.headoverlays1 = headoverlayarray1;
                chars.headoverlays2 = headoverlayarray2;
                chars.headoverlays3 = headoverlayarray3;
                using (gtaContext db = new gtaContext())
                {
                    db.Characters_Skin.Update(chars);
                    db.SaveChanges();
                }
            }
        }

        public static void SetCharacterFaceFeatures(int charid, string facefeatures)
        {
            if (charid == 0 || facefeatures == "") return;
            var chars = CharactersSkin.FirstOrDefault(x => x.charId == charid);
            if (chars != null)
            {
                chars.facefeatures = facefeatures;
                using (gtaContext db = new gtaContext())
                {
                    db.Characters_Skin.Update(chars);
                    db.SaveChanges();
                }
            }
        }

        public static bool ExistPhoneNumber(int phoneNumber)
        {
            try
            {
                var charData = PlayerCharacters.ToList().FirstOrDefault(x => x.phonenumber == phoneNumber);
                if (charData != null) return true;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return false;
        }

        public static int GetCharacterPhonenumber(int charId)
        {
            try
            {
                var chars = PlayerCharacters.ToList().FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.phonenumber;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return 0;
        }

        public static void GenerateCharacterPhonenumber(IPlayer player, int charId)
        {
            try
            {
                if (player == null || !player.Exists) return;
                int generatedNumber = new Random().Next(100000, 999999);
                if(ExistPhoneNumber(generatedNumber)) { 
                    GenerateCharacterPhonenumber(player, charId); 
                    return; 
                }

                var charData = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (charData == null) return;
                charData.phonenumber = generatedNumber;
                using (var db = new gtaContext())
                {
                    db.AccountsCharacters.Update(charData);
                    db.SaveChanges();
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static float[] GetCharacterHeadOverlay1(int charid)
        {
            if (charid == 0) return null;
            var chars = CharactersSkin.FirstOrDefault(x => x.charId == charid);
            if (chars != null)
            {
                return Array.ConvertAll(chars.headoverlays1.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));
            }

            return null;
        }
        public static float[] GetCharacterHeadOverlay2(int charid)
        {
            if (charid == 0) return null;
            var chars = CharactersSkin.FirstOrDefault(x => x.charId == charid);
            if (chars != null)
            {
                return Array.ConvertAll(chars.headoverlays2.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));
            }

            return null;
        }
        public static float[] GetCharacterHeadOverlay3(int charid)
        {
            if (charid == 0) return null;
            var chars = CharactersSkin.FirstOrDefault(x => x.charId == charid);
            if (chars != null)
            {
                return Array.ConvertAll(chars.headoverlays3.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));
            }

            return null;
        }

        public static string[] GetCharacterFaceFeatures(int charid)
        {
            if (charid == 0) return null;
            var chars = CharactersSkin.FirstOrDefault(x => x.charId == charid);
            if (chars != null)
            {
                return chars.facefeatures.Split(';');
            }
            return null;
        }

        public static string GetCharacterHeadBlends(int charid)
        {
            if (charid == 0) return "";
            var chars = CharactersSkin.FirstOrDefault(x => x.charId == charid);
            if (chars != null)
            {
                return chars.headblendsdata;
            }

            return "";
        }

        public static void CreateCharacterLastPos(int charid, Position pos, short dimension)
        {
            var CharLastPosData = new Characters_LastPos
            {
                charId = charid,
                lastPosX = pos.X,
                lastPosY = pos.Y,
                lastPosZ = pos.Z,
                lastDimension = (int)dimension
            };

            try
            {
                CharactersLastPos.Add(CharLastPosData);

                using (gtaContext db = new gtaContext())
                {
                    db.Characters_LastPos.Add(CharLastPosData);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Charselector:KillCharacter")]
        public static void KillCharacter(ClassicPlayer client, int charId)
        {
            try
            {
                if (client == null || !client.Exists) return;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);

                if (chars != null)
                {
                    chars.death = true;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }

                    if (client.Exists && client != null) LoginHandler.SendDataToCharselectorArea(client);
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static int GetCharacterPaydayTime(int charId)
        {
            if (charId == 0) return 0;
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            if(chars != null)
            {
                return chars.paydayTime;
            }
            return 0;
        }

        public static int GetCharacterTaxTime(int charId)
        {
            if (charId == 0) return 0;
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            if (chars != null)
            {
                return chars.taxTime;
            }
            return 0;
        }

        public static void IncreaseCharacterPaydayAndTaxTime(int charId)
        {
            try
            {
                if (charId == 0) return;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if (chars != null)
                {
                    chars.paydayTime += 1;
                    chars.taxTime += 1;

                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e) { Alt.Log($"{e}"); }
        }

        public static void ResetCharacterTaxTime(int charId)
        {
            try
            {
                if (charId <= 0) return;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if (chars != null)
                {
                    chars.taxTime = 0;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e) { Alt.Log($"{e}"); }
        }

        public static bool GetCharacterisDead(int charId)
        {
            if (charId == 0) return false;
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            if (chars != null)
            {
                return chars.death;
            }
            return false;
        }

        public static void ResetCharacterJobHourCounter(int charId)
        {
            try
            {
                if (charId == 0) return;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if(chars != null)
                {
                    chars.jobHourCounter = 0; 
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            } catch(Exception e) { Alt.Log($"{e}"); }
        }

        public static void IncreaseCharacterPlayTimeHours(int charId)
        {
            try
            {
                if (charId <= 0) return;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if(chars != null)
                {
                    chars.playtimeHours += 1;
                    using (var db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static void IncreaseCharacterJobHourCounter(int charId)
        {
            try
            {
                if (charId == 0) return;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if(chars != null)
                {
                    chars.jobHourCounter += 1;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }catch(Exception e) { Alt.Log($"{e}"); }
        }

        public static void ResetCharacterPaydayTime(int charId)
        {
            try
            {
                if (charId <= 0) return;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if(chars != null)
                {
                    chars.paydayTime = 0;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch(Exception e) { Alt.Log($"{e}"); }
        }

        public static string GetCharacterName(int charId)
        {
            try
            {
                if (charId <= 0) return "Unbekannt";
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if (chars != null)
                {
                    return chars.charname;
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
            return "Unbekannt";
        }

        public static string GetCharacterBirthdate(int charId)
        {
            try
            {
                if (charId <= 0) return "";
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if (chars != null)
                {
                    return chars.birthdate;
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
            return "";
        }

        public static void SetCharacterPhoneWallpaper(int charId, int WallpaperId)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);

            if (chars != null)
            {
                chars.wallpaper = WallpaperId;
                try
                {
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Alt.Log($"{e}");
                }
            }
        }

        public static int GetCharacterPhoneWallpaper(int charId)
        {
            try
            {
                if (charId <= 0) return 1;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.wallpaper;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return 1;
        }


        public static string GetCharacterBirthplace(int charId)
        {
            try
            {
                if (charId <= 0) return "None";
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if (chars != null)
                {
                    return chars.birthplace;
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
            return "None";
        }

        public static int GetCharacterAccState(int charId)
        {
            try
            {
                if (charId <= 0) return 0;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if (chars != null)
                {
                    return chars.accState;
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
            return 0;
        }

        public static string GetCharacterStreet(int charId)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            if(chars != null)
            {
                return chars.address;
            }
            return "Boulevard Del Perro 2a";
        }

        public static void SetCharacterStreet(int charId, string st)
        {
            try
            {
                if (charId <= 0 || st == "") return;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if(chars != null)
                {
                    chars.address = st;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static object GetCharacterWeapon(IPlayer player, string type)
        {
            object obj = null;
            if (player == null || !player.Exists) return obj;
            int charId = User.GetPlayerOnline(player);
            if (charId == 0) return obj;
            var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
            if(chars != null)
            {
                switch(type)
                {
                    case "PrimaryWeapon": obj = chars.weapon_Primary; break;
                    case "PrimaryAmmo": obj = chars.weapon_Primary_Ammo; break;
                    case "SecondaryWeapon": obj = chars.weapon_Secondary; break;
                    case "SecondaryAmmo": obj = chars.weapon_Secondary_Ammo; break;
                    case "SecondaryWeapon2": obj = chars.weapon_Secondary2; break;
                    case "SecondaryAmmo2": obj = chars.weapon_Secondary2_Ammo; break;
                    case "FistWeapon": obj = chars.weapon_Fist; break;
                }
            }
            return obj;
        }

        public static void SetCharacterWeapon(IPlayer player, string type, object weaponValue)
        {
            try
            {
                if (player == null || !player.Exists || type == "" || weaponValue == null) return;
                int charId = User.GetPlayerOnline(player);
                if (charId == 0) return;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null)
                {
                    switch(type)
                    {
                        case "PrimaryWeapon": chars.weapon_Primary = (string)weaponValue; break;
                        case "PrimaryAmmo": chars.weapon_Primary_Ammo = (int)weaponValue; break;
                        case "SecondaryWeapon": chars.weapon_Secondary = (string)weaponValue; break;
                        case "SecondaryAmmo": chars.weapon_Secondary_Ammo = (int)weaponValue; break;
                        case "SecondaryWeapon2": chars.weapon_Secondary2 = (string)weaponValue; break;
                        case "SecondaryAmmo2": chars.weapon_Secondary2_Ammo = (int)weaponValue; break;
                        case "FistWeapon": chars.weapon_Fist = (string)weaponValue; break;
                        case "FistWeaponAmmo": chars.weapon_Fist_Ammo = (int)weaponValue; break;
                    }

                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static bool ExistCharacterName(string charName)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charname == charName);

            if (chars != null)
            {
                return true;
            }

            return false;
        }

        public static bool GetCharacterGender(int charid)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charid);

            if (chars != null)
            {
                return chars.gender;
            }

            return false;
        }

        public static bool GetCharacterFirstJoin(int charId)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);

            if (chars != null)
            {
                return chars.firstJoin;
            }
            return false;
        }

        public static int GetCharacterIdFromCharName(string charname)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charname == charname);

            if (chars != null)
            {
                return chars.charId;
            }

            return 0;
        }

        public static float[] GetCharacterSkin(string type, int charid)
        {
            var chars = CharactersSkin.FirstOrDefault(p => p.charId == charid);

            if (chars != null)
            {
                switch (type)
                {
                    case "facefeatures":
                        return Array.ConvertAll(chars.facefeatures.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));;
                    case "headblendsdata":
                        return Array.ConvertAll(chars.headblendsdata.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));;
                    case "headoverlays1":
                        return Array.ConvertAll(chars.headoverlays1.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));;
                    case "headoverlays2":
                        return Array.ConvertAll(chars.headoverlays2.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));;
                    case "headoverlays3":
                        return Array.ConvertAll(chars.headoverlays3.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));;
                }
            }
            return null;
        }

        public static void SetCharacterSkin(IPlayer player)
        {
            try
            {
                int charId = User.GetPlayerOnline(player);
                if (charId <= 0) return;
                var chars = CharactersSkin.FirstOrDefault(p => p.charId == charId);
                if (chars != null)
                {
                    float[] facefeatures = Array.ConvertAll(chars.facefeatures.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));
                    float[] headblendsdata = Array.ConvertAll(chars.headblendsdata.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));
                    float[] headoverlays1 = Array.ConvertAll(chars.headoverlays1.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));
                    float[] headoverlays2 = Array.ConvertAll(chars.headoverlays2.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));
                    float[] headoverlays3 = Array.ConvertAll(chars.headoverlays3.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));


                    player.SetHeadBlendData(Convert.ToUInt32(headblendsdata[0]), Convert.ToUInt32(headblendsdata[1]), 0, Convert.ToUInt32(headblendsdata[2]), Convert.ToUInt32(headblendsdata[5]), 0, (float)headblendsdata[3], (float)headblendsdata[4], 0);
                    player.SetHeadOverlayColor(1, 1, (byte)headoverlays3[1], 1);
                    player.SetHeadOverlayColor(2, 1, (byte)headoverlays3[2], 1);
                    player.SetHeadOverlayColor(5, 2, (byte)headoverlays3[5], 1);
                    player.SetHeadOverlayColor(8, 2, (byte)headoverlays3[8], 1);
                    player.SetHeadOverlayColor(10, 1, (byte)headoverlays3[10], 1);
                    player.SetEyeColor((ushort)headoverlays1[14]);
                    player.SetHeadOverlay(0, (byte)headoverlays1[0], (float)headoverlays2[0]);
                    player.SetHeadOverlay(1, (byte)headoverlays1[1], (float)headoverlays2[1]);
                    player.SetHeadOverlay(2, (byte)headoverlays1[2], (float)headoverlays2[2]);
                    player.SetHeadOverlay(3, (byte)headoverlays1[3], (float)headoverlays2[3]);
                    player.SetHeadOverlay(4, (byte)headoverlays1[4], (float)headoverlays2[4]);
                    player.SetHeadOverlay(5, (byte)headoverlays1[5], (float)headoverlays2[5]);
                    player.SetHeadOverlay(6, (byte)headoverlays1[6], (float)headoverlays2[6]);
                    player.SetHeadOverlay(7, (byte)headoverlays1[7], (float)headoverlays2[7]);
                    player.SetHeadOverlay(8, (byte)headoverlays1[8], (float)headoverlays2[8]);
                    player.SetHeadOverlay(9, (byte)headoverlays1[9], (float)headoverlays2[9]);
                    player.SetHeadOverlay(10, (byte)headoverlays1[10], (float)headoverlays2[10]);

                    player.HairColor = (byte)headoverlays3[13];
                    player.HairHighlightColor = (byte)headoverlays2[13];

                    player.SetClothes(2, (ushort)headoverlays1[13], 0, 2);

                    for (int i = 0; i < 20; i++)
                    {
                        player.SetFaceFeature((byte)i, facefeatures[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static string GetPlayerCharacters(IPlayer player)
        {
            if (player == null || !player.Exists) return "";

            var items = PlayerCharacters.Where(x => x.accountId == User.GetPlayerAccountId(player)).Select(x => new
            {
                accountId = x.accountId,
                charId = x.charId,
                charname = x.charname,
                death = x.death,
                firstjoin = x.firstJoin,
                gender = x.gender,
            }).ToList();

            return JsonConvert.SerializeObject(items);
        }

        public static string GetCharacterInformations(int charId)
        {
            try
            {
                if (charId <= 0) return "[]";
                var items = PlayerCharacters.Where(x => x.charId == charId).Select(x => new
                {
                    x.charId,
                    x.charname,
                    x.birthdate,
                    x.birthplace,
                    address = $"{x.address}",
                    firstJoin = x.firstJoinTimestamp.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")),
                }).ToList();
                return JsonConvert.SerializeObject(items);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
                return "[]";
            }
        }

        public static string GetCharacterFactionInformations(int charId)
        {
            try
            {
                if (charId <= 0) return "[]";
                var items = PlayerCharacters.Where(x => x.charId == charId).Select(x => new
                {
                    x.charId,
                    x.charname,
                    x.birthdate,
                    factionId = ServerFactions.GetCharacterFactionId(charId),
                    factionShort = ServerFactions.GetFactionShortName(ServerFactions.GetCharacterFactionId(charId)),
                    servicenumber = ServerFactions.GetCharacterFactionServiceNumber(charId),
                    rankname = ServerFactions.GetFactionRankName(ServerFactions.GetCharacterFactionId(charId), ServerFactions.GetCharacterFactionRank(charId)),
                    firstJoin = x.firstJoinTimestamp.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")),
                }).ToList();
                return JsonConvert.SerializeObject(items);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
                return "[]";
            }
        }

        public static string GetCharacterFirstSpawnPlace(IPlayer player, int charId)
        {
            if (player == null || !player.Exists || charId == 0) return "";
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId && p.accountId == User.GetPlayerAccountId(player));

            if(chars != null)
            {
                return chars.firstSpawnPlace;
            }

            return "";
        }

        public static Position GetCharacterLastPosition(int charId)
        {
            var chars = CharactersLastPos.FirstOrDefault(p => p.charId == charId);

            if(chars != null)
            {
                return new Position(chars.lastPosX, chars.lastPosY, chars.lastPosZ);
            }

            return new Position(0, 0, 0);
        }

        public static void SetCharacterLastPosition(int charId, Position pos, int dimension)
        {
            var chars = CharactersLastPos.FirstOrDefault(p => p.charId == charId);

            if(chars != null)
            {
                chars.lastPosX = pos.X;
                chars.lastPosY = pos.Y;
                chars.lastPosZ = pos.Z;
                chars.lastDimension = dimension;

                try
                {
                    using (gtaContext db = new gtaContext())
                    {
                        db.Characters_LastPos.Update(chars);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Alt.Log($"{e}");
                }
            }
        }

        public static int GetCharacterLastDimension(int charId)
        {
            var chars = CharactersLastPos.FirstOrDefault(p => p.charId == charId);

            if(chars != null)
            {
                return chars.lastDimension;
            }

            return 0;
        }

        public static bool IsCharacterCrimeFlagged(int charId)
        {
            if (charId == 0) return false;
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            if(chars != null)
            {
                return chars.isCrime;
            }
            return false;
        }

        public static bool HasCharacterPermission(int charId, string permission)
        {
            if (charId == 0 || permission == "") return false;
            var chars = CharactersPermissions.FirstOrDefault(p => p.charId == charId && p.permissionName == permission);
            if(chars != null)
            {
                return true;
            }
            return false;
        }

        public static void AddCharacterPermission(int charId, string permission)
        {
            if (charId == 0 || permission == "") return;
            var chars = CharactersPermissions.FirstOrDefault(p => p.charId == charId && p.permissionName == permission);
            if (chars == null)
            {
                var permissionData = new Characters_Permissions
                {
                    charId = charId,
                    permissionName = permission
                };
                CharactersPermissions.Add(permissionData);

                using (gtaContext db = new gtaContext())
                {
                    db.Characters_Permissions.Add(permissionData);
                    db.SaveChanges();
                }
            }
        }

        public static void RemoveCharacterPermission(int charId, string permission)
        {
            if (charId == 0 || permission == "") return;
            var chars = CharactersPermissions.FirstOrDefault(p => p.charId == charId && p.permissionName == permission);
            if(chars != null)
            {
                CharactersPermissions.Remove(chars);
                using (gtaContext db = new gtaContext())
                {
                    db.Characters_Permissions.Remove(chars);
                    db.SaveChanges();
                }
            }
        }

        public static void SetCharacterCrimeFlagged(int charId, bool state)
        {
            if (charId == 0) return;
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            if (chars == null) return;
            chars.isCrime = state;
            try
            {
                using (gtaContext db = new gtaContext())
                {
                    db.AccountsCharacters.Update(chars);
                    db.SaveChanges();
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static int GetCharacterHealth(int charId)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);

            if(chars != null)
            {
                return chars.health;
            }

            return 0;
        }

        public static int GetCharacterArmor(int charId)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);

            if(chars != null)
            {
                return chars.armor;
            }

            return 0;
        }

        public static int GetCharacterHunger(int charId)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);

            if(chars != null)
            {
                return chars.hunger;
            }

            return 0;
        }

        public static int GetCharacterThirst(int charId)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);

            if(chars != null)
            {
                return chars.thirst;
            }

            return 0;
        }

        public static string GetCharacterClothes(int charId, string clothesType)
        {
            if (charId == 0) return "None";
            var chars = CharactersSkin.FirstOrDefault(p => p.charId == charId);
            if(chars != null)
            {
                switch(clothesType)
                {
                    case "Top":
                        return chars.clothesTop;
                    case "Torso":
                        return chars.clothesTorso;
                    case "Leg":
                        return chars.clothesLeg;
                    case "Feet":
                        return chars.clothesFeet;
                    case "Hat":
                        return chars.clothesHat;
                    case "Glass":
                        return chars.clothesGlass;
                    case "Necklace":
                        return chars.clothesNecklace;
                    case "Mask":
                        return chars.clothesMask;
                    case "Armor":
                        return chars.clothesArmor;
                    case "Undershirt":
                        return chars.clothesUndershirt;
                    case "Decal":
                        return chars.clothesDecal;
                    case "Bracelet":
                        return chars.clothesBracelet;
                    case "Watch":
                        return chars.clothesWatch;
                    case "Earring":
                        return chars.clothesEarring;

                }
            }
            return "None";
        }

        public static string GetCharacterBackpack(int charId)
        {
            if (charId == 0) return "None";
            var chars = CharactersSkin.FirstOrDefault(p => p.charId == charId);
            if(chars != null)
            {
                return chars.clothesBag;
            }
            return "None";
        }

        public static float GetCharacterBackpackSize(string backpack)
        {
            switch(backpack)
            {
                case "Rucksack":
                    return 15f;
                case "Tasche":
                    return 30f;
            }
            return 0f;
        }

        public static string GetCharacterJob(int charId)
        {
            if (charId == 0) return "None";
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            if(chars != null) { return chars.job; }
            return "None";
        }

        public static int GetCharacterJobHourCounter(int charId)
        {
            if (charId == 0) return 0;
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            if(chars != null) { return chars.jobHourCounter; }
            return 0;
        }

        public static void SetCharacterJob(int charId, string job)
        {
            try
            {
                if (charId == 0 || job == "") return;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if (chars != null)
                {
                    chars.job = job;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e) { Alt.Log($"{e}"); }
        }

        public static void SetCharacterLastJobPaycheck(int charId, DateTime dt)
        {
            try
            {
                if (charId == 0) return;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if (chars != null)
                {
                    chars.lastJobPaycheck = dt;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e) { Alt.Log($"{e}"); }
        }

        public static void SetCharacterLastLogin(int charId, DateTime dt)
        {
            try
            {
                if (charId <= 0) return;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if (chars != null)
                {
                    chars.lastLogin = dt;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static DateTime GetCharacterLastLogin(int charId)
        {
            DateTime dt = new DateTime();
            try
            {
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if (chars != null) dt = Convert.ToDateTime(chars.lastLogin);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return dt;
        }

        public static DateTime GetCharacterLastJobPaycheck(int charId)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            return chars.lastJobPaycheck;
        }

        public static DateTime GetCharacterFirstJoinDate(int charId)
        {
            DateTime dt = new DateTime(0001, 01, 01);
            if (charId <= 0) return dt;
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            if(chars != null)
            {
                dt = chars.firstJoinTimestamp;
            }
            return dt;
        }

        public static void SetCharacterHealth(int charId, int health)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);

            if(chars != null)
            {
                chars.health = health - 100;

                try
                {
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
                catch(Exception e)
                {
                    Alt.Log($"{e}");
                }
            }
        }

        public static void SetCharacterBirthplace(int charId, string place)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            if(chars != null)
            {
                chars.birthplace = place;
                try
                {
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
                catch(Exception e)
                {
                    Alt.Log($"{e}");
                }
            }
        }

        public static void setCharacterAccState(int charId, int state)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
            if(chars != null)
            {
                chars.accState = state;
                try
                {
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
                catch(Exception e)
                {
                    Alt.Log($"{e}");
                }
            }
        }

        public static void SetCharacterArmor(int charId, int armor)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);

            if(chars != null)
            {
                chars.armor = armor;

                try
                {
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
                catch(Exception e)
                {
                    Alt.Log($"{e}");
                }
            }
        }

        public static void SetCharacterHunger(int charId, int hunger)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);

            if(chars != null)
            {
                chars.hunger = hunger;
                if(chars.hunger > 100)
                {
                    chars.hunger = 100;
                }

                try
                {
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
                catch(Exception e)
                {
                    Alt.Log($"{e}");
                }
            }
        }

        public static void SetCharacterThirst(int charId, int thirst)
        {
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);

            if(chars != null)
            {
                chars.thirst = thirst;
                if (chars.thirst > 100)
                {
                    chars.thirst = 100;
                }
                try
                {
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
                catch(Exception e)
                {
                    Alt.Log($"{e}");
                }
            }
        }

        public static void SetCharacterFirstSpawnPlace(IPlayer player, int charId, string spawnplace)
        {
            if (player == null || !player.Exists || charId == 0) return;
            var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId && p.accountId == User.GetPlayerAccountId(player));

            if(chars != null)
            {
                chars.firstSpawnPlace = spawnplace;
                chars.firstJoin = false;

                try
                {
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
                catch(Exception e)
                {
                    Alt.Log($"{e}");
                }
            }
        }

        

        public static void SetCharacterBackpack(IPlayer player, string backpack)
        {
            if (player == null || !player.Exists) return;
            var charId = User.GetPlayerOnline(player);
            if (charId == 0) return;
            var chars = CharactersSkin.FirstOrDefault(p => p.charId == charId);
            if(chars != null)
            {
                switch(backpack)
                {
                    case "None":
                        player.SetClothes(5, 0, 0, 0);
                        break;
                    case "Rucksack":
                        player.SetClothes(5, 31, 0, 0);
                        break;
                    case "Tasche":
                        player.SetClothes(5, 45, 0, 0);
                        break;
                }

                try
                {
                    chars.clothesBag = backpack;
                    using (gtaContext db = new gtaContext())
                    {
                        db.Characters_Skin.Update(chars);
                        db.SaveChanges();
                    }
                }
                catch(Exception e)
                {
                    Alt.Log($"{e}");
                }
            }
        }

        public static void SetCharacterCorrectTorso(IPlayer player, int topID)
        {
            if (player == null || !player.Exists) return;
            int charId = User.GetPlayerOnline(player);
            if (charId == 0) return;
            int CTorso = 0;
            if (GetCharacterGender(charId) == false)
            {
                switch (topID)
                {
                    case 0: CTorso = 0; break;
                    case 1: CTorso = 0; break;
                    case 2: CTorso = 2; break;
                    case 3: CTorso = 14; break;
                    case 4: CTorso = 14; break;
                    case 5: CTorso = 5; break;
                    case 6: CTorso = 14; break;
                    case 7: CTorso = 14; break;
                    case 8: CTorso = 8; break;
                    case 9: CTorso = 0; break;
                    case 10: CTorso = 14; break;
                    case 11: CTorso = 15; break;
                    case 12: CTorso = 12; break;
                    case 13: CTorso = 11; break;
                    case 14: CTorso = 12; break;
                    case 15: CTorso = 15; break;
                    case 16: CTorso = 0; break;
                    case 17: CTorso = 5; break;
                    case 18: CTorso = 0; break;
                    case 19: CTorso = 1; break;
                    case 20: CTorso = 1; break;
                    case 21: CTorso = 11; break;
                    case 22: CTorso = 0; break;
                    case 23: CTorso = 1; break;
                    case 24: CTorso = 1; break;
                    case 25: CTorso = 11; break;
                    case 26: CTorso = 11; break;
                    case 27: CTorso = 1; break;
                    case 28: CTorso = 1; break;
                    case 29: CTorso = 1; break;
                    case 30: CTorso = 1; break;
                    case 31: CTorso = 1; break;
                    case 32: CTorso = 1; break;
                    case 33: CTorso = 0; break;
                    case 34: CTorso = 0; break;
                    case 35: CTorso = 1; break;
                    case 36: CTorso = 5; break;
                    case 37: CTorso = 12; break;
                    case 38: CTorso = 8; break;
                    case 39: CTorso = 0; break;
                    case 40: CTorso = 11; break;
                    case 41: CTorso = 12; break;
                    case 42: CTorso = 11; break;
                    case 43: CTorso = 11; break;
                    case 44: CTorso = 0; break;
                    case 45: CTorso = 15; break;
                    case 46: CTorso = 1; break;
                    case 47: CTorso = 0; break;
                    case 48: CTorso = 4; break;
                    case 49: CTorso = 4; break;
                    case 50: CTorso = 4; break;
                    case 51: CTorso = 4; break;
                    case 52: CTorso = 4; break;
                    case 53: CTorso = 4; break;
                    case 54: CTorso = 4; break;
                    case 55: CTorso = 0; break;
                    case 56: CTorso = 0; break;
                    case 57: CTorso = 4; break;
                    case 58: CTorso = 1; break;
                    case 59: CTorso = 1; break;
                    case 60: CTorso = 11; break;
                    case 61: CTorso = 4; break;
                    case 62: CTorso = 1; break;
                    case 63: CTorso = 0; break;
                    case 64: CTorso = 12; break;
                    case 65: CTorso = 4; break;
                    case 66: CTorso = 4; break;
                    case 67: CTorso = 4; break;
                    case 68: CTorso = 4; break;
                    case 69: CTorso = 4; break;
                    case 70: CTorso = 4; break;
                    case 71: CTorso = 0; break;
                    case 72: CTorso = 0; break;
                    case 73: CTorso = 0; break;
                    case 74: CTorso = 4; break;
                    case 75: CTorso = 6; break;
                    case 76: CTorso = 14; break;
                    case 77: CTorso = 4; break;
                    case 78: CTorso = 6; break;
                    case 79: CTorso = 6; break;
                    case 80: CTorso = 0; break;
                    case 81: CTorso = 0; break;
                    case 82: CTorso = 0; break;
                    case 83: CTorso = 0; break;
                    case 84: CTorso = 4; break;
                    case 85: CTorso = 1; break;
                    case 86: CTorso = 4; break;
                    case 87: CTorso = 4; break;
                    case 88: CTorso = 4; break;
                    case 89: CTorso = 6; break;
                    case 90: CTorso = 4; break;
                    case 91: CTorso = 15; break;
                    case 92: CTorso = 6; break;
                    case 93: CTorso = 0; break;
                    case 94: CTorso = 0; break;
                    case 95: CTorso = 11; break;
                    case 96: CTorso = 4; break;
                    case 97: CTorso = 0; break;
                    case 98: CTorso = 4; break;
                    case 99: CTorso = 1; break;
                    case 100: CTorso = 1; break;
                    case 101: CTorso = 1; break;
                    case 102: CTorso = 1; break;
                    case 103: CTorso = 1; break;
                    case 104: CTorso = 1; break;
                    case 105: CTorso = 0; break;
                    case 106: CTorso = 1; break;
                    case 107: CTorso = 4; break;
                    case 108: CTorso = 14; break;
                    case 109: CTorso = 11; break;
                    case 110: CTorso = 4; break;
                    case 111: CTorso = 4; break;
                    case 112: CTorso = 4; break;
                    case 113: CTorso = 6; break;
                    case 114: CTorso = 14; break;
                    case 115: CTorso = 4; break;
                    case 116: CTorso = 14; break;
                    case 117: CTorso = 6; break;
                    case 118: CTorso = 14; break;
                    case 119: CTorso = 1; break;
                    case 120: CTorso = 11; break;
                    case 121: CTorso = 4; break;
                    case 122: CTorso = 4; break;
                    case 123: CTorso = 0; break;
                    case 124: CTorso = 12; break;
                    case 125: CTorso = 4; break;
                    case 126: CTorso = 4; break;
                    case 127: CTorso = 14; break;
                    case 128: CTorso = 0; break;
                    case 129: CTorso = 4; break;
                    case 130: CTorso = 4; break;
                    case 131: CTorso = 0; break;
                    case 132: CTorso = 0; break;
                    case 133: CTorso = 11; break;
                    case 134: CTorso = 4; break;
                    case 135: CTorso = 0; break;
                    case 136: CTorso = 1; break;
                    case 137: CTorso = 11; break;
                    case 138: CTorso = 4; break;
                    case 139: CTorso = 4; break;
                    case 140: CTorso = 4; break;
                    case 141: CTorso = 6; break;
                    case 142: CTorso = 4; break;
                    case 143: CTorso = 14; break;
                    case 144: CTorso = 6; break;
                    case 145: CTorso = 14; break;
                    case 146: CTorso = 0; break;
                    case 147: CTorso = 4; break;
                    case 148: CTorso = 4; break;
                    case 149: CTorso = 14; break;
                    case 150: CTorso = 4; break;
                    case 151: CTorso = 1; break;
                    case 152: CTorso = 111; break;
                    case 153: CTorso = 4; break;
                    case 154: CTorso = 12; break;
                    case 155: CTorso = 14; break;
                    case 156: CTorso = 1; break;
                    case 157: CTorso = 112; break;
                    case 158: CTorso = 113; break;
                    case 159: CTorso = 114; break;
                    case 160: CTorso = 115; break;
                    case 161: CTorso = 6; break;
                    case 162: CTorso = 114; break;
                    case 163: CTorso = 1; break;
                    case 164: CTorso = 0; break;
                    case 165: CTorso = 6; break;
                    case 166: CTorso = 1; break;
                    case 167: CTorso = 1; break;
                    case 168: CTorso = 12; break;
                    case 169: CTorso = 1; break;
                    case 170: CTorso = 112; break;
                    case 171: CTorso = 4; break;
                    case 172: CTorso = 1; break;
                    case 173: CTorso = 112; break;
                    case 174: CTorso = 6; break;
                    case 175: CTorso = 114; break;
                    case 176: CTorso = 114; break;
                    case 177: CTorso = 2; break;
                    case 178: CTorso = 4; break;
                    case 179: CTorso = 112; break;
                    case 180: CTorso = 113; break;
                    case 181: CTorso = 1; break;
                    case 182: CTorso = 4; break;
                    case 183: CTorso = 1; break;
                    case 184: CTorso = 6; break;
                    case 185: CTorso = 4; break;
                    case 186: CTorso = 14; break;
                    case 187: CTorso = 6; break;
                    case 188: CTorso = 6; break;
                    case 189: CTorso = 4; break;
                    case 190: CTorso = 6; break;
                    case 191: CTorso = 1; break;
                    case 192: CTorso = 4; break;
                    case 193: CTorso = 0; break;
                    case 194: CTorso = 6; break;
                    case 195: CTorso = 6; break;
                    case 196: CTorso = 6; break;
                    case 197: CTorso = 6; break;
                    case 198: CTorso = 6; break;
                    case 199: CTorso = 6; break;
                    case 200: CTorso = 4; break;
                    case 201: CTorso = 3; break;
                    case 202: CTorso = 114; break;
                    case 203: CTorso = 4; break;
                    case 204: CTorso = 6; break;
                    case 205: CTorso = 114; break;
                    case 206: CTorso = 114; break;
                    case 207: CTorso = 114; break;
                    case 208: CTorso = 0; break;
                    case 209: CTorso = 6; break;
                    case 210: CTorso = 6; break;
                    case 211: CTorso = 6; break;
                    case 212: CTorso = 4; break;
                    case 213: CTorso = 113; break;
                    case 214: CTorso = 6; break;
                    case 215: CTorso = 1; break;
                    case 216: CTorso = 112; break;
                    case 217: CTorso = 6; break;
                    case 218: CTorso = 6; break;
                    case 219: CTorso = 2; break;
                    case 220: CTorso = 4; break;
                    case 221: CTorso = 4; break;
                    case 222: CTorso = 11; break;
                    case 223: CTorso = 2; break;
                    case 224: CTorso = 12; break;
                    case 225: CTorso = 8; break;
                    case 226: CTorso = 0; break;
                    case 227: CTorso = 4; break;
                    case 228: CTorso = 4; break;
                    case 229: CTorso = 4; break;
                    case 230: CTorso = 4; break;
                    case 231: CTorso = 4; break;
                    case 232: CTorso = 14; break;
                    case 233: CTorso = 14; break;
                    case 234: CTorso = 11; break;
                    case 235: CTorso = 0; break;
                    case 236: CTorso = 0; break;
                    case 237: CTorso = 5; break;
                    case 238: CTorso = 2; break;
                    case 239: CTorso = 2; break;
                    case 240: CTorso = 4; break;
                    case 241: CTorso = 0; break;
                    case 242: CTorso = 0; break;
                    case 243: CTorso = 4; break;
                    case 244: CTorso = 14; break;
                    case 245: CTorso = 6; break;
                    case 246: CTorso = 3; break;
                    case 247: CTorso = 114; break;
                    case 248: CTorso = 6; break;
                    case 249: CTorso = 6; break;
                    case 250: CTorso = 0; break;
                    case 251: CTorso = 4; break;
                    case 252: CTorso = 15; break;
                    case 253: CTorso = 4; break;
                    case 254: CTorso = 8; break;
                    case 255: CTorso = 4; break;
                    case 256: CTorso = 4; break;
                    case 257: CTorso = 6; break;
                    case 258: CTorso = 6; break;
                    case 259: CTorso = 6; break;
                    case 260: CTorso = 11; break;
                    case 261: CTorso = 4; break;
                    case 262: CTorso = 4; break;
                    case 263: CTorso = 4; break;
                    case 264: CTorso = 6; break;
                    case 265: CTorso = 4; break;
                    case 266: CTorso = 4; break;
                    case 267: CTorso = 14; break;
                    case 268: CTorso = 14; break;
                    case 269: CTorso = 1; break;
                    case 270: CTorso = 4; break;
                    case 271: CTorso = 0; break;
                    case 272: CTorso = 4; break;
                    case 273: CTorso = 0; break;
                    case 274: CTorso = 3; break;
                    case 275: CTorso = 4; break;
                    case 276: CTorso = 4; break;
                    case 277: CTorso = 164; break;
                    case 278: CTorso = 165; break;
                    case 279: CTorso = 4; break;
                    case 280: CTorso = 4; break;
                    case 281: CTorso = 6; break;
                    case 282: CTorso = 0; break;
                    case 283: CTorso = 166; break;
                    case 284: CTorso = 4; break;
                    case 285: CTorso = 17; break;
                    case 286: CTorso = 167; break;
                    case 287: CTorso = 3; break;
                    case 288: CTorso = 6; break;
                    case 289: CTorso = 2; break;
                    case 290: CTorso = 11; break;
                    case 291: CTorso = 168; break;
                    case 292: CTorso = 1; break;
                    case 293: CTorso = 1; break;
                    case 294: CTorso = 1; break;
                    case 295: CTorso = 1; break;
                    case 296: CTorso = 4; break;
                    case 297: CTorso = 4; break;
                    case 298: CTorso = 4; break;
                    case 299: CTorso = 0; break;
                    case 300: CTorso = 6; break;
                    case 301: CTorso = 6; break;
                    case 302: CTorso = 6; break;
                    case 303: CTorso = 4; break;
                    case 304: CTorso = 4; break;
                    case 305: CTorso = 4; break;
                    case 306: CTorso = 4; break;
                    case 307: CTorso = 6; break;
                    case 308: CTorso = 4; break;
                    case 309: CTorso = 1; break;
                    case 310: CTorso = 14; break;
                    case 311: CTorso = 1; break;
                    case 312: CTorso = 1; break;
                    case 313: CTorso = 0; break;
                    case 314: CTorso = 4; break;
                    case 315: CTorso = 4; break;
                    case 316: CTorso = 1; break;
                    case 317: CTorso = 1; break;
                    case 318: CTorso = 11; break;
                    case 319: CTorso = 11; break;
                    case 320: CTorso = 17; break;
                    case 321: CTorso = 1; break;
                    case 322: CTorso = 1; break;
                    case 323: CTorso = 0; break;
                    case 324: CTorso = 4; break;
                    case 325: CTorso = 0; break;
                    case 326: CTorso = 6; break;
                    case 327: CTorso = 113; break;
                    case 328: CTorso = 4; break;
                    case 329: CTorso = 4; break;
                    case 330: CTorso = 4; break;
                    case 331: CTorso = 4; break;
                    case 332: CTorso = 4; break;
                    case 333: CTorso = 4; break;
                    case 334: CTorso = 0; break;
                    case 335: CTorso = 8; break;
                    case 336: CTorso = 4; break;
                    case 337: CTorso = 11; break;
                    case 338: CTorso = 1; break;
                    case 339: CTorso = 14; break;
                    case 340: CTorso = 14; break;
                    case 341: CTorso = 14; break;
                    case 342: CTorso = 4; break;
                    case 343: CTorso = 4; break;
                    case 344: CTorso = 4; break;
                    case 345: CTorso = 0; break;
                    case 346: CTorso = 184; break;
                    case 347: CTorso = 184; break;
                    case 348: CTorso = 4; break;
                    case 349: CTorso = 1; break;
                    case 350: CTorso = 0; break;
                    case 351: CTorso = 0; break;
                    case 352: CTorso = 4; break;
                    case 353: CTorso = 4; break;
                    case 354: CTorso = 11; break;
                    case 355: CTorso = 184; break;
                    case 356: CTorso = 8; break;
                    case 357: CTorso = 2; break;
                    case 358: CTorso = 6; break;
                    case 359: CTorso = 4; break;
                    case 360: CTorso = 4; break;
                    case 361: CTorso = 4; break;
                    case 362: CTorso = 1; break;
                    case 363: CTorso = 4; break;
                    case 364: CTorso = 4; break;
                    case 365: CTorso = 114; break;
                    case 366: CTorso = 115; break;
                    case 367: CTorso = 114; break;
                    case 368: CTorso = 6; break;
                    case 369: CTorso = 2; break;
                    case 370: CTorso = 12; break;
                    case 371: CTorso = 4; break;
                    case 372: CTorso = 3; break;
                    case 373: CTorso = 4; break;
                    case 374: CTorso = 4; break;
                    case 375: CTorso = 4; break;
                    case 376: CTorso = 4; break;
                    case 377: CTorso = 0; break;
                    case 378: CTorso = 12; break;
                    case 379: CTorso = 4; break;
                    case 380: CTorso = 4; break;
                    case 381: CTorso = 4; break;
                    case 382: CTorso = 196; break;
                    case 383: CTorso = 196; break;
                    case 384: CTorso = 4; break;
                    case 385: CTorso = 4; break;
                    case 386: CTorso = 4; break;
                    case 387: CTorso = 4; break;
                    case 388: CTorso = 4; break;
                    case 389: CTorso = 4; break;
                    case 390: CTorso = 4; break;
                    case 391: CTorso = 4; break;
                    case 392: CTorso = 0; break;
                }
            }
            else
            {
                switch (topID)
                {
                    case 0: CTorso = 4; break;
                    case 1: CTorso = 5; break;
                    case 2: CTorso = 2; break;
                    case 3: CTorso = 3; break;
                    case 4: CTorso = 4; break;
                    case 5: CTorso = 4; break;
                    case 6: CTorso = 5; break;
                    case 7: CTorso = 6; break;
                    case 8: CTorso = 5; break;
                    case 9: CTorso = 0; break;
                    case 10: CTorso = 5; break;
                    case 11: CTorso = 15; break;
                    case 12: CTorso = 12; break;
                    case 13: CTorso = 15; break;
                    case 14: CTorso = 14; break;
                    case 15: CTorso = 15; break;
                    case 16: CTorso = 4; break;
                    case 17: CTorso = 9; break;
                    case 18: CTorso = 15; break;
                    case 19: CTorso = 0; break;
                    case 20: CTorso = 5; break;
                    case 21: CTorso = 16; break;
                    case 22: CTorso = 4; break;
                    case 23: CTorso = 0; break;
                    case 24: CTorso = 6; break;
                    case 25: CTorso = 6; break;
                    case 26: CTorso = 12; break;
                    case 27: CTorso = 0; break;
                    case 28: CTorso = 0; break;
                    case 29: CTorso = 9; break;
                    case 30: CTorso = 2; break;
                    case 31: CTorso = 5; break;
                    case 32: CTorso = 4; break;
                    case 33: CTorso = 4; break;
                    case 34: CTorso = 6; break;
                    case 35: CTorso = 5; break;
                    case 36: CTorso = 11; break;
                    case 37: CTorso = 4; break;
                    case 38: CTorso = 5; break;
                    case 39: CTorso = 5; break;
                    case 40: CTorso = 2; break;
                    case 41: CTorso = 3; break;
                    case 42: CTorso = 3; break;
                    case 43: CTorso = 3; break;
                    case 44: CTorso = 3; break;
                    case 45: CTorso = 3; break;
                    case 46: CTorso = 3; break;
                    case 47: CTorso = 3; break;
                    case 48: CTorso = 14; break;
                    case 49: CTorso = 14; break;
                    case 50: CTorso = 3; break;
                    case 51: CTorso = 3; break;
                    case 52: CTorso = 3; break;
                    case 53: CTorso = 9; break;
                    case 54: CTorso = 3; break;
                    case 55: CTorso = 3; break;
                    case 56: CTorso = 14; break;
                    case 57: CTorso = 3; break;
                    case 58: CTorso = 3; break;
                    case 59: CTorso = 3; break;
                    case 60: CTorso = 3; break;
                    case 61: CTorso = 3; break;
                    case 62: CTorso = 3; break;
                    case 63: CTorso = 3; break;
                    case 64: CTorso = 5; break;
                    case 65: CTorso = 5; break;
                    case 66: CTorso = 5; break;
                    case 67: CTorso = 2; break;
                    case 68: CTorso = 14; break;
                    case 69: CTorso = 5; break;
                    case 70: CTorso = 5; break;
                    case 71: CTorso = 1; break;
                    case 72: CTorso = 1; break;
                    case 73: CTorso = 14; break;
                    case 74: CTorso = 15; break;
                    case 75: CTorso = 9; break;
                    case 76: CTorso = 9; break;
                    case 77: CTorso = 6; break;
                    case 78: CTorso = 3; break;
                    case 79: CTorso = 1; break;
                    case 80: CTorso = 3; break;
                    case 81: CTorso = 3; break;
                    case 82: CTorso = 0; break;
                    case 83: CTorso = 0; break;
                    case 84: CTorso = 14; break;
                    case 85: CTorso = 9; break;
                    case 86: CTorso = 9; break;
                    case 87: CTorso = 3; break;
                    case 88: CTorso = 14; break;
                    case 89: CTorso = 3; break;
                    case 90: CTorso = 3; break;
                    case 91: CTorso = 3; break;
                    case 92: CTorso = 3; break;
                    case 93: CTorso = 3; break;
                    case 94: CTorso = 3; break;
                    case 95: CTorso = 3; break;
                    case 96: CTorso = 9; break;
                    case 97: CTorso = 6; break;
                    case 98: CTorso = 3; break;
                    case 99: CTorso = 7; break;
                    case 100: CTorso = 6; break;
                    case 101: CTorso = 15; break;
                    case 102: CTorso = 3; break;
                    case 103: CTorso = 3; break;
                    case 104: CTorso = 7; break;
                    case 105: CTorso = 0; break;
                    case 106: CTorso = 6; break;
                    case 107: CTorso = 6; break;
                    case 108: CTorso = 6; break;
                    case 109: CTorso = 0; break;
                    case 110: CTorso = 14; break;
                    case 111: CTorso = 4; break;
                    case 112: CTorso = 11; break;
                    case 113: CTorso = 11; break;
                    case 114: CTorso = 11; break;
                    case 115: CTorso = 11; break;
                    case 116: CTorso = 11; break;
                    case 117: CTorso = 11; break;
                    case 118: CTorso = 12; break;
                    case 119: CTorso = 14; break;
                    case 120: CTorso = 6; break;
                    case 121: CTorso = 3; break;
                    case 122: CTorso = 3; break;
                    case 123: CTorso = 3; break;
                    case 124: CTorso = 14; break;
                    case 125: CTorso = 14; break;
                    case 126: CTorso = 14; break;
                    case 127: CTorso = 3; break;
                    case 128: CTorso = 14; break;
                    case 129: CTorso = 14; break;
                    case 130: CTorso = 9; break;
                    case 131: CTorso = 3; break;
                    case 132: CTorso = 9; break;
                    case 133: CTorso = 6; break;
                    case 134: CTorso = 6; break;
                    case 135: CTorso = 3; break;
                    case 136: CTorso = 3; break;
                    case 137: CTorso = 7; break;
                    case 138: CTorso = 6; break;
                    case 139: CTorso = 6; break;
                    case 140: CTorso = 14; break;
                    case 141: CTorso = 14; break;
                    case 142: CTorso = 0; break;
                    case 143: CTorso = 7; break;
                    case 144: CTorso = 3; break;
                    case 145: CTorso = 3; break;
                    case 146: CTorso = 7; break;
                    case 147: CTorso = 3; break;
                    case 148: CTorso = 3; break;
                    case 149: CTorso = 128; break;
                    case 150: CTorso = 3; break;
                    case 151: CTorso = 1; break;
                    case 152: CTorso = 7; break;
                    case 153: CTorso = 3; break;
                    case 154: CTorso = 129; break;
                    case 155: CTorso = 130; break;
                    case 156: CTorso = 131; break;
                    case 157: CTorso = 132; break;
                    case 158: CTorso = 7; break;
                    case 159: CTorso = 131; break;
                    case 160: CTorso = 5; break;
                    case 161: CTorso = 9; break;
                    case 162: CTorso = 3; break;
                    case 163: CTorso = 5; break;
                    case 164: CTorso = 5; break;
                    case 165: CTorso = 0; break;
                    case 166: CTorso = 5; break;
                    case 167: CTorso = 129; break;
                    case 168: CTorso = 161; break;
                    case 169: CTorso = 153; break;
                    case 170: CTorso = 15; break;
                    case 171: CTorso = 153; break;
                    case 172: CTorso = 3; break;
                    case 173: CTorso = 4; break;
                    case 174: CTorso = 5; break;
                    case 175: CTorso = 129; break;
                    case 176: CTorso = 7; break;
                    case 177: CTorso = 131; break;
                    case 178: CTorso = 131; break;
                    case 179: CTorso = 11; break;
                    case 180: CTorso = 3; break;
                    case 181: CTorso = 129; break;
                    case 182: CTorso = 130; break;
                    case 183: CTorso = 5; break;
                    case 184: CTorso = 3; break;
                    case 185: CTorso = 3; break;
                    case 186: CTorso = 3; break;
                    case 187: CTorso = 5; break;
                    case 188: CTorso = 14; break;
                    case 189: CTorso = 7; break;
                    case 190: CTorso = 3; break;
                    case 191: CTorso = 5; break;
                    case 192: CTorso = 1; break;
                    case 193: CTorso = 5; break;
                    case 194: CTorso = 6; break;
                    case 195: CTorso = 153; break;
                    case 196: CTorso = 3; break;
                    case 197: CTorso = 3; break;
                    case 198: CTorso = 1; break;
                    case 199: CTorso = 1; break;
                    case 200: CTorso = 1; break;
                    case 201: CTorso = 1; break;
                    case 202: CTorso = 3; break;
                    case 203: CTorso = 8; break;
                    case 204: CTorso = 131; break;
                    case 205: CTorso = 3; break;
                    case 206: CTorso = 7; break;
                    case 207: CTorso = 131; break;
                    case 208: CTorso = 11; break;
                    case 209: CTorso = 12; break;
                    case 210: CTorso = 131; break;
                    case 211: CTorso = 131; break;
                    case 212: CTorso = 14; break;
                    case 213: CTorso = 3; break;
                    case 214: CTorso = 3; break;
                    case 215: CTorso = 3; break;
                    case 216: CTorso = 5; break;
                    case 217: CTorso = 130; break;
                    case 218: CTorso = 3; break;
                    case 219: CTorso = 5; break;
                    case 220: CTorso = 129; break;
                    case 221: CTorso = 161; break;
                    case 222: CTorso = 153; break;
                    case 223: CTorso = 15; break;
                    case 224: CTorso = 14; break;
                    case 225: CTorso = 12; break;
                    case 226: CTorso = 11; break;
                    case 227: CTorso = 3; break;
                    case 228: CTorso = 3; break;
                    case 229: CTorso = 11; break;
                    case 230: CTorso = 3; break;
                    case 231: CTorso = 3; break;
                    case 232: CTorso = 9; break;
                    case 233: CTorso = 11; break;
                    case 234: CTorso = 6; break;
                    case 235: CTorso = 9; break;
                    case 236: CTorso = 14; break;
                    case 237: CTorso = 3; break;
                    case 238: CTorso = 3; break;
                    case 239: CTorso = 3; break;
                    case 240: CTorso = 5; break;
                    case 241: CTorso = 3; break;
                    case 242: CTorso = 6; break;
                    case 243: CTorso = 6; break;
                    case 244: CTorso = 9; break;
                    case 245: CTorso = 14; break;
                    case 246: CTorso = 14; break;
                    case 247: CTorso = 4; break;
                    case 248: CTorso = 5; break;
                    case 249: CTorso = 14; break;
                    case 250: CTorso = 14; break;
                    case 251: CTorso = 3; break;
                    case 252: CTorso = 5; break;
                    case 253: CTorso = 1; break;
                    case 254: CTorso = 8; break;
                    case 255: CTorso = 131; break;
                    case 256: CTorso = 9; break;
                    case 257: CTorso = 6; break;
                    case 258: CTorso = 14; break;
                    case 259: CTorso = 3; break;
                    case 260: CTorso = 4; break;
                    case 261: CTorso = 3; break;
                    case 262: CTorso = 7; break;
                    case 263: CTorso = 3; break;
                    case 264: CTorso = 3; break;
                    case 265: CTorso = 3; break;
                    case 266: CTorso = 6; break;
                    case 267: CTorso = 1; break;
                    case 268: CTorso = 1; break;
                    case 269: CTorso = 9; break;
                    case 270: CTorso = 5; break;
                    case 271: CTorso = 3; break;
                    case 272: CTorso = 3; break;
                    case 273: CTorso = 5; break;
                    case 274: CTorso = 3; break;
                    case 275: CTorso = 5; break;
                    case 276: CTorso = 6; break;
                    case 277: CTorso = 6; break;
                    case 278: CTorso = 5; break;
                    case 279: CTorso = 15; break;
                    case 280: CTorso = 14; break;
                    case 281: CTorso = 14; break;
                    case 282: CTorso = 14; break;
                    case 283: CTorso = 12; break;
                    case 284: CTorso = 15; break;
                    case 285: CTorso = 3; break;
                    case 286: CTorso = 14; break;
                    case 287: CTorso = 8; break;
                    case 288: CTorso = 3; break;
                    case 289: CTorso = 3; break;
                    case 290: CTorso = 205; break;
                    case 291: CTorso = 206; break;
                    case 292: CTorso = 3; break;
                    case 293: CTorso = 3; break;
                    case 294: CTorso = 1; break;
                    case 295: CTorso = 14; break;
                    case 296: CTorso = 207; break;
                    case 297: CTorso = 3; break;
                    case 298: CTorso = 18; break;
                    case 299: CTorso = 208; break;
                    case 300: CTorso = 8; break;
                    case 301: CTorso = 1; break;
                    case 302: CTorso = 131; break;
                    case 303: CTorso = 11; break;
                    case 304: CTorso = 209; break;
                    case 305: CTorso = 3; break;
                    case 306: CTorso = 3; break;
                    case 307: CTorso = 3; break;
                    case 308: CTorso = 3; break;
                    case 309: CTorso = 3; break;
                    case 310: CTorso = 9; break;
                    case 311: CTorso = 3; break;
                    case 312: CTorso = 3; break;
                    case 313: CTorso = 3; break;
                    case 314: CTorso = 5; break;
                    case 315: CTorso = 5; break;
                    case 316: CTorso = 3; break;
                    case 317: CTorso = 3; break;
                    case 318: CTorso = 1; break;
                    case 319: CTorso = 3; break;
                    case 320: CTorso = 5; break;
                    case 321: CTorso = 0; break;
                    case 322: CTorso = 11; break;
                    case 323: CTorso = 11; break;
                    case 324: CTorso = 14; break;
                    case 325: CTorso = 3; break;
                    case 326: CTorso = 3; break;
                    case 327: CTorso = 3; break;
                    case 328: CTorso = 3; break;
                    case 329: CTorso = 9; break;
                    case 330: CTorso = 9; break;
                    case 331: CTorso = 18; break;
                    case 332: CTorso = 3; break;
                    case 333: CTorso = 3; break;
                    case 334: CTorso = 14; break;
                    case 335: CTorso = 14; break;
                    case 336: CTorso = 3; break;
                    case 337: CTorso = 14; break;
                    case 338: CTorso = 14; break;
                    case 339: CTorso = 3; break;
                    case 340: CTorso = 3; break;
                    case 341: CTorso = 1; break;
                    case 342: CTorso = 130; break;
                    case 343: CTorso = 3; break;
                    case 344: CTorso = 3; break;
                    case 345: CTorso = 3; break;
                    case 346: CTorso = 3; break;
                    case 347: CTorso = 3; break;
                    case 348: CTorso = 210; break;
                    case 349: CTorso = 14; break;
                    case 350: CTorso = 9; break;
                    case 351: CTorso = 3; break;
                    case 352: CTorso = 9; break;
                    case 353: CTorso = 3; break;
                    case 354: CTorso = 6; break;
                    case 355: CTorso = 6; break;
                    case 356: CTorso = 3; break;
                    case 357: CTorso = 9; break;
                    case 358: CTorso = 0; break;
                    case 359: CTorso = 9; break;
                    case 360: CTorso = 0; break;
                    case 361: CTorso = 3; break;
                    case 362: CTorso = 3; break;
                    case 363: CTorso = 5; break;
                    case 364: CTorso = 229; break;
                    case 365: CTorso = 229; break;
                    case 366: CTorso = 3; break;
                    case 367: CTorso = 3; break;
                    case 368: CTorso = 14; break;
                    case 369: CTorso = 14; break;
                    case 370: CTorso = 3; break;
                    case 371: CTorso = 3; break;
                    case 372: CTorso = 9; break;
                    case 373: CTorso = 229; break;
                    case 374: CTorso = 9; break;
                    case 375: CTorso = 130; break;
                    case 376: CTorso = 1; break;
                    case 377: CTorso = 14; break;
                    case 378: CTorso = 3; break;
                    case 379: CTorso = 5; break;
                    case 380: CTorso = 3; break;
                    case 381: CTorso = 1; break;
                    case 382: CTorso = 3; break;
                    case 383: CTorso = 3; break;
                    case 384: CTorso = 131; break;
                    case 385: CTorso = 132; break;
                    case 386: CTorso = 131; break;
                    case 387: CTorso = 7; break;
                    case 388: CTorso = 11; break;
                    case 389: CTorso = 6; break;
                    case 390: CTorso = 3; break;
                    case 391: CTorso = 8; break;
                    case 392: CTorso = 3; break;
                    case 393: CTorso = 3; break;
                    case 394: CTorso = 3; break;
                    case 395: CTorso = 14; break;
                    case 396: CTorso = 1; break;
                    case 397: CTorso = 3; break;
                    case 398: CTorso = 3; break;
                    case 399: CTorso = 5; break;
                    case 400: CTorso = 14; break;
                    case 401: CTorso = 14; break;
                    case 402: CTorso = 3; break;
                    case 403: CTorso = 5; break;
                    case 404: CTorso = 12; break;
                    case 405: CTorso = 12; break;
                    case 406: CTorso = 7; break;
                    case 407: CTorso = 3; break;
                    case 408: CTorso = 3; break;
                    case 409: CTorso = 3; break;
                    case 410: CTorso = 3; break;
                    case 411: CTorso = 5; break;
                    case 412: CTorso = 5; break;
                    case 413: CTorso = 14; break;
                    case 414: CTorso = 14; break;
                }
            }
            player.SetClothes(3, (ushort)CTorso, 0, 0);
        }

        [AsyncClientEvent("Server:ClothesShop:RequestCurrentSkin")]
        public static void SetCharacterCorrectClothes(IPlayer player, bool withWeapons = false)
        {
            if (player == null || !player.Exists) return;
            int charid = User.GetPlayerOnline(player);
            if (charid == 0) return;
            bool gender = GetCharacterGender(charid);
            SetCharacterBackpack(player, GetCharacterBackpack(charid));
            SetCharacterHairs((ClassicPlayer)player, charid);

            if (GetCharacterClothes(charid, "Top") == "None")
            {
                player.SetClothes(11, 15, 0, 0);
                player.SetClothes(3, 15, 0, 0);
            }
            else
            {
                player.SetClothes(11, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Top")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Top")), 0);
                SetCharacterCorrectTorso(player, ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Top")));
            }
            if (GetCharacterClothes(charid, "Leg") == "None")
            {
                if (gender) player.SetClothes(4, 15, 0, 0);
                else player.SetClothes(4, 21, 0, 0);
            }
            else player.SetClothes(4, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Leg")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Leg")), 0);

            if (GetCharacterClothes(charid, "Feet") == "None")
            {
                if (gender) player.SetClothes(6, 35, 0, 0);
                else player.SetClothes(6, 34, 0, 0);
            }
            else player.SetClothes(6, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Feet")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Feet")), 0);

            if (GetCharacterClothes(charid, "Mask") == "None") player.SetClothes(1, 0, 0, 0);
            else player.SetClothes(1, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Mask")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Mask")), 0);

            if (GetCharacterClothes(charid, "Necklace") == "None") player.SetClothes(7, 0, 0, 0);
            else player.SetClothes(7, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Necklace")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Necklace")), 0);

            if (GetCharacterClothes(charid, "Armor") == "None") player.SetClothes(9, 0, 0, 0);
            else player.SetClothes(9, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Armor")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Armor")), 0);

            if (GetCharacterClothes(charid, "Hat") == "None") player.ClearProps(0);
            else player.SetProps(0, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Hat")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Hat")));

            if (GetCharacterClothes(charid, "Glass") == "None") player.ClearProps(1);
            else player.SetProps(1, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Glass")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Glass")));

            if (GetCharacterClothes(charid, "Earring") == "None") player.ClearProps(2);
            else player.SetProps(2, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Earring")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Earring")));

            if (GetCharacterClothes(charid, "Watch") == "None") player.ClearProps(6);
            else player.SetProps(6, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Watch")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Watch")));

            if (GetCharacterClothes(charid, "Bracelet") == "None") player.ClearProps(7);
            else player.SetProps(7, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Bracelet")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Bracelet")));

            if (GetCharacterClothes(charid, "Undershirt") == "None")
            {
                if (!gender) player.SetClothes(8, 57, 0, 0);
                else if (gender) player.SetClothes(8, 34, 0, 0);
            }
            else player.SetClothes(8, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Undershirt")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Undershirt")), 0);

            if (GetCharacterClothes(charid, "Decal") == "None") player.SetClothes(10, 0, 0, 0);
            else player.SetClothes(10, (ushort)ServerClothes.GetClothesDraw(GetCharacterClothes(charid, "Decal")), (byte)ServerClothes.GetClothesTexture(GetCharacterClothes(charid, "Decal")), 0);
            if (withWeapons)
            {
                string primaryWeapon = (string)GetCharacterWeapon(player, "PrimaryWeapon");
                int primaryAmmo = (int)GetCharacterWeapon(player, "PrimaryAmmo");
                string SecWeapon = (string)GetCharacterWeapon(player, "SecondaryWeapon");
                int SecAmmo = (int)GetCharacterWeapon(player, "SecondaryAmmo");
                string Sec2Weapon = (string)GetCharacterWeapon(player, "SecondaryWeapon2");
                int Sec2Ammo = (int)GetCharacterWeapon(player, "SecondaryAmmo2");
                string FistWeapon = (string)GetCharacterWeapon(player, "FistWeapon");
                if (primaryWeapon != "None") { player.GiveWeapon(WeaponHandler.GetWeaponModelByName(primaryWeapon), 0, false); WeaponHandler.SetWeaponComponents(player, primaryWeapon); player.Emit("Client:Weapon:SetWeaponAmmo", (uint)WeaponHandler.GetWeaponModelByName(primaryWeapon), primaryAmmo); }
                if (SecWeapon != "None") { player.GiveWeapon(WeaponHandler.GetWeaponModelByName(SecWeapon), 0, false); WeaponHandler.SetWeaponComponents(player, SecWeapon); player.Emit("Client:Weapon:SetWeaponAmmo", (uint)WeaponHandler.GetWeaponModelByName(SecWeapon), SecAmmo); }
                if (Sec2Weapon != "None") { player.GiveWeapon(WeaponHandler.GetWeaponModelByName(Sec2Weapon), 0, false); WeaponHandler.SetWeaponComponents(player, Sec2Weapon); player.Emit("Client:Weapon:SetWeaponAmmo", (uint)WeaponHandler.GetWeaponModelByName(Sec2Weapon), Sec2Ammo); }
                if (FistWeapon != "None") { player.GiveWeapon(WeaponHandler.GetWeaponModelByName(FistWeapon), 1, false); }
            }
        }

        public static void SwitchCharacterClothesItem(IPlayer player, string ClothesName, string Type)
        {
            if (player == null || !player.Exists) return;
            bool ClothesGender = false;
            int charid = User.GetPlayerOnline(player);
            int clothesId = 0;
            if (charid == 0) return;
            if (ClothesName.Contains("-W-")) { ClothesGender = true; } 
            else if(ClothesName.Contains("-M-")) { ClothesGender = false; }
            else { ClothesGender = GetCharacterGender(charid); }

            if (GetCharacterClothes(charid, Type) == "None")
            {
                if (ClothesGender == GetCharacterGender(charid))
                {
                    SetCharacterClothes(charid, Type, ClothesName);
                    if (Type == "Top")
                    {
                        clothesId = 11;
                        var sItem = ServerItems.ServerItems_.FirstOrDefault(i => i.itemName == ClothesName);
                        if (sItem != null)
                        {
                            player.SetClothes((byte)clothesId, (ushort)sItem.ClothesDraw, (byte)sItem.ClothesTexture, 0);
                            player.SetClothes(10, (ushort)sItem.ClothesDecals, (byte)sItem.ClothesDecalsTexture, 0);
                            if (sItem.ClothesUndershirt != 0) { player.SetClothes(8, (ushort)sItem.ClothesUndershirt, (byte)sItem.ClothesUndershirtTexture, 0); }
                            else
                            {
                                if (ClothesGender == false) { player.SetClothes(8, 57, 0, 0); }
                                else { player.SetClothes(8, 2, 0, 0); }
                            }
                        }
                    }
                    else if (Type == "Leg") {
                        clothesId = 4;
                        player.SetClothes((byte)clothesId, (ushort)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw, (byte)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesTexture, 0);
                    }
                    else if (Type == "Feet") { clothesId = 6; player.SetClothes((byte)clothesId, (ushort)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw, (byte)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesTexture, 0); }
                    else if (Type == "Mask") { clothesId = 1; player.SetClothes((byte)clothesId, (ushort)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw, (byte)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesTexture, 0); }
                    else if (Type == "Necklace") { clothesId = 7; player.SetClothes((byte)clothesId, (ushort)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw, (byte)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesTexture, 0); }
                    else if (Type == "Undershirt") { clothesId = 8; player.SetClothes((byte)clothesId, (ushort)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw, (byte)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesTexture, 0); }
                    else if (Type == "Armor") { clothesId = 9; player.SetClothes((byte)clothesId, (ushort)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw, (byte)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesTexture, 0); }
                    else if (Type == "Hat") { clothesId = 0; player.SetProps((byte)clothesId, (ushort)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw, (byte)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesTexture); }
                    else if (Type == "Glass") { clothesId = 1; player.SetProps((byte)clothesId, (ushort)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw, (byte)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesTexture); }
                    else if (Type == "Earring") { clothesId = 2; player.SetProps((byte)clothesId, (ushort)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw, (byte)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesTexture); }
                    else if (Type == "Watch") { clothesId = 6; player.SetProps((byte)clothesId, (ushort)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw, (byte)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesTexture); }
                    else if (Type == "Bracelet") { clothesId = 7; player.SetProps((byte)clothesId, (ushort)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw, (byte)ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesTexture); }
                    if (Type == "Top") { SetCharacterCorrectTorso(player, ServerItems.ServerItems_.First(x => x.itemName == ClothesName).ClothesDraw); }
                    if (ClothesName.Contains("-M-")) { ClothesName = ClothesName.Replace("-M-", "♂"); }
                    else if (ClothesName.Contains("-W")) { ClothesName = ClothesName.Replace("-W-", "♀"); }
                    HUDHandler.SendNotification(player, 1, 5000, $"Du hast {ClothesName} angezogen!");
                }
                else { HUDHandler.SendNotification(player, 4, 5000, $"Dieses Kleidungsstück passt dir nicht ({ClothesName})."); }
            }
            else if(GetCharacterClothes(charid, Type) == ClothesName)
            {
                SetCharacterClothes(charid, Type, "None");
                if (Type == "Top")
                {
                    clothesId = 11;
                    player.SetClothes((byte)clothesId, 15, 0, 0);
                    SetCharacterCorrectTorso(player, ServerClothes.ServerClothes_.First(x => x.clothesName == ClothesName).draw);
                    if (ClothesGender == false) { player.SetClothes(8, 57, 0, 0); }
                    else { player.SetClothes(8, 2, 0, 0); }
                }
                else if (Type == "Leg")
                {
                    clothesId = 4;
                    if (ClothesGender == false) { player.SetClothes((byte)clothesId, 21, 0, 0); }
                    else { player.SetClothes((byte)clothesId, 15, 0, 0); }
                }
                else if (Type == "Feet")
                {
                    clothesId = 6;
                    if (ClothesGender == false) { player.SetClothes((byte)clothesId, 34, 0, 0); }
                    else { player.SetClothes((byte)clothesId, 35, 0, 0); }
                }
                else if (Type == "Mask")
                {
                    clothesId = 1;
                    player.SetClothes((byte)clothesId, 0, 0, 0);
                }
                else if(Type == "Necklace")
                {
                    clothesId = 7;
                    player.SetClothes((byte)clothesId, 0, 0, 0);
                }
                else if(Type == "Undershirt")
                {
                    clothesId = 8;
                    if(ClothesGender == false) { player.SetClothes((byte)clothesId, 57, 0, 0); }
                    else { player.SetClothes((byte)clothesId, 34, 0, 0); }
                }
                else if(Type == "Armor")
                {
                    clothesId = 9;
                    player.SetClothes((byte)clothesId, 0, 0, 0);
                }
                else if(Type == "Hat")
                {
                    clothesId = 0;
                    player.ClearProps((byte)clothesId);
                }
                else if(Type == "Glass")
                {
                    clothesId = 1;
                    player.ClearProps((byte)clothesId);
                }
                else if(Type == "Earring")
                {
                    clothesId = 2;
                    player.ClearProps((byte)clothesId);
                }
                else if(Type == "Watch")
                {
                    clothesId = 6;
                    player.ClearProps((byte)clothesId);
                }
                else if(Type == "Bracelet")
                {
                    clothesId = 7;
                    player.ClearProps((byte)clothesId);
                }

                if(ClothesName.Contains("-M-")) { ClothesName = ClothesName.Replace("-M-", "♂"); }
                else if(ClothesName.Contains("-W-")) { ClothesName = ClothesName.Replace("-W-", "♀"); }
                HUDHandler.SendNotification(player, 1, 5000, $"Du hast {ClothesName} ausgezogen.");
            }
            else if(GetCharacterClothes(charid, Type) != ClothesName && GetCharacterClothes(charid, Type) != "None")
            {
                string clothesTypeStr = "";
                if(Type == "Top") { clothesTypeStr = "deinen Oberkörper"; }
                else if(Type == "Leg") { clothesTypeStr = "deine Beine"; }
                else if(Type == "Feet") { clothesTypeStr = "deine Füße"; }
                else if(Type == "Mask") { clothesTypeStr = "dein Gesicht"; }
                else if(Type == "Necklace") { clothesTypeStr = "deinen Hals"; }
                else if(Type == "Undershirt") { clothesTypeStr = "dein Unterhemd"; }
                else if(Type == "Hat") { clothesTypeStr = "dein Kopf"; }
                else if(Type == "Glass") { clothesTypeStr = "deine Augen"; }
                else if(Type == "Earring") { clothesTypeStr = "deine Ohren"; }
                else if(Type == "Watch") { clothesTypeStr = "deinen Unterarm"; }
                else if(Type == "Bracelet") { clothesTypeStr = "deinen Unterarm"; }
                HUDHandler.SendNotification(player, 3, 5000, $"Du musst vorher {clothesTypeStr} freimachen.");
            }
        }

        [AsyncClientEvent("Server:ClothesStorage:setCharacterClothes")]
        public static void SwitchCharacterClothes(ClassicPlayer player, string Type, string clothesName)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || string.IsNullOrWhiteSpace(Type) || string.IsNullOrWhiteSpace(clothesName)) return;
                bool gender = GetCharacterGender(player.CharacterId);
                int charId = User.GetPlayerOnline(player);
                if (clothesName != "None")
                {
                    SetCharacterClothes(player.CharacterId, Type, clothesName);
                    switch (Type)
                    {
                        case "Hat":
                            player.SetProps(0, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName));
                            break;
                        case "Glass":
                            player.SetProps(1, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName));
                            break;
                        case "Earring":
                            player.SetProps(2, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName));
                            break;
                        case "Watch":
                            player.SetProps(6, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName));
                            break;
                        case "Bracelet":
                            player.SetProps(7, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName));
                            break;
                        case "Mask":
                            player.SetClothes(1, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName), 0);
                            break;
                        case "Necklace":
                            player.SetClothes(7, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName), 0);
                            break;
                        case "Undershirt":
                            player.SetClothes(8, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName), 0);
                            break;
                        case "Decal":
                            player.SetClothes(10, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName), 0);
                            break;
                        case "Armor":
                            player.SetClothes(9, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName), 0);
                            break;
                        case "Feet":
                            player.SetClothes(6, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName), 0);
                            break;
                        case "Top":
                            player.SetClothes(11, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName), 0);
                            break;
                        case "Torso":
                            SetCharacterCorrectTorso(player, ServerClothes.ServerClothes_.First(x => x.clothesName == clothesName).draw);
                            break;
                        case "Leg":
                            player.SetClothes(4, (ushort)ServerClothes.GetClothesDraw(clothesName), (byte)ServerClothes.GetClothesTexture(clothesName), 0);
                            break;
                    }
                }
                else
                {
                    SetCharacterClothes(player.CharacterId, Type, "None");
                    switch (Type)
                    {
                        case "Hat":
                            player.ClearProps(0);
                            break;
                        case "Glass":
                            player.ClearProps(1);
                            break;
                        case "Earring":
                            player.ClearProps(2);
                            break;
                        case "Watch":
                            player.ClearProps(6);
                            break;
                        case "Bracelet":
                            player.ClearProps(7);
                            break;
                        case "Mask":
                            player.SetClothes(1, 0, 0, 0);
                            break;
                        case "Necklace":
                            player.SetClothes(7, 0, 0, 0);
                            break;
                        case "Undershirt":
                            if (!gender) player.SetClothes(8, 57, 0, 0);
                            else if (gender) player.SetClothes(8, 34, 0, 0);
                            break;
                        case "Decal":
                            player.SetClothes(10, 0, 0, 0);
                            break;
                        case "Armor":
                            player.SetClothes(9, 0, 0, 0);
                            break;
                        case "Feet":
                            if (!gender) player.SetClothes(6, 34, 0, 0);
                            else if (gender) player.SetClothes(6, 35, 0, 0);
                            break;
                        case "Leg":
                            if (!gender) player.SetClothes(4, 21, 0, 0);
                            else if (gender) player.SetClothes(4, 15, 0, 0);
                            break;
                        case "Top":
                            if (GetCharacterClothes(charId, "Top") == "None")
                            {
                                player.SetClothes(3, 15, 0, 0);
                                player.SetClothes(11, 15, 0, 0);
                            }
                            else
                            {
                                player.SetClothes(11, 15, 0, 0);
                                SetCharacterCorrectTorso(player, ServerClothes.ServerClothes_.First(x => x.clothesName == clothesName).draw);
                            }
                            break;
                        case "Torso":
                            SetCharacterCorrectTorso(player, ServerClothes.ServerClothes_.First(x => x.clothesName == clothesName).draw);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static void SetCharacterClothes(int charid, string type, string clothes)
        {
            if (charid <= 0) return;
            var chars = CharactersSkin.FirstOrDefault(p => p.charId == charid);
            if (chars != null)
            {
                switch (type)
                {
                    case "Top":
                        chars.clothesTop = clothes;
                        break;
                    case "Torso":
                        chars.clothesTorso = clothes;
                        break;
                    case "Leg":
                        chars.clothesLeg = clothes;
                        break;
                    case "Feet":
                        chars.clothesFeet = clothes;
                        break;
                    case "Hat":
                        chars.clothesHat = clothes;
                        break;
                    case "Glass":
                        chars.clothesGlass = clothes;
                        break;
                    case "Necklace":
                        chars.clothesNecklace = clothes;
                        break;
                    case "Mask":
                        chars.clothesMask = clothes;
                        break;
                    case "Armor":
                        chars.clothesArmor = clothes;
                        break;
                    case "Undershirt":
                        chars.clothesUndershirt = clothes;
                        break;
                    case "Decal":
                        chars.clothesDecal = clothes;
                        break;
                    case "Bracelet":
                        chars.clothesBracelet = clothes;
                        break;
                    case "Watch":
                        chars.clothesWatch = clothes;
                        break;
                    case "Earring":
                        chars.clothesEarring = clothes;
                        break;
                }

                try
                {
                    using (gtaContext db = new gtaContext())
                    {
                        db.Characters_Skin.Update(chars);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Alt.Log($"{e}");
                }
            }
        }

        public static bool IsCharacterUnconscious(int charId)
        {
            try
            {
                if (charId <= 0) return false;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.isUnconscious;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return false;
        }

        public static bool IsCharacterStabilisiert(int charId, bool isStabilisiert)
        {
            try
            {
                if (charId <= 0) return false;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null)
                {
                    chars.isStabilisiert = isStabilisiert;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return false;
        }

        public static bool GetCharacterStabilisiert(int charId)
        {
            try
            {
                if (charId <= 0) return false;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.isStabilisiert;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return false;
        }

        public static void SetCharacterUnconscious(int charId, bool isUnconscious, int unconsciousTime)
        {
            try
            {
                if (charId <= 0) return;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if(chars != null)
                {
                    chars.isUnconscious = isUnconscious;
                    chars.unconsciousTime = unconsciousTime;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static int GetCharacterUnconsciousTime(int charId)
        {
            try
            {
                if (charId <= 0) return 0;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.unconsciousTime;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return 0;
        }

        public static bool GetCharacterUnconscious(int charId)
        {
            try
            {
                if (charId <= 0) return false;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.isUnconscious;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return false;
        }

        public static bool IsCharacterFastFarm(int charId)
        {
            try
            {
                if (charId <= 0) return false;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.isFastFarm;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return false;
        }

        /// <summary>
        /// Setzt Character für eine bestimmbare Zeit auf doppelte Aufsammelmenge
        /// </summary>
        /// <param name="charId"></param>
        /// <param name="isFastFarm"></param>
        /// <param name="fastFarmTime"></param>
        public static void SetCharacterFastFarm(int charId, bool isFastFarm, int fastFarmTime)
        {
            try
            {
                if (charId <= 0) return;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null)
                {
                    chars.isFastFarm = isFastFarm;
                    chars.fastFarmTime = fastFarmTime;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static int GetCharacterFastFarmTime(int charId)
        {
            try
            {
                if (charId <= 0) return 0;
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) return chars.fastFarmTime;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return 0;
        }

        public static void SetCharacterNewName(int charId, string name)
        {
            try
            {
                if (charId == 0 || name == "") return;
                var chars = PlayerCharacters.FirstOrDefault(p => p.charId == charId);
                if (chars != null)
                {
                    chars.charname = name;
                    using (gtaContext db = new gtaContext())
                    {
                        db.AccountsCharacters.Update(chars);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e) { Alt.Log($"{e}"); }
        }

        public static bool IsCharacterPhoneAnonym(ClassicPlayer player)
        {
            try
            {
                var chars = PlayerCharacters.ToList().FirstOrDefault(x => x.charId == player.CharacterId);
                if (chars != null) return chars.isPhoneAnonym;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return false;
        }

        public static void SetCharacterPhoneAnonym(int charId, bool isEnabled)
        {
            try
            {
                var chars = PlayerCharacters.FirstOrDefault(x => x.charId == charId);
                if (chars != null) chars.isPhoneAnonym = isEnabled;
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static bool GetAdminMember()
        {
            try
            {
                foreach (var Admin in User.Player.Where(x => x != null && x.adminLevel > 1 && x.adminLevel < 10).ToList())
                {
                    if (Admin != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return false;
        }

        public static void SetCharacterHairs(ClassicPlayer player, int charId)
        {
            if (player == null || !player.Exists || charId <= 0) return;
            var chars = CharactersSkin.FirstOrDefault(p => p.charId == charId);
            if (chars != null)
            {
                float[] headoverlays1 = Array.ConvertAll(chars.headoverlays1.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));
                float[] headoverlays2 = Array.ConvertAll(chars.headoverlays2.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));
                float[] headoverlays3 = Array.ConvertAll(chars.headoverlays3.Split(';'), param => float.Parse(param, CultureInfo.InvariantCulture));
                player.HairColor = (byte)headoverlays3[13];
                player.HairHighlightColor = (byte)headoverlays2[13];
                player.SetClothes(2, (ushort)headoverlays1[13], 0, 2);
            }
        }
    }
}
