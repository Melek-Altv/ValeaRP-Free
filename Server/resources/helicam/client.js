import * as alt from 'alt';
import game from 'natives';

import Scaleform from 'scaleform.mjs';

let [bol, sx, sy] = game.getActiveScreenResolution(0, 0);
let localPlayer = alt.Player.local;

var helicam = {};
helicam.show = false;
helicam.camera = null;
helicam.fov_max = 80.0;
helicam.fov_min = 10.0;
helicam.fov = (helicam.fov_max + helicam.fov_min) * 0.5;
helicam.scaleForm = new Scaleform('HELI_CAM');
helicam.render = null;
helicam.polmavHash = game.getHashKey("polmav");
helicam.zoomvalue = (1.0 / (helicam.fov_max - helicam.fov_min)) * (helicam.fov - helicam.fov_min);
helicam.mode = 0; // 0 - normal, 1 nightvision, 2 thermal
helicam.zoomspeed = 5.0;
helicam.speed_lr = 3.0;
helicam.speed_ud = 3.0;
helicam.lockedVeh = false;

function isHeliHighEnough(heli) {
    return game.getEntityHeightAboveGround(heli) > 1.5;
}

function Mathrad(degrees) {
    return degrees * Math.PI / 180;
};

function rotAnglesToVec(rot) {
    var z = Mathrad(rot.z);
    var x = Mathrad(rot.x);
    var num = Math.abs(Math.cos(x));
    return new alt.Vector3(-Math.sin(z) * num, Math.cos(z) * num, Math.sin(x));
}

function checkInputRotation(cam, zoomvalue) {
    var rightAxisX = game.getDisabledControlNormal(0, 220);
    var rightAxisY = game.getDisabledControlNormal(0, 221);
    var rotation = game.getCamRot(cam, 2);
    if (rightAxisX != 0.0 || rightAxisY != 0.0) {
        var new_z = rotation.z + rightAxisX * -1.0 * (helicam.speed_ud) * (zoomvalue + 0.1);
        var new_x = Math.max(Math.min(20.0, rotation.x + rightAxisY * -1.0 * (helicam.speed_lr) * (zoomvalue + 0.1)), -89.5);
        game.setCamRot(cam, new_x, 0.0, new_z, 2);
    }
}

function getVehicleInView(cam) {
    var coords = game.getCamCoord(cam);
    var forward_vector = rotAnglesToVec(game.getCamRot(cam, 2))
    var coords2 = new alt.Vector3(coords.x + (forward_vector.x * 200.0), coords.y + (forward_vector.y * 200.0), coords.z + (forward_vector.z * 200.0));

    var ray = game.startExpensiveSynchronousShapeTestLosProbe(coords.x, coords.y, coords.z, coords2.x, coords2.y, coords2.z, 10, localPlayer.vehicle.scriptID, 0);
    var [rayHandle, hit, endCoords, surfaceNormal, entityHit] = game.getShapeTestResult(ray);

    if (entityHit > 0 && game.isEntityAVehicle(entityHit)) {
        return entityHit;
    } else {
        return false;
    }
}

function handleZoom(cam) {
    if (game.isControlJustPressed(0, 241)) {    //SCROLLWHEEL UP
        helicam.fov = Math.max(helicam.fov - helicam.zoomspeed, helicam.fov_min);
    }
    if (game.isControlJustPressed(0, 242)) {    //SCROLLWHEEL DOWN
        helicam.fov = Math.min(helicam.fov + helicam.zoomspeed, helicam.fov_max);
    }
    var current_fov = game.getCamFov(cam);
    if (Math.abs(helicam.fov - current_fov) < 0.1) {
        helicam.fov = current_fov;
    }
    game.setCamFov(cam, current_fov + (helicam.fov - current_fov) * 0.05);
}

alt.on("gameEntityDestroy", function(entity) {
    if (helicam.show) {
        if (entity instanceof alt.Vehicle) {
            if (helicam.lockedVeh && helicam.lockedVeh == entity.scriptID) {
                helicam.lockedVeh = false;
            }
        }
    }
});

function renderHeliCam() {
    if (helicam.show && localPlayer.vehicle && helicam.camera) {
        if (isHeliHighEnough(localPlayer.vehicle.scriptID)) {
            if (helicam.scaleForm) {
                helicam.scaleForm.call("SET_ALT_FOV_HEADING", localPlayer.vehicle.pos.z, helicam.zoomvalue, game.getCamRot(helicam.camera, 2).z);
                helicam.scaleForm.render2D(true);
            }

            //Disable control keys like Radio Wheel, Weapon Wheel on Mouse Scroll if Helicam rendered.
            game.disableControlAction(0, 81, true);
            game.disableControlAction(0, 82, true);
            game.disableControlAction(0, 83, true);
            game.disableControlAction(0, 84, true);
            game.disableControlAction(0, 85, true);
            game.disableControlAction(0, 99, true);

            helicam.zoomvalue = (1.0 / (helicam.fov_max - helicam.fov_min)) * (helicam.fov - helicam.fov_min);
            checkInputRotation(helicam.camera, helicam.zoomvalue);

            if (helicam.lockedVeh) {
                if (game.doesEntityExist(helicam.lockedVeh)) {
                    game.pointCamAtEntity(helicam.camera, helicam.lockedVeh, 0.0, 0.0, 0.0, true);
                    drawText2d(`${game.getLabelText(game.getDisplayNameFromVehicleModel(game.getEntityModel(helicam.lockedVeh)))}\n${game.getVehicleNumberPlateText(helicam.lockedVeh)}\n${game.getEntitySpeed(helicam.lockedVeh) * 3.6}km/h`, 0.5, 0.82, 0.32, 0, 230, 230, 230, 100, true, true, 99);

                    var pos = game.getEntityCoords(helicam.lockedVeh, true);
                    const street = game.getStreetNameAtCoord(pos.x, pos.y, pos.z);
                    drawText2d(`${game.getStreetNameFromHashKey(street[1])}\n${game.getLabelText(game.getNameOfZone(pos.x, pos.y, pos.z))}`, 0.66, 0.47, 0.4, 2, 230, 230, 230, 100, true, true, 99);

                    if (game.isControlJustPressed(0, 25)) { // 	RIGHT MOUSE BUTTON
                        game.playSoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                        helicam.lockedVeh = false;
                        game.stopCamPointing(helicam.camera);
                    }
                } else {
                    helicam.lockedVeh = false;
                }
            } else {
                var vehicleDetected = getVehicleInView(helicam.camera);
                if (game.doesEntityExist(parseInt(vehicleDetected))) {
                    if (game.isControlJustPressed(0, 24)) { // LEFT MOUSE BUTTON
                        game.playSoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                        helicam.lockedVeh = vehicleDetected;
                    }
                }
            }

            handleZoom(helicam.camera);
        } else {
            helicam.hide();
            alt.emit("Client:HUD:sendNotification", 3, 3000, "Der Helikopter ist zu niedrig!");
        }
    } else {
        helicam.hide();
    }
}

function drawText(text, x, y) {
    game.beginTextCommandPrint('STRING');
    game.addTextComponentSubstringPlayerName(text);
    game.endTextCommandPrint(500 * text.length, true);
    game.endTextCommandDisplayText(x, y + 0.025);
}

function drawText2d(msg, x, y, scale, fontType, r, g, b, a, useOutline = true, useDropShadow = true, layer = 0, align = 0) {
    game.setScriptGfxDrawOrder(layer);
    game.beginTextCommandDisplayText('STRING');
    game.addTextComponentSubstringPlayerName(msg);
    game.setTextFont(fontType);
    game.setTextScale(1, scale);
    game.setTextWrap(0.0, 1.0);
    game.setTextCentre(true);
    game.setTextColour(r, g, b, a);
    game.setTextJustification(align);
    if (useOutline) game.setTextOutline();

    if (useDropShadow) game.setTextDropShadow();

    game.endTextCommandDisplayText(x, y, 0);
}


helicam.hide = function() {
    if (helicam.show) {
        game.enableControlAction(0, 81, true);
        game.enableControlAction(0, 82, true);
        game.enableControlAction(0, 83, true);
        game.enableControlAction(0, 84, true);
        game.enableControlAction(0, 85, true);
        game.enableControlAction(0, 99, true);
        helicam.show = false;
        if (helicam.camera) {
            game.renderScriptCams(false, false, 0, 1, 0, 0);
            game.destroyCam(helicam.camera, false);
            helicam.camera = null;
        }
        if (helicam.render) {
            alt.clearEveryTick(helicam.render);
            helicam.render = null;
        }

        game.setSeethrough(false);
        game.setNightvision(false);
    }
}

function changeVision(state) {
    game.setSeethrough(false);
    game.setNightvision(false);
    if (state == 1) {
        game.setNightvision(true);
    } else if (state == 2) {
        game.setSeethrough(true);
    }
}

alt.on("keyup", (i) => {
    if (parseInt(i) == 113 && alt.Player.local.scriptID == game.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, 0, true)) { // F2
        if (localPlayer.vehicle) {
            if (helicam.polmavHash == localPlayer.vehicle.model) {
                if (!helicam.show) {
                    if (isHeliHighEnough(localPlayer.vehicle.scriptID)) {
                        helicam.mode = 0;
                        helicam.lockedVeh = false;
						alt.emit("Client:HUD:ToggleHUD", false);
                        if (!helicam.camera) {
                            helicam.camera = game.createCam("DEFAULT_SCRIPTED_FLY_CAMERA", true);
                            game.attachCamToEntity(helicam.camera, localPlayer.vehicle.scriptID, 0.0, 2.5, -1.4, true);
                            game.setCamRot(helicam.camera, 0.0, 0.0, game.getEntityHeading(localPlayer.vehicle.scriptID), 2);
                            game.setCamFov(helicam.camera, helicam.fov);
                            game.renderScriptCams(true, false, 0, true, false, 0);
                        }
                        helicam.show = true;
                        if (!helicam.render) {
                            helicam.render = alt.everyTick(renderHeliCam);
                        }
                    } else {
                        alt.emit("Client:HUD:sendNotification", 3, 3000, "Der Helikopter ist zu niedrig!");
                    }
                } else {
                    helicam.hide();
					alt.emit("Client:HUD:ToggleHUD", true);
                }
            } else {
            }
        } else {
        }
    }
    if (parseInt(i) == 32) { // W
        if (helicam.show) {
            game.playSoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            helicam.mode++;
            if (helicam.mode == 3) {
                helicam.mode = 0;
            }
            changeVision(helicam.mode);
        }
    }
});