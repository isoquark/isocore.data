namespace IQ.Core.Data.Excel.Test

open System



module WB01 =
    type WS01 = {
        Col01 : string
        Col02 : int
        Col03 : decimal
        Col04 : DateTime
    }

    type WS02 = {
        Name : string
        Value : int
    }


module WB02 =
    type WS01 = {
        Col01 : string
        Col02 : int
        Col03 : decimal
        Col04 : DateTime
        Col05 : uint16
        Col06 : float
        Col07 : uint64
    
    }