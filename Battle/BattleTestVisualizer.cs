using UnityEngine;
using System.Collections.Generic;
using FartGame.Battle;

namespace FartGame.Battle
{
    /// <summary>
    /// 战斗测试视觉化组件
    /// 负责音符轨道和视觉反馈的渲染
    /// </summary>
    public class BattleTestVisualizer : MonoBehaviour
    {
        [Header("轨道设置")]
        [SerializeField] private float trackHeight = 10f;
        [SerializeField] private float judgeLineY = 0f;
        [SerializeField] private float trackWidth = 0.2f;
        
        [Header("音符预制体")]
        [SerializeField] private GameObject tapNotePrefab;
        [SerializeField] private GameObject holdNotePrefab;
        
        [Header("材质设置")]
        [SerializeField] private Material trackMaterial;
        [SerializeField] private Material tapMaterial;
        [SerializeField] private Material holdMaterial;
        [SerializeField] private Material judgeMaterial;
        
        [Header("对象池")]
        [SerializeField] private int poolSize = 50;
        private Queue<GameObject> tapNotePool;
        private Queue<GameObject> holdNotePool;
        private List<NoteVisual> activeNotes;
        
        [Header("轨道对象")]
        private GameObject trackLine;
        private GameObject judgeLine;
        
        [Header("判定反馈")]
        [SerializeField] private float feedbackDuration = 0.5f;
        private float lastJudgeTime = 0f;
        private BattleJudgeResult lastJudgeResult = BattleJudgeResult.None;
        
        private bool isInitialized = false;
        
        /// <summary>
        /// 音符视觉对象数据
        /// </summary>
        [System.Serializable]
        public class NoteVisual
        {
            public GameObject gameObject;
            public BattleNoteInfo noteInfo;
            public bool isActive;
            public Vector3 startPosition;
            public Vector3 targetPosition;
            
            public NoteVisual(GameObject go, BattleNoteInfo info)
            {
                gameObject = go;
                noteInfo = info;
                isActive = true;
                startPosition = go.transform.position;
                targetPosition = new Vector3(0, 0, 0); // 判定线位置
            }
        }
        
        /// <summary>
        /// 初始化视觉化系统
        /// </summary>
        public void Initialize()
        {
            CreateTrackElements();
            InitializeObjectPools();
            InitializeActiveNotesList();
            isInitialized = true;
            Debug.Log("[BattleTestVisualizer] 视觉化系统初始化完成");
        }
        
        /// <summary>
        /// 创建轨道元素
        /// </summary>
        private void CreateTrackElements()
        {
            // 创建轨道线
            trackLine = CreatePrimitiveObject("TrackLine", PrimitiveType.Cube);
            trackLine.transform.position = new Vector3(0, trackHeight / 2, 0);
            trackLine.transform.localScale = new Vector3(trackWidth, trackHeight, 0.1f);
            
            if (trackMaterial != null)
                trackLine.GetComponent<Renderer>().material = trackMaterial;
            else
                trackLine.GetComponent<Renderer>().material.color = Color.gray;
            
            // 创建判定线
            judgeLine = CreatePrimitiveObject("JudgeLine", PrimitiveType.Cube);
            judgeLine.transform.position = new Vector3(0, judgeLineY, 0.1f);
            judgeLine.transform.localScale = new Vector3(2f, 0.1f, 0.1f);
            
            if (judgeMaterial != null)
                judgeLine.GetComponent<Renderer>().material = judgeMaterial;
            else
                judgeLine.GetComponent<Renderer>().material.color = Color.yellow;
        }
        
        /// <summary>
        /// 初始化对象池
        /// </summary>
        private void InitializeObjectPools()
        {
            tapNotePool = new Queue<GameObject>();
            holdNotePool = new Queue<GameObject>();
            
            // 创建Tap音符池
            for (int i = 0; i < poolSize; i++)
            {
                GameObject tapNote = CreateTapNote();
                tapNote.SetActive(false);
                tapNotePool.Enqueue(tapNote);
            }
            
            // 创建Hold音符池
            for (int i = 0; i < poolSize / 2; i++)
            {
                GameObject holdNote = CreateHoldNote();
                holdNote.SetActive(false);
                holdNotePool.Enqueue(holdNote);
            }
            
            Debug.Log($"[BattleTestVisualizer] 对象池初始化完成 - Tap:{poolSize}, Hold:{poolSize/2}");
        }
        
        /// <summary>
        /// 初始化活跃音符列表
        /// </summary>
        private void InitializeActiveNotesList()
        {
            activeNotes = new List<NoteVisual>();
        }
        
        /// <summary>
        /// 创建基础几何体对象
        /// </summary>
        private GameObject CreatePrimitiveObject(string name, PrimitiveType type)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(transform);
            return obj;
        }
        
        /// <summary>
        /// 创建Tap音符
        /// </summary>
        private GameObject CreateTapNote()
        {
            GameObject note;
            
            if (tapNotePrefab != null)
            {
                note = Instantiate(tapNotePrefab, transform);
            }
            else
            {
                note = CreatePrimitiveObject("TapNote", PrimitiveType.Cube);
                note.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                
                if (tapMaterial != null)
                    note.GetComponent<Renderer>().material = tapMaterial;
                else
                    note.GetComponent<Renderer>().material.color = Color.red;
            }
            
            return note;
        }
        
        /// <summary>
        /// 创建Hold音符
        /// </summary>
        private GameObject CreateHoldNote()
        {
            GameObject note;
            
            if (holdNotePrefab != null)
            {
                note = Instantiate(holdNotePrefab, transform);
            }
            else
            {
                note = CreatePrimitiveObject("HoldNote", PrimitiveType.Cylinder);
                note.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
                
                if (holdMaterial != null)
                    note.GetComponent<Renderer>().material = holdMaterial;
                else
                    note.GetComponent<Renderer>().material.color = Color.blue;
            }
            
            return note;
        }
        
        /// <summary>
        /// 生成音符视觉对象
        /// </summary>
        public void SpawnNoteVisual(BattleNoteInfo noteInfo)
        {
            if (!isInitialized) return;
            
            GameObject noteObj = null;
            
            // 从对象池获取音符
            if (noteInfo.isHoldNote && holdNotePool.Count > 0)
            {
                noteObj = holdNotePool.Dequeue();
            }
            else if (!noteInfo.isHoldNote && tapNotePool.Count > 0)
            {
                noteObj = tapNotePool.Dequeue();
            }
            
            if (noteObj == null)
            {
                Debug.LogWarning("[BattleTestVisualizer] 对象池已空，创建新的音符对象");
                noteObj = noteInfo.isHoldNote ? CreateHoldNote() : CreateTapNote();
            }
            
            // 设置音符位置和状态
            SetupNoteVisual(noteObj, noteInfo);
            
            // 添加到活跃列表
            var noteVisual = new NoteVisual(noteObj, noteInfo);
            activeNotes.Add(noteVisual);
            
            Debug.Log($"[BattleTestVisualizer] 生成音符视觉: {noteInfo}");
        }
        
        /// <summary>
        /// 设置音符视觉对象
        /// </summary>
        private void SetupNoteVisual(GameObject noteObj, BattleNoteInfo noteInfo)
        {
            // 设置起始位置（轨道顶部）
            Vector3 startPos = new Vector3(0, trackHeight, 0);
            noteObj.transform.position = startPos;
            
            // Hold音符特殊设置
            if (noteInfo.isHoldNote)
            {
                // 根据持续时间调整长度
                float length = noteInfo.holdDuration * 2f; // 可调整比例
                noteObj.transform.localScale = new Vector3(0.4f, length, 0.4f);
            }
            
            noteObj.SetActive(true);
        }
        
        /// <summary>
        /// 更新视觉效果
        /// </summary>
        public void UpdateVisuals(double currentTime)
        {
            if (!isInitialized) return;
            
            UpdateNoteAnimations(currentTime);
            UpdateJudgeFeedback();
        }
        
        /// <summary>
        /// 更新音符动画
        /// </summary>
        private void UpdateNoteAnimations(double currentTime)
        {
            var notesToRemove = new List<NoteVisual>();
            
            foreach (var noteVisual in activeNotes)
            {
                if (!noteVisual.isActive) continue;
                
                var noteInfo = noteVisual.noteInfo;
                
                // 计算音符应该到达的位置
                double timeToJudge = noteInfo.judgementTime - currentTime;
                double totalDropTime = noteInfo.judgementTime - noteInfo.spawnTime;
                
                if (totalDropTime > 0)
                {
                    float progress = (float)(1.0 - (timeToJudge / totalDropTime));
                    progress = Mathf.Clamp01(progress);
                    
                    Vector3 currentPos = Vector3.Lerp(
                        new Vector3(0, trackHeight, 0),
                        new Vector3(0, judgeLineY, 0),
                        progress
                    );
                    
                    noteVisual.gameObject.transform.position = currentPos;
                }
                
                // 检查是否需要清理
                if (noteInfo.isProcessed && currentTime > noteInfo.judgementTime + 1.0)
                {
                    notesToRemove.Add(noteVisual);
                }
            }
            
            // 清理已完成的音符
            foreach (var noteVisual in notesToRemove)
            {
                RemoveNoteVisual(noteVisual);
            }
        }
        
        /// <summary>
        /// 更新判定反馈
        /// </summary>
        private void UpdateJudgeFeedback()
        {
            if (Time.time - lastJudgeTime < feedbackDuration)
            {
                // 显示判定结果
                Color feedbackColor = GetJudgeResultColor(lastJudgeResult);
                judgeLine.GetComponent<Renderer>().material.color = feedbackColor;
                
                // 缩放效果
                float scale = 1f + (feedbackDuration - (Time.time - lastJudgeTime)) * 0.5f;
                judgeLine.transform.localScale = new Vector3(2f * scale, 0.1f, 0.1f);
            }
            else
            {
                // 恢复默认状态
                if (judgeMaterial != null)
                    judgeLine.GetComponent<Renderer>().material = judgeMaterial;
                else
                    judgeLine.GetComponent<Renderer>().material.color = Color.yellow;
                
                judgeLine.transform.localScale = new Vector3(2f, 0.1f, 0.1f);
            }
        }
        
        /// <summary>
        /// 获取判定结果颜色
        /// </summary>
        private Color GetJudgeResultColor(BattleJudgeResult result)
        {
            return result switch
            {
                BattleJudgeResult.Perfect => Color.yellow,
                BattleJudgeResult.Good => Color.green,
                BattleJudgeResult.Miss => Color.red,
                _ => Color.white
            };
        }
        
        /// <summary>
        /// 显示判定结果
        /// </summary>
        public void ShowJudgeResult(BattleJudgeResult result, BattleNoteInfo noteInfo)
        {
            lastJudgeTime = Time.time;
            lastJudgeResult = result;
            
            Debug.Log($"[BattleTestVisualizer] 显示判定结果: {result} - {noteInfo}");
        }
        
        /// <summary>
        /// 显示Hold完成
        /// </summary>
        public void ShowHoldComplete(BattleJudgeResult result, BattleNoteInfo noteInfo)
        {
            ShowJudgeResult(result, noteInfo);
            Debug.Log($"[BattleTestVisualizer] Hold完成: {result} - {noteInfo}");
        }
        
        /// <summary>
        /// 处理音符被处理事件
        /// </summary>
        public void OnNoteProcessed(BattleNoteInfo noteInfo)
        {
            // 找到对应的视觉对象并标记
            var noteVisual = activeNotes.Find(n => n.noteInfo == noteInfo);
            if (noteVisual != null)
            {
                // 可以添加击中效果
                var renderer = noteVisual.gameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.white; // 击中时变白色
                }
            }
        }
        
        /// <summary>
        /// 处理音符错过事件
        /// </summary>
        public void OnNoteMissed(BattleNoteInfo noteInfo)
        {
            // 找到对应的视觉对象并标记
            var noteVisual = activeNotes.Find(n => n.noteInfo == noteInfo);
            if (noteVisual != null)
            {
                // 添加错过效果
                var renderer = noteVisual.gameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.gray; // 错过时变灰色
                }
            }
        }
        
        /// <summary>
        /// 移除音符视觉对象
        /// </summary>
        private void RemoveNoteVisual(NoteVisual noteVisual)
        {
            if (noteVisual == null) return;
            
            // 返回对象池
            noteVisual.gameObject.SetActive(false);
            
            if (noteVisual.noteInfo.isHoldNote)
            {
                holdNotePool.Enqueue(noteVisual.gameObject);
            }
            else
            {
                tapNotePool.Enqueue(noteVisual.gameObject);
            }
            
            // 从活跃列表移除
            activeNotes.Remove(noteVisual);
            noteVisual.isActive = false;
        }
        
        /// <summary>
        /// 清理所有音符
        /// </summary>
        public void ClearAllNotes()
        {
            foreach (var noteVisual in activeNotes)
            {
                if (noteVisual.gameObject != null)
                {
                    noteVisual.gameObject.SetActive(false);
                    
                    if (noteVisual.noteInfo.isHoldNote)
                        holdNotePool.Enqueue(noteVisual.gameObject);
                    else
                        tapNotePool.Enqueue(noteVisual.gameObject);
                }
            }
            
            activeNotes.Clear();
            Debug.Log("[BattleTestVisualizer] 已清理所有音符");
        }
        
        /// <summary>
        /// 销毁时清理
        /// </summary>
        private void OnDestroy()
        {
            ClearAllNotes();
        }
    }
}
