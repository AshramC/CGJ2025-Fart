using QFramework;

namespace FartGame
{
    public class RestorePlayerHealthCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var fartSystem = this.GetSystem<FartSystem>();
            fartSystem.RestoreToFullHealth();
            
            // Debug.Log("[RestorePlayerHealthCommand] 玩家血量已恢复");
        }
    }
}
