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
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace plugins_phisio1
{
    public partial class SVP_breath_new : UserControl
    {
        List<signalViewer.graphPanel> linkedChannels = new List<signalViewer.graphPanel>(); //--array of linked channels
       

        filter_FIR filter = new filter_FIR();
        public System.Timers.Timer timer = new System.Timers.Timer();

        signalViewer.graphPanel lk; 

        Pen filPen = new Pen(Color.Black, 1);
        Pen MarkPen = new Pen(Color.OrangeRed, 1);
        Pen MarkPen2 = new Pen(Color.OrangeRed, 3);
        Pen FiltPen = new Pen(Color.DeepSkyBlue, 1);
        Pen StartPen = new Pen(Color.Orange, 3);
        SolidBrush artArea = new SolidBrush(Color.FromArgb(150, Color.Gray));
        Pen artefaktpen = new Pen(Color.FromArgb(25, Color.Gray),3);


        List<selectionMember> marksList;
        List<selectionMember> startmark;
        List<selectionMember> startmark_copy;

        public List<int> rename_marks_exsp=new List<int>();
        public List<int> rename_marks_insp = new List<int>();

        //Signal signal = new Signal((int)mainView.sampleFrequency);
        Signal signal;
        Signal signal_copy;
        Signal PrewieSig;

        bool filter_CMD = false;
        int filter_CMD_order;
        float filter_CMD_cutoff;
        bool CMD=false;
     
        mainView mv; //-- instance of signal viewer program

        ManualResetEvent COMRE = new ManualResetEvent(false);

        public string helpFile = "SignalPlant_Physiocrate.pdf"; //--name of help file
        public string helpNamedDest; //= "SignalPlant.indd:Downsample:303"; //---named destination in the help file

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
            return "Respiration [PhysioCrate]";        //---plugin name, visible in Plugins menu
        }

        public void doExternalRefresh()//------
        {
            if(linkedChannels.Count == 1)
            doPreviewWork();
           
        }

        private void refrControls()
        {
            if (linkedChannels.Count == 0)
            {
                btChannels.Text = "Drag a channel here or click";
                groupBox5.Enabled = false;
                groupwindow.Enabled = false;
                btMarks.Enabled = false;
                bt_signal.Enabled = false;
                btProcess.Enabled = false;
                button2.Enabled = false;
                bt_marks.Enabled = false;
                bt_freq.Enabled = false;
                groupBox1.Enabled = false;
            }
            if (linkedChannels.Count == 1)
            {
               
                btChannels.Text = "Linked channel: " + linkedChannels[0].channelName;
               
                if (checkBox2.Checked == false)
                {

                    button3.Enabled = false;
                    btProcess.Enabled = true;
                    button2.Enabled = true;
                    bt_marks.Enabled = true;
                    bt_freq.Enabled = true;
                    groupBox1.Enabled = true;
                    groupBox5.Enabled = true;
                    groupwindow.Enabled = true;
                    btMarks.Enabled = true;
                }
                else
                {
                    groupBox5.Enabled = false;
                    groupwindow.Enabled = false;
                    btMarks.Enabled = false;
                    bt_signal.Enabled = false;
                    btProcess.Enabled = false;
                    button2.Enabled = false;
                    bt_marks.Enabled = false;
                    bt_freq.Enabled = false;
                    button3.Enabled = true;
                }
            }
            

            if (signal != null && signal.Original_Filtered != null)
            {
                bt_signal.Enabled = true;
            }


            if (marksList != null && marksList.Count > 0) btMarks.Text = "Artifact marks selected: " + marksList.Count;
            else btMarks.Text = "(Choose some artifact marks)";

            if (startmark != null)
            {
                button1.Text = "Start marks selected: " + startmark.Count.ToString();
                window.Enabled = true;
                numericUpDown3.Enabled = true;
            }
            else
            {
                button1.Text = "(Choose some start mark)";
                window.Enabled = false;
                numericUpDown3.Enabled =false;
            }
            pbx.Refresh();

            if (checkBox1.Checked == false)
            {
                
                numericUpDown1.Enabled = false;
                numericUpDown2.Enabled = false;
            }
            else
            {
                
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
            }
           

        }

        private void doPreviewWork()
        {

            if (!bgwPre.IsBusy)
            {
                bgwPre.RunWorkerAsync();
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
                lk = linkedChannels[0];


                //signal = new Signal((int)mainView.sampleFrequency, lk.dataCache[0].data, 1000, 0.1f);
                signal = new Signal((int)mainView.sampleFrequency, lk.dataCache[0].data);
                refrControls();
                doPreviewWork();

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    signal_copy = new Signal((int)mainView.sampleFrequency, lk.dataCache[0].data);
                }).Start();

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

                lk = linkedChannels[0];


                //signal = new Signal((int)mainView.sampleFrequency, lk.dataCache[0].data, 1000, 0.1f);
                signal = new Signal((int)mainView.sampleFrequency, lk.dataCache[0].data);
                refrControls();
                doPreviewWork();

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    signal_copy = new Signal((int)mainView.sampleFrequency, lk.dataCache[0].data);
                }).Start();

                }

        }



        public SVP_breath_new()
        {
            InitializeComponent();

        }

        private void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {


        }

        private void pbx_Paint(object sender, PaintEventArgs e)
        {

            try
            {


                if (PrewieSig != null)
                {
                    if (checkBox1.Checked == true)
                    {
                        string Text = "File: " + mainView.currentFile + "   Channel:" + linkedChannels[0].channelName + "    Time:" + (graphPanel.leftI / mainView.sampleFrequency).ToString() + " s - " + (graphPanel.rightI/ mainView.sampleFrequency).ToString() + " s    Filter: FIR LP, order:" + numericUpDown2.Value.ToString() + " Fc: " + numericUpDown1.Value.ToString() + " Hz";
                        e.Graphics.DrawString(Text, SystemFonts.DefaultFont, Brushes.Black, 25, pbx.Height - 15);
                    }
                    else
                    {
                        string Text = "File: " + mainView.currentFile + "   Channel:" + linkedChannels[0].channelName + "    Time:" + (graphPanel.leftI/ mainView.sampleFrequency).ToString() + " s - " + (graphPanel.rightI/ mainView.sampleFrequency).ToString() + " s ";
                        e.Graphics.DrawString(Text, SystemFonts.DefaultFont, Brushes.Black, 25, pbx.Height - 15);
                    }
                    if (PrewieSig.Output == null || PrewieSig.Output_Marks == null)
                    {
                        
                        draw(PrewieSig.Original, e, filPen);
                        if (PrewieSig.Original_Filtered != null)
                        {
                            draw(PrewieSig.Original_Filtered, PrewieSig.Original, e, FiltPen);
                        }
                        if (PrewieSig.Marks_Valid == null)
                        {
                            draw_marks(PrewieSig.Original_Marks, PrewieSig.Original, e, MarkPen);
                        }
                        else draw_marks(PrewieSig.Marks_Valid, PrewieSig.Original, e, MarkPen);
                    }
                    else
                    {
                        
                        draw(PrewieSig.Output, e, filPen);
                        if (PrewieSig.Output_Filtered != null)
                        {
                            draw(PrewieSig.Output_Filtered, PrewieSig.Output, e, FiltPen);
                        }
                        if (PrewieSig.Marks_Valid == null)
                        {
                            draw_marks(PrewieSig.Original_Marks, PrewieSig.Original, e, MarkPen);
                        }
                        else draw_marks(PrewieSig.Marks_Valid, PrewieSig.Original, e, MarkPen);
                    }

                    draw_start_marks(PrewieSig.AnotationList, PrewieSig.Original, e, MarkPen2);
                    

                    
                    
                }
            }
            catch (Exception exp)
            {
                e.Graphics.DrawString(exp.Message, SystemFonts.DefaultFont, Brushes.Black, pbx.Width / 2, pbx.Height / 2, mainView.sfc);
            }
        }



        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {



        }

        private void SVP_plugin_v3_Load(object sender, EventArgs e)
        {
            if (this.ParentForm != null)
                mv = (mainView)Application.OpenForms[0];

            timer.Interval = 100;
            timer.Elapsed += timer_Elapsed;

            dataGridView2.ColumnCount = 5;
           
            dataGridView2.Columns[0].Name = "mark name";
            dataGridView2.Columns[1].Name = "amplitude";
            dataGridView2.Columns[2].Name = "dt";
            dataGridView2.Columns[3].Name = "start time";
            dataGridView2.Columns[4].Name = "stop time";

            dataGridView3.ColumnCount = 5;
            
            dataGridView3.Columns[0].Name = "mark name";
            dataGridView3.Columns[1].Name = "amplitude";
            dataGridView3.Columns[2].Name = "dt";
            dataGridView3.Columns[3].Name = "start time";
            dataGridView3.Columns[4].Name = "stop time";

            dataGridView1.RowCount = 11;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridView1.RowHeadersWidth = 150;

            dataGridView1.Rows[0].HeaderCell.Value = "Amplitude.mean";
            dataGridView1.Rows[1].HeaderCell.Value = "Amplitude.std";
            dataGridView1.Rows[2].HeaderCell.Value = "Respiration period mean";
            dataGridView1.Rows[3].HeaderCell.Value = "Respiration period std";
            dataGridView1.Rows[4].HeaderCell.Value = "freq.mean";
            dataGridView1.Rows[5].HeaderCell.Value = "freq.std";
            dataGridView1.Rows[6].HeaderCell.Value = "Inspiration ";
            dataGridView1.Rows[7].HeaderCell.Value = "Inspiration std";
            dataGridView1.Rows[8].HeaderCell.Value = "Exspiration mean";
            dataGridView1.Rows[9].HeaderCell.Value = "Exspiration std";
            dataGridView1.Rows[10].HeaderCell.Value = "Ventilation";
            

            refrControls();

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
            signal.Marks();


            PrewieSig = signal.GetRange(viewL, N);
            PrewieSig.FitToPbx(pbx.Width);
         
        }



        private void bgwPre_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void bgwPre_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            refrControls();
            int g = 0;
            for (int i = 0;i< signal.AnotationList.Count; i++)
            {
                if (signal.AnotationList[i].minimum == false && signal.AnotationList[i].maximum == false) continue;
                if (signal.AnotationList[i].minimum == true && signal.AnotationList[i].maximum == false) { signal.AnotationList[i].minimum = false; continue; }
                if (signal.AnotationList[i].minimum == false && signal.AnotationList[i].maximum == true) { break; }
            }

            if (checkBox2.Checked)
            {
                signal.getInspirium(lk, rename_marks_insp);
                signal.getExspirium(lk, rename_marks_exsp);
            }
            else
            {
                signal.getInspirium(lk, null);
                signal.getExspirium(lk, null);
            }



            List<List<My_selectionMember>> results_Exspirium = signal._getMy_selectionMemberInRange(signal.Exspirium, startmark, (int)window.Value, marksList);
            List<List<My_selectionMember>> results_Inspirium = signal._getMy_selectionMemberInRange(signal.Inspirium, startmark, (int)window.Value, marksList);
            resultsPrint(results_Exspirium, dataGridView2);
            resultsPrint(results_Inspirium, dataGridView3);
            resultsStaisticsPrint(results_Inspirium, results_Exspirium, dataGridView1);

           

            COMRE.Set();
        }


        WindowingFunction wf = new WindowingFunction(40, WindowingFunction.windowsType.Hamming, 0); //--windowing function for filter

        public float[] computeFIR_LP(float[] input, int order, float cutOffFrequeny, float sampleFreq, filter_FIR filter)

        {

            float[] result = new float[input.Length];

            int maximumOrder = 1000;

            if (order < 0 || order > maximumOrder) return null;

            filter = filter.generateLowPass(order, cutOffFrequeny, wf);

            result = filter.doFIR(input.ToList<float>()).ToArray();

            return result;

        }


        public void draw(List<float> input, PaintEventArgs e, Pen color)
        {

            PointF transY = mainView.computeScaleandBaseFromMinMax(input.Min(), input.Max(), (int)(0.8f * pbx.Height));
            PointF transX = mainView.computeScaleandBaseFromMinMax(0, input.Count, pbx.Width);
            transY.Y += 0.1f * pbx.Height;

            float yo = 0;
            float xo = 0;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;



            for (int i = 0; i < input.Count; i++)
            {
                float y = (input[i] * transY.X + transY.Y);
                float x = (pbx.Width - i * transX.X - transX.Y);


                if (i > 0)
                    e.Graphics.DrawLine(color, x, y, xo, yo);



                xo = x;
                yo = y;
            }
        }

       

        public void draw(List<float> input, List<float> scale, PaintEventArgs e, Pen color)
        {
            if (input == null || scale == null) return;

            PointF transY = mainView.computeScaleandBaseFromMinMax(scale.Min(), scale.Max(), (int)(0.8f * pbx.Height));
            PointF transX = mainView.computeScaleandBaseFromMinMax(0, input.Count, pbx.Width);
            transY.Y += 0.1f * pbx.Height;

            float yo = 0;
            float xo = 0;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            for (int i = 0; i < input.Count; i++)
            {
                float y = (input[i] * transY.X + transY.Y);
                float x = (pbx.Width - i * transX.X - transX.Y);

                if (i > 0)
                    e.Graphics.DrawLine(color, x, y, xo, yo);
                xo = x;
                yo = y;
            }
        }


        public void draw_marks(List<float> input, List<float> scale, PaintEventArgs e, Pen color)
        {
            PointF transY = mainView.computeScaleandBaseFromMinMax(scale.Min(), scale.Max(), (int)(0.8f * pbx.Height));
            PointF transX = mainView.computeScaleandBaseFromMinMax(0, scale.Count, pbx.Width);
            transY.Y += 0.1f * pbx.Height;



            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (input == null) return;
            List<RectangleF> temp = new List<RectangleF>();
            int start = 0;
            int end = 0;
            Thread t = new Thread(() =>
            {
                

                for (int i = 1; i < input.Count - 1; i++)
                {

                    if (input[i] == 2 && input[i - 1] != 2)
                    {
                        start = i;
                    }

                    if (input[i] == 2 && ((input[i + 1] != 2)||(i+1)== input.Count-1))
                    {
                        end = i;
                        temp.Add(new RectangleF((pbx.Width - start * transX.X - transX.Y), (int)(0.05f * pbx.Height), (pbx.Width - (end - start) * transX.X - transX.Y), (int)(0.90f * pbx.Height)));
                    }
                }
            });
            t.Start();



            for (int i = 1; i < input.Count; i++)
            {
                
                float x = (pbx.Width - i * transX.X - transX.Y);


                if (input[i] == 1)
                    {
                        e.Graphics.DrawLine(color, x, (int)(0.05f * pbx.Height), x, (int)(0.95f * pbx.Height));
                        continue;
                    }              
            }
            t.Join();
            for (int i = 0; i < temp.Count; i++)
            {
                e.Graphics.FillRectangle(artArea, temp[i]);
            }

            
        }

      
        public void draw_start_marks(List<Anotations> input, List<float> scale, PaintEventArgs e, Pen color)
        {
            if (input == null || input.Count == 0) return;
            
            PointF transY = mainView.computeScaleandBaseFromMinMax(scale.Min(), scale.Max(), (int)(0.8f * pbx.Height));
            PointF transX = mainView.computeScaleandBaseFromMinMax(0, scale.Count, pbx.Width);
            transY.Y += 0.1f * pbx.Height;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int gg = 0;
            int start = 0;
            int end = 0;
            for (int i = 1; i < input.Count-1; i++)
            {
                
                float x = (pbx.Width - i * transX.X - transX.Y);

                if ((input[i-1].start == false && input[i].start == true))
                {
                    e.Graphics.DrawLine(color, x, (int)(0.05f * pbx.Height), x, (int)(0.95f * pbx.Height));
                    string Text = "Start "+gg.ToString();
                    e.Graphics.DrawString(Text, SystemFonts.DefaultFont, Brushes.Black, x-8, 8);
                    gg++;
                    
                }
                
            }

        }



        private void pbx_Resize(object sender, EventArgs e)
        {
            pbx.Refresh();
        }


        public void CMD_CALL(bool filter,int order,float cutoff)
        {
            lk = linkedChannels[0];
            
            if (filter)
            {
                signal = new Signal((int)mainView.sampleFrequency, lk.dataCache[0].data);
                recomputeFilter(CMD);
                signal.findMax(signal.Original_Filtered);
                signal.findMin(signal.Original_Filtered);
            }
            else
            {
                signal = new Signal((int)mainView.sampleFrequency, lk.dataCache[0].data);
                signal.findMax(signal.Original);
                signal.findMin(signal.Original);
            }

            signal.getInspirium(lk, null);
            signal.getExspirium(lk, null);
            List<List<My_selectionMember>> results_Exspirium = signal._getMy_selectionMemberInRange(signal.Exspirium, startmark, (int)window.Value, marksList);
            List<List<My_selectionMember>> results_Inspirium = signal._getMy_selectionMemberInRange(signal.Inspirium, startmark, (int)window.Value, marksList);

            cmd_results(results_Inspirium, results_Exspirium);
            COMRE.Set();
            
        }



        //RESPIRATION CHANNEL(Channel_1) PARAM(500;0.5) EXPORT(FiltSig; Marks)

        public string COMMAND_RESPIRATION(string parameters)
        {
            CMD = true;
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
                        filter_CMD = true;
                        string[] param = mainView.insideAbbrevs(pars[i]).Split(';');
                        if (param.Length != 2) return "Error: not enough parameters in PARAM().";

                        if (Int32.Parse(param[0]) <= 1 || Int32.Parse(param[0]) > 5000) return "Error:" + "filter order out of range";
                        numericUpDown2.Value = Int32.Parse(param[0]);


                        if (float.Parse(param[1]) <= 0 || float.Parse(param[1]) > 100) return "Error:" + "frequency out of range";
                        numericUpDown1.Value = decimal.Parse(param[1]);

                        
                    }

                    if (pars[i].Contains("AREA("))
                    {
                        string[] marks = mainView.insideAbbrevs(pars[i]).Split(';');
                        if (marks.Length != 3) return "Error: not enough MARKS parameters.";
                        window.Value = Int32.Parse(marks[1]);

                        signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();
                        sc.setFilter(marks[0]);
                        startmark = sc.result;

                        sc.setFilter(marks[2]);
                        marksList = sc.result;

                    }

                    if (i == pars.Length - 1)
                    {

                        CMD_CALL(filter_CMD,(int) numericUpDown2.Value,(float) numericUpDown1.Value);
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
                                    export_signal();
                                    break;

                                case "Marks":
                                    export_marks();
                                    break;


                                case "FreqFnc":
                                    export_BPMfunction();
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


        public void cmd_results(List<List<My_selectionMember>> results_inspirium, List<List<My_selectionMember>> results_exspirium)
        {




            int i = 0;
            foreach (List<My_selectionMember> list in results_inspirium)
            {

                var x = list.Select(amp => amp.amplitude).ToList();
                var amplituda = new statistics(null, x, 0, 0, "").avg;

                globalVariables.setVar("@Respiration_amplitude_avg_interval" + i.ToString(), amplituda.ToString());
                globalVariables.setVar("@Respiration_amplitude_sdev_interval" + i.ToString(), new statistics(null, x, 0, 0, "").dev.ToString());

                //x = list.Select(amp => amp.dT / mainView.sampleFrequency).ToList();

                //globalVariables.setVar("@Pressure_period_avg_interval" + i.ToString(), new statistics(null, x, 0, 0, "").avg.ToString());
                //globalVariables.setVar("@Pressure_period_sdev_interval" + i.ToString(), new statistics(null, x, 0, 0, "").dev.ToString());

                var z = list.Select(amp => new { amp.leftIndex, amp.rightIndex }).ToList();
                var temp_freq = new List<float>();
                var temp_period = new List<float>();
                for (int g = 1; g < z.Count; g++)
                {
                    temp_period.Add((z[g].leftIndex - z[g - 1].leftIndex) / mainView.sampleFrequency);
                    temp_freq.Add(60 / ((z[g].leftIndex - z[g - 1].leftIndex) / mainView.sampleFrequency));
                }
                var frekvence = new statistics(null, temp_freq, 0, 0, "").avg;
                globalVariables.setVar("@Respiration_BPM_avg_interval" + i.ToString(), frekvence.ToString());
                globalVariables.setVar("@Respiration_BPM_sdev_interval" + i.ToString(), new statistics(null, temp_freq, 0, 0, "").dev.ToString());
                //a.Rows[4].Cells[i].Value = frekvence.ToString();
                //a.Rows[5].Cells[i].Value = new statistics(null, temp_freq, 0, 0, "").dev.ToString();
                //a.Rows[2].Cells[i].Value = new statistics(null, temp_period, 0, 0, "").avg.ToString();
                //a.Rows[3].Cells[i].Value = new statistics(null, temp_period, 0, 0, "").dev.ToString();
                //a.Rows[10].Cells[i].Value = amplituda *frekvence;
                i++;
            }
            i = 0;
            foreach (List<My_selectionMember> list in results_exspirium)
            {

                var x = list.Select(amp => amp.dT / mainView.sampleFrequency).ToList();
                // a.Rows[8].Cells[i].Value = new statistics(null, x, 0, 0, "").avg.ToString();
                //a.Rows[9].Cells[i].Value = new statistics(null, x, 0, 0, "").dev.ToString();

                i++;
            }
        }

        public static string CMDDESCRIPTION_RESPIRATION()
        {
            return "Respiration analysis. Use : RESPIRATION CHANNEL(V1) PARAM(FIR_ORDER;FREQ_CUT) AREA(START_MARK_NAME;WINDOW_LENGTH;ART_MARK) EXPORT(FiltSig;FreqFnc,Marks)";
        }



        public class Anotations
        {
            public Nullable<bool> start;
            public bool maximum;
            public bool minimum;
            public bool artificial;

            public Anotations()
            {
                start = null;
                maximum = false;
                minimum = false;
                artificial = false;
            }

        }


        public class My_selectionMember : selectionMember
        {
            public float amplitude;
            public float dT;
            public My_selectionMember(int left, int right, string info, signalViewer.graphPanel lk, float ampl) : base(left, right, info, lk)
            {
                dT = right - left;
                amplitude = ampl;
            }
        }

        public class Signal
        {
            public int Fs;
            public List<float> Original;
            public List<float> Original_Filtered;
            public List<float> Output;

            

            public List<float> Original_Marks;
            public List<float> Output_Marks;
            public List<float> Output_Filtered;
            public List<Anotations> Output_Anotations;

            public List<float> Marks_Valid;

            public List<Anotations> AnotationList;

            public List<My_selectionMember> Inspirium;
            public List<My_selectionMember> Exspirium;




            public List<My_selectionMember> getMy_selectionMemberInRange(List<My_selectionMember> input, List<selectionMember> start, int window, List<selectionMember> artificial)
            {

                List<My_selectionMember> temp;
                List<My_selectionMember> temp2;
                if (!(start == null || start.Count == 0))
                {
                    temp = new List<My_selectionMember>();
                    foreach (selectionMember s in start)
                    {
                        foreach (My_selectionMember index in input)
                        {
                            if (s.leftIndex < index.leftIndex && index.rightIndex < s.leftIndex + Fs * window) temp.Add(index);
                        }
                    }
                }
                else
                {
                    temp = new List<My_selectionMember>(input);
                }

                if (!(artificial == null || artificial.Count == 0))
                {
                    temp2 = new List<My_selectionMember>();
                    foreach (selectionMember a in artificial)
                    {
                        foreach (My_selectionMember index in temp)
                        {
                            // if (!(a.leftIndex < index.leftIndex && index.rightIndex < a.rightIndex)) temp2.Add(index);
                            if (index.rightIndex < a.leftIndex || index.leftIndex > a.rightIndex) temp2.Add(index);

                        }
                    }
                    return temp2;
                }
                return temp;
            }



            public List<List<My_selectionMember>> _getMy_selectionMemberInRange(List<My_selectionMember> input, List<selectionMember> start, int window, List<selectionMember> artificial)
            {
                List<List<My_selectionMember>> output = new List<List<My_selectionMember>>();
                List<List<My_selectionMember>> output2 = new List<List<My_selectionMember>>();
                List<My_selectionMember> temp;
                List<My_selectionMember> temp2;
                if (!(start == null || start.Count == 0))
                {

                    temp = new List<My_selectionMember>();
                    foreach (selectionMember s in start)
                    {
                        foreach (My_selectionMember index in input)
                        {
                            if (s.leftIndex < index.leftIndex && index.rightIndex < s.leftIndex + Fs * window) temp.Add(index);
                        }
                        output.Add(new List<My_selectionMember>(temp));
                        temp.Clear();
                    }
                }
                else
                {
                    output.Add(new List<My_selectionMember>(input));
                }

                if (!(artificial == null || artificial.Count == 0))
                {
                    temp2 = new List<My_selectionMember>();
                    foreach (List<My_selectionMember> list in output)
                    {
                        
                         foreach (My_selectionMember index in list)
                         {
                            bool isOK = true;
                            foreach (selectionMember a in artificial)
                            {
                                if (!(index.rightIndex < a.leftIndex || index.leftIndex > a.rightIndex)) isOK = false; 

                            }
                            if(isOK) temp2.Add(index);

                        }
                        output2.Add(new List<My_selectionMember>(temp2));
                        temp2.Clear();
                    }
                    return output2;
                }
                return output;
            }

            //STEJNE JAKO _getMy_selectionMemberInRange JEN PREDELANE HRANICE V IF ARTIFICIAL AREA, veyme jeste krani bod prekryvu
            public List<List<My_selectionMember>> _getMy_selectionMemberInRange2(List<My_selectionMember> input, List<selectionMember> start, int window, List<selectionMember> artificial)
            {
                List<List<My_selectionMember>> output = new List<List<My_selectionMember>>();
                List<List<My_selectionMember>> output2 = new List<List<My_selectionMember>>();
                List<My_selectionMember> temp;
                List<My_selectionMember> temp2;
                if (!(start == null || start.Count == 0))
                {

                    temp = new List<My_selectionMember>();
                    foreach (selectionMember s in start)
                    {
                        foreach (My_selectionMember index in input)
                        {
                            if (s.leftIndex < index.leftIndex && index.rightIndex < s.leftIndex + Fs * window) temp.Add(index);
                        }
                        output.Add(new List<My_selectionMember>(temp));
                        temp.Clear();
                    }
                }
                else
                {
                    output.Add(new List<My_selectionMember>(input));
                }

                if (!(artificial == null || artificial.Count == 0))
                {
                    temp2 = new List<My_selectionMember>();
                    foreach (List<My_selectionMember> list in output)
                    {

                        foreach (My_selectionMember index in list)
                        {
                            bool isOK = true;
                            foreach (selectionMember a in artificial)
                            {
                                if (!(index.leftIndex < a.leftIndex || index.leftIndex > a.rightIndex)) isOK = false;

                            }
                            if (isOK) temp2.Add(index);

                        }
                        output2.Add(new List<My_selectionMember>(temp2));
                        temp2.Clear();
                    }
                    return output2;
                }
                return output;
            }


            public void getExspirium(signalViewer.graphPanel lk, List<int> num)
            {
                Exspirium = new List<My_selectionMember>();
                int pocet = 0;

                for (int i = 0; i < Original.Count; i++)
                {
                    if (AnotationList[i].maximum == true)
                    {
                        var min = i;
                        int max;
                        while (i < Original.Count && AnotationList[i].minimum == false)
                        {
                            i++;
                        }
                        if (i == Original.Count) return;
                        max = i - 1;
                        var ampl = Math.Abs(Original[max] - Original[min]);

                        if (num == null || num.Count == 0)
                            Exspirium.Add(new My_selectionMember(min, max, "Exspirium " + Exspirium.Count.ToString(), lk, ampl));
                        else
                        {
                            if(pocet<num.Count)
                            Exspirium.Add(new My_selectionMember(min, max, "Exspirium " + num[pocet].ToString(), lk, ampl));
                            pocet++;
                        }
                    }
                }
            }

            public void getInspirium(signalViewer.graphPanel lk,List<int>num)
            {
                Inspirium = new List<My_selectionMember>();
                int pocet=0;
                for (int i = 0; i < Original.Count; i++)
                {
                    if (AnotationList[i].minimum == true)
                    {
                        var min = i;
                        int max;
                        while (i < Original.Count && AnotationList[i].maximum == false)
                        {
                            i++;
                        }
                        if (i == Original.Count) return;
                        max = i - 1;
                        var ampl = Math.Abs(Original[max] - Original[min]);
                        if (num==null || num.Count == 0)
                            Inspirium.Add(new My_selectionMember(min, max, "Inspirium " + Inspirium.Count.ToString(), lk, ampl));
                        else
                        {
                            Inspirium.Add(new My_selectionMember(min, max, "Inspirium " + num[pocet].ToString(), lk, ampl));
                            pocet++;
                        }
                    }
                }
            }

            public void setAnotationsArtificial(List<selectionMember> input)
            {
                if (input == null || input.Count == 0) return;

                for (int i = 0; i < input.Count; i++)
                {
                    for (int j = input[i].leftIndex; j <= input[i].rightIndex; j++)
                    {
                        AnotationList[j].artificial = true;
                    }
                }
            }

            public void resetAnotationArtificial()
            {
                for (int i = 0; i < Original.Count; i++)
                {

                    AnotationList[i].artificial = false;

                }
            }

            public void setAnotationsStart(List<selectionMember> input,int window)
            {
                if (input == null || input.Count == 0)
                {
                    nullAnotationStart();
                    return;
                }
               
                AnotationList.ForEach(i => { i.start = new bool(); });
                for (int i = 0; i < input.Count; i++)
                {
                    for (int j = input[i].leftIndex; j <= input[i].rightIndex + Fs * window; j++)
                    {
                        AnotationList[j].start = true;
                    }
                }
            }

            public void nullAnotationStart()
            {
                for (int i = 0; i < Original.Count; i++)
                {

                    AnotationList[i].start = null;

                }
            }

            public void Marks()
            {
                Marks_Valid = new List<float>(new float[Original.Count]);
                
                for (int i = 0; i < Original.Count; i++)
                {
                    if (AnotationList[i].start == null)
                        Marks_Valid[i] = ((AnotationList[i].maximum | AnotationList[i].minimum) & (!AnotationList[i].artificial)) ? 1 : 0;
                    else
                        Marks_Valid[i] = ((AnotationList[i].maximum | AnotationList[i].minimum) & (!AnotationList[i].artificial) & AnotationList[i].start.GetValueOrDefault()) ? 1 : 0;
                    if (AnotationList[i].artificial == true) Marks_Valid[i] = 2;
                }
            }

           public filter_FIR filter = new filter_FIR();

            public Signal(int fs, List<float> data, int FiltOrder, float fc)
            {
                Fs = fs;
                Original = new List<float>(data);
                new Thread(() =>
                {

                    Thread.CurrentThread.IsBackground = true;
                    Original_Filtered = computeFIR_LP(Original, 1000, 0.1f, Fs);


                }).Start();
               
                AnotationList = new List<Anotations>();

                for (int i = 0; i < Original.Count; i++)
                {
                    AnotationList.Add(new Anotations());
                }

                findMax();
                findMin();

            }

            public Signal(int fs, List<float> data)
            {
                Fs = fs;
                Original = new List<float>(data);

                AnotationList = new List<Anotations>();

                for (int i = 0; i < Original.Count; i++)
                {
                    AnotationList.Add(new Anotations());
                }

                findMax();
                findMin();
            }

           

            public Signal(int fs)
            {
                Fs = fs;
            }

            public void CopyAnotations(Signal input)
            {
                if (input == null || input.AnotationList==null) return;
                 
               
                for (int i=0;i<input.AnotationList.Count;i++)
                {
                    AnotationList[i].maximum =input.AnotationList[i].maximum;
                    AnotationList[i].minimum = input.AnotationList[i].minimum;
                }
                Original_Marks = new List<float>(input.Original_Marks);
            }


            public Signal GetRange(int start, int N)
            {
                
                Signal Output = new Signal(Fs);
                Output.Original = new List<float>(Original.GetRange(start, N));
                if (Original_Filtered != null)
                    Output.Original_Filtered = new List<float>(Original_Filtered.GetRange(start, N));
                Output.Original_Marks = new List<float>(Original_Marks.GetRange(start, N));

                if (Marks_Valid != null)
                    Output.Marks_Valid = new List<float>(Marks_Valid.GetRange(start, N));

                Output.AnotationList = new List<Anotations>(AnotationList.GetRange(start, N));
                
                return Output;
            }

            public void FitToPbx(int width)
            {
                int CuttFactor = Original.Count / width;

                if (CuttFactor >= 2)
                {
                    DataResolutinDecrease(CuttFactor);
                    MarksResolutinDecrease(CuttFactor);
                    
                }
                
            }
        




            public void DataResolutinDecrease(int CuttFactor)
            {
                Output = new List<float>(new float[Original.Count / CuttFactor]);
                
                int i = 0;

                if (Original_Filtered != null)
                {
                    Output_Filtered = new List<float>(new float[Original.Count / CuttFactor]);
                    Parallel.For(0, Original.Count / CuttFactor, (j) =>
                    {
                        try
                        {
                            float temp = Original.GetRange(j * CuttFactor, CuttFactor).Max();
                            Output[j] = temp;

                            temp = Original_Filtered.GetRange(j * CuttFactor, CuttFactor).Max();
                            Output_Filtered[j] = temp;


                        }
                        catch { }
                    });
                }
                else
                {
                    Output_Filtered = null;
                    Parallel.For(0, Original.Count / CuttFactor, (j) =>
                    {
                        try
                        {
                            float tempx = Original.GetRange(j * CuttFactor, CuttFactor).Max();
                            Output[j] = tempx;
                            

                        }
                        catch { }
                    });
                }
                
            }

            public void MarksResolutinDecrease(int CuttFactor)
            {
                Output_Marks = new List<float>(new float[Original.Count / CuttFactor]);
                int i = 0;

                Parallel.For(0, Original.Count / CuttFactor, (j) =>
                {
                    try
                    {
                        float temp = Original_Marks.GetRange(j * CuttFactor, CuttFactor).Max();
                        Output_Marks[j] = temp;

                    }
                    catch { }
                });

            }




            public void findMax()
            {
                AnotationList.ForEach(i => { i.maximum = false; });

                Original_Marks = new List<float>(new float[Original.Count]);
                for (int i = 2 * Fs; i < Original.Count - 2 * Fs; i++)
                {
                    try
                    {
                        object sync = new object();
                        bool isOK = true;
                        if (!(Original[i - 1] < Original[i] && Original[i] >= Original[i + 1])) continue;

                        //if (!(Original_Filtered[i - Fs / 2] < Original_Filtered[i] && Original_Filtered[i] > Original_Filtered[i + Fs / 2])) continue;


                        Parallel.For(2,2* Fs, (j, loopState) =>
                         {

                             if (!(Original[i - j] < Original[i] && Original[i] >= Original[i + j]))
                             {

                                 lock (sync)
                                 {
                                     isOK = false;

                                 }
                                 loopState.Break();

                             }
                         });

                        if (isOK)
                        {
                            AnotationList[i].maximum = true;
                            Original_Marks[i] = 1;
                            i += Fs;
                        }

                    }
                    catch (Exception e)
                    {
                    }

                }
            }



            public void findMax(List<float> input)
            {
                AnotationList.ForEach(i => { i.maximum = false; });

                Original_Marks = new List<float>(new float[input.Count]);
                object sync = new object();
                var Options = new ParallelOptions();

                // Keep one core/CPU free...
                Options.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

                for (int i = 2 * Fs; i < input.Count - 2 * Fs; i++)
                {
                    bool isOK = true;
                    //try
                    //{
                        
                        
                        if (!(input[i - 1] < input[i] && input[i] >= input[i + 1])) continue;

                       
                        Parallel.For(2,2*Fs, Options, (j, loopState) =>
                        {

                            if (!(input[i - j] < input[i] && input[i] >= input[i + j]))
                            {

                                lock (sync)
                                {
                                    isOK = false;
                                    loopState.Break();
                                }
                                

                            }
                        });

                        if (isOK)
                        {
                            AnotationList[i].maximum = true;
                            Original_Marks[i] = 1;
                            i +=Fs;
                    }

                    //}
                    //catch (Exception e)
                    //{
                    //}

                }
            }


            public void findMin()
            {
                AnotationList.ForEach(i => { i.minimum = false; });


                for (int i = 2 * Fs; i < Original.Count - 2 * Fs; i++)
                {
                    try
                    {
                        object sync = new object();
                        bool isOK = true;
                        if (!(Original[i - 1] > Original[i] && Original[i] <= Original[i + 1])) continue;


                        Parallel.For(2,2*  Fs, (j, loopState) =>
                        {

                            if (!(Original[i - j] > Original[i] && Original[i] <= Original[i + j]))
                            {

                                lock (sync)
                                {
                                    isOK = false;
                                    loopState.Break();
                                }

                            }
                        });

                        if (isOK)
                        {
                            AnotationList[i].minimum = true;
                            Original_Marks[i] = 2;
                            i +=  Fs;
                        }

                    }
                    catch (Exception e)
                    {
                    }

                }
                Marks();
            }

            public void findMin(List<float> input)
            {

                AnotationList.ForEach(i => { i.minimum = false; });
                object sync = new object();
                var Options = new ParallelOptions();

                // Keep one core/CPU free...
                Options.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

                for (int i = 2 * Fs; i < input.Count - 2 * Fs; i++)
                {
                    //try
                    //{
                        
                        bool isOK = true;
                        if (!(input[i - 1] > input[i] && input[i] <= input[i + 1])) continue;


                        Parallel.For(2,2* Fs,Options, (j, loopState) =>
                        {

                            if (!(input[i - j] > input[i] && input[i] <= input[i + j]))
                            {

                                lock (sync)
                                {
                                    isOK = false;
                                    loopState.Stop();
                                }

                            }
                        });

                        if (isOK)
                        {
                            AnotationList[i].minimum = true;
                            Original_Marks[i] = 2;
                            i += Fs;
                        }

                    //}
                    //catch (Exception e)
                    //{
                    //}

                }
                Marks();
            }

            WindowingFunction wf = new WindowingFunction(40, WindowingFunction.windowsType.Hamming, 0);
            public List<float> computeFIR_LP(List<float> input, int order, float cutOffFrequeny, float sampleFreq)
            {
                List<float> result = new List<float>(input.Count);

                int maximumOrder = 5000;

                if (order < 0 || order > maximumOrder) return null;

                filter = filter.generateLowPass(order, cutOffFrequeny, wf);

                result = filter.doFIR(input);

                return result;
            }





            public void MinMaxModify(List<selectionMember> insp, List<selectionMember> exsp,List<int> rename_marks_exsp, List<int> rename_marks_insp)
            {
                
                AnotationList.ForEach(i => { i.minimum = false; });
                AnotationList.ForEach(i => { i.maximum = false; });
                Original_Marks = new List<float>(new float[Original.Count]);
                rename_marks_exsp.Clear();
                rename_marks_insp.Clear();

                foreach (selectionMember e in exsp)
                {
                    AnotationList[e.rightIndex].minimum = true;
                    AnotationList[e.leftIndex].maximum = true;
                    var info = e.info.Split(' ');
                    var cislo = Int32.Parse(info[1]);
                    rename_marks_exsp.Add(cislo);
                    int index;
                    if (Int32.Parse(Regex.Match(insp[0].info, @"\d+").Value) > Int32.Parse(Regex.Match(exsp[0].info, @"\d+").Value))
                    {
                        index = insp.FindIndex(x => x.info.Contains("Inspirium " + (cislo+1).ToString()));
                    }
                    else
                    {
                        index = insp.FindIndex(x => x.info.Contains("Inspirium " + (cislo ).ToString()));
                    }
                    if (index == -1) { }
                    else
                    insp[index].leftIndex = e.rightIndex;
                }

                foreach (selectionMember i in insp)
                {
                    AnotationList[i.rightIndex+1].maximum = true;
                    AnotationList[i.leftIndex-1].minimum = true;
                    var info = i.info.Split(' ');
                    var cislo = Int32.Parse(info[1]);
                    rename_marks_insp.Add(cislo);
                }

            }

        }






        private void btMarks_Click(object sender, EventArgs e)
        {
            signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();
            sc.setFilter("");
            marksList = null;
            signal.resetAnotationArtificial();
            if (sc.ShowDialog() == DialogResult.OK)
            {
                marksList = sc.result;

            }

            signal.setAnotationsArtificial(marksList);
            signal.Marks();
            doExternalRefresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();
            sc.setFilter("");
            startmark = null;
            signal.nullAnotationStart();
            if (sc.ShowDialog() == DialogResult.OK)
            {


                //startmark =new List<selectionMember>(sc.result);
                startmark = new List<selectionMember>();
                startmark_copy = new List<selectionMember>();
                foreach (selectionMember x in sc.result)
                {
                    startmark.Add((selectionMember)x.Clone());
                    startmark[startmark.Count - 1].info = "StartMark" + (startmark.Count - 1).ToString();
                    startmark_copy.Add((selectionMember)x.Clone());
                    startmark_copy[startmark_copy.Count - 1].info = "StartMark" + (startmark_copy.Count - 1).ToString();
                }
            }

            if (startmark != null)
                foreach (selectionMember x in startmark)
                {
                    x.rightIndex += (int)numericUpDown3.Value * signal.Fs;
                    x.leftIndex += (int)numericUpDown3.Value * signal.Fs;
                }
            signal.setAnotationsStart(startmark, (int)window.Value);
            signal.Marks();
            doExternalRefresh();
        }

        //volani z CMD
        public void export_marks()
        {
            signal.getInspirium(lk,null);
            signal.getExspirium(lk,null);



            List<List<My_selectionMember>> results_Exspirium = signal._getMy_selectionMemberInRange(signal.Exspirium, startmark, (int)window.Value, marksList);
            List<List<My_selectionMember>> results_Inspirium = signal._getMy_selectionMemberInRange(signal.Inspirium, startmark, (int)window.Value, marksList);
            foreach (List<My_selectionMember> list in results_Exspirium)
            {
                graphPanel.selectionAreas.AddRange(list);
            }
            foreach (List<My_selectionMember> list in results_Inspirium)
            {
                graphPanel.selectionAreas.AddRange(list);
            }
            mv.refreshMarks();
        }

        private void bt_marks_Click(object sender, EventArgs e)
        {
            signal.getInspirium(lk,null);
            signal.getExspirium(lk,null);



            List<List<My_selectionMember>> results_Exspirium = signal._getMy_selectionMemberInRange(signal.Exspirium, startmark, (int)window.Value, marksList);
            List<List<My_selectionMember>> results_Inspirium = signal._getMy_selectionMemberInRange(signal.Inspirium, startmark, (int)window.Value, marksList);
            foreach (List<My_selectionMember> list in results_Exspirium)
            {
                graphPanel.selectionAreas.AddRange(list);
            }
            foreach (List<My_selectionMember> list in results_Inspirium)
            {
                graphPanel.selectionAreas.AddRange(list);
            }

            if (!(startmark == null || startmark.Count == 0))
            {
                graphPanel.selectionAreas.AddRange(startmark);
            }
           
            mv.refreshMarks();

            resultsPrint(results_Exspirium,dataGridView2);
            resultsPrint(results_Inspirium, dataGridView3);
            resultsStaisticsPrint(results_Inspirium,results_Exspirium, dataGridView1);
        }

         
        void resultsStaisticsPrint(List<List<My_selectionMember>> results_inspirium, List<List<My_selectionMember>> results_exspirium, DataGridView a)
        {
            if (CMD) return;
            a.ColumnCount = results_inspirium.Count;
            int i = 0;
            foreach (List<My_selectionMember> list in results_inspirium)
            {
                dataGridView1.Columns[i].HeaderCell.Value = "window: "+i.ToString();
                var x = list.Select(amp => amp.amplitude).ToList();
                var amplituda = new statistics(null, x, 0, 0, "").avg;
                a.Rows[0].Cells[i].Value =amplituda.ToString();
                a.Rows[1].Cells[i].Value = new statistics(null, x, 0, 0, "").dev.ToString();


                x = list.Select(amp => amp.dT/ mainView.sampleFrequency).ToList();
                a.Rows[6].Cells[i].Value = new statistics(null, x, 0, 0, "").avg.ToString();
                a.Rows[7].Cells[i].Value = new statistics(null, x, 0, 0, "").dev.ToString();


                var z = list.Select(amp => new { amp.leftIndex,amp.rightIndex }).ToList();
                var temp_freq = new List<float>();
                var temp_period = new List<float>();
                for (int g=1;g<z.Count;g++)
                {
                    temp_period.Add((z[g].leftIndex - z[g - 1].leftIndex) / mainView.sampleFrequency);
                    temp_freq.Add(60 / ((z[g].leftIndex - z[g - 1].leftIndex) / mainView.sampleFrequency));
                }
                var frekvence = new statistics(null, temp_freq, 0, 0, "").avg;
                a.Rows[4].Cells[i].Value = frekvence.ToString();
                a.Rows[5].Cells[i].Value = new statistics(null, temp_freq, 0, 0, "").dev.ToString();
                a.Rows[2].Cells[i].Value = new statistics(null, temp_period, 0, 0, "").avg.ToString();
                a.Rows[3].Cells[i].Value = new statistics(null, temp_period, 0, 0, "").dev.ToString();
                a.Rows[10].Cells[i].Value = amplituda *frekvence;
                i++;
            }
            i = 0;
            foreach (List<My_selectionMember> list in results_exspirium)
            {

                var x = list.Select(amp => amp.dT / mainView.sampleFrequency).ToList();
                a.Rows[8].Cells[i].Value = new statistics(null, x, 0, 0, "").avg.ToString();
                a.Rows[9].Cells[i].Value = new statistics(null, x, 0, 0, "").dev.ToString();

                i++;
            }
        }



        void resultsPrint(List<List<My_selectionMember>> results,DataGridView a)
        {
            if (CMD) return;
            int count = 0;
            foreach (List<My_selectionMember> list in results)
            {
                count += list.Count()+1;
            }
            a.RowCount = count;
            int i = 0;
            foreach(List<My_selectionMember> list in results)
            {
                foreach (My_selectionMember mark in list)
                {
                    float temp;
                    
                    a.Rows[i].Cells[0].Value = mark.info;
                    a.Rows[i].Cells[1].Value = mark.amplitude.ToString();
                    a.Rows[i].Cells[2].Value = (mark.dT / mainView.sampleFrequency).ToString();
                    a.Rows[i].Cells[3].Value = (mark.leftIndex / mainView.sampleFrequency).ToString();
                    a.Rows[i].Cells[4].Value = (mark.rightIndex / mainView.sampleFrequency).ToString();
                    i++;
                }
                a.Rows[i].Cells[0].Value = "---";
                a.Rows[i].Cells[1].Value = "---";
                a.Rows[i].Cells[2].Value = "---";
                a.Rows[i].Cells[3].Value = "---";
                a.Rows[i].Cells[4].Value = "---";
                
                i++;
            }
        }

        private void window_ValueChanged(object sender, EventArgs e)
        {
            signal.setAnotationsStart(startmark, (int)window.Value);
            signal.Marks();
            doExternalRefresh();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (filter_CMD) return;
            recomputeFilter();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (filter_CMD) return;
            recomputeFilter();
        }

        public void recomputeFilter(bool cmd)
        {
            signal.Original_Filtered = null;



            signal.Original_Filtered = signal.computeFIR_LP(signal.Original, (int)50, (float)10, mainView.sampleFrequency);

        }
        public void recomputeFilter()
        {
            signal.Original_Filtered = null;
            

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                timer.Start();
                signal.Original_Filtered = signal.computeFIR_LP(signal.Original, (int)numericUpDown2.Value, (float)numericUpDown1.Value, mainView.sampleFrequency);

                Stopwatch watch = new Stopwatch();
                watch.Start();
                signal.findMax(signal.Original_Filtered);
                signal.findMin(signal.Original_Filtered);
                watch.Stop();
                var x = watch.Elapsed.TotalSeconds.ToString();

                timer.Stop();
                this.Invoke((MethodInvoker)delegate
                {
                    doExternalRefresh();
                    progressBar1.Value = 0;
                    filter_FIR.samplesProcessed = 0;
                    
                });
                
            }).Start();
            doExternalRefresh();
        }



        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
            double progress = filter_FIR.samplesProcessed / (double)signal.Original.Count * 100;

            try
            {

                if (progress >= 100)
                {
                    progress = 100;
                }
                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Value = (int)progress;
                    
                });



            }
            catch (Exception)
            { }

        }

     
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            
            if (checkBox1.Checked == true)
            {
                
                recomputeFilter();

            }
            else
            {
                signal.Original_Filtered = null;
                signal.CopyAnotations(signal_copy);
                signal.Marks();
            }
                // signal.Marks();
                
                doExternalRefresh();

            refrControls();
        }

        private void pbx_Click(object sender, EventArgs e)
        {

        }

        private void bt_signal_Click(object sender, EventArgs e)
        {
            export_signal();
        }
        public void export_signal()
        {
            if (signal.Original_Filtered != null)
            {
                string name = "FIR-" + numericUpDown2.Value.ToString() + " LP-" + numericUpDown1.Value.ToString() + " Hz";
                lk.dataCache.Add(new signalViewer.dataCacheLevel(name, signal.Original_Filtered));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap locBMP = new Bitmap(pbx.Width, pbx.Height);

            pbx.DrawToBitmap(locBMP, pbx.ClientRectangle);

            Clipboard.SetImage(locBMP);
        }

        private void btProcess_Click(object sender, EventArgs e)
        {
            signal.getInspirium(lk,null);
            signal.getExspirium(lk,null);



            List<List<My_selectionMember>> results_Exspirium = signal._getMy_selectionMemberInRange(signal.Exspirium, startmark, (int)window.Value, marksList);
            List<List<My_selectionMember>> results_Inspirium = signal._getMy_selectionMemberInRange(signal.Inspirium, startmark, (int)window.Value, marksList);
        
            resultsPrint(results_Exspirium, dataGridView2);
            resultsPrint(results_Inspirium, dataGridView3);
            resultsStaisticsPrint(results_Inspirium, results_Exspirium, dataGridView1);
            tabControl1.SelectedTab = tabPage3;
        }

        private void bt_freq_Click(object sender, EventArgs e)
        {
            export_BPMfunction();

        }

        public void export_BPMfunction()
        {
            signal.getInspirium(lk, null);
            signal.getExspirium(lk, null);


            List<List<My_selectionMember>> results_Exspirium = signal._getMy_selectionMemberInRange2(signal.Exspirium, startmark, (int)window.Value, marksList);
            List<float> output = new List<float>(new float[signal.Original.Count]);


            foreach (List<My_selectionMember> list in results_Exspirium)
            {
                var temp = new List<float>();

                var z = list.Select(amp => new { amp.leftIndex, amp.info }).ToList();

                for (int g = 1; g < z.Count; g++)
                {
                    var resultString1 = Int32.Parse(Regex.Match(z[g].info, @"\d+").Value);
                    var resultString2 = Int32.Parse(Regex.Match(z[g - 1].info, @"\d+").Value);
                    if (resultString1 != resultString2 + 1) continue;
                    temp.Add(60 / ((z[g].leftIndex - z[g - 1].leftIndex) / mainView.sampleFrequency));

                    for (int p = z[g - 1].leftIndex; p < z[g].leftIndex; p++)
                    {
                        output[p] = temp[temp.Count - 1];
                    }
                }

            }



            graphPanel gp = new graphPanel("Respiration_frequency_function", output, mv);



            mv.addNewChannel(gp, 0, true);

            mv.rebuiltAndRedrawAll();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (startmark != null)
            {
                startmark = new List<selectionMember>();
                foreach (selectionMember x in startmark_copy)
                {
                    startmark.Add((selectionMember)x.Clone());
                }

                foreach (selectionMember x in startmark)
                {
                    x.rightIndex += (int)numericUpDown3.Value * signal.Fs;
                    x.leftIndex += (int)numericUpDown3.Value * signal.Fs;
                }
                signal.setAnotationsStart(startmark, (int)window.Value);
                signal.Marks();
                doExternalRefresh();
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();
            sc.setFilter("Inspi");
            List<selectionMember> LoadedmMarks_insp =new List<selectionMember>(sc.result);
            sc.setFilter("Exspi");
            List<selectionMember> LoadedmMarks_exsp = new List<selectionMember>(sc.result);
            signal.MinMaxModify(LoadedmMarks_insp, LoadedmMarks_exsp,rename_marks_exsp, rename_marks_insp);
            signal.Marks();
            doExternalRefresh();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                button3.Enabled = true;
                bt_marks.Visible = false;
                signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();
                sc.setFilter("Inspi");
                List<selectionMember> LoadedmMarks_insp = new List<selectionMember>(sc.result);
                sc.setFilter("Exspi");
                List<selectionMember> LoadedmMarks_exsp = new List<selectionMember>(sc.result);
                signal.MinMaxModify(LoadedmMarks_insp, LoadedmMarks_exsp, rename_marks_exsp, rename_marks_insp);
                signal.Marks();
                doExternalRefresh();


            }
            else
            {
                button3.Enabled = false;
                signal.Original_Filtered = null;
                signal.CopyAnotations(signal_copy);
                signal.Marks();
            }
            doExternalRefresh();

            refrControls();
        }
    }

 
}
