namespace Brainfuck.Net.Interpreter.Core

open System
open System.Collections.Generic
open System.IO

type ExecutionContext =
    {
        mutable OpCount : int32;
        mutable Ip : int32;
        mutable CurrentCell : int32
        Stack : Stack<Int32>;
        Input : Stream
        Output : Stream
    }
