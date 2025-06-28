using UnityEngine;
using System;

namespace FartGame.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [Header("依赖引用")]
        [SerializeField] private MusicTimeManager musicTimeManager;
        [SerializeField] private BattleVisualController visualController;
        
        [Header("战斗状态")]
        [SerializeField] private BattleStatus currentStatus;
        [SerializeField] private PlayerBattleData playerData;
        [SerializeField] private EnemyData enemyData;
        
        private Action<BattleResult> onBattleComplete;
        private bool isInitialized = false;
        
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
            
            isInitialized = true;
            Debug.Log($"[BattleManager] 初始化完成 - 敌人: {enemyData.enemyName}");
        }
        
        // === 生命周期管理接口 ===
        public void StartBattle()
        {
            if (!isInitialized)
            {
                Debug.LogError("[BattleManager] 尚未初始化，无法开始战斗");
                return;
            }
            
            currentStatus.phase = BattlePhase.Preparing;
            Debug.Log("[BattleManager] 战斗开始");
            
            // TODO: 实现战斗开始逻辑
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
            
            // TODO: 实现战斗结束逻辑
            
            // 创建战斗结果
            BattleResult result = new BattleResult
            {
                isVictory = currentStatus.enemyStamina <= 0,
                remainingStamina = currentStatus.enemyStamina,
                // TODO: 添加其他结果数据
            };
            
            currentStatus.phase = BattlePhase.Completed;
            onBattleComplete?.Invoke(result);
        }
        
        // === 外部调用接口 ===
        public void OnPlayerInput(Direction direction, double inputTime)
        {
            if (currentStatus.phase != BattlePhase.Playing)
                return;
                
            Debug.Log($"[BattleManager] 玩家输入: {direction} at {inputTime:F3}");
            
            // TODO: 实现输入处理逻辑
        }
        
        public BattleStatus GetCurrentStatus()
        {
            return currentStatus;
        }
        
        public float GetBattleProgress()
        {
            if (enemyData == null || enemyData.battleSequence == null)
                return 0f;
                
            // TODO: 实现进度计算逻辑
            return 0f;
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
            // TODO: 实现准备阶段逻辑
        }
        
        private void UpdatePlayingPhase()
        {
            // TODO: 实现游戏阶段逻辑
        }
        
        private void UpdateEndingPhase()
        {
            // TODO: 实现结束阶段逻辑
        }
        
        // === Unity生命周期 ===
        private void OnDestroy()
        {
            Debug.Log("[BattleManager] 组件销毁");
        }
        
        // === 调试信息 ===
        private void OnGUI()
        {
            if (!isInitialized || !Application.isPlaying)
                return;
                
            GUILayout.BeginArea(new Rect(10, 100, 300, 150));
            GUILayout.Label("=== BattleManager 状态 ===");
            GUILayout.Label($"阶段: {currentStatus.phase}");
            GUILayout.Label($"敌人体力: {currentStatus.enemyStamina:F1}");
            GUILayout.Label($"连击数: {currentStatus.currentCombo}");
            GUILayout.Label($"音乐时间: {currentStatus.currentMusicTime:F3}s");
            GUILayout.EndArea();
        }
    }
}
