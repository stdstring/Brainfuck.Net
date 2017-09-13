open System
open System.IO
open Brainfuck.Net.Interpreter.Core

type ArgType =
    | Version
    | Help
    | Interactive
    | Load of filename: string
    | Source of code: string
    | Unknown

type ConfigCommand =
    | Load of filename: string
    | Source of code: string
    | Input of filename: string
    | ISource of input: string
    | Output of filename: string
    | SetVariable of name: string * value: int
    | GetVariable of name: string
    | GetAllVariables
    | EnterRunMode
    | Unknown

type RunCommand =
    | Run
    | RunOnce
    | GetState
    | GetCurrentMemoryCell
    | GetMemoryCell of cell: int
    | ShowInput
    | ShowOutput
    | ExitRunMode
    | Unknown

//let RecognizeArgs argv =
//    match argv with
//    | [||] | [|"--help"|] -> ArgType.Help
//    | [|"--version"|] -> ArgType.Version
//    | [|"--interactive"|] -> ArgType.Interactive
//    | [|"--load"; Filename|] -> ArgType.Load(filename = Filename)
//    | [|"--source"; Data|] -> ArgType.Source(data = Data)
//    | _ -> ArgType.Unknown

let RecognizeArgs =
    function
    | [||] | [|"--help"|] -> ArgType.Help
    | [|"--version"|] -> ArgType.Version
    | [|"--interactive"|] -> ArgType.Interactive
    | [|"--load"; filename|] -> ArgType.Load(filename = filename)
    | [|"--source"; code|] -> ArgType.Source(code = code)
    | _ -> ArgType.Unknown

let (|LoadCode | _|) (command : string) =
    match command.Split([|' '; '\t'|], 2, StringSplitOptions.RemoveEmptyEntries) with
    | [|"load"; filename|] -> Some(filename)
    | _ -> None

let (|SourceCode | _|) (command : string) =
    match command.Split([|' '; '\t'|], 2, StringSplitOptions.RemoveEmptyEntries) with
    | [|"source"; code|] -> Some(code)
    | _ -> None

let (|LoadInput | _|) (command : string) =
    match command.Split([|' '; '\t'|], 2, StringSplitOptions.RemoveEmptyEntries) with
    | [|"input"; filename|] -> Some(filename)
    | _ -> None

let (|SourceInput | _|) (command : string) =
    match command.Split([|' '; '\t'|], 2, StringSplitOptions.RemoveEmptyEntries) with
    | [|"isource"; input|] -> Some(input)
    | _ -> None

let (|RedirectOutput | _|) (command : string) =
    match command.Split([|' '; '\t'|], 2, StringSplitOptions.RemoveEmptyEntries) with
    | [|"output"; filename|] -> Some(filename)
    | _ -> None

let (|ToInt | _|) (source : string) =
    let mutable value = 0
    match Int32.TryParse(source, &value) with
    | true -> Some(value)
    | false -> None

let (|SetVariable | _|) (command : string) =
    match command.Split([|' '; '\t'|], 2, StringSplitOptions.RemoveEmptyEntries) with
    | [|"set"; commandRest|] ->
        match commandRest.Split([|'='|], 2, StringSplitOptions.RemoveEmptyEntries) with
        | [|name; value|] ->
            match value with
            | ToInt intValue -> Some(name.TrimEnd(), intValue)
            | _ -> None
        | _ -> None
    | _ -> None

let (|GetVariable | _|) (command : string) =
    match command.Split([|' '; '\t'|], 2, StringSplitOptions.RemoveEmptyEntries) with
    | [|"get"; name|] -> Some(name)
    | _ -> None

let ParseConfigCommand (command : string) =
    match command.Trim() with
    | LoadCode filename -> ConfigCommand.Load(filename = filename)
    | SourceCode code -> ConfigCommand.Source(code = code)
    | LoadInput filename -> ConfigCommand.Input(filename = filename)
    | SourceInput input -> ConfigCommand.ISource(input = input)
    | RedirectOutput filename -> ConfigCommand.Output(filename = filename)
    | SetVariable (name, value) -> ConfigCommand.SetVariable(name = name, value = value)
    | GetVariable name -> ConfigCommand.GetVariable(name = name)
    | "get all" -> ConfigCommand.GetAllVariables
    | "enter" -> ConfigCommand.EnterRunMode
    | _ -> ConfigCommand.Unknown

let (|GetMemory | _|) (command : string) =
    match command.Split([|' '; '\t'|], 2, StringSplitOptions.RemoveEmptyEntries) with
    | [|"memory"; value|] ->
        match value with
        | ToInt cell -> Some(cell)
        | _ -> None
    | _ -> None

let ParseRunCommand (command : string) =
    match command.Trim() with
    | "run all" -> RunCommand.Run
    | "run once" -> RunCommand.RunOnce
    | "state" -> RunCommand.GetState
    | "memory" -> RunCommand.GetCurrentMemoryCell
    | GetMemory cell -> RunCommand.GetMemoryCell(cell = cell)
    | "input" -> RunCommand.ShowInput
    | "output" -> RunCommand.ShowOutput
    | "exit" -> RunCommand.ExitRunMode
    | _ -> RunCommand.Unknown

let ShowVersion () =
    ()

let ShowHelp () =
    ()

let ExecuteCode (code : string) =
    ()

let ShowUnknownFile (filename: string) =
    ()

let ShowUnknown () =
    ()

[<Literal>]
let ConfigModePrompt = "config>>>"

[<Literal>]
let RunModePrompt = "run>>>"

let ProcessInteractiveMode () =
    ()

let ProcessArgs argsData =
    match argsData with
    | ArgType.Version -> ShowVersion ()
    | ArgType.Help -> ShowHelp ()
    | ArgType.Interactive -> ProcessInteractiveMode ()
    | ArgType.Load(filename = filename) ->
        match File.Exists(filename) with
        | true -> filename |> File.ReadAllText |> ExecuteCode
        | false -> filename |> ShowUnknownFile
    | ArgType.Source(code = code) -> code |> ExecuteCode
    | ArgType.Unknown -> ShowUnknown ()

let ProcessConfigCommand command =
    match command with
    | ConfigCommand.Load(filename = filename) -> ()
    | ConfigCommand.Source(code = code) -> ()
    | ConfigCommand.Input(filename = filename) -> ()
    | ConfigCommand.ISource(input = input) -> ()
    | ConfigCommand.Output(filename = filename) -> ()
    | ConfigCommand.SetVariable(name = name; value = value) -> ()
    | ConfigCommand.GetVariable(name = name) -> ()
    | ConfigCommand.GetAllVariables -> ()
    | ConfigCommand.EnterRunMode -> ()
    | ConfigCommand.Unknown -> ()

let ProcessRunMode () =
    RunModePrompt |> Console.Write;
    let command = Console.ReadLine()
    //ProcessRunCommand command
    ()

let ProcessRunCommand command =
    match command with
    | RunCommand.Run -> ()
    | RunCommand.RunOnce -> ()
    | RunCommand.GetState -> ()
    | RunCommand.GetCurrentMemoryCell -> ()
    | RunCommand.GetMemoryCell(cell = cell) -> ()
    | RunCommand.ShowInput -> ()
    | RunCommand.ShowOutput -> ()
    | RunCommand.ExitRunMode -> ()
    | RunCommand.Unknown -> ()

// DESCRIPTION:
//
// command line args
// options:
// 1) --version
// 2) --help
// 3) --interactive
// 4) --load code-filename
// 5) --source code-fragment
// allowed combinations:
// app -> show help
// app --help -> show help
// app --version -> show version number
// app --interactive -> run in interactive mode
// app --load code-filename -> execute code from file, input from stdin, output to stdout (stderr)
// app --source code-fragment -> execute fragment of code (written in single line), input from stdin, output to stdout (stderr)
// interactive mode commands:
// in config mode:
// load code-filename -> load code from file
// source code-fragment -> use specified fragment of code (written in single line)
// input input-filename -> load input from file
// isource input-fragment -> use specified fragment of input (written in single line with help of escape sequences)
// output output-filename -> save output into file
// set {variable}={value} -> setting value of the specified variable (now, variable = MemorySize, MemoryInitCell, MaxOpCount)
// get {variable} -> getting value of the specified variable (now, variable = MemorySize, MemoryInitCell, MaxOpCount)
// get all -> getting values of the all known variables (now, variable = MemorySize, MemoryInitCell, MaxOpCount)
// enter -> move into run mode
// in run mode:
// run all -> execute code to the end
// run once -> execute (one) current command
// state -> get current execution state (CurrentMemoryCell, CurrentIp, CurrentOpCount)
// memory -> get value of current memory cell
// memory memory-cell -> get value of memory cell by memory-cell address
// input -> show retrieved input
// output -> show generated output
// exit -> reset execution and move into config mode
//
// mandatory configurations:
// 1) load code-filename | source code-fragment
// 2) input input-filename | isource input-fragment
[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    0 // return an integer exit code
