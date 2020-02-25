using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace pvz
{
    public partial class func : Form
    {
        public func()
        {
            InitializeComponent();
        }

        public func(int pid)
        {
            InitializeComponent();
            this.pid = pid;
        }

        private static int baseaddr = 0x6A9EC0;
        private static int sunshine_offset_1 = 0x768;
        private static int sunshine_offset_2 = 0x5560;
        private static int coin_offset_1 = 0x82C;
        private static int coin_offset_2 = 0x28;
        private static int cooltime_offset_1 = 0x768;
        private static int cooltime_offset_2 = 0x144;
        private static int cooltime_offset_3 = 0x4C;
        private static int cooltime_step = 0x50;
        private static int block_nums = 10;
        private int pid = 0;
        private bool timer_isrunning = false;

        public static void CheatSunshine(int pid, int targetvalue)
        {
            int temp = 0, tmp = 0;
            API.ReadProcessMemory(pid, baseaddr, out temp, sizeof(int), out tmp);
            API.ReadProcessMemory(pid, temp + sunshine_offset_1, out temp, sizeof(int), out tmp);
            API.WriteProcessMemory(pid, temp + sunshine_offset_2, out targetvalue, sizeof(int), out tmp);
        }

        public static void CheatCoin(int pid, int targetvalue)
        {
            int temp = 0, tmp = 0;
            API.ReadProcessMemory(pid, baseaddr, out temp, sizeof(int), out tmp);
            API.ReadProcessMemory(pid, temp + coin_offset_1, out temp, sizeof(int), out tmp);
            API.WriteProcessMemory(pid, temp + coin_offset_2, out targetvalue, sizeof(int), out tmp);
        }

        public static void CheatCooltime(int pid, int targetvalue)
        {
            int temp = 0, tmp = 0;
            API.ReadProcessMemory(pid, baseaddr, out temp, sizeof(int), out tmp);
            API.ReadProcessMemory(pid, temp + cooltime_offset_1, out temp, sizeof(int), out tmp);
            API.ReadProcessMemory(pid, temp + cooltime_offset_2, out temp, sizeof(int), out tmp);
            temp += cooltime_offset_3;
            for (int i = 0; i < block_nums; ++i)
            {
                API.WriteProcessMemory(pid, temp + i * cooltime_step, out targetvalue, sizeof(int), out tmp);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CheatSunshine(this.pid, int.Parse(textBox1.Text));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CheatCooltime(this.pid, 10000);
            Thread.Sleep(500);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CheatCoin(this.pid, int.Parse(textBox2.Text));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (timer_isrunning)
            {
                timer1.Stop();
                label4.Text = "未开启";
                button3.Text = "开启";
            } else
            {
                timer1.Start();
                label4.Text = "已开启";
                button3.Text = "关闭";
            }
            timer_isrunning = !timer_isrunning;
        }
    }
}
