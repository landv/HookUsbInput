namespace HookUsbInput
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_pay = new System.Windows.Forms.TextBox();
            this.userControlPanelEx1 = new HookUsbInput.UserControlPanelEx(this.components);
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 60;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(25, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 33);
            this.label1.TabIndex = 1;
            this.label1.Text = "付款码：";
            // 
            // textBox_pay
            // 
            this.textBox_pay.Font = new System.Drawing.Font("宋体", 24F);
            this.textBox_pay.Location = new System.Drawing.Point(150, 34);
            this.textBox_pay.Name = "textBox_pay";
            this.textBox_pay.Size = new System.Drawing.Size(531, 44);
            this.textBox_pay.TabIndex = 2;
            this.textBox_pay.TextChanged += new System.EventHandler(this.TextBox_pay_TextChanged);
            // 
            // userControlPanelEx1
            // 
            this.userControlPanelEx1.BorderColor = System.Drawing.Color.White;
            this.userControlPanelEx1.BorderSize = 3;
            this.userControlPanelEx1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userControlPanelEx1.Location = new System.Drawing.Point(0, 0);
            this.userControlPanelEx1.Name = "userControlPanelEx1";
            this.userControlPanelEx1.Size = new System.Drawing.Size(719, 111);
            this.userControlPanelEx1.TabIndex = 0;
            this.userControlPanelEx1.Load += new System.EventHandler(this.userControlPanelEx1_Load);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(719, 111);
            this.Controls.Add(this.textBox_pay);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.userControlPanelEx1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.Deactivate += new System.EventHandler(this.Form1_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.VisibleChanged += new System.EventHandler(this.Form1_VisibleChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.Leave += new System.EventHandler(this.Form1_Leave);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private UserControlPanelEx userControlPanelEx1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_pay;
    }
}

