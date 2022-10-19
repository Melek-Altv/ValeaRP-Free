using AltV.Net;
using AltV.Net.Async;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using System;
using System.Collections.Generic;
using Altv_Roleplay.Utils;
using System.Linq;
using Newtonsoft.Json;
using System.Globalization;

namespace Altv_Roleplay.Handler
{
    class SmartphoneHandler : IScript
    {
        #region Anrufsystem
        [AsyncClientEvent("Server:Smartphone:tryCall")]
        public static void TryCall(ClassicPlayer player, int targetPhoneNumber)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || targetPhoneNumber <= 0 || !CharactersInventory.ExistCharacterItem(player.CharacterId, "Smartphone", "inventory") || CharactersInventory.GetCharacterItemAmount(player.CharacterId, "Smartphone", "inventory") <= 0 || !Characters.IsCharacterPhoneEquipped(player.CharacterId) || Characters.IsCharacterPhoneFlyModeEnabled(player.CharacterId) || Characters.GetCharacterPhonenumber(player.CharacterId) <= 0 || Characters.IsCharacterUnconscious(player.CharacterId) || player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs() || Characters.GetCharacterCurrentlyRecieveCaller(player.CharacterId) != 0 || Characters.GetCharacterPhoneTargetNumber(player.CharacterId) != 0) return;
                if (ServerFactions.IsNumberAFactionNumber(targetPhoneNumber))
                {
                    int factionId = ServerFactions.GetFactionIdByServiceNumber(targetPhoneNumber);
                    if (factionId <= 0) { 
                        player.EmitLocked("Client:Smartphone:ShowPhoneCallError", 1); 
                        return; 
                    }
                    int currentOwnerId = ServerFactions.GetCurrentServicePhoneOwner(factionId);
                    if (currentOwnerId <= 0) { 
                        player.EmitLocked("Client:Smartphone:ShowPhoneCallError", 2); 
                        return; 
                    }
                    ClassicPlayer targetServicePlayer = (ClassicPlayer)Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId == currentOwnerId);
                    if (targetServicePlayer == null || !targetServicePlayer.Exists || !Characters.IsCharacterPhoneEquipped(currentOwnerId) || Characters.IsCharacterPhoneFlyModeEnabled(currentOwnerId))
                    {
                        ServerFactions.sendMsg(factionId, "Da die Leitstelle nicht erreichbar war, wurde die Nummer zurückgesetzt. Jemand anderes sollte die Leitstelle nun übernehmen.");
                        ServerFactions.UpdateCurrentServicePhoneOwner(factionId, 0);
                        player.EmitLocked("Client:Smartphone:ShowPhoneCallError", 2);
                        return;
                    }
                    targetPhoneNumber = Characters.GetCharacterPhonenumber(currentOwnerId);
                    if (!Characters.ExistPhoneNumber(targetPhoneNumber))
                    {
                        ServerFactions.sendMsg(factionId, "Da die Leitstelle nicht erreichbar war, wurde die Nummer zurückgesetzt. Jemand anderes sollte die Leitstelle nun übernehmen."); ServerFactions.UpdateCurrent(factionId, 0); player.EmitLocked("Client:Smartphone:ShowPhoneCallError", 2); return;
                    }
                    if (Characters.GetCharacterPhoneTargetNumber(currentOwnerId) != 0 || Characters.GetCharacterCurrentlyRecieveCaller(currentOwnerId) != 0)
                    {
                        player.EmitLocked("Client:Smartphone:ShowPhoneCallError", 3);
                        return;
                    }
                    Characters.SetCharacterCurrentlyRecieveCallState(currentOwnerId, player.CharacterId);
                    Characters.SetCharacterCurrentlyRecieveCallState(player.CharacterId, currentOwnerId);
                    if (Characters.IsCharacterPhoneAnonym(player))
                    {
                        targetServicePlayer.EmitLocked("Client:Smartphone:showPhoneReceiveCall", 0);
                    }
                    else
                    {
                        targetServicePlayer.EmitLocked("Client:Smartphone:showPhoneReceiveCall", Characters.GetCharacterPhonenumber(player.CharacterId));
                    }
                    return;
                }

                if (!Characters.ExistPhoneNumber(targetPhoneNumber))
                {
                    player.EmitLocked("Client:Smartphone:ShowPhoneCallError", 1);
                    return;;
                }

                ClassicPlayer targetPlayer = (ClassicPlayer)Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId > 0 && Characters.GetCharacterPhonenumber(((ClassicPlayer)x).CharacterId) == targetPhoneNumber);
                if (targetPlayer == null || !targetPlayer.Exists || !Characters.IsCharacterPhoneEquipped(targetPlayer.CharacterId) || Characters.IsCharacterPhoneFlyModeEnabled(targetPlayer.CharacterId))
                {
                    var target = Characters.PlayerCharacters.ToList().FirstOrDefault(x => x.phonenumber == targetPhoneNumber);
                    // Angerufener Spieler hat kein Handy oder ist im Flugmodus, ist aber Online.
                    player.EmitLocked("Client:Smartphone:ShowPhoneCallError", 2);
                    if (target == null) return;
                    if (Characters.IsCharacterPhoneAnonym(player))
                    {
                        AnruflistenHandler.AddCallHistory(target.charId, 0, "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                        AnruflistenHandler.AddCallHistory(player.CharacterId, targetPhoneNumber, "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                    }
                    else 
                    {
                        AnruflistenHandler.AddCallHistory(target.charId, Characters.GetCharacterPhonenumber(player.CharacterId), "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                        AnruflistenHandler.AddCallHistory(player.CharacterId, targetPhoneNumber, "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                    }
                    if (targetPlayer != null && targetPlayer.Exists)
                        AnruflistenHandler.RefreshList(targetPlayer);
                    return;
                }

                if (Characters.GetCharacterPhoneTargetNumber(targetPlayer.CharacterId) != 0 || Characters.GetCharacterCurrentlyRecieveCaller(targetPlayer.CharacterId) != 0)
                {
                    player.EmitLocked("Client:Smartphone:ShowPhoneCallError", 3);
                    if (Characters.IsCharacterPhoneAnonym(player))
                    {
                        AnruflistenHandler.AddCallHistory(targetPlayer.CharacterId, 0, "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                        AnruflistenHandler.AddCallHistory(player.CharacterId, targetPhoneNumber, "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                    }
                    else
                    {
                        AnruflistenHandler.AddCallHistory(targetPlayer.CharacterId, Characters.GetCharacterPhonenumber(player.CharacterId), "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                        AnruflistenHandler.AddCallHistory(player.CharacterId, targetPhoneNumber, "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                    }
                    AnruflistenHandler.RefreshList(player);
                    return;
                }

                Characters.SetCharacterCurrentlyRecieveCallState(targetPlayer.CharacterId, player.CharacterId);
                Characters.SetCharacterCurrentlyRecieveCallState(player.CharacterId, targetPlayer.CharacterId);
                if (Characters.IsCharacterPhoneAnonym(player))
                {
                    targetPlayer.EmitLocked("Client:Smartphone:showPhoneReceiveCall", 0);
                }
                else
                {
                    targetPlayer.EmitLocked("Client:Smartphone:showPhoneReceiveCall", Characters.GetCharacterPhonenumber(player.CharacterId));
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:acceptCall")]
        public static void AcceptCall(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                if (Characters.GetCharacterCurrentlyRecieveCaller(player.CharacterId) <= 0) return;
                var callerId = Characters.GetCharacterCurrentlyRecieveCaller(player.CharacterId);
                ClassicPlayer caller = (ClassicPlayer)Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId == callerId);
                if (caller == null || !caller.Exists) return;
                Characters.SetCharacterCurrentlyRecieveCallState(caller.CharacterId, 0);
                Characters.SetCharacterCurrentlyRecieveCallState(player.CharacterId, 0);
                Characters.SetCharacterTargetPhoneNumber(caller.CharacterId, Characters.GetCharacterPhonenumber(player.CharacterId));
                Characters.SetCharacterTargetPhoneNumber(player.CharacterId, Characters.GetCharacterPhonenumber(caller.CharacterId));
                caller.EmitLocked("Client:Smartphone:showPhoneCallActive", Characters.GetCharacterPhonenumber(player.CharacterId));
                player.EmitLocked("Client:Smartphone:showPhoneCallActive", Characters.GetCharacterPhonenumber(caller.CharacterId));
                Alt.Emit("SaltyChat:StartCall", caller, player);

                if (Characters.IsCharacterPhoneAnonym(caller))
                {
                    AnruflistenHandler.AddCallHistory(player.CharacterId, 0, "accepted", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                    AnruflistenHandler.AddCallHistory(callerId, Characters.GetCharacterPhonenumber(player.CharacterId), "accepted", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                }
                else
                {
                    AnruflistenHandler.AddCallHistory(player.CharacterId, Characters.GetCharacterPhonenumber(callerId), "accepted", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                    AnruflistenHandler.AddCallHistory(callerId, Characters.GetCharacterPhonenumber(player.CharacterId), "accepted", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                }
                AnruflistenHandler.RefreshList(player);
                AnruflistenHandler.RefreshList(caller);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [ScriptEvent(ScriptEventType.PlayerDisconnect)]
        public static void PlayerDisconnect(ClassicPlayer player, string reason)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                DenyCall(player);
                Characters.SetCharacterCurrentFunkFrequence(player.CharacterId, null);
                int factionId = ServerFactions.GetCharacterFactionId(player.CharacterId);
                if (ServerFactions.GetCurrentServicePhoneOwner(factionId) == player.CharacterId)
                {
                    ServerFactions.UpdateCurrentServicePhoneOwner(factionId, 0);
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:denyCall")]
        public static void DenyCall(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                if (Characters.GetCharacterCurrentlyRecieveCaller(player.CharacterId) != 0)
                {
                    var callerId = Characters.GetCharacterCurrentlyRecieveCaller(player.CharacterId);
                    ClassicPlayer targetPlayer = (ClassicPlayer)Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId == callerId);
                    if(targetPlayer != null && targetPlayer.Exists && targetPlayer.CharacterId > 0)
                    {
                        Characters.SetCharacterTargetPhoneNumber(targetPlayer.CharacterId, 0);
                        Characters.SetCharacterCurrentlyRecieveCallState(targetPlayer.CharacterId, 0);
                        targetPlayer.EmitLocked("Client:Smartphone:ShowPhoneCallError", 4);
                    }

                    Characters.SetCharacterCurrentlyRecieveCallState(player.CharacterId, 0);
                    Characters.SetCharacterTargetPhoneNumber(player.CharacterId, 0);
                    player.EmitLocked("Client:Smartphone:ShowPhoneCallError", 4);
                    if (Characters.IsCharacterPhoneAnonym(targetPlayer))
                    {
                        AnruflistenHandler.AddCallHistory(player.CharacterId, 0, "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                        AnruflistenHandler.AddCallHistory(callerId, Characters.GetCharacterPhonenumber(player.CharacterId), "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                        AnruflistenHandler.RefreshList(player);
                        if (targetPlayer != null && targetPlayer.Exists)
                            AnruflistenHandler.RefreshList(targetPlayer);
                        return;
                    }
                    else if (Characters.IsCharacterPhoneAnonym(player))
                    {
                        AnruflistenHandler.AddCallHistory(player.CharacterId, Characters.GetCharacterPhonenumber(callerId), "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                        AnruflistenHandler.AddCallHistory(callerId, 0, "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                        AnruflistenHandler.RefreshList(player);
                        if (targetPlayer != null && targetPlayer.Exists)
                            AnruflistenHandler.RefreshList(targetPlayer);
                        return;
                    }
                    else
                    {
                        AnruflistenHandler.AddCallHistory(player.CharacterId, Characters.GetCharacterPhonenumber(callerId), "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                        AnruflistenHandler.AddCallHistory(callerId, Characters.GetCharacterPhonenumber(player.CharacterId), "declined", DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));
                        AnruflistenHandler.RefreshList(player);
                        if (targetPlayer != null && targetPlayer.Exists)
                            AnruflistenHandler.RefreshList(targetPlayer);
                        return;
                    }
                }

                if (Characters.GetCharacterPhoneTargetNumber(player.CharacterId) != 0)
                {
                    var phoneNumber = Characters.GetCharacterPhoneTargetNumber(player.CharacterId);
                    if (!Characters.ExistPhoneNumber(phoneNumber)) return;
                    ClassicPlayer targetPlayer = (ClassicPlayer)Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId > 0 && Characters.GetCharacterPhonenumber(((ClassicPlayer)x).CharacterId) == phoneNumber && Characters.GetCharacterPhoneTargetNumber(((ClassicPlayer)x).CharacterId) == Characters.GetCharacterPhonenumber(player.CharacterId));
                    if(targetPlayer != null && targetPlayer.Exists && targetPlayer.CharacterId > 0)
                    {
                        Characters.SetCharacterTargetPhoneNumber(targetPlayer.CharacterId, 0);
                        Characters.SetCharacterCurrentlyRecieveCallState(targetPlayer.CharacterId, 0);
                        targetPlayer.EmitLocked("Client:Smartphone:ShowPhoneCallError", 4);
                        AnruflistenHandler.RefreshList(targetPlayer);
                    }

                    Characters.SetCharacterTargetPhoneNumber(player.CharacterId, 0);
                    Characters.SetCharacterCurrentlyRecieveCallState(player.CharacterId, 0);
                    Alt.Emit("SaltyChat:EndCall", targetPlayer, player);
                    player.EmitLocked("Client:Smartphone:ShowPhoneCallError", 4);
                    AnruflistenHandler.RefreshList(player);
                    return;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:setWallpaperId")]
        public static void SetWallpaperId(ClassicPlayer player, int wallpaperId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;

                Characters.SetCharacterPhoneWallpaper(player.CharacterId, wallpaperId);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:setAnonymCallEnabled")]
        public static void SetAnonymCall(ClassicPlayer player, bool isEnabled)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                Characters.SetCharacterPhoneAnonym(player.CharacterId, isEnabled);
            }
            catch (Exception ex)
            {
                Alt.Log($"{ex}");
            }
        }
        #endregion

        #region SMS System
        [AsyncClientEvent("Server:Smartphone:requestChats")]
        public static void RequestSmartphoneChats(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || Characters.IsCharacterPhoneFlyModeEnabled(player.CharacterId) || Characters.IsCharacterUnconscious(player.CharacterId) || player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) return;
                int playerNumber = Characters.GetCharacterPhonenumber(player.CharacterId);
                if (!Characters.ExistPhoneNumber(playerNumber)) return;
                CharactersPhone.RequestChatJSON(player, playerNumber);
            }
            catch(Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:requestChatMessages")]
        public static void RequestChatMessages(ClassicPlayer player, int chatId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || chatId <= 0) return;
                var messages = CharactersPhone.CharactersPhoneChatMessages_.ToList().Where(x => x.chatId == chatId).Select(x => new
                {
                    x.id,
                    x.chatId,
                    from = x.fromNumber,
                    to = x.toNumber,
                    x.unix,
                    text = x.message,
                }).OrderBy(x => x.unix).TakeLast(50).ToList();

                var itemCount = (int)messages.Count;
                var iterations = Math.Floor((decimal)(itemCount / 5));
                var rest = itemCount % 5;
                for(var i = 0; i < iterations; i++)
                {
                    var skip = i * 5;
                    player.EmitLocked("Client:Smartphone:addMessageJSON", JsonConvert.SerializeObject(messages.Skip(skip).Take(5).ToList()));
                }
                if (rest != 0) player.EmitLocked("Client:Smartphone:addMessageJSON", JsonConvert.SerializeObject(messages.Skip((int)iterations * 5).ToList()));
                player.EmitLocked("Client:Smartphone:setAllMessages");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:createNewChat")]
        public static void CreateNewChat(ClassicPlayer player, int targetPhoneNumber)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || targetPhoneNumber <= 0) return;
                int phoneNumber = Characters.GetCharacterPhonenumber(player.CharacterId);
                if (!Characters.ExistPhoneNumber(phoneNumber) || !Characters.ExistPhoneNumber(targetPhoneNumber) || CharactersPhone.ExistChatByNumbers(phoneNumber, targetPhoneNumber) || phoneNumber == targetPhoneNumber) return;
                CharactersPhone.CreatePhoneChat(phoneNumber, targetPhoneNumber);
                CharactersPhone.RequestChatJSON(player, phoneNumber);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:sendChatMessage")]
        public static void SendChatMessage(ClassicPlayer player, int chatId, int phoneNumber, int targetPhoneNumber, int unix, string message)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || chatId <= 0 || phoneNumber <= 0 || targetPhoneNumber <= 0 || phoneNumber != Characters.GetCharacterPhonenumber(player.CharacterId) || !Characters.ExistPhoneNumber(targetPhoneNumber) || !CharactersPhone.ExistChatByNumbers(phoneNumber, targetPhoneNumber)) return;
                CharactersPhone.CreatePhoneChatMessage(chatId, phoneNumber, targetPhoneNumber, unix, message);
                RequestChatMessages(player, chatId);

                ClassicPlayer targetPlayer = (ClassicPlayer)Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId > 0 && Characters.GetCharacterPhonenumber(((ClassicPlayer)x).CharacterId) == targetPhoneNumber);
                if (targetPlayer == null || !targetPlayer.Exists) return;
                targetPlayer.EmitLocked("Client:Smartphone:recieveNewMessage", chatId, phoneNumber, message);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:deleteChat")]
        public static void DeleteChat(ClassicPlayer player, int chatId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || chatId <= 0 || !CharactersPhone.ExistChatById(chatId)) return;
                CharactersPhone.DeletePhoneChat(chatId);
                RequestSmartphoneChats(player);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
        #endregion

        #region Kontaktesystem
        [AsyncClientEvent("Server:Smartphone:requestPhoneContacts")]
        public static void RequestPhoneContacts(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                int phoneNumber = Characters.GetCharacterPhonenumber(player.CharacterId);
                var contacts = CharactersPhone.CharactersPhoneContacts_.ToList().Where(x => x.phoneNumber == phoneNumber).Select(x => new
                {
                    x.contactId,
                    x.contactName,
                    x.contactNumber,
                }).OrderBy(x => x.contactName).ToList();
                var itemCount = (int)contacts.Count;
                var iterations = Math.Floor((decimal)(itemCount / 10));
                var rest = itemCount % 10;
                for(var i = 0; i < iterations; i++)
                {
                    var skip = i * 10;
                    player.EmitLocked("Client:Smartphone:addContactJSON", JsonConvert.SerializeObject(contacts.Skip(skip).Take(10).ToList()));
                }
                if (rest != 0) player.EmitLocked("Client:Smartphone:addContactJSON", JsonConvert.SerializeObject(contacts.Skip((int)iterations * 10).ToList()));
                player.EmitLocked("Client:Smartphone:setAllContacts");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:editContact")]
        public static void EditContact(ClassicPlayer player, int contactId, string name, int contactNumber)
        {
            if (player == null || !player.Exists || player.CharacterId <= 0 || contactId <= 0 || name == "" || contactNumber <= 0) return;
            int phoneNumber = Characters.GetCharacterPhonenumber(player.CharacterId);
            if (phoneNumber <= 0) return;
            if(!CharactersPhone.ExistContactById(contactId, phoneNumber))
            {
                player.EmitLocked("Client:Smartphone:showNotification", "Kontakt konnte nicht bearbeitet werden.", "error", null, "error");
                return;
            }
            CharactersPhone.EditContact(contactId, contactNumber, name);
            RequestPhoneContacts(player);
        }

        [AsyncClientEvent("Server:Smartphone:addNewContact")]
        public static void AddNewContact(ClassicPlayer player, string name, int contactNumber)
        {
            try
            {
                if (player == null || !player.Exists || contactNumber <= 0 || name == "" || player.CharacterId <= 0) return;
                int phoneNumber = Characters.GetCharacterPhonenumber(player.CharacterId);
                if (phoneNumber <= 0) return;
                if (CharactersPhone.ExistContactByName(phoneNumber, name) || CharactersPhone.ExistContactByNumber(phoneNumber, contactNumber))
                {
                    player.EmitLocked("Client:Smartphone:showNotification", "Kontakt konnte nicht gespeichert werden.", "error", null, "error");
                    return;
                }
                CharactersPhone.CreatePhoneContact(phoneNumber, name, contactNumber);
                RequestPhoneContacts(player);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:deleteContact")]
        public static void DeleteContact(ClassicPlayer player, int contactId)
        {
            try
            {
                if (player == null || !player.Exists || contactId <= 0 || player.CharacterId <= 0) return;
                int phoneNumber = Characters.GetCharacterPhonenumber(player.CharacterId);
                if (phoneNumber <= 0 || !CharactersPhone.ExistContactById(contactId, phoneNumber)) return;
                CharactersPhone.DeletePhoneContact(contactId, phoneNumber);
                RequestPhoneContacts(player);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
        #endregion

        #region LSPD Intranet
        public static void RequestLSPDIntranet(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                if(!ServerFactions.IsCharacterInAnyFaction(player.CharacterId) || (ServerFactions.GetCharacterFactionId(player.CharacterId) != 2 && ServerFactions.GetCharacterFactionId(player.CharacterId) != 12) || !ServerFactions.IsCharacterInFactionDuty(player.CharacterId))
                {
                    player.EmitLocked("Client:Smartphone:ShowLSPDIntranetApp", false, "[]");
                    return;
                }

                string serverWanteds = JsonConvert.SerializeObject(CharactersWanteds.ServerWanteds_.Select(x => new
                {
                    x.wantedId,
                    x.wantedName,
                    x.paragraph,
                    x.category,
                    x.jailtime,
                    x.ticketfine,
                }).OrderBy(x => x.paragraph).ToList());
                player.EmitLocked("Client:Smartphone:ShowLSPDIntranetApp", true, serverWanteds);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:SearchLSPDIntranetPeople")]
        public static void SearchLSPDIntranetPeople(ClassicPlayer player, string name)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || string.IsNullOrWhiteSpace(name) || !ServerFactions.IsCharacterInAnyFaction(player.CharacterId) || !ServerFactions.IsCharacterInFactionDuty(player.CharacterId) || (ServerFactions.GetCharacterFactionId(player.CharacterId) != 2 && ServerFactions.GetCharacterFactionId(player.CharacterId) != 12)) return;
                var containedPlayers = Characters.PlayerCharacters.ToList().Where(x => x.charname.ToLower().Contains(name.ToLower()) && User.IsCharacterOnline(x.charId)).Select(x => new
                {
                    x.charId,
                    x.charname,
                }).OrderBy(x => x.charname).Take(15).ToList();

                player.EmitLocked("Client:Smartphone:SetLSPDIntranetSearchedPeople", JsonConvert.SerializeObject(containedPlayers));
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:GiveLSPDIntranetWanteds")]
        public static void GiveLSPDIntranetWanteds(ClassicPlayer player, int selectedCharId, string wanteds)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || selectedCharId <= 0 || string.IsNullOrWhiteSpace(wanteds) || !ServerFactions.IsCharacterInAnyFaction(player.CharacterId) || !ServerFactions.IsCharacterInFactionDuty(player.CharacterId) || (ServerFactions.GetCharacterFactionId(player.CharacterId) != 2 && ServerFactions.GetCharacterFactionId(player.CharacterId) != 12)) return;
                List<int> decompiledWanteds = JsonConvert.DeserializeObject<List<int>>(wanteds);
                if (decompiledWanteds == null) return;
                ClassicPlayer targetPlayer = (ClassicPlayer)Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId == selectedCharId);
                if(targetPlayer == null || !targetPlayer.Exists)
                {
                    HUDHandler.SendNotification(player, 3, 7000, $"Der Spieler ist nicht online ({selectedCharId})");
                    return;
                }

                string givenString = $"{DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE"))} {DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("de-DE"))} von {Characters.GetCharacterName(player.CharacterId)}.";

                CharactersWanteds.CreateCharacterWantedEntry(selectedCharId, givenString, decompiledWanteds);
                RequestLSPDIntranetPersonWanteds(player, selectedCharId);

                foreach(ClassicPlayer policeMember in Alt.GetAllPlayers().ToList().Where(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId > 0 && ServerFactions.IsCharacterInAnyFaction(((ClassicPlayer)x).CharacterId) && (ServerFactions.GetCharacterFactionId(((ClassicPlayer)x).CharacterId) == 2 || ServerFactions.GetCharacterFactionId(((ClassicPlayer)x).CharacterId) == 12))) {
                    HUDHandler.SendNotification(policeMember, 1, 3000, $"{Characters.GetCharacterName(player.CharacterId)} hat die Akte von {Characters.GetCharacterName(selectedCharId)} bearbeitet.");
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:requestLSPDIntranetPersonWanteds")]
        public static void RequestLSPDIntranetPersonWanteds(ClassicPlayer player, int selectedCharId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || selectedCharId <= 0 || !ServerFactions.IsCharacterInAnyFaction(player.CharacterId) || !ServerFactions.IsCharacterInFactionDuty(player.CharacterId) || (ServerFactions.GetCharacterFactionId(player.CharacterId) != 1 && ServerFactions.GetCharacterFactionId(player.CharacterId) != 2)) return;
                string wantedList = JsonConvert.SerializeObject(CharactersWanteds.CharactersWanteds_.Where(x => x.charId == selectedCharId).Select(x => new
                {
                    x.id,
                    x.wantedId,
                    x.givenString,
                }).ToList());

                player.EmitLocked("Client:Smartphone:setLSPDIntranetPersonWanteds", wantedList);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:requestPoliceAppMostWanteds")]
        public static void RequestPoliceAppMostWanteds(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || !ServerFactions.IsCharacterInAnyFaction(player.CharacterId) || !ServerFactions.IsCharacterInFactionDuty(player.CharacterId) || (ServerFactions.GetCharacterFactionId(player.CharacterId) != 2 && ServerFactions.GetCharacterFactionId(player.CharacterId) != 12)) return;
                if (ServerFactions.GetCharacterFactionId(player.CharacterId) == 2 && ServerFactions.GetCharacterFactionRank(player.CharacterId) < 6) { HUDHandler.SendNotification(player, 3, 2500, "Keine Berechtigung: ab Rang 6."); return; }
                string mostWantedList = JsonConvert.SerializeObject(Characters.PlayerCharacters.ToList().Where(x => User.IsCharacterOnline(x.charId) && CharactersWanteds.HasCharacterWanteds(x.charId) && Characters.IsCharacterPhoneEquipped(x.charId)).Select(x => new
                {
                    description = $"{x.charname} - {CharactersWanteds.GetCharacterWantedFinalJailTime(x.charId)} Hafteinheiten",
                    posX = $"{Characters.GetCharacterLastPosition(x.charId).X}",
                    posY = $"{Characters.GetCharacterLastPosition(x.charId).Y}",
                }).OrderBy(x => x.description).ToList());
                player.EmitLocked("Client:Smartphone:setPoliceAppMostWanteds", mostWantedList);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:Smartphone:DeleteLSPDIntranetWanted")]
        public static void DeleteLSPDIntranetWanted(ClassicPlayer player, int dbId, int selectedCharId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || dbId <= 0 || !CharactersWanteds.ExistWantedEntry(dbId) || !ServerFactions.IsCharacterInAnyFaction(player.CharacterId) || !ServerFactions.IsCharacterInFactionDuty(player.CharacterId) || (ServerFactions.GetCharacterFactionId(player.CharacterId) != 1 && ServerFactions.GetCharacterFactionId(player.CharacterId) != 2)) return;
                CharactersWanteds.RemoveWantedEntry(dbId);
                RequestLSPDIntranetPersonWanteds(player, selectedCharId);

                foreach (ClassicPlayer policeMember in Alt.GetAllPlayers().ToList().Where(x => x != null && x.Exists && ((ClassicPlayer)x).CharacterId > 0 && ServerFactions.IsCharacterInAnyFaction(((ClassicPlayer)x).CharacterId) && (ServerFactions.GetCharacterFactionId(((ClassicPlayer)x).CharacterId) == 1 || ServerFactions.GetCharacterFactionId(((ClassicPlayer)x).CharacterId) == 2)))
                {
                    HUDHandler.SendNotification(policeMember, 1, 3000, $"{Characters.GetCharacterName(player.CharacterId)} hat die Akte von {Characters.GetCharacterName(selectedCharId)} bearbeitet.");
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
        #endregion

        #region Funksystem
        [AsyncClientEvent("Server:Smartphone:joinRadioFrequence")]
        public static void JoinRadioFrequence(ClassicPlayer player, string radioFrequence)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || string.IsNullOrWhiteSpace(radioFrequence)) return;
                if (radioFrequence == "450000" || radioFrequence == "450100" || radioFrequence == "450200" || radioFrequence == "450300")
                {
                    if (!ServerFactions.IsCharacterInFactionDuty(player.CharacterId))
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Dieser Funk ist Verschlüsselt!");
                        Characters.SetCharacterCurrentFunkFrequence(player.CharacterId, null);
                        player.EmitLocked("Client:Smartphone:setCurrentFunkFrequence", "null");
                        Alt.Emit("SaltyChat:LeaveRadioChannel", player, radioFrequence);
                        return; 
                    }
                    if (!ServerFactions.IsCharacterInAnyFaction(player.CharacterId) || (ServerFactions.GetCharacterFactionId(player.CharacterId) != 2 && ServerFactions.GetCharacterFactionId(player.CharacterId) != 12) || !ServerFactions.IsCharacterInFactionDuty(player.CharacterId)) 
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Dieser Funk ist Verschlüsselt!");
                        Characters.SetCharacterCurrentFunkFrequence(player.CharacterId, null);
                        player.EmitLocked("Client:Smartphone:setCurrentFunkFrequence", "null");
                        Alt.Emit("SaltyChat:LeaveRadioChannel", player, radioFrequence);
                        return;
                    }
                }
                if (radioFrequence == "451000" || radioFrequence == "451100" || radioFrequence == "451200" || radioFrequence == "451300")
                {
                    if (!ServerFactions.IsCharacterInFactionDuty(player.CharacterId))
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Dieser Funk ist Verschlüsselt!");
                        Characters.SetCharacterCurrentFunkFrequence(player.CharacterId, null);
                        player.EmitLocked("Client:Smartphone:setCurrentFunkFrequence", "null");
                        Alt.Emit("SaltyChat:LeaveRadioChannel", player, radioFrequence);
                        return;
                    }
                    if (!ServerFactions.IsCharacterInAnyFaction(player.CharacterId) || (ServerFactions.GetCharacterFactionId(player.CharacterId) != 3 && ServerFactions.GetCharacterFactionId(player.CharacterId) != 4) || !ServerFactions.IsCharacterInFactionDuty(player.CharacterId))
                    {
                        HUDHandler.SendNotification(player, 4, 7000, "Dieser Funk ist Verschlüsselt!");
                        Characters.SetCharacterCurrentFunkFrequence(player.CharacterId, null);
                        player.EmitLocked("Client:Smartphone:setCurrentFunkFrequence", "null");
                        Alt.Emit("SaltyChat:LeaveRadioChannel", player, radioFrequence);
                        return;
                    }
                }

                if (Characters.GetCharacterCurrentFunkFrequence(player.CharacterId) != null)
                {
                    string currentFrequence = Characters.GetCharacterCurrentFunkFrequence(player.CharacterId);
                    Alt.Emit("SaltyChat:LeaveRadioChannel", player, currentFrequence);
                    player.EmitLocked("Client:Smartphone:setCurrentFunkFrequence", null);
                }

                Characters.SetCharacterCurrentFunkFrequence(player.CharacterId, radioFrequence);
                player.EmitLocked("Client:Smartphone:setCurrentFunkFrequence", radioFrequence);
                Alt.Emit("SaltyChat:JoinRadioChannel", player, radioFrequence, true);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [ServerEvent("Server:Smartphone:leaveRadioFrequence")]
        public static void LeaveRadioFrequenceServer(ClassicPlayer player)
        {
            LeaveRadioFrequence(player);
        }


        [AsyncClientEvent("Server:Smartphone:leaveRadioFrequence")]
        public static void LeaveRadioFrequence(ClassicPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || Characters.GetCharacterCurrentFunkFrequence(player.CharacterId) == null) return;
                string currentFrequence = Characters.GetCharacterCurrentFunkFrequence(player.CharacterId);
                Characters.SetCharacterCurrentFunkFrequence(player.CharacterId, null);
                player.EmitLocked("Client:Smartphone:setCurrentFunkFrequence", "null");
                Alt.Emit("SaltyChat:LeaveRadioChannel", player, currentFrequence);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }
        #endregion

        #region Utilities
        [AsyncClientEvent("Server:Smartphone:setFlyModeEnabled")]
        public static void SetFlyModeEnabled(ClassicPlayer player, bool isEnabled)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0) return;
                Characters.SetCharacterPhoneFlyModeEnabled(player.CharacterId, isEnabled);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
        #endregion
    }
}
 
 