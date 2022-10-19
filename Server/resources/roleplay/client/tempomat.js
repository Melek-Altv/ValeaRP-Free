import * as alt from 'alt';
import * as game from 'natives';

var tempomatActive = false;

alt.on("keyup", (key) => {
    if (key === 89 && alt.Player.local.vehicle != undefined && alt.Player.local.getMeta("IsCefOpen") == false) { // 89 = Y
        if (alt.Player.local.seat != 1) return;
        let speed = game.getEntitySpeed(alt.Player.local.vehicle.scriptID);
        if (!tempomatActive) {
            game.setVehicleMaxSpeed(alt.Player.local.vehicle.scriptID, speed);
			alt.emit("Client:HUD:sendNotification", 1, 5000, "Der Tempomat ist jetzt aktiviert.");
		}
        else {
           game.setVehicleMaxSpeed(alt.Player.local.vehicle.scriptID, 0.0);
		   alt.emit("Client:HUD:sendNotification", 1, 5000, "Der Tempomat ist jetzt deaktiviert.");
		}
        tempomatActive = !tempomatActive;
    }
});