import * as alt from 'alt';
import * as game from 'natives';
var canUseEKey = true;
var lastInteract = 0;
let toggleCrouch = false;

function canInteract() { return lastInteract + 1000 < Date.now() }

alt.on('keyup', (key) => {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (key == 'E'.charCodeAt(0)) {
        if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true || alt.Player.local.getMeta("IsCefOpen") == true) return;
        alt.emitServer("Server:KeyHandler:PressE");
    } else if (key == 'U'.charCodeAt(0)) {
        alt.emitServer("Server:KeyHandler:PressU");
    } else if (key == 'O'.charCodeAt(0)) {
        alt.emitServer("Server:KeyHandler:PressO");
    } 
});

alt.on('keydown', (key) => {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (key === 17 && alt.Player.local.getMeta("IsCefOpen") == false && alt.Player.local.getSyncedMeta("HasHandcuffs") == false && alt.Player.local.getSyncedMeta("HasRopeCuffs") == false) { //STRG
        game.disableControlAction(0, 36, true);
        if (!game.isPlayerDead(alt.Player.local) && !game.isPedSittingInAnyVehicle(alt.Player.local.scriptID)) {
            if (!game.isPauseMenuActive()) {
                
                game.requestAnimSet("move_ped_crouched");
                if (!toggleCrouch) {
                    game.setPedMovementClipset(alt.Player.local.scriptID, "move_ped_crouched", 0.45);
                    toggleCrouch = true;
                } else {
                    game.clearPedTasks(alt.Player.local.scriptID);
                    game.resetPedMovementClipset(alt.Player.local.scriptID, 0.45);
                    toggleCrouch = false;
                }
            }
        }
    } 
});

alt.onServer("Client:DoorManager:ManageDoor", (hash, pos, isLocked) => {
    if (hash != undefined && pos != undefined && isLocked != undefined) {
        // game.doorControl(game.getHashKey(hash), pos.x, pos.y, pos.z, isLocked, 0.0, 50.0, 0.0); //isLocked (0) = Open | isLocked (1) = True
        game.setStateOfClosestDoorOfType(hash, pos.x, pos.y, pos.z, isLocked, 0, 0);
    }
});

let localplayer = alt.Player.local;
let entityTypes = [null, 'ped', 'vehicle', 'object'];
function TrytoGetHash() {
    let [result, _entity] = game.getEntityPlayerIsFreeAimingAt(localplayer.scriptID);
    if (!_entity) { return; }
    const handleType = game.getEntityType(_entity);
    switch (handleType) {
        case 3:
            alt.log("Object-Hash : " + game.getEntityModel(_entity));

            alt.log("Object-Pos : " + game.getEntityCoords(_entity, undefined));

            alt.emitServer("Server:KeyHandler:CreateNewDoor", game.getEntityModel(_entity), game.getEntityCoords(_entity, undefined));
            break;
        default:
            alt.log("Other Entity Found : " + entityTypes[handleType]);
            break;
    }
}

alt.on('keydown', (key) => {
        switch (key) {
            case 123:
                TrytoGetHash();
                break;
        }
}); 