using QFramework;

namespace FartGame
{
    public class ConsumeFartCommand : AbstractCommand
    {
        private readonly float mAmount;
        
        public ConsumeFartCommand(float amount)
        {
            mAmount = amount;
        }
        
        protected override void OnExecute()
        {
            var fartSystem = this.GetSystem<FartSystem>();
            var playerModel = this.GetModel<PlayerModel>();
            
            // 检查是否有足够的屁值可以消耗
            if (playerModel.FartValue.Value >= mAmount)
            {
                // 通过直接修改Model来消耗屁值（因为这是Command层，有权限修改Model）
                var newValue = playerModel.FartValue.Value - mAmount;
                playerModel.FartValue.Value = newValue;
            }
        }
    }
}
