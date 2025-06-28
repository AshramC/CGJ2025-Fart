using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using QFramework;

namespace FartGame.Battle
{
    public class BattleManager : MonoBehaviour, IController, ICanSendEvent
    {
        public static BattleManager Instance { get; private set; }
        
        [Header("依赖引用")]
        // MusicTimeManager 改为通过单例访问
        [SerializeField] private BattleUI battleUI;
        
        [Header("战斗状态")]
        [SerializeField] private BattleStatus currentStatus;
        [SerializeField] private PlayerBattleData playerData;
        [SerializeField] private EnemyData enemyData;
        
        [Header("战斗本地数据")]
        [SerializeField] private BattleLocalData battleData;
        
        [Header("谱面系统")]
        private BattleChartManager chartManager;
        private BattleJudgement judgementSystem;
        
        private Action<BattleResult> onBattleComplete;
        private bool isInitialized = false;
        private bool isChartSystemReady = false;
        private bool isInBattle = false;
        
        #region 单例管理
        
        void Awake()
        {
            // 单例模式实现
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSingleton();
            }
            else if (Instance != this)
            {
                Debug.LogWarning("[BattleManager] 检测到重复实例，销毁多余对象");
                Destroy(gameObject);
            }
        }
        
        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        private void InitializeSingleton()
        {
            Debug.Log("[BattleManager] 单例初始化");
            
            // 初始化默认状态
            currentStatus = new BattleStatus
            {
                phase = BattlePhase.Initializing,
                enemyStamina = 0f,
                currentCombo = 0,
                buttTransparency = 1.0f,
                currentMusicTime = 0.0
            };
            
            isInitialized = true;
        }
        
        #endregion
        
        // === 依赖注入接口 ===
        public void Initialize(PlayerBattleData playerData, EnemyData enemyData, Action<BattleResult> onComplete)
        {
            // 运行时查找BattleUI组
            FindAndValidateBattleUI();
            
            this.playerData = playerData;
            this.enemyData = enemyData;
            this.onBattleComplete = onComplete;
            
            // 初始化本地数据
            battleData = new BattleLocalData();
            battleData.InitializeFromPlayer(playerData);
            
            // 初始化状态
            currentStatus = new BattleStatus
            {
                phase = BattlePhase.Initializing,
                enemyStamina = enemyData.maxStamina,
                currentCombo = 0,
                buttTransparency = 1.0f,
                currentMusicTime = 0.0
            };
            
            // 初始化UI显示
            if (battleUI != null)
            {
                battleUI.UpdateFartValueDirect(battleData.currentFartValue, GetMaxFartValue());
            }
            
            // 初始化谱面系统
            if (!InitializeChartSystem())
            {
                Debug.LogError("[BattleManager] 谱面系统初始化失败");
                return;
            }
            
            // 激活战斗UI
            if (battleUI != null)
            {
                battleUI.ResetUI(); // 重置UI状态
                battleUI.SetUIActive(true); // 激活UI
                Debug.Log("[BattleManager] UI已激活并重置");
            }
            else
            {
                Debug.LogWarning("[BattleManager] BattleUI不可用，跳过UI激活");
            }
            
            isInitialized = true;
            Debug.Log($"[BattleManager] 初始化完成 - 敌人: {enemyData.enemyName}");
        }
        
        // === 公共接口 ===
        
        /// <summary>
        /// 检查是否处于战斗状态
        /// </summary>
        public bool IsInBattle() => isInBattle;
        
        // === 生命周期管理接口 ===
        public void StartBattle()
        {
            if (!isInitialized || !isChartSystemReady)
            {
                Debug.LogError("[BattleManager] 系统未准备就绪，无法开始战斗");
                return;
            }
            
            if (isInBattle)
            {
                Debug.LogWarning("[BattleManager] 已在战斗中，无法启动新战斗");
                return;
            }
            
            currentStatus.phase = BattlePhase.Preparing;
            Debug.Log("[BattleManager] 战斗开始");
            
            // 启动音乐
            if (MusicTimeManager.Instance != null)
            {
                MusicTimeManager.Instance.StartPlaying();
            }
            
            // 切换到游戏阶段
            currentStatus.phase = BattlePhase.Playing;
            isInBattle = true;
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
            if (!isInBattle)
            {
                Debug.LogWarning("[BattleManager] 已经不在战斗中，无法结束战斗");
                return;
            }
            
            currentStatus.phase = BattlePhase.Ending;
            Debug.Log("[BattleManager] 战斗结束");
            
            // 停止音乐
            if (MusicTimeManager.Instance != null)
            {
                MusicTimeManager.Instance.StopPlaying();
            }
            
            // 隐藏战斗UI
            if (battleUI != null)
            {
                battleUI.SetUIActive(false);
                Debug.Log("[BattleManager] UI已隐藏");
            }
            else
            {
                Debug.LogWarning("[BattleManager] BattleUI不可用，跳过UI隐藏");
            }
            
            // 创建战斗结果
            BattleResult result = CreateBattleResult();
            
            // 清理战斗状态
            CleanupBattleState();
            
            // 发送战斗完成事件，由GameStateSystem统一处理
            var currentEnemyController = GetCurrentBattleEnemy();
            if (currentEnemyController == null)
            {
                Debug.LogWarning("[BattleManager] 未能获取当前战斗敌人，但仍将发送事件");
            }
            
            this.SendEvent(new FartGame.BattleCompletedEvent
            {
                Result = result,
                EnemyController = currentEnemyController,
                IsVictory = result.isVictory
            });
            
            currentStatus.phase = BattlePhase.Completed;
            isInBattle = false;
            onBattleComplete?.Invoke(result);
        }
        
        /// <summary>
        /// 清理战斗状态
        /// </summary>
        private void CleanupBattleState()
        {
            // 清理谱面系统
            if (chartManager != null)
            {
                chartManager.Reset();
            }
            
            // 重置数据
            battleData = null;
            playerData = null;
            enemyData = null;
            onBattleComplete = null;
            
            isChartSystemReady = false;
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
            if (MusicTimeManager.Instance != null && MusicTimeManager.Instance.IsPlaying())
            {
                currentStatus.currentMusicTime = MusicTimeManager.Instance.GetJudgementTime();
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
        // OnDestroy已在单例管理区域定义
        
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
            if (MusicTimeManager.Instance == null)
            {
                Debug.LogError("[BattleManager] MusicTimeManager.Instance未初始化");
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
                chartManager = new BattleChartManager(MusicTimeManager.Instance);
                
                // 加载谱面数据
                if (!chartManager.LoadChart(enemyData.chartData))
                {
                    Debug.LogError("[BattleManager] 谱面数据加载失败");
                    return false;
                }
                
                // 创建判定系统
                judgementSystem = new BattleJudgement(chartManager, MusicTimeManager.Instance);
                
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
                
                // 监听玩家受伤事件
                judgementSystem.OnPlayerDamaged += OnPlayerDamaged;
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
        /// 创建战斗结果（极简版）
        /// </summary>
        private BattleResult CreateBattleResult()
        {
            var result = new BattleResult
            {
                isVictory = !battleData.IsDefeated(), // 屁值大于0即为胜利
                potentialDamage = battleData.totalDamageReceived  // 战斗中累积的潜在伤害
            };
            
            Debug.Log($"[BattleManager] 战斗结果: 胜利={result.isVictory}, 潜在伤害={result.potentialDamage}");
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
            
            // 更新最后判定
            UpdateJudgement(result);
            
            // 只有Miss时才处理伤害
            if (result == BattleJudgeResult.Miss)
            {
                float damage = GetCurrentEnemyAttackPower();
                DamageFartValue(damage);
            }
            
            Debug.Log($"[BattleManager] 判定处理完成: {result}, 当前屁值: {battleData?.currentFartValue ?? 0f}");
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
        
        // === 新增：玩家受伤处理 ===
        private void OnPlayerDamaged(float damage)
        {
            Debug.Log($"[BattleManager] 玩家受到 {damage} 点伤害");
            
            // 直接扣除战斗本地屁值（高性能路径）
            DamageFartValue(damage);
        }
        
        // === 战斗本地数据操作方法（高性能，无延迟） ===
        
        /// <summary>
        /// 扣除屁值（战斗本地数据）
        /// </summary>
        public void DamageFartValue(float damage)
        {
            if (battleData == null) return;
            
            float oldValue = battleData.currentFartValue;
            battleData.currentFartValue = Mathf.Max(0f, oldValue - damage);
            battleData.totalDamageReceived += damage;
            
            // 立即更新UI（零延迟）
            if (battleUI != null)
            {
                battleUI.UpdateFartValueDirect(battleData.currentFartValue, GetMaxFartValue());
            }
            else
            {
                Debug.LogWarning("[BattleManager] BattleUI不可用，跳过屁值显示更新");
            }
            
            // 检查失败条件
            if (battleData.IsDefeated())
            {
                EndBattleWithDefeat();
            }
            
            Debug.Log($"[BattleManager] 玩家受伤 {damage}，屁值: {oldValue} → {battleData.currentFartValue}");
        }
        
        /// <summary>
        /// 更新判定结果
        /// </summary>
        public void UpdateJudgement(BattleJudgeResult result)
        {
            if (battleData == null) return;
            
            battleData.lastJudgement = result;
            
            // 立即更新UI反馈（零延迟）
            if (battleUI != null)
            {
                battleUI.ShowJudgementFeedback(result);
            }
            else
            {
                Debug.LogWarning("[BattleManager] BattleUI不可用，跳过判定反馈显示");
            }
        }
        
        /// <summary>
        /// 战斗失败处理
        /// </summary>
        private void EndBattleWithDefeat()
        {
            Debug.Log("[BattleManager] 战斗失败 - 屁值耗尽");
            
            // 立即结束战斗
            EndBattle();
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
        
        // === 公共数据访问接口 ===
        
        public float GetCurrentFartValue() => battleData?.currentFartValue ?? 0f;
        public float GetInitialFartValue() => battleData?.initialFartValue ?? 0f;
        public BattleJudgeResult GetLastJudgement() => battleData?.lastJudgement ?? BattleJudgeResult.None;
        public float GetFartValueRatio() => battleData?.GetFartValueRatio(GetMaxFartValue()) ?? 0f;
        public float GetTotalDamageReceived() => battleData?.totalDamageReceived ?? 0f;
        
        private float GetMaxFartValue()
        {
            return this.GetModel<GameConfigModel>().MaxFartValue;
        }
        
        private float GetCurrentEnemyAttackPower()
        {
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
            
            return 10f; // 默认伤害值
        }
        
        // === 获取当前战斗敌人 ===
        private EnemyController GetCurrentBattleEnemy()
        {
            // 从 GameManager 获取当前战斗的敌人引用
            var gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                return gameManager.GetCurrentBattleEnemy();
            }
            
            Debug.LogError("[BattleManager] 未找到 GameManager");
            return null;
        }
        
        // === 运行时查找BattleUI ===
        private bool FindAndValidateBattleUI()
        {
            if (battleUI == null)
            {
                battleUI = FindObjectOfType<BattleUI>();
                
                if (battleUI != null)
                {
                    Debug.Log("[BattleManager] 成功找到BattleUI组件");
                    return true;
                }
                else
                {
                    Debug.LogError("[BattleManager] 场景中未找到BattleUI组件，战斗UI功能将被禁用");
                    return false;
                }
            }
            
            return true; // battleUI已经存在
        }
    }
}
