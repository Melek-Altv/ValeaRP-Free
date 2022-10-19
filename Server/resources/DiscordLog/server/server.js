const token = "DEINTOKEN";

const Discord = require("discord.js");
const client = new Discord.Client();
//const TOKEN = './.env.TOKEN';
const { CommandHandler } = require("djs-commands");
const CH = new CommandHandler({
  folder: __dirname + "/commands/",
  prefix: ["/"]
});

client.on("ready", () => {
  console.log(client.user.username + " " + "ist hochgefahren.");

  const StartupEmbed = new Discord.MessageEmbed()
	.setTitle('Endless - Status')
	.setColor('#9e55ce')
	.setDescription('Der Server wurde **gestartet**!')
	.setColor('#9e55ce')
	.setThumbnail('https://cdn.discordapp.com/attachments/804702429152804866/952001545732513882/sa-logo.jpg')
	.setTimestamp();

	const channel = client.channels.cache.get('900356278885416999');
	channel.send(StartupEmbed);		

	client.user.setPresence({ game: { name: 'alt:V Multiplayer' }, status: 'online' });
});

client.on("message", message => {

  if (message.channel.type === "dm") return;
  if (message.author.type === "bot") return;
  let args = message.content.split(" ");
  let command = args[0];
  let cmd = CH.getCommand(command);
  if (!cmd) return;

  try {
    cmd.run(client, message, args);
  } catch (e) {
    console.log(e);
  }
}); 

client.login(token);
