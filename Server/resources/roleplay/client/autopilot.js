import * as alt from 'alt';
import * as native from "natives";
import * as game from 'natives';
import { Player, Vector3 } from "alt";
let coords = null;
let isAuto = false;
const triggerKeys = [87, 65, 83, 68];

alt.onServer('Autopilot:autopilot', (hash) => {
        if (alt.Player.local.seat != 1) return;
        let waypoint = game.getFirstBlipInfoId(8);
        coords = game.getBlipInfoIdCoord(waypoint); 
        if (coords == null) return;
        game.taskVehicleDriveToCoord(alt.Player.local, alt.Player.local.vehicle, coords.x, coords.y, coords.z, 30, 1, hash, 2883621, 50, 1);
        isAuto = true;
});

alt.everyTick(() => {
    let waypoint = game.getFirstBlipInfoId(8);
    coords = game.getBlipInfoIdCoord(waypoint);
    if (coords.x == 0 && coords.y == 0 && coords.z == 0 && isAuto == true && alt.Player.local.vehicle != undefined) { // 87 = W
        
        game.clearPedTasks(alt.Player.local);
        isAuto = false;
        game.setVehicleHandbrake(alt.Player.local.vehicle, true);
    }    
});

alt.on("keydown", (key) => {
    if (triggerKeys && isAuto == true) { // = W A S D
        if (alt.Player.local.seat != 1) return;
        game.clearPedTasks(alt.Player.local);
        isAuto = false;
        if(alt.Player.local.vehicle) game.setVehicleHandbrake(alt.Player.local.vehicle, false);
    }
	if(alt.Player.local.vehicle) game.setVehicleHandbrake(alt.Player.local.vehicle, false);
});