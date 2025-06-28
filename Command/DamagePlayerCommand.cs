using QFramework;
using UnityEngine;

namespace FartGame
{
    public class DamagePlayerCommand : AbstractCommand
    {
        private readonly float damageAmount;
        
        public DamagePlayerCommand(float damage)
        {
            damageAmount = damage;
        }
        
        protected override void OnExecute()
        {
            if (damageAmount <= 0f)
            {
                Debug.LogWarning("[DamagePlayerCommand] 伤害值无效，跳过处理");
                return;
            }
            
            // 获取系统引用
            var fartSystem = this.GetSystem<FartSystem>();
            var gameModel = this.GetModel<GameModel>();
            var playerModel = this.GetModel<PlayerModel>();
            
            // 记录受伤前的屁值
            float oldValue = playerModel.FartValue.Value;
            
            // 扣除屁值
            fartSystem.DamageFart(damageAmount);
            
            // 检查是否屁值归零（战斗失败条件）
            if (playerModel.FartValue.Value <= 0f && gameModel.CurrentGameState.Value == GameState.Battle)
            {
                Debug.Log("[DamagePlayerCommand] 玩家屁值归零，触发战斗失败");
                
                // 发送战斗失败事件
                var defeatResult = new FartGame.Battle.BattleResult
                {
                    isVictory = false,
                    remainingStamina = 0f,
                    accuracy = 0f,
                    totalHits = 0,
                    totalMisses = 1,
                    maxCombo = 0,
                    perfectCount = 0,
                    goodCount = 0,
                    missCount = 1,
                    totalNotes = 1,
                    chartAccuracy = 0f
                };
                
                this.SendCommand(new BattleDefeatCommand(defeatResult));
            }
            
            Debug.Log($"[DamagePlayerCommand] 玩家受到 {damageAmount} 点伤害，屁值从 {oldValue} 降至 {playerModel.FartValue.Value}");
        }
    }
}
