using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Windows.Threading;

namespace Zadanie1
{
    public partial class MainForm : Form
    {
        List<Process> Processes = new List<Process>();
        public MainForm()
        {
            InitializeComponent();
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            RunProcess("calc.exe");
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            Process proc = Processes[0];
            proc.Kill();
        }
        void RunProcess(string AssamblyName)
        {
            Process proc = Process.Start(AssamblyName);
            Processes.Add(proc);
            if (Process.GetCurrentProcess().Id == GetParentProcessId(proc.Id))
            {
                proc.EnableRaisingEvents = true;
                proc.Exited += proc_Exited;
                
            }
        }
        void proc_Exited(object sender, EventArgs e)
        {
            Process proc = sender as Process;
            IDCloseProcess.Invoke(new Action(() => IDCloseProcess.Text=proc.ExitCode.ToString()));

            Processes.Remove(proc);
        }
        int GetParentProcessId(int Id)
        {
            int parentId = 0;
            using (ManagementObject obj =
            new ManagementObject("win32_process.handle=" + Id.ToString()))
            {
                obj.Get();
                parentId = Convert.ToInt32(obj["ParentProcessId"]);
            }
            return parentId;
        }
    }
}
