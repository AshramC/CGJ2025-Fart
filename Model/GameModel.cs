using QFramework;

namespace FartGame
{
    public class GameModel : AbstractModel
    {
        public BindableProperty<GameState> CurrentGameState = new BindableProperty<GameState>(GameState.MainMenu);
        public BindableProperty<bool> IsPaused = new BindableProperty<bool>(false);
        
        protected override void OnInit()
        {
            // 游戏状态变化时发送事件
            CurrentGameState.Register(state => 
            {
                this.SendEvent(new GameStateChangedEvent { NewState = state });
            });
        }
    }
    
    public enum GameState
    {
        MainMenu,
        Gameplay,
        Paused,
        GameOver
    }
}
