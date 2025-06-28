using QFramework;

namespace FartGame
{
    public class PauseGameCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var gameModel = this.GetModel<GameModel>();
            
            // 只在游戏进行中才能暂停
            if (gameModel.CurrentGameState.Value != GameState.Gameplay)
                return;
            
            // 切换暂停状态
            gameModel.IsPaused.Value = !gameModel.IsPaused.Value;
            
            // 如果暂停了，切换到暂停状态；否则恢复游戏状态
            if (gameModel.IsPaused.Value)
            {
                gameModel.CurrentGameState.Value = GameState.Paused;
            }
            else
            {
                gameModel.CurrentGameState.Value = GameState.Gameplay;
            }
        }
    }
}
