namespace Brainfuck.Net.CodeGeneration.Tests

open System;
open System.Reflection;
open System.Reflection.Emit;
open NUnit.Framework;

[<TestFixture>]
type CodeTemplatesTest() =
    let subsystem : ISomeSubsystem = new SomeSubsystemImpl() :> ISomeSubsystem
    let subsystemField : FieldInfo = typeof<CodeTemplatesTest>.GetField("subsystem", BindingFlags.Instance ||| BindingFlags.NonPublic)

    [<Test>]
    member public this.BaseCtor() =
        let assemblyBuilder : AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(Name = "TestAssembly"), AssemblyBuilderAccess.Run)
        let moduleBuilder : ModuleBuilder = assemblyBuilder.DefineDynamicModule("TestModule");
        let typeBuilder : TypeBuilder = moduleBuilder.DefineType("TestType", TypeAttributes.Class, typeof<TestBaseClass>)
        let ctorBuilder : ConstructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public ||| MethodAttributes.SpecialName, CallingConventions.Standard, Array.empty<Type>)
        let generator : ILGenerator = ctorBuilder.GetILGenerator()
        let someField : FieldInfo = typeof<TestBaseClass>.GetField("SomeField")
        generator.Emit(OpCodes.Ldarg_0)
        generator.Emit(OpCodes.Call, typeof<TestBaseClass>.GetConstructor(Array.empty<Type>))
        generator.Emit(OpCodes.Ldarg_0)
        generator.Emit(OpCodes.Ldarg_0)
        generator.Emit(OpCodes.Ldfld, someField)
        generator.Emit(OpCodes.Ldc_I4_M1)
        generator.Emit(OpCodes.Add)
        generator.Emit(OpCodes.Stfld, someField)
        generator.Emit(OpCodes.Ret)
        let testType : Type = typeBuilder.CreateType()
        let testObject : TestBaseClass = Activator.CreateInstance(testType) :?> TestBaseClass
        // TODO (std_string) : use constant
        Assert.AreEqual(6666 - 1, testObject.SomeField)

    [<Test>]
    member public this.ChangeValueWithLocalVariable() =
        let source : int = 3
        let delta : int = 13
        let someValueGetter : MethodInfo = typeof<ISomeSubsystem>.GetProperty("SomeValue").GetGetMethod()
        let someValueSetter : MethodInfo = typeof<ISomeSubsystem>.GetProperty("SomeValue").GetSetMethod()
        let changeValue : DynamicMethod = new DynamicMethod("ChangeValueDynamicMethod", typeof<System.Void>, [|typeof<CodeTemplatesTest>|], typeof<CodeTemplatesTest>)
        changeValue.InitLocals <- true
        let generator : ILGenerator = changeValue.GetILGenerator()
        generator.DeclareLocal(typeof<byte>) |> ignore
        this.LoadSubsystemField(generator)
        generator.EmitCall(OpCodes.Call, someValueGetter, null)
        generator.Emit(OpCodes.Ldc_I4, delta)
        generator.Emit(OpCodes.Add)
        generator.Emit(OpCodes.Conv_I1)
        generator.Emit(OpCodes.Stloc_0)
        this.LoadSubsystemField(generator)
        generator.Emit(OpCodes.Ldloc_0)
        generator.EmitCall(OpCodes.Call, someValueSetter, null)
        generator.Emit(OpCodes.Ret)
        let changeValueAction : Action = changeValue.CreateDelegate(typeof<Action>, this) :?> Action
        subsystem.SomeValue <- source
        changeValueAction.Invoke()
        Assert.AreEqual(source + delta, subsystem.SomeValue)

    [<Test>]
    member public this.ChangeValueUseStackOnly() =
        let source : int = 3
        let delta : int = 13
        let someValueGetter : MethodInfo = typeof<ISomeSubsystem>.GetProperty("SomeValue").GetGetMethod();
        let someValueSetter : MethodInfo = typeof<ISomeSubsystem>.GetProperty("SomeValue").GetSetMethod();
        let changeValue : DynamicMethod = new DynamicMethod("ChangeValueDynamicMethod", typeof<System.Void>, [|typeof<CodeTemplatesTest>|], typeof<CodeTemplatesTest>)
        let generator : ILGenerator = changeValue.GetILGenerator()
        this.LoadSubsystemField(generator)
        generator.Emit(OpCodes.Dup)
        generator.EmitCall(OpCodes.Call, someValueGetter, null)
        generator.Emit(OpCodes.Ldc_I4, delta)
        generator.Emit(OpCodes.Add)
        generator.Emit(OpCodes.Conv_I1)
        generator.EmitCall(OpCodes.Call, someValueSetter, null)
        generator.Emit(OpCodes.Ret)
        let changeValueAction : Action = changeValue.CreateDelegate(typeof<Action>, this) :?> Action
        subsystem.SomeValue <- source
        changeValueAction.Invoke()
        Assert.AreEqual(source + delta, subsystem.SomeValue)

    [<Test>]
    member public this.SimpleWhileLoop()=
        let source : int = 10
        let loopCount : int = 9
        let simpleLoop : DynamicMethod = new DynamicMethod("SimpleWhileLoopDynamicMethod", typeof<int>, [|typeof<int>|])
        simpleLoop.InitLocals <- true
        let generator : ILGenerator = simpleLoop.GetILGenerator()
        // result
        generator.DeclareLocal(typeof<int>) |> ignore
        // counter
        generator.DeclareLocal(typeof<int>) |> ignore
        // labels
        let loopStart : Label = generator.DefineLabel()
        let afterLoop : Label = generator.DefineLabel()
        // body
        generator.Emit(OpCodes.Ldarg_0)
        generator.Emit(OpCodes.Stloc_0)
        generator.Emit(OpCodes.Ldc_I4, loopCount)
        generator.Emit(OpCodes.Stloc_1)
        // loop
        generator.MarkLabel(loopStart)
        generator.Emit(OpCodes.Ldloc_1)
        generator.Emit(OpCodes.Brfalse_S, afterLoop)
        generator.Emit(OpCodes.Ldloc_0)
        generator.Emit(OpCodes.Ldc_I4_1)
        generator.Emit(OpCodes.Add)
        generator.Emit(OpCodes.Stloc_0)
        generator.Emit(OpCodes.Ldloc_1)
        generator.Emit(OpCodes.Ldc_I4_M1)
        generator.Emit(OpCodes.Add)
        generator.Emit(OpCodes.Stloc_1)
        generator.Emit(OpCodes.Br_S, loopStart)
        // loop end
        generator.MarkLabel(afterLoop)
        generator.Emit(OpCodes.Ldloc_0)
        generator.Emit(OpCodes.Ret)
        let simpleLoopFunc : Func<int, int> = simpleLoop.CreateDelegate(typeof<Func<int, int>>) :?> Func<int, int>
        let result : int = simpleLoopFunc.Invoke(source)
        Assert.AreEqual(source + loopCount, result)

    [<Test>]
    member public this.NestedWhileLoops() =
        let source : int = 10
        let outerLoopCount : int = 6
        let innerLoopCount : int = 4
        let simpleLoop : DynamicMethod = new DynamicMethod("NestedWhileLoopsDynamicMethod", typeof<int>, [|typeof<int>|])
        simpleLoop.InitLocals <- true
        let generator : ILGenerator = simpleLoop.GetILGenerator()
        // result
        generator.DeclareLocal(typeof<int>) |> ignore
        // outer counter
        generator.DeclareLocal(typeof<int>) |> ignore
        // inner counter
        generator.DeclareLocal(typeof<int>) |> ignore
        // labels
        let outerLoop : Label = generator.DefineLabel()
        let innerLoop : Label = generator.DefineLabel()
        let afterOuterLoop : Label = generator.DefineLabel()
        let afterInnerLoop : Label = generator.DefineLabel()
        // body
        generator.Emit(OpCodes.Ldarg_0)
        generator.Emit(OpCodes.Stloc_0)
        generator.Emit(OpCodes.Ldc_I4, outerLoopCount)
        generator.Emit(OpCodes.Stloc_1)
        generator.Emit(OpCodes.Ldc_I4, innerLoopCount)
        generator.Emit(OpCodes.Stloc_2)
        // outer loop
        generator.MarkLabel(outerLoop)
        generator.Emit(OpCodes.Ldloc_1)
        generator.Emit(OpCodes.Brfalse_S, afterOuterLoop);
        // inner loop
        generator.MarkLabel(innerLoop)
        generator.Emit(OpCodes.Ldloc_2)
        generator.Emit(OpCodes.Brfalse_S, afterInnerLoop)
        generator.Emit(OpCodes.Ldloc_0)
        generator.Emit(OpCodes.Ldc_I4_1)
        generator.Emit(OpCodes.Add)
        generator.Emit(OpCodes.Stloc_0)
        generator.Emit(OpCodes.Ldloc_2)
        generator.Emit(OpCodes.Ldc_I4_M1)
        generator.Emit(OpCodes.Add)
        generator.Emit(OpCodes.Stloc_2)
        generator.Emit(OpCodes.Br_S, innerLoop)
        // inner loop end
        generator.MarkLabel(afterInnerLoop)
        generator.Emit(OpCodes.Ldc_I4, innerLoopCount)
        generator.Emit(OpCodes.Stloc_2)
        generator.Emit(OpCodes.Ldloc_1)
        generator.Emit(OpCodes.Ldc_I4_M1)
        generator.Emit(OpCodes.Add)
        generator.Emit(OpCodes.Stloc_1)
        generator.Emit(OpCodes.Br_S, outerLoop)
        generator.MarkLabel(afterOuterLoop)
        // outer loop end
        generator.Emit(OpCodes.Ldloc_0)
        generator.Emit(OpCodes.Ret)
        let simpleLoopFunc : Func<int, int> = simpleLoop.CreateDelegate(typeof<Func<int, int>>) :?> Func<int, int>
        let result : int = simpleLoopFunc.Invoke(source)
        Assert.AreEqual(source + (outerLoopCount * innerLoopCount), result)

    [<Test>]
    member public this.NestedWhileLoopsUseStack() =
        let source : int = 10
        let outerLoopCount : int = 6
        let innerLoopCount : int = 4
        let simpleLoop : DynamicMethod = new DynamicMethod("NestedWhileLoopsDynamicMethod", typeof<int>, [|typeof<int>|])
        simpleLoop.InitLocals <- true
        let generator : ILGenerator = simpleLoop.GetILGenerator()
        // result
        generator.DeclareLocal(typeof<int>) |> ignore
        // labels
        let outerLoop : Label = generator.DefineLabel()
        let innerLoop : Label = generator.DefineLabel()
        let afterOuterLoop : Label = generator.DefineLabel()
        let afterInnerLoop : Label = generator.DefineLabel()
        // body
        generator.Emit(OpCodes.Ldarg_0)
        generator.Emit(OpCodes.Stloc_0)
        generator.Emit(OpCodes.Ldc_I4, outerLoopCount)
        // outer loop
        generator.MarkLabel(outerLoop)
        generator.Emit(OpCodes.Dup)
        generator.Emit(OpCodes.Brfalse_S, afterOuterLoop)
        generator.Emit(OpCodes.Ldc_I4, innerLoopCount)
        // inner loop
        generator.MarkLabel(innerLoop)
        generator.Emit(OpCodes.Dup)
        generator.Emit(OpCodes.Brfalse_S, afterInnerLoop)
        generator.Emit(OpCodes.Ldloc_0)
        generator.Emit(OpCodes.Ldc_I4_1)
        generator.Emit(OpCodes.Add)
        generator.Emit(OpCodes.Stloc_0)
        generator.Emit(OpCodes.Ldc_I4_M1)
        generator.Emit(OpCodes.Add)
        generator.Emit(OpCodes.Br_S, innerLoop)
        // inner loop end
        generator.MarkLabel(afterInnerLoop)
        generator.Emit(OpCodes.Pop)
        generator.Emit(OpCodes.Ldc_I4_M1)
        generator.Emit(OpCodes.Add)
        generator.Emit(OpCodes.Br_S, outerLoop)
        generator.MarkLabel(afterOuterLoop)
        // outer loop end
        generator.Emit(OpCodes.Pop)
        generator.Emit(OpCodes.Ldloc_0)
        generator.Emit(OpCodes.Ret)
        let simpleLoopFunc : Func<int, int> = simpleLoop.CreateDelegate(typeof<Func<int, int>>) :?> Func<int, int>
        let result : int = simpleLoopFunc.Invoke(source)
        Assert.AreEqual(source + (outerLoopCount * innerLoopCount), result)

    member private this.LoadSubsystemField(generator : ILGenerator) =
        generator.Emit(OpCodes.Ldarg_0)
        generator.Emit(OpCodes.Ldfld, subsystemField)