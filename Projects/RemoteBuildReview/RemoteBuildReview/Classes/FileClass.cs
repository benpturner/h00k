using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace RemoteBuildReview
{

    public class FileClass
    {


        public static ArrayList filesfolders = new ArrayList();

        public static void TreeScan(string sDir)
        {
            filesfolders.Add(sDir);
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                    filesfolders.Add(f);
            }
            catch { return; }
            foreach (string d in Directory.GetDirectories(sDir))
                TreeScan(d);
        }

        public static void getperms(string folder, string outputfile)
        {
            DataSet1 testdataset = new DataSet1();
            TreeScan(folder);

            foreach (string item in filesfolders)
            {
                FileSecurity fileSecurity = new FileSecurity(item, AccessControlSections.Access);
                AuthorizationRuleCollection arc = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));
                foreach (FileSystemAccessRule rule in arc)
                {
                    testdataset.Tables["Permissions"].Rows.Add(item, rule.IdentityReference, rule.AccessControlType, rule.FileSystemRights);
                }
            }

            testdataset.WriteXml(outputfile);
        }
        

    }
}
