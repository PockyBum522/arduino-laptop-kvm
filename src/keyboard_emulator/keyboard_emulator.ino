#include <Wire.h>
#include "Keyboard.h"
#include "Mouse.h"

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
  SerialUSB.println("I2C Slave Demonstration");
}

void receiveEvent(int howmany) 
{
  String incomingData = "";
  char incomingChar = '\n';

  // Read while data received
  while (Wire.available() && incomingChar != ';') 
  {
    incomingChar = (char)Wire.read();

    incomingData += incomingChar;
  }

  incomingData += '0';

  if (incomingData[1] == 'k' &&
      incomingData[2] == ':')
  {
    sendKeyboardKey(incomingData[3]);
  }
  else if (incomingData[1] == 'm' &&
      incomingData[2] == ':')
  {
    int mouseMoveX = parseMouseMoveX(incomingData);
    int mouseMoveY = parseMouseMoveY(incomingData);

    moveMouseRelative(mouseMoveX, mouseMoveY);
  }
  else if (incomingData[1] == 'c' &&
           incomingData[2] == ':')
  {
    int convertedMouseButton = parseMouseCommand(incomingData);

    sendMouseCommand(convertedMouseButton);
  }
  
  // SerialUSB.println();
  // SerialUSB.println();
  // SerialUSB.print("0: ");
  // SerialUSB.print(incomingData[1]);
  // SerialUSB.print(", 1: ");
  // SerialUSB.print(incomingData[2]);
  // SerialUSB.println();

}

void loop() 
{
  // Time delay in loop
  delay(50);
}

// class Mouse_
// {
//   public:
//     Mouse_(void);
//     void begin(void);
//     void end(void);
//     void click(uint8_t b = MOUSE_LEFT);
//     void move(signed char x, signed char y, signed char wheel = 0); 
//     void press(uint8_t b = MOUSE_LEFT);   // press LEFT by default
//     void release(uint8_t b = MOUSE_LEFT); // release LEFT by default
//     bool isPressed(uint8_t b = MOUSE_LEFT); // check LEFT by default
// };

// extern Mouse_ Mouse;

void sendKeyboardKey(char keyToSend)
{
  // Emulate keyboard key here
  //SerialUSB.print("Key found, sending: ");
  //SerialUSB.println(incomingData[3]);

  Keyboard.write(keyToSend);
}

void moveMouseRelative(int mouseMoveX, int mouseMoveY)
{
  Mouse.move((signed char)mouseMoveX, (signed char)mouseMoveY);

  // SerialUSB.print("MX: ");
  // SerialUSB.print(mouseMoveX);
  // SerialUSB.print(", MY: ");
  // SerialUSB.print(mouseMoveY);
  // SerialUSB.println();
  // SerialUSB.println();
}

void sendMouseCommand(int buttonToSend)
{
  switch (buttonToSend) 
  {  
    case 0:
      Mouse.click();
      break;

    case 2:
      Mouse.press(2);
      delay(50);
      Mouse.release(2);
      delay(50);

    default:
      break;
  }
}

int parseMouseMoveX(String stringToParse)
{
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

int parseMouseCommand(String stringToParse)
{
  int mIndex = stringToParse.indexOf("c:"); // Find the index of the "m:" substring

  String mValueStr = stringToParse.substring(mIndex + 2, stringToParse.length()); // Extract the substring containing the value
  
  int mValue = mValueStr.toInt(); // Convert the substring to an integer
  
  return mValue;  
}