using QFramework;
using UnityEngine;

namespace FartGame
{
    public class ClearEnemyStaminaCommand : AbstractCommand
    {
        private readonly EnemyController mEnemyController;
        
        public ClearEnemyStaminaCommand(EnemyController enemyController)
        {
            mEnemyController = enemyController;
        }
        
        protected override void OnExecute()
        {
            if (mEnemyController != null && mEnemyController.CurrentStamina > 0)
            {
                // 清空敌人耐力值
                mEnemyController.ClearStamina();
                
                // 发送敌人被击败事件
                this.SendEvent(new EnemyDefeatedEvent
                {
                    EnemyTag = mEnemyController.GetEnemyTag(),
                    EnemyPosition = mEnemyController.transform.position,
                    RemainingStamina = 0f
                });
                
                // 触发敌人被击败后的处理
                this.SendCommand(new EnemyDefeatedCommand(mEnemyController.GetEnemyTag(), mEnemyController.transform.position));
            }
        }
    }
}
