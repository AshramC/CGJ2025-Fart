using QFramework;
using UnityEngine;

namespace FartGame
{
    // 注意：此Command的逻辑已经改变
    // 新架构中敌人击败后不会直接触发战斗，而是通过BattleVictoryCommand处理
    public class EnemyDefeatedCommand : AbstractCommand
    {
        private readonly string mEnemyTag;
        private readonly Vector3 mEnemyPosition;
        
        public EnemyDefeatedCommand(string enemyTag, Vector3 enemyPosition)
        {
            mEnemyTag = enemyTag;
            mEnemyPosition = enemyPosition;
        }
        
        protected override void OnExecute()
        {
            // 记录敌人被击败的信息
            Debug.Log($"[EnemyDefeatedCommand] Enemy defeated: {mEnemyTag} at position {mEnemyPosition}");
            
            // 在新架构中，敌人击败后的逻辑已经在BattleVictoryCommand中处理
            // 这里不再触发新的战斗，避免无限循环
            
            // 发送敌人被击败事件供其他系统监听
            this.SendEvent(new EnemyDefeatedEvent
            {
                EnemyTag = mEnemyTag,
                EnemyPosition = mEnemyPosition,
                RemainingStamina = 0f
            });
            
            Debug.Log("[EnemyDefeatedCommand] 敌人击败事件已发送");
        }
    }
}
