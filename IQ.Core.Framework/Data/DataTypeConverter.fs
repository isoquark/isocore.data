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
        | BitDataType -> typeof<bool>
        | UInt8DataType -> typeof<uint8>
        | UInt16DataType -> typeof<int32>
        | UInt32DataType -> typeof<int64>
        | UInt64DataType -> typeof<byte[]> //8
        | Int8DataType -> typeof<int16>
        | Int16DataType -> typeof<int16>
        | Int32DataType -> typeof<int>
        | Int64DataType -> typeof<int64>
                        
        | BinaryFixedDataType(_) -> typeof<byte[]>
        | BinaryVariableDataType(_) -> typeof<byte[]>
        | BinaryMaxDataType -> typeof<byte[]>
            
        | AnsiTextFixedDataType(length) -> typeof<string>
        | AnsiTextVariableDataType(length) -> typeof<string>
        | AnsiTextMaxDataType -> typeof<string>
            
        | UnicodeTextFixedDataType(length) -> typeof<string>
        | UnicodeTextVariableDataType(length) -> typeof<string>
        | UnicodeTextMaxDataType -> typeof<string>
            
        | DateTimeDataType(precision)-> typeof<BclDateTime>
        | DateTimeOffsetDataType -> typeof<BclDateTimeOffset>
        | TimeOfDayDataType -> typeof<BclTimeSpan>
        | DateDataType -> typeof<BclDateTime>
        | TimespanDataType -> typeof<int64>
            
        | Float32DataType -> typeof<float32>
        | Float64DataType -> typeof<float>
        | DecimalDataType(precision,scale) ->typeof<decimal>
        | MoneyDataType -> typeof<decimal>
        | GuidDataType -> typeof<Guid>
        | XmlDataType(schema) -> typeof<string>
        | VariantDataType -> typeof<obj>
        | CustomTableDataType(name) -> typeof<obj>
        | CustomObjectDataType(name,t) -> typeof<obj>
        | CustomPrimitiveDataType(name) -> typeof<obj>
        | TypedDocumentDataType(t) -> typeof<obj>

    [<TransformationAttribute>]
    let private timespanToTicks (ts : BclTimeSpan) =
        ts.Ticks

    [<TransformationAttribute>]
    let private ticksToTimespan (ticks : int64) =
        TimeSpan.FromTicks

    let toClrTransportValue dataType  (value : obj) =
        let value = value |> stripOption
        if value = null then
            DBNull.Value :> obj
        else
            let clrType = dataType |> toClrTransportType
            value |> Transformer.convert clrType
    

    let fromClrTransportValue dstType (value : obj) =
        value |> Transformer.convert dstType


