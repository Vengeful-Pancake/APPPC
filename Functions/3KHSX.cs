using APPPC.Control;
using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace APPPC.Functions
{
    public partial class KHSX : UserControl
    {
        string connString = "Host=192.168.1.12;Port=5432;Username=odoo;Password=2c-*kw?SG*fXv3pX;Database=QLSX3"; // admin string
        //string connString = "Host=192.168.1.12;Port=5432;Username=odoo-read;Password=lbgUfH,f6#-SkA#;Database=QLSX3";
        private DataTable allWorkorders;

        public KHSX()
        {
            InitializeComponent();
            LoadPendingWorkorders();
        }

        private void LoadPendingWorkorders()
        {
            string updatedQuery = @"
                SELECT DISTINCT
                    mp.sophieu AS lsx,
                    r.name AS product_name,
                    wo.name AS workorder_name,
                    CAST(wo.date_planned_start AS DATE) AS date_planned_start,
                    wo.state,
                    lgh.production_qty,
                    lgh.order_name,
                    CAST(lgh.order_line_ngaygiao AS DATE) AS date_planned_finished
                FROM mrp_production_data pd
                JOIN mrp_routing r ON pd.routing_id = r.id
                JOIN mrp_workorder wo ON pd.production_id = wo.production_id
                JOIN mrp_production mp ON pd.production_id = mp.id
                LEFT JOIN lich_giao_hang lgh ON lgh.production_id = pd.production_id
                WHERE wo.state = 'ready'
                ORDER BY date_planned_start DESC;

            ";

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(updatedQuery, conn))
                using (var adapter = new NpgsqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Add derived columns for display only (not sorting)
                    if (!dt.Columns.Contains("product_code"))
                        dt.Columns.Add("product_code", typeof(string));
                    if (!dt.Columns.Contains("product_name_clean"))
                        dt.Columns.Add("product_name_clean", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["product_name"] != DBNull.Value)
                        {
                            string full = row["product_name"].ToString();
                            int spaceIndex = full.IndexOf(' ');
                            if (spaceIndex > 0)
                            {
                                row["product_code"] = full.Substring(0, spaceIndex);
                                row["product_name_clean"] = full.Substring(spaceIndex + 1).Trim();
                            }
                            else
                            {
                                row["product_code"] = full;
                                row["product_name_clean"] = "";
                            }
                        }
                    }

                    // Filter duplicates using only the largest LSX (suffix first, then prefix)
                    var filteredRows = dt.AsEnumerable()
                        .GroupBy(row => new
                        {
                            ProductCode = row.Field<string>("product_code"),
                            ProductNameClean = row.Field<string>("product_name_clean"),
                            WorkorderName = row.Field<string>("workorder_name")
                        })
                        .Select(g =>
                        {
                            return g
                                .Select(r =>
                                {
                                    string lsx = r.Field<string>("lsx");
                                    int prefix = 0, suffix = 0;
                                    int mm = 0, yy = 0;
                                    if (!string.IsNullOrEmpty(lsx) && lsx.Contains("/"))
                                    {

                                        var parts = lsx.Split('/');
                                        int.TryParse(parts[0], out prefix);
                                        int.TryParse(parts[1].Substring(0, 2), out mm); // MM
                                        int.TryParse(parts[1].Substring(2), out yy);    // YY
                                        int.TryParse(parts[1], out suffix);
                                    }
                                    return new { Row = r, Prefix = prefix, YY = yy, MM = mm };
                                })
                                .OrderByDescending(x => x.YY)
                                .ThenByDescending(x => x.MM)
                                .ThenByDescending(x => x.Prefix)
                                .Select(x => x.Row)
                                .First();
                        })

                        .CopyToDataTable();
                    // Add desire and checker columns to dt
                    if (!dt.Columns.Contains("desire"))
                        dt.Columns.Add("desire", typeof(string));
                    if (!dt.Columns.Contains("checker"))
                        dt.Columns.Add("checker", typeof(string));

                    var ycDict = SQL.LoadYeuCauDates();


                    dt = filteredRows;

                    dataGridView2.DataSource = dt;
                    // Delay reordering until columns are fully generated
                    dataGridView2.DataBindingComplete += (s, e) =>
                    {
                        try
                        {
                            int columnCount = dataGridView2.Columns.Count;

                            // Gán vị trí cho "desire"
                            if (dataGridView2.Columns.Contains("desire"))
                            {
                                var desireCol = dataGridView2.Columns["desire"];
                                desireCol.DisplayIndex = Math.Min(columnCount - 2, desireCol.Index);
                            }

                            // Gán vị trí cho "checker"
                            if (dataGridView2.Columns.Contains("checker"))
                            {
                                var checkerCol = dataGridView2.Columns["checker"];
                                checkerCol.DisplayIndex = Math.Min(columnCount - 1, checkerCol.Index);
                            }

                            // Điền dữ liệu từ ycDict
                            foreach (DataGridViewRow row in dataGridView2.Rows)
                            {
                                if (row.IsNewRow) continue;

                                string rawLsx = row.Cells["lsx"]?.Value?.ToString() ?? "";
                                string normalizedLsx = rawLsx.Trim().Replace(" ", "").Replace("\u200B", "");

                                if (ycDict.TryGetValue(normalizedLsx, out var dates))
                                {
                                    if (row.Cells["desire"] != null)
                                        row.Cells["desire"].Value = dates.Date1;
                                    if (row.Cells["checker"] != null)
                                        row.Cells["checker"].Value = dates.Date2;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    };
                    dataGridView2.DataBindingComplete += (s, e) =>
                    {
                        try
                        {
                            // Move manually added columns to the end
                            int lastIndex = dataGridView2.Columns.Count - 1;
                            if (dataGridView2.Columns.Contains("desire"))
                                dataGridView2.Columns["desire"].DisplayIndex = lastIndex++;

                        }
                        catch (Exception ex)
                        {
                        }
                    };
                    dataGridView2.DataBindingComplete += (s, e) =>
                    {
                        try
                        {
                            // Move manually added columns to the end
                            int lastIndex = dataGridView2.Columns.Count - 1;
                            if (dataGridView2.Columns.Contains("checker"))
                                dataGridView2.Columns["checker"].DisplayIndex = lastIndex++;

                        }
                        catch (Exception ex)
                        {
                        }
                    };
                    // Fill NgayYeuCau and NgayYeuCauThucTe from SQL


                    // Make all columns readonly
                    foreach (DataGridViewColumn col in dataGridView2.Columns)
                        col.ReadOnly = true;

                    // Rename headers
                    if (dataGridView2.Columns.Contains("lsx"))
                        dataGridView2.Columns["lsx"].HeaderText = "LSX";
                    if (dataGridView2.Columns.Contains("product_code"))
                        dataGridView2.Columns["product_code"].HeaderText = "Mã SP";
                    if (dataGridView2.Columns.Contains("product_name_clean"))
                        dataGridView2.Columns["product_name_clean"].HeaderText = "Tên sản phẩm";
                    if (dataGridView2.Columns.Contains("workorder_name"))
                        dataGridView2.Columns["workorder_name"].HeaderText = "Quy trình hiện tại";
                    if (dataGridView2.Columns.Contains("date_planned_start"))
                        dataGridView2.Columns["date_planned_start"].HeaderText = "Ngày bắt đầu quy trình hiện tại";
                    if (dataGridView2.Columns.Contains("date_planned_finished"))
                        dataGridView2.Columns["date_planned_finished"].HeaderText = "Ngày kết thúc trên hệ thống";
                    if (dataGridView2.Columns.Contains("production_qty"))
                        dataGridView2.Columns["production_qty"].HeaderText = "Số lượng";
                    if (dataGridView2.Columns.Contains("order_name"))
                        dataGridView2.Columns["order_name"].HeaderText = "Số HĐ";
                    if (dataGridView2.Columns.Contains("state"))
                        dataGridView2.Columns["state"].Visible = false;
                    if (dataGridView2.Columns.Contains("product_name"))
                        dataGridView2.Columns["product_name"].Visible = false;


                    if (int.Parse(Session.CurrentUser.Quyenhan) == 2 || int.Parse(Session.CurrentUser.Quyenhan) > 4)
                    {
                        if (dataGridView2.Columns.Contains("desire"))
                            dataGridView2.Columns["desire"].ReadOnly = false;
                    }
                    if (int.Parse(Session.CurrentUser.Quyenhan) == 3 || int.Parse(Session.CurrentUser.Quyenhan) > 4)
                    {

                        if (dataGridView2.Columns.Contains("checker"))
                            dataGridView2.Columns["checker"].ReadOnly = false;
                    }


                    // Reorder preferred columns to the beginning
                    string[] preferredOrder = {
                "lsx", "product_code", "product_name_clean",
                "workorder_name", "date_planned_start", "date_planned_finished"
            };
                    int displayIndex = 0;
                    foreach (string colName in preferredOrder)
                    {
                        if (dataGridView2.Columns.Contains(colName))
                            dataGridView2.Columns[colName].DisplayIndex = displayIndex++;
                    }


                    allWorkorders = dt.Copy(); // for filtering
                }

                dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }


        private void ShowWorkordersByLSX(string lsx)
        {
            string query = @"
                SELECT DISTINCT
                    wo.id AS workorder_id,
                    wo.name AS workorder_name,
                    wo.qty_produced,
                    wo.state
                FROM mrp_workorder wo
                JOIN mrp_production_data pd ON pd.production_id = wo.production_id
                JOIN mrp_production mp ON pd.production_id = mp.id
                WHERE mp.sophieu = @lsx
                ORDER BY wo.id;
            ";

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@lsx", lsx);
                using (var adapter = new NpgsqlDataAdapter(cmd))
                {
                    DataTable dta = new DataTable();
                    adapter.Fill(dta);
                    dataGridView2.DataSource = dta;

                    FormatStateColors();

                    foreach (DataGridViewColumn col in dataGridView2.Columns)
                    {
                        col.ReadOnly = true;
                    }

                    // Hide manually added columns if they exist
                    if (dataGridView2.Columns.Contains("desire"))
                        dataGridView2.Columns["desire"].Visible = false;
                    if (dataGridView2.Columns.Contains("checker"))
                        dataGridView2.Columns["checker"].Visible = false;

                    // Rename headers
                    if (dataGridView2.Columns.Contains("workorder_id"))
                        dataGridView2.Columns["workorder_id"].HeaderText = "Mã công đoạn";
                    if (dataGridView2.Columns.Contains("workorder_name"))
                        dataGridView2.Columns["workorder_name"].HeaderText = "Quy trình hiện tại";
                    if (dataGridView2.Columns.Contains("qty_produced"))
                        dataGridView2.Columns["qty_produced"].HeaderText = "Số lượng đã sản xuất";
                }
            }
        }




        private void FormatStateColors()
        {
            foreach (DataGridViewRow row in dataGridView2.Rows)
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

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private void tab1_Click(object sender, EventArgs e) { }
        private void tab2_Click(object sender, EventArgs e) { }
        private void tab3_Click(object sender, EventArgs e) { }
        private void tab4_Click(object sender, EventArgs e) { }
        private void tab5_Click(object sender, EventArgs e) { }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentRow != null)
            {
                var lsx = dataGridView2.CurrentRow.Cells["lsx"].Value?.ToString();
                if (!string.IsNullOrEmpty(lsx))
                {
                    ShowWorkordersByLSX(lsx);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (allWorkorders == null) return;

            string filterText = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(filterText))
            {
                // Show full list
                dataGridView2.DataSource = allWorkorders;
            }
            else
            {
                // Just filter the DataView; do not trigger any tab or detailed view
                DataView view = new DataView(allWorkorders);
                view.RowFilter = $"lsx LIKE '%{filterText.Replace("'", "''")}%'";

                dataGridView2.DataSource = view;
            }
        }

        private void btnSaveYeuCau_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.IsNewRow) continue;

                string lsx = row.Cells["lsx"]?.Value?.ToString();
                string date1 = row.Cells["desire"]?.Value?.ToString();
                string date2 = row.Cells["checker"]?.Value?.ToString();
                try
                {
                    SQL.SaveYeuCauDates(lsx, date1, date2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi lưu LSX {lsx}: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            MessageBox.Show("Đã lưu các ngày yêu cầu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void chkCopyToAll_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một dòng trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] columns = { "desire", "checker" };

            foreach (string column in columns)
            {
                var value = dataGridView2.CurrentRow.Cells[column]?.Value?.ToString();
                var orderName = dataGridView2.CurrentRow.Cells["order_name"]?.Value?.ToString();

                if (!string.IsNullOrEmpty(orderName))
                {
                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        if (row.IsNewRow || row == dataGridView2.CurrentRow) continue;

                        if (row.Cells["order_name"]?.Value?.ToString() == orderName)
                        {
                            row.Cells[column].Value = value;
                        }
                    }
                }
            }

            MessageBox.Show("Đã sao chép dữ liệu sang các dòng cùng Số HĐ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
