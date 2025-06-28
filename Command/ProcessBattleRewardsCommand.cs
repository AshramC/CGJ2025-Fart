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
            // 验证入参
            if (result == null)
            {
                Debug.LogError("[ProcessBattleRewardsCommand] 战斗结果为空，无法处理奖励");
                return;
            }
            
            if (enemyController == null)
            {
                Debug.LogError("[ProcessBattleRewardsCommand] 敌人控制器为空，无法处理奖励");
                return;
            }
            
            var enemyConfig = enemyController.GetEnemyConfig();
            if (enemyConfig == null)
            {
                Debug.LogError("[ProcessBattleRewardsCommand] 敌人配置数据为空");
                return;
            }
            
            // 标记敌人为已击败
            enemyController.MarkAsDefeated();
            
            if (isVictory)
            {
                ProcessVictoryRewards(enemyConfig);
            }
            else
            {
                ProcessDefeatConsequences(enemyConfig);
            }
            
            Debug.Log($"[ProcessBattleRewardsCommand] 战斗奖励处理完成 - 胜利: {isVictory}, 准确率: {result.accuracy:P1}");
        }
        
        private void ProcessVictoryRewards(EnemyConfigSO config)
        {
            var fartSystem = this.GetSystem<FartSystem>();
            
            // 根据敌人类型分发不同奖励
            switch (config.enemyType)
            {
                case EnemyType.Normal:
                    // 普通敌人：直接分发奖励
                    ProcessNormalEnemyRewards(fartSystem, config);
                    break;
                    
                case EnemyType.ResourcePoint:
                    // 资源点：标记为可交互状态，不直接分发奖励
                    ProcessResourcePointVictory(config);
                    break;
                    
                default:
                    Debug.LogWarning($"[ProcessBattleRewardsCommand] 未知的敌人类型: {config.enemyType}");
                    break;
            }
        }
        
        private void ProcessNormalEnemyRewards(FartSystem fartSystem, EnemyConfigSO config)
        {
            if (fartSystem == null)
            {
                Debug.LogError("[ProcessBattleRewardsCommand] FartSystem为空，无法处理奖励");
                return;
            }
            
            // 提升屁值上限
            if (config.valueReward > 0)
            {
                fartSystem.IncreaseMaxFartValue(config.valueReward);
                Debug.Log($"[ProcessBattleRewardsCommand] 获得屁值上限提升: {config.valueReward}");
            }
            
            // 获得资源点（如果后续实现资源点消耗系统）
            if (config.resourceReward > 0)
            {
                Debug.Log($"[ProcessBattleRewardsCommand] 获得资源点: {config.resourceReward}（暂时未实现资源点系统）");
            }
        }
        
        private void ProcessResourcePointVictory(EnemyConfigSO config)
        {
            Debug.Log($"[ProcessBattleRewardsCommand] 资源点 {config.displayName} 已被击败，现在可以交互激活回复效果");
            // 资源点不直接分发奖励，等待玩家主动交互
        }
        
        private void ProcessDefeatConsequences(EnemyConfigSO config)
        {
            Debug.Log($"[ProcessBattleRewardsCommand] 战斗失败，处理失败后果");
            // TODO: 实现战斗失败的后果处理
        }
    }
}
