using System.Security.Cryptography;
using System.Text;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;


public class AesCipher
{
    private readonly byte[] Key;
    private readonly byte[] IV;

    public AesCipher(string key)
    {
        // Генерация ключа и вектора инициализации на основе введенного пароля
        using (var sha256 = SHA256.Create())
        {
            Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        }

        // Используем фиксированный вектор инициализации (можно сделать его случайным)
        IV = new byte[16]; // Для AES размер IV должен быть 16 байт
        Array.Copy(Key, IV, 16); // Для простоты берем первые 16 байт ключа как IV
    }

    public string Encrypt(string plainText)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }

    public string Decrypt(string cipherText)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}