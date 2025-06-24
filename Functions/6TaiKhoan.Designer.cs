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
            members = new DataGridView();
            group = new Label();
            comboBox4 = new ComboBox();
            comboBox5 = new ComboBox();
            comboBox6 = new ComboBox();
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
            comboBox7 = new ComboBox();
            addname = new Label();
            comboBox10 = new ComboBox();
            addmsnv = new Label();
            ((System.ComponentModel.ISupportInitialize)leaders).BeginInit();
            ((System.ComponentModel.ISupportInitialize)members).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Comic Sans MS", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(2, 26);
            label1.Name = "label1";
            label1.Size = new Size(131, 35);
            label1.TabIndex = 30;
            label1.Text = "Tài khoản";
            // 
            // btnChangePass
            // 
            btnChangePass.Font = new Font("Comic Sans MS", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnChangePass.Location = new Point(653, 256);
            btnChangePass.Name = "btnChangePass";
            btnChangePass.Size = new Size(75, 29);
            btnChangePass.TabIndex = 36;
            btnChangePass.Text = "Lưu";
            btnChangePass.UseVisualStyleBackColor = true;
            btnChangePass.Click += btnChangePass_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(1235, 226);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(84, 19);
            checkBox1.TabIndex = 37;
            checkBox1.Text = "Nhóm mới";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // newpass2
            // 
            newpass2.Location = new Point(192, 227);
            newpass2.Name = "newpass2";
            newpass2.Size = new Size(536, 23);
            newpass2.TabIndex = 38;
            // 
            // passchange
            // 
            passchange.AutoSize = true;
            passchange.Font = new Font("Comic Sans MS", 20.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            passchange.Location = new Point(12, 70);
            passchange.Name = "passchange";
            passchange.Size = new Size(216, 38);
            passchange.TabIndex = 39;
            passchange.Text = "Đổi mật khẩu:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Comic Sans MS", 11.25F);
            label3.Location = new Point(43, 130);
            label3.Name = "label3";
            label3.Size = new Size(98, 20);
            label3.TabIndex = 40;
            label3.Text = "Mật khẩu cũ:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Comic Sans MS", 11.25F);
            label4.Location = new Point(43, 178);
            label4.Name = "label4";
            label4.Size = new Size(106, 20);
            label4.TabIndex = 41;
            label4.Text = "Mật khẩu mới:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Comic Sans MS", 11.25F);
            label5.Location = new Point(43, 226);
            label5.Name = "label5";
            label5.Size = new Size(143, 20);
            label5.TabIndex = 42;
            label5.Text = "Xác nhận mật khẩu:";
            // 
            // infochange
            // 
            infochange.AutoSize = true;
            infochange.Font = new Font("Comic Sans MS", 20.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            infochange.Location = new Point(783, 70);
            infochange.Name = "infochange";
            infochange.Size = new Size(286, 38);
            infochange.TabIndex = 43;
            infochange.Text = "Thông tin nhân viên:";
            // 
            // newpass
            // 
            newpass.Location = new Point(192, 179);
            newpass.Name = "newpass";
            newpass.Size = new Size(536, 23);
            newpass.TabIndex = 44;
            // 
            // old_pass
            // 
            old_pass.Location = new Point(192, 127);
            old_pass.Name = "old_pass";
            old_pass.Size = new Size(536, 23);
            old_pass.TabIndex = 45;
            // 
            // chosenMSNV
            // 
            chosenMSNV.AutoSize = true;
            chosenMSNV.Font = new Font("Comic Sans MS", 11.25F);
            chosenMSNV.Location = new Point(825, 130);
            chosenMSNV.Name = "chosenMSNV";
            chosenMSNV.Size = new Size(114, 20);
            chosenMSNV.TabIndex = 46;
            chosenMSNV.Text = "Chọn nhân viên:";
            // 
            // chosengroup
            // 
            chosengroup.AutoSize = true;
            chosengroup.Font = new Font("Comic Sans MS", 11.25F);
            chosengroup.Location = new Point(825, 178);
            chosengroup.Name = "chosengroup";
            chosengroup.Size = new Size(52, 20);
            chosengroup.TabIndex = 47;
            chosengroup.Text = "Nhóm:";
            // 
            // chosenleader
            // 
            chosenleader.AutoSize = true;
            chosenleader.Font = new Font("Comic Sans MS", 11.25F);
            chosenleader.Location = new Point(825, 226);
            chosenleader.Name = "chosenleader";
            chosenleader.Size = new Size(83, 20);
            chosenleader.TabIndex = 48;
            chosenleader.Text = "Tổ trưởng:";
            // 
            // btnChangeInfo
            // 
            btnChangeInfo.Font = new Font("Comic Sans MS", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnChangeInfo.Location = new Point(1244, 251);
            btnChangeInfo.Name = "btnChangeInfo";
            btnChangeInfo.Size = new Size(75, 29);
            btnChangeInfo.TabIndex = 50;
            btnChangeInfo.Text = "Lưu";
            btnChangeInfo.UseVisualStyleBackColor = true;
            btnChangeInfo.Click += btnChangeInfo_Click;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(945, 131);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(374, 23);
            comboBox1.TabIndex = 51;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(945, 175);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(374, 23);
            comboBox2.TabIndex = 52;
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(945, 222);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(284, 23);
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
            leaders.Location = new Point(12, 379);
            leaders.MultiSelect = false;
            leaders.Name = "leaders";
            leaders.ReadOnly = true;
            leaders.RowHeadersVisible = false;
            leaders.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            leaders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            leaders.Size = new Size(337, 342);
            leaders.StandardTab = true;
            leaders.TabIndex = 54;
            leaders.CellClick += leaders_CellContentClick;
            leaders.CellContentClick += leaders_CellContentClick_1;
            // 
            // members
            // 
            members.BackgroundColor = SystemColors.Control;
            members.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            members.Location = new Point(355, 379);
            members.Name = "members";
            members.Size = new Size(373, 342);
            members.TabIndex = 55;
            // 
            // group
            // 
            group.AutoSize = true;
            group.Font = new Font("Comic Sans MS", 20.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            group.Location = new Point(12, 334);
            group.Name = "group";
            group.Size = new Size(230, 38);
            group.TabIndex = 56;
            group.Text = "Thông tin nhóm:";
            // 
            // comboBox4
            // 
            comboBox4.FormattingEnabled = true;
            comboBox4.Location = new Point(945, 524);
            comboBox4.Name = "comboBox4";
            comboBox4.Size = new Size(284, 23);
            comboBox4.TabIndex = 65;
            // 
            // comboBox5
            // 
            comboBox5.FormattingEnabled = true;
            comboBox5.Location = new Point(945, 479);
            comboBox5.Name = "comboBox5";
            comboBox5.Size = new Size(374, 23);
            comboBox5.TabIndex = 64;
            // 
            // comboBox6
            // 
            comboBox6.FormattingEnabled = true;
            comboBox6.Location = new Point(945, 435);
            comboBox6.Name = "comboBox6";
            comboBox6.Size = new Size(374, 23);
            comboBox6.TabIndex = 63;
            // 
            // btnAddMember
            // 
            btnAddMember.Font = new Font("Comic Sans MS", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnAddMember.Location = new Point(1244, 553);
            btnAddMember.Name = "btnAddMember";
            btnAddMember.Size = new Size(75, 29);
            btnAddMember.TabIndex = 62;
            btnAddMember.Text = "Lưu";
            btnAddMember.UseVisualStyleBackColor = true;
            btnAddMember.Click += btnAddMember_Click;
            // 
            // addleader
            // 
            addleader.AutoSize = true;
            addleader.Font = new Font("Comic Sans MS", 11.25F);
            addleader.Location = new Point(825, 528);
            addleader.Name = "addleader";
            addleader.Size = new Size(83, 20);
            addleader.TabIndex = 61;
            addleader.Text = "Tổ trưởng:";
            // 
            // addgroup
            // 
            addgroup.AutoSize = true;
            addgroup.Font = new Font("Comic Sans MS", 11.25F);
            addgroup.Location = new Point(825, 482);
            addgroup.Name = "addgroup";
            addgroup.Size = new Size(52, 20);
            addgroup.TabIndex = 60;
            addgroup.Text = "Nhóm:";
            // 
            // adddescript
            // 
            adddescript.AutoSize = true;
            adddescript.Font = new Font("Comic Sans MS", 11.25F);
            adddescript.Location = new Point(825, 434);
            adddescript.Name = "adddescript";
            adddescript.Size = new Size(68, 20);
            adddescript.TabIndex = 59;
            adddescript.Text = "Chức Vụ:";
            // 
            // addmember
            // 
            addmember.AutoSize = true;
            addmember.Font = new Font("Comic Sans MS", 20.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            addmember.Location = new Point(783, 288);
            addmember.Name = "addmember";
            addmember.Size = new Size(233, 38);
            addmember.TabIndex = 58;
            addmember.Text = "Thêm nhân viên:";
            // 
            // newgroup
            // 
            newgroup.AutoSize = true;
            newgroup.Location = new Point(1235, 528);
            newgroup.Name = "newgroup";
            newgroup.Size = new Size(84, 19);
            newgroup.TabIndex = 57;
            newgroup.Text = "Nhóm mới";
            newgroup.UseVisualStyleBackColor = true;
            // 
            // comboBox9
            // 
            comboBox9.FormattingEnabled = true;
            comboBox9.Location = new Point(945, 663);
            comboBox9.Name = "comboBox9";
            comboBox9.Size = new Size(374, 23);
            comboBox9.TabIndex = 72;
            // 
            // btnDeleteMember
            // 
            btnDeleteMember.Font = new Font("Comic Sans MS", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnDeleteMember.Location = new Point(1244, 692);
            btnDeleteMember.Name = "btnDeleteMember";
            btnDeleteMember.Size = new Size(75, 29);
            btnDeleteMember.TabIndex = 71;
            btnDeleteMember.Text = "Xóa";
            btnDeleteMember.UseVisualStyleBackColor = true;
            btnDeleteMember.Click += btnDeleteMember_Click;
            // 
            // deletedmember
            // 
            deletedmember.AutoSize = true;
            deletedmember.Font = new Font("Comic Sans MS", 11.25F);
            deletedmember.Location = new Point(825, 662);
            deletedmember.Name = "deletedmember";
            deletedmember.Size = new Size(80, 20);
            deletedmember.TabIndex = 68;
            deletedmember.Text = "Nhân viên:";
            // 
            // deletemember
            // 
            deletemember.AutoSize = true;
            deletemember.Font = new Font("Comic Sans MS", 20.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            deletemember.Location = new Point(783, 602);
            deletemember.Name = "deletemember";
            deletemember.Size = new Size(211, 38);
            deletemember.TabIndex = 67;
            deletemember.Text = "Xóa nhân viên:";
            // 
            // comboBox7
            // 
            comboBox7.FormattingEnabled = true;
            comboBox7.Location = new Point(945, 389);
            comboBox7.Name = "comboBox7";
            comboBox7.Size = new Size(374, 23);
            comboBox7.TabIndex = 75;
            // 
            // addname
            // 
            addname.AutoSize = true;
            addname.Font = new Font("Comic Sans MS", 11.25F);
            addname.Location = new Point(825, 388);
            addname.Name = "addname";
            addname.Size = new Size(106, 20);
            addname.TabIndex = 74;
            addname.Text = "Tên nhân viên:";
            // 
            // comboBox10
            // 
            comboBox10.FormattingEnabled = true;
            comboBox10.Location = new Point(945, 343);
            comboBox10.Name = "comboBox10";
            comboBox10.Size = new Size(374, 23);
            comboBox10.TabIndex = 77;
            comboBox10.SelectedIndexChanged += comboBox10_SelectedIndexChanged;
            // 
            // addmsnv
            // 
            addmsnv.AutoSize = true;
            addmsnv.Font = new Font("Comic Sans MS", 11.25F);
            addmsnv.Location = new Point(825, 342);
            addmsnv.Name = "addmsnv";
            addmsnv.Size = new Size(58, 20);
            addmsnv.TabIndex = 76;
            addmsnv.Text = "MSNV:";
            // 
            // TaiKhoan
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(comboBox10);
            Controls.Add(addmsnv);
            Controls.Add(comboBox7);
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
            Name = "TaiKhoan";
            Size = new Size(1340, 830);
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
        private ComboBox comboBox6;
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
        private ComboBox comboBox7;
        private Label addname;
        private ComboBox comboBox10;
        private Label addmsnv;
    }
}
