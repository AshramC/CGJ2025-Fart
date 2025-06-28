using QFramework;
using UnityEngine;

namespace FartGame
{
    public class StartEnemyBattleCommand : AbstractCommand
    {
        private readonly string mEnemyTag;
        private readonly Vector3 mBattlePosition;
        
        public StartEnemyBattleCommand(string enemyTag, Vector3 battlePosition)
        {
            mEnemyTag = enemyTag;
            mBattlePosition = battlePosition;
        }
        
        protected override void OnExecute()
        {
            // 发送战斗开始事件
            this.SendEvent(new EnemyBattleStartEvent
            {
                EnemyTag = mEnemyTag,
                BattlePosition = mBattlePosition
            });
            
            // 暂时为空实现，预留给未来的战斗系统
            Debug.Log($"Battle started with enemy: {mEnemyTag} at position {mBattlePosition}");
            
            // 未来可以在这里添加：
            // - 切换到战斗场景
            // - 初始化战斗UI
            // - 播放战斗音效
            // - 等等战斗相关逻辑
        }
    }
}
