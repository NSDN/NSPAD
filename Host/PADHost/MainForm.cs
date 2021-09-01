using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

using dotNSASM;
using HID;

namespace PADHost
{
    public partial class MainForm : Form
    {
        class LogCore : NSASM
        {
            readonly List<byte[]> byteCode;

            LogCore(string[][] code) : base(16, 16, 8, code)
            {
                byteCode = new List<byte[]>();
            }

            public static LogCore GetSimCore(string code)
            {
                // 防止仅使用了无操作数指令时, Run() 返回 null (即 prevDstReg 为 null)
                code += "\n___ \"END OF CODE\"\n";
                var c = Util.GetSegments(code);
                return new LogCore(c);
            }

            protected override NSASM Instance(NSASM super, string[][] code)
            {
                return new LogCore(code);
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
                funcList.Add("___", (dst, src, ext) =>
                {
                    return Result.OK;
                });

                funcList.Add("nop", (dst, src, ext) =>
                {
                    if (src != null) return Result.ERR;
                    if (dst != null) return Result.ERR;

                    byteCode.Add(new byte[] { 0x00 });
                    return Result.OK;
                });

                funcList.Add("jmp", (dst, src, ext) =>
                {
                    if (src != null) return Result.ERR;
                    if (dst == null) return Result.ERR;
                    if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                        return Result.ERR;

                    byte d = (byte)((int)dst.data & 0xFF);
                    byteCode.Add(new byte[] { 0x01, d });
                    return Result.OK;
                });

                funcList.Add("clr", (dst, src, ext) =>
                {
                    if (src != null) return Result.ERR;
                    if (dst != null) return Result.ERR;

                    byteCode.Add(new byte[] { 0x02 });
                    return Result.OK;
                });

                funcList.Add("prt", (dst, src, ext) =>
                {
                    if (src != null) return Result.ERR;
                    if (dst == null) return Result.ERR;
                    if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                        return Result.ERR;

                    byte d = (byte)((int)dst.data & 0xFF);
                    byteCode.Add(new byte[] { 0x03, d });
                    return Result.OK;
                });

                funcList.Add("hidp", (dst, src, ext) =>
                {
                    if (src != null) return Result.ERR;
                    if (dst != null) return Result.ERR;

                    byteCode.Add(new byte[] { 0x04 });
                    return Result.OK;
                });

                funcList.Add("strp", (dst, src, ext) =>
                {
                    if (src != null) return Result.ERR;
                    if (dst == null) return Result.ERR;
                    if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                        return Result.ERR;

                    byte d = (byte)((int)dst.data & 0xFF);
                    byteCode.Add(new byte[] { 0x05, d });
                    return Result.OK;
                });

                funcList.Add("out", (dst, src, ext) =>
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

                funcList.Add("keyp", (dst, src, ext) =>
                {
                    if (src != null) return Result.ERR;
                    if (dst != null) return Result.ERR;

                    byteCode.Add(new byte[] { 0x07 });
                    return Result.OK;
                });

                funcList.Add("ldi", (dst, src, ext) =>
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

                funcList.Add("wri", (dst, src, ext) =>
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

                funcList.Add("erase", (dst, src, ext) =>
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

                funcList.Add("sleep", (dst, src, ext) =>
                {
                    if (src != null) return Result.ERR;
                    if (dst == null) return Result.ERR;
                    if (dst.type != RegType.CHAR && dst.type != RegType.INT)
                        return Result.ERR;

                    byte d = (byte)((int)dst.data & 0xFFFF);
                    byteCode.Add(new byte[] { 0x0B, 0x00, (byte)(d & 0xFF), (byte)(d >> 8) });
                    return Result.OK;
                });

                funcList.Add("sysrst", (dst, src, ext) =>
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
            }

            protected override void LoadParamList()
            {
            
            }
        }

        Hid hid;
        IntPtr ptr;

        const int BUF_SIZE = 42;

        public MainForm()
        {
            InitializeComponent();
            devList.SelectedIndex = 0;

            hid = new Hid();
            hid.DataReceived += Hid_DataReceived;
            hid.DeviceRemoved += Hid_DeviceRemoved;

            ptr = new IntPtr(-1);

            Util.Output = (obj) => outputBox.Text += obj.ToString();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            outputBox.Clear();
            LogCore core = LogCore.GetSimCore(codeBox.Text);
            var result = core.Run();
            if (result != null)
            {
                byte[][] bytes = core.GetBytes();
                outputBox.Text += "Code: ";
                for (int i = 0; i < bytes.Length; i++)
                    for (int j = 0; j < bytes[i].Length; j++)
                        outputBox.Text += (bytes[i][j].ToString("x2") + " ");
                outputBox.Text += "\n";

                if ((int)ptr != -1)
                {
                    outputBox.Text += "Sending ...\n";

                    new Thread(new ThreadStart(() =>
                    {
                        Report report; int index = 0;
                        List<byte> buf = new List<byte>();
                        while (index < bytes.Length)
                        {
                            if (buf.Count + bytes[index].Length < BUF_SIZE)
                            {
                                buf.AddRange(bytes[index]);
                                index += 1;
                                if (index >= bytes.Length)
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
            }
        }

        bool debug = false;

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Connect")
            {
                ushort vid = 0x3232; byte mid = 0x02;
                ushort pid = 0xFFFF;
                switch (devList.SelectedIndex)
                {
                    case 0: // NSPAD v1.0
                        pid = 52;
                        debug = false;
                        break;
                    case 1: // NSPAD v1.0 Debug
                        pid = 52;
                        debug = true;
                        break;
                    default:
                        break;
                }
                ptr = hid.OpenDevice(vid, pid, mid);
                if ((int)ptr != -1)
                {
                    outputBox.Clear();
                    outputBox.Text += ("Connected to: " + devList.SelectedItem + "\n");
                    devList.Enabled = false;
                    btnConnect.Text = "Close";
                }
            }
            else
            {
                if ((int)ptr != -1)
                    hid.CloseDevice(ptr);
                ptr = new IntPtr(-1);
                devList.Enabled = true;
                btnConnect.Text = "Connect";
                debug = false;
            }
        }

        private void Hid_DeviceRemoved(object sender, EventArgs e)
        {
            Invoke(new ThreadStart(() => {
                ptr = new IntPtr(-1);
                devList.Enabled = true;
                btnConnect.Text = "Connect";
                debug = false;
            }));
        }

        private void Hid_DataReceived(object sender, Report e)
        {
            byte[] bytes = e.reportBuff;
            Invoke(new ThreadStart(() => {
                if (debug)
                    outputBox.Text = "Data: ";
                else
                    outputBox.Text += "Data: ";
                for (int i = 0; i < bytes.Length; i++)
                    outputBox.Text += (bytes[i].ToString("x2") + " ");
                outputBox.Text += "\n";
            }));
        }
    }
}
