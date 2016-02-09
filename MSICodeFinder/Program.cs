using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSICodeFinder
{
    class Program
    {
        enum CodeFinderMode
        {
            Undefined = 0,
            ProductCode,
            UpgradeCode
        };

        private static CodeFinderMode FinderMode;

        private static Guid Code;

        static void PrintUsageMessage()
        {
            string programName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            Console.WriteLine("USAGE: " + programName + " mode guid");
            Console.WriteLine("   mode = ProductCode | UpgradeCode");
            Console.WriteLine("   guid = MSI GUID to find the ProductCode or UpgradeCode for");
            Console.WriteLine("");
            Console.WriteLine("When mode is set to ProductCode, installed product codes belonging to the specified upgrade code GUID are returned, one per line.");
            Console.WriteLine("When mode is set to UpgradeCode, the upgrade code of the specified product code GUID is returned, if installed.");
        }

        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsageMessage();
                return 1;
            }

            if (args[0].ToLower() == "productcode")
                FinderMode = CodeFinderMode.ProductCode;
            else if (args[0].ToLower() == "upgradecode")
                FinderMode = CodeFinderMode.UpgradeCode;
            else
            {
                Console.WriteLine("Invalid mode specified.\n");
                PrintUsageMessage();
                return 1;
            }

            if (!Guid.TryParse(args[1], out Code))
            {
                Console.WriteLine("Invalid MSI GUID specified.\n");
                PrintUsageMessage();
                return 1;
            }

            if (FinderMode == CodeFinderMode.ProductCode)
            {
                List<Guid> productCodes = RegistryHelper.GetProductCodes(Code);

                foreach (Guid pCode in productCodes)
                {
                    Console.WriteLine(pCode.ToString("B"));
                }
            }
            else
            {
                Guid? upgradeCode = RegistryHelper.GetUpgradeCode(Code);

                if (upgradeCode != null)
                    Console.WriteLine(((Guid)upgradeCode).ToString("B"));
            }

            return 0;
        }
    }
}
