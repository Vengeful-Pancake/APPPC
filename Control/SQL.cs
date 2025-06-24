using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using static APPPC.Control.SQL;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace APPPC.Control
{
    public class Session
    {
        public static SQL.User CurrentUser { get; set; }
        public static List<SQL.User> User_s {  get; set; }
        public static List<WorkSummary> CalculateMonthlySummary(DateTime targetMonth)
        {
            var summaries = new List<WorkSummary>();
            var workData = SQL.GetWorkData();

            foreach (var user in Session.User_s)
            {
                var userWork = workData.FindAll(w =>
                    w.Msnv == user.Msnv &&
                    DateTime.TryParse(w.Date, out DateTime d) &&
                    d.Month == targetMonth.Month &&
                    d.Year == targetMonth.Year
                );

                float totalWorkHour = 0f;
                float totalExtraHour = 0f;
                int absentDays = 0;

                foreach (var work in userWork)
                {
                    totalWorkHour += work.WorkHour ?? 0f;
                    totalExtraHour += work.ExtraWork ?? 0f;

                    if (!string.IsNullOrWhiteSpace(work.Absent))
                        absentDays++;
                }

                summaries.Add(new WorkSummary
                {
                    Msnv = user.Msnv,
                    Hoten = user.Hoten,
                    TotalWorkHour = totalWorkHour,
                    TotalExtraHour = totalExtraHour,
                    AbsentDays = absentDays
                });
            }

            return summaries;
        }

    }
    public class SQL
    {

		public static string PostGreSQLConnectionString = "Host=192.168.1.12;Port=5432;Username=odoo-read;Password=lbgUfH,f6#-SkA#;Database=QLSX3";

        public static string GetConnectionString()
        {

            //return "Server=192.168.1.27,1433;Database=master;User Id=myuser;Password=mypassword;TrustServerCertificate=True;";
            return "Server=192.168.1.12,1376;Database=chamcong;User Id=chamcong_app1;Password=Cevr9sKBBRRXnbZ;TrustServerCertificate=True;";

        }


        public class WorkSummary
        {
            public string Msnv { get; set; }
            public string Hoten { get; set; }
            public float TotalWorkHour { get; set; }
            public float TotalExtraHour { get; set; }
            public int AbsentDays { get; set; }
        }

        public class User
        {
            public string Msnv, Hoten, Chucvu, Quyenhan, Tonhom, Nhom, Taikhoan, Matkhau, NghiViec;
            public User(string msnv, string hoten, string chucvu, string quyenhan, string tonhom, string nhom, string taikhoan, string matkhau)
            {
                Msnv = msnv; Hoten = hoten; Chucvu = chucvu; Quyenhan = quyenhan;
                Tonhom = tonhom; Nhom = nhom; Taikhoan = taikhoan; Matkhau = matkhau;
            }
        }

        public class Work
        {
            public string Msnv, Date, Note, Absent;
            public float? WorkHour, ExtraWork;

            public Work(string msnv, object workHour, string date, object extraWork, string note, string absent)
            {
                Msnv = msnv;
                WorkHour = float.TryParse(workHour?.ToString(), out float val) ? val : (float?)null;
                Date = date;
                ExtraWork = float.TryParse(extraWork?.ToString(), out float val2) ? val2 : (float?)null;
                Note = note;
                try
                {
                    if (absent != null)
                    {
                        Absent = absent.Replace(" ", "");
                    }
                }
                catch(Exception e) { }
            }
        }

        public static List<User> GetMainUsers()
        {
            var users = new List<User>();
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
            SELECT 
                MSNV, HoTen, ChucVu, QuyenHan, ToNhom,
                CASE 
                    WHEN QuyenHan > 3 AND Nhom_OVW IS NOT NULL THEN Nhom_OVW 
                    ELSE Nhom 
                END AS NhomFinal,
                TaiKhoan, MatKhau, NghiViec
            FROM Users
            ORDER BY MSNV", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bool nghiViec = !reader.IsDBNull(8) && reader.GetBoolean(8);

                        if (!nghiViec)
                        {
                            users.Add(new User(
                                reader.GetInt16(0).ToString(),      // MSNV
                                reader.GetString(1),                // HoTen
                                reader.GetString(2),                // ChucVu
                                reader.GetByte(3).ToString(),       // QuyenHan
                                reader.GetString(4),                // ToNhom
                                reader.GetByte(5).ToString(),       // NhomFinal
                                reader.GetString(6),                // TaiKhoan
                                reader.GetString(7)                 // MatKhau
                            ));
                        }
                    }
                }
            }
            return users;
        }
        public static List<User> GetUsers()
        {
            var users = new List<User>();
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT MSNV, HoTen, ChucVu, QuyenHan, ToNhom, Nhom, TaiKhoan, MatKhau, NghiViec FROM Users ORDER BY MSNV", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bool nghiViec = !reader.IsDBNull(8) && reader.GetBoolean(8); // Safely read NghiViec

                        if (!nghiViec) // Only add active users
                        {
                            users.Add(new User(
                                reader.GetInt16(0).ToString(),      // MSNV (smallint)
                                reader.GetString(1),                // HoTen
                                reader.GetString(2),                // ChucVu
                                reader.GetByte(3).ToString(),       // QuyenHan (tinyint)
                                reader.GetString(4),                // ToNhom
                                reader.GetByte(5).ToString(),       // Nhom (tinyint)
                                reader.GetString(6),                // TaiKhoan
                                reader.GetString(7)                 // MatKhau
                            ));
                        }
                    }
                }
            }
            return users;
        }

        public static List<Work> GetWorkData()
        {
            var workData = new List<Work>();
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT MSNV, WorkHour, CONVERT(VARCHAR, WorkDate, 23), ExtraHour, Note, Absent FROM Work", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        workData.Add(new Work(
                            reader.GetString(0), // MSNV
                            reader.IsDBNull(1) ? (float?)null : Convert.ToSingle(reader[1]), // WorkHour
                            reader.GetString(2), // WorkDate as string
                            reader.IsDBNull(3) ? (float?)null : Convert.ToSingle(reader[3]), // ExtraHour
                            reader.IsDBNull(4) ? null : reader.GetString(4), // Note
                            reader.IsDBNull(5) ? null : reader.GetString(5)  // Absent
                        ));
                    }

                }
            }
            return workData;
        }

        public static void SaveValue(string key, string value)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string cumsnv = Session.CurrentUser.Msnv.ToString();

                // ⚠️ Cẩn trọng với key để tránh SQL Injection
                string sql = $"UPDATE Users SET {key} = {value} WHERE MSNV = {cumsnv}";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@value", value);
                    cmd.Parameters.AddWithValue("@cumsnv", cumsnv);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void SaveUserValue(string key, string value, string msnv)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                // Use parameterized query to avoid SQL injection
                string sql = $"UPDATE Users SET {key} = @value WHERE MSNV = @msnv";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@value", value);
                    cmd.Parameters.AddWithValue("@msnv", msnv);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void SaveYeuCauDates(string lsx, string ngayYeuCau, string ngayYeuCauThucTe)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = @"
            MERGE INTO YCSX AS Target
            USING (SELECT @lsx AS LSX) AS Source
            ON Target.LSX = Source.LSX
            WHEN MATCHED THEN
                UPDATE SET Date1 = @date1, Date2 = @date2
            WHEN NOT MATCHED THEN
                INSERT (LSX, Date1, Date2) VALUES (@lsx, @date1, @date2);";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@lsx", lsx ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@date1", string.IsNullOrWhiteSpace(ngayYeuCau) ? (object)DBNull.Value : ngayYeuCau);
                    cmd.Parameters.AddWithValue("@date2", string.IsNullOrWhiteSpace(ngayYeuCauThucTe) ? (object)DBNull.Value : ngayYeuCauThucTe);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static Dictionary<string, (string Date1, string Date2)> LoadYeuCauDates()
        {
            var result = new Dictionary<string, (string, string)>();

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT LSX, Date1, Date2 FROM YCSX", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string rawLsx = reader["LSX"]?.ToString() ?? "";
                        string cleanedLsx = rawLsx.Trim().Replace(" ", "").Replace("\u200B", "");

                        string date1 = reader["Date1"]?.ToString();
                        string date2 = reader["Date2"]?.ToString();

                        if (!string.IsNullOrEmpty(cleanedLsx) && !result.ContainsKey(cleanedLsx))
                        {
                            result[cleanedLsx] = (date1, date2);
                        }
                    }
                }
            }

            return result;
        }
    }
}
