open System
open System.IO
open Brainfuck.Net.Interpreter.Core

[<Literal>]
let DefaultMemorySize = 50000

[<Literal>]
let DefaultInitCell = 0

[<Literal>]
let DefaultMaxOpCount = 100000

type ArgType =
    | Version
    | Help
    | Interactive
    | Load of filename: string * memorySize: int * initCell: int * maxOpCount: int
    | Source of code: string * memorySize: int * initCell: int * maxOpCount: int
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
    | Stop
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

let (|ToInt | _|) (source : string) =
    let mutable value = 0
    match Int32.TryParse(source, &value) with
    | true -> Some(value)
    | false -> None

// TODO (std_string) : create smart solution
let ConvertConfigArgs (memorySizeStr : string) (initCellStr : string) (maxOpCountStr : string) =
    match memorySizeStr with
    | ToInt memorySize ->
        match initCellStr with
        | ToInt initCell ->
            match maxOpCountStr with
            | ToInt maxOpCount -> Some(memorySize, initCell, maxOpCount)
            | _ -> None
        | _ -> None
    | _ -> None

let RecognizeArgs argv =
    match argv with
    | [||] | [|"--help"|] -> ArgType.Help
    | [|"--version"|] -> ArgType.Version
    | [|"--interactive"|] -> ArgType.Interactive
    // TODO (std_string) : create smart solution
    | [|"--load"; filename|] -> ArgType.Load(filename = filename, memorySize = DefaultMemorySize, initCell = DefaultInitCell, maxOpCount = DefaultMaxOpCount)
    // TODO (std_string) : create smart solution
    | [|"--load"; filename; "--size"; memorySizeStr; "--start"; initCellStr; "--max-op"; maxOpCountStr|] ->
        match ConvertConfigArgs memorySizeStr initCellStr maxOpCountStr with
        | Some (memorySize, initCell, maxOpCount) -> ArgType.Load(filename = filename, memorySize = memorySize, initCell = initCell, maxOpCount = maxOpCount)
        | _ -> ArgType.Unknown
    // TODO (std_string) : create smart solution
    | [|"--source"; code|] -> ArgType.Source(code = code, memorySize = DefaultMemorySize, initCell = DefaultInitCell, maxOpCount = DefaultMaxOpCount)
    // TODO (std_string) : create smart solution
    | [|"--source"; code; "--size"; memorySizeStr; "--start"; initCellStr; "--max-op"; maxOpCountStr|] ->
        match ConvertConfigArgs memorySizeStr initCellStr maxOpCountStr with
        | Some (memorySize, initCell, maxOpCount) -> ArgType.Source(code = code, memorySize = memorySize, initCell = initCell, maxOpCount = maxOpCount)
        | _ -> ArgType.Unknown
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
    | "stop" -> ConfigCommand.Stop
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

[<Literal>]
let ConfigModePrompt = "config>>>"

[<Literal>]
let RunModePrompt = "run>>>"

type InteractiveMode =
    // config mode
    | Config
    // run mode
    | Run
    // for stopping interactive mode
    | Stop

let ProcessRunCommand command =
    match command with
    | RunCommand.Run ->
        InteractiveMode.Run
    | RunCommand.RunOnce ->
        InteractiveMode.Run
    | RunCommand.GetState ->
        InteractiveMode.Run
    | RunCommand.GetCurrentMemoryCell ->
        InteractiveMode.Run
    | RunCommand.GetMemoryCell(cell = cell) ->
        InteractiveMode.Run
    | RunCommand.ShowInput ->
        InteractiveMode.Run
    | RunCommand.ShowOutput ->
        InteractiveMode.Run
    | RunCommand.ExitRunMode ->
        InteractiveMode.Config
    | RunCommand.Unknown ->
        InteractiveMode.Run

let ProcessRunMode () =
    let rec processImpl () =
        RunModePrompt |> Console.Write;
        let command = Console.ReadLine()
        match (command |> ParseRunCommand |> ProcessRunCommand) with
        | InteractiveMode.Run -> processImpl ()
        | _ -> ()
        //| InteractiveMode.Config -> InteractiveMode.Config
        //| InteractiveMode.Stop -> InteractiveMode.Stop
    processImpl ()

let ProcessConfigCommand command =
    match command with
    | ConfigCommand.Load(filename = filename) ->
        InteractiveMode.Config
    | ConfigCommand.Source(code = code) ->
        InteractiveMode.Config
    | ConfigCommand.Input(filename = filename) ->
        InteractiveMode.Config
    | ConfigCommand.ISource(input = input) ->
        InteractiveMode.Config
    | ConfigCommand.Output(filename = filename) ->
        InteractiveMode.Config
    | ConfigCommand.SetVariable(name = name; value = value) ->
        InteractiveMode.Config
    | ConfigCommand.GetVariable(name = name) ->
        InteractiveMode.Config
    | ConfigCommand.GetAllVariables ->
        InteractiveMode.Config
    | ConfigCommand.EnterRunMode ->
        InteractiveMode.Run
    | ConfigCommand.Stop ->
        InteractiveMode.Stop
    | ConfigCommand.Unknown ->
        InteractiveMode.Config

let ProcessConfigMode () =
    let rec processImpl () =
        ConfigModePrompt |> Console.Write;
        let command = Console.ReadLine()
        match (command |> ParseConfigCommand |> ProcessConfigCommand) with
        | InteractiveMode.Run -> ProcessRunMode ()
        | InteractiveMode.Config -> processImpl ()
        | InteractiveMode.Stop -> ()
    processImpl ()

let ShowVersion () =
    "version = XXX" |> Console.WriteLine
    ()

let ShowHelp () =
    "Help ..." |> Console.WriteLine
    ()

let ExecuteCode (memorySize : int) (initCell : int) (maxOpCount : int) (code : string) =
    let config = {MemorySize = memorySize; MemoryInitCell = initCell; MaxOpCount = maxOpCount; Input = Console.In; Output = Console.Out}
    let interpreter = new Interpreter();
    interpreter.Execute(code, config)
    ()

let ShowUnknownFile (filename: string) =
    Console.WriteLine("Unknown file with name \"{0}\"", filename)
    ()

let ShowUnknown () =
    "Unknown command ... " |> Console.WriteLine
    ()

let ProcessInteractiveMode () =
    ProcessConfigMode ()

let ProcessArgs argsData =
    match argsData with
    | ArgType.Version -> ShowVersion ()
    | ArgType.Help -> ShowHelp ()
    | ArgType.Interactive -> ProcessInteractiveMode ()
    | ArgType.Load(filename = filename; memorySize = memorySize; initCell = initCell; maxOpCount = maxOpCount) ->
        match File.Exists(filename) with
        | true -> filename |> File.ReadAllText |> ExecuteCode memorySize initCell maxOpCount
        | false -> filename |> ShowUnknownFile
    | ArgType.Source(code = code; memorySize = memorySize; initCell = initCell; maxOpCount = maxOpCount) -> code |> ExecuteCode memorySize initCell maxOpCount
    | ArgType.Unknown -> ShowUnknown ()

// DESCRIPTION:
//
// command line args
// options:
// 1) --version
// 2) --help
// 3) --interactive
// 4) --load code-filename [--size memory-size --start memory-init-cell --max-op max-op-count]
// 5) --source code-fragment [--size memory-size --start memory-init-cell --max-op max-op-count]
// default values:  memory-size = 50000, memory-init-cell = 0, max-op-count = 100000
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
// stop -> stop interactive mode
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
