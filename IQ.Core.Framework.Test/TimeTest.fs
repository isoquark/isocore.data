namespace IQ.Core.Framework.Test

open System

 open XUnit

type TimeTests(context,log) =    
    inherit ProjectTestContainer(context,log)
    [<Fact>]
    let ``Convert between BCL and Framework Date/Time representations``() =
        
        log.WriteLine("This is output for test 1")