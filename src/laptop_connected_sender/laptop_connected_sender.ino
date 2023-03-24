#include <Wire.h>

// Define Slave I2C Address
#define SLAVE_ADDR 9

// Define Slave answer size
#define ANSWERSIZE 5

String inputString = ""; // A string to store incoming serial data

void setup() 
{
  Serial.begin(115200);
  Wire.begin();

  pinMode(LED_BUILTIN, OUTPUT);

  Serial.println(F("Init done..."));
}

void loop() 
{
  checkForIncomingSerialData();

  checkForWireResponse();
}

void checkForIncomingSerialData()
{
  while (Serial.available()) 
  {
    char inChar = (char)Serial.read(); // Read a character from the serial buffer

    inputString += inChar; // Add the character to the input string

    if (inChar == ';') 
    {
      sendDataToKeyboardEmulatorBoard(inputString);
      
      inputString = ""; // Clear the input string for the next command

      return;
    }
  }
}

void sendDataToKeyboardEmulatorBoard(String dataToSend)
{
  // Write a character to the Slave
  Wire.beginTransmission(SLAVE_ADDR);
  Wire.print(dataToSend);
  Wire.endTransmission();
}

void checkForWireResponse()
{
  String response = "";
  
  while (Wire.available()) 
  {
    char b = Wire.read();
    response += b;
  }
}