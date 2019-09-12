using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace bme_mpu_viewer {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();

        }
        bool connected = false;
        int port_timeout = 2000;
        SerialPort port;
        Thread serialThread;
        bool firstConnect = true;
        bool continueSerialThread = true;
        private void enableControls(bool enable = true) {
            boxCOM.Enabled = !enable;
            boxSpeed.Enabled = !enable;

            btnClear1.Enabled = enable;
            btnClear2.Enabled = enable;
            btnClear3.Enabled = enable;
            btnClear4.Enabled = enable;
            btnConfig.Enabled = enable;

            boxGyro.Enabled  = enable;
            boxAcc.Enabled   = enable;

            boxOSAcc.Enabled  = enable;
            boxOSGyro.Enabled = enable;
            boxOSTemp.Enabled = enable;

            boxIIR.Enabled     = enable;
            boxMode.Enabled    = enable;
            boxStandby.Enabled = enable;

            trackSample.Enabled = enable;
        }
        private void btnConnect_Click(object sender, EventArgs e) {
            if (!connected) {
                port = new SerialPort(boxCOM.Text, Int32.Parse(boxSpeed.Text));
                port.ReadTimeout = port_timeout;
                try {
                    port.Open();
                    connected = true;
                    btnConnect.Text = "Disconnect";
                    //enable controls
                    enableControls();
                    //start thread
                    firstConnect = true;
                    serialThread = new Thread(new ThreadStart(serialThreadFct));
                    continueSerialThread = true;
                    serialThread.Start();
                } catch {
                    //get ports
                    string[] ports = SerialPort.GetPortNames();
                    //empty list and add them to the list
                    this.boxCOM.Items.Clear();
                    for (int i = 0; i < ports.Length; i++) {
                        boxCOM.Items.Add(ports[i]);
                    }
                    //if there is items
                    if (boxCOM.Items.Count > 0) {
                        //select first
                        boxCOM.SelectedIndex = 0;
                    }
                }
            } else {
                continueSerialThread = false;
                port.Close();
                connected = false;
                btnConnect.Text = "Connect";
                //disable controls
                enableControls(false);
            }
        }

        private void serialThreadFct() {
            while (continueSerialThread) {
                try {
                    String input = port.ReadLine();
                    String[] split = input.Split(' ');

                    if (split.Length == 17) {
                        //update ui
                        this.Invoke((MethodInvoker)delegate {
                            //boxes
                            txtAccX.Text = split[0];
                            txtAccY.Text = split[1];
                            txtAccZ.Text = split[2];
                            double x, y, z;
                            if (double.TryParse(split[0], out x) &&
                                double.TryParse(split[1], out y) &&
                                double.TryParse(split[2], out z)) {
                                    txtAccR.Text = Math.Sqrt(Math.Pow(x, 2) +
                                                             Math.Pow(y, 2) +
                                                             Math.Pow(z, 2)).ToString();
                            }
                            txtGyrX.Text = split[3];
                            txtGyrY.Text = split[4];
                            txtGyrZ.Text = split[5];
                            txtMagX.Text = split[6];
                            txtMagY.Text = split[7];
                            txtMagZ.Text = split[8];
                            txtMPUtemp.Text = split[9];
                            //gfs afs
                            txtSample.Text = split[12];
                            txtBMEtemp.Text = split[13];
                            txtPres.Text = split[14];
                            txtAlt.Text = split[15];
                            txtHumid.Text = split[16];
                            if (firstConnect) {
                                firstConnect = false;
                                trackSample.Value = Int32.Parse(split[12]);
                            }
                            //charts
                            chartAcc.Series[0].Points.AddY(split[0]);
                            chartAcc.Series[1].Points.AddY(split[1]);
                            chartAcc.Series[2].Points.AddY(split[2]);
                            chartGyr.Series[0].Points.AddY(split[3]);
                            chartGyr.Series[1].Points.AddY(split[4]);
                            chartGyr.Series[2].Points.AddY(split[5]);
                            chartMag.Series[0].Points.AddY(split[6]);
                            chartMag.Series[1].Points.AddY(split[7]);
                            chartMag.Series[2].Points.AddY(split[8]);
                            chartBME.Series[0].Points.AddY(split[13]);
                            chartBME.Series[1].Points.AddY(split[14]);
                            chartBME.Series[2].Points.AddY(split[15]);
                            chartBME.Series[3].Points.AddY(split[16]);
                        });
                    }
                } catch {
                    //timeout
                }
            }
        }

        private void btnClear1_Click(object sender, EventArgs e) {
            chartAcc.Series[0].Points.Clear();
            chartAcc.Series[1].Points.Clear();
            chartAcc.Series[2].Points.Clear();
        }

        private void btnClear2_Click(object sender, EventArgs e) {
            chartGyr.Series[0].Points.Clear();
            chartGyr.Series[1].Points.Clear();
            chartGyr.Series[2].Points.Clear();
        }

        private void btnClear3_Click(object sender, EventArgs e) {
            chartMag.Series[0].Points.Clear();
            chartMag.Series[1].Points.Clear();
            chartMag.Series[2].Points.Clear();
        }

        private void btnClear4_Click(object sender, EventArgs e) {
            chartBME.Series[0].Points.Clear();
            chartBME.Series[1].Points.Clear();
            chartBME.Series[2].Points.Clear();
            chartBME.Series[3].Points.Clear();
        }

        private void btnConfig_Click(object sender, EventArgs e) {
            String output = ""; //     TODOTODOTODOTODOTODOTODOTODOTODOTODOTODOTODOTODOTODO
            Console.WriteLine(output);

            if (connected) {
                port.Write(output);
            }
        }

        private void trackSample_Scroll(object sender, EventArgs e) {
            txtSample.Text = trackSample.Value.ToString();
        }
        //https://stackoverflow.com/questions/15805121/c-sharp-winforms-creating-a-chart-with-multiple-y-axis-3-or-more
        private void chkSeparateAxis_CheckedChanged(object sender, EventArgs e) {
            if (chkSeparateAxis.Checked) {
                // Set custom chart area position
                chartBME.ChartAreas["ChartArea1"].Position = new ElementPosition(25, 10, 68, 85);
                chartBME.ChartAreas["ChartArea1"].InnerPlotPosition = new ElementPosition(10, 0, 90, 90);

                // Create extra Y axis for second and third series
                CreateYAxis(chartBME, chartBME.ChartAreas["ChartArea1"], chartBME.Series[2], 13, 8);
                CreateYAxis(chartBME, chartBME.ChartAreas["ChartArea1"], chartBME.Series[3], 22, 8);
                CreateYAxis(chartBME, chartBME.ChartAreas["ChartArea1"], chartBME.Series[4], 24, 8);
            } else {
                // Set default chart areas
                chartBME.Series[0].ChartArea = "ChartArea1";
                chartBME.Series[1].ChartArea = "ChartArea1";
                chartBME.Series[2].ChartArea = "ChartArea1";

                // Remove newly created series and chart areas
                while (chartBME.Series.Count > 3) {
                    chartBME.Series.RemoveAt(3);
                }
                while (chartBME.ChartAreas.Count > 1) {
                    chartBME.ChartAreas.RemoveAt(1);
                }

                // Set default chart are position to Auto
                chartBME.ChartAreas["ChartArea1"].Position.Auto = true;
                chartBME.ChartAreas["ChartArea1"].InnerPlotPosition.Auto = true;

            }
        }
        public void CreateYAxis(Chart chart, ChartArea area, Series series, float axisOffset, float labelsSize) {
            // Create new chart area for original series
            ChartArea areaSeries = chart.ChartAreas.Add("ChartArea_" + series.Name);
            areaSeries.BackColor = Color.Transparent;
            areaSeries.BorderColor = Color.Transparent;
            areaSeries.Position.FromRectangleF(area.Position.ToRectangleF());
            areaSeries.InnerPlotPosition.FromRectangleF(area.InnerPlotPosition.ToRectangleF());
            areaSeries.AxisX.MajorGrid.Enabled = false;
            areaSeries.AxisX.MajorTickMark.Enabled = false;
            areaSeries.AxisX.LabelStyle.Enabled = false;
            areaSeries.AxisY.MajorGrid.Enabled = false;
            areaSeries.AxisY.MajorTickMark.Enabled = false;
            areaSeries.AxisY.LabelStyle.Enabled = false;
            areaSeries.AxisY.IsStartedFromZero = area.AxisY.IsStartedFromZero;


            series.ChartArea = areaSeries.Name;

            // Create new chart area for axis
            ChartArea areaAxis = chart.ChartAreas.Add("AxisY_" + series.ChartArea);
            areaAxis.BackColor = Color.Transparent;
            areaAxis.BorderColor = Color.Transparent;
            areaAxis.Position.FromRectangleF(chart.ChartAreas[series.ChartArea].Position.ToRectangleF());
            areaAxis.InnerPlotPosition.FromRectangleF(chart.ChartAreas[series.ChartArea].InnerPlotPosition.ToRectangleF());

            // Create a copy of specified series
            Series seriesCopy = chart.Series.Add(series.Name + "_Copy");
            seriesCopy.ChartType = series.ChartType;
            foreach (DataPoint point in series.Points) {
                seriesCopy.Points.AddXY(point.XValue, point.YValues[0]);
            }

            // Hide copied series
            seriesCopy.IsVisibleInLegend = false;
            seriesCopy.Color = Color.Transparent;
            seriesCopy.BorderColor = Color.Transparent;
            seriesCopy.ChartArea = areaAxis.Name;

            // Disable drid lines & tickmarks
            areaAxis.AxisX.LineWidth = 0;
            areaAxis.AxisX.MajorGrid.Enabled = false;
            areaAxis.AxisX.MajorTickMark.Enabled = false;
            areaAxis.AxisX.LabelStyle.Enabled = false;
            areaAxis.AxisY.MajorGrid.Enabled = false;
            areaAxis.AxisY.IsStartedFromZero = area.AxisY.IsStartedFromZero;
            areaAxis.AxisY.LabelStyle.Font = area.AxisY.LabelStyle.Font;

            // Adjust area position
            areaAxis.Position.X -= axisOffset;
            areaAxis.InnerPlotPosition.X += labelsSize;

        }
    }
}
