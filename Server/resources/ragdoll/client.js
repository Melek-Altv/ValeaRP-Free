import * as alt from 'alt';
import Ragdoll from './ragdoll.js';

const ragdoll = new Ragdoll(alt.Player.local);
const ragdollKey = 'R';

alt.on('keydown', (key) => {
  if (alt.gameControlsEnabled()) {
    if (key === ragdollKey.charCodeAt(0)) {
      ragdoll.start();
    }
  }
});

alt.on('keyup', (key) => {
  if (alt.gameControlsEnabled()) {
    if (key === ragdollKey.charCodeAt(0)) {
      ragdoll.stop();
    }
  }
});

alt.onServer("Client:Ragdoll:SetPedToRagdoll", (state, delay) => {
  alt.setTimeout(() => {
      if (state) {
          ragdoll.start();
      } else {
          ragdoll.stop();
      }
  }, delay);
});