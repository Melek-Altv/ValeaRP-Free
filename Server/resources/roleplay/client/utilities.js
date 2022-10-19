import * as alt from 'alt';
import * as game from 'natives';
import { hudBrowser } from './hud.js';

let blip = null;
let markers = [];
let playerTattoos = undefined;
let attachmentList = [];

export function clearTattoos(entity) {
    game.clearPedDecorations(entity);
}

export function setTattoo(entity, collection, hash) {
    game.addPedDecorationFromHashes(entity, game.getHashKey(collection), game.getHashKey(hash));
}

export function setClothes(entity, compId, draw, tex) {
    game.setPedComponentVariation(entity, compId, draw, tex, 0);
}

alt.onServer("Client:Utilities:setTattoos", (tattooJSON) => {
    playerTattoos = JSON.parse(tattooJSON);
    setCorrectTattoos();
});

export function setCorrectTattoos() {
    clearTattoos(alt.Player.local.scriptID);
    for (var i in playerTattoos) setTattoo(alt.Player.local.scriptID, playerTattoos[i].collection, playerTattoos[i].hash);
}

alt.onServer("returnVehicleMods", (curVeh, modId) => {
    if (curVeh == null || curVeh == undefined) return;
    game.setVehicleModKit(curVeh.scriptID, 0);
    let maxMods = game.getNumVehicleMods(curVeh.scriptID, parseInt(modId));
    if (maxMods === 0) {
    } else {
        for (var i = 0; i < maxMods; i++) {
            alt.emitServer("Server:Utilities:createNewMod", `${game.getLabelText(game.getModTextLabel(curVeh.scriptID, modId, i))}`, parseInt(modId), parseInt(i + 1));
        }
    }
});

alt.onServer("Client:Entity:setTime", (hour, minute) => {
    game.setClockTime(parseInt(hour), parseInt(minute), 0);
    alt.setMsPerGameMinute(60000);
});

alt.onServer("Client:Minijob:CreateJobMarker", (name, color, sprite, markersprite, X, Y, Z, bobUpAndDown) => {
    blip = createBlip(X, Y, Z, sprite, 0.8, color, true, name);
    markers.push({
        type: markersprite,
        x: X,
        y: Y,
        z: Z,
        scaleX: 1,
        scaleY: 1,
        scaleZ: 1,
        red: 46,
        green: 133,
        blue: 232,
        alpha: 150,
        bobUpAndDown: bobUpAndDown
    });
});

alt.onServer("Client:Minijob:RemoveJobMarker", () => {
    markers = [];
    if (blip != null || blip.valid)
        blip.destroy();
});

alt.onServer("Client:Minijob:RemoveJobMarkerWithFreeze", (delay) => {
    markers = [];
    let vehicle = alt.Player.local.vehicle.scriptID;
    if (blip != null || blip.valid)
        blip.destroy();

    alt.setTimeout(() => {
        if (vehicle != null) {
            game.freezeEntityPosition(vehicle, true);
        }
        alt.setTimeout(() => {
            if (vehicle != null) {
                game.freezeEntityPosition(vehicle, false);
            }
        }, delay);
    }, 500);
});

alt.onServer("Client:Utilities:repairVehicle", (vehicle) => {
    repairVehicle(vehicle);
});

alt.onServer("Client:Utilities:setIntoVehicle", (vehicle) => {
    alt.setTimeout(() => {
        game.setPedIntoVehicle(alt.Player.local.scriptID, vehicle.scriptID, -1)
    }, 100);
});

function createBlip(X, Y, Z, sprite, scale, color, shortRange, name) {
    var newBlip = new alt.PointBlip(X, Y, Z);
    newBlip.sprite = sprite;
    newBlip.scale = scale;
    newBlip.color = color;
    newBlip.shortRange = shortRange;
    newBlip.name = name;
    newBlip.routeColor = color;
    newBlip.route = true;
    return newBlip;
}

export function repairVehicle(vehicle) {
    if (vehicle != null && vehicle instanceof alt.Vehicle) {
        game.setVehicleFixed(vehicle.scriptID);
        game.setVehicleDeformationFixed(vehicle.scriptID);
    }
}

export function GetDirectionFromRotation(rotation) {
    var z = rotation.z * (Math.PI / 180.0);
    var x = rotation.x * (Math.PI / 180.0);
    var num = Math.abs(Math.cos(x));

    return new alt.Vector3(
        (-Math.sin(z) * num),
        (Math.cos(z) * num),
        Math.sin(x)
    );
}

export class Raycast {
    static player = alt.Player.local;

    static line(radius, distance) {
        let position = game.getPedBoneCoords(alt.Player.local.scriptID, 31086, 0.5, 0, 0);
        let direction = GetDirectionFromRotation(game.getGameplayCamRot(2));
        let farAway = new alt.Vector3((direction.x * distance) + (position.x), (direction.y * distance) + (position.y), (direction.z * distance) + (position.z));
        let ray = game.startShapeTestCapsule(position.x, position.y, position.z, farAway.x, farAway.y, farAway.z, radius, -1, alt.Player.local.scriptID, 7);
        return this.result(ray);
    }

    static result(ray) {
        let result = game.getShapeTestResult(ray, undefined, undefined, undefined, undefined);
        let hitEntity = result[4];
        if (!game.isEntityAPed(hitEntity) && !game.isEntityAnObject(hitEntity) && !game.isEntityAVehicle(hitEntity)) return undefined;
        return {
            isHit: result[1],
            pos: new alt.Vector3(result[2].x, result[2].y, result[2].z),
            hitEntity,
            entityType: game.getEntityType(hitEntity),
            entityHash: game.getEntityModel(hitEntity)
        }
    }
}

alt.everyTick(() => {
    if (markers.length >= 1) {
        for (var i = 0; i < markers.length; i++) {
            game.drawRect(0, 0, 0, 0, 0, 0, 0, 0, false);
            game.drawMarker(markers[i].type, markers[i].x, markers[i].y, markers[i].z, 0, 0, 0, 0, 0, 0, markers[i].scaleX, markers[i].scaleY, markers[i].scaleZ, markers[i].red, markers[i].green, markers[i].blue, markers[i].alpha, markers[i].bobUpAndDown, false, 2, false, undefined, undefined, false);
        }
    }
});

alt.onServer("Client:Utilities:cleanVehicle", (vehicle) => {
    game.washDecalsFromVehicle(vehicle, 1.0);
    game.setVehicleDirtLevel(vehicle, 0.0);
});

alt.on("streamSyncedMetaChange", (entity, key, value, oldValue) => {
    if (entity == undefined || key != "player_attachment") return;
    if (value == undefined && oldValue != undefined) {
        oldValue = JSON.parse(oldValue);
        const foundedObject = attachmentList.find(x => x.id == oldValue.id);
        if (foundedObject == undefined) return;
        game.deleteObject(foundedObject.prop);
        attachmentList = attachmentList.filter(x => x.id != oldValue.id);
    } else if (value != undefined && oldValue == undefined) {
        value = JSON.parse(value);
        let objModel = game.getHashKey(value.model);
        game.requestModel(objModel);
        let requestInter = alt.setInterval(() => {
            if (!game.hasModelLoaded(objModel)) return;
            let createdObject = game.createObject(objModel, entity.pos.x, entity.pos.y, entity.pos.z, true, true, false);
            game.attachEntityToEntity(createdObject, entity, game.getPedBoneIndex(entity, value.bone), value.posX, value.posY, value.posZ, value.rotX, value.rotY, value.rotZ, false, false, false, false, 1, true);
            attachmentList.push({ "id": value.id, "prop": createdObject });
            alt.clearInterval(requestInter);
        }, 0);
    }
});

alt.on("gameEntityCreate", (entity) => {
    if (entity == undefined || !entity.hasStreamSyncedMeta("player_attachment")) return;
    var value = JSON.parse(entity.getStreamSyncedMeta("player_attachment"));
    let objModel = game.getHashKey(value.model);
    game.requestModel(objModel);
    let requestInter = alt.setInterval(() => {
        if (!game.hasModelLoaded(objModel)) return;
        let createdObject = game.createObject(objModel, entity.pos.x, entity.pos.y, entity.pos.z, true, true, false);
        game.attachEntityToEntity(createdObject, entity, game.getPedBoneIndex(entity, value.bone), value.posX, value.posY, value.posZ, value.rotX, value.rotY, value.rotZ, false, false, false, false, 1, true);
        attachmentList.push({ "id": value.id, "prop": createdObject });
        alt.clearInterval(requestInter);
    }, 0);
});

alt.on("gameEntityDestroy", (entity) => {
    if (entity == undefined || !entity.hasStreamSyncedMeta("player_attachment")) return;
    var value = JSON.parse(entity.getStreamSyncedMeta("player_attachment"));
    const foundedObject = attachmentList.find(x => x.id == value.id);
    if (foundedObject == undefined) return;
    game.deleteObject(foundedObject.prop);
    attachmentList = attachmentList.filter(x => x.id != value.id);
});

alt.onServer('Client:Animation:PlayScenario', (scenarioName, duration) => {
    game.taskStartScenarioInPlace(alt.Player.local, scenarioName, 0, true)
    if (duration > 0) {
        alt.setTimeout(() => {
            game.clearPedTasks(alt.Player.local.scriptID);
        }, duration);
    }
})