using HID;
using PADCore;

using System;
using System.Threading;
using System.Collections.Generic;

namespace PADLib
{
    public class NSPAD
    {
        Hid hid;
        IntPtr ptr;

        List<byte[]> recBytes;

        const int BUF_SIZE = 42;

        public bool Connected
        {
            get;            
            private set;
        }

        public NSPAD()
        {
            hid = new Hid();
            hid.DataReceived += Hid_DataReceived;
            hid.DeviceRemoved += Hid_DeviceRemoved;

            ptr = new IntPtr(-1);
            recBytes = new List<byte[]>();
        }

        ~NSPAD()
        {
            Close();
        }

        private void Hid_DeviceRemoved(object sender, EventArgs e)
        {
            Connected = false;
        }

        private void Hid_DataReceived(object sender, Report e)
        {
            byte[] bytes = e.reportBuff;
            recBytes.Add(bytes);
        }

        protected void Connect()
        {
            ushort vid = 0x3232; byte mid = 0x02;
            ushort pid = 52;
            ptr = hid.OpenDevice(vid, pid, mid);
            if ((int)ptr != -1)
            {
                Connected = true;
            }
        }

        protected void Close()
        {
            if ((int)ptr != -1)
                hid.CloseDevice(ptr);
            ptr = new IntPtr(-1);
        }

        protected void SendCmds(params byte[][] cmds)
        {
            new Thread(new ThreadStart(() =>
            {
                Report report; int index = 0;
                List<byte> buf = new List<byte>();
                while (index < cmds.Length)
                {
                    if (buf.Count + cmds[index].Length < BUF_SIZE)
                    {
                        buf.AddRange(cmds[index]);
                        index += 1;
                        if (index >= cmds.Length)
                        {
                            report = new Report(0x55, buf.ToArray());
                            hid.Write(report);
                            buf.Clear();
                            break;
                        }
                    }
                    else
                    {
                        report = new Report(0x55, buf.ToArray());
                        hid.Write(report);
                        Thread.Sleep(500);
                        buf.Clear();
                    }
                }
            })).Start();
        }

        public ushort[][] ReadConfig()
        {
            if (!Connected)
                Connect();

            if (!Connected)
                return null;

            recBytes.Clear();
            PADASM exe = PADASM.GetExecutor(
                "  .clr" + "\n" +
                "  mov r0, 2" + "\n" +
                "  mov r1, 0" + "\n" +
                "[head]" + "\n" +
                "  add r3, r0, r1" + "\n" +
                "  .ldi r1, r3" + "\n" +
                "  loop r1, 16, [head]" + "\n" +
                "  .hidp" + "\n" +
                "  end"
            );
            if (exe.Run() == null)
                return null;
            SendCmds(exe.GetBytes());
            while (recBytes.Count < 1)
                Thread.Sleep(1);

            exe = PADASM.GetExecutor(
                "  .clr" + "\n" +
                "  mov r0, 18" + "\n" +
                "  mov r1, 0" + "\n" +
                "[head]" + "\n" +
                "  add r3, r0, r1" + "\n" +
                "  .ldi r1, r3" + "\n" +
                "  loop r1, 16, [head]" + "\n" +
                "  .hidp" + "\n" +
                "  end"
            );
            if (exe.Run() == null)
                return null;
            SendCmds(exe.GetBytes());
            while (recBytes.Count < 2)
                Thread.Sleep(1);

            exe = PADASM.GetExecutor(
                "  .clr" + "\n" +
                "  mov r0, 34" + "\n" +
                "  mov r1, 0" + "\n" +
                "[head]" + "\n" +
                "  add r3, r0, r1" + "\n" +
                "  .ldi r1, r3" + "\n" +
                "  loop r1, 16, [head]" + "\n" +
                "  .hidp" + "\n" +
                "  end"
            );
            if (exe.Run() == null)
                return null;
            SendCmds(exe.GetBytes());
            while (recBytes.Count < 3)
                Thread.Sleep(1);

            int ptr = 0;
            ushort[][] conf = new ushort[4][];
            for (int i = 0; i < 4; i++)
            {
                conf[i] = new ushort[6];
                for (int j = 0; j < 6; j++)
                {
                    conf[i][j] = recBytes[ptr / 16][ptr % 16];
                    conf[i][j] |= (ushort)(recBytes[ptr / 16][ptr % 16 + 1] << 8);
                    ptr += 2;
                }
            }
            return conf;
        }

        public void WriteConfig(ushort[][] conf)
        {
            if (!Connected)
                Connect();

            if (!Connected)
                return;

            string code = "";
            code += ".erase 0xAA, 0xAA55" + "\n";
            code += ".wri 0, 0xA5" + "\n";
            code += ".wri 1, 0xA5" + "\n";

            int ptr = 2;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    ushort v = conf[i][j];
                    code += $".wri {ptr}, {v & 0xFF}" + "\n";
                    code += $".wri {ptr + 1}, {v >> 8}" + "\n";
                    ptr += 2;
                }
            }

            code += ".sysrst 0x55, 0x55AA" + "\n";
            PADASM exe = PADASM.GetExecutor(code);
            if (exe.Run() == null)
                return;
            SendCmds(exe.GetBytes());
        }
    }
}
