using Shipwreck.Subprocessing.Example.Properties;
using Shipwreck.Subprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Shipwreck.Subprocessing.Example
{
    public partial class ServiceForm : Form
    {
        private DuplexSubprocessHost<IDuplexService, IDuplexCallback> _DuplexHost;

        public ServiceForm()
        {
            InitializeComponent();
            leftOperand.Value = Settings.Default.AppSettingItem;
            rightOperand.Value = Settings.Default.UserSettingItem;
        }


        public DuplexSubprocessHost<IDuplexService, IDuplexCallback> DuplexHost
        {
            get
            {
                if (_DuplexHost == null)
                {
                    _DuplexHost = new DuplexSubprocessHost<IDuplexService, IDuplexCallback>(new TestHost());
                }
                return _DuplexHost;
            }
        }

        private void duplexButton_Click(object sender, EventArgs e)
        {
            var c = new SubprocessConfiguration();
            c.ConfigCreated += (_, args) =>
            {
                Console.WriteLine(File.ReadAllText(args.FileName));

                var ach = new AppConfigHelper(args.FileName);
                ach.SetClientSetting(() => Settings.Default.AppSettingItem, leftOperand.Value.ToString());
                ach.SetClientSetting(() => Settings.Default.UserSettingItem, rightOperand.Value.ToString());

                using (var rs = typeof(Program).Assembly.GetManifestResourceStream(typeof(Program), "subprocess.config"))
                {
                    var xd = new XmlDocument();
                    xd.Load(rs);

                    var sde = ach.Document.SelectSingleNode("/configuration/system.diagnostics");
                    if (sde == null)
                    {
                        sde = ach.Document.CreateElement("system.diagnostics");
                        sde.OwnerDocument.DocumentElement.AppendChild(sde);
                    }
                    sde.InnerXml = xd.SelectSingleNode("/configuration/system.diagnostics").InnerXml;
                }

                ach.Save();
            };
            Settings.Default.UserSettingItem = (int)rightOperand.Value;
            Settings.Default.Save();
            DuplexHost.StartNew(DuplexClientForm.EntryPoint, TestEnum.aaa, c);
        }

        private void HostForm_Load(object sender, EventArgs e)
        {

        }
    }
}
