using QFramework;
using UnityEngine;

namespace FartGame
{
    // 注意：此Command已废弃，因为新架构中敌人不再有耐力值概念
    // 保留仅为了兼容性，实际上不应该被调用
    [System.Obsolete("ClearEnemyStaminaCommand is deprecated. Use new battle system instead.")]
    public class ClearEnemyStaminaCommand : AbstractCommand
    {
        private readonly EnemyController mEnemyController;
        
        public ClearEnemyStaminaCommand(EnemyController enemyController)
        {
            mEnemyController = enemyController;
        }
        
        protected override void OnExecute()
        {
            Debug.LogWarning("[ClearEnemyStaminaCommand] 此Command已废弃，不再执行任何操作。请使用新的战斗系统。");
            
            // 在新架构中，敌人击败通过 BattleVictoryCommand 处理
            // 这里不执行任何操作，避免编译错误
        }
    }
}
