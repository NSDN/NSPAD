#include "sys.h"

#include "ch559.h"
#include "pin.h"

void sysPinConfig() {
    P1_PU &= ~(0x3F);       // P10~P15, Output Open-Drain
    P1_DIR = 0x3F;

    P4_DIR = 0x80;          // P47, Output
    P4_PU &= ~(0x0F);       // P40~P43, Input

    PORT_CFG &= ~(bP2_OC);  // P24, Output Push-Pull
    P2_DIR |= (1 << 4);
    PORT_CFG &= ~(bP3_OC);  // P36 P37, Input Pull-Up
}

void sysClockConfig() {
    SAFE_MOD = 0x55;
    SAFE_MOD = 0xAA;
    CLOCK_CFG = bOSC_EN_INT;
    CLOCK_CFG &= ~MASK_SYS_CK_DIV;
    CLOCK_CFG |= 6; // fSys = 56MHz;
    PLL_CFG = 0xFC; // fPll = 336MHz;
    SAFE_MOD = 0xFF;
}

void sysTickConfig() {
    T2MOD |= (bTMR_CLK | bT2_CLK); C_T2 = 0;
    RCAP2 = T2COUNT = 0xFFFF - (uint16_t) (56000000L / 1000L);   // 1000Hz
    TR2 = 1;
    ET2 = 1;
}

volatile uint32_t sysTickCount = 0;

void __tim2Interrupt() __interrupt (INT_NO_TMR2) __using (2) {
    if (TF2) {
        TF2 = 0;
        sysTickCount += 1;
    }
}

uint32_t sysGetTickCount() {
    return sysTickCount;
}

void delay_us(uint16_t n) {
	while (n) {  // total = 12~13 Fsys cycles, 1uS @Fsys=12MHz
		++ SAFE_MOD;  // 2 Fsys cycles, for higher Fsys, add operation here
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
        ++ SAFE_MOD;
		-- n;
	}
}

void delay(uint16_t n) {
	while (n) {
        delay_us(1000);
		-- n;
	}
}                  
