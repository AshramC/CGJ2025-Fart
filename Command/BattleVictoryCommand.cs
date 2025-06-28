using QFramework;
using UnityEngine;
using FartGame.Battle;

namespace FartGame
{
    public class BattleVictoryCommand : AbstractCommand
    {
        private readonly BattleResult result;
        
        public BattleVictoryCommand(BattleResult result)
        {
            this.result = result;
        }
        
        protected override void OnExecute()
        {
            var gameModel = this.GetModel<GameModel>();
            
            // 切换回游戏状态
            gameModel.CurrentGameState.Value = GameState.Gameplay;
            
            // 处理胜利奖励
            ProcessVictoryRewards();
            
            Debug.Log($"[BattleVictory] 战斗胜利 - 准确率: {result.accuracy:P1}");
        }
        
        private void ProcessVictoryRewards()
        {
            // 增加经验、金币等奖励逻辑
            // 可以在这里添加具体的奖励处理
            Debug.Log("[BattleVictory] 处理胜利奖励");
        }
    }
}
