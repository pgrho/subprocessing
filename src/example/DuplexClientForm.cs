using Shipwreck.Subprocessing.Example.Properties;
using Shipwreck.Subprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shipwreck.Subprocessing.Example
{
    public partial class DuplexClientForm : Form, IDuplexCallback
    {
        public static void EntryPoint(IDuplexSubprocessStartInfo<IDuplexService, IDuplexCallback> startInfo)
        {
            try
            {
                Helper.PrintEntryPoint();
                Application.EnableVisualStyles();
                Application.Run(new DuplexClientForm(startInfo));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }

        IDuplexSubprocessStartInfo<IDuplexService, IDuplexCallback> _StartInfo;

        public DuplexClientForm(IDuplexSubprocessStartInfo<IDuplexService, IDuplexCallback> startInfo)
        {
            InitializeComponent();
            _StartInfo = startInfo;
            leftOperand.Value = Settings.Default.AppSettingItem;
            rightOperand.Value = Settings.Default.UserSettingItem;
        }

        public void Report(int c)
        {
            Console.WriteLine("Reported:{0}", c);
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => label1.Text = c.ToString()));
            }
            else
            {
                label1.Text = c.ToString();
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            EnsureOpened();
            var r = _StartInfo.Service.Add((int)leftOperand.Value, (int)rightOperand.Value);
            Console.WriteLine("Add Result:{0}", r);
        }

        private void EnsureOpened()
        {
            if (!_StartInfo.IsOpened)
            {
                _StartInfo.Open(this);
            }
        }

        private void subtractButton_Click(object sender, EventArgs e)
        {
            EnsureOpened();
            int r;
            _StartInfo.Service.Subtract(null, (int)leftOperand.Value, (int)rightOperand.Value, out r);
            Console.WriteLine("Subtract Output Parameter:{0}", r);
        }

        private void multiplyButton_Click(object sender, EventArgs e)
        {
            EnsureOpened();
            _StartInfo.Service.Multipy((int)leftOperand.Value, null, (int)rightOperand.Value);
        }

        private void divideButton_Click(object sender, EventArgs e)
        {
            EnsureOpened();
            _StartInfo.Service.Divide((int)leftOperand.Value, 0, (int)rightOperand.Value, null);
        }
    }
}
