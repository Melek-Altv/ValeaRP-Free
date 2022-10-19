import * as alt from 'alt'
import * as game from 'natives'
let StarsCEF = null;

alt.onServer("Client:Stars:CreateCEF", (currPoliceMembers) => {
    if (StarsCEF == null) {
        StarsCEF = new alt.WebView("http://resource/client/cef/stars/index.html");
        StarsCEF.emit("CEF:Stars:CreateStarsCEF", currPoliceMembers);
    }
});

alt.onServer("Client:Stars:UpdateStars", (currPoliceMembers) => {
    if (StarsCEF != null) {
        StarsCEF.emit("CEF:Stars:UpdateStarsCef", currPoliceMembers);
    }
});