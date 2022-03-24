using System.Text;

namespace Falcon.Server.Utils
{
    public static class Cryptography
    {
        // XOR encryption
        public static string EncryptDecrypt(string text, int encryptionKey)
        {
            StringBuilder inputString = new(text);
            StringBuilder outputString = new(text.Length);
            char character;
            for (int i = 0; i < text.Length; i++)
            {
                character = inputString[i];
                character = (char)(character ^ encryptionKey);
                outputString.Append(character);
            }
            return outputString.ToString();
        }
    }
}