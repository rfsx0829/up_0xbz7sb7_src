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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace pvz
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        const int PROCESS_ALL_ACCESS = 0x001FFFFF;
        const int baseaddr = 0x6A9EC0; // 基址
        const int offset_1 = 0x768;
        const int offset_2 = 0x5560;

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Process processes = Process.GetProcessesByName("PlantsVsZombies(原版启动)")[0];
                int handle = API.OpenProcess(PROCESS_ALL_ACCESS, false, processes.Id).ToInt32();
                (new func(handle)).Show();
            }
            catch(IndexOutOfRangeException)
            {
                MessageBox.Show("请先打开游戏");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }
    }

    class API
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, out int lpBuffer, int nSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, out int lpBuffer, int nSize, out int lpNumberOfBytesWrite);
    }
}
