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
        
        void Start()
        {
            // 初始化架构
            FartGameArchitecture.InitArchitecture();
            
            // 获取系统引用
            mFartSystem = this.GetSystem<FartSystem>();
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
        
        // === 战斗系统启动接口 ===
        public void StartBattle(EnemyData enemyData)
        {
            if (currentBattleManager != null)
            {
                Debug.LogWarning("[战斗系统] 已在战斗中，无法启动新战斗");
                return;
            }
            
            // 1. 暂停主游戏系统
            PauseMainGameSystems();
            
            // 2. 切换游戏状态（通过QFramework）
            this.SendCommand<StartBattleCommand>();
            
            // 3. 提取玩家数据
            var playerData = ExtractPlayerBattleData();
            
            // 4. 实例化战斗系统
            InstantiateBattleSystem(playerData, enemyData);
        }
        
        // === 数据提取方法 ===
        private PlayerBattleData ExtractPlayerBattleData()
        {
            var playerModel = this.GetModel<PlayerModel>();
            return new PlayerBattleData
            {
                fartValue = playerModel.FartValue.Value,
                position = playerModel.Position.Value,
                isInFumeMode = playerModel.IsFumeMode.Value
            };
        }
        
        // === 战斗完成回调 ===
        private void OnBattleComplete(BattleResult result)
        {
            Debug.Log($"[战斗系统] 战斗结束 - 胜利: {result.isVictory}");
            
            // 处理战斗结果
            ApplyBattleResults(result);
            
            // 销毁战斗系统
            DestroyBattleSystem();
            
            // 恢复主游戏
            ResumeMainGameSystems();
            
            // 切换回主游戏状态
            this.SendCommand(new EndBattleCommand(result));
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
        
        private void InstantiateBattleSystem(PlayerBattleData playerData, EnemyData enemyData)
        {
            if (battleSystemPrefab == null)
            {
                Debug.LogError("[游戏管理器] battleSystemPrefab未设置");
                return;
            }
            
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
            
            Debug.Log("[游戏管理器] 战斗系统实例化完成");
        }
        
        private void DestroyBattleSystem()
        {
            if (currentBattleManager != null)
            {
                Destroy(currentBattleManager.gameObject);
                currentBattleManager = null;
                musicTimeManager = null;
                Debug.Log("[游戏管理器] 战斗系统销毁完成");
            }
        }
        
        private void ApplyBattleResults(BattleResult result)
        {
            // TODO: 将战斗结果应用到游戏状态
            Debug.Log($"[游戏管理器] 应用战斗结果: 胜利={result.isVictory}, 精度={result.accuracy:F2}");
        }
    }
}
