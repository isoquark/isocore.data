namespace IQ.Core.Etl

open IQ.Core.Framework


[<AutoOpen>]
module EtlVocabulary =            
    /// <summary>
    /// Represents a data source
    /// </summary>
    type Source<'S> = Source of (unit->'S seq)
    
    /// <summary>
    /// Represents a data destination
    /// </summary>
    type Target<'T> = Target of ('T seq->unit)

    type Transform<'S, 'T, 'C> = Transform of ('S seq->'T seq->'C)
    
    type Filter<'T,'C> = Filter of ('T seq->'T seq->'C)

    type Join<'S0,'S1,'T,'C> = Join of ('S0 seq ->'S1 seq ->'T seq->'C)
    
    //type Split<'T, 'C> = Split of 'T s

