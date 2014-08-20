using System;
using System.Collections.Generic;
using System.Linq;
using TowerFall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Patcher;
using Monocle;

namespace Mod
{
	[Patch]
	public class MyMatchVariants : MatchVariants
	{
		[Header("MODS")]
		public Variant NoHeadBounce;
		public Variant NoLedgeGrab;
		public Variant AwfullySlowArrows;
		public Variant AwfullyFastArrows;
		public Variant InfiniteArrows;
		public Variant NoDodgeCooldowns;
		[PerPlayer]
		public Variant GottaGoFast;

		public MyMatchVariants()
		{
			this.CreateLinks(NoHeadBounce, NoTimeLimit);
			this.CreateLinks(NoDodgeCooldowns, ShowDodgeCooldown);
			this.CreateLinks(AwfullyFastArrows, AwfullySlowArrows);
			this.CreateLinks(SpeedBoots, GottaGoFast);
		}
	}

	[Patch]
	public class MyXGamepadData : MInput.XGamepadData
	{
		public MyXGamepadData(PlayerIndex playerIndex) : base(playerIndex) {}

		public bool BackPressed {
			get { return this.CurrentState.Buttons.Back == ButtonState.Pressed && this.PreviousState.Buttons.Back == ButtonState.Released; }
		}
	}

	[Patch]
	public abstract class MyPlayerInput : PlayerInput
	{
		public virtual bool SlowButton {
			get {
				if ((PlayerInput)this is XGamepadInput)
					return ((MyXGamepadData)((XGamepadInput)(PlayerInput)this).XGamepad).BackPressed;
				else
					return false;
			}
		}
	}

	[Patch]
	public class MyPlayer : Player
	{
		public MyPlayer(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, Player.HatState hatState, bool frozen = true) : base(playerIndex, position, allegiance, teamColor, inventory, hatState, frozen) {}

		public override bool CanGrabLedge(int a, int b)
		{
			if (((MyMatchVariants)Level.Session.MatchSettings.Variants).NoLedgeGrab)
				return false;
			return base.CanGrabLedge(a, b);
		}

		public override int GetDodgeExitState()
		{
			if (((MyMatchVariants)Level.Session.MatchSettings.Variants).NoDodgeCooldowns) {
				this.DodgeCooldown();
			}
			return base.GetDodgeExitState();

		}

		public override float MaxRunSpeed
		{
			get
			{
				float res = base.MaxRunSpeed;
				if (((MyMatchVariants)Level.Session.MatchSettings.Variants).GottaGoFast[this.PlayerIndex]) {
					return res * 1.4f;
				}
				return res;
			}
		}

		public override int NormalUpdate()
		{
			// SpeedBoots add little dust clouds below our feet... we want those too.
			bool hasSpeedBoots = Level.Session.MatchSettings.Variants.SpeedBoots[this.PlayerIndex];
			if (((MyMatchVariants)Level.Session.MatchSettings.Variants).GottaGoFast[this.PlayerIndex]) {
				typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * 2f, null);
				Level.Session.MatchSettings.Variants.SpeedBoots[this.PlayerIndex] = true;
			}
			int res = base.NormalUpdate();
			if (((MyMatchVariants)Level.Session.MatchSettings.Variants).GottaGoFast[this.PlayerIndex]) {
				typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / 2f, null);
				Level.Session.MatchSettings.Variants.SpeedBoots[this.PlayerIndex] = hasSpeedBoots;
			}
			return res;
		}

		public override void ShootArrow()
		{
			ArrowTypes[] at = new ArrowTypes[1];
			if (((MyMatchVariants)Level.Session.MatchSettings.Variants).InfiniteArrows) {
				at[0] = this.Arrows.UseArrow();
				this.Arrows.AddArrows(at);
			}
			base.ShootArrow();
			if (((MyMatchVariants)Level.Session.MatchSettings.Variants).InfiniteArrows) {
				this.Arrows.AddArrows(at);
			}

		}

		public override void HurtBouncedOn(int bouncerIndex)
		{
			if (!((MyMatchVariants)Level.Session.MatchSettings.Variants).NoHeadBounce)
				base.HurtBouncedOn(bouncerIndex);
		}

		public override void Update()
		{
			base.Update();
			if (((MyPlayerInput)TFGame.PlayerInputs[this.PlayerIndex]).SlowButton)
				Level.OrbLogic.DoTimeOrb(delay: false);
		}
	}

	[Patch]
	public abstract class MyArrow : Arrow
	{
		const float AwfullySlowArrowMult = 0.2f;
		const float AwfullyFastArrowMult = 3.0f;

		public MyArrow() : base()
		{
			if (((MyMatchVariants)Level.Session.MatchSettings.Variants).AwfullyFastArrows) {
				this.NormalHitbox = new WrapHitbox(6f, 3f, -1f, -1f);
				this.otherArrowHitbox = new WrapHitbox(12f, 4f, -2f, -2f);
			}
		}

		public override void ArrowUpdate()
		{
			if (((MyMatchVariants)Level.Session.MatchSettings.Variants).AwfullySlowArrows) {
				// Engine.TimeMult *= AwfullySlowArrowMult;
				typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * AwfullySlowArrowMult, null);
				base.ArrowUpdate();
				// Engine.TimeMult /= AwfullySlowArrowMult;
				typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / AwfullySlowArrowMult, null);
			}
			else if (((MyMatchVariants)Level.Session.MatchSettings.Variants).AwfullyFastArrows) {
				typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * AwfullyFastArrowMult, null);
				base.ArrowUpdate();
				typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / AwfullyFastArrowMult, null);
			} else
				base.ArrowUpdate();
		}
	}

	[Patch]
	public class MyKeyboardInput : KeyboardInput
	{
		public new static void LoadConfigs()
		{
			var p2Config = new KeyboardConfig {
				Dodge = new[]{ Keys.RightShift },
				Down = new[]{ Keys.S },
				Jump = new[]{ Keys.J },
				Left = new[]{ Keys.A },
				Right = new[]{ Keys.D },
				Shoot = new[]{ Keys.K },
				Up = new[]{ Keys.W }
			};
			Configs = new[]{ new KeyboardConfig(), p2Config };
		}
	}


	public class MyRollcallElement : RollcallElement
	{
		public MyRollcallElement(int playerIndex) : base(playerIndex) { }

		public override int MaxPlayers
		{
			get
			{
				return (MainMenu.RollcallMode == MainMenu.RollcallModes.Trials) ? 1 : 4;
			}
		}
	}

	[Patch]
	public class MyQuestRoundLogic : QuestRoundLogic
	{
		public MyQuestRoundLogic(Session session) : base(session) { }

        public override void OnLevelLoadFinish()
        {
            base.OnLevelLoadFinish();

            base.Players = 0;
            for (int i = 0; i < 4; i++)
            {
                if (TFGame.Players[i])
                {
                    base.Players++;
                    if (base.Players <= 2)
                    {
                        // the first two players are already taken care of by base.
                        continue;
                    }
                    // This patch doesn't work with the injector because it doesn't yet support generics
                    //base.Session.CurrentLevel.Add<QuestPlayerHUD>(this.PlayerHUDs[i] = new QuestPlayerHUD(this, (base.Players % 2 == 0) ? Facing.Left : Facing.Right, i));
                    this.SpawnPlayer(i, false);
                }
            }
        }
    }

	[Patch]
	public class MyRoundLogic : RoundLogic
	{
		protected MyRoundLogic(Session session, bool canHaveMiasma)
			: base(session, canHaveMiasma)
		{
		}

		public new static RoundLogic GetRoundLogic(Session session)
		{
			if (session.MatchSettings.Mode == RespawnRoundLogic.Mode)
				return new RespawnRoundLogic(session);
			else
				return RoundLogic.GetRoundLogic(session);
		}
	}

	[Patch]
	public class MyVersusModeButton : VersusModeButton
	{
		public MyVersusModeButton(Vector2 position, Vector2 tweenFrom)
			: base(position, tweenFrom)
		{
		}

		public new static string GetModeName(Modes mode)
		{
			if (mode == RespawnRoundLogic.Mode)
				return "RESPAWN";
			else
				return VersusModeButton.GetModeName(mode);
		}

		public new static Subtexture GetModeIcon(Modes mode)
		{
			if (mode == RespawnRoundLogic.Mode)
				return TFGame.MenuAtlas["gameModes/headhunters"];
			else
				return VersusModeButton.GetModeIcon(mode);
		}

		public override void Update()
		{
			Modes mode = MainMenu.VersusMatchSettings.Mode;
			if (this.Selected) {
				if (mode >= Modes.TeamDeathmatch && MenuInput.Right) {
					if (mode == RespawnRoundLogic.Mode) {
						return;
					}
					MainMenu.VersusMatchSettings.Mode = RespawnRoundLogic.Mode;
					Sounds.ui_move2.Play(160f, 1f);
					this.iconWiggler.Start();
					base.OnConfirm();
					this.UpdateSides();
					return;
				} else if (mode == RespawnRoundLogic.Mode && MenuInput.Left) {
					MainMenu.VersusMatchSettings.Mode = Modes.TeamDeathmatch;
					Sounds.ui_move2.Play(160f, 1f);
					this.iconWiggler.Start();
					base.OnConfirm();
					this.UpdateSides();
					return;
				}
			}
			base.Update();
		}

		public override void UpdateSides()
		{
			base.UpdateSides();
			this.DrawRight = (MainMenu.VersusMatchSettings.Mode < RespawnRoundLogic.Mode);
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
				if (this.Mode == RespawnRoundLogic.Mode) {
					int goals = this.PlayerGoals(5, 8, 10);
					return (int)Math.Ceiling(((float)goals * MatchSettings.GoalMultiplier[(int)this.MatchLength]));
				} else {
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

		public override void Update()
		{
			if (MainMenu.VersusMatchSettings.Mode == RespawnRoundLogic.Mode) {
				var coinSprite = this.coinSprite;
				this.coinSprite = this.skullSprite;
				base.Update();
				this.coinSprite = coinSprite;
			} else {
				base.Update();
			}
		}
	}

	public class RespawnRoundLogic : RoundLogic
	{
		public static readonly Modes Mode = (Modes)42;

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

		public override bool CheckForAllButOneDead()
		{
			return false;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (base.RoundStarted && base.Session.CurrentLevel.Ending && base.Session.CurrentLevel.CanEnd)
			{
				if (this.endDelay)
				{
					this.endDelay.Update();
					return;
				}
				base.Session.EndRound();
			}
		}

		public override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex)
		{
			base.OnPlayerDeath(player, corpse, playerIndex, cause, position, killerIndex);
			List<Vector2> xMLPositions = this.Session.CurrentLevel.GetXMLPositions("PlayerSpawn");
			xMLPositions.Shuffle(new Random());
			Player entity = new Player(playerIndex, xMLPositions[0], Allegiance.Neutral, Allegiance.Neutral,
				                this.Session.GetPlayerInventory(playerIndex), this.Session.GetSpawnHatState(playerIndex), frozen: false);
			this.Session.CurrentLevel.Add(entity);

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
}
