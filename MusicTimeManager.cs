using UnityEngine;

/// <summary>
/// 音乐时间管理器 - 改进版本
/// 移除单例模式，通过SystemManager管理生命周期
/// 职责：音乐播放 + 时间基准服务
/// </summary>
public class MusicTimeManager : MonoBehaviour
{
    [Header("音乐播放设置")]
    public AudioClip musicClip;
    public float musicStartOffset = 0f;
    public float musicVolume = 0.8f;
    public float musicStartDelay = 1.0f;

    [Header("运行时状态")]
    public bool isPlaying = false;
    public bool isInitialized = false;

    // 时间管理
    private double dspStartTime;
    private double audioStartTime;
    private double musicStartTime;
    private AudioSource musicSource;
    private bool hasMusicStarted = false;

    /// <summary>
    /// 组件初始化 - 由SystemManager调用
    /// 替代原来的Awake和Start逻辑
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.LogWarning("[MusicTimeManager] 重复初始化，跳过");
            return;
        }

        InitializeAudioSource();
        isInitialized = true;
        
        Debug.Log("[MusicTimeManager] 初始化完成");
    }

    void Update()
    {
        if (isPlaying && isInitialized)
        {
            CheckMusicPlayback();
        }
    }

    /// <summary>
    /// 配置音乐播放
    /// </summary>
    public void ConfigureMusic(AudioClip clip, float startOffset = 0f, float volume = 0.8f)
    {
        if (!isInitialized)
        {
            Debug.LogError("[MusicTimeManager] 尚未初始化，无法配置音乐");
            return;
        }

        musicClip = clip;
        musicStartOffset = startOffset;
        musicVolume = volume;
        
        // 更新AudioSource设置
        if (musicSource != null)
        {
            musicSource.clip = musicClip;
            musicSource.volume = musicVolume;
        }
        
        Debug.Log($"[MusicTimeManager] 音乐配置完成: {clip?.name ?? "null"}");
    }

    /// <summary>
    /// 开始播放
    /// </summary>
    public void StartPlaying(float customDelay = -1f)
    {
        if (!isInitialized)
        {
            Debug.LogError("[MusicTimeManager] 尚未初始化，无法开始播放");
            return;
        }

        if (isPlaying) 
        {
            Debug.LogWarning("[MusicTimeManager] 已在播放中");
            return;
        }

        dspStartTime = AudioSettings.dspTime;
        float actualDelay = customDelay >= 0 ? customDelay : musicStartDelay;

        musicStartTime = dspStartTime + actualDelay;
        audioStartTime = musicStartTime + musicStartOffset;

        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.volume = musicVolume;
            musicSource.PlayScheduled(musicStartTime);
            hasMusicStarted = false;
            
            Debug.Log($"[MusicTimeManager] 音乐计划播放时间: {musicStartTime:F3}");
        }
        else
        {
            Debug.LogWarning("[MusicTimeManager] AudioSource或AudioClip为空，仅启动时间计算");
        }

        isPlaying = true;
        Debug.Log("[MusicTimeManager] 开始播放");
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    public void StopPlaying()
    {
        if (!isPlaying)
        {
            return;
        }

        isPlaying = false;
        hasMusicStarted = false;
        
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
        
        Debug.Log("[MusicTimeManager] 停止播放");
    }

    /// <summary>
    /// 获取游戏时间（从音乐开始计算）
    /// </summary>
    public double GetGameTime()
    {
        if (!isPlaying || !isInitialized) return 0.0;
        
        double currentDspTime = AudioSettings.dspTime;
        return currentDspTime - musicStartTime + musicStartOffset;
    }

    /// <summary>
    /// 获取音乐时间
    /// </summary>
    public double GetMusicTime()
    {
        if (!isPlaying || !isInitialized) return 0.0;
        
        double currentDspTime = AudioSettings.dspTime;
        return currentDspTime - musicStartTime + musicStartOffset;
    }

    /// <summary>
    /// 获取判定基准时间
    /// </summary>
    public double GetJudgementTime()
    {
        double result = GetMusicTime();
        
        // 添加调试验证（不过于频繁输出）
        if (Application.isPlaying && isPlaying && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[MusicTimeManager] JudgementTime: {result:F3}s (DSP: {AudioSettings.dspTime:F3}s, Start: {musicStartTime:F3}s)");
        }
        
        return result;
    }

    /// <summary>
    /// 检查是否正在播放
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying && isInitialized;
    }

    /// <summary>
    /// 获取音乐播放状态
    /// </summary>
    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    /// <summary>
    /// 初始化音频源
    /// </summary>
    void InitializeAudioSource()
    {
        // 获取或创建AudioSource
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 配置AudioSource
        musicSource.playOnAwake = false;
        musicSource.loop = false;
        musicSource.volume = musicVolume;
        
        if (musicClip != null)
        {
            musicSource.clip = musicClip;
        }
        
        Debug.Log("[MusicTimeManager] AudioSource初始化完成");
    }

    /// <summary>
    /// 检查音乐播放状态
    /// </summary>
    void CheckMusicPlayback()
    {
        if (!isInitialized) return;

        double currentDspTime = AudioSettings.dspTime;
        
        // 检查音乐是否应该开始播放
        if (!hasMusicStarted && currentDspTime >= musicStartTime)
        {
            hasMusicStarted = true;
            Debug.Log($"[MusicTimeManager] 音乐开始播放: {currentDspTime:F3}");
        }
    }
    

    /// <summary>
    /// 重置到初始状态
    /// </summary>
    public void Reset()
    {
        StopPlaying();
        dspStartTime = 0;
        audioStartTime = 0;
        musicStartTime = 0;
        hasMusicStarted = false;
        isPlaying = false;  // 新增：确保播放标志重置
        
        // 新增：重置AudioSource状态
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.time = 0f;
            Debug.Log("[MusicTimeManager] AudioSource状态已重置");
        }
        
        Debug.Log("[MusicTimeManager] 重置完成");
    }

    // 调试信息
    // void OnGUI()
    // {
    //     if (!Application.isPlaying || !isInitialized) return;
    //
    //     GUILayout.BeginArea(new Rect(10, 10, 300, 100));
    //     GUILayout.Label("=== MusicTimeManager 状态 ===");
    //     GUILayout.Label($"状态: {GetStatusInfo()}");
    //     GUILayout.Label($"DSP时间: {AudioSettings.dspTime:F3}");
    //     
    //     if (isPlaying)
    //     {
    //         GUILayout.Label($"游戏时间: {GetGameTime():F3}s");
    //         GUILayout.Label($"音乐时间: {GetMusicTime():F3}s");
    //     }
    //     
    //     GUILayout.EndArea();
    // }
}