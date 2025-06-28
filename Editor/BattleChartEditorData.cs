using UnityEngine;
using System.Collections.Generic;
using FartGame.Battle;

namespace FartGame.Editor
{
    /// <summary>
    /// 编辑器专用音符信息
    /// 扩展BattleBeatEvent以支持编辑器特性
    /// </summary>
    [System.Serializable]
    public class EditorNoteInfo
    {
        [Header("基础信息")]
        public int measure;
        public int beat;
        public BattleEventType noteType = BattleEventType.Tap;
        public int holdEndBeat = 0;
        
        [Header("编辑器状态")]
        public bool isSelected = false;
        
        [Header("时间信息")]
        public float beatPosition;      // 节拍位置（支持小数）
        public double judgementTime;    // 精确判定时间
        public double holdDuration;     // Hold音符持续时间
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public EditorNoteInfo()
        {
            measure = 0;
            beat = 0;
            noteType = BattleEventType.Tap;
            holdEndBeat = 0;
            isSelected = false;
            beatPosition = 0f;
            judgementTime = 0.0;
            holdDuration = 0.0;
        }
        
        /// <summary>
        /// 从BattleBeatEvent创建
        /// </summary>
        public EditorNoteInfo(BattleBeatEvent battleEvent)
        {
            measure = battleEvent.measure;
            beat = battleEvent.beat;
            noteType = battleEvent.eventType;
            holdEndBeat = battleEvent.holdEndBeat;
            isSelected = false;
            
            // 计算节拍位置
            beatPosition = measure * 8 + beat; // 假设每小节8拍，将在CalculateBeatPosition中重新计算
            judgementTime = 0.0; // 将在CalculateJudgementTime中计算
            holdDuration = 0.0;  // 将在CalculateHoldDuration中计算
        }
        
        /// <summary>
        /// 转换为BattleBeatEvent
        /// </summary>
        public BattleBeatEvent ToBattleBeatEvent()
        {
            var battleEvent = new BattleBeatEvent(measure, beat);
            battleEvent.eventType = noteType;
            battleEvent.holdEndBeat = holdEndBeat;
            return battleEvent;
        }
        
        /// <summary>
        /// 检查是否为Hold音符
        /// </summary>
        public bool IsHoldNote()
        {
            return noteType == BattleEventType.Hold && holdEndBeat > beat;
        }
        
        /// <summary>
        /// 获取位置描述字符串
        /// </summary>
        public string GetPositionString()
        {
            if (IsHoldNote())
            {
                return $"M{measure + 1}B{beat + 1}~B{holdEndBeat + 1}";
            }
            else
            {
                return $"M{measure + 1}B{beat + 1}";
            }
        }
        
        /// <summary>
        /// 计算Hold持续拍数
        /// </summary>
        public int GetHoldDurationInBeats()
        {
            if (IsHoldNote())
            {
                return holdEndBeat - beat;
            }
            return 0;
        }
        
        /// <summary>
        /// 更新时间相关信息
        /// </summary>
        public void UpdateTiming(float bpm, int beatsPerMeasure)
        {
            // 重新计算节拍位置
            beatPosition = measure * beatsPerMeasure + beat;
            
            // 计算判定时间
            float beatDuration = 60f / bpm;
            judgementTime = beatPosition * beatDuration;
            
            // 计算Hold持续时间
            if (IsHoldNote())
            {
                holdDuration = GetHoldDurationInBeats() * beatDuration;
            }
            else
            {
                holdDuration = 0.0;
            }
        }
        
        /// <summary>
        /// 检查是否与另一个音符在同一位置
        /// </summary>
        public bool IsSamePosition(EditorNoteInfo other)
        {
            return measure == other.measure && beat == other.beat;
        }
        
        /// <summary>
        /// 复制音符信息
        /// </summary>
        public EditorNoteInfo Clone()
        {
            var clone = new EditorNoteInfo();
            clone.measure = measure;
            clone.beat = beat;
            clone.noteType = noteType;
            clone.holdEndBeat = holdEndBeat;
            clone.isSelected = false; // 克隆时不保持选中状态
            clone.beatPosition = beatPosition;
            clone.judgementTime = judgementTime;
            clone.holdDuration = holdDuration;
            return clone;
        }
        
        public override string ToString()
        {
            string typeChar = noteType switch
            {
                BattleEventType.Tap => "●",
                BattleEventType.Hold => "━",
                _ => "○"
            };
            
            return $"{GetPositionString()}: {typeChar} {noteType} (T:{judgementTime:F2}s)";
        }
    }
    
    /// <summary>
    /// 战斗谱面编辑器数据
    /// 管理编辑器状态和数据转换
    /// </summary>
    [System.Serializable]
    public class BattleChartEditorData
    {
        [Header("谱面基本信息")]
        public string chartName = "新谱面";
        public int bpm = 120;
        public int measures = 4;
        public int beatsPerMeasure = 8;
        public float audioOffset = 0f;
        
        [Header("音频设置")]
        public AudioClip audioClip;
        
        [Header("编辑器音符数据")]
        public List<EditorNoteInfo> notes = new List<EditorNoteInfo>();
        
        [Header("编辑器状态")]
        public List<EditorNoteInfo> selectedNotes = new List<EditorNoteInfo>();
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public BattleChartEditorData()
        {
            chartName = "新谱面";
            bpm = 120;
            measures = 4;
            beatsPerMeasure = 8;
            audioOffset = 0f;
            audioClip = null;
            notes = new List<EditorNoteInfo>();
            selectedNotes = new List<EditorNoteInfo>();
        }
        
        /// <summary>
        /// 从BattleChartData加载数据
        /// </summary>
        public void FromBattleChartData(BattleChartData chartData)
        {
            if (chartData == null)
            {
                Debug.LogError("[BattleChartEditorData] chartData为空");
                return;
            }
            
            // 复制基本信息
            chartName = chartData.chartName;
            bpm = chartData.bpm;
            measures = chartData.measures;
            beatsPerMeasure = chartData.beatsPerMeasure;
            
            // 清空并重新加载音符数据
            notes.Clear();
            selectedNotes.Clear();
            
            foreach (var battleEvent in chartData.events)
            {
                var editorNote = new EditorNoteInfo(battleEvent);
                editorNote.UpdateTiming(bpm, beatsPerMeasure);
                notes.Add(editorNote);
            }
            
            Debug.Log($"[BattleChartEditorData] 从BattleChartData加载: {chartName}, {notes.Count}个音符");
        }
        
        /// <summary>
        /// 转换为BattleChartData
        /// </summary>
        public BattleChartData ToBattleChartData()
        {
            var chartData = ScriptableObject.CreateInstance<BattleChartData>();
            ToBattleChartData(chartData);
            return chartData;
        }
        
        /// <summary>
        /// 更新现有的BattleChartData
        /// </summary>
        public void ToBattleChartData(BattleChartData chartData)
        {
            if (chartData == null)
            {
                Debug.LogError("[BattleChartEditorData] chartData为空");
                return;
            }
            
            // 复制基本信息
            chartData.chartName = chartName;
            chartData.bpm = bpm;
            chartData.measures = measures;
            chartData.beatsPerMeasure = beatsPerMeasure;
            
            // 清空并重新填充事件数据
            chartData.events.Clear();
            
            foreach (var editorNote in notes)
            {
                var battleEvent = editorNote.ToBattleBeatEvent();
                chartData.events.Add(battleEvent);
            }
            
            Debug.Log($"[BattleChartEditorData] 转换为BattleChartData: {chartName}, {chartData.events.Count}个事件");
        }
        
        /// <summary>
        /// 计算节拍位置到判定时间的转换
        /// </summary>
        public double CalculateJudgementTime(float beatPosition)
        {
            if (bpm <= 0) return 0.0;
            
            float beatDuration = 60f / bpm;
            return beatPosition * beatDuration + audioOffset;
        }
        
        /// <summary>
        /// 计算判定时间到节拍位置的转换
        /// </summary>
        public float CalculateBeatPosition(double judgementTime)
        {
            if (bpm <= 0) return 0f;
            
            float beatDuration = 60f / bpm;
            return (float)((judgementTime - audioOffset) / beatDuration);
        }
        
        /// <summary>
        /// 获取谱面总时长
        /// </summary>
        public float GetTotalDuration()
        {
            if (bpm <= 0 || beatsPerMeasure <= 0) return 0f;
            
            float beatDuration = 60f / bpm;
            float measureDuration = beatDuration * beatsPerMeasure;
            
            return measures * measureDuration;
        }
        
        /// <summary>
        /// 添加音符
        /// </summary>
        public bool AddNote(int measure, int beat, BattleEventType noteType)
        {
            // 检查位置是否有效
            if (measure < 0 || measure >= measures || beat < 0 || beat >= beatsPerMeasure)
            {
                Debug.LogWarning($"[BattleChartEditorData] 音符位置无效: M{measure}B{beat}");
                return false;
            }
            
            // 检查是否已存在音符
            var existing = GetNoteAt(measure, beat);
            if (existing != null)
            {
                Debug.LogWarning($"[BattleChartEditorData] 位置已存在音符: M{measure + 1}B{beat + 1}");
                return false;
            }
            
            // 创建新音符
            var newNote = new EditorNoteInfo();
            newNote.measure = measure;
            newNote.beat = beat;
            newNote.noteType = noteType;
            
            // 如果是Hold音符，设置默认结束位置
            if (noteType == BattleEventType.Hold)
            {
                newNote.holdEndBeat = Mathf.Min(beat + 2, beatsPerMeasure - 1);
            }
            
            // 更新时间信息
            newNote.UpdateTiming(bpm, beatsPerMeasure);
            
            notes.Add(newNote);
            
            Debug.Log($"[BattleChartEditorData] 添加音符: {newNote}");
            return true;
        }
        
        /// <summary>
        /// 移除指定位置的音符
        /// </summary>
        public bool RemoveNoteAt(int measure, int beat)
        {
            var noteToRemove = GetNoteAt(measure, beat);
            if (noteToRemove != null)
            {
                notes.Remove(noteToRemove);
                selectedNotes.Remove(noteToRemove);
                
                Debug.Log($"[BattleChartEditorData] 移除音符: M{measure + 1}B{beat + 1}");
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 获取指定位置的音符
        /// </summary>
        public EditorNoteInfo GetNoteAt(int measure, int beat)
        {
            return notes.Find(note => note.measure == measure && note.beat == beat);
        }
        
        /// <summary>
        /// 清空所有音符
        /// </summary>
        public void ClearAllNotes()
        {
            notes.Clear();
            selectedNotes.Clear();
            Debug.Log("[BattleChartEditorData] 清空所有音符");
        }
        
        /// <summary>
        /// 选择音符
        /// </summary>
        public void SelectNote(EditorNoteInfo note, bool multiSelect = false)
        {
            if (note == null) return;
            
            if (!multiSelect)
            {
                ClearSelection();
            }
            
            if (!selectedNotes.Contains(note))
            {
                note.isSelected = true;
                selectedNotes.Add(note);
            }
        }
        
        /// <summary>
        /// 取消选择音符
        /// </summary>
        public void DeselectNote(EditorNoteInfo note)
        {
            if (note == null) return;
            
            note.isSelected = false;
            selectedNotes.Remove(note);
        }
        
        /// <summary>
        /// 清空选择
        /// </summary>
        public void ClearSelection()
        {
            foreach (var note in selectedNotes)
            {
                note.isSelected = false;
            }
            selectedNotes.Clear();
        }
        
        /// <summary>
        /// 复制选中的音符
        /// </summary>
        public List<EditorNoteInfo> CopySelectedNotes()
        {
            var copiedNotes = new List<EditorNoteInfo>();
            foreach (var note in selectedNotes)
            {
                copiedNotes.Add(note.Clone());
            }
            return copiedNotes;
        }
        
        /// <summary>
        /// 粘贴音符
        /// </summary>
        public void PasteNotes(List<EditorNoteInfo> notesToPaste, int targetMeasure, int targetBeat)
        {
            if (notesToPaste == null || notesToPaste.Count == 0) return;
            
            // 计算偏移量
            var firstNote = notesToPaste[0];
            int measureOffset = targetMeasure - firstNote.measure;
            int beatOffset = targetBeat - firstNote.beat;
            
            foreach (var note in notesToPaste)
            {
                int newMeasure = note.measure + measureOffset;
                int newBeat = note.beat + beatOffset;
                
                // 检查新位置是否有效
                if (newMeasure >= 0 && newMeasure < measures && 
                    newBeat >= 0 && newBeat < beatsPerMeasure)
                {
                    AddNote(newMeasure, newBeat, note.noteType);
                }
            }
        }
        
        /// <summary>
        /// 更新所有音符的时间信息
        /// </summary>
        public void UpdateAllNoteTiming()
        {
            foreach (var note in notes)
            {
                note.UpdateTiming(bpm, beatsPerMeasure);
            }
        }
        
        /// <summary>
        /// 验证编辑器数据
        /// </summary>
        public (bool isValid, List<string> errors) ValidateData()
        {
            var errors = new List<string>();
            
            // 验证基本参数
            if (bpm <= 0) errors.Add("BPM必须大于0");
            if (measures <= 0) errors.Add("小节数必须大于0");
            if (beatsPerMeasure <= 0) errors.Add("每小节拍数必须大于0");
            
            // 验证音符位置
            foreach (var note in notes)
            {
                if (note.measure < 0 || note.measure >= measures)
                {
                    errors.Add($"音符 {note.GetPositionString()} 小节位置无效");
                }
                
                if (note.beat < 0 || note.beat >= beatsPerMeasure)
                {
                    errors.Add($"音符 {note.GetPositionString()} 拍子位置无效");
                }
                
                // 验证Hold音符
                if (note.IsHoldNote())
                {
                    if (note.holdEndBeat <= note.beat)
                    {
                        errors.Add($"Hold音符 {note.GetPositionString()} 结束位置无效");
                    }
                    
                    if (note.holdEndBeat >= beatsPerMeasure)
                    {
                        errors.Add($"Hold音符 {note.GetPositionString()} 结束位置超出小节范围");
                    }
                }
            }
            
            bool isValid = errors.Count == 0;
            return (isValid, errors);
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public string GetStatistics()
        {
            int tapCount = 0;
            int holdCount = 0;
            
            foreach (var note in notes)
            {
                if (note.noteType == BattleEventType.Tap)
                    tapCount++;
                else if (note.noteType == BattleEventType.Hold)
                    holdCount++;
            }
            
            return $"总音符: {notes.Count} | Tap: {tapCount} | Hold: {holdCount} | 选中: {selectedNotes.Count}";
        }
    }
}
