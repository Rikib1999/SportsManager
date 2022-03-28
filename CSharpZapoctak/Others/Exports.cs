using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CSharpZapoctak.Others
{
    static class Exports
    {
        public static void Export(System.Windows.Controls.DataGrid dataGrid, string format, int? exportTop)
        {
            //create excel file
            Microsoft.Office.Interop.Excel.Application excelApplication = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook excelWorkbook;
            excelWorkbook = excelApplication.Workbooks.Add();
            Microsoft.Office.Interop.Excel.Worksheet table = (Microsoft.Office.Interop.Excel.Worksheet)excelApplication.ActiveSheet;

            //info
            table.Range["A" + 1].Value = char.ToUpper(SportsData.sport.name[0]) + SportsData.sport.name.Substring(1).Replace('_', '-');
            if (SportsData.competition.id > 0) { table.Range["A" + 1].Value += " - " + SportsData.competition.Name; }
            if (SportsData.season.id > 0) { table.Range["A" + 1].Value += " - " + SportsData.season.Name; }

            //header
            List<string> propNames = new List<string>();
            List<DataGridColumn> columns = new List<DataGridColumn>();
            for (int i = 1; i < dataGrid.Columns.Count; i++)
            {
                if (dataGrid.Columns[i].Visibility != Visibility.Visible) { continue; }
                columns.Add(dataGrid.Columns[i]);
            }
            columns = columns.OrderBy(x => x.DisplayIndex).ToList();

            for (int i = 0; i < columns.Count; i++)
            {
                var binding = (columns[i] as DataGridBoundColumn).Binding as Binding;
                propNames.Add(binding.Path.Path);
                char col = (char)('A' + i);
                table.Range[col + "2"].Value = columns[i].Header;
            }

            //table data
            int rowCount = dataGrid.Items.Count;
            if (exportTop != null && exportTop < rowCount) { rowCount = (int)exportTop; }
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columns.Count; j++)
                {
                    string s = GetPropertyValue(dataGrid.Items[i], propNames[j]).ToString();
                    char col = (char)('A' + j);
                    table.Range[col + "" + (i + 3)].Value = s;
                }
            }

            //create table
            var range = table.get_Range("A2:" + (char)('A' + columns.Count - 1) + "" + (rowCount + 2));
            table.ListObjects.AddEx(XlListObjectSourceType.xlSrcRange, range, Type.Missing, Microsoft.Office.Interop.Excel.XlYesNoGuess.xlYes, Type.Missing).Name = "MyTableStyle";
            table.ListObjects.get_Item("MyTableStyle").TableStyle = "TableStyleMedium1";

            //select path
            string tablePath = "";

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            switch (format)
            {
                case "PDF":
                    saveFileDialog.Filter = "PDF Files | *.pdf";
                    saveFileDialog.DefaultExt = "pdf";
                    break;
                case "XLSX":
                    saveFileDialog.Filter = "XLSX | *.xlsx";
                    saveFileDialog.DefaultExt = "xlsx";
                    break;
                default:
                    break;
            }
            saveFileDialog.FileName = "table";

            bool? result = saveFileDialog.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                tablePath = saveFileDialog.FileName;

                switch (format)
                {
                    case "PDF":
                        //export to pdf
                        try
                        {
                            excelWorkbook.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, tablePath);
                        }
                        catch (Exception) { }
                        break;
                    case "XLSX":
                        excelWorkbook.SaveCopyAs(tablePath);
                        break;
                    default:
                        break;
                }
            }

            excelWorkbook.Close(false);
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            foreach (var prop in propertyName.Split('.').Select(s => obj.GetType().GetProperty(s)))
            {
                obj = prop.GetValue(obj, null);
            }

            return obj;
        }
    }
}
