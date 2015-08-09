// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework.Contracts

open System

type BinaryFunc<'T,'TResult> = Func<'T,'T,'TResult>
type BinaryFunc<'T> = BinaryFunc<'T,'T>

type BinaryPredicate<'T0,'T1> = Func<'T0,'T1,bool>
type BinaryPredicate<'T> = BinaryPredicate<'T,'T>

type UnaryFunc<'T,'TResult> = Func<'T,'TResult>
type UnaryFunc<'T> = Func<'T,'T>


