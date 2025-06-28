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
        
        [Header("Battle System - 通过单例管理")]
        // battleSystemPrefab 已移除，使用单例模式
        private EnemyController currentBattleEnemy;
        
        void Start()
        {
            // 初始化架构
            FartGameArchitecture.InitArchitecture();
            
            // 初始化Manager系统
            InitializeBattleSystem();
            InitializeMusicTimeManager();
            
            // 获取系统引用
            mFartSystem = this.GetSystem<FartSystem>();
            
            // 监听战斗开始事件
            this.RegisterEvent<EnemyBattleStartEvent>(OnEnemyBattleStart).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        
        void Update()
        {
            // 驱动FartSystem更新（非战斗状态时）
            if (mFartSystem != null && (BattleManager.Instance == null || !BattleManager.Instance.IsInBattle()))
            {
                mFartSystem.Update();
            }
        }
        
        // === Manager初始化方法 ===
        
        /// <summary>
        /// 初始化战斗系统
        /// </summary>
        private void InitializeBattleSystem()
        {
            if (BattleManager.Instance != null)
            {
                Debug.Log("[GameManager] BattleManager已存在，跳过初始化");
                return;
            }
            
            // 创建 BattleManager GameObject
            var battleManagerObject = new GameObject("BattleManager");
            battleManagerObject.AddComponent<BattleManager>();
            
            Debug.Log("[GameManager] BattleManager单例初始化完成");
        }
        
        /// <summary>
        /// 初始化音乐时间管理器
        /// </summary>
        private void InitializeMusicTimeManager()
        {
            if (MusicTimeManager.Instance != null)
            {
                Debug.Log("[GameManager] MusicTimeManager已存在，跳过初始化");
                return;
            }
            
            // 创建 MusicTimeManager GameObject
            var musicManagerObject = new GameObject("MusicTimeManager");
            musicManagerObject.AddComponent<MusicTimeManager>();
            
            Debug.Log("[GameManager] MusicTimeManager单例初始化完成");
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
            if (BattleManager.Instance != null && BattleManager.Instance.IsInBattle())
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
            
            // 创建战斗数据
            var playerData = CreatePlayerBattleData();
            var enemyData = CreateEnemyData(enemyConfig);
            
            // 直接通过单例启动战斗
            BattleManager.Instance.Initialize(playerData, enemyData, OnBattleComplete);
            BattleManager.Instance.StartBattle();
        }
        
        // === 战斗完成回调 ===
        private void OnBattleComplete(BattleResult result)
        {
            Debug.Log($"[游戏管理器] 战斗结束 - 胜利: {result.isVictory}");
            
            // 处理战斗结果
            ApplyBattleResults(result);
            
            // 清理当前战斗敌人引用
            currentBattleEnemy = null;
        }
        
        // === 战斗数据创建方法 ===
        
        private PlayerBattleData CreatePlayerBattleData()
        {
            var playerModel = this.GetModel<PlayerModel>();
            return new PlayerBattleData
            {
                fartValue = playerModel.FartValue.Value,
                position = playerModel.Position.Value,
                isInFumeMode = playerModel.IsFumeMode.Value
            };
        }
        
        private EnemyData CreateEnemyData(EnemyConfigSO enemyConfig)
        {
            return new EnemyData
            {
                enemyName = enemyConfig.displayName,
                maxStamina = 100f, // 暂时固定值
                chartData = enemyConfig.battleChart
            };
        }
        
        // === Prefab相关方法已移除，使用单例模式 ===
        
        private void ApplyBattleResults(BattleResult result)
        {
            // 输出简化的战斗结果
            Debug.Log($"[游戏管理器] 战斗结果详情:");
            Debug.Log($"  胜利: {result.isVictory}");
            Debug.Log($"  潜在伤害: {result.potentialDamage}");
            
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
