#ifndef __PIN_H
#define __PIN_H

#include "ch559.h"

// LEDs
#define LED     P47

// Mode SW
#define MODE    ((~P3 & 0xC0) >> 6)
#define MODE0   P36
#define MODE1   P37

// Beeper
#define BEEPER  P24

// Buttons
#define BCc     (P1_DIR)
#define BCx     (P1)
#define BC0     P10
#define BC1     P11
#define BC2     P12
#define BC3     P13
#define BC4     P14
#define BC5     P15

#define BRx     (P4_IN & 0x0F)
#define BR0     P40
#define BR1     P41
#define BR2     P42
#define BR3     P43

#endif
