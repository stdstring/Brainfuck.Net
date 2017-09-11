﻿// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

type ArgType =
    | Version
    | Help
    | Interactive
    | Load of filename: string
    | Source of data: string
    | Unknown

type ConfigCommand =
    | Load of filename: string
    | Source of data: string
    | Input of filename: string
    | ISource of data: string
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
    | [|"--source"; data|] -> ArgType.Source(data = data)
    | _ -> ArgType.Unknown

let ProcessArgs argsData =
    match argsData with
    | ArgType.Version -> ()
    | ArgType.Help -> ()
    | ArgType.Interactive -> ()
    | ArgType.Load(filename = filename) -> ()
    | ArgType.Source(data = data) -> ()
    | ArgType.Unknown -> ()

let ProcessConfigCommand command =
    match command with
    | ConfigCommand.Load(filename = filename) -> ()
    | ConfigCommand.Source(data = data) -> ()
    | ConfigCommand.Input(filename = filename) -> ()
    | ConfigCommand.ISource(data = data) -> ()
    | ConfigCommand.Output(filename = filename) -> ()
    | ConfigCommand.SetVariable(name = name; value = value) -> ()
    | ConfigCommand.GetVariable(name = name) -> ()
    | ConfigCommand.GetAllVariables -> ()
    | ConfigCommand.EnterRunMode -> ()
    | ConfigCommand.Unknown -> ()

let ProcessRunnCommand command =
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
