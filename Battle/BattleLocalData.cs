using UnityEngine;

namespace FartGame.Battle
{
    /// <summary>
    /// 战斗本地数据结构 - 战斗内独立维护的数据
    /// 避免与全局Model的实时同步，提升性能
    /// </summary>
    [System.Serializable]
    public class BattleLocalData
    {
        [Header("核心数据")]
        [Tooltip("战斗内当前屁值")]
        public float currentFartValue;
        
        [Tooltip("战斗开始时的屁值")]
        public float initialFartValue;
        
        [Tooltip("最后判定结果")]
        public BattleJudgeResult lastJudgement;
        
        [Tooltip("总共受到的伤害")]
        public float totalDamageReceived;
        
        /// <summary>
        /// 从玩家数据初始化
        /// </summary>
        public void InitializeFromPlayer(PlayerBattleData playerData)
        {
            currentFartValue = playerData.fartValue;
            initialFartValue = playerData.fartValue;
            lastJudgement = BattleJudgeResult.None;
            totalDamageReceived = 0f;
            
            Debug.Log($"[BattleLocalData] 初始化完成 - 初始屁值: {initialFartValue}");
        }
        
        /// <summary>
        /// 获取屁值变化量
        /// </summary>
        public float GetFartValueDelta()
        {
            return currentFartValue - initialFartValue;
        }
        
        /// <summary>
        /// 检查是否失败
        /// </summary>
        public bool IsDefeated()
        {
            return currentFartValue <= 0f;
        }
        
        /// <summary>
        /// 获取屁值百分比
        /// </summary>
        public float GetFartValueRatio(float maxFartValue)
        {
            return maxFartValue > 0f ? currentFartValue / maxFartValue : 0f;
        }
    }
}
