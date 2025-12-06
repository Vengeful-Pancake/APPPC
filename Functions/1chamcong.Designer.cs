namespace APPPC.Functions
{
    partial class chamcong
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            dataGridView1 = new DataGridView();
            tab1 = new Button();
            label1 = new Label();
            tab2 = new Button();
            tab3 = new Button();
            tab4 = new Button();
            tab5 = new Button();
            label2 = new Label();
            dateTimePicker1 = new DateTimePicker();
            comboBox1 = new ComboBox();
            dataGridView2 = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            TotalWorkHour = new DataGridViewTextBoxColumn();
            TotalExtraHour = new DataGridViewTextBoxColumn();
            TotalWorkDay = new DataGridViewTextBoxColumn();
            TotalExtraDay = new DataGridViewTextBoxColumn();
            TotalDay = new DataGridViewTextBoxColumn();
            OffDay = new DataGridViewTextBoxColumn();
            btnExport = new Button();
            btnSave = new Button();
            btnExportNS = new Button();
            STT = new DataGridViewTextBoxColumn();
            MSNV = new DataGridViewTextBoxColumn();
            HoTen = new DataGridViewTextBoxColumn();
            Date = new DataGridViewTextBoxColumn();
            WorkHour = new DataGridViewTextBoxColumn();
            ExtraHour = new DataGridViewTextBoxColumn();
            Absent = new DataGridViewComboBoxColumn();
            ToNhom = new DataGridViewTextBoxColumn();
            Note = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Raised;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Arial", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { STT, MSNV, HoTen, Date, WorkHour, ExtraHour, Absent, ToNhom, Note });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Arial", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.Padding = new Padding(0, 0, 3, 3);
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            dataGridView1.GridColor = Color.DarkOrchid;
            dataGridView1.Location = new Point(12, 178);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Sunken;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new Size(1318, 621);
            dataGridView1.TabIndex = 8;
            // 
            // tab1
            // 
            tab1.BackgroundImage = Properties.Resources.choose0;
            tab1.FlatAppearance.CheckedBackColor = Color.DarkViolet;
            tab1.FlatAppearance.MouseDownBackColor = Color.DarkOrchid;
            tab1.FlatAppearance.MouseOverBackColor = Color.Violet;
            tab1.Font = new Font("Arial Narrow", 11.25F);
            tab1.ForeColor = SystemColors.Window;
            tab1.Location = new Point(12, 70);
            tab1.Name = "tab1";
            tab1.Size = new Size(200, 50);
            tab1.TabIndex = 0;
            tab1.Text = "Bảng chấm công ngày";
            tab1.UseVisualStyleBackColor = true;
            tab1.Click += tab1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial Narrow", 21.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(2, 26);
            label1.Name = "label1";
            label1.Size = new Size(83, 33);
            label1.TabIndex = 1;
            label1.Text = "label1";
            // 
            // tab2
            // 
            tab2.BackgroundImage = Properties.Resources.choose0;
            tab2.FlatAppearance.CheckedBackColor = Color.DarkViolet;
            tab2.FlatAppearance.MouseDownBackColor = Color.DarkOrchid;
            tab2.FlatAppearance.MouseOverBackColor = Color.Violet;
            tab2.Font = new Font("Arial Narrow", 11.25F);
            tab2.ForeColor = SystemColors.Window;
            tab2.Location = new Point(218, 70);
            tab2.Name = "tab2";
            tab2.Size = new Size(200, 50);
            tab2.TabIndex = 2;
            tab2.Text = "Bảng chấm công tháng";
            tab2.UseVisualStyleBackColor = true;
            tab2.Click += tab2_Click;
            // 
            // tab3
            // 
            tab3.BackgroundImage = Properties.Resources.choose0;
            tab3.FlatAppearance.CheckedBackColor = Color.DarkViolet;
            tab3.FlatAppearance.MouseDownBackColor = Color.DarkOrchid;
            tab3.FlatAppearance.MouseOverBackColor = Color.Violet;
            tab3.Font = new Font("Arial Narrow", 11.25F);
            tab3.ForeColor = SystemColors.Window;
            tab3.Location = new Point(424, 70);
            tab3.Name = "tab3";
            tab3.Size = new Size(200, 50);
            tab3.TabIndex = 3;
            tab3.Text = "Xem ngày công";
            tab3.UseVisualStyleBackColor = true;
            tab3.Click += tab3_Click;
            // 
            // tab4
            // 
            tab4.BackgroundImage = Properties.Resources.choose0;
            tab4.FlatAppearance.CheckedBackColor = Color.DarkViolet;
            tab4.FlatAppearance.MouseDownBackColor = Color.DarkOrchid;
            tab4.FlatAppearance.MouseOverBackColor = Color.Violet;
            tab4.Font = new Font("Arial Narrow", 11.25F);
            tab4.ForeColor = SystemColors.Window;
            tab4.Location = new Point(630, 70);
            tab4.Name = "tab4";
            tab4.Size = new Size(200, 50);
            tab4.TabIndex = 4;
            tab4.Text = "Xem công tháng";
            tab4.UseVisualStyleBackColor = true;
            tab4.Click += tab4_Click;
            // 
            // tab5
            // 
            tab5.BackgroundImage = Properties.Resources.choose0;
            tab5.FlatAppearance.CheckedBackColor = Color.DarkViolet;
            tab5.FlatAppearance.MouseDownBackColor = Color.DarkOrchid;
            tab5.FlatAppearance.MouseOverBackColor = Color.Violet;
            tab5.Font = new Font("Arial Narrow", 11.25F);
            tab5.ForeColor = SystemColors.Window;
            tab5.Location = new Point(836, 70);
            tab5.Name = "tab5";
            tab5.Size = new Size(200, 50);
            tab5.TabIndex = 5;
            tab5.Text = "Tổng công tháng";
            tab5.UseVisualStyleBackColor = true;
            tab5.Click += tab5_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(5, 802);
            label2.Name = "label2";
            label2.Size = new Size(1250, 25);
            label2.TabIndex = 7;
            label2.Text = "* Xem/ Chấm công ngày cho tất cả nhân viên trong ngày được chọn, xem/ chấm công tháng cho một (1) nhân viên trong cả tháng.";
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.CalendarFont = new Font("Comic Sans MS", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dateTimePicker1.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dateTimePicker1.Format = DateTimePickerFormat.Short;
            dateTimePicker1.Location = new Point(12, 138);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(99, 21);
            dateTimePicker1.TabIndex = 9;
            dateTimePicker1.ValueChanged += dateTimePicker1_ValueChanged;
            // 
            // comboBox1
            // 
            comboBox1.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(162, 138);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(248, 23);
            comboBox1.TabIndex = 11;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.AllowUserToOrderColumns = true;
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.Raised;
            dataGridView2.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("Arial", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dataGridView2.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Columns.AddRange(new DataGridViewColumn[] { Column1, dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, TotalWorkHour, TotalExtraHour, TotalWorkDay, TotalExtraDay, TotalDay, OffDay });
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = SystemColors.Window;
            dataGridViewCellStyle4.Font = new Font("Arial", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle4.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle4.Padding = new Padding(0, 0, 3, 3);
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            dataGridView2.DefaultCellStyle = dataGridViewCellStyle4;
            dataGridView2.GridColor = Color.DarkOrchid;
            dataGridView2.Location = new Point(162, 238);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Sunken;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.Size = new Size(1318, 621);
            dataGridView2.TabIndex = 12;
            dataGridView2.Visible = false;
            dataGridView2.CellContentClick += dataGridView2_CellContentClick;
            // 
            // Column1
            // 
            Column1.HeaderText = "STT";
            Column1.Name = "Column1";
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewTextBoxColumn1.HeaderText = "MSNV";
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            dataGridViewTextBoxColumn1.Width = 79;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewTextBoxColumn2.HeaderText = "Họ Tên";
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.ReadOnly = true;
            dataGridViewTextBoxColumn2.SortMode = DataGridViewColumnSortMode.Programmatic;
            dataGridViewTextBoxColumn2.Width = 82;
            // 
            // TotalWorkHour
            // 
            TotalWorkHour.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            TotalWorkHour.HeaderText = "Tổng giờ làm";
            TotalWorkHour.Name = "TotalWorkHour";
            TotalWorkHour.Width = 82;
            // 
            // TotalExtraHour
            // 
            TotalExtraHour.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            TotalExtraHour.HeaderText = "Tổng giờ làm thêm";
            TotalExtraHour.Name = "TotalExtraHour";
            TotalExtraHour.Width = 85;
            // 
            // TotalWorkDay
            // 
            TotalWorkDay.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            TotalWorkDay.HeaderText = "Tổng công làm tháng";
            TotalWorkDay.Name = "TotalWorkDay";
            TotalWorkDay.Width = 95;
            // 
            // TotalExtraDay
            // 
            TotalExtraDay.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            TotalExtraDay.HeaderText = "Tổng công thêm tháng";
            TotalExtraDay.Name = "TotalExtraDay";
            TotalExtraDay.Width = 107;
            // 
            // TotalDay
            // 
            TotalDay.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            TotalDay.HeaderText = "Tổng công tháng";
            TotalDay.Name = "TotalDay";
            TotalDay.Width = 107;
            // 
            // OffDay
            // 
            OffDay.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            OffDay.HeaderText = "Số ngày vắng";
            OffDay.Name = "OffDay";
            // 
            // btnExport
            // 
            btnExport.BackgroundImage = Properties.Resources.choose0;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnExport.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.Font = new Font("Arial Narrow", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnExport.ForeColor = Color.Transparent;
            btnExport.Location = new Point(1120, 138);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(116, 34);
            btnExport.TabIndex = 13;
            btnExport.Text = "Xuất File";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += button1_Click;
            // 
            // btnSave
            // 
            btnSave.BackgroundImage = Properties.Resources.choose0;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnSave.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Arial Narrow", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnSave.ForeColor = Color.Transparent;
            btnSave.Location = new Point(1242, 138);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(88, 34);
            btnSave.TabIndex = 10;
            btnSave.Text = "Lưu";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnExportNS
            // 
            btnExportNS.BackgroundImage = Properties.Resources.choose0;
            btnExportNS.FlatAppearance.BorderSize = 0;
            btnExportNS.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnExportNS.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnExportNS.FlatStyle = FlatStyle.Flat;
            btnExportNS.Font = new Font("Arial Narrow", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnExportNS.ForeColor = Color.Transparent;
            btnExportNS.Location = new Point(998, 138);
            btnExportNS.Name = "btnExportNS";
            btnExportNS.Size = new Size(116, 34);
            btnExportNS.TabIndex = 14;
            btnExportNS.Text = "Xuất File NS";
            btnExportNS.UseVisualStyleBackColor = true;
            btnExportNS.Visible = false;
            btnExportNS.Click += btnExportNS_Click;
            // 
            // STT
            // 
            STT.HeaderText = "STT";
            STT.Name = "STT";
            STT.Width = 66;
            // 
            // MSNV
            // 
            MSNV.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            MSNV.HeaderText = "MSNV";
            MSNV.Name = "MSNV";
            MSNV.ReadOnly = true;
            MSNV.Width = 79;
            // 
            // HoTen
            // 
            HoTen.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            HoTen.HeaderText = "Họ Tên";
            HoTen.Name = "HoTen";
            HoTen.ReadOnly = true;
            HoTen.SortMode = DataGridViewColumnSortMode.Programmatic;
            HoTen.Width = 88;
            // 
            // Date
            // 
            Date.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Date.HeaderText = "Ngày làm";
            Date.Name = "Date";
            Date.ReadOnly = true;
            Date.Width = 99;
            // 
            // WorkHour
            // 
            WorkHour.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            WorkHour.HeaderText = "Giờ làm";
            WorkHour.Name = "WorkHour";
            WorkHour.Width = 91;
            // 
            // ExtraHour
            // 
            ExtraHour.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            ExtraHour.HeaderText = "Làm thêm";
            ExtraHour.Name = "ExtraHour";
            ExtraHour.Width = 104;
            // 
            // Absent
            // 
            Absent.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Absent.HeaderText = "Vắng";
            Absent.Items.AddRange(new object[] { "", "P", "VR", "TU", "BH" });
            Absent.Name = "Absent";
            Absent.Resizable = DataGridViewTriState.True;
            Absent.SortMode = DataGridViewColumnSortMode.Automatic;
            Absent.Width = 71;
            // 
            // ToNhom
            // 
            ToNhom.HeaderText = "Tổ Nhóm";
            ToNhom.Name = "ToNhom";
            ToNhom.ReadOnly = true;
            // 
            // Note
            // 
            Note.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Note.HeaderText = "Ghi chú";
            Note.Name = "Note";
            // 
            // chamcong
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnExportNS);
            Controls.Add(btnExport);
            Controls.Add(dataGridView2);
            Controls.Add(comboBox1);
            Controls.Add(btnSave);
            Controls.Add(dateTimePicker1);
            Controls.Add(dataGridView1);
            Controls.Add(label2);
            Controls.Add(tab5);
            Controls.Add(tab4);
            Controls.Add(tab3);
            Controls.Add(tab2);
            Controls.Add(label1);
            Controls.Add(tab1);
            Name = "chamcong";
            Size = new Size(1340, 830);
            SizeChanged += CC_panel_SizeChanged;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button tab1;
        private Label label1;
        private Button tab2;
        private Button tab3;
        private Button tab4;
        private Button tab5;
        private Label label2;
        private DataGridView dataGridView1;
        private DateTimePicker dateTimePicker1;
        private ComboBox comboBox1;
        private DataGridView dataGridView2;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn TotalWorkHour;
        private DataGridViewTextBoxColumn TotalExtraHour;
        private DataGridViewTextBoxColumn TotalWorkDay;
        private DataGridViewTextBoxColumn TotalExtraDay;
        private DataGridViewTextBoxColumn TotalDay;
        private DataGridViewTextBoxColumn OffDay;
        private Button btnExport;
        private Button btnSave;
        private Button btnExportNS;
        private DataGridViewTextBoxColumn STT;
        private DataGridViewTextBoxColumn MSNV;
        private DataGridViewTextBoxColumn HoTen;
        private DataGridViewTextBoxColumn Date;
        private DataGridViewTextBoxColumn WorkHour;
        private DataGridViewTextBoxColumn ExtraHour;
        private DataGridViewComboBoxColumn Absent;
        private DataGridViewTextBoxColumn ToNhom;
        private DataGridViewTextBoxColumn Note;
    }
}
