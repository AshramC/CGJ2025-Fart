using QFramework;
using UnityEngine;

namespace FartGame
{
    public class GameOverCommand : AbstractCommand
    {
        private readonly GameOverReason mReason;
        
        public GameOverCommand(GameOverReason reason)
        {
            mReason = reason;
        }
        
        protected override void OnExecute()
        {
            var gameModel = this.GetModel<GameModel>();
            var playerModel = this.GetModel<PlayerModel>();
            
            // 防止重复执行游戏结束逻辑
            if (gameModel.CurrentGameState.Value == GameState.GameOver)
                return;
            
            // 记录游戏数据用于事件
            float finalFartValue = playerModel.FartValue.Value;
            Vector3 playerPosition = playerModel.Position.Value;
            float gameDuration = Time.time; // 简化的游戏时长计算
            
            // 关闭熏模式
            playerModel.IsFumeMode.Value = false;
            
            // 切换到游戏结束状态
            gameModel.CurrentGameState.Value = GameState.GameOver;
            gameModel.IsPaused.Value = false;
            
            // 发送游戏结束事件
            this.SendEvent(new GameOverEvent
            {
                Reason = mReason,
                FinalFartValue = finalFartValue,
                GameDuration = gameDuration,
                PlayerPosition = playerPosition
            });
        }
    }
}
