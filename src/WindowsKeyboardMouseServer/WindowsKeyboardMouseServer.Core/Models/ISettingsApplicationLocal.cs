namespace WindowsKeyboardMouseServer.Core.Models;

/// <summary>
/// Settings for the application that are stored locally on end user's computer 
/// </summary>
public interface ISettingsApplicationLocal
{
    /// <summary>
    /// Store of the last selected COM port
    /// </summary>
    string LastSelectedComPort { get; set; }
}