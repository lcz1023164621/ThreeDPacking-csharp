using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Grab_Callback_CS
{
    static class Grab_Callback
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Grab_Callback_CS());
        }
    }
}
