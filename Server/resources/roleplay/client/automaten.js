import * as alt from 'alt';
import * as native from 'natives';
let Machine = null;

function Take(machine, item, price) {
    alt.emitServer("Server:Take", machine, item, price);
}

alt.onServer('Client:BeanMachine:Open', () => {
    alt.log("test client");
    if (Machine == null) {
        alt.showCursor(true);
        alt.toggleGameControls(false);
        Machine = new alt.WebView("http://resource/client/cef/automaten/beanmachine.html");
        Machine.focus();
        native.displayRadar(false);
        Machine.on("Client:Close", CloseCEF);
        Machine.on("Client:Take", Take);
        Machine.on('sound', (sound, family) => { 
            native.playSoundFrontend(-1, sound, family, true); 
            native.playSoundFrontend(-1, sound, family, false); 
        });
    }
    
});

alt.onServer('Client:eCola:Open', () => {
    if (Machine == null) {
        alt.showCursor(true);
        alt.toggleGameControls(false);
        Machine = new alt.WebView("http://resource/client/cef/automaten/ecola.html");
        Machine.focus();
        native.displayRadar(false);
        Machine.on("Client:Close", CloseCEF);
        Machine.on("Client:Take", Take);
        Machine.on('sound', (sound, family) => { 
            native.playSoundFrontend(-1, sound, family, true); 
            native.playSoundFrontend(-1, sound, family, false); 
        });
    }
    
});

alt.onServer('Client:Raineinside:Open', () => {
    if (Machine == null) {
        alt.showCursor(true);
        alt.toggleGameControls(false);
        Machine = new alt.WebView("http://resource/client/cef/automaten/raineinside.html");
        Machine.focus();
        native.displayRadar(false);
        Machine.on("Client:Close", CloseCEF);
        Machine.on("Client:Take", Take);
        Machine.on('sound', (sound, family) => { 
            native.playSoundFrontend(-1, sound, family, true); 
            native.playSoundFrontend(-1, sound, family, false); 
        });
    }
    
});

alt.onServer('Client:Raineoutside:Open', () => {
    if (Machine == null) {
        alt.showCursor(true);
        alt.toggleGameControls(false);
        Machine = new alt.WebView("http://resource/client/cef/automaten/raineoutside.html");
        Machine.focus();
        native.displayRadar(false);
        Machine.on("Client:Close", CloseCEF);
        Machine.on("Client:Take", Take);
        Machine.on('sound', (sound, family) => { 
            native.playSoundFrontend(-1, sound, family, true); 
            native.playSoundFrontend(-1, sound, family, false); 
        });
    }
    
});

alt.onServer('Client:Sprunk:Open', () => {
    if (Machine == null) {
        alt.showCursor(true);
        alt.toggleGameControls(false);
        Machine = new alt.WebView("http://resource/client/cef/automaten/sprunk.html");
        Machine.focus();
        native.displayRadar(false);
        Machine.on("Client:Close", CloseCEF);
        Machine.on("Client:Take", Take);
        Machine.on('sound', (sound, family) => { 
            native.playSoundFrontend(-1, sound, family, true); 
            native.playSoundFrontend(-1, sound, family, false); 
        });
    }
    
});

alt.onServer('Client:Candybox:Open', () => {
    if (Machine == null) {
        alt.showCursor(true);
        alt.toggleGameControls(false);
        Machine = new alt.WebView("http://resource/client/cef/automaten/candybox.html");
        Machine.focus();
        native.displayRadar(false);
        Machine.emit('CEF:Load:Items');
        Machine.on("Client:Close", CloseCEF);
        Machine.on("Client:Take", Take);
        Machine.on('sound', (sound, family) => { 
            native.playSoundFrontend(-1, sound, family, true); 
            native.playSoundFrontend(-1, sound, family, false); 
        });
    }
    
});

let CloseCEF = function() {
    Machine.unfocus();
    Machine.destroy();
    Machine = null;
    alt.showCursor(false);
    alt.toggleGameControls(true);
    native.displayRadar(true);
}