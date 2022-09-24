using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace ProcessManipulation2
{
    public partial class Form1 : Form
    {
        const uint WM_SETTEXT = 0x0C;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd,
                                uint Msg, int wParam,
        [MarshalAs(UnmanagedType.LPStr)] string lParam);
        List<Process> Processes = new List<Process>();
        int Counter = 0;
        delegate void ProcessDelegate(Process proc);
        public Form1()
        {
            InitializeComponent();
            LoadAvailableAssemblies();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            RunProcess(AvailableAssemblies.SelectedItem.ToString());
        }
        void LoadAvailableAssemblies()
        {
            //название файла сборки текущего приложения
            string except = new FileInfo(Application.ExecutablePath).Name;
            //получаем название файла без расширения
            except = except.Substring(0, except.IndexOf("."));
            //получаем все *.exe файлы из домашней
            //директории
            string[] files = Directory.GetFiles(Application.StartupPath, "*.exe");
            foreach (var file in files)
            {
                //получаем имя файла
                string fileName = new FileInfo(file).Name;
                /*если имя файла не содержит имени исполняемого
                *файла проекта, то оно добавляется в список*/
                if (fileName.IndexOf(except) == -1)
                    AvailableAssemblies.Items.Add(fileName);
            }
        }
        void RunProcess(string AssamblyName)
        {
            //запускаем процесс на соновании исполняемого
            //файла
            Process proc = Process.Start(AssamblyName);
            //добавляем процесс в список
            Processes.Add(proc);
            /*проверяем, стал ли созданный процесс дочерним,
            *по отношению к текущему и, если стал, выводим
            *MessageBox*/
            if (Process.GetCurrentProcess().Id == GetParentProcessId(proc.Id))
                MessageBox.Show(proc.ProcessName +
                    " действительно дочерний процесс текущего процесса!");
            proc.EnableRaisingEvents = true;
            proc.Exited += proc_Exited;
            SetChildWindowText(proc.MainWindowHandle, "Child process #" + (++Counter));

            if (!StartedAssemblies.Items.Contains(proc.ProcessName))
                StartedAssemblies.Items.Add(proc.ProcessName);
            /*убираем приложение из списка доступных
            *приложений*/
            StartedAssemblies.Items.Remove(StartedAssemblies.SelectedItem);
        }
        void SetChildWindowText(IntPtr Handle, string text)
        {
            SendMessage(Handle, WM_SETTEXT, 0, text);
        }
        int GetParentProcessId(int Id)
        {
            int parentId = 0;
            using (ManagementObject obj =
            new ManagementObject("win32_process.handle=" +
            Id.ToString()))
            {
                obj.Get();
                parentId = Convert.ToInt32(obj["ParentProcessId"]);
            }
            return parentId;
        }
        void proc_Exited(object sender, EventArgs e)
        {
            Process proc = sender as Process;
            //убираем процесс из списка запущенных
            //приложений
            StartedAssemblies.Items.Remove(proc.ProcessName);
            //добавляем процесс в список доступных приложений
            //AvailableAssemblies.Items.Add(proc.ProcessName);
            //убираем процесс из списка дочерних процессов
            Processes.Remove(proc);
            //уменьшаем счётчик дочерних процессов на 1
            Counter--;
            int index = 0;
            /*меняем текст для главных окон всех дочерних процессов*/
            foreach (var p in Processes)
                SetChildWindowText(p.MainWindowHandle, "Child process #" + ++index);
        }
        void ExecuteOnProcessesByName(string ProcessName, ProcessDelegate func)
        {
            Process[] processes = Process.
            GetProcessesByName(ProcessName);
            foreach (var process in processes)
                if (Process.GetCurrentProcess().Id ==
                GetParentProcessId(process.Id))
                func(process);
        }
        void Kill(Process proc)
        {
            proc.Kill();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ExecuteOnProcessesByName(StartedAssemblies.SelectedItem.ToString(), Kill);
            StartedAssemblies.Items.Remove(StartedAssemblies.SelectedItem);
        }
        void CloseMainWindow(Process proc)
        {
            proc.CloseMainWindow();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ExecuteOnProcessesByName(StartedAssemblies.SelectedItem.ToString(),
                                        CloseMainWindow);
            StartedAssemblies.Items.Remove(StartedAssemblies.SelectedItem);
        }
        void Refresh(Process proc)
        {
            proc.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ExecuteOnProcessesByName(StartedAssemblies.SelectedItem.ToString(), Refresh);
        }

        private void AvailableAssemblies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AvailableAssemblies.SelectedItems.Count == 0)
                button1.Enabled = false;
            else
                button1.Enabled = true;
        }

        private void StartedAssemblies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (StartedAssemblies.SelectedItems.Count == 0)
            {
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled= false;
            }
            else
            {
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var proc in Processes)
                proc.Kill();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            RunProcess("calc.exe");
        }
    }
}
