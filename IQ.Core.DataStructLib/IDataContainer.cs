// Copyright (c) Mihaela Iridon and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;

namespace IQ.Core.DataStructLib
{
    public interface IDataContainer : IDisposable
    {
        void Init(int? n = null);
        long ExecuteRandomLookup(int numLookups = 1000000);
    }
}
