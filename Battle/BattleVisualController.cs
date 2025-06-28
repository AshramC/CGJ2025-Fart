using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace FartGame.Battle
{
    public class BattleVisualController : MonoBehaviour
    {
        [Header("UI组件引用")]
        [SerializeField] private Text comboText;
        [SerializeField] private Slider staminaBar;
        [SerializeField] private Text judgementFeedbackText;
        [SerializeField] private Transform arrowContainer;
        [SerializeField] private GameObject arrowPrefab;
        
        [Header("视觉效果")]
        [SerializeField] private Image buttImage;
        [SerializeField] private GameObject emotionHeadPrefab;
        [SerializeField] private GameObject fartBubblePrefab;
        [SerializeField] private Transform beatIndicatorContainer;
        
        [Header("设置")]
        [SerializeField] private float arrowMoveSpeed = 5f;
        [SerializeField] private Color hitColor = Color.green;
        [SerializeField] private Color missColor = Color.red;
        
        private MusicTimeManager timeManager;
        private BattleManager battleManager;
        private List<GameObject> activeArrows = new List<GameObject>();
        private bool isInitialized = false;
        
        // 依赖注入
        public void Initialize(MusicTimeManager timeManager, BattleManager battleManager)
        {
            this.timeManager = timeManager;
            this.battleManager = battleManager;
            isInitialized = true;
            
            Debug.Log("[BattleVisualController] 初始化完成");
        }
        
        // === 箭头显示管理 ===
        public void ShowDirectionArrow(Direction direction, double appearTime)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[BattleVisualController] 尚未初始化");
                return;
            }
            
            // TODO: 实现箭头显示逻辑
            Debug.Log($"[BattleVisualController] 显示箭头: {direction} at {appearTime:F3}");
        }
        
        public void UpdateArrowPositions()
        {
            if (!isInitialized || timeManager == null)
                return;
                
            // TODO: 实现箭头位置更新逻辑
            double currentTime = timeManager.GetJudgementTime();
            
            foreach (var arrow in activeArrows)
            {
                if (arrow != null)
                {
                    // TODO: 根据时间更新箭头位置
                }
            }
        }
        
        // === 透明度系统 ===
        public void UpdateButtTransparency(float transparency)
        {
            if (buttImage != null)
            {
                Color color = buttImage.color;
                color.a = transparency;
                buttImage.color = color;
            }
        }
        
        public void ShowEmotionHead()
        {
            if (emotionHeadPrefab != null)
            {
                // TODO: 实现表情头部显示逻辑
                Debug.Log("[BattleVisualController] 显示表情头部");
            }
        }
        
        public void ShowFartBubbleEffect()
        {
            if (fartBubblePrefab != null)
            {
                // TODO: 实现屁气泡特效逻辑
                Debug.Log("[BattleVisualController] 播放屁气泡特效");
            }
        }
        
        // === 节拍提示 ===
        public void ShowBeatIndicator(double beatTime)
        {
            // TODO: 实现节拍指示器显示逻辑
            Debug.Log($"[BattleVisualController] 显示节拍指示器: {beatTime:F3}");
        }
        
        public void UpdateBeatVisuals()
        {
            if (!isInitialized || timeManager == null)
                return;
                
            // TODO: 实现节拍视觉更新逻辑
            double currentTime = timeManager.GetJudgementTime();
        }
        
        // === UI状态更新 ===
        public void UpdateComboDisplay(int combo)
        {
            if (comboText != null)
            {
                comboText.text = $"连击: {combo}";
            }
        }
        
        public void UpdateStaminaBar(float stamina)
        {
            if (staminaBar != null)
            {
                staminaBar.value = stamina;
            }
        }
        
        public void ShowJudgementFeedback(JudgementResult result)
        {
            if (judgementFeedbackText != null)
            {
                if (result.isHit)
                {
                    judgementFeedbackText.text = "HIT!";
                    judgementFeedbackText.color = hitColor;
                }
                else if (result.isMiss)
                {
                    judgementFeedbackText.text = "MISS!";
                    judgementFeedbackText.color = missColor;
                }
                
                // TODO: 添加反馈文本的淡出动画
            }
        }
        
        // === Unity生命周期 ===
        private void Update()
        {
            if (!isInitialized)
                return;
                
            UpdateArrowPositions();
            UpdateBeatVisuals();
        }
        
        private void OnDestroy()
        {
            // 清理箭头对象
            foreach (var arrow in activeArrows)
            {
                if (arrow != null)
                {
                    Destroy(arrow);
                }
            }
            activeArrows.Clear();
            
            Debug.Log("[BattleVisualController] 组件销毁");
        }
        
        // === 私有辅助方法 ===
        private void ClearActiveArrows()
        {
            foreach (var arrow in activeArrows)
            {
                if (arrow != null)
                {
                    Destroy(arrow);
                }
            }
            activeArrows.Clear();
        }
        
        // === 调试方法 ===
        public void TestArrowDisplay()
        {
            ShowDirectionArrow(Direction.Up, timeManager?.GetJudgementTime() ?? 0);
        }
        
        public void TestJudgementFeedback()
        {
            ShowJudgementFeedback(new JudgementResult { isHit = true, accuracy = 0.95f });
        }
    }
}
