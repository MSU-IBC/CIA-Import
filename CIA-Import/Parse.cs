using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBC.Database;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CIA_Import
{
    class Parse
    {
        public static void Parse2085(TextBox t, string tableData, int fieldid, int tagid)
        {
            db db = new db(System.Configuration.ConfigurationSettings.AppSettings["db"]);
            db.ParameterAdd("@FieldID", fieldid);
            db.ParameterAdd("@TagID", tagid);

            db.ParameterAdd("@IsEstimate", "");
            var parsedData = Regex.Matches(tableData.Trim(), @"(\w*): (\d{1,3}(\,\d{3})*)\s*km\s*(\((\d{4})\))?\s*");
            int year = 0;
            double total = 0d;
            string notes = "";
            int c = 0;
            db.SetSqlStoredProcedure();
            for (var i = 0; i < parsedData.Count; i++)
            {
                if (parsedData[i].Groups[1].Value == "total")
                {
                    Double.TryParse(parsedData[i].Groups[2].Value, out total);
                }
                else if (parsedData[i].Groups[1].Value == "note")
                {
                    notes = parsedData[i].Groups[2].Value;
                }
                if (parsedData[i].Groups[5].Value != "")
                {
                    int.TryParse(parsedData[i].Groups[5].Value, out year);
                }
                
            }
            if (year == 0)
            {
                return;
            }
            db.ParameterAdd("@Value", total);
            db.ParameterAdd("@Year", year);
            db.ParameterAdd("@Note", notes);
            try
            {
                db.ParameterEdit("@IsEstimate", false);
                var ex = db.ExecuteSql("CIA_Insert_Data");
                if (ex != null)
                {
                    t.Text += ex.Message + "--->" + Environment.NewLine;
                }
            }
            catch (Exception convEx)
            {
                t.Text += convEx.Message + "-->"  + fieldid + Environment.NewLine;
            }
            db.CloseConnection();
        }
        public static void Parse2091(TextBox t, string tableData, int fieldid, int tagid)
        {
            db db = new db(System.Configuration.ConfigurationSettings.AppSettings["db"]);
            db.ParameterAdd("@FieldID", fieldid);
            db.ParameterAdd("@TagID", tagid);
            db.ParameterAdd("@IsEstimate", false);

            var parsedData = Regex.Matches(tableData.Trim(), @"(\w*): (\d{1,3}(\,\d{3})*\.?\d*)\s*deaths\/1,000 live births\s*(\((\d{4})\s*(est.?)?\))?\s*");
            int year = 0;
            double total = 0d;
            string notes = "";
            int c = 0;
            db.SetSqlStoredProcedure();
            for (var i = 0; i < parsedData.Count; i++)
            {
                if (parsedData[i].Groups[1].Value == "total")
                {
                    Double.TryParse(parsedData[i].Groups[2].Value, out total);
                }
                else if (parsedData[i].Groups[1].Value == "note")
                {
                    notes = parsedData[i].Groups[2].Value;
                }
                if (parsedData[i].Groups[5].Value != "")
                {
                    int.TryParse(parsedData[i].Groups[5].Value, out year);
                    if (parsedData[i].Groups[6].Value != "")
                    {
                        db.ParameterEdit("@IsEstimate", true);
                    }
                }
                

            }
            if (year == 0)
            {
                return;
            }
            db.ParameterAdd("@Value", total);
            db.ParameterAdd("@Year", year);
            db.ParameterAdd("@Note", notes);
            try
            {
                var ex = db.ExecuteSql("CIA_Insert_Data");
                if (ex != null)
                {
                    t.Text += ex.Message + "--->" + Environment.NewLine;
                }
            }
            catch (Exception convEx)
            {
                t.Text += convEx.Message + "-->" + fieldid + Environment.NewLine;
            }
            db.CloseConnection();
        }
        public static void Parse2121(TextBox t, string tableData, int fieldid, int tagid)
        {
            db db = new db(System.Configuration.ConfigurationSettings.AppSettings["db"]);
            db.ParameterAdd("@FieldID", fieldid);
            db.ParameterAdd("@TagID", tagid);
            db.ParameterAdd("@IsEstimate", false);

            var parsedData = Regex.Match(tableData.Trim(), @"(total:)? (\d{1,3}(\,\d{3})*)\s*km.*\b(\d{4})\b\)$*", RegexOptions.Singleline);
            int year = 0;
            double total = 0d;
            string notes = "";
            int c = 0;
            db.SetSqlStoredProcedure();
            if (parsedData.Groups.Count < 1)
            {
                return;
            }
            db.ParameterAdd("@Value", Convert.ToDouble(parsedData.Groups[2].Value));
            db.ParameterAdd("@Year", parsedData.Groups[4].Value);
            db.ParameterAdd("@Note", "");
            try
            {
                var ex = db.ExecuteSql("CIA_Insert_Data");
                if (ex != null)
                {
                    t.Text += ex.Message + "--->" + Environment.NewLine;
                }
            }
            catch (Exception convEx)
            {
                t.Text += convEx.Message + "-->" + fieldid + Environment.NewLine;
            }
            db.CloseConnection();
        }

        public static void Parse2056(TextBox t, string tableData, int fieldid, int tagid)
        {
            var getYear = Regex.Match(tableData.Trim(), @".*(\(\b(\d{0,2}\s*\w*\s\d{2,4}|\d{4}|FY\d{0,4}\/?\d{0,4})\s*(est.?)?\b.*)");
            string year = getYear.Groups[1].Value;

            var getNote = Regex.Match(tableData.Trim(), @"note: (.*)");
            var note = getNote.Length > 0 ? "note: " + getNote.Groups[1].Value : "";
            var getRevenue = Regex.Match(tableData.Trim(), @"revenues: (.*)");
            Parse.ParseTableData(getRevenue.Groups[1].Value.Trim() + Environment.NewLine + note, 2056, tagid);
            var getExpenditure = Regex.Match(tableData.Trim(), @"expenditures: (.*)");
            Parse.ParseTableData(getExpenditure.Groups[1].Value.Trim() + Environment.NewLine + note, 20561, tagid);
        }

        public static string ParseTableData(string tableData, int fieldid, int tagid)
        {
            var sb = new StringBuilder();
            var db = new db(System.Configuration.ConfigurationSettings.AppSettings["db"]);
            db.ParameterAdd("@FieldID", fieldid);
            db.ParameterAdd("@TagID", tagid);
            db.ParameterAdd("@Value", "");
            db.ParameterAdd("@Year", "");
            db.ParameterAdd("@IsEstimate", "");
            var parsedData = Regex.Split(tableData.Trim(), @"[\r\n]+").ToList();
            int c = 0;
            var note = parsedData.FirstOrDefault(a => a.Trim().StartsWith("note:"));
            var noteRemove = note;
            if (note == null)
            {
                note = "";
            }
            else
            {
                note = note.Replace("note:", "").Trim();
            }
            db.ParameterAdd("@Note", note);
            db.SetSqlStoredProcedure();
            var noteDateMatch = Regex.Matches(note, @".*\(\b(\d{0,2}\s*\w*\s\d{2,4}|\d{4}|FY\d{0,4}\/?\d{0,4})\s*(est.?)?\b.*$", RegexOptions.Singleline);
            string noteDate = "";
            if (noteDateMatch.Count > 0)
            {
                noteDate = parseDate(noteDateMatch[0].Groups[1].Value);
            }
            parsedData.Remove(noteRemove);
            foreach (var i in parsedData)
            {
                var regExString = @"^\$?(-?\d{1,3}(\,\d{3})*\.?\d*)%?\s*(\w*)?.*\(\b(\w*\s\d{1,2},\s?\d{2,4}|\d{0,2}\s*\w*\s\d{2,4}|\d{4}|FY\d{0,4}\/?\d{0,4})\s*(est.?)?\b.*$";
                var matches = Regex.Match(i.Trim(), regExString);
                if (matches.Groups.Count < 2 && noteDate == "")
                {
                    sb.AppendLine("DID NOT MATCH-->" + i.Trim());
                    continue;
                }
                else if(matches.Groups.Count < 2 && noteDate != "")
                {
                    matches = Regex.Match(i.Trim() + " " + note, regExString);
                    if (matches.Groups.Count < 2 && noteDate != "")
                    {
                        sb.AppendLine("2nd Chance DID NOT MATCH-->" + i.Trim());
                        continue;
                    }
                }
                try
                {
                    var value = Convert.ToDecimal(matches.Groups[1].Value);
                    switch (matches.Groups[3].Value.Trim())
                    {
                        case "thousand":
                            value *= 1000m;
                            break;
                        case "million":
                            value *= 1000000m;
                            break;
                        case "billion":
                            value *= 1000000000m;
                            break;
                        case "trillion":
                            value *= 1000000000000m;
                            break;
                    }
                    db.ParameterEdit("@Value", value);
                    var date = parseDate(matches.Groups[4].Value) ?? noteDate;
                    if (date == "")
                    {
                        continue;
                    }
                    db.ParameterEdit("@Year", date);
                    db.ParameterEdit("@IsEstimate", string.IsNullOrEmpty(matches.Groups[5].Value) ? false : true);
                    var ex = db.ExecuteSql("CIA_Insert_Data");
                    if (ex != null)
                    {
                        sb.AppendLine(ex.Message + "--->");
                    }
                }
                catch (Exception convEx)
                {
                    sb.AppendLine(convEx.Message + "-->" + tagid + "--->" + fieldid);

                }
            }
            db.CloseConnection();
            return sb.ToString();
        }

        private static string parseDate(string date)
        {
            DateTime dateOut;
            int year;
            if (int.TryParse(date, out year))
            {
                return year.ToString();
            }
            if (DateTime.TryParse(date, out dateOut))
            {
                return dateOut.Year.ToString();
            }
            var matches = Regex.Match(date, @"^FY\d{2}/(\d{2})");
            if (matches.Groups.Count > 1)
            {
                return "20" + matches.Groups[1].Value;
            }
            return null;
        }
    }
}
