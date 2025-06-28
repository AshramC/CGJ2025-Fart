using QFramework;
using UnityEngine;

namespace FartGame
{
    /// <summary>
    /// 更新玩家屁值命令（战斗结束时使用）
    /// 专门用于战斗结束后将本地屁值同步到PlayerModel
    /// </summary>
    public class UpdatePlayerFartValueCommand : AbstractCommand
    {
        private readonly float finalFartValue;
        
        public UpdatePlayerFartValueCommand(float value)
        {
            finalFartValue = value;
        }
        
        protected override void OnExecute()
        {
            var playerModel = this.GetModel<PlayerModel>();
            var oldValue = playerModel.FartValue.Value;
            
            // 更新PlayerModel的屁值
            playerModel.FartValue.Value = finalFartValue;
            
            Debug.Log($"[UpdatePlayerFartValueCommand] 屁值同步: {oldValue} → {finalFartValue}");
            
            // 发送屁值更新事件，供其他系统响应
            this.SendEvent(new PlayerFartValueUpdatedEvent
            {
                OldValue = oldValue,
                NewValue = finalFartValue,
                Source = "Battle"
            });
        }
    }
    
    /// <summary>
    /// 玩家屁值更新事件（简化版）
    /// </summary>
    public struct PlayerFartValueUpdatedEvent
    {
        public float OldValue;
        public float NewValue;
        public string Source; // "Battle", "Resource", "Manual" 等
    }
}
