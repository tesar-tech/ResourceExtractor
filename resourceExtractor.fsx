//https://github.com/tesar-tech/ResourceExtractor
open System.Text.RegularExpressions
open System.IO

let folderToDeepSearchForFiles = __SOURCE_DIRECTORY__  + @"\ResourceExtractorSampleApp\ResourceExtractorSampleApp"
let finalResourceFilePath =  folderToDeepSearchForFiles + @"\Strings\en-US\Resources.resw"


let ignoredFolders = seq["bin" ;"obj";"Properties";"Backup";"MultilingualResources";"Strings";"AppPackages";"Assets";"BundleArtifacts";"Resources"]//there are also xaml files 
let xmlPrefix = 
 """<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
"""
let xmlPostfix = """</root>"""
let getDataElementsFromXaml filePath =  
    Regex.Matches(File.ReadAllText(filePath), """x:Uid="(.+?)"(\s+(.+?)="(.+?)")+""")
  |> Seq.cast<Match> 
  |> Seq.collect (fun x -> match System.Int32.TryParse( (string( x.Groups.[1]))|> Seq.last|>string) with
                            | (true,strCount) ->seq[ for capture in 0..strCount-1  -> (x.Groups.[1].ToString()+"."+string( (x.Groups.[3].Captures.[capture])),string(  (x.Groups.[4].Captures.[capture])) )]
                            |_ -> seq[ (string( x.Groups.Item 1)+"."+string( x.Groups.[3].Captures.[0]),string( x.Groups.[4].Captures.[0]) )])
  |> Seq.map (fun (name ,value) ->"\t<data name=\""+name+ "\" xml:space=\"preserve\">\n\t\t<value>" + value+ "</value>\n\t</data>\n") |> Seq.fold (+) ""

let getDataElementsFromCs filePath =  
    Regex.Matches(File.ReadAllText(filePath), """ResourceLoader.GetForCurrentView\(\).GetString\("(.*)"\);\/\/(.*)""")
 |> Seq.cast<Match> 
 |> Seq.map (fun x->"\t<data name=\""+(x.Groups.[1]).Value + "\" xml:space=\"preserve\">\n\t\t<value>" + (x.Groups.[2]).Value+ "</value>\n\t</data>\n") |> Seq.fold (+) ""

let ignoredFoldersFullPaths = ignoredFolders|> Seq.map (fun x ->folderToDeepSearchForFiles+"\\"+x )

let dirsWithoutIgnored = 
  Directory.GetDirectories folderToDeepSearchForFiles 
  |> Seq.filter (fun x -> Seq.contains x ignoredFoldersFullPaths|>not   )
  |> Seq.map  Path.GetFullPath 
  //|> Seq.append (Seq.singleton folderToDeepSearchForFiles) //dont want to deepSearch here again (SearchOption.AllDirectories)

let getDataElementsFromCs2 filePath =  //first match is from getstring, second is from Accelerators
  let fromGetString = Seq.cast<Match>( Regex.Matches(File.ReadAllText(filePath), """ResourceLoader.GetForCurrentView\(\).GetString\("(.*)"\);\/\/(.*)"""))
  let m = Regex.Match(File.ReadAllText(filePath),"""\/\/ResourceExtractorAcceleratorsStart\s+([\s\S]*)\s+\/\/ResourceExtractorAcceleratorsEnd""")
  match m.Success with
    |false -> Seq.empty
    |true -> Regex.Matches(m.Value  , """\s*\(\s*"(.+?)"\s*,\s*"(.+?)".*""" ) 
          |> Seq.cast<Match> 
  |> Seq.append fromGetString 
  |> Seq.map (fun x->"\t<data name=\""+(x.Groups.[1]).Value + "\" xml:space=\"preserve\">\n\t\t<value>" + (x.Groups.[2]).Value+ "</value>\n\t</data>\n") |> Seq.fold (+) ""


let DataElementsFromXaml =
 dirsWithoutIgnored |> Seq.map (fun c -> 
 Directory.GetFiles(c, "*.xaml",SearchOption.AllDirectories) )//seq<string[]>
 |> Seq.collect (id) //seq<string>
 |> Seq.append  (Directory.GetFiles(folderToDeepSearchForFiles, "*.xaml",SearchOption.TopDirectoryOnly))//include root folder, but files only
 |> Seq.map (Path.GetFullPath >>getDataElementsFromXaml )
 |> String.concat "\n"    


let DataElementsFromCs =
 dirsWithoutIgnored |> Seq.map (fun c -> 
 Directory.GetFiles(c, "*.cs",SearchOption.AllDirectories) )//seq<string[]>
 |> Seq.collect (id) //seq<string>
 |> Seq.append  (Directory.GetFiles(folderToDeepSearchForFiles, "*.cs",SearchOption.TopDirectoryOnly))//include root folder, but files only
 |> Seq.map (Path.GetFullPath >>getDataElementsFromCs2 )
 |> String.concat "\n"    

let allDataElements =  (DataElementsFromCs + DataElementsFromXaml).Replace("\n\n","")//remove unnecessary empty rows

// This will rewrite existing Resources.resw file
File.WriteAllText(finalResourceFilePath,xmlPrefix+allDataElements+xmlPostfix);

//https://github.com/tesar-tech/ResourceExtractor