using SportsManager.Models;
using SportsManager.ViewModels;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SportsManager.Others
{
    /// <summary>
    /// Static class containing methods for data export into different formats.
    /// </summary>
    public static class Exports
    {
        /// <summary>
        /// Exports datagrid table in XLSX or PDF format. Brings up the file dialog window.
        /// </summary>
        /// <param name="dataGrid">Datagrid to export.</param>
        /// <param name="format">XLSX or PDF.</param>
        /// <param name="exportTop">Number of rows from top to be exported. If null, all rows will be exported.</param>
        public static void ExportTable(DataGrid dataGrid, string format, int? exportTop)
        {
            //create excel file
            Microsoft.Office.Interop.Excel.Application excelApplication = new();
            _Workbook excelWorkbook;
            excelWorkbook = excelApplication.Workbooks.Add();
            Worksheet table = (Worksheet)excelApplication.ActiveSheet;

            //info
            table.Range["A" + 1].Value = SportsData.SPORT.FormattedName;
            if (SportsData.IsCompetitionSet()) { table.Range["A" + 1].Value += " - " + SportsData.COMPETITION.Name; }
            if (SportsData.IsSeasonSet()) { table.Range["A" + 1].Value += " - " + SportsData.SEASON.Name; }

            //header
            List<string> propNames = new();
            List<DataGridColumn> columns = new();
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                if (dataGrid.Columns[i].Visibility != Visibility.Visible || dataGrid.Columns[i] is DataGridTemplateColumn) { continue; }
                columns.Add(dataGrid.Columns[i]);
            }
            columns = columns.OrderBy(x => x.DisplayIndex).ToList();

            for (int i = 0; i < columns.Count; i++)
            {
                Binding binding = (columns[i] as DataGridBoundColumn).Binding as Binding;
                propNames.Add(binding.Path.Path);
                string col = GetColumnName(i);
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
                    string col = GetColumnName(j);
                    table.Range[col + "" + (i + 3)].Value = s;
                }
            }

            //create table
            Microsoft.Office.Interop.Excel.Range range = table.get_Range("A2:" + GetColumnName(columns.Count - 1)/*(char)('A' + columns.Count - 1)*/ + "" + (rowCount + 2));
            table.ListObjects.AddEx(XlListObjectSourceType.xlSrcRange, range, Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name = "MyTableStyle";
            table.ListObjects.get_Item("MyTableStyle").TableStyle = "TableStyleLight9";

            //page orientation
            if (columns.Count > 7)
            {
                table.PageSetup.Orientation = XlPageOrientation.xlLandscape;
            }

            SaveExcelSheet(format, excelWorkbook, "table");

            excelWorkbook.Close(false);
        }

        /// <summary>
        /// Gets the lexicographical column name by its order index.
        /// </summary>
        /// <remarks>Starting with "A" for index 1 and "Z" for index 26, continuing with "AA" for index 27 and so on.</remarks>
        /// <param name="index">Number of the desired column</param>
        /// <returns>Column name in uppercase format.</returns>
        public static string GetColumnName(int index)
        {
            const byte BASE = 'Z' - 'A' + 1;
            string name = string.Empty;
            do
            {
                name = Convert.ToChar('A' + index % BASE) + name;
                index = index / BASE - 1;
            }
            while (index >= 0);
            return name;
        }

        /// <summary>
        /// Exports group standings in XLSX or PDF format. Brings up the file dialog window.
        /// </summary>
        /// <param name="groups">Collection of groups which standings should be exported.</param>
        /// <param name="format">XLSX or PDF.</param>
        /// <param name="lastRound">Name of round until which the standings should be calculated.</param>
        public static void ExportStandings(ObservableCollection<Group> groups, string format, string lastRound)
        {
            //load excel file
            string tempPath = Path.GetTempFileName();
            File.WriteAllBytes(tempPath, Properties.Resources.standings);
            Microsoft.Office.Interop.Excel.Application excelApplication = new();
            _Workbook excelWorkbook;
            excelWorkbook = excelApplication.Workbooks.Open(tempPath);
            Worksheet standings = (Worksheet)excelApplication.ActiveSheet;

            // if group has more than 42 teams, divide it into more pages

            Microsoft.Office.Interop.Excel.Range headerRange = standings.Range["A1:O10"];
            Microsoft.Office.Interop.Excel.Range tableHeaderRange = standings.Range["A11:O12"];

            int currentColor = 0;
            Enum[] colors = new Enum[] { XlRgbColor.rgbRed, XlRgbColor.rgbBlue, XlRgbColor.rgbGreen, XlRgbColor.rgbDarkOrange, XlRgbColor.rgbDarkMagenta, XlRgbColor.rgbDarkOliveGreen,
                                        XlRgbColor.rgbDarkSalmon, XlRgbColor.rgbDarkSeaGreen, XlRgbColor.rgbDarkGoldenrod, XlRgbColor.rgbNavy, XlRgbColor.rgbBlack, XlRgbColor.rgbTomato,
                                        XlRgbColor.rgbPurple, XlRgbColor.rgbDarkGray, XlRgbColor.rgbForestGreen, XlRgbColor.rgbFireBrick};

            int currentPage = 1;
            int rowsAtPage = 56;
            int rowsForGroupAtPage = 44;
            int pageStartRow = 11;
            int marginRows = 2;
            int tableHeaderRows = 2;
            int currentRow = pageStartRow;
            int rowsLeftAtPage = rowsForGroupAtPage;

            //set round
            standings.Range["F5"].Value = "Standings after " + lastRound;

            //insert season logo
            InsertLogo(SportsData.COMPETITION.ImagePath, 240, 200, "A1", standings);

            foreach (Group g in groups)
            {
                int teamCount = g.Teams.Count;
                Microsoft.Office.Interop.Excel.Range headerDestination;

                bool tableFitsWholePage = teamCount + tableHeaderRows <= rowsForGroupAtPage;
                bool tableFitsRestOfPage = teamCount + tableHeaderRows <= rowsLeftAtPage;
                bool noSpaceForTable = rowsLeftAtPage < tableHeaderRows + 1;

                //if table fits the page
                if ((tableFitsWholePage && !tableFitsRestOfPage) || noSpaceForTable)
                {
                    //if table does not fit current page, turn to next page
                    if (teamCount + tableHeaderRows > rowsLeftAtPage)
                    {
                        currentPage++;
                        currentRow = ((currentPage - 1) * rowsAtPage) + pageStartRow;
                        rowsLeftAtPage = rowsForGroupAtPage;

                        //copy page header
                        headerDestination = standings.Range["A" + (((currentPage - 1) * rowsAtPage) + 1)];
                        headerRange.Copy(headerDestination);
                    }
                }

                //create table
                //copy table header
                headerDestination = standings.Range["A" + currentRow];
                tableHeaderRange.Copy(headerDestination);
                standings.Range["B" + currentRow].Value = g.Name;
                standings.Range["B" + currentRow + ":" + "D" + currentRow].Merge();
                standings.Range["B" + (currentRow + 1) + ":" + "D" + (currentRow + 1)].Merge();

                //color header
                standings.Range["A" + currentRow + ":" + "D" + currentRow].Interior.Color = colors[currentColor];
                currentRow++;
                rowsLeftAtPage--;
                standings.Range["K" + currentRow].Interior.Color = colors[currentColor];
                currentRow++;
                rowsLeftAtPage--;

                //border table
                standings.Range["A" + currentRow + ":" + "O" + (currentRow + Math.Min(teamCount - 1, rowsLeftAtPage - 1))].BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin);

                //fill table
                int place = 1;
                foreach (Team t in g.Teams)
                {
                    if (rowsLeftAtPage <= 0)
                    {
                        currentPage++;
                        currentRow = ((currentPage - 1) * rowsAtPage) + pageStartRow;
                        rowsLeftAtPage = rowsForGroupAtPage;

                        //copy page header
                        headerDestination = standings.Range["A" + (((currentPage - 1) * rowsAtPage) + 1)];
                        headerRange.Copy(headerDestination);

                        //create table
                        //copy table header
                        headerDestination = standings.Range["A" + currentRow];
                        tableHeaderRange.Copy(headerDestination);
                        standings.Range["B" + currentRow].Value = g.Name;
                        standings.Range["B" + currentRow + ":" + "D" + currentRow].Merge();
                        standings.Range["B" + (currentRow + 1) + ":" + "D" + (currentRow + 1)].Merge();

                        //color header
                        standings.Range["A" + currentRow + ":" + "D" + currentRow].Interior.Color = colors[currentColor];
                        currentRow++;
                        rowsLeftAtPage--;
                        standings.Range["K" + currentRow].Interior.Color = colors[currentColor];
                        currentRow++;
                        rowsLeftAtPage--;

                        //border table
                        standings.Range["A" + currentRow + ":" + "O" + (currentRow + Math.Min(teamCount - place, rowsLeftAtPage - 1))].BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin);
                    }

                    standings.Range["A" + currentRow].NumberFormat = "@";
                    standings.Range["A" + currentRow].Value = place + ".";
                    place++;
                    standings.Range["B" + currentRow + ":" + "D" + currentRow].Merge();
                    standings.Range["B" + currentRow].Value = t.Name;
                    standings.Range["E" + currentRow].Value = ((TeamTableStats)t.Stats).GamesPlayed;
                    standings.Range["F" + currentRow].Value = ((TeamTableStats)t.Stats).Wins;
                    standings.Range["G" + currentRow].Value = ((TeamTableStats)t.Stats).WinsOT;
                    standings.Range["H" + currentRow].Value = ((TeamTableStats)t.Stats).Ties;
                    standings.Range["I" + currentRow].Value = ((TeamTableStats)t.Stats).LossesOT;
                    standings.Range["J" + currentRow].Value = ((TeamTableStats)t.Stats).Losses;
                    standings.Range["K" + currentRow].Value = ((TeamTableStats)t.Stats).Points;
                    standings.Range["K" + currentRow].Font.Bold = true;
                    standings.Range["L" + currentRow].Value = ((TeamTableStats)t.Stats).Goals;
                    standings.Range["M" + currentRow].Value = ((TeamTableStats)t.Stats).GoalsAgainst;
                    standings.Range["N" + currentRow].Value = ((TeamTableStats)t.Stats).GoalDifference;
                    standings.Range["O" + currentRow].Value = ((TeamTableStats)t.Stats).PenaltyMinutes;

                    currentRow++;
                    rowsLeftAtPage--;
                }

                //add margin
                currentRow += marginRows;
                rowsLeftAtPage -= marginRows;

                currentColor = (currentColor + 1) % colors.Length;
            }

            standings.Rows.RowHeight = 15;

            SaveExcelSheet(format, excelWorkbook, "standings");

            excelWorkbook.Close(false);
            File.Delete(tempPath);
        }

        /// <summary>
        /// Gets the value of a property by the given name.
        /// </summary>
        /// <param name="obj">Object to get the property from.</param>
        /// <param name="propertyName">Name of the requested property.</param>
        /// <returns>The value of the requested property.</returns>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            foreach (System.Reflection.PropertyInfo prop in propertyName.Split('.').Select(s => obj.GetType().GetProperty(s)))
            {
                obj = prop.GetValue(obj, null);
            }

            return obj;
        }

        /// <summary>
        /// Insert picture into excel sheet.
        /// </summary>
        /// <param name="logoPath">Path for the image of the logo.</param>
        /// <param name="width">In pixels.</param>
        /// <param name="height">In pixels.</param>
        /// <param name="range">Range of excel cells where the image will be put.</param>
        /// <param name="sheet">Excel sheet where the image will be put.</param>
        public static void InsertLogo(string logoPath, double width, double height, string range, Worksheet sheet)
        {
            if (!File.Exists(logoPath)) { return; }
            object missing = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Excel.Range picPosition = sheet.get_Range(range);
            Pictures p = sheet.Pictures(missing) as Pictures;
            Picture pic = p.Insert(logoPath, missing);
            //1 unit = 1.33 pixels
            double ratio = 1.0 + (1.0 / 3.0);
            double maxWidth = width / ratio;
            double maxHeight = height / ratio;
            if (pic.Height >= pic.Width)
            {
                pic.Height = maxHeight;
                if (pic.Width > maxWidth)
                {
                    pic.Width = maxWidth;
                    double offset = (maxHeight - pic.Height) / 2.0;
                    pic.Left = Convert.ToDouble(picPosition.Left);
                    pic.Top = Convert.ToDouble(picPosition.Top) + offset;
                }
                else
                {
                    double offset = (maxWidth - pic.Width) / 2.0;
                    pic.Left = Convert.ToDouble(picPosition.Left) + offset;
                    pic.Top = Convert.ToDouble(picPosition.Top);
                }
            }
            else
            {
                pic.Width = maxWidth;
                if (pic.Height > maxHeight)
                {
                    pic.Height = maxHeight;
                    double offset = (maxWidth - pic.Width) / 2.0;
                    pic.Left = Convert.ToDouble(picPosition.Left) + offset;
                    pic.Top = Convert.ToDouble(picPosition.Top);
                }
                else
                {
                    double offset = (maxHeight - pic.Height) / 2.0;
                    pic.Left = Convert.ToDouble(picPosition.Left);
                    pic.Top = Convert.ToDouble(picPosition.Top) + offset;
                }

            }
            pic.Placement = XlPlacement.xlMove;
        }

        /// <summary>
        /// Saves the given control as PNG. Brings up the file dialog window.
        /// </summary>
        /// <param name="control">Control to save.</param>
        /// <param name="name">New name of the saved file</param>
        public static void ExportControlToImage(FrameworkElement control, string name)
        {
            RenderTargetBitmap rtb = new((int)control.ActualWidth, (int)control.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            Rect bounds = VisualTreeHelper.GetDescendantBounds(control);
            DrawingVisual dv = new();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush vb = new(control);
                ctx.DrawRectangle(vb, null, new Rect(new System.Windows.Point(), bounds.Size));
            }
            rtb.Render(dv);

            PngBitmapEncoder png = new();
            png.Frames.Add(BitmapFrame.Create(rtb));
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "PNG Files | *.png";
            saveFileDialog.DefaultExt = "png";
            saveFileDialog.FileName = name;

            bool? result = saveFileDialog.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                //select path
                string imagePath = saveFileDialog.FileName;
                using Stream fileStream = new FileStream(imagePath, FileMode.Create);
                png.Save(fileStream);
            }
        }

        /// <summary>
        /// Saves excel sheet in XLSX format or exports it to PDF format. Brings up the file dialog window.
        /// </summary>
        /// <param name="format">XLSX or PDF.</param>
        /// <param name="excelWorkbook">Excel workbook to save.</param>
        /// <param name="filename">New name of the saved file.</param>
        public static void SaveExcelSheet(string format, _Workbook excelWorkbook, string filename)
        {
            string tablePath;

            SaveFileDialog saveFileDialog = new();
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
            saveFileDialog.FileName = filename;

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
                            excelWorkbook.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, tablePath);
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
        }
    }
}