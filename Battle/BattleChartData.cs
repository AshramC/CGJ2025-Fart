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
        /// <param name="beatEvent">要添加的事件</param>
        /// <param name="forceAdd">是否强制添加（忽略冲突）</param>
        /// <returns>是否成功添加</returns>
        public bool AddEvent(BattleBeatEvent beatEvent, bool forceAdd = false)
        {
            if (beatEvent == null)
            {
                Debug.LogWarning("[BattleChartData] 尝试添加空的BattleBeatEvent");
                return false;
            }
            
            // 验证事件数据的基本有效性
            var (isValidEvent, errorMessage) = beatEvent.Validate(measures, beatsPerMeasure);
            if (!isValidEvent)
            {
                Debug.LogError($"[BattleChartData] 事件数据无效: {beatEvent} - {errorMessage}");
                return false;
            }
            
            // 检查冲突（除非强制添加）
            if (!forceAdd)
            {
                var conflicts = BattleEventConflictDetector.GetConflictDetails(beatEvent, events);
                if (conflicts.Count > 0)
                {
                    Debug.LogError($"[BattleChartData] 事件添加失败，发现 {conflicts.Count} 个冲突:");
                    foreach (var conflict in conflicts)
                    {
                        Debug.LogError($"[BattleChartData]   - {conflict.description}");
                    }
                    return false;
                }
            }
            
            events.Add(beatEvent);
            Debug.Log($"[BattleChartData] 成功添加事件: {beatEvent}{(forceAdd ? " (强制添加)" : "")}");
            return true;
        }
        
        /// <summary>
        /// 安全添加事件（带详细结果信息）
        /// </summary>
        /// <param name="beatEvent">要添加的事件</param>
        /// <returns>添加结果和详细信息</returns>
        public (bool success, string message, List<ConflictDetail> conflicts) SafeAddEvent(BattleBeatEvent beatEvent)
        {
            if (beatEvent == null)
            {
                return (false, "事件数据为空", new List<ConflictDetail>());
            }
            
            // 验证事件数据的基本有效性
            var (isValidEvent, errorMessage) = beatEvent.Validate(measures, beatsPerMeasure);
            if (!isValidEvent)
            {
                return (false, $"事件数据无效: {errorMessage}", new List<ConflictDetail>());
            }
            
            // 检查冲突
            var conflicts = BattleEventConflictDetector.GetConflictDetails(beatEvent, events);
            if (conflicts.Count > 0)
            {
                string conflictMsg = $"发现 {conflicts.Count} 个冲突: " + 
                    string.Join("; ", conflicts.Select(c => c.description));
                return (false, conflictMsg, conflicts);
            }
            
            // 添加事件
            events.Add(beatEvent);
            return (true, $"成功添加事件: {beatEvent}", new List<ConflictDetail>());
        }
        
        /// <summary>
        /// 尝试在附近寻找空闲位置并添加事件
        /// </summary>
        /// <param name="beatEvent">要添加的事件</param>
        /// <param name="searchRadius">搜索半径</param>
        /// <returns>添加结果、最终位置和详细信息</returns>
        public (bool success, BattleBeatEvent finalEvent, string message) TryAddEventNearby(
            BattleBeatEvent beatEvent, int searchRadius = 4)
        {
            if (beatEvent == null)
            {
                return (false, null, "事件数据为空");
            }
            
            // 先尝试原位置
            var (directSuccess, directMessage, _) = SafeAddEvent(beatEvent);
            if (directSuccess)
            {
                return (true, beatEvent, directMessage);
            }
            
            // 原位置冲突，寻找附近空闲位置
            var freePosition = BattleEventConflictDetector.FindNearbyFreePosition(
                beatEvent.measure, beatEvent.beat, events, measures, beatsPerMeasure, searchRadius);
                
            if (freePosition.HasValue)
            {
                var (newMeasure, newBeat) = freePosition.Value;
                var adjustedEvent = new BattleBeatEvent(newMeasure, newBeat);
                
                // 如果是Hold事件，保持相同的持续时间
                if (beatEvent.IsHoldEvent())
                {
                    int duration = beatEvent.holdEndBeat - beatEvent.beat;
                    adjustedEvent = new BattleBeatEvent(newMeasure, newBeat, newBeat + duration);
                    
                    // 检查调整后的Hold事件是否超出边界
                    if (adjustedEvent.holdEndBeat >= beatsPerMeasure)
                    {
                        return (false, null, "附近没有足够空间放置Hold事件");
                    }
                }
                
                var (adjustedSuccess, adjustedMessage, _) = SafeAddEvent(adjustedEvent);
                if (adjustedSuccess)
                {
                    return (true, adjustedEvent, 
                        $"原位置冲突，已调整到 {adjustedEvent.GetPositionString()}: {adjustedMessage}");
                }
            }
            
            return (false, null, $"原位置冲突且附近无空闲位置: {directMessage}");
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
            
            // 验证单个事件数据
            foreach (var evt in events)
            {
                if (evt == null)
                {
                    errors.Add("发现空的事件数据");
                    continue;
                }
                
                var (isValidEvent, errorMessage) = evt.Validate(measures, beatsPerMeasure);
                if (!isValidEvent)
                {
                    errors.Add($"事件 {evt} 验证失败: {errorMessage}");
                }
            }
            
            // 使用新的冲突检测系统检查所有事件冲突
            var allConflicts = BattleEventConflictDetector.ValidateEventList(events);
            if (allConflicts.Count > 0)
            {
                // 按冲突类型分组报告
                var conflictGroups = allConflicts.GroupBy(c => c.type);
                
                foreach (var group in conflictGroups)
                {
                    string conflictTypeName = GetConflictTypeName(group.Key);
                    errors.Add($"{conflictTypeName} ({group.Count()}个):");
                    
                    foreach (var conflict in group)
                    {
                        errors.Add($"  - {conflict.description}");
                    }
                }
                
                // 调试输出详细冲突信息
                BattleEventConflictDetector.DebugPrintConflicts(allConflicts, "[BattleChartData]");
            }
            
            bool isValid = errors.Count == 0;
            if (isValid)
            {
                Debug.Log($"[BattleChartData] 谱面验证通过: {chartName} (共{events.Count}个事件)");
            }
            else
            {
                Debug.LogWarning($"[BattleChartData] 谱面验证失败: {chartName}\n错误: {string.Join("\n", errors)}");
            }
            
            return (isValid, errors);
        }
        
        /// <summary>
        /// 获取冲突类型的中文名称
        /// </summary>
        private string GetConflictTypeName(ConflictType conflictType)
        {
            return conflictType switch
            {
                ConflictType.SamePositionTap => "Tap事件位置重复",
                ConflictType.TapHoldOverlap => "Tap与Hold重叠",
                ConflictType.HoldHoldOverlap => "Hold事件时间重叠",
                ConflictType.SamePositionHold => "Hold事件完全重复",
                _ => "未知冲突类型"
            };
        }
        
        /// <summary>
        /// 获取谱面冲突统计信息
        /// </summary>
        public (int totalConflicts, Dictionary<ConflictType, int> conflictCounts) GetConflictStatistics()
        {
            var allConflicts = BattleEventConflictDetector.ValidateEventList(events);
            var conflictCounts = new Dictionary<ConflictType, int>();
            
            foreach (var conflict in allConflicts)
            {
                if (conflictCounts.ContainsKey(conflict.type))
                    conflictCounts[conflict.type]++;
                else
                    conflictCounts[conflict.type] = 1;
            }
            
            return (allConflicts.Count, conflictCounts);
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
            
            // 复制事件列表（强制添加以保持原始数据完整性）
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
                AddEvent(newEvent, true); // 强制添加，保持原数据
            }
            
            // 验证复制后的数据
            var (isValid, validationErrors) = ValidateChart();
            if (!isValid)
            {
                Debug.LogWarning($"[BattleChartData] 复制的谱面数据存在冲突:\n{string.Join("\n", validationErrors)}");
            }
            
            Debug.Log($"[BattleChartData] 已从 {sourceChart.chartName} 复制谱面数据，验证结果: {(isValid ? "通过" : "存在冲突")}");
        }
        
        /// <summary>
        /// 创建测试谱面数据（使用安全添加方法）
        /// </summary>
        public void GenerateTestChart()
        {
            ClearAllEvents();
            
            // 生成简单的测试谱面：每小节第1拍和第5拍放置Tap事件
            for (int measure = 0; measure < measures; measure++)
            {
                // 第1拍
                AddEvent(new BattleBeatEvent(measure, 0), false);
                
                // 第5拍（如果有的话）
                if (beatsPerMeasure > 4)
                {
                    AddEvent(new BattleBeatEvent(measure, 4), false);
                }
                
                // 每隔一小节添加一个Hold事件（确保不与已有事件冲突）
                if (measure % 2 == 0 && beatsPerMeasure > 6)
                {
                    // 尝试在第3拍到第7拍添加Hold，如果冲突则寻找附近位置
                    var holdEvent = new BattleBeatEvent(measure, 2, 6);
                    var (success, finalEvent, message) = TryAddEventNearby(holdEvent);
                    if (!success)
                    {
                        Debug.LogWarning($"[BattleChartData] 无法添加Hold事件到小节{measure + 1}: {message}");
                    }
                }
            }
            
            // 验证生成的谱面
            var (isValid, validationErrors) = ValidateChart();
            if (!isValid)
            {
                Debug.LogError($"[BattleChartData] 生成的测试谱面存在问题:\n{string.Join("\n", validationErrors)}");
            }
            
            Debug.Log($"[BattleChartData] 生成测试谱面完成，包含 {events.Count} 个事件，验证结果: {(isValid ? "通过" : "失败")}");
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
