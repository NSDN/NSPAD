#ifndef __INSTR_H
#define __INSTR_H

#include "cvm.h"
#include "ch559.h"

#include "sys.h"
#include "pin.h"
#include "usb.h"
#include "rom.h"
#include "key.h"

#define SHIFT 0x80
static uint8_c _asciimap[128] = {	// 模拟字符串输入用的查找表
	0x00,             // NUL
	0x00,             // SOH
	0x00,             // STX
	0x00,             // ETX
	0x00,             // EOT
	0x00,             // ENQ
	0x00,             // ACK  
	0x00,             // BEL
	0x2a,			  // BS	Backspace
	0x2b,			  // TAB ab
	0x28,			  // LF	Enter
	0x00,             // VT 
	0x00,             // FF 
	0x00,             // CR 
	0x00,             // SO 
	0x00,             // SI 
	0x00,             // DEL
	0x00,             // DC1
	0x00,             // DC2
	0x00,             // DC3
	0x00,             // DC4
	0x00,             // NAK
	0x00,             // SYN
	0x00,             // ETB
	0x00,             // CAN
	0x00,             // EM 
	0x00,             // SUB
	0x00,             // ESC
	0x00,             // FS 
	0x00,             // GS 
	0x00,             // RS 
	0x00,             // US 

	0x2c,		   //  ' '
	0x1e|SHIFT,	   // !
	0x34|SHIFT,	   // "
	0x20|SHIFT,    // #
	0x21|SHIFT,    // $
	0x22|SHIFT,    // %
	0x24|SHIFT,    // &
	0x34,          // '
	0x26|SHIFT,    // (
	0x27|SHIFT,    // )
	0x25|SHIFT,    // *
	0x2e|SHIFT,    // +
	0x36,          // ,
	0x2d,          // -
	0x37,          // .
	0x38,          // /
	0x27,          // 0
	0x1e,          // 1
	0x1f,          // 2
	0x20,          // 3
	0x21,          // 4
	0x22,          // 5
	0x23,          // 6
	0x24,          // 7
	0x25,          // 8
	0x26,          // 9
	0x33|SHIFT,    // :
	0x33,          // ;
	0x36|SHIFT,    // <
	0x2e,          // =
	0x37|SHIFT,      // >
	0x38|SHIFT,      // ?
	0x1f|SHIFT,      // @
	0x04|SHIFT,      // A
	0x05|SHIFT,      // B
	0x06|SHIFT,      // C
	0x07|SHIFT,      // D
	0x08|SHIFT,      // E
	0x09|SHIFT,      // F
	0x0a|SHIFT,      // G
	0x0b|SHIFT,      // H
	0x0c|SHIFT,      // I
	0x0d|SHIFT,      // J
	0x0e|SHIFT,      // K
	0x0f|SHIFT,      // L
	0x10|SHIFT,      // M
	0x11|SHIFT,      // N
	0x12|SHIFT,      // O
	0x13|SHIFT,      // P
	0x14|SHIFT,      // Q
	0x15|SHIFT,      // R
	0x16|SHIFT,      // S
	0x17|SHIFT,      // T
	0x18|SHIFT,      // U
	0x19|SHIFT,      // V
	0x1a|SHIFT,      // W
	0x1b|SHIFT,      // X
	0x1c|SHIFT,      // Y
	0x1d|SHIFT,      // Z
	0x2f,          // [
	0x31,          // bslash
	0x30,          // ]
	0x23|SHIFT,    // ^
	0x2d|SHIFT,    // _
	0x35,          // `
	0x04,          // a
	0x05,          // b
	0x06,          // c
	0x07,          // d
	0x08,          // e
	0x09,          // f
	0x0a,          // g
	0x0b,          // h
	0x0c,          // i
	0x0d,          // j
	0x0e,          // k
	0x0f,          // l
	0x10,          // m
	0x11,          // n
	0x12,          // o
	0x13,          // p
	0x14,          // q
	0x15,          // r
	0x16,          // s
	0x17,          // t
	0x18,          // u
	0x19,          // v
	0x1a,          // w
	0x1b,          // x
	0x1c,          // y
	0x1d,          // z
	0x2f|SHIFT,    // {
	0x31|SHIFT,    // |
	0x30|SHIFT,    // }
	0x35|SHIFT,    // ~
	0			   // DEL
};

volatile uint8_t out_ptr = 0;	// HID发送缓冲区指针
volatile __bit dummy;
#define DUMMY_THING(op) (dummy = op->type_dst_expr != 0)	// 避免编译器警告

/*
 * 空指令
 */
cvm_ret __instr_nop(CVM_OP* op) {
    __asm__("nop");
    DUMMY_THING(op);
    return CVM_RET_OK;
}

/*
 * 直接跳转指令
 */
cvm_ret __instr_jmp(CVM_OP* op) {
    cvm_jmp(op->dst);
    return CVM_RET_OK;
}

/*
 * 清空HID发送缓冲区
 */
cvm_ret __instr_clr(CVM_OP* op) {
	for (uint8_t i = 0; i < HID_BUF_SIZE; i++)
        setHIDData(i, 0);
    out_ptr = 0;
    DUMMY_THING(op);
    return CVM_RET_OK;
}

/*
 * 向HID发送缓冲区写入一个字节
 */
cvm_ret __instr_prt(CVM_OP* op) {
    setHIDData(out_ptr, op->type_dst_expr);
    out_ptr += 1;
    return CVM_RET_OK;
}

/*
 * 将HID发送缓冲区内的数据上传
 */
cvm_ret __instr_hidp(CVM_OP* op) {
    pushHIDData();
    DUMMY_THING(op);
    return __instr_clr(op);
}

/*
 * 将HID发送缓冲区内的数据看作字符串并模拟按键输入
 */
cvm_ret __instr_strp(CVM_OP* op) {
    uint8_t t = op->type_dst_expr;

    uint8_t c = 0;
    for (uint8_t i = 0; i < out_ptr; i++) {
        c = getHIDData(i);
        c = _asciimap[c];
        usbSetKeycode(0, 1);
        usbSetKeycode(1, (c & SHIFT) ? 0x02 : 0x00);
        usbSetKeycode(2, c);
        usbPushKeydata();
        delay(t / 2);
        usbSetKeycode(1, 0);
        usbSetKeycode(2, 0);
        usbPushKeydata();
        delay(t);
    }

    out_ptr = 0;
    return CVM_RET_OK;
}

/*
 * 设置键值
 */
cvm_ret __instr_out(CVM_OP* op) {
    usbSetKeycode(op->type_dst_expr, op->dst);
    return CVM_RET_OK;
}

/*
 * 将按键信息上传
 */
cvm_ret __instr_keyp(CVM_OP* op) {
    usbPushKeydata();
    DUMMY_THING(op);
    return CVM_RET_OK;
}

/*
 * 将内部存储器中的一个字节存入HID发送缓冲区
 */
cvm_ret __instr_ldi(CVM_OP* op) {
    setHIDData(op->type_dst_expr, romRead8i(op->dst));
    return CVM_RET_OK;
}

/*
 * 将一个字节存入内部存储器
 */
cvm_ret __instr_wri(CVM_OP* op) {
    romWrite8i(op->dst, op->type_dst_expr);
    return CVM_RET_OK;
}

/*
 * 内部存储器块擦除
 */
cvm_ret __instr_erase(CVM_OP* op) {
    if (op->type_dst_expr == 0xAA) {
		romErase(op->dst);
        return CVM_RET_OK;
    }
    return CVM_RET_ERR;
}

/*
 * 延时一段时间，单位为毫秒
 */
cvm_ret __instr_sleep(CVM_OP* op) {
    delay(op->dst); //TODO
    return CVM_RET_OK;
}

/*
 * 系统软复位
 */
cvm_ret __instr_sysrst(CVM_OP* op) {
    if (op->type_dst_expr == 0x55 && op->dst == 0x55AA) {
		delay(500);
		EA = 0;
        SAFE_MOD = 0x55;
        SAFE_MOD = 0xAA;
        GLOBAL_CFG |= bSW_RESET;
        return CVM_RET_OK;
    }
    return CVM_RET_ERR;
}

/*
 * 重加载配置
 */
cvm_ret __instr_reload(CVM_OP* op) {
	DUMMY_THING(op);
	
    LED = !LED;
    delay(250);
    LED = !LED;
    delay(250);

    loadKeyConfig();

    return CVM_RET_OK;
}

#endif