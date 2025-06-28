using QFramework;
using UnityEngine;
using FartGame.Battle;

namespace FartGame
{
    public class BattleDefeatCommand : AbstractCommand
    {
        private readonly BattleResult result;
        
        public BattleDefeatCommand(BattleResult result)
        {
            this.result = result;
        }
        
        protected override void OnExecute()
        {
            var gameModel = this.GetModel<GameModel>();
            
            // 切换回游戏状态
            gameModel.CurrentGameState.Value = GameState.Gameplay;
            
            // 处理失败惩罚（如果有的话）
            ProcessDefeatPenalties();
            
            Debug.Log($"[BattleDefeat] 战斗失败 - 准确率: {result.accuracy:P1}");
        }
        
        private void ProcessDefeatPenalties()
        {
            // 失败惩罚逻辑（可选）
            Debug.Log("[BattleDefeat] 处理失败惩罚");
        }
    }
}
