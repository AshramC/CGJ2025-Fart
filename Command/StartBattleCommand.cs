using QFramework;

namespace FartGame
{
    public class StartBattleCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var gameModel = this.GetModel<GameModel>();
            gameModel.CurrentGameState.Value = GameState.Battle;
            
            Debug.Log("[StartBattleCommand] 切换到战斗状态");
        }
    }
}
