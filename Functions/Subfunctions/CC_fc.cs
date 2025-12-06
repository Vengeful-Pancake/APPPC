using APPPC.Control;
using ClosedXML.Excel;
using System.Data;
using System.Globalization;



namespace APPPC.CC_Helpers
{
    public static class ExportHelper
    {

        private const double EPS = 1e-9;

        // Force US locale so decimal separator is "."
        private static string UsFmt(string fmt) => $"[$-409]{fmt}";

        // Integer-only writer
        private static void SetInteger(IXLCell cell, double value)
        {
            cell.Clear(XLClearOptions.Contents);
            cell.Value = value;
            cell.Style.NumberFormat.Format = UsFmt("0");
        }

        // Smart writer: 0 decimals if integer-like, otherwise 0.## with dot
        private static void SetNumericSmart(IXLCell cell, double value)
        {
            cell.Clear(XLClearOptions.Contents);
            cell.Value = value;

            // integer-like? (handles 1, 1.000000, etc.)
            bool isIntLike = Math.Abs(value - Math.Round(value, 0, MidpointRounding.AwayFromZero)) < EPS;

            cell.Style.NumberFormat.Format = isIntLike ? UsFmt("0") : UsFmt("0.##");
        }



        public static void ExportToExcel(int year, int month, int standardDays)
        {
            var users = SQL.GetUsers();
            var workData = SQL.GetWorkData();

            string templatePath = "Resources/Assets/Template.xlsx";

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveFileDialog.Title = "Chọn nơi lưu file";
                saveFileDialog.FileName = $"Giờ Công Tháng {month}-{year}.xlsx";

                if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

                string outputPath = saveFileDialog.FileName;
                File.Copy(templatePath, outputPath, true);

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
                    int startCol = 6; // F


                    // when setting headers
                    for (int day = 1; day <= totalDays; day++)
                    {
                        var date = new DateTime(year, month, day);
                        string label = weekdayMap[date.DayOfWeek];
                        ws.Cell(4, startCol + day - 1).Value = label;
                        if (ws2 != null) ws2.Cell(4, startCol + day - 1).Value = label;
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
                        int zeroWorkdays = 0;
                        int AnKD = 0;

                        for (int day = 1; day <= totalDays; day++)
                        {
                            var date = new DateTime(year, month, day);
                            string dateStr = date.ToString("yyyy-MM-dd");
                            var work = workData.FirstOrDefault(w => w.Msnv == user.Msnv && w.Date == dateStr);

                            float wh = work?.WorkHour ?? 0f;
                            float eh = work?.ExtraWork ?? 0f;
                            string status = work?.Absent?.Trim().ToUpper();

                            var cell1 = ws.Cell(rowIndex, startCol + day - 1);
                            var cell2 = ws2.Cell(rowIndex, startCol + day - 1);

                            // day values
                            double days = Math.Round(wh / 8.0, 2, MidpointRounding.AwayFromZero);
                            double extraH = Math.Round(eh, 2, MidpointRounding.AwayFromZero);

                            SetNumericSmart(cell1, days);
                            if (ws2 != null) SetNumericSmart(cell2, extraH);


                            // visual marks
                            if (date.DayOfWeek == DayOfWeek.Sunday)
                            {
                                cell1.Style.Fill.BackgroundColor = XLColor.LightGray;
                                cell2.Style.Fill.BackgroundColor = XLColor.LightGray;
                            }
                            else
                            {
                                bool hasData = workData.Any(w => w.Msnv == user.Msnv && w.Date == dateStr && w.WorkHour > 0);
                                if (!hasData && string.IsNullOrEmpty(status)) zeroWorkdays++;
                            }

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
                                if (status == "P") totalP += 1;
                                if (status == "VR") { totalVR += 1; totalAbsent += 1; }
                                if (status == "TU") totalTU += 1;
                                if (status == "BH") { totalBH += 1; totalAbsent += 1; }
                            }

                            totalHour += wh;
                            totalExtra += eh;

                            if ((wh + eh) >= 11.5f) AnKD++;
                        }

                        // 🔁 Recalculate AFTER the loop
                        double totalDaysWorked = totalHour / 8.0;
                        double totalExtraDays = totalExtra / 8.0;
                        double totalCombined = totalDaysWorked + totalExtraDays;
                        double ngayCC = Math.Min(totalCombined, standardDays);
                        double ngayThem = totalCombined - ngayCC;

                        // decimals or integers depending on value
                        SetNumericSmart(ws.Cell(rowIndex, 37), totalDaysWorked);
                        SetNumericSmart(ws.Cell(rowIndex, 38), totalExtra);
                        SetNumericSmart(ws.Cell(rowIndex, 39), totalExtraDays);
                        SetNumericSmart(ws.Cell(rowIndex, 40), totalCombined);
                        SetNumericSmart(ws.Cell(rowIndex, 41), ngayCC);
                        SetNumericSmart(ws.Cell(rowIndex, 42), ngayThem);

                        if (ws2 != null)
                        {
                            SetNumericSmart(ws2.Cell(rowIndex, 37), totalDaysWorked);
                            SetNumericSmart(ws2.Cell(rowIndex, 38), totalExtra);
                            SetNumericSmart(ws2.Cell(rowIndex, 39), totalExtraDays);
                            SetNumericSmart(ws2.Cell(rowIndex, 40), totalCombined);
                            SetNumericSmart(ws2.Cell(rowIndex, 41), ngayCC);
                            SetNumericSmart(ws2.Cell(rowIndex, 42), ngayThem);
                        }

                        int roundedDays = (int)Math.Round(totalHour / 8.0, MidpointRounding.AwayFromZero);

                        // col 48 may be input/another value; treat empty as 0
                        int col48_ws = ws.Cell(rowIndex, 48).TryGetValue<double>(out var v48a) ? (int)Math.Round(v48a) : 0;
                        int col48_ws2 = ws2.Cell(rowIndex, 48).TryGetValue<double>(out var v48b) ? (int)Math.Round(v48b) : 0;

                        int col49_ws = roundedDays - col48_ws;
                        int col49_ws2 = roundedDays - col48_ws2;

                        SetInteger(ws.Cell(rowIndex, 43), totalVR);
                        SetInteger(ws.Cell(rowIndex, 44), totalBH);
                        SetInteger(ws.Cell(rowIndex, 45), totalTU);
                        SetInteger(ws.Cell(rowIndex, 46), totalP);
                        SetInteger(ws.Cell(rowIndex, 47), AnKD);
                        SetInteger(ws.Cell(rowIndex, 49), col49_ws);
                        SetInteger(ws.Cell(rowIndex, 50), totalAbsent);

                        if (ws2 != null) SetInteger(ws2.Cell(rowIndex, 49), col49_ws2);


                        if (totalVR > 0) ws.Cell(rowIndex, 43).Style.Fill.BackgroundColor = XLColor.FromHtml("#00FFFF");
                        if (totalBH > 0) ws.Cell(rowIndex, 44).Style.Fill.BackgroundColor = XLColor.FromHtml("#FF0000");
                        if (totalTU > 0) ws.Cell(rowIndex, 45).Style.Fill.BackgroundColor = XLColor.FromHtml("#FF00FF");

                        if (zeroWorkdays > 0) ws.Cell(rowIndex, 52).Value = "Có ngày chưa chấm!";

                        rowIndex++;
                    }

                    ws.Cell(2, 4).Value = standardDays;
                    ws2.Cell(2, 4).Value = standardDays;

                    // … (the rest of your column deletion & header code stays the same)

                    workbook.Save();
                }

                MessageBox.Show("Đã xuất file thành công:\n" + outputPath);
            }
        }

        // ExportNSToExcel(...) unchanged


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
