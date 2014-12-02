using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OaktreeLab.IO;

namespace FlashAirSample {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private FlashAirIO dev;

        private void Form1_Load( object sender, EventArgs e ) {
            dev = new FlashAirIO( "flashair1" );

            dev.Pins[ 1 ].mode = FlashAirIO.IOMode.Read;
            dev.Pins[ 2 ].mode = FlashAirIO.IOMode.Write;
            dev.Pins[ 7 ].mode = FlashAirIO.IOMode.Write;
            dev.Pins[ 8 ].mode = FlashAirIO.IOMode.Write;
            dev.Pins[ 9 ].mode = FlashAirIO.IOMode.Write;
        }

        private void buttonDoIO_Click( object sender, EventArgs e ) {
            dev.Pins[ 1 ].value = checkBoxPin1.Checked ? 1 : 0;
            dev.Pins[ 2 ].value = checkBoxPin2.Checked ? 1 : 0;
            dev.Pins[ 7 ].value = checkBoxPin7.Checked ? 1 : 0;
            dev.Pins[ 8 ].value = checkBoxPin8.Checked ? 1 : 0;
            dev.Pins[ 9 ].value = checkBoxPin9.Checked ? 1 : 0;

            dev.GPIOReadWrite();

            checkBoxPin1.Checked = dev.Pins[ 1 ].value != 0 ? true : false;
            checkBoxPin2.Checked = dev.Pins[ 2 ].value != 0 ? true : false;
            checkBoxPin7.Checked = dev.Pins[ 7 ].value != 0 ? true : false;
            checkBoxPin8.Checked = dev.Pins[ 8 ].value != 0 ? true : false;
            checkBoxPin9.Checked = dev.Pins[ 9 ].value != 0 ? true : false;
        }

        private void buttonReadShm_Click( object sender, EventArgs e ) {
            textBoxData.Text = dev.ReadSharedMemory( 0, 16 );
        }

        private void buttonWriteShm_Click( object sender, EventArgs e ) {
            dev.WriteSharedMemory( 0, textBoxData.Text );
        }

        private void buttonGetVersion_Click( object sender, EventArgs e ) {
            textBoxVersion.Text = dev.GetVersion();
        }

    }
}
