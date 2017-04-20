namespace MyRevit.EarthWork.UI
{
    partial class EarthworkBlockCPSettingsForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cb_IsVisible = new System.Windows.Forms.CheckBox();
            this.cb_IsHalftone = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.btn_Color = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.traceBar_Transparency = new System.Windows.Forms.TrackBar();
            this.tb_Transparency = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.btn_ResetTransparency = new System.Windows.Forms.Button();
            this.btn_Submit = new System.Windows.Forms.Button();
            this.btn_Apply = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.traceBar_Transparency)).BeginInit();
            this.SuspendLayout();
            // 
            // cb_IsVisible
            // 
            this.cb_IsVisible.AutoSize = true;
            this.cb_IsVisible.Location = new System.Drawing.Point(12, 12);
            this.cb_IsVisible.Name = "cb_IsVisible";
            this.cb_IsVisible.Size = new System.Drawing.Size(48, 16);
            this.cb_IsVisible.TabIndex = 0;
            this.cb_IsVisible.Text = "可见";
            this.cb_IsVisible.UseVisualStyleBackColor = true;
            // 
            // cb_IsHalftone
            // 
            this.cb_IsHalftone.AutoSize = true;
            this.cb_IsHalftone.Location = new System.Drawing.Point(280, 12);
            this.cb_IsHalftone.Name = "cb_IsHalftone";
            this.cb_IsHalftone.Size = new System.Drawing.Size(60, 16);
            this.cb_IsHalftone.TabIndex = 1;
            this.cb_IsHalftone.Text = "半色调";
            this.cb_IsHalftone.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.btn_Color);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 34);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(328, 92);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "表面填充图案";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(96, 59);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(187, 20);
            this.comboBox1.TabIndex = 10;
            this.comboBox1.Text = "<按材质>";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(289, 57);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(33, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "...";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // btn_Color
            // 
            this.btn_Color.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_Color.Location = new System.Drawing.Point(96, 30);
            this.btn_Color.Name = "btn_Color";
            this.btn_Color.Size = new System.Drawing.Size(226, 23);
            this.btn_Color.TabIndex = 7;
            this.btn_Color.UseVisualStyleBackColor = true;
            this.btn_Color.Click += new System.EventHandler(this.btn_Color_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "填充图案:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(55, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "颜色:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.traceBar_Transparency);
            this.groupBox2.Controls.Add(this.tb_Transparency);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(13, 132);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(328, 56);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "曲面透明度";
            // 
            // traceBar_Transparency
            // 
            this.traceBar_Transparency.AutoSize = false;
            this.traceBar_Transparency.Location = new System.Drawing.Point(96, 24);
            this.traceBar_Transparency.Name = "traceBar_Transparency";
            this.traceBar_Transparency.Size = new System.Drawing.Size(187, 21);
            this.traceBar_Transparency.TabIndex = 10;
            this.traceBar_Transparency.TabStop = false;
            this.traceBar_Transparency.TickStyle = System.Windows.Forms.TickStyle.None;
            this.traceBar_Transparency.Scroll += new System.EventHandler(this.traceBar_Transparency_Scroll);
            // 
            // tb_Transparency
            // 
            this.tb_Transparency.Location = new System.Drawing.Point(283, 24);
            this.tb_Transparency.Name = "tb_Transparency";
            this.tb_Transparency.Size = new System.Drawing.Size(39, 21);
            this.tb_Transparency.TabIndex = 12;
            this.tb_Transparency.TextChanged += new System.EventHandler(this.tb_Transparency_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(43, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "透明度:";
            // 
            // btn_ResetTransparency
            // 
            this.btn_ResetTransparency.Location = new System.Drawing.Point(13, 194);
            this.btn_ResetTransparency.Name = "btn_ResetTransparency";
            this.btn_ResetTransparency.Size = new System.Drawing.Size(55, 23);
            this.btn_ResetTransparency.TabIndex = 4;
            this.btn_ResetTransparency.Text = "重设";
            this.btn_ResetTransparency.UseVisualStyleBackColor = true;
            this.btn_ResetTransparency.Click += new System.EventHandler(this.btn_ResetTransparency_Click);
            // 
            // btn_Submit
            // 
            this.btn_Submit.Location = new System.Drawing.Point(163, 194);
            this.btn_Submit.Name = "btn_Submit";
            this.btn_Submit.Size = new System.Drawing.Size(55, 23);
            this.btn_Submit.TabIndex = 5;
            this.btn_Submit.Text = "确定";
            this.btn_Submit.UseVisualStyleBackColor = true;
            this.btn_Submit.Click += new System.EventHandler(this.btn_Submit_Click);
            // 
            // btn_Apply
            // 
            this.btn_Apply.Location = new System.Drawing.Point(285, 194);
            this.btn_Apply.Name = "btn_Apply";
            this.btn_Apply.Size = new System.Drawing.Size(55, 23);
            this.btn_Apply.TabIndex = 6;
            this.btn_Apply.Text = "应用";
            this.btn_Apply.UseVisualStyleBackColor = true;
            this.btn_Apply.Click += new System.EventHandler(this.btn_Apply_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(224, 194);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(55, 23);
            this.btn_Cancel.TabIndex = 7;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // EarthworkBlockCPSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 227);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Apply);
            this.Controls.Add(this.btn_Submit);
            this.Controls.Add(this.btn_ResetTransparency);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cb_IsHalftone);
            this.Controls.Add(this.cb_IsVisible);
            this.Name = "EarthworkBlockCPSettingsForm";
            this.Text = "颜色/透明度";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.traceBar_Transparency)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cb_IsVisible;
        private System.Windows.Forms.CheckBox cb_IsHalftone;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btn_Color;
        private System.Windows.Forms.Button btn_ResetTransparency;
        private System.Windows.Forms.Button btn_Submit;
        private System.Windows.Forms.Button btn_Apply;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.TextBox tb_Transparency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar traceBar_Transparency;
    }
}