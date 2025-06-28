using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FartGame.Battle
{
    /// <summary>
    /// 战斗事件冲突类型枚举
    /// </summary>
    public enum ConflictType
    {
        None,                   // 无冲突
        SamePositionTap,       // 同位置Tap冲突
        TapHoldOverlap,        // Tap与Hold重叠
        HoldHoldOverlap,       // Hold与Hold重叠
        SamePositionHold       // 同位置Hold冲突
    }

    /// <summary>
    /// 冲突详情
    /// </summary>
    public struct ConflictDetail
    {
        public ConflictType type;
        public BattleBeatEvent conflictingEvent;
        public string description;
        public List<(int measure, int beat)> conflictPositions;

        public ConflictDetail(ConflictType type, BattleBeatEvent conflictingEvent, string description, List<(int, int)> positions = null)
        {
            this.type = type;
            this.conflictingEvent = conflictingEvent;
            this.description = description;
            this.conflictPositions = positions ?? new List<(int, int)>();
        }
    }

    /// <summary>
    /// 战斗事件冲突检测器
    /// 提供纯函数式的冲突检测算法
    /// </summary>
    public static class BattleEventConflictDetector
    {
        /// <summary>
        /// 获取事件占用的所有拍子位置
        /// </summary>
        /// <param name="beatEvent">要检查的事件</param>
        /// <returns>占用的位置集合 (measure, beat)</returns>
        public static HashSet<(int measure, int beat)> GetOccupiedBeats(BattleBeatEvent beatEvent)
        {
            var occupiedBeats = new HashSet<(int, int)>();
            
            if (beatEvent == null)
            {
                Debug.LogWarning("[ConflictDetector] 输入事件为空");
                return occupiedBeats;
            }

            if (beatEvent.eventType == BattleEventType.Tap)
            {
                // Tap事件只占用起始位置
                occupiedBeats.Add((beatEvent.measure, beatEvent.beat));
            }
            else if (beatEvent.eventType == BattleEventType.Hold && beatEvent.IsHoldEvent())
            {
                // Hold事件占用从开始到结束的所有拍子（包含边界）
                for (int beat = beatEvent.beat; beat <= beatEvent.holdEndBeat; beat++)
                {
                    occupiedBeats.Add((beatEvent.measure, beat));
                }
            }
            else
            {
                Debug.LogWarning($"[ConflictDetector] 无效的Hold事件: {beatEvent}");
                // 对于无效的Hold事件，至少标记起始位置
                occupiedBeats.Add((beatEvent.measure, beatEvent.beat));
            }

            return occupiedBeats;
        }

        /// <summary>
        /// 检查新事件是否与现有事件冲突
        /// </summary>
        /// <param name="newEvent">新事件</param>
        /// <param name="existingEvents">现有事件列表</param>
        /// <returns>是否存在冲突</returns>
        public static bool HasConflict(BattleBeatEvent newEvent, List<BattleBeatEvent> existingEvents)
        {
            if (newEvent == null || existingEvents == null || existingEvents.Count == 0)
                return false;

            var newEventBeats = GetOccupiedBeats(newEvent);
            
            foreach (var existingEvent in existingEvents)
            {
                if (existingEvent == null) continue;
                
                var existingEventBeats = GetOccupiedBeats(existingEvent);
                
                // 检查是否有重叠
                if (newEventBeats.Overlaps(existingEventBeats))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取详细的冲突信息
        /// </summary>
        /// <param name="newEvent">新事件</param>
        /// <param name="existingEvents">现有事件列表</param>
        /// <returns>冲突详情列表</returns>
        public static List<ConflictDetail> GetConflictDetails(BattleBeatEvent newEvent, List<BattleBeatEvent> existingEvents)
        {
            var conflicts = new List<ConflictDetail>();
            
            if (newEvent == null || existingEvents == null)
                return conflicts;

            var newEventBeats = GetOccupiedBeats(newEvent);
            
            foreach (var existingEvent in existingEvents)
            {
                if (existingEvent == null) continue;
                
                var existingEventBeats = GetOccupiedBeats(existingEvent);
                var overlappingBeats = newEventBeats.Intersect(existingEventBeats).ToList();
                
                if (overlappingBeats.Count > 0)
                {
                    var conflictType = DetermineConflictType(newEvent, existingEvent);
                    var description = GenerateConflictDescription(newEvent, existingEvent, conflictType, overlappingBeats);
                    
                    conflicts.Add(new ConflictDetail(
                        conflictType,
                        existingEvent,
                        description,
                        overlappingBeats
                    ));
                }
            }

            return conflicts;
        }

        /// <summary>
        /// 确定冲突类型
        /// </summary>
        private static ConflictType DetermineConflictType(BattleBeatEvent newEvent, BattleBeatEvent existingEvent)
        {
            bool newIsTap = newEvent.eventType == BattleEventType.Tap;
            bool newIsHold = newEvent.eventType == BattleEventType.Hold;
            bool existingIsTap = existingEvent.eventType == BattleEventType.Tap;
            bool existingIsHold = existingEvent.eventType == BattleEventType.Hold;

            if (newIsTap && existingIsTap)
            {
                return ConflictType.SamePositionTap;
            }
            else if ((newIsTap && existingIsHold) || (newIsHold && existingIsTap))
            {
                return ConflictType.TapHoldOverlap;
            }
            else if (newIsHold && existingIsHold)
            {
                // 检查是否是完全相同的位置
                if (newEvent.measure == existingEvent.measure && 
                    newEvent.beat == existingEvent.beat &&
                    newEvent.holdEndBeat == existingEvent.holdEndBeat)
                {
                    return ConflictType.SamePositionHold;
                }
                else
                {
                    return ConflictType.HoldHoldOverlap;
                }
            }

            return ConflictType.None;
        }

        /// <summary>
        /// 生成冲突描述
        /// </summary>
        private static string GenerateConflictDescription(BattleBeatEvent newEvent, BattleBeatEvent existingEvent, 
            ConflictType conflictType, List<(int measure, int beat)> overlappingBeats)
        {
            string newPos = newEvent.GetPositionString();
            string existingPos = existingEvent.GetPositionString();
            
            switch (conflictType)
            {
                case ConflictType.SamePositionTap:
                    return $"Tap事件位置冲突：新事件 {newPos} 与现有Tap事件 {existingPos} 在同一位置";
                
                case ConflictType.TapHoldOverlap:
                    if (newEvent.eventType == BattleEventType.Tap)
                        return $"Tap与Hold重叠：新Tap事件 {newPos} 与现有Hold事件 {existingPos} 重叠";
                    else
                        return $"Hold与Tap重叠：新Hold事件 {newPos} 与现有Tap事件 {existingPos} 重叠";
                
                case ConflictType.HoldHoldOverlap:
                    return $"Hold事件重叠：新Hold事件 {newPos} 与现有Hold事件 {existingPos} 存在时间重叠";
                
                case ConflictType.SamePositionHold:
                    return $"Hold事件完全重复：新Hold事件 {newPos} 与现有Hold事件完全相同";
                
                default:
                    return $"未知冲突类型：{newPos} 与 {existingPos}";
            }
        }

        /// <summary>
        /// 检查指定位置是否被占用
        /// </summary>
        /// <param name="measure">小节</param>
        /// <param name="beat">拍子</param>
        /// <param name="existingEvents">现有事件列表</param>
        /// <returns>是否被占用</returns>
        public static bool IsPositionOccupied(int measure, int beat, List<BattleBeatEvent> existingEvents)
        {
            if (existingEvents == null) return false;

            foreach (var existingEvent in existingEvents)
            {
                if (existingEvent == null) continue;
                
                var occupiedBeats = GetOccupiedBeats(existingEvent);
                if (occupiedBeats.Contains((measure, beat)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 寻找附近的空闲位置
        /// </summary>
        /// <param name="preferredMeasure">首选小节</param>
        /// <param name="preferredBeat">首选拍子</param>
        /// <param name="existingEvents">现有事件列表</param>
        /// <param name="maxMeasures">最大小节数</param>
        /// <param name="beatsPerMeasure">每小节拍数</param>
        /// <param name="searchRadius">搜索半径（拍数）</param>
        /// <returns>找到的空闲位置，如果没找到返回null</returns>
        public static (int measure, int beat)? FindNearbyFreePosition(
            int preferredMeasure, int preferredBeat, 
            List<BattleBeatEvent> existingEvents,
            int maxMeasures, int beatsPerMeasure, 
            int searchRadius = 4)
        {
            // 首先检查首选位置是否空闲
            if (!IsPositionOccupied(preferredMeasure, preferredBeat, existingEvents))
            {
                return (preferredMeasure, preferredBeat);
            }

            // 在附近搜索空闲位置
            for (int radius = 1; radius <= searchRadius; radius++)
            {
                // 检查前后的位置
                for (int offset = -radius; offset <= radius; offset += radius * 2)
                {
                    int targetBeat = preferredBeat + offset;
                    int targetMeasure = preferredMeasure;
                    
                    // 处理跨小节的情况
                    while (targetBeat < 0)
                    {
                        targetBeat += beatsPerMeasure;
                        targetMeasure--;
                    }
                    while (targetBeat >= beatsPerMeasure)
                    {
                        targetBeat -= beatsPerMeasure;
                        targetMeasure++;
                    }
                    
                    // 检查是否在有效范围内
                    if (targetMeasure >= 0 && targetMeasure < maxMeasures)
                    {
                        if (!IsPositionOccupied(targetMeasure, targetBeat, existingEvents))
                        {
                            return (targetMeasure, targetBeat);
                        }
                    }
                }
            }

            // 没有找到空闲位置
            return null;
        }

        /// <summary>
        /// 获取所有被占用的位置
        /// </summary>
        /// <param name="existingEvents">现有事件列表</param>
        /// <returns>所有被占用的位置集合</returns>
        public static HashSet<(int measure, int beat)> GetAllOccupiedPositions(List<BattleBeatEvent> existingEvents)
        {
            var allOccupied = new HashSet<(int, int)>();
            
            if (existingEvents == null) return allOccupied;

            foreach (var existingEvent in existingEvents)
            {
                if (existingEvent == null) continue;
                
                var occupied = GetOccupiedBeats(existingEvent);
                allOccupied.UnionWith(occupied);
            }

            return allOccupied;
        }

        /// <summary>
        /// 验证事件列表的完整性（检查所有事件之间是否有冲突）
        /// </summary>
        /// <param name="events">事件列表</param>
        /// <returns>所有冲突的详细信息</returns>
        public static List<ConflictDetail> ValidateEventList(List<BattleBeatEvent> events)
        {
            var allConflicts = new List<ConflictDetail>();
            
            if (events == null || events.Count <= 1) return allConflicts;

            for (int i = 0; i < events.Count; i++)
            {
                var currentEvent = events[i];
                if (currentEvent == null) continue;

                // 检查与之后所有事件的冲突（避免重复检查）
                var laterEvents = events.Skip(i + 1).ToList();
                var conflicts = GetConflictDetails(currentEvent, laterEvents);
                
                allConflicts.AddRange(conflicts);
            }

            return allConflicts;
        }

        /// <summary>
        /// 调试：打印冲突详情
        /// </summary>
        public static void DebugPrintConflicts(List<ConflictDetail> conflicts, string prefix = "[ConflictDetector]")
        {
            if (conflicts == null || conflicts.Count == 0)
            {
                Debug.Log($"{prefix} 未发现冲突");
                return;
            }

            Debug.LogWarning($"{prefix} 发现 {conflicts.Count} 个冲突:");
            for (int i = 0; i < conflicts.Count; i++)
            {
                var conflict = conflicts[i];
                Debug.LogError($"{prefix} 冲突 {i + 1}: {conflict.description}");
                
                if (conflict.conflictPositions.Count > 0)
                {
                    var positions = string.Join(", ", conflict.conflictPositions.Select(p => $"小节{p.measure + 1}拍{p.beat + 1}"));
                    Debug.LogError($"{prefix}   重叠位置: {positions}");
                }
            }
        }
    }
}
