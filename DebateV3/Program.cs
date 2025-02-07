using System;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        string inputDatFolder = "Nonstop_Dat";
        string outputTxtFolder = "Nonstop_TXT";
        string newTxtFolder = "New_Nonstop_TXT";
        string outputDatFolder = "New_Nonstop_Dat";

        ProcessAllFiles(inputDatFolder, outputTxtFolder, newTxtFolder, outputDatFolder);

        Console.WriteLine("Processing Complete.");
        Console.ReadLine();
    }

    public static void ProcessAllFiles(string inputDatFolder, string outputTxtFolder, string newTxtFolder, string outputDatFolder)
    {
        try
        {
            if (!Directory.Exists(inputDatFolder) || Directory.GetFiles(inputDatFolder, "*.dat").Length == 0)
            {
                Console.WriteLine($"The folder '{inputDatFolder}' does not exist or does not contain files '.dat'.");
                return;
            }

            if (!Directory.Exists(outputTxtFolder))
            {
                Directory.CreateDirectory(outputTxtFolder);
            }

            foreach (var datFile in Directory.GetFiles(inputDatFolder, "*.dat"))
            {
                string txtFileName = Path.GetFileNameWithoutExtension(datFile) + ".txt";
                string txtFilePath = Path.Combine(outputTxtFolder, txtFileName);
                ExtractDebate(datFile, txtFilePath);
            }
            Console.Write("\n");
			
            if (!Directory.Exists(newTxtFolder) || Directory.GetFiles(newTxtFolder, "*.txt").Length == 0)
            {
                Console.WriteLine($"The Folder '{newTxtFolder}' does not exist or does not contain files '.txt'.");
                return;
            }

            if (!Directory.Exists(outputDatFolder))
            {
                Directory.CreateDirectory(outputDatFolder);
            }

            foreach (var txtFile in Directory.GetFiles(newTxtFolder, "*.txt"))
            {
                string datFileName = Path.GetFileNameWithoutExtension(txtFile) + ".dat";
                string datFilePath = Path.Combine(outputDatFolder, datFileName);
                RebuildFile(txtFile, datFilePath);
            }
            Console.Write("\n");
            Console.WriteLine("All files were processed successfully.");
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
                // Cabeçalho
                ushort Time = reader.ReadUInt16();
                uint NumberSeccion = reader.ReadByte();
                uint unk1 = reader.ReadByte();
                ushort unk2 = reader.ReadUInt16();
                ushort unk3 = reader.ReadUInt16();
                ushort unk4 = reader.ReadUInt16();
                ushort unk5 = reader.ReadUInt16();

                writer.WriteLine("Cabeçalho:");
                writer.WriteLine($"Time: <{Time}>");
                writer.WriteLine($"Number_Sections: <{NumberSeccion}>");
                writer.WriteLine($"unk1: <{unk1}>");
                writer.WriteLine($"unk2: <{unk2}>");
                writer.WriteLine($"unk3: <{unk3}>");
                writer.WriteLine($"unk4: <{unk4}>");
                writer.WriteLine($"unk5: <{unk5}>");
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
			
                    writer.WriteLine($"ID_Dialogue <{dialogId}>");
                    writer.WriteLine($"ID_Section <{sectionId}>");
                    writer.WriteLine($"CHK_ID <{difficulty}>");
					
                    for (int j = 0; j < 199; j++)
                    {
                        byte[] unknownData = reader.ReadBytes(2);
                        if (unknownData.Length < 2)
                            break;

                        ushort unknownValue = BitConverter.ToUInt16(unknownData, 0);
                        if (j == 0)
                        {
                            writer.WriteLine($"Minimum_Difficulty <{unknownValue}>");
                        }
                        else if (j == 2)
                        {
                            writer.WriteLine($"Seconds_Recovered_from_NOISE <{unknownValue}>");
                        }
                        else if (j == 3)
                        {
                            writer.WriteLine($"NOISE_durability <{unknownValue}>");
                        }
                        else if (j == 5)
                        {
                            writer.WriteLine($"Truth_Bullet_needed <{unknownValue}>");
                        }
                        else if (j == 12)
                        {
                            writer.WriteLine($"Reverse_delay <{unknownValue}>");
                        }
                        else if (j == 14)
                        {
                            writer.WriteLine($"End_transition_timing_TEXT_RELIANT <{unknownValue}>");
                        }
                        else if (j == 16)
                        {
                            writer.WriteLine($"End_transition_timing_TEXT_INDEPENDENT <{unknownValue}>");
                        }
                        else if (j == 168)
                        {
                            writer.WriteLine($"Character_ID <{unknownValue}>");
                        }
                        else if (j == 169)
                        {
                            writer.WriteLine($"Character_anim <{unknownValue}>");
                        }
                        else
                        {
                            writer.WriteLine($"unk_{j} <{unknownValue}>");
                        }
                    }

                    writer.WriteLine("==== End Section ====");
                    writer.WriteLine();
                }

                // Extrair seções de Efeitos e Voz
                writer.WriteLine("Voice and Effects Sections:");
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    byte[] effectVoiceData = reader.ReadBytes(64);
                    string effectVoice = Encoding.UTF8.GetString(effectVoiceData).TrimEnd('\0');
                    writer.WriteLine($"<{effectVoice}>");
                }
            }

            Console.WriteLine($"The file '{outputFilePath}' was extracted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao processar o arquivo: " + ex.Message);
        }
    }

    public static void RebuildFile(string inputTxtPath, string outputFilePath)
    {
        try
        {
            using (StreamReader reader = new StreamReader(inputTxtPath))
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
