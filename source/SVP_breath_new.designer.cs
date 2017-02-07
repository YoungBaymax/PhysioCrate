namespace plugins_phisio1
{
    partial class SVP_breath_new
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btChannels = new System.Windows.Forms.Button();
            this.topPanel = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.groupwindow = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.btMarks = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.window = new System.Windows.Forms.NumericUpDown();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button2 = new System.Windows.Forms.Button();
            this.bt_freq = new System.Windows.Forms.Button();
            this.bt_signal = new System.Windows.Forms.Button();
            this.bt_marks = new System.Windows.Forms.Button();
            this.btProcess = new System.Windows.Forms.Button();
            this.bgw = new System.ComponentModel.BackgroundWorker();
            this.bgwPre = new System.ComponentModel.BackgroundWorker();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.pbx = new System.Windows.Forms.PictureBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridView3 = new System.Windows.Forms.DataGridView();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.topPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            this.groupwindow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.window)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbx)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btChannels
            // 
            this.btChannels.AllowDrop = true;
            this.btChannels.Location = new System.Drawing.Point(2, 9);
            this.btChannels.Name = "btChannels";
            this.btChannels.Size = new System.Drawing.Size(215, 85);
            this.btChannels.TabIndex = 15;
            this.btChannels.Text = "(Choose channel)";
            this.btChannels.UseVisualStyleBackColor = true;
            this.btChannels.Click += new System.EventHandler(this.btChannels_Click);
            this.btChannels.DragDrop += new System.Windows.Forms.DragEventHandler(this.btChannels_DragDrop);
            this.btChannels.DragEnter += new System.Windows.Forms.DragEventHandler(this.btChannels_DragEnter);
            // 
            // topPanel
            // 
            this.topPanel.BackColor = System.Drawing.SystemColors.Control;
            this.topPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.topPanel.Controls.Add(this.groupBox1);
            this.topPanel.Controls.Add(this.groupBox5);
            this.topPanel.Controls.Add(this.groupwindow);
            this.topPanel.Controls.Add(this.btChannels);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(1192, 108);
            this.topPanel.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.checkBox2);
            this.groupBox1.Location = new System.Drawing.Point(867, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(187, 91);
            this.groupBox1.TabIndex = 34;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "External marks";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(7, 41);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(174, 39);
            this.button3.TabIndex = 1;
            this.button3.Text = "Accept SignalPlant marks changes";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(9, 17);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(116, 17);
            this.checkBox2.TabIndex = 0;
            this.checkBox2.Text = "Use external marks";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.checkBox1);
            this.groupBox5.Controls.Add(this.label3);
            this.groupBox5.Controls.Add(this.label9);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.numericUpDown1);
            this.groupBox5.Controls.Add(this.numericUpDown2);
            this.groupBox5.Location = new System.Drawing.Point(605, 2);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(256, 92);
            this.groupBox5.TabIndex = 30;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Filter setting";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(158, 18);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(15, 14);
            this.checkBox1.TabIndex = 33;
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 29;
            this.label3.Text = "FIR Low-pass";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(18, 63);
            this.label9.MaximumSize = new System.Drawing.Size(120, 26);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(107, 13);
            this.label9.TabIndex = 28;
            this.label9.Text = "Cutoff frequency [Hz]";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 27;
            this.label2.Text = "Filter order";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.DecimalPlaces = 2;
            this.numericUpDown1.Location = new System.Drawing.Point(158, 61);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(55, 20);
            this.numericUpDown1.TabIndex = 25;
            this.numericUpDown1.Tag = "cutoff:NUD_Value";
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(158, 35);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericUpDown2.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(55, 20);
            this.numericUpDown2.TabIndex = 26;
            this.numericUpDown2.Tag = "firorder:NUD_Value";
            this.numericUpDown2.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown2.ValueChanged += new System.EventHandler(this.numericUpDown2_ValueChanged);
            // 
            // groupwindow
            // 
            this.groupwindow.Controls.Add(this.label5);
            this.groupwindow.Controls.Add(this.numericUpDown3);
            this.groupwindow.Controls.Add(this.label4);
            this.groupwindow.Controls.Add(this.btMarks);
            this.groupwindow.Controls.Add(this.button1);
            this.groupwindow.Controls.Add(this.window);
            this.groupwindow.Location = new System.Drawing.Point(224, 2);
            this.groupwindow.Name = "groupwindow";
            this.groupwindow.Size = new System.Drawing.Size(375, 92);
            this.groupwindow.TabIndex = 32;
            this.groupwindow.TabStop = false;
            this.groupwindow.Text = "Processed area";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Start offset [s]";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.Location = new System.Drawing.Point(117, 38);
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(56, 20);
            this.numericUpDown3.TabIndex = 33;
            this.numericUpDown3.ValueChanged += new System.EventHandler(this.numericUpDown3_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 13);
            this.label4.TabIndex = 34;
            this.label4.Text = "Window length [s]";
            // 
            // btMarks
            // 
            this.btMarks.Location = new System.Drawing.Point(204, 12);
            this.btMarks.Name = "btMarks";
            this.btMarks.Size = new System.Drawing.Size(154, 74);
            this.btMarks.TabIndex = 31;
            this.btMarks.Text = "(Choose artifact marks)";
            this.btMarks.UseVisualStyleBackColor = true;
            this.btMarks.Click += new System.EventHandler(this.btMarks_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 61);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(167, 25);
            this.button1.TabIndex = 33;
            this.button1.Text = "(Choose start mark)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // window
            // 
            this.window.Location = new System.Drawing.Point(117, 12);
            this.window.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.window.Name = "window";
            this.window.Size = new System.Drawing.Size(56, 20);
            this.window.TabIndex = 0;
            this.window.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.window.ValueChanged += new System.EventHandler(this.window_ValueChanged);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(1184, 499);
            this.dataGridView1.TabIndex = 33;
            // 
            // bottomPanel
            // 
            this.bottomPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bottomPanel.Controls.Add(this.progressBar1);
            this.bottomPanel.Controls.Add(this.button2);
            this.bottomPanel.Controls.Add(this.bt_freq);
            this.bottomPanel.Controls.Add(this.bt_signal);
            this.bottomPanel.Controls.Add(this.bt_marks);
            this.bottomPanel.Controls.Add(this.btProcess);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 633);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(1192, 37);
            this.bottomPanel.TabIndex = 3;
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar1.Location = new System.Drawing.Point(160, 0);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(631, 35);
            this.progressBar1.TabIndex = 21;
            // 
            // button2
            // 
            this.button2.Dock = System.Windows.Forms.DockStyle.Right;
            this.button2.Location = new System.Drawing.Point(791, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(85, 35);
            this.button2.TabIndex = 5;
            this.button2.Text = "Export image to clipboard";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // bt_freq
            // 
            this.bt_freq.Dock = System.Windows.Forms.DockStyle.Right;
            this.bt_freq.Location = new System.Drawing.Point(876, 0);
            this.bt_freq.Name = "bt_freq";
            this.bt_freq.Size = new System.Drawing.Size(121, 35);
            this.bt_freq.TabIndex = 23;
            this.bt_freq.Text = "Export respiration rate variability";
            this.bt_freq.UseVisualStyleBackColor = true;
            this.bt_freq.Click += new System.EventHandler(this.bt_freq_Click);
            // 
            // bt_signal
            // 
            this.bt_signal.Dock = System.Windows.Forms.DockStyle.Right;
            this.bt_signal.Location = new System.Drawing.Point(997, 0);
            this.bt_signal.Name = "bt_signal";
            this.bt_signal.Size = new System.Drawing.Size(92, 35);
            this.bt_signal.TabIndex = 0;
            this.bt_signal.Text = "Export filtered signal";
            this.bt_signal.Click += new System.EventHandler(this.bt_signal_Click);
            // 
            // bt_marks
            // 
            this.bt_marks.Dock = System.Windows.Forms.DockStyle.Right;
            this.bt_marks.Location = new System.Drawing.Point(1089, 0);
            this.bt_marks.Name = "bt_marks";
            this.bt_marks.Size = new System.Drawing.Size(101, 35);
            this.bt_marks.TabIndex = 22;
            this.bt_marks.Text = "Export marks";
            this.bt_marks.UseVisualStyleBackColor = true;
            this.bt_marks.Click += new System.EventHandler(this.bt_marks_Click);
            // 
            // btProcess
            // 
            this.btProcess.Dock = System.Windows.Forms.DockStyle.Left;
            this.btProcess.Location = new System.Drawing.Point(0, 0);
            this.btProcess.Name = "btProcess";
            this.btProcess.Size = new System.Drawing.Size(160, 35);
            this.btProcess.TabIndex = 20;
            this.btProcess.Text = "Compute";
            this.btProcess.UseVisualStyleBackColor = true;
            this.btProcess.Visible = false;
            this.btProcess.Click += new System.EventHandler(this.btProcess_Click);
            // 
            // bgw
            // 
            this.bgw.WorkerReportsProgress = true;
            this.bgw.WorkerSupportsCancellation = true;
            this.bgw.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgw_DoWork);
            this.bgw.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgw_ProgressChanged);
            this.bgw.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgw_RunWorkerCompleted);
            // 
            // bgwPre
            // 
            this.bgwPre.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgwPre_DoWork);
            this.bgwPre.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgwPre_ProgressChanged);
            this.bgwPre.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgwPre_RunWorkerCompleted);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(121, 375);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "label1";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 108);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1192, 525);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.pbx);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1184, 499);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Picture";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // pbx
            // 
            this.pbx.BackColor = System.Drawing.Color.White;
            this.pbx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbx.Location = new System.Drawing.Point(3, 3);
            this.pbx.Name = "pbx";
            this.pbx.Size = new System.Drawing.Size(1178, 493);
            this.pbx.TabIndex = 6;
            this.pbx.TabStop = false;
            this.pbx.Click += new System.EventHandler(this.pbx_Click);
            this.pbx.Paint += new System.Windows.Forms.PaintEventHandler(this.pbx_Paint);
            this.pbx.Resize += new System.EventHandler(this.pbx_Resize);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridView3);
            this.tabPage2.Controls.Add(this.dataGridView2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1184, 499);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Tables";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridView3
            // 
            this.dataGridView3.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView3.Dock = System.Windows.Forms.DockStyle.Right;
            this.dataGridView3.Location = new System.Drawing.Point(595, 3);
            this.dataGridView3.Name = "dataGridView3";
            this.dataGridView3.Size = new System.Drawing.Size(586, 493);
            this.dataGridView3.TabIndex = 1;
            // 
            // dataGridView2
            // 
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Dock = System.Windows.Forms.DockStyle.Left;
            this.dataGridView2.Location = new System.Drawing.Point(3, 3);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.Size = new System.Drawing.Size(586, 493);
            this.dataGridView2.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.dataGridView1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1184, 499);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Statistics";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // SVP_breath_new
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.topPanel);
            this.Name = "SVP_breath_new";
            this.Size = new System.Drawing.Size(1192, 670);
            this.Load += new System.EventHandler(this.SVP_plugin_v3_Load);
            this.topPanel.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            this.groupwindow.ResumeLayout(false);
            this.groupwindow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.window)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbx)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btChannels;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Button btProcess;
        private System.ComponentModel.BackgroundWorker bgw;
        private System.ComponentModel.BackgroundWorker bgwPre;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button bt_signal;
        private System.Windows.Forms.Button bt_marks;
        private System.Windows.Forms.Button bt_freq;
        private System.Windows.Forms.Button btMarks;
        private System.Windows.Forms.GroupBox groupwindow;
        private System.Windows.Forms.NumericUpDown window;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.PictureBox pbx;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dataGridView3;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Button button3;
    }
}
