#include <Wire.h>

// Define Slave I2C Address
#define SLAVE_ADDR 9

// Define Slave answer size
#define ANSWERSIZE 5

void setup() 
{
  // Initialize I2C communications as Master
  Wire.begin();

  SerialUSB.begin(115200);
}

byte i = 0;

void loop() 
{
  delay(1000);
  SerialUSB.println("\nWrite data to slave");

  // Write a character to the Slave
  Wire.beginTransmission(SLAVE_ADDR);
  Wire.write(i);
  Wire.endTransmission();

  i += 1;

  SerialUSB.print("One byte sent to slave = ");
  SerialUSB.println(i, HEX);

  // Read response from Slave
  // Read back 5 characters
  int number_of_bytes_returned = Wire.requestFrom(SLAVE_ADDR, ANSWERSIZE);

  // Add characters to string
  String response = "";
  
  while (Wire.available()) 
  {
    char b = Wire.read();
    response += b;
  }

  // Print to Serial Monitor
  SerialUSB.print(number_of_bytes_returned);
  SerialUSB.println(" bytes returned from slave = " + response);
}