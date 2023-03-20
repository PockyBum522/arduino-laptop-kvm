#include <Wire.h>

// Define Slave I2C Address
#define SLAVE_ADDR 9

// Define Slave answer size
#define ANSWERSIZE 5

// Define string with response to Master
String answer = "Hello";

void setup() 
{
  // Initialize I2C communications as Slave
  Wire.begin(SLAVE_ADDR);

  // Function to run when data requested from master
  Wire.onRequest(*requestEvent);

  // Function to run when data received from master
  Wire.onReceive(*receiveEvent);

  // Setup Serial Monitor
  SerialUSB.begin(115200);
  SerialUSB.println("I2C Slave Demonstration");
}

void receiveEvent(int howmany) 
{
  byte x = 0;
  
  if (howmany == 0) 
  {
    return;
  }

  // Read while data received
  while (Wire.available()) 
  {
    x = Wire.read();
  }

  // Print to Serial Monitor
  SerialUSB.print("\nReceive event - data receviced = ");
  SerialUSB.println(x,HEX);
}

void requestEvent() 
{
  // Setup byte variable in the correct size
  byte response[ANSWERSIZE];

  // Format answer as array
  for (byte i=0;i<ANSWERSIZE;i++) 
  {
    response[i] = (byte)answer.charAt(i);
  }

  // Send response back to Master
  Wire.write(response,sizeof(response));

  // Print to Serial Monitor
  SerialUSB.println("Request event - data sent");
}

void loop() 
{
  // Time delay in loop
  delay(50);
}