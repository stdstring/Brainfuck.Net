namespace Brainfuck.Net.Interpreter.Core

open System
open System.Collections.Generic

type public ExecutionContextFactory() =

    let KnownChars = "><+-.,[]"

    let prepare_program (program : string) : string =
        program |> String.Concat |> Seq.filter (fun ch -> KnownChars.IndexOf(ch) >= 0) |> String.Concat

    member public this.Create(program : string, config : InterpreterConfig) : ExecutionContext =
        let memory = Array.create config.MemorySize 0uy
        let preparedProgram = program |> prepare_program
        {Config = config; Program = preparedProgram; OpCount = 0; Ip = 0; CurrentCell = 0; Memory = memory; Stack = new Stack<Int32>()}