using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace CustomSpawner
{
	public sealed class Config : IConfig
	{
		[Description("If the plugin is enabled or not.")]
		public bool IsEnabled { get; set; } = true;

		[Description("Show debug messages?")]
		public bool ShowDebug { get; set; } = false;
		
		[Description("Upper text shown to the user")]
		public string UpperText { get; set; } = "Welcome to the Server!";

		[Description("Bottom text shown to the user")]
		public string BottomText { get; set; } = "Go stand next to the team you want to play as!";

		[Description("Your discord group invite")]
		public string DiscordInvite { get; set; } = "discord.gg/kognity";

		[Description("If you want a Broadcast to show the class that the player will vote as when they're standing on the circle")]
		public bool VotingBroadcast { get; set; } = true;

		[Description("Spawn queue")]
		public string SpawnQueue { get; set; } = "40143140314414041340";

		public string StartingSoonText { get; set; } = "The game will be starting soon";
		public string PausedServer { get; set; } = "Server is paused";
		public string RoundStarted{ get; set; } = "Round is being started";
		public string SecondRemain { get; set; } = "second remain";
		public string SecondsRemain { get; set; } = "seconds remain";
		public string PlayerHasConnected { get; set; } = "player has connected";
		public string PlayersHaveConnected { get; set; } = "players have connected";

		public string SCPTeam { get; set; } = "You are voting for SCP team!";
		public string ClassDTeam { get; set; } = "You are voting for Class D team!";
		public string ScientistTeam { get; set; } = "You are voting for Scientist team!";
		public string GuardTeam { get; set; } = "You are voting for Guard team!";
		public string RandomTeam{ get; set; } = "You are voting for random team!";

		public string RandomTeamDummy { get; set; } = "Random Team";
		public string ClassDTeamDummy { get; set; } = "Class D Team";
		public string SCPTeamDummy { get; set; } = "SCP Team";
		public string ScientistTeamDummy { get; set; } = "Scientist Team";
		public string MTFTeamDummy { get; set; } = "MTF Team";

		
		[Description("Spawn points for player upon joining (pre-round)")]
		public Vector3 SpawnPoint { get; set; } = new Vector3(240, 978, 96);
		[Description("Spawn point and rotation of the Class D dummy")]
		public KeyValuePair<Vector3, Quaternion> ClassDPoint { get; set; } = new (new Vector3(249, 980, 81.5f), Quaternion.Euler(0, 340f, 0));
		[Description("Spawn point and rotation of the Guard dummy")]
		public KeyValuePair<Vector3, Quaternion> GuardPoint { get; set; } = new (new Vector3(237, 980, 81.7f), Quaternion.Euler(0f, 12f, 0f));
		[Description("Spawn point and rotation of the tutorial (random team) dummy")]
		public KeyValuePair<Vector3, Quaternion> Tutorial { get; set; } = new (new Vector3(228, 980, 87.6f), Quaternion.Euler(0f, 55.8f, 0f));
		[Description("Spawn point and rotation of the SCP dummy")]
		public KeyValuePair<Vector3, Quaternion> SCPPoint { get; set; } = new (new Vector3(223, 980, 99), Quaternion.Euler(0, 100.64f, 0f));
		[Description("Spawn point and rotation of the Scientist dummy")]
		public KeyValuePair<Vector3, Quaternion> ScientistPoint { get; set; } = new (new Vector3(226, 980, 107), Quaternion.Euler(0,129.25f , 0));
	}	
}
