using QFramework;

namespace FartGame
{
    public class FartGameArchitecture : Architecture<FartGameArchitecture>
    {
        protected override void Init()
        {
            // 注册数据层
            RegisterModel(new GameConfigModel());
            RegisterModel(new GameModel());
            RegisterModel(new PlayerModel());
            
            // 注册系统层
            RegisterSystem(new GameStateSystem());
            RegisterSystem(new FartSystem());
        }
    }
}
