/**
 testing some serial reading and converting to float as well as writing on serial again
*/

float mx=1.0f;
float mx_s=1.0f;//scale
float mx_b=0.0f;//bias

int serialPeriod=1000; //ms
long serialTime;

void setup() {
  Serial.begin(9600);
}
void loop() {
  if(millis()>serialTime){
    mx=(mx-mx_b)*(float)mx_s;
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
    mx = getValue(input,' ',0).toFloat(); //DONT WORK DONT WORK DONT WORK DONT WORK DONT WORK 
    mx_b = getValue(input,' ',1).toFloat(); //DONT WORK DONT WORK DONT WORK DONT WORK DONT WORK 
    mx_s = getValue(input,' ',2).toFloat(); //DONT WORK DONT WORK DONT WORK DONT WORK DONT WORK 
  }
}
void serialSend(){ //BEWARE OF BUFFER SIZE
  Serial.print(mx);
  Serial.print(" ");
  Serial.print(mx_b);
  Serial.print(" ");
  Serial.println(mx_s);
}
