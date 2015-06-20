open IQ.Core.Services.Wcf.CustomServiceHostUtil

type ConsoleHost() =
    inherit ConsoleServiceHostFactory()
    //overwrite host open/closed/closing handlers as needed
        

[<EntryPoint>]
let main argv = 
    let consoleHost = ConsoleHost()
    consoleHost.Run() //will start hosts and will wait for user to press a key to close the hosts
    0 // return an integer exit code


