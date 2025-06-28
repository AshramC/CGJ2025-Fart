using QFramework;
using UnityEngine;

namespace FartGame
{
    public class StartEnemyBattleCommand : AbstractCommand
    {
        private readonly EnemyController enemyController;
        private readonly EnemyConfigSO enemyConfig;
        
        public StartEnemyBattleCommand(EnemyController controller, EnemyConfigSO config)
        {
            enemyController = controller;
            enemyConfig = config;
        }
        
        protected override void OnExecute()
        {
            if (enemyController == null || enemyConfig == null)
            {
                Debug.LogError("[StartEnemyBattleCommand] 敌人控制器或配置数据为空");
                return;
            }
            
            // 验证战斗谱面数据
            if (enemyConfig.battleChart == null)
            {
                Debug.LogError($"[StartEnemyBattleCommand] 敌人 {enemyConfig.displayName} 缺少战斗谱面数据");
                return;
            }
            
            // 切换游戏状态到战斗
            var gameModel = this.GetModel<GameModel>();
            gameModel.CurrentGameState.Value = GameState.Battle;
            
            // 发送战斗开始事件，包含完整的敌人信息
            this.SendEvent(new EnemyBattleStartEvent
            {
                EnemyTag = enemyConfig.displayName,
                BattlePosition = enemyController.transform.position,
                enemyController = enemyController,
                enemyConfig = enemyConfig
            });
            
            Debug.Log($"[StartEnemyBattleCommand] 开始战斗 - 敌人: {enemyConfig.displayName}, 类型: {enemyConfig.enemyType}");
        }
    }
}
