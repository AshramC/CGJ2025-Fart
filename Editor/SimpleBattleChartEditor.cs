// using UnityEngine;
// using UnityEditor;
// using FartGame.Battle;
// using System.Collections.Generic;
// using System.Linq;
//
// namespace FartGame.Editor
// {
//     /// <summary>
//     /// 简化战斗谱面编辑器
//     /// 专为CGJ2025项目的BattleChartData设计
//     /// </summary>
//     public class SimpleBattleChartEditor : EditorWindow
//     {
//         [Header("编辑器状态")]
//         private BattleChartEditorData editorData;
//         private BattleChartData currentChart;
//         private List<EditorNoteInfo> clipboard = new List<EditorNoteInfo>(); // 剪贴板
//         
//         [Header("UI状态")]
//         private Vector2 scrollPosition = Vector2.zero;
//         private BattleEventType selectedEventType = BattleEventType.Tap;
//         
//         [Header("显示设置")]
//         private float beatWidth = 60f;
//         private float trackHeight = 100f;
//         
//         [Header("量化设置")]
//         private float quantizeValue = 1f; // 量化值（1=整拍，0.5=半拍，0.25=四分一拍）
//         private bool enableQuantize = true; // 是否启用量化
//         
//         [Header("音频控制")]
//         private bool isPlaying = false;
//         private float currentTime = 0f;
//         private AudioSource audioSource;
//         
//         [Header("颜色配置")]
//         private readonly Color backgroundColor = new Color(0.2f, 0.2f, 0.2f);
//         private readonly Color measureLineColor = Color.white;
//         private readonly Color beatLineColor = Color.gray;
//         private readonly Color playheadColor = Color.red;
//         private readonly Color tapColor = Color.cyan;
//         private readonly Color holdColor = Color.green;
//         
//         /// <summary>
//         /// 打开编辑器窗口
//         /// </summary>
//         [MenuItem("CGJ2025/Battle Chart Editor")]
//         public static void OpenWindow()
//         {
//             var window = GetWindow<SimpleBattleChartEditor>("Battle Chart Editor");
//             window.minSize = new Vector2(800, 500);
//             window.Show();
//         }
//         
//         /// <summary>
//         /// 打开编辑器窗口并加载指定谱面
//         /// </summary>
//         public static void OpenWindow(BattleChartData chartData)
//         {
//             var window = GetWindow<SimpleBattleChartEditor>("Battle Chart Editor");
//             window.minSize = new Vector2(800, 500);
//             window.LoadChart(chartData);
//             window.Show();
//         }
//         
//         void OnEnable()
//         {
//             InitializeEditor();
//         }
//         
//         void OnDisable()
//         {
//             CleanupEditor();
//         }
//         
//         void OnGUI()
//         {
//             try
//             {
//                 DrawHeader();
//                 DrawAudioControls();
//                 DrawEventTools();
//                 DrawTimeline();
//                 DrawFooter();
//                 
//                 // 处理播放更新
//                 HandlePlaybackUpdate();
//                 
//                 // 绘制用户反馈
//                 DrawUserFeedback();
//             }
//             catch (System.Exception ex)
//             {
//                 HandleUIException(ex);
//             }
//         }
//         
//         /// <summary>
//         /// 初始化编辑器
//         /// </summary>
//         void InitializeEditor()
//         {
//             // 创建默认编辑器数据
//             if (editorData == null)
//             {
//                 editorData = new BattleChartEditorData();
//             }
//             
//             // 初始化音频系统
//             InitializeAudioSystem();
//             
//             Debug.Log("[SimpleBattleChartEditor] 编辑器初始化完成");
//         }
//         
//         /// <summary>
//         /// 清理编辑器
//         /// </summary>
//         void CleanupEditor()
//         {
//             StopPlayback();
//             CleanupAudioSystem();
//             
//             Debug.Log("[SimpleBattleChartEditor] 编辑器清理完成");
//         }
//         
//         /// <summary>
//         /// 初始化音频系统
//         /// </summary>
//         void InitializeAudioSystem()
//         {
//             var audioGO = GameObject.Find("BattleChartEditor_AudioPlayer");
//             if (audioGO == null)
//             {
//                 audioGO = new GameObject("BattleChartEditor_AudioPlayer");
//                 audioGO.hideFlags = HideFlags.HideAndDontSave;
//             }
//             
//             audioSource = audioGO.GetComponent<AudioSource>();
//             if (audioSource == null)
//             {
//                 audioSource = audioGO.AddComponent<AudioSource>();
//             }
//             
//             audioSource.loop = false;
//             audioSource.playOnAwake = false;
//             audioSource.volume = 0.8f;
//         }
//         
//         /// <summary>
//         /// 清理音频系统
//         /// </summary>
//         void CleanupAudioSystem()
//         {
//             var audioGO = GameObject.Find("BattleChartEditor_AudioPlayer");
//             if (audioGO != null)
//             {
//                 DestroyImmediate(audioGO);
//             }
//         }
//         
//         /// <summary>
//         /// 绘制标题栏
//         /// </summary>
//         void DrawHeader()
//         {
//             EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
//             
//             string title = editorData != null ? $"编辑谱面: {editorData.chartName}" : "Battle Chart Editor";
//             EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
//             
//             GUILayout.FlexibleSpace();
//             
//             // 文件操作按钮
//             if (GUILayout.Button("新建", EditorStyles.toolbarButton, GUILayout.Width(50)))
//             {
//                 CreateNewChart();
//             }
//             
//             if (GUILayout.Button("加载", EditorStyles.toolbarButton, GUILayout.Width(50)))
//             {
//                 LoadChartFromFile();
//             }
//             
//             if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(50)))
//             {
//                 SaveChartToFile();
//             }
//             
//             GUILayout.Space(10);
//             
//             // 谱面工具按钮
//             if (GUILayout.Button("验证", EditorStyles.toolbarButton, GUILayout.Width(50)))
//             {
//                 ValidateChart();
//             }
//             
//             if (GUILayout.Button("统计", EditorStyles.toolbarButton, GUILayout.Width(50)))
//             {
//                 ShowDetailedStatistics();
//             }
//             
//             EditorGUILayout.EndHorizontal();
//             
//             // 显示谱面信息
//             if (editorData != null)
//             {
//                 EditorGUILayout.BeginVertical("box");
//                 EditorGUILayout.LabelField("谱面信息", EditorStyles.boldLabel);
//                 
//                 EditorGUILayout.BeginHorizontal();
//                 EditorGUILayout.LabelField($"BPM: {editorData.bpm} | 小节: {editorData.measures} | 每小节拍数: {editorData.beatsPerMeasure}");
//                 EditorGUILayout.EndHorizontal();
//                 
//                 EditorGUILayout.BeginHorizontal();
//                 EditorGUILayout.LabelField($"时长: {editorData.GetTotalDuration():F2}秒 | 音符数: {editorData.notes.Count}");
//                 
//                 // 显示验证状态
//                 var (isValid, errors) = editorData.ValidateData();
//                 if (isValid)
//                 {
//                     EditorGUILayout.LabelField(" | 状态: ✓ 有效", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.green } });
//                 }
//                 else
//                 {
//                     EditorGUILayout.LabelField($" | 状态: ✗ {errors.Count}个错误", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } });
//                 }
//                 EditorGUILayout.EndHorizontal();
//                 
//                 EditorGUILayout.EndVertical();
//             }
//         }
//         
//         /// <summary>
//         /// 绘制音频控制
//         /// </summary>
//         void DrawAudioControls()
//         {
//             EditorGUILayout.BeginVertical("box");
//             EditorGUILayout.LabelField("音频控制", EditorStyles.boldLabel);
//             
//             EditorGUILayout.BeginHorizontal();
//             
//             // 播放/暂停按钮
//             string playButtonText = isPlaying ? "暂停" : "播放";
//             if (GUILayout.Button(playButtonText, GUILayout.Width(80)))
//             {
//                 TogglePlayback();
//             }
//             
//             // 停止按钮
//             if (GUILayout.Button("停止", GUILayout.Width(80)))
//             {
//                 StopPlayback();
//             }
//             
//             // 时间显示
//             float totalDuration = editorData != null ? editorData.GetTotalDuration() : 0f;
//             EditorGUILayout.LabelField($"时间: {currentTime:F2}s / {totalDuration:F2}s");
//             
//             // 进度条
//             float newTime = EditorGUILayout.Slider(currentTime, 0f, totalDuration);
//             if (Mathf.Abs(newTime - currentTime) > 0.1f)
//             {
//                 SeekToTime(newTime);
//             }
//             
//             EditorGUILayout.EndHorizontal();
//             
//             // 音频文件设置
//             EditorGUILayout.BeginHorizontal();
//             EditorGUILayout.LabelField("音频文件:", GUILayout.Width(80));
//             var newAudio = EditorGUILayout.ObjectField(editorData?.audioClip, typeof(AudioClip), false) as AudioClip;
//             if (newAudio != (editorData?.audioClip))
//             {
//                 if (editorData != null)
//                 {
//                     editorData.audioClip = newAudio;
//                     if (audioSource != null)
//                     {
//                         audioSource.clip = newAudio;
//                     }
//                 }
//             }
//             EditorGUILayout.EndHorizontal();
//             
//             // 音频偏移设置
//             if (editorData != null)
//             {
//                 EditorGUILayout.BeginHorizontal();
//                 EditorGUILayout.LabelField("音频偏移:", GUILayout.Width(80));
//                 
//                 float newOffset = EditorGUILayout.FloatField(editorData.audioOffset, GUILayout.Width(80));
//                 if (Mathf.Abs(newOffset - editorData.audioOffset) > 0.001f)
//                 {
//                     editorData.audioOffset = newOffset;
//                     editorData.UpdateAllNoteTiming(); // 更新所有音符的时间信息
//                     Debug.Log($"[SimpleBattleChartEditor] 音频偏移设置为: {editorData.audioOffset:F3}s");
//                 }
//                 
//                 EditorGUILayout.LabelField("秒", GUILayout.Width(20));
//                 
//                 // 快捷调整按钮
//                 if (GUILayout.Button("-0.1s", GUILayout.Width(50)))
//                 {
//                     AdjustAudioOffset(-0.1f);
//                 }
//                 if (GUILayout.Button("+0.1s", GUILayout.Width(50)))
//                 {
//                     AdjustAudioOffset(0.1f);
//                 }
//                 if (GUILayout.Button("重置", GUILayout.Width(50)))
//                 {
//                     ResetAudioOffset();
//                 }
//                 
//                 EditorGUILayout.EndHorizontal();
//                 
//                 // 显示同步状态
//                 if (isPlaying)
//                 {
//                     EditorGUILayout.BeginHorizontal();
//                     float syncedTime = currentTime + editorData.audioOffset;
//                     EditorGUILayout.LabelField($"同步状态: 谱面时间={currentTime:F2}s, 音频时间={syncedTime:F2}s", EditorStyles.miniLabel);
//                     EditorGUILayout.EndHorizontal();
//                 }
//             }
//             
//             EditorGUILayout.EndVertical();
//         }
//         
//         /// <summary>
//         /// 绘制事件工具
//         /// </summary>
//         void DrawEventTools()
//         {
//             EditorGUILayout.BeginVertical("box");
//             EditorGUILayout.LabelField("事件工具", EditorStyles.boldLabel);
//             
//             EditorGUILayout.BeginHorizontal();
//             
//             // 事件类型选择
//             EditorGUILayout.LabelField("事件类型:", GUILayout.Width(80));
//             selectedEventType = (BattleEventType)EditorGUILayout.EnumPopup(selectedEventType, GUILayout.Width(100));
//             
//             // 快捷按钮
//             if (GUILayout.Button("●Tap", GUILayout.Width(60)))
//             {
//                 selectedEventType = BattleEventType.Tap;
//             }
//             if (GUILayout.Button("━Hold", GUILayout.Width(60)))
//             {
//                 selectedEventType = BattleEventType.Hold;
//             }
//             
//             GUILayout.Space(20);
//             
//             // 清空按钮
//             if (GUILayout.Button("清空所有事件", GUILayout.Width(100)))
//             {
//                 if (EditorUtility.DisplayDialog("确认", "确定要删除所有事件吗？", "确定", "取消"))
//                 {
//                     ClearAllEvents();
//                 }
//             }
//             
//             EditorGUILayout.EndHorizontal();
//             
//             // 显示设置
//             EditorGUILayout.BeginHorizontal();
//             EditorGUILayout.LabelField("缩放:", GUILayout.Width(40));
//             beatWidth = EditorGUILayout.Slider(beatWidth, 30f, 120f, GUILayout.Width(150));
//             
//             GUILayout.Space(20);
//             
//             // 量化设置
//             enableQuantize = EditorGUILayout.Toggle("量化:", enableQuantize, GUILayout.Width(60));
//             if (enableQuantize)
//             {
//                 EditorGUILayout.LabelField("精度:", GUILayout.Width(40));
//                 int quantizeIndex = GetQuantizeIndex(quantizeValue);
//                 string[] quantizeOptions = { "1/1", "1/2", "1/4", "1/8", "1/16" };
//                 float[] quantizeValues = { 1f, 0.5f, 0.25f, 0.125f, 0.0625f };
//                 
//                 int newQuantizeIndex = EditorGUILayout.Popup(quantizeIndex, quantizeOptions, GUILayout.Width(60));
//                 if (newQuantizeIndex >= 0 && newQuantizeIndex < quantizeValues.Length)
//                 {
//                     quantizeValue = quantizeValues[newQuantizeIndex];
//                 }
//             }
//             EditorGUILayout.EndHorizontal();
//             
//             EditorGUILayout.EndVertical();
//         }
//         
//         /// <summary>
//         /// 绘制时间轴
//         /// </summary>
//         void DrawTimeline()
//         {
//             if (editorData == null)
//             {
//                 EditorGUILayout.HelpBox("请先加载或创建谱面", MessageType.Info);
//                 return;
//             }
//             
//             var timelineRect = GUILayoutUtility.GetRect(position.width - 20, trackHeight + 100);
//             EditorGUI.DrawRect(timelineRect, backgroundColor);
//             
//             // 滚动视图
//             var scrollViewRect = new Rect(timelineRect.x, timelineRect.y, timelineRect.width, timelineRect.height);
//             var contentRect = new Rect(0, 0, GetTimelineWidth(), timelineRect.height);
//             
//             scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, contentRect);
//             
//             // 绘制网格
//             DrawBeatGrid(contentRect);
//             
//             // 绘制事件
//             DrawEvents(contentRect);
//             
//             // 绘制播放头
//             if (isPlaying)
//             {
//                 DrawPlayhead(contentRect);
//             }
//             
//             // 处理鼠标输入
//             HandleMouseInput(contentRect);
//             
//             GUI.EndScrollView();
//         }
//         
//         /// <summary>
//         /// 绘制底部状态栏
//         /// </summary>
//         void DrawFooter()
//         {
//             EditorGUILayout.BeginVertical("box");
//             
//             EditorGUILayout.BeginHorizontal();
//             EditorGUILayout.LabelField("操作提示:", EditorStyles.boldLabel, GUILayout.Width(80));
//             EditorGUILayout.LabelField("左键=添加/选择 | 右键=删除 | 空格=播放/暂停 | Ctrl+C=复制 | Ctrl+V=粘贴 | Ctrl+X=剪切 | Ctrl+D=重复 | Ctrl+Q=量化 | G=切换量化 | ←→=调偏移 | Ctrl+R=重置偏移 | Del=删除 | Ctrl+A=全选 | Esc=取消选择", EditorStyles.miniLabel);
//             EditorGUILayout.EndHorizontal();
//             
//             // 统计信息
//             if (editorData != null)
//             {
//                 EditorGUILayout.LabelField(editorData.GetStatistics(), EditorStyles.miniLabel);
//             }
//             
//             EditorGUILayout.EndVertical();
//         }
//         
//         /// <summary>
//         /// 处理播放更新
//         /// </summary>
//         void HandlePlaybackUpdate()
//         {
//             if (isPlaying && audioSource != null && audioSource.isPlaying)
//             {
//                 currentTime = audioSource.time;
//                 Repaint();
//             }
//         }
//         
//         /// <summary>
//         /// 获取时间轴总宽度
//         /// </summary>
//         float GetTimelineWidth()
//         {
//             if (editorData == null) return 100f;
//             return editorData.measures * editorData.beatsPerMeasure * beatWidth + 100f;
//         }
//         
//         /// <summary>
//         /// 绘制节拍网格
//         /// </summary>
//         void DrawBeatGrid(Rect rect)
//         {
//             if (editorData == null) return;
//             
//             int totalBeats = editorData.measures * editorData.beatsPerMeasure;
//             
//             for (int beat = 0; beat <= totalBeats; beat++)
//             {
//                 float x = beat * beatWidth;
//                 if (x > rect.width) break;
//                 
//                 // 小节线用粗线，拍线用细线
//                 bool isMeasureLine = (beat % editorData.beatsPerMeasure) == 0;
//                 Color lineColor = isMeasureLine ? measureLineColor : beatLineColor;
//                 float lineWidth = isMeasureLine ? 2f : 1f;
//                 
//                 var lineRect = new Rect(x, rect.y, lineWidth, rect.height);
//                 EditorGUI.DrawRect(lineRect, lineColor);
//                 
//                 // 显示小节标记
//                 if (isMeasureLine && beat > 0)
//                 {
//                     int measure = beat / editorData.beatsPerMeasure;
//                     var labelRect = new Rect(x + 2, rect.y, 50, 20);
//                     GUI.Label(labelRect, $"M{measure + 1}", EditorStyles.miniLabel);
//                 }
//                 
//                 // 显示拍子标记
//                 if (!isMeasureLine)
//                 {
//                     int beatInMeasure = beat % editorData.beatsPerMeasure;
//                     var labelRect = new Rect(x + 2, rect.y + 20, 30, 15);
//                     GUI.Label(labelRect, $"B{beatInMeasure + 1}", EditorStyles.miniLabel);
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 绘制事件
//         /// </summary>
//         void DrawEvents(Rect rect)
//         {
//             if (editorData == null) return;
//             
//             foreach (var note in editorData.notes)
//             {
//                 float beatPosition = note.measure * editorData.beatsPerMeasure + note.beat;
//                 float x = beatPosition * beatWidth;
//                 
//                 if (x < scrollPosition.x - beatWidth || x > scrollPosition.x + rect.width) continue;
//                 
//                 Color eventColor = note.noteType == BattleEventType.Tap ? tapColor : holdColor;
//                 
//                 // 如果音符被选中，使用高亮颜色
//                 if (note.isSelected)
//                 {
//                     eventColor = Color.yellow;
//                 }
//                 
//                 if (note.noteType == BattleEventType.Hold && note.IsHoldNote())
//                 {
//                     // 绘制Hold条
//                     float holdWidth = note.GetHoldDurationInBeats() * beatWidth;
//                     var holdRect = new Rect(x - 8, rect.y + 60, holdWidth, 20);
//                     EditorGUI.DrawRect(holdRect, eventColor);
//                     
//                     // 绘制Hold边框
//                     var borderRect = new Rect(x - 9, rect.y + 59, holdWidth + 2, 22);
//                     EditorGUI.DrawRect(borderRect, Color.black);
//                     EditorGUI.DrawRect(holdRect, eventColor);
//                 }
//                 else
//                 {
//                     // 绘制Tap点
//                     var eventRect = new Rect(x - 8, rect.y + 60, 16, 20);
//                     var borderRect = new Rect(x - 9, rect.y + 59, 18, 22);
//                     EditorGUI.DrawRect(borderRect, Color.black);
//                     EditorGUI.DrawRect(eventRect, eventColor);
//                 }
//                 
//                 // 显示音符类型标记
//                 string noteChar = note.noteType == BattleEventType.Tap ? "●" : "━";
//                 var charRect = new Rect(x - 6, rect.y + 40, 20, 20);
//                 GUI.Label(charRect, noteChar, EditorStyles.boldLabel);
//                 
//                 // 显示位置信息
//                 var posRect = new Rect(x - 15, rect.y + 85, 30, 15);
//                 GUI.Label(posRect, $"{note.measure + 1}:{note.beat + 1}", EditorStyles.miniLabel);
//                 
//                 // 显示时间信息(调试用)
//                 if (note.isSelected)
//                 {
//                     var timeRect = new Rect(x - 20, rect.y + 100, 40, 15);
//                     GUI.Label(timeRect, $"{note.judgementTime:F2}s", EditorStyles.miniLabel);
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 绘制播放头
//         /// </summary>
//         void DrawPlayhead(Rect rect)
//         {
//             if (editorData == null) return;
//             
//             float beatDuration = 60f / editorData.bpm;
//             float currentBeat = currentTime / beatDuration;
//             float x = currentBeat * beatWidth;
//             
//             var playheadRect = new Rect(x - 1, rect.y, 2, rect.height);
//             EditorGUI.DrawRect(playheadRect, playheadColor);
//             
//             // 播放头顶部三角形
//             var triangleRect = new Rect(x - 8, rect.y - 10, 16, 10);
//             EditorGUI.DrawRect(triangleRect, playheadColor);
//         }
//         
//         /// <summary>
//         /// 处理鼠标输入
//         /// </summary>
//         void HandleMouseInput(Rect rect)
//         {
//             Event evt = Event.current;
//             
//             if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
//             {
//                 // 计算点击的拍位置
//                 float clickX = evt.mousePosition.x;
//                 float rawBeatPosition = clickX / beatWidth;
//                 
//                 // 量化处理
//                 int measure, beat;
//                 QuantizeToBeatPosition(rawBeatPosition, out measure, out beat);
//                     
//                     if (measure < editorData.measures)
//                     {
//                         if (evt.button == 0) // 左键点击
//                         {
//                             bool multiSelect = evt.control || evt.command; // Ctrl/Cmd键多选
//                             var existingNote = editorData.GetNoteAt(measure, beat);
//                             
//                             if (existingNote != null)
//                             {
//                                 // 选择音符
//                                 SelectNoteAt(measure, beat, multiSelect);
//                             }
//                             else
//                             {
//                                 // 添加新音符
//                                 if (!multiSelect)
//                                 {
//                                     editorData.ClearSelection();
//                                 }
//                                 AddEventAt(measure, beat);
//                             }
//                             
//                             evt.Use();
//                             Repaint();
//                         }
//                         else if (evt.button == 1) // 右键点击
//                         {
//                             RemoveEventAt(measure, beat);
//                             evt.Use();
//                             Repaint();
//                         }
//                     }
//                 }
//             }
//             
//             // 处理键盘快捷键
//             if (evt.type == EventType.KeyDown)
//             {
//                 switch (evt.keyCode)
//                 {
//                     case KeyCode.Space:
//                         TogglePlayback();
//                         evt.Use();
//                         break;
//                     case KeyCode.T:
//                         selectedEventType = BattleEventType.Tap;
//                         evt.Use();
//                         Repaint();
//                         break;
//                     case KeyCode.H:
//                         selectedEventType = BattleEventType.Hold;
//                         evt.Use();
//                         Repaint();
//                         break;
//                     case KeyCode.Delete:
//                     case KeyCode.Backspace:
//                         DeleteSelectedNotes();
//                         evt.Use();
//                         Repaint();
//                         break;
//                     case KeyCode.A:
//                         if (evt.control || evt.command)
//                         {
//                             SelectAllNotes();
//                             evt.Use();
//                             Repaint();
//                         }
//                         break;
//                     case KeyCode.Escape:
//                         ClearSelection();
//                         evt.Use();
//                         Repaint();
//                         break;
//                     case KeyCode.C:
//                         if (evt.control || evt.command)
//                         {
//                             CopySelectedNotes();
//                             evt.Use();
//                         }
//                         break;
//                     case KeyCode.V:
//                         if (evt.control || evt.command)
//                         {
//                             PasteNotes();
//                             evt.Use();
//                             Repaint();
//                         }
//                         break;
//                     case KeyCode.X:
//                         if (evt.control || evt.command)
//                         {
//                             CutSelectedNotes();
//                             evt.Use();
//                             Repaint();
//                         }
//                         break;
//                     case KeyCode.D:
//                         if (evt.control || evt.command)
//                         {
//                             DuplicateSelectedNotes();
//                             evt.Use();
//                             Repaint();
//                         }
//                         break;
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 删除选中的音符
//         /// </summary>
//         void DeleteSelectedNotes()
//         {
//             if (editorData == null || editorData.selectedNotes.Count == 0) return;
//             
//             var notesToDelete = new List<EditorNoteInfo>(editorData.selectedNotes);
//             foreach (var note in notesToDelete)
//             {
//                 editorData.notes.Remove(note);
//             }
//             editorData.selectedNotes.Clear();
//             
//             Debug.Log($"[SimpleBattleChartEditor] 删除了 {notesToDelete.Count} 个选中的音符");
//         }
//         
//         /// <summary>
//         /// 选择所有音符
//         /// </summary>
//         void SelectAllNotes()
//         {
//             if (editorData == null) return;
//             
//             editorData.ClearSelection();
//             foreach (var note in editorData.notes)
//             {
//                 editorData.SelectNote(note, true);
//             }
//             
//             Debug.Log($"[SimpleBattleChartEditor] 选择了所有 {editorData.notes.Count} 个音符");
//         }
//         
//         /// <summary>
//         /// 清空选择
//         /// </summary>
//         void ClearSelection()
//         {
//             if (editorData == null) return;
//             
//             editorData.ClearSelection();
//             Debug.Log("[SimpleBattleChartEditor] 清空选择");
//         }
//         
//         /// <summary>
//         /// 复制选中的音符
//         /// </summary>
//         void CopySelectedNotes()
//         {
//             if (editorData == null || editorData.selectedNotes.Count == 0)
//             {
//                 Debug.Log("[SimpleBattleChartEditor] 没有选中的音符可复制");
//                 return;
//             }
//             
//             clipboard.Clear();
//             clipboard.AddRange(editorData.CopySelectedNotes());
//             
//             Debug.Log($"[SimpleBattleChartEditor] 复制了 {clipboard.Count} 个音符到剪贴板");
//         }
//         
//         /// <summary>
//         /// 剪切选中的音符
//         /// </summary>
//         void CutSelectedNotes()
//         {
//             if (editorData == null || editorData.selectedNotes.Count == 0)
//             {
//                 Debug.Log("[SimpleBattleChartEditor] 没有选中的音符可剪切");
//                 return;
//             }
//             
//             // 先复制
//             CopySelectedNotes();
//             
//             // 再删除
//             DeleteSelectedNotes();
//             
//             Debug.Log($"[SimpleBattleChartEditor] 剪切了 {clipboard.Count} 个音符");
//         }
//         
//         /// <summary>
//         /// 粘贴音符
//         /// </summary>
//         void PasteNotes()
//         {
//             if (editorData == null || clipboard.Count == 0)
//             {
//                 Debug.Log("[SimpleBattleChartEditor] 剪贴板为空，无法粘贴");
//                 return;
//             }
//             
//             // 计算粘贴位置：当前播放时间对应的节拍位置
//             float beatDuration = 60f / editorData.bpm;
//             float currentBeat = currentTime / beatDuration;
//             int targetMeasure = Mathf.FloorToInt(currentBeat / editorData.beatsPerMeasure);
//             int targetBeat = Mathf.FloorToInt(currentBeat % editorData.beatsPerMeasure);
//             
//             // 确保目标位置在有效范围内
//             targetMeasure = Mathf.Clamp(targetMeasure, 0, editorData.measures - 1);
//             targetBeat = Mathf.Clamp(targetBeat, 0, editorData.beatsPerMeasure - 1);
//             
//             PasteNotesAt(targetMeasure, targetBeat);
//         }
//         
//         /// <summary>
//         /// 在指定位置粘贴音符
//         /// </summary>
//         void PasteNotesAt(int targetMeasure, int targetBeat)
//         {
//             if (editorData == null || clipboard.Count == 0) return;
//             
//             // 计算偏移量
//             var firstNote = clipboard[0];
//             int measureOffset = targetMeasure - firstNote.measure;
//             int beatOffset = targetBeat - firstNote.beat;
//             
//             int pastedCount = 0;
//             var newlyPastedNotes = new List<EditorNoteInfo>();
//             
//             foreach (var note in clipboard)
//             {
//                 int newMeasure = note.measure + measureOffset;
//                 int newBeat = note.beat + beatOffset;
//                 
//                 // 检查新位置是否有效
//                 if (newMeasure >= 0 && newMeasure < editorData.measures && 
//                     newBeat >= 0 && newBeat < editorData.beatsPerMeasure)
//                 {
//                     // 检查位置是否已占用
//                     var existing = editorData.GetNoteAt(newMeasure, newBeat);
//                     if (existing == null)
//                     {
//                         // 创建新音符
//                         var newNote = note.Clone();
//                         newNote.measure = newMeasure;
//                         newNote.beat = newBeat;
//                         
//                         // 如果是Hold音符，调整结束位置
//                         if (newNote.IsHoldNote())
//                         {
//                             int holdDuration = newNote.GetHoldDurationInBeats();
//                             newNote.holdEndBeat = Mathf.Min(newBeat + holdDuration, editorData.beatsPerMeasure - 1);
//                         }
//                         
//                         // 更新时间信息
//                         newNote.UpdateTiming(editorData.bpm, editorData.beatsPerMeasure);
//                         
//                         editorData.notes.Add(newNote);
//                         newlyPastedNotes.Add(newNote);
//                         pastedCount++;
//                     }
//                     else
//                     {
//                         Debug.LogWarning($"[SimpleBattleChartEditor] 粘贴位置 M{newMeasure + 1}B{newBeat + 1} 已被占用");
//                     }
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"[SimpleBattleChartEditor] 粘贴位置 M{newMeasure + 1}B{newBeat + 1} 超出范围");
//                 }
//             }
//             
//             // 选中新粘贴的音符
//             if (newlyPastedNotes.Count > 0)
//             {
//                 editorData.ClearSelection();
//                 foreach (var note in newlyPastedNotes)
//                 {
//                     editorData.SelectNote(note, true);
//                 }
//             }
//             
//             Debug.Log($"[SimpleBattleChartEditor] 粘贴了 {pastedCount}/{clipboard.Count} 个音符到 M{targetMeasure + 1}B{targetBeat + 1}");
//         }
//         
//         /// <summary>
//         /// 重复选中的音符
//         /// </summary>
//         void DuplicateSelectedNotes()
//         {
//             if (editorData == null || editorData.selectedNotes.Count == 0)
//             {
//                 Debug.Log("[SimpleBattleChartEditor] 没有选中的音符可重复");
//                 return;
//             }
//             
//             // 临时保存剪贴板
//             var tempClipboard = new List<EditorNoteInfo>(clipboard);
//             
//             // 复制到剪贴板
//             CopySelectedNotes();
//             
//             // 找到选中音符的最右边位置作为粘贴起点
//             int maxMeasure = editorData.selectedNotes.Max(n => n.measure);
//             int maxBeat = editorData.selectedNotes.Where(n => n.measure == maxMeasure).Max(n => n.beat);
//             
//             // 计算下一个可用位置
//             int nextBeat = maxBeat + 1;
//             int nextMeasure = maxMeasure;
//             
//             if (nextBeat >= editorData.beatsPerMeasure)
//             {
//                 nextBeat = 0;
//                 nextMeasure++;
//             }
//             
//             // 在计算出的位置粘贴
//             if (nextMeasure < editorData.measures)
//             {
//                 PasteNotesAt(nextMeasure, nextBeat);
//             }
//             else
//             {
//                 Debug.LogWarning("[SimpleBattleChartEditor] 无法在当前谱面范围内重复音符");
//             }
//             
//             // 恢复原剪贴板
//             clipboard = tempClipboard;
//         }
//         
//         /// <summary>
//         /// 切换播放状态
//         /// </summary>
//         void TogglePlayback()
//         {
//             if (!ValidateEditorState()) return;
//             
//             SafeExecute(() => {
//                 if (editorData?.audioClip == null)
//                 {
//                     ShowUserMessage("请先加载音频文件", MessageType.Warning);
//                     return;
//                 }
//                 
//                 if (isPlaying)
//                 {
//                     audioSource.Pause();
//                     isPlaying = false;
//                     ShowUserMessage("播放已暂停", MessageType.Info);
//                 }
//                 else
//                 {
//                     audioSource.time = currentTime;
//                     audioSource.Play();
//                     isPlaying = true;
//                     ShowUserMessage("开始播放", MessageType.Info);
//                 }
//             }, "切换播放状态");
//         }
//         
//         /// <summary>
//         /// 停止播放
//         /// </summary>
//         void StopPlayback()
//         {
//             SafeExecute(() => {
//                 if (audioSource != null)
//                 {
//                     audioSource.Stop();
//                 }
//                 isPlaying = false;
//                 currentTime = 0f;
//                 ShowUserMessage("播放已停止", MessageType.Info);
//             }, "停止播放");
//         }
//         
//         /// <summary>
//         /// 跳转到指定时间
//         /// </summary>
//         void SeekToTime(float time)
//         {
//             float totalDuration = editorData != null ? editorData.GetTotalDuration() : 0f;
//             currentTime = Mathf.Clamp(time, 0f, totalDuration);
//             
//             if (audioSource != null && editorData?.audioClip != null)
//             {
//                 audioSource.time = currentTime + editorData.audioOffset;
//             }
//         }
//         
//         /// <summary>
//         /// 创建新谱面
//         /// </summary>
//         void CreateNewChart()
//         {
//             // 创建新的编辑器数据
//             editorData = new BattleChartEditorData();
//             editorData.chartName = "新谱面";
//             editorData.bpm = 120;
//             editorData.measures = 4;
//             editorData.beatsPerMeasure = 8;
//             
//             // 清空当前谱面引用
//             currentChart = null;
//             
//             // 重置播放状态
//             StopPlayback();
//             
//             Debug.Log("[SimpleBattleChartEditor] 创建新谱面");
//         }
//         
//         /// <summary>
//         /// 从文件加载谱面
//         /// </summary>
//         void LoadChartFromFile()
//         {
//             StartProgressOperation("加载谱面");
//             
//             SafeExecute(() => {
//                 string path = EditorUtility.OpenFilePanel("加载谱面", "Assets", "asset");
//                 if (string.IsNullOrEmpty(path))
//                 {
//                     CompleteProgressOperation();
//                     return;
//                 }
//                 
//                 UpdateProgress(0.3f);
//                 
//                 // 转换为相对路径
//                 if (path.StartsWith(Application.dataPath))
//                 {
//                     path = "Assets" + path.Substring(Application.dataPath.Length);
//                 }
//                 
//                 UpdateProgress(0.5f);
//                 
//                 var chartAsset = AssetDatabase.LoadAssetAtPath<BattleChartData>(path);
//                 if (chartAsset != null)
//                 {
//                     UpdateProgress(0.8f);
//                     LoadChart(chartAsset);
//                     UpdateProgress(1.0f);
//                     ShowUserMessage($"谱面加载成功: {chartAsset.chartName}", MessageType.Info);
//                 }
//                 else
//                 {
//                     ShowUserMessage("无法加载谱面文件，请检查文件格式", MessageType.Error);
//                 }
//                 
//                 CompleteProgressOperation();
//             }, "加载谱面");
//         }
//         
//         /// <summary>
//         /// 保存谱面到文件
//         /// </summary>
//         void SaveChartToFile()
//         {
//             if (!ValidateEditorState())
//             {
//                 ShowUserMessage("编辑器状态无效，无法保存", MessageType.Error);
//                 return;
//             }
//             
//             if (editorData == null)
//             {
//                 EditorUtility.DisplayDialog("错误", "没有可保存的谱面数据", "确定");
//                 return;
//             }
//             
//             StartProgressOperation("保存谱面");
//             
//             SafeExecute(() => {
//                 string path = EditorUtility.SaveFilePanel("保存谱面", "Assets", editorData.chartName, "asset");
//                 if (string.IsNullOrEmpty(path))
//                 {
//                     CompleteProgressOperation();
//                     return;
//                 }
//                 
//                 UpdateProgress(0.2f);
//                 
//                 // 转换为相对路径
//                 if (path.StartsWith(Application.dataPath))
//                 {
//                     path = "Assets" + path.Substring(Application.dataPath.Length);
//                 }
//                 
//                 UpdateProgress(0.4f);
//                 
//                 // 验证数据
//                 var (isValid, errors) = editorData.ValidateData();
//                 if (!isValid)
//                 {
//                     string errorMsg = $"谱面数据有 {errors.Count} 个错误，确定要保存吗？\n\n前3个错误：\n";
//                     for (int i = 0; i < Mathf.Min(3, errors.Count); i++)
//                     {
//                         errorMsg += $"{i + 1}. {errors[i]}\n";
//                     }
//                     
//                     if (!EditorUtility.DisplayDialog("数据验证警告", errorMsg, "仍要保存", "取消"))
//                     {
//                         CompleteProgressOperation();
//                         return;
//                     }
//                 }
//                 
//                 UpdateProgress(0.6f);
//                 
//                 // 创建或更新BattleChartData
//                 if (currentChart == null)
//                 {
//                     currentChart = editorData.ToBattleChartData();
//                 }
//                 else
//                 {
//                     editorData.ToBattleChartData(currentChart);
//                 }
//                 
//                 UpdateProgress(0.8f);
//                 
//                 // 保存到文件
//                 AssetDatabase.CreateAsset(currentChart, path);
//                 AssetDatabase.SaveAssets();
//                 AssetDatabase.Refresh();
//                 
//                 UpdateProgress(1.0f);
//                 
//                 Debug.Log($"[SimpleBattleChartEditor] 谱面已保存: {path}");
//                 ShowUserMessage($"谱面已成功保存到: {System.IO.Path.GetFileName(path)}", MessageType.Info);
//                 
//                 CompleteProgressOperation();
//             }, "保存谱面");
//         }
//         
//         /// <summary>
//         /// 加载谱面
//         /// </summary>
//         void LoadChart(BattleChartData chartData)
//         {
//             if (chartData == null)
//             {
//                 Debug.LogError("[SimpleBattleChartEditor] 谱面数据为空");
//                 return;
//             }
//             
//             currentChart = chartData;
//             
//             if (editorData == null)
//             {
//                 editorData = new BattleChartEditorData();
//             }
//             
//             editorData.FromBattleChartData(chartData);
//             
//             // 重置播放状态
//             StopPlayback();
//             
//             Debug.Log($"[SimpleBattleChartEditor] 加载谱面: {chartData.chartName}, {editorData.notes.Count}个音符");
//         }
//         
//         /// <summary>
//         /// 在指定位置添加事件
//         /// </summary>
//         void AddEventAt(int measure, int beat)
//         {
//             if (!ValidateEditorState()) return;
//             
//             SafeExecute(() => {
//                 bool success = editorData.AddNote(measure, beat, selectedEventType);
//                 if (success)
//                 {
//                     ShowUserMessage($"已添加{selectedEventType}音符 M{measure + 1}B{beat + 1}", MessageType.Info);
//                     Debug.Log($"[SimpleBattleChartEditor] 添加音符: M{measure + 1}B{beat + 1} {selectedEventType}");
//                 }
//                 else
//                 {
//                     ShowUserMessage($"无法添加音符到 M{measure + 1}B{beat + 1}，位置可能已被占用", MessageType.Warning);
//                 }
//             }, "添加音符");
//         }
//         
//         /// <summary>
//         /// 移除指定位置的事件
//         /// </summary>
//         void RemoveEventAt(int measure, int beat)
//         {
//             if (!ValidateEditorState()) return;
//             
//             SafeExecute(() => {
//                 bool removed = editorData.RemoveNoteAt(measure, beat);
//                 if (removed)
//                 {
//                     ShowUserMessage($"已删除音符 M{measure + 1}B{beat + 1}", MessageType.Info);
//                     Debug.Log($"[SimpleBattleChartEditor] 移除音符: M{measure + 1}B{beat + 1}");
//                 }
//                 else
//                 {
//                     ShowUserMessage($"位置 M{measure + 1}B{beat + 1} 没有音符可删除", MessageType.Warning);
//                 }
//             }, "删除音符");
//         }
//         
//         /// <summary>
//         /// 清空所有事件
//         /// </summary>
//         void ClearAllEvents()
//         {
//             if (editorData != null)
//             {
//                 editorData.ClearAllNotes();
//                 Debug.Log("[SimpleBattleChartEditor] 清空所有音符");
//             }
//         }
//         
//         /// <summary>
//         /// 选择指定位置的音符
//         /// </summary>
//         void SelectNoteAt(int measure, int beat, bool multiSelect = false)
//         {
//             if (editorData == null) return;
//             
//             var note = editorData.GetNoteAt(measure, beat);
//             if (note != null)
//             {
//                 editorData.SelectNote(note, multiSelect);
//                 Debug.Log($"[SimpleBattleChartEditor] 选择音符: {note.GetPositionString()}");
//             }
//         }
//     }
//
//
// }
