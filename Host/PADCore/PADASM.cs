using System;
using System.Collections.Generic;

using dotNSASM;

namespace PADCore
{
    public class PADASM : NSASM
    {
        public static void SetOutput(Util.Printer printer)
        {
            Util.Output = printer;
        }

        readonly List<byte[]> byteCode;

        PADASM(string[][] code) : base(16, 16, 8, code)
        {
            byteCode = new List<byte[]>();
        }

        public static PADASM GetExecutor(string code)
        {
            // 防止仅使用了无操作数指令时, Run() 返回 null (即 prevDstReg 为 null)
            code += "\n___ \"END OF CODE\"\n";
            var c = Util.GetSegments(code);
            return new PADASM(c);
        }

        protected override NSASM Instance(NSASM super, string[][] code)
        {
            return new PADASM(code);
        }

        public byte[][] GetBytes()
        {
            return byteCode.ToArray();
        }

        public new Register Run()
        {
            byteCode.Clear();
            return base.Run();
        }

        protected override void LoadFuncList()
        {
            base.LoadFuncList();

            funcList.Add("___", (dst, src, ext) =>
            {
                return Result.OK;
            });

            funcList.Add(".nop", (dst, src, ext) =>
            {
                if (src != null) return Result.ERR;
                if (dst != null) return Result.ERR;

                byteCode.Add(new byte[] { 0x00 });
                return Result.OK;
            });

            funcList.Add(".jmp", (dst, src, ext) =>
            {
                if (src != null) return Result.ERR;
                if (dst == null) return Result.ERR;
                if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                    return Result.ERR;

                byte d = (byte)((int)dst.data & 0xFF);
                byteCode.Add(new byte[] { 0x01, d });
                return Result.OK;
            });

            funcList.Add(".clr", (dst, src, ext) =>
            {
                if (src != null) return Result.ERR;
                if (dst != null) return Result.ERR;

                byteCode.Add(new byte[] { 0x02 });
                return Result.OK;
            });

            funcList.Add(".prt", (dst, src, ext) =>
            {
                if (src != null) return Result.ERR;
                if (dst == null) return Result.ERR;
                if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                    return Result.ERR;

                byte d = (byte)((int)dst.data & 0xFF);
                byteCode.Add(new byte[] { 0x03, d });
                return Result.OK;
            });

            funcList.Add(".hidp", (dst, src, ext) =>
            {
                if (src != null) return Result.ERR;
                if (dst != null) return Result.ERR;

                byteCode.Add(new byte[] { 0x04 });
                return Result.OK;
            });

            funcList.Add(".strp", (dst, src, ext) =>
            {
                if (src != null) return Result.ERR;
                if (dst == null) return Result.ERR;
                if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                    return Result.ERR;

                byte d = (byte)((int)dst.data & 0xFF);
                byteCode.Add(new byte[] { 0x05, d });
                return Result.OK;
            });

            funcList.Add(".out", (dst, src, ext) =>
            {
                if (src == null) return Result.ERR;
                if (dst == null) return Result.ERR;
                if (src.type != RegType.CHAR && src.type != RegType.INT)
                    return Result.ERR;
                if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                    return Result.ERR;

                ushort s = (ushort)((int)src.data & 0xFFFF);
                byte d = (byte)((int)dst.data & 0xFF);
                byteCode.Add(new byte[] { 0x06, d, (byte)(s & 0xFF), (byte)(s >> 8) });
                return Result.OK;
            });

            funcList.Add(".keyp", (dst, src, ext) =>
            {
                if (src != null) return Result.ERR;
                if (dst != null) return Result.ERR;

                byteCode.Add(new byte[] { 0x07 });
                return Result.OK;
            });

            funcList.Add(".ldi", (dst, src, ext) =>
            {
                if (src == null) return Result.ERR;
                if (dst == null) return Result.ERR;
                if (src.type != RegType.CHAR && src.type != RegType.INT)
                    return Result.ERR;
                if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                    return Result.ERR;

                ushort s = (ushort)((int)src.data & 0xFFFF);
                byte d = (byte)((int)dst.data & 0xFF);
                byteCode.Add(new byte[] { 0x08, d, (byte)(s & 0xFF), (byte)(s >> 8) });
                return Result.OK;
            });

            funcList.Add(".wri", (dst, src, ext) =>
            {
                if (src == null) return Result.ERR;
                if (dst == null) return Result.ERR;
                if (src.type != RegType.CHAR && src.type != RegType.INT)
                    return Result.ERR;
                if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                    return Result.ERR;

                byte s = (byte)((int)src.data & 0xFF);
                ushort d = (ushort)((int)dst.data & 0xFFFF);
                byteCode.Add(new byte[] { 0x09, s, (byte)(d & 0xFF), (byte)(d >> 8) });
                return Result.OK;
            });

            funcList.Add(".erase", (dst, src, ext) =>
            {
                if (src == null) return Result.ERR;
                if (dst == null) return Result.ERR;
                if (src.type != RegType.CHAR && src.type != RegType.INT)
                    return Result.ERR;
                if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                    return Result.ERR;

                ushort s = (ushort)((int)src.data & 0xFFFF);
                byte d = (byte)((int)dst.data & 0xFF);
                byteCode.Add(new byte[] { 0x0A, d, (byte)(s & 0xFF), (byte)(s >> 8) });
                return Result.OK;
            });

            funcList.Add(".sleep", (dst, src, ext) =>
            {
                if (src != null) return Result.ERR;
                if (dst == null) return Result.ERR;
                if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                    return Result.ERR;

                byte d = (byte)((int)dst.data & 0xFFFF);
                byteCode.Add(new byte[] { 0x0B, 0x00, (byte)(d & 0xFF), (byte)(d >> 8) });
                return Result.OK;
            });

            funcList.Add(".sysrst", (dst, src, ext) =>
            {
                if (src == null) return Result.ERR;
                if (dst == null) return Result.ERR;
                if (src.type != RegType.CHAR && src.type != RegType.INT)
                    return Result.ERR;
                if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                    return Result.ERR;

                ushort s = (ushort)((int)src.data & 0xFFFF);
                byte d = (byte)((int)dst.data & 0xFF);
                byteCode.Add(new byte[] { 0x0C, d, (byte)(s & 0xFF), (byte)(s >> 8) });
                return Result.OK;
            });

            funcList.Add(".reload", (dst, src, ext) =>
            {
                if (src != null) return Result.ERR;
                if (dst != null) return Result.ERR;

                byteCode.Add(new byte[] { 0x0D });
                return Result.OK;
            });
        }

        protected override void LoadParamList()
        {
            base.LoadParamList();
        }
    }
}
