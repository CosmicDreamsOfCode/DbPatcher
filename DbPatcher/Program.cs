using FrostySdk.IO;
using System.IO;
using System.Reflection;

namespace DbPatcher
{
    public class Program
    {
        static void Main(string[] args)
        {
            #region Arguments
            if (args.Length == 0)
            {
                Console.WriteLine("ShaderDb Bytecode Patcher - By Cosmic and Natalie");
                Console.WriteLine("Usage: DbPatcher [Path to original bytecode] [Path to replacement bytecode] [Path to ShaderDb resource]");
                Console.WriteLine("Press <Enter> to exit... ");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                return;
            }
            string inputpath = args[0];
            if (!File.Exists(inputpath))
            {
                Console.WriteLine("Original bytecode file does not exist at specified location.");
                Console.WriteLine("Press <Enter> to exit... ");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                return;
            }
            if (args.Length <= 1)
            {
                Console.WriteLine("Missing argument.");
                Console.WriteLine("Press <Enter> to exit... ");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                return;
            }
            string replacementBinary = args[1];
            if (!File.Exists(replacementBinary))
            {
                Console.WriteLine("Replacement bytecode file does not exist at specified location.");
                Console.WriteLine("Press <Enter> to exit... ");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                return;
            }
            if (args.Length <= 2)
            {
                Console.WriteLine("Missing argument.");
                Console.WriteLine("Press <Enter> to exit... ");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                return;
            }
            string resPath = args[2];
            if (!File.Exists(resPath))
            {
                Console.WriteLine("ShaderDb file does not exist at specified location.");
                Console.WriteLine("Press <Enter> to exit... ");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                return;
            } 
            #endregion

            int index = args[2].LastIndexOf("\\");
            string resDir = resPath.Substring(0, index);

            byte[] permutationBuffer = new byte[20];
            int entryPos;
            byte[] afterEntry;
            byte[] beforeEntry;
            byte[] modifiedEntry;

            using (NativeReader reader = new(new FileStream(inputpath, FileMode.Open, FileAccess.Read))) //read input dxbc and grab its magic and checksum to search for it in the db
            {
                reader.Read(permutationBuffer, 0, permutationBuffer.Length);
            }

            using (NativeReader reader = new(new FileStream(resPath, FileMode.Open, FileAccess.Read))) //read the shaderdb
            {
                byte[] dbBuff = reader.ReadToEnd();
                long permutationPos = ScanBytePattern(permutationBuffer, dbBuff); //scan for input
                //Console.WriteLine("Permutation header found at " + permutationPos.ToString("X2")); 

                reader.Position = permutationPos -4;
                entryPos = (int)reader.Position; //save our position so we can use it later
                uint permutationSize = reader.ReadUInt();
                reader.Position = permutationPos + permutationSize;
                afterEntry = reader.ReadToEnd(); //we need to save everything after the entry so we can write it after our replacement bytecode
                reader.Position = 0;
                beforeEntry = reader.ReadBytes(entryPos);
            }

            using (NativeReader reader = new(new FileStream(replacementBinary, FileMode.Open, FileAccess.Read))) //read the replacement bytecode
            {
                modifiedEntry = reader.ReadToEnd();
            }

            using (FileStream fileStream = File.Create(resDir + "PatchedShaderDb.res")) //write the new shaderdb
            {
                // Get the size of the modifed permutation entry to write it later.
                byte[] modifiedEntrySize = BitConverter.GetBytes(modifiedEntry.Length);

                // Write a new shaderdb.res file with all of the necessary data.
                fileStream.Write(beforeEntry);
                fileStream.Write(modifiedEntrySize);
                fileStream.Write(modifiedEntry);
                fileStream.Write(afterEntry);

                // Calculate the size of the new file length and write it to the shaderdb.
                uint size = (uint)fileStream.Length - 0x1C;
                byte[] newDbSize = BitConverter.GetBytes(size);
                fileStream.Position = 0x18;
                fileStream.Write(newDbSize);
            }
        }

        // Parameters are the byte sequence you want to search for and the file byte array you want to search in.
        static long ScanBytePattern(byte[] sequence, byte[] buffer)
        {
            // Start iterating through the file's bytes
            for (int i = 0; i < buffer.Length; i++)
            {
                bool sequenceFound = true;

                // Start iterating through the byte sequence
                for (int j = 0; j < sequence.Length; j++)
                {
                    // If the buffer at position i + the sequence index is not equal to the sequence at position j, skip to the next buffer iteration
                    if (buffer[i + j] != sequence[j])
                    {
                        sequenceFound = false;
                        break;
                    }
                }

                // Return position i if sequence found is still true
                if (sequenceFound)
                    return i;
            }

            // If the sequence is not found, return 0 (perhaps do error handling if it returns 0)
            return 0;
        }
    }
}