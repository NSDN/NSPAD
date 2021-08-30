#include "rom.h"

#include "ch559.h"
#include "sys.h"

/*
 * 内部存储器写入
 */
void __flash_write(uint16_t addr, uint16_t data) {
    addr &= (FLASH_SIZE - 2); // 偶地址有效
    SAFE_MOD = 0x55;
    SAFE_MOD = 0xAA;
    GLOBAL_CFG |= bDATA_WE;
    ROM_ADDR = DATA_FLASH_ADDR + addr;
    ROM_DATA = data;
    ROM_CTRL = 0x9A;
    SAFE_MOD = 0x55;
    SAFE_MOD = 0xAA;
    GLOBAL_CFG &= ~bDATA_WE;
}

/*
 * 内部存储器读取
 */
uint8_t __flash_read(uint16_t addr) {
    addr &= (FLASH_SIZE - 1);
    return (uint8_t) (*(uint8_c*) (DATA_FLASH_ADDR + addr));
}

/*
 * 从内部存储器读取一个字节
 */
uint8_t romRead8i(uint16_t addr) {
    return __flash_read(addr);
}

/*
 * 向内部存储器写入一个字节
 */
void romWrite8i(uint16_t addr, uint8_t data) {
    uint8_t buf = 0;
    if (addr & 0x0001) {
        buf = __flash_read(addr & 0xFFFE);
        __flash_write(addr, buf | (data << 8));
    } else {
        buf = __flash_read(addr | 0x0001);
        __flash_write(addr, data | (buf << 8));
    }
}

/*
 * 从内部存储器读取一个字
 */
uint16_t romRead16i(uint16_t addr) {
    return ((uint16_t) __flash_read(addr)) | (((uint16_t) __flash_read(addr + 1)) << 8);
}

/*
 * 向内部存储器写入一个字
 */
void romWrite16i(uint16_t addr, uint16_t data) {
    __flash_write(addr, data);
}
