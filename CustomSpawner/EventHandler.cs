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
using Mirror.LiteNetLib4Mirror;
using MonoMod.Utils;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace CustomSpawner
{
	using Exiled.API.Features.Pickups;
	using Exiled.API.Features.Roles;
	using Exiled.Events.EventArgs.Player;
	using Exiled.Events.Handlers;
	using PlayerRoles;
	using Item = Exiled.API.Features.Items.Item;
	using Player = Exiled.API.Features.Player;

	public class EventHandler
	{
		public EventHandler()
		{
			SpawnPoint = Config.SpawnPoint;
			ClassDPoint = Config.ClassDPoint;
			GuardPoint = Config.GuardPoint;
			Tutorial = Config.Tutorial;
			SCPPoint = Config.SCPPoint;
			ScientistPoint = Config.ScientistPoint;
			dummySpawnPointsAndRotations.Clear();
			dummySpawnPointsAndRotations.AddRange(new Dictionary<RoleTypeId, (Vector3, Quaternion)>()
			{
				{RoleTypeId.ClassD, (Config.ClassDPoint, Quaternion.Euler(Config.ClassDRotation))},
				{RoleTypeId.FacilityGuard, (Config.GuardPoint, Quaternion.Euler(Config.GuardRotation))},
				{RoleTypeId.Tutorial, (Config.Tutorial, Quaternion.Euler(Config.TutorialRotation))},
				{RoleTypeId.Scp173, (Config.SCPPoint, Quaternion.Euler(Config.SCPRotation))},
				{RoleTypeId.Scientist, (Config.ScientistPoint, Quaternion.Euler(Config.ScientistRotation))}
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

		Dictionary<RoleTypeId, (Vector3, Quaternion)> dummySpawnPointsAndRotations = new Dictionary<RoleTypeId, (Vector3, Quaternion)>();

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
					ev.Player.RoleManager.ServerSetRole(RoleTypeId.Tutorial, RoleChangeReason.None);
					Scp096Role.TurnedPlayers.Add(ev.Player);
					Scp173Role.TurnedPlayers.Add(ev.Player);
				});

				Timing.CallDelayed(1.5f, () =>
				{
					ev.Player.Position = SpawnPoint;
				});
			}
		}

		public void OnRoundStart()
		{
			if(Player.List.Any(x => x.Role.Type != RoleTypeId.Tutorial && x.Role.Type != RoleTypeId.Spectator))
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

			Log.Debug($"Player List count: {Player.List.Count()}");
			Log.Debug($"Config::Spawnquene count: {Config.SpawnQueue.Count()}");
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

			Log.Debug($"Calculated people to spawn! SCPS: {SCPsToSpawn}, CDs: {ClassDsToSpawn}, Guard: {GuardsToSpawn}, Science: {ScientistsToSpawn}");
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
					Log.Debug($"Added {player.Nickname} to scp spawnlist!");
				}
				else if (Vector3.Distance(player.Position, ClassDPoint) <= 3)
				{
					ClassDPlayers.Add(player);
					Log.Debug($"Added {player.Nickname} to cd spawnlist!");
				}
				else if (Vector3.Distance(player.Position, ScientistPoint) <= 3)
				{
					ScientistPlayers.Add(player);
					Log.Debug($"Added {player.Nickname} to sc spawnlist!");
				}
				else if (Vector3.Distance(player.Position, GuardPoint) <= 3)
				{
					GuardPlayers.Add(player);
					Log.Debug($"Added {player.Nickname} to guard spawnlist!");
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
			Log.Debug($"Filling blanks: Players (SCP/CD/SC/MTF): {SCPPlayers.Count()}, {ClassDPlayers.Count()}, {ScientistPlayers.Count()}, {GuardPlayers.Count()}");
			foreach (var playerPly in SCPPlayers)
			{
				Log.Debug($"{playerPly.Nickname}, {playerPly.UserId}");
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
			Log.Debug($"Filled blanks: Players (SCP/CD/SC/MTF): {SCPPlayers.Count()}, {ClassDPlayers.Count()}, {ScientistPlayers.Count()}, {GuardPlayers.Count()}");
			// ---------------------------------------------------------------------------------------\\
			// Okay we have the list! Time to spawn everyone in, we'll leave SCP for last as it has a bit of logic.

			Timing.CallDelayed(1.5f, () =>
			{
				foreach (Player ply in PlayersToSpawnAsClassD)
				{
					Log.Debug($"spawning {ply.Nickname} as CD");
					ply.RoleManager.ServerSetRole(RoleTypeId.ClassD, RoleChangeReason.RoundStart);
					Log.Debug("spawned");
				}

				foreach (Player ply in PlayersToSpawnAsScientist)
				{
					Log.Debug($"spawning {ply.Nickname} as SC");
					ply.RoleManager.ServerSetRole(RoleTypeId.Scientist, RoleChangeReason.RoundStart);
					Log.Debug("spawned");
				}

				foreach (Player ply in PlayersToSpawnAsGuard)
				{
					Log.Debug($"spawning {ply.Nickname} as Guard");
					ply.RoleManager.ServerSetRole(RoleTypeId.FacilityGuard, RoleChangeReason.RoundStart);
					Log.Debug("spawned");
				}

				// ---------------------------------------------------------------------------------------\\

				// SCP Logic, preventing SCP-079 from spawning if there isn't at least 2 other SCPs
				List<RoleTypeId> Roles = new List<RoleTypeId>
				{
					RoleTypeId.Scp049, RoleTypeId.Scp096, RoleTypeId.Scp106, RoleTypeId.Scp173, RoleTypeId.Scp939,
				};

				if (PlayersToSpawnAsSCP.Count > 2)
					Roles.Add(RoleTypeId.Scp079);

				foreach (Player ply in PlayersToSpawnAsSCP)
				{
					RoleTypeId role = Roles[Random.Range(0, Roles.Count)];
					Roles.Remove(role);

					Log.Debug($"spawning {ply.Nickname} as scp {role.ToString()}");
					ply.RoleManager.ServerSetRole(role, RoleChangeReason.RoundStart);
					Log.Debug("spawned");
				}

				Timing.CallDelayed(1f, () =>
				{
					Round.IsLocked = false;
					Scp096Role.TurnedPlayers.Clear();
					Scp173Role.TurnedPlayers.Clear();
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

		private int i = 0;
		public void OnWaitingForPlayers()
		{
			//Log.Info("1");
			Round.IsLocked = true;

			SCPsToSpawn = 0;
			ClassDsToSpawn = 0;
			ScientistsToSpawn = 0;
			GuardsToSpawn = 0;


			//Log.Info("2");
			Dictionary<RoleTypeId, string> dummiesToSpawn = new Dictionary<RoleTypeId, string>
			{
				{ RoleTypeId.Tutorial, Config.RandomTeamDummy },
				{ RoleTypeId.ClassD, Config.ClassDTeamDummy },
				{ RoleTypeId.Scp173, Config.SCPTeamDummy },
				{ RoleTypeId.Scientist, Config.ScientistTeamDummy },
				{ RoleTypeId.FacilityGuard, Config.MTFTeamDummy },
			};


			GameObject.Find("StartRound").transform.localScale = Vector3.zero;

			//Log.Info("3");
			if (lobbyTimer.IsRunning)
			{
				Timing.KillCoroutines(lobbyTimer);
			}
			lobbyTimer = Timing.RunCoroutine(LobbyTimer());

			//Log.Info("4");
			if(Config.EnableDummies)
			foreach (var Role in dummiesToSpawn)
			{
				//Log.Info("	4.1");
				GameObject obj = UnityEngine.Object.Instantiate(
					NetworkManager.singleton.playerPrefab);
				CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
				if (ccm == null)
					Log.Error("CCM is null, this can cause problems!");
				ccm.Hub.roleManager.CurrentRole = ccm.Hub.roleManager.GetRoleBase(Role.Key);
				ccm.GodMode = true;
				//ccm.OldRefreshPlyModel(PlayerManager.localPlayer);
				obj.GetComponent<NicknameSync>().Network_myNickSync = Role.Value;
				ccm.Hub._playerId = new RecyclablePlayerId(9999 + i);
				ccm.Hub.Network_playerId = new RecyclablePlayerId(9999 + i);
				obj.transform.localScale = new Vector3(2.3f, 2.3f, 2.3f);

				//Log.Info("	4.2");
				obj.transform.position = dummySpawnPointsAndRotations[Role.Key].Item1;
				obj.transform.rotation = dummySpawnPointsAndRotations[Role.Key].Item2;

				NetworkServer.Spawn(obj);
				Dummies.Add(obj);
				DummiesManager.dummies.Add(obj, obj.GetComponent<ReferenceHub>());
				
				//Log.Info("	4.3");
				var pickup = Item.Create(ItemType.SCP018).CreatePickup(dummySpawnPointsAndRotations[Role.Key].Item1);
				GameObject gameObject = pickup.Base.gameObject;
				gameObject.transform.localScale = new Vector3(30f, 0.1f, 30f);

				/*var light = Light.Create(dummySpawnPointsAndRotations[Role.Key].Item1, dummySpawnPointsAndRotations[Role.Key].Item2.eulerAngles);
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
				}*/
				//Log.Info("	4.4");
				NetworkServer.UnSpawn(gameObject);
				NetworkServer.Spawn(pickup.Base.gameObject);
				Dummies.Add(pickup.Base.gameObject);

				boll.Add(pickup);

				//Log.Info("	4.5");
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
				pickup.Base.transform.localPosition = dummySpawnPointsAndRotations[Role.Key].Item1 + Vector3.down * 3.3f;
				//Log.Info("	4.6");
				i++;
			}
		}

		private IEnumerator<float> LobbyTimer()
		{
			StringBuilder message = new StringBuilder();
			var text = $"\n\n\n\n\n\n\n\n\n<b>{Config.DiscordInvite ?? "null"}</b>\n<color=%rainbow%><b>{Config.UpperText}\n{Config.BottomText}</b></color>";
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
					//ply.ShowHint(message.ToString(), 1f);

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
