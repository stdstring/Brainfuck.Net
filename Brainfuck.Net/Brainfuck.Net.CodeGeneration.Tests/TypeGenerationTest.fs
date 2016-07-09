namespace Brainfuck.Net.CodeGeneration.Tests

open System
open System.Linq
open System.Reflection
open System.Reflection.Emit
open NUnit.Framework

[<TestFixture>]
type TypeGenerationTest() =
    let assemblyName : string = "TestAssembly"
    let moduleName : string = "TestModule"
    let typeName : string = "TestClass"

    [<Test>]
    member public this.GenerateAssemblyWithClass() =
        let generatedAssembly : Assembly = this.GenerateAssembly()
        let desiredType : Type = generatedAssembly.GetTypes().FirstOrDefault(fun t -> String.Equals(t.Name, typeName))
        Assert.IsNotNull(desiredType)
        Assert.IsTrue(desiredType.IsClass)
        Assert.IsTrue(desiredType.IsPublic)
        let defaultCtor : ConstructorInfo = desiredType.GetConstructor(Array.empty<Type>)
        Assert.IsNotNull(defaultCtor)
        Assert.IsTrue(defaultCtor.IsPublic)
        let doMethod : MethodInfo = desiredType.GetMethod("Do", Array.empty<Type>)
        Assert.IsNotNull(doMethod);
        Assert.IsTrue(doMethod.IsPublic);
        let do2Method : MethodInfo = desiredType.GetMethod("Do2", [|typeof<obj>; typeof<int>|])
        Assert.IsNotNull(do2Method)
        Assert.IsTrue(do2Method.IsPublic)

    member private this.GenerateAssembly() : Assembly =
        let overrideAttrs : MethodAttributes = MethodAttributes.Public ||| MethodAttributes.ReuseSlot ||| MethodAttributes.Virtual ||| MethodAttributes.HideBySig
        let assemblyName : AssemblyName = new AssemblyName(assemblyName)
        let assemblyBuilder : AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave)
        let moduleBuilder : ModuleBuilder = assemblyBuilder.DefineDynamicModule(moduleName)
        let typeBuilder : TypeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public ||| TypeAttributes.Class, typeof<obj>, [|typeof<ISomeTestInterface>|])
        let doMethodBuilder : MethodBuilder = typeBuilder.DefineMethod("Do", overrideAttrs)
        let doMethodBody : ILGenerator = doMethodBuilder.GetILGenerator()
        doMethodBody.Emit(OpCodes.Ret)
        let do2MethodBuilder : MethodBuilder = typeBuilder.DefineMethod("Do2", overrideAttrs, typeof<string>, [|typeof<obj>; typeof<int>|])
        let do2MethodBody : ILGenerator = do2MethodBuilder.GetILGenerator()
        // param1
        do2MethodBody.Emit(OpCodes.Pop)
        // param2
        do2MethodBody.Emit(OpCodes.Pop)
        do2MethodBody.Emit(OpCodes.Ldstr, "ReturnValue")
        do2MethodBody.Emit(OpCodes.Ret)
        typeBuilder.CreateType() |> ignore
        assemblyBuilder :> Assembly