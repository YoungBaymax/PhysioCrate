using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using signalViewer;
using System.Threading;

namespace plugins_phisio1
{
    public partial class SVP_breath : UserControl
    {
        List<signalViewer.graphPanel> linkedChannels = new List<signalViewer.graphPanel>(); //--array of linked channels

        string marksFilter = ""; //--filter, deffining marks selection
        List<selectionMember> marksList;
        List<selectionMember> marksList_out = new List<selectionMember>();
        selectionMember start_mark;

        signalViewer.graphPanel lkk;

        filter_FIR filter = new filter_FIR();
        //Object result = null; //---only some result for ilustrative purpose....

        bool cmd_call = false;
        //float[] source = null;
        //float[] results = null;

        float trackbar2_val = 0.5f;
        int trackbar1_val = 500;
        bool processallowed = false;
        Signal Breath = new Signal(mainView.sampleFrequency);
        Signal Breath_all = new Signal(mainView.sampleFrequency);

        int windowsize = 30;

        public System.Timers.Timer timer = new System.Timers.Timer();

        int NumSamples = 0;

        public string helpFile = "SignalPlant.pdf"; //--name of help file
        public string helpNamedDest = "SignalPlant.indd:Downsample:303"; //---named destination in the help file

        int refrcounter = 0;

        Pen sorPen = new Pen(Color.Gray, 1);
        Pen detPen = new Pen(Color.Red, 1);
        Pen filPen = new Pen(Color.Black, 1);
        Pen markPen = new Pen(Color.Blue, 4);
        SolidBrush artArea = new SolidBrush(Color.FromArgb(25, Color.Gray));//Color.FromArgb(50, Color.Red)

        bool presetLoading = false;
        bool exportsignal = true;
        bool exportmarks = true;
        bool exportfreq = true;

        ManualResetEvent COMRE = new ManualResetEvent(false); //---manual reset event. Important when this plugin generates a COMMAND too. Otherwise this can be deleted.

        float bgwPercentDone = 0;

        mainView mv; //-- instance of signal viewer program

        public string getDeveloperInfo()
        {
            return "Petr Nejedlý, 2015"; //----change to your name
        }

        public string getDescription()
        {
            return "Breath signal analysis"; //----enter description for your plugin
        }

        public string getCategory()
        {
            return "Analysis"; //---set category in plugins menu. If it does not exists, new category will be created
        }


        public string getName()
        {
            return "Breath variability";        //---plugin name, visible in Plugins menu
        }

        public void doExternalRefresh()//------
        {

            //if (!bgwPre.IsBusy)
            //{
            //    bgwPre.RunWorkerAsync();
            //}
            //refrControls();

            doPreviewWork();

        }


        public void presetBeforeLoaded()
        {
            presetLoading = true;
            /*
          This funciotn is called after user clicks on preset in menu, but before corresponding controls are filled with new values. 
           */
        }

        public void presetLoaded()
        {
            /*
             * This funciotn is called after user clicks on preset in menu. Corresponding controls (with filled TAG field) receives values from preset
             * and here you have to set values from controls to corresponding fields
            */
            presetLoading = false;

            refrControls();
            doPreviewWork();


        }





        private void refrControls()
        {
            /*
             * This function is called for refreshing a plugin form. 
             
             * */

            btChannels.Text = "";
            if (linkedChannels.Count == 0) btChannels.Text = "Drag a channel here or click";
            if (linkedChannels.Count == 1) btChannels.Text = "Linked channel: " + linkedChannels[0].channelName;
            if (linkedChannels.Count > 1) btChannels.Text = "Linked channels count: " + linkedChannels.Count.ToString();

            btProcess.Enabled = (linkedChannels.Count > 0 && processallowed);
            bt_marks.Enabled = (linkedChannels.Count > 0 && !processallowed && exportmarks);
            bt_signal.Enabled = (linkedChannels.Count > 0 && !processallowed && exportsignal);
            bt_freq.Enabled = (linkedChannels.Count > 0 && !processallowed && exportfreq);


            if (bgw.IsBusy && bgw.CancellationPending) btProcess.Text = "Wait for cancellation";
            if (bgw.IsBusy && !bgw.CancellationPending) btProcess.Text = "Cancel";
            if (!bgw.IsBusy) btProcess.Text = "Process";

            if (marksList != null && marksList.Count > 0) btMarks.Text = "Artifact marks selected: " + marksList.Count;
            else btMarks.Text = "(Choose some artifact marks)";

            if (start_mark != null)
            {
                button1.Text = "Start mark: " + start_mark.info;
                numericUpDown3.Enabled = true;
            }
            else
            {
                button1.Text = "(Choose some start mark)";
                numericUpDown3.Enabled = false;
            }

            numericUpDown1.Value = (decimal)trackbar2_val;
            numericUpDown2.Value = (decimal)trackbar1_val;
            trackBar2.Value = (int)(trackbar2_val * 10);
            trackBar1.Value = trackbar1_val;
            pbx.Refresh();

            


        }

        private void doPreviewWork()
        {
            /*This function creates anything for preview. Not neccesary to implement
             * */



            if (!bgwPre.IsBusy)
            {
                bgwPre.RunWorkerAsync();
                //pbx.Refresh();
            }


        }

        private void btChannels_DragDrop(object sender, DragEventArgs e)
        {
            /*
             *This method add dragged channel to linkedchannels
             */
            string s = e.Data.GetData(DataFormats.Text).ToString();

            try
            {
                linkedChannels.Add(mv.getGPbyName(s));
                refrControls();
                doPreviewWork();

            }
            catch (Exception exp)
            {
                mainView.log(exp, "Error while drag&drop", this); // this line will log error into "errorlog.txt"
                MessageBox.Show("Error:" + exp.Message);
            }
        }

        private void btChannels_DragEnter(object sender, DragEventArgs e)
        {
            string s = e.Data.GetData(DataFormats.Text).ToString();

            if (mv.getGPbyName(s) != null)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void btChannels_Click(object sender, EventArgs e)
        {
            signalViewer.selectChannelForm sc = new signalViewer.selectChannelForm();
            sc.regenerateList(linkedChannels);

            if (sc.ShowDialog() == DialogResult.OK)
            {
                linkedChannels.Clear();
                for (int i = 0; i < sc.lv.SelectedItems.Count; i++)
                {
                    linkedChannels.Add(mv.getGPbyName(sc.lv.SelectedItems[i].Text));
                }
                refrControls();
                doPreviewWork();
            }
            trackbar1_val = (int)(mainView.sampleFrequency * 2);
        }


        private void btProcess_Click(object sender, EventArgs e)
        {
            if (!bgw.IsBusy)
            {
                bgw.RunWorkerAsync();
            }
            else
            {
                bgw.CancelAsync();
            }

            refrControls();
        }


       
        public SVP_breath()
        {
            InitializeComponent();

           


            timer.Interval = 100;
            timer.Elapsed += timer_Elapsed;

            dataGridView1.ColumnCount = 4;
            dataGridView1.Columns[0].Name = "Quantity";
            dataGridView1.Columns[1].Name = "Unit";
            dataGridView1.Columns[2].Name = "Average";
            dataGridView1.Columns[3].Name = "Standard deviation";
            dataGridView1.RowCount = 3;
            //
            dataGridView2.ColumnCount = 6;
            dataGridView2.Columns[0].Name = "Interval";
            dataGridView2.Columns[1].Name = "Interval start";
            dataGridView2.Columns[2].Name = "Interval end";
            dataGridView2.Columns[3].Name = "Frequency";
            dataGridView2.Columns[4].Name = "Amplitude";
            dataGridView2.Columns[5].Name = "Ventilation";

            
            dataGridView1.ReadOnly = true;
            dataGridView2.ReadOnly = true;

            tabPage2.Parent = null;
            tabPage3.Parent = null;

            refrControls();
        }


        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            double progress = filter_FIR.samplesProcessed / (double)NumSamples * 100;

            try
            {

                if (progress >= 100)
                {
                    progress = 100;
                }
                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Value = (int)progress;
                    bgw.ReportProgress((int)progress);
                });
            }
            catch (Exception)
            { }

        }


        private void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            bgwPercentDone = e.ProgressPercentage;
            progressBar1.Value = (int)bgwPercentDone;
            pbx.Refresh();

        }

        private void pbx_Paint(object sender, PaintEventArgs e)
        {
            if (linkedChannels == null || linkedChannels.Count == 0)
            {
                e.Graphics.DrawString("Please attach any channels first", SystemFonts.DefaultFont, Brushes.Black, pbx.Width / 2, pbx.Height / 2, mainView.sfc);
                return;
            }

            //INFO pod obrázkem
            if (linkedChannels.Count == 1)
            {
                int N = graphPanel.rightI - graphPanel.leftI;
                float viewLsec = graphPanel.leftI / Breath_all.Fs;
                float viewPsec = graphPanel.rightI / Breath_all.Fs;


                string Text = "File: " + mainView.currentFile + "   Channel:" + linkedChannels[0].channelName + "    Time:" + viewLsec.ToString() + " s - " + viewPsec.ToString() + " s    Filter: FIR order:" + trackbar1_val.ToString() + " Fc: " + trackbar2_val.ToString() + " Hz";
                e.Graphics.DrawString(Text, SystemFonts.DefaultFont, Brushes.Black, 0, pbx.Height - 15);

            }

            if (bgw.IsBusy)
            {
                e.Graphics.DrawString("Working " + bgwPercentDone.ToString("00.0") + "%", SystemFonts.DefaultFont, Brushes.Black, pbx.Width / 2, pbx.Height / 2, mainView.sfc);
                progressBar1.Value = (int)bgwPercentDone;
                return;
            }

            if (Breath.Original == null || Breath.Filtered == null)
            {
                e.Graphics.DrawString("No result was achieved", SystemFonts.DefaultFont, Brushes.Black, pbx.Width / 2, pbx.Height / 2, mainView.sfc);
                return;
            }
            else
            {
                draw(Breath.Original.ToArray(), e, sorPen);
                try
                {
                    draw(Breath.Filtered.ToArray(), e, filPen, Breath.Original.ToArray());
                    draw_detections(Breath.Filtered.ToArray(), Breath.max_valid.ToArray(), e, detPen, true, Breath.Original.ToArray());
                    draw_detections(Breath.Filtered.ToArray(), Breath.min_valid.ToArray(), e, detPen, false, Breath.Original.ToArray());


                    if (Breath.PreviewMarksValid != null) draw_marks_scale(Breath.Filtered.ToArray(), Breath.PreviewMarksValid, e, markPen, Breath.Original.ToArray());

                    //if (start_mark != null && mark_valid(start_mark, graphPanel.leftI, graphPanel.rightI))
                    if (start_mark != null)
                    {
                        List <string>text= new List<string>();
                        List<int> start_mark_in_List = new List<int>();
                        if (mark_valid(start_mark, graphPanel.leftI, graphPanel.rightI))
                        {
                            text.Add("Start");
                            start_mark_in_List.Add(Breath.start_mark_corr.leftIndex);
                        }

                        if (Breath.start_mark_corr.leftIndex + (int)Breath.Fs * windowsize< graphPanel.rightI)
                        {
                            text.Add("Stop");
                            start_mark_in_List.Add(Breath.start_mark_corr.leftIndex + (int)Breath.Fs * windowsize);
                        }

                        if (start_mark_in_List.Count == 0) return;
                        draw_detections(Breath.Filtered.ToArray(), start_mark_in_List.ToArray(), e, markPen, true, Breath.Original.ToArray(), text);
                        

                    }
            }
                catch (Exception exp)
            {
                //e.Graphics.DrawString(exp.Message, SystemFonts.DefaultFont, Brushes.Black, pbx.Width / 2, pbx.Height / 2, mainView.sfc);
            }
            return;

            }
        }

        public bool mark_valid(selectionMember A, int Left, int Right)
        {
            if (A.rightIndex > Left && A.leftIndex < Right)
            {
                return true;
            }
            return false;

        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            //bgw.ReportProgress(10);

           
            if (linkedChannels.Count <= 0)
            {
                return;
            }

            lkk = linkedChannels[0];



            //Breath_all.Original = new List<float>(lkk.dataCache[lkk.currentDataChace].data);
            Breath_all.Original = new List<float>(lkk.dataCache[0].data);
            NumSamples = Breath_all.Original.Count;

            timer.Start();
            filter_FIR.samplesProcessed = 0;
            Breath_all.Filtered = new List<float>(computeFIR_LP(Breath_all.Original.ToArray(), trackbar1_val, trackbar2_val, mainView.sampleFrequency).ToList<float>());
            Breath_all.max_detect((float)delay.Value);
            Breath_all.min_detect(Breath_all.Max_position[0], (float)delay.Value);
            Breath_all.getmarks(marksList);
            Breath_all.frequency(Breath_all.Max_position);
            //bgw.ReportProgress(50);


            Breath_all.amplitude();
            Breath_all.frequency_stat(windowsize, start_mark, linkedChannels);

            // Breath_all.amplitude_stat();
            //Breath_all.ventilation();
            //Breath_all.ventilation_stat();
            Breath_all.freqfunction();
            //bgw.ReportProgress(100);
            timer.Stop();

            COMRE.Set();

        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            tabPage2.Parent = tabControl1;
            tabPage3.Parent = tabControl1;

            processallowed = false;
            refrControls();
            progressBar1.Value = 0;

            bt_marks.Enabled = true;
            bt_signal.Enabled = true;
            bt_freq.Enabled = true;
            exportsignal = true;
            exportmarks = true;
            exportfreq = true;

            //label4.Text = Breath_all.Frequency_stat.avg.ToString("00.0000")+ " ± " + Breath_all.Frequency_stat.dev.ToString("0.0000");
            //label6.Text = Breath_all.Amplitude_stat.avg.ToString("0.0000") + " ± " + Breath_all.Amplitude_stat.dev.ToString("0.0000");
            //label8.Text = Breath_all.Ventilation_stat.avg.ToString("0.0000") + " ± " + Breath_all.Ventilation_stat.dev.ToString("0.0000");

            globalVariables.setVar("@Breath_frequency_avg", Breath_all.Frequency_stat.avg);
            globalVariables.setVar("@Breath_frequency_sdev", Breath_all.Frequency_stat.dev);
            globalVariables.setVar("@Breath_amplitude_avg", Breath_all.Amplitude_stat.avg);
            globalVariables.setVar("@Breath_amplitude_sdev", Breath_all.Amplitude_stat.dev);
            globalVariables.setVar("@Breath_ventilation_avg", Breath_all.Ventilation_stat.avg);
            globalVariables.setVar("@Breath_ventilation_sdev", Breath_all.Ventilation_stat.dev);

            if (!cmd_call)
            {
                dataGridView1.ReadOnly = false;

                dataGridView1.RowCount = 3;

                dataGridView1.Rows[0].Cells[0].Value = "Frequency";
                dataGridView1.Rows[0].Cells[1].Value = "breaths/min";
                dataGridView1.Rows[0].Cells[2].Value = Breath_all.Frequency_stat.avg.ToString("00.0000");
                dataGridView1.Rows[0].Cells[3].Value = Breath_all.Frequency_stat.dev.ToString("0.0000");


                dataGridView1.Rows[1].Cells[0].Value = "Amplitude";
                dataGridView1.Rows[1].Cells[1].Value = " ";
                dataGridView1.Rows[1].Cells[2].Value = Breath_all.Amplitude_stat.avg.ToString("00.0000");
                dataGridView1.Rows[1].Cells[3].Value = Breath_all.Amplitude_stat.dev.ToString("0.0000");

                dataGridView1.Rows[2].Cells[0].Value = "Ventilation";
                dataGridView1.Rows[2].Cells[1].Value = " ";
                dataGridView1.Rows[2].Cells[2].Value = Breath_all.Ventilation_stat.avg.ToString("00.0000");
                dataGridView1.Rows[2].Cells[3].Value = Breath_all.Ventilation_stat.dev.ToString("0.0000");

                dataGridView1.ReadOnly = true;
                //Clipboard.SetText(Breath_all.to_clip);
                dataGridView2.ReadOnly = false;
                //dataGridView2.AutoGenerateColumns = true;
                //dataGridView2.DataSource = Breath_all.Amplitude;

                dataGridView2.RowCount = Breath_all.Frequency_valid.Count;
                float start, stop;
                for (int i = 0; i < Breath_all.Frequency_valid.Count; i++)
                {
                    dataGridView2.Rows[i].Cells[0].Value = i.ToString();
                    start = Breath_all.Frequency_pos_valid[i].X / Breath_all.Fs;
                    stop = Breath_all.Frequency_pos_valid[i].Y / Breath_all.Fs;
                    dataGridView2.Rows[i].Cells[1].Value = start.ToString();
                    dataGridView2.Rows[i].Cells[2].Value = stop.ToString();
                    dataGridView2.Rows[i].Cells[3].Value = Breath_all.Frequency_valid[i].ToString();
                    dataGridView2.Rows[i].Cells[4].Value = Breath_all.Amplitude_valid[i].ToString();
                    dataGridView2.Rows[i].Cells[5].Value = Breath_all.Ventilation_valid[i].ToString();

                }

                //dataGridView2.Refresh();
                dataGridView2.ReadOnly = true;

                tabControl1.SelectTab(2);
                
            }
        }

        public void ResetResultLable()
        {

            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
        }

        private void btMarks_Click(object sender, EventArgs e)
        {
            signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();
            sc.setFilter("");

            if (sc.ShowDialog() == DialogResult.OK)
            {
                marksList = sc.result;
                marksFilter = sc.filter;
                //refrControls();
                processallowed = true;

                ResetResultLable();
                
            }
            doExternalRefresh();
        }

        private void SVP_plugin_v3_Load(object sender, EventArgs e)
        {
            if (this.ParentForm != null)
                mv = (mainView)Application.OpenForms[0]; //--this links mv property to signalplant application

            if (mv.gpList == null)
            {
                MessageBox.Show("No data in SignalPlant. Please load data first.");
                this.ParentForm.Close();
            }
            if (mv.gpList.Count == 0)
            {
                MessageBox.Show("No data in SignalPlant. Please load data first.");
                this.ParentForm.Close();
            }

        }

        private void pbx_Click(object sender, EventArgs e)
        {

        }

        private void bgwPre_DoWork(object sender, DoWorkEventArgs e)
        {
            
            if (linkedChannels.Count <= 0)
            {
                btProcess.Enabled = false;
                return;
            }

            int viewL = graphPanel.leftI;
            int viewP = graphPanel.rightI;
            int N = viewP - viewL;




            signalViewer.graphPanel lk = linkedChannels[0];

            //Breath.Original = new List<float>(lk.dataCache[lk.currentDataChace].data.GetRange(viewL,N));
            Breath.Original = new List<float>(lk.dataCache[0].data.GetRange(viewL, N));

            Breath.Filtered = new List<float>(computeFIR_LP(Breath.Original.ToArray(), trackbar1_val, trackbar2_val, mainView.sampleFrequency).ToList<float>());


            Breath.getmarks(marksList);

            Breath.start_mark_corr_set(start_mark);
            if (Breath.start_mark_corr != null)
            {
                Breath.start_mark_corr.leftIndex -= viewL;
                Breath.start_mark_corr.rightIndex -= viewL;
            }

            Breath.previewMarks(viewL, viewP, start_mark, windowsize, viewL);
            Breath.max_detect((float)delay.Value);
            Breath.pre_max_valid(windowsize, Breath.start_mark_corr, linkedChannels, viewL);
            if (Breath.max_valid.Count != 0)
            {
                Breath.min_detect(Breath.max_valid[0], (float)delay.Value);

                Breath.pre_min_valid(windowsize, Breath.start_mark_corr, linkedChannels, viewL);

            }
            else if (Breath.min_valid != null) Breath.min_valid.Clear();
        }

        private void bgwPre_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void bgwPre_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            refrControls();


        }

        WindowingFunction wf = new WindowingFunction(40, WindowingFunction.windowsType.Hamming, 0); //--windowing function for filter

        public float[] computeFIR_LP(float[] input, int order, float cutOffFrequeny, float sampleFreq)

        {

            float[] result = new float[input.Length];

            int maximumOrder = 5000;

            if (order < 0 || order > maximumOrder) return null;



            //filter_FIR filter = new filter_FIR();


            filter = filter.generateLowPass(order, cutOffFrequeny, wf);

            result = filter.doFIR(input.ToList<float>()).ToArray();





            return result;

        }

        public float[] diff(float[] input, float dt)//derivace
        {
            float[] result = new float[input.Length - 1];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (input[i + 1] - input[i]) / dt;
            }

            return result;
        }

        public float[] breath_freq(float[] input_sig, float[] input_det, float sampleFreq)//vrati schodovou funkci dechove frekvence
        {

            float[] result = new float[input_sig.Length];

            for (int i = 0; i < input_det.Length - 1; i++)
            {
                int n0 = (int)input_det[i];
                while (n0 < input_det[i + 1])
                {
                    result[n0] = 60 * sampleFreq / (input_det[i + 1] - input_det[i]);
                    n0++;
                }
            }


            return result;
        }

        public List<float> breath_getfreq(int[] input_det, float sampleFreq)//vrati hodnoty frekvence pro jednotlive useky mezi maximy
        {
            List<float> result = new List<float>();

            for (int i = 0; i < input_det.Length - 1; i++)
            {

                result.Add(60 * sampleFreq / (input_det[i + 1] - input_det[i]));

            }
            return result;

        }

        public void draw(float[] input, PaintEventArgs e, Pen color)
        {
            PointF transY = mainView.computeScaleandBaseFromMinMax(input.Min(), input.Max(), (int)(0.8f * pbx.Height));
            PointF transX = mainView.computeScaleandBaseFromMinMax(0, input.Length, pbx.Width);
            transY.Y += 0.1f * pbx.Height;

            float yo = 0;
            float xo = 0;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            for (int i = 0; i < input.Length; i++)
            {
                float y = (input[i] * transY.X + transY.Y);
                float x = (pbx.Width - i * transX.X - transX.Y);

                if (i > 0)
                    e.Graphics.DrawLine(color, x, y, xo, yo);//Pens.Gray
                xo = x;
                yo = y;
            }
        }

        public void draw(float[] input, PaintEventArgs e, Pen color, float[] scale)
        {
            PointF transY = mainView.computeScaleandBaseFromMinMax(scale.Min(), scale.Max(), (int)(0.8f * pbx.Height));
            PointF transX = mainView.computeScaleandBaseFromMinMax(0, scale.Length, pbx.Width);
            transY.Y += 0.1f * pbx.Height;

            float yo = 0;
            float xo = 0;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            for (int i = 0; i < input.Length; i++)
            {
                float y = (input[i] * transY.X + transY.Y);
                float x = (pbx.Width - i * transX.X - transX.Y);

                if (i > 0)
                    e.Graphics.DrawLine(color, x, y, xo, yo);//Pens.Gray
                xo = x;
                yo = y;
            }
        }

        public void draw_detections(float[] input, int[] det_input, PaintEventArgs e, Pen color, bool mark, float[] scale)
        {
            PointF transY = mainView.computeScaleandBaseFromMinMax(scale.Min(), scale.Max(), (int)(0.8f * pbx.Height));
            PointF transX = mainView.computeScaleandBaseFromMinMax(0, scale.Length, pbx.Width);
            transY.Y += 0.1f * pbx.Height;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;


            for (int i = 0; i < det_input.Length; i++)
            {
                float y = 1 * (input[det_input[i]] * transY.X + transY.Y);
                float x = (pbx.Width - det_input[i] * transX.X - transX.Y);

                if (i >= 0)
                {
                    if (mark == true)
                    {
                        e.Graphics.DrawLine(color, x, (int)(0.05f * pbx.Height), x, (int)(0.95f * pbx.Height));//Pens.Gray
                    }
                    else
                    {
                        e.Graphics.DrawLine(color, x, y - 10, x, y + 10);//Pens.Gray
                        e.Graphics.DrawLine(color, x - 10, y, x + 10, y);//Pens.Gray
                    }
                }
            }
        }

        public void draw_detections(float[] input, int[] det_input, PaintEventArgs e, Pen color, bool mark, float[] scale,List<string> text)
        {
            PointF transY = mainView.computeScaleandBaseFromMinMax(scale.Min(), scale.Max(), (int)(0.8f * pbx.Height));
            PointF transX = mainView.computeScaleandBaseFromMinMax(0, scale.Length, pbx.Width);
            transY.Y += 0.1f * pbx.Height;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;


            for (int i = 0; i < det_input.Length; i++)
            {
                float y = 1 * (input[det_input[i]] * transY.X + transY.Y);
                float x = (pbx.Width - det_input[i] * transX.X - transX.Y);

                if (i >= 0)
                {
                    if (mark == true)
                    {
                        e.Graphics.DrawLine(color, x, (int)(0.05f * pbx.Height), x, (int)(0.95f * pbx.Height));//Pens.Gray
                        e.Graphics.DrawString(text[i], SystemFonts.DefaultFont, Brushes.Blue, x-5, 5);
                    }
                    else
                    {
                        e.Graphics.DrawLine(color, x, y - 10, x, y + 10);//Pens.Gray
                        e.Graphics.DrawLine(color, x - 10, y, x + 10, y);//Pens.Gray
                    }
                }
            }
        }


        public void draw_marks_scale(float[] input, List<selectionMember> marks, PaintEventArgs e, Pen color, float[] scale)
        {
            PointF transY = mainView.computeScaleandBaseFromMinMax(scale.Min(), scale.Max(), (int)(0.8f * pbx.Height));
            PointF transX = mainView.computeScaleandBaseFromMinMax(0, scale.Length, pbx.Width);
            transY.Y += 0.1f * pbx.Height;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;


            for (int i = 0; i < marks.Count(); i++)
            {

                float x1 = (pbx.Width - marks[i].leftIndex * transX.X - transX.Y);
                float x2 = (pbx.Width - marks[i].rightIndex * transX.X - transX.Y);

                if (i >= 0)
                {

                    // e.Graphics.DrawLine(color, x1, 0, x1, transY.Y * 0.72f);
                    // e.Graphics.DrawLine(color, x2, 0, x2, transY.Y * 0.72f);
                    e.Graphics.FillRectangle(artArea, x1, (int)(0.05f * pbx.Height), x2 - x1, (int)(0.90f * pbx.Height));
                }
            }
        }

        public float[] zero_detect(float[] input)
        {
            List<float> detekce = new List<float>();
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] < 0.000001 && input[i] > -0.000001)
                {
                    detekce.Add(i);
                }
            }
            float[] result = new float[detekce.Count];
            detekce.CopyTo(result);
            return result;
        }


        public float[] max_detect(float[] input)
        {
            List<float> detekce = new List<float>();
            for (int i = 1; i < input.Length - 1; i++)
            {
                if (input[i] > input[i - 1] && input[i] >= input[i + 1])
                {
                    detekce.Add(i);
                }
            }
            float[] result = new float[detekce.Count];
            detekce.CopyTo(result);
            return result;
        }



        public float[] min_detect(float[] input)
        {
            List<float> detekce = new List<float>();
            for (int i = 1; i < input.Length - 1; i++)
            {
                if (input[i] < input[i - 1] && input[i] <= input[i + 1])
                {
                    detekce.Add(i);
                }
            }
            float[] result = new float[detekce.Count];
            detekce.CopyTo(result);
            return result;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            trackbar1_val = trackBar1.Value;
            if (!bgwPre.IsBusy)
            {
                refrControls();
                doPreviewWork();

            }


            processallowed = true;
            progressBar1.Value = 0;
            ResetResultLable();
            tabPage2.Parent = null;
            tabPage3.Parent = null;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            trackbar2_val = (trackBar2.Value) / 10f;
            if (!bgwPre.IsBusy)
            {
                refrControls();
                doPreviewWork();

            }

            processallowed = true;
            progressBar1.Value = 0;
            ResetResultLable();
            tabPage2.Parent = null;
            tabPage3.Parent = null;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

            trackbar2_val = (float)numericUpDown1.Value;
            refrControls();

            if (!bgwPre.IsBusy && !presetLoading)
            {
                doPreviewWork();
            }
            //if (linkedChannels.Count > 0)
            //{
            //    btProcess.Enabled = true;
            //}
            processallowed = true;
            progressBar1.Value = 0;
            ResetResultLable();
            tabPage2.Parent = null;
            tabPage3.Parent = null;

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            trackbar1_val = (int)numericUpDown2.Value;
            refrControls();
            if (!bgwPre.IsBusy && !presetLoading)
            {
                doPreviewWork();
            }
            //if (linkedChannels.Count > 0)
            //{

            //    btProcess.Enabled = true;
            //}
            processallowed = true;
            progressBar1.Value = 0;
            ResetResultLable();
            tabPage2.Parent = null;
            tabPage3.Parent = null;
        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void pbx_Resize(object sender, EventArgs e)
        {
            pbx.Refresh();
        }

        private void bt_marks_Click(object sender, EventArgs e)
        {
            call_bt_marks_Click();
        }


        //
        //MARKS OUT
        //
        public void call_bt_marks_Click()
        {
            //for (int i = 0; i < Breath_all.Max_position.Count; i++)
            //{
            //    marksList_out.Add(new selectionMember(Breath_all.Max_position[i], Breath_all.Max_position[i], "BREATH_MAX "+i.ToString(), linkedChannels[0]));
            //}

            //graphPanel.selectionAreas.AddRange(marksList_out); // add new marks

            //mv.refreshMarks(); // refresh marks in SignalPlant
            graphPanel.selectionAreas.AddRange(Breath_all.marksList_valid);
            mv.refreshMarks();
            exportmarks = false;
            refrControls();
        }

        private void bt_signal_Click(object sender, EventArgs e)
        {
            call_bt_signal_Click();
        }

        public void call_bt_signal_Click()
        {
            if (!(Breath_all.Filtered == null))
            {
                string name = "FIR-" + trackbar1_val.ToString() + " LP-" + trackbar2_val.ToString() + " Hz";
                lkk.dataCache.Add(new signalViewer.dataCacheLevel(name, Breath_all.Filtered));

                exportsignal = false;
                refrControls();
            }
            else
            {
                MessageBox.Show("Click Process first");
                return;
            }
        }

        private void bt_freq_Click(object sender, EventArgs e)
        {
            call_bt_freq_Click();
        }
        public void call_bt_freq_Click()
        {
            graphPanel gp = new graphPanel("FREQ", Breath_all.Freqfunction, mv);
            gp.unit = "breaths/min";
            mv.addNewChannel(gp, 0, true);

            mv.rebuiltAndRedrawAll();
            exportfreq = false;
            refrControls();
        }



        //public string COMMAND_BREATH(string parameters)
        //{

        //    // dekompozice
        //    //BREATH CHANNEL(RESP) PARAM(500,0.5) MARKS(1,30)
        //    try
        //    {
        //        mv = (mainView)Application.OpenForms[0];
        //        string[] pars = parameters.Split(' ');

        //        string channel_name = mainView.insideAbbrevs(pars[1]);

        //        linkedChannels.Add(mv.getGPbyName(channel_name));


        //        string[] param = mainView.insideAbbrevs(pars[2]).Split(',');


        //        if (Int32.Parse(param[0]) <= 0 || Int32.Parse(param[0]) > 1000) return "Error:"+"filter order out of range";
        //        trackbar1_val = Int32.Parse(param[0]);

        //        if (float.Parse(param[1] + ',' + param[2]) <= 0 || float.Parse(param[1] + ',' + param[2]) > 5) return "Error:" + "frequency out of range";
        //        trackbar2_val = float.Parse(param[1] +','+param[2]);

        //        if (pars.Length ==4)
        //        {
        //            string[] marks = mainView.insideAbbrevs(pars[3]).Split(',');
        //            windowsize = Int32.Parse(marks[1]);
        //            signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();
        //            sc.setFilter("");
        //            marksList = sc.result;
        //            foreach (selectionMember znacka in marksList)
        //            {
        //                if (znacka.info.Contains(marks[0]))
        //                {
        //                    start_mark = znacka;
        //                    break;
        //                }
        //            }
        //         }

        //        bgw.RunWorkerAsync();
        //        COMRE.WaitOne();
        //       // call_bt_freq_Click();
        //       // call_bt_marks_Click();
        //       // call_bt_signal_Click();


        //        int r = 0;
        //    }
        //    catch (Exception e)
        //    {
        //        return "Error:" + e.Message;
        //    }


        //    return ("Completed Succesfully");

        //}

        //public static string CMDDESCRIPTION_BREATH()
        //{
        //    return "Breath. Use : BREATH CHANNEL(V1) PARAM(FIR_ORDER,FREQ_CUT) MARKS(START_MARK_NAME,WINDOW_LENGTH)";
        //}

        public string COMMAND_BREATH(string parameters)
        {
            try
            {
                mv = (mainView)Application.OpenForms[0];

                string[] pars = parameters.Split(' ');

                if (pars[1] == "") return "Not enough parameters. Set channel name.";
                else
                {
                    linkedChannels.Add(mv.getGPbyName(mainView.insideAbbrevs(pars[1])));
                }

                for (int i = 2; i < pars.Length; i++)
                {
                    if (pars[i] == null) break;


                    
                    if (pars[i].Contains("EXPORT(") && i < pars.Length)
                    {
                        string temp = pars[i];
                        pars[i] = pars[pars.Length - 1];
                        pars[pars.Length - 1] = temp;
                    }

                    if (pars[i].Contains("PARAM("))
                    {

                        string[] param = mainView.insideAbbrevs(pars[i]).Split(';');
                        if (param.Length != 3) return "Error: not enough parameters in PARAM().";

                        if (Int32.Parse(param[0]) <= 0 || Int32.Parse(param[0]) > 5000) return "Error:" + "filter order out of range";
                        trackbar1_val = Int32.Parse(param[0]);


                        if (float.Parse(param[1]) <= 0 || float.Parse(param[1]) > 100) return "Error:" + "frequency out of range";
                        trackbar2_val = float.Parse(param[1]);


                    }

                    if (pars[i].Contains("AREA("))
                    {
                        string[] marks = mainView.insideAbbrevs(pars[i]).Split(';');
                        if (marks.Length != 3) return "Error: not enough MARKS parameters.";
                        windowsize = Int32.Parse(marks[1]);

                        signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();
                        sc.setFilter(marks[0]);
                        start_mark = sc.result[0];

                        sc.setFilter(marks[2]);
                        marksList = sc.result;

                    }

                    if (i == pars.Length - 1)
                    {
                        cmd_call = true;
                        bgw.RunWorkerAsync();
                        COMRE.WaitOne();
                    }

                    if (pars[i].Contains("EXPORT("))
                    {
                        

                        string[] export = mainView.insideAbbrevs(pars[i]).Split(';');
                        for (int j = 0; j < export.Length; j++)
                        {
                            switch (export[j])
                            {
                                case "FiltSig":
                                    call_bt_signal_Click();
                                    break;

                                case "Marks":
                                    call_bt_marks_Click();
                                    break;


                                case "FreqFnc":
                                    call_bt_freq_Click();
                                    break;




                            }
                        }
                    }


                }
            }
            catch (Exception e)
            {
                return "Error:" + e.Message;
            }

            return ("Completed Succesfully");
        }
        //==============================================================================================================================================================================================
        // BREATH CHANNEL(Channel_1) EXPORT(FiltSig;FreqFnc;Marks)
        public static string CMDDESCRIPTION_BREATH()
        {
            return "Breath analysis. Use : BREATH CHANNEL(V1) PARAM(FIR_ORDER;FREQ_CUT) AREA(START_MARK_NAME;WINDOW_LENGTH;ART_MARK) EXPORT(FiltSig;FreqFnc,Marks)";
        }
        //================================================================================================================================================================================================
        public class Signal
        {
            public float Fs;
            public List<float> Original;
            public List<float> Filtered;
            public List<float> Freqfunction;
            public List<int> Max_position;
            public List<int> Min_position;
            public List<float> Frequency;
            public statistics Frequency_stat;
            public List<Point> Frequency_pos;
            public List<float> Amplitude;
            public statistics Amplitude_stat;
            public List<float> Ventilation;
            public statistics Ventilation_stat;
            public List<selectionMember> marksList;
            public string to_clip;
            public List<selectionMember> PreviewMarksValid;

            //
            //VALID znamena znacky v vybrane oblasti <startmark , startmark+30>
            //
            public List<float> Amplitude_valid;
            public List<float> Frequency_valid;
            public List<float> Ventilation_valid;

            public List<selectionMember> marksList_valid;
            public List<Point> Frequency_pos_valid;

            public selectionMember start_mark_corr;
            public void start_mark_corr_set(selectionMember x)
            {
                if (x != null)
                    start_mark_corr = new selectionMember(x.leftIndex, x.rightIndex, x.info, x.linkedChannel);
                else
                    start_mark_corr = null;
                //start_mark_corr = x;
            }

            public void previewMarks(int Left, int Right, selectionMember start_mark, int windowsize, int correction)
            {
                int start = 0;
                if (start_mark != null) start = start_mark.leftIndex;
                PreviewMarksValid = new List<selectionMember>();

                if (marksList != null)
                {
                    for (int i = 0; i < marksList.Count(); i++)
                    {
                        bool isOK = true;
                        //prunik intervalu
                        if (!(marksList[i].rightIndex > Left && marksList[i].leftIndex < Right))
                        {
                            isOK = false;

                        }

                        if (start_mark != null)
                        {
                            if (!(marksList[i].rightIndex > start_mark.leftIndex && marksList[i].leftIndex < start_mark.rightIndex + Fs * windowsize))
                            {
                                isOK = false;
                            }
                        }

                        if (isOK == true)
                        {
                            selectionMember temp = new selectionMember(marksList[i].leftIndex, marksList[i].rightIndex, marksList[i].info, marksList[i].linkedChannel);
                            PreviewMarksValid.Add(temp);
                        }
                    }
                }

                for (int i = 0; i < PreviewMarksValid.Count; i++)
                {
                    PreviewMarksValid[i].leftIndex -= correction;
                    PreviewMarksValid[i].rightIndex -= correction;
                }
            }



            public void getmarks(List<selectionMember> marks)
            {
                marksList = new List<selectionMember>();
                marksList = marks;
            }

            public Signal(float fs)
            {
                Fs = fs;
            }

            public void max_detect(float delay)
            {
                Max_position = new List<int>();
                for (int i = 1; i < Filtered.Count - 1; i++)
                {
                    if (Filtered[i] > Filtered[i - 1] && Filtered[i] >= Filtered[i + 1])
                    {
                        Max_position.Add(i);
                        i += (int)(delay * Fs);
                    }
                }

            }

            public void min_detect(int first_max_pos, float delay)
            {
                Min_position = new List<int>();
                for (int i = first_max_pos; i < Filtered.Count - 1; i++)
                {
                    if (Filtered[i] < Filtered[i - 1] && Filtered[i] <= Filtered[i + 1])
                    {
                        Min_position.Add(i);
                        i += (int)(delay * Fs);
                    }
                }

            }

            //vrati hodnoty frekvence pro jednotlive useky mezi detekcemi
            public void frequency(List<int> input)
            {
                Frequency = new List<float>();
                Frequency_pos = new List<Point>();

                for (int i = 0; i < input.Count - 1; i++)
                {
                    Frequency_pos.Add(new Point(input[i], input[i + 1]));
                    Frequency.Add(60 * Fs / (input[i + 1] - input[i]));

                }

            }



            public List<int> max_valid;
            public List<int> min_valid;

            public void pre_max_valid(int windowsize, selectionMember start_mark, List<signalViewer.graphPanel> linkedChannels, int correction)
            {
                max_valid = new List<int>();


                int start = 0;
                if (start_mark != null) start = start_mark.leftIndex;


                for (int i = 0; i < Max_position.Count; i++)
                {


                    bool isOK = true;
                    if (marksList != null)
                    {
                        foreach (selectionMember znacka in marksList)
                        {

                            if (!(Max_position[i] > znacka.rightIndex - correction || Max_position[i] < znacka.leftIndex - correction))
                            {

                                isOK = false;
                                break;

                            }
                        }
                    }

                    if (start_mark != null)
                    {



                        if (!(Max_position[i] > start && Max_position[i] < start + Fs * windowsize))
                        {

                            isOK = false;


                        }

                    }

                    if (isOK)
                    {


                        max_valid.Add(Max_position[i]);

                    }
                }

            }

            public void pre_min_valid(int windowsize, selectionMember start_mark, List<signalViewer.graphPanel> linkedChannels, int correction)
            {

                min_valid = new List<int>();

                int start = 0;
                if (start_mark != null) start = start_mark.leftIndex;


                for (int i = 0; i < Min_position.Count; i++)
                {


                    bool isOK = true;
                    if (marksList != null)
                    {
                        foreach (selectionMember znacka in marksList)
                        {

                            if (!(Min_position[i] > znacka.rightIndex - correction || Min_position[i] < znacka.leftIndex - correction))
                            {

                                isOK = false;
                                break;

                            }
                        }
                    }

                    if (start_mark != null)
                    {



                        if (!(Min_position[i] > start && Min_position[i] < start + Fs * windowsize))
                        {

                            isOK = false;


                        }

                    }

                    if (isOK)
                    {


                        min_valid.Add(Min_position[i]);

                    }
                }

            }

            public void frequency_stat(int windowsize, selectionMember start_mark, List<signalViewer.graphPanel> linkedChannels)
            {
                max_valid = new List<int>();
                List<float> freq_temp = new List<float>();
                List<float> ampl_temp = new List<float>();
                List<float> vent_temp = new List<float>();
                List<Point> frequency_pos_temp = new List<Point>();
                marksList_valid = new List<selectionMember>();
                int start = 0;

                if (start_mark != null) start = start_mark.leftIndex;


                for (int i = 0; i < Frequency_pos.Count; i++)
                {


                    bool isOK = true;
                    if (marksList != null)
                    {
                        foreach (selectionMember znacka in marksList)
                        {


                            if (!(Frequency_pos[i].X > znacka.rightIndex || Frequency_pos[i].Y < znacka.leftIndex))
                            {

                                isOK = false;
                                break;

                            }
                        }
                    }

                    if (start_mark != null)
                    {



                        if (!(Frequency_pos[i].X > start && Frequency_pos[i].Y < start + Fs * windowsize))
                        {

                            isOK = false;


                        }

                    }

                    if (isOK)
                    {
                        frequency_pos_temp.Add(Frequency_pos[i]);
                        max_valid.Add(Frequency_pos[i].X);
                        freq_temp.Add(Frequency[i]);
                        ampl_temp.Add(Amplitude[i]);
                        vent_temp.Add(Amplitude[i] * Frequency[i]);


                        marksList_valid.Add(new selectionMember(Frequency_pos[i].X, Frequency_pos[i].Y, "Interval " + marksList_valid.Count.ToString(), linkedChannels[0]));
                    }
                }


                Frequency_stat = new statistics(null, freq_temp, 0, 0, "");
                Amplitude_stat = new statistics(null, ampl_temp, 0, 0, "");
                Ventilation_stat = new statistics(null, vent_temp, 0, 0, "");
                Amplitude_valid = ampl_temp;
                Frequency_valid = freq_temp;
                Ventilation_valid = vent_temp;
                Frequency_pos_valid = frequency_pos_temp;
            }



            //vrati amplitudu mezi maxiy a minimy
            public void amplitude()
            {
                Amplitude = new List<float>();

                for (int i = 0; i < Min_position.Count && i < Max_position.Count; i++)
                {

                    Amplitude.Add(Filtered[Max_position[i]] - Filtered[Min_position[i]]);

                }

            }

            public void amplitude_stat()
            {
                List<double> temp = new List<double>();


                Amplitude_stat = new statistics(null, Amplitude, 0, 0, "");
            }

            public void ventilation()
            {
                Ventilation = new List<float>();
                for (int i = 0; i < Frequency.Count; i++)
                {
                    Ventilation.Add(Frequency[i] * Amplitude[i]);
                }
            }

            public void ventilation_stat()
            {
                Ventilation_stat = new statistics(null, Ventilation, 0, 0, "");
            }

            public int find_neares_higher(List<int> x, int value)
            {
                int temp = 1000000;
                int output = 0;
                for (int i = 0; i < x.Count; i++)
                {
                    if (x[i] < value) continue;
                    if (x[i] - value < temp)
                    {
                        temp = x[i] - value;
                        output = x[i];
                    }

                }

                return output;
            }

            public void freqfunction()
            {
                Freqfunction = new List<float>();

                for (int i = 0; i < Max_position.Count - 1; i++)
                {
                    int x0;
                    if (i == 0)
                    {
                        x0 = 0;
                        while (x0 < Max_position[0])
                        {
                            Freqfunction.Add(0);
                            x0++;
                        }
                    }

                    x0 = Max_position[i];
                    while (x0 < Max_position[i + 1])
                    {
                        Freqfunction.Add(Frequency[i]);
                        x0++;
                    }
                }

                if (marksList_valid != null)
                {
                    for (int i = 0; i < Freqfunction.Count; i++)
                    {
                        if (i < marksList_valid[0].leftIndex || i > marksList_valid[marksList_valid.Count - 1].rightIndex) Freqfunction[i] = 0;
                    }


                    for (int i = marksList_valid[0].leftIndex; i < marksList_valid[marksList_valid.Count - 1].rightIndex; i++)
                    {
                        bool iOK = false;
                        for (int j = 0; j < marksList_valid.Count; j++)
                        {
                            if (i >= marksList_valid[j].leftIndex && i <= marksList_valid[j].rightIndex)
                            {
                                iOK = true;
                                break;
                                

                            }
                            
                        }
                        if(!iOK) Freqfunction[i] = 0;
                    }

                }


            }

        }

            private void btMarks_Click_1(object sender, EventArgs e)
            {

            }

            private void numericUpDown3_ValueChanged(object sender, EventArgs e)
            {
                windowsize = (int)numericUpDown3.Value;
                if (!bgwPre.IsBusy)
                {
                    refrControls();
                    doPreviewWork();

                }

                processallowed = true;
                progressBar1.Value = 0;
                ResetResultLable();
            }

            private void button1_Click(object sender, EventArgs e)
            {
                start_mark = null;
                signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();
                sc.setFilter("");


                if (sc.ShowDialog() == DialogResult.OK)
                {
                    if (sc.result.Count > 1) MessageBox.Show("More then 1 start markers were chosen. Only the first one is selected.");
                    if (sc.result.Count != 0)
                    {
                        start_mark = sc.result[0];
                    }
                    processallowed = true;

                    ResetResultLable();
                    refrControls();
                    doPreviewWork();
                }
            }

            private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
            {

            }

            private void button2_Click(object sender, EventArgs e)
            {
                Bitmap locBMP = new Bitmap(pbx.Width, pbx.Height);

                pbx.DrawToBitmap(locBMP, pbx.ClientRectangle);

                Clipboard.SetImage(locBMP);

            }

            private void label1_Click_1(object sender, EventArgs e)
            {

            }

            private void label2_Click_1(object sender, EventArgs e)
            {

            }

        private void delay_ValueChanged(object sender, EventArgs e)
        {
            doExternalRefresh();
            processallowed = true;
            tabPage2.Parent = null;
            tabPage3.Parent = null;
        }
    }


    }


