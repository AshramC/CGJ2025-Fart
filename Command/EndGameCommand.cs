using QFramework;

namespace FartGame
{
    public class EndGameCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var gameModel = this.GetModel<GameModel>();
            var playerModel = this.GetModel<PlayerModel>();
            
            // 关闭熏模式
            playerModel.IsFumeMode.Value = false;
            
            // 切换到游戏结束状态
            gameModel.CurrentGameState.Value = GameState.GameOver;
            gameModel.IsPaused.Value = false;
            
            // 可以在这里添加游戏结束的其他逻辑
            // 比如保存分数、显示结算界面等
        }
    }
}
