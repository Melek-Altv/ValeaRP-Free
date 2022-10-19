import * as alt from 'alt';
import * as game from 'natives';

var FrakSkinBrowser = null;

alt.onServer("Client:FrakSkinBrowser:Load", () => {
    if (FrakSkinBrowser == null) {
        alt.showCursor(true);
        alt.toggleGameControls(false);
        alt.emit("Client:HUD:setCefStatus", true);
        FrakSkinBrowser = new alt.WebView("http://resource/client/cef/faktionSkins/index.html", true);
        FrakSkinBrowser.focus();
        
        FrakSkinBrowser.on("Client:FrakSkinBrowser:cefIsReady", () => {
            alt.emitServer("Server:FrakSkinBrowser:cefIsReady");
        });

        FrakSkinBrowser.on("Client:FrakSkinBrowser:setSkin", (id) => {
            alt.emitServer("Server:FrakSkinBrowser:setSkin", id);
        });

        alt.onServer('Client:FrakSkinBrowser:SetSkins', (html) => {
            FrakSkinBrowser.emit("CEF:FrakSkinBrowser:SetSkins", html);
        });

        alt.onServer("Server:FrakSkinBrowser:Close", () => {
            FrakSkinBrowser.unfocus();
            FrakSkinBrowser.destroy();
            FrakSkinBrowser = null;
            alt.showCursor(false);
            alt.toggleGameControls(true);
            alt.emit("Client:HUD:setCefStatus", false);
        });

        FrakSkinBrowser.on("Client:FrakSkinBrowser:Close", () => {
            FrakSkinBrowser.unfocus();
            FrakSkinBrowser.destroy();
            FrakSkinBrowser = null;
            alt.showCursor(false);
            alt.toggleGameControls(true);
            alt.emit("Client:HUD:setCefStatus", false);
        });
    }
});
