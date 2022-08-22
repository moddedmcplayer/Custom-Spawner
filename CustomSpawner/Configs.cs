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
		public string DiscordInvite { get; set; } = "discord.gg/yourinvite";

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
		public Vector3 ClassDPoint { get; set; } = new Vector3()
		{
			x = 249,
			y = 980,
			z = 81.5f
		};
		public Quaternion ClassDRotation { get; set; } = new Quaternion()
		{
			w = 0, 
			x= 340f,
			z = 0
		};
		
		[Description("Spawn point of the Guard dummy")]
		public Vector3 GuardPoint { get; set; } = new Vector3()
		{
			x = 237,
			y = 980,
			z = 81.7f
		};
		public Quaternion GuardRotation { get; set; } = new Quaternion()
		{
			w = 0, 
			x= 12f,
			z = 0
		};

		[Description("Spawn point of the tutorial (random team) dummy")]
		public Vector3 Tutorial { get; set; } = new Vector3()
		{
			x = 228,
			y = 980,
			z = 87.6f
		};
		public Quaternion TutorialRotation { get; set; } = new Quaternion()
		{
			w = 0, 
			x= 55.8f,
			z = 0
		};

		[Description("Spawn point and rotation of the SCP dummy")]
		public Vector3 SCPPoint { get; set; } = new Vector3()
		{
			x = 223f,
			y = 980f,
			z = 99f
		};
		public Quaternion SCPRotation { get; set; } = new Quaternion()
		{
			w = 0, 
			x= 100.64f,
			z = 0
		};

		[Description("Spawn point and rotation of the Scientist dummy")]
		public Vector3 ScientistPoint { get; set; } = new Vector3()
		{
			x = 226,
			y = 980,
			z = 107
		};
		public Quaternion ScientistRotation { get; set; } = new Quaternion()
		{
			w = 0, 
			x = 129.25f,
			z = 0
		};
	}	
}
