namespace Brainfuck.Net.Interpreter.Core

open System
open System.Collections.Generic
open System.IO

type public Interpreter() =

    //let MemorySize = 30000
    //let MaxOpCount = 100000
    let KnownChars = "><+-.,[]"

    let read_char (input : TextReader) : char =
        match input.Read() with
        | -1 -> failwith "unexpected eof"
        | value -> value |> char

    let write_char (character : char) (output : TextWriter) : unit =
        output.Write(character)

    let find_block_end startIndex (program : string) =
        let rec find_block_end_impl index balance =
            match program.[index] with
            | '[' -> find_block_end_impl (index + 1) (balance + 1)
            | ']' when balance > 1 -> find_block_end_impl (index + 1) (balance - 1)
            | ']' when balance = 1 -> index
            | _ -> find_block_end_impl (index + 1) balance
        find_block_end_impl startIndex 0

    let execute_command (context : ExecutionContext) (program : string) =
        let memory = context.Memory
        let command = program.[context.Ip]
        match command with
        | '>' ->
            context.CurrentCell <- context.CurrentCell + 1
            context.Ip <- context.Ip + 1
            context.OpCount <- context.OpCount + 1
        | '<' ->
            context.CurrentCell <- context.CurrentCell - 1
            context.Ip <- context.Ip + 1
            context.OpCount <- context.OpCount + 1
            ()
        | '+' ->
            memory.[context.CurrentCell] <- memory.[context.CurrentCell] + 1uy
            context.Ip <- context.Ip + 1
            context.OpCount <- context.OpCount + 1
        | '-' ->
            memory.[context.CurrentCell] <- memory.[context.CurrentCell] - 1uy
            context.Ip <- context.Ip + 1
            context.OpCount <- context.OpCount + 1
        | '.' ->
            write_char (memory.[context.CurrentCell] |> char) context.Config.Output
            context.Ip <- context.Ip + 1
            context.OpCount <- context.OpCount + 1
        | ',' ->
            memory.[context.CurrentCell] <- read_char context.Config.Input |> byte
            context.Ip <- context.Ip + 1
            context.OpCount <- context.OpCount + 1
        | '[' ->
            match memory.[context.CurrentCell] with
            | 0uy ->
                let blockEndIp = find_block_end context.Ip program
                context.Ip <- blockEndIp + 1
                context.OpCount <- context.OpCount + 2
            | _ ->
                context.Ip <- context.Ip + 1
                context.Stack.Push(context.Ip)
                context.OpCount <- context.OpCount + 1
        | ']' ->
            match memory.[context.CurrentCell] with
            | 0uy ->
                context.Stack.Pop() |> ignore
                context.Ip <- context.Ip + 1
                context.OpCount <- context.OpCount + 1
            | _ ->
                context.Ip <- context.Stack.Peek()
                context.OpCount <- context.OpCount + 2
        | _ -> failwith "bad command"

    let execute_program (context : ExecutionContext) (program : string) : unit =
        let programSize = program.Length
        let rec execute_program_impl () =
            match context with
            | context when context.Ip = programSize -> ()
            | context when context.OpCount >= context.Config.MaxOpCount -> "\nPROCESS TIME OUT. KILLED!!!" |> context.Config.Output.Write
            | _ ->
                execute_command context program
                execute_program_impl ()
        execute_program_impl ()

    let prepare(program : string) : string =
        program |> String.Concat |> Seq.filter (fun ch -> KnownChars.IndexOf(ch) >= 0) |> String.Concat

    member public this.Execute(program : string, config : InterpreterConfig) : unit =
        let memory = Array.create config.MemorySize 0uy
        let context = {Config = config; OpCount = 0; Ip = 0; CurrentCell = 0; Memory = memory; Stack = new Stack<Int32>()}
        program |> prepare |> execute_program context
