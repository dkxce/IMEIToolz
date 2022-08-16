using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IMEI_TOOLZ
{
    // http://alter.org.ua/ru/docs/other/imei/

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string imei = "";
            while (!CheckIMEI(imei)) imei = GenerateImei();
            textBox1.Text = imei;
        }

        public bool CheckIMEI(string imei)
        {
            if (string.IsNullOrEmpty(imei)) return false;
            imei = imei.Trim();
            string sub = "";
            int total = 0;

            int length = imei.Length - 1;
            if (imei.Length == 15)
            {
                for (int i = 0; i < length; i++)
                {
                    if (i % 2 != 0)
                    {
                        sub = ((int)char.GetNumericValue(imei[i]) * 2).ToString();
                        total += sub.Length == 2 ? (int)char.GetNumericValue(sub[0]) + (int)char.GetNumericValue(sub[1]) : (int)char.GetNumericValue(sub[0]);
                    }
                    else
                        total += ((int)char.GetNumericValue(imei[i]));
                };

                if ((int)char.GetNumericValue(imei[length]) == ((((total / 10) + 1) * 10) - total))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public void SetIMEICheckSum(ref string imei)
        {
            if (string.IsNullOrEmpty(imei)) return;
            imei = imei.Trim();
            string sub = "";
            int total = 0;

            if (imei.Length == 14) imei += "0";
            int length = imei.Length - 1;
            if (imei.Length == 15)
            {
                for (int i = 0; i < length; i++)
                {
                    if (i % 2 != 0)
                    {
                        sub = ((int)char.GetNumericValue(imei[i]) * 2).ToString();
                        total += sub.Length == 2 ? (int)char.GetNumericValue(sub[0]) + (int)char.GetNumericValue(sub[1]) : (int)char.GetNumericValue(sub[0]);
                    }
                    else
                        total += ((int)char.GetNumericValue(imei[i]));
                };

                int val = ((((total / 10) + 1) * 10) - total);
                imei = imei.Remove(length) + val.ToString();
            };
        }

        public string ImeiToHex(string imei)
        {
            if (string.IsNullOrEmpty(imei)) return "";
            imei = imei.Trim();
            if (imei.Length != 15) return "";
            string res = "08 ";
            res += imei.Substring(0,1) + "A ";
            for (int i = 1; i < imei.Length; i += 2)
                res += imei.Substring(i + 1, 1) + imei.Substring(i, 1) + " ";
            return res;
        }

        public string ImeiToDec(string imei)
        {
            if (string.IsNullOrEmpty(imei)) return "";
            imei = imei.Trim();
            if (imei.Length != 15) return "";
            string res = "08 ";
            res += int.Parse(imei.Substring(0, 1) + "A", System.Globalization.NumberStyles.HexNumber).ToString() + " ";
            for (int i = 1; i < imei.Length; i += 2)
                res += int.Parse(imei.Substring(i + 1, 1) + imei.Substring(i, 1), System.Globalization.NumberStyles.HexNumber).ToString() + " ";
            return res;
        }

        public string GenerateImei()
        {
            Random rnd = new Random();
            string imei = textBox3.Text.Trim();
            while(imei.Length < 14)
                imei += (rnd.Next(0, 9)).ToString();
            SetIMEICheckSum(ref imei);
            return imei;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string imei = textBox1.Text.Trim();
            bool isok = CheckIMEI(imei);
            label1.Text = isok ? " - OK" : " - BAD";
            label1.ForeColor = isok ? Color.Green : Color.Red;
            if (!isok) return;
            textBox2.Clear();

            textBox2.Text += $"TAC: {imei.Substring(0, 2)}-{imei.Substring(2, 6)}\r\n";
            textBox2.Text += $"SNR: {imei.Substring(8, 6)}\r\n";
            textBox2.Text += $"CD: {imei.Substring(14)}\r\n";
            textBox2.Text += "\r\n";
            textBox2.Text += "NV_RAM (hex): " + ImeiToHex(imei).Trim() + "\r\n";
            textBox2.Text += "NV_RAM (hex): " + ImeiToHex(imei).Replace(" ","") + "\r\n";
            textBox2.Text += "NV_RAM (dec): " + ImeiToDec(imei).Trim() + "\r\n";
            textBox2.Text += "NV_RAM (dec): " + ImeiToDec(imei).Replace(" ", "") + "\r\n";

            btn34up();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Text = comboBox1.Items[0].ToString();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            btn34up();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btn34up();
        }
        private void btn34up()
        {
            long fl = 0;
            long tp = 0;
            try { fl = (new System.IO.FileInfo(textBox4.Text.Trim()).Length); } catch { };
            button3.Enabled = (fl > 0) && (comboBox1.Text.Trim().Replace(" ", "").Length > 0) && (comboBox1.Text.Trim().Replace(" ", "").Length % 2 == 0);
            button4.Enabled = long.TryParse(textBox5.Text.Trim().Replace(" ", ""), out tp) && (tp > 0) && (tp < fl) && CheckIMEI(textBox1.Text.Trim());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = Environment.ExpandEnvironmentVariables("%TEMP%");
            string[] files = System.IO.Directory.GetFiles(path, "*.tmp");
            string fn = "";
            textBox4.Text = "";
            foreach (string file in files)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(file);
                if (fi.CreationTime < DateTime.Now.AddHours(-10)) continue;
                if (fi.Length < 14 * 1024 * 1024) continue;
                if (fi.Length > 16 * 1024 * 1024) continue;
                textBox4.Text = file;
            };
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string f = textBox4.Text.Trim();
            try { if (!System.IO.File.Exists(f)) return; } catch { }

            string t = comboBox1.Text.Trim().Replace(" ", "");
            if (t.Length % 2 != 0) return;

            byte[] toF = new byte[t.Length / 2];
            for (int i = 0; i < toF.Length; i++)
                toF[i] = (byte)Convert.ToInt32(t.Substring(i * 2, 2), 16);

            System.IO.FileStream fs = new System.IO.FileStream(f, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            byte[] buff = new byte[16*1024*1024];
            int pos = -1;
            while(true)
            {
                int read = fs.Read(buff, 0, buff.Length);
                if(read == 0) break;
                int k = 0;
                for (pos = 0; pos < read; pos++)
                    if (buff[pos] != toF[k++]) k = 0;
                    else if (k == toF.Length) break;
            };
            if (pos == fs.Length) pos = -1;
            fs.Close();
            textBox5.Text = pos.ToString();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            btn34up();
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            btn34up();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string f = "";
            long fl = 0;
            long tp = 0;
            try { fl = (new System.IO.FileInfo(f = textBox4.Text.Trim()).Length); } catch { };
            if (fl == 0) return;
            long.TryParse(textBox5.Text.Trim().Replace(" ", ""), out tp);
            if (tp <= 0) return;
            if (tp >= fl) return;            
            string hex = ImeiToHex(textBox1.Text.Trim()).Trim().Replace(" ","");
            byte[] toR = new byte[hex.Length / 2];
            for (int i = 0; i < toR.Length; i++)
                toR[i] = (byte)Convert.ToInt32(hex.Substring(i * 2, 2), 16);

            System.IO.FileStream fs = new System.IO.FileStream(f, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);
            fs.Position = tp;
            fs.Write(toR, 0, toR.Length);
            fs.Close();
            MessageBox.Show($"{toR.Length} bytes to {System.IO.Path.GetFileName(f)} has been writed", "Modify IMEI");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach(System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            {
                if (p.ProcessName.ToLower().StartsWith("imei-recovery"))
                {
                    MessageBox.Show($"Found {p.Id} as {p.ProcessName}", "Huawei E173 IMEI Recovery");
                    return;
                };                
            };
            MessageBox.Show($"Not Found Any Process", "Huawei E173 IMEI Recovery");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            {
                if (p.ProcessName.ToLower().StartsWith("imei-recovery"))
                {
                    bool ok = ProcSleeper.SuspendProcess(p.Id);
                    MessageBox.Show($"{p.ProcessName}\r\nProcess {p.Id} " + (ok ? " has bin Paused " : " Error"), "Huawei E173 IMEI Recovery");
                    return;
                };
            };
            MessageBox.Show($"Not Found Any Process", "Huawei E173 IMEI Recovery");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            {
                if (p.ProcessName.ToLower().StartsWith("imei-recovery"))
                {
                    bool ok = ProcSleeper.ResumeProcess(p.Id);
                    MessageBox.Show($"{p.ProcessName}\r\nProcess {p.Id} " + (ok ? " has bin Resumed " : " Error"), "Huawei E173 IMEI Recovery");
                    return;
                };
            };
            MessageBox.Show($"Not Found Any Process", "Huawei E173 IMEI Recovery");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try { Process.Start(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imei-recovery-template-huawei-e173-353238856958110.exe")); } catch { };
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try { Process.Start(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dc-unlocker2client.exe")); } catch { };
        }
    }

    public class ProcSleeper
    {
        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        private static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);


        public static bool SuspendProcess(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid); // throws exception if process does not exist
                foreach (ProcessThread pT in process.Threads)
                {
                    IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);
                    if (pOpenThread == IntPtr.Zero) continue;
                    SuspendThread(pOpenThread);
                    CloseHandle(pOpenThread);
                    return true;
                };
            }
            catch { };
            return false;
        }

        public static bool ResumeProcess(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                if (process.ProcessName == string.Empty) return false;
                foreach (ProcessThread pT in process.Threads)
                {
                    IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);
                    if (pOpenThread == IntPtr.Zero) continue;
                    int suspendCount = 0;
                    do { suspendCount = ResumeThread(pOpenThread); }
                    while (suspendCount > 0);
                    CloseHandle(pOpenThread);
                    return true;
                }
            }
            catch { };
            return false;
        }
    }
}
