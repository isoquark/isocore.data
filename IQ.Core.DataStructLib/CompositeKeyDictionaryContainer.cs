// Copyright (c) Mihaela Iridon and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;

namespace IQ.Core.DataStructLib
{ 
    public class CompositeKeyDictionaryContainer : DataContainer<Dictionary<UInt64, string>, string>
    {
        private static UInt64 GetKeySlow(Type inputType, Type outputType)
        {
            var inHash = inputType.GetHashCode();
            var outHash = outputType.GetHashCode();
            var compositeHash = string.Format("{0}{1}", inHash, outHash);

            var key = 0ul;
            if (UInt64.TryParse(compositeHash, out key))
                return key;
            throw new ApplicationException("could not convert key to UInt64");
        }

        private static UInt64 GetKey(Type inputType, Type outputType)
        {
            var inHash = inputType.GetHashCode();
            var outHash = outputType.GetHashCode();

            return ((ulong)inHash << 32) + (ulong)outHash; //same execution time as with bitwise OR
        }

        private static Tuple<UInt64,string> GetKVP(Type inputType, Type outputType)
        {
            var key = GetKey(inputType, outputType);
            var delPlaceholder = string.Format("{0}_{1}", inputType.Name, outputType.Name);

            return new Tuple<UInt64,string>(key, delPlaceholder);
        }

        protected override Dictionary<UInt64, string> PopulateContainerWithRandomData(int numberOfTypes)
        {
            var result = new Dictionary<UInt64, string>();
            var typeCnt = _types.Length;
            Console.WriteLine("number of types = {0}", typeCnt);

            //foreach (var tIn in _types)
            for (int i = 0; i< Math.Min(numberOfTypes, typeCnt); i++)
            {
                var tIn = _types[i];
                //foreach (var tOut in _types)
                for (int j = 0; j < Math.Min(numberOfTypes, typeCnt); j++)
                {
                    var tOut = _types[j];
                    var kvp = GetKVP(tIn, tOut);
                    result.Add(kvp.Item1, kvp.Item2);
                }
            }
            return result;
        }

        protected override string GetData(Type inType, Type outType)
        {
            var key = GetKey(inType, outType);

            if (_converterMap.ContainsKey(key))
                return _converterMap[key];
            return null;
        }
    }
}
