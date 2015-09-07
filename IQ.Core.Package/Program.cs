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

    public class PackageConfig
    {
        public Version Version { get; set; }

        public List<string> AssemblyNames { get; set; }
    }

    public class PackageToolConfig
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
                XmlDocumentation = false,
                DebugInfo = true
            });
            repack.Repack();
        }
    }

    class Program
    {

        private static string GetSourceDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
        }

        private static void ObserveResolutions()
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

        private static readonly string WorkingDirectory = @"C:\Temp\isocore.data";
        private static readonly string TargetDirectory = @"C:\Work\lib\packages";
        private static Version PackageVersion = Version.Parse("1.0.31");

        private static void CreateIsocoreData()
        {

            var libdir = Path.Combine(WorkingDirectory, @"lib\net45\");
            Directory.CreateDirectory(libdir);

            var outputAssemblyName = "isocore.data.dll";
            var outputAssemblyPath = Path.Combine(libdir, outputAssemblyName);
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

            var nuspec = GetResourceText("isocore.data.nuspec");
            var assFiles = new List<string>();
            foreach(var assName in assNames)
            {
                var assembly = Assembly.LoadFrom($"{assName}.dll");
                assFiles.Add(assembly.CodeBase.Replace("file:///", String.Empty));
            }
            Packaging.MergeAssemblies(outputAssemblyPath, PackageVersion, assFiles.ToArray());

        }

        private static void NupackIsocoreData()
        {
            var outputNuspecName = "isocore.data.nuspec";
            var outputNuspecPath = Path.Combine(WorkingDirectory, outputNuspecName);
            var versionText = String.Format($"{PackageVersion.Major}.{PackageVersion.Minor}.{PackageVersion.Build}");
            var nuspec = GetResourceText(outputNuspecName).Replace("$VERSION$", versionText);
            File.WriteAllText(outputNuspecPath, nuspec);



            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            //p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "nuget.exe";
            p.StartInfo.Arguments = $"pack \"{outputNuspecPath}\" -Verbosity detailed -OutputDirectory \"{TargetDirectory}\"";
            p.Start();
            p.WaitForExit();
        }


        static void Main(string[] args)
        {
            if (Directory.Exists(WorkingDirectory))
                Directory.Delete(WorkingDirectory, true);

            Directory.CreateDirectory(WorkingDirectory);

            CreateIsocoreData();
            NupackIsocoreData();

            
        }
    }
}
