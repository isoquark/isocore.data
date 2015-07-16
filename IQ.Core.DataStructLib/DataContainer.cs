// Copyright (c) Mihaela Iridon and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace IQ.Core.DataStructLib
{
    public abstract class DataContainer<TContainer, TData> : IDataContainer where TContainer : class, new()
    {
        protected static readonly Type[] _types = typeof(int).Assembly.GetTypes();
        protected TContainer _converterMap = null;

        protected abstract TData GetData(Type inType, Type outType);

        public long ExecuteRandomLookup(int numLookups)
        {
            var notFound = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var lookupValues = GenerateRandomLookupKeys(numLookups);
            for (int i = 0; i < numLookups; i++)
            //Parallel.For(0, numLookups, (i) =>
            {
                var inOutTypes = lookupValues[i];
                var val = GetData(inOutTypes.Item1, inOutTypes.Item2);
                if (val == null) Interlocked.Increment(ref notFound);
            }//);
            stopwatch.Stop();
            Console.WriteLine("Not found = " + notFound);
            return stopwatch.ElapsedMilliseconds;
        }

        public void Init(int? numberOfTypes = default(int?))
        {
            if (!numberOfTypes.HasValue)
                numberOfTypes = _types.Length;
            _converterMap = PopulateContainerWithRandomData(numberOfTypes.Value);
        }

        protected abstract TContainer PopulateContainerWithRandomData(int numberOfTypes);

        private static List<Tuple<Type, Type>> GenerateRandomLookupKeys(int numLookups)
        {
            var rand = new Random(DateTime.Now.Millisecond);
            var max = _types.Length;

            //fill out an array of what types to look up, uses random, we don't want the random gen to skew results
            var lookupValues = new List<Tuple<Type, Type>>();
            for (int i = 0; i < numLookups; i++)
            {
                lookupValues.Add(new Tuple<Type, Type>(_types[rand.Next(0, max)], _types[rand.Next(0, max)]));
            }
            return lookupValues;
        }

        #region IDisposable Support
        //protected abstract void DisposeContainer();
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //DisposeContainer();
                    _converterMap = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~DataContainer()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
