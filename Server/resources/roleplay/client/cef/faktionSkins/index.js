$(document).ready(function() {
    setTimeout(function() {
        alt.emit("Client:FrakSkinBrowser:cefIsReady");    
    }, 500);
});

alt.on("CEF:FrakSkinBrowser:SetSkins", (html) => {
    $("#skins").html(html);
});

$("#closeskin").click(function (e) { 
    alt.emit("Client:FrakSkinBrowser:Close");    
});

function callskin(id) {
    alt.emit("Client:FrakSkinBrowser:setSkin", id); 
}