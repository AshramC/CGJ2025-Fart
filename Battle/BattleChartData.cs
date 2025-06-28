using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FartGame.Battle
{
    /// <summary>
    /// 战斗谱面数据 ScriptableObject - 精简版本
    /// 专注于战斗系统的谱面配置，使用统一的BPM和拍数
    /// </summary>
    [CreateAssetMenu(fileName = "New Battle Chart", menuName = "Battle System/Battle Chart Data")]
    public class BattleChartData : ScriptableObject
    {
        [Header("谱面基本参数")]
        [Tooltip("每分钟节拍数")]
        [Range(60, 300)]
        public int bpm = 120;
        
        [Tooltip("总小节数")]
        [Range(1, 32)]
        public int measures = 8;
        
        [Tooltip("每小节拍数")]
        [Range(4, 16)]
        public int beatsPerMeasure = 8;
        
        [Header("下落设置")]
        [Tooltip("音符固定下落时间（秒）")]
        [Range(0.5f, 5f)]
        public float fixedDropTime = 2.0f;
        
        [Header("谱面信息")]
        [Tooltip("谱面名称")]
        public string chartName = "新战斗谱面";
        
        [Tooltip("谱面描述")]
        [TextArea(2, 4)]
        public string description = "战斗谱面描述";
        
        [Tooltip("难度等级")]
        [Range(1, 5)]
        public int difficulty = 1;
        
        [Header("事件数据")]
        [Tooltip("所有战斗事件列表")]
        public List<BattleBeatEvent> events = new List<BattleBeatEvent>();
        
        /// <summary>
        /// 获取指定位置的事件
        /// </summary>
        public BattleBeatEvent GetEventAt(int measure, int beat)
        {
            return events.Find(e => e.measure == measure && e.beat == beat);
        }
        
        /// <summary>
        /// 添加事件
        /// </summary>
        public void AddEvent(BattleBeatEvent beatEvent)
        {
            if (beatEvent == null)
            {
                Debug.LogWarning("[BattleChartData] 尝试添加空的BattleBeatEvent");
                return;
            }
            
            // 检查是否已存在相同位置的事件
            var existing = GetEventAt(beatEvent.measure, beatEvent.beat);
            if (existing != null)
            {
                Debug.LogWarning($"[BattleChartData] 位置 {beatEvent.GetPositionString()} 已存在事件");
                return;
            }
            
            events.Add(beatEvent);
            Debug.Log($"[BattleChartData] 添加事件: {beatEvent}");
        }
        
        /// <summary>
        /// 移除指定位置的事件
        /// </summary>
        public bool RemoveEventAt(int measure, int beat)
        {
            var eventToRemove = GetEventAt(measure, beat);
            if (eventToRemove != null)
            {
                events.Remove(eventToRemove);
                Debug.Log($"[BattleChartData] 移除事件: {eventToRemove}");
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 计算事件的绝对时间
        /// </summary>
        public float CalculateEventTime(BattleBeatEvent beatEvent)
        {
            if (bpm <= 0 || beatsPerMeasure <= 0) return 0f;
            
            float baseBeatDuration = 60f / bpm;  // 每拍时长
            float measureDuration = baseBeatDuration * beatsPerMeasure;  // 每小节时长
            
            return beatEvent.measure * measureDuration + beatEvent.beat * baseBeatDuration;
        }
        
        /// <summary>
        /// 计算Hold事件的持续时间
        /// </summary>
        public float CalculateHoldDuration(BattleBeatEvent holdEvent)
        {
            if (!holdEvent.IsHoldEvent()) return 0f;
            
            float baseBeatDuration = 60f / bpm;
            return holdEvent.CalculateDurationInBeats() * baseBeatDuration;
        }
        
        /// <summary>
        /// 获取谱面总时长
        /// </summary>
        public float GetTotalDuration()
        {
            if (bpm <= 0 || beatsPerMeasure <= 0) return 0f;
            
            float baseBeatDuration = 60f / bpm;
            float measureDuration = baseBeatDuration * beatsPerMeasure;
            
            return measures * measureDuration;
        }
        
        /// <summary>
        /// 验证谱面数据
        /// </summary>
        public (bool isValid, List<string> errors) ValidateChart()
        {
            var errors = new List<string>();
            
            // 验证基本参数
            if (bpm <= 0) errors.Add("BPM 必须大于 0");
            if (measures <= 0) errors.Add("小节数必须大于 0");
            if (beatsPerMeasure <= 0) errors.Add("每小节拍数必须大于 0");
            if (fixedDropTime <= 0) errors.Add("下落时间必须大于 0");
            
            // 验证事件数据
            foreach (var evt in events)
            {
                var (isValidEvent, errorMessage) = evt.Validate(measures, beatsPerMeasure);
                if (!isValidEvent)
                {
                    errors.Add($"事件 {evt} 验证失败: {errorMessage}");
                }
            }
            
            // 检查重复事件
            var duplicates = events
                .GroupBy(evt => new { evt.measure, evt.beat })
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
                
            foreach (var duplicate in duplicates)
            {
                errors.Add($"重复事件: 小节{duplicate.measure + 1}拍{duplicate.beat + 1}");
            }
            
            // 检查Hold事件冲突
            foreach (var holdEvent in events.Where(e => e.IsHoldEvent()))
            {
                for (int beatIndex = holdEvent.beat + 1; beatIndex <= holdEvent.holdEndBeat; beatIndex++)
                {
                    var conflictEvent = events.Find(e => e.measure == holdEvent.measure && e.beat == beatIndex);
                    if (conflictEvent != null && conflictEvent != holdEvent)
                    {
                        errors.Add($"Hold事件 {holdEvent.GetPositionString()} 与事件 {conflictEvent.GetPositionString()} 冲突");
                    }
                }
            }
            
            bool isValid = errors.Count == 0;
            if (isValid)
            {
                Debug.Log($"[BattleChartData] 谱面验证通过: {chartName}");
            }
            else
            {
                Debug.LogWarning($"[BattleChartData] 谱面验证失败: {chartName}\n错误: {string.Join("\n", errors)}");
            }
            
            return (isValid, errors);
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public void PrintStatistics()
        {
            var tapCount = events.Count(e => e.eventType == BattleEventType.Tap);
            var holdCount = events.Count(e => e.eventType == BattleEventType.Hold);
            
            Debug.Log($"[BattleChartData] 谱面统计 - {chartName}:");
            Debug.Log($"  小节数: {measures}, 每小节拍数: {beatsPerMeasure}, BPM: {bpm}");
            Debug.Log($"  总时长: {GetTotalDuration():F2}秒");
            Debug.Log($"  事件数: {events.Count} (轻拍:{tapCount}, 长按:{holdCount})");
            Debug.Log($"  难度: {difficulty}/5");
            Debug.Log($"  下落时间: {fixedDropTime}秒");
        }
        
        /// <summary>
        /// 清空所有事件
        /// </summary>
        public void ClearAllEvents()
        {
            events.Clear();
            Debug.Log($"[BattleChartData] 已清空所有事件");
        }
        
        /// <summary>
        /// 复制谱面数据
        /// </summary>
        public void CopyFromChart(BattleChartData sourceChart)
        {
            if (sourceChart == null)
            {
                Debug.LogError("[BattleChartData] 源谱面为空");
                return;
            }
            
            // 复制基本参数
            bpm = sourceChart.bpm;
            measures = sourceChart.measures;
            beatsPerMeasure = sourceChart.beatsPerMeasure;
            fixedDropTime = sourceChart.fixedDropTime;
            chartName = sourceChart.chartName;
            description = sourceChart.description;
            difficulty = sourceChart.difficulty;
            
            // 复制事件列表
            events.Clear();
            foreach (var evt in sourceChart.events)
            {
                var newEvent = new BattleBeatEvent
                {
                    measure = evt.measure,
                    beat = evt.beat,
                    eventType = evt.eventType,
                    holdEndBeat = evt.holdEndBeat
                };
                events.Add(newEvent);
            }
            
            Debug.Log($"[BattleChartData] 已从 {sourceChart.chartName} 复制谱面数据");
        }
        
        /// <summary>
        /// 创建测试谱面数据
        /// </summary>
        public void GenerateTestChart()
        {
            ClearAllEvents();
            
            // 生成简单的测试谱面：每小节第1拍和第5拍放置Tap事件
            for (int measure = 0; measure < measures; measure++)
            {
                // 第1拍
                AddEvent(new BattleBeatEvent(measure, 0));
                
                // 第5拍（如果有的话）
                if (beatsPerMeasure > 4)
                {
                    AddEvent(new BattleBeatEvent(measure, 4));
                }
                
                // 每隔一小节添加一个Hold事件
                if (measure % 2 == 0 && beatsPerMeasure > 6)
                {
                    AddEvent(new BattleBeatEvent(measure, 2, 6)); // 第3拍到第7拍的Hold
                }
            }
            
            Debug.Log($"[BattleChartData] 生成测试谱面，包含 {events.Count} 个事件");
        }
        
        // Unity编辑器验证
        void OnValidate()
        {
            // 确保参数在合理范围内
            bpm = Mathf.Clamp(bpm, 60, 300);
            measures = Mathf.Clamp(measures, 1, 32);
            beatsPerMeasure = Mathf.Clamp(beatsPerMeasure, 4, 16);
            difficulty = Mathf.Clamp(difficulty, 1, 5);
            fixedDropTime = Mathf.Clamp(fixedDropTime, 0.5f, 5f);
            
            // 如果谱面名为空，使用文件名
            if (string.IsNullOrEmpty(chartName))
            {
                chartName = name;
            }
        }
    }
}
