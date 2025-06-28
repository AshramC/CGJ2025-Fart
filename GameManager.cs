using QFramework;
using UnityEngine;

namespace FartGame
{
    // 游戏管理器，负责驱动系统更新
    public class GameManager : MonoBehaviour, IController
    {
        private FartSystem mFartSystem;
        
        void Start()
        {
            // 初始化架构
            FartGameArchitecture.InitArchitecture();
            
            // 获取系统引用
            mFartSystem = this.GetSystem<FartSystem>();
        }
        
        void Update()
        {
            // 驱动FartSystem更新
            if (mFartSystem != null)
            {
                mFartSystem.Update();
            }
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
    }
}
