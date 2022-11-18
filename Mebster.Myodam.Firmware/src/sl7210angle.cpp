#include <unilexaversion.h>

#include <sla_si7210.h>

sl_si7210_configure_t devConfig;
byte devAddrOne=0x31;
byte devAddrTwo=0x32;

double vA,vB;           // values from hall sensors


/****************************
 * embeded calibration data
 ****************************/

struct calib { 
  double x;
  double yA;
  double yB;
};

calib calibData[]= {
#ifdef UNILEXA_01
{-40,-5.657,-0.688},
{-38,-6.836,-0.825},
{-36,-8.372,-1.033},
{-34,-10.143,-1.336},
{-32,-11.613,-1.706},
{-30,-12.530,-2.128},
{-28,-12.872,-2.756},
{-26,-12.650,-3.236},
{-24,-11.830,-4.033},
{-22,-10.395,-5.118},
{-20,-8.568,-6.481},
{-18,-6.438,-8.217},
{-16,-5.331,-9.282},
{-14,-3.617,-10.831},
{-12,-2.191,-11.997},
{-10,-0.623,-12.791},
{-8,0.885,-12.788},
{-6,2.215,-12.135},
{-4,3.557,-11.023},
{-2,5.138,-9.377},
{0,7.017,-7.355},
{2,8.801,-5.473},
{4,10.068,-4.128},
{6,11.410,-2.537},
{8,12.358,-0.828},
{10,12.460,0.565},
{12,11.782,2.158},
{14,10.597,3.665},
{16,9.170,5.177},
{18,7.658,6.808},
{20,6.100,8.656},
{22,4.927,10.166},
{24,3.922,11.470},
{26,3.073,12.347},
{28,2.373,12.585},
{30,1.796,12.010},
{32,1.477,11.195},
{34,1.128,9.718},
{36,0.902,8.412},
{38,0.721,7.055},
{40,0.528,5.486}
#endif

#ifdef UNILEXA_02
{40,-0.585,-6.378},
{38,-0.692,-7.317},
{36,-0.871,-8.677},
{34,-1.157,-10.317},
{32,-1.550,-11.673},
{30,-1.943,-12.307},
{28,-2.513,-12.358},
{26,-3.281,-11.640},
{24,-4.265,-10.251},
{22,-5.167,-8.966},
{20,-6.477,-7.246},
{18,-7.918,-5.583},
{16,-9.290,-4.168},
{14,-10.816,-2.613},
{12,-11.797,-1.372},
{10,-12.336,-0.010},
{8,-12.112,1.351},
{6,-11.270,2.727},
{4,-9.716,4.492},
{2,-8.128,6.150},
{0,-6.463,7.958},
{-2,-4.802,9.811},
{-4,-3.598,11.111},
{-6,-2.130,12.336},
{-8,-0.751,12.812},
{-10,0.752,12.356},
{-12,2.255,11.142},
{-14,3.572,9.847},
{-16,5.660,7.865},
{-18,7.180,6.592},
{-20,9.352,5.022},
{-22,10.565,4.205},
{-24,11.563,3.515},
{-26,12.568,2.573},
{-28,12.577,2.013},
{-30,11.898,1.571},
{-32,10.510,1.182},
{-34,8.752,0.883},
{-36,7.336,0.698},
{-38,5.990,0.562},
{-40,4.037,0.370}
#endif
};

int xcount = sizeof(calibData)/sizeof(calib);

double trojclenka(double y1, double y2, double x1, double x2, double y){
  if (y1 != y2) {
    return (x1 - x2) / (y1 - y2) * (y - y2) + x2;
  } else {
    return (x1 + x2) / 2;
  }
}

/************************************************
 * initializes both sensors, returns error code
 ************************************************/

int sl7210angleInit(void){
    int initResult=0;
    if (devAddrOne>0) {
        initResult=sl_si7210_init(devAddrOne);
        if (!initResult) {return initResult;};
        devConfig.threshold=0;
        devConfig.hysteresis=0;
        devConfig.polarity=0;
        devConfig.output_invert=false;
        initResult=sl_si7210_configure(devAddrOne,&devConfig);
        if (!initResult) {return initResult;};
    }
    if (devAddrTwo>0) {
        initResult=sl_si7210_init(devAddrTwo);
        if (!initResult) {return initResult;};
        devConfig.threshold=0;
        devConfig.hysteresis=0;
        devConfig.polarity=0;
        devConfig.output_invert=false;
        initResult=sl_si7210_configure(devAddrTwo,&devConfig);
    }
    return initResult;
}

/***************************************
 * reads sensors and calculates angle
 * - returns angle + error code
 * take about 18 ms to complete
 **************************************/

int sl7210angleRead(float &pozice){
  float field=0;  
  int errSensor=0;
  pozice=360;
  errSensor=sl_si7210_measure(devAddrOne,20000,&field);
  if (!errSensor) {
    vA = field;
  } else {
    return errSensor;
  }
  delay(10);
  errSensor=sl_si7210_measure(devAddrTwo,20000,&field);
  if (!errSensor) {
    vB = field;
  } else {
    return errSensor;
  }  
  double besterr = 1600;
  double newerr;
  int i1 = 0;
  for (int i = 0; i < xcount - 1; i++) {
    newerr = pow(calibData[i].yA - vA, 2)+pow(calibData[i + 1].yA - vA, 2)+
             pow(calibData[i].yB - vB, 2)+pow(calibData[i + 1].yB - vB, 2);
    if (newerr <= besterr) {
      besterr = newerr;
      i1 = i;
    }
  }
  double
    sizeA=abs(calibData[i1].yA - calibData[i1 + 1].yA),
    sizeB=abs(calibData[i1].yB - calibData[i1 + 1].yB);
  double
    poziceA = trojclenka(calibData[i1].yA, calibData[i1 + 1].yA, calibData[i1].x, calibData[i1 + 1].x, vA),
    poziceB = trojclenka(calibData[i1].yB, calibData[i1 + 1].yB, calibData[i1].x, calibData[i1 + 1].x, vB);
  pozice=(poziceA*sizeA+poziceB*sizeB)/(sizeA+sizeB);
  return 0;
}