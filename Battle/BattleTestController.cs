using UnityEngine;
using FartGame.Battle;

namespace FartGame.Battle
{
    /// <summary>
    /// 战斗系统测试控制器
    /// 提供简易的可视化测试环境
    /// </summary>
    public class BattleTestController : MonoBehaviour
    {
        [Header("依赖组件")]
        [SerializeField] private MusicTimeManager musicTimeManager;
        [SerializeField] private BattleTestVisualizer visualizer;
        
        [Header("测试配置")]
        [SerializeField] private BattleChartData[] testCharts;
        [SerializeField] private int currentChartIndex = 0;
        
        [Header("战斗系统")]
        private BattleManager battleManager;
        private BattleChartManager chartManager;
        private BattleJudgement judgementSystem;
        
        [Header("测试状态")]
        [SerializeField] private bool isInitialized = false;
        [SerializeField] private bool isTesting = false;
        
        private void Start()
        {
            InitializeTestSystem();
        }
        
        private void Update()
        {
            if (isTesting)
            {
                HandleTestInput();
                UpdateTestSystem();
            }
        }
        
        /// <summary>
        /// 初始化测试系统
        /// </summary>
        private void InitializeTestSystem()
        {
            // 获取或创建依赖组件
            if (musicTimeManager == null)
            {
                musicTimeManager = GetComponent<MusicTimeManager>();
                if (musicTimeManager == null)
                {
                    musicTimeManager = gameObject.AddComponent<MusicTimeManager>();
                }
            }
            
            if (visualizer == null)
            {
                visualizer = GetComponent<BattleTestVisualizer>();
                if (visualizer == null)
                {
                    visualizer = gameObject.AddComponent<BattleTestVisualizer>();
                }
            }
            
            // 初始化音乐时间管理器
            musicTimeManager.Initialize();
            
            // 初始化视觉化组件
            visualizer.Initialize();
            
            isInitialized = true;
            Debug.Log("[BattleTestController] 测试系统初始化完成");
        }
        
        /// <summary>
        /// 开始测试
        /// </summary>
        public void StartTest()
        {
            if (!isInitialized)
            {
                Debug.LogError("[BattleTestController] 系统未初始化");
                return;
            }
            
            if (testCharts == null || testCharts.Length == 0)
            {
                Debug.LogError("[BattleTestController] 没有测试谱面数据");
                return;
            }
            
            var currentChart = testCharts[currentChartIndex];
            if (currentChart == null)
            {
                Debug.LogError("[BattleTestController] 当前谱面数据为空");
                return;
            }
            
            // 创建战斗系统组件
            InitializeBattleSystem(currentChart);
            
            // 开始播放
            musicTimeManager.StartPlaying();
            
            isTesting = true;
            Debug.Log($"[BattleTestController] 开始测试谱面: {currentChart.chartName}");
        }
        
        /// <summary>
        /// 停止测试
        /// </summary>
        public void StopTest()
        {
            if (isTesting)
            {
                musicTimeManager.StopPlaying();
                
                // 清理战斗系统
                CleanupBattleSystem();
                
                // 清理视觉元素
                visualizer.ClearAllNotes();
                
                isTesting = false;
                Debug.Log("[BattleTestController] 测试已停止");
            }
        }
        
        /// <summary>
        /// 重置测试
        /// </summary>
        public void ResetTest()
        {
            StopTest();
            musicTimeManager.Reset();
            Debug.Log("[BattleTestController] 测试已重置");
        }
        
        /// <summary>
        /// 切换谱面
        /// </summary>
        public void SwitchChart(int index)
        {
            if (testCharts == null || index < 0 || index >= testCharts.Length) return;
            
            bool wasPlaying = isTesting;
            if (wasPlaying) StopTest();
            
            currentChartIndex = index;
            Debug.Log($"[BattleTestController] 切换到谱面: {testCharts[index]?.chartName ?? "空"}");
            
            if (wasPlaying) StartTest();
        }
        
        /// <summary>
        /// 初始化战斗系统
        /// </summary>
        private void InitializeBattleSystem(BattleChartData chartData)
        {
            try
            {
                // 创建谱面管理器
                chartManager = new BattleChartManager(musicTimeManager);
                
                // 加载谱面数据
                if (!chartManager.LoadChart(chartData))
                {
                    Debug.LogError("[BattleTestController] 谱面加载失败");
                    return;
                }
                
                // 创建判定系统
                judgementSystem = new BattleJudgement(chartManager, musicTimeManager);
                
                // 设置事件监听
                SetupBattleEvents();
                
                Debug.Log("[BattleTestController] 战斗系统初始化成功");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BattleTestController] 战斗系统初始化失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 设置战斗事件监听
        /// </summary>
        private void SetupBattleEvents()
        {
            if (chartManager != null)
            {
                chartManager.OnNoteSpawn += visualizer.SpawnNoteVisual;
                chartManager.OnNoteProcessed += visualizer.OnNoteProcessed;
                chartManager.OnNoteAutoMiss += visualizer.OnNoteMissed;
            }
            
            if (judgementSystem != null)
            {
                judgementSystem.OnJudgeResult += OnJudgeResult;
                judgementSystem.OnHoldComplete += OnHoldComplete;
                judgementSystem.OnAutoMiss += OnAutoMiss;
            }
        }
        
        /// <summary>
        /// 清理战斗系统
        /// </summary>
        private void CleanupBattleSystem()
        {
            chartManager?.Reset();
            judgementSystem?.ResetStatistics();
            chartManager = null;
            judgementSystem = null;
        }
        
        /// <summary>
        /// 处理测试输入
        /// </summary>
        private void HandleTestInput()
        {
            // 空格键 - 判定输入
            if (Input.GetKeyDown(KeyCode.Space) && judgementSystem != null)
            {
                judgementSystem.ProcessSpacePress();
            }
            
            if (Input.GetKeyUp(KeyCode.Space) && judgementSystem != null)
            {
                judgementSystem.ProcessSpaceRelease();
            }
            
            // 数字键切换谱面
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    SwitchChart(i - 1);
                }
            }
            
            // R键重置
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetTest();
            }
            
            // P键暂停/继续
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (isTesting && musicTimeManager.IsPlaying())
                {
                    musicTimeManager.StopPlaying();
                }
                else if (isTesting)
                {
                    musicTimeManager.StartPlaying();
                }
            }
        }
        
        /// <summary>
        /// 更新测试系统
        /// </summary>
        private void UpdateTestSystem()
        {
            if (chartManager != null)
            {
                chartManager.UpdateNoteLifecycle();
                
                // 检查是否完成
                if (chartManager.IsCompleted())
                {
                    Debug.Log("[BattleTestController] 谱面测试完成");
                    StopTest();
                }
            }
            
            if (judgementSystem != null)
            {
                judgementSystem.UpdateHoldProgress();
            }
            
            // 更新视觉化组件
            visualizer.UpdateVisuals(musicTimeManager.GetJudgementTime());
        }
        
        // === 战斗事件处理 ===
        private void OnJudgeResult(BattleJudgeResult result, BattleNoteInfo noteInfo)
        {
            visualizer.ShowJudgeResult(result, noteInfo);
        }
        
        private void OnHoldComplete(BattleJudgeResult result, BattleNoteInfo noteInfo)
        {
            visualizer.ShowHoldComplete(result, noteInfo);
        }
        
        private void OnAutoMiss(BattleNoteInfo noteInfo)
        {
            visualizer.OnNoteMissed(noteInfo);
        }
        
        // === OnGUI调试界面 ===
        private void OnGUI()
        {
            if (!isInitialized) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 400, 300));
            GUILayout.Label("=== 战斗系统测试器 ===");
            
            // 基础控制
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(isTesting ? "停止测试" : "开始测试", GUILayout.Width(100)))
            {
                if (isTesting) StopTest(); else StartTest();
            }
            if (GUILayout.Button("重置", GUILayout.Width(60)))
            {
                ResetTest();
            }
            GUILayout.EndHorizontal();
            
            // 谱面选择
            GUILayout.Label($"当前谱面: {currentChartIndex + 1}/{testCharts?.Length ?? 0}");
            if (testCharts != null && testCharts.Length > 0)
            {
                var currentChart = testCharts[currentChartIndex];
                GUILayout.Label($"名称: {currentChart?.chartName ?? "空"}");
            }
            
            // 系统状态
            GUILayout.Label($"音乐时间: {musicTimeManager.GetJudgementTime():F2}s");
            GUILayout.Label($"播放状态: {(musicTimeManager.IsPlaying() ? "播放中" : "暂停")}");
            
            // 谱面信息
            if (chartManager != null)
            {
                GUILayout.Label($"谱面进度: {chartManager.GetProgress() * 100:F1}%");
                GUILayout.Label(chartManager.GetStatusInfo());
            }
            
            // 判定统计
            if (judgementSystem != null)
            {
                GUILayout.Label(judgementSystem.GetStatisticsSummary());
            }
            
            // 操作说明
            GUILayout.Space(10);
            GUILayout.Label("操作说明:");
            GUILayout.Label("空格键 - 判定");
            GUILayout.Label("1-9 - 切换谱面");
            GUILayout.Label("R - 重置");
            GUILayout.Label("P - 暂停/继续");
            
            GUILayout.EndArea();
        }
    }
}
