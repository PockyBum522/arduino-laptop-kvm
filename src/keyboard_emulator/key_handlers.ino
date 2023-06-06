void sendKeyboardKey(char keyToSend, bool released)
{
  #ifdef DEBUG_MODE_ON
    SerialUSB.print("Key found, sending: ");
    SerialUSB.println(keyToSend);
  #endif
  
  if (!released)
    Keyboard.press(keyToSend);
  else
    Keyboard.release(keyToSend);
}

void sendKeyboardKeyWithNoAsciiCode(int keyToSend, bool released)
{
  #ifdef DEBUG_MODE_ON
    SerialUSB.print("Key found, sending: ");
    SerialUSB.println(keyToSend);
  #endif
  
  // These are sent by SerialDataSender.cs
  switch(keyToSend)
  {
    case 112:
      keyToSend = KEY_F1;
      break;

    case 113:
      keyToSend = KEY_F2;
      break;

    case 114:
      keyToSend = KEY_F3;
      break;

    case 115:
      keyToSend = KEY_F4;
      break;

    case 116:
      keyToSend = KEY_F5;
      break;

    case 117:
      keyToSend = KEY_F6;
      break;

    case 118:
      keyToSend = KEY_F7;
      break;

    case 119:
      keyToSend = KEY_F8;
      break;

    case 120:
      keyToSend = KEY_F9;
      break;

    case 121:
      keyToSend = KEY_F10;
      break;

    case 122:
      keyToSend = KEY_F11;
      break;

    case 123:
      keyToSend = KEY_F12;
      break;

    case 20:
      keyToSend = KEY_CAPS_LOCK;
      break;

    case 162:
      keyToSend = KEY_LEFT_CTRL;
      break;

    case 164:
      keyToSend = KEY_LEFT_ALT;
      break;

    case 165:
      keyToSend = KEY_LEFT_ALT;
      break;

    case 160:
      keyToSend = KEY_LEFT_SHIFT;
      break;

    case 91:
      keyToSend = KEY_LEFT_GUI;
      break; 

    case 163:
      keyToSend = KEY_RIGHT_CTRL;
      break;

    case 161:
      keyToSend = KEY_RIGHT_SHIFT;
      break;

    case 32:
      keyToSend = KEY_RIGHT_ALT;
      break;

    case 38:
      keyToSend = KEY_UP_ARROW;
      break;

    case 39:
      keyToSend = KEY_RIGHT_ARROW;
      break;

    case 40:
      keyToSend = KEY_DOWN_ARROW;
      break;

    case 37:
      keyToSend = KEY_LEFT_ARROW;
      break;

    case 8:
      keyToSend = KEY_BACKSPACE;
      break;

    case 46:
      keyToSend = KEY_DELETE;
      break;
  
    case 9:
      keyToSend = KEY_TAB;
      break;
  
    case 13:
      keyToSend = KEY_RETURN;
      break;
    // case :
    //   keyToSend = ;
    //   break;

// #define KEY_TAB           0xB3
// #define KEY_RETURN        0xB0
// #define KEY_MENU          0xED // "Keyboard Application" in USB standard
// #define KEY_ESC           0xB1
// #define KEY_INSERT        0xD1
// #define         0xD4
// #define KEY_PAGE_UP       0xD3
// #define KEY_PAGE_DOWN     0xD6
// #define KEY_HOME          0xD2
// #define KEY_END           0xD5
// #define      0xC1
// #define KEY_PRINT_SCREEN  0xCE // Print Screen / SysRq
// #define KEY_SCROLL_LOCK   0xCF
// #define KEY_PAUSE         0xD0 // Pause / Break

// // Numeric keypad
// #define KEY_NUM_LOCK      0xDB
// #define KEY_KP_SLASH      0xDC
// #define KEY_KP_ASTERISK   0xDD
// #define KEY_KP_MINUS      0xDE
// #define KEY_KP_PLUS       0xDF
// #define KEY_KP_ENTER      0xE0
// #define KEY_KP_1          0xE1
// #define KEY_KP_2          0xE2
// #define KEY_KP_3          0xE3
// #define KEY_KP_4          0xE4
// #define KEY_KP_5          0xE5
// #define KEY_KP_6          0xE6
// #define KEY_KP_7          0xE7
// #define KEY_KP_8          0xE8
// #define KEY_KP_9          0xE9
// #define KEY_KP_0          0xEA
// #define KEY_KP_DOT        0xEB



  }

  if (!released)
    Keyboard.press(keyToSend);
  else
    Keyboard.release(keyToSend);
}

int parseKeyboardKey(String stringToParse)
{
  int mIndex = stringToParse.indexOf("k:"); // Find the index of the substring
  int cIndex = stringToParse.indexOf(","); 

  String mValueStr = stringToParse.substring(mIndex + 2, cIndex); // Extract the substring containing the value
  
  int mValue = mValueStr.toInt(); // Convert the substring to an integer
  
  #ifdef DEBUG_MODE_ON
    SerialUSB.print("Key code Raw: ");
    SerialUSB.print(mValueStr);
    SerialUSB.println();
  #endif

  return mValue;  
}

int parseKeyboardKeyWithNoAsciiCode(String stringToParse)
{
  int mIndex = stringToParse.indexOf("s:"); // Find the index of the substring
  int cIndex = stringToParse.indexOf(","); 

  String mValueStr = stringToParse.substring(mIndex + 2, cIndex); // Extract the substring containing the value
  
  int mValue = mValueStr.toInt(); // Convert the substring to an integer
  
  #ifdef DEBUG_MODE_ON
    SerialUSB.print("Key code Raw: ");
    SerialUSB.print(mValueStr);
    SerialUSB.println();
  #endif

  return mValue;  
}

bool parseKeyboardKeyReleased(String stringToParse)
{
  int mIndex = stringToParse.indexOf(","); 

  String mValueStr = stringToParse.substring(mIndex + 1, mIndex + 2); // Extract the substring containing the value
  
  int mValue = mValueStr.toInt(); // Convert the substring to an integer
  
  #ifdef DEBUG_MODE_ON
    SerialUSB.print("Key released: ");
    SerialUSB.print(mValue);
    SerialUSB.println();
    SerialUSB.print("RAW Key released: ");
    SerialUSB.print(mValueStr);
    SerialUSB.println();
  #endif

  if (mValue == 1)
    return true;
    
  return false;
}