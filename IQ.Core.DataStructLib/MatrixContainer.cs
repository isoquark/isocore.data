using System;
using System.Collections.Concurrent;

namespace IQ.Core.DataStructLib
{
    public class MatrixOfString
    {
        private string[,] _container;
        public string[,] Container
        {
            get { return _container; }
            set { _container = value; }
        }
    }

    public class MatrixContainer : DataContainer<MatrixOfString, string>
    {
        static MatrixContainer()
        {
           for (var i =0; i< _types.Length; i++)
           {
               typeIndex.TryAdd(_types[i].GetHashCode(), i);
           }
        }

        //type  hashcode indexing dictionary; if it needs to grow at runtime, remove readonly
        static readonly ConcurrentDictionary<int, int> typeIndex = new ConcurrentDictionary<int, int>();

        protected override string GetData(Type inType, Type outType)
        {
            var i = typeIndex[inType.GetHashCode()];
            var j = typeIndex[outType.GetHashCode()];

            return _converterMap.Container[i,j];
        }

        protected override MatrixOfString PopulateContainerWithRandomData(int numberOfTypes)
        {
            var typeCnt = _types.Length;
            string[,] matrix = new string[typeCnt, typeCnt];

            for (int i = 0; i < Math.Min(numberOfTypes, typeCnt); i++)
            {
                var tIn = _types[i];
                var inHash = tIn.GetHashCode();

                for (int j = 0; j < Math.Min(numberOfTypes, typeCnt); j++)
                {
                    var tOut = _types[j];
                    var outHash = tOut.GetHashCode();
                    var i1 = typeIndex[inHash];
                    var j1 = typeIndex[outHash];
                    matrix[i1, j1] = string.Format("{0}.{1}", tIn.Name, tOut.Name);
                }
            }

            var result = new MatrixOfString { Container = matrix };
            return result;
        }
    }
}
