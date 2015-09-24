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

module WB03 =
    type WS01() =
        member val Col01 = String.Empty with get, set 
        member val Col02 = 0 with get, set
        member val Col03 = false with get, set
        member val Col04 = 0.0 with get, set
        member val Col05 = DateTime.MinValue with get,set