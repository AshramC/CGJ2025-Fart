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
            // 注意：此Command已废弃，状态管理已移至GameStateSystem
            // 失败处理已移至ProcessBattleRewardsCommand
            Debug.LogWarning("[BattleDefeatCommand] 此Command已废弃，请使用ProcessBattleRewardsCommand");
            
            Debug.Log($"[BattleDefeat] 战斗失败 - 准确率: {result.accuracy:P1}");
        }
        
        private void ProcessDefeatPenalties()
        {
            // 失败惩罚逻辑（可选）
            Debug.Log("[BattleDefeat] 处理失败惩罚");
        }
    }
}
