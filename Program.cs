using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TakipsiClient
{
    class Service : ServiceBase
    {
        public Service()
        {
            ServiceName = Program.ServiceName;
        }

        protected override void OnStart(string[] args)
        {
            Program.Start(args);
        }

        protected override void OnStop()
        {
            Program.Stop();
        }
    }

    class Program
    {
        [DllImport("cid.dll", EntryPoint = "CidData", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public static extern string CidData();

        [DllImport("cid.dll", EntryPoint = "CidStart")]
        public static extern string CidStart();

        public const string ServiceName = "TakipsiClient";

        static void Main(string[] args)
        {
            CidStart();

            //Program.Start(args);
            while (true) {
                string temp = "";
                // 
                temp = CidData();
                if (temp != "")
                {
                    Console.WriteLine(temp);
                }
                Console.ReadKey();
            }

            if (!Environment.UserInteractive)
            {
                using (var service = new Service())
                {
                    ServiceBase.Run(service);
                }
            }
            else
            {
                ManagedInstallerClass.InstallHelper(new string[] {
                    Assembly.GetExecutingAssembly().Location
                });
            }
        }

        public static void Start(string[] args)
        {

            Timer aTimer = new Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            aTimer.Enabled = true;
        }

        public static void Stop()
        {
            // onstop code here
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            string temp = "";
            // 
            temp = CidData();
            if (temp != "")
            {
                Console.WriteLine(temp);
            }
        }
    }

    [RunInstaller(true)]
    public class MyServiceInstaller : Installer
    {
        public MyServiceInstaller()
        {
            var spi = new ServiceProcessInstaller();
            var si = new ServiceInstaller();

            spi.Account = ServiceAccount.LocalSystem;
            spi.Username = null;
            spi.Password = null;

            si.DisplayName = Program.ServiceName;
            si.ServiceName = Program.ServiceName;
            si.Description = "Taksi Durağı Takip Yazılımı için CallerID cihazı okuyup gönderen istemci servisi.";
            si.StartType = ServiceStartMode.Automatic;

            Installers.Add(spi);
            Installers.Add(si);
        }
    }

    public class ConstCharPtrMarshaler : ICustomMarshaler
    {
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            return Marshal.PtrToStringAnsi(pNativeData);
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            return IntPtr.Zero;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
        }

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public int GetNativeDataSize()
        {
            return IntPtr.Size;
        }

        static readonly ConstCharPtrMarshaler instance = new ConstCharPtrMarshaler();

        public static ICustomMarshaler GetInstance(string cookie)
        {
            return instance;
        }
    }
}
