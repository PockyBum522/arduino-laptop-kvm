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

  if (incomingData[0] == 'm' &&
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
  else if (incomingData[0] == 'k' &&
           incomingData[1] == ':')
  {
    sendKeyboardKey(parseKeyboardKey(incomingData), parseKeyboardKeyReleased(incomingData));
  }
  else if (incomingData[0] == 's' &&
           incomingData[1] == ':')
  {
    sendKeyboardKeyWithNoAsciiCode(parseKeyboardKeyWithNoAsciiCode(incomingData), parseKeyboardKeyReleased(incomingData));
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
  delay(10);
}