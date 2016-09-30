namespace Brainfuck.Net.Interpreter.Core

open System
open System.Collections.Generic
open System.IO

type ExecutionContext =
    {
        Config : InterpreterConfig;
        mutable OpCount : Int32;
        mutable Ip : Int32;
        mutable CurrentCell : Int32
        Memory : byte[]
        Stack : Stack<Int32>;
        Input : Stream
        Output : Stream
    }
