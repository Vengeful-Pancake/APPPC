namespace APPPC.Functions
{
    partial class KHSX1
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
            dataGridView1 = new DataGridView();
            Date = new DataGridViewTextBoxColumn();
            Total = new DataGridViewTextBoxColumn();
            Not = new DataGridViewTextBoxColumn();
            ClosestDate = new DataGridViewTextBoxColumn();
            Status = new DataGridViewTextBoxColumn();
            Note = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Date, Total, Not, ClosestDate, Status, Note });
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(1310, 588);
            dataGridView1.TabIndex = 0;
            // 
            // Date
            // 
            Date.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Date.HeaderText = "Ngày cần lập kế hoạch";
            Date.Name = "Date";
            Date.Width = 109;
            // 
            // Total
            // 
            Total.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Total.HeaderText = "Tổng kế hoạch cần lập";
            Total.Name = "Total";
            Total.Width = 124;
            // 
            // Not
            // 
            Not.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Not.HeaderText = "Chưa hoàn thành";
            Not.Name = "Not";
            Not.Width = 114;
            // 
            // ClosestDate
            // 
            ClosestDate.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            ClosestDate.HeaderText = "Ngày cận deadline";
            ClosestDate.Name = "ClosestDate";
            ClosestDate.Width = 119;
            // 
            // Status
            // 
            Status.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Status.HeaderText = "Tình trạng";
            Status.Name = "Status";
            Status.Width = 80;
            // 
            // Note
            // 
            Note.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Note.HeaderText = "Ghi chú";
            Note.Name = "Note";
            // 
            // KHSX1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(dataGridView1);
            Name = "KHSX1";
            Size = new Size(1310, 588);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn Date;
        private DataGridViewTextBoxColumn Total;
        private DataGridViewTextBoxColumn Not;
        private DataGridViewTextBoxColumn ClosestDate;
        private DataGridViewTextBoxColumn Status;
        private DataGridViewTextBoxColumn Note;
    }
}
