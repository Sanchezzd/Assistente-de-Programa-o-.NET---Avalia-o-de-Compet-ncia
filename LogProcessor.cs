using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class LogProcessor
{
    static void Main(string[] args)
    {
        try
        {
            string filePath = @"caminho_para_o_arquivo.log";
            Dictionary<string, List<string>> realmHashes = ParseLogFile(filePath);

            WriteResultToFile("resultado.txt", realmHashes);

            Console.WriteLine("Processamento concluído com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocorreu um erro durante o processamento do arquivo de log: {ex.Message}");
        }
    }

    static Dictionary<string, List<string>> ParseLogFile(string filePath)
    {
        var realmHashes = new Dictionary<string, List<string>>();
        string currentBlock = "";
        bool errorFound = false;
        string currentRealm = "";
        string currentHash = "";

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("Iniciando interpretação da mensagem"))
                {
                    currentBlock = "";
                    errorFound = false;
                }

                currentBlock += line + Environment.NewLine;

                if (line.Contains("System.ArgumentException: Parameter 'P_ID_TIPO_RESIDENCIA' not found in the collection."))
                {
                    errorFound = true;
                    currentHash = ExtractValueFromLine(currentBlock, @"item.MensagemHash:\s+(\w+)");
                    currentRealm = ExtractValueFromLine(currentBlock, @"item.RealmInicial:\s+(\w+)");
                }

                if (line.Contains("Trabalho com pedidos foi finalizado."))
                {
                    if (errorFound)
                    {
                        if (!realmHashes.ContainsKey(currentRealm))
                        {
                            realmHashes[currentRealm] = new List<string>();
                        }
                        realmHashes[currentRealm].Add(currentHash);
                    }
                }
            }
        }

        return realmHashes;
    }

    static string ExtractValueFromLine(string block, string pattern)
    {
        var match = Regex.Match(block, pattern);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return null;
    }

    static void WriteResultToFile(string filePath, Dictionary<string, List<string>> realmHashes)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var pair in realmHashes)
            {
                writer.WriteLine(pair.Key);
                foreach (var hash in pair.Value)
                {
                    writer.WriteLine(hash);
                }
                writer.WriteLine();
            }
        }
    }
}

