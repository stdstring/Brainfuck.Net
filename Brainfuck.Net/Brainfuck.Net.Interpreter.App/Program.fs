// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

// DESCRIPTION:
//
// command line args
// options:
// 1) --version
// 2) --help
// 3) --interactive
// 4) --code code-file
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
// status -> get current execution status (CurrentMemoryCell, CurrentIp, CurrentOpCount)
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
