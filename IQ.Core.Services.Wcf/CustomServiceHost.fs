namespace IQ.Core.Services.Wcf

open System
open System.IO
open System.Reflection
open System.ServiceModel
open System.ServiceModel.Activation
open System.Configuration
open Helpers

module CustomServiceHostUtil =
   //extend this class to create your custom host factory (for web-hosted or Windows Service-hosted WCF services)
   //override the host open/closed/closing event handlers as needed
   type CustomServiceHostFactory() =
        inherit ServiceHostFactory()
     
        //virtual protected members
        abstract member host_Opened : obj -> EventArgs -> unit
        abstract member host_Closing : obj -> EventArgs -> unit
        abstract member host_Closed : obj -> EventArgs -> unit

        override this.host_Opened host eventargs = () 
        override this.host_Closing host eventargs = ()
        override this.host_Closed host eventargs = ()
        //end virtual protected members

        //local private members
        member this.hostOpened (host :obj) (e : EventArgs) =
            let h = host :?> ServiceHost
            let t = h.Description.ServiceType
            //Console.WriteLine("Host for service type {0} OPENED.", t.Name)
            this.host_Opened() |> ignore

        member this.hostClosing (host :obj) (e : EventArgs) =
            let h = host :?> ServiceHost
            let t = h.Description.ServiceType
            //Console.WriteLine("Host for service type {0} CLOSING.", t.Name)
            this.host_Closing() |> ignore

        member this.hostClosed (host :obj) (e : EventArgs) =
            let h = host :?> ServiceHost
            let t = h.Description.ServiceType
            //Console.WriteLine("Host for service type {0} CLOSED.", t.Name)
            this.host_Closed() |> ignore

        member this.AddHandlers (host : ServiceHost byref) =
            host.Opened.AddHandler(new EventHandler(this.hostOpened))
            host.Closing.AddHandler(new EventHandler(this.hostClosing))
            host.Closed.AddHandler(new EventHandler(this.hostClosed))

        member this.CreateServiceHost serviceType =
            let host = new ServiceHost(serviceType)
            this.AddHandlers(ref host)
            host

        override  this.CreateServiceHost (serviceType, baseAddresses)  =
            let host = new ServiceHost(serviceType, baseAddresses)
            this.AddHandlers(ref host)
            host

    //extend this type for Console-hosted WCF services. Service endpoins will be read from the config file and
    //the corresponding services will be self-hosted. See sample ConsoleHost application.
    //Being a console host, it contains output messages to the Console window.
    type ConsoleServiceHostFactory() =
        inherit CustomServiceHostFactory()

        let mutable hosts = List.empty

        member this.GetServiceImplementationTypesBasedOnSvcModelFromConfig ()=
            let path = getItemFromAppSettingsWithDefault "ServiceImplementationsPath" 
                            (fun() -> Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) |> Path.GetFullPath)
            let pattern = getItemFromAppSettingsWithDefault "WcfHosting.EndpointContractAssemblyNamePrefix"
                            (fun() -> "")
            let asmFiles = Directory.GetFiles(path, String.Format("{0}*.dll", pattern)) //implementation assemblies
            let assemblies = asmFiles |> Seq.map (fun x -> Assembly.Load(AssemblyName.GetAssemblyName(x)) )
            let allTypes = assemblies |>  Seq.collect (fun a -> a.GetTypes() |> Seq.map (fun x -> x)) //select many
            let props = getServiceEndpoints() 
            [ for prop in props -> prop.Name, (getTypeImplementingServiceEndpoints prop allTypes)] |> Map.ofSeq           

        member this.StartRun () =
            Console.WriteLine("Starting service hosts...")
            this.GetServiceImplementationTypesBasedOnSvcModelFromConfig() |>
                Seq.iter (fun (s) -> 
                            s.Value |> Seq.iter (fun sType -> 
                                                    let host = (this :> CustomServiceHostFactory).CreateServiceHost (sType)
                                                    Console.WriteLine("Opening {0} Host for service {1}", sType.Name, s.Key)
                                                    host.Open()
                                                    hosts <- host :: hosts
                                               )
                        )
            Console.WriteLine("ALL hosts opened. Press any key to shut down hosts")

        member this.StopRun () = 
            Console.WriteLine("Closing all hosts...")
            hosts |> List.iter (fun h -> h.Close())
            Console.WriteLine("Hosts closed. BYE.")

        member this.Run () =
            this.StartRun()
            Console.ReadKey() |> ignore
            this.StopRun () 
            System.Threading.Thread.Sleep(1000)

