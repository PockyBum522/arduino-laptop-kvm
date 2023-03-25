using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace WindowsKeyboardMouseServer.Core.Logic;

/// <summary>
/// Handles getting chars so we can send over serial from the key codes that we get from the button events
/// </summary>
public class KeyConverter
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Key mapping options
    /// </summary>
    public enum MapType : uint
    {
        /// <summary>
        /// uCode parameter is a virtual-key code and is translated into a scan code.
        /// If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned.
        /// </summary>
        MAPVK_VK_TO_VSC = 0x0,
        
        /// <summary>
        /// uCode parameter is a scan code and is translated into a virtual-key code that does not distinguish between left- and right-hand keys
        /// </summary>
        MAPVK_VSC_TO_VK = 0x1,
        
        /// <summary>
        /// uCode parameter is a virtual-key code and is translated into an unshifted character value in the low order word of the return value.
        /// Dead keys (diacritics) are indicated by setting the top bit of the return value.
        /// </summary>
        MAPVK_VK_TO_CHAR = 0x2,
        
        /// <summary>
        /// uCode parameter is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand keys
        /// </summary>
        MAPVK_VSC_TO_VK_EX = 0x3,
    }
    // ReSharper restore InconsistentNaming

    [DllImport( "user32.dll" )]
    private static extern int ToUnicode(
        uint wVirtualKey,
        uint wScanCode,
        byte[] lpKeyState,
        [Out, MarshalAs( UnmanagedType.LPWStr, SizeParamIndex = 4 )] 
        StringBuilder returnBuffer,
        int cchBuff,
        uint wFlags );

    [DllImport( "user32.dll" )]
    private static extern bool GetKeyboardState( byte[] lpKeyState );

    [DllImport( "user32.dll" )]
    private static extern uint MapVirtualKey( uint uCode, MapType uMapType );

    /// <summary>
    /// Converts a key to the equivalent char
    /// </summary>
    /// <param name="key">Key to convert</param>
    /// <returns>Char equivalent to passed key</returns>
    public int GetAsciiCodeFromKey( Key key )
    {
        var ch = ' ';
       
        var virtualKey = KeyInterop.VirtualKeyFromKey( key );
        var keyboardState = new byte[256];
        GetKeyboardState( keyboardState );
        
        var scanCode = MapVirtualKey( (uint)virtualKey, MapType.MAPVK_VK_TO_VSC );
        var stringBuilder = new StringBuilder( 2 );

        var result = ToUnicode( (uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0 );
        switch ( result )
        {
            case -1:
                break;
            case 0:
                break;
            case 1:
            {
                ch = stringBuilder[0];
                break;
            }
            default:
            {
                ch = stringBuilder[0];
                break;
            }
        }
        
        return (int)ch;
    }
    
    /// <summary>
    /// Converts a key to the equivalent char
    /// </summary>
    /// <param name="key">Key to convert</param>
    /// <returns>Char equivalent to passed key</returns>
    public char GetCharFromKey( Key key )
    {
        var ch = ' ';

        var virtualKey = KeyInterop.VirtualKeyFromKey( key );
        var keyboardState = new byte[256];
        GetKeyboardState( keyboardState );

        var scanCode = MapVirtualKey( (uint)virtualKey, MapType.MAPVK_VK_TO_VSC );
        var stringBuilder = new StringBuilder( 2 );

        var result = ToUnicode( (uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0 );
        switch ( result )
        {
            case -1:
                break;
            case 0:
                break;
            case 1:
            {
                ch = stringBuilder[0];
                break;
            }
            default:
            {
                ch = stringBuilder[0];
                break;
            }
        }
        return ch;
    }
}