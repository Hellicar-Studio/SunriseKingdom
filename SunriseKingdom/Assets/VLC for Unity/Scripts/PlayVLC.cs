/**************************************************************************************
*  
*   VLC Mediaplayer Integration Script for UNITY - Version 1.06
*   (c) 2016 Christian Holzer
*   
*   Thanks a lot for purchasing, I really hope this asset is useful to you. If you have any questions
*   or feature requests, you can contact me either per mail or you can post in this forum thread below.
*   
*   Contact: chunityassets@gmail.com
*   Unity Forum Thread: http://forum.unity3d.com/threads/vlc-player-for-unity.387372/
*
*   1.06 - FIXES AND FEATURES:

    - Added warning and removed errors if VideoInBackground feature was used in Editor
    - More Control functions (experimental): Seek to Second, Next Video, Previous Video, Toggle Loop
    - You can now add your own VLC Command line options for unlimited possibilities
    - Access Video Statistics: Current Time, Length, Volume level, loop
    - various small fixes or improvements
*
*
**************************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using UnityEngine.UI;
using Application = UnityEngine.Application;
using Color = System.Drawing.Color;
using Debug = UnityEngine.Debug;
using Screen = UnityEngine.Screen;


public class PlayVLC : MonoBehaviour {
   
    #region PublicVariables

    [Header("Use built-in VLC Player in StreamingAssets")]
    [Tooltip("Want to use built-in VLC from StreamingAssets? This way your player must not have VLC player installed.")]
    public bool UseBuiltInVLC = true;

    [Header("Use installed VLC Player")]
    [Tooltip("If you don't want to bundle VLC with your app, but use the installed VLC Player on your users PC. Recommended VLC version is 2.0.8. Smaller Build: Delete vlc from StreamingAssets in Build!")]
    public string InstallPath = @"C:\Program Files\VideoLAN\VLC\vlc.exe";

    [Header("Play from Streaming Assets")]
    [Tooltip("Want to play from StreamingAssets? Use this option if you package the video with the game.")]
    public bool PlayFromStreamingAssets = true;
    [Tooltip("Path or name with extension relative from StreamingAssets folder, eg. test.mp4")]
    public string StreamingAssetsVideoFilename = "";
    public string[] StreamingAssetsVideoPlaylistItems;

    [Header("Alternative: External video Path")] [Tooltip("Where is the video you want to play? Nearly all video formats are supported by VLC")]
    public string[] VideoPaths;
    public InputField ExternalField;

    [Header("SRT Subtitles")]
    public bool UseSubtitles = false;
    public string StreamingAssetsSubtitlePath;

    [Header(" Display Modes - Direct3D recommended")]
    public RenderMode UsedRenderMode;
    public enum RenderMode {
        Direct3DMode =0,
        VLC_QT_InterfaceFullscreen=1,
        FullScreenOverlayModePrimaryDisplay=2
    }
  
    [Header("Playback Options")]

    [Tooltip("Use as Intro?")]
    public bool PlayOnStart = false;
    [Tooltip("Video will loop, make sure to enable skipping or call Kill.")]
    public bool LoopVideo = false;
    [Tooltip("Skip Video with any key. Forces Unity to remain the focused window.")]
    public bool SkipVideoWithAnyKey = true;
    [Tooltip("Call Play, Pause, Stop etc. fuctions from code or gui buttons. Only possible for 1 video at a time.")]
    public bool EnableVlcControls = false;
    [Tooltip("If enabled, video will be fullscreen even if Unity is windowed. If disabled, video will be shown over the whole unity window when playing it fullscreen.")]
    public bool CompleteFullscreen = false;

    [Header("Audio Options")]
    [Tooltip("When loading a video, just play the sound. Useful for Youtube music videos, for example.")]
    public bool AudioOnly = false;
    [Tooltip("Shows only the video without sound.")]
    public bool NoAudio = false;

    [Header("VLC Command Line Parameters to Use")]
    [Tooltip("For unlimited possibilities, you can put VLC command line parameters seperated by blank spaces here. See the VLC help for all available commands: https://wiki.videolan.org/VLC_command-line_help/")]
    public string ExtraCommandLineParameters = "";

    [Header("Windowed playback")]
    [Tooltip("Render \"windowed\" video on GUI RectTransform?.")]
    public bool UseGUIVideoPanelPosition = false;
    public RectTransform GuiVideoPanel;
    [Header("Skip Video Hint")]
    [Tooltip("Show a skip hint under the video.")]
    public bool ShowBottomSkipHint = false;
    public GameObject BottomSkipHint;

    [Header("Video in Background (Experimental)")]
    [Tooltip("If enabled, fullsceen video will be played under the rendered unity window. 3D Objects and UI will remain visible. Uses keying, modify VideoInBackgroundCameraPrefab prefab for a different color key, if there are any problems.")]
    public bool VideoInBackground = false;
    [Tooltip("Drag the Camera Prefab that comes with this package here, or create your own keying Camera.")]
    public GameObject VideoInBackgroundCameraPrefab;

    [Header("New features in 1.03 (Using VLC 2.2.1: Youtube streaming)")]
    [Tooltip("Obsolete if you check PinVideo. Otherwise, if you use a higher version than 2.0.8, you have to check this box - Otherwise there might be a problem introduced by a unfixed bug in VLC releases since 2.1.0.")]
    public bool UseVlc210OrHigher = true;
    [Tooltip("Pin the video to the UI panel or Unity window. You can then scale or move the UI elements dynamically, and the video will do the same and handle aspect automatically.")]
    public bool PinVideo=true;


    [Header("NEW: Control Requests - Additional Controls and Functionality, use different ports for more instances of PlayVLC (Experimental)")]
    [Tooltip("Use new control functionality - experimental")]
    public bool EnableControlRequests = true;
    //public string CustomHTTPRequestURL = "";
    [Tooltip("Specify custom port, use different ports for multiple videos at once!")]
    public string VLCControlPort = "8080";
    private string userName = "";
    private string userPassword = "vlcHttp";
    
    [HideInInspector]
    public Text Debtext;
    [Header("Debug - Only change when neccessary")]   

    [Tooltip("This setting fixes the sometimes experieced green line bug that VLC has on certain AMD cards.")] public
    bool NoHardwareYUVConversion = false;
    [Tooltip("Interval that the video information is updated. Only if new Control Requests are enabled.")]
    public float VideoInfoUpdateInterval = 0.5f;
    public bool DisableHighDpiCompatibilityMode = false;
    [Tooltip("It is not recommended to disable this.")]
    public bool FlickerFix = true;
    private bool _focusInUpdate = false;
    public string Tex2DStreamPort = "1234";
    //--------------------------------------------------------
    private int nameSeed;



    #endregion PublicVariables

    #region PrivateVariables

    private Process _vlc;
    private IntPtr _unityHwnd;
    private IntPtr _vlcHwnd;

    private RECT _unityWindowRect;
    private RECT _vlcWindowRect;

    private uint _unityWindowID = 0;

    private float _mainMonitorWidth = 0;
    private Vector2 _realCurrentMonitorDeskopResolution;
    private Rect _realCurrentMonitorBounds;
    //private bool _pinToGuiRectDistanceTaken = false;

    private int _pinToGuiRectLeftOffset;
    private int _pinToGuiRectTopOffset;

    [HideInInspector]
    public bool _isPlaying = false;  
    public bool IsPlaying{
        get { return _isPlaying; }
        set { _isPlaying = value; }
    }

    private bool _thisVlcProcessWasEnded = false;
    private bool _qtCheckEnabled = false;

    private Camera[] allCameras;
    private GameObject VideoInBackgroundCamera;

    

    private float _nXpos; 
    private float _nYpos ;
    private float _nWidth;
    private float _nHeight;

   // private Rect _oldPrect;

    private float _prev_nXpos;
    private float _prev_nYpos;
    private float _prev_nWidth;
    private float _prev_nHeight;
    private float highdpiscale;

    private PlayVLC[] videos;
    private float bottomSkipHintSize;


    //----------New in 1.04:------------------------------------
    private bool TextureStream = false;
    private bool TranscodeTextureStream = true;

    private int _currentAudioLevel = 100;
    private int _desiredAudioLevel;

    private bool VideoInBackground_CheckAllowed=false;

    //Video Info from HTTPRequests to VLC for current video 
    private int _VideoInfo_CurrentTimeSeconds;
    private int _VideoInfo_DurationSeconds;
    private float _VideoInfo_PositionPercentage;
    private int _VideoInfo_CurrentVolume;
    private float _VideoInfo_PlayBackRate;
    private bool _VideoInfo_IsLooping;
    private bool _VideoInfo_IsRandomPlayback;
    
    //more additional Info coming soon
    //private string _VideoInfo_VideoCodec;
    //private string _VideoInfo_AudioCodec;


    #endregion PrivateVariables

    #region dll Import

    [DllImport("user32.dll")]public static extern IntPtr FindWindow(string className, string windowName);
    [DllImport("user32.dll")]internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")]static extern uint GetActiveWindow();
    [DllImport("user32.dll")]private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
    internal static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    [DllImport("User32.dll")]private static extern IntPtr MonitorFromPoint([In]Point pt, [In]uint dwFlags);
    [DllImport("Shcore.dll")]private static extern IntPtr GetDpiForMonitor([In]IntPtr hmonitor, [In]DpiType dpiType, [Out]out uint dpiX, [Out]out uint dpiY);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT{
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("gdi32.dll")]static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
    public enum DeviceCap{
        VERTRES = 10,
        DESKTOPVERTRES = 117,
    }

    public enum DpiType{
        Effective = 0,
        Angular = 1,
        Raw = 2,
    }

    [DllImport("user32.dll")]static extern IntPtr GetDC(IntPtr hWnd);

    #endregion



    private float GetMainSceenUserScalingFactor(){
#if !UNITY_EDITOR
  System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
  IntPtr desktop = g.GetHdc();
  int logicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
  int physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
  float screenScalingFactor = (float)physicalScreenHeight / (float)logicalScreenHeight;
#else
        float screenScalingFactor = 1;
#endif
        return screenScalingFactor;
    }
    
    private bool UnityIsOnPrimaryScreen() {
        _unityWindowRect = new RECT();
        GetWindowRect(_unityHwnd, ref _unityWindowRect);

        if (_unityWindowRect.Left + 10 < 0 || _unityWindowRect.Left + 10 > _mainMonitorWidth) {
            return false;
        }else {
            return true;
        }
    }

    private void UpdateUnityWindowRect() {
        _unityWindowRect = new RECT();
        GetWindowRect(_unityHwnd, ref _unityWindowRect);
    }

    public void TextureStreamDontTranscode() {
        TranscodeTextureStream = false;
    }

    public void TextureStreamDoTranscode() {
        TranscodeTextureStream = true;
    }

    public void EnableTextureStream() {
        TextureStream = true;
    }


    private Vector2 GetCurrentMonitorDesktopResolution() {
        Vector2 v;
        Form f = new Form();
        f.BackColor = Color.Black;
        f.ForeColor = Color.Black;
        f.ShowInTaskbar = false;
        f.Opacity = 0.0f;
        f.Show();
        f.StartPosition= FormStartPosition.Manual;
         UpdateUnityWindowRect();
        f.Location = new Point(_unityWindowRect.Left, _unityWindowRect.Top);

        f.WindowState = FormWindowState.Maximized;

        float SF = GetMainSceenUserScalingFactor();

        if (UnityIsOnPrimaryScreen() && GetMainSceenUserScalingFactor() != 1) {
            v = new Vector2((f.DesktopBounds.Width - 16)*SF, (f.DesktopBounds.Height + 40 - 16)*SF);
            _realCurrentMonitorBounds = new Rect((f.DesktopBounds.Left + 8)*SF, (f.DesktopBounds.Top)*SF,
                (f.DesktopBounds.Width - 16)*SF, (f.DesktopBounds.Height + 40)*SF);
        }else {
            v = new Vector2(f.DesktopBounds.Width - 16, f.DesktopBounds.Height + 40 - 16);
            _realCurrentMonitorBounds = new Rect((f.DesktopBounds.Left + 8), (f.DesktopBounds.Top),
                (f.DesktopBounds.Width - 16), (f.DesktopBounds.Height + 40));
        }

        f.Close();
        _realCurrentMonitorDeskopResolution = v;
        return v;
    }

    private void CheckErrors() {

        //TODO: when playing YT videos, check if connected to the web first: else show warning https://stackoverflow.com/questions/2031824/what-is-the-best-way-to-check-for-internet-connectivity-using-net

       /* if (VideoPath.Length > 5 && VideoPath.StartsWith("https://www.youtube.com/watch?") && !PlayFromStreamingAssets){
            Debug.LogWarning("You are streaming from youtube, make sure you've got a internet connection. Seeking might be less performant, depending on your internet speed.");
        } */

        if (StreamingAssetsVideoFilename.Length < 1 && PlayFromStreamingAssets && !File.Exists(Application.dataPath.Replace("/", "\\") + "\\StreamingAssets\\" +StreamingAssetsVideoFilename))
        {
            Debug.LogError("Please enter a valid video file name!");
        }
      /*  if (VideoPath.Length < 1 && !PlayFromStreamingAssets && !ExternalField.gameObject.activeSelf) {
            Debug.LogError("Please enter a valid video file name!");
        } */
        if ((!VideoInBackground && LoopVideo && (CompleteFullscreen || !UseGUIVideoPanelPosition) && !SkipVideoWithAnyKey &&
            !ShowBottomSkipHint) || (UsedRenderMode==RenderMode.FullScreenOverlayModePrimaryDisplay &&  !SkipVideoWithAnyKey)) {
            Debug.LogWarning("You are possibly playing a looping fullscreen video you can't skip! Consider using skipping features, or your players won't be able to get past this video.");
        }
        if (UseGUIVideoPanelPosition && !GuiVideoPanel) {
            Debug.LogError("If you want to play on a Gui Panel, get the one from the prefabs folder and assign it to this script.");
        }
        if (ShowBottomSkipHint && !BottomSkipHint){
            Debug.LogError("If you want to show the prefab skip hint, place the prefab in your GUI and assign it to this script.");
        }
        if (UsedRenderMode != RenderMode.Direct3DMode) {
            Debug.LogWarning("Please consider using Direct3D Mode. Other modes are experimental or less performant.");
        }
        if (!UseBuiltInVLC) {
            Debug.LogWarning("Consider using built-in VLC, unless you know you'll have it installed on your target machine.");
        }

        if(UseSubtitles && !File.Exists(Application.dataPath.Replace("/", "\\") + "\\StreamingAssets\\" + StreamingAssetsSubtitlePath)) {
            Debug.LogError("Subtitle not found. Did you enter the correct path to the subtitle file?");
        }

        if (SkipVideoWithAnyKey)
        {
            EnableVlcControls = false;
            EnableControlRequests = false;
            Debug.LogWarning("[SkipVideoWithAnyKey] is checked on PlayVLC-Instance on GameObject \""+this.gameObject.name+"\", disabling [EnableVlcControls] and [EnableHTTPRequests]");
        }

        if (CompleteFullscreen) {
            UseGUIVideoPanelPosition = false;
            Debug.LogWarning("[CompleteFullscreen] is checked on PlayVLC-Instance on GameObject \"" + this.gameObject.name + "\", disabling [UseGUIVideoPanelPosition]");
        }
        
    }

    void Awake() {
        nameSeed = (int)(UnityEngine.Random.value*1000000);

        CheckErrors();
        _mainMonitorWidth = SystemInformation.PrimaryMonitorMaximizedWindowSize.Width * GetMainSceenUserScalingFactor();
        _unityHwnd = (IntPtr)GetActiveWindow();
        _unityWindowRect = new RECT();
        GetWindowRect(_unityHwnd, ref _unityWindowRect);
        _realCurrentMonitorDeskopResolution = GetCurrentMonitorDesktopResolution();
        _unityWindowID = GetActiveWindow();
      }

    void Start() {

        videos = GameObject.FindObjectsOfType<PlayVLC>();

        if (PlayOnStart) {
            Play();
        }
    }

    public static Rect RectTransformToScreenSpace(RectTransform transform) {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
    }

    private uint GetCurrentMonitorDPI() {
        var monitor = MonitorFromPoint(new Point(_unityWindowRect.Left + 15, _unityWindowRect.Top), 2);
        uint CurrentMonitorDPI_X, CurrentMonitorDPI_Y;
        GetDpiForMonitor(monitor, DpiType.Raw, out CurrentMonitorDPI_X, out CurrentMonitorDPI_Y);
        return CurrentMonitorDPI_X;
    }

    public void QuitAllVideos(){
     if(videos!=null && videos.Length>0)   {
            foreach (PlayVLC video in videos){
                if(video.IsPlaying)
                    video.StopVideo();
            }
        }
    }

#region Controls

    private string GetShortCutCodes() {
        string p = "";
        if (EnableVlcControls) {            
            p += " --global-key-play-pause \"p\" ";        
            p += " --global-key-jump+short \"4\" ";
            p += " --global-key-jump+medium \"5\" ";
            p += " --global-key-jump+long \"6\" ";
            p += " --global-key-jump-long \"1\" ";
            p += " --global-key-jump-medium \"2\" ";
            p += " --global-key-jump-short \"3\" ";
            p += " --global-key-vol-down \"7\" ";
            p += " --global-key-vol-up \"8\" ";
            p += " --global-key-vol-mute \"9\" ";
        }
        return p;
    }

    /// <summary>
    /// Incease Volume. Depreciated, use Control Requests to VLC --> SetVolumeLevel
    /// </summary>
    /// <param name="newAudioLevel"></param>
    public void VolumeUp()
    {
        if (_isPlaying && EnableVlcControls)
        {
            keybd_event((byte)0x38, 0x89, 0x1 | 0, 0);
            keybd_event((byte)0x38, 0x89, 0x1 | 0x2, 0);
            _currentAudioLevel += 5;
        }
    }

    /// <summary>
    /// Decrease Volume. Depreciated, use Control Requests to VLC --> SetVolumeLevel
    /// </summary>
    /// <param name="newAudioLevel"></param>
    public void VolumeDown()
    {
        if (_isPlaying && EnableVlcControls)
        {
            keybd_event((byte)0x37, 0x88, 0x1 | 0, 0);
            keybd_event((byte)0x37, 0x88, 0x1 | 0x2, 0);
            _currentAudioLevel -= 5;
        }
    }

    /// <summary>
    /// Set volume to a value. Depreciated, use Control Requests to VLC --> SetVolumeLevel
    /// </summary>
    /// <param name="newAudioLevel"></param>
    public void SetVolumeTo(int newAudioLevel)
    {
        _desiredAudioLevel = newAudioLevel;

        int num = Mathf.Abs(_desiredAudioLevel - _currentAudioLevel) / 5;
        for (int i = 0; i < num; i++)
        {
            if (_currentAudioLevel > _desiredAudioLevel)
            {
                VolumeDown();
            }
            else if (_currentAudioLevel < _desiredAudioLevel)
            {
                VolumeUp();
            }
        }
    }
    /// <summary>
    /// Toggle Mute
    /// </summary>
    public void ToggleMute()
    {
        if (_isPlaying && EnableVlcControls)
        {
            keybd_event((byte)0x39, 0x8A, 0x1 | 0, 0);
            keybd_event((byte)0x39, 0x8A, 0x1 | 0x2, 0);
        }
    }

    /// <summary>
    /// Pause or Resume the current video, you can use the new CR_TogglePlayPause()-function instead.
    /// </summary>
    public void Pause(){
    if (_isPlaying && EnableVlcControls) { 
        keybd_event((byte)0x50, 0x99, 0x1 | 0, 0);
        keybd_event((byte)0x50, 0x99, 0x1 | 0x2, 0);
       }
    }


    /// <summary>
    /// Seek forward for a short amount of time, you can use the new -function instead.
    /// </summary>
    public void SeekForwardShort() {
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x34, 0x85, 0x1 | 0, 0);
            keybd_event(0x34, 0x85, 0x1 | 0x2, 0);

        }
    }
    /// <summary>
    /// Seek forward for a medium amount of time, you can use the new -function instead.
    /// </summary>
    public void SeekForwardMedium(){
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x35, 0x86, 0x1 | 0, 0);
            keybd_event(0x35, 0x86, 0x1 | 0x2, 0);
        }
    }
    /// <summary>
    /// Seek forward for a longer time, you can use the new -function instead.
    /// </summary>
    public void SeekForwardLong(){
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x36, 0x87, 0x1 | 0, 0);
            keybd_event(0x36, 0x87, 0x1 | 0x2, 0);
        }
    }
    /// <summary>
    /// Seek backward for a short amount of time, you can use the new -function instead.
    /// </summary>
    public void SeekBackwardShort(){
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x33, 0x84, 0x1 | 0, 0);
            keybd_event(0x33, 0x84, 0x1 | 0x2, 0);
        }
    }
    /// <summary>
    /// Seek backward for a medium amount of time, you can use the new -function instead.
    /// </summary>
    public void SeekBackwardMedium(){
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x32, 0x83, 0x1 | 0, 0);
            keybd_event(0x32, 0x83, 0x1 | 0x2, 0);
        }
    }
    /// <summary>
    /// Seek forward for a longer time, you can use the new -function instead.
    /// </summary>
    public void SeekBackwardLong(){
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x31, 0x82, 0x1 | 0, 0);
            keybd_event(0x31, 0x82, 0x1 | 0x2, 0);
        }
    }

#endregion

#region CONTROL REQUESTS 
    /*NEW CONTROL AND INFO FUNCTIONS: Control multiple videos at the same time with these new and improved control functions! */

        /// <summary>
        /// Print Video Information to console
        /// </summary>
    public void CR_PrintVideoInfo() {
        if (EnableControlRequests) {
            PrintSavedVideoInfoToConsole();
        }else {
            Debug.LogWarning("Please enable Control Requests");
        }
    }

    private void UpdateVideoInformation() {
        if (IsPlaying && VLCWindowIsRendered()){
            var request = WebRequest.Create("http://localhost:" + VLCControlPort + "/requests/status.xml");
            string authInfo = userName + ":" + userPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
            request.BeginGetResponse(new AsyncCallback(VideoDataFinishRequest), request);
        }
    }

    void VideoDataFinishRequest(IAsyncResult result)
    {
        using (HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse)
        {
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string xmlString = sr.ReadToEnd();
            sr.Close();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            _VideoInfo_CurrentTimeSeconds = Convert.ToInt16(xmlDoc["root"]["time"].InnerText);
            _VideoInfo_DurationSeconds = Convert.ToInt16(xmlDoc["root"]["length"].InnerText);
            _VideoInfo_CurrentVolume = Convert.ToInt16(xmlDoc["root"]["volume"].InnerText);
            _VideoInfo_PlayBackRate = Convert.ToSingle(xmlDoc["root"]["rate"].InnerText);
            _VideoInfo_PositionPercentage = Convert.ToSingle(xmlDoc["root"]["position"].InnerText);
            _VideoInfo_IsLooping = Convert.ToBoolean(xmlDoc["root"]["loop"].InnerText);
            _VideoInfo_IsRandomPlayback = Convert.ToBoolean(xmlDoc["root"]["random"].InnerText);
        }
    }

    private void PrintSavedVideoInfoToConsole() {
        if (IsPlaying) {
            print("Current Time in Seconds : " + _VideoInfo_CurrentTimeSeconds);
            print("Duration in Seconds: " + _VideoInfo_DurationSeconds);
            print("Current Volume Level: " + _VideoInfo_CurrentVolume);
            print("Current Playback Rate: " + _VideoInfo_PlayBackRate);
            print("Current Position Percentage in Timeline: " + _VideoInfo_PositionPercentage);
            print("Is Looping: " + _VideoInfo_IsLooping);
            print("Is Random Playback: " + _VideoInfo_IsRandomPlayback);
        }
        else {
            Debug.LogWarning("Play a video to access the video info. Command requests must be activated in the settings.");
        }
        
    }

    /// <summary>
    /// Get current time in seconds.
    /// </summary>
    /// <returns>current time in seconds ,returns -1 if not playing</returns>
    public int CR_GetCurrentTime() {
        if (IsPlaying) {
            return _VideoInfo_CurrentTimeSeconds;
        }else {
            return -1;
        }
    }
    /// <summary>
    /// Get length of the current video in seconds.
    /// </summary>
    /// <returns> video length in seconds, returns -1 if not playing</returns>
    public int CR_GetVideoDuration(){
      if (IsPlaying){
            return _VideoInfo_CurrentTimeSeconds;
        }else{
            return -1;
        }
    }
    /// <summary>
    /// Get current volume level
    /// </summary>
    /// <returns>current volume level, returns -1 if not playing</returns>
    public int CR_GetCurrentVolumeLevel() {
        if (IsPlaying){
            return _VideoInfo_CurrentTimeSeconds;
        }else{
            return -1;
        }
    }
    /// <summary>
    /// Returns playback rate
    /// </summary>
    /// <returns>Playback multiplier, returns -1 if not playing</returns>
    public float CR_GetCurrentPlaybackRate() {
        if (IsPlaying) {
            return _VideoInfo_CurrentTimeSeconds;
        }else {
            return -1;
        }
    }

    /// <summary>
    /// Returns the current position in the timeline
    /// </summary>
    /// <returns> current position in the timeline from 0 to 1, returns -1 if not playing</returns>
    public float CR_GetCurrentVideoSeekPosition() {
        if (IsPlaying){
            return _VideoInfo_PositionPercentage;
        }else{
            return -1;
        }
    }

    /// <summary>
    /// Is the video looping?
    /// </summary>
    /// <returns></returns>
    public bool CR_IsVideoLooping() {

        if (IsPlaying) {
            return _VideoInfo_IsLooping;
        }

        return false;
    }

    /// <summary>
    /// Is the video playlist set random playback?
    /// </summary>
    /// <returns></returns>
    public bool CR_IsRandomPlayback(){
        if (IsPlaying){
            return _VideoInfo_IsRandomPlayback;
        }
        return false;
    }

    /// <summary>
    /// Toggle Pause / Play
    /// </summary>
    public void CR_TogglePlayPause() {
        if (EnableControlRequests){
            DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=pl_pause");
        }else{
            Debug.LogWarning("Please enable Control Requests for CR_TogglePlayPause()");
        }
    }

    /// <summary>
    /// Jump to a specific second in the timeline of the video
    /// </summary>
    /// <param name="second">Second to jump to</param>
    public void CR_JumpToSecond(int second) {
        if (EnableControlRequests) {
            DoVLCCommandWebRequest("http://localhost:"+ VLCControlPort+"/requests/status.xml?command=seek&val=" + second);
        }else {
            Debug.LogWarning("Please enable Control Requests for CR_JumpToSecond(int second)");
        }
    }

    /// <summary>
    /// Jump forward or backward for a specific amount of time
    /// </summary>
    /// <param name="second">Number of seconds to go forward (+ positive int) or backward (- negative int)</param>
    public void CR_SeekAdditive(int second){
        if (EnableControlRequests) {
            DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=seek&val=" +((second >= 0) ? "+" : "-") + second);
        }else {
            Debug.LogWarning(
                "Please enable Control Requests for CR_SeekAdditive(int second)");
        }
    }

    /// <summary>
    /// When having multiple videos, jump to the next video in the list 
    /// </summary>
    public void CR_NextVideoInList() {
        if (EnableControlRequests){
            DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=pl_next");
        }else{
            Debug.LogWarning("Please enable Control Requests for CR_NextVideoInList()");
        }
    }
    /// <summary>
    /// When having multiple videos, jump to the previous video in the list 
    /// </summary>
    public void CR_PreviousVideoInList(){
        if (EnableControlRequests){
            DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=pl_previous");
        }else{
            Debug.LogWarning("Please enable Control Requests for CR_PreviousVideoInList()");
        }
    }

    /// <summary>
    /// Set the value for the playback speed 
    /// </summary>
    /// <param name="multi">Playback speed multiplier</param>
    public void CR_SetPlayBackspeedMultiplier(float multi) {
        if (EnableControlRequests && multi >0){
            DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=rate&val=" + multi);
        }else{
            Debug.LogWarning("Please enable Control Requests for CR_SetPlayBackspeedMultiplier(float multi), multiplier supplied must be > 0");
        }
    }

    /// <summary>
    /// Set Volume in %
    /// </summary>
    /// <param name="percent">new audio volume in percent</param>
    public void SetVolumeLevelPercent(int percent) {
        if (EnableControlRequests) {

            print("http://localhost:" + VLCControlPort + "/requests/status.xml?command=volume&val=" + percent + "%");
            DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=volume&val=" + percent + "%");
        }
        else {
            Debug.LogWarning("Please enable Control Requests for SetVolumeLevelPercent(int percent)");
        }
    }

    /// <summary>
    /// Set the absolute audio level
    /// </summary>
    /// <param name="val">new audio level, 255 is standard</param>
    public void CR_SetVolumeLevelAbsolute(int val){
        if (EnableControlRequests) {
            DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=volume&val=" + val);
        }else {
            Debug.LogWarning("Please enable Control Requests for CR_SetVolumeLevelAbsolute(int val)");
        }
    }

    /// <summary>
    /// Add or remove a value to/from audio level
    /// </summary>
    /// <param name="val">If positive int, increase volume. If negative, decrease volume.</param>
    public void CR_SetVolumeAdditive(int val)
    {
        if (EnableControlRequests){
                DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=volume&val="+((val>=0)?"+":"-")+ val );
        }else{
            Debug.LogWarning("Please enable Control Requests for CR_SetVolumeAdditive(int val)");
        }
    }

    /// <summary>
    /// Toggle loop for this instance
    /// </summary>
    public void CR_ToggleLoopWhilePlaying(){
        if (EnableControlRequests ){
            DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=pl_loop");
        }else{
            Debug.LogWarning("Please enable Control Requests for CR_ToggleLoopWhilePlaying()");
        }
    }

    /// <summary>
    /// Toggle random playback for the videos, if using a multiple videos
    /// </summary>
    public void CR_ToggleRandomPlayback(){
        if (EnableControlRequests){
            DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=pl_random");
        }else{
            Debug.LogWarning("Please enable Control Requests for CR_ToggleRandomPlayback()");
        }
    }

    //TODO: There seems to be a bug with the current vlc version in this regard, investigate!
    /// <summary>
    /// Add a video that is in streaming assets. Make sure it is really there before calling this function
    /// </summary>
    /// <param name="path">filepath to the media file, starting from Streaming Assets folder</param>
   /* public void CR_AddStreamingAssetsVideoToCurrentPlaylist(string path) {
        if (EnableControlRequests){
            string SAPath= "\"" + Application.dataPath.Replace("/", "\\") + "\\StreamingAssets\\" + path + "\"";
            DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=in_enqueue&input=" + SAPath);
        }else{
            Debug.LogWarning("Please enable Control Requests for  CR_AddStreamingAssetsVideoToCurrentPlaylist(string path)");
        }
    }*/

    //TODO: There seems to be a bug with the current vlc version in this regard, investigate!
    
    /// <summary>
    ///  Add a external video to the playlist
    /// </summary>
    /// <param name="URL">external url to a media file</param>
    /*public void CR_AddExternalVideoToCurrentPlaylist(string URL){
        if (EnableControlRequests){
            DoVLCCommandWebRequest("http://localhost:" + VLCControlPort + "/requests/status.xml?command=in_enqueue&input=" + URL);
        }else{
            Debug.LogWarning("Please enable Control Requests for CR_AddExternalVideoToCurrentPlaylist(string URL)");
        }
    }*/
    
    private void DoVLCCommandWebRequest(string url, bool async=true) {
        if (IsPlaying && EnableControlRequests && EnableVlcControls) {
            WebRequest request = WebRequest.Create(url);
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(userName + ":" + userPassword));

            if (async) {
                request.BeginGetResponse(new AsyncCallback(FinishWebRequest), request);
            }else {
                request.Timeout = 100;
                WebResponse wr = request.GetResponse();
                wr.Close();
            }
        }       
    }

    void FinishWebRequest(IAsyncResult result) {
        using (HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse) {
        }
    }

#endregion

    private bool CheckQTAllowed() {
        if (UsedRenderMode == RenderMode.VLC_QT_InterfaceFullscreen) {
#if !UNITY_EDITOR
            return Screen.fullScreen;
#else
            return true;
#endif
        }else {
            return true;
        }
    }

    private float GetHighDPIScale() {
        float highdpiscale = 1;

        if (!DisableHighDpiCompatibilityMode) {
            if (UnityIsOnPrimaryScreen() && GetMainSceenUserScalingFactor() > 1) {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
                    highdpiscale = GetMainSceenUserScalingFactor();
#endif
            }
        }

        return highdpiscale;
    }

    private Rect GetPanelRect() {
        float highdpiscale = GetHighDPIScale(); 
        
        Rect panel = RectTransformToScreenSpace(GuiVideoPanel);

        float leftOffset = 0;
        float topOffset = 0;

        if (!Screen.fullScreen) {
#if UNITY_EDITOR_WIN
                leftOffset = 7;
                topOffset = 47;
#endif

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
                leftOffset = 3; 
                topOffset = 20; 
#endif
        }

        float fullScreenResolutionModifierX = 1;
        float fullScreenResolutionModifierY = 1;
        float blackBorderOffsetX = 0;

        if (Screen.fullScreen) {
            fullScreenResolutionModifierX = _realCurrentMonitorDeskopResolution.x/(float) Screen.currentResolution.width;
            fullScreenResolutionModifierY = _realCurrentMonitorDeskopResolution.y/(float) Screen.currentResolution.height;

            float aspectMonitor = _realCurrentMonitorDeskopResolution.x/_realCurrentMonitorDeskopResolution.y;
            float aspectUnity = (float) Screen.currentResolution.width/(float) Screen.currentResolution.height;
            blackBorderOffsetX = (_realCurrentMonitorDeskopResolution.x - ((aspectUnity/aspectMonitor)*_realCurrentMonitorDeskopResolution.x))/2;
        }

        float aspectOffsetX = (_realCurrentMonitorDeskopResolution.x - blackBorderOffsetX*2)/_realCurrentMonitorDeskopResolution.x;
        float left = panel.xMin*fullScreenResolutionModifierX*aspectOffsetX + _unityWindowRect.Left + leftOffset;
        float top = panel.yMin*fullScreenResolutionModifierY + _unityWindowRect.Top + topOffset;

        _nXpos = ((blackBorderOffsetX + left*highdpiscale));
        _nYpos = top*highdpiscale;
        _nWidth = (panel.width*fullScreenResolutionModifierX*aspectOffsetX)*highdpiscale;
        _nHeight = panel.height*highdpiscale*fullScreenResolutionModifierY;
        return new Rect(_nXpos, _nYpos, _nWidth, _nHeight);
    }

    private Rect GetFullscreenRect() {
        return new Rect();
    }

    private Rect GetCompleteFullscreenRect(){
        return new Rect();
    }

    public void Play() {
        _currentAudioLevel = 100;

        bool qtPlayAllowed = CheckQTAllowed();

        if (!_isPlaying && qtPlayAllowed) {

            if (EnableControlRequests) {
                InvokeRepeating("UpdateVideoInformation",1f, 1f);
            }
             
              //  QuitAllVideos();
            _realCurrentMonitorDeskopResolution = GetCurrentMonitorDesktopResolution();

            _isPlaying = true;
            _thisVlcProcessWasEnded = false;
            if (GuiVideoPanel != null) {
                //  GuiVideoPanel.GetComponent<UnityEngine.UI.Image>().enabled = false;
            }

            //------------------------------FILE--------------------------------------------------

            string usedVideoPath = "";

            if (PlayFromStreamingAssets) {
                if (StreamingAssetsVideoFilename.Length > 0) {
                    usedVideoPath = "\"" + Application.dataPath.Replace("/", "\\") + "\\StreamingAssets\\" +
                                    StreamingAssetsVideoFilename + "\"";

                    if (StreamingAssetsVideoPlaylistItems.Length > 0) {
                        for (int i = 0; i < StreamingAssetsVideoPlaylistItems.Length; i++) {
                            string s = StreamingAssetsVideoPlaylistItems[i];
                            usedVideoPath += " \"" + Application.dataPath.Replace("/", "\\") + "\\StreamingAssets\\" +
                                s + "\" ";
                        }
                    }
                }else {
                    Debug.LogError("ERROR: No StreamingAssets video path(s) set.");
                }
            }
            else {

                if (ExternalField != null && ExternalField.text.Length > 0)                {
                    //use external field
                    usedVideoPath = "\"" + ExternalField.text + "\"";
                    print(usedVideoPath + " will be used.");
                }
                else if (VideoPaths.Length > 0)
                {
                    //use string array
                    string s = "";

                    foreach (string videoPath in VideoPaths)
                    {
                        if (videoPath.Length > 0)
                        {
                            s += "\"" + videoPath + "\" ";
                        }
                    }
                    usedVideoPath = s;
                }
                else
                {
                    Debug.LogError("ERROR: No URL video path(s) set.");
                }

            }

            if (UseSubtitles){
                usedVideoPath += " --sub-file \"" + Application.dataPath.Replace("/", "\\") + "\\StreamingAssets\\" + StreamingAssetsSubtitlePath + "\" ";
            }

            string _path = usedVideoPath + " --ignore-config --no-crashdump " + GetShortCutCodes();

            //print(usedVideoPath);

            if (!TextureStream)
            {
                //------------------------------DIRECT3D--------------------------------------------------

                if (UsedRenderMode == RenderMode.Direct3DMode) {
                _unityWindowRect = new RECT();
                GetWindowRect(_unityHwnd, ref _unityWindowRect);

                int width = Mathf.Abs(_unityWindowRect.Right - _unityWindowRect.Left);
                int height = Mathf.Abs(_unityWindowRect.Bottom - _unityWindowRect.Top);

                highdpiscale = GetHighDPIScale();

                _path += @" -I=dummy --no-mouse-events --no-interact --no-video-deco "; //

                if (!VideoInBackground) {
                    _path += @" --video-on-top ";
                }

                //--------------------------- ON UI----------------------------------------------------------


                if (UseGUIVideoPanelPosition && GuiVideoPanel) {
                    Rect pRect = GetPanelRect();

                    if (!UseVlc210OrHigher) {
                        _path += @" --video-x=" + pRect.xMin + " --video-y=" + pRect.yMin + " --width=" +
                                 (pRect.xMax - pRect.xMin) + " --height=" + (pRect.yMax - pRect.yMin) + " ";
                    }
                    else {
                        _path += @" --video-x=" + 6000 + " --video-y=" + 6000;

                        //   _path += @" --video-x=" + pRect.left + " --video-y=" + pRect.top + " --width=" +
                        //      (pRect.xMax - pRect.xMin) + " --height=" + (pRect.yMax - pRect.yMin) + " ";

                    }
                }
                else {
                    //--------------------------- NORMAL FS-----------------------------------------

                    float bottomSkipHintSize = 0;

                    if (ShowBottomSkipHint) {
                        BottomSkipHint.SetActive(true);

                        //Add click to skip hint when not skipping with any button
                        BottomSkipHint.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(KillVLCProcess);

                        if (UnityIsOnPrimaryScreen() && GetMainSceenUserScalingFactor() > 1) {
                            bottomSkipHintSize =
                                RectTransformToScreenSpace(BottomSkipHint.GetComponent<RectTransform>()).height*
                                GetMainSceenUserScalingFactor();
                        }
                        else {
                            bottomSkipHintSize =
                                RectTransformToScreenSpace(BottomSkipHint.GetComponent<RectTransform>()).height;
                        }
#if UNITY_EDITOR_WIN
                        bottomSkipHintSize =
                            RectTransformToScreenSpace(BottomSkipHint.GetComponent<RectTransform>()).height;
#endif
                    }

                    if (_unityWindowRect.Top == 0) {
                        _unityWindowRect.Top = -1;
                        height += 2;
                    }
                    if (_unityWindowRect.Left == 0) {
                        _unityWindowRect.Left = -1;
                        width += 2;
                    }

                    //--------------------------- COMPLETE FS-----------------------------------------
                    if (CompleteFullscreen) {

                        GetCurrentMonitorDesktopResolution();

                        _path += @" --video-x=" + _realCurrentMonitorBounds.xMin + " --video-y=" +
                                 _realCurrentMonitorBounds.yMin + " --width=" + _realCurrentMonitorBounds.width +
                                 " --height=" + (_realCurrentMonitorBounds.height + 4) + " ";
                        //  print(_realCurrentMonitorBounds);

                    }
                    else {
                        if (Screen.fullScreen) {
                            _path += @" --video-x=" + _unityWindowRect.Left*highdpiscale + " --video-y=" +
                                     (_unityWindowRect.Top - 1)*highdpiscale + " --width=" + width*highdpiscale +
                                     " --height=" + (height - bottomSkipHintSize)*highdpiscale + " ";
                        }
                        else {
                            float leftOffset = 7;
#if !UNITY_EDITOR_WIN && UNITY_STANDALONE_WIN
                            leftOffset = 3;
#endif
                            _path += @" --video-x=" + (_unityWindowRect.Left + leftOffset)*highdpiscale + " --video-y=" +
                                     (_unityWindowRect.Top - 1)*highdpiscale + " --width=" +
                                     (width - leftOffset*2)*highdpiscale +
                                     " --height=" + (height - bottomSkipHintSize)*highdpiscale + " ";
                        }
                    }
                }
            }

            //------------------------------END DIRECT3D--------------------------------------------------

            if (UsedRenderMode == RenderMode.FullScreenOverlayModePrimaryDisplay ||
                UsedRenderMode == RenderMode.VLC_QT_InterfaceFullscreen) {
                _path += @"--fullscreen ";
                if (UsedRenderMode == RenderMode.FullScreenOverlayModePrimaryDisplay) {
                    _path += @" -I=dummy ";
                }
                else {
                    //QT
                    _path += @" --no-qt-privacy-ask --no-interact ";

                    int val = PlayerPrefs.GetInt("UnitySelectMonitor"); //0=left 1=right

#if UNITY_EDITOR_WIN
                    val = 0;
#endif
                    if (val == 1 && UnityIsOnPrimaryScreen())
                        _path += " --qt-fullscreen-screennumber=0";
                    if (val == 0 && !UnityIsOnPrimaryScreen())
                        _path += " --qt-fullscreen-screennumber=1";
                    if (val == 1 && !UnityIsOnPrimaryScreen())
                        _path += " --qt-fullscreen-screennumber=1";
                    if (val == 0 && UnityIsOnPrimaryScreen())
                        _path += " --qt-fullscreen-screennumber=0";
                }
            }
            else {
                _path += @" --no-qt-privacy-ask --qt-minimal-view ";
            }

            //--------------------------------------------------------------------------------

            _path += " --play-and-exit --no-keyboard-events --video-title-timeout=0 --no-interact --video-title="+ nameSeed + "  ";

            if (!LoopVideo /*&& !VideoInBackground*/) {
                _path += " --no-repeat --no-loop";
            }
            else {
                _path += "  --loop --repeat";
            }

                if (AudioOnly) {
                    _path += " --no-video ";
                }
                if (NoAudio) {
                    _path += " --no-audio ";
                }
                if (NoHardwareYUVConversion) {
                    _path += " --no-directx-hw-yuv ";
                }

                if (ExtraCommandLineParameters.Length>0) {
                    _path += " "+@ExtraCommandLineParameters+" ";
                }

                if (EnableControlRequests) {
                    _path += " --extraintf=http --http-password=vlcHttp - --http-port=" + VLCControlPort+" ";
                }



            }
            else if (TextureStream){
                
                if (TranscodeTextureStream) {
                    _path += @" -I=dummy :sout-transcode-threads=4 :sout=#transcode{vcodec=h264,acodec=mpga,ab=128,channels=2,samplerate=44100}:http{mux=ffmpeg{mux=flv},dst=:"+ Tex2DStreamPort+"/stream} :sout-keep ";

                }
                else {
                 _path += @" -I=dummy :sout=#http{mux=ffmpeg{mux=flv},dst=:" + Tex2DStreamPort + "/stream} :sout-keep ";
                }


              

            }
                        

            //----------------------------VLC PROCESS -------------------- 
            _vlc = new Process();
        
            if (UseBuiltInVLC) {
                _vlc.StartInfo.FileName = Application.dataPath + @"/StreamingAssets/vlc/vlc.exe";
            }else {
                _vlc.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            }

         // print(_path);
            
            _vlc.StartInfo.Arguments = @_path;
            _vlc.StartInfo.CreateNoWindow = true;
            _vlc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; 
            _vlc.Start();

            if (!TextureStream) {

                if (UsedRenderMode == RenderMode.VLC_QT_InterfaceFullscreen) {
                    //InvokeRepeating("FocusUnity", 3f, 1f);
                }
                else {

                    //New in 1.01
                    if (FlickerFix) {
                        _focusInUpdate = true;
                    }
                    else {
                        InvokeRepeating("FocusUnity", 0.025f, .05f);
                    }
                }

                if (VideoInBackground) {
                    StartCoroutine("HandleBackgroundVideo");
                }
            }
           
        }
    }

   private IEnumerator HandleBackgroundVideo() {

        yield return new WaitForSeconds(3);   
        allCameras = FindObjectsOfType<Camera>();
        foreach (Camera c in allCameras){
            c.gameObject.SetActive(false);
        }
        Debtext.text += "Starting BG video \n";
        VideoInBackgroundCamera = Instantiate(VideoInBackgroundCameraPrefab);
        VideoInBackgroundCamera.GetComponent<BackgroundKey>().ActivateTransparency(_unityHwnd);
        yield return new WaitForSeconds(1);
        VideoInBackground_CheckAllowed = true;
   }

    private void ResetBackgroundVideo() {
#if !UNITY_EDITOR
         if (VideoInBackgroundCamera != null){
            VideoInBackgroundCamera.GetComponent<BackgroundKey>().DisableTransparency();
            Destroy(VideoInBackgroundCamera);
        }

        foreach (Camera c in allCameras) {
            if (c != VideoInBackgroundCamera) {
                c.gameObject.SetActive(true);
            }
        }
#else
        Debug.LogWarning("PLEASE BUILD THE SCENE FOR THE VIDEO IN BACKGROUND FEATURE, OTHERWISE IT WILL NOT WORK!");
#endif
    }
   
    void FocusUnity(){
        GetFocus();
    }

    public void StopVideo() {
        if (VideoInBackground) {

            VideoInBackground_CheckAllowed = false;
            ResetBackgroundVideo();
        }
        if(_isPlaying)
        KillVLCProcess();
    }

    private void KillVLCProcess(){
        try {
            _vlc.Kill();
        }
        catch (Exception) {}
    }

    private bool VLCWindowIsRendered() {
        GetWindowRect(_vlcHwnd, ref _vlcWindowRect);
        return ((_vlcWindowRect.Top - _vlcWindowRect.Bottom) != 0);
    }

    private void Pin() {
       
        if (_isPlaying && (PinVideo || UseVlc210OrHigher)) { 
           
           // if (_vlcHwnd == IntPtr.Zero ){ //Hwnd changes with next playlist item apparently
               _vlcHwnd = FindWindow(null, nameSeed.ToString());
          //  }

            GetWindowRect(_vlcHwnd, ref _vlcWindowRect); 

          if(VLCWindowIsRendered())  {
                if (UseGUIVideoPanelPosition && GuiVideoPanel){ 

                    Rect pRect = GetPanelRect();
                    //TODO: This doesnt get called on a fullscreen switch while playing videos, since windowpos did not change apparently
                    if ( Math.Abs(_vlcWindowRect.Top- (int)pRect.yMin) > 3 || Math.Abs(_vlcWindowRect.Bottom - (int)pRect.yMax) > 3 || Math.Abs(_vlcWindowRect.Left - (int)pRect.xMin) > 3 || Math.Abs(_vlcWindowRect.Right - (int)pRect.xMax) > 3) {  //TODO FERTIG MACHEN

                        MoveWindow(_vlcHwnd, (int)pRect.xMin, (int)pRect.yMin, (int)(pRect.xMax - pRect.xMin), (int)(pRect.yMax - pRect.yMin), true);
                    }
          
                }else if (CompleteFullscreen) {
                   
                        if (Math.Abs(_vlcWindowRect.Top - (int)_realCurrentMonitorBounds.yMin) > 3 ||Math.Abs(_vlcWindowRect.Bottom - (int)_realCurrentMonitorBounds.yMax) > 3 ||Math.Abs(_vlcWindowRect.Left - (int)_realCurrentMonitorBounds.xMin) > 3 ||Math.Abs(_vlcWindowRect.Right - (int)_realCurrentMonitorBounds.xMax) > 3){
                        MoveWindow(_vlcHwnd, (int)_realCurrentMonitorBounds.xMin, (int)_realCurrentMonitorBounds.yMin, (int)_realCurrentMonitorBounds.width, (int)_realCurrentMonitorBounds.height, true);
                    }
                }else {
                    if (ShowBottomSkipHint) {
                        if (UnityIsOnPrimaryScreen()) {
                            bottomSkipHintSize =RectTransformToScreenSpace(BottomSkipHint.GetComponent<RectTransform>()).height*GetMainSceenUserScalingFactor();
                        }else {
                            bottomSkipHintSize =RectTransformToScreenSpace(BottomSkipHint.GetComponent<RectTransform>()).height;
                        }
                    }
                    else {
                        bottomSkipHintSize = 0;
                    }
              
                    if (Math.Abs(_vlcWindowRect.Top - (int)_unityWindowRect.Top) > 3 ||Math.Abs(_vlcWindowRect.Bottom - ((int)_unityWindowRect.Bottom -(int)bottomSkipHintSize)) > 3 ||Math.Abs(_vlcWindowRect.Left - (int)_unityWindowRect.Left) > 3 ||Math.Abs(_vlcWindowRect.Right - (int)_unityWindowRect.Right) > 3) {
                        MoveWindow(_vlcHwnd, _unityWindowRect.Left, _unityWindowRect.Top,_unityWindowRect.Right - _unityWindowRect.Left,_unityWindowRect.Bottom - _unityWindowRect.Top - (int) bottomSkipHintSize, true);
                    }
                }
            }
        }
    }

    void LateUpdate() {
        Pin();
    }

    void Update() {

        if (_isPlaying) {

            if (_focusInUpdate /*&& FlickerFix*/ && UsedRenderMode == RenderMode.Direct3DMode)
                FocusUnity();

            try {
                if (_vlc.HasExited && !_thisVlcProcessWasEnded) {

                    ShowWindow(_unityHwnd, 1);

                    _thisVlcProcessWasEnded = true;
                    CancelInvoke("FocusUnity");

                    if(EnableControlRequests)
                        CancelInvoke("UpdateVideoInformation");

                    _isPlaying = _qtCheckEnabled = _focusInUpdate= false;

                    _vlcHwnd = IntPtr.Zero; 

                    if (BottomSkipHint) {
                        BottomSkipHint.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                        BottomSkipHint.SetActive(false);
                    }
                    if (GuiVideoPanel != null) {
                        GuiVideoPanel.GetComponent<UnityEngine.UI.Image>().enabled = true;
                    }

                    if (VLCWindowIsRendered() == false && VideoInBackground && VideoInBackground_CheckAllowed){
                        ResetBackgroundVideo();
                        Debtext.text += "VIDEO AUS - RESET \n";
                    }

                    VideoInBackground_CheckAllowed = false;
                }
            }
            catch (Exception) {
            }

            if (SkipVideoWithAnyKey) {
                if (_isPlaying) {
                    if ((!Input.GetKeyDown(KeyCode.LeftAlt) && !Input.GetKeyDown(KeyCode.RightAlt) && Input.anyKeyDown) ||
                        Input.GetKeyUp(KeyCode.Space)) {
                        KillVLCProcess();
                        ShowWindow(_unityHwnd, 5);
                    }
                }
            }
        }
    }


   private void QTCheckFullScreenEnd() {

        float SF = 1;

        if (UnityIsOnPrimaryScreen()) {
            SF = GetMainSceenUserScalingFactor();
        }
       
        SetForegroundWindow(_vlcHwnd);
        ShowWindow(_vlcHwnd, 5);
        
        RECT vlcSize = new RECT();
        GetWindowRect(_vlcHwnd, ref vlcSize);
     
          if ((vlcSize.Right-vlcSize.Left)* SF > 0 && (vlcSize.Right - vlcSize.Left) * SF != (int)GetCurrentMonitorDesktopResolution().x && (vlcSize.Bottom - vlcSize.Top) * SF > 0 && (vlcSize.Bottom-vlcSize.Top) * SF != GetCurrentMonitorDesktopResolution().y) {
              KillVLCProcess();
          }
    }


    private void GetFocus(){
       
        if (_unityWindowID != GetActiveWindow() && _isPlaying) {

            keybd_event((byte) 0xA4, 0x45, 0x1 | 0, 0);
            keybd_event((byte) 0xA4, 0x45, 0x1 | 0x2, 0);
            

              if ( UsedRenderMode == RenderMode.VLC_QT_InterfaceFullscreen && !_qtCheckEnabled) {
                   QTCheckFullScreenEnd();
                  _qtCheckEnabled = true;
                _vlcHwnd = FindWindow(null, nameSeed.ToString());
            }
            else {
                  if (!_qtCheckEnabled) {
                    SetForegroundWindow(_unityHwnd);
                    ShowWindow(_unityHwnd, 5);
                }
            }
        }

        if (_isPlaying && UsedRenderMode == RenderMode.VLC_QT_InterfaceFullscreen && _qtCheckEnabled){
            QTCheckFullScreenEnd();
        }
    }

    void OnApplicationQuit() {
        try {
            if(_isPlaying && !_thisVlcProcessWasEnded)
                _vlc.Kill();
        }
        catch (Exception) {}

    }
}

