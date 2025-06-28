using QFramework;
using UnityEngine;
using FartGame.Battle;

namespace FartGame
{
    public class ProcessBattleRewardsCommand : AbstractCommand
    {
        private readonly BattleResult result;
        private readonly EnemyController enemyController;
        private readonly bool isVictory;
        
        public ProcessBattleRewardsCommand(BattleResult result, EnemyController enemyController, bool isVictory)
        {
            this.result = result;
            this.enemyController = enemyController;
            this.isVictory = isVictory;
        }
        
        protected override void OnExecute()
        {
            var fartSystem = this.GetSystem<FartSystem>();
            
            if (isVictory)
            {
                Debug.Log($"[ProcessBattleRewardsCommand] 战斗胜利处理 - 伤害: {result.potentialDamage}");
                
                // 1. 应用战斗伤害
                if (result.potentialDamage > 0f)
                {
                    this.SendCommand(new ApplyBattleDamageCommand(result.potentialDamage));
                }
                
                // 2. 给予胜利奖励
                ProcessVictoryRewards();
                
                // 3. 标记敌人为已击败
                if (enemyController != null)
                {
                    enemyController.MarkAsDefeated();
                }
            }
            else
            {
                Debug.Log("[ProcessBattleRewardsCommand] 战斗失败处理 - 恢复满血");
                
                // 1. 恢复满血
                this.SendCommand(new RestorePlayerHealthCommand());
                
                // 2. 无其他奖励或惩罚
            }
        }
        
        private void ProcessVictoryRewards()
        {
            if (enemyController?.GetEnemyConfig() == null) return;
            
            var config = enemyController.GetEnemyConfig();
            var fartSystem = this.GetSystem<FartSystem>();
            
            // 根据敌人类型给予奖励
            switch (config.enemyType)
            {
                case EnemyType.Normal:
                    fartSystem.IncreaseMaxFartValue(config.valueReward);
                    Debug.Log($"[ProcessBattleRewardsCommand] 普通敌人奖励: +{config.valueReward} 屁值上限");
                    break;
                    
                case EnemyType.ResourcePoint:
                    Debug.Log("[ProcessBattleRewardsCommand] 资源点敌人击败，转为可交互状态");
                    break;
            }
        }

    }
}
