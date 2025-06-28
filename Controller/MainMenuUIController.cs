using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace FartGame
{
    public class MainMenuUIController : MonoBehaviour, IController
    {
        [Header("UI References")]
        public Button startButton;
        public Button quitButton;
        public GameObject menuPanel;
        
        private GameModel mGameModel;
        
        void Start()
        {
            mGameModel = this.GetModel<GameModel>();
            
            // 绑定按钮事件
            if (startButton != null)
            {
                startButton.onClick.AddListener(() =>
                {
                    this.SendCommand<StartGameCommand>();
                });
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(() =>
                {
                    #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                    #else
                        Application.Quit();
                    #endif
                });
            }
            
            // 监听游戏状态变化
            mGameModel.CurrentGameState.RegisterWithInitValue(state =>
            {
                // 只在主菜单状态显示菜单UI
                if (menuPanel != null)
                {
                    menuPanel.SetActive(state == GameState.MainMenu);
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
    }
}
