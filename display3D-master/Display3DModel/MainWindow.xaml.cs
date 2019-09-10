using HelixToolkit.Wpf;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Display3DModel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Path to the model file
        private const string MODEL_PATH = "Model/Model.stl";
        ModelVisual3D device3D;
        public MainWindow()
        {
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
        private Model3D Display3d(string model)
        {
            Model3D device = null;
            try
            {
                //Adding a gesture here
                viewPort3d.RotateGesture = new MouseGesture(MouseAction.LeftClick);

                //Import 3D model file
                ModelImporter import = new ModelImporter();

                //Load the 3D model file
                device = import.Load(model);
            }
            catch (Exception e)
            {
                // Handle exception in case can not file 3D model
                MessageBox.Show("Exception Error : " + e.StackTrace);
            }
            return device;
        }
        int angX, angY, angZ;



        private void Button_Click(object sender, RoutedEventArgs e) {
            angX += 10;
            performRotation();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e) {
            angY += 10;
            performRotation();
        }
        private void Button_Click_2(object sender, RoutedEventArgs e) {
            angZ += 10;
            performRotation();
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
    }
}