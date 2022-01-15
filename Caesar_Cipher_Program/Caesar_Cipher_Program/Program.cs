using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Caesar_Cipher_Program
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            string up = "---------------------------";
            string caesarCipher = "|      CAESAR CIPHER      |";
            string down = "---------------------------";
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (up.Length / 2)) + "}", up));
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (caesarCipher.Length / 2)) + "}", caesarCipher));
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (down.Length / 2)) + "}", down));
            Console.WriteLine();

            Console.ResetColor();
            Console.WriteLine("Encrypt or decrypt? (1 - Encrypt, 2 - Decrypt)");
            Console.ForegroundColor = ConsoleColor.Blue;
            int option = int.Parse(Console.ReadLine());
            Console.ResetColor();
            Console.WriteLine();

            if (option == 1)
            {
                Console.WriteLine("Enter text to encode: ");
                Console.ForegroundColor = ConsoleColor.Blue;
                string textToEncode = Console.ReadLine();
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Enter shift to encode with: (1-26)");
                Console.ForegroundColor = ConsoleColor.Blue;
                int shift = int.Parse(Console.ReadLine());
                Console.ResetColor();
                Console.WriteLine();

                Console.WriteLine(Encode(textToEncode, shift));
            }
            else if(option == 2)
            {
                Console.WriteLine("Enter text to decode: ");
                Console.ForegroundColor = ConsoleColor.Blue;
                string textToDecode = Console.ReadLine();
                Console.ResetColor();
                Console.WriteLine();

                Decode(textToDecode);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Choose correct shift: ");
                Console.ForegroundColor = ConsoleColor.Blue;

                try
                {
                    int correctShift = int.Parse(Console.ReadLine());
                    Console.ResetColor();

                    string[] correctWords = Encode(textToDecode, 26 - correctShift).Split();
                    AddCorrectWords(correctWords);
                }
                catch (Exception)
                {
                    Console.ResetColor();
                    Environment.Exit(0);
                }
            }
        }

        //Encoding a text by a given shift.
        public static string Encode(string text, int shift)
        {
            char[] characters = text.ToCharArray();

            for (int i = 0; i < characters.Length; i++)
            {
                bool isLower = true;

                if (characters[i] >= 'A' & characters[i] <= 'Z')
                {
                    isLower = false;
                }
                else if(characters[i] >= 'a' & characters[i] <= 'z')
                {
                    isLower = true;
                }

                int charPositionAtASCIITableAfterShift = characters[i] + shift;

                if (isLower && charPositionAtASCIITableAfterShift > 122)
                {
                    int newPositionAtASCIITable = (charPositionAtASCIITableAfterShift - 122);
                    charPositionAtASCIITableAfterShift = 96 + newPositionAtASCIITable;
                }
                else if (!isLower && charPositionAtASCIITableAfterShift > 90)
                {
                    int newPositionAtASCIITable = (charPositionAtASCIITableAfterShift - 90);
                    charPositionAtASCIITableAfterShift = 64 + newPositionAtASCIITable;
                }
                if (!Char.IsLetter(characters[i]))
                {
                    charPositionAtASCIITableAfterShift = (char)characters[i];
                }

                characters[i] = (char)charPositionAtASCIITableAfterShift;
            }

            return string.Join(null, characters);
        }

        //Decoding a given text. Decoding works in the same way as encoding, but all possible shifts are tested. 
        //I implented an AI, collecting data from the decoded text. It compares all the decoded words to a file, 
        //containing real words. The answer, that has the highest real words count is the suggested answer.

        public static void Decode(string text)
        {
            int shift = 0;
            string[] currentResult = null;
            Dictionary<int, KeyValuePair<string[], int>> options = new Dictionary<int, KeyValuePair<string[], int>>();

            for (int i = 0; i < 26; i++)
            {
                shift = i + 1;
                currentResult = new[] { Encode(text, 26 - shift) };
                int currentResultCorrectWordsCount = CheckForCorrectWords(currentResult);
                KeyValuePair<string[], int> currentKvp = new KeyValuePair<string[], int>(currentResult, currentResultCorrectWordsCount);

                options.Add(shift, currentKvp);

                Console.WriteLine($"Shift - {shift}: " + String.Join(' ', currentResult));
            }

            try
            {
                KeyValuePair<string[], int> keyValuePairWithBiggestMatch = new KeyValuePair<string[], int>();

                foreach (KeyValuePair<string[], int> keyValuePair in options.Values)
                {
                    if (keyValuePair.Value > keyValuePairWithBiggestMatch.Value)
                        keyValuePairWithBiggestMatch = keyValuePair;
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"Suggested answer: ");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(String.Join(' ', keyValuePairWithBiggestMatch.Key));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" (shift = { options.FirstOrDefault(x => x.Value.Equals(keyValuePairWithBiggestMatch)).Key }) ");
                Console.ResetColor();
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Not enough data to suggest an answer!");
                Console.ResetColor();
            }
        }

        //Adding all the correct words (words from the selected shift after decoding) to a .txt file.
        public static void AddCorrectWords(string[] correctWords)
        {
            string knownCorrectWords = "";

            for (int i = 0; i < correctWords.Length; i++)
            {
                correctWords[i] = RemoveSpecialCharacters(correctWords[i]);
            }

            using (StreamReader reader = new StreamReader("words.txt"))
            {
                knownCorrectWords = reader.ReadToEnd();

                reader.Close();
            }

            using (StreamWriter writer = File.AppendText("words.txt"))
            {
                foreach (string word in correctWords)
                {
                    if (!knownCorrectWords.Contains(word))
                    {
                        writer.WriteLine(word);
                    }
                }

                writer.Close();
            }
        }

        //Checking for matching correct words in the .txt file.
        public static int CheckForCorrectWords(string[] words)
        {
            string strW = string.Join(' ', words);
            strW = RemoveSpecialCharacters(strW);
            
            string[] currentResult = strW.Split();

            int currentCount = 0;

            for (int i = 0; i < words.Length; i++)
            {
                using (StreamReader reader = new StreamReader("words.txt"))
                {
                    string knownCorrectWords = reader.ReadToEnd();

                    foreach (string str in currentResult)
                    {
                        if (knownCorrectWords.Contains(str.ToLower()))
                        {
                            currentCount++;
                        }
                    }

                    reader.Close();
                }
            }

            return currentCount;
        }

        //A method for removing all the special characters from a string, before adding it to the .txt file.
        public static string RemoveSpecialCharacters(string word)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char character in word)
            {
                if ((character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z') || character == ' ')
                {
                    stringBuilder.Append(character);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
