import * as alt from 'alt';
import * as game from 'natives';
var markers = [];

alt.onServer("Client:ServerMarkers:LoadAllMarkers", (markerArray) => {
    markerArray = JSON.parse(markerArray);

    for (var i in markerArray) {
        markers.push({
            type: markerArray[i].type,
            x: markerArray[i].posX,
            y: markerArray[i].posY,
            z: markerArray[i].posZ,
            scaleX: markerArray[i].scaleX,
            scaleY: markerArray[i].scaleY,
            scaleZ: markerArray[i].scaleZ,
            red: markerArray[i].red,
            green: markerArray[i].green,
            blue: markerArray[i].blue,
            alpha: markerArray[i].alpha,
            bobUpAndDown: markerArray[i].bobUpAndDown
        });
    }
});

alt.everyTick(() => {
    if (markers.length >= 1) {
        for (var i = 0; i < markers.length; i++) {
            game.drawRect(0, 0, 0, 0, 0, 0, 0, 0, false);
            game.drawMarker(markers[i].type, parseFloat(markers[i].x), parseFloat(markers[i].y), parseFloat(markers[i].z), 0, 0, 0, 0, 0, 0, parseFloat(markers[i].scaleX), parseFloat(markers[i].scaleY), parseFloat(markers[i].scaleZ), parseInt(markers[i].red), parseInt(markers[i].green), parseInt(markers[i].blue), parseInt(markers[i].alpha), markers[i].bobUpAndDown, false, 2, false, undefined, undefined, false);
        }
    }
});