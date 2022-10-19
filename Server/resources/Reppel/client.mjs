import alt, { Player, Vector3 } from 'alt';
import * as native from 'natives';

const player = alt.Player.local;
const maxSpeed = 10.0;
const minHeight = 15.0;
const maxHeight = 45.0;
const maxAngle = 15.0;


alt.on('keydown', (key) => {
    if( key == '0'.charCodeAt(0) ){
    const vehicle = native.getVehiclePedIsIn(player.scriptID, 0);;
        if (!vehicle) {
            return;
        }

        if (native.doesVehicleAllowRappel(1,"0xFCFCB68B",vehicle) == true) {
           alt.emit("Client:HUD:sendNotification", 1, 3500, "Sie können sich nicht von diesem Fahrzeug abseilen.");
            return;
        }

        if ( native.getEntitySpeed(1,"0xFCFCB68B",vehicle) > maxSpeed ) {
            alt.emit("Client:HUD:sendNotification", 1, 3500, "Das Fahrzeug ist zum Abseilen zu schnell.");
            return;
        }

        if(native.getPedInVehicleSeat(vehicle, -1, 0) == player.scriptID || native.getPedInVehicleSeat(vehicle, 0, 0) == player.scriptID ){
            alt.emit("Client:HUD:sendNotification", 1, 3500, "Sie können sich nicht von Ihrem Sitz abseilen.");
            return;
        }

        const taskStatus = native.getScriptTaskStatus(player.scriptID, -275944640);
        if (taskStatus === 0 || taskStatus === 1) {
            alt.emit("Client:HUD:sendNotification", 1, 3500, "Sie lassen sich bereits abseilen.");
            return;
        }

        const curHeight = native.getEntityHeightAboveGround(vehicle);
        if (curHeight < minHeight || curHeight > maxHeight) {
            alt.emit("Client:HUD:sendNotification", 1, 3500, "Das Fahrzeug ist zum Abseilen zu niedrig / zu hoch.");
            return;
        }

        if (!native.isEntityUpright(vehicle, maxAngle) || native.isEntityUpsidedown(vehicle)) {
            alt.emit("Client:HUD:sendNotification", 1, 3500, "Das Fahrzeug ist zum Abseilen zu niedrig / zu hoch.");
            return;
        }

        native.clearPedTasks(player.scriptID);
        native.taskRappelFromHeli(player.scriptID, 10);
    }
});
