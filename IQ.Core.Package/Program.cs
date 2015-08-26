using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using System.IO;
using NuGet;

//using IQ.Core.Data;
//using IQ.Core.Data.Sql;
//using IQ.Core.Framework;

//using static IQ.Core.Data.DataAttributes;

namespace IQ.Core.Package
{

    //[Schema("SqlTest")]
    //public interface ISqlTestRoutines
    //{
    //    [Procedure]
    //    int pTable03Insert(Byte Col01, Int16 Col02, Int32 Col03, Int64 Col04);

    //}


    static class NugetUtil
    {

    }


    static class Packaging
    {
        public static void MergeAssemblies(string outfileName, Version version, params string[] sources)
        {
            var repack = new ILRepacking.ILRepack(new ILRepacking.RepackOptions
            {
                InputAssemblies = sources,
                OutputFile = outfileName,
                Version = version,
                SearchDirectories = new List<string>(),
                XmlDocumentation = false
            });
            repack.Repack();
        }
    }

    class Program
    {
        //private static void TestSqlData()
        //{
        //    var cs = "Initial Catalog=isocore;Data Source=eXaCore03;Integrated Security=SSPI";
        //    var config = SqlDataStoreConfig.NewSqlDataStoreConfig(cs, ClrMetadataProvider.getDefault());
        //    var store = SqlDataStore.get(config);
        //    var routines = store.GetContract<ISqlTestRoutines>();
        //    var result = routines.pTable03Insert(5, 10, 15, 20);
        //}

        private static IReadOnlyList<string> GetSimpleAssemblyNames(string configName)
        {
            return new string[]
            {
                "IQ.Core.Contracts.dll",
                "IQ.Core.Framework.dll",
                "IQ.Core.Data.dll",
                "IQ.Core.Data.Excel.dll",
                "IQ.Core.Data.Sql.dll",
                "IQ.Core.Math.dll",
                "IQ.Core.Synthetics.dll",
                "AutoFac.dll",
                "Castle.Core.dll",
                "CsvHelper.dll",
                "EPPlus.dll",
                "FSharp.Compiler.Service.dll",
                "FSharp.Core.dll",
                "FSharp.Data.dll",
                "FSharp.Text.RegexProvider.dll",
                "MathNet.Numerics.dll",
            };

        }

        private static string GetSourceDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
        }

        static void Main(string[] args)
        {
            var hostAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var loadedNames = new Dictionary<string, System.Reflection.Assembly>();
            System.AppDomain.CurrentDomain.AssemblyResolve += (s, eventArgs) =>
                {
                    var assname = new AssemblyName(eventArgs.Name);
                    if (loadedNames.ContainsKey(assname.FullName))
                        return loadedNames[assname.FullName];

                    
                    var resname = $"IQ.Core.Package.Assemblies.{eventArgs.Name}";
                    using (var stream = hostAssembly.GetManifestResourceStream(resname))
                    {
                        if (stream != null)
                        {
                            var data = new Byte[stream.Length];
                            stream.Read(data, 0, data.Length);
                            var a = System.Reflection.Assembly.Load(data);
                            loadedNames[a.GetName().FullName] = a;
                            return a;
                        }
                        else
                        {
                            return null;
                        }
                    }

                };

            var configName = "isocore.data.dll";
            var assemblyFiles = new List<string>();
            foreach(var simpleName in GetSimpleAssemblyNames(configName))
            {
                var assembly = Assembly.LoadFrom(simpleName);
                assemblyFiles.Add(assembly.CodeBase.Replace("file:///", String.Empty));

            }

            var version = new Version(1, 0, 26);
            var outdll = @"C:\Work\Lib\dll\isocore.data.dll";
            Packaging.MergeAssemblies(outdll, version, assemblyFiles.ToArray());

            //var outpkg = Path.ChangeExtension(outdll, $".{version.Major}.{version.Minor}.{version.Build}.nupkg");
            //var packageMetadata = new ManifestMetadata
            //{
            //    Authors = "Chris Moore",
            //    Version = version.ToString(),
            //    Id = "isocore.data",
            //    Description = "Data development"
            //};
            //var packageBuilder = new PackageBuilder();


            Console.WriteLine("Done");
        }
    }
}
