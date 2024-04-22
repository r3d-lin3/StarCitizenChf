﻿namespace StarCitizenChf;

public record AnalysisResult(List<(int,int)> CommonSequences, int[] CommonBytes, byte[] ValuesAtCommonBytes);

public static class Analysis
{
    public static AnalysisResult AnalyzeSimilarities(string inputFolder)
    {
        var decompressedFiles = Directory.GetFiles(inputFolder, "*.bin", SearchOption.AllDirectories)
            .Select(x => (Path.GetFileName(x), File.ReadAllBytes(x))).ToArray();

        var smallest = decompressedFiles.MinBy(x => x.Item2.Length).Item2.Length;
        //analyze byte by byte if it is the same in all files
        var commonBytes = new List<int>();
        for (var i = 0; i < smallest; i++)
        {
            if (decompressedFiles.All(b => b.Item2[i] == decompressedFiles[0].Item2[i]))
                commonBytes.Add(i);
        }

        var valuesAtCommonBytes = commonBytes.Select(i => decompressedFiles[0].Item2[i]).ToArray();

        //compute sequences of bytes in a row. This is useful for finding patterns in the data
        //input: 0, 1, 2, 3, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15
        //output: 0-3, 5-10, 12-15
        var sequences = new List<(int, int)>();
        for (var i = 0; i < commonBytes.Count; i++)
        {
            var start = commonBytes[i];
            var end = start;
            while (i + 1 < commonBytes.Count && commonBytes[i + 1] == end + 1)
            {
                end++;
                i++;
            }

            sequences.Add((start, end));
        }

        return new AnalysisResult(sequences, commonBytes.ToArray(), valuesAtCommonBytes);
    }

    public static bool IsMale(ReadOnlySpan<byte> decompressedData)
    {
        ReadOnlySpan<byte> woman = [0xAD, 0x4C, 0xB0, 0xEF, 0x94, 0x4A, 0x79, 0xD0, 0x53, 0xC2, 0xD3, 0xB4, 0x58, 0x25, 0x38, 0xAD];
        ReadOnlySpan<byte> man = [0x61, 0x4A, 0x6B, 0x14, 0xD5, 0x39, 0xF4, 0x25, 0x49, 0x8A, 0xB6, 0xDF, 0x86, 0xA4, 0x99, 0xA9];
        
        var search = decompressedData.Slice(8, 16);
        
        if (search.SequenceEqual(woman))
            return false;
        if (search.SequenceEqual(man))
            return true;
        
        throw new Exception();
    }
}