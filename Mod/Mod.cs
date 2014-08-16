using System;
using TowerFall;
using Microsoft.Xna.Framework;
using Patcher;

namespace Mod
{
	[Patch]
	public class MyMatchVariants : MatchVariants
	{
		[Header("MODS")]
		public Variant NoHeadBounce;
		public Variant NoLedgeGrab;
		public Variant InfiniteArrows;
		public Variant NoDodgeCooldowns;

		public MyMatchVariants()
		{
			this.CreateLinks(NoHeadBounce, NoTimeLimit);
			this.CreateLinks(NoDodgeCooldowns, ShowDodgeCooldown);
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
	}
}
