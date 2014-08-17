using System;
using System.Collections.Generic;
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

}
