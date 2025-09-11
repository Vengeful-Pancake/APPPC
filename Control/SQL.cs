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

                    if (work.Absent == "VR" || work.Absent == "BH")
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

#if DEBUG

        public static string PostGreSQLConnectionString = "Host=192.168.1.12;Port=5432;Username=odoo;Password=2c-*kw?SG*fXv3pX;Database=QLSX3";
#else
        
        public static string PostGreSQLConnectionString = "Host=192.168.1.12;Port=5432;Username=odoo-read;Password=lbgUfH,f6#-SkA#;Database=QLSX3";
#endif
        public static string GetConnectionString()
        {

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

        public static int SaveShiftDays(DataTable tab4)
        {
            int total = 0;
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    foreach (DataRow r in tab4.Rows)
                    {
                        var date = r.Field<DateTime>("DateOnly").Date;
                        string? ka = r["ka"] == DBNull.Value ? null : r["ka"]?.ToString();
                        string? note = r["note"] == DBNull.Value ? null : r["note"]?.ToString();
                        string? mach = r.Table.Columns.Contains("Machine") && r["Machine"] != DBNull.Value
                                       ? r["Machine"]?.ToString()
                                       : null;
                        if (string.IsNullOrWhiteSpace(mach)) continue;

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.CommandText = @"
                                IF EXISTS (SELECT 1 FROM dbo.Shift WITH (UPDLOCK, HOLDLOCK) WHERE [Date]=@d AND [Machine]=@m)
                                BEGIN
                                    UPDATE dbo.Shift
                                      SET [Shift]=@s, [Note]=COALESCE(@n,[Note])
                                    WHERE [Date]=@d AND [Machine]=@m;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO dbo.Shift ([Date],[Shift],[Note],[Machine])
                                    VALUES (@d,@s,@n,@m);
                                END";
                            cmd.Parameters.Add("@d", SqlDbType.Date).Value = date;
                            cmd.Parameters.Add("@s", SqlDbType.NVarChar, 50).Value = (object?)ka ?? DBNull.Value;
                            cmd.Parameters.Add("@n", SqlDbType.NVarChar, 255).Value =
                                string.IsNullOrWhiteSpace(note) ? (object)DBNull.Value : note;
                            cmd.Parameters.Add("@m", SqlDbType.NChar, 32).Value = mach.Trim();
                            total += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
            }
            return total;
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
                                reader.GetInt32(0).ToString(),      // MSNV
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
                                reader.GetInt32(0).ToString(),      // MSNV (smallint)
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
        // using Microsoft.Data.SqlClient;
        // using System.Data;

        public static Dictionary<string, string> GetLsxMachineMap()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var conn = new SqlConnection(GetConnectionString());
            conn.Open();

            // Prefer a row with Date NULL (i.e., base assignment) else the most recent dated row
            var sql = @"
                WITH ranked AS (
                    SELECT 
                        RTRIM(LSX) AS LSX,
                        RTRIM(Machine_ID) AS Machine_ID,
                        [Date],
                        ROW_NUMBER() OVER (
                            PARTITION BY LSX
                            ORDER BY CASE WHEN [Date] IS NULL THEN 0 ELSE 1 END, [Date] DESC
                        ) AS rn
                    FROM dbo.Machine
                    WHERE Machine_ID IS NOT NULL
                )
                SELECT LSX, Machine_ID
                FROM ranked
                WHERE rn = 1;";

            using var cmd = new SqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                var lsx = (rd["LSX"]?.ToString() ?? "").Trim();
                var mid = (rd["Machine_ID"]?.ToString() ?? "").Trim();
                if (!string.IsNullOrEmpty(lsx) && !map.ContainsKey(lsx))
                    map[lsx] = mid;
            }

            return map;
        }

        public static void SaveYeuCauDates(string lsx, string ngayBHYeuCau, string ngaySXYeuCau, string ngayTTYeuCau, string ghichu)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = @"
                    MERGE INTO YCSX AS Target
                    USING (SELECT @lsx AS LSX) AS Source
                    ON Target.LSX = Source.LSX
                    WHEN MATCHED THEN
                        UPDATE SET Date1 = @date1, Date2 = @date2, Date3 = @date3, Note = @note
                    WHEN NOT MATCHED THEN
                        INSERT (LSX, Date1, Date2, Date3, Note) VALUES (@lsx, @date1, @date2, @date3, @note);";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@lsx", lsx ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@date1", string.IsNullOrWhiteSpace(ngayBHYeuCau) ? (object)DBNull.Value : ngayBHYeuCau);
                    cmd.Parameters.AddWithValue("@date2", string.IsNullOrWhiteSpace(ngaySXYeuCau) ? (object)DBNull.Value : ngaySXYeuCau);
                    cmd.Parameters.AddWithValue("@date3", string.IsNullOrWhiteSpace(ngayTTYeuCau) ? (object)DBNull.Value : ngayTTYeuCau);
                    cmd.Parameters.AddWithValue("@note", string.IsNullOrWhiteSpace(ghichu) ? (object)DBNull.Value : ghichu);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        // ===== Helper to trim nchar padding =====
        private static string T(object v) => (v?.ToString() ?? "").Trim();

        // ====== TAB 2: Save LSX -> Machine_ID (upsert by LSX) ======
        public static void SaveWorkorderMachine(string lsx, string machineId)
        {
            if (string.IsNullOrWhiteSpace(lsx) || string.IsNullOrWhiteSpace(machineId)) return;

            using var conn = new SqlConnection(GetConnectionString());
            conn.Open();

            var sql = @"
                MERGE dbo.Machine AS t
                USING (SELECT @LSX AS LSX) AS s
                ON (t.LSX = s.LSX)
                WHEN MATCHED THEN UPDATE SET t.Machine_ID = @MID
                WHEN NOT MATCHED THEN INSERT (LSX, Machine_ID, [Date], [Order])
                        VALUES (@LSX, @MID, NULL, NULL);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@LSX", SqlDbType.NChar, 10).Value = lsx.Trim();
            cmd.Parameters.Add("@MID", SqlDbType.NChar, 10).Value = machineId.Trim();
            cmd.ExecuteNonQuery();
        }

        // ====== TAB 3: Load machine list for dropdown ======
        public static List<string> LoadMachineListForPlan()
        {
            var list = new List<string>();
            using var conn = new SqlConnection(GetConnectionString());
            conn.Open();

            // pick existing machines (you can replace with a catalog later)
            var sql = @"SELECT DISTINCT Machine_ID FROM dbo.Machine 
                WHERE Machine_ID IS NOT NULL 
                ORDER BY Machine_ID";   // sorted by Machine_ID
            using var cmd = new SqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) list.Add(T(rd[0]));
            return list;
        }

        // ====== TAB 3: Load brand-new daily plan (Machine + Date) ======
        public static DataTable LoadPlanForMachineDay(string machineId, DateTime day)
        {
            using var conn = new SqlConnection(GetConnectionString());
            var sql = @"
                SELECT
                  RTRIM(LSX)        AS LSX,
                  RTRIM(Machine_ID) AS Machine_ID,
                  CAST([Date] AS date) AS RawDate,
                  [Order]           AS sequence
                FROM dbo.Machine
                WHERE Machine_ID = @MID
                  AND ([Date] IS NULL OR CAST([Date] AS date) = @D)
                ORDER BY Machine_ID, CASE WHEN [Order] IS NULL THEN 9999 ELSE [Order] END, LSX;";

            using var da = new SqlDataAdapter(sql, conn);
            da.SelectCommand.Parameters.Add("@MID", SqlDbType.NChar, 10).Value = machineId.Trim();
            da.SelectCommand.Parameters.Add("@D", SqlDbType.Date).Value = day.Date;

            var dt = new DataTable();
            da.Fill(dt);
            if (!dt.Columns.Contains("sequence")) dt.Columns.Add("sequence", typeof(int));
            return dt;
        }

        // ====== TAB 3: Save plan (overwrite day for that machine, promote NULL-date rows) ======
        // Distinct planned dates to populate DateSorter for Tab 3
        public static List<DateTime> LoadPlanDates()
        {
            var list = new List<DateTime>();
            using var conn = new SqlConnection(GetConnectionString());
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT DISTINCT CAST([Date] AS date) AS d
                FROM dbo.Machine
                WHERE [Date] IS NOT NULL
                ORDER BY d;", conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read() && !rd.IsDBNull(0)) list.Add(rd.GetDateTime(0).Date);
            return list;
        }



        // Save per-row using each row's RawDate (if any). If checked with RawDate null,
        // it will be saved to 'fallbackDate' (the current starting date).
        public static void SavePlanUsingCheckbox(string machineId, DateTime fallbackDate, DataTable grid)
        {
            using var conn = new SqlConnection(GetConnectionString());
            conn.Open();
            using var tx = conn.BeginTransaction();

            foreach (DataRow r in grid.Rows)
            {
                string lsx = (r["LSX"]?.ToString() ?? "").Trim();
                if (string.IsNullOrEmpty(lsx)) continue;

                bool planned = r.Table.Columns.Contains("Date") && r["Date"] != DBNull.Value && Convert.ToBoolean(r["Date"]);
                short? seq = null;
                if (r["sequence"] != DBNull.Value && short.TryParse(r["sequence"].ToString(), out var s) && s > 0) seq = s;

                string ka = (r["ka"]?.ToString() ?? "").Trim();

                if (planned)
                {
                    DateTime useDate =
                        (r.Table.Columns.Contains("RawDate") && r["RawDate"] != DBNull.Value)
                        ? Convert.ToDateTime(r["RawDate"]).Date
                        : fallbackDate.Date;

                    using var up = new SqlCommand(@"
                MERGE dbo.Machine AS t
                USING (SELECT @LSX AS LSX) AS s ON t.LSX = s.LSX
                WHEN MATCHED THEN 
                    UPDATE SET t.Machine_ID=@MID, t.[Date]=@D, t.[Order]=@Seq,
                               t.Ka=@Ka, t.[new]=@NewDay, t.[skip]=@SkipDays
                WHEN NOT MATCHED THEN
                    INSERT (LSX, Machine_ID, [Date], [Order], Ka, [new], [skip])
                    VALUES (@LSX, @MID, @D, @Seq, @Ka, @NewDay, @SkipDays);", conn, tx);
                    up.Parameters.Add("@LSX", SqlDbType.NChar, 10).Value = lsx;
                    up.Parameters.Add("@MID", SqlDbType.NChar, 10).Value = machineId.Trim();
                    up.Parameters.Add("@D", SqlDbType.DateTime).Value = useDate;
                    up.Parameters.Add("@Seq", SqlDbType.SmallInt).Value = (object?)seq ?? DBNull.Value;
                    up.Parameters.Add("@Ka", SqlDbType.NVarChar, 30).Value = string.IsNullOrEmpty(ka) ? (object)DBNull.Value : ka;
                    up.ExecuteNonQuery();
                }
                else
                {
                    // Unplan but keep Ka/new/skip so we don't lose user's choice
                    using var up = new SqlCommand(@"
                UPDATE dbo.Machine 
                SET [Date]=NULL, [Order]=NULL, Ka=@Ka, [new]=@NewDay, [skip]=@SkipDays
                WHERE LSX=@LSX AND Machine_ID=@MID;", conn, tx);
                    up.Parameters.Add("@LSX", SqlDbType.NChar, 10).Value = lsx;
                    up.Parameters.Add("@MID", SqlDbType.NChar, 10).Value = machineId.Trim();
                    up.Parameters.Add("@Ka", SqlDbType.NVarChar, 30).Value = string.IsNullOrEmpty(ka) ? (object)DBNull.Value : ka;
                    up.ExecuteNonQuery();
                }
            }

            tx.Commit();
        }



        // Get rows of a machine whose [Date] is NULL OR >= (startDate+1), sorted by Machine_ID then Order
        public static DataTable LoadPlanForMachineFromDate(string machineId, DateTime startDate)
        {
            using var conn = new SqlConnection(GetConnectionString());
            var sql = @"
        SELECT
          RTRIM(LSX)        AS LSX,
          RTRIM(Machine_ID) AS Machine_ID,
          CAST([Date] AS date) AS RawDate,
          CAST([Order] AS smallint) AS sequence,
          RTRIM(ISNULL([Note], '')) AS note,
          RTRIM(ISNULL([Ka], ''))   AS ka
        FROM dbo.Machine
        WHERE Machine_ID = @MID
          AND ([Date] IS NULL OR CAST([Date] AS date) >= @D)
        ORDER BY Machine_ID,
                 CASE WHEN [Date] IS NULL THEN 0 ELSE 1 END,
                 CAST([Date] AS date),
                 CASE WHEN [Order] IS NULL THEN 9999 ELSE [Order] END,
                 LSX;";
            using var da = new SqlDataAdapter(sql, conn);
            da.SelectCommand.Parameters.Add("@MID", SqlDbType.NChar, 10).Value = machineId.Trim();
            da.SelectCommand.Parameters.Add("@D", SqlDbType.Date).Value = startDate.Date;

            var dt = new DataTable();
            da.Fill(dt);

            // make sure expected columns exist with correct types
            if (!dt.Columns.Contains("sequence")) dt.Columns.Add("sequence", typeof(short));
            if (!dt.Columns.Contains("ka")) dt.Columns.Add("ka", typeof(string));
            if (!dt.Columns.Contains("note")) dt.Columns.Add("note", typeof(string));

            return dt;
        }


        // Sum of sogio_can per LSX (Postgres)
        public static Dictionary<string, double> LoadTimeNeededForLsxs(IEnumerable<string?> lsxs)
        {
            var list = lsxs.Where(s => !string.IsNullOrWhiteSpace(s))
                           .Select(s => s!.Trim()).Distinct().ToList();
            var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            if (list.Count == 0) return result;

            var inVals = string.Join(",", list.Select((s, i) => $"@p{i}"));
            var sql = $@"
                SELECT mp.sophieu AS lsx, COALESCE(SUM(wo.sogio_can),0) AS hours
                FROM mrp_workorder wo
                JOIN mrp_production mp ON mp.id = wo.production_id
                WHERE mp.sophieu IN ({inVals})
                GROUP BY mp.sophieu;";

            using var conn = new NpgsqlConnection(PostGreSQLConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            for (int i = 0; i < list.Count; i++) cmd.Parameters.AddWithValue($"@p{i}", list[i]);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                var k = (rd["lsx"]?.ToString() ?? "").Trim();
                var v = rd.IsDBNull(1) ? 0.0 : Convert.ToDouble(rd[1]);
                if (!string.IsNullOrEmpty(k)) result[k] = v;
            }
            return result;
        }


        public static Dictionary<DateTime, (string Shift, string Note)> LoadShiftDays(
    string machineId, DateTime from, DateTime to)
        {
            var map = new Dictionary<DateTime, (string, string)>();
            if (string.IsNullOrWhiteSpace(machineId)) return map;

            using var conn = new SqlConnection(GetConnectionString());
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT CAST([Date] AS date) AS D,
                       RTRIM(COALESCE([Shift],'')) AS Shift,
                       RTRIM(COALESCE([Note],''))  AS Note
                FROM dbo.Shift
                WHERE [Machine] = @m
                  AND [Date] >= @from AND [Date] < DATEADD(day,1,@to);", conn);

            cmd.Parameters.Add("@m", SqlDbType.NChar, 32).Value = machineId.Trim();
            cmd.Parameters.Add("@from", SqlDbType.Date).Value = from.Date;
            cmd.Parameters.Add("@to", SqlDbType.Date).Value = to.Date;

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                var d = rd.GetDateTime(0).Date;
                var s = rd.IsDBNull(1) ? "" : rd.GetString(1);
                var n = rd.IsDBNull(2) ? "" : rd.GetString(2);
                map[d] = (s, n);
            }
            return map;
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
