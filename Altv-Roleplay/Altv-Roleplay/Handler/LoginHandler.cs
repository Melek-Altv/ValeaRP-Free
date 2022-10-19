using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Services;
using Altv_Roleplay.Utils;
using Newtonsoft.Json;

namespace Altv_Roleplay.Handler
{
    class LoginHandler : IScript
    {

        private string Key = "VEwSvcqG1N0SwcxayaBdSvnNbX4gbESh7lsnizVVTm5SLoHInQt54wS9lurd5Fr4EHKYhHfgJY6X3LVzQhaBr2ARXf2iedGRDOLVqi7eoRUWiV3dgH6R9cDe3siXiBht";
        private string RequestUrl = "https://DEINEDOMAIN.eu/WbbVerify.php";

        private static readonly HttpClient Client = new HttpClient();

        [ScriptEvent(ScriptEventType.PlayerConnect)]
        public static void OnPlayerConnect_Handler(ClassicPlayer player, string reason)
        {
            if (player == null || !player.Exists) return;
            player.SetSyncedMetaData("PLAYER_SPAWNED", false);
            player.SetSyncedMetaData("ADMINLEVEL", 0);
            player.SetPlayerIsCuffed("handcuffs", false);
            player.SetPlayerIsCuffed("ropecuffs", false);
            player.SetPlayerCurrentMinijob("None");
            player.SetPlayerCurrentMinijobRouteId(0);
            player.SetPlayerCurrentMinijobStep("None");
            player.SetPlayerCurrentMinijobActionCount(0);
            player.SetPlayerFarmingActionMeta("None");
            User.SetPlayerOnline(player, 0);
            player.EmitLocked("Client:Pedcreator:spawnPed", ServerPeds.GetAllServerPeds());
            CreateLoginBrowser(player);
        }

        [ScriptEvent(ScriptEventType.PlayerDisconnect)]
        public static void OnPlayerDisconnected_Handler(ClassicPlayer player, string reason)
        {
            try
            {
                if (player == null) return;
                int charId = User.GetPlayerOnline(player);
                if (User.GetPlayerOnline(player) != 0) Characters.SetCharacterLastPosition(User.GetPlayerOnline(player), player.Position, player.Dimension);
                User.SetPlayerOnline(player, 0);
                Characters.SetCharacterCurrentFunkFrequence(player.CharacterId, null);
                ServerFactions.SetFactionCharacterOnline(charId, false);
                var vehicle = ServerVehicles.ServerVehicles_.Where(x => x.id > 0 && x.charid == (int)player.GetCharacterMetaId() && x.plate == $"TEST-{charId}");
                if (vehicle != null)
                {
                    foreach (var veh in Alt.GetAllVehicles().Cast<ClassicVehicle>().Where(x => x != null && x.Exists && x.NumberplateText == $"TEST-{charId}").ToList())
                    {
                        ServerVehicles.RemoveVehiclePermanently(veh);
                        HUDHandler.SendNotification(player, 4, 5000, "Die Testzeit ist jetzt vorbei.");
                        player.SetPlayerCurrentTestVehicle("None");
                        veh.Remove();
                        return;
                    }
                }
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        public static void CreateLoginBrowser(ClassicPlayer client)
        {
            if (client == null || !client.Exists) return;
            lock (client)
            {
                client.Model = 0x3D843282;
                client.Dimension = 10000;
                client.Position = new Position(3120, 5349, 10);
                client.LastPosition = new Position(3120, 5349, 10);
            }
            client.EmitLocked("Client:Login:CreateCEF"); //Login triggern
        }

        public static async Task<LoginResponse> MakePostRequest(string requestUrl, string username, string password, string key)
        {
            var values = new Dictionary<string, string>
            {
                { "Username", username },
                { "Password", password },
                { "Key", key }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await Client.PostAsync(requestUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<LoginResponse>(responseString);
        }

        [AsyncClientEvent("Server:Login:ValidateLoginCredentials")]
        public void ValidateLoginCredentials(ClassicPlayer client, string username, string password)
        {
            LoginResponse loginInfo;
            if (client == null || !client.Exists) return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                client.EmitLocked("Client:Login:showError", "Eines der Felder wurde nicht ordnungsgemäß ausgefüllt.");
                return;
            }

            loginInfo = MakePostRequest(RequestUrl, username, password, Key).Result;

            switch (loginInfo.StatusCode)
            {
                case LoginStatusCode.Success:
                    if (loginInfo.UserData.Banned)
                    {
                        if (User.IsPlayerBanned(client))
                        {
                            client.EmitLocked("Client:Login:showError", "Du wurdest gebannt. Grund: {0}", loginInfo.UserData.BanReason);
                        }
                        else
                        {
                            User.SetPlayerBanned(client, true, loginInfo.UserData.BanReason);
                            client.EmitLocked("Client:Login:showError", "Du wurdest gebannt. Grund: {0}", loginInfo.UserData.BanReason);
                        }
                    }
                    else
                    {
                        if (User.IsPlayerBanned(client))
                        {
                            User.SetPlayerBanned(client, false, "");
                        }
                    }

                    if (!User.ExistPlayerName(username))
                    {
                        User.CreatePlayerAccount(client, username, loginInfo.UserData.Email, password);
                    }

                    client.Dimension = (short)User.GetPlayerAccountId(client);
                    client.EmitLocked("Client:Login:SaveLoginCredentialsToStorage", username, password);
                    User.SetPlayerOnline(client, 0);
                    SendDataToCharselectorArea(client);
                    LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, true, "Erfolgreich eingeloggt.");
                    stopwatch.Stop();
                    if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"ValidateLoginCredentials benötigte {stopwatch.Elapsed.Milliseconds}ms");
                    break;

                case LoginStatusCode.WrongPasswordUsername:
                    client.EmitLocked("Client:Login:showError", "Der Benutzername oder das Passwort stimmen nicht überein.");
                    LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Der Benutzername oder das Passwort stimmen nicht überein.");
                    break;

                case LoginStatusCode.DataMissing:
                    client.EmitLocked("Client:Login:showError", "Trage deinen Benutzernamen oder Passwort ein.");
                    LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Trage deinen Benutzernamen oder Passwort ein.");
                    new LoginResponse(LoginStatusCode.DataMissing, null);
                    break;

                case LoginStatusCode.KeyWrong:
                    client.EmitLocked("Client:Login:showError", "Der Login Service ist nicht erreichbar.");
                    LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Key-Error: WbbVerify");
                    new LoginResponse(LoginStatusCode.KeyWrong, null);
                    break;

                default:
                    new LoginResponse(LoginStatusCode.Error, null);
                    break;
            }

            if (!User.ExistPlayerName(username))
            {
                client.EmitLocked("Client:Login:showError", "Der eingegebene Benutzername wurde nicht gefunden.");
                LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Der eingegebene Benutzername wurde nicht gefunden ({username}).");
                return;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, User.GetPlayerPassword(username)))
            {
                client.EmitLocked("Client:Login:showError", "Das eingegebene Passwort ist falsch.");
                LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Das eingegebene Passwort ist falsch");
                return;
            }

            if (User.GetPlayerSocialclubId(username) != 0) { if (User.GetPlayerSocialclubId(username) != client.SocialClubId) { client.EmitLocked("Client:Login:showError", "Fehler bei der Anmeldung (Fehlercode 508)."); return; } }
            else { User.SetPlayerSocialID(client); }


            if (User.GetPlayerHardwareID(client) != 0) { if (User.GetPlayerHardwareID(client) != client.HardwareIdHash) { client.EmitLocked("Client:Login:showError", "Fehler bei der Anmeldung (Fehlercode 187)."); return; } }
            else { User.SetPlayerHardwareID(client); }

            if (User.IsPlayerBanned(client))
            {
                client.EmitLocked("Client:Login:showError", "Dieser Benutzeraccount wurde gebannt, im Support melden.");
                LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Dieser Benutzeraccount wurde gebannt, im Support melden ({username}).");
                return;
            }

            client.EmitLocked("Client:Login:SaveLoginCredentialsToStorage", username, password);
            User.SetPlayerOnline(client, 0);
            lock (client)
            {
                if (client == null || !client.Exists) return;
                client.accountId = (short)User.GetPlayerAccountId(client);
                client.Dimension = (short)User.GetPlayerAccountId(client);
            }
            SendDataToCharselectorArea(client);
            LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, true, "Erfolgreich eingeloggt.");
            stopwatch.Stop();
        }

        [AsyncClientEvent("Server:Charselector:PreviewCharacter")]
        public void PreviewCharacter(IPlayer client, int charid)
        {
            if (client == null || !client.Exists) return;
            client.EmitLocked("Client:Charselector:ViewCharacter", Characters.GetCharacterGender(charid), Characters.GetCharacterSkin("facefeatures", charid), Characters.GetCharacterSkin("headblendsdata", charid), Characters.GetCharacterSkin("headoverlays1", charid), Characters.GetCharacterSkin("headoverlays2", charid), Characters.GetCharacterSkin("headoverlays3", charid));
        }

        public static void SendDataToCharselectorArea(ClassicPlayer client)
        {
            if (client == null || !client.Exists || ((ClassicPlayer)client).accountId <= 0) return;
            var charArray = Characters.GetPlayerCharacters(client);
            int MaxCharacters = User.GetPlayerMaxCharacters(client);
            lock (client)
            {
                if (client == null || !client.Exists) return;
                client.Position = new Position((float)402.778, (float)-996.9758, (float)-98);
            }
            client.EmitLocked("Client:Charselector:sendCharactersToCEF", charArray, MaxCharacters);
            client.EmitLocked("Client:Login:showArea", "charselect");
        }

        [AsyncClientEvent("Server:Charselector:spawnChar")]
        public static async void CharacterSelectedSpawnPlace(ClassicPlayer client, string spawnstr, string charcid)
        {
            if (client == null || !client.Exists || spawnstr == null || charcid == null || client.accountId <= 0 || User.GetPlayerAccountId(client) <= 0) return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int charid = Convert.ToInt32(charcid);
            if (charid <= 0) return;
            if (User.GetPlayerAccountId(client) != Characters.GetCharacterAccountId(charid))
            {
                client.Kick("Login Fehler!");
                return;
            }
            string charName = Characters.GetCharacterName(charid);
            User.SetPlayerOnline(client, charid); //Online Feld = CharakterID
            lock (client)
            {
                if (client == null || !client.Exists) return;
                client.CharacterId = charid;
            }

            if (User.IsPlayerICWhitelisted(charid) == false)
            {
                foreach (var admin in Alt.GetAllPlayers().Where(x => x != null && x.Exists && x.AdminLevel() > 0 && x.AdminLevel() < 10))
                {
                    HUDHandler.SendNotification(admin, 1, 15000, $"[Einreise] {Characters.GetCharacterName(User.GetPlayerOnline(client))} (ID: {User.GetPlayerOnline(client)}) benötigt ein Visum!");
                }
            }

            if (Characters.GetCharacterFirstJoin(charid) && Characters.GetCharacterFirstSpawnPlace(client, charid) == "unset")
            {
                Characters.SetCharacterFirstSpawnPlace(client, charid, spawnstr);
                CharactersInventory.AddCharacterItem(charid, "Einreisegeschenk", 1, "inventory");

                CharactersInventory.AddCharacterItem(charid, "Tasche", 1, "inventory");
                Characters.SetCharacterBackpack(client, "Tasche");

                CharactersInventory.AddCharacterItem(charid, "Tablet", 1, "inventory");
                CharactersInventory.AddCharacterItem(charid, "Smartphone", 1, "inventory");

                CharactersInventory.AddCharacterItem(charid, "Cola", 20, "inventory");
                CharactersInventory.AddCharacterItem(charid, "Pizza", 20, "inventory");

                // First-Spawn Kleider#
                if (!Characters.GetCharacterGender((int)client.GetCharacterMetaId()))
                {
                    //Männlich
                    Characters.SwitchCharacterClothes(client, "Top", "DLC_MP_LOW2_M_JBIB_2_2");
                    Characters.SwitchCharacterClothes(client, "Leg", "DLC_MP_IE_M_LEGS_6_10");
                    Characters.SwitchCharacterClothes(client, "Feet", "DLC_MP_BIKER_M_FEET_1_2");
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, "DLC_MP_LOW2_M_JBIB_2_2");
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, "DLC_MP_IE_M_LEGS_6_10");
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, "DLC_MP_BIKER_M_FEET_1_2");
                }
                else
                {
                    //Weiblich
                    Characters.SwitchCharacterClothes(client, "Top", "DLC_MP_LOW_F_JBIB_5_0");
                    Characters.SwitchCharacterClothes(client, "Leg", "DLC_MP_X17_F_LEGS_4_2");
                    Characters.SwitchCharacterClothes(client, "Feet", "DLC_MP_HIPS_F_FEET0_32");
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, "DLC_MP_LOW_F_JBIB_5_0");
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, "DLC_MP_X17_F_LEGS_4_2");
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, "DLC_MP_HIPS_F_FEET0_3");
                }
                

                switch (spawnstr)
                {
                    case "lsairport":
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_Airport, 0);
                        break;
                    case "beach":
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_Beach, 0);
                        break;
                    case "sandyshores":
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_SandyShores, 0);
                        break;
                    case "paletobay":
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_PaletoBay, 0);
                        break;
                    case null:
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_Airport, 0);
                        break;
                }
            }

            lock (client)
            {
                if (client == null || !client.Exists) return;
                if (Characters.GetCharacterGender(charid)) client.Model = 0x9C9EFFD8;
                else client.Model = 0x705E61F2;
            }

            client.EmitLocked("Client:ServerBlips:LoadAllBlips", ServerBlips.GetAllServerBlips());
            client.EmitLocked("Client:ServerMarkers:LoadAllMarkers", ServerBlips.GetAllServerMarkers());
            Position dbPos = Characters.GetCharacterLastPosition(charid);
            lock (client)
            {
                if (client == null || !client.Exists) return;
                client.Position = dbPos;
                client.Spawn(dbPos, 0);
            }
            lock (client)
            {
                if (client == null || !client.Exists) return;
                client.LastPosition = new Position((float)402.778, (float)-996.9758, (float)-98);//by Sheytan
                client.Dimension = Characters.GetCharacterLastDimension(charid);
                client.Health = (ushort)(Characters.GetCharacterHealth(charid) + 100);
                client.Armor = (ushort)Characters.GetCharacterArmor(charid);
            }
            HUDHandler.CreateHUDBrowser(client); //HUD erstellen
            WeatherHandler.SetRealWeather(client);
            Characters.SetCharacterCorrectClothes(client, true);
            Characters.SetCharacterSkin(client);
            if (ServerFactions.IsCharacterInAnyFaction(charid) && ServerFactions.IsCharacterInFactionDuty(charid))
            {
                ServerFactions.SetCharactersFactionClothes(client, charid);
            }
            Characters.SetCharacterLastLogin(charid, DateTime.Now);
            Characters.SetCharacterCurrentFunkFrequence(charid, null);
            Alt.Log($"Eingeloggt {client.Name}");
            Alt.Emit("SaltyChat:EnablePlayer", client, (int)charid);
            client.EmitLocked("SaltyChat_OnConnected");
            client.SetSyncedMetaData("NAME", client.CharacterId + " | " + Characters.GetCharacterName((int)client.GetCharacterMetaId()));
            if (Characters.IsCharacterUnconscious(charid))
            {
                lock (client)
                {
                    if (client == null || !client.Exists) return;
                    client.Spawn(dbPos, 0);
                    client.Emit("Client:Ragdoll:SetPedToRagdoll", true, 0);
                    client.Emit("Client:Deathscreen:openCEF");
                    client.SetPlayerIsUnconscious(true);
                }

            }
            if (Characters.IsCharacterFastFarm(charid))
            {
                var fastFarmTime = Characters.GetCharacterFastFarmTime(charid) * 60000;
                client.EmitLocked("Client:Inventory:PlayEffect", "DrugsMichaelAliensFight", fastFarmTime);
                HUDHandler.SendNotification(client, 2, 7000, $"Du bist durch dein Koks noch {fastFarmTime} Minuten effektiver.");
            }
            ServerAnimations.RequestAnimationMenuContent(client);
            if (Characters.IsCharacterPhoneEquipped(charid) && CharactersInventory.ExistCharacterItem(charid, "Smartphone", "inventory") && CharactersInventory.GetCharacterItemAmount(charid, "Smartphone", "inventory") > 0)
            {
                client.EmitLocked("Client:Smartphone:equipPhone", true, Characters.GetCharacterPhonenumber(charid), Characters.IsCharacterPhoneFlyModeEnabled(charid), Characters.GetCharacterPhoneWallpaper(charid));
                Characters.SetCharacterPhoneEquipped(charid, true);
            }
            else if(!Characters.IsCharacterPhoneEquipped(charid) || !CharactersInventory.ExistCharacterItem(charid, "Smartphone", "inventory") || CharactersInventory.GetCharacterItemAmount(charid, "Smartphone", "inventory") <= 0) {
                client.EmitLocked("Client:Smartphone:equipPhone", false, Characters.GetCharacterPhonenumber(charid), Characters.IsCharacterPhoneFlyModeEnabled(charid), Characters.GetCharacterPhoneWallpaper(charid));
                Characters.SetCharacterPhoneEquipped(charid, false);
            }
            SmartphoneHandler.RequestLSPDIntranet(client);
            client.SetStreamSyncedMetaData("sharedUsername", $"{charName} ({Characters.GetCharacterAccountId(charid)})");
            client.SetSyncedMetaData("ADMINLEVEL", client.AdminLevel());
            client.SetSyncedMetaData("PLAYER_SPAWNED", true);

            if (Characters.IsCharacterInJail(charid))

            {
                HUDHandler.SendNotification(client, 1, 2500, $"Du befindest dich noch {Characters.GetCharacterJailTime(charid)} Minuten im Gefängnis.", 8000);
                lock (client)
                {
                    if (client == null || !client.Exists) return;//Das Raus Nehmen
                    client.Position = new Position(1691.4594f, 2565.7056f, 45.556763f);
                    client.LastPosition = client.Position;
                }
                if (Characters.GetCharacterGender(charid) == false)
                {
                    //Mann
                    client.SetClothes(11, 5, 0, 0);
                    client.SetClothes(3, 5, 0, 0);
                    client.SetClothes(4, 5, 7, 0);
                    client.SetClothes(6, 7, 0, 0);
                    client.SetClothes(8, 1, 88, 0);
                } else
                {
                    //Frau
                    client.SetClothes(11, 247, 0, 0);
                    client.SetClothes(4, 66, 6, 0);
                    client.SetClothes(3, 4, 0, 0);
                    client.SetClothes(8, 3, 0, 0);
                    client.SetClothes(6, 60, 9, 0);
                }
            }
            var Chardatas = Characters.PlayerCharacters.ToList().FirstOrDefault(x => x.charId == client.CharacterId);
            if (Chardatas.isPed)
            {
                if (Chardatas.PedHash != 0)
                {
                    client.Model = Chardatas.PedHash;
                }
            }
            ServerFactions.SetFactionCharacterOnline(client.CharacterId, true);
            client.updateTattoos();
            stopwatch.Stop();
            await Task.Delay(5000);
            ServerTattoos.GetAllTattoos(client);
        }
    }
}
