import * as alt from 'alt';
import * as game from 'natives';

let tabletBrowser = null;
let lastInteract = 0;
let tabletReady = false;

alt.on('keyup', (key) => {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (key == 0x73 && alt.Player.local.getMeta("IsCefOpen") == false && alt.Player.local.getSyncedMeta("HasHandcuffs") == false && alt.Player.local.getSyncedMeta("HasRopeCuffs") == false) {
        if (tabletBrowser == null) {
            alt.emitServer("Server:Tablet:openCEF");
        }
    }
	else if (key === 27 && tabletBrowser != null) {
        closeTabletCEF();
    }
});

function canInteract() { return lastInteract + 1000 < Date.now() }

alt.onServer('Client:Tablet:createCEF', () => {
    openTabletCEF();
    game.requestAnimDict("amb@world_human_seat_wall_tablet@female@base");
    let animInterval = alt.setInterval(() => {
        if (!game.hasAnimDictLoaded("amb@world_human_seat_wall_tablet@female@base")) return;
		game.taskPlayAnim(alt.Player.local.scriptID, "amb@world_human_seat_wall_tablet@female@base", "base", 1.0, -1, -1, 50, 0, false, false, false);
        alt.clearInterval(animInterval);
    }, 0);
});

alt.onServer('Client:Tablet:finaly', () => {
    if (tabletBrowser != null) {
        let interval = alt.setInterval(() => {
            if (tabletReady) {
                alt.clearInterval(interval);
                tabletBrowser.emit("CEF:Tablet:openCEF");
            }
        }, 0);
    }
});

alt.onServer("Client:Tablet:setTabletHomeAppData", (array) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetInternetAppAppStoreContent", array);
    }
});

alt.onServer("Client:Tablet:SetBankingAppContent", (bankArray, historyArray) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetBankingAppContent", bankArray, historyArray);
    }
});

alt.onServer("Client:Tablet:SetLifeinaderAds", (ads) => {
    if (tabletBrowser == null) return;
    tabletBrowser.emit("CEF:Tablet:SetLifeinvaderAds", ads);
});

alt.onServer("Client:Tablet:SetEventsAppContent", (array) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetEventsAppEventEntrys", array);
    }
});

alt.onServer("Client:Tablet:NotesAppAddNotesContent", (array) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:NotesAppAddNotesContent", array);
    }
});

alt.onServer("Client:Tablet:SetVehiclesAppContent", (array) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetVehiclesAppContent", array);
    }
});

alt.onServer("Client:Tablet:SetVehicleStoreAppContent", (array) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetVehicleStoreAppContent", array);
    }
});

alt.onServer("Client:Tablet:SetCompanyAppContent", (companyId, infoArray, memberArray) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetCompanyAppContent", companyId, infoArray, memberArray);
    }
});

alt.onServer("Client:Tablet:SetFactionManagerAppContent", (factionId, infoArray, memberArray, rankArray) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetFactionManagerAppContent", factionId, infoArray, memberArray, rankArray);
    }
});

alt.onServer("Client:Tablet:SetFactionAppContent", (dutyMemberCount, dispatchCount, vehicleArray) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetFactionAppContent", dutyMemberCount, dispatchCount, vehicleArray);
    }
});

alt.onServer("Client:Tablet:SetInternetAppVehicleSearchData", (Price, manufactur, category, trunkcapacity, maxfuel, fueltype, seats, tax) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetInternetAppVehicleSearchData", Price, manufactur, category, trunkcapacity, maxfuel, fueltype, seats, tax);
    }
});

alt.onServer("Client:Tablet:SetLSPDAppPersonSearchData", (charName, gender, birthdate, birthplace, address, job, mainBankAcc, firstJoinDate) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetLSPDAppPersonSearchData", charName, gender, birthdate, birthplace, address, job, mainBankAcc, firstJoinDate);
    }
});

alt.onServer("Client:Tablet:SetLSPDAppSearchVehiclePlateData", (owner, name, manufactor, buydate, trunk, maxfuel, tax, fueltype) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetLSPDAppSearchVehiclePlateData", owner, name, manufactor, buydate, trunk, maxfuel, tax, fueltype);
    }
});

alt.onServer("Client:Tablet:SetLSPDAppLocateVehiclePlateData", (x, y) => {
    game.setNewWaypoint(parseFloat(x), parseFloat(y));
});

alt.onServer("Client:Tablet:SetLSPDAppLicenseSearchData", (charName, licArray) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetLSPDAppLicenseSearchData", charName, licArray);
    }
});

alt.onServer("Client:Tablet:SetJusticeAppSearchedBankAccounts", (accountArray) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetJusticeAppSearchedBankAccounts", accountArray);
    }
});

alt.onServer("Client:Tablet:SetJusticeAppBankTransactions", (array) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetJusticeAppBankTransactions", array);
    }
});

alt.onServer("Client:Tablet:setDispatches", (factionId, dispatchArray) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetDispatches", factionId, dispatchArray);
    }
});

alt.onServer("Client:Tablet:SetTutorialAppContent", (array) => {
    if (tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:SetTutorialAppContent", array);
    }
});

alt.onServer("Client:Tablet:sendDispatchSound", (filePath) => {
    if(tabletBrowser != null) {
        tabletBrowser.emit("CEF:Tablet:playDispatchSound", filePath);
    }
})

alt.onServer('Client:Tablet:closeCEF', () => {
    closeTabletCEF();
});

let openTabletCEF = function() {
    if (tabletBrowser == null && alt.Player.local.getMeta("IsCefOpen") == false && alt.Player.local.getSyncedMeta("PLAYER_SPAWNED") == true) {
        alt.showCursor(true);
        alt.toggleGameControls(false);
        tabletBrowser = new alt.WebView("http://resource/client/cef/tablet/index.html");
        tabletBrowser.focus();
        alt.emit("Client:HUD:setCefStatus", true);
        alt.emitServer("Server:Tablet:Attachment", true);
        tabletBrowser.on("Client:Tablet:cefIsReady", () => {
            tabletReady = true;
            alt.emitServer("Server:Tablet:RequestTabletData");
        });

        tabletBrowser.on("Client:Tablet:AppStoreInstallUninstallApp", AppStoreInstallUninstallApp);
        tabletBrowser.on("Client:Tablet:BankingAppnewTransaction", BankingAppnewTransaction);
        tabletBrowser.on("Client:Tablet:EventsAppNewEntry", EventsAppNewEntry);
        tabletBrowser.on("Client:Tablet:NotesAppNewNote", NotesAppNewNote);
        tabletBrowser.on("Client:Tablet:NotesAppDeleteNote", NotesAppDeleteNote);
        tabletBrowser.on("Client:Tablet:LocateVehicle", LocateTabletVehicle);
        tabletBrowser.on("Client:Tablet:VehicleStoreBuyVehicle", VehicleStoreBuyVehicle);
        tabletBrowser.on("Client:Tablet:CompanyAppInviteNewMember", CompanyAppInviteNewMember);
        tabletBrowser.on("Client:Tablet:CompanyAppLeaveCompany", CompanyAppLeaveCompany);
        tabletBrowser.on("Client:Tablet:CompanyAppRankAction", CompanyAppRankAction);
        tabletBrowser.on("Client:Tablet:FactionManagerAppInviteNewMember", FactionManagerAppInviteNewMember);
        tabletBrowser.on("Client:Tablet:FactionManagerRankAction", FactionManagerRankAction);
        tabletBrowser.on("Client:Tablet:FactionManagerSetRankPaycheck", FactionManagerSetRankPaycheck);
        tabletBrowser.on("Client:Tablet:InternetAppSearchVehicle", InternetAppSearchVehicle);
        tabletBrowser.on("Client:Tablet:LSPDAppSearchPerson", LSPDAppSearchPerson);
        tabletBrowser.on("Client:Tablet:LSPDAppSearchVehiclePlate", LSPDAppSearchVehiclePlate);
        tabletBrowser.on("Client:Tablet:LSPDAppLocateVehiclePlate", LSPDAppLocateVehiclePlate);
        tabletBrowser.on("Client:Tablet:LSPDAppSearchLicense", LSPDAppSearchLicense);
        tabletBrowser.on("Client:Tablet:DMVAppSearchLicense", DMVAppSearchLicense);
        tabletBrowser.on("Client:Tablet:LSPDAppTakeLicense", LSPDAppTakeLicense);
        tabletBrowser.on("Client:Tablet:JusticeAppGiveWeaponLicense", JusticeAppGiveWeaponLicense);
        tabletBrowser.on("Client:Tablet:JusticeAppSearchBankAccounts", JusticeAppSearchBankAccounts);
        tabletBrowser.on("Client:Tablet:JusticeAppViewBankTransactions", JusticeAppViewBankTransactions);
        tabletBrowser.on("Client:Tablet:sendDispatchToFaction", sendDispatchToFaction);
        tabletBrowser.on("Client:Tablet:DeleteFactionDispatch", DeleteFactionDispatch);
    }
}

function DeleteFactionDispatch(factionId, senderId) {
    if (factionId <= 0 || senderId < 0) return;
    if (!canInteract) return;
    lastInteract = Date.now();
    alt.emitServer("Server:Tablet:DeleteFactionDispatch", parseInt(factionId), parseInt(senderId));
}

function AppStoreInstallUninstallApp(action, appname) {
    if (action != "install" && action != "uninstall") return;
    if (appname == "" || appname == "undefined") return;
    if (!canInteract) return;
    lastInteract = Date.now();
    let isInstalling = false;
    if (action == "install") { isInstalling = true; } else if (action == "uninstall") { isInstalling = false; }
    alt.emitServer("Server:Tablet:AppStoreInstallUninstallApp", appname, isInstalling);
}

function BankingAppnewTransaction(targetBankNumber, transactiontext, moneyAmount) {
    if (!canInteract) return;
    lastInteract = Date.now();
    alt.emitServer("Server:Tablet:BankingAppNewTransaction", parseInt(targetBankNumber), transactiontext, parseInt(moneyAmount));
}

function EventsAppNewEntry(title, callNumber, eventDate, Time, location, eventType, information) {
    if (!canInteract) return;
    lastInteract = Date.now();
    alt.emitServer("Server:Tablet:EventsAppNewEntry", title, callNumber, eventDate, Time, location, eventType, information);
}

function NotesAppNewNote(title, text, color) {
    if (!canInteract) return;
    lastInteract = Date.now();
    alt.emitServer("Server:Tablet:NotesAppNewNote", title, text, color);
}

function NotesAppDeleteNote(noteId) {
    if (!canInteract) return;
    lastInteract = Date.now();
    alt.emitServer("Server:Tablet:NotesAppDeleteNote", parseInt(noteId));
}

function LocateTabletVehicle(x, y) {
    if (x == null || y == null || x == undefined || y == undefined) return;
    game.setNewWaypoint(x, y);
    alt.emit("Client:HUD:sendNotification", 1, 7000, "Das Fahrzeug wurde dir auf der Karte markiert.");
	closeTabletCEF();
}

function VehicleStoreBuyVehicle(hash, shopId, color) {
    if (!canInteract) return;
    lastInteract = Date.now();
    alt.emitServer("Server:Tablet:VehicleStoreBuyVehicle", hash, parseInt(shopId), color);
}

function CompanyAppInviteNewMember(charName, companyId) {
    if (!canInteract) return;
    lastInteract = Date.now();
    alt.emitServer("Server:Tablet:CompanyAppInviteNewMember", charName, parseInt(companyId));
}

function CompanyAppLeaveCompany() {
    if (!canInteract) return;
    lastInteract = Date.now();
    alt.emitServer("Server:Tablet:CompanyAppLeaveCompany");
}

function CompanyAppRankAction(rankId, charId) {
    if (charId <= 0 || charId == undefined || charId == null) return;
    alt.emitServer("Server:Tablet:CompanyAppRankAction", parseInt(rankId), parseInt(charId));
}

function FactionManagerAppInviteNewMember(charName, dienstnummer, factionId) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (charName == "" || dienstnummer <= 0 || factionId <= 0 || dienstnummer == null || dienstnummer == undefined || factionId == undefined || factionId == null) return;
    alt.emitServer("Server:Tablet:FactionManagerAppInviteNewMember", charName, parseInt(dienstnummer), parseInt(factionId));
}

function FactionManagerRankAction(action, charId) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (action != "rankup" && action != "rankdown" && action != "remove") return;
    if (charId <= 0 || charId == undefined) return;
    alt.emitServer("Server:Tablet:FactionManagerRankAction", action, parseInt(charId));
}

function FactionManagerSetRankPaycheck(rankId, paycheck) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (rankId <= 0 || paycheck <= 0) return;
    alt.emitServer("Server:Tablet:FactionManagerSetRankPaycheck", parseInt(rankId), parseInt(paycheck));
}

function InternetAppSearchVehicle(vehicleName) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (vehicleName.length <= 0 || vehicleName == "") return;
    alt.emitServer("Server:Tablet:InternetAppSearchVehicle", vehicleName);
}

function LSPDAppSearchPerson(charName) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (charName.length <= 0 || charName == "") return;
    alt.emitServer("Server:Tablet:LSPDAppSearchPerson", charName);
}

function LSPDAppSearchVehiclePlate(plate) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (plate.length <= 0 || plate == "") return;
    alt.emitServer("Server:Tablet:LSPDAppSearchVehiclePlate", plate);
}

function LSPDAppLocateVehiclePlate(plate) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (plate.length <= 0 || plate == "") return;
    alt.emitServer("Server:Tablet:LSPDAppLocateVehiclePlate", plate);
}

function LSPDAppSearchLicense(charName) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (charName.length <= 0 || charName == "") return;
    alt.emitServer("Server:Tablet:LSPDAppSearchLicense", charName);
}

function LSPDAppTakeLicense(charName, lic) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (charName.length <= 0 || charName == "" || lic == "" || lic.length <= 0) return;
    alt.emitServer("Server:Tablet:LSPDAppTakeLicense", charName, lic);
}

function JusticeAppGiveWeaponLicense(charName) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (charName.length <= 0) return;
    alt.emitServer("Server:Tablet:JusticeAppGiveWeaponLicense", charName);
}

function JusticeAppSearchBankAccounts(charName) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (charName.length <= 0) return;
    alt.emitServer("Server:Tablet:JusticeAppSearchBankAccounts", charName);
}

function JusticeAppViewBankTransactions(accNumber) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (accNumber.length <= 0) return;
    alt.emitServer("Server:Tablet:JusticeAppViewBankTransactions", parseInt(accNumber));
}

function sendDispatchToFaction(factionId, msg) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (factionId <= 0 || msg == undefined || msg == "") return;
    alt.emitServer("Server:Tablet:sendDispatchToFaction", parseInt(factionId), msg);
}

function DMVAppSearchLicense(charName) {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (charName.length <= 0 || charName == "") return;
    alt.emitServer("Server:Tablet:DMVAppSearchLicense", charName);
}

export function closeTabletCEF() {
    if (tabletBrowser != null) {
        alt.emit("Client:HUD:setCefStatus", false);
        alt.emitServer("Server:Tablet:Attachment", false);
        tabletBrowser.off("Client:Tablet:AppStoreInstallUninstallApp", AppStoreInstallUninstallApp);
        tabletBrowser.off("Client:Tablet:BankingAppnewTransaction", BankingAppnewTransaction);
        tabletBrowser.off("Client:Tablet:EventsAppNewEntry", EventsAppNewEntry);
        tabletBrowser.off("Client:Tablet:NotesAppNewNote", NotesAppNewNote);
        tabletBrowser.off("Client:Tablet:NotesAppDeleteNote", NotesAppDeleteNote);
        tabletBrowser.off("Client:Tablet:LocateVehicle", LocateTabletVehicle);
        tabletBrowser.off("Client:Tablet:VehicleStoreBuyVehicle", VehicleStoreBuyVehicle);
        tabletBrowser.off("Client:Tablet:CompanyAppInviteNewMember", CompanyAppInviteNewMember);
        tabletBrowser.off("Client:Tablet:CompanyAppLeaveCompany", CompanyAppLeaveCompany);
        tabletBrowser.off("Client:Tablet:CompanyAppRankAction", CompanyAppRankAction);
        tabletBrowser.off("Client:Tablet:FactionManagerAppInviteNewMember", FactionManagerAppInviteNewMember);
        tabletBrowser.off("Client:Tablet:FactionManagerRankAction", FactionManagerRankAction);
        tabletBrowser.off("Client:Tablet:FactionManagerSetRankPaycheck", FactionManagerSetRankPaycheck);
        tabletBrowser.off("Client:Tablet:LSPDAppSearchPerson", LSPDAppSearchPerson);
        tabletBrowser.off("Client:Tablet:InternetAppSearchVehicle", InternetAppSearchVehicle);
        tabletBrowser.off("Client:Tablet:LSPDAppSearchVehiclePlate", LSPDAppSearchVehiclePlate);
        tabletBrowser.off("Client:Tablet:LSPDAppLocateVehiclePlate", LSPDAppLocateVehiclePlate);
        tabletBrowser.off("Client:Tablet:LSPDAppSearchLicense", LSPDAppSearchLicense);
        tabletBrowser.off("Client:Tablet:DMVAppSearchLicense", DMVAppSearchLicense);
        tabletBrowser.off("Client:Tablet:LSPDAppTakeLicense", LSPDAppTakeLicense);
        tabletBrowser.off("Client:Tablet:JusticeAppGiveWeaponLicense", JusticeAppGiveWeaponLicense);
        tabletBrowser.off("Client:Tablet:JusticeAppSearchBankAccounts", JusticeAppSearchBankAccounts);
        tabletBrowser.off("Client:Tablet:JusticeAppViewBankTransactions", JusticeAppViewBankTransactions);
        tabletBrowser.off("Client:Tablet:sendDispatchToFaction", sendDispatchToFaction);
        tabletBrowser.off("Client:Tablet:DeleteFactionDispatch", DeleteFactionDispatch);
        tabletBrowser.unfocus();
        tabletBrowser.destroy();
        tabletBrowser = null;
        alt.showCursor(false);
        alt.toggleGameControls(true);
    }

    tabletReady = false;
    game.clearPedTasks(alt.Player.local.scriptID);
}