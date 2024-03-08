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



namespace HV_Power_Supply_GUI_ver._1
{
    public partial class Form1 : Form
    {
        Communication communication;

        private uint device_ip_address;
        private uint device_net_mask;
        private uint device_gateway;
        private bool device_connected;

        //---- mod A ----
        bool btn_3V3_I_IN_A_state = false;
        bool btn_3V3_II_IN_A_state = false;
        bool btn_VCB_I_IN_A_state = false;
        bool btn_VCB_II_IN_A_state = false;
        bool btn_3V3_I_OUT_A_state = false;
        bool btn_3V3_II_OUT_A_state = false;
        bool btn_VCB_I_OUT_A_state = false;
        bool btn_VCB_II_OUT_A_state = false;
               
        float voltageRead_3V3_I_A = 100;
        float voltageRead_3V3_II_A = 100;
        float voltageRead_VCB_I_A = 100;
        float voltageRead_VCB_II_A = 100;

        float temperatureRead_3V3_I_A = -300;
        float temperatureRead_3V3_II_A = -300;
        float temperatureRead_VCB_I_A = -300;
        float temperatureRead_VCB_II_A = -300;

        float numericUpDown_3V3_OVP_A_value = 3.4f;
        float numericUpDown_3V3_OTP_A_value = 85;
        float numericUpDown_VCB_OVP_A_value = 4.2f;
        float numericUpDown_VCB_OTP_A_value = 85;

        //---- mod B ----
        bool btn_3V3_I_IN_B_state = false;
        bool btn_3V3_II_IN_B_state = false;
        bool btn_VCB_I_IN_B_state = false;
        bool btn_VCB_II_IN_B_state = false;
        bool btn_3V3_I_OUT_B_state = false;
        bool btn_3V3_II_OUT_B_state = false;
        bool btn_VCB_I_OUT_B_state = false;
        bool btn_VCB_II_OUT_B_state = false;

        float voltageRead_3V3_I_B = 100;
        float voltageRead_3V3_II_B = 100;
        float voltageRead_VCB_I_B = 100;
        float voltageRead_VCB_II_B = 100;

        float temperatureRead_3V3_I_B = -300;
        float temperatureRead_3V3_II_B = -300;
        float temperatureRead_VCB_I_B = -300;
        float temperatureRead_VCB_II_B = -300;

        float numericUpDown_3V3_OVP_B_value = 3.4f;
        float numericUpDown_3V3_OTP_B_value = 85;
        float numericUpDown_VCB_OVP_B_value = 4.2f;
        float numericUpDown_VCB_OTP_B_value = 85;

        double MIN3V3 = 3.2; //proste to mel byt define, ale neumim ho tu.
        double MAX3V3 = 3.4;
        double MINVCB = 3.8;
        double MAXVCB = 4.1;
        double MAXTEMP = 80;

        public enum eAutoPowerupState : byte
        {
            init,
            checkTemperature_Mod1,
            checkTemperature_Mod2,
            checkVoltage_Mod1,
            checkVoltage_Mod2,
            Enable_In_Mod1,
            Enable_In_Mod2,
            Enable_Out_Mod1,
            Enable_Out_Mod2,
            end_OK,
            end_Error
        };

        public Form1()
        {
            InitializeComponent();
            communication = new Communication(XserialPort,5005, ExecuteCommand);
        }


        //scan serial ports
        private void button_PortScan_Click(object sender, EventArgs e)
        {
            comboBoxPorts.Items.Clear();

            foreach (String s in communication.GetPortNames())
            {
                comboBoxPorts.Items.Add(s);
            }


            if (comboBoxPorts.Items.Count > 0)
            {
                comboBoxPorts.SelectedIndex = 0;
            }

            if (communication.IsOpen())
            {
                label_SerialStatus.Text = "Open";
            }
            else
            {
                label_SerialStatus.Text = "Close";
            }

        }

        //open/close serial port
        private void button_OpenClose_Click(object sender, EventArgs e)
        {
            Communication.eCommunicationType comm = communication.GetCommunicationType();

            if(comm == Communication.eCommunicationType.serial) 
            {
                communication.Close_Serial();
                label_SerialStatus.Text = "Close";
                timer_req.Enabled = false;
                //timer_Read.Enabled = false;
                CommunicationControlEnable(true);
                return;
            }

            else if(comm == Communication.eCommunicationType.udp) 
            {
                communication.Close_UDP();
                label_SerialStatus.Text = "Close";
                timer_req.Enabled = false;
                CommunicationControlEnable(true);
                return;
            }
            

            if (radioButton_Serial.Checked == true) 
            {
                if (comboBoxPorts.SelectedIndex < 0)
                {
                    MessageBox.Show("Nevybran port", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                communication.Open_Serial(comboBoxPorts.SelectedItem as String);
            }         
            else if(radioButton_UDP.Checked == true) 
            {
                communication.Open_UDP(textBox_IP.Text);
            }


            comm = communication.GetCommunicationType();

            if (comm == Communication.eCommunicationType.serial)
            {
                label_SerialStatus.Text = "Open Serial";

                communication.SendCommand(Communication.eCommandCode.getsetting, 0);
                communication.SendCommand(Communication.eCommandCode.ip_getsetting, 0);

                timer_req.Interval = 400;
                timer_req.Enabled = true;

                CommunicationControlEnable(false);

            }
            else if (comm == Communication.eCommunicationType.udp)
            {
                label_SerialStatus.Text = "Open UDP";


                communication.SendCommand(Communication.eCommandCode.ip_store_endpoint, 0);
                communication.SendCommand(Communication.eCommandCode.getsetting, 0);           
                communication.SendCommand(Communication.eCommandCode.ip_getsetting, 0);

                timer_req.Interval = 200;
                timer_req.Enabled = true;

                CommunicationControlEnable(false);
            }
            else 
            {
                label_SerialStatus.Text = "Close";
                CommunicationControlEnable(true);
            }
        }

        private void CommunicationControlEnable(bool Enable) 
        {
            radioButton_UDP.Enabled = Enable;
            radioButton_Serial.Enabled = Enable;
            button_PortScan.Enabled = Enable;
            comboBoxPorts.Enabled = Enable;
            textBox_IP.Enabled = Enable;
        }

        private void button_set_Click(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.ip_getsetting, 0);
            Form_Setting form = new Form_Setting();

            form.xip_address = device_ip_address;
            form.xnet_mask = device_net_mask;
            form.xgateway = device_gateway;
            form.xserial = communication;
            
            form.ShowDialog();
            DialogResult fdr= form.DialogResult;

            if (fdr == DialogResult.OK)
            {
                device_ip_address = form.xip_address;
                device_net_mask = form.xnet_mask;
                device_gateway = form.xgateway;

                
                textBox_IP.Text = form.string_from_ip(device_ip_address);
            }
        }


        //receive from serial
        private void timer_Read_Tick(object sender, EventArgs e)
        {
            //communication.SerialReadCommand();
        }

        private void ExecuteCommand() 
        {
            switch (communication.ReadCommand_Code)
            {
                case Communication.eCommandCode.NON:
                    break;
               
                case Communication.eCommandCode.Connected:
                    if (communication.ReadCommand_Data == 1) device_connected = true;
                    break;

                //---- mod A ----
                case Communication.eCommandCode.ch1_A_in_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_3V3_I_IN_A_state = true;
                        btn_3V3_I_IN_A.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_3V3_I_IN_A_state = false;
                        btn_3V3_I_IN_A.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch2_A_in_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_3V3_II_IN_A_state = true;
                        btn_3V3_II_IN_A.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_3V3_II_IN_A_state = false;
                        btn_3V3_II_IN_A.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch3_A_in_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_VCB_I_IN_A_state = true;
                        btn_VCB_I_IN_A.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_VCB_I_IN_A_state = false;
                        btn_VCB_I_IN_A.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch4_A_in_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_VCB_II_IN_A_state = true;
                        btn_VCB_II_IN_A.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_VCB_II_IN_A_state = false;
                        btn_VCB_II_IN_A.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch1_A_out_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_3V3_I_OUT_A_state = true;
                        btn_3V3_I_OUT_A.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_3V3_I_OUT_A_state = false;
                        btn_3V3_I_OUT_A.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch2_A_out_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_3V3_II_OUT_A_state = true;
                        btn_3V3_II_OUT_A.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_3V3_II_OUT_A_state = false;
                        btn_3V3_II_OUT_A.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch3_A_out_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_VCB_I_OUT_A_state = true;
                        btn_VCB_I_OUT_A.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_VCB_I_OUT_A_state = false;
                        btn_VCB_I_OUT_A.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch4_A_out_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_VCB_II_OUT_A_state = true;
                        btn_VCB_II_OUT_A.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_VCB_II_OUT_A_state = false;
                        btn_VCB_II_OUT_A.BackColor = Color.Red;
                    }
                    break;

                //---- mod B ----
                case Communication.eCommandCode.ch1_B_in_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_3V3_I_IN_B_state = true;
                        btn_3V3_I_IN_B.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_3V3_I_IN_B_state = false;
                        btn_3V3_I_IN_B.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch2_B_in_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_3V3_II_IN_B_state = true;
                        btn_3V3_II_IN_B.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_3V3_II_IN_B_state = false;
                        btn_3V3_II_IN_B.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch3_B_in_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_VCB_I_IN_B_state = true;
                        btn_VCB_I_IN_B.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_VCB_I_IN_B_state = false;
                        btn_VCB_I_IN_B.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch4_B_in_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_VCB_II_IN_B_state = true;
                        btn_VCB_II_IN_B.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_VCB_II_IN_B_state = false;
                        btn_VCB_II_IN_B.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch1_B_out_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_3V3_I_OUT_B_state = true;
                        btn_3V3_I_OUT_B.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_3V3_I_OUT_B_state = false;
                        btn_3V3_I_OUT_B.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch2_B_out_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_3V3_II_OUT_B_state = true;
                        btn_3V3_II_OUT_B.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_3V3_II_OUT_B_state = false;
                        btn_3V3_II_OUT_B.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch3_B_out_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_VCB_I_OUT_B_state = true;
                        btn_VCB_I_OUT_B.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_VCB_I_OUT_B_state = false;
                        btn_VCB_I_OUT_B.BackColor = Color.Red;
                    }
                    break;

                case Communication.eCommandCode.ch4_B_out_enable:
                    if (communication.ReadCommand_Data > 0)
                    {
                        btn_VCB_II_OUT_B_state = true;
                        btn_VCB_II_OUT_B.BackColor = Color.Green;
                    }
                    else
                    {
                        btn_VCB_II_OUT_B_state = false;
                        btn_VCB_II_OUT_B.BackColor = Color.Red;
                    }
                    break;

                //---- mod A ----
                case Communication.eCommandCode.enable_OVP_A_3V3:
                    if (communication.ReadCommand_Data > 0) checkBox_3V3_OVP_A.Checked = true;
                    else checkBox_3V3_OVP_A.Checked = false;
                    break;

                case Communication.eCommandCode.enable_OTP_A_3V3:
                    if (communication.ReadCommand_Data > 0) checkBox_3V3_OTP_A.Checked = true;
                    else checkBox_3V3_OTP_A.Checked = false;
                    break;

                case Communication.eCommandCode.enable_OVP_A_VCB:
                    if (communication.ReadCommand_Data > 0) checkBox_VCB_OVP_A.Checked = true;
                    else checkBox_VCB_OVP_A.Checked = false;
                    break;

                case Communication.eCommandCode.enable_OTP_A_VCB:
                    if (communication.ReadCommand_Data > 0) checkBox_VCB_OTP_A.Checked = true;
                    else checkBox_VCB_OTP_A.Checked = false;
                    break;

                //---- mod B ----
                case Communication.eCommandCode.enable_OVP_B_3V3:
                    if (communication.ReadCommand_Data > 0) checkBox_3V3_OVP_B.Checked = true;
                    else checkBox_3V3_OVP_B.Checked = false;
                    break;

                case Communication.eCommandCode.enable_OTP_B_3V3:
                    if (communication.ReadCommand_Data > 0) checkBox_3V3_OTP_B.Checked = true;
                    else checkBox_3V3_OTP_B.Checked = false;
                    break;

                case Communication.eCommandCode.enable_OVP_B_VCB:
                    if (communication.ReadCommand_Data > 0) checkBox_VCB_OVP_B.Checked = true;
                    else checkBox_VCB_OVP_B.Checked = false;
                    break;

                case Communication.eCommandCode.enable_OTP_B_VCB:
                    if (communication.ReadCommand_Data > 0) checkBox_VCB_OTP_B.Checked = true;
                    else checkBox_VCB_OTP_B.Checked = false;
                    break;

                //---- mod A ----
                case Communication.eCommandCode.ch1_A_getvoltage:
                    voltageRead_3V3_I_A = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch2_A_getvoltage:
                    voltageRead_3V3_II_A = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch3_A_getvoltage:
                    voltageRead_VCB_I_A = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch4_A_getvoltage:
                    voltageRead_VCB_II_A = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch1_A_gettemperature:
                    temperatureRead_3V3_I_A = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch2_A_gettemperature:
                    temperatureRead_3V3_II_A = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch3_A_gettemperature:
                    temperatureRead_VCB_I_A = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch4_A_gettemperature:
                    temperatureRead_VCB_II_A = communication.ReadCommand_Data_float;
                    break;

                //---- mod B ----
                case Communication.eCommandCode.ch1_B_getvoltage:
                    voltageRead_3V3_I_B = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch2_B_getvoltage:
                    voltageRead_3V3_II_B = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch3_B_getvoltage:
                    voltageRead_VCB_I_B = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch4_B_getvoltage:
                    voltageRead_VCB_II_B = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch1_B_gettemperature:
                    temperatureRead_3V3_I_B = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch2_B_gettemperature:
                    temperatureRead_3V3_II_B = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch3_B_gettemperature:
                    temperatureRead_VCB_I_B = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.ch4_B_gettemperature:
                    temperatureRead_VCB_II_B = communication.ReadCommand_Data_float;
                    break;

                //---- mod A ----
                case Communication.eCommandCode.get_OVP_A_3V3:
                    numericUpDown_3V3_OVP_A_value = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.get_OTP_A_3V3:
                    numericUpDown_3V3_OTP_A_value = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.get_OVP_A_VCB:
                    numericUpDown_VCB_OVP_A_value = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.get_OTP_A_VCB:
                    numericUpDown_VCB_OTP_A_value = communication.ReadCommand_Data_float;
                    break;

                //---- mod B ----
                case Communication.eCommandCode.get_OVP_B_3V3:
                    numericUpDown_3V3_OVP_B_value = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.get_OTP_B_3V3:
                    numericUpDown_3V3_OTP_B_value = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.get_OVP_B_VCB:
                    numericUpDown_VCB_OVP_B_value = communication.ReadCommand_Data_float;
                    break;

                case Communication.eCommandCode.get_OTP_B_VCB:
                    numericUpDown_VCB_OTP_B_value = communication.ReadCommand_Data_float;
                    break;


                case Communication.eCommandCode.error_signals:
                    error_label(communication.ReadCommand_Data);
                    break;

                case Communication.eCommandCode.ip_get_myip:
                    device_ip_address = communication.ReadCommand_Data;
                    break;

                case Communication.eCommandCode.ip_get_mymask:
                   device_net_mask = communication.ReadCommand_Data;
                    break;

                case Communication.eCommandCode.ip_get_mygatew:
                    device_gateway = communication.ReadCommand_Data;
                    break;

                default:
                    break;
            }
        }


        int timer_ui = 0;
        private void timer_UpdateUI_Tick(object sender, EventArgs e)
        {
            //---- mod A ----
            if (voltageRead_3V3_I_A<100)
                label_3V3_I_volt_A.Text = voltageRead_3V3_I_A.ToString("0.00") + " V";
            else
                label_3V3_I_volt_A.Text = "-.--" + " V";

            if (btn_3V3_I_IN_A_state)
            {
                if (voltageRead_3V3_I_A > MIN3V3 && voltageRead_3V3_I_A < MAX3V3)
                    Ind_3V3_I_volt_A.BackColor = Color.Green;
                else
                    Ind_3V3_I_volt_A.BackColor = Color.Red;
            }
            else
                Ind_3V3_I_volt_A.BackColor = System.Drawing.SystemColors.Control;

            if (voltageRead_3V3_II_A < 100)
                label_3V3_II_volt_A.Text = voltageRead_3V3_II_A.ToString("0.00") + " V";
            else
                label_3V3_II_volt_A.Text = "-.--" + " V";

            if (btn_3V3_II_IN_A_state)
            {
                if (voltageRead_3V3_II_A > MIN3V3 && voltageRead_3V3_II_A < MAX3V3)
                    Ind_3V3_II_volt_A.BackColor = Color.Green;
                else
                    Ind_3V3_II_volt_A.BackColor = Color.Red;
            }
            else
                Ind_3V3_II_volt_A.BackColor = System.Drawing.SystemColors.Control;

            if (voltageRead_VCB_I_A < 100)
                label_VCB_I_volt_A.Text = voltageRead_VCB_I_A.ToString("0.00") + " V";
            else
                label_VCB_I_volt_A.Text = "-.--" + " V";

            if (btn_VCB_I_IN_A_state)
            {
                if (voltageRead_VCB_I_A > MINVCB && voltageRead_VCB_I_A < MAXVCB)
                    Ind_VCB_I_volt_A.BackColor = Color.Green;
                else
                    Ind_VCB_I_volt_A.BackColor = Color.Red;
            }
            else
                Ind_VCB_I_volt_A.BackColor = System.Drawing.SystemColors.Control;

            if (voltageRead_VCB_II_A < 100)
                label_VCB_II_volt_A.Text = voltageRead_VCB_II_A.ToString("0.00") + " V";
            else
                label_VCB_II_volt_A.Text = "-.--" + " V";

            if (btn_VCB_II_IN_A_state)
            {
                if (voltageRead_VCB_II_A > MINVCB && voltageRead_VCB_II_A < MAXVCB)
                    Ind_VCB_II_volt_A.BackColor = Color.Green;
                else
                    Ind_VCB_II_volt_A.BackColor = Color.Red;
            }
            else
                Ind_VCB_II_volt_A.BackColor = System.Drawing.SystemColors.Control;

            //---- mod B ----
            if (voltageRead_3V3_I_B < 100)
                label_3V3_I_volt_B.Text = voltageRead_3V3_I_B.ToString("0.00") + " V";
            else
                label_3V3_I_volt_B.Text = "-.--" + " V";

            if (btn_3V3_I_IN_B_state)
            {
                if (voltageRead_3V3_I_B > MIN3V3 && voltageRead_3V3_I_B < MAX3V3)
                    Ind_3V3_I_volt_B.BackColor = Color.Green;
                else
                    Ind_3V3_I_volt_B.BackColor = Color.Red;
            }
            else
                Ind_3V3_I_volt_B.BackColor = System.Drawing.SystemColors.Control;

            if (voltageRead_3V3_II_B < 100)
                label_3V3_II_volt_B.Text = voltageRead_3V3_II_B.ToString("0.00") + " V";
            else
                label_3V3_II_volt_B.Text = "-.--" + " V";

            if (btn_3V3_II_IN_B_state)
            {
                if (voltageRead_3V3_II_B > MIN3V3 && voltageRead_3V3_II_B < MAX3V3)
                    Ind_3V3_II_volt_B.BackColor = Color.Green;
                else
                    Ind_3V3_II_volt_B.BackColor = Color.Red;
            }
            else
                Ind_3V3_II_volt_B.BackColor = System.Drawing.SystemColors.Control;

            if (voltageRead_VCB_I_B < 100)
                label_VCB_I_volt_B.Text = voltageRead_VCB_I_B.ToString("0.00") + " V";
            else
                label_VCB_I_volt_B.Text = "-.--" + " V";

            if (btn_VCB_I_IN_B_state)
            {
                if (voltageRead_VCB_I_B > MINVCB && voltageRead_VCB_I_B < MAXVCB)
                    Ind_VCB_I_volt_B.BackColor = Color.Green;
                else
                    Ind_VCB_I_volt_B.BackColor = Color.Red;
            }
            else
                Ind_VCB_I_volt_B.BackColor = System.Drawing.SystemColors.Control;

            if (voltageRead_VCB_II_B < 100)
                label_VCB_II_volt_B.Text = voltageRead_VCB_II_B.ToString("0.00") + " V";
            else
                label_VCB_II_volt_B.Text = "-.--" + " V";

            if (btn_VCB_II_IN_B_state)
            {
                if (voltageRead_VCB_II_B > MINVCB && voltageRead_VCB_II_B < MAXVCB)
                    Ind_VCB_II_volt_B.BackColor = Color.Green;
                else
                    Ind_VCB_II_volt_B.BackColor = Color.Red;
            }
            else
                Ind_VCB_II_volt_B.BackColor = System.Drawing.SystemColors.Control;

            //---- mod A ----
            if (temperatureRead_3V3_I_A > -200)
            {
                label_3V3_I_temp_A.Text = "Temp: " + temperatureRead_3V3_I_A.ToString(".0") + " °C";

                if (temperatureRead_3V3_I_A < MAXTEMP)
                    Ind_3V3_I_temp_A.BackColor = Color.Green;
                else
                    Ind_3V3_I_temp_A.BackColor = Color.Red;
            }
            else
            {
                label_3V3_I_temp_A.Text = "Temp: " + "-.--" + " °C";
                Ind_3V3_I_temp_A.BackColor = System.Drawing.SystemColors.Control;
            }

            if (temperatureRead_3V3_II_A > -200)
            {
                label_3V3_II_temp_A.Text = "Temp: " + temperatureRead_3V3_II_A.ToString(".0") + " °C";

                if (temperatureRead_3V3_II_A < MAXTEMP)
                    Ind_3V3_II_temp_A.BackColor = Color.Green;
                else
                    Ind_3V3_II_temp_A.BackColor = Color.Red;
            }
            else
            {
                label_3V3_II_temp_A.Text = "Temp: " + "-.--" + " °C";
                Ind_3V3_II_temp_A.BackColor = System.Drawing.SystemColors.Control;
            }

            if (temperatureRead_VCB_I_A > -200)
            {
                label_VCB_I_temp_A.Text = "Temp: " + temperatureRead_VCB_I_A.ToString(".0") + " °C";

                if (temperatureRead_VCB_I_A < MAXTEMP)
                    Ind_VCB_I_temp_A.BackColor = Color.Green;
                else
                    Ind_VCB_I_temp_A.BackColor = Color.Red;
            }
            else
            {
                label_VCB_I_temp_A.Text = "Temp: " + "-.--" + " °C";
                Ind_VCB_I_temp_A.BackColor = System.Drawing.SystemColors.Control;
            }

            if (temperatureRead_VCB_II_A > -200)
            {
                label_VCB_II_temp_A.Text = "Temp: " + temperatureRead_VCB_II_A.ToString(".0") + " °C";

                if (temperatureRead_VCB_II_A < MAXTEMP)
                    Ind_VCB_II_temp_A.BackColor = Color.Green;
                else
                    Ind_VCB_II_temp_A.BackColor = Color.Red;
            }
            else
            {
                label_VCB_II_temp_A.Text = "Temp: " + "-.--" + " °C";
                Ind_VCB_II_temp_A.BackColor = System.Drawing.SystemColors.Control;
            }

            //---- mod B ----
            if (temperatureRead_3V3_I_B > -200)
            {
                label_3V3_I_temp_B.Text = "Temp: " + temperatureRead_3V3_I_B.ToString(".0") + " °C";

                if (temperatureRead_3V3_I_B < MAXTEMP)
                    Ind_3V3_I_temp_B.BackColor = Color.Green;
                else
                    Ind_3V3_I_temp_B.BackColor = Color.Red;
            }
            else
            {
                label_3V3_I_temp_B.Text = "Temp: " + "-.--" + " °C";
                Ind_3V3_I_temp_B.BackColor = System.Drawing.SystemColors.Control;
            }

            if (temperatureRead_3V3_II_B > -200)
            {
                label_3V3_II_temp_B.Text = "Temp: " + temperatureRead_3V3_II_B.ToString(".0") + " °C";

                if (temperatureRead_3V3_II_B < MAXTEMP)
                    Ind_3V3_II_temp_B.BackColor = Color.Green;
                else
                    Ind_3V3_II_temp_B.BackColor = Color.Red;
            }
            else
            {
                label_3V3_II_temp_B.Text = "Temp: " + "-.--" + " °C";
                Ind_3V3_II_temp_B.BackColor = System.Drawing.SystemColors.Control;
            }

            if (temperatureRead_VCB_I_B > -200)
            {
                label_VCB_I_temp_B.Text = "Temp: " + temperatureRead_VCB_I_B.ToString(".0") + " °C";

                if (temperatureRead_VCB_I_B < MAXTEMP)
                    Ind_VCB_I_temp_B.BackColor = Color.Green;
                else
                    Ind_VCB_I_temp_B.BackColor = Color.Red;
            }
            else
            {
                label_VCB_I_temp_B.Text = "Temp: " + "-.--" + " °C";
                Ind_VCB_I_temp_B.BackColor = System.Drawing.SystemColors.Control;
            }

            if (temperatureRead_VCB_II_B > -200)
            {
                label_VCB_II_temp_B.Text = "Temp: " + temperatureRead_VCB_II_B.ToString(".0") + " °C";

                if (temperatureRead_VCB_II_B < MAXTEMP)
                    Ind_VCB_II_temp_B.BackColor = Color.Green;
                else
                    Ind_VCB_II_temp_B.BackColor = Color.Red;
            }
            else
            {
                label_VCB_II_temp_B.Text = "Temp: " + "-.--" + " °C";
                Ind_VCB_II_temp_B.BackColor = System.Drawing.SystemColors.Control;
            }

            if (timer_ui > 20)
            {
                //---- mod A ----
                numericUpDown_3V3_OVP_A.Value = (decimal)numericUpDown_3V3_OVP_A_value;
                numericUpDown_3V3_OTP_A.Value = (decimal)numericUpDown_3V3_OTP_A_value;
                numericUpDown_VCB_OVP_A.Value = (decimal)numericUpDown_VCB_OVP_A_value;
                numericUpDown_VCB_OTP_A.Value = (decimal)numericUpDown_VCB_OTP_A_value;

                //---- mod B ----
                numericUpDown_3V3_OVP_B.Value = (decimal)numericUpDown_3V3_OVP_B_value;
                numericUpDown_3V3_OTP_B.Value = (decimal)numericUpDown_3V3_OTP_B_value;
                numericUpDown_VCB_OVP_B.Value = (decimal)numericUpDown_VCB_OVP_B_value;
                numericUpDown_VCB_OTP_B.Value = (decimal)numericUpDown_VCB_OTP_B_value;

                timer_ui = 0;
            }
            timer_ui++;
        }

        private void error_label(uint value) 
        {
            //---- mod A ----
            if ((value & (1 << 0)) > 0) Ind_3V3_I_OVP_A.BackColor = Color.Red;
            else Ind_3V3_I_OVP_A.BackColor = System.Drawing.SystemColors.Control;
           
            if ((value & (1 << 1)) > 0) Ind_3V3_II_OVP_A.BackColor = Color.Red;
            else Ind_3V3_II_OVP_A.BackColor = System.Drawing.SystemColors.Control;

            if ((value & (1 << 2)) > 0) Ind_VCB_I_OVP_A.BackColor = Color.Red;
            else Ind_VCB_I_OVP_A.BackColor = System.Drawing.SystemColors.Control;

            if ((value & (1 << 3)) > 0) Ind_VCB_II_OVP_A.BackColor = Color.Red;
            else Ind_VCB_II_OVP_A.BackColor = System.Drawing.SystemColors.Control;

            //---- mod B ----
            if ((value & (1 << 4)) > 0) Ind_3V3_I_OVP_B.BackColor = Color.Red;
            else Ind_3V3_I_OVP_B.BackColor = System.Drawing.SystemColors.Control;

            if ((value & (1 << 5)) > 0) Ind_3V3_II_OVP_B.BackColor = Color.Red;
            else Ind_3V3_II_OVP_B.BackColor = System.Drawing.SystemColors.Control;

            if ((value & (1 << 6)) > 0) Ind_VCB_I_OVP_B.BackColor = Color.Red;
            else Ind_VCB_I_OVP_B.BackColor = System.Drawing.SystemColors.Control;

            if ((value & (1 << 7)) > 0) Ind_VCB_II_OVP_B.BackColor = Color.Red;
            else Ind_VCB_II_OVP_B.BackColor = System.Drawing.SystemColors.Control;

            //---- mod A ----
            if ((value & (1 << 8)) > 0) Ind_3V3_I_OTP_A.BackColor = Color.Red;
            else Ind_3V3_I_OTP_A.BackColor = System.Drawing.SystemColors.Control;
          
            if ((value & (1 << 9)) > 0) Ind_3V3_II_OTP_A.BackColor = Color.Red;
            else Ind_3V3_II_OTP_A.BackColor = System.Drawing.SystemColors.Control;
           
            if ((value & (1 << 10)) > 0) Ind_VCB_I_OTP_A.BackColor = Color.Red;
            else Ind_VCB_I_OTP_A.BackColor = System.Drawing.SystemColors.Control;
           
            if ((value & (1 << 11)) > 0) Ind_VCB_II_OTP_A.BackColor = Color.Red;
            else Ind_VCB_II_OTP_A.BackColor = System.Drawing.SystemColors.Control;

            //---- mod B ----
            if ((value & (1 << 12)) > 0) Ind_3V3_I_OTP_B.BackColor = Color.Red;
            else Ind_3V3_I_OTP_B.BackColor = System.Drawing.SystemColors.Control;

            if ((value & (1 << 13)) > 0) Ind_3V3_II_OTP_B.BackColor = Color.Red;
            else Ind_3V3_II_OTP_B.BackColor = System.Drawing.SystemColors.Control;

            if ((value & (1 << 14)) > 0) Ind_VCB_I_OTP_B.BackColor = Color.Red;
            else Ind_VCB_I_OTP_B.BackColor = System.Drawing.SystemColors.Control;

            if ((value & (1 << 15)) > 0) Ind_VCB_II_OTP_B.BackColor = Color.Red;
            else Ind_VCB_II_OTP_B.BackColor = System.Drawing.SystemColors.Control;

        }

        int timer_setting = 0;
        private void timer_req_Tick(object sender, EventArgs e)
        {
            if (device_connected) 
            {
                label_SerialStatus.BackColor = Color.Green;
            }
            else 
            {
                label_SerialStatus.BackColor = Color.Red;

                //---- mod A ----
                label_3V3_I_volt_A.Text = "-.--" + " V";
                label_3V3_II_volt_A.Text = "-.--" + " V";
                label_VCB_I_volt_A.Text = "-.--" + " V";
                label_VCB_II_volt_A.Text = "-.--" + " V";

                label_3V3_I_temp_A.Text = "Temp: " + "-.--" + " °C";
                label_3V3_II_temp_A.Text = "Temp: " + "-.--" + " °C";
                label_VCB_I_temp_A.Text = "Temp: " + "-.--" + " °C";
                label_VCB_II_temp_A.Text = "Temp: " + "-.--" + " °C";

                //---- mod B ----
                label_3V3_I_volt_B.Text = "-.--" + " V";
                label_3V3_II_volt_B.Text = "-.--" + " V";
                label_VCB_I_volt_B.Text = "-.--" + " V";
                label_VCB_II_volt_B.Text = "-.--" + " V";

                label_3V3_I_temp_B.Text = "Temp: " + "-.--" + " °C";
                label_3V3_II_temp_B.Text = "Temp: " + "-.--" + " °C";
                label_VCB_I_temp_B.Text = "Temp: " + "-.--" + " °C";
                label_VCB_II_temp_B.Text = "Temp: " + "-.--" + " °C";
            }

            communication.SendCommand(Communication.eCommandCode.Connected, 1);
            device_connected = false;

            communication.SendCommand(Communication.eCommandCode.getallvalues, 0);
            
            if(timer_setting >= 20) 
            {
                communication.SendCommand(Communication.eCommandCode.getsetting, 0);
                communication.SendCommand(Communication.eCommandCode.get_OVP_A_3V3, 0);
                communication.SendCommand(Communication.eCommandCode.get_OTP_A_3V3, 0);
                communication.SendCommand(Communication.eCommandCode.get_OVP_A_VCB, 0);
                communication.SendCommand(Communication.eCommandCode.get_OTP_A_VCB, 0);
                communication.SendCommand(Communication.eCommandCode.get_OVP_B_3V3, 0);
                communication.SendCommand(Communication.eCommandCode.get_OTP_B_3V3, 0);
                communication.SendCommand(Communication.eCommandCode.get_OVP_B_VCB, 0);
                communication.SendCommand(Communication.eCommandCode.get_OTP_B_VCB, 0);
                timer_setting = 0;
            }
            timer_setting++;
        
         }

        //channels input enable buttons-------------------------------------------------------------------------------------

        //---- mod A ----
        private void btn_3V3_I_IN_A_Click(object sender, EventArgs e)
        {
            if (!btn_3V3_I_IN_A_state)
                communication.SendCommand(Communication.eCommandCode.ch1_A_in_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch1_A_in_enable, 0);

            if (btn_3V3_I_IN_A_state && btn_3V3_I_OUT_A_state) //turn off output also
                communication.SendCommand(Communication.eCommandCode.ch1_A_out_enable, 0);
           
            communication.SendCommand(Communication.eCommandCode.getsetting, 0);

        }

        private void btn_3V3_II_IN_A_Click(object sender, EventArgs e)
        {
            if (!btn_3V3_II_IN_A_state)
                communication.SendCommand(Communication.eCommandCode.ch2_A_in_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch2_A_in_enable, 0);

            if (btn_3V3_II_IN_A_state && btn_3V3_II_OUT_A_state) //turn off output also
                communication.SendCommand(Communication.eCommandCode.ch2_A_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void btn_VCB_I_IN_A_Click(object sender, EventArgs e)
        {
            if (!btn_VCB_I_IN_A_state)
                communication.SendCommand(Communication.eCommandCode.ch3_A_in_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch3_A_in_enable, 0);


            if (btn_VCB_I_IN_A_state && btn_VCB_I_OUT_A_state) //turn off output also
                communication.SendCommand(Communication.eCommandCode.ch3_A_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void btn_VCB_II_IN_A_Click(object sender, EventArgs e)
        {
            if (!btn_VCB_II_IN_A_state)
                communication.SendCommand(Communication.eCommandCode.ch4_A_in_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch4_A_in_enable, 0);

            if (btn_VCB_II_IN_A_state && btn_VCB_II_OUT_A_state) //turn off output also
                communication.SendCommand(Communication.eCommandCode.ch4_A_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        //---- mod B ----
        private void btn_3V3_I_IN_B_Click(object sender, EventArgs e)
        {
            if (!btn_3V3_I_IN_B_state)
                communication.SendCommand(Communication.eCommandCode.ch1_B_in_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch1_B_in_enable, 0);

            if (btn_3V3_I_IN_B_state && btn_3V3_I_OUT_B_state) //turn off output also
                communication.SendCommand(Communication.eCommandCode.ch1_B_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);

        }

        private void btn_3V3_II_IN_B_Click(object sender, EventArgs e)
        {
            if (!btn_3V3_II_IN_B_state)
                communication.SendCommand(Communication.eCommandCode.ch2_B_in_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch2_B_in_enable, 0);

            if (btn_3V3_II_IN_B_state && btn_3V3_II_OUT_B_state) //turn off output also
                communication.SendCommand(Communication.eCommandCode.ch2_B_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void btn_VCB_I_IN_B_Click(object sender, EventArgs e)
        {
            if (!btn_VCB_I_IN_B_state)
                communication.SendCommand(Communication.eCommandCode.ch3_B_in_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch3_B_in_enable, 0);


            if (btn_VCB_I_IN_B_state && btn_VCB_I_OUT_B_state) //turn off output also
                communication.SendCommand(Communication.eCommandCode.ch3_B_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void btn_VCB_II_IN_B_Click(object sender, EventArgs e)
        {
            if (!btn_VCB_II_IN_B_state)
                communication.SendCommand(Communication.eCommandCode.ch4_B_in_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch4_B_in_enable, 0);

            if (btn_VCB_II_IN_B_state && btn_VCB_II_OUT_B_state) //turn off output also
                communication.SendCommand(Communication.eCommandCode.ch4_B_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        //channels output enable buttons-------------------------------------------------------------------------------------
        //---- mod A ----
        private void btn_3V3_I_OUT_A_Click(object sender, EventArgs e)
        {
            if(!btn_3V3_I_OUT_A_state && !btn_3V3_I_IN_A_state)//do not turn out output if input is inactive
                return;

            if(!btn_3V3_I_OUT_A_state && btn_3V3_II_OUT_A_state && checkbox_OCP_A.Checked) // do not turn on multiple outputs together
                return;

            if (!btn_3V3_I_OUT_A_state)
                communication.SendCommand(Communication.eCommandCode.ch1_A_out_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch1_A_out_enable, 0);
            
            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void btn_3V3_II_OUT_A_Click(object sender, EventArgs e)
        {
            if(!btn_3V3_II_OUT_A_state && !btn_3V3_II_IN_A_state)//do not turn out output if input is inactive
                return;

            if(!btn_3V3_II_OUT_A_state && btn_3V3_I_OUT_A_state && checkbox_OCP_A.Checked) // do not turn on multiple outputs together
                return;

            if (!btn_3V3_II_OUT_A_state)
                communication.SendCommand(Communication.eCommandCode.ch2_A_out_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch2_A_out_enable, 0);
        
            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void btn_VCB_I_OUT_A_Click(object sender, EventArgs e)
        {
            if(!btn_VCB_I_OUT_A_state && !btn_VCB_I_IN_A_state)//do not turn out output if input is inactive
                return;

            if(!btn_VCB_I_OUT_A_state && btn_VCB_II_OUT_A_state && checkbox_OCP_A.Checked) // do not turn on multiple outputs together
                return;

            if (!btn_VCB_I_OUT_A_state)
                communication.SendCommand(Communication.eCommandCode.ch3_A_out_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch3_A_out_enable, 0);
            
            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void btn_VCB_II_OUT_A_Click(object sender, EventArgs e)
        {
            if (!btn_VCB_II_OUT_A_state && !btn_VCB_II_IN_A_state)//do not turn out output if input is inactive
                return;

            if (!btn_VCB_II_OUT_A_state && btn_VCB_I_OUT_A_state && checkbox_OCP_A.Checked) // do not turn on multiple outputs together
                return;

            if (!btn_VCB_II_OUT_A_state)
                communication.SendCommand(Communication.eCommandCode.ch4_A_out_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch4_A_out_enable, 0);
    
            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        //---- mod B ----
        private void btn_3V3_I_OUT_B_Click(object sender, EventArgs e)
        {
            if (!btn_3V3_I_OUT_B_state && !btn_3V3_I_IN_B_state)//do not turn out output if input is inactive
                return;

            if (!btn_3V3_I_OUT_B_state && btn_3V3_II_OUT_B_state && checkbox_OCP_B.Checked) // do not turn on multiple outputs together
                return;

            if (!btn_3V3_I_OUT_B_state)
                communication.SendCommand(Communication.eCommandCode.ch1_B_out_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch1_B_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void btn_3V3_II_OUT_B_Click(object sender, EventArgs e)
        {
            if (!btn_3V3_II_OUT_B_state && !btn_3V3_II_IN_B_state)//do not turn out output if input is inactive
                return;

            if (!btn_3V3_II_OUT_B_state && btn_3V3_I_OUT_B_state && checkbox_OCP_B.Checked) // do not turn on multiple outputs together
                return;

            if (!btn_3V3_II_OUT_B_state)
                communication.SendCommand(Communication.eCommandCode.ch2_B_out_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch2_B_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void btn_VCB_I_OUT_B_Click(object sender, EventArgs e)
        {
            if (!btn_VCB_I_OUT_B_state && !btn_VCB_I_IN_B_state)//do not turn out output if input is inactive
                return;

            if (!btn_VCB_I_OUT_B_state && btn_VCB_II_OUT_B_state && checkbox_OCP_B.Checked) // do not turn on multiple outputs together
                return;

            if (!btn_VCB_I_OUT_B_state)
                communication.SendCommand(Communication.eCommandCode.ch3_B_out_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch3_B_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void btn_VCB_II_OUT_B_Click(object sender, EventArgs e)
        {
            if (!btn_VCB_II_OUT_B_state && !btn_VCB_II_IN_B_state)//do not turn out output if input is inactive
                return;

            if (!btn_VCB_II_OUT_B_state && btn_VCB_I_OUT_B_state && checkbox_OCP_B.Checked) // do not turn on multiple outputs together
                return;

            if (!btn_VCB_II_OUT_B_state)
                communication.SendCommand(Communication.eCommandCode.ch4_B_out_enable, 1);
            else
                communication.SendCommand(Communication.eCommandCode.ch4_B_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        //output collision protection
        //---- mod A ----
        private void checkbox_OCP_A_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkbox_OCP_A.Checked)
                return;

            if(btn_3V3_I_OUT_A_state && btn_3V3_II_OUT_A_state) //two 3V3 outputs active
                communication.SendCommand(Communication.eCommandCode.ch2_A_out_enable, 0);

            if (btn_VCB_I_OUT_A_state && btn_VCB_II_OUT_A_state) //two VCB outputs active
                communication.SendCommand(Communication.eCommandCode.ch4_A_out_enable, 0);
        }

        //---- mod B ----
        private void checkbox_OCP_B_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkbox_OCP_B.Checked)
                return;

            if (btn_3V3_I_OUT_B_state && btn_3V3_II_OUT_B_state) //two 3V3 outputs active
                communication.SendCommand(Communication.eCommandCode.ch2_A_out_enable, 0);

            if (btn_VCB_I_OUT_B_state && btn_VCB_II_OUT_B_state) //two VCB outputs active
                communication.SendCommand(Communication.eCommandCode.ch4_A_out_enable, 0);
        }

        //turn off all inputs
        //---- mod A ----
        private void btn_IN_OFF_A_Click(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.ch1_A_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch2_A_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch3_A_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch4_A_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.ch1_A_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch2_A_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch3_A_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch4_A_in_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);

            labelErr_A.BackColor = System.Drawing.SystemColors.Control;
            labelOK_A.BackColor = System.Drawing.SystemColors.Control;
        }

        //---- mod B ----
        private void btn_IN_OFF_B_Click(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.ch1_B_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch2_B_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch3_B_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch4_B_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.ch1_B_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch2_B_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch3_B_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch4_B_in_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);

            labelErr_B.BackColor = System.Drawing.SystemColors.Control;
            labelOK_B.BackColor = System.Drawing.SystemColors.Control;
        }

        //turn off all outputs
        //---- mod A ----
        private void btn_OUT_OFF_A_Click(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.all_channels_off_A, 1);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);

            labelErr_A.BackColor = System.Drawing.SystemColors.Control;
            labelOK_A.BackColor = System.Drawing.SystemColors.Control;
        }

        //---- mod B ----
        private void btn_OUT_OFF_B_Click(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.all_channels_off_B, 1);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);

            labelErr_B.BackColor = System.Drawing.SystemColors.Control;
            labelOK_B.BackColor = System.Drawing.SystemColors.Control;
        }

        //---- mod A ----
        bool autoPowerUp_Flag_A;
        int autoPowerUp_OK_Flag_A;
        bool autoPowerUp_Error_Flag_A;
        bool end3V3_Flag_A;
        bool endVCB_Flag_A;

        private eAutoPowerupState autoPowerUp_State_3V3_A = eAutoPowerupState.init;
        private eAutoPowerupState autoPowerUp_State_VCB_A = eAutoPowerupState.init;

        //---- mod B ----
        bool autoPowerUp_Flag_B;
        int autoPowerUp_OK_Flag_B;
        bool autoPowerUp_Error_Flag_B;
        bool end3V3_Flag_B;
        bool endVCB_Flag_B;

        private eAutoPowerupState autoPowerUp_State_3V3_B = eAutoPowerupState.init;
        private eAutoPowerupState autoPowerUp_State_VCB_B = eAutoPowerupState.init;

        //automatic powerup trigger button
        //---- mod A ----
        private void btn_Automatic_Powerup_A_Click(object sender, EventArgs e)
        {
            autoPowerUp_Flag_A = true;
            autoPowerUp_State_3V3_A = eAutoPowerupState.init;
            autoPowerUp_State_VCB_A = eAutoPowerupState.init;

            autoPowerUp_OK_Flag_A = 0;
            autoPowerUp_Error_Flag_A = false;

            end3V3_Flag_A = false;
            endVCB_Flag_A = false;

            labelErr_A.BackColor = System.Drawing.SystemColors.Control;
            labelOK_A.BackColor = System.Drawing.SystemColors.Control;
        }

        //---- mod B ----
        private void btn_Automatic_Powerup_B_Click(object sender, EventArgs e)
        {
            autoPowerUp_Flag_B = true;
            autoPowerUp_State_3V3_B = eAutoPowerupState.init;
            autoPowerUp_State_VCB_B = eAutoPowerupState.init;

            autoPowerUp_OK_Flag_B = 0;
            autoPowerUp_Error_Flag_B = false;

            end3V3_Flag_B = false;
            endVCB_Flag_B = false;

            labelErr_B.BackColor = System.Drawing.SystemColors.Control;
            labelOK_B.BackColor = System.Drawing.SystemColors.Control;
        }

        //automatic powerup sequence
        private void timer_powerUpSequence_Tick(object sender, EventArgs e)
        {
            //---- mod A ----
            if (autoPowerUp_Flag_A)
            {                                 
                switch (autoPowerUp_State_3V3_A)
                {
                    case eAutoPowerupState.init:
                        labelWait_A.BackColor = Color.Orange;
                        autoPowerUp_State_3V3_A = eAutoPowerupState.checkTemperature_Mod1;
                        break;

                    case eAutoPowerupState.checkTemperature_Mod1:
                        if (temperatureRead_3V3_I_A < MAXTEMP)
                            autoPowerUp_State_3V3_A = eAutoPowerupState.Enable_In_Mod1;
                        else
                            autoPowerUp_State_3V3_A = eAutoPowerupState.checkTemperature_Mod2; // continue to module 2                   
                        break;

                    case eAutoPowerupState.Enable_In_Mod1:
                        communication.SendCommand(Communication.eCommandCode.ch1_A_in_enable, 1);
                        autoPowerUp_State_3V3_A = eAutoPowerupState.checkVoltage_Mod1;
                        break;

                    case eAutoPowerupState.checkVoltage_Mod1:
                        if (voltageRead_3V3_I_A > MIN3V3 && voltageRead_3V3_I_A < MAX3V3)
                            autoPowerUp_State_3V3_A = eAutoPowerupState.Enable_Out_Mod1;
                        else
                        {
                            communication.SendCommand(Communication.eCommandCode.ch1_A_in_enable, 0); // power off module 1
                            autoPowerUp_State_3V3_A = eAutoPowerupState.checkTemperature_Mod2; // continue to module 2
                        }
                        break;

                    case eAutoPowerupState.Enable_Out_Mod1:
                        communication.SendCommand(Communication.eCommandCode.ch1_A_out_enable, 1);
                        autoPowerUp_State_3V3_A = eAutoPowerupState.end_OK; // OK end
                        break;

                    case eAutoPowerupState.checkTemperature_Mod2:
                        if (temperatureRead_3V3_II_A < MAXTEMP)
                            autoPowerUp_State_3V3_A = eAutoPowerupState.Enable_In_Mod2;
                        else
                            autoPowerUp_State_3V3_A = eAutoPowerupState.end_Error; // fail                       
                        break;

                    case eAutoPowerupState.Enable_In_Mod2:
                        communication.SendCommand(Communication.eCommandCode.ch2_A_in_enable, 1);
                        autoPowerUp_State_3V3_A = eAutoPowerupState.checkVoltage_Mod2;
                        break;

                    case eAutoPowerupState.checkVoltage_Mod2: // check module 2 voltage
                        if (voltageRead_3V3_II_A > MIN3V3 && voltageRead_3V3_II_A < MAX3V3)
                            autoPowerUp_State_3V3_A = eAutoPowerupState.Enable_Out_Mod2;
                        else
                        {
                            communication.SendCommand(Communication.eCommandCode.ch2_A_in_enable, 0);
                            autoPowerUp_State_3V3_A = eAutoPowerupState.end_Error; // fail
                        }
                        break;

                    case eAutoPowerupState.Enable_Out_Mod2:// set output module 2
                        communication.SendCommand(Communication.eCommandCode.ch2_A_out_enable, 1);
                        autoPowerUp_State_3V3_A = eAutoPowerupState.end_OK; // OK end
                        break;

                    case eAutoPowerupState.end_OK: // rail online, OK
                        autoPowerUp_OK_Flag_A++;
                        end3V3_Flag_A = true;
                        break;

                    case eAutoPowerupState.end_Error: // rail offline, autoset fail
                        autoPowerUp_Error_Flag_A = true;
                        end3V3_Flag_A = true;
                        break;

                    default:
                        break;
                }

                switch (autoPowerUp_State_VCB_A)
                {
                    case eAutoPowerupState.init: 

                        autoPowerUp_State_VCB_A = eAutoPowerupState.checkTemperature_Mod1;
                        break;

                    case eAutoPowerupState.checkTemperature_Mod1:
                        if (temperatureRead_VCB_I_A < MAXTEMP)
                            autoPowerUp_State_VCB_A = eAutoPowerupState.Enable_In_Mod1;
                        else
                            autoPowerUp_State_VCB_A = eAutoPowerupState.checkTemperature_Mod2; // continue to module 2
                        break;

                    case eAutoPowerupState.Enable_In_Mod1:
                        communication.SendCommand(Communication.eCommandCode.ch3_A_in_enable, 1);
                        autoPowerUp_State_VCB_A = eAutoPowerupState.checkVoltage_Mod1;
                        break;

                    case eAutoPowerupState.checkVoltage_Mod1: 
                        if (voltageRead_VCB_I_A > MINVCB && voltageRead_VCB_I_A < MAXVCB)
                            autoPowerUp_State_VCB_A = eAutoPowerupState.Enable_Out_Mod1;
                        else
                        {
                            communication.SendCommand(Communication.eCommandCode.ch3_A_in_enable, 0); // power off module 1
                            autoPowerUp_State_VCB_A = eAutoPowerupState.checkTemperature_Mod2; // continue to module 2
                        }
                        break;

                    case eAutoPowerupState.Enable_Out_Mod1:
                        communication.SendCommand(Communication.eCommandCode.ch3_A_out_enable, 1);
                        autoPowerUp_State_VCB_A = eAutoPowerupState.end_OK; // OK end
                        break;

                    case eAutoPowerupState.checkTemperature_Mod2: 
                        if (temperatureRead_VCB_II_A < MAXTEMP)
                            autoPowerUp_State_VCB_A = eAutoPowerupState.Enable_In_Mod2;
                        else
                            autoPowerUp_State_VCB_A = eAutoPowerupState.end_Error; // fail
                        break;

                    case eAutoPowerupState.Enable_In_Mod2:
                        communication.SendCommand(Communication.eCommandCode.ch4_A_in_enable, 1);
                        autoPowerUp_State_VCB_A = eAutoPowerupState.checkVoltage_Mod2;
                        break;

                    case eAutoPowerupState.checkVoltage_Mod2: // check module 2 voltage
                        if (voltageRead_VCB_II_A > MINVCB && voltageRead_VCB_II_A < MAXVCB)
                            autoPowerUp_State_VCB_A = eAutoPowerupState.Enable_Out_Mod2;
                        else
                        {
                            communication.SendCommand(Communication.eCommandCode.ch4_A_in_enable, 0); // power off module 2
                            autoPowerUp_State_VCB_A = eAutoPowerupState.end_Error; // fail
                        }
                        break;

                    case eAutoPowerupState.Enable_Out_Mod2:// set output module 2
                        communication.SendCommand(Communication.eCommandCode.ch4_A_out_enable, 1);
                        autoPowerUp_State_VCB_A = eAutoPowerupState.end_OK; // OK end
                        break;

                    case eAutoPowerupState.end_OK: // rail online, OK
                        autoPowerUp_OK_Flag_A++;
                        endVCB_Flag_A = true;
                        break;

                    case eAutoPowerupState.end_Error: // rail offline, autoset fail
                        autoPowerUp_Error_Flag_A = true;
                        endVCB_Flag_A = true;
                        break;

                    default:
                        break;
                }

                if(end3V3_Flag_A && endVCB_Flag_A)// autoset finished
                {
                    if(autoPowerUp_Error_Flag_A)
                    {
                        labelErr_A.BackColor = Color.Red;
                    }
                    else if(autoPowerUp_OK_Flag_A > 1) // OK
                    {
                        labelOK_A.BackColor = Color.Green;
                    }
                    else
                    {
                        labelErr_A.BackColor = Color.Red;
                    }

                    labelWait_A.BackColor = System.Drawing.SystemColors.Control;

                    autoPowerUp_State_3V3_A = eAutoPowerupState.init;
                    autoPowerUp_State_VCB_A = eAutoPowerupState.init;
                    autoPowerUp_Flag_A = false;
                    end3V3_Flag_A = false;
                    endVCB_Flag_A = false;
                }

            }


            //---- mod B ----
            if (autoPowerUp_Flag_B)
            {
                switch (autoPowerUp_State_3V3_B)
                {
                    case eAutoPowerupState.init:
                        labelWait_B.BackColor = Color.Orange;
                        autoPowerUp_State_3V3_B = eAutoPowerupState.checkTemperature_Mod1;
                        break;

                    case eAutoPowerupState.checkTemperature_Mod1:
                        if (temperatureRead_3V3_I_B < MAXTEMP)
                            autoPowerUp_State_3V3_B = eAutoPowerupState.Enable_In_Mod1;
                        else
                            autoPowerUp_State_3V3_B = eAutoPowerupState.checkTemperature_Mod2; // continue to module 2                   
                        break;

                    case eAutoPowerupState.Enable_In_Mod1:
                        communication.SendCommand(Communication.eCommandCode.ch1_B_in_enable, 1);
                        autoPowerUp_State_3V3_B = eAutoPowerupState.checkVoltage_Mod1;
                        break;

                    case eAutoPowerupState.checkVoltage_Mod1:
                        if (voltageRead_3V3_I_B > MIN3V3 && voltageRead_3V3_I_B < MAX3V3)
                            autoPowerUp_State_3V3_B = eAutoPowerupState.Enable_Out_Mod1;
                        else
                        {
                            communication.SendCommand(Communication.eCommandCode.ch1_B_in_enable, 0); // power off module 1
                            autoPowerUp_State_3V3_B = eAutoPowerupState.checkTemperature_Mod2; // continue to module 2
                        }
                        break;

                    case eAutoPowerupState.Enable_Out_Mod1:
                        communication.SendCommand(Communication.eCommandCode.ch1_B_out_enable, 1);
                        autoPowerUp_State_3V3_B = eAutoPowerupState.end_OK; // OK end
                        break;

                    case eAutoPowerupState.checkTemperature_Mod2:
                        if (temperatureRead_3V3_II_B < MAXTEMP)
                            autoPowerUp_State_3V3_B = eAutoPowerupState.Enable_In_Mod2;
                        else
                            autoPowerUp_State_3V3_B = eAutoPowerupState.end_Error; // fail                       
                        break;

                    case eAutoPowerupState.Enable_In_Mod2:
                        communication.SendCommand(Communication.eCommandCode.ch2_B_in_enable, 1);
                        autoPowerUp_State_3V3_B = eAutoPowerupState.checkVoltage_Mod2;
                        break;

                    case eAutoPowerupState.checkVoltage_Mod2: // check module 2 voltage
                        if (voltageRead_3V3_II_B > MIN3V3 && voltageRead_3V3_II_B < MAX3V3)
                            autoPowerUp_State_3V3_B = eAutoPowerupState.Enable_Out_Mod2;
                        else
                        {
                            communication.SendCommand(Communication.eCommandCode.ch2_B_in_enable, 0);
                            autoPowerUp_State_3V3_B = eAutoPowerupState.end_Error; // fail
                        }
                        break;

                    case eAutoPowerupState.Enable_Out_Mod2:// set output module 2
                        communication.SendCommand(Communication.eCommandCode.ch2_B_out_enable, 1);
                        autoPowerUp_State_3V3_B = eAutoPowerupState.end_OK; // OK end
                        break;

                    case eAutoPowerupState.end_OK: // rail online, OK
                        autoPowerUp_OK_Flag_B++;
                        end3V3_Flag_B = true;
                        break;

                    case eAutoPowerupState.end_Error: // rail offline, autoset fail
                        autoPowerUp_Error_Flag_B = true;
                        end3V3_Flag_B = true;
                        break;

                    default:
                        break;
                }

                switch (autoPowerUp_State_VCB_B)
                {
                    case eAutoPowerupState.init:

                        autoPowerUp_State_VCB_B = eAutoPowerupState.checkTemperature_Mod1;
                        break;

                    case eAutoPowerupState.checkTemperature_Mod1:
                        if (temperatureRead_VCB_I_B < MAXTEMP)
                            autoPowerUp_State_VCB_B = eAutoPowerupState.Enable_In_Mod1;
                        else
                            autoPowerUp_State_VCB_B = eAutoPowerupState.checkTemperature_Mod2; // continue to module 2
                        break;

                    case eAutoPowerupState.Enable_In_Mod1:
                        communication.SendCommand(Communication.eCommandCode.ch3_B_in_enable, 1);
                        autoPowerUp_State_VCB_B = eAutoPowerupState.checkVoltage_Mod1;
                        break;

                    case eAutoPowerupState.checkVoltage_Mod1:
                        if (voltageRead_VCB_I_B > MINVCB && voltageRead_VCB_I_B < MAXVCB)
                            autoPowerUp_State_VCB_B = eAutoPowerupState.Enable_Out_Mod1;
                        else
                        {
                            communication.SendCommand(Communication.eCommandCode.ch3_B_in_enable, 0); // power off module 1
                            autoPowerUp_State_VCB_B = eAutoPowerupState.checkTemperature_Mod2; // continue to module 2
                        }
                        break;

                    case eAutoPowerupState.Enable_Out_Mod1:
                        communication.SendCommand(Communication.eCommandCode.ch3_B_out_enable, 1);
                        autoPowerUp_State_VCB_B = eAutoPowerupState.end_OK; // OK end
                        break;

                    case eAutoPowerupState.checkTemperature_Mod2:
                        if (temperatureRead_VCB_II_B < MAXTEMP)
                            autoPowerUp_State_VCB_B = eAutoPowerupState.Enable_In_Mod2;
                        else
                            autoPowerUp_State_VCB_B = eAutoPowerupState.end_Error; // fail
                        break;

                    case eAutoPowerupState.Enable_In_Mod2:
                        communication.SendCommand(Communication.eCommandCode.ch4_B_in_enable, 1);
                        autoPowerUp_State_VCB_B = eAutoPowerupState.checkVoltage_Mod2;
                        break;

                    case eAutoPowerupState.checkVoltage_Mod2: // check module 2 voltage
                        if (voltageRead_VCB_II_B > MINVCB && voltageRead_VCB_II_B < MAXVCB)
                            autoPowerUp_State_VCB_B = eAutoPowerupState.Enable_Out_Mod2;
                        else
                        {
                            communication.SendCommand(Communication.eCommandCode.ch4_B_in_enable, 0); // power off module 2
                            autoPowerUp_State_VCB_B = eAutoPowerupState.end_Error; // fail
                        }
                        break;

                    case eAutoPowerupState.Enable_Out_Mod2:// set output module 2
                        communication.SendCommand(Communication.eCommandCode.ch4_B_out_enable, 1);
                        autoPowerUp_State_VCB_B = eAutoPowerupState.end_OK; // OK end
                        break;

                    case eAutoPowerupState.end_OK: // rail online, OK
                        autoPowerUp_OK_Flag_B++;
                        endVCB_Flag_B = true;
                        break;

                    case eAutoPowerupState.end_Error: // rail offline, autoset fail
                        autoPowerUp_Error_Flag_B = true;
                        endVCB_Flag_B = true;
                        break;

                    default:
                        break;
                }

                if (end3V3_Flag_B && endVCB_Flag_B)// autoset finished
                {
                    if (autoPowerUp_Error_Flag_B)
                    {
                        labelErr_B.BackColor = Color.Red;
                    }
                    else if (autoPowerUp_OK_Flag_B > 1) // OK
                    {
                        labelOK_B.BackColor = Color.Green;
                    }
                    else
                    {
                        labelErr_B.BackColor = Color.Red;
                    }

                    labelWait_B.BackColor = System.Drawing.SystemColors.Control;

                    autoPowerUp_State_3V3_B = eAutoPowerupState.init;
                    autoPowerUp_State_VCB_B = eAutoPowerupState.init;
                    autoPowerUp_Flag_B = false;
                    end3V3_Flag_B = false;
                    endVCB_Flag_B = false;
                }
            }
        }


        //protection checkboxes-------------------------------------------------------------------------------------
        //---- mod A ----
        private void checkBox_3V3_OVP_A_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_3V3_OVP_A.Checked)
                communication.SendCommand(Communication.eCommandCode.enable_OVP_A_3V3, 1);
            else
                communication.SendCommand(Communication.eCommandCode.enable_OVP_A_3V3, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void checkBox_VCB_OVP_A_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_VCB_OVP_A.Checked)
                communication.SendCommand(Communication.eCommandCode.enable_OVP_A_VCB, 1);
            else
                communication.SendCommand(Communication.eCommandCode.enable_OVP_A_VCB, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void checkBox_3V3_OTP_A_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_3V3_OTP_A.Checked)
                communication.SendCommand(Communication.eCommandCode.enable_OTP_A_3V3, 1);
            else
                communication.SendCommand(Communication.eCommandCode.enable_OTP_A_3V3, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void checkBox_VCB_OTP_A_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_VCB_OTP_A.Checked)
                communication.SendCommand(Communication.eCommandCode.enable_OTP_A_VCB, 1);
            else
                communication.SendCommand(Communication.eCommandCode.enable_OTP_A_VCB, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        //---- mod B ----
        private void checkBox_3V3_OVP_B_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_3V3_OVP_B.Checked)
                communication.SendCommand(Communication.eCommandCode.enable_OVP_B_3V3, 1);
            else
                communication.SendCommand(Communication.eCommandCode.enable_OVP_B_3V3, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);

        }

        private void checkBox_VCB_OVP_B_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_VCB_OVP_B.Checked)
                communication.SendCommand(Communication.eCommandCode.enable_OVP_B_VCB, 1);
            else
                communication.SendCommand(Communication.eCommandCode.enable_OVP_B_VCB, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void checkBox_3V3_OTP_B_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_3V3_OTP_B.Checked)
                communication.SendCommand(Communication.eCommandCode.enable_OTP_B_3V3, 1);
            else
                communication.SendCommand(Communication.eCommandCode.enable_OTP_B_3V3, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        private void checkBox_VCB_OTP_B_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_VCB_OTP_B.Checked)
                communication.SendCommand(Communication.eCommandCode.enable_OTP_B_VCB, 1);
            else
                communication.SendCommand(Communication.eCommandCode.enable_OTP_B_VCB, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }

        //protection numericupdown-------------------------------------------------------------------------------------
        //---- mod A ----
        private void numericUpDown_3V3_OVP_A_ValueChanged(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.set_OVP_A_3V3, (uint)(numericUpDown_3V3_OVP_A.Value * 10));

            communication.SendCommand(Communication.eCommandCode.get_OVP_A_3V3, 0);
        }

        private void numericUpDown_VCB_OVP_A_ValueChanged(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.set_OVP_A_VCB, (uint)(numericUpDown_VCB_OVP_A.Value * 10));

            communication.SendCommand(Communication.eCommandCode.get_OVP_A_VCB, 0);
        }

        private void numericUpDown_3V3_OTP_A_ValueChanged(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.set_OTP_A_3V3, (uint)(numericUpDown_3V3_OTP_A.Value * 10));

            communication.SendCommand(Communication.eCommandCode.get_OTP_A_3V3, 0);
        }

        private void numericUpDown_VCB_OTP_A_ValueChanged(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.set_OTP_A_VCB, (uint)(numericUpDown_VCB_OTP_A.Value * 10));

            communication.SendCommand(Communication.eCommandCode.get_OTP_A_VCB, 0);
        }

        //---- mod B ----
        private void numericUpDown_3V3_OVP_B_ValueChanged(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.set_OVP_B_3V3, (uint)(numericUpDown_3V3_OVP_B.Value * 10));

            communication.SendCommand(Communication.eCommandCode.get_OVP_B_3V3, 0);
        }

        private void numericUpDown_VCB_OVP_B_ValueChanged(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.set_OVP_B_VCB, (uint)(numericUpDown_VCB_OVP_B.Value * 10));

            communication.SendCommand(Communication.eCommandCode.get_OVP_B_VCB, 0);
        }

        private void numericUpDown_3V3_OTP_B_ValueChanged(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.set_OTP_B_3V3, (uint)(numericUpDown_3V3_OTP_B.Value * 10));

            communication.SendCommand(Communication.eCommandCode.get_OTP_B_3V3, 0);
        }

        private void numericUpDown_VCB_OTP_B_ValueChanged(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.set_OTP_B_VCB, (uint)(numericUpDown_VCB_OTP_B.Value * 10));

            communication.SendCommand(Communication.eCommandCode.get_OTP_B_VCB, 0);
        }

        //everything off-------------------------------------------------------------------------------------
        private void btn_allOff_Click(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.ch1_A_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch2_A_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch3_A_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch4_A_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.ch1_A_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch2_A_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch3_A_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch4_A_in_enable, 0);

            communication.SendCommand(Communication.eCommandCode.ch1_B_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch2_B_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch3_B_out_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch4_B_out_enable, 0);

            communication.SendCommand(Communication.eCommandCode.ch1_B_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch2_B_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch3_B_in_enable, 0);
            communication.SendCommand(Communication.eCommandCode.ch4_B_in_enable, 0);

            communication.SendCommand(Communication.eCommandCode.getsetting, 0);
        }


        //---- mod A ----
        private void btn_protectionDefaults_A_Click(object sender, EventArgs e)
        {
            numericUpDown_3V3_OVP_A.Value = (decimal)3.4;
            numericUpDown_VCB_OVP_A.Value = (decimal)4.2;
            numericUpDown_3V3_OTP_A.Value = (decimal)85.0;
            numericUpDown_VCB_OTP_A.Value = (decimal)85.0;
        }

        //---- mod B ----
        private void btn_protectionDefaults_B_Click(object sender, EventArgs e)
        {
            numericUpDown_3V3_OVP_B.Value = (decimal)3.4;
            numericUpDown_VCB_OVP_B.Value = (decimal)4.2;
            numericUpDown_3V3_OTP_B.Value = (decimal)85.0;
            numericUpDown_VCB_OTP_B.Value = (decimal)85.0;
        }

        private void btn_moduleReset_Click(object sender, EventArgs e)
        {
            communication.SendCommand(Communication.eCommandCode.reset, 1);
        }
    }
}