using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Test2
{
    public partial class Form1 : Form
    {
        const string stats_file = @"C:\Users\ecalf\Desktop\PUBG_Stats\pubg_stats.txt";

        LowLevelKeyboardHook kbh;

        int t_wins = 0;
        int t_kills = 0;
        int t_kos = 0;
        int t_loss = 0;

        public Form1()
        {
            InitializeComponent();

            // Initiate Keyboard Hook
            kbh = new LowLevelKeyboardHook();
            kbh.OnKeyPressed += Kbh_OnKeyPressed;
            kbh.OnKeyUnpressed += Kbh_OnKeyUnpressed;

            // Load data
            string[] lines = System.IO.File.ReadAllLines(stats_file);

            // Output current stats to textbox
            RichTextNewLine(lines[0]);
            RichTextNewLine(lines[1]);
            RichTextNewLine(lines[2]);
            RichTextNewLine(lines[3]);
            RichTextNewLine(lines[4]);

            // Update internal ints with values from file
            t_wins = Int32.Parse(lines[1].Split()[2]);
            t_kills = Int32.Parse(lines[2].Split()[2]);
            t_kos = Int32.Parse(lines[3].Split()[2]);
            t_loss = Int32.Parse(lines[4].Split()[2]);
        }

        // Do something on keypress
        private void Kbh_OnKeyPressed(object sender, Keys e)
        {
            if (e == Keys.F5)
            {
                RichTextNewLine("+Win");
                t_wins++;
            }
            else if (e == Keys.F6)
            {
                RichTextNewLine("+Kill");
                t_kills++;
            }
            else if (e == Keys.F7)
            {
                RichTextNewLine("+KO");
                t_kos++;
            }
            else if (e == Keys.F8)
            {
                RichTextNewLine("+Loss");
                t_loss++;
            }

            WriteStats();
        }

        // Write stats to file
        private void WriteStats()
        {
            string[] to_write = {
                "Today: " + DateTime.Now.ToString("M/d/yyyy"),
                "Total Wins: " + t_wins,
                "Total Kills: " + t_kills,
                "Total KO's: " + t_kos,
                "Total Losses: " + t_loss
                };

            System.IO.File.WriteAllLines(stats_file, to_write);
        }

        // AppendText to TextBox + Newline
        void RichTextNewLine(string s)
        {
            richTextBox1.AppendText(s + '\n');
        }

        // Do nothing when key unpressed
        private void Kbh_OnKeyUnpressed(object sender, Keys e)
        {
            // Do nothing
        }

        // Hook button
        private void button1_Click(object sender, EventArgs e)
        {
            kbh.HookKeyboard();
            label2.Text = "ON";
            RichTextNewLine("Hooked");
        }

        // Unhook button
        private void button2_Click(object sender, EventArgs e)
        {
            kbh.UnHookKeyboard();
            label2.Text = "OFF";
            RichTextNewLine("Unhooked");
        }
    }

    public class LowLevelKeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYUP = 0x105;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<Keys> OnKeyPressed;
        public event EventHandler<Keys> OnKeyUnpressed;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public LowLevelKeyboardHook()
        {
            _proc = HookCallback;
        }

        public void HookKeyboard()
        {
            _hookID = SetHook(_proc);
        }

        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                OnKeyPressed.Invoke(this, ((Keys)vkCode));
            }
            else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                OnKeyUnpressed.Invoke(this, ((Keys)vkCode));
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
