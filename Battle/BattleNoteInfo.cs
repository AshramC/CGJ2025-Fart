using UnityEngine;

namespace FartGame.Battle
{
    /// <summary>
    /// 战斗音符状态枚举
    /// </summary>
    public enum BattleNoteState
    {
        Waiting,     // 等待击中
        Hit,         // 已击中
        Missed,      // 已错过
        Cleanup      // 已清理
    }

    /// <summary>
    /// Hold事件状态枚举
    /// </summary>
    public enum HoldState
    {
        WaitingForPress,    // 等待按下
        Holding,           // 持续中
        Completed,         // 已完成
        Failed            // 失败
    }

    /// <summary>
    /// 战斗音符信息 - 运行时数据结构
    /// 存储预计算的时间信息和运行时状态
    /// </summary>
    [System.Serializable]
    public class BattleNoteInfo
    {
        [Header("预计算时间信息")]
        [Tooltip("音符的精确判定时间")]
        public double judgementTime;
        
        [Tooltip("音符的生成时间")]
        public double spawnTime;
        
        [Header("事件信息")]
        [Tooltip("音符类型")]
        public BattleEventType eventType;
        
        [Tooltip("原始节拍事件数据")]
        public BattleBeatEvent beatEvent;
        
        [Header("Hold事件信息")]
        [Tooltip("是否为Hold音符")]
        public bool isHoldNote = false;
        
        [Tooltip("Hold结束时间")]
        public double holdEndTime = 0.0;
        
        [Tooltip("Hold持续时间（秒）")]
        public float holdDuration = 0f;
        
        [Header("运行时状态")]
        [Tooltip("是否已经被处理过")]
        public bool isProcessed = false;
        
        [Tooltip("音符当前状态")]
        public BattleNoteState noteState = BattleNoteState.Waiting;
        
        [Header("Hold状态信息")]
        [Tooltip("Hold状态（仅Hold音符有效）")]
        public HoldState holdState = HoldState.WaitingForPress;
        
        [Tooltip("Hold完成度（0-1）")]
        [Range(0f, 1f)]
        public float holdCompletion = 0f;
        
        [Tooltip("Hold开始时间")]
        public double holdStartTime = 0.0;
        
        /// <summary>
        /// 构造函数 - 存储预计算的时间
        /// </summary>
        public BattleNoteInfo(double judgementTime, double spawnTime, BattleBeatEvent beatEvent)
        {
            this.judgementTime = judgementTime;
            this.spawnTime = spawnTime;
            this.beatEvent = beatEvent;
            this.eventType = beatEvent.eventType;
            this.isProcessed = false;
            this.noteState = BattleNoteState.Waiting;
            
            // 设置Hold信息
            if (beatEvent.IsHoldEvent())
            {
                this.isHoldNote = true;
                this.holdState = HoldState.WaitingForPress;
                this.holdCompletion = 0f;
            }
        }
        
        /// <summary>
        /// 默认构造函数（用于序列化）
        /// </summary>
        public BattleNoteInfo()
        {
            // 用于序列化
        }
        
        /// <summary>
        /// 设置Hold时间信息
        /// </summary>
        public void SetHoldTiming(double holdEndTime, float holdDuration)
        {
            if (isHoldNote)
            {
                this.holdEndTime = holdEndTime;
                this.holdDuration = holdDuration;
            }
        }
        
        /// <summary>
        /// 获取与输入时间的偏差
        /// </summary>
        public float GetTimingError(double inputTime)
        {
            return (float)(inputTime - judgementTime);
        }
        
        /// <summary>
        /// 检查此音符是否在判定窗口内
        /// </summary>
        public bool IsInJudgementWindow(double inputTime, float maxWindow = 0.2f)
        {
            return Mathf.Abs(GetTimingError(inputTime)) <= maxWindow;
        }
        
        /// <summary>
        /// 检查音符是否可以被判定
        /// </summary>
        public bool CanBeJudged()
        {
            return !isProcessed && noteState == BattleNoteState.Waiting;
        }
        
        /// <summary>
        /// 标记为已击中
        /// </summary>
        public void MarkAsHit()
        {
            isProcessed = true;
            noteState = BattleNoteState.Hit;
            
            if (isHoldNote)
            {
                holdState = HoldState.Holding;
                holdStartTime = judgementTime;
            }
        }
        
        /// <summary>
        /// 标记为已错过
        /// </summary>
        public void MarkAsMissed()
        {
            isProcessed = true;
            noteState = BattleNoteState.Missed;
            
            if (isHoldNote)
            {
                holdState = HoldState.Failed;
            }
        }
        
        /// <summary>
        /// 标记为已清理
        /// </summary>
        public void MarkAsCleanup()
        {
            noteState = BattleNoteState.Cleanup;
        }
        
        /// <summary>
        /// 重置处理状态（用于重新开始）
        /// </summary>
        public void ResetProcessedState()
        {
            isProcessed = false;
            noteState = BattleNoteState.Waiting;
            holdState = HoldState.WaitingForPress;
            holdCompletion = 0f;
            holdStartTime = 0.0;
        }
        
        /// <summary>
        /// 更新Hold进度
        /// </summary>
        public void UpdateHoldProgress(double currentTime, bool isHolding)
        {
            if (!isHoldNote || holdState != HoldState.Holding) return;
            
            if (isHolding)
            {
                // 计算Hold完成度
                double elapsedTime = currentTime - holdStartTime;
                holdCompletion = Mathf.Clamp01((float)(elapsedTime / holdDuration));
                
                // 检查是否完成
                if (currentTime >= holdEndTime)
                {
                    holdState = HoldState.Completed;
                    holdCompletion = 1f;
                }
            }
            else
            {
                // 松开了按键，Hold失败
                holdState = HoldState.Failed;
            }
        }
        
        /// <summary>
        /// 检查Hold是否已完成
        /// </summary>
        public bool IsHoldCompleted()
        {
            return isHoldNote && holdState == HoldState.Completed;
        }
        
        /// <summary>
        /// 检查Hold是否失败
        /// </summary>
        public bool IsHoldFailed()
        {
            return isHoldNote && holdState == HoldState.Failed;
        }
        
        /// <summary>
        /// 获取音符位置信息
        /// </summary>
        public string GetBeatPosition()
        {
            return beatEvent?.GetPositionString() ?? "未知位置";
        }
        
        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            string info = $"BattleNote[{eventType}]: 判定时间={judgementTime:F3}s, 生成时间={spawnTime:F3}s, 位置={GetBeatPosition()}";
            
            if (isHoldNote)
            {
                info += $", Hold({holdDuration:F2}s, {holdCompletion:P0})";
            }
            
            return info;
        }
        
        /// <summary>
        /// 比较方法，用于排序
        /// </summary>
        public int CompareTo(BattleNoteInfo other)
        {
            if (other == null) return 1;
            return judgementTime.CompareTo(other.judgementTime);
        }
        
        public override string ToString()
        {
            return $"BattleNote[{eventType}]@{judgementTime:F2}s ({GetBeatPosition()})";
        }
    }
}
