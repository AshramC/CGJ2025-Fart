using QFramework;
using UnityEngine;

namespace FartGame
{
    public class SetFartValueCommand : AbstractCommand
    {
        private readonly float mNewValue;
        
        public SetFartValueCommand(float newValue)
        {
            mNewValue = newValue;
        }
        
        protected override void OnExecute()
        {
            var playerModel = this.GetModel<PlayerModel>();
            var config = this.GetModel<GameConfigModel>();
            
            // 限制在有效范围内
            float clampedValue = Mathf.Clamp(mNewValue, config.MinFartValue, config.MaxFartValue);
            
            // 设置新的屁值
            playerModel.FartValue.Value = clampedValue;
            
            // 注意：体型和速度会通过FartSystem自动更新
        }
    }
    
    // 增加屁值的便捷命令
    public class AddFartValueCommand : AbstractCommand
    {
        private readonly float mAmount;
        
        public AddFartValueCommand(float amount)
        {
            mAmount = amount;
        }
        
        protected override void OnExecute()
        {
            var playerModel = this.GetModel<PlayerModel>();
            var config = this.GetModel<GameConfigModel>();
            
            float newValue = playerModel.FartValue.Value + mAmount;
            newValue = Mathf.Clamp(newValue, config.MinFartValue, config.MaxFartValue);
            
            playerModel.FartValue.Value = newValue;
        }
    }
}
