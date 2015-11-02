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


using IQ.Core.Contracts;
using IQ.Core.Data;
using IQ.Core.Data.Contracts;
using IQ.Core.Framework;
using IQ.Core.Math;
using IQ.Core.Synthetics;

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

        private static List<string> AssemblyNames = new List<string>
                {
                    ContractAssemblyDescriptor.SimpleName,
                    FrameworkAssemblyDescriptor.SimpleName,
                    DataAssemblyDescriptor.SimpleName,
                    ExcelAssemblyDescriptor.SimpleName,
                    SqlAssemblyDescriptor.SimpleName,
                    MathAssemblyDescriptor.SimpleName,
                    SyntheticsAssemblyDescriptor.SimpleName,
                    TextDataAssemblyDescriptor.SimpleName

                };
        private static PackageToolConfig CreateConfig(string version, string outdir, string workdir)
        {
            return new PackageToolConfig
            {
                InputAssemblyNames = AssemblyNames,
                CondensedAssemblyName = "isocore.data.dll",
                OutputNuspecName = "isocore.data.nuspec",
                NuspecTemplateName = "isocore.data.nuspec",
                OutputDirectory = outdir,
                Version = Version.Parse(version),
                WorkingDirectory = workdir

            };
        }

        private static string GetResourceText(string partialName)
        {
            var ass = Assembly.GetExecutingAssembly();
            var resname = ass.GetManifestResourceNames().First(x => x.Contains(partialName));
            using (var reader = new StreamReader(ass.GetManifestResourceStream(resname)))
            {
                return reader.ReadToEnd();
            }

        }

        private static void CreateIsocoreData(PackageToolConfig config)
        {

            var libdir = Path.Combine(config.WorkingDirectory, @"lib\net45\");
            Directory.CreateDirectory(libdir);

            var outputAssemblyPath = Path.Combine(libdir, config.CondensedAssemblyName);

            var nuspec = GetResourceText(config.NuspecTemplateName);
            var assFiles = new List<string>();
            foreach(var assName in AssemblyNames)
            {
                var assembly = Assembly.LoadFrom($"{assName}.dll");
                assFiles.Add(assembly.CodeBase.Replace("file:///", String.Empty));
            }
            Packaging.MergeAssemblies(outputAssemblyPath, config.Version, assFiles.ToArray());

        }

        private static void NupackIsocoreData(PackageToolConfig config)
        {
            var outputNuspecPath = Path.Combine(config.WorkingDirectory, config.OutputNuspecName);
            var versionText = String.Format($"{config.Version.Major}.{config.Version.Minor}.{config.Version.Build}");
            var nuspec = GetResourceText(config.OutputNuspecName).Replace("$VERSION$", versionText);
            File.WriteAllText(outputNuspecPath, nuspec);


            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "nuget.exe";
            p.StartInfo.Arguments = $"pack \"{outputNuspecPath}\" -Verbosity detailed -OutputDirectory \"{config.OutputDirectory}\"";
            p.Start();
            p.WaitForExit();
        }

        private static void PublishDacpac(PackageToolConfig config)
        {

        }


        static void Main(string[] args)
        {
            //var config = CreateConfig("1.0.84", @"T:\lib\nuget\external", @"C:\Temp\isocore.data");
            var config = CreateConfig("1.0.101", @"C:\Work\lib\packages", @"C:\Temp\isocore.data");

            if (Directory.Exists(config.WorkingDirectory))
                Directory.Delete(config.WorkingDirectory, true);

            Directory.CreateDirectory(config.WorkingDirectory);

            CreateIsocoreData(config);
            NupackIsocoreData(config);
            PublishDacpac(config);

            
        }
    }
}
