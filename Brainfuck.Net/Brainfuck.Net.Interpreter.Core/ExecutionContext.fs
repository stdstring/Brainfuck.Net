﻿namespace Brainfuck.Net.Interpreter.Core

open System
open System.Collections.Generic
open System.IO

type ExecutionContext =
    {
        Config : InterpreterConfig;
        Program : string;
        mutable OpCount : Int32;
        mutable Ip : Int32;
        mutable CurrentCell : Int32
        Memory : Byte[]
        Stack : Stack<Int32>;
    }
