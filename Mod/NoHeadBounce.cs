using System;
using TowerFall;
using Microsoft.Xna.Framework;

namespace Mod
{
	public class MyMatchVariants : MatchVariants
	{
		[Header("MODS")]
		public Variant NoHeadBounce;

		public MyMatchVariants()
		{
			this.CreateLinks(NoHeadBounce, NoTimeLimit);
		}
	}

	public class MyPlayer : Player
	{
		public MyPlayer(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, Player.HatState hatState, bool frozen = true) : base(playerIndex, position, allegiance, teamColor, inventory, hatState, frozen) {}

		public override void HurtBouncedOn(int bouncerIndex)
		{
			if (!((MyMatchVariants)Level.Session.MatchSettings.Variants).NoHeadBounce)
				base.HurtBouncedOn(bouncerIndex);
		}
	}
}