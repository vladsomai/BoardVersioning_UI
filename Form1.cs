using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

#pragma warning disable IDE1006 // Naming Styles

namespace BoardVersioning_UI
{
    public partial class BoardVersioning : Form
    {
        public BoardVersioning()
        {
            InitializeComponent();
            
        }

        private void BoardVersioning_Load(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        //browse button
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog raw = new OpenFileDialog();
            if (raw.ShowDialog() == DialogResult.OK)
            {
                string rawFile = raw.FileName;
                textBox1.Text = rawFile;
            }
            raw.Dispose();
        }

        //browse button
        private void button2_Click(object sender, EventArgs e)
        {
           
            using (OpenFileDialog raw = new OpenFileDialog())
            {
                if (raw.ShowDialog() == DialogResult.OK)
                {
                    string rawFile = raw.FileName;
                    textBox2.Text = rawFile;
                }
            }
        }
        //browse button
        private void button3_Click_1(object sender, EventArgs e)
        {
         
            using (OpenFileDialog board = new OpenFileDialog())
            {
                if (board.ShowDialog() == DialogResult.OK)
                {
                    string boardpath = board.FileName;
                    textBox3.Text += boardpath;
                }
            }
        }

        //text box for raw bom path
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //enter in log the bom path
            richTextBox1.AppendText("The raw BOM is: \n");
            richTextBox1.AppendText(textBox1.Text+"\n");
        }

        //loggging window
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            //make the richtextbox autoscroll to last item
            richTextBox1.Focus();    
        }

        //text box for variant bom path
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.AppendText("The variant BOM is: \n");
            richTextBox1.AppendText(textBox2.Text + "\n");
        }

        //parse button
        private void button3_Click(object sender, EventArgs e)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string rawPath = textBox1.Text;
            string variantPath = textBox2.Text;
            string boardPath = textBox3.Text;
            string versionName = textBox4.Text;

            //string rawPath     = @"C:\Users\uidn4945\Downloads\20053011_AD_AsyPartListProp_Max.csv";
            //string variantPath = @"C:\Users\uidn4945\Downloads\20060556_AA_AsyPartListProp_Var.csv";
            //string boardPath   = @"C:\Users\uidn4945\Downloads\board";
            //string versionName = @"V02";

            if (!File.Exists(rawPath))
            {
                richTextBox1.AppendText("Raw BOM not found!\n");
                return;
            }
            if (!File.Exists(variantPath))
            {
                richTextBox1.AppendText("Variant BOM not found!\n");
                return;
            }
            if (!File.Exists(boardPath))
            {
                richTextBox1.AppendText("\"board\" file not found!\n");
                return;
            }

            if(versionName.Length == 0)
            {
                richTextBox1.AppendText("Please enter a version name!\n");
                return;
            }

            //READ THE BOMS -> ADD THEM INTO AN ARRAY
            string[] rawBom = System.IO.File.ReadAllLines(rawPath);
            string[] variantBom = System.IO.File.ReadAllLines(variantPath);
            List<string> rawDesignators = new List<string>();
            List<string> variantDesignators = new List<string>();

            string[] splitLine;

            //Add only lines that contain A2C into a list
            foreach (string line in rawBom)
            {
                if (line[0] == '#')
                {

                }
                else if (line.Contains("A2C") || line.Contains("A3C"))
                {
                    splitLine = line.Split(';');
                    rawDesignators.Add(splitLine[0]);
                }
            }

            foreach (string line in variantBom)
            {
                if (line[0] == '#')
                {

                }
                else if (line.Contains("A2C") || line.Contains("A3C"))
                {
                    splitLine = line.Split(';');
                    variantDesignators.Add(splitLine[0]);
                }
            }

            List<string> missingInVariant = new List<string>();
            foreach (string designator in rawDesignators)
            {
                if (!variantDesignators.Contains(designator))
                    missingInVariant.Add(designator);
            }

            //Parse the board file
            string[] board = File.ReadAllLines(boardPath);
            string modBoardPath = desktopPath + @"\board";
            StreamWriter modBoard = new StreamWriter(modBoardPath);//create the board to be modified

            bool nodesFound = false;//in case word "NODES" is found continue writing the BOM file.
            foreach (string boardLine in board)
            {
                if (boardLine.Contains("HEADING"))
                    modBoard.Write($"VERSIONS\n  {versionName};\n\n");

                if (boardLine.Contains("NODES") || boardLine.Contains("PIN_MAP") || boardLine.Contains("CONNECTIONS"))
                    nodesFound = true;

                if (nodesFound)
                {
                    modBoard.WriteLine(boardLine);
                }
                else
                {
                    splitLine = boardLine.Split(' ');
                    try
                    {
                        if(boardLine.Contains("VERSION"))
                        {
                            modBoard.WriteLine(boardLine);
                            continue;
                        }
                        foreach (string designator in missingInVariant)
                        {
                            if (splitLine[2] == designator)
                            {
                                if (splitLine[2][0] == 'X')//connectors done have "PN" keyword, we use "NT" instead
                                {
                                    string tempBoardLinePar1 = boardLine.Substring(0, boardLine.IndexOf("NT"));
                                    string tempBoardLinePar2 = boardLine.Substring(boardLine.IndexOf("NT"), boardLine.IndexOf(";") - boardLine.IndexOf("NT"));
                                    modBoard.WriteLine(tempBoardLinePar1 + " NP " + tempBoardLinePar2 + $"   VERSION {versionName};");
                                }
                                else // all other components contain PN keyword
                                {
                                    string tempBoardLinePar1 = boardLine.Substring(0, boardLine.IndexOf("PN"));
                                    string tempBoardLinePar2 = boardLine.Substring(boardLine.IndexOf("PN"), boardLine.IndexOf(";") - boardLine.IndexOf("PN"));
                                    modBoard.WriteLine(tempBoardLinePar1 + " NP " + tempBoardLinePar2 + $"   VERSION {versionName};");
                                }
                            }
                        }
                        modBoard.WriteLine(boardLine);
                    }
                    catch (Exception)
                    {
                        modBoard.WriteLine(boardLine);
                    }
                }
            }
            modBoard.Close();
            richTextBox1.AppendText("\nParsing done, please check file \"modBoard\" on your Desktop!\nUse command \"check board \"modBoard\";list\" to identify any errors that may occur!");
        }
    }
}
