using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IBC.Database;
using System.IO;
using HtmlAgilityPack;
using TidyNet;
using System.Text.RegularExpressions;

namespace CIA_Import
{
    public partial class Form1 : Form
    {
        db _db = new db(System.Configuration.ConfigurationSettings.AppSettings["db"]);
        public Dictionary<string, int> _countryTagList = new Dictionary<string, int>();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowse.ShowDialog() == DialogResult.OK)
            {
                textFolder.Text = folderBrowse.SelectedPath;
            }
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            Exception ex;

            // I had to throw in the len as Budget Revenue and Expenditures have the same ID except the Expenditures has a 1 at the end.
            ex = _db.ExecuteSqlReader("SELECT * FROM CIA_Fields WHERE fieldid > 2000 AND LEN(fieldid) = 4");

            if (ex != null)
            {
                throw new Exception(ex.Message);
            }
            var fieldIDs = new List<int>();
            while (_db.Reader.Read())
            {
                 fieldIDs.Add((int)_db.Reader["FieldID"]);
            }
            _db.Reader.Close();
            foreach(var f in fieldIDs){
                textBoxOutput.Text += f + Environment.NewLine;
                var input = File.OpenRead(textFolder.Text + "\\" + f + ".html");
                var tmc = new TidyMessageCollection();
                var output = new MemoryStream();

                var tidy = new Tidy();
                tidy.Options.DocType = DocType.Strict;
                tidy.Options.DropFontTags = true;
                tidy.Options.LogicalEmphasis = true;
                tidy.Options.Xhtml = true;
                tidy.Options.XmlOut = true;
                tidy.Options.MakeClean = true;
                tidy.Options.TidyMark = false;
                tidy.Options.WrapLen = 0;
                tidy.Parse(input, output, tmc);

                var result = Encoding.UTF8.GetString(output.ToArray());
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(result);

                var categoryData = doc.DocumentNode.SelectNodes("//td[@class='category_data']");
                if (categoryData != null)
                {
                    foreach (var i in categoryData)
                    {
                        if (i != null)
                        {
                            var tagID = _countryTagList.SingleOrDefault(a => a.Key == i.ParentNode.ParentNode.Id);
                            if (tagID.Key == null)
                            {
                                continue;
                            }
                            switch(f)
                            {
                                case 2085:
                                    Parse.Parse2085(textBoxOutput, i.InnerText, f, tagID.Value);
                                    break;
                                case 2091:
                                    Parse.Parse2091(textBoxOutput, i.InnerText, f, tagID.Value);
                                    break;
                                case 2121:
                                    Parse.Parse2121(textBoxOutput, i.InnerText, f, tagID.Value);
                                    break;
                                case 2056:
                                    Parse.Parse2056(textBoxOutput, i.InnerText, f, tagID.Value);
                                    break;
                                default:
                                    textBoxOutput.Text += Parse.ParseTableData(i.InnerText, f, tagID.Value);
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    textBoxOutput.Text += f + ": NO DATA" + Environment.NewLine;
                }
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            var ex = _db.ExecuteSqlReader("SELECT Abbr, TagID FROM Country");
            if (ex != null)
            {
                throw new Exception(ex.Message);
            }
            while (_db.Reader.Read())
            {
                _countryTagList.Add(((string)_db.Reader["Abbr"]).ToLower(), (int)_db.Reader["TagID"]);
            }
            _db.Reader.Close();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _db.CloseConnection();
        }

    }
}
