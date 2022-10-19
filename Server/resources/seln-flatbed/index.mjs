import * as alt from 'alt';

let tows = []

alt.onClient('nvrpg:flatbed:removetow', (player, thistow) => {
    let temptows = []
    tows.forEach(tow => {
        if (tow.flatbed != thistow.flatbed) {
            temptows.push(tow)
        }
    });
    tows = temptows
});

alt.onClient('nvrpg:flatbed:addtow', (player, thisflatbed, thistowed) => {
    tows.push({ flatbed : thisflatbed, towed : thistowed })
});

alt.onClient('nvrpg:flatbed:gettowedvehicleslist', (player) => {
    alt.emitClient(player, 'nvrpg:flatbed:sendtowedvehicleslist', tows);
});

alt.onClient('nvrpg:flatbed:getvehicleslist', (player) => {
    let list = alt.Vehicle.all;
    alt.emitClient(player, 'nvrpg:flatbed:sendvehicleslist', list);
});
