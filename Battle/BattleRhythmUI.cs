using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace FartGame.Battle
{
    /// <summary>
    /// 战斗音游UI管理器 - 处理左右生成note向中间移动的2D界面
    /// 基于时间精确计算note位置，参考SimpleGameUI的设计模式
    /// </summary>
    public class BattleRhythmUI : MonoBehaviour
    {
        [Header("UI设置")]
        [Tooltip("Canvas组件，如果为空则自动查找")]
        public Canvas rhythmCanvas;
        
        [Tooltip("左侧生成位置比例（0-1）")]
        public float leftSpawnRatio = 0.1f;
        
        [Tooltip("右侧生成位置比例（0-1）")]
        public float rightSpawnRatio = 0.9f;
        
        [Tooltip("中央判定线位置比例（0-1）")]
        public float judgementRatio = 0.5f;
        
        [Tooltip("垂直中心位置比例（0-1）")]
        public float verticalCenter = 0.5f;
        
        [Header("Note设置")]
        [Tooltip("Note大小")]
        public float noteSize = 30f;
        
        [Tooltip("判定线宽度")]
        public float judgementLineWidth = 4f;
        
        [Tooltip("判定线高度")]
        public float judgementLineHeight = 200f;
        
        [Header("调试设置")]
        public bool enableDebugLog = false;
        
        [Header("运行时状态")]
        [SerializeField] private bool isInitialized = false;
        
        // 依赖组件（参考SimpleGameUI的依赖注入模式）
        private MusicTimeManager musicTimeManager;
        private BattleChartManager chartManager;
        
        // UI组件
        private Canvas mainCanvas;
        private GameObject judgementLine;
        private Transform uiParent;
        
        // Note管理（参考SimpleGameUI的对象池模式）
        private Dictionary<BattleNoteInfo, GameObject> activeNotes;
        private Queue<GameObject> notePool;
        private readonly int poolSize = 20;
        
        // 位置计算缓存
        private float leftSpawnX;
        private float rightSpawnX;
        private float judgementX;
        private float centerY;
        
        /// <summary>
        /// 设置依赖关系 - 参考SimpleGameUI的依赖注入模式
        /// </summary>
        public void SetDependencies(MusicTimeManager timeManager, BattleChartManager chartMgr)
        {
            musicTimeManager = timeManager;
            chartManager = chartMgr;
            
            if (musicTimeManager == null)
            {
                Debug.LogError("[BattleRhythmUI] MusicTimeManager依赖注入失败！");
            }
            
            if (chartManager == null)
            {
                Debug.LogError("[BattleRhythmUI] BattleChartManager依赖注入失败！");
            }
            else
            {
                // 订阅ChartManager的事件
                chartManager.OnNoteSpawn += HandleNoteSpawn;
                chartManager.OnNoteProcessed += HandleNoteProcessed;
            }
            
            LogDebug("依赖关系设置完成");
        }
        
        /// <summary>
        /// 组件初始化 - 参考SimpleGameUI的初始化模式
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[BattleRhythmUI] 重复初始化，跳过");
                return;
            }
            
            // 验证依赖
            if (musicTimeManager == null || chartManager == null)
            {
                Debug.LogError("[BattleRhythmUI] 依赖组件未注入，初始化失败！");
                return;
            }
            
            // 初始化UI
            if (!InitializeUI())
            {
                Debug.LogError("[BattleRhythmUI] UI初始化失败！");
                return;
            }
            
            // 计算位置
            CalculatePositions();
            
            // 创建判定线
            CreateJudgementLine();
            
            // 初始化Note管理
            InitializeNoteManagement();
            
            isInitialized = true;
            LogDebug("初始化完成");
        }
        
        void Update()
        {
            if (isInitialized)
            {
                UpdateActiveNotes();
            }
        }
        
        /// <summary>
        /// 初始化UI系统 - 参考SimpleGameUI的Canvas设置
        /// </summary>
        private bool InitializeUI()
        {
            try
            {
                // 获取或查找Canvas
                if (rhythmCanvas != null)
                {
                    mainCanvas = rhythmCanvas;
                }
                else
                {
                    mainCanvas = FindObjectOfType<Canvas>();
                    if (mainCanvas == null)
                    {
                        Debug.LogError("[BattleRhythmUI] 未找到Canvas组件！");
                        return false;
                    }
                }
                
                // 创建UI父对象
                GameObject uiParentObj = new GameObject("BattleRhythmUI");
                uiParentObj.transform.SetParent(mainCanvas.transform, false);
                uiParent = uiParentObj.transform;
                
                LogDebug("UI系统初始化完成");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BattleRhythmUI] UI初始化失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 计算各种位置 - 参考SimpleGameUI的位置计算
        /// </summary>
        private void CalculatePositions()
        {
            RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;
            
            // 计算水平位置
            leftSpawnX = canvasWidth * (leftSpawnRatio - 0.5f);
            rightSpawnX = canvasWidth * (rightSpawnRatio - 0.5f);
            judgementX = canvasWidth * (judgementRatio - 0.5f);
            
            // 计算垂直位置
            centerY = canvasHeight * (verticalCenter - 0.5f);
            
            LogDebug($"位置计算完成 - 左侧X: {leftSpawnX}, 右侧X: {rightSpawnX}, 判定X: {judgementX}, 中心Y: {centerY}");
        }
        
        /// <summary>
        /// 创建中央判定线
        /// </summary>
        private void CreateJudgementLine()
        {
            // 创建判定线GameObject
            judgementLine = new GameObject("JudgementLine");
            judgementLine.transform.SetParent(uiParent, false);
            
            // 添加Image组件
            Image lineImage = judgementLine.AddComponent<Image>();
            lineImage.color = Color.red;
            
            // 设置RectTransform - 垂直线
            RectTransform rectTransform = judgementLine.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(judgementLineWidth, judgementLineHeight);
            rectTransform.anchoredPosition = new Vector2(judgementX, centerY);
            
            LogDebug("判定线创建完成");
        }
        
        /// <summary>
        /// 初始化Note管理 - 参考SimpleGameUI的对象池模式
        /// </summary>
        private void InitializeNoteManagement()
        {
            activeNotes = new Dictionary<BattleNoteInfo, GameObject>();
            notePool = new Queue<GameObject>();
            
            // 预创建对象池
            for (int i = 0; i < poolSize; i++)
            {
                GameObject noteObject = CreateNoteObject();
                noteObject.SetActive(false);
                notePool.Enqueue(noteObject);
            }
            
            LogDebug($"Note对象池初始化完成，预创建 {poolSize} 个对象");
        }
        
        /// <summary>
        /// 创建Note对象
        /// </summary>
        private GameObject CreateNoteObject()
        {
            GameObject noteObject = new GameObject("Note");
            noteObject.transform.SetParent(uiParent, false);
            
            // 添加Image组件
            Image noteImage = noteObject.AddComponent<Image>();
            noteImage.color = Color.white;
            
            // 设置RectTransform
            RectTransform rectTransform = noteObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(noteSize, noteSize);
            
            return noteObject;
        }
        
        /// <summary>
        /// 从对象池获取Note对象
        /// </summary>
        private GameObject GetNoteFromPool()
        {
            GameObject noteObject;
            
            if (notePool.Count > 0)
            {
                noteObject = notePool.Dequeue();
            }
            else
            {
                // 对象池用完了，创建新的
                noteObject = CreateNoteObject();
                LogDebug("对象池不足，创建新的Note对象");
            }
            
            noteObject.SetActive(true);
            return noteObject;
        }
        
        /// <summary>
        /// 将Note对象返回对象池
        /// </summary>
        private void ReturnNoteToPool(GameObject noteObject)
        {
            if (noteObject != null)
            {
                noteObject.SetActive(false);
                notePool.Enqueue(noteObject);
            }
        }
        
        /// <summary>
        /// 处理Note生成事件
        /// </summary>
        private void HandleNoteSpawn(BattleNoteInfo noteInfo)
        {
            if (!isInitialized || activeNotes.ContainsKey(noteInfo))
            {
                return;
            }
            
            // 从对象池获取Note对象
            GameObject noteObject = GetNoteFromPool();
            if (noteObject == null) return;
            
            // 设置note外观
            Image image = noteObject.GetComponent<Image>();
            image.color = GetNoteColor(noteInfo.eventType);
            
            // 判断从左侧还是右侧生成（基于小节数）
            bool fromLeft = ShouldSpawnFromLeft(noteInfo);
            float startX = fromLeft ? leftSpawnX : rightSpawnX;
            
            // 设置初始位置
            RectTransform rectTransform = noteObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(startX, centerY);
            
            // 添加到活跃列表
            activeNotes[noteInfo] = noteObject;
            
            LogDebug($"Note生成: {noteInfo} (从{(fromLeft ? "左" : "右")}侧)");
        }
        
        /// <summary>
        /// 处理Note处理完成事件
        /// </summary>
        private void HandleNoteProcessed(BattleNoteInfo noteInfo)
        {
            if (!isInitialized || !activeNotes.ContainsKey(noteInfo))
            {
                return;
            }
            
            // 获取note对象
            GameObject noteObject = activeNotes[noteInfo];
            
            // 从活跃列表移除
            activeNotes.Remove(noteInfo);
            
            // 返回对象池
            ReturnNoteToPool(noteObject);
            
            LogDebug($"Note移除: {noteInfo}");
        }
        
        /// <summary>
        /// 更新活跃的Note位置 - 基于时间的精确计算
        /// </summary>
        private void UpdateActiveNotes()
        {
            if (musicTimeManager == null || activeNotes.Count == 0)
            {
                return;
            }
            
            double currentTime = musicTimeManager.GetJudgementTime();
            
            foreach (var kvp in activeNotes)
            {
                BattleNoteInfo noteInfo = kvp.Key;
                GameObject noteObject = kvp.Value;
                
                if (noteObject == null || !noteObject.activeInHierarchy)
                {
                    continue;
                }
                
                // 使用NoteRenderHelper的时间计算方法
                float progress = NoteRenderHelper.CalculateDropProgress(
                    currentTime, noteInfo.judgementTime, noteInfo.spawnTime);
                
                // 判断从哪一侧生成
                bool fromLeft = ShouldSpawnFromLeft(noteInfo);
                float startX = fromLeft ? leftSpawnX : rightSpawnX;
                
                // 计算当前X位置（从生成位置向判定线移动）
                float currentX = Mathf.Lerp(startX, judgementX, progress);
                
                // 更新位置
                RectTransform rectTransform = noteObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(currentX, centerY);
                
                // 更新透明度（基于生命周期）
                Image image = noteObject.GetComponent<Image>();
                if (image != null)
                {
                    float alpha = NoteRenderHelper.CalculateNoteAlpha(
                        currentTime, noteInfo.judgementTime, noteInfo.spawnTime, noteInfo.isProcessed);
                    Color color = image.color;
                    color.a = alpha;
                    image.color = color;
                }
            }
        }
        
        /// <summary>
        /// 判断Note应该从哪一侧生成
        /// </summary>
        private bool ShouldSpawnFromLeft(BattleNoteInfo noteInfo)
        {
            // 基于小节数决定：奇数小节从左，偶数小节从右
            return noteInfo.beatEvent.measure % 2 == 0;
        }
        
        /// <summary>
        /// 根据事件类型获取Note颜色
        /// </summary>
        private Color GetNoteColor(BattleEventType eventType)
        {
            return eventType switch
            {
                BattleEventType.Tap => Color.cyan,
                BattleEventType.Hold => Color.yellow,
                _ => Color.white
            };
        }
        
        /// <summary>
        /// 清理所有Note
        /// </summary>
        public void ClearAllNotes()
        {
            if (!isInitialized)
            {
                return;
            }
            
            // 将所有活跃note返回对象池
            foreach (var kvp in activeNotes)
            {
                ReturnNoteToPool(kvp.Value);
            }
            
            activeNotes.Clear();
            LogDebug("所有Note已清理");
        }
        
        /// <summary>
        /// 获取UI状态信息
        /// </summary>
        public string GetUIStatus()
        {
            if (!isInitialized)
            {
                return "未初始化";
            }
            
            return $"活跃Note: {activeNotes.Count} | 对象池: {notePool.Count}";
        }
        
        /// <summary>
        /// 调试日志输出
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[BattleRhythmUI] {message}");
            }
        }
        
        /// <summary>
        /// 组件销毁时清理
        /// </summary>
        void OnDestroy()
        {
            // 取消订阅事件
            if (chartManager != null)
            {
                chartManager.OnNoteSpawn -= HandleNoteSpawn;
                chartManager.OnNoteProcessed -= HandleNoteProcessed;
            }
            
            LogDebug("组件销毁，事件取消订阅");
        }
    }
}
