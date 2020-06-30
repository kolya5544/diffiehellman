using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {
<<<<<<< HEAD
        public static byte[] PublicKey = new byte[1024 * 8];
=======
        public static byte[] PublicKey = new byte[1024 * 1];
>>>>>>> 5c21123cb55592eb91d63083d0f573f17a0b2238

        static void Main(string[] args)
        {
            GenerateRandomBytes(1234567, ref PublicKey);
            Console.WriteLine("---> Diffie-Hellman Server-Chat Client <---");
            Console.Write("Enter IP to connect: ");
            string ip = Console.ReadLine();
            Console.WriteLine("Starting the connection...");
            byte[] Key = null;
            NetworkStream ns = null;
            try
            {
                var Algo = HashAlgorithm.Create("SHA256");
                TcpClient c = new TcpClient(ip, 7887);
                ns = c.GetStream();

                ns.ReadTimeout = 5000;
                ns.WriteTimeout = 10000;

                //Generate our own private key.
<<<<<<< HEAD
                byte[] PrivateKey = new byte[1024 * 8];
                GenerateRandomBytes(ref PrivateKey);

                //Creating own mixture.
                byte[] MixSent = new byte[1024 * 8];
=======
                byte[] PrivateKey = new byte[1024 * 1];
                GenerateRandomBytes(ref PrivateKey);

                //Creating own mixture.
                byte[] MixSent = new byte[1024 * 1];
>>>>>>> 5c21123cb55592eb91d63083d0f573f17a0b2238
                for (int i = 0; i < MixSent.Length; i++)
                {
                    MixSent[i] = (byte)(PublicKey[i] ^ PrivateKey[i]);
                }
                //Sending own mixture
                ns.Write(MixSent, 0, MixSent.Length);

                //Reading MIX of Public and Private.
<<<<<<< HEAD
                byte[] MixReceived = new byte[1024 * 8];
                MixReceived = Receive(ns);

                //Combining to create sign.
                byte[] Sign = new byte[1024 * 8];
=======
                byte[] MixReceived = new byte[1024 * 1];
                MixReceived = Receive(ns);

                //Combining to create sign.
                byte[] Sign = new byte[1024 * 1];
>>>>>>> 5c21123cb55592eb91d63083d0f573f17a0b2238
                for (int i = 0; i < Sign.Length; i++)
                {
                    Sign[i] = (byte)(MixReceived[i] ^ PrivateKey[i]);
                }
                
                //Getting final key.
                Key = Algo.ComputeHash(Sign);
                Console.WriteLine("Successful Diffie-Hellman.");
            } catch
            {
                Console.WriteLine("Failed Diffie-Hellman connection."); return;
            }
            finally
            {
                Console.WriteLine("Press ENTER to enter chat mode.");
                Console.ReadLine();
                Console.Clear();
                try
                {
                    StreamReader sr = new StreamReader(ns);
                    StreamWriter sw = new StreamWriter(ns);
                    sw.AutoFlush = true;
                    while (true)
                    {
                        Console.Write(">");
                        string text = Console.ReadLine();
                        string ToSend = Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(text), Key));
                        sw.Write(ToSend + "\r\n");
                        Console.WriteLine("Sent '" + text + "' using protected protocol.");
                        string Received = sr.ReadLine();
                        string RCVDecrypted = Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(Received),Key)).Trim((char)0x00);
                        Console.WriteLine("Received '" + RCVDecrypted + "' using protected protocol.");
                        Console.WriteLine("-------------------------------");
                    }
                } catch (Exception e)
                {
                    Console.WriteLine("Unexpected error!");
                }
            }
        }

        public static byte[] Receive(NetworkStream ns)
        {
            while (!ns.DataAvailable) { Thread.Sleep(200); }
            List<byte> bytes = new List<byte>();
            int endFactor = 0;
            while (endFactor < 2 * 10) //2 seconds
            {
                while (ns.DataAvailable)
                {
<<<<<<< HEAD
                    byte[] buffer = new byte[2048];
=======
                    byte[] buffer = new byte[1024];
>>>>>>> 5c21123cb55592eb91d63083d0f573f17a0b2238
                    ns.Read(buffer, 0, buffer.Length);
                    bytes.AddRange(buffer);
                    endFactor = 0;
                }
                Thread.Sleep(100);
                endFactor++;
            }
            return bytes.ToArray();
        }

        private static string HEX(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        public static void GenerateRandomBytes(int seed, ref byte[] barray)
        {
            Random rng = new Random(seed);
            for (int i = 0; i < barray.Length; i++)
            {
                barray[i] = (byte)rng.Next(0, 256);
            }
        }
        public static void GenerateRandomBytes(ref byte[] barray)
        {
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(barray);
        }
        private static byte[] Encrypt(byte[] toEncrypt, byte[] key)
        {
            List<byte> te = toEncrypt.ToList();
            while (te.Count % 16 != 0) te.Add(0x00);
            toEncrypt = te.ToArray();
            AesCryptoServiceProvider aes = CreateProvider(key);
            List<byte> K = new List<byte>();
            K = key.ToList();
            while (K.Count < 32)
            {
                K.Add(0x00);
            }
            key = K.ToArray();
            ICryptoTransform cTransform = aes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncrypt, 0, toEncrypt.Length);
            aes.Clear();
            return resultArray;
        }
        public static AesCryptoServiceProvider CreateProvider(byte[] key)
        {
            return new AesCryptoServiceProvider
            {
                KeySize = 256,
                BlockSize = 128,
                Key = key,
                Padding = PaddingMode.None,
                Mode = CipherMode.ECB
            };
        }
        private static byte[] Decrypt(byte[] toDecrypt, byte[] key)
        {
            AesCryptoServiceProvider aes = CreateProvider(key);
            List<byte> K = new List<byte>();
            K = key.ToList();
            while (K.Count < 32)
            {
                K.Add(0x00);
            }
            key = K.ToArray();
            ICryptoTransform cTransform = aes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toDecrypt, 0, toDecrypt.Length);
            aes.Clear();
            return resultArray;
        }
    }
}
