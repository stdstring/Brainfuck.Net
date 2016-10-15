namespace Brainfuck.Net.Interpreter.Core.Tests

open System
open System.IO
open NUnit.Framework
open Brainfuck.Net.Interpreter.Core

[<TestFixture>]
type InterpreterTests() =

    let memorySize = 30000
    let memoryInitCell = 0
    let maxOpCount = 100000

    [<Test>]
    member public this.ProcessUnexpectedEof() =
        let input = new StringReader("A")
        let output = new StringWriter()
        let config = {MemorySize = memorySize; MemoryInitCell = memoryInitCell; MaxOpCount = maxOpCount; Input = input; Output = output}
        let interpreter = new Interpreter()
        Assert.Throws<InvalidOperationException>(fun () -> interpreter.Execute(",,", config)) |> ignore
