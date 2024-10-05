using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SHARED_UHD_SCENARIO_SMD;

namespace RE4_UHD_SCENARIO_SMD_TOOL
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Console.WriteLine(Shared.HeaderText());

            if (args.Length == 0)
            {
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/RE4-UHD-SCENARIO-SMD-TOOL");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();
            }
            else if (args.Length >= 1 && File.Exists(args[0]))
            {
                try
                {
                    FileInfo fileInfo1 = new FileInfo(args[0]);
                    string file1Extension = fileInfo1.Extension.ToUpperInvariant();
                    Console.WriteLine("File1: " + fileInfo1.Name);

                    MainAction.Actions(fileInfo1, file1Extension, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error1: " + ex);
                }
            }
            else
            {
                Console.WriteLine("File specified does not exist.");
            }

            Console.WriteLine("Finished!!!");
        }

    }
}
