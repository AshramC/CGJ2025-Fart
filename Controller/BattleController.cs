using QFramework;
using UnityEngine;
using UnityEngine.UI;
using FartGame.Battle;

namespace FartGame
{
    /// <summary>
    /// 战斗控制器 - 专门管理战斗时的UI实时更新和用户交互
    /// 已简化：移除敌人血量显示，专注于基本UI控制
    /// </summary>
    public class BattleController : MonoBehaviour, IController
    {
        [Header("战斗UI组件")]
        [SerializeField] private Text judgmentText;
        [SerializeField] private GameObject pauseMenu;
        
        [Header("引用 - 通过单例访问")]
        // battleManager 已移除，通过 BattleManager.Instance 访问
        
        private bool isActive = false;
        private float lastJudgmentDisplayTime = 0f;
        
        void Start()
        {
            // 初始化UI状态
            SetUIActive(false);
        }
        
        void Update()
        {
            if (!isActive || BattleManager.Instance == null || !BattleManager.Instance.IsInBattle()) return;
            
            // 实时更新UI（直接从BattleManager获取数据）
            UpdateBattleUI();
        }
        
        private void UpdateBattleUI()
        {
            var status = BattleManager.Instance.GetCurrentStatus();
            
            // 注意：屁值显示已移至BattleUI组件处理，此处不再更新
            
            // 显示最新判定结果（可选）
            if (judgmentText != null)
            {
                var lastJudgment = BattleManager.Instance.GetLastJudgment();
                var lastJudgmentTime = BattleManager.Instance.GetLastJudgmentTime();
                
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
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.PauseBattle();
                if (pauseMenu != null)
                {
                    pauseMenu.SetActive(true);
                }
            }
        }
        
        public void OnResumeClicked()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.ResumeBattle();
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
