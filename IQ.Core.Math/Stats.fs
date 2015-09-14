namespace IQ.Core.Math

open IQ.Core.Framework

open MathNet.Numerics.Statistics

module Stats =
    /// <summary>
    /// Computes the standard deviation of the supplied values
    /// </summary>
    let sd(items : 'T seq) =
        let converted : float seq = items |> SmartConvert.convertAll
        Statistics.StandardDeviation(converted) |> SmartConvert.convertT<'T>   
    

