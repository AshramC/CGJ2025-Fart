using QFramework;
using UnityEngine;
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
                case GameState.Battle:
                    HandleBattleState();
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
        
        private void HandleBattleState()
        {
            // 进入战斗状态处理
            Debug.Log("[GameStateSystem] 进入战斗状态");
            
            // 暂停主游戏系统更新
            mGameModel.IsPaused.Value = false; // 战斗状态不算暂停，但需要区分于普通游戏
            
            // 监听战斗完成事件
            this.RegisterEvent<BattleCompletedEvent>(OnBattleCompleted);
        }
        
        private void OnBattleCompleted(BattleCompletedEvent e)
        {
            Debug.Log($"[GameStateSystem] 战斗完成 - 胜利: {e.IsVictory}");
            
            // 取消监听
            this.UnRegisterEvent<BattleCompletedEvent>(OnBattleCompleted);
            
            // 验证事件数据
            if (e.Result == null)
            {
                Debug.LogError("[GameStateSystem] 战斗结果为空，但仍将切换状态");
            }
            
            if (e.EnemyController == null)
            {
                Debug.LogError("[GameStateSystem] 敌人控制器为空，将跳过奖励处理");
            }
            
            // 统一处理状态切换
            mGameModel.CurrentGameState.Value = GameState.Gameplay;
            
            // 发送奖励处理命令（仅在数据有效时）
            if (e.Result != null && e.EnemyController != null)
            {
                this.SendCommand(new ProcessBattleRewardsCommand(e.Result, e.EnemyController, e.IsVictory));
            }
            else
            {
                Debug.LogWarning("[GameStateSystem] 数据不完整，跳过奖励处理");
            }
            
            Debug.Log("[GameStateSystem] 战斗状态处理完成，已返回主游戏状态");
        }
        
        private void HandleGameOverState()
        {
            // 游戏结束处理
            var playerModel = this.GetModel<PlayerModel>();
            playerModel.IsFumeMode.Value = false;
        }
    }
}
