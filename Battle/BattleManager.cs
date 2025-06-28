using UnityEngine;
using UnityEngine.InputSystem;
using System;
using QFramework;

namespace FartGame.Battle
{
    public class BattleManager : MonoBehaviour, IController
    {
        [Header("依赖引用")]
        [SerializeField] private MusicTimeManager musicTimeManager;
        [SerializeField] private BattleVisualController visualController;
        [SerializeField] private FartGame.BattleController battleController;
        
        [Header("战斗状态")]
        [SerializeField] private BattleStatus currentStatus;
        [SerializeField] private PlayerBattleData playerData;
        [SerializeField] private EnemyData enemyData;
        
        [Header("谱面系统")]
        private BattleChartManager chartManager;
        private BattleJudgement judgementSystem;
        
        private Action<BattleResult> onBattleComplete;
        private bool isInitialized = false;
        private bool isChartSystemReady = false;
        
        // === 依赖注入接口 ===
        public void Initialize(PlayerBattleData playerData, EnemyData enemyData, Action<BattleResult> onComplete)
        {
            this.playerData = playerData;
            this.enemyData = enemyData;
            this.onBattleComplete = onComplete;
            
            // 初始化状态
            currentStatus = new BattleStatus
            {
                phase = BattlePhase.Initializing,
                enemyStamina = enemyData.maxStamina,
                currentCombo = 0,
                buttTransparency = 1.0f,
                currentMusicTime = 0.0
            };
            
            // 初始化谱面系统
            if (!InitializeChartSystem())
            {
                Debug.LogError("[BattleManager] 谱面系统初始化失败");
                return;
            }
            
            // 激活战斗UI
            if (battleController != null)
            {
                battleController.SetUIActive(true);
            }
            
            isInitialized = true;
            Debug.Log($"[BattleManager] 初始化完成 - 敌人: {enemyData.enemyName}");
        }
        
        // === 生命周期管理接口 ===
        public void StartBattle()
        {
            if (!isInitialized || !isChartSystemReady)
            {
                Debug.LogError("[BattleManager] 系统未准备就绪，无法开始战斗");
                return;
            }
            
            currentStatus.phase = BattlePhase.Preparing;
            Debug.Log("[BattleManager] 战斗开始");
            
            // 启动音乐
            if (musicTimeManager != null)
            {
                musicTimeManager.StartPlaying();
            }
            
            // 切换到游戏阶段
            currentStatus.phase = BattlePhase.Playing;
        }
        
        public void PauseBattle()
        {
            if (currentStatus.phase == BattlePhase.Playing)
            {
                currentStatus.phase = BattlePhase.Paused;
                Debug.Log("[BattleManager] 战斗暂停");
                
                // TODO: 实现战斗暂停逻辑
            }
        }
        
        public void ResumeBattle()
        {
            if (currentStatus.phase == BattlePhase.Paused)
            {
                currentStatus.phase = BattlePhase.Playing;
                Debug.Log("[BattleManager] 战斗恢复");
                
                // TODO: 实现战斗恢复逻辑
            }
        }
        
        public void EndBattle()
        {
            currentStatus.phase = BattlePhase.Ending;
            Debug.Log("[BattleManager] 战斗结束");
            
            // 停止音乐
            if (musicTimeManager != null)
            {
                musicTimeManager.StopPlaying();
            }
            
            // 隐藏战斗UI
            if (battleController != null)
            {
                battleController.SetUIActive(false);
            }
            
            // 创建战斗结果
            BattleResult result = CreateBattleResult();
            
            // 根据胜利/失败发送不同的Command
            if (result.isVictory)
            {
                this.SendCommand(new FartGame.BattleVictoryCommand(result));
            }
            else
            {
                this.SendCommand(new FartGame.BattleDefeatCommand(result));
            }
            
            currentStatus.phase = BattlePhase.Completed;
            onBattleComplete?.Invoke(result);
        }
        
        // === 外部调用接口 ===
        public void OnPlayerInput(Direction direction, double inputTime)
        {
            // 保留原有接口兼容性，但新系统不使用方向输入
            if (currentStatus.phase != BattlePhase.Playing)
                return;
                
            Debug.Log($"[BattleManager] 收到方向输入: {direction} at {inputTime:F3}（新系统仅支持空格键）");
        }
        
        public BattleStatus GetCurrentStatus()
        {
            return currentStatus;
        }
        
        public float GetBattleProgress()
        {
            if (chartManager == null)
                return 0f;
                
            return chartManager.GetProgress();
        }
        
        // === 内部Update驱动 ===
        private void Update()
        {
            if (!isInitialized || currentStatus.phase == BattlePhase.Completed)
                return;
                
            // 更新当前音乐时间
            if (musicTimeManager != null && musicTimeManager.IsPlaying())
            {
                currentStatus.currentMusicTime = musicTimeManager.GetJudgementTime();
            }
            
            // 处理输入
            HandleInput();
            
            // 根据当前阶段执行相应逻辑
            switch (currentStatus.phase)
            {
                case BattlePhase.Preparing:
                    UpdatePreparingPhase();
                    break;
                case BattlePhase.Playing:
                    UpdatePlayingPhase();
                    break;
                case BattlePhase.Ending:
                    UpdateEndingPhase();
                    break;
            }
        }
        
        // === 内部阶段更新方法 ===
        private void UpdatePreparingPhase()
        {
            // 准备阶段暂时直接跳转到游戏阶段
            currentStatus.phase = BattlePhase.Playing;
        }
        
        private void UpdatePlayingPhase()
        {
            // 更新谱面系统
            if (chartManager != null)
            {
                chartManager.UpdateNoteLifecycle();
                
                // 检查是否完成
                if (chartManager.IsCompleted())
                {
                    EndBattle();
                }
            }
            
            // 更新判定系统
            if (judgementSystem != null)
            {
                judgementSystem.UpdateHoldProgress();
            }
        }
        
        private void UpdateEndingPhase()
        {
            // 结束阶段直接完成
            currentStatus.phase = BattlePhase.Completed;
        }
        
        // === Unity生命周期 ===
        private void OnDestroy()
        {
            // 清理谱面系统
            if (chartManager != null)
            {
                chartManager.Reset();
            }
            
            Debug.Log("[BattleManager] 组件销毁");
        }
        
        // === 调试信息 ===
        private void OnGUI()
        {
            if (!isInitialized || !Application.isPlaying)
                return;
                
            GUILayout.BeginArea(new Rect(10, 100, 300, 200));
            GUILayout.Label("=== BattleManager 状态 ===");
            GUILayout.Label($"阶段: {currentStatus.phase}");
            GUILayout.Label($"敌人体力: {currentStatus.enemyStamina:F1}");
            GUILayout.Label($"连击数: {currentStatus.currentCombo}");
            GUILayout.Label($"音乐时间: {currentStatus.currentMusicTime:F3}s");
            
            if (chartManager != null)
            {
                GUILayout.Label($"谱面进度: {chartManager.GetProgress() * 100:F1}%");
                GUILayout.Label(chartManager.GetStatusInfo());
            }
            
            if (judgementSystem != null)
            {
                GUILayout.Label(judgementSystem.GetStatisticsSummary());
            }
            
            GUILayout.Label("按空格键进行判定");
            GUILayout.EndArea();
        }
        
        // === 谱面系统初始化 ===
        private bool InitializeChartSystem()
        {
            // 检查依赖
            if (musicTimeManager == null)
            {
                Debug.LogError("[BattleManager] MusicTimeManager未设置");
                return false;
            }
            
            if (enemyData?.chartData == null)
            {
                Debug.LogError("[BattleManager] 敌人数据中缺少谱面数据");
                return false;
            }
            
            try
            {
                // 创建谱面管理器
                chartManager = new BattleChartManager(musicTimeManager);
                
                // 加载谱面数据
                if (!chartManager.LoadChart(enemyData.chartData))
                {
                    Debug.LogError("[BattleManager] 谱面数据加载失败");
                    return false;
                }
                
                // 创建判定系统
                judgementSystem = new BattleJudgement(chartManager, musicTimeManager);
                
                // 设置事件监听
                SetupChartEvents();
                
                isChartSystemReady = true;
                Debug.Log("[BattleManager] 谱面系统初始化成功");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BattleManager] 谱面系统初始化异常: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 设置谱面事件监听
        /// </summary>
        private void SetupChartEvents()
        {
            if (chartManager != null)
            {
                chartManager.OnNoteSpawn += OnNoteSpawn;
                chartManager.OnNoteProcessed += OnNoteProcessed;
                chartManager.OnNoteAutoMiss += OnNoteAutoMiss;
            }
            
            if (judgementSystem != null)
            {
                judgementSystem.OnJudgeResult += OnJudgeResult;
                judgementSystem.OnHoldComplete += OnHoldComplete;
                judgementSystem.OnAutoMiss += OnAutoMiss;
            }
        }
        
        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            if (currentStatus.phase != BattlePhase.Playing || judgementSystem == null) return;
            
            // 空格键按下
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                judgementSystem.ProcessSpacePress();
            }
            
            // 空格键释放
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                judgementSystem.ProcessSpaceRelease();
            }
        }
        
        /// <summary>
        /// 创建战斗结果
        /// </summary>
        private BattleResult CreateBattleResult()
        {
            var result = new BattleResult
            {
                isVictory = true, // 暂时默认胜利，后续根据逻辑调整
                remainingStamina = currentStatus.enemyStamina,
                totalHits = 0,
                totalMisses = 0,
                accuracy = 0f,
                maxCombo = currentStatus.currentCombo
            };
            
            // 填充谱面相关数据
            if (judgementSystem != null)
            {
                result.perfectCount = judgementSystem.perfectCount;
                result.goodCount = judgementSystem.goodCount;
                result.missCount = judgementSystem.missCount;
                result.totalNotes = judgementSystem.perfectCount + judgementSystem.goodCount + judgementSystem.missCount;
                result.chartAccuracy = judgementSystem.GetAccuracy();
                result.averageTimingError = 0f; // TODO: 实现平均时间偏差计算
                result.holdNotesCompleted = 0; // TODO: 实现Hold统计
                result.holdNotesTotal = 0;
                
                // 更新基本数据
                result.totalHits = result.perfectCount + result.goodCount;
                result.totalMisses = result.missCount;
                result.accuracy = result.chartAccuracy;
            }
            
            Debug.Log($"[BattleManager] 战斗结果: 胜利={result.isVictory}, 准确率={result.accuracy:P1}");
            return result;
        }
        
        // === 谱面事件处理 ===
        private void OnNoteSpawn(BattleNoteInfo noteInfo)
        {
            Debug.Log($"[BattleManager] Note生成: {noteInfo}");
            // TODO: 触发UI显示事件
        }
        
        private void OnNoteProcessed(BattleNoteInfo noteInfo)
        {
            Debug.Log($"[BattleManager] Note处理完成: {noteInfo}");
            // TODO: 触发UI更新事件
        }
        
        private void OnNoteAutoMiss(BattleNoteInfo noteInfo)
        {
            Debug.Log($"[BattleManager] Note自动Miss: {noteInfo}");
            // TODO: 触发Miss特效
        }
        
        private void OnJudgeResult(BattleJudgeResult result, BattleNoteInfo noteInfo)
        {
            Debug.Log($"[BattleManager] 判定结果: {result} - {noteInfo}");
            
            // 更新连击数
            if (result == BattleJudgeResult.Perfect || result == BattleJudgeResult.Good)
            {
                currentStatus.currentCombo++;
            }
            else
            {
                currentStatus.currentCombo = 0;
            }
            
            // TODO: 触发判定反馈事件
        }
        
        private void OnHoldComplete(BattleJudgeResult result, BattleNoteInfo noteInfo)
        {
            Debug.Log($"[BattleManager] Hold完成: {result} - {noteInfo}");
            // TODO: 处理Hold完成逻辑
        }
        
        private void OnAutoMiss(BattleNoteInfo noteInfo)
        {
            Debug.Log($"[BattleManager] 自动Miss: {noteInfo}");
            currentStatus.currentCombo = 0;
            // TODO: 触发Miss特效
        }
        
        // === 为BattleController提供的数据接口 ===
        public float GetEnemyMaxHealth()
        {
            return enemyData?.maxStamina ?? 100f;
        }
        
        public BattleJudgeResult? GetLastJudgment()
        {
            return judgementSystem?.GetLastJudgment();
        }
        
        public float GetLastJudgmentTime()
        {
            return judgementSystem?.GetLastJudgmentTime() ?? 0f;
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGame.FartGameArchitecture.Interface;
        }
    }
}
