#include "key.h"

#include "ch559.h"
#include "sys.h"
#include "pin.h"
#include "usb.h"

#define SHIFT 0x80
const static uint8_c _asciimap[128] = {
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
    0x29,             // ESC
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

#define KNO 0x00
#define KAT 0x11
#define KSW 0x12

const static uint8_c biKeyMap[2][4][6][2] = {
    {
        { { 'q', 'w' }, { 'e', 'r' }, { 't', 'y' }, { 'u', 'i' }, { 'o', 'p' }, { '\b', KNO } },
        { { 'a', 's' }, { 'd', 'f' }, { 'g', 'h' }, { 'j', 'k' }, { 'l', ';' }, { '\'', '\"'} },
        { { KAT, KNO }, { 'z', 'x' }, { 'c', 'v' }, { 'b', 'n' }, { 'm', ',' }, { '\n', KNO } },
        { { KNO, KNO }, { ' ', KNO }, { '.', KNO }, { '\\',KNO }, { KSW, KNO }, {  KNO, KNO } }
    },
    {
        { { '7', KNO }, { '8', KNO }, { '9', KNO }, { '+', KNO }, { '*', KNO }, { '\b', KNO } },
        { { '4', KNO }, { '5', KNO }, { '6', KNO }, { '-', KNO }, { '/', KNO }, {  '!', KNO } },
        { { '1', KNO }, { '2', KNO }, { '3', KNO }, { '(', KNO }, { ')', KNO }, { '\n', KNO } },
        { { KNO, KNO }, { '0', KNO }, { '.', KNO }, { '=', KNO }, { KSW, KNO }, {  KNO, KNO } },
    }
};

#define ESC 0x27
#define TAB 0x09
#define SFT 0x80

const static uint8_c sinKeyMap[4][6] = {
    { ESC, '1', '2', '3', '4', '5' },
    { TAB, 'q', 'w', 'e', 'r', 't' },
    { SFT, 'a', 's', 'd', 'f', 'g' },
    { KNO, 'z', 'x', ' ', 'v', KNO },
};

#define PLY 0x81
#define STP 0x82
#define PRE 0x83
#define NEX 0x84

const static uint8_c z81KeyMap[4][6] = {
    { 'n', '0', '1', '2', '3', '4' },
    { 'a', 'b', '5', '6', '7', '8' },
    { 'c', 'd', '9', 'e', 'f', 'y' },
    { KNO, PLY, STP, PRE, NEX, KNO },
};

__bit mode = 0, alter = 0;
uint8_t prev = 0, now = 0;
__bit shifted = 0;

#define __press(i) ((BRx & (1 << i)) == 0)

void biScanCol(uint8_t col) {
    BCx = ~(0x01 << col);
    delay_us(500);  // RC电路充电时间需要大约 470us

    usbSetKeycode(0, 1);

    uint8_t count = 0;
    for (uint8_t i = 0; i < 4; i++) {
        const uint8_t* btn = biKeyMap[mode][i][col];

        if (btn[0] == KNO)
            continue;

        uint8_t val = 0;
        if (__press(i)) {
            if (btn[1] == KNO) {
                switch (btn[0]) {
                    case KSW:
                        while (__press(i))
                            delay_us(10);
                        mode = !mode;
                        break;
                    case KAT:
                        alter = 1;
                        break;
                    default:
                        val = _asciimap[btn[0] & 0x7F];
                        usbSetKeycode(1, (val & SHIFT) ? 0x02 : 0x00);
                        usbSetKeycode(2 + col, val & 0x7F);

                        now |= (1 << col);
                        count += 1;
                        break;
                }
            } else {
                val = alter ? btn[1] : btn[0];
                val = _asciimap[val & 0x7F];
                usbSetKeycode(1, (val & SHIFT) ? 0x02 : 0x00);
                usbSetKeycode(2 + col, val & 0x7F);

                now |= (1 << col);
                count += 1;
            }
        } else {
            if (btn[1] == KNO) {
                switch (btn[0]) {
                    case KSW:
                        __asm__("nop");
                        break;
                    case KAT:
                        alter = 0;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    if (count == 0) {
        usbSetKeycode(2 + col, 0);
        now &= ~(1 << col);
    }
}

void sinScanCol(uint8_t col) {
    BCx = ~(0x01 << col);
    delay_us(500);  // RC电路充电时间需要大约 470us

    usbSetKeycode(0, 1);
    usbSetKeycode(1, shifted ? 0x02 : 0x00);

    uint8_t count = 0;
    for (uint8_t i = 0; i < 4; i++) {
        uint8_t btn = sinKeyMap[i][col];

        if (btn == KNO)
            continue;

        if (__press(i)) {
            if (btn == SFT)
                shifted = 1;
            else {
                usbSetKeycode(2 + col, _asciimap[btn & 0x7F] & 0x7F);

                now |= (1 << col);
                count += 1;
            }
        } else {
            if (btn == SFT)
                shifted = 0;
        }
    }

    if (count == 0) {
        usbSetKeycode(2 + col, 0);
        now &= ~(1 << col);
    }
}

void z81ScanCol(uint8_t col) {
    BCx = ~(0x01 << col);
    delay_us(500);  // RC电路充电时间需要大约 470us

    usbSetKeycode(0, 1);
    usbSetKeycode(1, 0);

    uint8_t count = 0;
    for (uint8_t i = 0; i < 4; i++) {
        uint8_t btn = z81KeyMap[i][col];

        if (btn == KNO)
            continue;

        if (__press(i)) {
            uint8_t code = 0;
            switch (btn) {
                case PLY:
                    code = 0xCD;
                    goto TAG_MEDIA;
                case STP:
                    code = 0xCC;
                    goto TAG_MEDIA;
                case PRE:
                    code = 0xB6;
                    goto TAG_MEDIA;
                case NEX:
                    code = 0xB5;
            TAG_MEDIA:
                    usbSetKeycode(0, 2);
                    usbSetKeycode(1, code);
                    usbSetKeycode(2, 0);
                    usbPushKeydata();
                    delay(100);
                    usbSetKeycode(1, 0);
                    usbPushKeydata();
                    delay(100);
                    usbSetKeycode(0, 1);
                    usbSetKeycode(1, 0);
                    break;
                default:
                    usbSetKeycode(2 + col, _asciimap[btn & 0x7F] & 0x7F);

                    now |= (1 << col);
                    count += 1;
                    break;
            }
        }
    }

    if (count == 0) {
        usbSetKeycode(2 + col, 0);
        now &= ~(1 << col);
    }
}

void scanKeys() {
    void (*colScanFunc)(uint8_t arg) = NULL;
    switch (MODE) {
        case 0x0:
            colScanFunc = &sinScanCol;
            break;
        case 0x1:
            colScanFunc = &biScanCol;
            break;
        case 0x2:
            colScanFunc = &biScanCol;
            break;
        case 0x3:
            colScanFunc = &z81ScanCol;
            break;
        default:
            break;
    }

    if (colScanFunc != NULL) {
        usbReleaseAll();

        for (uint8_t i = 0; i < 6; i++)
            colScanFunc(i);

        if (prev != now) {
            prev = now;

            usbPushKeydata();
        }
    }
}
