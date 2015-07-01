namespace IQ.Core.Services.Wcf.SampleService

open System.Runtime.Serialization
open System


    [<CLIMutable>]
    type Name = {
        FirstName : string
        LastName : string
        MiddleInitial : char 
    }

    
//    [<DataContract>]
//    type Name() =
//        let mutable _firstName : string = String.Empty
//        let mutable _lastName : string = String.Empty
//        let mutable _middleInitial : Char = ' '
//
//        [<DataMember>]
//        member public l.FirstName
//            with get() = _firstName
//            and set(value) = _firstName <- value
//
//        [<DataMember>]
//        member public l.LastName
//            with get() = _lastName
//            and set(value) = _lastName <- value
//
//        [<DataMember>]
//        member public l.MiddleInitial
//            with get() = _middleInitial
//            and set(value) = _middleInitial <- value