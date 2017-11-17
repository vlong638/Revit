namespace MyRevit.SubsidenceMonitor.UI
{
    partial class CPSettingsForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cb_IsSurfaceVisible = new System.Windows.Forms.CheckBox();
            this.cb_FillPattern = new System.Windows.Forms.ComboBox();
            this.btn_Color = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_IsHalftone = new System.Windows.Forms.CheckBox();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_Submit = new System.Windows.Forms.Button();
            this.btn_ResetTransparency = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.traceBar_Transparency = new System.Windows.Forms.TrackBar();
            this.tb_Transparency = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
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
            this.cb_IsVisible.TabIndex = 1;
            this.cb_IsVisible.Text = "可见";
            this.cb_IsVisible.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cb_IsSurfaceVisible);
            this.groupBox1.Controls.Add(this.cb_FillPattern);
            this.groupBox1.Controls.Add(this.btn_Color);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 34);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(328, 105);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "表面填充图案";
            // 
            // cb_IsSurfaceVisible
            // 
            this.cb_IsSurfaceVisible.AutoSize = true;
            this.cb_IsSurfaceVisible.Location = new System.Drawing.Point(15, 20);
            this.cb_IsSurfaceVisible.Name = "cb_IsSurfaceVisible";
            this.cb_IsSurfaceVisible.Size = new System.Drawing.Size(48, 16);
            this.cb_IsSurfaceVisible.TabIndex = 11;
            this.cb_IsSurfaceVisible.Text = "可见";
            this.cb_IsSurfaceVisible.UseVisualStyleBackColor = true;
            // 
            // cb_FillPattern
            // 
            this.cb_FillPattern.FormattingEnabled = true;
            this.cb_FillPattern.Location = new System.Drawing.Point(96, 72);
            this.cb_FillPattern.Name = "cb_FillPattern";
            this.cb_FillPattern.Size = new System.Drawing.Size(226, 20);
            this.cb_FillPattern.TabIndex = 10;
            this.cb_FillPattern.Text = "<按材质>";
            // 
            // btn_Color
            // 
            this.btn_Color.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_Color.Location = new System.Drawing.Point(96, 43);
            this.btn_Color.Name = "btn_Color";
            this.btn_Color.Size = new System.Drawing.Size(226, 23);
            this.btn_Color.TabIndex = 7;
            this.btn_Color.UseVisualStyleBackColor = true;
            this.btn_Color.Click += new System.EventHandler(this.btn_Color_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "填充图案:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(55, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "颜色:";
            // 
            // cb_IsHalftone
            // 
            this.cb_IsHalftone.AutoSize = true;
            this.cb_IsHalftone.Location = new System.Drawing.Point(280, 12);
            this.cb_IsHalftone.Name = "cb_IsHalftone";
            this.cb_IsHalftone.Size = new System.Drawing.Size(60, 16);
            this.cb_IsHalftone.TabIndex = 3;
            this.cb_IsHalftone.Text = "半色调";
            this.cb_IsHalftone.UseVisualStyleBackColor = true;
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(285, 207);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(55, 23);
            this.btn_Cancel.TabIndex = 11;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_Submit
            // 
            this.btn_Submit.Location = new System.Drawing.Point(224, 207);
            this.btn_Submit.Name = "btn_Submit";
            this.btn_Submit.Size = new System.Drawing.Size(55, 23);
            this.btn_Submit.TabIndex = 10;
            this.btn_Submit.Text = "确定";
            this.btn_Submit.UseVisualStyleBackColor = true;
            this.btn_Submit.Click += new System.EventHandler(this.btn_Submit_Click);
            // 
            // btn_ResetTransparency
            // 
            this.btn_ResetTransparency.Location = new System.Drawing.Point(12, 207);
            this.btn_ResetTransparency.Name = "btn_ResetTransparency";
            this.btn_ResetTransparency.Size = new System.Drawing.Size(55, 23);
            this.btn_ResetTransparency.TabIndex = 9;
            this.btn_ResetTransparency.Text = "重设";
            this.btn_ResetTransparency.UseVisualStyleBackColor = true;
            this.btn_ResetTransparency.Click += new System.EventHandler(this.btn_ResetTransparency_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.traceBar_Transparency);
            this.groupBox2.Controls.Add(this.tb_Transparency);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(12, 145);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(328, 56);
            this.groupBox2.TabIndex = 8;
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
            // CPSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(353, 243);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Submit);
            this.Controls.Add(this.btn_ResetTransparency);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cb_IsHalftone);
            this.Controls.Add(this.cb_IsVisible);
            this.Name = "CPSettingsForm";
            this.Text = "CPSettingsForm";
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cb_IsSurfaceVisible;
        private System.Windows.Forms.ComboBox cb_FillPattern;
        private System.Windows.Forms.Button btn_Color;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cb_IsHalftone;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_Submit;
        private System.Windows.Forms.Button btn_ResetTransparency;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TrackBar traceBar_Transparency;
        private System.Windows.Forms.TextBox tb_Transparency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ColorDialog colorDialog1;
    }
}