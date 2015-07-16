// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Math

type IStats =
    abstract runifd: count: int * min : 'T * max : 'T -> 'T[]


