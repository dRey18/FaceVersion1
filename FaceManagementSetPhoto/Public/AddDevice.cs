using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace FaceManagementSetPhoto
{
    public partial class AddDevice : Form
    {
        public static DeviceInfo struDeviceInfo;

        public AddDevice()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (textBoxDeviceAddress.Text.Length <= 0 || textBoxDeviceAddress.Text.Length >128)
            {
                MessageBox.Show(Properties.Resources.deviceAddressTips);
                return;
            }

            int port;
            int.TryParse(textBoxPort.Text, out port);
            if (textBoxPort.Text.Length > 5 || port <= 0)
            {
                MessageBox.Show(Properties.Resources.portTips);
                return;
            }

            if (textBoxUserName.Text.Length > 32 || textBoxPassword.Text.Length > 16)
            {
                MessageBox.Show(Properties.Resources.usernameAndPasswordTips);
                return;
            }

            Login();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void Login()
        {
            struDeviceInfo = new DeviceInfo();
            struDeviceInfo.strUsername = textBoxUserName.Text;
            struDeviceInfo.strPassword = textBoxPassword.Text;
            struDeviceInfo.strDeviceIP = textBoxDeviceAddress.Text;
            struDeviceInfo.strHttpPort = textBoxPort.Text;

            if (Security.Login(struDeviceInfo))
            {
                // user check success
                struDeviceInfo.bIsLogin = true;
                this.Close();
            }
        }

        //private void AddDevice_Load(object sender, EventArgs e)
        //{
        //    MultiLanguage.LoadLanguage(this);
        //}

    }
}
