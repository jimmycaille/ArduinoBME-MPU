using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bme_mpu_viewer {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e) {

        }

        private void btnClear1_Click(object sender, EventArgs e) {

        }

        private void btnClear2_Click(object sender, EventArgs e) {

        }

        private void btnClear3_Click(object sender, EventArgs e) {

        }

        private void btnClear4_Click(object sender, EventArgs e) {

        }

        private void btnConfig_Click(object sender, EventArgs e) {

        }

        private void trackSample_Scroll(object sender, EventArgs e) {
            txtSample.Text = trackSample.Value.ToString();
        }
    }
}
