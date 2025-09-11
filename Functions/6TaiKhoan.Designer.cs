namespace APPPC.Functions
{
    partial class TaiKhoan

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
            label1 = new Label();
            btnChangePass = new Button();
            checkBox1 = new CheckBox();
            newpass2 = new TextBox();
            passchange = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            infochange = new Label();
            newpass = new TextBox();
            old_pass = new TextBox();
            chosenMSNV = new Label();
            chosengroup = new Label();
            chosenleader = new Label();
            btnChangeInfo = new Button();
            comboBox1 = new ComboBox();
            comboBox2 = new ComboBox();
            comboBox3 = new ComboBox();
            leaders = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            members = new DataGridView();
            group = new Label();
            comboBox4 = new ComboBox();
            comboBox5 = new ComboBox();
            btnAddMember = new Button();
            addleader = new Label();
            addgroup = new Label();
            adddescript = new Label();
            addmember = new Label();
            newgroup = new CheckBox();
            comboBox9 = new ComboBox();
            btnDeleteMember = new Button();
            deletedmember = new Label();
            deletemember = new Label();
            addname = new Label();
            addmsnv = new Label();
            textBox1 = new TextBox();
            comboBox6 = new ComboBox();
            textBox2 = new TextBox();
            textBox3 = new TextBox();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)leaders).BeginInit();
            ((System.ComponentModel.ISupportInitialize)members).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(2, 24);
            label1.Name = "label1";
            label1.Size = new Size(126, 29);
            label1.TabIndex = 30;
            label1.Text = "Tài khoản";
            // 
            // btnChangePass
            // 
            btnChangePass.Font = new Font("Arial", 11.25F);
            btnChangePass.Location = new Point(560, 239);
            btnChangePass.Name = "btnChangePass";
            btnChangePass.Size = new Size(64, 27);
            btnChangePass.TabIndex = 36;
            btnChangePass.Text = "Lưu";
            btnChangePass.UseVisualStyleBackColor = true;
            btnChangePass.Click += btnChangePass_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(1059, 211);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(73, 18);
            checkBox1.TabIndex = 37;
            checkBox1.Text = "Nhóm mới";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // newpass2
            // 
            newpass2.Location = new Point(165, 212);
            newpass2.Name = "newpass2";
            newpass2.Size = new Size(460, 20);
            newpass2.TabIndex = 38;
            // 
            // passchange
            // 
            passchange.AutoSize = true;
            passchange.Font = new Font("Arial", 20.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            passchange.Location = new Point(10, 65);
            passchange.Name = "passchange";
            passchange.Size = new Size(193, 32);
            passchange.TabIndex = 39;
            passchange.Text = "Đổi mật khẩu:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Arial", 8.25F);
            label3.Location = new Point(37, 121);
            label3.Name = "label3";
            label3.Size = new Size(68, 14);
            label3.TabIndex = 40;
            label3.Text = "Mật khẩu cũ:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Arial", 8.25F);
            label4.Location = new Point(37, 166);
            label4.Name = "label4";
            label4.Size = new Size(73, 14);
            label4.TabIndex = 41;
            label4.Text = "Mật khẩu mới:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Arial", 8.25F);
            label5.Location = new Point(37, 211);
            label5.Name = "label5";
            label5.Size = new Size(102, 14);
            label5.TabIndex = 42;
            label5.Text = "Xác nhận mật khẩu:";
            // 
            // infochange
            // 
            infochange.AutoSize = true;
            infochange.Font = new Font("Arial", 20.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            infochange.Location = new Point(671, 65);
            infochange.Name = "infochange";
            infochange.Size = new Size(319, 32);
            infochange.TabIndex = 43;
            infochange.Text = "Đổi thông tin nhân viên:";
            // 
            // newpass
            // 
            newpass.Location = new Point(165, 167);
            newpass.Name = "newpass";
            newpass.Size = new Size(460, 20);
            newpass.TabIndex = 44;
            // 
            // old_pass
            // 
            old_pass.Location = new Point(165, 119);
            old_pass.Name = "old_pass";
            old_pass.Size = new Size(460, 20);
            old_pass.TabIndex = 45;
            // 
            // chosenMSNV
            // 
            chosenMSNV.AutoSize = true;
            chosenMSNV.Font = new Font("Arial", 8.25F);
            chosenMSNV.Location = new Point(707, 121);
            chosenMSNV.Name = "chosenMSNV";
            chosenMSNV.Size = new Size(85, 14);
            chosenMSNV.TabIndex = 46;
            chosenMSNV.Text = "Chọn nhân viên:";
            // 
            // chosengroup
            // 
            chosengroup.AutoSize = true;
            chosengroup.Font = new Font("Arial", 8.25F);
            chosengroup.Location = new Point(707, 166);
            chosengroup.Name = "chosengroup";
            chosengroup.Size = new Size(37, 14);
            chosengroup.TabIndex = 47;
            chosengroup.Text = "Nhóm:";
            // 
            // chosenleader
            // 
            chosenleader.AutoSize = true;
            chosenleader.Font = new Font("Arial", 8.25F);
            chosenleader.Location = new Point(707, 211);
            chosenleader.Name = "chosenleader";
            chosenleader.Size = new Size(58, 14);
            chosenleader.TabIndex = 48;
            chosenleader.Text = "Tổ trưởng:";
            // 
            // btnChangeInfo
            // 
            btnChangeInfo.Font = new Font("Arial", 11.25F);
            btnChangeInfo.Location = new Point(1066, 234);
            btnChangeInfo.Name = "btnChangeInfo";
            btnChangeInfo.Size = new Size(64, 27);
            btnChangeInfo.TabIndex = 50;
            btnChangeInfo.Text = "Lưu";
            btnChangeInfo.UseVisualStyleBackColor = true;
            btnChangeInfo.Click += btnChangeInfo_Click;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(810, 122);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(321, 22);
            comboBox1.TabIndex = 51;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(810, 163);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(321, 22);
            comboBox2.TabIndex = 52;
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(810, 207);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(244, 22);
            comboBox3.TabIndex = 53;
            // 
            // leaders
            // 
            leaders.AllowUserToAddRows = false;
            leaders.AllowUserToDeleteRows = false;
            leaders.AllowUserToOrderColumns = true;
            leaders.AllowUserToResizeRows = false;
            leaders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            leaders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            leaders.BackgroundColor = SystemColors.Control;
            leaders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            leaders.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2 });
            leaders.Location = new Point(10, 354);
            leaders.MultiSelect = false;
            leaders.Name = "leaders";
            leaders.ReadOnly = true;
            leaders.RowHeadersVisible = false;
            leaders.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            leaders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            leaders.Size = new Size(289, 319);
            leaders.StandardTab = true;
            leaders.TabIndex = 54;
            leaders.CellClick += leaders_CellContentClick;
            leaders.CellContentClick += leaders_CellContentClick_1;
            // 
            // Column1
            // 
            Column1.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column1.HeaderText = "Column1";
            Column1.Name = "Column1";
            Column1.ReadOnly = true;
            Column1.Visible = false;
            // 
            // Column2
            // 
            Column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Column2.HeaderText = "Column2";
            Column2.Name = "Column2";
            Column2.ReadOnly = true;
            Column2.Visible = false;
            // 
            // members
            // 
            members.AllowUserToAddRows = false;
            members.AllowUserToDeleteRows = false;
            members.AllowUserToOrderColumns = true;
            members.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            members.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            members.BackgroundColor = SystemColors.Control;
            members.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            members.EnableHeadersVisualStyles = false;
            members.Location = new Point(304, 354);
            members.MultiSelect = false;
            members.Name = "members";
            members.ReadOnly = true;
            members.RowHeadersVisible = false;
            members.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            members.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            members.Size = new Size(320, 319);
            members.StandardTab = true;
            members.TabIndex = 55;
            // 
            // group
            // 
            group.AutoSize = true;
            group.Font = new Font("Arial", 20.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            group.Location = new Point(10, 312);
            group.Name = "group";
            group.Size = new Size(224, 32);
            group.TabIndex = 56;
            group.Text = "Thông tin nhóm:";
            // 
            // comboBox4
            // 
            comboBox4.FormattingEnabled = true;
            comboBox4.Location = new Point(809, 527);
            comboBox4.Name = "comboBox4";
            comboBox4.Size = new Size(244, 22);
            comboBox4.TabIndex = 65;
            comboBox4.SelectedIndexChanged += comboBox4_SelectedIndexChanged;
            // 
            // comboBox5
            // 
            comboBox5.FormattingEnabled = true;
            comboBox5.Location = new Point(809, 485);
            comboBox5.Name = "comboBox5";
            comboBox5.Size = new Size(321, 22);
            comboBox5.TabIndex = 64;
            comboBox5.SelectedIndexChanged += comboBox5_SelectedIndexChanged;
            // 
            // btnAddMember
            // 
            btnAddMember.Font = new Font("Comic Sans MS", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnAddMember.Location = new Point(1065, 554);
            btnAddMember.Name = "btnAddMember";
            btnAddMember.Size = new Size(64, 27);
            btnAddMember.TabIndex = 62;
            btnAddMember.Text = "Lưu";
            btnAddMember.UseVisualStyleBackColor = true;
            btnAddMember.Click += btnAddMember_Click;
            // 
            // addleader
            // 
            addleader.AutoSize = true;
            addleader.Font = new Font("Arial", 8.25F);
            addleader.Location = new Point(706, 531);
            addleader.Name = "addleader";
            addleader.Size = new Size(58, 14);
            addleader.TabIndex = 61;
            addleader.Text = "Tổ trưởng:";
            // 
            // addgroup
            // 
            addgroup.AutoSize = true;
            addgroup.Font = new Font("Arial", 8.25F);
            addgroup.Location = new Point(706, 488);
            addgroup.Name = "addgroup";
            addgroup.Size = new Size(37, 14);
            addgroup.TabIndex = 60;
            addgroup.Text = "Nhóm:";
            // 
            // adddescript
            // 
            adddescript.AutoSize = true;
            adddescript.Font = new Font("Arial", 8.25F);
            adddescript.Location = new Point(706, 443);
            adddescript.Name = "adddescript";
            adddescript.Size = new Size(64, 14);
            adddescript.TabIndex = 59;
            adddescript.Text = "Quyền Hạn:";
            // 
            // addmember
            // 
            addmember.AutoSize = true;
            addmember.Font = new Font("Arial", 20.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            addmember.Location = new Point(671, 269);
            addmember.Name = "addmember";
            addmember.Size = new Size(227, 32);
            addmember.TabIndex = 58;
            addmember.Text = "Thêm nhân viên:";
            // 
            // newgroup
            // 
            newgroup.AutoSize = true;
            newgroup.Location = new Point(1058, 531);
            newgroup.Name = "newgroup";
            newgroup.Size = new Size(73, 18);
            newgroup.TabIndex = 57;
            newgroup.Text = "Nhóm mới";
            newgroup.UseVisualStyleBackColor = true;
            newgroup.CheckedChanged += newgroup_CheckedChanged;
            // 
            // comboBox9
            // 
            comboBox9.FormattingEnabled = true;
            comboBox9.Location = new Point(809, 657);
            comboBox9.Name = "comboBox9";
            comboBox9.Size = new Size(321, 22);
            comboBox9.TabIndex = 72;
            // 
            // btnDeleteMember
            // 
            btnDeleteMember.Font = new Font("Arial", 11.25F);
            btnDeleteMember.Location = new Point(1065, 684);
            btnDeleteMember.Name = "btnDeleteMember";
            btnDeleteMember.Size = new Size(64, 27);
            btnDeleteMember.TabIndex = 71;
            btnDeleteMember.Text = "Xóa";
            btnDeleteMember.UseVisualStyleBackColor = true;
            btnDeleteMember.Click += btnDeleteMember_Click;
            // 
            // deletedmember
            // 
            deletedmember.AutoSize = true;
            deletedmember.Font = new Font("Arial", 8.25F);
            deletedmember.Location = new Point(706, 656);
            deletedmember.Name = "deletedmember";
            deletedmember.Size = new Size(58, 14);
            deletedmember.TabIndex = 68;
            deletedmember.Text = "Nhân viên:";
            // 
            // deletemember
            // 
            deletemember.AutoSize = true;
            deletemember.Font = new Font("Arial", 20.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            deletemember.Location = new Point(670, 600);
            deletemember.Name = "deletemember";
            deletemember.Size = new Size(204, 32);
            deletemember.TabIndex = 67;
            deletemember.Text = "Xóa nhân viên:";
            // 
            // addname
            // 
            addname.AutoSize = true;
            addname.Font = new Font("Arial", 8.25F);
            addname.Location = new Point(707, 362);
            addname.Name = "addname";
            addname.Size = new Size(78, 14);
            addname.TabIndex = 74;
            addname.Text = "Tên nhân viên:";
            // 
            // addmsnv
            // 
            addmsnv.AutoSize = true;
            addmsnv.Font = new Font("Arial", 8.25F);
            addmsnv.Location = new Point(707, 319);
            addmsnv.Name = "addmsnv";
            addmsnv.Size = new Size(40, 14);
            addmsnv.TabIndex = 76;
            addmsnv.Text = "MSNV:";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(810, 362);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(320, 20);
            textBox1.TabIndex = 78;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // comboBox6
            // 
            comboBox6.FormattingEnabled = true;
            comboBox6.Location = new Point(809, 444);
            comboBox6.Name = "comboBox6";
            comboBox6.Size = new Size(321, 22);
            comboBox6.TabIndex = 63;
            comboBox6.SelectedIndexChanged += comboBox6_SelectedIndexChanged;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(810, 324);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(320, 20);
            textBox2.TabIndex = 79;
            textBox2.TextChanged += textBox2_TextChanged;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(810, 402);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(320, 20);
            textBox3.TabIndex = 81;
            textBox3.TextChanged += textBox3_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Arial", 8.25F);
            label2.Location = new Point(707, 402);
            label2.Name = "label2";
            label2.Size = new Size(53, 14);
            label2.TabIndex = 80;
            label2.Text = "Chức Vụ:";
            // 
            // TaiKhoan
            // 
            AutoScaleDimensions = new SizeF(6F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(textBox3);
            Controls.Add(label2);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(addmsnv);
            Controls.Add(addname);
            Controls.Add(comboBox9);
            Controls.Add(btnDeleteMember);
            Controls.Add(deletedmember);
            Controls.Add(deletemember);
            Controls.Add(comboBox4);
            Controls.Add(comboBox5);
            Controls.Add(comboBox6);
            Controls.Add(btnAddMember);
            Controls.Add(addleader);
            Controls.Add(addgroup);
            Controls.Add(adddescript);
            Controls.Add(addmember);
            Controls.Add(newgroup);
            Controls.Add(group);
            Controls.Add(members);
            Controls.Add(leaders);
            Controls.Add(comboBox3);
            Controls.Add(comboBox2);
            Controls.Add(comboBox1);
            Controls.Add(btnChangeInfo);
            Controls.Add(chosenleader);
            Controls.Add(chosengroup);
            Controls.Add(chosenMSNV);
            Controls.Add(old_pass);
            Controls.Add(newpass);
            Controls.Add(infochange);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(passchange);
            Controls.Add(newpass2);
            Controls.Add(checkBox1);
            Controls.Add(btnChangePass);
            Controls.Add(label1);
            Font = new Font("Arial", 8.25F);
            Name = "TaiKhoan";
            Size = new Size(1149, 775);
            Load += TaiKhoan_Load;
            ((System.ComponentModel.ISupportInitialize)leaders).EndInit();
            ((System.ComponentModel.ISupportInitialize)members).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private Button btnChangePass;
        private CheckBox checkBox1;
        private TextBox newpass2;
        private Label passchange;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label infochange;
        private TextBox newpass;
        private TextBox old_pass;
        private Label chosenMSNV;
        private Label chosengroup;
        private Label chosenleader;
        private Button btnChangeInfo;
        private ComboBox comboBox1;
        private ComboBox comboBox2;
        private ComboBox comboBox3;
        private DataGridView leaders;
        private DataGridView members;
        private Label group;
        private ComboBox comboBox4;
        private ComboBox comboBox5;
        private Button btnAddMember;
        private Label addleader;
        private Label addgroup;
        private Label adddescript;
        private Label addmember;
        private CheckBox newgroup;
        private ComboBox comboBox9;
        private Button btnDeleteMember;
        private Label deletedmember;
        private Label deletemember;
        private Label addname;
        private Label addmsnv;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private TextBox textBox1;
        private ComboBox comboBox6;
        private TextBox textBox2;
        private TextBox textBox3;
        private Label label2;
    }
}
