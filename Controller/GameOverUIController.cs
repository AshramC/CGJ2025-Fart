using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace FartGame
{
    public class GameOverUIController : MonoBehaviour, IController
    {
        [Header("UI References")]
        public Button restartButton;
        public Button mainMenuButton;
        public Text gameOverReasonText;
        public Text finalStatsText;
        public GameObject gameOverPanel;
        
        private GameModel mGameModel;
        private PlayerModel mPlayerModel;
        
        void Start()
        {
            mGameModel = this.GetModel<GameModel>();
            mPlayerModel = this.GetModel<PlayerModel>();
            
            // 绑定按钮事件
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(() =>
                {
                    this.SendCommand<StartGameCommand>();
                });
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(() =>
                {
                    mGameModel.CurrentGameState.Value = GameState.MainMenu;
                });
            }
            
            // 监听游戏状态变化
            mGameModel.CurrentGameState.RegisterWithInitValue(state =>
            {
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(state == GameState.GameOver);
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
            
            // 监听游戏结束事件，更新UI显示
            this.RegisterEvent<GameOverEvent>(OnGameOverEvent)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        
        private void OnGameOverEvent(GameOverEvent e)
        {
            // 更新游戏结束原因显示
            if (gameOverReasonText != null)
            {
                string reasonText = GetGameOverReasonText(e.Reason);
                gameOverReasonText.text = reasonText;
            }
            
            // 更新最终统计显示
            if (finalStatsText != null)
            {
                string statsText = $"最终屁值: {e.FinalFartValue:F1}\n游戏时长: {e.GameDuration:F1}秒";
                finalStatsText.text = statsText;
            }
        }
        
        private string GetGameOverReasonText(GameOverReason reason)
        {
            switch (reason)
            {
                case GameOverReason.FartDepleted:
                    return "屁值耗尽！";
                case GameOverReason.TimeUp:
                    return "时间到！";
                case GameOverReason.PlayerDeath:
                    return "玩家阵亡！";
                case GameOverReason.Manual:
                    return "游戏结束";
                default:
                    return "游戏结束";
            }
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
    }
}
