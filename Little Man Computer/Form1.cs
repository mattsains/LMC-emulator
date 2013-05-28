using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Little_Man_Computer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private lmc LMC = new lmc();

        private void Form1_Load(object sender, EventArgs e)
        {
            //some interface intialization code
            GridRAM.Columns[0].Width = 30;
            GridRAM.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            for (int i = 0; i < 100; i++)
            {
                GridRAM.Rows.Add();
                GridRAM[0, i].Value = i.ToString("00");
            }
            GridRAM.Focus();
            GridRAM.CurrentCell = GridRAM[1, 0];
        }

        private Dictionary<string, byte> labels = new Dictionary<string, byte>();

        private SortedDictionary<byte, string> errors = new SortedDictionary<byte, string>();
        private void GridRAM_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            edtMessages.Clear();
            errors = new SortedDictionary<byte, string>();
            labels = new Dictionary<string, byte>();
            for (byte addr = 0; addr < 100; addr++)
            {
                string line = (string)GridRAM[1, addr].Value;
                if (line == null) continue;
                line = line.Trim().ToUpper();

                //Make sure the labels are accounted for
                if (line[0] == ':')
                {
                    string[] parts = line.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    //validation: what can a label be called?
                    parts[0] = parts[0].Substring(1); //remove the :
                    int dummy; //not ever used
                    if (parts[0] == "" || new string[] { "ADD", "SUB", "LDA", "STO", "BR", "BRP", "BRZ", "HLT", "IN", "OUT", "DAT" }.Contains(parts[0]) || int.TryParse(parts[0], out dummy))
                    {
                        AddError(addr, "Label '" + parts[0] + "' is invalid!");
                        continue;
                    }
                    //update the label dictionary
                    if (labels.ContainsKey(parts[0])) labels[parts[0]] = addr;
                    else labels.Add(parts[0], addr);
                }
            }

            for (byte addr = 0; addr < 100; addr++)
            {
                string line = (string)GridRAM[1, addr].Value;
                if (line == null) continue;
                line = line.Trim().ToUpper();
                
                if (line[0] == ':')
                {
                    string[] parts = line.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        line = parts[1];//remove the label
                    }
                    catch (IndexOutOfRangeException) { continue; }
                }
                //now translate this into opcode
                try
                {

                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //translate labels
                    for (int i = 0; i < parts.Length; i++)
                        if (labels.ContainsKey(parts[i]))
                        {
                            parts[i] = labels[parts[i]].ToString();
                        }

                    line = "";
                    foreach (string part in parts)
                        line += part + " ";

                    ushort opcode = Mnemonic.ToByteCode(line);
                    LMC.RAM[addr] = opcode;
                }
                catch (Exception except)
                {
                    AddError(addr, except.Message);
                }
            }
        }

        private void DisplayErrors()
        {
            edtMessages.Clear();

            foreach (KeyValuePair<byte, string> error in errors)
                if (error.Key == 255)
                    edtMessages.Text += "\n   " + error.Value + "\n";
                else
                    edtMessages.Text += error.Key + ": " + error.Value + "\n";
        }
        private void AddError(byte line, string message)
        {
            if (errors.ContainsKey(line)) errors[line] = message;
            else errors.Add(line, message);
            DisplayErrors();
        }
        private void RemoveError(byte line)
        {
            if (errors.ContainsKey(line)) errors.Remove(line);
        }

        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (errors.Count>0)
            {
                AddError(255,"You can't run the LMC if there's syntax errors!");
                return;
            }
            LMC.Begin();

            try
            {
                while (LMC.Running)
                {
                    LMC.Step();
                }
                edtMessages.Text += "\n    Program HALTed at "+LMC.IR+"\n";
            }
            catch (Exception exep)
            {
                edtMessages.Text += "\n   " + exep.Message + "\n";
            }
            finally
            {
                LMC.Reset();
            }
        }

        public void PutInRAM(byte addr, ushort val)
        {
            GridRAM[1, addr].Value = val.ToString();
        }

        private void GridRAM_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                GridRAM[1, i].Value = "";
                LMC.RAM[i] = 0;   
            }
            LMC.Reset();
        }
        public void SetAC(ushort n)
        {
            lblAC.Text = n.ToString();
        }

        private void btnStep_Click(object sender, EventArgs e)
        {
            if (errors.Count>0) 
            {
                AddError(255,"You can't run the LMC if there's syntax errors!");
                return;
            }

            if (LMC.Running)
            {
                try
                {
                    LMC.Step();
                }
                catch (Exception exept)
                {
                    edtMessages.Text = exept.Message;
                    LMC.Reset();
                    return;//don't update grid
                }
            }
            else
            {
                LMC.Begin();
               
            }
            GridRAM.CurrentCell = GridRAM[0, LMC.IR];
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Filter = "Little Man computer|*.lmc|Text Document|*.txt";

            s.AddExtension = true;
            DialogResult result=s.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                StreamWriter SW = new StreamWriter(s.FileName);
                int i;
                for (i = 0; i < 100; i++)
                {
                    string line=(string)GridRAM[1,i].Value;
                    if (line == null)
                        break;
                    else SW.WriteLine(line);
                }
                //now we have to shift this in manually
                bool hasEntered = false;
                for (; i < 100; i++)
                {
                    string line = (string)GridRAM[1, i].Value;
                    if (line == null)
                    {
                        if (!hasEntered)
                        {
                            hasEntered = true;
                            SW.WriteLine();
                        }
                        continue;
                    }
                    else
                    {
                        SW.WriteLine(i + " " + line);
                        hasEntered = false;
                    }
                }
                SW.Close();
            }
        }
    }
}