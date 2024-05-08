﻿using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChfParser;

namespace StarCitizenChf;

public static class Processing
{
    private static readonly JsonSerializerOptions opts = new() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };

    public static async Task ProcessCharacter(string chf)
    {
        if (!chf.EndsWith(".chf"))
            throw new ArgumentException("Not a chf file", nameof(chf));

        var bin = Path.ChangeExtension(chf, ".bin");
        var chfFile = ChfFile.FromChf(chf);
        if (!File.Exists(bin))
            await chfFile.WriteToBinFileAsync(bin);
        
        var json = Path.ChangeExtension(chf, ".json");
        //if (!File.Exists(json))
        {
            var data = await File.ReadAllBytesAsync(bin);
            var character = StarCitizenCharacter.FromBytes(data);
            var jsonString = JsonSerializer.Serialize(character, opts);
            await File.WriteAllTextAsync(json, jsonString);
        }
        
        var dna = Path.ChangeExtension(chf, ".dna");
        //if (!File.Exists(dna))
        {
            const uint dnaStart = 0x30;
            var dnaBytes = chfFile.Data.AsSpan().Slice((int)dnaStart, 216).ToArray();
            await File.WriteAllBytesAsync(dna, dnaBytes);
        }
    }

    public static string FixWeirdDnaString(string dna)
    {
        if (dna.Length != 384)
            throw new ArgumentException("Invalid length", nameof(dna));
        
        var stringBuilder = new StringBuilder();

        //reverse endianness
        for (var i = 0; i < 48; i++)
        {
            var start = i * 8;
            var part = dna.Substring(start, 8);
            stringBuilder.Append(part[6..8]);
            stringBuilder.Append(part[4..6]);
            stringBuilder.Append(part[2..4]);
            stringBuilder.Append(part[0..2]);
        }
        
        return stringBuilder.ToString();
    }
    
    public static string FixWeirdDnaString2(string dna)
    {
        if (dna.Length != 384)
            throw new ArgumentException("Invalid length", nameof(dna));

        var bytes = Convert.FromHexString(dna);
        var bytesReversed = bytes.Reverse().ToArray();
        return Convert.ToHexString(bytesReversed);
    }
}