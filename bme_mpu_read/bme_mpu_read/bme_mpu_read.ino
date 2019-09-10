/**
 MPU lib : https://github.com/FaBoPlatform/FaBo9AXIS-MPU9250-Library
 
*/

#include <Wire.h>
#include <FaBo9Axis_MPU9250.h>

FaBo9Axis fabo_9axis;
byte gfs,afs;
float ax,ay,az,gx,gy,gz,mx,my,mz,temp;
int serialPeriod=200; //ms
long serialTime;

void setup() {
  Serial.begin(9600);

  if (fabo_9axis.begin()) {
    Serial.println("configured FaBo 9Axis I2C Brick");
  } else {
    Serial.println("device error");
    while(1);
  }
}
void loop() {
  fabo_9axis.readAccelXYZ(&ax,&ay,&az);
  fabo_9axis.readGyroXYZ(&gx,&gy,&gz);
  fabo_9axis.readMagnetXYZ(&mx,&my,&mz);
  fabo_9axis.readTemperature(&temp);

  if(millis()>serialTime){
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
    serialPeriod = getValue(input,' ',0).toInt();
    gfs = getValue(input,' ',1).toInt() < 4 ? getValue(input,' ',1).toInt() : 0;
    afs = getValue(input,' ',2).toInt() < 4 ? getValue(input,' ',2).toInt() : 0;
    fabo_9axis.configMPU9250(gfs,afs);
  }
}
void serialSend(){
  Serial.print(ax);
  Serial.print(" ");
  Serial.print(ay);
  Serial.print(" ");
  Serial.print(az);
  Serial.print(" ");
  Serial.print(gx);
  Serial.print(" ");
  Serial.print(gy);
  Serial.print(" ");
  Serial.print(gz);
  Serial.print(" ");
  Serial.print(mx);
  Serial.print(" ");
  Serial.print(my);
  Serial.print(" ");
  Serial.print(mz);
  Serial.print(" ");
  Serial.print(temp);
  Serial.print(" ");
  Serial.print(gfs);
  Serial.print(" ");
  Serial.print(afs);
  Serial.print(" ");
  Serial.println(serialPeriod);
}
