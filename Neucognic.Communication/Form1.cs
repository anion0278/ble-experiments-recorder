using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bleRecorder_test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        byte[] tx_buffer = new byte[25];
        string rx_buffer;
        byte sn_low;
        byte sn_hi;
        bool simulation_run = false;
        bool ack = false;



        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                button1.Text = "Connect";
            } else
            {
                serialPort1.PortName = textPortName.Text;
                serialPort1.Open();
                if (serialPort1.IsOpen)
                {
                    button1.Text = "Disconnect";
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tx_buffer[0] = 0x44;
            tx_buffer[1] = 0x54;
            tx_buffer[2] = 0x3D;
            tx_buffer[3] = 0x53;

            serialPort1.BaudRate = 38400;
            serialPort1.Parity = System.IO.Ports.Parity.None;
            serialPort1.StopBits = System.IO.Ports.StopBits.One;
            serialPort1.DataBits = 8;
               

            int sn = int.Parse(textBox1.Text);
            sn_low = (byte) (sn & 0xFF);
            sn_hi = (byte) ((sn >> 8) & 0xFF);

            comboBox1.Text = "50us";

            label24.Text = trackBar1.Minimum.ToString();
            label25.Text = trackBar1.Maximum.ToString();

            trackBar1.Value = (int)numericUpDown1.Value;
            label26.Text = trackBar1.Value.ToString();

        }

        private void btnNeuGait_PowerOff_Click(object sender, EventArgs e)
        {
            tx_buffer[4] = 0x07;
            tx_buffer[5] = 0x00;
            tx_buffer[6] = 0x04;

            tx_buffer[7] = 0xED;
            tx_buffer[8] = 0x07;

            tx_buffer[9] = sn_hi;
            tx_buffer[10] = sn_low;

            tx_buffer[11] = CalculateCheckSum();

            tx_buffer[12] = 0x0D;
            tx_buffer[13] = 0x0A;



            if (serialPort1.IsOpen)
            {
                serialPort1.Write(tx_buffer, 0, 14);
            }

        }


        byte CalculateCheckSum()
        {
            byte checksum = 0;

            for (int i = 5; i <= (tx_buffer[6]) + 6; i++)
            {
                checksum ^= tx_buffer[i];
            }
            return checksum;
        }

        private void btnNeuGait_ReatBatteryFuelLevel_Click(object sender, EventArgs e)
        {
            rx_buffer = "";

            tx_buffer[4] = 0x07;

            tx_buffer[5] = 0x00;
            tx_buffer[6] = 0x04;

            tx_buffer[7] = 0xED;
            tx_buffer[8] = 0x06;

            tx_buffer[9] = sn_hi;
            tx_buffer[10] = sn_low;

            tx_buffer[11] = CalculateCheckSum();

            tx_buffer[12] = 0x0D;
            tx_buffer[13] = 0x0A;



            if (serialPort1.IsOpen)
            {
                serialPort1.Write(tx_buffer, 0, 14);
            }


        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

            string rx = serialPort1.ReadExisting();
            rx_buffer += rx;

            int find = rx_buffer.IndexOf("BAT");
            if (find > 0)
            {
                int bat_value = 0;
                try
                {
                    bat_value = (byte)rx_buffer[find + 3];
                }
                catch (Exception)
                {

                }
                
                if (bat_value > 0)
                {
                    textBox2.BeginInvoke(new Action(() => { textBox2.AppendText("\r\nBAT: " + bat_value.ToString() + "\r\n"); }));
                    rx_buffer = "";
                }
            }

            find = rx_buffer.IndexOf("ACK");
            if (find > 0)
            {
                ack = true;
                rx_buffer = "";
            }


            textBox2.BeginInvoke(new Action(() => { textBox2.AppendText(rx); }));

            //rx_buffer = "";




        }


        private void Btn_SetSimulationParameters(object sender, EventArgs e)
        {
            byte pusle_width;
            byte stim_waveform;

            switch (comboBox1.Text)
            {

                case "100us":
                    pusle_width = 0x04;
                    break;

                case "200us":
                    pusle_width = 0x08;
                    break;

                case "300s":
                    pusle_width = 0x0C;
                    break;

                case "400us":
                    pusle_width = 0x10;
                    break;

                case "50us":
                default:
                    pusle_width = 0;
                    break;

            }

            if (comboBox2.Text == "ASYM")
            {
                stim_waveform = 1;
            } else
            {
                stim_waveform = 0;
            }


            tx_buffer[4] = 0x0A;

            tx_buffer[5] = 0x00;
            tx_buffer[6] = 0x07;

            tx_buffer[7] = 0xED;
            tx_buffer[8] = 0x01;

            tx_buffer[9] = sn_hi;
            tx_buffer[10] = sn_low;

            tx_buffer[11] = byte.Parse(numericUpDown1.Text);
            tx_buffer[12] = (byte)(pusle_width | stim_waveform);
            tx_buffer[13] = byte.Parse(numericUpDown2.Text);
            
                        
            tx_buffer[14] = CalculateCheckSum();

            tx_buffer[15] = 0x0D;
            tx_buffer[16] = 0x0A;



            if (serialPort1.IsOpen)
            {
                ack = false;
                serialPort1.Write(tx_buffer, 0, 17);
            }


        }

        private void Btn_NeugaitStartStimulation_Click(object sender, EventArgs e)
        {

            tx_buffer[4] = 0x09;

            tx_buffer[5] = 0x00;
            tx_buffer[6] = 0x06;

            tx_buffer[7] = 0xED;
            tx_buffer[8] = 0x08;

            tx_buffer[9] = sn_hi;
            tx_buffer[10] = sn_low;

            int delay = byte.Parse(numericUpDown3.Text);


            tx_buffer[11] = (byte)((delay >> 8) & 0xFF);
            tx_buffer[12] = (byte)(delay & 0xFF);

            tx_buffer[13] = CalculateCheckSum();

            tx_buffer[14] = 0x0D;
            tx_buffer[15] = 0x0A;

            if (serialPort1.IsOpen)
            {
                ack = false;
                serialPort1.Write(tx_buffer, 0, 16);
            }

            simulation_run = true;


        }

        private void btn_NeugaitStopStimulation_Click(object sender, EventArgs e)
        {

            tx_buffer[4] = 0x09;

            tx_buffer[5] = 0x00;
            tx_buffer[6] = 0x06;

            tx_buffer[7] = 0xED;
            tx_buffer[8] = 0x09;

            tx_buffer[9] = sn_hi;
            tx_buffer[10] = sn_low;

            int delay = byte.Parse(numericUpDown3.Text);


            tx_buffer[11] = (byte)((delay >> 8) & 0xFF);
            tx_buffer[12] = (byte)(delay & 0xFF);

            tx_buffer[13] = CalculateCheckSum();

            tx_buffer[14] = 0x0D;
            tx_buffer[15] = 0x0A;

            if (serialPort1.IsOpen)
            {
                ack = false;
                serialPort1.Write(tx_buffer, 0, 16);
            }

            simulation_run = false;
        }

        private void btn_NeuGaitSetMode_Click(object sender, EventArgs e)
        {

            byte mode;

            switch (comboBox3.Text)
            {
                case "WALK":
                    mode = 1;
                    break;
                case "EXEC":
                    mode = 2;
                    break;

                case "STANDBY":
                default:
                    mode = 0;
                    break;
            }


            tx_buffer[4] = 0x08;

            tx_buffer[5] = 0x00;
            tx_buffer[6] = 0x05;

            tx_buffer[7] = 0xED;
            tx_buffer[8] = 0x05;

            tx_buffer[9] = sn_hi;
            tx_buffer[10] = sn_low;

            tx_buffer[11] = mode;

            tx_buffer[12] = CalculateCheckSum();

            tx_buffer[13] = 0x0D;
            tx_buffer[14] = 0x0A;

            if (serialPort1.IsOpen)
            {
                serialPort1.Write(tx_buffer, 0, 15);
            }

        }

        private void button3_Click_1(object sender, EventArgs e)
        {

            tx_buffer[4] = 0x08;

            tx_buffer[5] = 0x00;
            tx_buffer[6] = 0x05;

            tx_buffer[7] = 0xED;
            tx_buffer[8] = 0x00;

            tx_buffer[9] = sn_hi;
            tx_buffer[10] = sn_low;

            tx_buffer[11] = (byte)numericUpDown4.Value;

            tx_buffer[12] = CalculateCheckSum();

            tx_buffer[13] = 0x0D;
            tx_buffer[14] = 0x0A;

            if (serialPort1.IsOpen)
            {
                serialPort1.Write(tx_buffer, 0, 15);
            }

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            numericUpDown1.Value = trackBar1.Value;
            label26.Text = trackBar1.Value.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (simulation_run && checkBox1.Checked)
            {
                if (checkBox2.Checked == false)
                {
                    Btn_SetSimulationParameters(null, null);
                }
                else
                {

                    btn_NeugaitStopStimulation_Click(null, null);

                    //while(ack == false)
                    //{

                    //}

                    Btn_SetSimulationParameters(null, null);

                    //while (ack == false)
                    //{

                    //}

                    Btn_NeugaitStartStimulation_Click(null, null);


                }



            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
        }

        private void Btn_SetGaitParameters_Click(object sender, EventArgs e)
        {
            tx_buffer[4] = 0x0B;

            tx_buffer[5] = 0x00;
            tx_buffer[6] = 0x08;

            tx_buffer[7] = 0xED;
            tx_buffer[8] = 0x02;

            tx_buffer[9] = sn_hi;
            tx_buffer[10] = sn_low;

            tx_buffer[11] = (byte)(num_GaitRiseTime.Value / 255);
            tx_buffer[12] = (byte)(num_GaitFallTime.Value / 255);
            tx_buffer[13] = (byte)(num_GaitExtensionTime.Value / 255);
            tx_buffer[14] = (byte)(num_GaitMaxStimulationTime.Value);


            tx_buffer[15] = CalculateCheckSum();

            tx_buffer[16] = 0x0D;
            tx_buffer[17] = 0x0A;

            if (serialPort1.IsOpen)
            {
                ack = false;
                serialPort1.Write(tx_buffer, 0, 18);
            }

        }

        private void Btn_SetExerciceParameters_Click(object sender, EventArgs e)
        {
            tx_buffer[4] = 0x12;

            tx_buffer[5] = 0x00;
            tx_buffer[6] = 0x0F;

            tx_buffer[7] = 0xED;
            tx_buffer[8] = 0x03;

            tx_buffer[9] = sn_hi;
            tx_buffer[10] = sn_low;

            tx_buffer[11] = (byte)(num_ExcerciseRiseTime.Value / 255);
            tx_buffer[12] = (byte)(num_ExcerciseFallTime.Value / 255);
            tx_buffer[13] = (byte)(num_ExcerciseStimDuration1.Value / 255);
            tx_buffer[14] = (byte)(num_ExcerciseRestDuration1.Value / 255);
            tx_buffer[15] = (byte)(num_ExcerciseStimDuration2.Value / 255);
            tx_buffer[16] = (byte)(num_ExcerciseRestDuration2.Value / 255);
            tx_buffer[17] = (byte)(num_ExcerciseStimDuration3.Value / 255);
            tx_buffer[18] = (byte)(num_ExcerciseRestDuration3.Value / 255);
            tx_buffer[19] = (byte)(num_ExcerciseStimDuration4.Value / 255);
            tx_buffer[20] = (byte)(num_ExcerciseRestDuration4.Value / 255);
            tx_buffer[21] = (byte)(num_ExerciseTotalExcerciceTime.Value);

            tx_buffer[22] = CalculateCheckSum();

            tx_buffer[23] = 0x0D;
            tx_buffer[24] = 0x0A;

            if (serialPort1.IsOpen)
            {
                ack = false;
                serialPort1.Write(tx_buffer, 0, 25);
            }


        }
    }
}


