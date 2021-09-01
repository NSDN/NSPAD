#include "rom.h"

#include "ch559.h"
#include "sys.h"

/**
 * 需人工操作写入控制，因此额外提供了擦除函数，每次写入前需全片擦除
 * 这样就不会每写入一个字节都要整个块都擦除，存储器寿命更长
 **/

static uint8_c __at (DATA_FLASH_ADDR) ROM[FLASH_SIZE];

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
    return ROM[addr];
}

/*
 * 存储器块擦除，现阶段 addr 只取 0xAA55, 以保证安全
 */
void romErase(uint16_t addr) {
    if (addr != 0xAA55)
        return;

    SAFE_MOD = 0x55;
    SAFE_MOD = 0xAA;
    GLOBAL_CFG |= bDATA_WE;
    ROM_ADDR = DATA_FLASH_ADDR;
    ROM_CTRL = 0xA6;
    SAFE_MOD = 0x55;
    SAFE_MOD = 0xAA;
    GLOBAL_CFG &= ~bDATA_WE;
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
