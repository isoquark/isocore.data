// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.MetaCode.Test.Prototypes

type Enum1 =
    | Field01 = 1
    | Field02 = 2

type Enum2 =
    | Field01 = 1u
    | Field02 = 2u 

type Class1() =
    //This will never show up in the IL
    let LetValue1 = 3
    //This will never show up in the IL
    [<Literal>]
    let LetValue2 = 4u

    let message1 = "hello"

    [<Literal>]
    let message2 = "world"

    //auto-implemented property
    member val public Property1 = 5
    //auto-implemented property
    member val private Property2 = 6u
    //auto-implemented property
    member val internal Property3 = 6L

    member this.Property4 = message1

    member this.Property5 = message2


    [<DefaultValue>]
    val mutable public Field1 : int

type Record1 = {
    Record1Field1 : string
    Record1Field2 : int
    Record1Field3 : decimal
}    

type Union1 =
    | Union1Case1
    | Union1Case2
   
type Union2 =
    | Union2Case1 of int
    | Union2Case2 of string    



