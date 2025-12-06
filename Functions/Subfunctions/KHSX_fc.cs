// File: APPPC/KHSX_Helpers/WorkorderService.cs
using APPPC.Control;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace APPPC.KHSX_Helpers
{
    public static class WorkorderService
    {
        // ---------------------------
        // UI column arranger (stable)
        // ---------------------------
        public static void Arranger(DataGridView grid)
        {
            string[] ordered =
            {
                "lsx","product_code","product_name","company","workorder_name","routing_name",
                "date_planned_start","date_planned_finished","production_qty","can_sx",
                "order_name","state","desire","checker","extra","baighep","ngay_co_nl","machine","ghichu"
            };

            int idx = 0;
            foreach (var c in ordered)
                if (grid.Columns.Contains(c))
                    grid.Columns[c].DisplayIndex = idx++;
        }

        // ---------------------------
        // Helpers
        // ---------------------------
        private static string Norm(string s) =>
            (s ?? string.Empty).Trim().Replace(" ", "").Replace("\u200B", "");

        private static string BuildSafeSort(DataTable t, params string[] candidates)
        {
            var ok = new List<string>();
            foreach (var c in candidates)
            {
                var name = c.Split(' ')[0];
                if (t.Columns.Contains(name)) ok.Add(c);
            }
            return string.Join(", ", ok);
        }

        private static void EnsureExtraColumns(DataTable dt, bool includeMachine)
        {
            if (!dt.Columns.Contains("desire")) dt.Columns.Add("desire", typeof(DateTime));
            if (!dt.Columns.Contains("checker")) dt.Columns.Add("checker", typeof(DateTime));
            if (!dt.Columns.Contains("extra")) dt.Columns.Add("extra", typeof(DateTime));
            if (!dt.Columns.Contains("ghichu")) dt.Columns.Add("ghichu", typeof(string));
            if (!dt.Columns.Contains("baighep")) dt.Columns.Add("baighep", typeof(string));
            if (!dt.Columns.Contains("ngay_co_nl")) dt.Columns.Add("ngay_co_nl", typeof(string));
            if (includeMachine && !dt.Columns.Contains("machine")) dt.Columns.Add("machine", typeof(string));
        }


        // ---------------------------
        // Permission model
        // ---------------------------
        private static (bool canDesire, bool canChecker, bool canExtra, bool canGhichu) EvaluatePermissions()
        {
            int qh = 0;
            _ = int.TryParse(Session.CurrentUser?.Quyenhan, out qh);

            bool canDesire = (qh == 2 || qh >= 5);
            bool canChecker = (qh == 3 || qh >= 6);
            bool canExtra = (qh == 3 || qh >= 6);
            bool canGhichu = canDesire || canChecker || canExtra;

            return (canDesire, canChecker, canExtra, canGhichu);
        }

        /// <summary>
        /// Shallow check: is this logical column writable for CURRENT USER (ignoring grid state).
        /// </summary>
        public static bool IsColumnWritable(string columnName)
        {
            var key = (columnName ?? "").Trim().ToLowerInvariant();
            var (canDesire, canChecker, canExtra, canGhichu) = EvaluatePermissions();
            return key switch
            {
                "desire" => canDesire,
                "checker" => canChecker,
                "extra" => canExtra,
                "ghichu" => canGhichu,
                _ => false
            };
        }

        /// <summary>
        /// Back-compat helper for your Excel import: also honors the grid column's ReadOnly flag if present.
        /// </summary>
        public static bool ColumnIsEditable(DataGridView grid, string columnName)
        {
            bool allowed = IsColumnWritable(columnName);
            if (!allowed) return false;

            if (grid != null && grid.Columns != null && grid.Columns.Contains(columnName))
                return !grid.Columns[columnName].ReadOnly;

            return true;
        }

        // ---------------------------
        // Grid binding & formatting
        // ---------------------------
        private static void BindAndCommonSetup(
            DataGridView grid, DataTable dt,
            int currentTab, bool showCompany, bool showDatePlannedStart, bool showMachine, bool showGhepAndNl)
        {
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            grid.AutoGenerateColumns = true;
            grid.DataSource = new DataView(dt);

            var headers = new[]
            {
                "lsx:LSX","product_code:Mã SP","product_name:Tên sản phẩm","workorder_name:Quy trình hiện tại","routing_name:",
                "company:Khách hàng","date_planned_start:Ngày bắt đầu quy trình hiện tại",
                "date_planned_finished:Ngày Giao (ĐH)","production_qty:Số lượng",
                "order_name:Số ĐH","state:Trạng thái",
                "desire:Lịch Nhận Tuần","checker:Ngày SX Phản Hồi","extra:Ngày Giao Hàng",
                "baighep:Bài ghép","ngay_co_nl:Ngày có NL","machine:Máy","ghichu:Ghi Chú","can_sx:Cần SX"
            };

            foreach (var map in headers)
            {
                var parts = map.Split(':');
                var name = parts[0]; var title = parts[1];
                if (!grid.Columns.Contains(name)) continue;
                if (string.IsNullOrEmpty(title))
                    grid.Columns[name].Visible = false;
                else
                    grid.Columns[name].HeaderText = title;
            }

            // Show/hide per screen
            if (grid.Columns.Contains("company")) grid.Columns["company"].Visible = showCompany;
            if (grid.Columns.Contains("date_planned_start")) grid.Columns["date_planned_start"].Visible = showDatePlannedStart;
            if (grid.Columns.Contains("machine")) grid.Columns["machine"].Visible = showMachine;
            if (grid.Columns.Contains("baighep")) grid.Columns["baighep"].Visible = showGhepAndNl;
            if (grid.Columns.Contains("ngay_co_nl")) grid.Columns["ngay_co_nl"].Visible = showGhepAndNl;

            // Dates format
            foreach (var name in new[] { "desire", "checker", "extra", "date_planned_start", "date_planned_finished" })
                if (grid.Columns.Contains(name))
                    grid.Columns[name].DefaultCellStyle.Format = "dd/MM/yyyy";

            // Always hide these two in this screen
            if (grid.Columns.Contains("date_planned_start")) grid.Columns["date_planned_start"].Visible = false;
            if (grid.Columns.Contains("date_planned_finished")) grid.Columns["date_planned_finished"].Visible = false;

            // ReadOnly by perm
            foreach (DataGridViewColumn c in grid.Columns) c.ReadOnly = true;
            var (canDesire, canChecker, canExtra, canGhichu) = EvaluatePermissions();

            if (grid.Columns.Contains("desire")) grid.Columns["desire"].ReadOnly = !canDesire;
            if (grid.Columns.Contains("checker")) grid.Columns["checker"].ReadOnly = !canChecker;
            if (grid.Columns.Contains("extra")) grid.Columns["extra"].ReadOnly = !canExtra;
            if (grid.Columns.Contains("ghichu")) grid.Columns["ghichu"].ReadOnly = !canGhichu;

            // Tên sản phẩm: single line, wide
            if (grid.Columns.Contains("product_name"))
            {
                var c = grid.Columns["product_name"];
                c.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                c.FillWeight = 260;
            }
            // Global row/header single-line
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.RowTemplate.Height = Math.Max(grid.RowTemplate.Height, 28);

            // Sorting
            if (grid.DataSource is DataView dv)
            {
                if (currentTab == 1)
                    dv.Sort = BuildSafeSort(dv.Table, "desire ASC", "date_planned_finished ASC", "company ASC");
                else if (currentTab == 2)
                    dv.Sort = BuildSafeSort(dv.Table, "date_planned_start ASC", "date_planned_finished ASC");
                else
                    dv.Sort = BuildSafeSort(dv.Table, "date_planned_finished ASC", "order_name ASC");
            }

            Arranger(grid);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // ---------------------------
        // DB connection (Postgres)
        // ---------------------------
        private static NpgsqlConnection OpenDb()
        {
            var csb = new NpgsqlConnectionStringBuilder(SQL.PostGreSQLConnectionString)
            {
                KeepAlive = 30,
                IncludeErrorDetail = true,
                Timeout = 15,
                CommandTimeout = 300
            };

            var conn = new NpgsqlConnection(csb.ConnectionString);
            conn.Open();
            using (var setCmd = new NpgsqlCommand("SET statement_timeout = 600000;", conn))
                setCmd.ExecuteNonQuery();
            return conn;
        }

        // ---------------------------
        // Unified Postgres query
        // ---------------------------
        private static readonly string QueryUnified = @"
WITH
pat AS (SELECT '([0-9]{3,5}/[0-9]{3,5}(?:_XTP)?)'::text AS rx),

-- 1) LSX candidates (ranked)
lsx_mp AS (
  SELECT mp.id AS production_id, mp.product_id, mp.routing_id, mp.product_qty,
         mp.sophieu AS lsx_raw, 1 AS src_rank
  FROM mrp_production mp, pat
  WHERE COALESCE(mp.sophieu,'') <> '' AND mp.sophieu ~ pat.rx
),
lsx_sp AS (
  SELECT DISTINCT COALESCE(sm.production_id, mp2.id) AS production_id,
         mp2.product_id, mp2.routing_id, mp2.product_qty,
         sp.sophieu AS lsx_raw, 2 AS src_rank
  FROM stock_picking sp
  LEFT JOIN stock_move sm ON sm.picking_id = sp.id AND COALESCE(sm.state,'') <> 'cancel'
  LEFT JOIN mrp_production mp2 ON mp2.id = sm.production_id,
  pat
  WHERE COALESCE(sp.sophieu,'') <> '' AND sp.sophieu ~ pat.rx
),
lsx_sm AS (
  SELECT DISTINCT sm.production_id, mp.product_id, mp.routing_id, mp.product_qty,
         substring(sm.origin from pat.rx) AS lsx_raw, 3 AS src_rank
  FROM stock_move sm
  LEFT JOIN mrp_production mp ON mp.id = sm.production_id,
  pat
  WHERE COALESCE(sm.origin,'') ~ pat.rx
),
lsx_mail_mp AS (
  SELECT mp.id AS production_id, mp.product_id, mp.routing_id, mp.product_qty,
         substring(mm.record_name from pat.rx) AS lsx_raw, 4 AS src_rank
  FROM mail_message mm
  JOIN mrp_production mp ON mm.model = 'mrp.production' AND mm.res_id = mp.id,
  pat
  WHERE COALESCE(mm.record_name,'') ~ pat.rx
),
lsx_mail_tv AS (
  SELECT DISTINCT
         COALESCE(mp.id, mp2.id, sm.production_id) AS production_id,
         COALESCE(mp.product_id, mp2.product_id, mp3.product_id) AS product_id,
         COALESCE(mp.routing_id, mp2.routing_id, mp3.routing_id) AS routing_id,
         COALESCE(mp.product_qty, mp2.product_qty, mp3.product_qty) AS product_qty,
         substring(mtv.new_value_char from pat.rx) AS lsx_raw, 5 AS src_rank
  FROM mail_tracking_value mtv
  JOIN mail_message mm ON mm.id = mtv.mail_message_id
  LEFT JOIN mrp_production mp  ON mm.model = 'mrp.production' AND mm.res_id = mp.id
  LEFT JOIN stock_picking sp   ON mm.model = 'stock.picking'   AND mm.res_id = sp.id
  LEFT JOIN stock_move sm      ON mm.model = 'stock.move'      AND mm.res_id = sm.id
  LEFT JOIN mrp_production mp2 ON sp.id IS NOT NULL
                                AND EXISTS (SELECT 1 FROM stock_move sm2 WHERE sm2.picking_id = sp.id AND sm2.production_id = mp2.id)
  LEFT JOIN mrp_production mp3 ON sm.production_id = mp3.id,
  pat
  WHERE COALESCE(mtv.new_value_char,'') ~ pat.rx
),
lsx_woprod AS (
  SELECT DISTINCT wop.production_id, mp.product_id, mp.routing_id, mp.product_qty,
         substring(wop.name from pat.rx) AS lsx_raw, 6 AS src_rank
  FROM mrp_workorder_produced wop
  LEFT JOIN mrp_production mp ON mp.id = wop.production_id,
  pat
  WHERE COALESCE(wop.name,'') ~ pat.rx
),
lsx_candidates AS (
  SELECT * FROM lsx_mp
  UNION ALL SELECT * FROM lsx_sp
  UNION ALL SELECT * FROM lsx_sm
  UNION ALL SELECT * FROM lsx_mail_mp
  UNION ALL SELECT * FROM lsx_mail_tv
  UNION ALL SELECT * FROM lsx_woprod
),
best_lsx AS (
  SELECT *
  FROM (
    SELECT
      NormStr(lsx_raw) AS k,
      lsx_raw,
      production_id, product_id, routing_id, product_qty,
      ROW_NUMBER() OVER (
        PARTITION BY NormStr(lsx_raw)
        ORDER BY (production_id IS NOT NULL) DESC, src_rank ASC, production_id DESC NULLS LAST
      ) AS rn
    FROM lsx_candidates
    WHERE COALESCE(lsx_raw,'') <> ''
  ) s
  WHERE rn = 1
),

-- 2) Workorders
wo_best AS (
  SELECT x.* FROM (
    SELECT
      wo.production_id,
      COALESCE(NULLIF(wo.routing_equip_name,''),'Thành Phẩm') AS workorder_name,
      CAST(wo.date_planned_start AS date) AS date_planned_start,
      wo.state,
      ROW_NUMBER() OVER (
        PARTITION BY wo.production_id
        ORDER BY CASE wo.state WHEN 'progress' THEN 0 WHEN 'ready' THEN 1 WHEN 'pending' THEN 2 ELSE 9 END,
                 COALESCE(wo.date_planned_start, wo.create_date) DESC NULLS LAST,
                 wo.id DESC
      ) AS rn
    FROM mrp_workorder wo
    WHERE COALESCE(wo.state,'') <> 'cancel'
  ) x
  WHERE x.rn = 1
),
wo_agg AS (
  SELECT
    wo.production_id,
    COUNT(*) FILTER (WHERE COALESCE(wo.state,'') <> 'cancel') AS wo_cnt,
    COUNT(*) FILTER (WHERE wo.state = 'done')                  AS wo_done_cnt,
    COALESCE(SUM(wo.qty_produced),0)                           AS wo_qty_done
  FROM mrp_workorder wo
  GROUP BY wo.production_id
),

-- 3) SO linkage (group_id → sale_order) with fallback from origin
so_via_picking AS (
  SELECT DISTINCT
    mp.id AS production_id,
    sp.group_id AS group_id
  FROM mrp_production mp
  JOIN stock_move sm ON sm.production_id = mp.id AND COALESCE(sm.state,'') <> 'cancel'
  JOIN stock_picking sp ON sp.id = sm.picking_id
),
so_via_move AS (
  SELECT DISTINCT
    sm.production_id,
    sm.group_id
  FROM stock_move sm
  WHERE COALESCE(sm.state,'') <> 'cancel' AND sm.group_id IS NOT NULL
),
so_group_pick AS (
  SELECT production_id, MAX(group_id) AS group_id
  FROM (
    SELECT * FROM so_via_picking
    UNION ALL
    SELECT * FROM so_via_move
  ) z
  GROUP BY production_id
),
so_link AS (
  SELECT
    sgp.production_id,
    so.id AS so_id,
    so.name AS so_name,
    so.partner_id
  FROM so_group_pick sgp
  LEFT JOIN sale_order so ON so.procurement_group_id = sgp.group_id
),
so_from_origin AS (
  SELECT sm.production_id,
         COALESCE(NULLIF(substring(sm.origin from '(SO[0-9]+)'),''), NULLIF(sm.origin,'')) AS so_guess
  FROM stock_move sm
  WHERE COALESCE(sm.origin,'') <> ''
),
so_pick AS (
  SELECT production_id, COALESCE(MAX(so_name), MAX(so_guess)) AS order_name
  FROM (
    SELECT sl.production_id, sl.so_name, NULL::text AS so_guess FROM so_link sl
    UNION ALL
    SELECT sfo.production_id, NULL, sfo.so_guess FROM so_from_origin sfo
  ) z
  GROUP BY production_id
),

-- 4) Earliest promised date among SO moves
pick AS (
  SELECT so.id AS so_id, MIN(sm.date_expected)::date AS giao_date
  FROM stock_move sm
  JOIN procurement_group pg ON pg.id = sm.group_id
  JOIN sale_order so        ON so.procurement_group_id = pg.id
  WHERE COALESCE(sm.state,'') <> 'cancel'
  GROUP BY so.id
),

-- 5) Product info & fallbacks
prod_base AS (
  SELECT mp.id AS production_id, pp.id AS product_id,
         NULLIF(pp.default_code,'') AS pt_code,
         NULLIF(pt.name,'')        AS pt_name
  FROM mrp_production mp
  LEFT JOIN product_product  pp ON pp.id = mp.product_id
  LEFT JOIN product_template pt ON pt.id = pp.product_tmpl_id
),
prod_fallback AS (
  SELECT mp.id AS production_id,
         NULLIF(MAX(wo.ma_hh), '')  AS wo_code,
         NULLIF(MAX(bom.ma_hh), '') AS bom_code
  FROM mrp_production mp
  LEFT JOIN mrp_workorder wo ON wo.production_id = mp.id
  LEFT JOIN mrp_bom      bom ON bom.id = mp.bom_id
  GROUP BY mp.id
),
wop_name AS (
  SELECT wop.production_id, NULLIF(MAX(wop.product_name),'') AS wo_product_name
  FROM mrp_workorder_produced wop
  GROUP BY wop.production_id
),

-- 6) Aggregates for ""done""
sm_agg AS (
  SELECT
    sm.production_id,
    COUNT(*) FILTER (WHERE COALESCE(sm.state,'') <> 'cancel') AS mv_cnt,
    COUNT(*) FILTER (WHERE sm.state = 'done')                 AS mv_done_cnt
  FROM stock_move sm
  GROUP BY sm.production_id
),
-- NEW: mark ""done"" if a finished-product move is done (raw_material_production_id IS NULL)
fp_done AS (
  SELECT
    sm.production_id,
    BOOL_OR(sm.state = 'done' AND sm.raw_material_production_id IS NULL) AS has_fp_done
  FROM stock_move sm
  GROUP BY sm.production_id
),
suffix AS (SELECT '/(0525|0625|0725)(_XTP)?$'::text AS rx),

state_calc AS (
  SELECT
    mp.id AS production_id,
    CASE
      WHEN mp.state = 'cancel' THEN 'cancel'
      WHEN mp.state = 'done'   THEN 'done'
      WHEN (wa.wo_cnt > 0 AND wa.wo_done_cnt = wa.wo_cnt)
           OR (COALESCE(wa.wo_qty_done,0) >= COALESCE(mp.product_qty,0) - 0.001)
        THEN 'done'
      WHEN fd.has_fp_done THEN 'done'                            -- NEW
      WHEN (sa.mv_cnt > 0 AND sa.mv_done_cnt = sa.mv_cnt) THEN 'done'
      WHEN mp.sophieu ~ (SELECT rx FROM suffix) THEN 'done'
      ELSE COALESCE(wb.state, COALESCE(mp.state,'unknown'))
    END AS computed_state
  FROM best_lsx bl
  LEFT JOIN mrp_production mp ON mp.id = bl.production_id
  LEFT JOIN wo_agg  wa ON wa.production_id = bl.production_id
  LEFT JOIN sm_agg  sa ON sa.production_id = bl.production_id
  LEFT JOIN fp_done fd ON fd.production_id = bl.production_id    -- NEW
  LEFT JOIN wo_best wb ON wb.production_id = bl.production_id
),

-- 7) Assemble rows
final_raw AS (
  SELECT
    regexp_replace(REPLACE(bl.lsx_raw, '_XTP',''), '_XTP','', 'gi') AS lsx,
    bl.lsx_raw                                                     AS lsx_raw,

    COALESCE(pb.pt_code, pf.wo_code, pf.bom_code) AS product_code,

    COALESCE(
      NULLIF(regexp_replace(COALESCE(pb.pt_name, wn.wo_product_name), '\s*-\s*[^-]+$', '', 'g'), ''),
      COALESCE(pb.pt_name, wn.wo_product_name)
    ) AS product_name,

    COALESCE(
      NULLIF(btrim(split_part(COALESCE(pb.pt_name, wn.wo_product_name),
                              '-', array_length(string_to_array(COALESCE(pb.pt_name, wn.wo_product_name),'-'),1))), ''),
      rp.name
    ) AS company,

    wb.workorder_name,
    wb.date_planned_start,
    spk.order_name,

    COALESCE(sq.so_line_qty, mp.product_qty, 0) AS production_qty,
    pk.giao_date                                 AS date_planned_finished,
    GREATEST(COALESCE(sq.so_line_qty, mp.product_qty, 0) - COALESCE(dn.qty_done,0), 0) AS can_sx,

    sc.computed_state AS state,
    wb.production_id  AS production_id_for_rank
  FROM best_lsx bl
  LEFT JOIN mrp_production mp ON mp.id = bl.production_id
  LEFT JOIN wo_best        wb ON wb.production_id = bl.production_id
  LEFT JOIN prod_base      pb ON pb.production_id = bl.production_id
  LEFT JOIN prod_fallback  pf ON pf.production_id = bl.production_id
  LEFT JOIN wop_name       wn ON wn.production_id = bl.production_id
  LEFT JOIN so_link         sl ON sl.production_id = bl.production_id
  LEFT JOIN res_partner     rp ON rp.id           = sl.partner_id
  LEFT JOIN so_pick        spk ON spk.production_id = bl.production_id
  LEFT JOIN (
      SELECT so.id AS so_id, sol.product_id, SUM(sol.product_uom_qty) AS so_line_qty
      FROM sale_order_line sol
      JOIN sale_order so ON so.id = sol.order_id
      GROUP BY so.id, sol.product_id
  ) sq ON sq.so_id = sl.so_id AND sq.product_id = bl.product_id
  LEFT JOIN (
      SELECT production_id, COALESCE(SUM(qty_produced),0) AS qty_done
      FROM mrp_workorder
      GROUP BY production_id
  ) dn ON dn.production_id = bl.production_id
  LEFT JOIN pick           pk ON pk.so_id        = sl.so_id
  LEFT JOIN state_calc     sc ON sc.production_id = bl.production_id
),

-- 8) De-dupe: prefer a DONE record if any (so the LSX is removed by the filter)
final_dedup AS (
  SELECT *
  FROM (
    SELECT fr.*,
           ROW_NUMBER() OVER (
             PARTITION BY fr.lsx
             ORDER BY
               CASE WHEN fr.state = 'done' THEN 0 ELSE 1 END,            -- NEW: prefer done
               CASE WHEN fr.lsx_raw ILIKE '%\_XTP' THEN 1 ELSE 0 END,
               CASE WHEN fr.order_name IS NULL THEN 1 ELSE 0 END,
               fr.date_planned_start DESC NULLS LAST,
               fr.production_id_for_rank DESC NULLS LAST
           ) AS rn
    FROM final_raw fr
  ) s
  WHERE rn = 1
)

SELECT
  lsx,
  product_code,
  product_name,
  company,
  workorder_name,
  date_planned_start,
  order_name,
  production_qty,
  date_planned_finished,
  can_sx,
  state
FROM final_dedup
WHERE
  (@showOld = TRUE OR date_planned_start IS NULL OR date_planned_start >= (CURRENT_DATE - INTERVAL '1 year'))
  AND COALESCE(state,'') NOT IN ('done','cancel')
  AND (
    @showEmpty = TRUE
    OR (
      COALESCE(lsx,'') <> ''
      AND COALESCE(order_name,'') <> ''
      AND COALESCE(workorder_name,'') <> ''
      AND COALESCE(can_sx,0) > 0
    )
  )
ORDER BY lsx;

";

        // ---------------------------
        // Public API
        // ---------------------------
        public static DataTable LoadPendingWorkorders(
            DataGridView gridPlan,               // not used but kept for signature parity
            DataGridView grid,                   // target grid
            CheckBox showEmpty,
            CheckBox showOld,
            ComboBox dateSorter,
            int modetab)
        {
            grid.SuspendLayout();
            try
            {
                EnsureNormStrFunction();

                var dt = new DataTable();

                using (var conn = OpenDb())
                {
                    try
                    {
                        using var cmd = new NpgsqlCommand(QueryUnified, conn);
                        cmd.Parameters.Add("@showEmpty", NpgsqlDbType.Boolean).Value = showEmpty.Checked;
                        cmd.Parameters.Add("@showOld", NpgsqlDbType.Boolean).Value = showOld.Checked;
                        cmd.CommandTimeout = 300;

                        using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        dt.Load(reader);
                    }
                    catch (PostgresException ex)
                    {
                        MessageBox.Show($"PostgreSQL error {ex.SqlState} at pos {ex.Position}\n{ex.MessageText}\n{ex.Detail}\n{ex.Where}",
                            "PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw;
                    }
                    catch (NpgsqlException ex)
                    {
                        MessageBox.Show($"Npgsql error: {ex.Message}\nInner: {ex.InnerException?.Message}",
                            "Npgsql", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Unhandled: {ex.GetType().Name}\n{ex.Message}", ".NET",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw;
                    }
                }

                // Ensure editable columns exist (client side)
                EnsureExtraColumns(dt, includeMachine: false);

                // Overlay values from SQL Server YCSX (optional) and remove delivered rows
                try
                {
                    var map = SQL.LoadYeuCauDates(); // shape may vary; reflection below handles Date3/Note if present
                    var killRows = new List<DataRow>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var row = dt.Rows[i];
                        var key = Norm(row["lsx"]?.ToString() ?? "");
                        if (string.IsNullOrEmpty(key)) continue;

                        if (!map.TryGetValue(key, out var datesObj)) continue;

                        object box = datesObj!;
                        var t = box.GetType();

                        string Read(string name, string alt = null)
                        {
                            var p = t.GetProperty(name);
                            if (p != null) return p.GetValue(box)?.ToString();
                            var f = t.GetField(name);
                            if (f != null) return f.GetValue(box)?.ToString();
                            if (!string.IsNullOrEmpty(alt))
                            {
                                var p2 = t.GetProperty(alt);
                                if (p2 != null) return p2.GetValue(box)?.ToString();
                                var f2 = t.GetField(alt);
                                if (f2 != null) return f2.GetValue(box)?.ToString();
                            }
                            return null;
                        }

                        var s1 = Read("Date1", "Item1");
                        var s2 = Read("Date2", "Item2");
                        var s3 = Read("Date3", "Extra") ?? Read("Item3");
                        var s4 = Read("Note", "Ghichu") ?? Read("Item4");

                        if (DateTime.TryParse(s1, out var d1)) row["desire"] = d1.Date;
                        if (DateTime.TryParse(s2, out var d2)) row["checker"] = d2.Date;
                        if (DateTime.TryParse(s3, out var d3)) row["extra"] = d3.Date;
                        if (!string.IsNullOrWhiteSpace(s4)) row["ghichu"] = s4;

                        // If Ngày Giao Hàng exists -> exclude whole LSX from app
                        if (!row.IsNull("extra") && DateTime.TryParse(row["extra"].ToString(), out _))
                            killRows.Add(row);
                    }

                    foreach (var r in killRows) dt.Rows.Remove(r);
                    dt.AcceptChanges();
                }
                catch
                {
                    // Overlay is optional; swallow errors to keep main grid working
                }

                BindAndCommonSetup(
                    grid, dt,
                    currentTab: 1,
                    showCompany: true,
                    showDatePlannedStart: false,   // hidden here
                    showMachine: false,
                    showGhepAndNl: false
                );

                // Fill date sorter from desire (if any)
                dateSorter.Items.Clear();
                foreach (var g in dt.AsEnumerable()
                                    .Where(r => r.Field<DateTime?>("desire") != null)
                                    .GroupBy(r => r.Field<DateTime>("desire"))
                                    .OrderByDescending(g => g.Key))
                {
                    dateSorter.Items.Add($"{g.Key:dd/MM/yyyy} ({g.Count()})");
                }
                dateSorter.Enabled = true;
                dateSorter.Visible = true;
                dateSorter.Tag = "desire";

                return dt;
            }
            finally
            {
                grid.ResumeLayout();
            }
        }

        /// <summary>
        /// After saving, call this to drop any rows that just received a delivery date (Ngày Giao Hàng).
        /// </summary>
        public static void ExcludeDeliveredRows(DataGridView grid)
        {
            if (grid?.DataSource is not DataView dv || dv.Table == null) return;

            var toDelete = new List<DataRow>();
            foreach (DataRow r in dv.Table.Rows)
            {
                if (dv.Table.Columns.Contains("extra") && !r.IsNull("extra") &&
                    DateTime.TryParse(r["extra"]?.ToString(), out _))
                {
                    toDelete.Add(r);
                }
            }
            foreach (var r in toDelete) r.Delete();
            dv.Table.AcceptChanges();
        }

        // ---------------------------
        // DB helper (idempotent)
        // ---------------------------
        private static void EnsureNormStrFunction()
        {
            using var conn = OpenDb();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
CREATE OR REPLACE FUNCTION NormStr(text) RETURNS text
LANGUAGE sql IMMUTABLE AS $f$
  SELECT regexp_replace(
           regexp_replace(coalesce($1,''), '\s+', '', 'g'),
           '[' || chr(8203) || chr(8204) || chr(8205) || ']', '', 'g'
         );
$f$;";
            cmd.ExecuteNonQuery();
        }

        // ---------------------------
        // Optional: production plan viewer (unchanged)
        // ---------------------------
        public static void ProductionPlan(string lsx, DataGridView targetGrid)
        {
            const string q = @"
                SELECT DISTINCT
                    wo.routing_equip_name AS workorder_name,
                    wo.qty_produced,
                    wo.sogio_can,
                    wo.state
                FROM mrp_workorder wo
                JOIN mrp_production mp ON mp.id = wo.production_id
                WHERE NormStr(mp.sophieu) = NormStr(@lsx);";

            using var conn = OpenDb();
            using var cmd = new NpgsqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@lsx", lsx ?? "");
            using var adp = new NpgsqlDataAdapter(cmd);

            var dt = new DataTable();
            adp.Fill(dt);

            foreach (DataRow row in dt.Rows)
                if (string.IsNullOrWhiteSpace(row["workorder_name"]?.ToString()))
                    row["workorder_name"] = "Thành Phẩm";

            targetGrid.DataSource = dt;
            foreach (DataGridViewColumn c in targetGrid.Columns) c.ReadOnly = true;

            if (targetGrid.Columns.Contains("workorder_name")) targetGrid.Columns["workorder_name"].HeaderText = "Quy trình hiện tại";
            if (targetGrid.Columns.Contains("qty_produced")) targetGrid.Columns["qty_produced"].HeaderText = "Số lượng đã sản xuất";
            if (targetGrid.Columns.Contains("sogio_can")) targetGrid.Columns["sogio_can"].HeaderText = "Định mức thời gian";
            if (targetGrid.Columns.Contains("state"))
            {
                targetGrid.Columns["state"].HeaderText = "Trạng thái";
                FormatStateColors(targetGrid);
            }
        }

        private static void FormatStateColors(DataGridView grid)
        {
            foreach (DataGridViewRow r in grid.Rows)
            {
                var raw = r.Cells["state"]?.Value?.ToString();
                if (string.IsNullOrEmpty(raw)) continue;

                switch (raw.ToLowerInvariant())
                {
                    case "done": r.DefaultCellStyle.BackColor = Color.LightGreen; break;
                    case "ready": r.DefaultCellStyle.BackColor = Color.LightYellow; break;
                    case "pending": r.DefaultCellStyle.BackColor = Color.LightCoral; break;
                }
            }
        }
    }
}
