using QFramework;
using UnityEngine;

namespace FartGame
{
    public class EnemyController : MonoBehaviour, IController
    {
        [Header("敌人配置")]
        [Tooltip("敌人的配置数据")]
        public EnemyConfigSO enemyConfig;
        
        [Header("运行时状态")]
        [Tooltip("是否已被击败")]
        [SerializeField] private bool isDefeated = false;
        
        [Tooltip("资源点是否正在提供回复效果（仅ResourcePoint类型使用）")]
        [SerializeField] private bool isResourceActive = false;
        
        // 公共只读属性
        public bool IsDefeated => isDefeated;
        public bool IsResourceActive => isResourceActive;
        public EnemyType EnemyType => enemyConfig != null ? enemyConfig.enemyType : EnemyType.Normal;
        
        void Start()
        {
            if (enemyConfig == null)
            {
                Debug.LogError($"Enemy {gameObject.name} missing EnemyConfigSO!");
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (!isDefeated)
                {
                    // 敌人未被击败，触发战斗
                    TriggerBattle();
                }
                else if (enemyConfig.enemyType == EnemyType.ResourcePoint && !isResourceActive)
                {
                    // 资源点已被击败且未激活，触发资源点交互
                    TriggerResourcePoint();
                }
                // 普通敌人已被击败或资源点已激活，无反应
            }
        }
        
        private void TriggerBattle()
        {
            if (enemyConfig == null)
            {
                Debug.LogError($"Enemy {gameObject.name} 缺少配置数据，无法启动战斗");
                return;
            }
            
            Debug.Log($"[敌人控制器] 触发战斗 - {enemyConfig.displayName}");
            this.SendCommand(new StartEnemyBattleCommand(this, enemyConfig));
        }
        
        private void TriggerResourcePoint()
        {
            if (enemyConfig == null || enemyConfig.enemyType != EnemyType.ResourcePoint)
            {
                return;
            }
            
            Debug.Log($"[敌人控制器] 激活资源点 - {enemyConfig.displayName}");
            this.SendCommand(new ActivateResourcePointCommand(this, enemyConfig.healingRate, enemyConfig.healingDuration));
        }
        
        // 标记为已击败（供Command调用）
        public void MarkAsDefeated()
        {
            isDefeated = true;
            Debug.Log($"[敌人控制器] {enemyConfig?.displayName ?? gameObject.name} 已被击败");
            
            // 可以在这里添加视觉效果，比如改变材质、播放动画等
        }
        
        // 激活资源点回复效果（供Command调用）
        public void ActivateResourcePoint()
        {
            if (enemyConfig.enemyType == EnemyType.ResourcePoint)
            {
                isResourceActive = true;
                Debug.Log($"[敌人控制器] 资源点 {enemyConfig.displayName} 已激活");
                
                // 可以在这里添加视觉效果
            }
        }
        
        // 停止资源点回复效果（供Command调用）
        public void DeactivateResourcePoint()
        {
            if (enemyConfig.enemyType == EnemyType.ResourcePoint)
            {
                isResourceActive = false;
                Debug.Log($"[敌人控制器] 资源点 {enemyConfig.displayName} 已停止");
            }
        }
        
        // 重置敌人状态（供重新开始游戏时调用）
        public void ResetEnemy()
        {
            isDefeated = false;
            isResourceActive = false;
            Debug.Log($"[敌人控制器] {enemyConfig?.displayName ?? gameObject.name} 状态已重置");
        }
        
        // 获取敌人配置信息（供外部调用）
        public EnemyConfigSO GetEnemyConfig()
        {
            return enemyConfig;
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
        
        // 在Inspector中显示调试信息
        void OnValidate()
        {
            // 验证配置数据
            if (enemyConfig != null && enemyConfig.battleChart == null)
            {
                Debug.LogWarning($"Enemy {gameObject.name} 缺少战斗谱面数据");
            }
        }
    }
}
