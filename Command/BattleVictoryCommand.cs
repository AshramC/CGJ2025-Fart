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
            // 注意：此Command已废弃，状态管理已移至GameStateSystem
            // 奖励处理已移至ProcessBattleRewardsCommand
            Debug.LogWarning("[BattleVictoryCommand] 此Command已废弃，请使用ProcessBattleRewardsCommand");
            
            Debug.Log($"[BattleVictory] 战斗胜利 - 准确率: {result.accuracy:P1}");
        }
        
        private void ProcessVictoryRewards()
        {
            // 从当前战斗上下文获取敌人信息
            var currentEnemyController = GetCurrentBattleEnemy();
            if (currentEnemyController == null)
            {
                Debug.LogWarning("[BattleVictory] 未找到当前战斗的敌人信息，使用默认奖励");
                return;
            }
            
            var enemyConfig = currentEnemyController.GetEnemyConfig();
            if (enemyConfig == null)
            {
                Debug.LogError("[BattleVictory] 敌人配置数据为空");
                return;
            }
            
            // 标记敌人为已击败
            currentEnemyController.MarkAsDefeated();
            
            var fartSystem = this.GetSystem<FartSystem>();
            
            // 根据敌人类型分发不同奖励
            switch (enemyConfig.enemyType)
            {
                case EnemyType.Normal:
                    // 普通敌人：直接分发奖励
                    ProcessNormalEnemyRewards(fartSystem, enemyConfig);
                    break;
                    
                case EnemyType.ResourcePoint:
                    // 资源点：标记为可交互状态，不直接分发奖励
                    ProcessResourcePointVictory(enemyConfig);
                    break;
                    
                default:
                    Debug.LogWarning($"[BattleVictory] 未知的敌人类型: {enemyConfig.enemyType}");
                    break;
            }
        }
        
        private void ProcessNormalEnemyRewards(FartSystem fartSystem, EnemyConfigSO config)
        {
            // 提升屁值上限
            if (config.valueReward > 0)
            {
                fartSystem.IncreaseMaxFartValue(config.valueReward);
                Debug.Log($"[BattleVictory] 获得屁值上限提升: {config.valueReward}");
            }
            
            // 获得资源点（如果后续实现资源点消耗系统）
            if (config.resourceReward > 0)
            {
                Debug.Log($"[BattleVictory] 获得资源点: {config.resourceReward}（暂时未实现资源点系统）");
            }
        }
        
        private void ProcessResourcePointVictory(EnemyConfigSO config)
        {
            Debug.Log($"[BattleVictory] 资源点 {config.displayName} 已被击败，现在可以交互激活回复效果");
            // 资源点不直接分发奖励，等待玩家主动交互
        }
        
        private EnemyController GetCurrentBattleEnemy()
        {
            // 从 GameManager 获取当前战斗的敌人引用
            var gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                return gameManager.GetCurrentBattleEnemy();
            }
            
            Debug.LogError("[BattleVictory] 未找到 GameManager");
            return null;
        }
    }
}
