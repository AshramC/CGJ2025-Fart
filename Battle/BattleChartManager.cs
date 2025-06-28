using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FartGame.Battle
{
    /// <summary>
    /// 战斗谱面管理器 - 核心谱面逻辑
    /// 负责谱面数据解析、Note生成、时间计算和生命周期管理
    /// </summary>
    public class BattleChartManager
    {
        [Header("谱面数据")]
        private BattleChartData currentChart;
        private List<BattleNoteInfo> allNotes;
        
        [Header("Note队列管理")]
        private Queue<BattleNoteInfo> upcomingNotes;
        private List<BattleNoteInfo> activeNotes;
        
        [Header("依赖引用")]
        private MusicTimeManager musicTimeManager;
        
        [Header("运行时状态")]
        private bool isInitialized = false;
        private int totalNotes = 0;
        private int processedNotes = 0;
        
        [Header("调试设置")]
        private bool enableDebugLog = true;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BattleChartManager(MusicTimeManager timeManager)
        {
            musicTimeManager = timeManager;
            upcomingNotes = new Queue<BattleNoteInfo>();
            activeNotes = new List<BattleNoteInfo>();
            allNotes = new List<BattleNoteInfo>();
        }
        
        /// <summary>
        /// 加载谱面数据
        /// </summary>
        public bool LoadChart(BattleChartData chartData)
        {
            if (chartData == null)
            {
                Debug.LogError("[BattleChartManager] 谱面数据为空");
                return false;
            }
            
            // 验证谱面数据
            var (isValid, errors) = chartData.ValidateChart();
            if (!isValid)
            {
                Debug.LogError($"[BattleChartManager] 谱面验证失败: {string.Join(", ", errors)}");
                return false;
            }
            
            currentChart = chartData;
            
            // 生成所有音符信息
            if (!GenerateAllNotes())
            {
                return false;
            }
            
            // 初始化队列
            InitializeNoteQueues();
            
            isInitialized = true;
            LogDebug($"谱面加载成功: {chartData.chartName}, 总音符数: {totalNotes}");
            
            return true;
        }
        
        /// <summary>
        /// 生成所有音符信息
        /// </summary>
        private bool GenerateAllNotes()
        {
            if (currentChart == null) return false;
            
            allNotes.Clear();
            
            try
            {
                foreach (var beatEvent in currentChart.events)
                {
                    var noteInfo = CreateNoteInfoFromBeatEvent(beatEvent);
                    if (noteInfo != null)
                    {
                        allNotes.Add(noteInfo);
                    }
                }
                
                // 按时间排序
                allNotes.Sort((a, b) => a.judgementTime.CompareTo(b.judgementTime));
                
                totalNotes = allNotes.Count;
                processedNotes = 0;
                
                LogDebug($"生成了 {totalNotes} 个音符");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BattleChartManager] 生成音符失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 从BeatEvent创建NoteInfo
        /// </summary>
        private BattleNoteInfo CreateNoteInfoFromBeatEvent(BattleBeatEvent beatEvent)
        {
            if (beatEvent == null || currentChart == null) return null;
            
            try
            {
                // 计算事件时间
                float eventTime = currentChart.CalculateEventTime(beatEvent);
                double judgementTime = eventTime;
                double spawnTime = judgementTime - currentChart.fixedDropTime;
                
                var noteInfo = new BattleNoteInfo(judgementTime, spawnTime, beatEvent);
                
                // Hold事件特殊处理
                if (beatEvent.IsHoldEvent())
                {
                    float holdDuration = currentChart.CalculateHoldDuration(beatEvent);
                    double holdEndTime = judgementTime + holdDuration;
                    
                    noteInfo.SetHoldTiming(holdEndTime, holdDuration);
                    
                    LogDebug($"生成Hold音符: {noteInfo.GetBeatPosition()}, 持续时间: {holdDuration:F2}s");
                }
                
                return noteInfo;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BattleChartManager] 创建NoteInfo失败: {beatEvent} - {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 初始化Note队列
        /// </summary>
        private void InitializeNoteQueues()
        {
            upcomingNotes.Clear();
            activeNotes.Clear();
            
            // 重置所有音符状态
            foreach (var note in allNotes)
            {
                note.ResetProcessedState();
                upcomingNotes.Enqueue(note);
            }
            
            LogDebug($"队列初始化完成，等待音符: {upcomingNotes.Count}");
        }
        
        /// <summary>
        /// 更新Note生命周期
        /// </summary>
        public void UpdateNoteLifecycle()
        {
            if (!isInitialized || musicTimeManager == null) return;
            
            double currentTime = musicTimeManager.GetJudgementTime();
            
            // 处理即将到来的音符
            ProcessUpcomingNotes(currentTime);
            
            // 更新活跃音符
            UpdateActiveNotes(currentTime);
        }
        
        /// <summary>
        /// 处理即将到来的音符
        /// </summary>
        private void ProcessUpcomingNotes(double currentTime)
        {
            while (upcomingNotes.Count > 0)
            {
                var nextNote = upcomingNotes.Peek();
                
                if (currentTime >= nextNote.spawnTime)
                {
                    var note = upcomingNotes.Dequeue();
                    SpawnNote(note);
                }
                else
                {
                    break;
                }
            }
        }
        
        /// <summary>
        /// 生成音符
        /// </summary>
        private void SpawnNote(BattleNoteInfo noteInfo)
        {
            activeNotes.Add(noteInfo);
            
            // 触发UI事件
            OnNoteSpawn?.Invoke(noteInfo);
            
            LogDebug($"生成音符: {noteInfo}");
        }
        
        /// <summary>
        /// 更新活跃音符
        /// </summary>
        private void UpdateActiveNotes(double currentTime)
        {
            var notesToMiss = new List<BattleNoteInfo>();
            var notesToRemove = new List<BattleNoteInfo>();
            
            foreach (var note in activeNotes)
            {
                if (!note.isProcessed)
                {
                    // 检查是否应该自动Miss
                    double missWindow = GetMissWindow();
                    if (currentTime > note.judgementTime + missWindow)
                    {
                        notesToMiss.Add(note);
                    }
                    // 更新Hold进度
                    else if (note.isHoldNote && note.holdState == HoldState.Holding)
                    {
                        // 这里需要判定系统提供按键状态，暂时跳过
                        // note.UpdateHoldProgress(currentTime, isKeyHeld);
                    }
                }
                else
                {
                    // 检查是否应该清理
                    if (ShouldCleanupNote(note, currentTime))
                    {
                        notesToRemove.Add(note);
                    }
                }
            }
            
            // 处理自动Miss
            foreach (var note in notesToMiss)
            {
                HandleNoteMissed(note);
            }
            
            // 清理已完成的音符
            foreach (var note in notesToRemove)
            {
                RemoveActiveNote(note);
            }
        }
        
        /// <summary>
        /// 处理音符Miss
        /// </summary>
        private void HandleNoteMissed(BattleNoteInfo noteInfo)
        {
            noteInfo.MarkAsMissed();
            processedNotes++;
            
            if (activeNotes.Contains(noteInfo))
            {
                activeNotes.Remove(noteInfo);
            }
            
            // 触发事件
            OnNoteAutoMiss?.Invoke(noteInfo);
            OnNoteProcessed?.Invoke(noteInfo);
            
            LogDebug($"音符自动Miss: {noteInfo} ({processedNotes}/{totalNotes})");
        }
        
        /// <summary>
        /// 移除活跃音符
        /// </summary>
        private void RemoveActiveNote(BattleNoteInfo noteInfo)
        {
            activeNotes.Remove(noteInfo);
            LogDebug($"清理音符: {noteInfo}");
        }
        
        /// <summary>
        /// 检查是否应该清理音符
        /// </summary>
        private bool ShouldCleanupNote(BattleNoteInfo note, double currentTime)
        {
            // 音符处理完成后1秒清理
            return currentTime > note.judgementTime + 1.0;
        }
        
        /// <summary>
        /// 获取最近的可判定音符
        /// </summary>
        public BattleNoteInfo GetNearestNote(double inputTime)
        {
            if (activeNotes == null || activeNotes.Count == 0) return null;
            
            BattleNoteInfo nearestNote = null;
            double minDistance = double.MaxValue;
            double earlyHitWindow = 0.5; // 允许提前击中的窗口
            
            foreach (var note in activeNotes)
            {
                if (!note.CanBeJudged()) continue;
                
                double timeDiff = inputTime - note.judgementTime;
                double distance = System.Math.Abs(timeDiff);
                
                // 只返回还有击中机会的音符
                if (timeDiff >= -earlyHitWindow && timeDiff <= 0 && distance < minDistance)
                {
                    minDistance = distance;
                    nearestNote = note;
                }
            }
            
            return nearestNote;
        }
        
        /// <summary>
        /// 标记音符为已处理
        /// </summary>
        public void MarkNoteAsProcessed(BattleNoteInfo noteInfo)
        {
            if (noteInfo != null && !noteInfo.isProcessed)
            {
                noteInfo.MarkAsHit();
                processedNotes++;
                
                if (activeNotes.Contains(noteInfo))
                {
                    activeNotes.Remove(noteInfo);
                }
                
                OnNoteProcessed?.Invoke(noteInfo);
                LogDebug($"音符已击中: {noteInfo} ({processedNotes}/{totalNotes})");
            }
        }
        
        /// <summary>
        /// 更新Hold音符进度
        /// </summary>
        public void UpdateHoldNote(BattleNoteInfo noteInfo, double currentTime, bool isHolding)
        {
            if (noteInfo != null && noteInfo.isHoldNote)
            {
                noteInfo.UpdateHoldProgress(currentTime, isHolding);
                
                // 检查Hold完成状态
                if (noteInfo.IsHoldCompleted() || noteInfo.IsHoldFailed())
                {
                    MarkNoteAsProcessed(noteInfo);
                }
            }
        }
        
        /// <summary>
        /// 获取Miss判定窗口
        /// </summary>
        private double GetMissWindow()
        {
            return 0.2; // 200ms的Miss窗口
        }
        
        /// <summary>
        /// 检查是否完成
        /// </summary>
        public bool IsCompleted()
        {
            return processedNotes >= totalNotes && upcomingNotes.Count == 0 && activeNotes.Count == 0;
        }
        
        /// <summary>
        /// 获取进度
        /// </summary>
        public float GetProgress()
        {
            if (totalNotes == 0) return 1f;
            return (float)processedNotes / totalNotes;
        }
        
        /// <summary>
        /// 重置管理器
        /// </summary>
        public void Reset()
        {
            upcomingNotes?.Clear();
            activeNotes?.Clear();
            allNotes?.Clear();
            processedNotes = 0;
            totalNotes = 0;
            isInitialized = false;
            currentChart = null;
            
            LogDebug("谱面管理器已重置");
        }
        
        /// <summary>
        /// 获取活跃音符列表（调试用）
        /// </summary>
        public List<BattleNoteInfo> GetActiveNotes()
        {
            return activeNotes?.ToList() ?? new List<BattleNoteInfo>();
        }
        
        /// <summary>
        /// 获取状态信息
        /// </summary>
        public string GetStatusInfo()
        {
            if (!isInitialized) return "未初始化";
            
            return $"进度: {GetProgress() * 100:F1}% | " +
                   $"音符: {processedNotes}/{totalNotes} | " +
                   $"活跃: {activeNotes.Count} | " +
                   $"等待: {upcomingNotes.Count}";
        }
        
        /// <summary>
        /// 调试日志
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[BattleChartManager] {message}");
            }
        }
        
        #region === 事件定义 ===
        
        /// <summary>
        /// Note生成事件
        /// </summary>
        public System.Action<BattleNoteInfo> OnNoteSpawn;
        
        /// <summary>
        /// Note处理完成事件
        /// </summary>
        public System.Action<BattleNoteInfo> OnNoteProcessed;
        
        /// <summary>
        /// Note自动Miss事件
        /// </summary>
        public System.Action<BattleNoteInfo> OnNoteAutoMiss;
        
        #endregion
    }
}
