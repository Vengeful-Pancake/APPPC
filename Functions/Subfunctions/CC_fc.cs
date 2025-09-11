using APPPC.Control;
using ClosedXML.Excel;
using System.Data;

namespace APPPC.CC_Helpers
{
    public static class ExportHelper
    {

        public static void ExportToExcel(int year, int month, int standardDays)
        {
            var users = SQL.GetUsers();
            var workData = SQL.GetWorkData();

            string templatePath = "Template.xlsx";
            string datestamp = DateTime.Now.ToString("ddMMyy-HHmmss");

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveFileDialog.Title = "Chọn nơi lưu file";
                saveFileDialog.FileName = $"Giờ Công Tháng {month}-{year}.xlsx";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return; // User cancelled
                }

                string outputPath = saveFileDialog.FileName;

                File.Copy(templatePath, outputPath, true);

                // Continue with the rest of your code using outputPath
                using (var workbook = new XLWorkbook(outputPath))
                {
                    var ws = workbook.Worksheet("Giờ công");
                    var ws2 = workbook.Worksheets.Contains("Làm thêm") ? workbook.Worksheet("Làm thêm") : null;

                    var weekdayMap = new Dictionary<DayOfWeek, string>
                    {
                        [DayOfWeek.Monday] = "T2",
                        [DayOfWeek.Tuesday] = "T3",
                        [DayOfWeek.Wednesday] = "T4",
                        [DayOfWeek.Thursday] = "T5",
                        [DayOfWeek.Friday] = "T6",
                        [DayOfWeek.Saturday] = "T7",
                        [DayOfWeek.Sunday] = "CN"
                    };

                    int totalDays = DateTime.DaysInMonth(year, month);
                    int startCol = 6; // Column F

                    for (int day = 1; day <= totalDays; day++)
                    {
                        var date = new DateTime(year, month, day);
                        string label = weekdayMap[date.DayOfWeek];
                        ws.Cell(4, startCol + day - 1).Value = label;
                        ws2.Cell(4, startCol + day - 1).Value = label;
                    }

                    int rowIndex = 5;
                    int stt = 1;

                    foreach (var user in users)
                    {
                        string msnv = user.Msnv.PadLeft(3, '0');
                        ws.Cell(rowIndex, 1).Value = stt++;
                        ws.Cell(rowIndex, 2).Value = user.Nhom;
                        ws.Cell(rowIndex, 3).Value = msnv;
                        ws.Cell(rowIndex, 4).Value = user.Hoten;
                        ws.Cell(rowIndex, 5).Value = user.Chucvu;

                        ws2.Cell(rowIndex, 2).Value = user.Nhom;
                        ws2.Cell(rowIndex, 3).Value = msnv;
                        ws2.Cell(rowIndex, 4).Value = user.Hoten;
                        ws2.Cell(rowIndex, 5).Value = user.Chucvu;

                        float totalHour = 0f, totalExtra = 0f, totalP = 0, totalVR = 0, totalTU = 0, totalBH = 0, totalAbsent = 0;
                        float totalDaysWorked = totalHour / 8;
                        float totalExtraDays = totalExtra / 8;
                        float totalCombined = totalDaysWorked + totalExtraDays;
                        float ngayCC = Math.Min(totalCombined, standardDays);
                        float ngayThem = totalCombined - ngayCC;
                        int zeroWorkdays = 0;
                        int AnKD = 0;
                        for (int day = 1; day <= totalDays; day++)
                        {
                            var date = new DateTime(year, month, day);
                            string dateStr = date.ToString("yyyy-MM-dd");
                            var work = workData.FirstOrDefault(w => w.Msnv == user.Msnv && w.Date == dateStr);

                            float wh = work?.WorkHour ?? 0f;
                            float eh = work?.ExtraWork ?? 0f;
                            string status = work?.Absent?.Trim().ToUpper(); // check loại vắng

                            var cell1 = ws.Cell(rowIndex, startCol + day - 1);
                            var cell2 = ws2.Cell(rowIndex, startCol + day - 1);


                            decimal days = Math.Round((decimal)wh / 8m, 2, MidpointRounding.AwayFromZero);
                            decimal extra = Math.Round((decimal)eh, 2, MidpointRounding.AwayFromZero);

                            // clear any old text content
                            cell1.Clear(XLClearOptions.Contents);
                            cell1.SetValue(days);                       // writes a number
                            cell1.Style.NumberFormat.SetFormat("0.##"); // shows 1, 1.5, 0.75 etc.

                            cell2.Clear(XLClearOptions.Contents);
                            cell2.SetValue(extra);
                            cell2.Style.NumberFormat.SetFormat("0.##");


                            // Highlight Sundays in light gray
                            if (date.DayOfWeek == DayOfWeek.Sunday)
                            {
                                cell1.Style.Fill.BackgroundColor = XLColor.LightGray;
                                cell2.Style.Fill.BackgroundColor = XLColor.LightGray;
                            }
                            else 
                            {
                                bool hasData = workData.Any(w => w.Msnv == user.Msnv && w.Date == dateStr && w.WorkHour > 0);
                                if (!hasData && string.IsNullOrEmpty(status))
                                    zeroWorkdays++;
                            }

                            // Highlight based on Absent status
                            if (!string.IsNullOrEmpty(status))
                            {
                                var color = status switch
                                {
                                    "P" => XLColor.FromHtml("#FFFF00"),
                                    "VR" => XLColor.FromHtml("#00FFFF"),
                                    "TU" => XLColor.FromHtml("#FF00FF"),
                                    "BH" => XLColor.FromHtml("#FF0000"),
                                    _ => null
                                };

                                if (color != null)
                                {
                                    cell1.Style.Fill.BackgroundColor = color;
                                    cell2.Style.Fill.BackgroundColor = color;
                                }
                                if (status == "P") { totalP += 1; }
                                else if (status == "VR") { totalVR += 1; totalAbsent += 1; }
                                else if (status == "TU") { totalTU += 1; }
                                else if (status == "BH") { totalBH += 1; totalAbsent += 1; }
                            }

                            totalHour += wh;
                            totalExtra += eh;
                        
                            if ((wh + eh) >= 11.5f)
                            {
                                AnKD++;
                            }
                        }
                        ws.Cell(rowIndex, 37).Value = totalDaysWorked;
                        ws.Cell(rowIndex, 38).Value = totalExtra;
                        ws.Cell(rowIndex, 39).Value = totalExtraDays;
                        ws.Cell(rowIndex, 40).Value = totalCombined;
                        ws.Cell(rowIndex, 41).Value = ngayCC;
                        ws.Cell(rowIndex, 42).Value = ngayThem;
                        ws.Cell(rowIndex, 43).Value = totalVR;
                        ws.Cell(rowIndex, 44).Value = totalBH;
                        ws.Cell(rowIndex, 45).Value = totalTU;
                        ws.Cell(rowIndex, 46).Value = totalP;


                        ws2.Cell(rowIndex, 37).Value = totalDaysWorked;
                        ws2.Cell(rowIndex, 38).Value = totalExtra;
                        ws2.Cell(rowIndex, 39).Value = totalExtraDays;
                        ws2.Cell(rowIndex, 40).Value = totalCombined;
                        ws2.Cell(rowIndex, 41).Value = ngayCC;
                        ws2.Cell(rowIndex, 42).Value = ngayThem;

                        ws.Cell(rowIndex, 47).Value = AnKD;
                        string anAnAnColLetter = XLHelper.GetColumnLetterFromNumber(48); // AnAnAn = 48

                        ws.Cell(rowIndex, 49).FormulaA1 = $"=ROUND({totalHour / 8}, 0)-{anAnAnColLetter}{rowIndex}";
                        ws2.Cell(rowIndex, 49).FormulaA1 = $"=ROUND({totalHour / 8}, 0)-{anAnAnColLetter}{rowIndex}";

                        ws.Cell(rowIndex, 50).Value = totalAbsent;
                        if (totalVR > 0)
                            ws.Cell(rowIndex, 43).Style.Fill.BackgroundColor = XLColor.FromHtml("#00FFFF"); // VR - Cyan

                        if (totalBH > 0)
                            ws.Cell(rowIndex, 44).Style.Fill.BackgroundColor = XLColor.FromHtml("#FF0000"); // BH - Red

                        if (totalTU > 0)
                            ws.Cell(rowIndex, 45).Style.Fill.BackgroundColor = XLColor.FromHtml("#FF00FF"); // TU - Magenta

                        if (totalP > 0)
                            ws.Cell(rowIndex, 46).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFFF00"); // P - Yellow



                        if (zeroWorkdays > 0)
                        {
                            ws.Cell(rowIndex, 52).Value = "Có ngày chưa chấm!";
                        }

                        rowIndex++;
                    }

                    ws.Cell(2, 4).Value = standardDays;
                    ws2.Cell(2, 4).Value = standardDays;


                    // Delete columns based on number of days in the month
                    if (totalDays == 30)
                    {
                        ws.Column(36).Delete();
                        ws2?.Column(36).Delete();
                    }
                    else if (totalDays == 29)
                    {
                        ws.Column(36).Delete();
                        ws.Column(35).Delete();
                        ws2?.Column(36).Delete();
                        ws2?.Column(35).Delete();
                    }
                    else if (totalDays == 28)
                    {
                        ws.Column(36).Delete();
                        ws.Column(35).Delete();
                        ws.Column(34).Delete();
                        ws2?.Column(36).Delete();
                        ws2?.Column(35).Delete();
                        ws2?.Column(34).Delete();
                    }
                    ws.Range("D1:X1").Merge();
                    ws2.Range("D1:X1").Merge();
                    ws.Cell("D1").Value = $"Ngày Công Tháng {month} - {year}";
                    ws2.Cell("D1").Value = $"Giờ Thêm Tháng {month} - {year}";
                    ws.Cell("D1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    ws2.Cell("D1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    workbook.Save();
                }

                MessageBox.Show("Đã xuất file thành công:\n" + outputPath);
            }


            
        }

        public static void ExportNSToExcel(DateTimePicker dateTimePicker1)
        {
            int year = dateTimePicker1.Value.Year;
            int month = dateTimePicker1.Value.Month;

            var workData = SQL.GetWorkData()
                .Where(w =>
                {
                    if (DateTime.TryParse(w.Date, out DateTime parsedDate))
                    {
                        return parsedDate.Year == year && parsedDate.Month == month;
                    }
                    return false;
                }).ToList();

            if (workData.Count == 0)
            {
                MessageBox.Show($"Không có dữ liệu chấm công cho tháng {month}/{year}.");
                return;
            }
            // Use SaveFileDialog
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveFileDialog.Title = "Chọn nơi lưu file";
                saveFileDialog.FileName = $"NS-{month:00}-{year}.xlsx";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string outputPath = saveFileDialog.FileName;

                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Work Summary");
                        ws.Cell(1, 1).Value = "MSNV";
                        ws.Cell(1, 2).Value = "Ngày";
                        ws.Cell(1, 3).Value = "TG LV";
                        ws.Cell(1, 4).Value = "Vắng";

                        int row = 2;
                        int WorkingHourTotal;
                        int[] VPMSNV = [1, 47, 24, 27, 29, 31, 35, 42, 52, 66, 79, 101, 115, 172, 176, 181, 183, 184, 196, 205, 208, 211];
                        foreach (var w in workData)
                        {
                            int msnv;
                            if (int.TryParse(w.Msnv?.Trim(), out msnv) && !VPMSNV.Contains(msnv))
                            {
                                ws.Cell(row, 1).Value = msnv;

                                if (DateTime.TryParse(w.Date?.ToString(), out DateTime parsedDate))
                                {
                                    ws.Cell(row, 2).Value = parsedDate;
                                    ws.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yy";
                                }
                                else
                                {
                                    ws.Cell(row, 2).Value = w.Date;
                                }

                                double wh = w.WorkHour ?? 0;
                                double extra = w.ExtraWork ?? 0;
                                ws.Cell(row, 3).Value = wh + extra;
                                ws.Cell(row, 4).Value = w.Absent ?? "";
                                row++;
                            }

                        }

                        workbook.SaveAs(outputPath);
                    }

                    MessageBox.Show($"Đã xuất dữ liệu cho tháng {month}/{year}:\n" + outputPath);
                }
            }
        }
    }
}
