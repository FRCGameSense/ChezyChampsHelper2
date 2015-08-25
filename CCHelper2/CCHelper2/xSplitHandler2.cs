using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace CCHelper2
{
    class xSplitHandler2
    {
        private string[] tagsArray;

        public bool checkXsplitLocation(string path)
        {
            //Search for an XSplit install, and show a messagebox if it's not where you expect it to be.
            string xSplitLocation = path + "\\XSplit.Core.exe";

            if (!System.IO.File.Exists(xSplitLocation))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void loadTagsFromXML()
        {
            string xmlFile;

            xmlFile = System.IO.Path.Combine(Properties.Settings.Default.XsplitInstallLocation, "streamcontrol.xml");

            tagsArray = System.IO.File.ReadAllLines(xmlFile);
        }

        public void writeXMLFile()
        {
            string xmlFile;
            xmlFile = System.IO.Path.Combine(Properties.Settings.Default.XsplitInstallLocation, "streamcontrol.xml");

            System.IO.File.WriteAllLines(xmlFile, tagsArray);
        }

        /// <summary>
        /// changes the tag in the streamcontrol.xml file.  NOTE: useCDATA is no longer used and will always be false
        /// </summary>
        /// <param name="tag">string representing the tag to change</param>
        /// <param name="value">the string to set the tag to</param>
        /// <param name="useCDATA">no longer used.</param>
        public void changeXMLTag(string tag, string value, bool useCDATA)
        {
            useCDATA = false;

            string beginTag = " <" + tag + ">";
            string endTag = "</" + tag + ">";

            bool foundTag = false;

            if (tagsArray != null)
            {
                for (int i = 0; i < tagsArray.Length; i++)
                {
                    string line = tagsArray[i];
                    int index = line.IndexOf(beginTag);
                    if (line.IndexOf(beginTag) != -1)
                    {
                        foundTag = true;
                        if (useCDATA)
                        {
                            line = beginTag + "<![CDATA[" + value + "]]>" + endTag;
                        }
                        else
                        {
                            line = beginTag + value + endTag;
                        }

                        tagsArray[i] = line;
                    }
                }
            }
            //if you didn't find the tag, create it.
            if (!foundTag)
            {
                string newLine = null;
                if (useCDATA)
                {
                    newLine = beginTag + "<![CDATA[" + value + "]]>" + endTag;
                }
                else
                {
                    newLine = beginTag + value + endTag;
                }

                List<string> lineList = tagsArray.ToList<string>();
                lineList.Insert(tagsArray.Length - 1, newLine);
                tagsArray = lineList.ToArray();
            }
        }

        public void copyStreamControlXMLToXsplitLocation(string xSplitLocation)
        {
            File.WriteAllText(xSplitLocation + "\\streamcontrol.xml", Properties.Resources.streamcontrol);
        }

        public void backUpStreamControlXML()
        {
            string xmlName = "streamcontrol.xml";
            string backupXmlName = "streamcontrol_backup(" + DateTime.Now.ToString("yy-MM-dd-HHmm") + ").xml";

            string xSplitFile = System.IO.Path.Combine(Properties.Settings.Default.XsplitInstallLocation, xmlName);
            string ccPath = System.IO.Path.Combine(Properties.Settings.Default.GSFolderLocation, "Documents", "streamcontrolbackups");

            var directory = new System.IO.DirectoryInfo(ccPath);
            System.IO.FileInfo latestBackup = null;
            try
            {
                latestBackup = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            string ccFile = System.IO.Path.Combine(latestBackup.DirectoryName, latestBackup.ToString());

            string currXML = System.IO.File.ReadAllText(xSplitFile);
            string latestBackupXML = System.IO.File.ReadAllText(ccFile);

            if (!String.Equals(currXML, latestBackupXML))
            {
                ccFile = System.IO.Path.Combine(ccPath, backupXmlName);

                try
                {
                    System.IO.File.Copy(xSplitFile, ccFile, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
