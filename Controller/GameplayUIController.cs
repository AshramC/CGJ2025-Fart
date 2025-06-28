using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace FartGame
{
    public class GameplayUIController : MonoBehaviour, IController
    {
        [Header("UI References")]
        public Text fartValueText;
        public Text modeText;
        public Slider fartValueSlider;
        public Image bodySizeIndicator;
        public Text speedText;
        public GameObject gameplayPanel;
        
        [Header("UI Settings")]
        public float maxSliderValue = 200f; // 滑动条显示的最大值
        
        private PlayerModel mPlayerModel;
        private GameModel mGameModel;
        
        void Start()
        {
            mPlayerModel = this.GetModel<PlayerModel>();
            mGameModel = this.GetModel<GameModel>();
            
            // 绑定屁值显示
            if (fartValueText != null)
            {
                mPlayerModel.FartValue.RegisterWithInitValue(value =>
                {
                    fartValueText.text = $"屁值: {value:F1}";
                }).UnRegisterWhenGameObjectDestroyed(gameObject);
            }
            
            // 绑定屁值滑动条
            if (fartValueSlider != null)
            {
                mPlayerModel.FartValue.RegisterWithInitValue(value =>
                {
                    fartValueSlider.value = value / maxSliderValue;
                }).UnRegisterWhenGameObjectDestroyed(gameObject);
            }
            
            // 绑定模式显示
            if (modeText != null)
            {
                mPlayerModel.IsFumeMode.RegisterWithInitValue(isFume =>
                {
                    modeText.text = isFume ? "熏模式" : "普通模式";
                    modeText.color = isFume ? Color.red : Color.white;
                }).UnRegisterWhenGameObjectDestroyed(gameObject);
            }
            
            // 绑定体型指示器
            if (bodySizeIndicator != null)
            {
                mPlayerModel.BodySize.RegisterWithInitValue(size =>
                {
                    // 将体型映射到UI指示器大小
                    float uiScale = 0.5f + (size - 1f) * 0.3f;
                    bodySizeIndicator.transform.localScale = Vector3.one * uiScale;
                }).UnRegisterWhenGameObjectDestroyed(gameObject);
            }
            
            // 绑定速度显示
            if (speedText != null)
            {
                mPlayerModel.MoveSpeed.RegisterWithInitValue(speed =>
                {
                    speedText.text = $"速度: {speed:F1}";
                }).UnRegisterWhenGameObjectDestroyed(gameObject);
            }
            
            // 监听游戏状态，控制UI显示
            mGameModel.CurrentGameState.RegisterWithInitValue(state =>
            {
                if (gameplayPanel != null)
                {
                    gameplayPanel.SetActive(state == GameState.Gameplay || state == GameState.Paused);
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
    }
}
