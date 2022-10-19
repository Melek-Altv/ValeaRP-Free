import * as alt from "alt";
import * as game from "natives";

var audioengine = new alt.WebView("http://resource/client/index.html");
audioengine.unfocus();

alt.onServer("playHowl2d", (audiopath) => { //from server
    audioengine.emit("playHowl2d", audiopath, 0.4);
});

alt.on("playHowl2d", (audiopath) => { //from client
    audioengine.emit("playHowl2d", audiopath, 0.4); 
});