namespace Brainfuck.Net.Interpreter.Core

open System.IO

type InterpreterConfig =
    {
        MemorySize : int;
        MemoryInitCell : int;
        MaxOpCount : int;
        Input : TextReader;
        Output : TextWriter;
    }