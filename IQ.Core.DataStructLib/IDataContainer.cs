using System;

namespace IQ.Core.DataStructLib
{
    public interface IDataContainer : IDisposable
    {
        void Init(int? n = null);
        long ExecuteRandomLookup(int numLookups = 1000000);
    }
}
