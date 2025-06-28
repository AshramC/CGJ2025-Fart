using QFramework;
using UnityEngine;
using FartGame.Battle;

namespace FartGame
{
    // 游戏管理器，负责驱动系统更新
    public class GameManager : MonoBehaviour, IController
    {
        [Header("Core Systems")]
        private FartSystem mFartSystem;
        
        [Header("Battle System")]
        [SerializeField] private GameObject battleSystemPrefab;
        
        private BattleManager currentBattleManager;
        private MusicTimeManager musicTimeManager;
        private EnemyController currentBattleEnemy;
        
        void Start()
        {
            // 初始化架构
            FartGameArchitecture.InitArchitecture();
            
            // 获取系统引用
            mFartSystem = this.GetSystem<FartSystem>();
            
            // 监听战斗开始事件
            this.RegisterEvent<EnemyBattleStartEvent>(OnEnemyBattleStart).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        
        void Update()
        {
            // 驱动FartSystem更新（非战斗状态时）
            if (mFartSystem != null && currentBattleManager == null)
            {
                mFartSystem.Update();
            }
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
        
        // === 战斗事件处理 ===
        private void OnEnemyBattleStart(EnemyBattleStartEvent e)
        {
            // 从事件中获取敌人信息，启动战斗
            StartBattle(e.enemyController, e.enemyConfig);
        }
        
        // === 战斗系统启动接口 ===
        public void StartBattle(EnemyController enemyController, EnemyConfigSO enemyConfig)
        {
            if (currentBattleManager != null)
            {
                Debug.LogWarning("[战斗系统] 已在战斗中，无法启动新战斗");
                return;
            }
            
            // 验证参数
            if (enemyController == null || enemyConfig == null)
            {
                Debug.LogError("[游戏管理器] 敌人控制器或配置数据为空，无法启动战斗");
                return;
            }
            
            if (enemyConfig.battleChart == null)
            {
                Debug.LogError($"[游戏管理器] 敌人 {enemyConfig.displayName} 缺少战斗谱面数据，无法启动战斗");
                return;
            }
            
            Debug.Log($"[游戏管理器] 启动战斗 - 敌人: {enemyConfig.displayName}, 谱面: {enemyConfig.battleChart.chartName}");
            
            // 存储当前战斗敌人引用
            currentBattleEnemy = enemyController;
            
            // 1. 暂停主游戏系统
            PauseMainGameSystems();
            
            // 2. 实例化战斗系统
            InstantiateBattleSystem(enemyConfig);
        }
        
        // === 战斗完成回调 ===
        private void OnBattleComplete(BattleResult result)
        {
            Debug.Log($"[游戏管理器] 战斗结束 - 胜利: {result.isVictory}");
            
            // 处理战斗结果
            ApplyBattleResults(result);
            
            // 销毁战斗系统
            DestroyBattleSystem();
            
            // 恢复主游戏
            ResumeMainGameSystems();
        }
        
        // === 系统管理方法 ===
        private void PauseMainGameSystems()
        {
            Debug.Log("[游戏管理器] 暂停主游戏系统");
            // TODO: 暂停相关系统更新
        }
        
        private void ResumeMainGameSystems()
        {
            Debug.Log("[游戏管理器] 恢复主游戏系统");
            // TODO: 恢复相关系统更新
        }
        
        private void InstantiateBattleSystem(EnemyConfigSO enemyConfig)
        {
            if (battleSystemPrefab == null)
            {
                Debug.LogError("[游戏管理器] battleSystemPrefab未设置");
                return;
            }
            
            // 直接从 PlayerModel 获取数据
            var playerModel = this.GetModel<PlayerModel>();
            var playerData = new PlayerBattleData
            {
                fartValue = playerModel.FartValue.Value,
                position = playerModel.Position.Value,
                isInFumeMode = playerModel.IsFumeMode.Value
            };
            
            // 从 EnemyConfigSO 创建 EnemyData
            var enemyData = new EnemyData
            {
                enemyName = enemyConfig.displayName,
                maxStamina = 100f, // 暂时固定值
                chartData = enemyConfig.battleChart
            };
            
            var battleObject = Instantiate(battleSystemPrefab);
            currentBattleManager = battleObject.GetComponent<BattleManager>();
            
            if (currentBattleManager == null)
            {
                Debug.LogError("[游戏管理器] BattleManager组件未找到");
                Destroy(battleObject);
                return;
            }
            
            // 获取MusicTimeManager
            musicTimeManager = battleObject.GetComponent<MusicTimeManager>();
            if (musicTimeManager != null)
            {
                musicTimeManager.Initialize();
            }
            
            // 初始化战斗系统
            currentBattleManager.Initialize(playerData, enemyData, OnBattleComplete);
            currentBattleManager.StartBattle();
            
            Debug.Log("[游戏管理器] 战斗系统实例化完成");
        }
        
        private void DestroyBattleSystem()
        {
            if (currentBattleManager != null)
            {
                Destroy(currentBattleManager.gameObject);
                currentBattleManager = null;
                musicTimeManager = null;
                currentBattleEnemy = null; // 清空敌人引用
                Debug.Log("[游戏管理器] 战斗系统销毁完成");
            }
        }
        
        private void ApplyBattleResults(BattleResult result)
        {
            // 输出详细的战斗结果
            Debug.Log($"[游戏管理器] 战斗结果详情:");
            Debug.Log($"  胜利: {result.isVictory}");
            Debug.Log($"  准确率: {result.accuracy:P1}");
            Debug.Log($"  总音符: {result.totalNotes}");
            Debug.Log($"  Perfect: {result.perfectCount}, Good: {result.goodCount}, Miss: {result.missCount}");
            Debug.Log($"  最大连击: {result.maxCombo}");
            
            // TODO: 将战斗结果应用到游戏状态
            // 例如：更新玩家经验、解锁新内容等
        }
        
        // === 公共接口 ===
        // 获取当前战斗敌人的公共接口
        public EnemyController GetCurrentBattleEnemy()
        {
            return currentBattleEnemy;
        }
    }
}
