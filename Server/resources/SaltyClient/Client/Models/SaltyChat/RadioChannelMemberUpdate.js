export class RadioChannelMemberUpdate {
    playerNames;
    isPrimaryChannel;
    constructor(playerNames, isPrimaryChannel) {
        this.playerNames = playerNames;
        this.isPrimaryChannel = isPrimaryChannel;
    }
}
