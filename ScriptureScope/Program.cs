using System.IO;

namespace ScriptureTelescope
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase), "NIV.xml");
            var t = new Scope(path);
            t.ProcessCommands();
        }
    }
}
