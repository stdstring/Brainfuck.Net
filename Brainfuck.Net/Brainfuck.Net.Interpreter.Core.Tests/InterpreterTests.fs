namespace Brainfuck.Net.Interpreter.Core.Tests

open System
open System.IO
open System.Text
open NUnit.Framework
open Brainfuck.Net.Interpreter.Core

[<TestFixture>]
type InterpreterTests() =

    let memorySize = 30000
    let memoryInitCell = 0
    let maxOpCount = 100000

    let casesDir = "..\\..\\..\\TestCases"
    let inExtension = ".in"
    let outExtension = ".out"
    let srcExtension = ".src"

    [<TestCase("Hackerrank\\case0")>]
    [<TestCase("Hackerrank\\case1")>]
    [<TestCase("Hackerrank\\case2")>]
    [<TestCase("Hackerrank\\case3")>]
    [<TestCase("Hackerrank\\case4")>]
    [<TestCase("Hackerrank\\case5")>]
    [<TestCase("Hackerrank\\case6")>]
    [<TestCase("Hackerrank\\case7")>]
    [<TestCase("Hackerrank\\case8")>]
    [<TestCase("Hackerrank\\case9")>]
    [<TestCase("Hackerrank\\case10")>]
    [<TestCase("Hackerrank\\case11")>]
    [<TestCase("Hackerrank\\case12")>]
    [<TestCase("Hackerrank\\case13")>]
    [<TestCase("Hackerrank\\case14")>]
    [<TestCase("Hackerrank\\case15")>]
    [<TestCase("Hackerrank\\case16")>]
    [<TestCase("Hackerrank\\case17")>]
    member public this.Execute(name : string) =
        let inPath = Path.Combine(casesDir, name + inExtension)
        let outPath = Path.Combine(casesDir, name + outExtension)
        let srcPath = Path.Combine(casesDir, name + srcExtension)
        let inData = File.ReadAllText(inPath, Encoding.ASCII)
        let outData = File.ReadAllText(outPath, Encoding.ASCII)
        let srcData = File.ReadAllText(srcPath, Encoding.ASCII)
        let input = new StringReader(inData)
        let output = new StringWriter()
        let config = {MemorySize = memorySize; MemoryInitCell = memoryInitCell; MaxOpCount = maxOpCount; Input = input; Output = output}
        let interpreter = new Interpreter()
        interpreter.Execute(srcData, config)
        let actualOut = output.GetStringBuilder().ToString()
        actualOut |> Console.WriteLine
        Assert.That(actualOut, Is.EqualTo(outData))