namespace Brainfuck.Net.Interpreter.Core

open System
open System.IO

type public Interpreter() =

    //let MemorySize = 30000
    //let MaxOpCount = 100000
    let KnownChars = "><+-.,[]"

    let read_char (input : TextReader) : char =
        match input.Read() with
        | -1 -> raise (InvalidOperationException("unexpected eof"))
        | value -> value |> char

    let write_char (character : char) (output : TextWriter) : unit =
        output.Write(character)

    let find_block_end (context : ExecutionContext) =
        let rec find_block_end_impl index balance =
            match context.Program.[index] with
            | '[' -> find_block_end_impl (index + 1) (balance + 1)
            | ']' when balance > 1 -> find_block_end_impl (index + 1) (balance - 1)
            | ']' when balance = 1 -> index
            | _ -> find_block_end_impl (index + 1) balance
        find_block_end_impl context.Ip 0

    let execute_command (context : ExecutionContext) =
        let memory = context.Memory
        let command = context.Program.[context.Ip]
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
                let blockEndIp = context |> find_block_end
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
        | _ -> raise (InvalidOperationException("bad command"))

    member public this.Execute(program : string, config : InterpreterConfig) : unit =
        let factory = new ExecutionContextFactory()
        factory.Create(program, config) |> this.Execute

    member public this.Execute(context : ExecutionContext) : unit =
        let rec execute context =
            match this.ExecuteNextCommand(context) with
            | ExecutionState.Run -> context |> execute
            | ExecutionState.Stop -> ()
            | _ -> raise (InvalidOperationException("bad execution state"))
        context |> execute

    member public this.ExecuteNextCommand(context : ExecutionContext) : ExecutionState =
        let programSize = context.Program.Length
        match context with
        | context when context.Ip = programSize -> ExecutionState.Stop
        | context when context.OpCount >= context.Config.MaxOpCount ->
            "\nPROCESS TIME OUT. KILLED!!!" |> context.Config.Output.Write
            ExecutionState.Stop
        | _ ->
            context |> execute_command
            ExecutionState.Run
