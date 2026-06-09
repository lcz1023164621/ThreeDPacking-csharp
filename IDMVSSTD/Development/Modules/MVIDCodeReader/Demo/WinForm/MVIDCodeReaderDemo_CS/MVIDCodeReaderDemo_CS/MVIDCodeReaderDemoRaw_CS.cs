using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MVIDCodeReaderDemo_CS
{
    public partial class MVIDCodeReaderDemoRaw_CS : Form
    {
        public MVIDCodeReaderDemoRaw_CS()
        {
            InitializeComponent();

            comboBoxConvertType.Items.Insert(0, "MONO8");
            comboBoxConvertType.Items.Insert(1, "BGR24");
            comboBoxConvertType.SelectedIndex = 0;
        }

        private void CloseForm_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
