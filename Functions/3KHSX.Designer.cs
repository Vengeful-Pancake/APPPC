namespace APPPC.Functions
{
    partial class KHSX
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
            tab1 = new Button();
            label1 = new Label();
            tab2 = new Button();
            tab3 = new Button();
            tab4 = new Button();
            tab5 = new Button();
            tab6 = new Button();
            dataGridView2 = new DataGridView();
            desire = new DataGridViewTextBoxColumn();
            checker = new DataGridViewTextBoxColumn();
            textBox1 = new TextBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            button1 = new Button();
            btnSaveYeuCau = new Button();
            chkCopyToAll = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
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
            tab1.Text = "KHSX bán hàng";
            tab1.UseVisualStyleBackColor = true;
            tab1.Click += tab1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Comic Sans MS", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(2, 26);
            label1.Name = "label1";
            label1.Size = new Size(238, 35);
            label1.TabIndex = 9;
            label1.Text = "Kế hoạch bán hàng";
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
            tab2.Text = "KHSX theo máy";
            tab2.UseVisualStyleBackColor = true;
            tab2.Visible = false;
            tab2.Click += tab2_Click;
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
            tab3.Text = "Xem KHSX";
            tab3.UseVisualStyleBackColor = true;
            tab3.Visible = false;
            tab3.Click += tab3_Click;
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
            tab4.Text = "Xem công tháng";
            tab4.UseVisualStyleBackColor = true;
            tab4.Visible = false;
            tab4.Click += tab4_Click;
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
            tab5.Visible = false;
            tab5.Click += tab5_Click;
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
            tab6.Visible = false;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.BackgroundColor = SystemColors.ButtonHighlight;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Columns.AddRange(new DataGridViewColumn[] { desire, checker });
            dataGridView2.Location = new Point(3, 206);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.Size = new Size(1334, 621);
            dataGridView2.TabIndex = 16;
            dataGridView2.CellContentClick += dataGridView2_CellContentClick;
            // 
            // desire
            // 
            desire.HeaderText = "Ngày Yêu Cầu";
            desire.Name = "desire";
            // 
            // checker
            // 
            checker.HeaderText = "Ngày Yêu Cầu Thực Tế";
            checker.Name = "checker";
            checker.Resizable = DataGridViewTriState.True;
            checker.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox1.Location = new Point(3, 165);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(406, 26);
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
            chkCopyToAll.Location = new Point(836, 147);
            chkCopyToAll.Name = "chkCopyToAll";
            chkCopyToAll.Size = new Size(218, 44);
            chkCopyToAll.TabIndex = 22;
            chkCopyToAll.Text = "Copy vào số hđ giống nhau";
            chkCopyToAll.UseVisualStyleBackColor = true;
            chkCopyToAll.Click += this.chkCopyToAll_Click;
            // 
            // KHSX
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(chkCopyToAll);
            Controls.Add(btnSaveYeuCau);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(dataGridView2);
            Controls.Add(tab6);
            Controls.Add(tab5);
            Controls.Add(tab4);
            Controls.Add(tab3);
            Controls.Add(tab2);
            Controls.Add(label1);
            Controls.Add(tab1);
            Name = "KHSX";
            Size = new Size(1340, 830);
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
        private Button tab6;
        private DataGridView dataGridView2;
        private DataGridView dataGridView1;
        private TextBox textBox1;
        private ContextMenuStrip contextMenuStrip1;
        private DataGridViewTextBoxColumn desire;
        private DataGridViewTextBoxColumn checker;
        private Button button1;
        private Button btnSaveYeuCau;
        private Button chkCopyToAll;
    }
}
