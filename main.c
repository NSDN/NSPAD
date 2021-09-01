#include "ch559.h"

#include "sys.h"
#include "pin.h"
#include "usb.h"
#include "key.h"
#include "cvm.h"
#include "rom.h"

void __usbDeviceInterrupt() __interrupt (INT_NO_USB) __using (1);
void __tim2Interrupt() __interrupt (INT_NO_TMR2) __using (2);
extern uint8_t FLAG;

cvm_ret cvmWDTCallback() {
    return CVM_RET_OK;
}

void main() {
    sysPinConfig();
    LED = 0;

    sysClockConfig();
    delay(10);

    PWM_CYCLE = 0x00;
    PWM_CK_SE = 0x00;
    PWM_DATA = 0x00;
    PWM_CTRL |= bPWM_OUT_EN;
    PWM_CTRL |= bPWM_MOD_MFM;

    sysTickConfig();
    usbDevInit();
    EA = 1;
    UEP1_T_LEN = 0;
    UEP2_T_LEN = 0;
    UEP3_T_LEN = 0;

    FLAG = 1;

    delay(50);
    usbReleaseAll();
    usbPushKeydata();
    requestHIDData();

    LED = !LED;
    delay(250);
    LED = !LED;
    delay(250);
    LED = !LED;
    delay(250);

    loadKeyConfig();

    uint8_t prevMode = 0;

    while (1) {
        scanKeys();

        if (prevMode != MODE) {
            prevMode = MODE;
            LED = !LED;
        }

        if (hasHIDData()) {
            cvm_wdt(NULL);
            cvm_run(fetchHIDData(), HID_BUF_SIZE);
            requestHIDData();
            cvm_wdt(&cvmWDTCallback);
        }
    }
}
