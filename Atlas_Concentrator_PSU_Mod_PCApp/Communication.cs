using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HV_Power_Supply_GUI_ver._1
{
    class Communication
    {

        public enum eCommandCode : byte
        {
        NON,
        Connected,
        ch1_A_in_enable,
        ch2_A_in_enable,
        ch3_A_in_enable,
        ch4_A_in_enable,
        ch1_B_in_enable,
        ch2_B_in_enable,
        ch3_B_in_enable,
        ch4_B_in_enable,
        ch1_A_out_enable,
        ch2_A_out_enable,
        ch3_A_out_enable,
        ch4_A_out_enable,
        ch1_B_out_enable,
        ch2_B_out_enable,
        ch3_B_out_enable,
        ch4_B_out_enable,
        ch1_A_getvoltage,
        ch2_A_getvoltage,
        ch3_A_getvoltage,
        ch4_A_getvoltage,
        ch1_B_getvoltage,
        ch2_B_getvoltage,
        ch3_B_getvoltage,
        ch4_B_getvoltage,
        ch1_A_gettemperature,
        ch2_A_gettemperature,
        ch3_A_gettemperature,
        ch4_A_gettemperature,
        ch1_B_gettemperature,
        ch2_B_gettemperature,
        ch3_B_gettemperature,
        ch4_B_gettemperature,
        all_channels_off_A,
        all_channels_off_B,
        enable_OVP_A_3V3,
        enable_OVP_A_VCB,
        enable_OVP_B_3V3,
        enable_OVP_B_VCB,
        enable_OTP_A_3V3,
        enable_OTP_A_VCB,
        enable_OTP_B_3V3,
        enable_OTP_B_VCB,
        set_OVP_A_3V3,
        set_OVP_A_VCB,
        set_OVP_B_3V3,
        set_OVP_B_VCB,
        set_OTP_A_3V3,
        set_OTP_A_VCB,
        set_OTP_B_3V3,
        set_OTP_B_VCB,
        get_OVP_A_3V3,
        get_OVP_A_VCB,
        get_OVP_B_3V3,
        get_OVP_B_VCB,
        get_OTP_A_3V3,
        get_OTP_A_VCB,
        get_OTP_B_3V3,
        get_OTP_B_VCB,
        error_signals,
        getallvalues,
        getsetting,
        thatsall,
        LED,
        ip_store_endpoint,
        ip_store_myip,
        ip_store_mymask,
        ip_store_mygatew,
        ip_get_myip,
        ip_get_mymask,
        ip_get_mygatew,
        ip_getsetting,
        reset
        }

        string[] command_strings =
        {
        "NON",
        "Connected",
        "ch1_A_in_enable",
        "ch2_A_in_enable",
        "ch3_A_in_enable",
        "ch4_A_in_enable",
        "ch1_B_in_enable",
        "ch2_B_in_enable",
        "ch3_B_in_enable",
        "ch4_B_in_enable",
        "ch1_A_out_enable",
        "ch2_A_out_enable",
        "ch3_A_out_enable",
        "ch4_A_out_enable",
        "ch1_B_out_enable",
        "ch2_B_out_enable",
        "ch3_B_out_enable",
        "ch4_B_out_enable",
        "ch1_A_getvoltage",
        "ch2_A_getvoltage",
        "ch3_A_getvoltage",
        "ch4_A_getvoltage",
        "ch1_B_getvoltage",
        "ch2_B_getvoltage",
        "ch3_B_getvoltage",
        "ch4_B_getvoltage",
        "ch1_A_gettemperature",
        "ch2_A_gettemperature",
        "ch3_A_gettemperature",
        "ch4_A_gettemperature",
        "ch1_B_gettemperature",
        "ch2_B_gettemperature",
        "ch3_B_gettemperature",
        "ch4_B_gettemperature",
        "all_channels_off_A",
        "all_channels_off_B",
        "enable_OVP_A_3V3",
        "enable_OVP_A_VCB",
        "enable_OVP_B_3V3",
        "enable_OVP_B_VCB",
        "enable_OTP_A_3V3",
        "enable_OTP_A_VCB",
        "enable_OTP_B_3V3",
        "enable_OTP_B_VCB",
        "set_OVP_A_3V3",
        "set_OVP_A_VCB",
        "set_OVP_B_3V3",
        "set_OVP_B_VCB",
        "set_OTP_A_3V3",
        "set_OTP_A_VCB",
        "set_OTP_B_3V3",
        "set_OTP_B_VCB",
        "get_OVP_A_3V3",
        "get_OVP_A_VCB",
        "get_OVP_B_3V3",
        "get_OVP_B_VCB",
        "get_OTP_A_3V3",
        "get_OTP_A_VCB",
        "get_OTP_B_3V3",
        "get_OTP_B_VCB",
        "error_signals",
        "getallvalues",
        "getsetting",
        "thatsall",
        "LED",
        "ip_store_endpoint",
        "ip_store_myip",
        "ip_store_mymask",
        "ip_store_mygatew",
        "ip_get_myip",
        "ip_get_mymask",
        "ip_get_mygatew",
        "ip_getsetting",
        "reset"
        };

        public enum eCommunicationType : byte
        {
            non,
            serial,
            udp
        };

        

        Timer read_timer = new Timer();

        public delegate void efunction();
        efunction ExecuteFunction;
        private SerialPort serialport;
        private UdpClient udp;
        private int udpport;
        private IPAddress ip_enpoint;
        private eCommunicationType CommunicationType = eCommunicationType.non;




        public Communication(SerialPort serialport, int UDPPort, efunction f)
        {
            this.serialport = serialport;
            //this.udp = new UdpClient(UDPPort);
            udpport = UDPPort;
            CommunicationType = eCommunicationType.non;
            ExecuteFunction = f;

            read_timer.Tick += new System.EventHandler(SerialReadCommand);
            read_timer.Interval = 10;
        }

        public bool Open_UDP(string ipadress)
        {
            udp = new UdpClient(udpport);

            IPAddress ip;
            if (IPAddress.TryParse(ipadress, out ip))
            {
                ip_enpoint = ip;
                //udp.Connect(ip, udpport);
                udp.BeginReceive(new AsyncCallback(UdpReceive), udp);
                CommunicationType = eCommunicationType.udp;
                return true;
            }
            CommunicationType = eCommunicationType.non;

            return false;
        }

        public void Close_UDP()
        {
            if (!(CommunicationType == eCommunicationType.udp)) return;

            udp.Close();

            CommunicationType = eCommunicationType.non;
        }

        public string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        public bool Open_Serial(string name)
        {
            serialport.PortName = name;

            try
            {
                serialport.Open();
                CommunicationType = eCommunicationType.serial;
                read_timer.Enabled = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CommunicationType = eCommunicationType.non;
                return false;
            }
        }

        public void Close_Serial()
        {
            if (!(CommunicationType == eCommunicationType.serial)) return;

            read_timer.Enabled = false;
            serialport.Close();
            CommunicationType = eCommunicationType.non;
        }

        public bool IsOpen()
        {
            return serialport.IsOpen;
        }

        public eCommunicationType GetCommunicationType()
        {
            return CommunicationType;
        }

        public void SendCommand(eCommandCode Command, UInt32 Data)
        {

            //string s = command_strings[(int)Command] + "=" + Data.ToString() + "\n\r";

            string s = "/" + ((int)Command).ToString() + "=" + Data.ToString() + "\n\r";

            if (CommunicationType == eCommunicationType.serial)
            {
                if (!serialport.IsOpen) return;
                serialport.Write(s);
            }

            else if (CommunicationType == eCommunicationType.udp)
            {
                byte[] data = Encoding.ASCII.GetBytes(s);
                udp.Send(data, data.Length, new IPEndPoint(ip_enpoint, udpport));
            }

        }




        public eCommandCode ReadCommand_Code;
        public UInt32 ReadCommand_Data;
        public float ReadCommand_Data_float;

        public string lineXXX;

        byte[] lineBuffer = new byte[128];
        int LineBuf_pos = 0;

        private void SerialReadCommand(object sender, EventArgs e)
        {
            if (!serialport.IsOpen)
            {
                ReadCommand_Code = eCommandCode.NON;
                return;
            }


            while (serialport.BytesToRead > 0)
            {
                byte b = (byte)serialport.ReadByte();

                if ((b == '\n') || (b == '\r'))
                {
                    if (LineBuf_pos == 0) //zatim nic v bufferu
                        continue; // pokracuj

                    //cmd_string = Encoding.UTF8.GetString(lineBuffer, 0, LineBuf_pos);
                    ProcessLine(Encoding.UTF8.GetString(lineBuffer, 0, LineBuf_pos));
                    ExecuteFunction();

                    LineBuf_pos = 0;
                    return;
                }
                lineBuffer[LineBuf_pos] = b;
                LineBuf_pos++;

                if (LineBuf_pos >= lineBuffer.Length) //preteka
                {
                    LineBuf_pos = lineBuffer.Length - 1;
                }//TODO vypis overflow
            }

            ReadCommand_Code = eCommandCode.NON;
            return;
        }

        private void ProcessLine(string line)
        {
            if (String.IsNullOrEmpty(line))
            {
                ReadCommand_Code = eCommandCode.NON;
                return;
            }

            
            string[] polozky = line.Split('=');
            

            if (polozky.Length >= 2)
            {
                if (polozky[0].StartsWith("/"))
                {
                    string s = polozky[0].Remove(0, 1);
                    uint x = 0;

                    if (!UInt32.TryParse(s, out x))
                    {
                        ReadCommand_Code = 0;
                    }
                    else
                    {
                        ReadCommand_Code = (eCommandCode)x;
                    }

                }

                else
                {
                    for (int i = 0; i < command_strings.Length; i++)
                    {
                        if (String.Compare(polozky[0], command_strings[i]) == 0)
                        {
                            ReadCommand_Code = (eCommandCode)i;
                            break;
                        }
                        else
                        {
                            ReadCommand_Code = eCommandCode.NON;
                        }
                    }
                }

                if (!UInt32.TryParse(polozky[1], out ReadCommand_Data))
                {
                    
                    ReadCommand_Data = 0;
                }



                if (!float.TryParse(polozky[1], System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out ReadCommand_Data_float))
                {
                    ReadCommand_Data_float = 0;
                }
                
            }
        }

        private void UdpReceive(IAsyncResult ar) 
        {

            UdpClient uu = ar.AsyncState as UdpClient;
            if (uu == null)
            {

                return;
            }

            IPEndPoint ipe = new IPEndPoint(ip_enpoint, 0);
            //IPEndPoint ipe = new IPEndPoint(IPAddress.Any, 0); //port 0 je pro vsechny
            
            try 
            {
                byte[] data = uu.EndReceive(ar, ref ipe);
                string s = Encoding.ASCII.GetString(data);
                s = s.Replace('\n', ' ');
                s = s.Replace('\r', ' ');
                s = s.Trim();
                //s = s + 0;


                ProcessLine(s);
                ExecuteFunction();

                uu.BeginReceive(new AsyncCallback(UdpReceive), uu);
            }
            catch(Exception ie)
            {

                Close_UDP();
                return;
            }
            
        }
    
    }
}
