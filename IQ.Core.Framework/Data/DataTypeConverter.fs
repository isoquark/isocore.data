namespace IQ.Core.Data

open System

open IQ.Core.Framework

module internal DataTypeConverter =
    let private stripOption (o : obj) =
        if o = null then
            o
        else if o |> Option.isOptionValue then
            match o |> Option.unwrapValue with
            | Some(x) -> x 
            | None -> DBNull.Value :> obj
        else
             o
    
    let toClrTransportType storageType =
        match storageType with
        | BitStorage -> typeof<bool>
        | UInt8Storage -> typeof<uint8>
        | UInt16Storage -> typeof<int32>
        | UInt32Storage -> typeof<int64>
        | UInt64Storage -> typeof<byte[]> //8
        | Int8Storage -> typeof<int16>
        | Int16Storage -> typeof<int16>
        | Int32Storage -> typeof<int>
        | Int64Storage -> typeof<int64>
                        
        | BinaryFixedStorage(_) -> typeof<byte[]>
        | BinaryVariableStorage(_) -> typeof<byte[]>
        | BinaryMaxStorage -> typeof<byte[]>
            
        | AnsiTextFixedStorage(length) -> typeof<string>
        | AnsiTextVariableStorage(length) -> typeof<string>
        | AnsiTextMaxStorage -> typeof<string>
            
        | UnicodeTextFixedStorage(length) -> typeof<string>
        | UnicodeTextVariableStorage(length) -> typeof<string>
        | UnicodeTextMaxStorage -> typeof<string>
            
        | DateTime32Storage -> typeof<DateTime>
        | DateTime64Storage -> typeof<DateTime>
        | DateTimeStorage(precision)-> typeof<DateTime>
        | DateTimeOffsetStorage -> typeof<DateTimeOffset>
        | TimeOfDayStorage -> typeof<TimeSpan>
        | DateStorage -> typeof<DateTime>
        | TimespanStorage -> typeof<int64>
            
        | Float32Storage -> typeof<float32>
        | Float64Storage -> typeof<float>
        | DecimalStorage(precision,scale) ->typeof<decimal>
        | MoneyStorage -> typeof<decimal>
        | GuidStorage -> typeof<Guid>
        | XmlStorage(schema) -> typeof<string>
        | VariantStorage -> typeof<obj>
        | CustomTableStorage(name) -> typeof<obj>
        | CustomObjectStorage(name,t) -> typeof<obj>
        | CustomPrimitiveStorage(name) -> typeof<obj>

    let private timespanToTicks (ts : TimeSpan) =
        ts.Ticks

    let private ticksToTimespan (ticks : int64) =
        TimeSpan.FromTicks

    let toClrTransportValue storageType  (value : obj) =
        let value = value |> stripOption
        if value = null then
            DBNull.Value :> obj
        else
            let clrType = storageType |> toClrTransportType
            value |> Converter.convert clrType
    
            


