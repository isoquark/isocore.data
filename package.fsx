open System.IO
//OK, this is quite lame but is the absolute minimum I need to be able to package up
//my work; it assumes the existence of a folder structure organized like a NUGET
//package...then manual steps are required to specify the version
//I just don't have time right now to deal with properly-automated package
//creation

//TODO: Read version from NUSPEC file
//TODO: Read VS local package repository setting from (registry?)

let nuspecSrcFile = Path.Combine(__SOURCE_DIRECTORY__, "isocore.nuspec")
let nuspecDstFile = Path.Combine(__SOURCE_DIRECTORY__, @"build\packaging\package\isocore.nuspec")
File.Copy(nuspecSrcFile, nuspecDstFile, true)

let srcDir = Path.Combine(__SOURCE_DIRECTORY__, @"build\targets\anycpu")
let srcFiles = Directory.GetFiles(srcDir, "IQ.Core.*") |> Array.filter(fun x -> x.IndexOf(".Test.") = - 1 && x.IndexOf("TestFramework") = -1)
let dstDir = Path.Combine(__SOURCE_DIRECTORY__, @"build\packaging\package\lib\net45\")
if Directory.Exists(dstDir) then
    Directory.Delete(dstDir, true)
Directory.CreateDirectory(dstDir)
srcFiles |> Array.iter(fun srcFile ->
    let dstFile = Path.Combine(dstDir, Path.GetFileName(srcFile))
    File.Copy(srcFile, dstFile, true)
)

let batFilePath = Path.Combine(__SOURCE_DIRECTORY__, @"build\packaging\package\nupak.bat")
File.WriteAllText(batFilePath, @"..\..\..\.nuget\nuget pack isocore.nuspec")

 




