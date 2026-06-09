using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MVIDCodeReaderDemo_CS
{
    static class MVIDCodeReaderDemo
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MVIDCodeReaderDemo_CS());
        }
    }
}
