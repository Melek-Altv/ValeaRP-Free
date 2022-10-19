import * as alt from 'alt';
import * as game from 'natives';

let lastInteract = 0;

alt.onServer("Client:Vehicles:ToggleDoorState", (veh, doorid, state) => {
    toggleDoor(veh, parseInt(doorid), state);
});

alt.on("gameEntityCreate", (entity) => {
    if (entity instanceof alt.Vehicle) {
        if (!entity.hasStreamSyncedMeta("IsVehicleCardealer")) return;
        if (entity.getStreamSyncedMeta("IsVehicleCardealer") == true) {
            game.freezeEntityPosition(entity.scriptID, true);
            game.setEntityInvincible(entity.scriptID, true);
        }
    }
});

function toggleDoor(vehicle, doorid, state) {
    if (state) {
        game.setVehicleDoorOpen(vehicle.scriptID, doorid, false, false);
    } else {
        game.setVehicleDoorShut(vehicle.scriptID, doorid, false);
    }
}

alt.onServer("Client:Vehicles:lockUpdate", (veh) => 
{
   alt.setTimeout(() => 
   {
        game.setVehicleLights(veh.scriptID, 2)
        alt.setTimeout(() => 
        {
            game.setVehicleLights(veh.scriptID, 0)
            alt.setTimeout(() => 
            {
                game.setVehicleLights(veh.scriptID, 2)
                alt.setTimeout(() => 
                {
                    game.setVehicleLights(veh.scriptID, 0)
                }, 200)
            }, 200)
        }, 200)
    }, 200)
});