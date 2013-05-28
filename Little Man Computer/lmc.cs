using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace Little_Man_Computer
{
    class lmc
    {
        public ushort[] RAM = new ushort[100];
        public ushort AC=0;
        public byte IR = 0;
        public bool Running = false;

        public void Step()
        {
            ushort instruction = RAM[IR++];
            byte address = (byte)(instruction % 100);

            switch (instruction / 100)
            {
                case 0: if (address == 0) { Running = false; IR--; } else throw new Exception(string.Format("HALT: invalid opcode reached at address {0}", IR - 1)); break;
                case 1: AC += RAM[address]; break;
                case 2: AC -= RAM[address]; break;
                case 3: RAM[address] = AC; Program.form1.PutInRAM(address, RAM[address]); break;
                case 4: break;
                case 5: AC = RAM[address]; break;
                case 6: IR = address; break;
                case 7: if (AC == 0) IR = address; break;
                case 8: if (AC > 500) IR = address; break;
                case 9: if (address == 1) { AC = ushort.Parse(Interaction.InputBox("Please enter a number!", "IN", "0")); }
                    else if (address == 2) { Interaction.MsgBox(AC, MsgBoxStyle.OkOnly, "OUT"); } break;
                default: throw new Exception(string.Format("HALT: invalid opcode reached at address {0}", IR - 1));
            }
        }

        public void Begin()
        {
            IR = 0;
            AC = 0;
            Running = true;
        }
        public void Reset()
        {
            Running = false;
        }
    }

    static class Mnemonic
    {
        static public ushort ToByteCode(string instruction)
        {
            instruction = instruction.ToUpper();
            string[] parts = instruction.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) throw new Exception("An empty string is not valid syntax");

            //check for numbers
            ushort potentialNumber;
            if (ushort.TryParse(parts[0], out potentialNumber))
            {
                return potentialNumber;
            }
            else if (parts[0] == "DAT")
            {
                return Mnemonic.ToByteCode(parts[1]);//DAT is followed either by an instruction or a number
            }
            else if (parts[0] == "IN")
            {
                if (parts.Length > 1) throw new Exception("Unexpected arguments for IN instruction");
                else return 901;
            }
            else if (parts[0] == "OUT")
            {
                if (parts.Length > 1) throw new Exception("Unexpected arguments for OUT instruction");
                else return 902;
            }
            else if (parts[0] == "HLT")
            {
                if (parts.Length > 1) throw new Exception("Unexpected arguments for HLT instruction");
                else return 0;
            }
            else
            {
                if (parts.Length < 2) throw new Exception("Argument expected for " + parts[0] + " instruction");
                if (parts.Length > 2) throw new Exception("Unexpected second argument for " + parts[0] + " instruction");

                ushort address;
                try { address = ushort.Parse(parts[1]); }
                catch (Exception) { throw new Exception("Argument is invalid"); }

                if (address > 99) throw new Exception("Argument is out of bounds!");

                //otherwise we should be ready to parse an instruction with a parameter
                switch (parts[0])
                {
                    case "ADD": return (ushort)(100 + address);
                    case "SUB": return (ushort)(200 + address);
                    case "STO": return (ushort)(300 + address);
                    case "LDA": return (ushort)(500 + address);
                    case "BR": return (ushort)(600 + address);
                    case "BRZ": return (ushort)(700 + address);
                    case "BRP": return (ushort)(800 + address);
                    default: throw new Exception("Invalid opcode " + parts[0]);
                }
            }
        }
    }
}
