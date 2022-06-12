using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace FlashProw
{
    class RegRead
    {
        [DllImport("advapi32.dll", EntryPoint = "RegQueryInfoKey", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        private static extern int RegQueryInfoKey(
            UIntPtr hkey,
            out StringBuilder lpClass,
            ref uint lpcbClass,
            IntPtr lpReserved,
            out uint lpcSubKeys,
            out uint lpcbMaxSubKeyLen,
            out uint lpcbMaxClassLen,
            out uint lpcValues,
            out uint lpcbMaxValueNameLen,
            out uint lpcbMaxValueLen,
            out uint lpcbSecurityDescriptor,
            ref FILETIME lpftLastWriteTime);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(UIntPtr hKey);


        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RegOpenKeyEx(
          UIntPtr hKey,
          string subKey,
          int ulOptions,
          int samDesired,
          out UIntPtr hkResult);



        private static DateTime ToDateTime(FILETIME ft)
        {
            IntPtr buf = IntPtr.Zero;
            try
            {
                long[] longArray = new long[1];
                int cb = Marshal.SizeOf(ft);
                buf = Marshal.AllocHGlobal(cb);
                Marshal.StructureToPtr(ft, buf, false);
                Marshal.Copy(buf, longArray, 0, 1);
                return DateTime.FromFileTime(longArray[0]);
            }
            finally
            {
                if (buf != IntPtr.Zero) Marshal.FreeHGlobal(buf);
            }
        }

        public static DateTime? GetDateModified(RegistryHive registryHive, string path)
        {
            var lastModified = new FILETIME();
            var lpcbClass = new uint();
            var lpReserved = new IntPtr();
            UIntPtr key = UIntPtr.Zero;

            try
            {
                try
                {
                    var hive = new UIntPtr(unchecked((uint)registryHive));
                    if (RegOpenKeyEx(hive, path, 0, (int)RegistryRights.ReadKey, out key) != 0)
                    {
                        return null;
                    }

                    uint lpcbSubKeys;
                    uint lpcbMaxKeyLen;
                    uint lpcbMaxClassLen;
                    uint lpcValues;
                    uint maxValueName;
                    uint maxValueLen;
                    uint securityDescriptor;
                    StringBuilder sb;
                    if (RegQueryInfoKey(
                                 key,
                                 out sb,
                                 ref lpcbClass,
                                 lpReserved,
                                 out lpcbSubKeys,
                                 out lpcbMaxKeyLen,
                                 out lpcbMaxClassLen,
                                 out lpcValues,
                                 out maxValueName,
                                 out maxValueLen,
                                 out securityDescriptor,
                                 ref lastModified) != 0)
                    {
                        return null;
                    }

                    var result = ToDateTime(lastModified);
                    return result;
                }
                finally
                {
                    if (key != UIntPtr.Zero)
                        RegCloseKey(key);

                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<Dev_Machine> GetDevs()
        {
            RegistryKey myKey = Registry.LocalMachine;
            RegistryKey usbstor = myKey.OpenSubKey("SYSTEM").OpenSubKey("CurrentControlSet").OpenSubKey("Enum");
            List<Dev_Machine> dml = new List<Dev_Machine>();
            foreach (string str in usbstor.OpenSubKey("USBSTOR").GetSubKeyNames())
            {
                RegistryKey work = usbstor.OpenSubKey("USBSTOR").OpenSubKey(str.ToString());
                foreach (string strin in work.GetSubKeyNames())
                {
                    Dev_Machine dm = new Dev_Machine();
                    if (strin.Length > 2)
                        dm.SetId(strin.Substring(0, strin.Length - 2));

                    dm.SetName(work.OpenSubKey(strin).GetValue("FriendlyName"));

                    object sa = work.OpenSubKey(strin).GetValue("HardwareID");
                    if (sa == null)
                        dm.SetError();
                    else
                    {
                        string[] s = ((IEnumerable)sa).Cast<object>()
                             .Select(x => x.ToString())
                             .ToArray();

                        if (s[s.Length - 1].Trim().ToLower().Contains("gendisk"))
                            dm.SetTypeDev(true);
                    }

                    var dateModified = GetDateModified(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Enum\USBSTOR\" + str + @"\" + strin);

                    if (dateModified.HasValue)
                        dm.SetDate(dateModified.Value);
                    else
                        dm.SetError();
                    
                    if (dm.GetTypeDev())
                    {
                        var dateModifiedUnder = GetDateModified(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Enum\USBSTOR\" + str + @"\" + strin + @"\Device Parameters\Partmgr");
                        
                        if (dateModifiedUnder.HasValue)
                            dm.SetDate(dateModifiedUnder.Value);
                        

                    }

                    int isset = -1;
                    for (int i = 0; i < dml.Count; i++)
                    {
                        if (dml[i].GetId().Equals(dm.GetId()))
                        {
                            isset = i;
                        }
                    }
                    if (isset == -1)
                    {
                        dml.Add(dm);
                    }
                    else
                    {
                        if ((dm.GetDateOrig() != null) && ((dml[isset].GetDateOrig() == null) || (dm.GetDateOrig() > dml[isset].GetDateOrig())))
                        {
                            dml[isset].SetDate(dm.GetDateOrig());
                        }
                    }
                }
            }

            foreach (string str in usbstor.OpenSubKey("USB").GetSubKeyNames())
            {
                RegistryKey work = usbstor.OpenSubKey("USB").OpenSubKey(str.ToString());
                foreach (string strin in work.GetSubKeyNames())
                {
                    Dev_Machine dm = new Dev_Machine();

                    bool add = false;

                    int len = strin.LastIndexOf('&');
                    if (len < 1)
                        len = strin.Length;

                    if (strin.Length > 2)
                        dm.SetId(strin.Substring(0, len));

                    dm.SetName(work.OpenSubKey(strin).GetValue("FriendlyName"));

                    object sa = work.OpenSubKey(strin).GetValue("LowerFilters");
                    if (sa != null)
                    {
                        string[] s = ((IEnumerable)sa).Cast<object>()
                             .Select(x => x.ToString())
                             .ToArray();
                        if (s[s.Length - 1].Trim().ToLower().Equals("winusb"))
                            add = true;
                    }

                    var dateModified = GetDateModified(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Enum\USB\" + str + @"\" + strin);

                    if (dateModified.HasValue)
                        dm.SetDate(dateModified.Value);
                    else
                        dm.SetError();
                    
                    if (dm.GetName().Trim().ToLower().Equals("n/a"))
                    {
                        string s = work.OpenSubKey(strin).GetValue("DeviceDesc").ToString();
                        if (!s.StartsWith("@"))
                            dm.SetName(s);
                    }

                    if (add)
                    {
                        int isset = -1;
                        for (int i = 0; i < dml.Count; i++)
                        {
                            if (dml[i].GetId().Equals(dm.GetId()))
                            {
                                isset = i;
                            }
                        }
                        if (isset == -1)
                        {
                            dml.Add(dm);
                        }
                        else
                        {
                            if ((dm.GetDateOrig() != null) && ((dml[isset].GetDateOrig() == null) || (dm.GetDateOrig() > dml[isset].GetDateOrig())))
                            {
                                dml[isset].SetDate(dm.GetDateOrig());
                            }
                        }
                    }
                }
            }

            BaseXML xml = new BaseXML();
            List<Dev_Acs> lda = xml.GetDev();
            foreach (Dev_Machine dm in dml)
            {

                foreach (Dev_Acs da in lda)
                    if (da.GetId().Equals(dm.GetId()))
                    {
                        dm.SetAcs(true);
                        break;
                    }

            }

            List<int> dels = new List<int>();

            for (int i = dml.Count - 1; i >= 0; i--)
                if (dml[i].GetAcs())
                    dels.Add(i);

            foreach (int del in dels)
                dml.RemoveAt(del);

            return dml;

        }
    }

    public class Dev_Machine
    {
        private string Id;
        private string ViewId;
        private string Name;
        private DateTime Date;
        private bool Err;
        private bool Acs;
        private bool Flash;

        public Dev_Machine()
        {
            ViewId = "N/A";
            Name = "N/A";
            Err = false;
            Acs = false;
            Flash = false;
        }

        public void SetId(string id)
        {
            Id = id;
            if (id.Contains("&"))
                ViewId += ", parentid prefix: " + id;
            else
            {
                ViewId = id;
            }
        }

        public void SetName(object name)
        {
            if (name != null)
                Name = name.ToString();
            else
                Err = true;
        }

        public void SetTypeDev(bool type)
        {
            Flash = type;
        }

        public void SetError()
        {
            Err = true;
        }

        public void SetAcs(bool acs)
        {
            Acs = acs;
        }

        public void SetDate(DateTime date)
        {
            if (Date == null)
            {
                Date = date;
                return;
            }

            if (Date < date)
                Date = date;
        }

        public bool GetError()
        {
            return Err;
        }

        public bool GetTypeDev()
        {
            return Flash;
        }

        public bool GetAcs()
        {
            return Acs;
        }

        public string GetViewId()
        {
            return ViewId;
        }

        public string GetId()
        {
            return Id;
        }

        public string GetName()
        {
            return Name;
        }

        public string GetDate()
        {
            return (Date == null) ? "" : String.Format("{0}-{1:d2}-{2:d2} {3:d2}:{4:d2}", Date.Year, Date.Month, Date.Day, Date.Hour, Date.Minute);
        }

        public DateTime GetDateOrig()
        {
            return Date;
        }

    }

    public class Dev_Acs
    {
        private string Id;
        private string Name;
        private string FName;
        private bool Del;
        
        public Dev_Acs(string id, string fname, string name)
        {
            Id = id;
            FName = fname;
            Name = name;
            Del = false;
        }

        public string GetId() {
            return Id;
        }

        public string GetName() {
            return Name;
        }

        public string GetFName() {
            return FName;
        }

        public void SetDel(bool del) {
            Del = del;
        }

        public bool GetDel() {
            return Del;
        }
    }

    public class BaseXML
    {
        public BaseXML()
        {
            

        }

        public void AddDev(string n_id, string n_fname, string n_name)
        {
            List<Dev_Acs> lda = new List<Dev_Acs>();

            XmlDocument document;
            XmlElement devs;

            if (File.Exists("db.xml"))
            {
                document = new XmlDocument();
                document.Load("db.xml");

                devs = document.DocumentElement;



                foreach (XmlElement dev_r in devs.ChildNodes)
                {
                    lda.Add(new Dev_Acs(dev_r.GetAttribute("id"), dev_r.GetAttribute("fname"), dev_r.GetAttribute("name")));
                }
            }

            lda.Add(new Dev_Acs(n_id, n_fname, n_name));

            document = new XmlDocument();
            XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
            document.AppendChild(xmlDeclaration);
            devs = document.CreateElement("devs");

            foreach (Dev_Acs da in lda)
            {
                XmlElement dev_w = document.CreateElement("dev");

                dev_w.SetAttribute("id", da.GetId());
                dev_w.SetAttribute("fname", da.GetFName());
                dev_w.SetAttribute("name", da.GetName());

                devs.AppendChild(dev_w);
            }

            document.AppendChild(devs);

            document.Save("db.xml");

        }

        public void DelDev(List<Dev_Acs> lda)
        {
            XmlDocument document = new XmlDocument();
            XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
            document.AppendChild(xmlDeclaration);
            XmlElement devs = document.CreateElement("devs");

            foreach (Dev_Acs da in lda)
            {
                if (!da.GetDel())
                {
                    XmlElement dev_w = document.CreateElement("dev");

                    dev_w.SetAttribute("id", da.GetId());
                    dev_w.SetAttribute("fname", da.GetFName());
                    dev_w.SetAttribute("name", da.GetName());

                    devs.AppendChild(dev_w);
                }
            }

            document.AppendChild(devs);

            document.Save("db.xml");

        }

        public List<Dev_Acs> GetDev()
        {
            List<Dev_Acs> lda = new List<Dev_Acs>();

            XmlDocument document;
            XmlElement devs;

            if (File.Exists("db.xml"))
            {
                document = new XmlDocument();
                document.Load("db.xml");

                devs = document.DocumentElement;



                foreach (XmlElement dev_r in devs.ChildNodes)
                {
                    lda.Add(new Dev_Acs(dev_r.GetAttribute("id"), dev_r.GetAttribute("fname"), dev_r.GetAttribute("name")));
                }
            }

            return lda;
        }

    }

}
