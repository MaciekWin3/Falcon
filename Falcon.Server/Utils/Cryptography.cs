namespace Falcon.Server.Utils
{
    public static class Cryptography
    {
        public static string XorEncrypt(string text, int encryptionKey)
        {
            char[] encryptedChars = new char[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                int encryptedCharCode = character + encryptionKey;
                char encryptedChar = (char)encryptedCharCode;
                encryptedChars[i] = encryptedChar;
            }
            return string.Concat(encryptedChars);
        }

        public static string XorDecrypt(string text, int encryptionKey)
        {
            return XorEncrypt(text, -encryptionKey);
        }
    }
}