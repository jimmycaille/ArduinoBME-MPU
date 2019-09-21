/**
 MPU lib : https://github.com/FaBoPlatform/FaBo9AXIS-MPU9250-Library
 BME lib : adafruit bme280 and Adafruit unified sensor
 BH1750  : by PeterEmbedded
*/

#include <Wire.h>
#include <FaBo9Axis_MPU9250.h>


#include <SPI.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BME280.h>
#define BME_SCK 13
#define BME_MISO 12
#define BME_MOSI 11
#define BME_CS 10

#define SEALEVELPRESSURE_HPA (1013.25)

Adafruit_BME280 bme; // I2C
//Adafruit_BME280 bme(BME_CS); // hardware SPI
//Adafruit_BME280 bme(BME_CS, BME_MOSI, BME_MISO, BME_SCK); // software SPI



#include <BH1750FVI.h>
BH1750FVI LightSensor(BH1750FVI::k_DevModeContLowRes);


// HC-SR03 Define Trig and Echo pin:
#define trigPin 2
#define echoPin 3

/**
  WIRING :
  
  BME and/or
  MPU -> Nano
  
  SDA -> A4
  SCL -> A5
  GND -> GND
  VCC -> 5V

  HC-SR03
  d2 -> trig
  d3 -> echo
  
*/

FaBo9Axis fabo_9axis;
byte gfs,afs;
float ax,ay,az,gx,gy,gz,mx,my,mz,temp;
float mx_s=1.0f,my_s=1.0f,mz_s=1.0f;//scale
float mx_b=0.0f,my_b=0.0f,mz_b=0.0f;//bias
float temp2,pres,alt,humid;
int bmeMode, bmeSampleT, bmeSampleP, bmeSampleH, bmeFilter, bmeStandby;

int lux;

double distance;

int serialPeriod=200; //ms
long serialTime;

bool bme_detected,mpu_detected;




float mx0,my0,mz0;


void setup() {
    pinMode(trigPin, OUTPUT);
  pinMode(echoPin, INPUT);
  
  Serial.begin(9600);
  fabo_9axis.searchDevice();
  if (fabo_9axis.begin()) {
    mpu_detected=true;
    //Serial.println("configured FaBo 9Axis I2C Brick");
  } else {
    //Serial.println("mpu device error");
  }

LightSensor.begin();//error handling ?
LightSensor.SetMode(BH1750FVI::k_DevModeContHighRes);

/*
 * 
 *     typedef enum eDeviceMode {
      k_DevModeContHighRes     = 0x10,
      k_DevModeContHighRes2    = 0x11,
      k_DevModeContLowRes      = 0x13,
      k_DevModeOneTimeHighRes  = 0x20,
      k_DevModeOneTimeHighRes2 = 0x21,
      k_DevModeOneTimeLowRes   = 0x23
    } eDeviceMode_t;
 */

    unsigned status;
    // default settings
    // (you can also pass in a Wire library object like &Wire2)
    status = bme.begin();  
    if (!status) {
      /*
        Serial.println("Could not find a valid BME280 sensor, check wiring, address, sensor ID!");
        Serial.print("SensorID was: 0x"); Serial.println(bme.sensorID(),16);
        Serial.print("        ID of 0xFF probably means a bad address, a BMP 180 or BMP 085\n");
        Serial.print("   ID of 0x56-0x58 represents a BMP 280,\n");
        Serial.print("        ID of 0x60 represents a BME 280.\n");
        Serial.print("        ID of 0x61 represents a BME 680.\n");
        */
    }else{
      bme_detected=true;
    }

        // For more details on the following scenarious, see chapter
    // 3.5 "Recommended modes of operation" in the datasheet
    
/*
    // weather monitoring
    Serial.println("-- Weather Station Scenario --");
    Serial.println("forced mode, 1x temperature / 1x humidity / 1x pressure oversampling,");
    Serial.println("filter off");
    bme.setSampling(Adafruit_BME280::MODE_FORCED,
                    Adafruit_BME280::SAMPLING_X1, // temperature
                    Adafruit_BME280::SAMPLING_X1, // pressure
                    Adafruit_BME280::SAMPLING_X1, // humidity
                    Adafruit_BME280::FILTER_OFF   );
                      
    // suggested rate is 1/60Hz (1m)
    delayTime = 60000; // in milliseconds
*/

/*    
    // humidity sensing
    Serial.println("-- Humidity Sensing Scenario --");
    Serial.println("forced mode, 1x temperature / 1x humidity / 0x pressure oversampling");
    Serial.println("= pressure off, filter off");
    bme.setSampling(Adafruit_BME280::MODE_FORCED,
                    Adafruit_BME280::SAMPLING_X1,   // temperature
                    Adafruit_BME280::SAMPLING_NONE, // pressure
                    Adafruit_BME280::SAMPLING_X1,   // humidity
                    Adafruit_BME280::FILTER_OFF );
                      
    // suggested rate is 1Hz (1s)
    delayTime = 1000;  // in milliseconds
*/

/*    
    // indoor navigation
    Serial.println("-- Indoor Navigation Scenario --");
    Serial.println("normal mode, 16x pressure / 2x temperature / 1x humidity oversampling,");
    Serial.println("0.5ms standby period, filter 16x");
    bme.setSampling(Adafruit_BME280::MODE_NORMAL,
                    Adafruit_BME280::SAMPLING_X2,  // temperature
                    Adafruit_BME280::SAMPLING_X16, // pressure
                    Adafruit_BME280::SAMPLING_X1,  // humidity
                    Adafruit_BME280::FILTER_X16,
                    Adafruit_BME280::STANDBY_MS_0_5 );
    
    // suggested rate is 25Hz
    // 1 + (2 * T_ovs) + (2 * P_ovs + 0.5) + (2 * H_ovs + 0.5)
    // T_ovs = 2
    // P_ovs = 16
    // H_ovs = 1
    // = 40ms (25Hz)
    // with standby time that should really be 24.16913... Hz
    delayTime = 41;
    */
    
    /*
    // gaming
    Serial.println("-- Gaming Scenario --");
    Serial.println("normal mode, 4x pressure / 1x temperature / 0x humidity oversampling,");
    Serial.println("= humidity off, 0.5ms standby period, filter 16x");
    bme.setSampling(Adafruit_BME280::MODE_NORMAL,
                    Adafruit_BME280::SAMPLING_X1,   // temperature
                    Adafruit_BME280::SAMPLING_X4,   // pressure
                    Adafruit_BME280::SAMPLING_NONE, // humidity
                    Adafruit_BME280::FILTER_X16,
                    Adafruit_BME280::STANDBY_MS_0_5 );
                      
    // Suggested rate is 83Hz
    // 1 + (2 * T_ovs) + (2 * P_ovs + 0.5)
    // T_ovs = 1
    // P_ovs = 4
    // = 11.5ms + 0.5ms standby
    delayTime = 12;
*/
}
void loop() {
  if(millis()>serialTime){
    //https://www.makerguides.com/hc-sr04-arduino-tutorial/
    // Clear the trigPin by setting it LOW:
    digitalWrite(trigPin, LOW);
    delayMicroseconds(5);
    // Trigger the sensor by setting the trigPin high for 10 microseconds:
    digitalWrite(trigPin, HIGH);
    delayMicroseconds(10);
    digitalWrite(trigPin, LOW);
    // Read the echoPin, pulseIn() returns the duration (length of the pulse) in microseconds:
    distance = pulseIn(echoPin, HIGH, 20000);//timeout in us
    // Calculate the distance:
    distance= distance*0.034/2;
    
    lux = LightSensor.GetLightIntensity();

    
    if(mpu_detected){
      fabo_9axis.readAccelXYZ(&ax,&ay,&az);
      fabo_9axis.readGyroXYZ(&gx,&gy,&gz);
      fabo_9axis.readMagnetXYZ(&mx,&my,&mz);
      fabo_9axis.readTemperature(&temp);
  
      mx0=(mx-mx_b)*mx_s; //TRY TO USE OTHER VARS, NOT WRITTEN BY MPU LIB 
      my0=(my-my_b)*my_s;
      mz0=(mz-mz_b)*mz_s;
      /*
      mx0=mx;
      my0=my;
      mz0=mz;*/
    }
    if(bme_detected){
      if(bmeMode == 1 || bmeMode == 2) bme.takeForcedMeasurement();
      temp2= bme.readTemperature();
      pres = bme.readPressure() / 100.0F;
      alt  = bme.readAltitude(SEALEVELPRESSURE_HPA);
      humid= bme.readHumidity();
    }
    serialReceive();
    serialSend();
    serialTime+=serialPeriod;
  }
}
//https://arduino.stackexchange.com/questions/1013/how-do-i-split-an-incoming-string
String getValue(String data, char separator, int index){
    int found = 0;
    int strIndex[] = { 0, -1 };
    int maxIndex = data.length() - 1;

    for (int i = 0; i <= maxIndex && found <= index; i++) {
        if (data.charAt(i) == separator || i == maxIndex) {
            found++;
            strIndex[0] = strIndex[1] + 1;
            strIndex[1] = (i == maxIndex) ? i+1 : i;
        }
    }
    return found > index ? data.substring(strIndex[0], strIndex[1]) : "";
}

void serialReceive(){
  if(Serial.available()){
    String input = Serial.readStringUntil("\n");

    //get each values
    serialPeriod = getValue(input,' ',0).toInt();
    gfs = getValue(input,' ',1).toInt() < 4 ? getValue(input,' ',1).toInt() : 0;
    afs = getValue(input,' ',2).toInt() < 4 ? getValue(input,' ',2).toInt() : 0;
    mx_b = getValue(input,' ',3).toFloat();
    my_b = getValue(input,' ',4).toFloat();
    mz_b = getValue(input,' ',5).toFloat();
    mx_s = getValue(input,' ',6).toFloat();
    my_s = getValue(input,' ',7).toFloat();
    mz_s = getValue(input,' ',8).toFloat();
    bmeMode   =getValue(input,' ',9).toInt() < 4 ? getValue(input,' ',9).toInt() : 0;
    bmeSampleT=getValue(input,' ',10).toInt() < 6 ? getValue(input,' ',10).toInt() : 0;
    bmeSampleP=getValue(input,' ',11).toInt() < 6 ? getValue(input,' ',11).toInt() : 0;
    bmeSampleH=getValue(input,' ',12).toInt() < 6 ? getValue(input,' ',12).toInt() : 0;
    bmeFilter =getValue(input,' ',13).toInt() < 5 ? getValue(input,' ',13).toInt() : 0;
    bmeStandby=getValue(input,' ',14).toInt() < 8 ? getValue(input,' ',14).toInt() : 0;

    Serial.print("RCV: ");
    Serial.print(serialPeriod);
    Serial.print(" ");
    Serial.print(gfs);
    Serial.print(" ");
    Serial.print(afs);
    Serial.print(" ");
    Serial.print(mx_b);
    Serial.print(" ");
    Serial.print(my_b);
    Serial.print(" ");
    Serial.print(mz_b);
    Serial.print(" ");
    Serial.print(mx_s);
    Serial.print(" ");
    Serial.print(my_s);
    Serial.print(" ");
    Serial.print(mz_s);
    Serial.print(" ");
    Serial.print(bmeMode);
    Serial.print(" ");
    Serial.print(bmeSampleT);
    Serial.print(" ");
    Serial.print(bmeSampleP);
    Serial.print(" ");
    Serial.print(bmeSampleH);
    Serial.print(" ");
    Serial.print(bmeFilter);
    Serial.print(" ");
    Serial.print(bmeStandby);
    Serial.println(" END");
    
    //configure sensors
    fabo_9axis.configMPU9250(gfs,afs);
    bme.setSampling(bmeMode, bmeSampleT, bmeSampleP, bmeSampleH, bmeFilter, bmeStandby);
  }
}
void serialSend(){ //BEWARE OF BUFFER SIZE
  Serial.print(ax,2);
  Serial.print(" ");
  Serial.print(ay,2);
  Serial.print(" ");
  Serial.print(az,2);
  Serial.print(" ");
  Serial.print(gx,2);
  Serial.print(" ");
  Serial.print(gy,2);
  Serial.print(" ");
  Serial.print(gz,2);
  Serial.print(" ");
  Serial.print(mx0,2);
  Serial.print(" ");
  Serial.print(my0,2);
  Serial.print(" ");
  Serial.print(mz0,2);
  Serial.print(" ");
  Serial.print(temp,2);
  Serial.print(" ");
  Serial.print(gfs,2);
  Serial.print(" ");
  Serial.print(afs,2);
  Serial.print(" ");
  Serial.print(serialPeriod);
  Serial.print(" ");
  Serial.print(temp2,2);
  Serial.print(" ");
  Serial.print(pres,2);
  Serial.print(" ");
  Serial.print(alt,2);
  Serial.print(" ");
  Serial.print(humid,2);
  Serial.print(" ");
  Serial.print(lux);//no ,2 with int otherwise base 2
  Serial.print(" ");
  Serial.println(distance,2);
}
