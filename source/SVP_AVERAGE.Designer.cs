namespace plugins_package_1
{
    partial class SVP_avgTool
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
            this.button1 = new System.Windows.Forms.Button();
            this.lr = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.nud = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.bgw = new System.ComponentModel.BackgroundWorker();
            this.pbar = new System.Windows.Forms.ProgressBar();
            this.button3 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.nud1 = new System.Windows.Forms.NumericUpDown();
            this.pbx = new System.Windows.Forms.PictureBox();
            this.bgwPre = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.nud)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbx)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.AllowDrop = true;
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(313, 46);
            this.button1.TabIndex = 13;
            this.button1.Text = "(Select a channel)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            this.button1.DragDrop += new System.Windows.Forms.DragEventHandler(this.panel1_DragDrop);
            this.button1.DragEnter += new System.Windows.Forms.DragEventHandler(this.panel1_DragEnter);
            // 
            // lr
            // 
            this.lr.AutoSize = true;
            this.lr.Location = new System.Drawing.Point(334, 5);
            this.lr.Name = "lr";
            this.lr.Size = new System.Drawing.Size(86, 13);
            this.lr.TabIndex = 17;
            this.lr.Text = "Averaging radius";
            // 
            // button2
            // 
            this.button2.AllowDrop = true;
            this.button2.Dock = System.Windows.Forms.DockStyle.Left;
            this.button2.Location = new System.Drawing.Point(0, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(232, 41);
            this.button2.TabIndex = 18;
            this.button2.Text = "Process";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // nud
            // 
            this.nud.Location = new System.Drawing.Point(327, 21);
            this.nud.Maximum = 100;
            this.nud.Minimum = 4;
            this.nud.Name = "nud";
            this.nud.Size = new System.Drawing.Size(267, 45);
            this.nud.TabIndex = 26;
            this.nud.Tag = "AvgRadius:Value";
            this.nud.Value = 4;
            this.nud.Scroll += new System.EventHandler(this.nud_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(795, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 28;
            this.label1.Text = "Windowing function";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // bgw
            // 
            this.bgw.WorkerReportsProgress = true;
            this.bgw.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgw_DoWork);
            this.bgw.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgw_ProgressChanged);
            this.bgw.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgw_RunWorkerCompleted);
            // 
            // pbar
            // 
            this.pbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbar.Location = new System.Drawing.Point(232, 0);
            this.pbar.Name = "pbar";
            this.pbar.Size = new System.Drawing.Size(722, 41);
            this.pbar.TabIndex = 29;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(743, 22);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(203, 30);
            this.button3.TabIndex = 30;
            this.button3.Tag = "Wintype:Text";
            this.button3.Text = "(Select a type)";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pbar);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 541);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(954, 41);
            this.panel1.TabIndex = 31;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.nud1);
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.lr);
            this.panel2.Controls.Add(this.button3);
            this.panel2.Controls.Add(this.nud);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(954, 57);
            this.panel2.TabIndex = 32;
            // 
            // nud1
            // 
            this.nud1.Location = new System.Drawing.Point(609, 18);
            this.nud1.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud1.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.nud1.Name = "nud1";
            this.nud1.Size = new System.Drawing.Size(67, 20);
            this.nud1.TabIndex = 31;
            this.nud1.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.nud1.ValueChanged += new System.EventHandler(this.nud1_ValueChanged);
            this.nud1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.nud1_KeyPress);
            this.nud1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.nud1_KeyUp);
            // 
            // pbx
            // 
            this.pbx.BackColor = System.Drawing.Color.White;
            this.pbx.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbx.Location = new System.Drawing.Point(0, 57);
            this.pbx.Name = "pbx";
            this.pbx.Size = new System.Drawing.Size(954, 484);
            this.pbx.TabIndex = 33;
            this.pbx.TabStop = false;
            this.pbx.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pbx.Resize += new System.EventHandler(this.pbx_Resize);
            // 
            // bgwPre
            // 
            this.bgwPre.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgwPre_DoWork);
            this.bgwPre.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgwPre_RunWorkerCompleted);
            // 
            // SVP_avgTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbx);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Name = "SVP_avgTool";
            this.Size = new System.Drawing.Size(954, 582);
            this.Load += new System.EventHandler(this.SVP_avgTool_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.avgTool_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.nud)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbx)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lr;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TrackBar nud;
        private System.Windows.Forms.Label label1;
        private System.ComponentModel.BackgroundWorker bgw;
        private System.Windows.Forms.ProgressBar pbar;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pbx;
        private System.ComponentModel.BackgroundWorker bgwPre;
        private System.Windows.Forms.NumericUpDown nud1;
    }
}
