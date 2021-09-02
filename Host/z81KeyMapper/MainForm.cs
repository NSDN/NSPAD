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

        private const ushort MED_PLAY = 0x8001;
        private const ushort MED_STOP = 0x8002;
        private const ushort MED_PREV = 0x8003;
        private const ushort MED_NEXT = 0x8004;
        private const ushort KEY_LEFT = 0x8005;
        private const ushort KEY_UP = 0x8006;
        private const ushort KEY_RIGHT = 0x8007;
        private const ushort KEY_DOWN = 0x8008;

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
            int val = e.KeyValue;
            switch (val)
            {
                case 37:    // Left
                    val = KEY_LEFT;
                    break;
                case 38:    // Up
                    val = KEY_UP;
                    break;
                case 39:    // Right
                    val = KEY_RIGHT;
                    break;
                case 40:    // Down
                    val = KEY_DOWN;
                    break;
                case '<' | 0x80:
                    val = ',';
                    break;
                case '>' | 0x80:
                    val = '.';
                    break;
                default:
                    if (val > 0x7F)
                        return;
                    break;
            }
            if (currKey != null)
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
                switch (val)
                {
                    case 0x08:
                        str += "[Backspace]";
                        break;
                    case 0x09:
                        str += "[TAB]";
                        break;
                    case 0x0D:
                        str += "[Enter]";
                        break;
                    case 0x1B:
                        str += "[ESC]";
                        break;
                    case 0x20:
                        str += "[Space]";
                        break;
                    case KEY_LEFT:
                        str += "[Left]";
                        break;
                    case KEY_UP:
                        str += "[Up]";
                        break;
                    case KEY_RIGHT:
                        str += "[Right]";
                        break;
                    case KEY_DOWN:
                        str += "[Down]";
                        break;
                    default:
                        str += (char)(val & 0x7F);
                        break;
                }
                code |= (ushort)val;
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
                                case 0xFFFF:
                                    btn.Text = "";
                                    break;
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
                                    switch (v)
                                    {
                                        case 0x08:
                                            btn.Text += "[Backspace]";
                                            break;
                                        case 0x09:
                                            btn.Text += "[Tab]";
                                            break;
                                        case 0x0D:
                                            btn.Text += "[Enter]";
                                            break;
                                        case 0x1B:
                                            btn.Text += "[ESC]";
                                            break;
                                        case 0x20:
                                            btn.Text += "[Space]";
                                            break;
                                        case KEY_LEFT:
                                            btn.Text += "[Left]";
                                            break;
                                        case KEY_UP:
                                            btn.Text += "[Up]";
                                            break;
                                        case KEY_RIGHT:
                                            btn.Text += "[Right]";
                                            break;
                                        case KEY_DOWN:
                                            btn.Text += "[Down]";
                                            break;
                                        default:
                                            btn.Text += (char)(v & 0x7F);
                                            break;
                                    }
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
