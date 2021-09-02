using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

using PADCore;
using HID;

namespace PADHost
{
    public partial class MainForm : Form
    {
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

            PADASM.SetOutput((obj) => outputBox.Text += obj.ToString());
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            outputBox.Clear();
            PADASM core = PADASM.GetExecutor(codeBox.Text);
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
