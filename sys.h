#ifndef __SYS_H
#define __SYS_H

#include <stdint.h>

void sysPinConfig();
void sysClockConfig();
void sysTickConfig();
uint32_t sysGetTickCount();
void delay_us(uint16_t n);
void delay(uint16_t n);

#endif
