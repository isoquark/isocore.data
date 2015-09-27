using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using System.IO;
using System.Diagnostics;




namespace IQ.Core.Package
{


    public class PackageToolConfig
    {
        public string WorkingDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public Version Version { get; set; }
        public List<string> InputAssemblyNames { get; set; }
        public string OutputNuspecName { get; set; }       
        public string CondensedAssemblyName { get; set; }
        public string NuspecTemplateName { get; set; }
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
                XmlDocumentation = false,
                DebugInfo = true
            });
            repack.Repack();
        }
    }

    class Program
    {

        private static PackageToolConfig CreateConfig(string version, string outdir, string workdir)
        {
            return new PackageToolConfig
            {
                InputAssemblyNames = new List<string>
                {
                    Contracts.ContractAssemblyDescriptor.SimpleName,
                    Framework.FrameworkAssemblyDescriptor.SimpleName,
                    Data.DataAssemblyDescriptor.SimpleName,
                    Data.Excel.ExcelAssemblyDescriptor.SimpleName,
                    Data.Sql.SqlAssemblyDescriptor.SimpleName,
                    Math.MathAssemblyDescriptor.SimpleName,
                    Synthetics.SyntheticsAssemblyDescriptor.SimpleName,

                },
                CondensedAssemblyName = "isocore.data.dll",
                OutputNuspecName = "isocore.data.nuspec",
                NuspecTemplateName = "isocore.data.nuspec",
                OutputDirectory = outdir,
                Version = Version.Parse(version),
                WorkingDirectory = workdir

            };
        }

        //private static string GetSourceDirectory()
        //{
        //    return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
        //}

        //private static void ObserveResolutions()
        //{
        //    var hostAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        //    var loadedNames = new Dictionary<string, System.Reflection.Assembly>();
        //    System.AppDomain.CurrentDomain.AssemblyResolve += (s, eventArgs) =>
        //        {
        //            var assname = new AssemblyName(eventArgs.Name);
        //            if (loadedNames.ContainsKey(assname.FullName))
        //                return loadedNames[assname.FullName];


        //            var resname = $"IQ.Core.Package.Assemblies.{eventArgs.Name}";
        //            using (var stream = hostAssembly.GetManifestResourceStream(resname))
        //            {
        //                if (stream != null)
        //                {
        //                    var data = new Byte[stream.Length];
        //                    stream.Read(data, 0, data.Length);
        //                    var a = System.Reflection.Assembly.Load(data);
        //                    loadedNames[a.GetName().FullName] = a;
        //                    return a;
        //                }
        //                else
        //                {
        //                    return null;
        //                }
        //            }

        //        };
        //}


        private static string GetResourceText(string partialName)
        {
            var ass = Assembly.GetExecutingAssembly();
            var resname = ass.GetManifestResourceNames().First(x => x.Contains(partialName));
            using (var reader = new StreamReader(ass.GetManifestResourceStream(resname)))
            {
                return reader.ReadToEnd();
            }

        }

        //private static readonly string WorkingDirectory = @"C:\Temp\isocore.data";
        //private static readonly string TargetDirectory = @"C:\Work\lib\packages";
        //private static Version PackageVersion = Version.Parse("1.0.31");

        private static void CreateIsocoreData(PackageToolConfig config)
        {

            var libdir = Path.Combine(config.WorkingDirectory, @"lib\net45\");
            Directory.CreateDirectory(libdir);

            //var outputAssemblyName = "isocore.data.dll";
            var outputAssemblyPath = Path.Combine(libdir, config.CondensedAssemblyName);
            //The simple names of the assemblies to be packaged
            var assNames = new []
                {
                    Contracts.ContractAssemblyDescriptor.SimpleName,
                    Framework.FrameworkAssemblyDescriptor.SimpleName,
                    Data.DataAssemblyDescriptor.SimpleName,
                    Data.Excel.ExcelAssemblyDescriptor.SimpleName,
                    Data.Sql.SqlAssemblyDescriptor.SimpleName,
                    Math.MathAssemblyDescriptor.SimpleName,
                    Synthetics.SyntheticsAssemblyDescriptor.SimpleName,
                };

            var nuspec = GetResourceText(config.NuspecTemplateName);
            var assFiles = new List<string>();
            foreach(var assName in assNames)
            {
                var assembly = Assembly.LoadFrom($"{assName}.dll");
                assFiles.Add(assembly.CodeBase.Replace("file:///", String.Empty));
            }
            Packaging.MergeAssemblies(outputAssemblyPath, config.Version, assFiles.ToArray());

        }

        private static void NupackIsocoreData(PackageToolConfig config)
        {
            //var outputNuspecName = "isocore.data.nuspec";
            var outputNuspecPath = Path.Combine(config.WorkingDirectory, config.OutputNuspecName);
            var versionText = String.Format($"{config.Version.Major}.{config.Version.Minor}.{config.Version.Build}");
            var nuspec = GetResourceText(config.OutputNuspecName).Replace("$VERSION$", versionText);
            File.WriteAllText(outputNuspecPath, nuspec);



            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            //p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "nuget.exe";
            p.StartInfo.Arguments = $"pack \"{outputNuspecPath}\" -Verbosity detailed -OutputDirectory \"{config.OutputDirectory}\"";
            p.Start();
            p.WaitForExit();
        }


        static void Main(string[] args)
        {

            //var config = CreateConfig("1.0.35", @"C:\dev\packages", @"C:\Temp\isocore.data");
            var config = CreateConfig("1.0.35", @"C:\Work\lib\packages", @"C:\Temp\isocore.data");

            if (Directory.Exists(config.WorkingDirectory))
                Directory.Delete(config.WorkingDirectory, true);

            Directory.CreateDirectory(config.WorkingDirectory);

            CreateIsocoreData(config);
            NupackIsocoreData(config);

            
        }
    }
}
