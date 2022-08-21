using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Mirror;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AdminToys;
using Exiled.API.Features.Items;
using Mirror.LiteNetLib4Mirror;
using MonoMod.Utils;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace CustomSpawner
{
	public class EventHandler
	{
		public CustomSpawner plugin;
		public EventHandler(CustomSpawner plugin)
		{
			this.plugin = plugin;
			SpawnPoint = this.plugin.Config.SpawnPoint;
			ClassDPoint = this.plugin.Config.ClassDPoint.Item1;
			GuardPoint = this.plugin.Config.GuardPoint.Item1;
			Tutorial = this.plugin.Config.Tutorial.Item1;
			SCPPoint = this.plugin.Config.SCPPoint.Item1;
			ScientistPoint = this.plugin.Config.ScientistPoint.Item1;
			dummySpawnPointsAndRotations.Clear();
			dummySpawnPointsAndRotations.AddRange(new Dictionary<RoleType, (Vector3, Quaternion)>()
			{
				{RoleType.ClassD, this.plugin.Config.ClassDPoint},
				{RoleType.FacilityGuard, this.plugin.Config.GuardPoint},
				{RoleType.Tutorial, this.plugin.Config.Tutorial},
				{RoleType.Scp173, this.plugin.Config.SCPPoint},
				{RoleType.Scientist, this.plugin.Config.ScientistPoint}
			});
		}

		private readonly Config Config = CustomSpawner.Singleton.Config;

		private static Vector3 SpawnPoint; // Spawn point for all players when they get set to tutorial

		// Spawn points for the different teams, making an arch shape
		public static Vector3 ClassDPoint;
		public static Vector3 GuardPoint;
		public static Vector3 Tutorial;
		public static Vector3 SCPPoint;
		public static Vector3 ScientistPoint;

		private CoroutineHandle lobbyTimer;

		private int SCPsToSpawn = 0;
		private int ClassDsToSpawn = 0;
		private int ScientistsToSpawn = 0;
		private int GuardsToSpawn = 0;

		private List<Pickup> boll = new List<Pickup> { }; // boll :flushed:

		private List<GameObject> Dummies = new List<GameObject> { };

		Dictionary<RoleType, KeyValuePair<Vector3, Quaternion>> dummySpawnPointsAndRotations = new Dictionary<RoleType, KeyValuePair<Vector3, Quaternion>>();

		public void OnPickingUp(PickingUpItemEventArgs ev)
		{
			if (boll.Contains(ev.Pickup))
				ev.IsAllowed = false;
		}

		public void OnVerified(VerifiedEventArgs ev)
		{
			if (!Round.IsStarted && (GameCore.RoundStart.singleton.NetworkTimer > 1 || GameCore.RoundStart.singleton.NetworkTimer == -2))
			{
				Timing.CallDelayed(1f, () =>
				{
					ev.Player.IsOverwatchEnabled = false;
					ev.Player.SetRole(RoleType.Tutorial);
					Scp096.TurnedPlayers.Add(ev.Player);
					Scp173.TurnedPlayers.Add(ev.Player);
				});

				Timing.CallDelayed(1.5f, () =>
				{
					ev.Player.Position = SpawnPoint;
				});
			}
		}

		public void OnRoundStart()
		{
			if(Player.List.Any(x => x.Role.Type != RoleType.Tutorial && x.Role.Type != RoleType.Spectator))
				return;
			
			foreach (var thing in Dummies)
			{
				DummiesManager.dummies.Remove(thing);
				UnityEngine.Object.Destroy(thing); // Deleting the dummies and SCP-018 circles
			}
			if (lobbyTimer.IsRunning)
			{
				Timing.KillCoroutines(lobbyTimer);
			}

			Log.Debug($"Player List count: {Player.List.Count()}", Config.ShowDebug);
			Log.Debug($"Config::Spawnquene count: {Config.SpawnQueue.Count()}", Config.ShowDebug);
			for (int x = 0; x < Player.List.ToList().Count; x++)
			{
				if (x >= Config.SpawnQueue.Count())
				{
					ClassDsToSpawn += 1;
					continue;
				}
				switch (Config.SpawnQueue[x])
				{
					case '4':
						ClassDsToSpawn += 1;
						break;
					case '0':
						SCPsToSpawn += 1;
						break;
					case '1':
						GuardsToSpawn += 1;
						break;
					case '3':
						ScientistsToSpawn += 1;
						break;
				}
			}

			Log.Debug($"Calculated people to spawn! SCPS: {SCPsToSpawn}, CDs: {ClassDsToSpawn}, Guard: {GuardsToSpawn}, Science: {ScientistsToSpawn}", Config.ShowDebug);
			List<Player> BulkList = Player.List.ToList();
			List<Player> SCPPlayers = new List<Player> { };
			List<Player> ScientistPlayers = new List<Player> { };
			List<Player> GuardPlayers = new List<Player> { };
			List<Player> ClassDPlayers = new List<Player> { };

			List<Player> PlayersToSpawnAsSCP = new List<Player> { };
			List<Player> PlayersToSpawnAsScientist = new List<Player> { };
			List<Player> PlayersToSpawnAsGuard = new List<Player> { };
			List<Player> PlayersToSpawnAsClassD = new List<Player> { };

			foreach (var player in Player.List)
			{
				if (Vector3.Distance(player.Position, SCPPoint) <= 3)
				{
					SCPPlayers.Add(player);
					Log.Debug($"Added {player.Nickname} to scp spawnlist!", Config.ShowDebug);
				}
				else if (Vector3.Distance(player.Position, ClassDPoint) <= 3)
				{
					ClassDPlayers.Add(player);
					Log.Debug($"Added {player.Nickname} to cd spawnlist!", Config.ShowDebug);
				}
				else if (Vector3.Distance(player.Position, ScientistPoint) <= 3)
				{
					ScientistPlayers.Add(player);
					Log.Debug($"Added {player.Nickname} to sc spawnlist!", Config.ShowDebug);
				}
				else if (Vector3.Distance(player.Position, GuardPoint) <= 3)
				{
					GuardPlayers.Add(player);
					Log.Debug($"Added {player.Nickname} to guard spawnlist!", Config.ShowDebug);
				}
			}
			// ---------------------------------------------------------------------------------------\\
			// ClassD
			if (ClassDsToSpawn != 0)
			{
				if (ClassDPlayers.Count <= ClassDsToSpawn) // Less people (or equal) voted than what is required in the game.
				{
					foreach (Player ply in ClassDPlayers)
					{
						PlayersToSpawnAsClassD.Add(ply);
						ClassDsToSpawn -= 1;
						BulkList.Remove(ply);
					}
				}
				else // More people voted than what is required, time to play the game of chance.
				{
					for (int x = 0; x < ClassDsToSpawn; x++)
					{
						Player Ply = ClassDPlayers[Random.Range(0, ClassDPlayers.Count)];
						PlayersToSpawnAsClassD.Add(Ply);
						ClassDPlayers.Remove(Ply); // Removing winner from the list
						BulkList.Remove(Ply); // Removing the winners from the bulk list
					}
					ClassDsToSpawn = 0;
				}
			}
			// ---------------------------------------------------------------------------------------\\
			// Scientists
			if (ScientistsToSpawn != 0)
			{
				if (ScientistPlayers.Count <= ScientistsToSpawn) // Less people (or equal) voted than what is required in the game.
				{
					foreach (Player ply in ScientistPlayers)
					{
						PlayersToSpawnAsScientist.Add(ply);
						ScientistsToSpawn -= 1;
						BulkList.Remove(ply);
					}
				}
				else // More people voted than what is required, time to play the game of chance.
				{
					for (int x = 0; x < ScientistsToSpawn; x++)
					{
						Player Ply = ScientistPlayers[Random.Range(0, ScientistPlayers.Count)];
						PlayersToSpawnAsScientist.Add(Ply);
						ScientistPlayers.Remove(Ply); // Removing winner from the list
						BulkList.Remove(Ply); // Removing the winners from the bulk list
					}
					ScientistsToSpawn = 0;
				}
			}
			// ---------------------------------------------------------------------------------------\\
			// Guards
			if (GuardsToSpawn != 0)
			{
				if (GuardPlayers.Count <= GuardsToSpawn) // Less people (or equal) voted than what is required in the game.
				{
					foreach (Player ply in GuardPlayers)
					{
						PlayersToSpawnAsGuard.Add(ply);
						GuardsToSpawn -= 1;
						BulkList.Remove(ply);
					}
				}
				else // More people voted than what is required, time to play the game of chance.
				{
					for (int x = 0; x < GuardsToSpawn; x++)
					{
						Player Ply = GuardPlayers[Random.Range(0, GuardPlayers.Count)];
						PlayersToSpawnAsGuard.Add(Ply);
						GuardPlayers.Remove(Ply); // Removing winner from the list
						BulkList.Remove(Ply); // Removing the winners from the bulk list
					}
					GuardsToSpawn = 0;
				}
			}
			// ---------------------------------------------------------------------------------------\\
			// SCPs
			if (SCPsToSpawn != 0)
			{
				if (SCPPlayers.Count <= SCPsToSpawn) // Less people (or equal) voted than what is required in the game.
				{
					foreach (Player ply in SCPPlayers)
					{
						PlayersToSpawnAsSCP.Add(ply);
						SCPsToSpawn -= 1;
						BulkList.Remove(ply);
					}
				}
				else // More people voted than what is required, time to play the game of chance.
				{
					for (int x = 0; x < SCPsToSpawn; x++)
					{
						Player Ply = SCPPlayers[Random.Range(0, SCPPlayers.Count)];
						SCPPlayers.Remove(Ply);
						PlayersToSpawnAsSCP.Add(Ply); // Removing winner from the list
						BulkList.Remove(Ply); // Removing the winners from the bulk list
					}
					SCPsToSpawn = 0;
				}
			}
			Log.Debug($"Filling blanks: Players (SCP/CD/SC/MTF): {SCPPlayers.Count()}, {ClassDPlayers.Count()}, {ScientistPlayers.Count()}, {GuardPlayers.Count()}", Config.ShowDebug);
			foreach (var playerPly in SCPPlayers)
			{
				Log.Debug($"{playerPly.Nickname}, {playerPly.UserId}", Config.ShowDebug);
			}
			// ---------------------------------------------------------------------------------------\\
			// ---------------------------------------------------------------------------------------\\
			// ---------------------------------------------------------------------------------------\\
			// ---------------------------------------------------------------------------------------\\

			// At this point we need to check for any blanks and fill them in via the bulk list guys
			if (ClassDsToSpawn != 0)
			{
				for (int x = 0; x < ClassDsToSpawn; x++)
				{
					Player Ply = BulkList[Random.Range(0, BulkList.Count)];
					PlayersToSpawnAsClassD.Add(Ply);
					BulkList.Remove(Ply); // Removing the winners from the bulk list
				}
			}
			if (SCPsToSpawn != 0)
			{
				for (int x = 0; x < SCPsToSpawn; x++)
				{
					Player Ply = BulkList[Random.Range(0, BulkList.Count)];
					PlayersToSpawnAsSCP.Add(Ply);
					BulkList.Remove(Ply); // Removing the winners from the bulk list
				}
			}
			if (ScientistsToSpawn != 0)
			{
				for (int x = 0; x < ScientistsToSpawn; x++)
				{
					Player Ply = BulkList[Random.Range(0, BulkList.Count)];
					PlayersToSpawnAsScientist.Add(Ply);
					BulkList.Remove(Ply); // Removing the winners from the bulk list
				}
			}
			if (GuardsToSpawn != 0)
			{
				for (int x = 0; x < GuardsToSpawn; x++)
				{
					Player Ply = BulkList[Random.Range(0, BulkList.Count)];
					PlayersToSpawnAsGuard.Add(Ply);
					BulkList.Remove(Ply); // Removing the winners from the bulk list
				}
			}
			Log.Debug($"Filled blanks: Players (SCP/CD/SC/MTF): {SCPPlayers.Count()}, {ClassDPlayers.Count()}, {ScientistPlayers.Count()}, {GuardPlayers.Count()}", Config.ShowDebug);
			// ---------------------------------------------------------------------------------------\\
			// Okay we have the list! Time to spawn everyone in, we'll leave SCP for last as it has a bit of logic.

			Timing.CallDelayed(1.5f, () =>
			{
				foreach (Player ply in PlayersToSpawnAsClassD)
				{
					Log.Debug($"spawning {ply.Nickname} as CD", Config.ShowDebug);
					ply.SetRole(RoleType.ClassD);
					Log.Debug("spawned", Config.ShowDebug);
				}

				foreach (Player ply in PlayersToSpawnAsScientist)
				{
					Log.Debug($"spawning {ply.Nickname} as SC", Config.ShowDebug);
					ply.SetRole(RoleType.Scientist);
					Log.Debug("spawned", Config.ShowDebug);
				}

				foreach (Player ply in PlayersToSpawnAsGuard)
				{
					Log.Debug($"spawning {ply.Nickname} as Guard");
					ply.SetRole(RoleType.FacilityGuard);
					Log.Debug("spawned", Config.ShowDebug);
				}

				// ---------------------------------------------------------------------------------------\\

				// SCP Logic, preventing SCP-079 from spawning if there isn't at least 2 other SCPs
				List<RoleType> Roles = new List<RoleType>
				{
					RoleType.Scp049, RoleType.Scp096, RoleType.Scp106, RoleType.Scp173, RoleType.Scp93953,
					RoleType.Scp93989
				};

				if (PlayersToSpawnAsSCP.Count > 2)
					Roles.Add(RoleType.Scp079);

				foreach (Player ply in PlayersToSpawnAsSCP)
				{
					RoleType role = Roles[Random.Range(0, Roles.Count)];
					Roles.Remove(role);

					Log.Debug($"spawning {ply.Nickname} as scp {role.ToString()}", Config.ShowDebug);
					ply.SetRole(role);
					Log.Debug("spawned", Config.ShowDebug);
				}

				Timing.CallDelayed(1f, () =>
				{
					Round.IsLocked = false;
					Scp096.TurnedPlayers.Clear();
					Scp173.TurnedPlayers.Clear();
				});

				// I will come back to this later
				/*
				var test = new RoundSummary.SumInfo_ClassList // Still don't know if this does anything
				{
					class_ds = ClassDsToSpawn,
					scientists = ScientistsToSpawn,
					scps_except_zombies = SCPsToSpawn,
					mtf_and_guards = GuardsToSpawn
				};
				RoundSummary.singleton.SetStartClassList(test);*/
			});
		}

		public void OnWaitingForPlayers()
		{
			Round.IsLocked = true;

			SCPsToSpawn = 0;
			ClassDsToSpawn = 0;
			ScientistsToSpawn = 0;
			GuardsToSpawn = 0;


			Dictionary<RoleType, string> dummiesToSpawn = new Dictionary<RoleType, string>
			{
				{ RoleType.Tutorial, Config.RandomTeamDummy },
				{ RoleType.ClassD, Config.ClassDTeamDummy },
				{ RoleType.Scp173, Config.SCPTeamDummy },
				{ RoleType.Scientist, Config.ScientistTeamDummy },
				{ RoleType.FacilityGuard, Config.MTFTeamDummy },
			};


			GameObject.Find("StartRound").transform.localScale = Vector3.zero;

			if (lobbyTimer.IsRunning)
			{
				Timing.KillCoroutines(lobbyTimer);
			}
			lobbyTimer = Timing.RunCoroutine(LobbyTimer());

			foreach (var Role in dummiesToSpawn)
			{
				GameObject obj = UnityEngine.Object.Instantiate(
					NetworkManager.singleton.playerPrefab);
				CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
				if (ccm == null)
					Log.Error("CCM is null, this can cause problems!");
				ccm.CurClass = Role.Key;
				ccm.GodMode = true;
				//ccm.OldRefreshPlyModel(PlayerManager.localPlayer);
				obj.GetComponent<NicknameSync>().Network_myNickSync = Role.Value;
				obj.GetComponent<QueryProcessor>().PlayerId = 9999;
				obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
				obj.transform.localScale = new Vector3(2.3f, 2.3f, 2.3f);

				obj.transform.position = dummySpawnPointsAndRotations[Role.Key].Key;
				obj.transform.rotation = dummySpawnPointsAndRotations[Role.Key].Value;

				NetworkServer.Spawn(obj);
				Dummies.Add(obj);
				DummiesManager.dummies.Add(obj, obj.GetComponent<ReferenceHub>());
				
				var pickup = Item.Create(ItemType.SCP018).Spawn(dummySpawnPointsAndRotations[Role.Key].Key);
				GameObject gameObject = pickup.Base.gameObject;
				gameObject.transform.localScale = new Vector3(30f, 0.1f, 30f);

				var light = Light.Create(dummySpawnPointsAndRotations[Role.Key].Key, dummySpawnPointsAndRotations[Role.Key].Value.eulerAngles);
				light.Intensity = 4f;
				light.Range = 10f;
				switch (Role.Key.GetTeam())
				{
					case Team.RSC:
						light.Color = Color.white;
						break;
					case Team.MTF:
						light.Color = Color.gray;
						break;
					case Team.SCP:
						light.Color = Color.red;
						break;
					case Team.CDP:
						light.Color = Color.yellow;
						break;
					default:
						light.Color = Color.blue;
						break;
				}
				NetworkServer.UnSpawn(gameObject);
				NetworkServer.Spawn(pickup.Base.gameObject);
				Dummies.Add(pickup.Base.gameObject);

				boll.Add(pickup);

				Rigidbody rigidBody = pickup.Base.gameObject.GetComponent<Rigidbody>();
				Collider[] collider = pickup.Base.gameObject.GetComponents<Collider>();
				foreach (Collider thing in collider)
				{
					thing.enabled = false;
				}
				if (rigidBody != null)
				{
					rigidBody.useGravity = false;
					rigidBody.detectCollisions = false;
				}
				pickup.Base.transform.localPosition = dummySpawnPointsAndRotations[Role.Key].Key + Vector3.down * 3.3f;
			}
		}

		private IEnumerator<float> LobbyTimer()
		{
			StringBuilder message = new StringBuilder();
			var text = $"\n\n\n\n\n\n\n\n\n<b>{Config.DiscordInvite}</b>\n<color=%rainbow%><b>{Config.UpperText}\n{Config.BottomText}</b></color>";
			int x = 0;
			string[] colors = { "#f54242", "#f56042", "#f57e42", "#f59c42", "#f5b942", "#f5d742", "#f5f542", "#d7f542", "#b9f542", "#9cf542", "#7ef542", "#60f542", "#42f542", "#42f560", "#42f57b", "#42f599", "#42f5b6", "#42f5d4", "#42f5f2", "#42ddf5", "#42bcf5", "#429ef5", "#4281f5", "#4263f5", "#4245f5", "#5a42f5", "#7842f5", "#9642f5", "#b342f5", "#d142f5", "#ef42f5", "#f542dd", "#f542c2", "#f542aa", "#f5428d", "#f5426f", "#f54251" };
			while (!Round.IsStarted)
			{
				message.Clear();
				for (int i = 0; i < 0; i++)
				{
					message.Append("\n");
				}

				message.Append($"<size=40><color=yellow><b>{Config.StartingSoonText}, %seconds</b></color></size>");

				short NetworkTimer = GameCore.RoundStart.singleton.NetworkTimer;

				switch (NetworkTimer)
				{
					case -2: message.Replace("%seconds", Config.PausedServer); break;

					case -1: message.Replace("%seconds", Config.RoundStarted); break;

					case 1: message.Replace("%seconds", $"{NetworkTimer} {Config.SecondRemain}"); break;

					case 0: message.Replace("%seconds", Config.RoundStarted); break;

					default: message.Replace("%seconds", $"{NetworkTimer} {Config.SecondsRemain}"); break;
				}

				message.Append($"\n<size=30><i>%players</i></size>");

				if (Player.List.Count() == 1) message.Replace("%players", $"{Player.List.Count()} {Config.PlayerHasConnected}");
				else message.Replace("%players", $"{Player.List.Count()} {Config.PlayersHaveConnected}");

				message.Append(text.Replace("%rainbow%", colors[x++ % colors.Length]));

				foreach (Player ply in Player.List)
				{
					ply.ShowHint(message.ToString(), 1f);

					if (!Config.VotingBroadcast)
						continue;

					if (Vector3.Distance(ply.Position, SCPPoint) <= 3)
					{
						ply.Broadcast(1, $"<i>{Config.SCPTeam}</i>");
					}
					else if (Vector3.Distance(ply.Position, ClassDPoint) <= 3)
					{
						ply.Broadcast(1, $"<i>{Config.ClassDTeam}</i>");
					}
					else if (Vector3.Distance(ply.Position, ScientistPoint) <= 3)
					{
						ply.Broadcast(1, $"<i>{Config.ScientistTeam}</i>");
					}
					else if (Vector3.Distance(ply.Position, GuardPoint) <= 3)
					{
						ply.Broadcast(1, $"<i>{Config.GuardTeam}</i>");
					}
					else
					{
						ply.Broadcast(1, $"<i>{Config.RandomTeam}</i>");
					}
				}
				x++;
				yield return Timing.WaitForSeconds(1f);
			}
		}
	}
}
