namespace IQ.Core.MetaCode

open System
open System.Reflection
open System.Reflection.Emit
open System.IO


open FSharp.Reflection


open IQ.Core.Framework



[<AutoOpen>]
module ClrElementExtensions =
    type ClrTypeInfo
    with
        member this.BclVisibility = 
            match this.DeclaringType with
            | Some(declarer) ->
                match this.Access with
                | ClrAccessKind.Internal -> TypeAttributes.NestedAssembly
                | ClrAccessKind.Private -> TypeAttributes.NestedPrivate
                | ClrAccessKind.Public -> TypeAttributes.NestedPublic
                | ClrAccessKind.ProtectedAndInternal -> TypeAttributes.NestedFamANDAssem
                | ClrAccessKind.ProtectedOrInternal -> TypeAttributes.NestedFamORAssem
                | ClrAccessKind.Protected -> TypeAttributes.NestedFamily
                | _ -> nosupport()
            | None ->
                match this.Access with
                | ClrAccessKind.Internal -> 
                    TypeAttributes.NotPublic
                | ClrAccessKind.Public -> 
                    TypeAttributes.Public
                | _ -> nosupport()

    type ClrAssembly
    with
        member this.BclName = AssemblyName(this.Name.Text)

    type ClrEnum 
    with
        member this.BclVisibility = this.Info.BclVisibility

     
    type ClrType
    with
        member this.BclVisibility = this.Info.BclVisibility


            


module ClrTypeGenerator =
    let private genEnumLiteral (eb : EnumBuilder)  (f : ClrField) =
        let value = Convert.ChangeType(f.LiteralValue |> Option.get, eb.GetEnumUnderlyingType() )
        eb.DefineLiteral(f.Name.Text, value)        
    
    let private genEnum (mb : ModuleBuilder) (e : ClrEnum) =
        let enumName = match e.Name.FullName with | Some(x) -> x | None -> e.Name.SimpleName
        let numericType = e.NumericType |> Type.get 
        let visibility = e.BclVisibility        
        let eb = mb.DefineEnum(enumName, visibility, numericType)
        e.Literals |> List.map(fun f -> f |> genEnumLiteral eb) |> ignore
        eb.CreateType()
        
    let generate (mb : ModuleBuilder) (t : ClrType) =
        match t with
        | EnumType(e) ->e |> genEnum mb
        | _ -> nosupport()


module ClrAssemblyGenerator =
        
    let generate(a : ClrAssembly) = 
        let assname = a.BclName
        let ab = AssemblyBuilder.DefineDynamicAssembly(assname, AssemblyBuilderAccess.RunAndSave)
        let dir = Environment.CurrentDirectory
        let filename = sprintf "%s.dll" assname.Name
        let path = Path.Combine(dir, filename)
        let mb = ab.DefineDynamicModule(assname.Name, filename)
        let buildType t = t |> ClrTypeGenerator.generate mb 
        let types = a.Types |> List.map buildType        
        ab.Save(filename)
        path
        
       
        
    

