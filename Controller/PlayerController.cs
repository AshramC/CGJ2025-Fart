using QFramework;
using UnityEngine;

namespace FartGame
{
    public class PlayerController : MonoBehaviour, IController
    {
        private PlayerModel mModel;
        private GameModel mGameModel;
        private Rigidbody mRigidbody;
        private Collider mCollider;
        
        [Header("Visual References")]
        public GameObject visualObject; // 玩家的视觉表现对象
        
        void Start()
        {
            mModel = this.GetModel<PlayerModel>();
            mGameModel = this.GetModel<GameModel>();
            mRigidbody = GetComponent<Rigidbody>();
            mCollider = GetComponent<Collider>();
            
            // 如果没有指定visualObject，默认使用自身
            if (visualObject == null)
            {
                visualObject = gameObject;
            }
            
            // 绑定位置同步
            mModel.Position.RegisterWithInitValue(pos =>
            {
                transform.position = pos;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
            
            // 绑定体型变化
            mModel.BodySize.RegisterWithInitValue(size =>
            {
                visualObject.transform.localScale = Vector3.one * size;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
            
            // 绑定熏模式碰撞设置
            mModel.IsFumeMode.RegisterWithInitValue(isFume =>
            {
                if (mCollider != null)
                {
                    mCollider.isTrigger = isFume;
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        
        void Update()
        {
            // 只在游戏进行中且未暂停时处理输入
            if (mGameModel.CurrentGameState.Value != GameState.Gameplay || mGameModel.IsPaused.Value)
                return;
                
            HandleInput();
            HandleMovement();
        }
        
        private void HandleInput()
        {
            // 空格键切换熏模式
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.SendCommand<ToggleFumeModeCommand>();
            }
            
            // ESC键暂停游戏
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.SendCommand<PauseGameCommand>();
            }
        }
        
        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            Vector3 movement = new Vector3(horizontal, 0, vertical);
            movement = movement.normalized * mModel.MoveSpeed.Value * Time.deltaTime;
            
            if (mRigidbody != null)
            {
                mRigidbody.MovePosition(transform.position + movement);
            }
            else
            {
                transform.position += movement;
            }
            
            // 更新位置到Model
            mModel.Position.Value = transform.position;
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
    }
}
