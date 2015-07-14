namespace IQ.Core.Framework

open System

type BinaryFunc<'T,'TResult> = Func<'T,'T,'TResult>
type BinaryFunc<'T> = BinaryFunc<'T,'T>

type BinaryPredicate<'T0,'T1> = Func<'T0,'T1,bool>
type BinaryPredicate<'T> = BinaryPredicate<'T,'T>

type UnaryFunc<'T,'TResult> = Func<'T,'TResult>
type UnaryFunc<'T> = Func<'T,'T>


