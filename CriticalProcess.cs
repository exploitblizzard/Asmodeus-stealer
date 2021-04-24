using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace UploadAFile

{
    class CriticalProcess
    {
        [DllImport("ntdll.dll", SetLastError = true)]   //import a critical dll files
        public static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);

    }
}
