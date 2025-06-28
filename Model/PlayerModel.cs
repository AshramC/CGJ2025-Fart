using QFramework;
using UnityEngine;

namespace FartGame
{
    public class PlayerModel : AbstractModel
    {
        // 核心属性 - 使用BindableProperty实现自动UI绑定
        public BindableProperty<float> FartValue = new BindableProperty<float>(100f);
        public BindableProperty<float> MoveSpeed = new BindableProperty<float>(5f);
        public BindableProperty<float> BodySize = new BindableProperty<float>(1f);
        public BindableProperty<Vector3> Position = new BindableProperty<Vector3>(Vector3.zero);
        public BindableProperty<bool> IsFumeMode = new BindableProperty<bool>(false);
        
        // 计算属性
        public float InfluenceRadius => BodySize.Value * 2f;
        
        protected override void OnInit()
        {
            // 初始值将由System层设置，这里只做默认初始化
            // Model层不能访问其他Model
        }
    }
}
