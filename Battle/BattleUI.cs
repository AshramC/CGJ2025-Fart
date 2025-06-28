using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace FartGame.Battle
{
    /// <summary>
    /// 战斗UI管理器 - 专门处理战斗界面的直接UI更新
    /// 不依赖Model层，直接从BattleManager获取数据，实现零延迟更新
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Slider fartValueSlider;
        [SerializeField] private Text judgementText;
        
        [Header("判定反馈设置")]
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color missColor = Color.red;
        [SerializeField] private float feedbackDuration = 1f;
        
        [Header("调试设置")]
        [SerializeField] private bool enableDebugLog = false;
        
        /// <summary>
        /// 直接更新屁值显示（高性能，零延迟）
        /// </summary>
        public void UpdateFartValueDirect(float currentValue, float maxValue)
        {
            if (fartValueSlider != null && maxValue > 0f)
            {
                float ratio = currentValue / maxValue;
                fartValueSlider.value = ratio;
                
                if (enableDebugLog)
                {
                    Debug.Log($"[BattleUI] 屁值更新: {currentValue}/{maxValue} ({ratio:P1})");
                }
            }
        }
        
        /// <summary>
        /// 显示判定反馈（高性能，零延迟）
        /// </summary>
        public void ShowJudgementFeedback(BattleJudgeResult result)
        {
            if (judgementText != null)
            {
                switch (result)
                {
                    case BattleJudgeResult.Success:
                        judgementText.text = "SUCCESS";
                        judgementText.color = successColor;
                        break;
                    case BattleJudgeResult.Miss:
                        judgementText.text = "MISS";
                        judgementText.color = missColor;
                        break;
                    default:
                        return; // 不显示None状态
                }
                
                // 启动判定文本淡出
                StartCoroutine(FadeOutJudgementText());
                
                if (enableDebugLog)
                {
                    Debug.Log($"[BattleUI] 判定反馈: {result}");
                }
            }
        }
        
        /// <summary>
        /// 判定文本淡出效果
        /// </summary>
        private IEnumerator FadeOutJudgementText()
        {
            if (judgementText == null) yield break;
            
            // 等待一段时间后开始淡出
            yield return new WaitForSeconds(feedbackDuration * 0.5f);
            
            Color originalColor = judgementText.color;
            float fadeTime = feedbackDuration * 0.5f;
            
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
                judgementText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            
            // 清空文本并恢复透明度
            judgementText.text = "";
            judgementText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }
        
        /// <summary>
        /// 设置UI激活状态
        /// </summary>
        public void SetUIActive(bool active)
        {
            gameObject.SetActive(active);
            
            if (enableDebugLog)
            {
                Debug.Log($"[BattleUI] UI状态: {(active ? "激活" : "停用")}");
            }
        }
        
        /// <summary>
        /// 重置UI状态
        /// </summary>
        public void ResetUI()
        {
            if (fartValueSlider != null)
            {
                fartValueSlider.value = 1f; // 重置为满屁值
            }
            
            if (judgementText != null)
            {
                judgementText.text = "";
                judgementText.color = Color.white;
            }
            
            // 停止所有协程
            StopAllCoroutines();
            
            if (enableDebugLog)
            {
                Debug.Log("[BattleUI] UI已重置");
            }
        }
        
        /// <summary>
        /// 验证UI组件完整性
        /// </summary>
        private void Start()
        {
            ValidateComponents();
        }
        
        /// <summary>
        /// 验证必要的UI组件是否已设置
        /// </summary>
        private void ValidateComponents()
        {
            bool hasErrors = false;
            
            if (fartValueSlider == null)
            {
                Debug.LogError("[BattleUI] fartValueSlider未设置！");
                hasErrors = true;
            }
            
            if (judgementText == null)
            {
                Debug.LogError("[BattleUI] judgementText未设置！");
                hasErrors = true;
            }
            
            if (!hasErrors)
            {
                Debug.Log("[BattleUI] 组件验证通过");
            }
        }
        
        /// <summary>
        /// 获取当前UI状态信息（调试用）
        /// </summary>
        public string GetUIStatus()
        {
            if (!gameObject.activeInHierarchy)
            {
                return "UI未激活";
            }
            
            string status = "BattleUI状态: ";
            
            if (fartValueSlider != null)
            {
                status += $"屁值显示={fartValueSlider.value:P1} ";
            }
            
            if (judgementText != null && !string.IsNullOrEmpty(judgementText.text))
            {
                status += $"判定显示={judgementText.text} ";
            }
            
            return status;
        }
    }
}
