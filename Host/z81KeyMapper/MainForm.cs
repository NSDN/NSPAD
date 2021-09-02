using System;
using System.Windows.Forms;

using PADLib;

namespace z81KeyMapper
{
    public partial class MainForm : Form
    {
        private const ushort MOD_CTRL = 0x0100;
        private const ushort MOD_SHIFT = 0x0200;
        private const ushort MOD_ALT = 0x0400;

        private const ushort MED_PLAY = 0x0081;
        private const ushort MED_STOP = 0x0082;
        private const ushort MED_PREV = 0x0083;
        private const ushort MED_NEXT = 0x0084;

        private NSPAD pad;

        private int currR, currC;
        private Button currKey;
        private ushort[][] keyConf;
        private string prevKeyText;

        public MainForm()
        {
            InitializeComponent();

            pad = new NSPAD();

            currR = -1; currC = -1;
            currKey = null;
            keyConf = new ushort[4][];
            for (int i = 0; i < 4; i++)
            {
                keyConf[i] = new ushort[6];
                for (int j = 0; j < 6; j++)
                    keyConf[i][j] = 0x0000;
            }
            prevKeyText = null;
        }

        private void BtnXX_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                string args = button.Name.Replace("btn", "");
                if (!int.TryParse(args[0].ToString(), out currR))
                    currR = -1;
                if (!int.TryParse(args[1].ToString(), out currC))
                    currC = -1;
                if (currR >= 0 && currC >= 0)
                {
                    if (currKey != null)
                        if (prevKeyText != null)
                        {
                            currKey.Text = prevKeyText;
                            prevKeyText = null;
                        }

                    currKey = button;

                    if (prevKeyText == null)
                        prevKeyText = currKey.Text;

                    currKey.Text = "等待按下...";
                }
                else
                    currKey = null;
            }
        }

        private void BtnXX_KeyMod(object sender, KeyEventArgs e)
        {
            if (currKey != null && e.KeyValue <= 0x7F)
            {
                string str = ""; ushort code = 0x0000;
                if (e.Control)
                {
                    str += "Ctrl + ";
                    code |= MOD_CTRL;
                }
                if (e.Alt)
                {
                    str += "Alt + ";
                    code |= MOD_ALT;
                }
                if (e.Shift)
                {
                    str += "Shift + ";
                    code |= MOD_SHIFT;
                }
                byte k = (byte)(e.KeyValue & 0x7F);

                str += (char)k;
                code |= k;
                currKey.Text = str;
                keyConf[currR][currC] = code;

                currKey = null;
                currR = -1; currC = -1;
            }
        }

        private void btnPull_Click(object sender, EventArgs e)
        {
            btnPull.Enabled = false;
            var conf = pad.ReadConfig();
            if (conf != null)
            {
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 6; j++)
                    {
                        var v = keyConf[i][j] = conf[i][j];
                        if (tableKeys.Controls[$"btn{i}{j}"] is Button btn)
                        {
                            switch (v)
                            {
                                case MED_PLAY:
                                    btn.Text = "[播放]";
                                    break;
                                case MED_STOP:
                                    btn.Text = "[停止]";
                                    break;
                                case MED_PREV:
                                    btn.Text = "[上一曲]";
                                    break;
                                case MED_NEXT:
                                    btn.Text = "[下一曲]";
                                    break;
                                default:
                                    btn.Text = "";
                                    if ((v & MOD_CTRL) != 0)
                                        btn.Text += "Ctrl + ";
                                    if ((v & MOD_ALT) != 0)
                                        btn.Text += "Alt + ";
                                    if ((v & MOD_SHIFT) != 0)
                                        btn.Text += "Shift + ";
                                    btn.Text += (char)(v & 0x7F);
                                    break;
                            }
                        }
                    }

                currKey = null;
                currR = -1; currC = -1;
            }
            btnPull.Enabled = true;
        }

        private void btnPush_Click(object sender, EventArgs e)
        {
            btnPush.Enabled = false;
            pad.WriteConfig(keyConf);
            btnPush.Enabled = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (currKey != null)
            {
                prevKeyText = null;
                currKey.Text = "";
                keyConf[currR][currC] = 0x0000;

                currKey = null;
                currR = -1; currC = -1;
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (currKey != null)
            {
                prevKeyText = null;
                currKey.Text = "[播放]";
                keyConf[currR][currC] = MED_PLAY;

                currKey = null;
                currR = -1; currC = -1;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (currKey != null)
            {
                prevKeyText = null;
                currKey.Text = "[停止]";
                keyConf[currR][currC] = MED_STOP;

                currKey = null;
                currR = -1; currC = -1;
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (currKey != null)
            {
                prevKeyText = null;
                currKey.Text = "[上一曲]";
                keyConf[currR][currC] = MED_PREV;

                currKey = null;
                currR = -1; currC = -1;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currKey != null)
            {
                prevKeyText = null;
                currKey.Text = "[下一曲]";
                keyConf[currR][currC] = MED_NEXT;

                currKey = null;
                currR = -1; currC = -1;
            }
        }
    }
}
