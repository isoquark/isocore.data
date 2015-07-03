using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IQ.Core.DataStructLib
{
    public class DictionaryOfDictionariesContainer : DataContainer<Dictionary<int, Dictionary<int, string>>, string>
    {
        //private string[,] _converterMap = null;

        private static Tuple<int, int> GetKeys(Type inputType, Type outputType)
        {
            var inHash = inputType.GetHashCode();
            var outHash = outputType.GetHashCode();
            return new Tuple<int, int>(inHash, outHash);
        }

        protected override Dictionary<int, Dictionary<int, string>> PopulateContainerWithRandomData(int numberOfTypes)
        {
            var typeCnt = _types.Length;
            var result = new Dictionary<int, Dictionary<int, string>>();
            Console.WriteLine("number of types = {0}", typeCnt);

            for (int i = 0; i < Math.Min(numberOfTypes, typeCnt); i++)
            {
                var tIn = _types[i];
                var inHash = tIn.GetHashCode();
                result.Add(inHash, new Dictionary<int, string>());

                for (int j = 0; j < Math.Min(numberOfTypes, typeCnt); j++)
                {
                    var tOut = _types[j];
                    result[inHash].Add(tOut.GetHashCode(), string.Format("{0}.{1}", tIn.Name, tOut.Name));
                }
            }
            return result;
        }

        protected override string GetData(Type inType, Type outType)
        {
            var key = GetKeys(inType, outType);

            if (_converterMap.ContainsKey(key.Item1))
            {
                var d = _converterMap[key.Item1];
                if (d.ContainsKey(key.Item2))
                    return _converterMap[key.Item1][key.Item2]; //overwrite the val value; I only care that I retrieved the stored value
            }
            return null;
        }
    }
}
