namespace APPPC
{
    partial class loggin
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btn_password = new Button();
            btn_username = new Button();
            btn_login = new Button();
            button1 = new Button();
            SuspendLayout();
            // 
            // btn_password
            // 
            btn_password.AccessibleName = "in_password";
            btn_password.BackColor = Color.Transparent;
            btn_password.BackgroundImage = Properties.Resources.l10;
            btn_password.FlatAppearance.BorderSize = 0;
            btn_password.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btn_password.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn_password.FlatStyle = FlatStyle.Flat;
            btn_password.Font = new Font("Comic Sans MS", 14.25F);
            btn_password.ForeColor = Color.Transparent;
            btn_password.Location = new Point(606, 637);
            btn_password.Margin = new Padding(4, 3, 4, 3);
            btn_password.Name = "btn_password";
            btn_password.Padding = new Padding(66, 0, 0, 0);
            btn_password.Size = new Size(362, 60);
            btn_password.TabIndex = 1;
            btn_password.TextAlign = ContentAlignment.MiddleLeft;
            btn_password.UseVisualStyleBackColor = false;
            btn_password.Click += btn_password_Click;
            // 
            // btn_username
            // 
            btn_username.AccessibleDescription = "in_username";
            btn_username.AccessibleName = "in_username";
            btn_username.BackColor = Color.Transparent;
            btn_username.BackgroundImage = Properties.Resources.l00;
            btn_username.Cursor = Cursors.IBeam;
            btn_username.FlatAppearance.BorderSize = 0;
            btn_username.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btn_username.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn_username.FlatStyle = FlatStyle.Flat;
            btn_username.Font = new Font("Comic Sans MS", 14.25F);
            btn_username.ForeColor = Color.Transparent;
            btn_username.Location = new Point(606, 583);
            btn_username.Margin = new Padding(4, 3, 4, 3);
            btn_username.Name = "btn_username";
            btn_username.Padding = new Padding(66, 0, 0, 0);
            btn_username.Size = new Size(362, 60);
            btn_username.TabIndex = 2;
            btn_username.TabStop = false;
            btn_username.TextAlign = ContentAlignment.MiddleLeft;
            btn_username.TextImageRelation = TextImageRelation.TextAboveImage;
            btn_username.UseMnemonic = false;
            btn_username.UseVisualStyleBackColor = false;
            btn_username.Click += btn_username_Click;
            // 
            // btn_login
            // 
            btn_login.AccessibleName = "in_password";
            btn_login.BackColor = Color.Transparent;
            btn_login.FlatAppearance.BorderSize = 0;
            btn_login.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btn_login.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn_login.FlatStyle = FlatStyle.Flat;
            btn_login.Font = new Font("Comic Sans MS", 14.25F);
            btn_login.ForeColor = Color.Transparent;
            btn_login.Location = new Point(738, 717);
            btn_login.Margin = new Padding(4, 3, 4, 3);
            btn_login.Name = "btn_login";
            btn_login.Size = new Size(110, 43);
            btn_login.TabIndex = 3;
            btn_login.UseVisualStyleBackColor = false;
            btn_login.Click += btn_login_Click;
            // 
            // button1
            // 
            button1.BackgroundImage = Properties.Resources.bar;
            button1.Font = new Font("Comic Sans MS", 14.25F);
            button1.ForeColor = SystemColors.Window;
            button1.Location = new Point(13, 849);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(101, 39);
            button1.TabIndex = 4;
            button1.Text = "Exit";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // loggin
            // 
            AcceptButton = btn_login;
            AutoScaleDimensions = new SizeF(12F, 26F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.login;
            ClientSize = new Size(1600, 900);
            Controls.Add(button1);
            Controls.Add(btn_login);
            Controls.Add(btn_username);
            Controls.Add(btn_password);
            Font = new Font("Comic Sans MS", 14.25F);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "loggin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DA";
            Load += loggin_Load;
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Button btn_password;
        private Button btn_username;
        private Button btn_login;
        private Button button1;
    }
}
