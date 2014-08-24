using TowerFall;
using Patcher;


namespace Mod
{
	// FIXME: still needs spawn points for 3rd and 4th player etc.
	//[Patch]
	public class MyRollcallElement : RollcallElement
	{
		public MyRollcallElement(int playerIndex)
			: base(playerIndex)
		{
		}

		public override int MaxPlayers {
			get {
				return (MainMenu.RollcallMode == MainMenu.RollcallModes.Trials) ? 1 : 4;
			}
		}
	}

	[Patch]
	public class MyQuestRoundLogic : QuestRoundLogic
	{
		public MyQuestRoundLogic(Session session)
			: base(session)
		{
		}

		public override void OnLevelLoadFinish()
		{
			base.OnLevelLoadFinish();

			base.Players = 0;
			for (int i = 0; i < 4; i++) {
				if (TFGame.Players[i]) {
					base.Players++;
					if (base.Players <= 2) {
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
