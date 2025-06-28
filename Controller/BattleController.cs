using QFramework;
using UnityEngine;
using UnityEngine.UI;
using FartGame.Battle;

namespace FartGame
{
    /// <summary>
    /// 战斗控制器 - 专门管理战斗时的UI实时更新和用户交互
    /// </summary>
    public class BattleController : MonoBehaviour, IController
    {
        [Header("战斗UI组件")]
        [SerializeField] private Slider enemyHealthSlider;
        [SerializeField] private Text judgmentText;
        [SerializeField] private GameObject pauseMenu;
        
        [Header("引用")]
        [SerializeField] private BattleManager battleManager;
        
        private bool isActive = false;
        private float lastJudgmentDisplayTime = 0f;
        
        void Start()
        {
            // 初始化UI状态
            SetUIActive(false);
        }
        
        void Update()
        {
            if (!isActive || battleManager == null) return;
            
            // 实时更新UI（直接从BattleManager获取数据）
            UpdateBattleUI();
        }
        
        private void UpdateBattleUI()
        {
            var status = battleManager.GetCurrentStatus();
            
            // 更新敌人血量
            if (enemyHealthSlider != null)
            {
                float maxHealth = battleManager.GetEnemyMaxHealth();
                if (maxHealth > 0)
                {
                    enemyHealthSlider.value = status.enemyStamina / maxHealth;
                }
            }
            
            // 显示最新判定结果（可选）
            if (judgmentText != null)
            {
                var lastJudgment = battleManager.GetLastJudgment();
                var lastJudgmentTime = battleManager.GetLastJudgmentTime();
                
                if (lastJudgment.HasValue && lastJudgmentTime > lastJudgmentDisplayTime)
                {
                    judgmentText.text = lastJudgment.Value.ToString();
                    lastJudgmentDisplayTime = lastJudgmentTime;
                }
                
                // 判定文本自动消失
                if (Time.time - lastJudgmentTime > 1f)
                {
                    judgmentText.text = "";
                }
            }
        }
        
        public void SetUIActive(bool active)
        {
            isActive = active;
            gameObject.SetActive(active);
            
            if (!active && pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
        }
        
        private void OnPauseClicked()
        {
            if (battleManager != null)
            {
                battleManager.PauseBattle();
                if (pauseMenu != null)
                {
                    pauseMenu.SetActive(true);
                }
            }
        }
        
        public void OnResumeClicked()
        {
            if (battleManager != null)
            {
                battleManager.ResumeBattle();
                if (pauseMenu != null)
                {
                    pauseMenu.SetActive(false);
                }
            }
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
    }
}
