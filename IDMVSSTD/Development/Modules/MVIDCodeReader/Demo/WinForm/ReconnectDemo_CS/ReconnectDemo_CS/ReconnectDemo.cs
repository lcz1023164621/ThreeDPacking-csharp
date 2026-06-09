using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ReconnectDemo_CS
{
    static class ReconnectDemo
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ReconnectDemo_CS());
        }
    }
}
