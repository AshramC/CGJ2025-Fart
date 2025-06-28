using QFramework;
using UnityEngine;

namespace FartGame
{
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
            Debug.Log($"Enemy defeated: {mEnemyTag} at position {mEnemyPosition}");
            
            // 触发开始战斗命令
            this.SendCommand(new StartEnemyBattleCommand(mEnemyTag, mEnemyPosition));
            
            // 这里可以添加其他敌人被击败后的逻辑
            // 比如掉落道具、经验值奖励等
        }
    }
}
