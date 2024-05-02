using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChfParser;
using ChfUtils;
using StarCitizenChf;

var csprojFolder = Path.GetFullPath(@"..\..\..\");
var folders = new Folders(csprojFolder);

GuidUtils.Test();

//Downloads all characters from the website and saves them to the website characters folder.
//await Download.DownloadAllMetadata(folders.MetadataFile);
//await Download.DownloadAllCharacters(folders.MetadataFile, folders.WebsiteCharacters);

//Imports all non-modded characters exported from the game into our local characters folder.
await Utils.ImportGameCharacters(folders.LocalCharacters);
await Utils.ConvertAllBinariesToChfAsync(folders.ModdedCharacters);

//Extracts all chf files into bins, reverses these bins for easier analysis.
//also tries to extract some color information to compare to images.
await Processing.ProcessAllCharacters(folders.WebsiteCharacters);
await Processing.ProcessAllCharacters(folders.LocalCharacters);

var web = Utils.LoadFilesWithNames(folders.WebsiteCharacters, "*.bin");
var local = Utils.LoadFilesWithNames(folders.LocalCharacters, "*.bin");
var allBins = web.Concat(local).ToArray();
var characters = allBins.Select(x =>  StarCitizenCharacter.FromBytes(x.name, x.data)).ToArray();

var reversed = new List<string>();
foreach (var (data, name) in allBins)
{
    reversed.Add(BitConverter.ToString(data.Reverse().ToArray()));
}

File.WriteAllLines(Path.Combine(folders.Base, "reversed1.txt"), reversed);

List<string> remaining = new();
for (var index = 0; index < characters.Length; index++)
{
    var character = characters[index];
    var last = character.LastReadIndex;
    var data = allBins[index].data;
    
    remaining.Add(BitConverter.ToString(data[last..]));
}

//File.WriteAllLines(Path.Combine(folders.Base, "remaining.txt"), remaining);

return;

// prints all data buffers to a file, one per line, in 4 byte chunks for easy visual comparison.
await Analysis.PrintAllToFileAsync(Directory.GetFiles(folders.Base, "*.bin", SearchOption.AllDirectories), Path.Combine(folders.Base, "bins.txt"));
