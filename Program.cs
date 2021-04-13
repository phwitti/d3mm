using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace d3mm
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationProperties.Init();
#if NET5_0
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            while (ShowWindow())
            {
                ApplicationProperties.Exit();
                ApplicationProperties.Init();
            }

            ApplicationProperties.Exit();
        }

        static bool ShowWindow()
        {
            try
            {
                D3ModManager d3mm = new D3ModManager();
                Application.Run(d3mm);
                return d3mm.Reopen;
            }
            catch
            {
                return true;
            }
        }
    }
}
