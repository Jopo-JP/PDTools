﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Buffers.Binary;

namespace PDTools.Crypto.SimulationInterface
{
    /// <summary>
    /// Used to decrypt packets from GT6's Simulator Interface.
    /// </summary>
    public class SimulatorInterfaceCryptor : ISimulationInterfaceCryptor
    {
        private Salsa20 _salsa;

        public const string Key = "Simulator Interface Packet ver 0.0";

        public SimulatorInterfaceCryptor()
        {
            _salsa = new Salsa20(Encoding.ASCII.GetBytes(Key), 0x22);
        }

        public void Decrypt(Span<byte> bytes)
        {
            // Input should be 0x94 (or 0x128 in certain cases?)

            // Reset offset
            _salsa.Set(0);

            int iv1 = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(0x40)); // Seed IV is always located there
            int iv2 = (int)(iv1 ^ 0xDEADBEEF) + 0x40; // Lol

            Span<byte> iv = stackalloc byte[8];
            BinaryPrimitives.WriteInt32LittleEndian(iv, iv2);
            BinaryPrimitives.WriteInt32LittleEndian(iv[4..], iv1);
            _salsa.SetIV(iv);

            _salsa.Decrypt(bytes, 0x94);

            // Magic should be "G6S0" when decrypted 
        }
    }
}
