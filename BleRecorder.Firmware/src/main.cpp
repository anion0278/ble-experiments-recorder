#define SERIALDEBUG
#define BLINKERDEBUG

#define SLEEP_TIME 12 // exoresed in 10 sec ticks
bool volatile neverSleep=true;
bool neverSleepAgain=true; // just for testing

#include <Aggregatorversion.h>

#ifdef Aggregator_01
    #define DEVICE_NAME "Aggregator-UP20L"
    //#define DEVICE_NAME "Aggregator-DEV-MM1"
    #define DEVICE_SERIAL_NO "2125001"
#endif

#ifdef Aggregator_02
    #define DEVICE_NAME "Aggregator"
    //#define DEVICE_NAME "Aggregator-DEV-MM1"
    #define DEVICE_SERIAL_NO "112123001"
#endif

#define DEVICE_BLE_SERIAL "NA"

#include <ArduinoBLE.h>                     // BLE komunikace, device, controller, services, characteristics
#include <Nicla_System.h>                   // NICLA stadnard library - leds, etc.
#include <BoschSensortec.h>                 // main Bosch Sensortec library (sensor manager)
#include <sensors/SensorOrientation.h>      // vitrual sensor - rotation
#include <BQ25120A.h>                       // power managment chip library

#include <HardwareBLESerial.h>              // nRF standard BLE Serial port emulator

#include <mbed.h>                           // mbed rtos core
#include <events/mbed_events.h>             // evnets are user defined functions called by queue
#include <events/mbed_shared_queues.h>      // queue holds list of scheduled event

#include <sla_si7210.h>                     // BW library for hall sensors
#include <sl7210angle.h>                    // include with angle calulation argorithm

#include <string.h>
#include <chrono>                           // allows constants like "50s"

using namespace mbed;
using namespace events;
using namespace std::chrono;

/*****************************************
* BLE Service - 180A - DEVICE DESCRIPTION 
******************************************/
BLEService bleDeviceService("180A");

BLECharacteristic bleDeviceProductName("2A24",BLERead,"Aggregator");
BLECharacteristic bleDeviceSerialNo("2A25",BLERead,DEVICE_BLE_SERIAL);
BLECharacteristic bleDeviceFirmwareVersion("2A26",BLERead,"0.00.00001");
BLECharacteristic bleDeviceHardwareVersion("2A27",BLERead,DEVICE_SERIAL_NO);
// BLECharacteristic bleDeviceSoftwareVersion("2A28",BLERead,"0.00.00000");
BLECharacteristic bleDeviceManufacturer("2A29",BLERead,"experimenter");

/******************************
* BLE Service - 180F - BATTERY 
*******************************/
BLEService batteryService("180F");

BLEUnsignedCharCharacteristic batteryLevelChar("2A19", BLERead | BLENotify); // battery level 0-100
BLEByteCharacteristic batteryStausChar("2A1A",BLERead | BLENotify); // power status
/* bit encoded power status:
   00xxxxxx - unknown
   01xxxxxx - not supported
   10xxxxxx - good
   11xxxxxx - critically low
   xx00xxxx - unknown
   xx01xxxx - non chargeable
   xx10xxxx - not charging
   xx11xxxx - charging
   xxxx00xx - unknow
   xxxx01xx - not suported
   xxxx10xx - not discharging
   xxxx11xx - charging
   xxxxxx00 - not supported
   xxxxxx01 - no battery power suppored
   xxxxxx10 - no battery installed
   xxxxxx11 - battery present
*/

/*****************************
* BLE Sesrvice - E000 - SMHJ
*****************************/
BLEService smhjService("E000");

BLECharacteristic smhjMode("0001",BLERead | BLENotify,"POSITION"); // text indicatioof data mode, in future will be used to switch mode
BLEDescriptor modeNameDesciptor("2901","Mode");

BLECharacteristic smhjI2CError("1000",BLERead | BLENotify,"N/A"); // last known external I2C error
BLEDescriptor I2CErrorDesciptor("2901","I2CError");

/*****************************
* BLE Sesrvice - Nordic UART
*****************************/

HardwareBLESerial &bleSerial = HardwareBLESerial::getInstance();

void bleSetupServices(void){
    BLE.setDeviceName("Aggregator");
    BLE.setLocalName(DEVICE_NAME);
    BLE.setAppearance(0x0443); // running sensor - hip mounted

    bleDeviceService.addCharacteristic(bleDeviceProductName);
    bleDeviceService.addCharacteristic(bleDeviceSerialNo);
    bleDeviceService.addCharacteristic(bleDeviceFirmwareVersion);
    bleDeviceService.addCharacteristic(bleDeviceHardwareVersion);
    bleDeviceService.addCharacteristic(bleDeviceManufacturer);
    BLE.addService(bleDeviceService);

    batteryService.addCharacteristic(batteryLevelChar);
    batteryService.addCharacteristic(batteryStausChar);
    BLE.addService(batteryService);

    smhjService.addCharacteristic(smhjMode);
    smhjMode.addDescriptor(modeNameDesciptor);
    smhjMode.setValue("ANGLE");
    
    smhjService.addCharacteristic(smhjI2CError);
    smhjI2CError.addDescriptor(I2CErrorDesciptor);

    BLE.addService(smhjService);    

    bleSerial.begin();

    BLE.setAdvertisedService(smhjService);
}

/*****************
 * Nicla Sensors
 ****************/

SensorOrientation orientation(SENSOR_ID_ORI);

int volatile sl7210Result;

/***************
 * multitasking
 ***************/

EventQueue *queue;
rtos::Mutex my_mutex;
bool volatile queueRunning=false;
uint64_t volatile lastTick=0;
uint64_t volatile lastCheck=0;
int volatile sleepCounter=0;
uint64_t volatile wakeMilis=0;

/*******************
 * Power managemetn
 *******************/

enum pwmStatus_t {none,charging,charged,fault};
pwmStatus_t volatile pwmStatus=none;
uint8_t volatile pwmLevel=0;                    // % of capacity 
uint8_t volatile vLevel=0;                      // % of full voltage (80% V-> fully discharged)
uint8_t volatile pwmChargeControl=0;            // snapshot of Charge Control register

void enableCD()
{
  pinMode(p25, OUTPUT);
  digitalWrite(p25, HIGH);
}

void disableCD()
{
  pinMode(p25, OUTPUT);
  digitalWrite(p25, LOW);
}

byte bq25120_getBatteryPercentage(void){
    byte data;
    nicla::_pmic.writeByte(BQ25120A_ADDRESS,BQ25120A_BATT_MON,0x80);
    delay(1);
    data=nicla::_pmic.readByte(BQ25120A_ADDRESS,BQ25120A_BATT_MON);
    vLevel=((data & 0b01100000)>>5)*10+60;
    switch ((data & 0b00011100)>>3)
    {
        case 0b111:vLevel+=8;break;
        case 0b110:vLevel+=6;break;
        case 0b011:vLevel+=4;break;
        case 0b010:vLevel+=2;break;
    }
    if (vLevel>=90){return 100;}        // 90 for 4.35+,    98 for 4.2-
    else if (vLevel>=88){return 75;}    // 88               96
    else if (vLevel>=86){return 50;}    // 86               92
    else if (vLevel>=84){return 25;}    // 84               86
    else {return 0;}
}

void updateBattery(void){
    enableCD();
    byte status=nicla::_pmic.getStatus();
    pwmLevel=bq25120_getBatteryPercentage();
    pwmChargeControl=nicla::_pmic.readByte(BQ25120A_ADDRESS,BQ25120A_FAST_CHG);
    disableCD();
    switch ((status & 0b11000000)>>6)
    {
    case 0: // Ready
        pwmStatus=none;
        break;
    case 1: // Charging
        pwmStatus=charging;
        break;
    case 2: // Charged
        pwmStatus=charged;
        break;
    case 3: // Fault
        pwmStatus=fault;
    }
    if (BLE.central()) {
        batteryLevelChar.writeValue(pwmLevel); 
        batteryStausChar.writeValue(status); // direct status reg
    }
    if (queueRunning) {queue->call_in(10s,updateBattery);}
}

/*********************************************************************
 * Debug utilities
 * - printTick - prints entire diagnostic tick base of global variables
 * - blinker - creates colorfull led show for 2 s
 *********************************************************************/

void printTick(void){
    #ifdef SERIALDEBUG
        Serial.print("Tick: ");
        Serial.print(lastTick/60000);
        Serial.print(":");
        if ((lastTick/1000)%60<10) { Serial.print("0");}
        Serial.print((lastTick/1000)%60);
        Serial.print("s I2C: "); 
        Serial.print(sl7210Result);
        Serial.print(" BAT:Stat: 0x");
        Serial.print(pwmStatus,HEX);
        Serial.print(" ChgCntr: 0x");
        Serial.print(pwmChargeControl,HEX);
        Serial.print(" Level: ");
        Serial.print(pwmLevel);
        Serial.print("% / ");
        Serial.print(vLevel);
        Serial.print("%V");
        if ((sleepCounter+5>=SLEEP_TIME)and(sleepCounter<=SLEEP_TIME)and(!neverSleep)){
            Serial.print(" - sleeping in: ");
            Serial.print(SLEEP_TIME-sleepCounter);
            Serial.print(" ticsk");
        }
        Serial.println();
    #endif
}

void blinker(void){
        for (int i=0;i<3;i++){
            nicla::leds.setColor(red);
            delay(100);
            nicla::leds.setColor(green);
            delay(100);
            nicla::leds.setColor(blue);
            delay(100);
        }
        nicla::leds.setColor(off);
      delay(300);
}

/*************
 * ReadAgnle
 *************/

void readAngle(void){
    my_mutex.lock();

    /*********************************
     * ticker - each 10 milis        
     * - keep alive power management 
     * - count ticks to sleep
     * - print serial debug when enabled        
     * *******************************/
    if (millis()>lastTick+10000){
        lastTick=millis();

        nicla::enable3V3LDO();
        if(sl7210Result!=0){
            sl7210Result=sl7210angleInit();
        }

        sleepCounter++;

        printTick();        
        
        if ((sleepCounter>=SLEEP_TIME)and(!neverSleep)){
            queueRunning=false;
            my_mutex.unlock();
            return;
        }
    }

    uint32_t timestamp=(millis()-wakeMilis)/10;
    float angle;

    // updating angle to BLE / using sl7210 if available, otherwise sensorcet.orientation.roll()
    if (BLE.central()){
        sleepCounter=0;

        sensortec.update();
        if(sl7210Result!=0){
            angle=orientation.roll();
            smhjMode.setValue("POS");
        } else {
            sl7210Result=sl7210angleRead(angle);
            if (sl7210Result!=0) {
                char result[20];
                sprintf(result,"%i",sl7210Result);
                smhjI2CError.writeValue(result);
            } else {
                smhjMode.setValue("ANG");
            }
        }
        
        char result[20]; 
        sprintf(result,"+T:%ld,F:%5.2f",timestamp,angle); 
        bleSerial.println(result);
        bleSerial.flush();
    } else {
        if (millis()>lastCheck+2000){
            lastCheck=millis();
            sl7210Result=sl7210angleRead(angle);
            if (sl7210Result!=0) {
                char result[20];
                sprintf(result,"%i",sl7210Result);
                smhjI2CError.writeValue(result);
            }
        }
    }

    my_mutex.unlock();
    BLE.poll(); // ble event processing, its here in most frequent event
    if (queueRunning) {queue->call_in(50ms,readAngle);}
}


/************************
 * setatusLed
 * - power failure - fast red blinking
 * - charging - red
 * - charged - green
 * 
 * - emulator - white color
 * = real angle - blue color
 * 
 * - pairing - blinking 1s/1s
 * - paired - constatn ligh with short blink of power status
 **/

void statusLed(void){
    if (pwmStatus==fault){
        nicla::leds.setColor(red);
        delay(100);
        nicla::leds.setColor(off);
        delay(100);
        if (queueRunning) {queue->call(statusLed);}
        return;
    }

    RGBColors bleStatusColor=blue;
    switch (pwmStatus)
    {
    case charging:
        bleStatusColor=blue;
        break;
    case charged:
        bleStatusColor=green;
        break;
    default:
        bleStatusColor=blue;
        break;
    };
    
    nicla::leds.setColor(bleStatusColor);
    delay(1000);
    
    if (!BLE.central()) {bleStatusColor=off;}
    if (sl7210Result==0){
        nicla::leds.setColor(bleStatusColor);
        delay(1000);
    } else {
        nicla::leds.setColor(bleStatusColor);
        delay(400);
        nicla::leds.setColor(red);
        delay(100);
        nicla::leds.setColor(bleStatusColor);
        delay(500);
    }
    if (queueRunning) {queue->call(statusLed);}
}

/********************
 * turnOn + turnOff
 *******************/

void turnOn(void){
    pinMode(P0_10, OUTPUT);
    digitalWrite(P0_10, HIGH);  

    #ifdef SERIALDEBUG
    Serial.println("--- Waking up ---");
    #endif

    // Wire1.begin()           // inst not sleeping, does not need wakeup 
    nicla::leds.begin();
    nicla::leds.setIntensity(4);
    smhjMode.setValue("NON");
    BLE.advertise();
    nicla::enable3V3LDO();
    
    sensortec.begin();
    orientation.begin();
    Wire.begin();
    
    sl7210Result=sl7210angleInit();
    if (sl7210Result!=0){
        char result[20];
        sprintf(result,"%d",sl7210Result);
        smhjI2CError.writeValue(result);
    }
}

void turnOff(void){
    nicla::leds.setColor(off);
    nicla::leds.end();
    BLE.stopAdvertise();
    // Wire1.end();                // Wire1 is needed for BHI sensor to operate, used for wake up
    // Wire.end();                 // Wire is needed to read change in hands angle
    // nicla::disableLDO();        // We have to figure out turning power on wile on battery
    #ifdef SERIALDEBUG
    Serial.print("--- Entering sleep ---\n Deep sleep allowed: ");
    Serial.println(sleep_manager_can_deep_sleep());
    #endif
}

void setup() {
    pinMode(P0_10, OUTPUT);
    digitalWrite(P0_10, HIGH);  

    Wire1.begin();              // internal Wire1 to BHI260, pwm, leds
    
    nicla::started=true;
    
    nicla::leds.begin();
    nicla::leds.setIntensity(4);
    
    while (!BLE.begin()) 
    {
        nicla::leds.setColor(blue);
        delay(100);
        nicla::leds.setColor(off);
        delay(100);     
    }
    
    bleSetupServices();

    queue = mbed_event_queue();                 // reusing internall queue
    
    #ifdef SERIALDEBUG
    Serial.begin(9600);
    Serial.println();
    Serial.print(DEVICE_NAME); 
    Serial.println(" Started\n");
    Serial.flush();
    #endif
}

void loop() {
    sleepCounter = 0;  
    
    turnOn();  

    // schedulling the events
    wakeMilis=millis();
    queueRunning=true;
    queue->call(readAngle); 
    queue->call(updateBattery);
    queue->call(statusLed);             

    // disatch loop;
    while (queueRunning) {queue->dispatch_once();}

    turnOff();
    
    // waiting for wake/up signal - device roll

    float oldAngle=360;
    float angle;
    uint32_t counter=0;
    while(1) {
        queue->dispatch_for(2s);
        nicla::enable3V3LDO();  // wathdog protection
        if (sl7210Result!=0){
            sl7210Result=sl7210angleInit();
        }
        if (sl7210Result==0){
            sl7210Result=sl7210angleRead(angle);
        }
        if (sl7210Result!=0){angle=360;}
        #ifdef SERIALDEBUG
        Serial.print(++counter);Serial.print(": ");
        Serial.print(angle);Serial.println();
        #endif
        if ((abs(oldAngle-angle)>10)and(oldAngle!=360)and(angle!=360)){
            break;
        }
        oldAngle = angle;
    }

    neverSleep=neverSleepAgain; // just for testing
}