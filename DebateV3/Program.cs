using System;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        string inputDatFolder = "Nonstop_Dat";
        string outputCsvFolder = "Nonstop_CSV";
        string newCsvFolder = "New_Nonstop_CSV";
        string outputDatFolder = "New_Nonstop_Dat";

        ProcessAllFiles(inputDatFolder, outputCsvFolder, newCsvFolder, outputDatFolder);

        Console.WriteLine("Processing Complete.");
        Console.ReadLine();
    }

    public static void ProcessAllFiles(string inputDatFolder, string outputCsvFolder, string newCsvFolder, string outputDatFolder)
    {
        try
        {
            if (!Directory.Exists(inputDatFolder))
            {
                Console.WriteLine($"The folder '{inputDatFolder}' does not exist. Creating folder...");
                Directory.CreateDirectory(inputDatFolder);
                Console.WriteLine($"Folder created.");
            }
            if (Directory.GetFiles(inputDatFolder, "*.dat").Length == 0)
            {
                Console.WriteLine($"The folder '{inputDatFolder}' does not contain any '.dat' files.");
            }

            if (!Directory.Exists(outputCsvFolder))
            {
                Directory.CreateDirectory(outputCsvFolder);
            }

            foreach (var datFile in Directory.GetFiles(inputDatFolder, "*.dat"))
            {
                string csvFileName = Path.GetFileNameWithoutExtension(datFile) + ".csv";
                string csvFilePath = Path.Combine(outputCsvFolder, csvFileName);
                ExtractDebate(datFile, csvFilePath);
            }
            Console.Write("\n");
			
            if (!Directory.Exists(newCsvFolder))
            {
                Console.WriteLine($"The folder '{newCsvFolder}' does not exist. Creating folder...");
                Directory.CreateDirectory(newCsvFolder);
                Console.WriteLine($"Folder created.");
            }

            if (Directory.GetFiles(newCsvFolder, "*.csv").Length == 0)
            {
                Console.WriteLine($"The folder '{newCsvFolder}' does not contain any '.csv' files.");
            }

            if (!Directory.Exists(outputDatFolder))
            {
                Directory.CreateDirectory(outputDatFolder);
            }

            foreach (var csvFile in Directory.GetFiles(newCsvFolder, "*.csv"))
            {
                string datFileName = Path.GetFileNameWithoutExtension(csvFile) + ".dat";
                string datFilePath = Path.Combine(outputDatFolder, datFileName);
                RebuildFile(csvFile, datFilePath);
            }
            Console.Write("\n");
            Console.WriteLine("All available files were processed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao processar os arquivos: " + ex.Message);
        }
    }

    public static void ExtractDebate(string inputFilePath, string outputFilePath)
    {
        try
        {
            using (FileStream fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                // Cabeçalho (header)
                ushort Time = reader.ReadUInt16();
                uint NumberSeccion = reader.ReadByte();
                uint unk1 = reader.ReadByte();
                ushort unk2 = reader.ReadUInt16();
                ushort unk3 = reader.ReadUInt16();
                ushort unk4 = reader.ReadUInt16();
                ushort unk5 = reader.ReadUInt16();

                writer.WriteLine("\"Opening_information\"");
                writer.WriteLine($"\"Time\",\"{Time}\"");
                writer.WriteLine($"\"Section_Number\",\"{NumberSeccion}\"");
                writer.WriteLine($"\"unk1\",\"{unk1}\"");
                writer.WriteLine($"\"unk2\",\"{unk2}\"");
                writer.WriteLine($"\"unk3\",\"{unk3}\"");
                writer.WriteLine($"\"unk4\",\"{unk4}\"");
                writer.WriteLine($"\"unk5\",\"{unk5}\"");

                writer.Write("\"\",");
                writer.Write("\"ID_Dialogue\",");
                writer.Write("\"ID_Section\",");
                writer.Write("\"CHK_ID\",");
                for (int j = 0; j < 199; j++)
                {
                    if (j == 0)
                    {
                        writer.Write($"\"Minimum_Difficulty\",");
                    }
                    else if (j == 2)
                    {
                        writer.Write($"\"Seconds_Recovered_from_NOISE\",");
                    }
                    else if (j == 3)
                    {
                        writer.Write($"\"NOISE_durability\",");
                    }
                    else if (j == 5)
                    {
                        writer.Write($"\"Truth_Bullet_needed\",");
                    }
                    else if (j == 12)
                    {
                        writer.Write($"\"Reverse_delay\",");
                    }
                    else if (j == 14)
                    {
                        writer.Write($"\"End_transition_timing_TEXT_RELIANT\",");
                    }
                    else if (j == 16)
                    {
                        writer.Write($"\"End_transition_timing_TEXT_INDEPENDENT\",");
                    }
                    else if (j == 168)
                    {
                        writer.Write($"\"Character_ID\",");
                    }
                    else if (j == 169)
                    {
                        writer.Write($"\"Character_anim\",");
                    }
                    else if (j == 198)
                    {
                        writer.Write($"\"unk_{j}\"");
                    }
                    else
                    {
                        writer.Write($"\"unk_{j}\",");
                    }
                }
                writer.WriteLine();
                // loop sections
                for (int i = 0; i < NumberSeccion; i++)
                {
                    byte[] idData = reader.ReadBytes(2);
                    ushort dialogId = BitConverter.ToUInt16(idData, 0);

                    byte[] sectionData = reader.ReadBytes(2);
                    ushort sectionId = BitConverter.ToUInt16(sectionData, 0);
					
					byte[] difficultyData = reader.ReadBytes(2);
                    ushort difficulty = BitConverter.ToUInt16(difficultyData, 0);

                    writer.Write($"\"Section_{i}\",");
                    writer.Write($"\"{dialogId}\",");
                    writer.Write($"\"{sectionId}\",");
                    writer.Write($"\"{difficulty}\",");
					
                    for (int j = 0; j < 199; j++)
                    {
                        byte[] unknownData = reader.ReadBytes(2);
                        if (unknownData.Length < 2)
                            break;

                        ushort unknownValue = BitConverter.ToUInt16(unknownData, 0);
                        if (j == 198)
                        {
                            writer.Write($"\"{unknownValue}\"");
                        }
                        else
                        {
                            writer.Write($"\"{unknownValue}\",");
                        }
                    }

                    writer.WriteLine();
                }

                // Extrair seções de Efeitos e Voz
                writer.WriteLine("\"Closing_information\"");
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    byte[] effectVoiceData = reader.ReadBytes(64);
                    string effectVoice = Encoding.UTF8.GetString(effectVoiceData).TrimEnd('\0');
                    writer.WriteLine($"\"{effectVoice}\"");
                }
            }

            Console.WriteLine($"The file '{outputFilePath}' was extracted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao processar o arquivo: " + ex.Message);
        }
    }

    public static void RebuildFile(string inputCsvPath, string outputFilePath)
    {
        try
        {
            using (StreamReader reader = new StreamReader(inputCsvPath))
            using (FileStream fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // Ler e escrever o cabeçalho
                reader.ReadLine(); // Pular "Cabeçalho:"
                ushort Time = ushort.Parse(reader.ReadLine().Split('<', '>')[1].Trim());
                writer.Write(Time);
                uint NumberSeccion = uint.Parse(reader.ReadLine().Split('<', '>')[1].Trim());
                writer.Write((byte)NumberSeccion);
                uint unk1 = uint.Parse(reader.ReadLine().Split('<', '>')[1].Trim());
                writer.Write((byte)unk1);
                ushort unk2 = ushort.Parse(reader.ReadLine().Split('<', '>')[1].Trim());
                writer.Write(unk2);
                ushort unk3 = ushort.Parse(reader.ReadLine().Split('<', '>')[1].Trim());
                writer.Write(unk3);
                ushort unk4 = ushort.Parse(reader.ReadLine().Split('<', '>')[1].Trim());
                writer.Write(unk4);
                ushort unk5 = ushort.Parse(reader.ReadLine().Split('<', '>')[1].Trim());
                writer.Write(unk5);

                // Pular linha em branco
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    string dialogLine = reader.ReadLine();
                    if (dialogLine.StartsWith("ID_Dialogue"))
                    {
                        ushort dialogId = ushort.Parse(dialogLine.Split('<', '>')[1]);
                        writer.Write(dialogId);

                        string sectionLine = reader.ReadLine();
                        ushort sectionId = ushort.Parse(sectionLine.Split('<', '>')[1]);
                        writer.Write(sectionId);

                        string difficultyLine = reader.ReadLine();
                        ushort difficulty = ushort.Parse(difficultyLine.Split('<', '>')[1]);
                        writer.Write(difficulty);

                        for (int i = 0; i < 199; i++)
                        {
                            string unknownLine = reader.ReadLine();
                            if (unknownLine.StartsWith("==== End Section ===="))
                                break;

                            ushort unknownValue = ushort.Parse(unknownLine.Split('<', '>')[1]);
                            writer.Write(unknownValue);
                        }

                        // Pular linha em branco
                        reader.ReadLine();
                    }
                    else if (dialogLine.StartsWith("<") && dialogLine.EndsWith(">"))
                    {
                        // Reconstruir seções de Efeitos e Voz
                        string effectVoice = dialogLine.Trim('<', '>');
                        byte[] effectVoiceData = Encoding.UTF8.GetBytes(effectVoice);
                        Array.Resize(ref effectVoiceData, 64); // Ajustar tamanho para 64 bytes, preenchendo com 0x00 se necessário
                        writer.Write(effectVoiceData);
                    }
                }
            }

            Console.WriteLine($"The file '{outputFilePath}' was successfully rebuilt!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao reconstruir o arquivo: " + ex.Message);
        }
    }
}
