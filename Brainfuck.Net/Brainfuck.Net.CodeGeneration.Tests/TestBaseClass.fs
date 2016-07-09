namespace Brainfuck.Net.CodeGeneration.Tests

type TestBaseClass=
    val mutable public SomeField : int

    // TODO (std_string) : use constant
    new() = {SomeField = 6666}
