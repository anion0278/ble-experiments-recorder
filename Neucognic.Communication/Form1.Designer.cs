
namespace myodam_test
{
    partial class Form1
    {
        /// <summary>
        /// Vyžaduje se proměnná návrháře.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Uvolněte všechny používané prostředky.
        /// </summary>
        /// <param name="disposing">hodnota true, když by se měl spravovaný prostředek odstranit; jinak false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kód generovaný Návrhářem Windows Form

        /// <summary>
        /// Metoda vyžadovaná pro podporu Návrháře - neupravovat
        /// obsah této metody v editoru kódu.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textPortName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.btnNeuGait_PowerOff = new System.Windows.Forms.Button();
            this.btnNeuGait_ReatBatteryFuelLevel = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.btn_NeuGaitStartStimulation = new System.Windows.Forms.Button();
            this.btn_NeugaitStopStimulation = new System.Windows.Forms.Button();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.btn_NeuGaitSetMode = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(180, 28);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(79, 50);
            this.button1.TabIndex = 0;
            this.button1.Text = "Connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(107, 58);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(57, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "003001";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Stimulator SN:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // textPortName
            // 
            this.textPortName.Location = new System.Drawing.Point(107, 28);
            this.textPortName.Name = "textPortName";
            this.textPortName.Size = new System.Drawing.Size(57, 20);
            this.textPortName.TabIndex = 3;
            this.textPortName.Text = "COM3";
            this.textPortName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Serial port:";
            // 
            // serialPort1
            // 
            this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
            // 
            // btnNeuGait_PowerOff
            // 
            this.btnNeuGait_PowerOff.Location = new System.Drawing.Point(30, 145);
            this.btnNeuGait_PowerOff.Name = "btnNeuGait_PowerOff";
            this.btnNeuGait_PowerOff.Size = new System.Drawing.Size(75, 23);
            this.btnNeuGait_PowerOff.TabIndex = 5;
            this.btnNeuGait_PowerOff.Text = "Power Off";
            this.btnNeuGait_PowerOff.UseVisualStyleBackColor = true;
            this.btnNeuGait_PowerOff.Click += new System.EventHandler(this.btnNeuGait_PowerOff_Click);
            // 
            // btnNeuGait_ReatBatteryFuelLevel
            // 
            this.btnNeuGait_ReatBatteryFuelLevel.Location = new System.Drawing.Point(30, 116);
            this.btnNeuGait_ReatBatteryFuelLevel.Name = "btnNeuGait_ReatBatteryFuelLevel";
            this.btnNeuGait_ReatBatteryFuelLevel.Size = new System.Drawing.Size(134, 23);
            this.btnNeuGait_ReatBatteryFuelLevel.TabIndex = 6;
            this.btnNeuGait_ReatBatteryFuelLevel.Text = "Read Battery Fuel Level";
            this.btnNeuGait_ReatBatteryFuelLevel.UseVisualStyleBackColor = true;
            this.btnNeuGait_ReatBatteryFuelLevel.Click += new System.EventHandler(this.btnNeuGait_ReatBatteryFuelLevel_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(12, 449);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(592, 173);
            this.textBox2.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(194, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Current [mA]";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(194, 145);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Pulse width:";
            // 
            // comboBox1
            // 
            this.comboBox1.DisplayMember = "0";
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "50us",
            "100us",
            "200us",
            "300us",
            "400us"});
            this.comboBox1.Location = new System.Drawing.Point(311, 142);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(72, 21);
            this.comboBox1.TabIndex = 10;
            this.comboBox1.Text = "50us";
            this.comboBox1.ValueMember = "0";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown1.Location = new System.Drawing.Point(311, 116);
            this.numericUpDown1.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(72, 20);
            this.numericUpDown1.TabIndex = 11;
            this.numericUpDown1.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(194, 171);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Stim Waveform:";
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
            "SYM",
            "ASYM"});
            this.comboBox2.Location = new System.Drawing.Point(311, 168);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(72, 21);
            this.comboBox2.TabIndex = 13;
            this.comboBox2.Text = "SYM";
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown2.Location = new System.Drawing.Point(311, 195);
            this.numericUpDown2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(72, 20);
            this.numericUpDown2.TabIndex = 15;
            this.numericUpDown2.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(194, 197);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(102, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Stim Frequency [Hz]";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(197, 222);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(186, 23);
            this.button2.TabIndex = 16;
            this.button2.Text = "Set stimulation parameters";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btn_NeuGaitStartStimulation
            // 
            this.btn_NeuGaitStartStimulation.BackColor = System.Drawing.Color.Aquamarine;
            this.btn_NeuGaitStartStimulation.Location = new System.Drawing.Point(197, 367);
            this.btn_NeuGaitStartStimulation.Name = "btn_NeuGaitStartStimulation";
            this.btn_NeuGaitStartStimulation.Size = new System.Drawing.Size(134, 23);
            this.btn_NeuGaitStartStimulation.TabIndex = 17;
            this.btn_NeuGaitStartStimulation.Text = "Start Stimulation";
            this.btn_NeuGaitStartStimulation.UseVisualStyleBackColor = false;
            this.btn_NeuGaitStartStimulation.Click += new System.EventHandler(this.button3_Click);
            // 
            // btn_NeugaitStopStimulation
            // 
            this.btn_NeugaitStopStimulation.BackColor = System.Drawing.Color.LightCoral;
            this.btn_NeugaitStopStimulation.Location = new System.Drawing.Point(197, 396);
            this.btn_NeugaitStopStimulation.Name = "btn_NeugaitStopStimulation";
            this.btn_NeugaitStopStimulation.Size = new System.Drawing.Size(134, 23);
            this.btn_NeugaitStopStimulation.TabIndex = 18;
            this.btn_NeugaitStopStimulation.Text = "Stop Stimulation";
            this.btn_NeugaitStopStimulation.UseVisualStyleBackColor = false;
            this.btn_NeugaitStopStimulation.Click += new System.EventHandler(this.btn_NeugaitStopStimulation_Click);
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.Location = new System.Drawing.Point(111, 367);
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(57, 20);
            this.numericUpDown3.TabIndex = 19;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(68, 369);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 13);
            this.label7.TabIndex = 20;
            this.label7.Text = "Delay:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(68, 327);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "Mode:";
            // 
            // comboBox3
            // 
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Items.AddRange(new object[] {
            "STANDBY",
            "WALK",
            "EXEC"});
            this.comboBox3.Location = new System.Drawing.Point(111, 324);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(74, 21);
            this.comboBox3.TabIndex = 22;
            this.comboBox3.Text = "WALK";
            // 
            // btn_NeuGaitSetMode
            // 
            this.btn_NeuGaitSetMode.Location = new System.Drawing.Point(197, 322);
            this.btn_NeuGaitSetMode.Name = "btn_NeuGaitSetMode";
            this.btn_NeuGaitSetMode.Size = new System.Drawing.Size(75, 23);
            this.btn_NeuGaitSetMode.TabIndex = 23;
            this.btn_NeuGaitSetMode.Text = "Set mode";
            this.btn_NeuGaitSetMode.UseVisualStyleBackColor = true;
            this.btn_NeuGaitSetMode.Click += new System.EventHandler(this.btn_NeuGaitSetMode_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(197, 286);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 24;
            this.button3.Text = "Set Audio Tone";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // numericUpDown4
            // 
            this.numericUpDown4.Location = new System.Drawing.Point(111, 289);
            this.numericUpDown4.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown4.Name = "numericUpDown4";
            this.numericUpDown4.Size = new System.Drawing.Size(53, 20);
            this.numericUpDown4.TabIndex = 25;
            this.numericUpDown4.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(623, 642);
            this.Controls.Add(this.numericUpDown4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.btn_NeuGaitSetMode);
            this.Controls.Add(this.comboBox3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.numericUpDown3);
            this.Controls.Add(this.btn_NeugaitStopStimulation);
            this.Controls.Add(this.btn_NeuGaitStartStimulation);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.btnNeuGait_ReatBatteryFuelLevel);
            this.Controls.Add(this.btnNeuGait_PowerOff);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textPortName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Neugait Test";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textPortName;
        private System.Windows.Forms.Label label2;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.Button btnNeuGait_PowerOff;
        private System.Windows.Forms.Button btnNeuGait_ReatBatteryFuelLevel;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btn_NeuGaitStartStimulation;
        private System.Windows.Forms.Button btn_NeugaitStopStimulation;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Button btn_NeuGaitSetMode;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.NumericUpDown numericUpDown4;
    }
}

