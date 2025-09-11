namespace APPPC.Functions
{
    partial class KHSX_panel
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            tab1 = new Button();
            label1 = new Label();
            tab2 = new Button();
            tab3 = new Button();
            dataGridView2 = new DataGridView();
            textBox1 = new TextBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            button1 = new Button();
            btnSaveYeuCau = new Button();
            chkCopyToAll = new Button();
            DateSorter = new ComboBox();
            From = new DateTimePicker();
            To = new DateTimePicker();
            ShowEmpty = new CheckBox();
            ShowOld = new CheckBox();
            tab4 = new Button();
            Default_Ka = new ComboBox();
            searcher = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // tab1
            // 
            tab1.BackgroundImage = Properties.Resources.choose0;
            tab1.FlatAppearance.BorderSize = 0;
            tab1.Font = new Font("Arial", 11.25F, FontStyle.Bold);
            tab1.ForeColor = SystemColors.Window;
            tab1.Location = new Point(12, 70);
            tab1.Name = "tab1";
            tab1.Size = new Size(200, 50);
            tab1.TabIndex = 8;
            tab1.Text = "Lịch Nhận Tuần";
            tab1.UseVisualStyleBackColor = true;
            tab1.Click += tab1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(2, 26);
            label1.Name = "label1";
            label1.Size = new Size(186, 29);
            label1.TabIndex = 9;
            label1.Text = "Lịch nhận tuần";
            // 
            // tab2
            // 
            tab2.BackgroundImage = Properties.Resources.choose0;
            tab2.FlatAppearance.BorderSize = 0;
            tab2.Font = new Font("Arial", 11.25F, FontStyle.Bold);
            tab2.ForeColor = SystemColors.Window;
            tab2.Location = new Point(218, 70);
            tab2.Name = "tab2";
            tab2.Size = new Size(200, 50);
            tab2.TabIndex = 10;
            tab2.Text = "KHSX theo Công Đoạn";
            tab2.UseVisualStyleBackColor = true;
            tab2.Click += tab2_Click;
            // 
            // tab3
            // 
            tab3.BackgroundImage = Properties.Resources.choose0;
            tab3.FlatAppearance.BorderSize = 0;
            tab3.Font = new Font("Arial", 11.25F, FontStyle.Bold);
            tab3.ForeColor = SystemColors.Window;
            tab3.Location = new Point(424, 70);
            tab3.Name = "tab3";
            tab3.Size = new Size(200, 50);
            tab3.TabIndex = 11;
            tab3.Text = "KHSX máy";
            tab3.UseVisualStyleBackColor = true;
            tab3.Click += tab3_Click;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.BackgroundColor = SystemColors.ButtonHighlight;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Location = new Point(3, 206);
            dataGridView2.Name = "dataGridView2";
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("Arial", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dataGridView2.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.Size = new Size(1334, 621);
            dataGridView2.TabIndex = 16;
            dataGridView2.CellContentClick +=  dataGridView2_CellContentClick;
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox1.Location = new Point(3, 165);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(121, 26);
            textBox1.TabIndex = 18;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // button1
            // 
            button1.Font = new Font("Arial", 9F);
            button1.Location = new Point(1073, 152);
            button1.Name = "button1";
            button1.Size = new Size(129, 39);
            button1.TabIndex = 19;
            button1.Text = "Kiểm tra";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // btnSaveYeuCau
            // 
            btnSaveYeuCau.Font = new Font("Arial", 9F);
            btnSaveYeuCau.Location = new Point(1208, 152);
            btnSaveYeuCau.Name = "btnSaveYeuCau";
            btnSaveYeuCau.Size = new Size(129, 39);
            btnSaveYeuCau.TabIndex = 20;
            btnSaveYeuCau.Text = "Lưu";
            btnSaveYeuCau.UseVisualStyleBackColor = true;
            btnSaveYeuCau.Click += btnSaveYeuCau_Click;
            // 
            // chkCopyToAll
            // 
            chkCopyToAll.Font = new Font("Arial", 9F);
            chkCopyToAll.Location = new Point(879, 152);
            chkCopyToAll.Name = "chkCopyToAll";
            chkCopyToAll.Size = new Size(188, 37);
            chkCopyToAll.TabIndex = 22;
            chkCopyToAll.Text = "Copy vào ĐH tương tự";
            chkCopyToAll.UseVisualStyleBackColor = true;
            chkCopyToAll.Click += chkCopyToAll_Click;
            // 
            // DateSorter
            // 
            DateSorter.FormattingEnabled = true;
            DateSorter.Location = new Point(659, 126);
            DateSorter.Name = "DateSorter";
            DateSorter.Size = new Size(214, 23);
            DateSorter.TabIndex = 25;
            DateSorter.SelectedIndexChanged += DateSorter_SelectedIndexChanged;
            // 
            // From
            // 
            From.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            From.Location = new Point(605, 165);
            From.Name = "From";
            From.Size = new Size(131, 21);
            From.TabIndex = 26;
            From.ValueChanged += From_ValueChanged;
            // 
            // To
            // 
            To.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            To.Location = new Point(742, 165);
            To.Name = "To";
            To.Size = new Size(131, 21);
            To.TabIndex = 27;
            To.ValueChanged += To_ValueChanged;
            // 
            // ShowEmpty
            // 
            ShowEmpty.AutoSize = true;
            ShowEmpty.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ShowEmpty.Location = new Point(12, 126);
            ShowEmpty.Name = "ShowEmpty";
            ShowEmpty.Size = new Size(112, 19);
            ShowEmpty.TabIndex = 28;
            ShowEmpty.Text = "Hiện LSX Trống";
            ShowEmpty.UseVisualStyleBackColor = true;
            ShowEmpty.CheckedChanged += ShowEmpty_CheckedChanged;
            // 
            // ShowOld
            // 
            ShowOld.AutoSize = true;
            ShowOld.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ShowOld.Location = new Point(12, 146);
            ShowOld.Name = "ShowOld";
            ShowOld.Size = new Size(166, 19);
            ShowOld.TabIndex = 29;
            ShowOld.Text = "Hiện LSX Cũ (quá 1 năm)";
            ShowOld.UseVisualStyleBackColor = true;
            ShowOld.CheckedChanged += ShowOld_CheckedChanged;
            // 
            // tab4
            // 
            tab4.BackgroundImage = Properties.Resources.choose0;
            tab4.FlatAppearance.BorderSize = 0;
            tab4.Font = new Font("Arial", 11.25F, FontStyle.Bold);
            tab4.ForeColor = SystemColors.Window;
            tab4.Location = new Point(630, 70);
            tab4.Name = "tab4";
            tab4.Size = new Size(200, 50);
            tab4.TabIndex = 30;
            tab4.Text = "Sắp xếp Ka làm việc";
            tab4.UseVisualStyleBackColor = true;
            tab4.Click += tab4_Click;
            // 
            // Default_Ka
            // 
            Default_Ka.FormattingEnabled = true;
            Default_Ka.Location = new Point(439, 125);
            Default_Ka.Name = "Default_Ka";
            Default_Ka.Size = new Size(214, 23);
            Default_Ka.TabIndex = 31;
            Default_Ka.SelectedIndexChanged += Default_Ka_SelectedIndexChanged;
            // 
            // searcher
            // 
            searcher.FormattingEnabled = true;
            searcher.Location = new Point(130, 167);
            searcher.Name = "searcher";
            searcher.Size = new Size(108, 23);
            searcher.TabIndex = 32;
            searcher.SelectedIndexChanged += searcher_SelectedIndexChanged;
            // 
            // KHSX_panel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(searcher);
            Controls.Add(Default_Ka);
            Controls.Add(tab4);
            Controls.Add(ShowOld);
            Controls.Add(ShowEmpty);
            Controls.Add(To);
            Controls.Add(From);
            Controls.Add(DateSorter);
            Controls.Add(chkCopyToAll);
            Controls.Add(btnSaveYeuCau);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(dataGridView2);
            Controls.Add(tab3);
            Controls.Add(tab2);
            Controls.Add(label1);
            Controls.Add(tab1);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "KHSX_panel";
            Size = new Size(1340, 830);
            SizeChanged += KHSX_panel_SizeChanged;
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button tab1;
        private Label label1;
        private Button tab2;
        private Button tab3;
        private DataGridView dataGridView2;
        private DataGridView dataGridView1;
        private TextBox textBox1;
        private ContextMenuStrip contextMenuStrip1;
        private Button button1;
        private Button btnSaveYeuCau;
        private Button chkCopyToAll;
        private ComboBox DateSorter;
        private DateTimePicker From;
        private DateTimePicker To;
        private CheckBox ShowEmpty;
        private CheckBox ShowOld;
        private Button tab4;
        private ComboBox Default_Ka;
        private ComboBox searcher;
    }
}
