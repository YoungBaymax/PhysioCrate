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
using signalViewer.MathUtils;

namespace plugins2
{
    public partial class SVP_HFO : UserControl
    {
        List<signalViewer.graphPanel> linkedChannels = new List<signalViewer.graphPanel>(); //--array of linked channels

        string marksFilter = "";
        List<selectionMember> marksList; //---array of linked marks


        ManualResetEvent COMRE = new ManualResetEvent(false); //---manual reset event. Important when this plugin generates a command too. Otherwise it can be deleted

        float bgwPercentDone = 0;

        mainView mv;


        Signal result;
        //PointF[] pointsResult;
        Signal origSignal;
        //PointF[] pointsOrig;
        Signal averagedResult;
        //PointF[] pointsAvResult;

        int viewL;
        int viewR;
        int binPtsCount;

        signalViewer.WindowingFunction pluginWindowFunction = new WindowingFunction(0, 0, 0);
        signalViewer.WindowingFunction pluginWindowFunctionLP = new WindowingFunction(0, 0, 0);

        float tresholdValue;
        bool tresholdValueChangingAllowed;
        bool drawTreshold;
        float ptsTreshold;

        bool mouseOverTreshold;
        bool tresholdMovingAllowed;

        bool drawViewMarks;
        List<selectionMember> marksPositions;
        bool marksComputed;

        List<selectionMember> StartMarks;
        List<selectionMember> InvStartMarks;
        string currentStartMarkFilter;
        List<selectionMember> ArtMarks;
        string currentArtMarkFilter;
        bool iMarksRecompRequired = false;

        int numOfMarksGenerated;
        int numOfMarksFiltered;

        bool computeAll;

        bool fftRecompRequired = true;
        bool sigPtsRecompRequired = true;

        public System.Timers.Timer timer2 = new System.Timers.Timer();
        public System.Timers.Timer timerPaint = new System.Timers.Timer();

        float deleteEdgesValue;

        bool loadingParametersActive;

        Signal[] signal_list = new Signal[3];

        bool presetLoading;

        int exported = 0;

        int exportedMarkCount = 2;



        public string getDeveloperInfo()
        {
            return "Ján Virgala, ISI CAS, 2016"; //----change to your name + credentials
        }

        public string getDescription()
        {
            return "EMG Analysis Plugin"; //----enter description for your plugin
        }

        public string getCategory()
        {
            return "Analysis"; //---set category in plugins menu. If it does not exists, new category will be created
        }


        public string getName()
        {
            return "EMG [PhysioCrate]";        //---plugin name, visible in Plugins menu
        }

        public void doExternalRefresh()
        {
            /*
             *  This is calle from signalViewer, when doing RefreshAllPluginForms()
             */

            if ((viewR - viewL > 50000 && rdbViewArea.Checked) || (!rdbViewArea.Checked && fftRecompRequired))
                lblComputing.Visible = true;

            timerPaint.Start();
            timerPaint2.Start();

            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync();
            
        }       

        public void presetBeforeLoaded()
        {
            /*
          This funciotn is called after user clicks on preset in menu, but before corresponding controls are filled with new values. 
           */
            presetLoading = true;
        }

        public void presetLoaded()
        {
            /*
             * This funciotn is called after user clicks on preset in menu. Corresponding controls (with filled TAG field) receives values from preset
             * and here you have to set values from controls to corresponding fields
            */

            int index = signalViewer.WindowingFunction.windowNames.ToList().IndexOf(btnWindow.Text);
            if (index < 0)
                index = 0;

            pluginWindowFunction.winType = index;

            presetLoading = false;

            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync();

            refrControls();
        }


        //   /*

        //--When you need to have a command for scripts too you have to implement these two functions

        public string COMMAND_NAME(string parameters) //---change to COMMAND_SOMETHING if you need to name new command as "SOMETHING".
        {
            string pars = parameters.Substring(10);

            linkedChannels.Clear();



            string linkedChannelsFilter = "*";

            if (pars.IndexOf("CHANNEL(") > 0)
            {

                int posAbbrev = pars.IndexOf(")");

                int start = pars.IndexOf("CHANNEL(") + 8;

                string sub1 = pars.Substring(start, posAbbrev - start + 1);

                linkedChannelsFilter = sub1.Substring(0, sub1.Length - 1);
            }



            mainView.footerMessage = "ShapeFinder started";


            bgw.RunWorkerAsync();

            COMRE.WaitOne();

            linkedChannels = null;
            GC.Collect();
            return "Done";
        }

        public static string CMDDESCRIPTION_NAME() //---change NAME to same as in the funcion before
        {
            return "FIND ANOTATIONS FOR CHALLENGE 2014";
        }

        //  */

        private void setMV()
        {
            if (mv == null) mv = mainView.returnMV();
        }

        private void refrControls()
        {

            /*
             * This function is called for refreshing a plugin form. 
             
             * */

            sigPtsRecompRequired = true;

            setMV();

            btChannels.Text = "";
            if (linkedChannels.Count == 0) btChannels.Text = "Drag a channel here or click";
            if (linkedChannels.Count == 1) btChannels.Text = "Channel : " + linkedChannels[0].channelName;
            if (linkedChannels.Count > 1) btChannels.Text = linkedChannels.Count.ToString() + " channels are choosed";

            btProcess.Enabled = linkedChannels.Count > 0;

            if (bgw.IsBusy && bgw.CancellationPending) btProcess.Text = "Wait for cancellation";
            if (bgw.IsBusy && !bgw.CancellationPending) btProcess.Text = "Cancel";
            if (!bgw.IsBusy) btProcess.Text = "Process";

            if (chbArtMarks.Checked)
                if (bgwMarkPos.IsBusy)
                    bgwMarkPos.CancelAsync();
                else
                    bgwMarkPos.RunWorkerAsync();

            if (!bgw.IsBusy && !bgwMarkPos.IsBusy || result.values == null)
                pbx.Refresh();

        }

        private void doPreviewWork()
        {
            /*This function creates anything for preview. Not neccesary to implement */
        }

        private void btChannels_DragDrop(object sender, DragEventArgs e)
        {
            string s = e.Data.GetData(DataFormats.Text).ToString();

            try
            {
                linkedChannels.Add(mv.getGPbyName(s));
                refrControls();
                doPreviewWork();
            }
            catch (Exception exp)
            {
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

        public SVP_HFO()
        {
            InitializeComponent();

            binPtsCount = 4;

            signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();
            sc.setFilter(marksFilter);
            marksList = sc.result;

            result = new Signal((float)0.62, (float)0.35, true);
            origSignal = new Signal((float)0.2, (float)0.85, true);
            averagedResult = new Signal((float)0.62, (float)0.35, false);

            Signal[] signal_list = new Signal[3];
            signal_list[0] = origSignal;
            signal_list[1] = result;
            signal_list[2] = averagedResult;

            cbxSignal.SelectedIndex = 0;

            nudHF.Value = 550;
            nudLF.Value = 200;

            tresholdValueChangingAllowed = true;

            drawViewMarks = false;
            marksComputed = false;

            timer2.Elapsed += timer2_Elapsed;
            timer2.Interval = 10;

            drawViewMarks = false;

            currentStartMarkFilter = "start";
            currentArtMarkFilter = "art";

            StartMarks = new List<selectionMember>();
            ArtMarks = new List<selectionMember>();

            fftRecompRequired = true;

            rdbViewArea.Checked = true;

            refrControls();
        }

        private void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            bgwPercentDone = e.ProgressPercentage;
            pbx.Refresh();
        }


        private PointF getFactorXandY(Signal signal)
        {
            float[] values;

            if (rdbViewArea.Checked)
                values = signal.values;
            else
                values = signal.viewSignal;


            PointF factor = new PointF();

            factor.X = (float)(pbx.Width / (float)(values.Length));
            factor.Y = (float)(signal.Scale_Y * pbx.Height) / (float)(values.Min() - values.Max());

            float a = values.Min();
            float b = values.Max();

            if (Single.IsNaN(factor.Y) || Single.IsInfinity(factor.Y))
            {
                factor.Y = 1;
            }

            return factor;
        }


        private float getAdditionToSignal(Signal signal)
        {
            float[] values;

            if (rdbViewArea.Checked)
                values = signal.values;
            else
                values = signal.viewSignal;


            float addition = ((values.Max() - values.Min()) / 2 * signal.FactorXandY.Y) + values.Min() * signal.FactorXandY.Y - (pbx.Height * signal.Vertical_position);
            return addition;
        }

        private PointF[] ComputePointsF(Signal signal)
        {
            float[] signalValues;

            PointF[] pointsSignal = new PointF[viewR - viewL];

           // if (signal != averagedResult)
           // {
                signal.FactorXandY = getFactorXandY(signal);
                signal.Addition_to_signal = getAdditionToSignal(signal);
           // }

            if (rdbViewArea.Checked)
                signalValues = signal.values;
            else
                signalValues = signal.viewSignal;

            if (signal.FactorXandY.X == 0)
            {
                signal.FactorXandY = getFactorXandY(signal);
                signal.Addition_to_signal = getAdditionToSignal(signal);
            }

            if ((double)signalValues.Length / (double)pbx.Width < binPtsCount)
            {
                pointsSignal = new PointF[viewR - viewL];

                int x = 0;
                do
                {
                    pointsSignal[x].X = x * signal.FactorXandY.X;
                    pointsSignal[x].Y = signalValues[x] * signal.FactorXandY.Y - signal.Addition_to_signal;
                    x++;
                } while (x < pointsSignal.Length && x < signalValues.Length);

                if (drawTreshold)
                    ptsTreshold = tresholdValue * result.FactorXandY.Y - result.Addition_to_signal;
            }
            else
            {
                pointsSignal = new PointF[pbx.Width * 2];

                if (rdbViewArea.Checked)
                {//netrebaaa
                    signalValues = new float[signal.values.Length];
                    signalValues = signal.values;
                }
                else
                {
                    signalValues = new float[signal.viewSignal.Length];
                    signalValues = signal.viewSignal;
                }

                double pointsInPix = (double)signalValues.Length / (double)pbx.Width;
                double nums = (int)Math.Floor((double)signalValues.Length / (double)pbx.Width);
                double decs = pointsInPix - nums;

                int k = 0;

                int iEnd = 0;
                int iStart = 0;
                double decIndex = 0;


                if (pointsInPix < 1)
                    return pointsSignal;

                while (k < pbx.Width)
                {
                    decIndex = (k + 1) * nums + (k + 1) * decs;

                    iEnd = (int)Math.Floor(decIndex);

                    float[] part = new float[iEnd - iStart];
                    Array.Copy(signalValues, iStart, part, 0, part.Length);

                    pointsSignal[k].X = k;
                    pointsSignal[k].Y = part.Max() * signal.FactorXandY.Y - signal.Addition_to_signal;

                    pointsSignal[2 * pbx.Width - k - 2].X = k;
                    pointsSignal[2 * pbx.Width - k - 2].Y = part.Min() * signal.FactorXandY.Y - signal.Addition_to_signal;

                    iStart = iEnd;
                    k++;
                }
            }

            pointsSignal[pointsSignal.Length - 1].Y = pointsSignal[pointsSignal.Length - 2].Y;

            return pointsSignal;
        }

        #region ciarove kreslenie computePointsF
        //
        private PointF[] ComputePointsF(Signal signal, bool nove)
        {
            float[] signalValues;

            PointF[] pointsSignal = new PointF[viewR - viewL];

            if (signal != averagedResult)
            {
                signal.FactorXandY = getFactorXandY(signal);
                signal.Addition_to_signal = getAdditionToSignal(signal);
            }

            if (rdbViewArea.Checked)
                signalValues = signal.values;
            else
                signalValues = signal.viewSignal;

            if (signal.FactorXandY.X == 0)
            {
                signal.FactorXandY = getFactorXandY(signal);
                signal.Addition_to_signal = getAdditionToSignal(signal);
            }

            if ((double)signalValues.Length / (double)pbx.Width < binPtsCount)
            {
                pointsSignal = new PointF[viewR - viewL];

                int x = 0;
                do
                {
                    pointsSignal[x].X = x * signal.FactorXandY.X;
                    pointsSignal[x].Y = signalValues[x] * signal.FactorXandY.Y - signal.Addition_to_signal;
                    x++;
                } while (x < pointsSignal.Length && x < signalValues.Length);

                if (drawTreshold)
                    ptsTreshold = tresholdValue * result.FactorXandY.Y - result.Addition_to_signal;
            }
            else
            {
                pointsSignal = new PointF[pbx.Width];

                if (rdbViewArea.Checked)
                {//netrebaaa
                    signalValues = new float[signal.values.Length];
                    signalValues = signal.values;
                }
                else
                {
                    signalValues = new float[signal.viewSignal.Length];
                    signalValues = signal.viewSignal;
                }

                double pointsInPix = (double)signalValues.Length / (double)pbx.Width;
                double nums = (int)Math.Floor((double)signalValues.Length / (double)pbx.Width);
                double decs = pointsInPix - nums;

                int k = 0;

                int iEnd = 0;
                int iStart = 0;
                double decIndex = 0;


                if (pointsInPix < 1)
                    return pointsSignal;

                signal.PointInMark = new bool[pointsSignal.Length];


                while (k < pbx.Width)
                {
                    decIndex = (k + 1) * nums + (k + 1) * decs;

                    iEnd = (int)Math.Floor(decIndex);

                    float[] part = new float[iEnd - iStart];
                    Array.Copy(signalValues, iStart, part, 0, part.Length);

                    pointsSignal[k].X = part.Max() * signal.FactorXandY.Y - signal.Addition_to_signal;
                    pointsSignal[k].Y = part.Min() * signal.FactorXandY.Y - signal.Addition_to_signal;

                    int middleIndex = iStart + (iEnd - iStart) / 2;

                    if (rdbMarkArea.Checked && StartMarks.Count > 0)
                    {
                        if (StartMarks.Where(x => (x.leftIndex < middleIndex && x.rightIndex > middleIndex)).Count() > 0)
                            signal.PointInMark[k] = true;
                        else
                            signal.PointInMark[k] = false;
                    }
                    iStart = iEnd;
                    k++;
                }
            }

            pointsSignal[pointsSignal.Length - 1].Y = pointsSignal[pointsSignal.Length - 2].Y;

            return pointsSignal;
        }
        //
        #endregion

        private decimal GetViewTimeLength()
        {
            decimal time;
            return time = (decimal)(viewR - viewL) / (decimal)mainView.sampleFrequency;
        }

        # region Paint
        private void pbx_Paint(object sender, PaintEventArgs e)
        {
            #region Conditions
            if (linkedChannels == null || linkedChannels.Count == 0)
            {
                e.Graphics.DrawString("Please attach any channels first", SystemFonts.DefaultFont, Brushes.Black, pbx.Width / 2, pbx.Height / 2, mainView.sfc);
                return;
            }

            if (bgw.IsBusy)
            {
                return;
            }
            /*
             * Paint results here....
             */


            if (origSignal.values == null) //result.values == null ||
            {
                e.Graphics.DrawString("No result was achieved", SystemFonts.DefaultFont, Brushes.Black, pbx.Width / 2, pbx.Height / 2, mainView.sfc);
                return;
            }

            if (chbSmooth.Checked)
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            else
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            lblViewTimeLength.Text = GetViewTimeLength().ToString("0.00 sec");



            if (!rdbViewArea.Checked && (origSignal.viewSignal == null || result.viewSignal == null || (averagedResult.viewSignal == null && chbAverage.Checked)))
                return;

            if (rdbMarkArea.Checked)
            {
                float factorX = (float)pbx.Width / (viewR - viewL);
            }
            #endregion


            #region Detekčné značky I.
            // vykreslenie detekčných značiek
            if (drawViewMarks && marksComputed)
            {
                float factorX = ((float)pbx.Width / (viewR - viewL));
                Brush brush = new SolidBrush(Color.FromArgb(255, Color.Orange));

                for (int p = 0; p < marksPositions.Count; p++)
                {
                    float markWidth = (marksPositions[p].rightIndex - marksPositions[p].leftIndex) * factorX;
                    if (markWidth < 1) markWidth = 1;

                    if (marksPositions[p].leftIndex >= viewL && marksPositions[p].rightIndex <= viewR)
                        e.Graphics.FillRectangle(brush, (marksPositions[p].leftIndex - viewL) * factorX, 5, markWidth, pbx.Height - 12);

                    else if (marksPositions[p].leftIndex < viewL && marksPositions[p].rightIndex > viewL)
                    {
                        markWidth = (marksPositions[p].rightIndex - viewL) * factorX;
                        if (markWidth < 1) markWidth = 1;
                        if (markWidth > pbx.Width) markWidth = pbx.Width;
                        e.Graphics.FillRectangle(brush, 0, 5, markWidth, pbx.Height - 12);
                    }

                    else if (marksPositions[p].leftIndex < viewR && marksPositions[p].rightIndex > viewR)
                    {
                        markWidth = (marksPositions[p].leftIndex - viewL) * factorX;
                        if (markWidth < 1) markWidth = 1;
                        e.Graphics.FillRectangle(brush, markWidth, 5, pbx.Width, pbx.Height - 12);
                    }
                    //}
                }
            }
            //else
            //    lblMarksGenerated.Text = String.Format("Marks generated: -\nMarks filtered: -");
            #endregion


            #region OrigSignal

            int signalValuesLength;
            
            if (rdbViewArea.Checked)
                signalValuesLength = result.values.Length; // staci len dlzka
            else
                signalValuesLength = result.viewSignal.Length;

            if (origSignal.drawing_allowed)
            {
                if (sigPtsRecompRequired)
                    origSignal.pointsF = ComputePointsF(origSignal);

                if ((double)signalValuesLength / (double)pbx.Width < binPtsCount)
                    e.Graphics.DrawLines(Pens.Gray, origSignal.pointsF);
                else
                {
                    e.Graphics.DrawPolygon(Pens.Black, origSignal.pointsF);
                    e.Graphics.FillPolygon(Brushes.Gray, origSignal.pointsF);
                }
            }
            #endregion


            #region Result
            if (result.drawing_allowed == true)
            {
                //s Pen pen;
                
                if (sigPtsRecompRequired)
                    result.pointsF = ComputePointsF(result);

                //s result.pointsF = ComputePointsF(result, true);

                if (!chbAverage.Checked)
                {
       #region StareVykreslovanie
                    //if (sigPtsRecompRequired)
                    //    pointsResult = ComputePointsF(result);

                    /*
                                        //if (rdbMarkArea.Checked)
                                        //{
                                        //    if (viewL < StartMark[0].leftIndex)
                                        //    {
                                        //        //PointF[] pointsResultL = pointsResult.Where(x => x.X < StartMark[0].leftIndex * result.FactorXandY.X).ToArray();//.CopyTo(PointsResultL, 0);
                                        //        //if (pointsResultL.Length > 0) { e.Graphics.DrawLines(Pens.Gray, pointsResultL); }
                                        //        int length = StartMark[0].leftIndex - viewL;
                                        //        if (viewR < StartMark[0].leftIndex)
                                        //            length = viewR - viewL;

                                        //        if (StartMark[0].leftIndex < viewR)
                                        //            length += 1;

                                        //        PointF[] pointsResultL = new PointF[length];
                                        //        Array.Copy(pointsResult, pointsResultL, length);
                                        //        if (pointsResultL.Length > 1) e.Graphics.DrawLines(Pens.Gray, pointsResultL);
                                        //    }

                                        //    if (viewR > StartMark[0].rightIndex)
                                        //    {
                                        //        //PointF[] pointsResultR = pointsResult.Where(x => x.X > StartMark[0].rightIndex * result.FactorXandY.X).ToArray();//.CopyTo(PointsResultR, 0); ;
                                        //        //if (pointsResultR.Length > 0) { e.Graphics.DrawLines(Pens.Gray, pointsResultR); }
                                        //        int right = StartMark[0].rightIndex;
                                        //        if (StartMark[0].rightIndex < viewR)
                                        //            right = viewR;

                                        //        int left = viewL;
                                        //        if (viewL < StartMark[0].rightIndex)
                                        //            left = StartMark[0].rightIndex;

                                        //        PointF[] pointsResultR = new PointF[right - left];
                                        //        Array.Copy(pointsResult, left - viewL, pointsResultR, 0, right - left);
                                        //        if (pointsResultR.Length > 1) e.Graphics.DrawLines(Pens.Gray, pointsResultR);
                                        //    }
                                        //    if (viewL < StartMark[0].rightIndex && viewR > StartMark[0].leftIndex && StartMark[0].rightIndex - StartMark[0].leftIndex > 0)
                                        //    {
                                        //        //    PointF[] pointsResultM = pointsResult.Where(x => (x.X <= StartMark[0].rightIndex * result.FactorXandY.X) && (x.X >= StartMark[0].leftIndex * result.FactorXandY.X)).ToArray();//.CopyTo(PointsResultM, 0);
                                        //        //    if (pointsResultM.Length > 0) { e.Graphics.DrawLines(Pens.Blue, pointsResultM); }
                                        //        int left = StartMark[0].leftIndex;
                                        //        if (left < viewL)
                                        //            left = viewL;

                                        //        int right = StartMark[0].rightIndex;
                                        //        if (right > viewR)
                                        //            right = viewR;

                                        //        if (viewR > StartMark[0].rightIndex)
                                        //            right += 1;

                                        //        PointF[] pointsResultM = new PointF[right - left];
                                        //        Array.Copy(pointsResult, left - viewL, pointsResultM, 0, right - left);
                                        //        Pen pen = Pens.Blue;
                                        //        if (chbAverage.Checked) pen = Pens.Gray;
                                        //        if (pointsResultM.Length > 1) e.Graphics.DrawLines(pen, pointsResultM);
                                        //    }
                                        //}
                                        //else  
                    */
#endregion
                    if ((double)signalValuesLength / (double)pbx.Width < binPtsCount)
                    {
                        //if (chbAverage.Checked)
                        //    e.Graphics.DrawLines(Pens.Gray, pointsResult);
                        //else
                        e.Graphics.DrawLines(Pens.Blue, result.pointsF);
                    }
                    else
                    {
                        //s for (int i = 0; i < result.pointsF.Length; i++ )
                        //s {
                        //s     pen = Pens.Gray;
                           
                        //s     if (rdbMarkArea.Checked && StartMarks.Count > 0 && result.PointInMark != null)
                        //s         if (result.PointInMark[i])
                        //s             pen = Pens.Blue;

                        //s     e.Graphics.DrawLine(pen, (float)i, result.pointsF[i].X, (float)i, result.pointsF[i].Y);
                        
                        if (chbAverage.Checked)
                        {
                            //e.Graphics.FillPolygon(Brushes.Gray, result.pointsF);
                        }
                        else
                        {
                            e.Graphics.DrawPolygon(Pens.Blue, result.pointsF);
                            e.Graphics.FillPolygon(Brushes.LightBlue, result.pointsF);
                        }
                    }
                }
            }
            #endregion


            #region AveragedResult
            if (averagedResult.drawing_allowed && chbAverage.Checked)
            {
                //Average(result);
                if (sigPtsRecompRequired)
                    averagedResult.pointsF = ComputePointsF(averagedResult);

                if ((double)signalValuesLength / (double)pbx.Width < binPtsCount)
                {
                    //pointsAvResult = ComputePointsF(averagedResult);
                    e.Graphics.DrawLines(Pens.SeaGreen, averagedResult.pointsF);
                }
                else
                {
                    e.Graphics.FillPolygon(Brushes.MediumSeaGreen, averagedResult.pointsF);
                    e.Graphics.DrawPolygon(Pens.SeaGreen, averagedResult.pointsF);
                }
            }
            #endregion


            #region Detekčné značky II.
            // vykreslenie detekčných značiek
            if (drawViewMarks && marksComputed)
            {
                float factorX = ((float)pbx.Width / (viewR - viewL));
                Brush brush = new SolidBrush(Color.FromArgb(tbOpacity.Value, Color.Orange));

                for (int p = 0; p < marksPositions.Count; p++)
                {
                    float markWidth = (marksPositions[p].rightIndex - marksPositions[p].leftIndex) * factorX;
                    if (markWidth < 1) markWidth = 1;

                    if (marksPositions[p].leftIndex >= viewL && marksPositions[p].rightIndex <= viewR)
                        e.Graphics.FillRectangle(brush, (marksPositions[p].leftIndex - viewL) * factorX, 5, markWidth, pbx.Height - 12);

                    else if (marksPositions[p].leftIndex < viewL && marksPositions[p].rightIndex > viewL)
                    {
                        markWidth = (marksPositions[p].rightIndex - viewL) * factorX;
                        if (markWidth < 1) markWidth = 1;
                        if (markWidth > pbx.Width) markWidth = pbx.Width;
                        e.Graphics.FillRectangle(brush, 0, 5, markWidth, pbx.Height - 12);
                    }

                    else if (marksPositions[p].leftIndex < viewR && marksPositions[p].rightIndex > viewR)
                    {
                        markWidth = (marksPositions[p].leftIndex - viewL) * factorX;
                        if (markWidth < 1) markWidth = 1;
                        e.Graphics.FillRectangle(brush, markWidth, 5, pbx.Width, pbx.Height - 12);
                    }
                    //}
                }
            }
            //else
            //    lblMarksGenerated.Text = String.Format("Marks generated: -\nMarks filtered: -");
            #endregion


            #region Threshold
            if (drawTreshold)
            {
                if (result.values != null)
                {
                    PointF factor = getFactorXandY(result);
                    float addition = getAdditionToSignal(result);

                    ptsTreshold = tresholdValue * result.FactorXandY.Y - result.Addition_to_signal;
                    e.Graphics.DrawLine(Pens.Black, 0, ptsTreshold, pbx.Width, ptsTreshold);
                }
            }
            #endregion


            #region Start značky
            // vykreslenie STart značiek II
            if (InvStartMarks != null && InvStartMarks.Count > 0 && rdbMarkArea.Checked)
            {
                float factorX = ((float)pbx.Width / (viewR - viewL));
                Brush brush = new SolidBrush(Color.FromArgb(tbOpacity.Value, Color.Gray));
                for (int p = 0; p < InvStartMarks.Count; p++)
                {
                    //if ((!rdbViewArea.Checked && (InvStartMarks[p].leftIndex >= viewL && InvStartMarks[p].rightIndex <= viewR)) || rdbViewArea.Checked)
                    //{
                    if (InvStartMarks[p].leftIndex >= viewL && InvStartMarks[p].rightIndex <= viewR)
                    {
                        e.Graphics.FillRectangle(brush, (InvStartMarks[p].leftIndex - viewL) * factorX, 0, (InvStartMarks[p].rightIndex - InvStartMarks[p].leftIndex) * factorX, pbx.Height);
                        //e.Graphics.DrawRectangle(Pens.Black, (ArtMarks[p].leftIndex - viewL) * factorX, 0, (ArtMarks[p].rightIndex - ArtMarks[p].leftIndex) * factorX, pbx.Height);
                    }
                    else if (InvStartMarks[p].leftIndex < viewL && InvStartMarks[p].rightIndex > viewL)
                    {
                        int leftIndex = (InvStartMarks[p].leftIndex < viewL ? viewL : InvStartMarks[p].leftIndex);
                        e.Graphics.FillRectangle(brush, 0, 0, (InvStartMarks[p].rightIndex - leftIndex) * factorX, pbx.Height);
                        //e.Graphics.DrawRectangle(Pens.Black, 0, 0, (ArtMarks[p].rightIndex - ArtMarks[p].leftIndex) * factorX, pbx.Height);
                    }
                    else if (InvStartMarks[p].leftIndex < viewR && InvStartMarks[p].rightIndex >= viewR)
                    {
                        e.Graphics.FillRectangle(brush, (InvStartMarks[p].leftIndex - viewL) * factorX, 0, pbx.Width, pbx.Height);
                        //e.Graphics.DrawRectangle(Pens.Black, (ArtMarks[p].leftIndex - viewL) * factorX, 0, pbx.Width, pbx.Height);
                    }
                    //}
                }
            }
            #endregion


            #region Art Znacky
            // vykreslenie art značiek II.
            if (ArtMarks.Count > 0 && chbArtMarks.Checked)
            {
                float factorX = ((float)pbx.Width / (viewR - viewL));
                Brush brush = new SolidBrush(Color.FromArgb(tbOpacity.Value, Color.Gray));
                for (int p = 0; p < ArtMarks.Count; p++)
                {
                    //if ((!rdbViewArea.Checked && (ArtMarks[p].leftIndex >= viewL && ArtMarks[p].rightIndex <= viewR)) || rdbViewArea.Checked)
                    //{
                        if (ArtMarks[p].leftIndex >= viewL && ArtMarks[p].rightIndex <= viewR)
                        {
                            e.Graphics.FillRectangle(brush, (ArtMarks[p].leftIndex - viewL) * factorX, 0, (ArtMarks[p].rightIndex - ArtMarks[p].leftIndex) * factorX, pbx.Height);
                            //e.Graphics.DrawRectangle(Pens.Black, (ArtMarks[p].leftIndex - viewL) * factorX, 0, (ArtMarks[p].rightIndex - ArtMarks[p].leftIndex) * factorX, pbx.Height);
                        }
                        else if (ArtMarks[p].leftIndex < viewL && ArtMarks[p].rightIndex > viewL)
                        {
                            int leftIndex = (ArtMarks[p].leftIndex < viewL ? viewL : ArtMarks[p].leftIndex);
                            e.Graphics.FillRectangle(brush, 0, 0, (ArtMarks[p].rightIndex - leftIndex) * factorX, pbx.Height);
                            //e.Graphics.DrawRectangle(Pens.Black, 0, 0, (ArtMarks[p].rightIndex - ArtMarks[p].leftIndex) * factorX, pbx.Height);
                        }
                        else if (ArtMarks[p].leftIndex < viewR && ArtMarks[p].rightIndex > viewR)
                        {
                            e.Graphics.FillRectangle(brush, (ArtMarks[p].leftIndex - viewL) * factorX, 0, pbx.Width, pbx.Height);
                            //e.Graphics.DrawRectangle(Pens.Black, (ArtMarks[p].leftIndex - viewL) * factorX, 0, pbx.Width, pbx.Height);
                        }
                    }
                //}
            }
            #endregion


            #region Info
            if (chbInfo.Checked)
            {
                string fileName = mainView.currentFile;
                string channelName = (linkedChannels[0].channelName != null ? linkedChannels[0].channelName.ToString() : "");

                string s1 = String.Format("File: {0}, Channel: {1}", fileName, channelName);
                e.Graphics.DrawString(s1, SystemFonts.DefaultFont, Brushes.Black, pbx.Width / 2, pbx.Height - 15, mainView.sfc);

                string s = String.Format("FFT range: {0}-{1} Hz, Smoothing radius= {2}, Win. func.= {3}, Treshold val.= {4}, Mark filter: min gap = {5} ms, min length = {6} ms, Position: {7} - {8} sec",
                                         nudLF.Value.ToString(), nudHF.Value.ToString(), nudAverageFilter.Value.ToString(), btnWindow.Text.ToString(), tresholdValue.ToString(), nudMinGapLength.Value.ToString(),
                                         nudMinMarkLength.Value.ToString(), (GetTimeLengthFromSamples(viewL) / 1000).ToString(), (GetTimeLengthFromSamples(viewR) / 1000).ToString());
                e.Graphics.DrawString(s, SystemFonts.DefaultFont, Brushes.Black, pbx.Width / 2, 0, mainView.sfc);
            }
            #endregion


            sigPtsRecompRequired = false;

        }
#endregion

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (linkedChannels.Count <= 0)
                return;

            if (result.values == null)
                fftRecompRequired = true;

            viewL = graphPanel.leftI;
            viewR = graphPanel.rightI;

            int wholeDataLength = linkedChannels[0].dataCache[0].data.Count;

            this.Invoke((MethodInvoker)delegate
            {
                rdbViewArea.Enabled = false;
                rdbMarkArea.Enabled = false;
                rdbWholeArea.Enabled = false;
            });


            if (rdbViewArea.Checked)
            {
                origSignal.values = new float[viewR - viewL];
                linkedChannels[0].dataCache[0].data.CopyTo(viewL, origSignal.values, 0, viewR - viewL);
                result.defaultValues = new float[viewR - viewL];
                result.values = new float[viewR - viewL];
            }
            else if (origSignal.values == null || result.values == null || result.viewSignal == null || fftRecompRequired)
            {
                origSignal.viewSignal = new float[viewR - viewL];
                origSignal.values = new float[wholeDataLength];

                result.viewSignal = new float[viewR - viewL];
                result.values = new float[wholeDataLength];
                result.defaultValues = new float[wholeDataLength];

                linkedChannels[0].dataCache[0].data.CopyTo(0, origSignal.values, 0, wholeDataLength);
                linkedChannels[0].dataCache[0].data.CopyTo(viewL, origSignal.viewSignal, 0, viewR - viewL);
            }
            else 
            {
                //result.defaultValues = new float[wholeDataLength];
                //result.values = new float[wholeDataLength];
                //result.viewSignal = new float[viewR - viewL];

                origSignal.viewSignal = new float[viewR - viewL];
                linkedChannels[0].dataCache[0].data.CopyTo(viewL, origSignal.viewSignal, 0, viewR - viewL);
            }

            float loFreq = (float)nudLF.Value;
            float hiFreq = (float)nudHF.Value;

            WindowingFunction winFunction = new WindowingFunction(viewR - viewL, pluginWindowFunction.winType, pluginWindowFunction.winParam);

            timer2.Start();            

            if ((fftRecompRequired && !rdbViewArea.Checked) || rdbViewArea.Checked || (!rdbViewArea.Checked && result.values.Length != wholeDataLength))
            {
                result.defaultValues = FFT.FFT_filter(origSignal.values, loFreq, hiFreq, false, chbEnvelope.Checked, winFunction.winType, (int)winFunction.winParam);
            }


            if (chbAbs.Checked)
            {
                for (int i = 0; i < result.defaultValues.Length; i++)
                {
                    result.defaultValues[i] = Math.Abs(result.defaultValues[i]);
                }
            }

            if (chbPower.Checked)
            {
                for (int i = 0; i < result.defaultValues.Length; i++)
                {
                    result.defaultValues[i] = result.defaultValues[i] * result.defaultValues[i];
                }
            }

            //pseudo default values
            if (result.values == null)
            {
                result.values = new float[result.defaultValues.Length];
            }
            result.defaultValues.CopyTo(result.values, 0);
            

            if (!rdbViewArea.Checked)
            {
                if (linkedChannels[0].dataCache[0].data.Count == result.values.Length)
                   // if (result.values.Length + viewL >= (viewR - viewL))
                        Average(result);
            }
            else
                Average(result);


            if (averagedResult.values != null)
            {
                if (rdbViewArea.Checked)
                    averagedResult.defaultValues = new float[viewR - viewL];
                else
                    averagedResult.defaultValues = new float[wholeDataLength];

                averagedResult.values.CopyTo(averagedResult.defaultValues, 0);
                DeleteEdges(averagedResult);


                DeleteEdges(result);

                if (!rdbViewArea.Checked)
                {
                    averagedResult.viewSignal = new float[viewR - viewL];

                    Array.Copy(averagedResult.values, averagedResult.viewSignal, (int)(viewR - viewL));
                }
            }
            else
                DeleteEdges(result);

            if (chbAverage.Checked)
                averagedResult.pointsF = ComputePointsF(averagedResult);
            else
                origSignal.pointsF = ComputePointsF(origSignal);

            result.pointsF = ComputePointsF(result);
            sigPtsRecompRequired = false;
        }


        private void Average(Signal signal)
        {
            sigPtsRecompRequired = true;

            int winLength = (int)nudAverageFilter.Value * 2 + 1;
            int winHalfIndex = winLength / 2;


            if (!rdbViewArea.Checked)
            {
                signal.viewSignal = new float[(viewR - viewL)];
                Array.Copy(signal.values, viewL, signal.viewSignal, 0, (int)(viewR - viewL));
                averagedResult.viewSignal = new float[signal.viewSignal.Length];

                if (nudAverageFilter.Value == 0)
                {
                    averagedResult.values = signal.values;
                    averagedResult.viewSignal = signal.viewSignal;
                    averagedResult.drawing_allowed = true;
                    return;
                }
                ptsTreshold = tresholdValue * averagedResult.FactorXandY.Y - averagedResult.Addition_to_signal;

            }

            float suma = 0;
            averagedResult.values = new float[signal.values.Length];

            WindowingFunction winFunction = new WindowingFunction(winLength, pluginWindowFunctionLP.winType, pluginWindowFunctionLP.winParam);
            float[] wfWindow = winFunction.window.ToArray();

            if (nudAverageFilter.Value == 0)
            {
                averagedResult.values = signal.values;
                averagedResult.drawing_allowed = true;
                return;
            }

            for (int i = winHalfIndex; i < averagedResult.values.Length - winHalfIndex; i++)
            {
                int k = 0;
                suma = 0;
                for (int j = i - winHalfIndex; j <= i + winHalfIndex; j++)
                {
                    suma += signal.values[j] * wfWindow[k];
                    k++;
                }
                averagedResult.values[i] = suma / wfWindow.Sum();
            }


            //TODO: posledne hodnoty zvlast v kombinacii s mazanim hran
            for (int i = 0; i < winHalfIndex; i++)
            {
                averagedResult.values[i] = averagedResult.values[winHalfIndex];
                averagedResult.values[averagedResult.values.Length - i - 1] = averagedResult.values[averagedResult.values.Length - i - 2];
            }

            averagedResult.drawing_allowed = true;


            if (!rdbViewArea.Checked)
                Array.Copy(averagedResult.values, viewL, averagedResult.viewSignal, 0, (viewR - viewL));


            averagedResult.FactorXandY = result.FactorXandY;
            averagedResult.Addition_to_signal = result.Addition_to_signal;
        }


        private void DeleteEdges(Signal signal)
        {
            deleteEdgesValue = GetNumOfSamples((float)nudCompensate.Value);
            if (deleteEdgesValue > Math.Floor((double)result.defaultValues.Length / 2))
                return;


            if (!chbDeleteEdges.Checked)
                return;

            sigPtsRecompRequired = true;

            signal.defaultValues.CopyTo(signal.values, 0);

            for (int i = 0; i < deleteEdgesValue; i++)
            {
                signal.values[i] = signal.values[(int)deleteEdgesValue];
                signal.values[signal.values.Length - 1 - i] = signal.values[(int)deleteEdgesValue];
            }

            if (!rdbViewArea.Checked)
            {
                signal.viewSignal = new float[viewR - viewL];
                Array.Copy(signal.values, viewL, signal.viewSignal, 0, (int)(viewR - viewL));
            }

            if (!bgw.IsBusy && !bgwMarkPos.IsBusy)
                pbx.Refresh();
        }


        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (result.values != null)
            {
                nudTresholdValue.Maximum = 10 * (decimal)(result.values.Max() - result.values.Min());
                nudTresholdValue.Minimum = -10 * (decimal)(result.values.Max() - result.values.Min());
            }

            timer2.Stop();

            if (drawViewMarks)
            {
                if (!bgwMarkPos.IsBusy)
                    bgwMarkPos.RunWorkerAsync();
                else
                    bgwMarkPos.CancelAsync();
            }

            if (graphPanel.leftI != viewL || graphPanel.rightI != viewR)
            {
                if (!bgw.IsBusy)
                    bgw.RunWorkerAsync();
                else
                    bgw.CancelAsync();
            }
            else if (!drawViewMarks)
                pbx.Refresh();

            rdbMarkArea.Enabled = true; // netreba
            rdbViewArea.Enabled = true;
            rdbWholeArea.Enabled = true;

            lblComputing.Visible = false;
            fftRecompRequired = false;

            if (rdbMarkArea.Checked && StartMarks.Count > 0 && iMarksRecompRequired)
                getInvertedStartMarks(StartMarks);
        }


        private void bgwMarkPos_DoWork(object sender, DoWorkEventArgs e)
        {
            marksComputed = false;

            if (chbAverage.Checked)
                marksPositions = GetMarksPositions(averagedResult);
            else
                marksPositions = GetMarksPositions(result);
        }

        private void bgwMarkPos_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            marksComputed = true;

            dtgAll.Refresh();
            dtgStats.Refresh();

            if (!bgw.IsBusy && !bgwMarkPos.IsBusy)
                pbx.Refresh();
        }

        private void SaveMarks()
        {
            graphPanel.selectionAreas.AddRange(GetMarksPositions(result));
            mv.refreshMarks();
            mv.rebuiltAndRedrawAll();
            txtMarkDescription.Text = String.Format("Start_Mark_{0}", exportedMarkCount);
            exportedMarkCount++;
        }

        private void btMarks_Click(object sender, EventArgs e)
        {
            signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();
            sc.setFilter(marksFilter);

            if (sc.ShowDialog() == DialogResult.OK)
            {
                marksList = sc.result;
                marksFilter = sc.filter;
                refrControls();
            }
        }

        private void btnWindow_Click(object sender, EventArgs e)
        {
            signalViewer.windowFunc winfunctionDialogWindow = new signalViewer.windowFunc();

            winfunctionDialogWindow.wf.winType = pluginWindowFunction.winType;

            winfunctionDialogWindow.wf.winParam = pluginWindowFunction.winParam;

            winfunctionDialogWindow.Location = new Point(btnWindow.Left + this.ParentForm.Location.X, btnWindow.Top + this.ParentForm.Location.Y);


            if (winfunctionDialogWindow.ShowDialog() == DialogResult.OK)
            {
                string str = signalViewer.WindowingFunction.windowNames[winfunctionDialogWindow.wf.winType];

                if (signalViewer.WindowingFunction.numParams[winfunctionDialogWindow.wf.winType] > 0)
                {
                    str += " (" + winfunctionDialogWindow.wf.winParam.ToString() + ")";
                }

                btnWindow.Text = str;

                if (pluginWindowFunction.winType != winfunctionDialogWindow.wf.winType)
                {
                    pluginWindowFunction.winType = winfunctionDialogWindow.wf.winType;
                    pluginWindowFunction.winParam = winfunctionDialogWindow.wf.winParam;

                    fftRecompRequired = true;

                    bgw.RunWorkerAsync();
                }
            }
        }

        private void nudTresholdPercents_ValueChanged(object sender, EventArgs e)
        {
            if (!nudTresholdPercents.Focused)
                return;

            if (tresholdValueChangingAllowed)
            {
                drawTreshold = true;
                chbTresholdView.Checked = true;
                tresholdValueChangingAllowed = false;
                SetTresholdValuePercents();

                RedrawTresholdParameters();

                if (!bgwMarkPos.IsBusy)
                    bgwMarkPos.RunWorkerAsync();
                else
                    bgwMarkPos.CancelAsync();

                tresholdValueChangingAllowed = true;
            }
        }


        private void SetTresholdValuePercents()
        {
            if (result.values != null)//|| result.viewSignal != null
            {
                float a = result.values.Min();
                float range = result.values.Max() - result.values.Min();
                tresholdValue = result.values.Min() + range * (float)nudTresholdPercents.Value / 100;
            }
            else
                MessageBox.Show("Compute signal result first");
        }

        #region DELETE ME
        //private void nudPercentil_ValueChanged(object sender, EventArgs e)
        //{
        //    tresholdChangingAllowed = true;

        //    if (!nudPercentil.Focused)
        //        return;

        //    if (tresholdChangingAllowed)
        //    {
        //        drawTreshold = true;
        //        chbTresholdView.Checked = true;
        //        tresholdChangingAllowed = false;
        //        SetTresholdValuePercentil();
        //        nudTresholdValue.Value = (decimal)tresholdValue;
        //        RedrawTresholdParameters();

        //        if (!bgwMarkPos.IsBusy)
        //            bgwMarkPos.RunWorkerAsync();
        //        else
        //            bgwMarkPos.CancelAsync();

        //        //if (!presetLoading)
        //        //    pbx.Refresh();
        //        tresholdChangingAllowed = true;
        //    }
        //}
        #endregion
        private void txtPercentil_TextChanged(object sender, EventArgs e)
        {
            if (!tresholdValueChangingAllowed)
                return;

            if (!txtPercentil.Focused)
                return;


            drawTreshold = true;
            chbTresholdView.Checked = true;
            tresholdValueChangingAllowed = false;
            SetTresholdValuePercentil();
            RedrawTresholdParameters();

            if (!bgwMarkPos.IsBusy)
                bgwMarkPos.RunWorkerAsync();
            else
                bgwMarkPos.CancelAsync();

            tresholdValueChangingAllowed = true;
        }

        
        private void SetTresholdValuePercentil()
        {
            double percentil;
            if (!Double.TryParse(txtPercentil.Text, out percentil))
                return;

            float[] values = new float[result.values.Length];
            int len = result.values.Length;

            if (chbAverage.Checked && averagedResult != null)
                Array.Copy(averagedResult.values, values, len);
            else
                Array.Copy(result.values, values, len);

            Array.Sort(values);

            int index = (int)(percentil / 100 * (len - 1));
            tresholdValue = values[index];
        }

        private void nudTresholdValue_ValueChanged(object sender, EventArgs e)
        {
            if (!nudTresholdValue.Focused)
                return;

            chbTresholdView.Checked = true;
            tresholdValue = (float)nudTresholdValue.Value;
            tresholdValueChangingAllowed = false;
            RedrawTresholdParameters();

            if (!bgwMarkPos.IsBusy)
                bgwMarkPos.RunWorkerAsync();
            else
                bgwMarkPos.CancelAsync();

            tresholdValueChangingAllowed = true;
        }


        private void SetTresholdValueMouse()
        {
            tresholdValue = (ptsTreshold + result.Addition_to_signal) / result.FactorXandY.Y;
        }


        private void RedrawTresholdParameters()
        {
            nudTresholdPercents.Value = GetPercentalValue(result) * 100;
            nudTresholdValue.Value = (decimal)tresholdValue;


            float[] values = new float[result.values.Length];
            int len = result.values.Length;

            if (chbAverage.Checked && averagedResult != null)
                Array.Copy(averagedResult.values, values, len);
            else
                Array.Copy(result.values, values, len);

            if (tresholdValue > values.Max())
            {
                txtPercentil.Text = "100";
                return;
            }

            if (values.Length < 1)
                return;
            #region HIDE
            //TODO ak je vacsia hodnota ako vs signaly

            /*double dif = Math.Abs(values[0] - tresholdValue);
            float val1 = values[0];
            float val2;

            for (int i = 1; i < values.Length; i++)
            {
                if (Math.Abs(values[i] - tresholdValue) < dif)
                {
                    dif = Math.Abs(values[i] - tresholdValue);
                    val1 = values[i];
                }
                else if (Math.Abs(values[i] - tresholdValue) > dif || i == values.Length - 1)
                {
                    for (int j = i; j >= 0; j--)
                    {
                        if (Math.Abs(values[j] - tresholdValue) != dif)
                            //val2 = values[j];
                    }
                }
            }*/
            #endregion
            float[] difs = new float[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                difs[i] = Math.Abs(values[i] - tresholdValue);
            }

            float[] sortedByDifsValues = new float[values.Length];
            Array.Copy(values, sortedByDifsValues, values.Length);

            Array.Sort(difs, sortedByDifsValues);

            float closestVal = sortedByDifsValues[0];
            float[] closestValues = values.Where(x => x == closestVal).ToArray();

            int smallerValuesCount = sortedByDifsValues.Where(x => x < closestVal).Count();

            if (closestValues.Length > 1)
            {
                double val1 = (double)(smallerValuesCount) / values.Length * 100;
                double val2 = (double)(smallerValuesCount + closestValues.Length) / values.Length * 100;
                txtPercentil.Text = string.Format("{0:0.0000} - {1:0.0000}", val1, val2);
            }
            else
            {
                double percentil = (double)(smallerValuesCount) / values.Length * 100;
                string strPercentil = string.Format("{0:0.000}", percentil);
                txtPercentil.Text = strPercentil;
            }

        }

        private decimal GetPercentalValue(Signal signal)
        {
            if (signal.values == null)
                return 0;

            float range = signal.values.Max() - signal.values.Min();
            decimal percents = (decimal)((tresholdValue - signal.values.Min()) / range);

            return percents;
        }

        private void pbx_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawTreshold)
            {
                if (e.Location.Y > ptsTreshold - 3 && e.Location.Y < ptsTreshold + 3)
                {
                    Cursor.Current = Cursors.HSplit;
                    mouseOverTreshold = true;
                }
                else
                {
                    Cursor.Current = Cursors.Arrow;
                    mouseOverTreshold = false;
                }

                if (tresholdMovingAllowed)
                {
                    ptsTreshold = e.Location.Y;
                    SetTresholdValueMouse();
                    RedrawTresholdParameters();

                    if (chbDrawMarks.Checked)
                        pbx.Refresh();
                }
            }

        }

        private void pbx_MouseDown(object sender, MouseEventArgs e)
        {
            if (mouseOverTreshold)
                tresholdMovingAllowed = true;
            else
            {
                tresholdMovingAllowed = false;
            }
        }
        
        private void pbx_MouseUp(object sender, MouseEventArgs e)
        {
            if (tresholdMovingAllowed)
            {
                ptsTreshold = e.Location.Y;
                SetTresholdValueMouse();
                RedrawTresholdParameters();

                if (!bgwMarkPos.IsBusy)
                    bgwMarkPos.RunWorkerAsync();               
                else
                    bgwMarkPos.CancelAsync();
            }

            tresholdMovingAllowed = false;
        }

        private int GetNumOfSamples(float ms)
        {
            float fvz = mainView.sampleFrequency;
            int numOfSamples = (int)(fvz * ms / 1000);
            return numOfSamples;
        }

        private decimal GetTimeLengthFromSamples(int numOfSamples)
        {
            decimal time;
            return time = Math.Floor((decimal)numOfSamples / (decimal)mainView.sampleFrequency * 1000);
        }

        private List<selectionMember> GetMarksPositions(Signal signal)
        {
            List<selectionMember> sms = new List<selectionMember>();

            if (rdbMarkArea.Checked && (StartMarks == null || StartMarks.Count == 0))
                return sms;

            bool newMarkStarted = false;

            int leftPosition = 0;
            int rightPosition = 0;
            int numOfMarks = 0;

            for (int i = 0; i < signal.values.Length; i++)
            {
                //if ((rdbMarkArea.Checked && (i >= StartMark[0].leftIndex && i <= StartMark[0].rightIndex)) || !rdbMarkArea.Checked)
                //{
                if (rdbMarkArea.Checked && StartMarks.Any(x => (i >= x.leftIndex && i <= x.rightIndex)) || !rdbMarkArea.Checked)
                {
                    bool inArtMark = false;

                    if (chbArtMarks.Checked)
                    {
                        if (rdbViewArea.Checked)
                            foreach (selectionMember mark in ArtMarks)
                            {
                                if (mark.leftIndex - viewL <= i && mark.rightIndex - viewL >= i)
                                    inArtMark = true;
                            }
                        else
                            foreach (selectionMember mark in ArtMarks)
                            {
                                if (mark.leftIndex <= i && mark.rightIndex >= i)
                                    inArtMark = true;
                            }
                    }
                    if (signal.values[i] > tresholdValue && !newMarkStarted && !inArtMark)
                    {
                        if (!inArtMark)
                        {
                            newMarkStarted = true;
                            leftPosition = i;
                        }
                    }

                    else if ((signal.values[i] <= tresholdValue && newMarkStarted)|| (signal.values[i] >= tresholdValue && newMarkStarted && inArtMark))
                    {
                        newMarkStarted = false;
                        rightPosition = i;

                        selectionMember newMark = new selectionMember(leftPosition, rightPosition, txtMarkDescription.Text, mv.gpList[0]);
                        sms.Add(newMark);

                        numOfMarks++;
                    }
                }
            }

            if (rdbViewArea.Checked)
            {
                foreach (selectionMember mark in sms)
                {
                    mark.leftIndex += viewL;
                    mark.rightIndex += viewL;
                }
            }
          
            sms = MarkGapFilter(sms);
            sms = MarkMinLengthFilter(sms);

            numOfMarksGenerated = sms.Count;
            numOfMarksFiltered = numOfMarks - sms.Count;

            this.Invoke((MethodInvoker)delegate
            {
                //lblMarksGenerated.Text = String.Format("Marks generated: {0}\nMarks filtered: {1}", sms.Count, numOfMarksFiltered);
                btnGenerateMarks.Text = String.Format("Export {0} marks", sms.Count);
            });

            return sms;
        }

        private List<selectionMember> MarkGapFilter(List<selectionMember> markList)
        {
            if (nudMinGapLength.Value == 0 || markList.Count <= 0)
                return markList;

            int i = 0;
            int j;
            int gapEndIndex;
            List<selectionMember> filteredMarkList = new List<selectionMember>();
            int minGapLength = GetNumOfSamples((float)nudMinGapLength.Value);
            int farestMarkIndex;

            int xStart = 0;
            int xEnd = linkedChannels[0].dataCache[0].data.Count - 1;

            if (rdbViewArea.Checked)
            {
                xStart = viewL;
                xEnd = viewR;
            }

            for (int x = xStart; x < xEnd; x++)
            {
                if (markList[i].rightIndex == x && i < markList.Count)
                {
                    gapEndIndex = minGapLength + x;
                    farestMarkIndex = markList[i].rightIndex;

                    for (j = i + 1; j < markList.Count; j++)
                    {
                        if (markList[j].leftIndex <= gapEndIndex)
                        {
                            farestMarkIndex = markList[j].rightIndex;
                            gapEndIndex = markList[j].rightIndex + minGapLength;
                        }
                        else
                            break;
                    }

                    selectionMember newMark = new selectionMember(markList[i].leftIndex, farestMarkIndex, txtMarkDescription.Text, mv.gpList[0]);
                    filteredMarkList.Add(newMark);
                    i = j - 1;
                    i++;

                    if (i >= markList.Count)
                        break;
                }
            }
            return filteredMarkList;
        }

        private List<selectionMember> MarkMinLengthFilter(List<selectionMember> markList)
        {
            if (nudMinGapLength.Value == 0 || markList.Count <= 0)
                return markList;

            List<selectionMember> filteredMarkList = new List<selectionMember>();
            int minMarkLength = GetNumOfSamples((float)nudMinMarkLength.Value);

            for (int i = 0; i < markList.Count; i++)
            {
                if (markList[i].rightIndex - markList[i].leftIndex >= minMarkLength)
                    filteredMarkList.Add(markList[i]);
            }

            return filteredMarkList;
        }

        private void pbx_Resize(object sender, EventArgs e)
        {
            sigPtsRecompRequired = true;
            pbx.Refresh();
        }

        private void nudMinGapLength_ValueChanged(object sender, EventArgs e)
        {
            if (presetLoading)
                return;

            chbDrawMarks.Checked = true;

            if (!bgwMarkPos.IsBusy)
                bgwMarkPos.RunWorkerAsync();
            else
                bgwMarkPos.CancelAsync();            
        }

        private void nudMinMarkLength_ValueChanged(object sender, EventArgs e)
        {
            if (presetLoading)
                return;

            chbDrawMarks.Checked = true;

            if (!bgwMarkPos.IsBusy)
                bgwMarkPos.RunWorkerAsync();
            else
                bgwMarkPos.CancelAsync(); 
        }

        private bool CheckFilterParameters()
        {
            bool isOk = false;

            if (nudLF.Value >= 0 && nudLF.Value < nudHF.Value && nudHF.Value >= 0) //&& //cbxWindow.SelectedIndex >= 0)
            {
                isOk = true;
                lblInvalidFilter.Visible = false;
            }
            else
                lblInvalidFilter.Visible = true;
            return isOk;
        }

        private void nudLF_ValueChanged(object sender, EventArgs e)
        {
            if (presetLoading)
                return;

            if (CheckFilterParameters() && !bgw.IsBusy)
                bgw.RunWorkerAsync();

            fftRecompRequired = true;
        }

        private void nudHF_ValueChanged(object sender, EventArgs e)
        {
            if (presetLoading)
                return;

            if (CheckFilterParameters() && !bgw.IsBusy)
                bgw.RunWorkerAsync();

            fftRecompRequired = true;
        }

        private void chbPower_CheckedChanged(object sender, EventArgs e)
        {
            if (presetLoading)
                return;

            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync();
        }


        void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    pgbBgwCompleted.Value = (int)FFT.fftProc;
                });
            }
            catch (Exception)
            { }
        }


        private void btnGenerateMarks_Click(object sender, EventArgs e)
        {
            if (bgw.IsBusy || bgwMarkPos.IsBusy)
                MessageBox.Show("Please wait for operation to complete");

            if (rdbViewArea.Checked)
            {
                DialogResult dialogResult = MessageBox.Show("You haven't computed the whole signal. This will generate only the marks, displayed in view. \nDo you want to proceed anyway?", "Whole Signal not computed", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    graphPanel.selectionAreas.AddRange(marksPositions);
                    mv.refreshMarks();
                    mv.rebuiltAndRedrawAll();
                    txtMarkDescription.Text = String.Format("Marks");           
                }
            }
            else
            {
                graphPanel.selectionAreas.AddRange(marksPositions);
                mv.refreshMarks();
                mv.rebuiltAndRedrawAll();
            }
        }

        private void nudCompensate_ValueChanged(object sender, EventArgs e)
        {
            if (chbDeleteEdges.Checked)
            {
                if (presetLoading)
                    return;

                if (origSignal.values != null)
                    nudCompensate.Maximum = GetTimeLengthFromSamples(origSignal.values.Length / 2);

                deleteEdgesValue = (GetNumOfSamples((float)nudCompensate.Value));


                if (chbAverage.Checked)
                    DeleteEdges(averagedResult);
                else
                    DeleteEdges(result);

                if (!bgwMarkPos.IsBusy)
                    bgwMarkPos.RunWorkerAsync();
                else
                    bgwMarkPos.CancelAsync();

                if (!bgw.IsBusy && !chbDrawMarks.Checked)
                    pbx.Refresh();
            }
        }

        private void SetViewParameters()
        {
            if (gbViewParam.Visible && !loadingParametersActive)
            {
                int ind = cbxSignal.SelectedIndex;
                signal_list[ind].Scale_Y = (float)nuSize.Value / 100;
                signal_list[ind].Vertical_position = (float)nuPosition.Value / 100;
                signal_list[ind].drawing_allowed = checkBoxDrawing.Checked;

                pbx.Refresh();
            }
        }

        private void LoadViewParameters()
        {
            if (signal_list[0] == null)
            {
                signal_list[0] = origSignal;
                signal_list[1] = result;
                signal_list[2] = averagedResult;
            }

            loadingParametersActive = true;

            int ind = cbxSignal.SelectedIndex;
            nuSize.Value = (decimal)signal_list[ind].Scale_Y * 100;
            nuPosition.Value = (decimal)signal_list[ind].Vertical_position * 100;

            tbSize.Value = (int)nuSize.Value;
            tbPosition.Value = (int)nuPosition.Value;
            checkBoxDrawing.Checked = signal_list[ind].drawing_allowed;

            loadingParametersActive = false;
        }

        private void btViewOptions_Click(object sender, EventArgs e)
        {
            if (gbViewParam.Visible == false)
                LoadViewParameters();
            gbViewParam.Visible = !gbViewParam.Visible;
        }

        private void RedrawViewParameters()
        {
            if (nuSize.Value > tbSize.Maximum)
                tbSize.Value = tbSize.Maximum;
            else
                tbSize.Value = (int)nuSize.Value;

            if (nuPosition.Value > tbPosition.Maximum)
                tbPosition.Value = tbPosition.Maximum;
            else
                tbPosition.Value = (int)nuPosition.Value;
        }

        private void cbxSignal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gbViewParam.Visible == true)
                LoadViewParameters();
        }

        private void tbSize_Scroll(object sender, EventArgs e)
        {
            nuSize.Value = tbSize.Value;
            sigPtsRecompRequired = true;
            SetViewParameters();
        }

        private void nuSize_ValueChanged(object sender, EventArgs e)
        {
            RedrawViewParameters();
            sigPtsRecompRequired = true;
            SetViewParameters();
        }

        private void tbPosition_Scroll(object sender, EventArgs e)
        {
            nuPosition.Value = tbPosition.Value;
            sigPtsRecompRequired = true;
            SetViewParameters();
        }

        private void nuPosition_ValueChanged(object sender, EventArgs e)
        {
            RedrawViewParameters();
            sigPtsRecompRequired = true;
            SetViewParameters();
        }

        private void checkBoxDrawing_CheckedChanged(object sender, EventArgs e)
        {
            SetViewParameters();
            pbx.Refresh();
        }


        private void btDefaultView_Click(object sender, EventArgs e)
        {
            origSignal.Scale_Y = (float)0.2;
            origSignal.Vertical_position = (float)0.85;
            origSignal.drawing_allowed = true;

            result.Scale_Y = (float)0.70;
            result.Vertical_position = (float)0.35;
            result.drawing_allowed = true;

            averagedResult.Scale_Y = (float)0.70;
            averagedResult.Vertical_position = (float)0.35;
            averagedResult.drawing_allowed = false;



            LoadViewParameters();
            SetViewParameters();
        }

        private void btViewParamHide_Click(object sender, EventArgs e)
        {
            gbViewParam.Visible = false;
        }

        private void chbSmooth_CheckedChanged(object sender, EventArgs e)
        {
            if (presetLoading)
                return;

            pbx.Refresh();
        }

        private void chbCompensate_CheckedChanged(object sender, EventArgs e)
        {
            if (presetLoading)
                return;

            pbx.Refresh();
            nudCompensate.Enabled = !nudCompensate.Enabled;

            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync();
        }

        private void chbTresholdView_CheckedChanged(object sender, EventArgs e)
        {
            SetTresholdValuePercents();
            ptsTreshold = tresholdValue * result.FactorXandY.Y - result.Addition_to_signal;
            chbDrawMarks.Checked = true;

            if (chbTresholdView.Checked)
            {
                nudTresholdValue.Enabled = true;
                txtPercentil.Enabled = true;
                nudTresholdPercents.Enabled = true;
            }
            else
            {
                nudTresholdValue.Enabled = true;
                txtPercentil.Enabled = true;
                nudTresholdPercents.Enabled = true;
            }

            if (chbTresholdView.Checked)
                drawTreshold = true;
            else
                drawTreshold = false;

            pbx.Refresh();
        }

        private void chbDrawMarks_CheckedChanged(object sender, EventArgs e)
        {
            if (chbDrawMarks.Checked)
                drawViewMarks = true;
            else
                drawViewMarks = false;

            if (!bgwMarkPos.IsBusy)
                bgwMarkPos.RunWorkerAsync();
            else
                bgwMarkPos.CancelAsync(); 
        }

        private void getInvertedStartMarks(List<selectionMember> marks)
        {
            if (bgw.IsBusy || marks.Count < 1)
                return;

            InvStartMarks = new List<selectionMember>();
            float[] signalValues;

            if (rdbMarkArea.Checked)
                signalValues = result.values;
            else
                signalValues = result.viewSignal;

            bool[] inStartMark = new bool[signalValues.Length];

            for (int i = 0; i < signalValues.Length; i++)
            {
                if (StartMarks.Where(x => x.leftIndex <= i && x.rightIndex >= i).Count() > 0)
                    inStartMark[i] = true;
                else
                    inStartMark[i] = false;
            }

            bool insideInv = false;
            int leftIndex = 0;

            for (int i = 0; i < signalValues.Length; i++)
            {
                if (!inStartMark[i] && !insideInv)
                {
                    insideInv = true;
                    leftIndex = i;
                }
                if (inStartMark[i] && insideInv)
                {
                    InvStartMarks.Add(new selectionMember(leftIndex, i - 1, "inverse", mv.gpList[0]));
                    insideInv = false;
                }                
            }
            if (insideInv && !inStartMark[inStartMark.Length - 1])
                InvStartMarks.Add(new selectionMember(leftIndex, inStartMark.Length - 1, "inverse", mv.gpList[0]));

            iMarksRecompRequired = false;
        }

        private void btnSelectMarks_Click(object sender, EventArgs e)
        {
            if (!rdbMarkArea.Checked)
                return;

            signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();

            sc.setFilter(currentStartMarkFilter);

            if (sc.ShowDialog() == DialogResult.OK && sc.result.Count > 0)
            {
                StartMarks = sc.result;
                currentStartMarkFilter = sc.filter;
                btnSelectStartMarks.Text = String.Format("{0} start marks selected", StartMarks.Count);

                fftRecompRequired = true;
                sigPtsRecompRequired = true;
                iMarksRecompRequired = true;
                doExternalRefresh();

            }

            if (StartMarks.Count == 0)
            {
                rdbMarkArea.Checked = false;
                if (result.values != null && result.values.Length == linkedChannels[0].dataCache[0].data.Count)
                    rdbWholeArea.Checked = true;
                else
                    rdbViewArea.Checked = true;
            }

            //nudStartMarkLength.Enabled = (StartMark.Count == 0 ? nudStartMarkLength.Enabled = false : nudStartMarkLength.Enabled = true);
            getInvertedStartMarks(StartMarks);
        }

        private void btnSelectArtMarks_Click(object sender, EventArgs e)
        {
            signalViewer.selectMarkersForm sc = new signalViewer.selectMarkersForm();

            sc.setFilter(currentArtMarkFilter);

            if (sc.ShowDialog() == DialogResult.OK)
            {
                ArtMarks = sc.result;
                currentStartMarkFilter = sc.filter;
                btnSelectArtMarks.Text = String.Format("{0} Art. Marks selected", ArtMarks.Count);
                chbArtMarks.Checked = true;
                refrControls();
            }
        }

        private void nudStartMarkLength_ValueChanged(object sender, EventArgs e)
        {
            if (StartMarks.Count <= 0)
                return;

            for (int p = 0; p < StartMarks.Count; p++)
            {
                StartMarks[p].rightIndex = StartMarks[p].leftIndex + GetNumOfSamples((int)(nudStartMarkLength.Value * 1000));

                if (StartMarks[p].rightIndex > result.values.Length)
                    StartMarks[p].rightIndex = result.values.Length;
            }

            if (!bgwMarkPos.IsBusy)
                bgwMarkPos.RunWorkerAsync();
            else
            {
                bgwMarkPos.CancelAsync();
            }
        }


        private void rdbWholeArea_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbMarkArea.Checked)
                return;

            result.values = null;
            
            fftRecompRequired = true;

            if ((viewR - viewL > 50000 && rdbViewArea.Checked) || (!rdbViewArea.Checked && fftRecompRequired))
                lblComputing.Visible = true;

            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync(); 
        }

        private void rdbMarkArea_CheckedChanged(object sender, EventArgs e)
        {
            btnSelectStartMarks.Enabled = (rdbMarkArea.Checked) ? true : false;

            result.values = null;

            if (StartMarks.Count == 0)
                btnSelectStartMarks.PerformClick();
        }

        private void rdbViewArea_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbMarkArea.Checked)
                return;

            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync();  
        }

        void CreateStats()
        {
            dtgAll.ColumnCount = 2;

            dtgAll.Columns[0].Name = "parameter";
            dtgAll.Columns[1].Name = "value";

            dtgAll.RowCount = 4;

            dtgAll.Rows[0].Cells[0].Value = "max ampl";

            if (result.values!=null)
                dtgAll.Rows[0].Cells[1].Value = result.values.Max().ToString();
            else
                dtgAll.Rows[0].Cells[1].Value = "-";


            dtgAll.Rows[1].Cells[0].Value = "mean ampl";

            if (result.values != null)
                dtgAll.Rows[1].Cells[1].Value = result.values.Average().ToString();
            else
                dtgAll.Rows[1].Cells[1].Value = "-";


            dtgAll.Rows[2].Cells[0].Value = "marks generated";

            if (marksPositions != null && marksPositions.Count > 0)
                dtgAll.Rows[2].Cells[1].Value = marksPositions.Count.ToString();
            else
                dtgAll.Rows[2].Cells[1].Value = "-";


            dtgAll.Rows[3].Cells[0].Value = "marks filtered";

            if (marksPositions != null && marksPositions.Count > 0)
                dtgAll.Rows[3].Cells[1].Value = numOfMarksFiltered.ToString();
            else
                dtgAll.Rows[3].Cells[1].Value = "-";

            if (marksPositions == null || marksPositions.Count < 1)
                return;


            dtgStats.ColumnCount = 6;
            dtgStats.Columns[0].Name = "mark";
            dtgStats.Columns[1].Name = "length [ms]";
            dtgStats.Columns[2].Name = "mean ampl";
            dtgStats.Columns[3].Name = "max ampl";
            dtgStats.Columns[4].Name = "time in signal [sec]";
            dtgStats.Columns[5].Name = "MSE";

            dtgStats.RowCount = marksPositions.Count;

            for (int i = 0; i < marksPositions.Count; i++)
            {
                dtgStats.Rows[i].Cells[0].Value = marksPositions[i].info.ToString();
                dtgStats.Rows[i].Cells[1].Value = GetTimeLengthFromSamples(marksPositions[i].rightIndex - marksPositions[i].leftIndex).ToString();

                float[] sigInMark = new float[marksPositions[i].rightIndex - marksPositions[i].leftIndex];

                if (rdbViewArea.Checked)
                    Array.Copy(result.values, marksPositions[i].leftIndex - viewL, sigInMark, 0, sigInMark.Length);
                else
                    Array.Copy(result.values, marksPositions[i].leftIndex, sigInMark, 0, sigInMark.Length);

                dtgStats.Rows[i].Cells[2].Value = sigInMark.Average().ToString();
                dtgStats.Rows[i].Cells[3].Value = sigInMark.Max().ToString();
                dtgStats.Rows[i].Cells[4].Value = (GetTimeLengthFromSamples(marksPositions[i].leftIndex) / 1000).ToString("0.00") + " - " + (GetTimeLengthFromSamples(marksPositions[i].rightIndex) / 1000).ToString("0.00");

                double suma = 0;

                if (sigInMark.Length > 1)
                {
                    double avg = sigInMark.Average();

                    for (int j = 0; j < sigInMark.Length; j++)
                    {
                        suma = Math.Pow(sigInMark[j] - avg, 2);
                    }
                }
                dtgStats.Rows[i].Cells[5].Value = Math.Sqrt(suma);
            }

            dtgStats.ReadOnly = true;
        }

        private void tabControl1_Click(object sender, EventArgs e)
        {
            CreateStats();
        }

        private void chbArtMarks_CheckedChanged(object sender, EventArgs e)
        {
            btnSelectArtMarks.Enabled = (chbArtMarks.Checked) ? true : false;

            if (ArtMarks == null || ArtMarks.Count == 0)
                btnSelectArtMarks.PerformClick();
            else
                pbx.Refresh();
        }



        private void chbStartMarksLength_CheckedChanged(object sender, EventArgs e)
        {
            nudStartMarkLength.Enabled = (chbStartMarksLength.Checked ? true : false);
        }

        private void chbInfo_CheckedChanged(object sender, EventArgs e)
        {
            pbx.Refresh();
        }

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save an Image File";
            saveDialog.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";

            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Bitmap bmp = new Bitmap(pbx.Width, pbx.Height);

                pbx.DrawToBitmap(bmp, pbx.ClientRectangle);
                bmp.Save(saveDialog.FileName);
            }
        }

        private void rdbBandPass_CheckedChanged(object sender, EventArgs e)
        {
            //if (rdbBandPass.Checked)
            //    rdbBandStop.Checked = false;
            //else
            //    rdbBandStop.Checked = true;

            fftRecompRequired = true;

            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync();
        }

        private void rdbBandStop_CheckedChanged(object sender, EventArgs e)
        {
            //if (rdbBandPass.Checked)
            //    rdbBandStop.Checked = false;
            //else
            //    rdbBandStop.Checked = true;

            fftRecompRequired = true;

            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync();
        }

        private void chbEnvelope_CheckedChanged(object sender, EventArgs e)
        {
            fftRecompRequired = true;

            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync();
        }

        private void chbAbs_CheckedChanged(object sender, EventArgs e)
        {
            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync();
        }

        private void chbPower_CheckedChanged_1(object sender, EventArgs e)
        {
            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync();
        }

        private void btnExportSignal_Click(object sender, EventArgs e)
        {
            float[] values;

            if (chbAverage.Checked)
                values = averagedResult.values;
            else
                values = result.values;

            exported++;

            if (values != null)
            {
                exported++;
                string str = "exported " + exported.ToString();
                if (!rdbViewArea.Checked)
                {
                    graphPanel gp = new graphPanel(str, values.ToList(), mv);
                    mv.addNewChannel(gp, 0, true);
                    mv.safe_refresh_channel_list();
                }
                else
                    MessageBox.Show("Compute whole signal first");
            }
        }

        private void chbAverage_CheckedChanged(object sender, EventArgs e)
        {
            if (presetLoading)
                return;

            if (chbAverage.Checked)
                Average(result);

            pbx.Refresh();
            nudAverageFilter.Enabled = !nudAverageFilter.Enabled;
        }

        private void nudAverageFilter_ValueChanged(object sender, EventArgs e)
        {
            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
                bgw.CancelAsync();

            refrControls();
        }

        private void btnWinLP_Click(object sender, EventArgs e)
        {
            signalViewer.windowFunc winfunctionDialogWindow = new signalViewer.windowFunc();

            winfunctionDialogWindow.wf.winType = pluginWindowFunctionLP.winType;

            winfunctionDialogWindow.wf.winParam = pluginWindowFunctionLP.winParam;

            winfunctionDialogWindow.Location = new Point(btnWinLP.Left + this.ParentForm.Location.X, btnWinLP.Top + this.ParentForm.Location.Y);


            if (winfunctionDialogWindow.ShowDialog() == DialogResult.OK)
            {
                string str = signalViewer.WindowingFunction.windowNames[winfunctionDialogWindow.wf.winType];

                if (signalViewer.WindowingFunction.numParams[winfunctionDialogWindow.wf.winType] > 0)
                {
                    str += " (" + winfunctionDialogWindow.wf.winParam.ToString() + ")";
                }

                btnWinLP.Text = str;

                if (pluginWindowFunctionLP.winType != winfunctionDialogWindow.wf.winType)
                {                       
                    pluginWindowFunctionLP.winType = winfunctionDialogWindow.wf.winType;
                    pluginWindowFunctionLP.winParam = winfunctionDialogWindow.wf.winParam;

                    fftRecompRequired = true;

                    bgw.RunWorkerAsync();
                }

            }
        }
    }
}
