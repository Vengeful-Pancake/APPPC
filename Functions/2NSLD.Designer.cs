namespace APPPC.Functions
{
    partial class NSLD
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
            tab6 = new Button();
            tab5 = new Button();
            tab4 = new Button();
            tab3 = new Button();
            tab2 = new Button();
            label1 = new Label();
            tab1 = new Button();
            dataGridView1 = new DataGridView();
            MSNV = new DataGridViewTextBoxColumn();
            HoTen = new DataGridViewTextBoxColumn();
            Date = new DataGridViewTextBoxColumn();
            machine = new DataGridViewComboBoxColumn();
            Total = new DataGridViewTextBoxColumn();
            Extra = new DataGridViewComboBoxColumn();
            Note = new DataGridViewComboBoxColumn();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // tab6
            // 
            tab6.BackgroundImage = Properties.Resources.bar;
            tab6.FlatAppearance.BorderSize = 0;
            tab6.Font = new Font("Comic Sans MS", 11.25F);
            tab6.ForeColor = SystemColors.Window;
            tab6.Location = new Point(1042, 70);
            tab6.Name = "tab6";
            tab6.Size = new Size(200, 50);
            tab6.TabIndex = 14;
            tab6.Text = "button4";
            tab6.UseVisualStyleBackColor = true;
            // 
            // tab5
            // 
            tab5.BackgroundImage = Properties.Resources.bar;
            tab5.FlatAppearance.BorderSize = 0;
            tab5.Font = new Font("Comic Sans MS", 11.25F);
            tab5.ForeColor = SystemColors.Window;
            tab5.Location = new Point(836, 70);
            tab5.Name = "tab5";
            tab5.Size = new Size(200, 50);
            tab5.TabIndex = 13;
            tab5.Text = "button4";
            tab5.UseVisualStyleBackColor = true;
            // 
            // tab4
            // 
            tab4.BackgroundImage = Properties.Resources.bar;
            tab4.FlatAppearance.BorderSize = 0;
            tab4.Font = new Font("Comic Sans MS", 11.25F);
            tab4.ForeColor = SystemColors.Window;
            tab4.Location = new Point(630, 70);
            tab4.Name = "tab4";
            tab4.Size = new Size(200, 50);
            tab4.TabIndex = 12;
            tab4.Text = "Xem năng suất tháng";
            tab4.UseVisualStyleBackColor = true;
            // 
            // tab3
            // 
            tab3.BackgroundImage = Properties.Resources.bar;
            tab3.FlatAppearance.BorderSize = 0;
            tab3.Font = new Font("Comic Sans MS", 11.25F);
            tab3.ForeColor = SystemColors.Window;
            tab3.Location = new Point(424, 70);
            tab3.Name = "tab3";
            tab3.Size = new Size(200, 50);
            tab3.TabIndex = 11;
            tab3.Text = "Xem năng suất ngày";
            tab3.UseVisualStyleBackColor = true;
            // 
            // tab2
            // 
            tab2.BackgroundImage = Properties.Resources.bar;
            tab2.FlatAppearance.BorderSize = 0;
            tab2.Font = new Font("Comic Sans MS", 11.25F);
            tab2.ForeColor = SystemColors.Window;
            tab2.Location = new Point(218, 70);
            tab2.Name = "tab2";
            tab2.Size = new Size(200, 50);
            tab2.TabIndex = 10;
            tab2.Text = "Bảng năng suất tháng";
            tab2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Comic Sans MS", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(2, 26);
            label1.Name = "label1";
            label1.Size = new Size(84, 35);
            label1.TabIndex = 9;
            label1.Text = "label1";
            // 
            // tab1
            // 
            tab1.BackgroundImage = Properties.Resources.bar;
            tab1.FlatAppearance.BorderSize = 0;
            tab1.Font = new Font("Comic Sans MS", 11.25F);
            tab1.ForeColor = SystemColors.Window;
            tab1.Location = new Point(12, 70);
            tab1.Name = "tab1";
            tab1.Size = new Size(200, 50);
            tab1.TabIndex = 8;
            tab1.Text = "Bảng năng suất ngày";
            tab1.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { MSNV, HoTen, Date, machine, Total, Extra, Note });
            dataGridView1.GridColor = Color.DarkOrchid;
            dataGridView1.Location = new Point(12, 178);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Sunken;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new Size(1318, 621);
            dataGridView1.TabIndex = 15;
            // 
            // MSNV
            // 
            MSNV.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            MSNV.HeaderText = "MSNV";
            MSNV.Name = "MSNV";
            MSNV.ReadOnly = true;
            MSNV.Width = 65;
            // 
            // HoTen
            // 
            HoTen.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            HoTen.HeaderText = "Họ Tên";
            HoTen.Name = "HoTen";
            HoTen.ReadOnly = true;
            HoTen.SortMode = DataGridViewColumnSortMode.Programmatic;
            HoTen.Width = 70;
            // 
            // Date
            // 
            Date.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Date.HeaderText = "Ngày làm";
            Date.Name = "Date";
            Date.ReadOnly = true;
            Date.Width = 83;
            // 
            // machine
            // 
            machine.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            machine.HeaderText = "Máy";
            machine.Name = "machine";
            machine.Resizable = DataGridViewTriState.True;
            machine.SortMode = DataGridViewColumnSortMode.Automatic;
            machine.Width = 55;
            // 
            // Total
            // 
            Total.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Total.HeaderText = "Sản lượng";
            Total.Name = "Total";
            Total.Width = 85;
            // 
            // Extra
            // 
            Extra.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Extra.HeaderText = "Canh lề";
            Extra.Name = "Extra";
            Extra.Width = 53;
            // 
            // Note
            // 
            Note.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Note.HeaderText = "Ghi Chú";
            Note.Name = "Note";
            // 
            // button1
            // 
            button1.BackgroundImage = Properties.Resources.bar;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatAppearance.MouseDownBackColor = Color.Transparent;
            button1.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Comic Sans MS", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button1.ForeColor = Color.Transparent;
            button1.Location = new Point(1242, 138);
            button1.Name = "button1";
            button1.Size = new Size(88, 34);
            button1.TabIndex = 16;
            button1.Text = "Lưu";
            button1.UseVisualStyleBackColor = true;
            // 
            // NSLD
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(button1);
            Controls.Add(dataGridView1);
            Controls.Add(tab6);
            Controls.Add(tab5);
            Controls.Add(tab4);
            Controls.Add(tab3);
            Controls.Add(tab2);
            Controls.Add(label1);
            Controls.Add(tab1);
            Name = "NSLD";
            Size = new Size(1340, 830);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button tab6;
        private Button tab5;
        private Button tab4;
        private Button tab3;
        private Button tab2;
        private Label label1;
        private Button tab1;
        private DataGridView dataGridView1;
        private Button button1;
        private DataGridViewTextBoxColumn MSNV;
        private DataGridViewTextBoxColumn HoTen;
        private DataGridViewTextBoxColumn Date;
        private DataGridViewComboBoxColumn machine;
        private DataGridViewTextBoxColumn Total;
        private DataGridViewComboBoxColumn Extra;
        private DataGridViewComboBoxColumn Note;
    }
}
