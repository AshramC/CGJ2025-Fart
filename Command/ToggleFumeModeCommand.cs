using QFramework;

namespace FartGame
{
    public class ToggleFumeModeCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var playerModel = this.GetModel<PlayerModel>();
            var gameModel = this.GetModel<GameModel>();
            var config = this.GetModel<GameConfigModel>();
            
            // 检查游戏状态，只在游戏进行中允许切换
            if (gameModel.CurrentGameState.Value != GameState.Gameplay || gameModel.IsPaused.Value)
                return;
            
            // 如果要开启熏模式，检查屁值是否足够
            if (!playerModel.IsFumeMode.Value && playerModel.FartValue.Value <= config.MinFartValue)
            {
                // 屁值不足，无法开启熏模式
                return;
            }
            
            // 切换熏模式状态
            playerModel.IsFumeMode.Value = !playerModel.IsFumeMode.Value;
            
            // 发送熏模式变化事件
            this.SendEvent(new FumeModeChangedEvent 
            { 
                IsActive = playerModel.IsFumeMode.Value 
            });
        }
    }
}
