using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace DataService.Utils
{
    public class ProcessHelper
    {
        public static void ClosePhantomJSProcess()
        {
            foreach (var p in Process.GetProcessesByName("phantomjs")) p.Kill();
        }

        public static void StartProcessIfNotStarted()
        {
            var processPath = Path.Combine(HostingEnvironment.MapPath("/"), "phantomjs.exe");
            var pht = Process.GetProcessesByName("phantomjs");
            //TODO skip take
            if (pht.Length > 1)
            {
                Process.GetProcessesByName("phantomjs").Skip(1).Take(pht.Length -1).ToList().ForEach(p => p.Kill());
            }
            if (pht.Length == 0)
            {
                Process.Start(new ProcessStartInfo(processPath));
            }


        }
    }
}
