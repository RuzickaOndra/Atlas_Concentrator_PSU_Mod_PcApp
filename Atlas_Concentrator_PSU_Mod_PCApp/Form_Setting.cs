using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HV_Power_Supply_GUI_ver._1
{
    public partial class Form_Setting : Form
    {
        private Communication serial;
        private uint ip_address;
        private uint net_mask;
        private uint gateway;


        internal Communication xserial { get => serial; set => serial = value; }
        internal uint xip_address { get => ip_address; set => ip_address = value; }
        internal uint xnet_mask { get => net_mask; set => net_mask = value; }
        internal uint xgateway { get => gateway; set => gateway = value; }

        public Form_Setting()
        {
            InitializeComponent();

        }

        private void Form_Setting_Load(object sender, EventArgs e)
        {
            textBox_ip.Text = string_from_ip(ip_address);
            textBox_nm.Text = string_from_ip(net_mask);
            textBox_gw.Text = string_from_ip(gateway);
        }

        public uint ip_from_string(string s) 
        {
            string[] ip_bytes = s.Split('.');

            if(ip_bytes.Length != 4) 
            {
                MessageBox.Show("Chyba zadání", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }

            uint addr = 0;
            uint x;

            for(int i = 0; i < 4; i++) 
            {
                if (!uint.TryParse(ip_bytes[i], out x))
                {
                    MessageBox.Show("Chyba zadání", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }

                addr = addr | ((x & 0xff) << (3-i)*8);
            }

            return addr;

        }

        public string string_from_ip(uint ip) 
        {
            string s = string.Empty;
            byte[] ip_bytes = new byte[4];

            for(int i = 0; i < 4; i++) 
            {
                ip_bytes[i] = (byte)(ip >> (i * 8));
            }

            s = ip_bytes[3].ToString() + "." + ip_bytes[2].ToString() + "." + ip_bytes[1].ToString() + "." + ip_bytes[0].ToString();

            return s;
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            ip_address = ip_from_string(textBox_ip.Text);
            net_mask = ip_from_string(textBox_nm.Text);
            gateway = ip_from_string(textBox_gw.Text);

            serial.SendCommand(Communication.eCommandCode.ip_store_myip, ip_from_string(textBox_ip.Text));
            serial.SendCommand(Communication.eCommandCode.ip_store_mymask, ip_from_string(textBox_nm.Text));
            serial.SendCommand(Communication.eCommandCode.ip_store_mygatew, ip_from_string(textBox_gw.Text));

            DialogResult = DialogResult.OK;
            this.Close();
        }



        private void button_exit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button_save_reset_Click(object sender, EventArgs e)
        {
            ip_address = ip_from_string(textBox_ip.Text);
            net_mask = ip_from_string(textBox_nm.Text);
            gateway = ip_from_string(textBox_gw.Text);

            serial.SendCommand(Communication.eCommandCode.ip_store_myip, ip_from_string(textBox_ip.Text));
            serial.SendCommand(Communication.eCommandCode.ip_store_mymask, ip_from_string(textBox_nm.Text));
            serial.SendCommand(Communication.eCommandCode.ip_store_mygatew, ip_from_string(textBox_gw.Text));
            serial.SendCommand(Communication.eCommandCode.reset,0);

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
