using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Excess_Nop_Remover
{
    internal class Remover
    {
        public static void NopRemover(ModuleDefMD module)
        {
            foreach (var typeDef in module.Types)
            {
                foreach (var methodDef in typeDef.Methods)
                {
                    if (methodDef.HasBody)
                        RemoveUnusedNops(methodDef);
                }
            }
        }

        private static void RemoveUnusedNops(MethodDef methodDef)
        {
            if (!methodDef.HasBody)
                return;

            for (var i = 0; i < methodDef.Body.Instructions.Count; ++i)
            {
                var instruction = methodDef.Body.Instructions[i];

                if (instruction.OpCode != OpCodes.Nop)
                    continue;

                if (IsNopBranchTarget(methodDef, instruction))
                    continue;

                if (IsNopSwitchTarget(methodDef, instruction))
                    continue;

                if (IsNopExceptionHandlerTarget(methodDef, instruction))
                    continue;

                methodDef.Body.Instructions.RemoveAt(i);
                i--;
            }
        }

        private static bool IsNopBranchTarget(MethodDef methodDef, Instruction nopInstr)
        {
            foreach (var instruction in methodDef.Body.Instructions)
            {
                switch (instruction.OpCode.OperandType)
                {
                    case OperandType.InlineBrTarget:
                    case OperandType.ShortInlineBrTarget:
                        {
                            if (instruction.Operand == null)
                                continue;

                            if ((Instruction)instruction.Operand == nopInstr)
                                return true;

                            break;
                        }
                }
            }

            return false;
        }

        private static bool IsNopSwitchTarget(MethodDef methodDef, Instruction nopInstr)
        {
            foreach (var instruction in methodDef.Body.Instructions)
            {
                if (instruction.OpCode.OperandType != OperandType.InlineSwitch)
                    continue;

                if (instruction.Operand == null)
                    continue;

                var source = (Instruction[])instruction.Operand;

                if (source.Contains(nopInstr))
                    return true;
            }

            return false;
        }

        private static bool IsNopExceptionHandlerTarget(MethodDef methodDef, Instruction nopInstr)
        {
            if (methodDef.Body.HasExceptionHandlers) 
                return false;

            var exceptionHandlers = methodDef.Body.ExceptionHandlers;

            foreach (var exceptionHandler in exceptionHandlers)
            {
                if (exceptionHandler.FilterStart == nopInstr)
                    return true;

                if (exceptionHandler.HandlerEnd == nopInstr)
                    return true;
                
                if (exceptionHandler.HandlerStart == nopInstr)
                    return true;

                if (exceptionHandler.TryEnd == nopInstr)
                    return true;

                if (exceptionHandler.TryStart == nopInstr)
                    return true;
            }

            return false;
        }
    }
}
