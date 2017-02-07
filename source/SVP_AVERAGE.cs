using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using signalViewer;

namespace plugins_package_1
{
    public partial class SVP_avgTool : UserControl
    {
        List<signalViewer.graphPanel> linkedChannels = new List<signalViewer.graphPanel>();

        float[] averagingWindow = new float[0];

        public int previewSize = 0;

        public int avgRad = 10;

       // public Bitmap rb = new Bitmap(10,10);

        public int nr = 0;

        public int ld = 0; //--poslední dataindex pro pohled

        public int windowType = 0;

        //public string[] windowNames = new string[3]{"RECT","TRI","NUTT"};

      //  Task workTask;

        signalViewer.WindowingFunction wf = new signalViewer.WindowingFunction(1, 0, 0);

         mainView mv;

        ManualResetEvent COMRE = new ManualResetEvent(false);

        //-----help file arguments ----
        public string helpNamedDest = "SignalPlant.indd:Averaging:297"; //---named destination in the help file
        public string helpFile = "SignalPlant.pdf"; //--name of help file

        bool doItAgain = false;

        float[] source = null;
        float[] results = null;

        public string getDeveloperInfo()
        {
            return "Filip Plešinger, UPT, AVČR, 2013";
        }

        public string getDescription()
        {
            return "Vyhlazení pomocí průměrování. Syntaxe : automark(bool up;double value; bool deleteExisting) pro jednoduchou značku nebo automark(bool startUP; double value; bool endUP; double value; bool deleteExisting)";
        }

        public string getCategory()
        {
            return "Uncompleted";
        }

        public string getName()
        {
            return "Avg";
        }


        public SVP_avgTool()
        {
            InitializeComponent();
            refrControls();
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            string s = e.Data.GetData(DataFormats.Text).ToString();

            if (mv.getGPbyName(s) != null)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            string s = e.Data.GetData(DataFormats.Text).ToString();

            

            try
            {

                graphPanel gp = mv.getGPbyName(s);

                if (gp == null || linkedChannels.IndexOf(gp) > -1) return;

                this.linkedChannels.Add(gp);
                refrControls();
            }
            catch
            {
                MessageBox.Show("Error in drag&drop");
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            signalViewer.selectChannelForm sc = new signalViewer.selectChannelForm();

            sc.regenerateList(linkedChannels);



            if (sc.ShowDialog() == DialogResult.OK)
            {
                this.linkedChannels.Clear();

                for (int i = 0; i < sc.lv.SelectedItems.Count; i++)
                {
                    this.linkedChannels.Add(mv.getGPbyName(sc.lv.SelectedItems[i].Text));
                }
                refrControls();

              

            }
        }

      
        public void presetBeforeLoaded()
        {
      //      movingMid = true;
       //     movingP1 = true;
        //    movingP2 = true;
        }


        public void presetLoaded()
        {


            for (int i = 0; i < signalViewer.WindowingFunction.windowNames.Length; i++)
            {
                if (signalViewer.WindowingFunction.windowNames[i].Equals(button3.Text))
                {
                    wf.winType = i;
                    break;
                }
            }

            avgRad = nud.Value;
      
        }

        public void refrControls()
        {
            if (this.Parent == null) return;

            button1.Text = "";
            if (linkedChannels.Count == 0) button1.Text = "Drag a channel here or click";
            if (linkedChannels.Count == 1) button1.Text = "Channel : " + linkedChannels[0].channelName;
            if (linkedChannels.Count > 1) button1.Text = linkedChannels.Count.ToString() + " channels";

            if (bgw.IsBusy || linkedChannels.Count==0) button2.Enabled = false;
            else button2.Enabled = true;

            nud1.Value = avgRad;

         //   lr.Text = "Window radius: " + avgRad.ToString();

            doPreview();

            //this.Refresh();
        }


        public void doExternalRefresh()
        {
            doPreview();
        }

        private void avgTool_Paint(object sender, PaintEventArgs e)
        {
            if (ld != signalViewer.graphPanel.traceDataIndex)
            {
                doPreview();
                //pprev.
            }
        }
       /*
        private void generateWindow()
        {
            List<float> avgTemp = new List<float>();
            
            //averagingWindow.Clear();

            int n=0;

            int N = 2*avgRad+1;

            for (int step = -avgRad; step <= avgRad; step++)
            {

                n++;

                double w = 1;

                switch(windowType)
                {
                    case 1: w = 1 - Math.Abs((n - (N - 1) / 2) / ((N+1) / 2)); break;
                    case 2: w = 0.355768 - 0.487396 * Math.Cos((2 * Math.PI * n) / (N - 1)) + 0.144232 * Math.Cos((4 * Math.PI * n) / (N - 1)) - 0.012604 * Math.Cos((6 * Math.PI * n) / (N - 1)); break;
                        
                }
                avgTemp.Add((float)w);
            }

            averagingWindow = avgTemp.ToArray();
        }
       */
        private void doPreview()
        {
            if (bgwPre.IsBusy)
            {
                doItAgain = true;
                return;
            }
            else
                bgwPre.RunWorkerAsync();
        }


        public string COMMAND_AVG(string parameters)
        {
            // dekompozice

            try
            {

                string[] pars = parameters.Split(' ');

                if (pars.Length < 2) throw(new Exception("Include some parameters"));

                //--procházení parametrů a nastavení podmínek
                for (int i = 1; i < pars.Length; i++)
                {
                    string msg = pars[i];
                    


                    if (msg.Length>10 && msg.Substring(0,8).Equals("CHANNEL("))
                    {

                        string filtr = msg.Substring(8, msg.Length - 9);

                        for (int q = 0; q < mv.gpList.Count; q++)
                        {
                            if (signalViewer.mainView.respectsFilter(mv.gpList[q].channelName, filtr))
                                linkedChannels.Add(mv.gpList[q]);
                        }
                    }

                    if (msg.Length>9 && msg.Substring(0, 7).Equals("WINDOW("))
                    {
                        string[] winpars = msg.Substring(7, msg.Length - 8).Split(';');
                        if (winpars.Length < 2 || winpars.Length > 3) throw (new Exception("Bad number of windows parameters. Should be 2 or 3 : length, type [,param]."));


                        int pL = Convert.ToInt16(winpars[0]);
                        int pT = Convert.ToInt16(winpars[1]);
                        
                        float pP = 0;

                        if (winpars.Length==3) 
                            pP = Convert.ToSingle(winpars[2]);

                        wf = new signalViewer.WindowingFunction(2*pL+1, pT, pP);

                        //wf = new signalViewer.WindowingFunction(2 * avgRad + 1, wf.winType, wf.winParam);
                        averagingWindow = wf.window.ToArray();

                    }

                }

                bgw.RunWorkerAsync();
                COMRE.WaitOne();


                int r = 0;
            }
            catch (Exception e)
            {
                return "Error:" + e.Message;
            }


            return ("Completed Succesfully");

        }

        public static string CMDDESCRIPTION_AVG()
        {
            return "Averaging. Use : AVG CHANNEL(V1,V2,V3) WINDOW(LENGTH,TYPE,PARAM)";
        }



        private void doWork()
        {

            button2.Enabled = false;

            try
            {
                //if (!chint.Checked) return;
                if (linkedChannels.Count < 1)
                {
                    //MessageBox.Show("Je potřeba zadat nějaké kanály k filtrování, sakryš!");
                    return;
                }

                //generateWindow();

                if (averagingWindow.Length < 1) return;

                if (!bgw.IsBusy)
                bgw.RunWorkerAsync();

            }

            catch
            {
               // MessageBox.Show("Někde tu je chyba.");
            }

            finally
            {
                


            }
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {

            avgRad = (int)nud.Value;

            doPreview();
                
        }

        private void pprev_Paint(object sender, PaintEventArgs e)
        {
            

        }

        private void nud_Scroll(object sender, EventArgs e)
        {
            avgRad = nud.Value;

          //  lr.Text = "Window radius: " + avgRad.ToString();

            nud1.Value = avgRad;

            doPreview();

        }


        private void button2_Click(object sender, EventArgs e)
        {
            UseWaitCursor = true;
            button2.Enabled = false;
            doWork();
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {

            if (linkedChannels.Count == 0)
            {
                COMRE.Set();
                return;
            }

            float samplesToCompute = linkedChannels.Count * linkedChannels[0].dataCache[0].data.Count;
            float samplesComputed = 0;
            float percentDone = 0;

            Parallel.For(0, linkedChannels.Count, i =>
             {
                 signalViewer.graphPanel lk = linkedChannels[i];
                 {
                     // graphPanel.ri
                     float[] results = new float[lk.dataCache[lk.currentDataChace].data.Count];
                     float[] source = lk.dataCache[lk.currentDataChace].data.ToArray();
                     float[] nAVG = new float[wf.window.Count];

                     //lk.dataCache[lk.currentDataChace].data.CopyTo()
                     avgRad = (wf.window.Count - 1) / 2;

                     for (int d = 0; d < lk.dataCache[lk.currentDataChace].data.Count; d++)
                     {
                         for (int step = -avgRad; step <= avgRad; step++)
                         {
                             int dataIndex = d + step;
                             if (dataIndex < 0) dataIndex = 0;
                             if (dataIndex >= source.Length) dataIndex = source.Length - 1;
                             int avs = step + avgRad;

                             nAVG[avs] = source[dataIndex] * averagingWindow[avs];
                         }
                         results[d] = nAVG.Sum() / averagingWindow.Sum();

                         samplesComputed++;

                         percentDone = ((samplesComputed / samplesToCompute) * 100);

                         if (percentDone % 5 == 0)
                             bgw.ReportProgress((int)percentDone);

                     }

                     int cnt = (wf.window.Count - 1) / 2;

                     string str = "AVG WINDOW(" + cnt + ";" + signalViewer.WindowingFunction.windowNames[wf.winType] + "(" + wf.winType + ")";

                     if (signalViewer.WindowingFunction.numParams[wf.winType] > 0)
                         str += ";(" + wf.winParam.ToString() + ")";

                     lk.dataCache.Add(new signalViewer.dataCacheLevel(str, results.ToList<float>()));
                     lk.currentDataChace = lk.dataCache.Count - 1;
                 }
             });
            COMRE.Set();
        }

        private void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbar.Value = e.ProgressPercentage;
        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            COMRE.Set();

            mv.rebuiltAndRedrawAll();
            signalViewer.mainView.actualizePluginForms();

            if (this.Parent == null)
            {
               
            }
            else
            {
                UseWaitCursor = false;
                this.ParentForm.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            signalViewer.windowFunc wfd = new signalViewer.windowFunc();

            wfd.wf.winType = wf.winType;
            wfd.wf.winParam = wf.winParam;

            

            wfd.Location = new Point(button3.Left+this.ParentForm.Location.X, button3.Top+this.ParentForm.Location.Y);

            if (wfd.ShowDialog() == DialogResult.OK)
            {
                string str = signalViewer.WindowingFunction.windowNames[wfd.wf.winType];

                if (signalViewer.WindowingFunction.numParams[wfd.wf.winType] > 0)
                {
                    str += " (" + wfd.wf.winParam.ToString() + ")";
                }

                button3.Text = str; 

                wf.winType = wfd.wf.winType;
                wf.winParam = wfd.wf.winParam;
            }

            doPreview();

        }

        private void SVP_avgTool_Load(object sender, EventArgs e)
        {
            mv = (mainView)Application.OpenForms[0];

            button3.Text = signalViewer.WindowingFunction.windowNames[0];
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            nr++;
            if (results == null || source==null) return;


            PointF transY = mainView.computeScaleandBaseFromMinMax(source.Min(), source.Max(), pbx.Height);
            PointF transX = mainView.computeScaleandBaseFromMinMax(0, results.Length, pbx.Width);


            float yo = 0;
            float xo = 0;

            Pen resPen = new Pen(Color.Black, 1);
            Pen srcPen = new Pen(Color.Silver, 1);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            for (int i = 0; i < source.Length; i++)
            {
                float y = source[i] * transY.X + transY.Y;
                float x = pbx.Width-i * transX.X - transX.Y;

                if (i > 0)
                    e.Graphics.DrawLine(Pens.Gray, x, y, xo, yo);
                xo = x;
                yo = y;
            }


            yo = 0;
            xo = 0;

            for (int i=0;i<results.Length;i++)
            {
                float y = results[i] * transY.X + transY.Y;
                float x = pbx.Width-i * transX.X - transX.Y;

                if (i > 0)
                    e.Graphics.DrawLine(resPen, x, y, xo, yo);
                xo = x;
                yo = y;
            }




            if (linkedChannels != null && linkedChannels.Count > 0)
            {
                int q = linkedChannels.Count - 1;

                string ret = linkedChannels[q].channelName + ":" + linkedChannels[q].dataCache[linkedChannels[q].currentDataChace].name;
                e.Graphics.DrawString(ret, SystemFonts.DefaultFont, Brushes.Black, 4, 4);
            }
        }

        private void bgwPre_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (linkedChannels.Count < 1) return;

                previewSize = pbx.Width;

                if (previewSize <= 10) return;

                wf = new signalViewer.WindowingFunction(2 * avgRad + 1, wf.winType, wf.winParam);
                averagingWindow = wf.window.ToArray();

                if (averagingWindow.Length < 1) return;

                ld = graphPanel.leftI;
                int rd = graphPanel.rightI;
                int cnt = rd - ld;

                if (rd > ld + 5000)
                    rd = ld + 5000; //safe limit for quick preview

                results = new float[cnt];
                source = new float[cnt];

                int q = 0;

                linkedChannels[q].dataCache[linkedChannels[q].currentDataChace].data.CopyTo(ld, source, 0, cnt);
         
                float[] nAVG = new float[avgRad * 2+1];

                for (int d = 0; d < cnt; d++)
                {
                    int cstart = ld + d - avgRad;
                    int cend = cstart + 2 * avgRad;
                   
                    if (cstart>=0 && cend< linkedChannels[q].dataCache[linkedChannels[q].currentDataChace].data.Count)
                    linkedChannels[q].dataCache[linkedChannels[q].currentDataChace].data.CopyTo(cstart, nAVG, 0, nAVG.Length);

                    for (int i=0;i<nAVG.Length;i++)
                    {
                        nAVG[i] *= averagingWindow[i];
                    }

                    results[d] = nAVG.Sum() / averagingWindow.Sum();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error:"+ex.Message);
            }

          
        }

        private void bgwPre_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (doItAgain)
            {
                doItAgain = false;
                bgwPre.RunWorkerAsync();
            }
            else
             pbx.Refresh();

        }

        private void pbx_Resize(object sender, EventArgs e)
        {
            pbx.Refresh();
        }

        private void nud1_ValueChanged(object sender, EventArgs e)
        {
            if (this.Parent == null)
                return;

            if (nud1.Value > nud.Maximum)
                nud.Maximum = (int) nud1.Value;

            if (nud.Value!=nud1.Value)
            {
                nud.Value = (int) nud1.Value;
                avgRad = nud.Value;
                doPreview();
            }
        }

        private void nud1_KeyPress(object sender, KeyPressEventArgs e)
        {
        

        }

        private void nud1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                doPreview();
        }
    }
}
