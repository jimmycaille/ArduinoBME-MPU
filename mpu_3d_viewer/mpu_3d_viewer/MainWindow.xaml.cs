﻿using System;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;

namespace mpu_3d_viewer {
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        //Path to the model file
        private const string MODEL_PATH = "Model/Model.stl";

        double angX, angY, angZ;
        ModelVisual3D device3D;
        public MainWindow() {
            InitializeComponent();
            device3D = new ModelVisual3D();
            device3D.Content = Display3d(MODEL_PATH);
            // Add to view port
            viewPort3d.Children.Add(device3D);
        }
        /// <summary>
        /// Display 3D Model
        /// </summary>
        /// <param name="model">Path to the Model file</param>
        /// <returns>3D Model Content</returns>
        private Model3D Display3d(string model) {
            Model3D device = null;
            try {
                //Adding a gesture here
                viewPort3d.RotateGesture = new MouseGesture(MouseAction.LeftClick);

                //Import 3D model file
                ModelImporter import = new ModelImporter();

                //Load the 3D model file
                device = import.Load(model);
            } catch (Exception e) {
                // Handle exception in case can not file 3D model
                MessageBox.Show("Exception Error : " + e.StackTrace);
            }
            return device;
        }
        private void performRotation() {
            Transform3DGroup transforms = new Transform3DGroup();
            // Rotation around X
            transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), angX)));
            // Rotation around Y 
            transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), angY)));
            // Rotation around Z
            transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), angZ)));
            // Translate transform (if required)
            transforms.Children.Add(new TranslateTransform3D());
            device3D.Transform = transforms;
        }
        bool connected = false;
        int port_timeout = 2000;
        SerialPort port;
        Thread serialThread;
        bool firstConnect = true;
        bool continueSerialThread = true;
        private void enableControls(bool enable = true) {
            txtPitch.IsEnabled = enable;
            txtRoll.IsEnabled = enable;
            txtYaw.IsEnabled = enable;
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
                    btnConnect.Content = "Disconnect";
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
                btnConnect.Content = "Connect";
                //disable controls
                enableControls(false);
            }
        }
        
        
        private void serialThreadFct() {
            double x, y, z, pitch = 0, roll = 0, yaw = 0;
            double xh = 0, yh = 0, zh = 0,
                   xh2 = 0, yh2 = 0;
            while (continueSerialThread) {
                try {
                    String input = port.ReadLine();
                    String[] split = input.Split(' ');

                    if (split.Length == 17) {
                        //update ui
                        Dispatcher.Invoke(new Action(()=> {
                            double.TryParse(split[0], out x);
                            double.TryParse(split[1], out y);
                            double.TryParse(split[2], out z);
                            double.TryParse(split[6], out xh);
                            double.TryParse(split[7], out yh);
                            double.TryParse(split[8], out zh);

                            roll = Math.Atan2(-x, Math.Sqrt(y * y + z * z));
                            pitch = Math.Atan2(y, Math.Sqrt(x * x + z * z));
                            yh2 = (yh * Math.Cos(roll)) - (zh * Math.Sin(roll));
                            xh2 = (xh * Math.Cos(pitch)) + (yh * Math.Sin(roll) * Math.Sin(pitch)) + (zh * Math.Cos(roll) * Math.Sin(pitch));
                            yaw = Math.Atan2(yh2, xh2);

                            txtPitch.Text = (pitch * 57.3).ToString();
                            txtRoll.Text = (roll * 57.3).ToString();
                            txtYaw.Text = (yaw * 57.3).ToString();

                            if (chkReverseY.IsChecked ?? false) {
                                angZ = yaw * 57.3;
                            } else {
                                angZ = -yaw * 57.3;
                            }
                            if (chkReverseR.IsChecked ?? false) {
                                angX = -roll * 57.3;
                            } else {
                                angX = roll * 57.3;
                            }
                            if (chkReverseY.IsChecked ?? false) {
                                angY = pitch * 57.3;
                            } else {
                                angY = -pitch * 57.3;
                            }
                            performRotation();

                            if (firstConnect) {
                                firstConnect = false;
                                //trackSample.Value = Int32.Parse(split[12]);
                            }
                        }));
                    }
                } catch {
                    //timeout
                }
            }
        }
    }
}
