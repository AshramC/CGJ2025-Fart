using QFramework;
using FartGame.Battle;

namespace FartGame
{
    public class EndBattleCommand : AbstractCommand
    {
        private readonly BattleResult mBattleResult;
        
        public EndBattleCommand(BattleResult result = null)
        {
            mBattleResult = result;
        }
        
        protected override void OnExecute()
        {
            var gameModel = this.GetModel<GameModel>();
            gameModel.CurrentGameState.Value = GameState.Gameplay;
            
            // 发送战斗结束事件
            if (mBattleResult != null)
            {
                this.SendEvent(new BattleCompletedEvent { Result = mBattleResult });
            }
        }
    }
}
