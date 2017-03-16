/**************************************************************************************
*  
*   VLC Mediaplayer Integration Script for UNITY - Version 1.05
*   (c) 2016 Christian Holzer
*   
*   Thanks a lot for purchasing, I really hope this asset is useful to you. If you have any questions
*   or feature requests, you can contact me either per mail or you can post in this forum thread below.
*   
*   Contact: mail@christianholzer.com
*   Unity Forum Thread: http://forum.unity3d.com/threads/vlc-player-for-unity.387372/
*
*   1.05 - FIXES AND FEATURES:

    - Loop setting works in background video mode
	- Play multiple videos at once (fixed a glitch where this could lead to flickering)
	- Play multiple external videos / youtube in a playlist, just add paths seperated by blank space " " in the "VideoPath" field
	- Minor fixes
*
*
**************************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Application = UnityEngine.Application;
using Color = System.Drawing.Color;
using Debug = UnityEngine.Debug;
using Screen = UnityEngine.Screen;


public class PlayVLC : MonoBehaviour {
   // public Text Debtext;
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

    [Header("Alternative: External video Path")]
    [Tooltip("Where is the video you want to play? Nearly all video formats are supported by VLC")]
    public string VideoPath = "";

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

    [Header("Windowed playback")]
    [Tooltip("Render \"windowed\" video on GUI RectTransform?.")]
    public bool UseGUIVideoPanelPosition = false;
    public RectTransform GuiVideoPanel;
    [Header("Skip Video Hint")]
    [Tooltip("Show a skip hint under the video.")]
    public bool ShowBottomSkipHint = false;
    public GameObject BottomSkipHint;

    //----------New in 1.02b:------------------------------------
    [Header("Additional Settings")]
    [Tooltip("In case video start flickers slightly, use this.")]
    public bool FlickerFix = true;
    private bool _focusInUpdate = false;
   
    [Tooltip("If enabled, fullsceen video will be played under the rendered unity window. 3D Objects and UI will remain visible. Uses keying, modify VideoInBackgroundCameraPrefab prefab for a different color key, if there are any problems.")]
    public bool VideoInBackground = false;
    [Tooltip("Drag the Camera Prefab that comes with this package here, or create your own keying Camera.")]
    public GameObject VideoInBackgroundCameraPrefab;

    //----------New in 1.03:------------------------------------
    [Header("New features in 1.03 (Using VLC 2.2.1: Youtube streaming)")]
    [Tooltip("Obsolete if you check PinVideo. Otherwise, if you use a higher version than 2.0.8, you have to check this box - Otherwise there might be a problem introduced by a unfixed bug in VLC releases since 2.1.0.")]
    public bool UseVlc210OrHigher = true;
    [Tooltip("Pin the video to the UI panel or Unity window. You can then scale or move the UI elements dynamically, and the video will do the same and handle aspect automatically.")]
    public bool PinVideo=true;

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
    private bool _pinToGuiRectDistanceTaken = false;

    private int _pinToGuiRectLeftOffset;
    private int _pinToGuiRectTopOffset;

    public bool _isPlaying = false;
    [HideInInspector]
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

    private Rect _oldPrect;

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
        System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
        IntPtr desktop = g.GetHdc();
        int logicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
        int physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
        float screenScalingFactor = (float)physicalScreenHeight / (float)logicalScreenHeight;
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

       // print(this.gameObject.GetInstanceID());

        //TODO: when playing YT videos, check if connected to the web first: else show warning

        if (VideoPath.Length > 5 && VideoPath.StartsWith("https://www.youtube.com/watch?") && !PlayFromStreamingAssets){
            Debug.LogWarning("You are streaming from youtube, make sure you've got a internet connection. Seeking might be less performant, depending on your internet speed.");
        }

        if (StreamingAssetsVideoFilename.Length < 1 && PlayFromStreamingAssets && !File.Exists(Application.dataPath.Replace("/", "\\") + "\\StreamingAssets\\" +StreamingAssetsVideoFilename))
        {
            Debug.LogError("Please enter a valid video file name!");
        }
        if (VideoPath.Length < 1 && !PlayFromStreamingAssets) {
            Debug.LogError("Please enter a valid video file name!");
        }
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
            Debug.LogWarning("Please consider using Direct3D Mode. Other modes are experimental or less performant and will be updated with V 1.1 or later.");
        }
        if (!UseBuiltInVLC) {
            Debug.LogWarning("Consider using built-in VLC, unless you know you'll have it installed on your target machine.");
        }

        if(UseSubtitles && !File.Exists(Application.dataPath.Replace("/", "\\") + "\\StreamingAssets\\" + StreamingAssetsSubtitlePath)) {
            Debug.LogError("Subtitle not found. Did you enter the correct path to the subtitle file?");
        }

        
    }

    void Awake() {
        nameSeed = (int)(UnityEngine.Random.value*1000000);

        CheckErrors();
        _mainMonitorWidth = SystemInformation.PrimaryMonitorMaximizedWindowSize.Width * GetMainSceenUserScalingFactor();
        _unityHwnd = (IntPtr)GetActiveWindow();
        _unityWindowRect = new RECT();
        GetWindowRect(_unityHwnd, ref _unityWindowRect);
        //_realCurrentMonitorDeskopResolution = GetCurrentMonitorDesktopResolution();
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

    public void VolumeUp()
    {
        if (_isPlaying && EnableVlcControls)
        {
            keybd_event((byte)0x38, 0x89, 0x1 | 0, 0);
            keybd_event((byte)0x38, 0x89, 0x1 | 0x2, 0);
            _currentAudioLevel += 5;
        }
    }
    public void VolumeDown()
    {
        if (_isPlaying && EnableVlcControls)
        {
            keybd_event((byte)0x37, 0x88, 0x1 | 0, 0);
            keybd_event((byte)0x37, 0x88, 0x1 | 0x2, 0);
            _currentAudioLevel -= 5;
        }
    }

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

    public void ToggleMute()
    {
        if (_isPlaying && EnableVlcControls)
        {
            keybd_event((byte)0x39, 0x8A, 0x1 | 0, 0);
            keybd_event((byte)0x39, 0x8A, 0x1 | 0x2, 0);
        }
    }


    public void Pause(){
    if (_isPlaying && EnableVlcControls) { 
        keybd_event((byte)0x50, 0x99, 0x1 | 0, 0);
        keybd_event((byte)0x50, 0x99, 0x1 | 0x2, 0);
       }
    }



    public void SeekForwardShort() {
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x34, 0x85, 0x1 | 0, 0);
            keybd_event(0x34, 0x85, 0x1 | 0x2, 0);

        }
    }

    public void SeekForwardMedium(){
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x35, 0x86, 0x1 | 0, 0);
            keybd_event(0x35, 0x86, 0x1 | 0x2, 0);
        }
    }

    public void SeekForwardLong(){
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x36, 0x87, 0x1 | 0, 0);
            keybd_event(0x36, 0x87, 0x1 | 0x2, 0);
        }
    }
    public void SeekBackwardShort(){
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x33, 0x84, 0x1 | 0, 0);
            keybd_event(0x33, 0x84, 0x1 | 0x2, 0);
        }
    }
    public void SeekBackwardMedium(){
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x32, 0x83, 0x1 | 0, 0);
            keybd_event(0x32, 0x83, 0x1 | 0x2, 0);
        }
    }
    public void SeekBackwardLong(){
        if (_isPlaying && EnableVlcControls){
            keybd_event(0x31, 0x82, 0x1 | 0, 0);
            keybd_event(0x31, 0x82, 0x1 | 0x2, 0);
        }
    }

    public void OpenFile(string path) {
        //Coming in 1.1
    }

    public void FullscreenToggle(){
        MoveWindow(_vlcHwnd, (int)_realCurrentMonitorBounds.xMin, (int)_realCurrentMonitorBounds.yMin, (int)_realCurrentMonitorBounds.width,(int)_realCurrentMonitorBounds.height , true);
    }

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

        if (UnityIsOnPrimaryScreen() && GetMainSceenUserScalingFactor() > 1)
        {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
                    highdpiscale = GetMainSceenUserScalingFactor();
#endif
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

        //New in 1.03
        _nXpos = ((blackBorderOffsetX + left*highdpiscale));
        _nYpos = top*highdpiscale;
        _nWidth = (panel.width*fullScreenResolutionModifierX*aspectOffsetX)*highdpiscale;
        _nHeight = panel.height*highdpiscale*fullScreenResolutionModifierY;
       // print(_nXpos + "/" + _nYpos + "/" + _nWidth + "/" + _nHeight);
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
                }
                else {
                    Debug.LogError("ERROR: No StreamingAssets video path set.");
                }
            }
            else {
                if (VideoPath.Length > 0) {
                    usedVideoPath = VideoPath;
                }
                else {
                    Debug.LogError("ERROR: No video path set.");
                }
            }

            if (UseSubtitles){
                usedVideoPath += " --sub-file \"" + Application.dataPath.Replace("/", "\\") + "\\StreamingAssets\\" + StreamingAssetsSubtitlePath + "\" ";
            }

            string _path = usedVideoPath + " --ignore-config --no-crashdump " + GetShortCutCodes();

            

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


        }else if (TextureStream){
                
                if (TranscodeTextureStream) {
                    _path += @" -I=dummy :sout-transcode-threads=4 :sout=#transcode{vcodec=h264,acodec=mpga,ab=128,channels=2,samplerate=44100}:http{mux=ffmpeg{mux=flv},dst=:1234/stream} :sout-keep ";

                }
                else {
                 _path += @" -I=dummy :sout=#http{mux=ffmpeg{mux=flv},dst=:1234/stream} :sout-keep ";
                }


              

            }
                        

            //----------------------------VLC PROCESS -------------------- 
            _vlc = new Process();
        
            if (UseBuiltInVLC) {
                _vlc.StartInfo.FileName = Application.dataPath + @"/StreamingAssets/vlc/vlc.exe";
            }else {
                _vlc.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            }

           
            
            _vlc.StartInfo.Arguments = _path;
            _vlc.StartInfo.CreateNoWindow = true;
            _vlc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; //?
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
        VideoInBackgroundCamera = Instantiate(VideoInBackgroundCameraPrefab);
        VideoInBackgroundCamera.GetComponent<BackgroundKey>().ActivateTransparency(_unityHwnd);
    }

    private void ResetBackgroundVideo() {

        foreach (Camera c in allCameras) {
            if (c != VideoInBackgroundCamera) {
                c.gameObject.SetActive(true);
            }
        }

        if (VideoInBackgroundCamera != null)
        {
            VideoInBackgroundCamera.GetComponent<BackgroundKey>().DisableTransparency();
            Destroy(VideoInBackgroundCamera);
        }
    }

    void FocusUnity(){
        GetFocus();
    }

    public void StopVideo() {
        if (VideoInBackground) {
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
           
            if (_vlcHwnd == IntPtr.Zero){
                //_vlcHwnd = FindWindow(null, "VLC (Direct3D output)"); 
               _vlcHwnd = FindWindow(null, nameSeed.ToString());

            }

            GetWindowRect(_vlcHwnd, ref _vlcWindowRect); 

          if(VLCWindowIsRendered())  {
                if (UseGUIVideoPanelPosition && GuiVideoPanel){ 

                    Rect pRect = GetPanelRect();
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

            if (VLCWindowIsRendered() == false && VideoInBackground) {
                ResetBackgroundVideo();
                print("VIDEO AUS - RESET");
            }

            if (_focusInUpdate && FlickerFix && UsedRenderMode == RenderMode.Direct3DMode)
                FocusUnity();

            try {
                if (_vlc.HasExited && !_thisVlcProcessWasEnded) {

                    ShowWindow(_unityHwnd, 1);

                    _thisVlcProcessWasEnded = true;
                    _pinToGuiRectDistanceTaken = false;
                    CancelInvoke("FocusUnity");
                    _isPlaying = false;
                    _qtCheckEnabled = false;
                    _focusInUpdate = false;

                    _vlcHwnd = IntPtr.Zero; 
                    _oldPrect = new Rect(1, 1, 1, 1);

                    if (BottomSkipHint) {
                        BottomSkipHint.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                        BottomSkipHint.SetActive(false);
                    }
                    if (GuiVideoPanel != null) {
                        GuiVideoPanel.GetComponent<UnityEngine.UI.Image>().enabled = true;
                    }
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
                _vlcHwnd = FindWindow(null, StreamingAssetsVideoFilename + " - VLC media player");
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

