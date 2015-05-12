using Patcher;
using TowerFall;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;

namespace Mod
{
	[Patch]
	public class MyRoundLogic : RoundLogic
	{
		protected MyRoundLogic(Session session, bool canHaveMiasma)
			: base(session, canHaveMiasma)
		{
		}

		public new static RoundLogic GetRoundLogic(Session session)
		{
			switch (session.MatchSettings.Mode) {
				case RespawnRoundLogic.Mode:
					return new RespawnRoundLogic(session);
				case MobRoundLogic.Mode:
					return new MobRoundLogic(session);
				default:
					return RoundLogic.GetRoundLogic(session);
			}
		}
	}

	[Patch]
	public class MyVersusModeButton : VersusModeButton
	{
		static List<Modes> VersusModes = new List<Modes> {
			Modes.LastManStanding, Modes.HeadHunters, Modes.TeamDeathmatch,
			RespawnRoundLogic.Mode,	MobRoundLogic.Mode
		};

		public MyVersusModeButton(Vector2 position, Vector2 tweenFrom)
			: base(position, tweenFrom)
		{
		}

		public new static string GetModeName(Modes mode)
		{
			switch (mode) {
				case RespawnRoundLogic.Mode:
					return "RESPAWN";
				case MobRoundLogic.Mode:
					return "CRAWL";
				default:
					return VersusModeButton.GetModeName(mode);
			}
		}

		public new static Subtexture GetModeIcon(Modes mode)
		{
			switch (mode) {
				case RespawnRoundLogic.Mode:
					return TFGame.MenuAtlas["gameModes/respawn"];
				case MobRoundLogic.Mode:
					return TFGame.MenuAtlas["gameModes/crawl"];
				default:
					return VersusModeButton.GetModeIcon(mode);
			}
		}

		// completely re-write to make it enum-independent
		public override void Update()
		{
			// skip original implementation
			Patcher.Patcher.CallRealBase();

			Modes mode = MainMenu.VersusMatchSettings.Mode;
			if (this.Selected) {
				int idx = VersusModes.IndexOf(mode);
				if (idx < VersusModes.Count - 1 && MenuInput.Right) {
					MainMenu.VersusMatchSettings.Mode = VersusModes[idx + 1];
					Sounds.ui_move2.Play(160f, 1f);
					this.iconWiggler.Start();
					base.OnConfirm();
					this.UpdateSides();
				} else if (idx > 0 && MenuInput.Left) {
					MainMenu.VersusMatchSettings.Mode = VersusModes[idx - 1];
					Sounds.ui_move2.Play(160f, 1f);
					this.iconWiggler.Start();
					base.OnConfirm();
					this.UpdateSides();
				}
			}
		}

		public override void UpdateSides()
		{
			base.UpdateSides();
			this.DrawRight = (MainMenu.VersusMatchSettings.Mode < VersusModes[VersusModes.Count-1]);
		}
	}

	[Patch]
	public class MyMatchSettings : MatchSettings
	{
		public MyMatchSettings(LevelSystem levelSystem, Modes mode, MatchSettings.MatchLengths matchLength)
			: base(levelSystem, mode, matchLength)
		{
		}

		public override int GoalScore {
			get {
				switch (this.Mode) {
					case RespawnRoundLogic.Mode:
					case MobRoundLogic.Mode:
						int goals = this.PlayerGoals(5, 8, 10);
						return (int)Math.Ceiling(((float)goals * MatchSettings.GoalMultiplier[(int)this.MatchLength]));
					default:
						return base.GoalScore;
				}
			}
		}
	}

	[Patch]
	public class MyVersusCoinButton : VersusCoinButton
	{
		public MyVersusCoinButton(Vector2 position, Vector2 tweenFrom)
			: base(position, tweenFrom)
		{
		}

		public override void Render()
		{
			var mode = MainMenu.VersusMatchSettings.Mode;
			if (mode == RespawnRoundLogic.Mode || mode == MobRoundLogic.Mode) {
				MainMenu.VersusMatchSettings.Mode = Modes.HeadHunters;
				base.Render();
				MainMenu.VersusMatchSettings.Mode = mode;
			} else {
				base.Render();
			}
		}
	}

	[Patch]
	public class MyVersusRoundResults : VersusRoundResults
	{
		private Modes _oldMode;

		public MyVersusRoundResults(Session session, List<EventLog> events)
			: base(session, events)
		{
			this._oldMode = session.MatchSettings.Mode;
			if (this._oldMode == RespawnRoundLogic.Mode || this._oldMode == MobRoundLogic.Mode)
				session.MatchSettings.Mode = Modes.HeadHunters;
		}

		public override void TweenOut()
		{
			this.session.MatchSettings.Mode = this._oldMode;
			base.TweenOut();
		}
	}

	public class RespawnRoundLogic : RoundLogic
	{
		public const Modes Mode = (Modes)42;

		private KillCountHUD[] killCountHUDs = new KillCountHUD[4];
		private bool wasFinalKill;
		private Counter endDelay;

		public RespawnRoundLogic(Session session)
			: base(session, canHaveMiasma: false)
		{
			for (int i = 0; i < 4; i++) {
				if (TFGame.Players[i]) {
					killCountHUDs[i] = new KillCountHUD(i);
					this.Session.CurrentLevel.Add(killCountHUDs[i]);
				}
			}
			this.endDelay = new Counter();
			this.endDelay.Set(90);
		}

		public override void OnLevelLoadFinish()
		{
			base.OnLevelLoadFinish();
			base.Session.CurrentLevel.Add<VersusStart>(new VersusStart(base.Session));
			base.Players = base.SpawnPlayersFFA();
		}

		public override bool FFACheckForAllButOneDead()
		{
			return false;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (base.RoundStarted && base.Session.CurrentLevel.Ending && base.Session.CurrentLevel.CanEnd) {
				if (this.endDelay) {
					this.endDelay.Update();
					return;
				}
				base.Session.EndRound();
			}
		}

		protected Player RespawnPlayer(int playerIndex)
		{
			List<Vector2> spawnPositions = this.Session.CurrentLevel.GetXMLPositions("PlayerSpawn");

			var player = new Player(playerIndex, new Random().Choose(spawnPositions), Allegiance.Neutral, Allegiance.Neutral,
				            this.Session.GetPlayerInventory(playerIndex), this.Session.GetSpawnHatState(playerIndex),
							frozen: false, flash: false, indicator: true);
			this.Session.CurrentLevel.Add(player);
			player.Flash(120, null);
			Alarm.Set(player, 60, player.RemoveIndicator, Alarm.AlarmMode.Oneshot);
			return player;
		}

		protected virtual void AfterOnPlayerDeath(Player player)
		{
			this.RespawnPlayer(player.PlayerIndex);
		}

		public override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex)
		{
			base.OnPlayerDeath(player, corpse, playerIndex, cause, position, killerIndex);

			if (killerIndex == playerIndex || killerIndex == -1) {
				killCountHUDs[playerIndex].Decrease();
				base.AddScore(playerIndex, -1);
			} else if (killerIndex != -1) {
				killCountHUDs[killerIndex].Increase();
				base.AddScore(killerIndex, 1);
			}

			int winner = base.Session.GetWinner();
			if (this.wasFinalKill && winner == -1) {
				this.wasFinalKill = false;
				base.Session.CurrentLevel.Ending = false;
				base.CancelFinalKill();
				this.endDelay.Set(90);
			}
			if (!this.wasFinalKill && winner != -1) {
				base.Session.CurrentLevel.Ending = true;
				this.wasFinalKill = true;
				base.FinalKill(corpse, winner);
			}

			this.AfterOnPlayerDeath(player);
		}
	}

	public class KillCountHUD : Entity
	{
		int playerIndex;
		List<Sprite<int>> skullIcons = new List<Sprite<int>>();

		public int Count { get { return this.skullIcons.Count; } }

		public KillCountHUD(int playerIndex)
			: base(3)
		{
			this.playerIndex = playerIndex;
		}

		public void Increase()
		{
			Sprite<int> sprite = DeathSkull.GetSprite();

			if (this.playerIndex % 2 == 0) {
				sprite.X = 8 + 10 * skullIcons.Count;
			} else {
				sprite.X = 320 - 8 - 10 * skullIcons.Count;
			}

			sprite.Y = this.playerIndex / 2 == 0 ? 20 : 240 - 20;
			//sprite.Play(0, restart: false);
			sprite.Stop();
			this.skullIcons.Add(sprite);
			base.Add(sprite);
		}

		public void Decrease()
		{
			if (this.skullIcons.Any()) {
				base.Remove(this.skullIcons.Last());
				this.skullIcons.Remove(this.skullIcons.Last());
			}
		}

		public override void Render()
		{
			foreach (Sprite<int> sprite in this.skullIcons) {
				sprite.DrawOutline(1);
			}
			base.Render();
		}
	}

	[Patch]
	public class MyPlayerGhost : PlayerGhost
	{
		PlayerCorpse corpse;

		public MyPlayerGhost(PlayerCorpse corpse)
			: base(corpse)
		{
			this.corpse = corpse;
		}

		public override void Die(int killerIndex, Arrow arrow, Explosion explosion, ShockCircle circle)
		{
			base.Die(killerIndex, arrow, explosion, circle);
			var mobLogic = this.Level.Session.RoundLogic as MobRoundLogic;
			if (mobLogic != null) {
				mobLogic.OnPlayerDeath(
					null, this.corpse, this.PlayerIndex, DeathCause.Arrow, // FIXME
					this.Position, killerIndex
				);
			}
		}
	}

	public class MobRoundLogic : RespawnRoundLogic
	{
		public new const Modes Mode = (Modes)43;

		PlayerGhost[] activeGhosts = new PlayerGhost[4];

		public MobRoundLogic(Session session)
			: base(session)
		{
		}

		protected override void AfterOnPlayerDeath(Player player)
		{
		}

		void RemoveGhostAndRespawn(int playerIndex, Vector2 position=default(Vector2))
		{
			if (activeGhosts[playerIndex] != null) {
				var ghost = activeGhosts[playerIndex];
				var player = this.RespawnPlayer(playerIndex);
				// if we've been given a position, make sure the ghost spawns at that position and
				// retains its speed pre-spawn.
				if (position != default(Vector2)) {
					player.Position.X = position.X;
					player.Position.Y = position.Y;

					player.Speed.X = ghost.Speed.X;
					player.Speed.Y = ghost.Speed.Y;
				}
				activeGhosts[playerIndex].RemoveSelf();
				activeGhosts[playerIndex] = null;
			}
		}

		public override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex)
		{
			base.OnPlayerDeath(player, corpse, playerIndex, cause, position, killerIndex);
			this.Session.CurrentLevel.Add(activeGhosts[playerIndex] = new PlayerGhost(corpse));

			if (killerIndex == playerIndex || killerIndex == -1) {
				if (this.Session.CurrentLevel.LivingPlayers == 0) {
					var otherPlayers = TFGame.Players.Select((playing, idx) => playing && idx != playerIndex ? (int?)idx : null).Where(idx => idx != null).ToList();
					var randomPlayer = new Random().Choose(otherPlayers).Value;
					RemoveGhostAndRespawn(randomPlayer);
				}
			} else {
				RemoveGhostAndRespawn(killerIndex, position);
			}
		}
	}
}
