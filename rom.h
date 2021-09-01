#ifndef __ROM_H
#define __ROM_H

#include <stdint.h>

#define FLASH_SIZE 0x400

void romErase(uint16_t addr);
uint8_t romRead8i(uint16_t addr);
void romWrite8i(uint16_t addr, uint8_t data);
uint16_t romRead16i(uint16_t addr);
void romWrite16i(uint16_t addr, uint16_t data);

#endif