#include <Wire.h>
#include "Keyboard.h"
#include "Mouse.h"

// Define this if you want serial debug messages, comment it out outherwise
#define DEBUG_MODE_ON

// Define Slave I2C Address
#define SLAVE_ADDR 9

// Define Slave answer size
#define ANSWERSIZE 5

void setup() 
{
  // Initialize I2C communications as Slave
  Wire.begin(SLAVE_ADDR);
  Keyboard.begin();
  Mouse.begin();

  // Function to run when data received from master
  Wire.onReceive(*receiveEvent);

  // Setup Serial Monitor
  SerialUSB.begin(115200);
  
  #ifdef DEBUG_MODE_ON
    SerialUSB.println("Init done...");
  #endif
}

void receiveEvent(int howmany) 
{
  String incomingData = "";
  char incomingChar = '0'; // Just init to something, it'll get overwritten

  // Read while data received
  while (Wire.available() && incomingChar != ';') 
  {
    incomingChar = (char)Wire.read();

    incomingData += incomingChar;
  }

  //incomingData += '';

  #ifdef DEBUG_MODE_ON
    SerialUSB.print("Incoming string over serial: '");
    SerialUSB.print(incomingData);
    SerialUSB.println("'");
  #endif

  if (incomingData[0] == 'k' &&
      incomingData[1] == ':')
  {
    sendKeyboardKey(parseKeyboardKey(incomingData), parseKeyboardKeyReleased(incomingData));
  }
  else if (incomingData[0] == 'm' &&
      incomingData[1] == ':')
  {
    int mouseMoveX = parseMouseMoveX(incomingData);
    int mouseMoveY = parseMouseMoveY(incomingData);

    moveMouseRelative(mouseMoveX, mouseMoveY);
  }
  else if (incomingData[0] == 'c' &&
           incomingData[1] == ':')
  {
    int convertedMouseButton = parseMouseButton(incomingData);
    bool convertedMouseAction = parseMouseAction(incomingData);

    sendMouseCommand(convertedMouseButton, convertedMouseAction);
  }
  
  #ifdef DEBUG_MODE_ON
    SerialUSB.println();
    SerialUSB.println();
    SerialUSB.print("0: ");
    SerialUSB.print(incomingData[0]);
    SerialUSB.print(", 1: ");
    SerialUSB.print(incomingData[1]);
    SerialUSB.println();  
  #endif

}

void loop() 
{
  delay(50);
}

void sendKeyboardKey(char keyToSend, bool released)
{
  #ifdef DEBUG_MODE_ON
    SerialUSB.print("Key found, sending: ");
    SerialUSB.println(keyToSend);
  #endif
  
  // if (!released)
  //   Keyboard.press(keyToSend);
  // else
  //   Keyboard.release(keyToSend);
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

int parseKeyboardKey(String stringToParse)
{
  int mIndex = stringToParse.indexOf("k:"); // Find the index of the "m:" substring
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
  int mIndex = stringToParse.indexOf(","); // Find the index of the "m:" substring

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