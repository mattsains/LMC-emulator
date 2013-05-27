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
        }

        private Dictionary<string, byte> labels = new Dictionary<string, byte>();

        private List<string> errors = new List<string>();
        private void GridRAM_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string line = (string)GridRAM[1, e.RowIndex].Value;
            line=line.Trim().ToUpper();
            //Make sure the labels are accounted for
            if (line[0] == ':')
            {
                string[] parts = line.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                //validation: what can a label be called?
                parts[0] = parts[0].Substring(1);//remove the :
                int dummy;//not ever used
                if (parts[0] == "" || new string[] { "ADD", "SUB", "LDA", "STO", "BR", "BRP", "BRZ", "HLT", "IN", "OUT", "DAT" }.Contains(parts[0]) || int.TryParse(parts[0], out dummy))
                {
                    errors.Add("Label '" + parts[0] + "' is invalid!");
                    return;
                }
                //update the label dictionary
                if (labels.ContainsKey(parts[0])) labels[parts[0]] = (byte)e.RowIndex;
                else labels.Add(parts[0], (byte)e.RowIndex);
                line = parts[1];//remove the label
            }
            else if (labels.ContainsValue((byte)e.RowIndex))
                foreach (KeyValuePair<string, byte> label in labels) //ugly but the only way
                    if (label.Value == (byte)e.RowIndex)
                        labels.Remove(label.Key);
            //now translate this into opcode
            
        }
    }
}
