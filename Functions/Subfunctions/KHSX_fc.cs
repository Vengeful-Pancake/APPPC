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

        private static string querytab1 = @"
WITH pick AS (
  SELECT sm.production_id,
         MIN(COALESCE(sm.date_expected, sm.date, sp.date_done))::date AS giao_date,
         SUM(sm.product_uom_qty) AS move_qty,
         MIN(sp.origin) AS so_name
  FROM stock_move sm
  LEFT JOIN stock_picking sp ON sp.id = sm.picking_id
  WHERE sm.production_id IS NOT NULL AND COALESCE(sm.state,'') <> 'cancel'
  GROUP BY sm.production_id
),
done AS (
  SELECT wo.production_id, COALESCE(SUM(wo.qty_produced),0) AS qty_done
  FROM mrp_workorder wo
  GROUP BY wo.production_id
),
sol AS (
  SELECT so.name AS so_name, sol.product_id, SUM(sol.product_uom_qty) AS so_line_qty
  FROM sale_order_line sol
  JOIN sale_order so ON so.id = sol.order_id
  GROUP BY so.name, sol.product_id
),
ranked AS (
  SELECT
    mp.sophieu AS lsx,
    r.name     AS routing_name,
    COALESCE(NULLIF(wo.routing_equip_name,''),'Thành Phẩm') AS workorder_name,
    CAST(wo.date_planned_start AS date) AS date_planned_start,
    p.so_name AS order_name,
    COALESCE(s.so_line_qty, p.move_qty, 0) AS production_qty,
    p.giao_date AS date_planned_finished,
    GREATEST(COALESCE(s.so_line_qty, p.move_qty, 0) - COALESCE(dn.qty_done, 0), 0) AS can_sx,
    ROW_NUMBER() OVER (
      PARTITION BY mp.sophieu
      ORDER BY CASE wo.state WHEN 'ready' THEN 0 WHEN 'pending' THEN 1 ELSE 2 END,
               wo.date_planned_start DESC
    ) AS rn
  FROM mrp_production mp
  JOIN mrp_workorder wo ON wo.production_id = mp.id
  JOIN mrp_routing   r  ON r.id = mp.routing_id
  LEFT JOIN pick p   ON p.production_id = mp.id
  LEFT JOIN done dn  ON dn.production_id = mp.id
  LEFT JOIN sol  s   ON s.so_name = p.so_name AND s.product_id = mp.product_id
  WHERE COALESCE(wo.state,'') <> 'cancel'
    AND (@showOld = TRUE OR wo.date_planned_start >= (CURRENT_DATE - INTERVAL '1 year'))
)
SELECT
  lsx,
  split_part(routing_name,' ',1) AS product_code,
  ltrim(routing_name, split_part(routing_name,' ',1)) AS product_name,
  split_part(routing_name,'-', array_length(string_to_array(routing_name,'-'),1)) AS company,
  workorder_name,
  date_planned_start,
  order_name,
  production_qty,
  date_planned_finished,
  can_sx
FROM ranked
WHERE rn = 1
  AND (
    @showEmpty = TRUE OR
    (coalesce(lsx,'') <> '' AND coalesce(order_name,'') <> '' AND coalesce(workorder_name,'') <> '' AND can_sx > 0)
  );";

        private static string querytab2 = @"
WITH pick AS (
  SELECT sm.production_id,
         MIN(COALESCE(sm.date_expected, sm.date, sp.date_done))::date AS giao_date,
         SUM(sm.product_uom_qty) AS move_qty,
         MIN(sp.origin) AS so_name
  FROM stock_move sm
  LEFT JOIN stock_picking sp ON sp.id = sm.picking_id
  WHERE sm.production_id IS NOT NULL AND COALESCE(sm.state,'') <> 'cancel'
  GROUP BY sm.production_id
),
done AS (
  SELECT wo.production_id, COALESCE(SUM(wo.qty_produced),0) AS qty_done
  FROM mrp_workorder wo
  GROUP BY wo.production_id
),
sol AS (
  SELECT so.name AS so_name, sol.product_id, SUM(sol.product_uom_qty) AS so_line_qty
  FROM sale_order_line sol
  JOIN sale_order so ON so.id = sol.order_id
  GROUP BY so.name, sol.product_id
),
ranked AS (
  SELECT
    mp.sophieu AS lsx,
    r.name     AS routing_name,
    COALESCE(NULLIF(wo.routing_equip_name,''),'Thành Phẩm') AS workorder_name,
    p.so_name AS order_name,
    COALESCE(s.so_line_qty, p.move_qty, 0) AS production_qty,
    p.giao_date AS date_planned_finished,
    GREATEST(COALESCE(s.so_line_qty, p.move_qty, 0) - COALESCE(dn.qty_done, 0), 0) AS can_sx,
    ROW_NUMBER() OVER (
      PARTITION BY mp.sophieu
      ORDER BY CASE wo.state WHEN 'ready' THEN 0 WHEN 'pending' THEN 1 ELSE 2 END,
               wo.date_planned_start DESC
    ) AS rn
  FROM mrp_production mp
  JOIN mrp_workorder wo ON wo.production_id = mp.id
  JOIN mrp_routing   r  ON r.id = mp.routing_id
  LEFT JOIN pick p   ON p.production_id = mp.id
  LEFT JOIN done dn  ON dn.production_id = mp.id
  LEFT JOIN sol  s   ON s.so_name = p.so_name AND s.product_id = mp.product_id
  WHERE COALESCE(wo.state,'') <> 'cancel'
    AND (@showOld = TRUE OR wo.date_planned_start >= (CURRENT_DATE - INTERVAL '1 year'))
)
SELECT
  lsx,
  split_part(routing_name,' ',1) AS product_code,
  ltrim(routing_name, split_part(routing_name,' ',1)) AS product_name,
  workorder_name,
  order_name,
  production_qty,
  date_planned_finished,
  can_sx
FROM ranked
WHERE rn = 1
  AND (
    @showEmpty = TRUE OR
    (coalesce(lsx,'') <> '' AND coalesce(order_name,'') <> '' AND coalesce(workorder_name,'') <> '' AND can_sx > 0)
  );";

        private static string querytab3 = @"
WITH pick AS (
  SELECT sm.production_id,
         MIN(COALESCE(sm.date_expected, sm.date, sp.date_done))::date AS giao_date,
         SUM(sm.product_uom_qty) AS move_qty,
         MIN(sp.origin) AS so_name
  FROM stock_move sm
  LEFT JOIN stock_picking sp ON sp.id = sm.picking_id
  WHERE sm.production_id IS NOT NULL AND COALESCE(sm.state,'') <> 'cancel'
  GROUP BY sm.production_id
),
sol AS (
  SELECT so.name AS so_name, sol.product_id, SUM(sol.product_uom_qty) AS so_line_qty
  FROM sale_order_line sol
  JOIN sale_order so ON so.id = sol.order_id
  GROUP BY so.name, sol.product_id
),
done AS (
  SELECT wo.production_id, COALESCE(SUM(wo.qty_produced),0) AS qty_done
  FROM mrp_workorder wo
  GROUP BY wo.production_id
),
ranked AS (
  SELECT
    mp.sophieu AS lsx,
    r.name     AS routing_name,
    p.so_name AS order_name,
    COALESCE(s.so_line_qty, p.move_qty, 0) AS production_qty,
    p.giao_date AS date_planned_finished,
    ROW_NUMBER() OVER (
      PARTITION BY mp.sophieu
      ORDER BY COALESCE(wo.date_planned_start, now()) DESC
    ) AS rn
  FROM mrp_production mp
  JOIN mrp_workorder wo ON wo.production_id = mp.id
  JOIN mrp_routing   r  ON r.id = mp.routing_id
  LEFT JOIN pick p   ON p.production_id = mp.id
  LEFT JOIN sol  s   ON s.so_name = p.so_name AND s.product_id = mp.product_id
  LEFT JOIN done d   ON d.production_id = mp.id
  WHERE COALESCE(wo.state,'') <> 'cancel'
    AND (@showOld = TRUE OR wo.date_planned_start >= (CURRENT_DATE - INTERVAL '1 year'))
)
SELECT
  lsx,
  split_part(routing_name,' ',1) AS product_code,
  ltrim(routing_name, split_part(routing_name,' ',1)) AS product_name,
  order_name,
  production_qty,
  date_planned_finished
FROM ranked
WHERE rn = 1;";

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
                using var conn = new NpgsqlConnection(SQL.PostGreSQLConnectionString);
                conn.Open();

                // -------- local helpers (scoped to this method) ----------
                static string BuildSafeSort(DataTable t, params string[] candidates)
                {
                    var ok = new List<string>();
                    foreach (var c in candidates)
                    {
                        var col = c.Split(' ')[0].Trim();
                        if (t.Columns.Contains(col)) ok.Add(c);
                    }
                    return string.Join(", ", ok);
                }

                DataTable RunQuery(string sql)
                {
                    using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@showEmpty", ShowEmpty.Checked);
                    cmd.Parameters.AddWithValue("@showOld", ShowOld.Checked);
                    cmd.CommandTimeout = 90;

                    using var adp = new NpgsqlDataAdapter(cmd);
                    var dt = new DataTable();
                    dt.BeginLoadData();
                    adp.Fill(dt);
                    dt.EndLoadData();
                    return dt;
                }

                void EnsureExtraColumns(DataTable dt, bool includeMachine)
                {
                    if (!dt.Columns.Contains("desire")) dt.Columns.Add("desire", typeof(DateTime));
                    if (!dt.Columns.Contains("checker")) dt.Columns.Add("checker", typeof(DateTime));
                    if (!dt.Columns.Contains("extra")) dt.Columns.Add("extra", typeof(DateTime));
                    if (!dt.Columns.Contains("ghichu")) dt.Columns.Add("ghichu", typeof(string));
                    if (!dt.Columns.Contains("baighep")) dt.Columns.Add("baighep", typeof(string));
                    if (!dt.Columns.Contains("ngay_co_nl")) dt.Columns.Add("ngay_co_nl", typeof(string));
                    if (includeMachine && !dt.Columns.Contains("machine")) dt.Columns.Add("machine", typeof(string));
                }

                void BindAndCommonSetup(DataTable dt, int currentTab, bool showCompany, bool showDatePlannedStart, bool showMachine, bool showGhepAndNL)
                {
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    dataGridView2.AutoGenerateColumns = true;
                    dataGridView2.DataSource = new DataView(dt);

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

                    // per-tab visibility
                    if (dataGridView2.Columns.Contains("company")) dataGridView2.Columns["company"].Visible = showCompany;
                    if (dataGridView2.Columns.Contains("date_planned_start")) dataGridView2.Columns["date_planned_start"].Visible = showDatePlannedStart;
                    if (dataGridView2.Columns.Contains("machine")) dataGridView2.Columns["machine"].Visible = showMachine;

                    if (dataGridView2.Columns.Contains("baighep")) dataGridView2.Columns["baighep"].Visible = showGhepAndNL;
                    if (dataGridView2.Columns.Contains("ngay_co_nl")) dataGridView2.Columns["ngay_co_nl"].Visible = showGhepAndNL;

                    // common hides
                    if (currentTab == 1)
                    {
                        if (dataGridView2.Columns.Contains("state")) dataGridView2.Columns["state"].Visible = false;
                        if (dataGridView2.Columns.Contains("ka")) dataGridView2.Columns["ka"].Visible = false;
                    }

                    // format
                    foreach (var colName in new[] { "desire", "checker", "extra" })
                        if (dataGridView2.Columns.Contains(colName))
                            dataGridView2.Columns[colName].DefaultCellStyle.Format = "dd/MM/yyyy";

                    // readonly & editable rules
                    foreach (DataGridViewColumn col in dataGridView2.Columns) col.ReadOnly = true;
                    string[] editable = { "desire", "checker", "extra", "ghichu", "machine" };
                    int qh = int.TryParse(Session.CurrentUser.Quyenhan, out var tmpQh) ? tmpQh : 0;
                    foreach (string colName in editable)
                    {
                        if (!dataGridView2.Columns.Contains(colName)) continue;
                        bool enable =
                            (colName == "desire" && (qh == 2 || qh > 4)) ||
                            (colName == "checker" && (qh == 3 || qh > 4)) ||
                            (colName == "extra" && (qh > 1 || qh > 4)) ||
                            (colName == "note" && (qh > 1 || qh > 4)) ||
                            (colName == "machine" && (qh > 1 || qh > 4));
                        dataGridView2.Columns[colName].ReadOnly = !enable;
                    }

                    // safe sort: only sort by columns that exist in this tab
                    if (dataGridView2.DataSource is DataView dv)
                    {
                        if (currentTab == 1)
                            dv.Sort = BuildSafeSort(dv.Table,
                                "desire ASC", "date_planned_finished ASC", "company ASC");
                        else if (currentTab == 2)
                            dv.Sort = BuildSafeSort(dv.Table,
                                "desire ASC", "date_planned_finished ASC");
                        else // tab 3
                            dv.Sort = BuildSafeSort(dv.Table,
                                "date_planned_finished ASC", "order_name ASC");
                    }

                    arranger(dataGridView2);
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                // --------------------------------------------------------

                DataTable allWorkorders;

                // ===================== TAB 1 =====================
                if (modetab == 1)
                {
                    var dt = RunQuery(querytab1);

                    // add extra columns & fill Yêu Cầu
                    EnsureExtraColumns(dt, includeMachine: false);

                    var ycDict = SQL.LoadYeuCauDates();
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

                    // bind + per-tab display
                    BindAndCommonSetup(dt, currentTab: 1,
                        showCompany: true,
                        showDatePlannedStart: true,
                        showMachine: false,
                        showGhepAndNL: false);

                    // DateSorter only for Tab 1
                    DateSorter.Items.Clear();
                    foreach (var item in dt.AsEnumerable()
                                           .Where(r => r.Field<DateTime?>("desire") != null)
                                           .GroupBy(r => r.Field<DateTime>("desire"))
                                           .OrderByDescending(g => g.Key)
                                           .Select(g => $"{g.Key:dd/MM/yyyy} ({g.Count()})"))
                    {
                        DateSorter.Items.Add(item);
                    }
                    DateSorter.Enabled = true;
                    DateSorter.Visible = true;
                    DateSorter.Tag = "desire";
                    allWorkorders = dt;
                }
                // ===================== TAB 2 =====================
                else if (modetab == 2)
                {
                    var dt = RunQuery(querytab2);

                    // add columns
                    EnsureExtraColumns(dt, includeMachine: true);

                    // Excel maps
                    var bgMap = GetBaiGhepMap();
                    var nlMap = GetNgayCoNlMap();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var row = dt.Rows[i];
                        var key = (row["lsx"]?.ToString() ?? "").Trim().Replace(" ", "").Replace("\u200B", "");

                        row["baighep"] = (bgMap.TryGetValue(key, out var bg) && !string.IsNullOrWhiteSpace(bg)) ? bg : "Lẻ";
                        if (nlMap.TryGetValue(key, out var nl) && !string.IsNullOrWhiteSpace(nl))
                            row["ngay_co_nl"] = nl;
                    }
                    DateSorter.Enabled = false;
                    DateSorter.Visible = false;
                    DateSorter.Items.Clear();
                    DateSorter.SelectedIndex = -1;
                    DateSorter.Tag = null;
                    // Pre-fill machine for Tab 2
                    var machineMap = SQL.GetLsxMachineMap();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var row = dt.Rows[i];
                        var lsx = (row["lsx"]?.ToString() ?? "").Trim().Replace(" ", "").Replace("\u200B", "");
                        if (machineMap.TryGetValue(lsx, out var mid) && !string.IsNullOrWhiteSpace(mid))
                            row["machine"] = mid;
                    }

                    // bind + per-tab display
                    BindAndCommonSetup(dt, currentTab: 2,
                        showCompany: false,
                        showDatePlannedStart: false,
                        showMachine: true,
                        showGhepAndNL: true);
                    DateSorter.Enabled = false;
                    DateSorter.Visible = false;
                    DateSorter.Items.Clear();
                    DateSorter.SelectedIndex = -1;
                    DateSorter.Tag = null;
                    allWorkorders = dt;
                }
                // ===================== TAB 3 =====================
                else
                {
                    var dt = RunQuery(querytab3);

                    // add columns (no machine by default for tab 3)
                    EnsureExtraColumns(dt, includeMachine: false);

                    // Excel maps
                    var bgMap = GetBaiGhepMap();
                    var nlMap = GetNgayCoNlMap();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var row = dt.Rows[i];
                        var key = (row["lsx"]?.ToString() ?? "").Trim().Replace(" ", "").Replace("\u200B", "");

                        row["baighep"] = (bgMap.TryGetValue(key, out var bg) && !string.IsNullOrWhiteSpace(bg)) ? bg : "Lẻ";
                        if (nlMap.TryGetValue(key, out var nl) && !string.IsNullOrWhiteSpace(nl))
                            row["ngay_co_nl"] = nl;
                    }

                    // bind + per-tab display
                    BindAndCommonSetup(dt, currentTab: 3,
                        showCompany: false,
                        showDatePlannedStart: false,
                        showMachine: false,
                        showGhepAndNL: true);

                    allWorkorders = dt;
                }

                return allWorkorders;
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
