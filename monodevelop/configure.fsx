
open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.IO
open System.Diagnostics
open System.Text.RegularExpressions

let FSharpVersion = "3.2.17"

let LinuxPaths = 
    [ "/usr/lib/monodevelop"
      "/usr/local/monodevelop/lib/monodevelop"
      "/usr/local/lib/monodevelop"
      "/Applications/MonoDevelop.app/Contents/MacOS/lib/"
      "monodevelop"
      "/opt/mono/lib/monodevelop"
      "/Applications/Xamarin Studio.app/Contents/MacOS/lib/monodevelop" ]

let WindowsPaths = 
    [ @"C:\Program Files\Xamarin Studio"
      @"C:\Program Files\MonoDevelop"
      @"C:\Program Files (x86)\Xamarin Studio"
      @"C:\Program Files (x86)\MonoDevelop" ]

let MdCheckFile = "bin/MonoDevelop.Core.dll"


let IsWindows = (Path.DirectorySeparatorChar = '\\')

let GetPath (str: string list) =
    Path.GetFullPath (System.String.Join (Path.DirectorySeparatorChar.ToString (), str.Select(fun (s:string) -> s.Replace ('/', Path.DirectorySeparatorChar))))


let Grep (file, regex, group:string) =
    let m = Regex.Match (File.ReadAllText (GetPath [file]), regex)
    m.Groups.[group].Value

let FileReplace (file, outFile, toReplace:string, replacement:string) =
    File.WriteAllText (GetPath [outFile], File.ReadAllText(GetPath [file]).Replace(toReplace, replacement))

let Run (file, args) =
    let currentProcess = new Process ()
    currentProcess.StartInfo.FileName <- file
    currentProcess.StartInfo.Arguments <- args
    currentProcess.StartInfo.RedirectStandardOutput <- true
    currentProcess.StartInfo.UseShellExecute <- false
    currentProcess.StartInfo.WindowStyle <- ProcessWindowStyle.Hidden
    currentProcess.Start () |> ignore
    currentProcess.StandardOutput

Console.WriteLine "MonoDevelop F# add-in configuration script"
Console.WriteLine "------------------------------------------"

let mutable mdDir = null
let mutable mdVersion = "4.1.6"

// Look for the installation directory

if (File.Exists (GetPath ["../../../monodevelop.pc.in"])) then
    // Local MonoDevelop build directory
    mdDir <- GetPath [Environment.CurrentDirectory + "/../../../build"]
    if (File.Exists (GetPath [mdDir;  "../../main/configure.in"])) then 
        mdVersion <- Grep (GetPath [mdDir; "../../main/configure.in"], @"AC_INIT.*?(?<ver>([0-9]|\.)+)", "ver")
else
    // Using installed MonoDevelop
    let searchPaths = if IsWindows then WindowsPaths else LinuxPaths
    mdDir <- searchPaths.FirstOrDefault (fun p -> File.Exists (GetPath [p; MdCheckFile]))
    if (mdDir <> null) then
        let mutable mdExe = null
        if (File.Exists (GetPath[mdDir; "../../XamarinStudio"])) then
            mdExe <- GetPath[mdDir; "../../XamarinStudio"]
        elif (File.Exists (GetPath [mdDir; "../../MonoDevelop"])) then
            mdExe <- GetPath [mdDir; "../../MonoDevelop"]
        elif (File.Exists (GetPath[mdDir; "bin/XamarinStudio.exe"])) then
            mdExe <- GetPath[mdDir; "bin/XamarinStudio.exe"]
        elif (File.Exists (GetPath [mdDir; "bin/MonoDevelop.exe"])) then
            mdExe <- GetPath [mdDir; "bin/MonoDevelop.exe"]
        if (mdExe <> null) then
            let outp = Run(mdExe, "/?").ReadLine()
            mdVersion <- outp.Split([| ' ' |], StringSplitOptions.RemoveEmptyEntries).Last()

if not IsWindows then
    // Update the makefile. We don't use that on windows
    FileReplace ("Makefile.orig", "Makefile", "INSERT_MDROOT", mdDir)
    FileReplace ("Makefile", "Makefile", "INSERT_MDVERSION4", mdVersion)
    FileReplace ("Makefile", "Makefile", "INSERT_VERSION", FSharpVersion)
    
if (mdDir <> null) then
    Console.WriteLine ("MonoDevelop binaries found at: {0}", mdDir)
else
    Console.WriteLine ("MonoDevelop binaries not found. Continuing anyway")

Console.WriteLine ("Detected version: {0}", mdVersion)

let tag = if IsWindows then "windows" else "local"

let fsprojFile = "MonoDevelop.FSharpBinding/MonoDevelop.FSharp." + tag + ".fsproj"
let xmlFile = "MonoDevelop.FSharpBinding/FSharpBinding.addin.xml"

FileReplace ("MonoDevelop.FSharpBinding/MonoDevelop.FSharp.fsproj.orig", fsprojFile, "INSERT_FSPROJ_MDROOT", mdDir)
FileReplace (fsprojFile, fsprojFile, "INSERT_FSPROJ_MDVERSION4", mdVersion)
FileReplace (fsprojFile, fsprojFile, "INSERT_FSPROJ_MDTAG", tag)
FileReplace ("MonoDevelop.FSharpBinding/FSharpBinding.addin.xml.orig", xmlFile, "INSERT_FSPROJ_VERSION", FSharpVersion)
FileReplace (xmlFile, xmlFile, "INSERT_FSPROJ_MDVERSION4", mdVersion)
