using System;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace Excess_Nop_Remover
{
    class Program
    {
        private static string Asmpath;
        private static ModuleDefMD AsmethodMdOriginal;

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

                var directoryName = Path.GetDirectoryName(args[0]);

                if (!directoryName.EndsWith("\\"))
                    directoryName += "\\";

                Remover.NopRemover(AsmethodMdOriginal);
                Console.WriteLine("Working..");

                var filename = $"{directoryName}{Path.GetFileNameWithoutExtension(Asmpath)}-NopRemoved{Path.GetExtension(Asmpath)}";

                if (!AsmethodMdOriginal.Is32BitRequired)
                {
                    var moduleWriterOptions = new ModuleWriterOptions(AsmethodMdOriginal)
                    {
                        MetaDataLogger = DummyLogger.NoThrowInstance,
                        MetaDataOptions =
                        {
                            Flags = MetaDataFlags.PreserveAll
                        }
                    };
                    AsmethodMdOriginal.Write(filename, moduleWriterOptions);
                }
                else
                {
                    var moduleWriterOptions = new NativeModuleWriterOptions(AsmethodMdOriginal)
                    {
                        MetaDataLogger = DummyLogger.NoThrowInstance,
                        MetaDataOptions =
                        {
                            Flags = MetaDataFlags.PreserveAll
                        }
                    };
                    AsmethodMdOriginal.NativeWrite(filename, moduleWriterOptions);
                }

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine("\nDone! Saving Assembly...");
            }
            catch { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Invalid file."); }

            Console.ReadKey();
        }
    }
}
