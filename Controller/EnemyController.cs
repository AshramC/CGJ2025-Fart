using QFramework;
using UnityEngine;

namespace FartGame
{
    public class EnemyController : MonoBehaviour, IController
    {
        [Header("敌人配置")]
        [Tooltip("敌人的配置数据")]
        public EnemyConfigSO enemyConfig;
        
        [Header("调试信息")]
        [Tooltip("显示当前耐力值")]
        [SerializeField] private float currentStamina;
        
        private bool isDefeated = false;
        private FartSystem mFartSystem;
        
        // 当前耐力值的公共只读属性
        public float CurrentStamina => currentStamina;
        
        void Start()
        {
            // 初始化
            mFartSystem = this.GetSystem<FartSystem>();
            
            if (enemyConfig != null)
            {
                currentStamina = enemyConfig.initialStamina;
            }
            else
            {
                Debug.LogError($"Enemy {gameObject.name} missing EnemyConfigSO!");
                currentStamina = 100f; // 默认值
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            // 检查是否是玩家，且敌人未被击败
            if (other.CompareTag("Player") && !isDefeated)
            {
                // 检查玩家是否在熏模式
                if (mFartSystem != null && mFartSystem.IsPlayerInFumeMode())
                {
                    // 检查玩家是否有足够的屁值
                    float playerFartValue = mFartSystem.GetPlayerFartValue();
                    if (playerFartValue > 0 && currentStamina > 0)
                    {
                        // 计算消耗的屁值（可以根据敌人类型调整）
                        float fartConsumption = Mathf.Min(playerFartValue, currentStamina);
                        
                        // 发送消耗玩家屁值的命令
                        this.SendCommand(new ConsumeFartCommand(fartConsumption));
                        
                        // 发送清空敌人耐力的命令
                        this.SendCommand(new ClearEnemyStaminaCommand(this));
                    }
                }
            }
        }
        
        // 清空耐力值的方法（供Command调用）
        public void ClearStamina()
        {
            currentStamina = 0f;
            isDefeated = true;
            
            // 可以在这里添加视觉效果，比如改变材质、播放动画等
            Debug.Log($"Enemy {gameObject.name} stamina cleared!");
        }
        
        // 获取敌人Tag的方法（供Command调用）
        public string GetEnemyTag()
        {
            return enemyConfig != null ? enemyConfig.enemyTag : "Unknown";
        }
        
        // 重置敌人状态（供重新开始游戏时调用）
        public void ResetEnemy()
        {
            if (enemyConfig != null)
            {
                currentStamina = enemyConfig.initialStamina;
                isDefeated = false;
            }
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
        
        // 在Inspector中显示调试信息
        void OnValidate()
        {
            if (enemyConfig != null && Application.isPlaying)
            {
                currentStamina = Mathf.Max(0, currentStamina);
            }
        }
    }
}
