using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mpu_3d_viewer {
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        bool connected = false;
        int port_timeout = 2000;
        SerialPort port;
        Thread serialThread;
        bool firstConnect = true;
        bool continueSerialThread = true;
        private void enableControls(bool enable = true) {
            /*
            boxCOM.Enabled = !enable;
            boxSpeed.Enabled = !enable;

            btnClear1.Enabled = enable;
            btnClear2.Enabled = enable;
            btnClear3.Enabled = enable;
            btnClear4.Enabled = enable;
            btnConfig.Enabled = enable;

            boxGyro.Enabled = enable;
            boxAcc.Enabled = enable;

            boxOSAcc.Enabled = enable;
            boxOSGyro.Enabled = enable;
            boxOSTemp.Enabled = enable;

            boxIIR.Enabled = enable;
            boxMode.Enabled = enable;
            boxStandby.Enabled = enable;

            trackSample.Enabled = enable;*/
        }
        private void BtnConnect_Click(object sender, RoutedEventArgs e) {
            if (!connected) {
                port = new SerialPort(boxCOM.Text, Int32.Parse(boxSpeed.Text));
                port.ReadTimeout = port_timeout;
                try {
                    port.Open();
                    connected = true;
                    //btnConnect.Text = "Disconnect";
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
                //btnConnect.Text = "Connect";
                //disable controls
                enableControls(false);
            }
        }

        double magXmin = 32767, magXmax = -32768, magXbias = 0,
               magYmin = 32767, magYmax = -32768, magYbias = 0,
               magZmin = 32767, magZmax = -32768, magZbias = 0;
        

        bool applyMagBias, applyMagScale;
        private void serialThreadFct() {
            double x, y, z, pitch = 0, roll = 0;
            double xh = 0, yh = 0, zh = 0,
                   xh2 = 0, yh2 = 0;
            while (continueSerialThread) {
                try {
                    String input = port.ReadLine();
                    String[] split = input.Split(' ');

                    if (split.Length == 17) {
                        //update ui
                        Dispatcher.Invoke(new Action(()=> {

                            /*
                            //boxes
                            txtAccX.Text = split[0];
                            txtAccY.Text = split[1];
                            txtAccZ.Text = split[2];
                            if (double.TryParse(split[0], out x) &&
                                double.TryParse(split[1], out y) &&
                                double.TryParse(split[2], out z)) {
                                txtAccR.Text = Math.Sqrt(Math.Pow(x, 2) +
                                                         Math.Pow(y, 2) +
                                                         Math.Pow(z, 2)).ToString();
                                roll = Math.Atan2(-x, Math.Sqrt(y * y + z * z));
                                //roll = Math.Atan2(y, z);
                                pitch = Math.Atan2(y, Math.Sqrt(x * x + z * z));

                                txtPitch.Text = (pitch * 57.3).ToString();
                                txtRoll.Text = (roll * 57.3).ToString();
                            }
                            if (double.TryParse(split[6], out xh) &&
                                double.TryParse(split[7], out yh) &&
                                double.TryParse(split[8], out zh)) {


                                // yaw from mag
                                yh2 = (yh * Math.Cos(roll)) - (zh * Math.Sin(roll));
                                xh2 = (xh * Math.Cos(pitch)) + (yh * Math.Sin(roll) * Math.Sin(pitch)) + (zh * Math.Cos(roll) * Math.Sin(pitch));

                                txtYaw.Text = (Math.Atan2(yh2, xh2) * 57.3).ToString();
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

                            //mag2

                            if (xh < magXmin) {
                                magXmin = xh;
                                txtMagXmin.Text = magXmin.ToString();
                            }
                            if (xh > magXmax) {
                                magXmax = xh;
                                txtMagXmax.Text = magXmax.ToString();
                            }
                            if (yh < magYmin) {
                                magYmin = yh;
                                txtMagYmin.Text = magYmin.ToString();
                            }
                            if (yh > magYmax) {
                                magYmax = yh;
                                txtMagYmax.Text = magYmax.ToString();
                            }
                            if (zh < magZmin) {
                                magZmin = zh;
                                txtMagZmin.Text = magZmin.ToString();
                            }
                            if (zh > magZmax) {
                                magZmax = zh;
                                txtMagZmax.Text = magZmax.ToString();
                            }

                            //https://github.com/kriswiner/MPU6050/wiki/Simple-and-Effective-Magnetometer-Calibration

                            magXbias = (magXmin + magXmax) / 2;
                            magYbias = (magYmin + magYmax) / 2;
                            magZbias = (magZmin + magZmax) / 2;

                            txtMagXbias.Text = magXbias.ToString();
                            txtMagYbias.Text = magYbias.ToString();
                            txtMagZbias.Text = magZbias.ToString();

                            txtMagXdif.Text = ((magXmax - magXmin) / 2).ToString();
                            txtMagYdif.Text = ((magYmax - magYmin) / 2).ToString();
                            txtMagZdif.Text = ((magZmax - magZmin) / 2).ToString();

                            double magXscale = ((magXmax - magXmin) / 2 + (magYmax - magYmin) / 2 + (magZmax - magZmin) / 2) / 3 / ((magXmax - magXmin) / 2);
                            double magYscale = ((magXmax - magXmin) / 2 + (magYmax - magYmin) / 2 + (magZmax - magZmin) / 2) / 3 / ((magYmax - magYmin) / 2);
                            double magZscale = ((magXmax - magXmin) / 2 + (magYmax - magYmin) / 2 + (magZmax - magZmin) / 2) / 3 / ((magZmax - magZmin) / 2);

                            txtMagXratio.Text = magXscale.ToString();
                            txtMagYratio.Text = magYscale.ToString();
                            txtMagZratio.Text = magZscale.ToString();

                            //apply bias ONLY FOR CHARMAG2, other are raw values !
                            if (applyMagBias) {
                                xh -= magXbias;
                                yh -= magYbias;
                                zh -= magZbias;
                            }
                            if (applyMagScale) {

                                xh *= magXscale;
                                yh *= magYscale;
                                zh *= magZscale;
                            }

                            // yaw2 from mag
                            yh2 = (yh * Math.Cos(roll)) - (zh * Math.Sin(roll));
                            xh2 = (xh * Math.Cos(pitch)) + (yh * Math.Sin(roll) * Math.Sin(pitch)) + (zh * Math.Cos(roll) * Math.Sin(pitch));

                            txtYaw2.Text = (Math.Atan2(yh2, xh2) * 57.3).ToString();

                            chartMag2.Series[0].Points.AddXY(xh, yh);
                            chartMag2.Series[1].Points.AddXY(xh, zh);
                            chartMag2.Series[2].Points.AddXY(yh, zh);
                            */
                        }));
                    }
                } catch {
                    //timeout
                }
            }
        }
    }
}
