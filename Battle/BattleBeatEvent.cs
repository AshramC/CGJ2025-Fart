using UnityEngine;

namespace FartGame.Battle
{
    /// <summary>
    /// 战斗事件类型枚举 - 精简版本
    /// </summary>
    public enum BattleEventType
    {
        Tap = 1,    // 轻拍判定事件
        Hold = 2    // 长按判定事件
    }

    /// <summary>
    /// 战斗节拍事件数据结构 - 精简版本
    /// 只包含位置和类型信息，专注于谱面描述
    /// </summary>
    [System.Serializable]
    public class BattleBeatEvent
    {
        [Header("事件位置")]
        [Tooltip("小节索引（从0开始）")]
        public int measure;
        
        [Tooltip("拍子索引（从0开始）")]
        public int beat;
        
        [Header("事件类型")]
        [Tooltip("事件类型：Tap或Hold")]
        public BattleEventType eventType = BattleEventType.Tap;
        
        [Header("长按专用")]
        [Tooltip("Hold结束拍子（同小节内，仅Hold类型有效）")]
        public int holdEndBeat = 0;
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public BattleBeatEvent()
        {
            measure = 0;
            beat = 0;
            eventType = BattleEventType.Tap;
            holdEndBeat = 0;
        }
        
        /// <summary>
        /// Tap事件构造函数
        /// </summary>
        public BattleBeatEvent(int measure, int beat)
        {
            this.measure = measure;
            this.beat = beat;
            this.eventType = BattleEventType.Tap;
            this.holdEndBeat = 0;
        }
        
        /// <summary>
        /// Hold事件构造函数
        /// </summary>
        public BattleBeatEvent(int measure, int startBeat, int endBeat)
        {
            this.measure = measure;
            this.beat = startBeat;
            this.eventType = BattleEventType.Hold;
            this.holdEndBeat = endBeat;
        }
        
        /// <summary>
        /// 检查是否为Hold事件
        /// </summary>
        public bool IsHoldEvent()
        {
            return eventType == BattleEventType.Hold && holdEndBeat > beat;
        }
        
        /// <summary>
        /// 获取位置描述字符串
        /// </summary>
        public string GetPositionString()
        {
            if (IsHoldEvent())
            {
                return $"小节{measure + 1}拍{beat + 1}~拍{holdEndBeat + 1}";
            }
            else
            {
                return $"小节{measure + 1}拍{beat + 1}";
            }
        }
        
        /// <summary>
        /// 计算Hold持续拍数
        /// </summary>
        public int CalculateDurationInBeats()
        {
            if (IsHoldEvent())
            {
                return holdEndBeat - beat;
            }
            return 0;
        }
        
        /// <summary>
        /// 检查两个事件是否在同一位置
        /// </summary>
        public bool IsSamePosition(BattleBeatEvent other)
        {
            return measure == other.measure && beat == other.beat;
        }
        
        /// <summary>
        /// 验证事件数据的有效性
        /// </summary>
        public (bool isValid, string errorMessage) Validate(int maxMeasures, int beatsPerMeasure)
        {
            // 检查小节范围
            if (measure < 0 || measure >= maxMeasures)
                return (false, "小节索引超出范围");
                
            // 检查拍子范围
            if (beat < 0 || beat >= beatsPerMeasure)
                return (false, "拍子索引超出范围");
                
            // Hold事件特殊验证
            if (eventType == BattleEventType.Hold)
            {
                if (holdEndBeat <= beat)
                    return (false, "Hold结束拍子必须大于开始拍子");
                    
                if (holdEndBeat >= beatsPerMeasure)
                    return (false, "Hold结束拍子超出小节范围");
            }
            
            return (true, "");
        }
        
        /// <summary>
        /// 获取事件的显示字符串
        /// </summary>
        public override string ToString()
        {
            string typeChar = eventType switch
            {
                BattleEventType.Tap => "●",
                BattleEventType.Hold => "━",
                _ => "○"
            };
            
            return $"{GetPositionString()}: {typeChar} {eventType}";
        }
    }
}
