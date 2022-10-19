import * as alt from 'alt';
import * as game from 'natives';


alt.onServer("Client:Seatbelt:ToggleSeatbelt", (seatbelt) => {
    toggleSeatbelt(seatbelt);
});

let isSeatbelt = false;
function toggleSeatbelt(seatbelt) {
    if (!seatbelt) {
        game.setPedConfigFlag(alt.Player.local.scriptID, 32, false); 
        game.disableControlAction(0, 75, true);
    } else if (seatbelt) {
        game.setPedConfigFlag(alt.Player.local.scriptID, 32, true); 
        game.disableControlAction(0, 75, false);
    }
    isSeatbelt = !seatbelt;
}

alt.everyTick(() => {
    if (isSeatbelt) {
        game.disableControlAction(0, 75, true);
    }
});