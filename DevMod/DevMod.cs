using Patcher;
using TowerFall;
using Microsoft.Xna.Framework.Input;

namespace DevMod
{
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
