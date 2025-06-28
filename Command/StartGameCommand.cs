using QFramework;
using UnityEngine.SceneManagement;

namespace FartGame
{
    public class StartGameCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var gameModel = this.GetModel<GameModel>();
            var fartSystem = this.GetSystem<FartSystem>();
            
            // 通过System重置玩家数据
            fartSystem.ResetPlayerData();
            
            // 切换游戏状态
            gameModel.CurrentGameState.Value = GameState.Gameplay;
            gameModel.IsPaused.Value = false;
            
            // 如果需要加载游戏场景，取消注释下面的代码
            // SceneManager.LoadScene("GameplayScene");
        }
    }
}
