namespace Brainfuck.Net.CodeGeneration.Tests

type SomeSubsystemImpl() =
    let mutable _value : int = 0

    interface ISomeSubsystem with
        member this.SomeValue
            with get() = _value
            and set(value) = _value <- value
