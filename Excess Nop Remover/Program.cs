using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.IO;

namespace Excess_Nop_Remover
{
    class Program
    {
        public static string Asmpath;
        public static ModuleDefMD AsmethodMdOriginal;

        static void Main(string[] args)
        {
            Console.Title = "Excess Nop Remover - iYaReM";

            if (args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Drag'n drop file.");
                Console.ReadKey();
                return;
            }

            try
            {
                AsmethodMdOriginal = ModuleDefMD.Load(args[0]);
                Asmpath = args[0];

                string directoryName = Path.GetDirectoryName(args[0]);

                if (!directoryName.EndsWith("\\"))
                {
                    directoryName += "\\";
                }

                Remover.NopRemover(AsmethodMdOriginal);
                Console.WriteLine($"Working..");

                string filename = string.Format("{0}{1}-NopRemoved{2}", directoryName, Path.GetFileNameWithoutExtension(args[0]), Path.GetExtension(args[0]));

                if (!AsmethodMdOriginal.Is32BitRequired)
                {
                    ModuleWriterOptions moduleWriterOptions = new ModuleWriterOptions(AsmethodMdOriginal);
                    moduleWriterOptions.MetaDataLogger = DummyLogger.NoThrowInstance;
                    moduleWriterOptions.MetaDataOptions.Flags = MetaDataFlags.PreserveAll;
                    AsmethodMdOriginal.Write(filename, moduleWriterOptions);
                }
                else
                {
                    NativeModuleWriterOptions moduleWriterOptions = new NativeModuleWriterOptions(AsmethodMdOriginal);
                    moduleWriterOptions.MetaDataLogger = DummyLogger.NoThrowInstance;
                    moduleWriterOptions.MetaDataOptions.Flags = MetaDataFlags.PreserveAll;
                    AsmethodMdOriginal.NativeWrite(filename, moduleWriterOptions);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("");
                Console.WriteLine("Done! Saving Assembly...");
            }
            catch 
            { 
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Invalid file."); 
            }
            Console.ReadKey();
        }
    }
}
