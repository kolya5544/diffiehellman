using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace DiffieHellman
{
    class Program
    {
        public static byte[] PublicKey = new byte[1024 * 1];
        public static void Log(string t)
        {
            Console.WriteLine("["+DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")+"] "+t);
        }
        static void Main(string[] args)
        {
            Log("Generating public key...");
            GenerateRandomBytes(1234567, ref PublicKey);
            int port = 7887;
            Log("===[ Starting server at 7887 port ]===");
            var TCP = new TcpListener(port);
            TCP.Start();
            Log("Accepting connections...");
            var Algo = HashAlgorithm.Create("SHA256");
            while (true)
            {
                var client = TCP.AcceptTcpClient();

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    NetworkStream ns = client.GetStream();
                    ns.ReadTimeout = 3000;
                    ns.WriteTimeout = 3000;
                    string hostname = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    Log("Got " + hostname + " connected!");
                    //Generate our own private key.
                    byte[] PrivateKey = new byte[1024 * 1];
                    GenerateRandomBytes(ref PrivateKey);
                    byte[] MixReceived = null;
                    byte[] MixSent = null;
                    byte[] Key = null;
                    try
                    {
                        //Reading MIX of Public and Private.
                        MixReceived = new byte[1024 * 1];
                        MixReceived = Receive(ns);
                        //Preparing own mixture.
                        MixSent = new byte[1024 * 1];
                        for (int i = 0; i < MixSent.Length; i++)
                        {
                            MixSent[i] = (byte)(PublicKey[i] ^ PrivateKey[i]);
                        }
                        //Sending own mixture
                        ns.Write(MixSent, 0, MixSent.Length);

                        //Combining to create sign.
                        byte[] Sign = new byte[1024 * 1];
                        for (int i = 0; i < Sign.Length; i++)
                        {
                            Sign[i] = (byte)(MixReceived[i] ^ PrivateKey[i]);
                        }
                        
                        //Getting final key.
                        Key = Algo.ComputeHash(Sign);
                        Log("Successful Diffie-Hellman with " + hostname + ".");
                    }
                    catch (Exception e)
                    {
                        Log("Failed to create common key."); ns = null;
                    }

                    try
                    {
                        if (ns != null)
                        {
                            ns.WriteTimeout = 2000;
                            ns.ReadTimeout = 60000;
                            StreamReader sr = new StreamReader(ns);
                            StreamWriter sw = new StreamWriter(ns);
                            sw.AutoFlush = true;
                            while (true)
                            {
                                string NewMsg = sr.ReadLine();
                                byte[] Message = Convert.FromBase64String(NewMsg);
                                string DecryptedMessage = Encoding.UTF8.GetString(Decrypt(Message, Key)).Trim((char)0x00);
                                Log("'" + DecryptedMessage + "' by " + hostname + " using key NOBODY knows.");
                                string ToSendMSG = Reverse(DecryptedMessage);
                                ToSendMSG = Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(ToSendMSG), Key));
                                sw.Write(ToSendMSG + "\r\n");
                            }
                        }
                    } catch
                    {
                        
                    }
                    client.Close();
                }).Start();
                
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
                    byte[] buffer = new byte[1024];
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

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
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

        public static void GenerateRandomBytes(int seed, ref byte[] barray)
        {
            Random rng = new Random(seed);
            for (int i = 0; i<barray.Length; i++)
            {
                barray[i] = (byte)rng.Next(0,256);
            }
        }
        public static void GenerateRandomBytes(ref byte[] barray)
        {
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(barray);
        }
    }
}
