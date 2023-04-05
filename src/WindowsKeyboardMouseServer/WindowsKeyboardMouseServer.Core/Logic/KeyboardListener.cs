using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace WindowsKeyboardMouseServer.Core.Logic;

/// <summary>
/// Listens keyboard globally.
/// 
/// <remarks>Uses WH_KEYBOARD_LL.</remarks>
/// </summary>
public class KeyboardListener : IDisposable
{
    /// <summary>
    /// Creates global keyboard listener.
    /// </summary>
    public KeyboardListener()
    {
        // Dispatcher thread handling the KeyDown/KeyUp events.
        _dispatcher = Dispatcher.CurrentDispatcher;

        // We have to store the LowLevelKeyboardProc, so that it is not garbage collected runtime
        var hookedLowLevelKeyboardProc = (InterceptKeys.LowLevelKeyboardProc)LowLevelKeyboardProc;

        // Set the hook
        _hookId = InterceptKeys.SetHook(hookedLowLevelKeyboardProc);

        // Assign the asynchronous callback event
        HookedKeyboardCallbackAsync = KeyboardListener_KeyboardCallbackAsync;
    }

    private readonly Dispatcher _dispatcher;

    /// <summary>
    /// Destroys global keyboard listener.
    /// </summary>
    ~KeyboardListener()
    {
        Dispose();
    }

    /// <summary>
    /// Fired when any of the keys is pressed down.
    /// </summary>
    public event RawKeyEventHandler? KeyDown;

    /// <summary>
    /// Fired when any of the keys is released.
    /// </summary>
    public event RawKeyEventHandler? KeyUp;

    #region Inner workings

    /// <summary>
    /// Hook ID
    /// </summary>
    private readonly nint _hookId = nint.Zero;

    /// <summary>
    /// True if should not pass keypress to Windows after intercepted with hook
    /// </summary>
    private bool _handleKeypress;
    
    /// <summary>
    /// Asynchronous callback hook.
    /// </summary>
    /// <param name="character">Character</param>
    /// <param name="keyEvent">Keyboard event</param>
    /// <param name="vkCode">VKCode</param>
    private delegate void KeyboardCallbackAsync(InterceptKeys.KeyEvent keyEvent, int vkCode, string character);

    /// <summary>
    /// Actual callback hook.
    /// 
    /// <remarks>Calls asynchronously the asyncCallback.</remarks>
    /// </summary>
    /// <param name="nCode"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private nint LowLevelKeyboardProc(int nCode, nuint wParam, nint lParam)
    {
        var chars = "";

        if (nCode >= 0)
            if (wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WmKeydown ||
                wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WmKeyup ||
                wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WmSyskeydown ||
                wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WmSyskeyup)
            {
                // Captures the character(s) pressed only on WM_KEYDOWN
                chars = InterceptKeys.VkCodeToString((uint)Marshal.ReadInt32(lParam), 
                    (wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WmKeydown ||
                     wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WmSyskeydown));
                
                HookedKeyboardCallbackAsync.Invoke((InterceptKeys.KeyEvent)wParam, Marshal.ReadInt32(lParam), chars);
            }

        if (_handleKeypress) return 1;
            
        return InterceptKeys.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    /// <summary>
    /// When this is called, will start handling keypresses so they don't get sent through to windows
    /// </summary>
    public void StartHandlingKeypresses()
    {
        _handleKeypress = true;
    }
    
    /// <summary>
    /// When this is called, will stop handling keypresses so they will get sent through to windows
    /// </summary>
    public void StopHandlingKeypresses()
    {
        _handleKeypress = false;
    }

    /// <summary>
    /// Event to be invoked asynchronously (BeginInvoke) each time key is pressed.
    /// </summary>
    private KeyboardCallbackAsync HookedKeyboardCallbackAsync { get; set; }

    /// <summary>
    /// HookCallbackAsync procedure that calls accordingly the KeyDown or KeyUp events.
    /// </summary>
    /// <param name="keyEvent">Keyboard event</param>
    /// <param name="vkCode">VKCode</param>
    /// <param name="character">Character as string.</param>
    // ReSharper disable once CognitiveComplexity
    private void KeyboardListener_KeyboardCallbackAsync(InterceptKeys.KeyEvent keyEvent, int vkCode, string character)
    {
        switch (keyEvent)
        {
            // KeyDown events
            case InterceptKeys.KeyEvent.WmKeydown:
                if (KeyDown != null)
                    _dispatcher.BeginInvoke(new RawKeyEventHandler(KeyDown), this, new RawKeyEventArgs(vkCode, false, character));
                break;
            case InterceptKeys.KeyEvent.WmSyskeydown:
                if (KeyDown != null)
                    _dispatcher.BeginInvoke(new RawKeyEventHandler(KeyDown), this, new RawKeyEventArgs(vkCode, true, character));
                break;

            // KeyUp events
            case InterceptKeys.KeyEvent.WmKeyup:
                if (KeyUp != null)
                    _dispatcher.BeginInvoke(new RawKeyEventHandler(KeyUp), this, new RawKeyEventArgs(vkCode, false, character));
                break;
            case InterceptKeys.KeyEvent.WmSyskeyup:
                if (KeyUp != null)
                    _dispatcher.BeginInvoke(new RawKeyEventHandler(KeyUp), this, new RawKeyEventArgs(vkCode, true, character));
                break;

            default:
                break;
        }
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Disposes the hook.
    /// <remarks>This call is required as it calls the UnhookWindowsHookEx.</remarks>
    /// </summary>
    public void Dispose()
    {
        InterceptKeys.UnhookWindowsHookEx(_hookId);
    }

    #endregion
}

/// <summary>
/// Raw KeyEvent arguments.
/// </summary>
public class RawKeyEventArgs : EventArgs
{
    /// <summary>
    /// VKCode of the key.
    /// </summary>
    public int VkCode;

    /// <summary>
    /// WPF Key of the key.
    /// </summary>
    public Key Key;

    /// <summary>
    /// Is the hitted key system key.
    /// </summary>
    public bool IsSysKey;

    /// <summary>
    /// Convert to string.
    /// </summary>
    /// <returns>Returns string representation of this key, if not possible empty string is returned.</returns>
    public override string ToString()
    {
        return Character;
    }

    /// <summary>
    /// Unicode character of key pressed.
    /// </summary>
    public string Character;

    /// <summary>
    /// Create raw keyevent arguments.
    /// </summary>
    /// <param name="vkCode"></param>
    /// <param name="isSysKey"></param>
    /// <param name="character">Character</param>
    public RawKeyEventArgs(int vkCode, bool isSysKey, string character)
    {
        this.VkCode = vkCode;
        this.IsSysKey = isSysKey;
        this.Character = character;
        this.Key = System.Windows.Input.KeyInterop.KeyFromVirtualKey(vkCode);
    }

}

/// <summary>
/// Raw keyevent handler.
/// </summary>
/// <param name="sender">sender</param>
/// <param name="args">raw keyevent arguments</param>
public delegate void RawKeyEventHandler(object sender, RawKeyEventArgs args);

/// <summary>
/// Winapi Key interception helper class.
/// </summary>
internal static class InterceptKeys
{
    public delegate nint LowLevelKeyboardProc(int nCode, nuint wParam, nint lParam);
    public static int WhKeyboardLl = 13;

    /// <summary>
    /// Key event
    /// </summary>
    public enum KeyEvent 
    {    
        /// <summary>
        /// Key down
        /// </summary>
        WmKeydown = 256,

        /// <summary>
        /// Key up
        /// </summary>
        WmKeyup = 257,

        /// <summary>
        /// System key up
        /// </summary>
        WmSyskeyup = 261,

        /// <summary>
        /// System key down
        /// </summary>
        WmSyskeydown = 260
    }

    public static nint SetHook(LowLevelKeyboardProc proc)
    {
        using (var curProcess = Process.GetCurrentProcess())
        using (var curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WhKeyboardLl, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint CallNextHookEx(nint hhk, int nCode, nuint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint GetModuleHandle(string lpModuleName);
 
    // Note: Sometimes single VKCode represents multiple chars, thus string. 
    // E.g. typing "^1" (notice that when pressing 1 the both characters appear, 
    // because of this behavior, "^" is called dead key)

    [DllImport("user32.dll")]
    private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags, nint dwhkl);

    [DllImport("user32.dll")]
    private static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, nint dwhkl);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern nint GetKeyboardLayout(uint dwLayout);

    [DllImport("User32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("User32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    private static uint _lastVkCode = 0;
    private static uint _lastScanCode = 0;
    private static byte[] _lastKeyState = new byte[255];
    private static bool _lastIsDead = false;

    /// <summary>
    /// Convert VKCode to Unicode.
    /// <remarks>isKeyDown is required for because of keyboard state inconsistencies!</remarks>
    /// </summary>
    /// <param name="vkCode">VKCode</param>
    /// <param name="isKeyDown">Is the key down event?</param>
    /// <returns>String representing single unicode character.</returns>
    public static string VkCodeToString(uint vkCode, bool isKeyDown)
    {
        // ToUnicodeEx needs StringBuilder, it populates that during execution.
        var sbString = new System.Text.StringBuilder(5);

        var bKeyState = new byte[255];
        bool bKeyStateStatus;
        var isDead = false;

        // Gets the current windows window handle, threadID, processID
        var currentHWnd = GetForegroundWindow();
        uint currentProcessId;
        var currentWindowThreadId = GetWindowThreadProcessId(currentHWnd, out currentProcessId);

        // This programs Thread ID
        var thisProgramThreadId = GetCurrentThreadId();

        // Attach to active thread so we can get that keyboard state
        if (AttachThreadInput(thisProgramThreadId, currentWindowThreadId , true))
        {
            // Current state of the modifiers in keyboard
            bKeyStateStatus = GetKeyboardState(bKeyState);

            // Detach
            AttachThreadInput(thisProgramThreadId, currentWindowThreadId, false);
        }
        else
        {
            // Could not attach, perhaps it is this process?
            bKeyStateStatus = GetKeyboardState(bKeyState);
        }

        // On failure we return empty string.
        if (!bKeyStateStatus)
            return "";

        // Gets the layout of keyboard
        var hkl = GetKeyboardLayout(currentWindowThreadId);

        // Maps the virtual keycode
        var lScanCode = MapVirtualKeyEx(vkCode, 0, hkl);

        // Keyboard state goes inconsistent if this is not in place. In other words, we need to call above commands in UP events also.
        if (!isKeyDown)
            return "";

        // Converts the VKCode to unicode
        var relevantKeyCountInBuffer = ToUnicodeEx(vkCode, lScanCode, bKeyState, sbString, sbString.Capacity, (uint)0, hkl);

        var ret = "";

        switch (relevantKeyCountInBuffer)
        {
            // Dead keys (^,`...)
            case -1:
                isDead = true;

                // We must clear the buffer because ToUnicodeEx messed it up, see below.
                ClearKeyboardBuffer(vkCode, lScanCode, hkl);
                break;

            case 0:
                break;

            // Single character in buffer
            case 1:
                ret = sbString[0].ToString();
                break;

            // Two or more (only two of them is relevant)
            case 2:
            default:
                ret = sbString.ToString().Substring(0, 2);
                break;
        }

        // We inject the last dead key back, since ToUnicodeEx removed it.
        // More about this peculiar behavior see e.g: 
        //   http://www.experts-exchange.com/Programming/System/Windows__Programming/Q_23453780.html
        //   http://blogs.msdn.com/michkap/archive/2005/01/19/355870.aspx
        //   http://blogs.msdn.com/michkap/archive/2007/10/27/5717859.aspx
        if (_lastVkCode != 0 && _lastIsDead)
        {
            var sbTemp = new System.Text.StringBuilder(5);
            ToUnicodeEx(_lastVkCode, _lastScanCode, _lastKeyState, sbTemp, sbTemp.Capacity, (uint)0, hkl);
            _lastVkCode = 0;

            return ret;
        }
        
        // Save these
        _lastScanCode = lScanCode;
        _lastVkCode = vkCode;
        _lastIsDead = isDead;
        _lastKeyState = (byte[])bKeyState.Clone();

        return ret;
    }

    private static void ClearKeyboardBuffer(uint vk, uint sc, nint hkl)
    {
        var sb = new System.Text.StringBuilder(10);
            
        int rc;
        do {
            var lpKeyStateNull = new byte[255];
            rc = ToUnicodeEx(vk, sc, lpKeyStateNull, sb, sb.Capacity, 0, hkl);
        } while(rc < 0);
    }
}