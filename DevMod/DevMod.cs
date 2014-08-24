using Patcher;
using TowerFall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace DevMod
{
	[Patch]
	public class MyPlayer : Player
	{
		public MyPlayer(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, Player.HatState hatState, bool frozen = true)
			: base(playerIndex, position, allegiance, teamColor, inventory, hatState, frozen)
		{
		}

		public override void Update()
		{
			base.Update();
			if (((MyPlayerInput)TFGame.PlayerInputs[this.PlayerIndex]).SlowButton) {
				Level.OrbLogic.DoTimeOrb(delay: false);
			}
			if (((MyPlayerInput)TFGame.PlayerInputs[this.PlayerIndex]).GifButton) {
				Level.Session.EndRound();
			}
		}

	}

	[Patch]
	public class MyXGamepadData : MInput.XGamepadData
	{
		public MyXGamepadData(PlayerIndex playerIndex)
			: base(playerIndex)
		{
		}

		public bool BackPressed {
			get { return this.CurrentState.Buttons.Back == ButtonState.Pressed && this.PreviousState.Buttons.Back == ButtonState.Released; }
		}

		public bool CenterPressed {
			get { return this.CurrentState.Buttons.BigButton == ButtonState.Pressed && this.PreviousState.Buttons.BigButton == ButtonState.Released; }
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

		public virtual bool GifButton {
			get {
				if ((PlayerInput)this is XGamepadInput)
					return ((MyXGamepadData)((XGamepadInput)(PlayerInput)this).XGamepad).CenterPressed;
				else
					return false;
			}
		}

	}

	[Patch]
	public class MyKeyboardInput : KeyboardInput
	{
		public new static void LoadConfigs()
		{
			Configs = new[] {
				new KeyboardConfig {
					Dodge = new[]{ Keys.LeftShift },
					Down = new[]{ Keys.Down },
					Jump = new[]{ Keys.C },
					Left = new[]{ Keys.Left },
					Right = new[]{ Keys.Right },
					Shoot = new[]{ Keys.X },
					Up = new[]{ Keys.Up }
				},
				new KeyboardConfig {
					Dodge = new[]{ Keys.RightShift },
					Down = new[]{ Keys.S },
					Jump = new[]{ Keys.J },
					Left = new[]{ Keys.A },
					Right = new[]{ Keys.D },
					Shoot = new[]{ Keys.K },
					Up = new[]{ Keys.W }
				}
			};
		}
	}
}
