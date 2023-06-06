int parseMouseAction(String stringToParse)
{
  int mIndex = stringToParse.indexOf(","); // Find the index of the "m:" substring

  String mValueStr = stringToParse.substring(mIndex + 1, stringToParse.length()); // Extract the substring containing the value
  
  int mValue = mValueStr.toInt(); // Convert the substring to an integer
  
  #ifdef DEBUG_MODE_ON
    SerialUSB.print("mValue action: ");
    SerialUSB.print(mValue);
    SerialUSB.println();
  #endif

  if (mValue == 0)
    return false;

  return true;  
}

void sendMouseCommand(int buttonToSend, bool released)
{
  if (released)
  {
    Mouse.release(buttonToSend);
  }
  else
  {
    Mouse.press(buttonToSend);
  }
}

int parseMouseMoveX(String stringToParse)
{
  #ifdef DEBUG_MODE_ON
    SerialUSB.print("X Parse Raw: ");
    SerialUSB.print(stringToParse);
    SerialUSB.println();
  #endif

  int mIndex = stringToParse.indexOf("m:"); // Find the index of the "m:" substring
  
  int endIndex = stringToParse.indexOf(",", mIndex); // Find the index of the comma following the "m:" substring
  
  if (endIndex == -1) 
  { // If the comma is not found, return 0
    return 0;
  }

  String mValueStr = stringToParse.substring(mIndex + 3, endIndex); // Extract the substring containing the value
  
  // SerialUSB.println(mValueStr);

  int mValue = mValueStr.toInt(); // Convert the substring to an integer
  
  // We're sending values as 1 - 99, 50 = no move. This is what handles that.
  mValue = mValue - 50;

  return mValue;  
}

int parseMouseMoveY(String stringToParse)
{
  int mIndex = stringToParse.indexOf("m:"); // Find the index of the "m:" substring
  
  int commaIndex = stringToParse.indexOf(",", mIndex); // Find the index of the comma following the "m:" substring
  
  if (commaIndex == -1) 
  { // If the comma is not found, return 0
    return 0;
  }

  String mValueStr = stringToParse.substring(commaIndex + 2, stringToParse.length()); // Extract the substring containing the value
  
  int mValue = mValueStr.toInt(); // Convert the substring to an integer
  
  // We're sending values as 1 - 99, 50 = no move. This is what handles that.
  mValue = mValue - 50;

  return mValue;  
}

int parseMouseButton(String stringToParse)
{
  int mIndex = stringToParse.indexOf("c:"); // Find the index of the "m:" substring

  String mValueStr = stringToParse.substring(mIndex + 2, stringToParse.length()); // Extract the substring containing the value
  
  int mValue = mValueStr.toInt(); // Convert the substring to an integer
  
  return mValue;  
}

void moveMouseRelative(int mouseMoveX, int mouseMoveY)
{
  Mouse.move((signed char)mouseMoveX, (signed char)mouseMoveY);

  #ifdef DEBUG_MODE_ON
    SerialUSB.print("MX: ");
    SerialUSB.print(mouseMoveX);
    SerialUSB.print(", MY: ");
    SerialUSB.print(mouseMoveY);
    SerialUSB.println();
    SerialUSB.println();
  #endif
}