using QFramework;
using UnityEngine.SceneManagement;

namespace FartGame
{
    public class GameStateSystem : AbstractSystem
    {
        private GameModel mGameModel;
        
        protected override void OnInit()
        {
            mGameModel = this.GetModel<GameModel>();
            
            // 监听状态变化事件
            this.RegisterEvent<GameStateChangedEvent>(OnGameStateChanged);
        }
        
        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            switch (e.NewState)
            {
                case GameState.MainMenu:
                    HandleMainMenuState();
                    break;
                case GameState.Gameplay:
                    HandleGameplayState();
                    break;
                case GameState.Paused:
                    HandlePausedState();
                    break;
                case GameState.GameOver:
                    HandleGameOverState();
                    break;
            }
        }
        
        private void HandleMainMenuState()
        {
            // 重置游戏数据
            var fartSystem = this.GetSystem<FartSystem>();
            fartSystem.ResetPlayerData();
        }
        
        private void HandleGameplayState()
        {
            // 游戏开始时的初始化
            mGameModel.IsPaused.Value = false;
        }
        
        private void HandlePausedState()
        {
            mGameModel.IsPaused.Value = true;
        }
        
        private void HandleGameOverState()
        {
            // 游戏结束处理
            var playerModel = this.GetModel<PlayerModel>();
            playerModel.IsFumeMode.Value = false;
        }
    }
}
