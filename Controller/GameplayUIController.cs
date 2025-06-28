using QFramework;
using UnityEngine;
using UnityEngine.UI;
using FartGame.Battle;

namespace FartGame
{
    public class GameplayUIController : MonoBehaviour, IController
    {
        [Header("UI References")]
        public Text fartValueText;
        public Text modeText;
        public Slider fartValueSlider;
        public Image bodySizeIndicator;
        public Text speedText;
        public GameObject gameplayPanel;
        
        [Header("资源点回复 UI")]
        public GameObject healingIndicator;
        public Text healingTimeText;
        public Text healingRateText;
        
        [Header("测试功能")]
        public Button testBattleButton;
        
        [Header("UI Settings")]
        public float maxSliderValue = 200f; // 滑动条显示的最大值
        
        private PlayerModel mPlayerModel;
        private GameModel mGameModel;
        private FartSystem mFartSystem;
        
        void Start()
        {
            mPlayerModel = this.GetModel<PlayerModel>();
            mGameModel = this.GetModel<GameModel>();
            mFartSystem = this.GetSystem<FartSystem>();
            
            // 绑定屁值显示
            if (fartValueText != null)
            {
                mPlayerModel.FartValue.RegisterWithInitValue(value =>
                {
                    fartValueText.text = $"屁值: {value:F1}";
                }).UnRegisterWhenGameObjectDestroyed(gameObject);
            }
            
            // 绑定屁值滑动条
            if (fartValueSlider != null)
            {
                mPlayerModel.FartValue.RegisterWithInitValue(value =>
                {
                    fartValueSlider.value = value / maxSliderValue;
                }).UnRegisterWhenGameObjectDestroyed(gameObject);
            }
            
            // 绑定模式显示
            if (modeText != null)
            {
                mPlayerModel.IsFumeMode.RegisterWithInitValue(isFume =>
                {
                    modeText.text = isFume ? "熏模式" : "普通模式";
                    modeText.color = isFume ? Color.red : Color.white;
                }).UnRegisterWhenGameObjectDestroyed(gameObject);
            }
            
            // 绑定体型指示器
            if (bodySizeIndicator != null)
            {
                mPlayerModel.BodySize.RegisterWithInitValue(size =>
                {
                    // 将体型映射到UI指示器大小
                    float uiScale = 0.5f + (size - 1f) * 0.3f;
                    bodySizeIndicator.transform.localScale = Vector3.one * uiScale;
                }).UnRegisterWhenGameObjectDestroyed(gameObject);
            }
            
            // 绑定速度显示
            if (speedText != null)
            {
                mPlayerModel.MoveSpeed.RegisterWithInitValue(speed =>
                {
                    speedText.text = $"速度: {speed:F1}";
                }).UnRegisterWhenGameObjectDestroyed(gameObject);
            }
            
            // 监听游戏状态，控制UI显示
            mGameModel.CurrentGameState.RegisterWithInitValue(state =>
            {
                if (gameplayPanel != null)
                {
                    gameplayPanel.SetActive(state == GameState.Gameplay || state == GameState.Paused);
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
            
            // 监听资源点事件
            this.RegisterEvent<ResourcePointActivatedEvent>(OnResourcePointActivated).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<ResourcePointDeactivatedEvent>(OnResourcePointDeactivated).UnRegisterWhenGameObjectDestroyed(gameObject);
            
            // 绑定测试战斗按钮事件
            if (testBattleButton != null)
            {
                testBattleButton.onClick.AddListener(OnTestBattleClicked);
            }
        }
        
        void Update()
        {
            // 更新资源点回复状态显示
            UpdateHealingIndicator();
        }
        
        private void UpdateHealingIndicator()
        {
            if (mFartSystem == null || healingIndicator == null) return;
            
            var (isActive, rate, timeRemaining) = mFartSystem.GetHealingStatus();
            
            if (isActive)
            {
                // 显示回复效果
                healingIndicator.SetActive(true);
                
                // 更新回复速率显示
                if (healingRateText != null)
                {
                    healingRateText.text = $"每秒回复: {rate:F1}";
                }
                
                // 更新剩余时间显示
                if (healingTimeText != null)
                {
                    healingTimeText.text = $"剩余时间: {timeRemaining:F1}s";
                }
            }
            else
            {
                // 隐藏回复效果
                healingIndicator.SetActive(false);
            }
        }
        
        // === 资源点事件处理 ===
        private void OnResourcePointActivated(ResourcePointActivatedEvent e)
        {
            Debug.Log($"[GameplayUI] 资源点激活 - 每秒回复{e.healingRate}，持续{e.duration}秒");
        }
        
        private void OnResourcePointDeactivated(ResourcePointDeactivatedEvent e)
        {
            Debug.Log("[GameplayUI] 资源点回复效果结束");
        }
        
        // === 测试战斗功能 ===
        private void OnTestBattleClicked()
        {
            if (!IsBattleSystemReady())
            {
                Debug.LogWarning("[测试] 战斗系统尚未准备就绪");
                return;
            }
            
            Debug.Log("[测试] 开始创建虚拟敵人数据");
            
            // 创建虚拟敵人配置
            var testEnemyConfig = CreateTestEnemyConfig();
            
            if (!ValidateTestConfig(testEnemyConfig))
            {
                Debug.LogError("[测试] 虚拟敵人配置验证失败");
                return;
            }
            
            // 创建虚拟敵人控制器
            var virtualEnemy = CreateVirtualEnemyController(testEnemyConfig);
            
            Debug.Log("[测试] 发送战斗开始命令");
            
            // 发送战斗开始命令
            this.SendCommand(new StartEnemyBattleCommand(virtualEnemy, testEnemyConfig));
        }
        
        private EnemyConfigSO CreateTestEnemyConfig()
        {
            var config = ScriptableObject.CreateInstance<EnemyConfigSO>();
            config.enemyType = EnemyType.Normal;
            config.displayName = "测试敵人";
            config.description = "用于测试战斗流程的虚拟敵人";
            config.attackPower = 10f;
            config.valueReward = 20;
            config.resourceReward = 3;
            
            // 创建测试谱面
            config.battleChart = CreateTestBattleChart();
            
            Debug.Log("[测试] 创建虚拟敵人配置完成");
            return config;
        }
        
        private BattleChartData CreateTestBattleChart()
        {
            var chart = ScriptableObject.CreateInstance<BattleChartData>();
            chart.chartName = "测试谱面";
            chart.description = "简单的测试谱面";
            chart.bpm = 120;
            chart.measures = 4;
            chart.beatsPerMeasure = 8;
            chart.fixedDropTime = 2.0f;
            chart.difficulty = 1;
            
            // 使用现有的测试谱面生成方法
            chart.GenerateTestChart();
            
            return chart;
        }
        
        private EnemyController CreateVirtualEnemyController(EnemyConfigSO config)
        {
            // 创建临时GameObject
            var enemyObject = new GameObject("VirtualTestEnemy");
            var enemyController = enemyObject.AddComponent<EnemyController>();
            
            // 设置配置
            enemyController.enemyConfig = config;
            
            // 设置位置（可选）
            enemyObject.transform.position = Vector3.zero;
            
            return enemyController;
        }
        
        private bool ValidateTestConfig(EnemyConfigSO config)
        {
            if (config == null)
            {
                Debug.LogError("[测试] 敵人配置为空");
                return false;
            }
            
            if (config.battleChart == null)
            {
                Debug.LogError("[测试] 战斗谱面为空");
                return false;
            }
            
            var (isValid, errors) = config.battleChart.ValidateChart();
            if (!isValid)
            {
                Debug.LogError($"[测试] 测试谱面验证失败: {string.Join(", ", errors)}");
                return false;
            }
            
            Debug.Log($"[测试] 谱面验证结果: {isValid}");
            return true;
        }
        
        private bool IsBattleSystemReady()
        {
            var gameModel = this.GetModel<GameModel>();
            return gameModel.CurrentGameState.Value == GameState.Gameplay && 
                   !gameModel.IsPaused.Value;
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
    }
}
