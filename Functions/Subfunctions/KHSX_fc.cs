using APPPC.Control;
using Npgsql;
using System.Data;
using System.Globalization;
using ClosedXML.Excel;

namespace APPPC.KHSX_Helpers
{
    public static class WorkorderService
    {
        


        public static string OrderMap(string WOName, int index)
        {
            // You can later replace this with a real dictionary or logic based on DB
            var mockMap = new Dictionary<string, string[]>
            {
                ["Bế"] = new[] { "Máy Bế 1", "Máy Bế 2", "Máy Bế Tay 1", "Máy Ép Kim" },
                ["Dán"] = new[] { "Máy Dán 1", "Máy Dán 2" },
                ["Tráng Màng"] = new[] { "Tráng Màng 1", "Tráng Màng 2" },
                ["In"] = new[] { "In 1", "In 2", "Flexo", "In KTS", "Flexo 1", "Flexo 2" }
            };

            if (mockMap.TryGetValue(WOName, out var list) && index >= 1 && index <= list.Length)
                return list[index - 1];

            return $"{index}";
        }
        public static void arranger(DataGridView dataGridView2)
        {
            string[] orderedColumns = {
                "lsx","product_code","product_name","company","workorder_name","routing_name",
                "date_planned_start","date_planned_finished","production_qty","can_sx",
                "order_name","state","desire","checker","extra",
                "baighep","ngay_co_nl","machine","ghichu"
            };



            int displayIndex = 0;
            foreach (string colName in orderedColumns)
            {
                if (dataGridView2.Columns.Contains(colName))
                {
                    dataGridView2.Columns[colName].DisplayIndex = displayIndex;
                    displayIndex += 1;
                }
            }
        }
        private const string GhepBaiPath = @"\\dongau-nas\KHSX\DA\LCC\ghepbai.xlsx";
        private const string LccPath = @"\\dongau-nas\KHSX\DA\LCC\LCC.xlsx";
        private const string SheetGhep = "Lưu bài ghép";
        private const string SheetPHNL = "PHNL";

        private static Dictionary<string, string> _bgCache;
        private static DateTime _bgMtimeUtc;
        private static Dictionary<string, string> _nlCache;
        private static DateTime _nlMtimeUtc;

        private static string Norm(string s) =>
            (s ?? "").Trim().Replace(" ", "").Replace("\u200B", "");

        // Prefer-rightmost version
        private static int FindColumn(IXLWorksheet ws, string headerText, bool preferRightmost = false)
        {
            var header = ws.FirstRowUsed();
            int found = -1;
            foreach (var cell in header.CellsUsed())
            {
                var txt = cell.GetString().Trim();
                if (txt.Equals(headerText, StringComparison.OrdinalIgnoreCase))
                {
                    if (preferRightmost) found = cell.Address.ColumnNumber;     // keep updating → rightmost
                    else if (found < 0) found = cell.Address.ColumnNumber;     // first hit → leftmost
                }
            }
            return found;
        }


        // ---- Map LSX -> "Bài ghép" (sheet Lưu bài ghép, cột Ghi chú). Không có => "Lẻ"
        public static Dictionary<string, string> GetBaiGhepMap()
        {
            var mtime = File.Exists(GhepBaiPath) ? File.GetLastWriteTimeUtc(GhepBaiPath) : DateTime.MinValue;
            if (_bgCache != null && _bgMtimeUtc == mtime) return _bgCache;

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(GhepBaiPath)) { _bgCache = map; _bgMtimeUtc = mtime; return map; }

            using var wb = new XLWorkbook(GhepBaiPath);
            var ws = wb.Worksheet(SheetGhep);

            // Locate LSX and the *first* “Ghi chú” header
            int colLsx = FindColumn(ws, "LSX"); if (colLsx < 1) colLsx = 1;     // A
            int colGhiChu = FindColumn(ws, "Ghi chú");  // may be R in your file
                                                        // The desired data is the column right after “Ghi chú”
            int colNote = (colGhiChu > 0) ? colGhiChu + 1 : 19; // default to S if not found

            foreach (var row in ws.RowsUsed().Skip(1))
            {
                var key = (row.Cell(colLsx).GetString() ?? "")
                          .Trim().Replace(" ", "").Replace("\u200B", "");
                if (string.IsNullOrEmpty(key)) continue;

                // Read the cell *next to* “Ghi chú”
                var val = row.Cell(colNote).GetString().Trim();

                map[key] = string.IsNullOrWhiteSpace(val) ? "Lẻ" : val;
            }

            _bgCache = map; _bgMtimeUtc = mtime;
            return map;
        }


        // ---- Map LSX -> "Ngày có NL" (sheet PHNL, cột Ngày có NL), ưu tiên NL chính
        public static Dictionary<string, string> GetNgayCoNlMap()
        {
            var mtime = File.Exists(LccPath) ? File.GetLastWriteTimeUtc(LccPath) : DateTime.MinValue;
            if (_nlCache != null && _nlMtimeUtc == mtime) return _nlCache;

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(LccPath)) { _nlCache = map; _nlMtimeUtc = mtime; return map; }

            using var wb = new XLWorkbook(LccPath);
            var ws = wb.Worksheet(SheetPHNL);

            int colLsx = FindColumn(ws, "Số lệnh SX"); if (colLsx < 1) colLsx = 1;  // A fallback
            int colNgay = FindColumn(ws, "Ngày có NL"); if (colNgay < 1) colNgay = 8; // H fallback
            int colTK = FindColumn(ws, "TK");          // có thì ưu tiên vật tư chính theo TK

            // whitelist vật tư chính hay gặp (chị chỉnh thêm nếu muốn)
            string[] tkChinh = { "Ivory", "Fort", "Kraft", "trắng mờ", "Trắng mờ" };

            // 1st pass: chỉ lấy hàng có TK thuộc danh sách chính
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                var key = Norm(row.Cell(colLsx).GetString());
                if (string.IsNullOrEmpty(key)) continue;

                if (colTK > 0)
                {
                    var tk = row.Cell(colTK).GetString();
                    if (!tkChinh.Any(t => tk.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0))
                        continue;
                }

                var val = row.Cell(colNgay).GetString().Trim();
                if (!map.ContainsKey(key) && !string.IsNullOrEmpty(val))
                    map[key] = val;
            }

            // 2nd pass fallback: nếu chưa có, lấy dòng đầu tiên bất kỳ
            if (map.Count == 0)
            {
                foreach (var row in ws.RowsUsed().Skip(1))
                {
                    var key = Norm(row.Cell(colLsx).GetString());
                    if (string.IsNullOrEmpty(key)) continue;
                    var val = row.Cell(colNgay).GetString().Trim();
                    if (!map.ContainsKey(key) && !string.IsNullOrEmpty(val))
                        map[key] = val;
                }
            }

            _nlCache = map; _nlMtimeUtc = mtime;
            return map;
        }

        public static DataTable LoadPendingWorkorders(
            DataGridView dataGridView1,
            DataGridView dataGridView2,
            CheckBox ShowEmpty,
            CheckBox ShowOld,
            ComboBox DateSorter,
            int modetab)
        {
            dataGridView2.SuspendLayout();


            try
            {
                DataTable allWorkorders;
                string Query = @"
WITH base AS (
  SELECT mp.id AS production_id, mp.sophieu AS lsx, mp.product_id, mp.product_qty AS mo_qty
  FROM mrp_production mp
),
pick AS (
  /* aggregate once per production_id — much faster than DISTINCT ON + windows */
  SELECT
    sm.production_id,
    MIN(COALESCE(sm.date_expected, sm.date, sp.date_done))::date AS giao_date,
    SUM(sm.product_uom_qty) AS move_qty,
    MIN(sp.origin) AS so_name
  FROM stock_move sm
  LEFT JOIN stock_picking sp ON sp.id = sm.picking_id
  WHERE sm.production_id IS NOT NULL
    AND (sm.state IS NULL OR sm.state <> 'cancel')
  GROUP BY sm.production_id
),
sol AS (
  SELECT so.name AS so_name, sol.product_id, SUM(sol.product_uom_qty) AS so_line_qty
  FROM sale_order_line sol
  JOIN sale_order so ON so.id = sol.order_id
  GROUP BY so.name, sol.product_id
),                            -- <<< đóng ) và thêm dấu phẩy
done AS (
  SELECT wo.production_id, COALESCE(SUM(wo.qty_produced), 0) AS qty_done
  FROM mrp_workorder wo
  GROUP BY wo.production_id
),
ranked AS (
  SELECT
    mp.sophieu AS lsx,
    r.name     AS routing_name,
    COALESCE(NULLIF(wo.routing_equip_name,''),'Thành Phẩm') AS workorder_name,
    CAST(wo.date_planned_start AS date) AS date_planned_start,
    wo.state,
    pick.so_name AS order_name,
    COALESCE(sol.so_line_qty, pick.move_qty, 0) AS production_qty,
    pick.giao_date AS date_planned_finished,
    GREATEST(COALESCE(sol.so_line_qty, pick.move_qty, 0) - COALESCE(d.qty_done, 0), 0) AS can_sx,
    bom.note AS note
    ,
    ROW_NUMBER() OVER (
      PARTITION BY mp.sophieu
      ORDER BY CASE wo.state WHEN 'ready' THEN 0 WHEN 'pending' THEN 1 ELSE 2 END,
               wo.date_planned_start DESC
    ) AS rn
  FROM mrp_production_data pd
  JOIN mrp_workorder  wo ON pd.production_id = wo.production_id
  JOIN mrp_production mp ON pd.production_id = mp.id
  JOIN mrp_routing    r  ON mp.routing_id    = r.id
  LEFT JOIN mrp_bom   bom ON bom.id = mp.bom_id
  LEFT JOIN base  b   ON b.production_id  = mp.id
  LEFT JOIN pick  pick ON pick.production_id = mp.id
  LEFT JOIN sol   sol  ON sol.so_name = pick.so_name AND sol.product_id = b.product_id
  LEFT JOIN done  d    ON d.production_id = mp.id
  WHERE wo.state <> 'cancel'
    AND (@showOld = TRUE OR wo.date_planned_start >= (CURRENT_DATE - INTERVAL '1 year'))
)
SELECT
  lsx,
  routing_name,
  split_part(routing_name,' ',1) AS product_code,
  ltrim(routing_name, split_part(routing_name,' ',1)) AS product_name,
  split_part(routing_name,'-', array_length(string_to_array(routing_name,'-'),1)) AS company,
  workorder_name,
  date_planned_start,
  state,
  order_name,
  production_qty,
  date_planned_finished,
  can_sx,
  note
FROM ranked
WHERE rn = 1
  AND (
    @showEmpty = TRUE OR
    (coalesce(lsx,'') <> '' AND coalesce(order_name,'') <> '' AND coalesce(workorder_name,'') <> '' AND can_sx > 0)
  );";


                using (var conn = new NpgsqlConnection(SQL.PostGreSQLConnectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(Query, conn))
                    {
                        cmd.Parameters.AddWithValue("@showEmpty", ShowEmpty.Checked);
                        cmd.Parameters.AddWithValue("@showOld", ShowOld.Checked);
                        cmd.CommandTimeout = 90;

                        using (var adapter = new NpgsqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            dt.BeginLoadData();
                            adapter.Fill(dt);
                            dt.EndLoadData();

                            /* Only fetch Yêu Cầu dates when we actually need them (Tab1).
                               This avoids a second DB round trip on other tabs. */
                            if (modetab == 1)
                            {
                                if (!dt.Columns.Contains("desire")) dt.Columns.Add("desire", typeof(DateTime));
                                if (!dt.Columns.Contains("checker")) dt.Columns.Add("checker", typeof(DateTime));
                                if (!dt.Columns.Contains("extra")) dt.Columns.Add("extra", typeof(DateTime));
                                if (!dt.Columns.Contains("ghichu")) dt.Columns.Add("ghichu", typeof(string));// tạo cột
                                if (!dt.Columns.Contains("baighep")) dt.Columns.Add("baighep", typeof(string));
                                if (!dt.Columns.Contains("ngay_co_nl")) dt.Columns.Add("ngay_co_nl", typeof(string));



                                var ycDict = SQL.LoadYeuCauDates(); // keep your existing function
                                                                    // Fast dictionary lookups with a tight for loop
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    var row = dt.Rows[i];
                                    var key = (row["lsx"]?.ToString() ?? "").Trim().Replace(" ", "").Replace("\u200B", "");
                                    if (ycDict.TryGetValue(key, out var dates))
                                    {
                                        if (DateTime.TryParse(dates.Date1, out var d1)) row["desire"] = d1; else row["desire"] = DBNull.Value;
                                        if (DateTime.TryParse(dates.Date2, out var d2)) row["checker"] = d2; else row["checker"] = DBNull.Value;
                                    }
                                }
                            }
                            else
                            {
                                // still create columns so grid code doesn't branch everywhere
                                if (!dt.Columns.Contains("desire")) dt.Columns.Add("desire", typeof(DateTime));
                                if (!dt.Columns.Contains("checker")) dt.Columns.Add("checker", typeof(DateTime));
                                if (!dt.Columns.Contains("extra")) dt.Columns.Add("extra", typeof(DateTime));
                                if (!dt.Columns.Contains("ghichu")) dt.Columns.Add("ghichu", typeof(string));
                                // tạo cột
                                if (!dt.Columns.Contains("baighep")) dt.Columns.Add("baighep", typeof(string));
                                if (!dt.Columns.Contains("ngay_co_nl")) dt.Columns.Add("ngay_co_nl", typeof(string));

                                // nạp dữ liệu từ Excel (NAS)
                                var bgMap = GetBaiGhepMap();
                                var nlMap = GetNgayCoNlMap();

                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    var row = dt.Rows[i];
                                    var key = (row["lsx"]?.ToString() ?? "");
                                    key = key.Trim().Replace(" ", "").Replace("\u200B", "");

                                    // Bài ghép
                                    if (bgMap.TryGetValue(key, out var bg) && !string.IsNullOrWhiteSpace(bg))
                                        row["baighep"] = bg;
                                    else
                                        row["baighep"] = "Lẻ";

                                    // Ngày có NL (chỉ phản hồi NL chính)
                                    if (nlMap.TryGetValue(key, out var nl) && !string.IsNullOrWhiteSpace(nl))
                                        row["ngay_co_nl"] = nl;
                                }

                            }

                            // Pre-fill machine column only on Tab2
                            if (!dt.Columns.Contains("machine")) dt.Columns.Add("machine", typeof(string));
                            if (modetab == 2)
                            {
                                var machineMap = SQL.GetLsxMachineMap(); // cached in your SQL layer if possible
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    var row = dt.Rows[i];
                                    var lsx = (row["lsx"]?.ToString() ?? "").Trim().Replace(" ", "").Replace("\u200B", "");
                                    if (machineMap.TryGetValue(lsx, out var mid) && !string.IsNullOrWhiteSpace(mid))
                                        row["machine"] = mid;
                                }
                            }

                            // Bind (keep UI fast)
                            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None; // prevent thrashing
                            dataGridView2.DataSource = new DataView(dt);
                            dataGridView2.AutoGenerateColumns = true;

                            // Show/hide per tab
                            if (modetab == 1)
                            {
                                // what Tab1 SHOULD show
                                if (dataGridView2.Columns.Contains("date_planned_start")) dataGridView2.Columns["date_planned_start"].Visible = true;
                                if (dataGridView2.Columns.Contains("company")) dataGridView2.Columns["company"].Visible = true;
                                if (dataGridView2.Columns.Contains("checker")) dataGridView2.Columns["checker"].Visible = true;
                                if (dataGridView2.Columns.Contains("extra")) dataGridView2.Columns["extra"].Visible = true;

                                if (dataGridView2.Columns.Contains("baighep")) dataGridView2.Columns["baighep"].Visible = false;
                                if (dataGridView2.Columns.Contains("ngay_co_nl")) dataGridView2.Columns["ngay_co_nl"].Visible = false;

                                // what Tab1 should NOT show
                                if (dataGridView2.Columns.Contains("machine")) dataGridView2.Columns["machine"].Visible = false;
                                if (dataGridView2.Columns.Contains("state")) dataGridView2.Columns["state"].Visible = false; // hide Trạng thái
                                if (dataGridView2.Columns.Contains("ka")) dataGridView2.Columns["ka"].Visible = false; // hide Ca if present
                            }
                            else
                            {
                                if (dataGridView2.Columns.Contains("machine")) dataGridView2.Columns["machine"].Visible = true;
                                if (dataGridView2.Columns.Contains("date_planned_start")) dataGridView2.Columns["date_planned_start"].Visible = false;
                                if (dataGridView2.Columns.Contains("company")) dataGridView2.Columns["company"].Visible = false;
                                if (dataGridView2.Columns.Contains("checker")) dataGridView2.Columns["checker"].Visible = false;
                                if (dataGridView2.Columns.Contains("extra")) dataGridView2.Columns["extra"].Visible = false;
                                if (dataGridView2.Columns.Contains("baighep")) dataGridView2.Columns["baighep"].Visible = true;
                                if (dataGridView2.Columns.Contains("ngay_co_nl")) dataGridView2.Columns["ngay_co_nl"].Visible = true;
                                // keep 'state' as-is for other tabs unless you also want it hidden there
                            }


                            foreach (DataGridViewColumn col in dataGridView2.Columns) col.ReadOnly = true;

                            string[] editable = { "desire", "checker", "extra", "ghichu", "machine" };
                            foreach (string colName in editable)
                            {
                                if (dataGridView2.Columns.Contains(colName))
                                {
                                    bool enable =
                                        (colName == "desire" && (int.Parse(Session.CurrentUser.Quyenhan) == 2 || int.Parse(Session.CurrentUser.Quyenhan) > 4)) ||
                                        (colName == "checker" && (int.Parse(Session.CurrentUser.Quyenhan) == 3 || int.Parse(Session.CurrentUser.Quyenhan) > 4)) ||
                                        (colName == "extra" && (int.Parse(Session.CurrentUser.Quyenhan) > 1 || int.Parse(Session.CurrentUser.Quyenhan) > 4)) ||
                                        (colName == "note" && (int.Parse(Session.CurrentUser.Quyenhan) > 1 || int.Parse(Session.CurrentUser.Quyenhan) > 4)) ||
                                        (colName == "machine" && (int.Parse(Session.CurrentUser.Quyenhan) > 1 || int.Parse(Session.CurrentUser.Quyenhan) > 4));
                                    dataGridView2.Columns[colName].ReadOnly = !enable;
                                }
                            }

                            // headers
                            var headersMap = new[]
                            {
                                "lsx:LSX","product_code:Mã SP","product_name:Tên sản phẩm","workorder_name:Quy trình hiện tại","routing_name:",
                                "company:Khách hàng","date_planned_start:Ngày bắt đầu quy trình hiện tại",
                                "date_planned_finished:Ngày Giao (ĐH)","production_qty:Số lượng",
                                "order_name:Số ĐH","state: Trạng thái","desire:Lịch Nhận Tuần",
                                "checker:Ngày SX Phản Hồi","extra:Ngày Giao Hàng",
                                "baighep:Bài ghép","ngay_co_nl:Ngày có NL","machine:Máy","ghichu:Ghi Chú","can_sx:Cần SX"
                            };

                            foreach (var pair in headersMap)
                            {
                                var parts = pair.Split(':');
                                if (dataGridView2.Columns.Contains(parts[0]))
                                {
                                    if (string.IsNullOrEmpty(parts[1]))
                                        dataGridView2.Columns[parts[0]].Visible = false;
                                    else
                                        dataGridView2.Columns[parts[0]].HeaderText = parts[1];
                                }
                            }

                            foreach (var colName in new[] { "desire", "checker", "extra" })
                                if (dataGridView2.Columns.Contains(colName))
                                    dataGridView2.Columns[colName].DefaultCellStyle.Format = "dd/MM/yyyy";

                            // Sort once after binding (avoid re-sorts during load)
                            if (dataGridView2.DataSource is DataView dv)
                                dv.Sort = "desire ASC, date_planned_finished ASC, company ASC";

                            // Only build DateSorter for Tab1 (saves time elsewhere)
                            if (modetab == 1)
                            {
                                DateSorter.Items.Clear();
                                foreach (var item in dt.AsEnumerable()
                                                       .Where(r => r.Field<DateTime?>("desire") != null)
                                                       .GroupBy(r => r.Field<DateTime>("desire"))
                                                       .OrderByDescending(g => g.Key)
                                                       .Select(g => $"{g.Key:dd/MM/yyyy} ({g.Count()})"))
                                {
                                    DateSorter.Items.Add(item);
                                }
                            }

                            allWorkorders = dt;
                            arranger(dataGridView2);

                            // now that we’re done, autosize once
                            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                            return allWorkorders;
                        }
                    }
                }
            }
            finally
            {
                dataGridView2.ResumeLayout();
            }
        }

        public static void ProductionPlan(string lsx, DataGridView targetGrid)
        {
            string query = @"
            SELECT DISTINCT
                wo.routing_equip_name AS workorder_name,
                wo.qty_produced,
                wo.sogio_can
            FROM mrp_workorder wo
            JOIN mrp_production_data pd ON pd.production_id = wo.production_id
            JOIN mrp_production mp ON pd.production_id = mp.id
            WHERE mp.sophieu = @lsx;";

            using (var conn = new NpgsqlConnection(SQL.PostGreSQLConnectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@lsx", lsx);
                using (var adapter = new NpgsqlDataAdapter(cmd))
                {
                    DataTable dta = new DataTable();
                    adapter.Fill(dta);
                    targetGrid.DataSource = dta;

                    foreach (DataRow row in dta.Rows)
                    {
                        var name = row["workorder_name"]?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(name))
                            row["workorder_name"] = "Thành Phẩm";
                    }

                    foreach (DataGridViewColumn col in targetGrid.Columns)
                        col.ReadOnly = true;

                    if (targetGrid.Columns.Contains("workorder_id"))
                        targetGrid.Columns["workorder_id"].HeaderText = "Mã công đoạn";
                    if (targetGrid.Columns.Contains("workorder_name"))
                        targetGrid.Columns["workorder_name"].HeaderText = "Quy trình hiện tại";
                    if (targetGrid.Columns.Contains("qty_produced"))
                        targetGrid.Columns["qty_produced"].HeaderText = "Số lượng đã sản xuất";
                    if (targetGrid.Columns.Contains("sogio_can"))
                        targetGrid.Columns["sogio_can"].HeaderText = "Định mức thời gian";
                    if (targetGrid.Columns.Contains("state"))
                    {
                        targetGrid.Columns["state"].HeaderText = "Trạng thái";
                        FormatStateColors(targetGrid);
                    }
                }
            }
        }

        private static void FormatStateColors(DataGridView grid)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["state"].Value != null)
                {
                    string state = row.Cells["state"].Value.ToString().ToLower();
                    switch (state)
                    {
                        case "done": row.DefaultCellStyle.BackColor = Color.LightGreen; break;
                        case "ready": row.DefaultCellStyle.BackColor = Color.LightYellow; break;
                        case "pending": row.DefaultCellStyle.BackColor = Color.LightCoral; break;
                    }
                }
            }
        }


    }

}
