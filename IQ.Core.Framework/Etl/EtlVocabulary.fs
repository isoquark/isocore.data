// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
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

    type Combine<'S0,'S1,'T,'C> = Combine of ('S0 seq ->'S1 seq ->'T seq->'C)
    
    type CrossJoin<'S0, 'S1, 'C> = CrossJoin of ('S0 seq -> 'S1 seq -> ('S0*'S1) seq -> 'C)

    

