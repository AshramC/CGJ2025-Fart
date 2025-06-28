using UnityEngine;
using UnityEngine.InputSystem;

namespace FartGame.Battle
{

    public enum BattleJudgeResult
    {
        None,       // 无判定
        Success,    // 成功击中（原Perfect + Good合并）
        Miss        // 失误
    }

    /// <summary>
    /// 战斗判定系统 - 专为战斗系统设计
    /// 处理空格键输入、时间窗口判定、Hold事件特殊逻辑
    /// </summary>
    public class BattleJudgement
    {
        [Header("判定窗口设置（毫秒）")]
        private float successWindow = 100f;   // Success窗口（原 Perfect + Good 合并）
        private float missWindow = 200f;      // Miss窗口
        
        [Header("统计信息")]
        public int totalInputs = 0;
        public int missCount = 0;
        public int invalidCount = 0;
        
        [Header("最近判定信息")]
        private BattleJudgeResult lastJudgment = BattleJudgeResult.None;
        private float lastJudgmentTime = 0f;
        
        [Header("Hold状态")]
        private BattleNoteInfo currentHoldNote = null;
        private bool isHoldingKey = false;
        
        [Header("依赖引用")]
        private BattleChartManager chartManager;
        private MusicTimeManager musicTimeManager;
        
        [Header("调试设置")]
        private bool enableDebugLog = true;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BattleJudgement(BattleChartManager chartMgr, MusicTimeManager timeMgr)
        {
            chartManager = chartMgr;
            musicTimeManager = timeMgr;
        }
        
        /// <summary>
        /// 设置判定窗口
        /// </summary>
        public void SetJudgeWindows(float success, float miss)
        {
            successWindow = success;
            missWindow = miss;
            
            LogDebug($"判定窗口设置 - Success: ±{successWindow}ms, Miss: ±{missWindow}ms");
        }
        
        /// <summary>
        /// 处理空格键按下
        /// </summary>
        public void ProcessSpacePress()
        {
            if (!IsSystemReady()) return;
            
            double inputTime = musicTimeManager.GetJudgementTime();
            
            LogDebug($"空格键按下 - 输入时间: {inputTime:F3}s");
            
            // 获取最近的音符
            var nearestNote = chartManager.GetNearestNote(inputTime);
            
            if (nearestNote == null)
            {
                LogDebug("未找到可判定的音符");
                return;
            }
            
            // 计算时间偏差
            float timeDifference = (float)(inputTime - nearestNote.judgementTime) * 1000f; // 转换为毫秒
            float absTimeDiff = Mathf.Abs(timeDifference);
            
            // 检查是否在判定窗口内
            if (absTimeDiff > missWindow)
            {
                LogDebug($"输入超出判定窗口: {absTimeDiff:F1}ms > {missWindow}ms");
                return;
            }
            
            // 计算判定结果
            BattleJudgeResult result = CalculateJudgeResult(timeDifference);
            
            // 更新统计
            UpdateStatistics(result);
            
            // 处理不同类型的音符
            if (nearestNote.isHoldNote)
            {
                HandleHoldNotePress(nearestNote, result, inputTime);
            }
            else
            {
                HandleTapNote(nearestNote, result);
            }
            
            // 输出判定结果
            LogJudgeResult(result, timeDifference, nearestNote);
            
            // 触发反馈事件
            OnJudgeResult?.Invoke(result, nearestNote);
        }
        
        /// <summary>
        /// 处理空格键释放
        /// </summary>
        public void ProcessSpaceRelease()
        {
            if (!IsSystemReady()) return;
            
            isHoldingKey = false;
            
            if (currentHoldNote != null)
            {
                double currentTime = musicTimeManager.GetJudgementTime();
                chartManager.UpdateHoldNote(currentHoldNote, currentTime, false);
                
                LogDebug($"Hold释放: {currentHoldNote.GetBeatPosition()}, 完成度: {currentHoldNote.holdCompletion:P0}");
                
                // Hold结束，清除当前Hold状态
                currentHoldNote = null;
            }
        }
        
        /// <summary>
        /// 更新Hold状态（在Update中调用）
        /// </summary>
        public void UpdateHoldProgress()
        {
            if (!IsSystemReady() || currentHoldNote == null) return;
            
            double currentTime = musicTimeManager.GetJudgementTime();
            chartManager.UpdateHoldNote(currentHoldNote, currentTime, isHoldingKey);
            
            // 检查Hold是否结束
            if (currentHoldNote.IsHoldCompleted() || currentHoldNote.IsHoldFailed())
            {
                BattleJudgeResult holdResult = GetHoldFinalResult(currentHoldNote);
                
                LogDebug($"Hold完成: {currentHoldNote.GetBeatPosition()}, 结果: {holdResult}, 完成度: {currentHoldNote.holdCompletion:P0}");
                
                // 触发Hold完成事件
                OnHoldComplete?.Invoke(holdResult, currentHoldNote);
                
                currentHoldNote = null;
            }
        }
        
        /// <summary>
        /// 处理Tap音符
        /// </summary>
        private void HandleTapNote(BattleNoteInfo noteInfo, BattleJudgeResult result)
        {
            if (result != BattleJudgeResult.None)
            {
                chartManager.MarkNoteAsProcessed(noteInfo);
            }
        }
        
        /// <summary>
        /// 处理Hold音符按下
        /// </summary>
        private void HandleHoldNotePress(BattleNoteInfo noteInfo, BattleJudgeResult result, double inputTime)
        {
            if (result != BattleJudgeResult.None)
            {
                // Hold开始判定成功
                noteInfo.MarkAsHit();
                noteInfo.holdStartTime = inputTime;
                currentHoldNote = noteInfo;
                isHoldingKey = true;
                
                LogDebug($"Hold开始: {noteInfo.GetBeatPosition()}, 开始判定: {result}");
            }
            else
            {
                // Hold开始判定失败
                chartManager.MarkNoteAsProcessed(noteInfo);
            }
        }
        
        /// <summary>
        /// 获取Hold的最终判定结果 - 简化版本
        /// </summary>
        private BattleJudgeResult GetHoldFinalResult(BattleNoteInfo holdNote)
        {
            if (holdNote.IsHoldCompleted())
            {
                // 根据完成度给予判定，只区分Success/Miss
                if (holdNote.holdCompletion >= 0.7f)
                    return BattleJudgeResult.Success;
                else
                    return BattleJudgeResult.Miss;
            }
            else
            {
                return BattleJudgeResult.Miss;
            }
        }
        
        /// <summary>
        /// 处理自动Miss
        /// </summary>
        public void HandleAutoMiss(BattleNoteInfo noteInfo)
        {
            missCount++; // 不增加totalInputs，因为玩家没有输入
            
            // 触发玩家受伤
            TriggerPlayerDamage();
            
            LogJudgeResult(BattleJudgeResult.Miss, 0f, noteInfo, true);
            
            // 触发自动Miss事件
            OnAutoMiss?.Invoke(noteInfo);
        }
        
        /// <summary>
        /// 计算判定结果 - 简化为二档判定
        /// </summary>
        private BattleJudgeResult CalculateJudgeResult(float timeDiff)
        {
            float absTimeDiff = Mathf.Abs(timeDiff);
            
            if (absTimeDiff <= successWindow)
                return BattleJudgeResult.Success;
            else if (absTimeDiff <= missWindow)
                return BattleJudgeResult.Miss;
            else
                return BattleJudgeResult.None;
        }
        
        /// <summary>
        /// 更新统计信息 - 简化版本
        /// </summary>
        private void UpdateStatistics(BattleJudgeResult result)
        {
            totalInputs++;
            
            // 更新最近判定信息
            lastJudgment = result;
            lastJudgmentTime = UnityEngine.Time.time;
            
            switch (result)
            {
                case BattleJudgeResult.Success:
                    // 成功判定，不需要特殊处理
                    break;
                case BattleJudgeResult.Miss:
                    missCount++;
                    // Miss时触发玩家受伤
                    TriggerPlayerDamage();
                    break;
                case BattleJudgeResult.None:
                    invalidCount++;
                    break;
            }
        }
        
        /// <summary>
        /// 输出判定结果日志
        /// </summary>
        private void LogJudgeResult(BattleJudgeResult result, float timeDiff, BattleNoteInfo noteInfo, bool isAutoMiss = false)
        {
            string timingText = isAutoMiss ? "自动Miss" : (timeDiff > 0 ? $"+{timeDiff:F1}ms" : $"{timeDiff:F1}ms");
            string resultText = result switch
            {
                BattleJudgeResult.Success => "<color=green>✓ SUCCESS</color>",
                BattleJudgeResult.Miss => "<color=red>✗ MISS</color>",
                BattleJudgeResult.None => "<color=gray>○ 无效输入</color>",
                _ => "未知"
            };
            
            Debug.Log($"[战斗判定] {resultText} ({timingText}) - {noteInfo.GetBeatPosition()}");
        }
        
        /// <summary>
        /// 检查系统是否准备就绪
        /// </summary>
        private bool IsSystemReady()
        {
            return chartManager != null && musicTimeManager != null;
        }
        
        /// <summary>
        /// 重置统计信息
        /// </summary>
        public void ResetStatistics()
        {
            totalInputs = 0;
            missCount = 0;
            invalidCount = 0;
            currentHoldNote = null;
            isHoldingKey = false;
            lastJudgment = BattleJudgeResult.None;
            lastJudgmentTime = 0f;
            
            LogDebug("统计信息已重置");
        }
        
        /// <summary>
        /// 获取准确率 - 简化版本
        /// </summary>
        public float GetAccuracy()
        {
            int validInputs = totalInputs - invalidCount;
            if (validInputs == 0) return 0f;
            
            return (float)(validInputs - missCount) / validInputs;
        }
        
        /// <summary>
        /// 获取最近的判定结果
        /// </summary>
        public BattleJudgeResult? GetLastJudgment()
        {
            return lastJudgment == BattleJudgeResult.None ? null : lastJudgment;
        }
        
        /// <summary>
        /// 获取最近判定的时间
        /// </summary>
        public float GetLastJudgmentTime()
        {
            return lastJudgmentTime;
        }
        
        /// <summary>
        /// 触发玩家受伤（Miss时调用）
        /// </summary>
        private void TriggerPlayerDamage()
        {
            float attackPower = GetCurrentEnemyAttackPower();
            if (attackPower > 0f)
            {
                // 发送玩家受伤命令
                // 注意：这里需要通过某种方式获取到QFramework的架构引用
                // 但BattleJudgement不继承自任何QFramework类，所以需要通过事件或其他方式
                LogDebug($"玩家Miss，将受到 {attackPower} 点伤害");
                
                // 触发伤害事件
                OnPlayerDamaged?.Invoke(attackPower);
            }
        }
        
        /// <summary>
        /// 获取当前敌人的攻击力
        /// </summary>
        private float GetCurrentEnemyAttackPower()
        {
            // 从 GameManager 获取当前战斗敌人的攻击力
            var gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                var currentEnemy = gameManager.GetCurrentBattleEnemy();
                if (currentEnemy != null)
                {
                    var enemyConfig = currentEnemy.GetEnemyConfig();
                    if (enemyConfig != null)
                    {
                        return enemyConfig.attackPower;
                    }
                }
            }
            
            // 如果获取失败，返回默认值
            LogDebug("无法获取敌人攻击力，使用默认值");
            return 10f; // 默认伤害值
        }
        
        /// <summary>
        /// 获取统计摘要 - 简化版本
        /// </summary>
        public string GetStatisticsSummary()
        {
            int validInputs = totalInputs - invalidCount;
            int successCount = validInputs - missCount;
            float accuracy = GetAccuracy() * 100f;
            
            return $"总输入: {totalInputs} | Success: {successCount} | Miss: {missCount} | 准确率: {accuracy:F1}%";
        }
        
        /// <summary>
        /// 打印详细统计 - 简化版本
        /// </summary>
        public void PrintStatistics()
        {
            if (totalInputs == 0)
            {
                LogDebug("暂无输入数据");
                return;
            }
            
            int validInputs = totalInputs - invalidCount;
            int successCount = validInputs - missCount;
            float successRate = validInputs > 0 ? (float)successCount / validInputs * 100f : 0f;
            float missRate = validInputs > 0 ? (float)missCount / validInputs * 100f : 0f;
            float accuracy = GetAccuracy() * 100f;
            
            Debug.Log($"[战斗判定统计] 总输入: {totalInputs} | 有效: {validInputs} | 无效: {invalidCount}");
            Debug.Log($"Success: {successCount} ({successRate:F1}%) | Miss: {missCount} ({missRate:F1}%)");
            Debug.Log($"准确率: {accuracy:F1}%");
        }
        
        /// <summary>
        /// 调试日志
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[BattleJudgement] {message}");
            }
        }
        
        #region === 事件定义 ===
        
        /// <summary>
        /// 判定结果事件
        /// </summary>
        public System.Action<BattleJudgeResult, BattleNoteInfo> OnJudgeResult;
        
        /// <summary>
        /// Hold完成事件
        /// </summary>
        public System.Action<BattleJudgeResult, BattleNoteInfo> OnHoldComplete;
        
        /// <summary>
        /// 自动Miss事件
        /// </summary>
        public System.Action<BattleNoteInfo> OnAutoMiss;
        
        /// <summary>
        /// 玩家受伤事件
        /// </summary>
        public System.Action<float> OnPlayerDamaged;
        
        #endregion
    }
}
