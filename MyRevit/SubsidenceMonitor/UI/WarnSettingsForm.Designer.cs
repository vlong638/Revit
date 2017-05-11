namespace MyRevit.SubsidenceMonitor.UI
{
    partial class WarnSettingsForm
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
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_Submit = new System.Windows.Forms.Button();
            this.GroupBox_SkewBack = new System.Windows.Forms.GroupBox();
            this.tb_SkewBack_Speed = new System.Windows.Forms.TextBox();
            this.tb_SkewBack_StandardMillimeter = new System.Windows.Forms.TextBox();
            this.tb_SkewBack_WellMillimeter = new System.Windows.Forms.TextBox();
            this.tb_SkewBack_Day = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.tb_STBAP_MaxAxle = new System.Windows.Forms.TextBox();
            this.tb_STBAP_MinAxle = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tb_UnstressedPipeLineSubsidence_SumMillimeter = new System.Windows.Forms.TextBox();
            this.tb_UnstressedPipeLineSubsidence_WellMillimeter = new System.Windows.Forms.TextBox();
            this.tb_UnstressedPipeLineSubsidence_Day = new System.Windows.Forms.TextBox();
            this.tb_UnstressedPipeLineSubsidence_PipelineMillimeter = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tb_StressedPipeLineSubsidence_SumMillimeter = new System.Windows.Forms.TextBox();
            this.tb_StressedPipeLineSubsidence_WellMillimeter = new System.Windows.Forms.TextBox();
            this.tb_StressedPipeLineSubsidence_Day = new System.Windows.Forms.TextBox();
            this.tb_StressedPipeLineSubsidence_PipelineMillimeter = new System.Windows.Forms.TextBox();
            this.tb_PipeLineSubsidence_Day = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tb_SurfaceSubsidence_SumMillimeter = new System.Windows.Forms.TextBox();
            this.tb_SurfaceSubsidence_Day = new System.Windows.Forms.TextBox();
            this.tb_SurfaceSubsidence_DailyMillimeter = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tb_BuildingSubsidence_SumMillimeter = new System.Windows.Forms.TextBox();
            this.tb_BuildingSubsidence_Day = new System.Windows.Forms.TextBox();
            this.tb_BuildingSubsidence_DailyMillimeter = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.GroupBox_SkewBack.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(417, 336);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 157;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_Submit
            // 
            this.btn_Submit.Location = new System.Drawing.Point(318, 336);
            this.btn_Submit.Name = "btn_Submit";
            this.btn_Submit.Size = new System.Drawing.Size(75, 23);
            this.btn_Submit.TabIndex = 156;
            this.btn_Submit.Text = "确定";
            this.btn_Submit.UseVisualStyleBackColor = true;
            this.btn_Submit.Click += new System.EventHandler(this.btn_Submit_Click);
            // 
            // GroupBox_SkewBack
            // 
            this.GroupBox_SkewBack.Controls.Add(this.tb_SkewBack_Speed);
            this.GroupBox_SkewBack.Controls.Add(this.tb_SkewBack_StandardMillimeter);
            this.GroupBox_SkewBack.Controls.Add(this.tb_SkewBack_WellMillimeter);
            this.GroupBox_SkewBack.Controls.Add(this.tb_SkewBack_Day);
            this.GroupBox_SkewBack.Controls.Add(this.label1);
            this.GroupBox_SkewBack.Location = new System.Drawing.Point(12, 282);
            this.GroupBox_SkewBack.Name = "GroupBox_SkewBack";
            this.GroupBox_SkewBack.Size = new System.Drawing.Size(529, 48);
            this.GroupBox_SkewBack.TabIndex = 155;
            this.GroupBox_SkewBack.TabStop = false;
            this.GroupBox_SkewBack.Text = "墙体水平位移";
            // 
            // tb_SkewBack_Speed
            // 
            this.tb_SkewBack_Speed.Location = new System.Drawing.Point(386, 19);
            this.tb_SkewBack_Speed.Name = "tb_SkewBack_Speed";
            this.tb_SkewBack_Speed.Size = new System.Drawing.Size(21, 21);
            this.tb_SkewBack_Speed.TabIndex = 135;
            this.tb_SkewBack_Speed.Text = "20";
            // 
            // tb_SkewBack_StandardMillimeter
            // 
            this.tb_SkewBack_StandardMillimeter.Location = new System.Drawing.Point(277, 19);
            this.tb_SkewBack_StandardMillimeter.Name = "tb_SkewBack_StandardMillimeter";
            this.tb_SkewBack_StandardMillimeter.Size = new System.Drawing.Size(21, 21);
            this.tb_SkewBack_StandardMillimeter.TabIndex = 134;
            this.tb_SkewBack_StandardMillimeter.Text = "20";
            // 
            // tb_SkewBack_WellMillimeter
            // 
            this.tb_SkewBack_WellMillimeter.Location = new System.Drawing.Point(140, 19);
            this.tb_SkewBack_WellMillimeter.Name = "tb_SkewBack_WellMillimeter";
            this.tb_SkewBack_WellMillimeter.Size = new System.Drawing.Size(21, 21);
            this.tb_SkewBack_WellMillimeter.TabIndex = 131;
            this.tb_SkewBack_WellMillimeter.Text = "22";
            // 
            // tb_SkewBack_Day
            // 
            this.tb_SkewBack_Day.Location = new System.Drawing.Point(483, 19);
            this.tb_SkewBack_Day.Name = "tb_SkewBack_Day";
            this.tb_SkewBack_Day.Size = new System.Drawing.Size(21, 21);
            this.tb_SkewBack_Day.TabIndex = 133;
            this.tb_SkewBack_Day.Text = "20";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(515, 12);
            this.label1.TabIndex = 132;
            this.label1.Text = "报警值：端头井累计值      mm，标准段累计值      mm，变形速率      mm/d（连续     天）";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.tb_STBAP_MaxAxle);
            this.groupBox5.Controls.Add(this.tb_STBAP_MinAxle);
            this.groupBox5.Controls.Add(this.label3);
            this.groupBox5.Location = new System.Drawing.Point(12, 120);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(529, 48);
            this.groupBox5.TabIndex = 154;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "钢支撑轴力";
            // 
            // tb_STBAP_MaxAxle
            // 
            this.tb_STBAP_MaxAxle.Location = new System.Drawing.Point(135, 19);
            this.tb_STBAP_MaxAxle.Name = "tb_STBAP_MaxAxle";
            this.tb_STBAP_MaxAxle.Size = new System.Drawing.Size(21, 21);
            this.tb_STBAP_MaxAxle.TabIndex = 131;
            this.tb_STBAP_MaxAxle.Text = "80";
            // 
            // tb_STBAP_MinAxle
            // 
            this.tb_STBAP_MinAxle.Location = new System.Drawing.Point(262, 19);
            this.tb_STBAP_MinAxle.Name = "tb_STBAP_MinAxle";
            this.tb_STBAP_MinAxle.Size = new System.Drawing.Size(21, 21);
            this.tb_STBAP_MinAxle.TabIndex = 133;
            this.tb_STBAP_MinAxle.Text = "20";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(293, 12);
            this.label3.TabIndex = 132;
            this.label3.Text = "报警值：大于设计轴力      %，小于设计轴力      %";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tb_UnstressedPipeLineSubsidence_SumMillimeter);
            this.groupBox4.Controls.Add(this.tb_UnstressedPipeLineSubsidence_WellMillimeter);
            this.groupBox4.Controls.Add(this.tb_UnstressedPipeLineSubsidence_Day);
            this.groupBox4.Controls.Add(this.tb_UnstressedPipeLineSubsidence_PipelineMillimeter);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Location = new System.Drawing.Point(12, 228);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(529, 48);
            this.groupBox4.TabIndex = 153;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "管线沉降(无压)";
            // 
            // tb_UnstressedPipeLineSubsidence_SumMillimeter
            // 
            this.tb_UnstressedPipeLineSubsidence_SumMillimeter.Location = new System.Drawing.Point(464, 20);
            this.tb_UnstressedPipeLineSubsidence_SumMillimeter.Name = "tb_UnstressedPipeLineSubsidence_SumMillimeter";
            this.tb_UnstressedPipeLineSubsidence_SumMillimeter.Size = new System.Drawing.Size(20, 21);
            this.tb_UnstressedPipeLineSubsidence_SumMillimeter.TabIndex = 142;
            this.tb_UnstressedPipeLineSubsidence_SumMillimeter.Text = "23";
            // 
            // tb_UnstressedPipeLineSubsidence_WellMillimeter
            // 
            this.tb_UnstressedPipeLineSubsidence_WellMillimeter.Location = new System.Drawing.Point(314, 21);
            this.tb_UnstressedPipeLineSubsidence_WellMillimeter.Name = "tb_UnstressedPipeLineSubsidence_WellMillimeter";
            this.tb_UnstressedPipeLineSubsidence_WellMillimeter.Size = new System.Drawing.Size(20, 21);
            this.tb_UnstressedPipeLineSubsidence_WellMillimeter.TabIndex = 141;
            this.tb_UnstressedPipeLineSubsidence_WellMillimeter.Text = "23";
            // 
            // tb_UnstressedPipeLineSubsidence_Day
            // 
            this.tb_UnstressedPipeLineSubsidence_Day.Location = new System.Drawing.Point(139, 20);
            this.tb_UnstressedPipeLineSubsidence_Day.Name = "tb_UnstressedPipeLineSubsidence_Day";
            this.tb_UnstressedPipeLineSubsidence_Day.Size = new System.Drawing.Size(16, 21);
            this.tb_UnstressedPipeLineSubsidence_Day.TabIndex = 138;
            this.tb_UnstressedPipeLineSubsidence_Day.Text = "2";
            // 
            // tb_UnstressedPipeLineSubsidence_PipelineMillimeter
            // 
            this.tb_UnstressedPipeLineSubsidence_PipelineMillimeter.Location = new System.Drawing.Point(196, 20);
            this.tb_UnstressedPipeLineSubsidence_PipelineMillimeter.Name = "tb_UnstressedPipeLineSubsidence_PipelineMillimeter";
            this.tb_UnstressedPipeLineSubsidence_PipelineMillimeter.Size = new System.Drawing.Size(16, 21);
            this.tb_UnstressedPipeLineSubsidence_PipelineMillimeter.TabIndex = 140;
            this.tb_UnstressedPipeLineSubsidence_PipelineMillimeter.Text = "2";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 23);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(485, 12);
            this.label7.TabIndex = 139;
            this.label7.Text = "报警值：日报警值连续     天±     mm（无压）、±      mm（自流井）；累计      mm";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tb_StressedPipeLineSubsidence_SumMillimeter);
            this.groupBox3.Controls.Add(this.tb_StressedPipeLineSubsidence_WellMillimeter);
            this.groupBox3.Controls.Add(this.tb_StressedPipeLineSubsidence_Day);
            this.groupBox3.Controls.Add(this.tb_StressedPipeLineSubsidence_PipelineMillimeter);
            this.groupBox3.Controls.Add(this.tb_PipeLineSubsidence_Day);
            this.groupBox3.Location = new System.Drawing.Point(12, 174);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(529, 48);
            this.groupBox3.TabIndex = 152;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "管线沉降(有压)";
            // 
            // tb_StressedPipeLineSubsidence_SumMillimeter
            // 
            this.tb_StressedPipeLineSubsidence_SumMillimeter.Location = new System.Drawing.Point(464, 20);
            this.tb_StressedPipeLineSubsidence_SumMillimeter.Name = "tb_StressedPipeLineSubsidence_SumMillimeter";
            this.tb_StressedPipeLineSubsidence_SumMillimeter.Size = new System.Drawing.Size(20, 21);
            this.tb_StressedPipeLineSubsidence_SumMillimeter.TabIndex = 142;
            this.tb_StressedPipeLineSubsidence_SumMillimeter.Text = "23";
            // 
            // tb_StressedPipeLineSubsidence_WellMillimeter
            // 
            this.tb_StressedPipeLineSubsidence_WellMillimeter.Location = new System.Drawing.Point(314, 20);
            this.tb_StressedPipeLineSubsidence_WellMillimeter.Name = "tb_StressedPipeLineSubsidence_WellMillimeter";
            this.tb_StressedPipeLineSubsidence_WellMillimeter.Size = new System.Drawing.Size(20, 21);
            this.tb_StressedPipeLineSubsidence_WellMillimeter.TabIndex = 141;
            this.tb_StressedPipeLineSubsidence_WellMillimeter.Text = "23";
            // 
            // tb_StressedPipeLineSubsidence_Day
            // 
            this.tb_StressedPipeLineSubsidence_Day.Location = new System.Drawing.Point(139, 20);
            this.tb_StressedPipeLineSubsidence_Day.Name = "tb_StressedPipeLineSubsidence_Day";
            this.tb_StressedPipeLineSubsidence_Day.Size = new System.Drawing.Size(16, 21);
            this.tb_StressedPipeLineSubsidence_Day.TabIndex = 138;
            this.tb_StressedPipeLineSubsidence_Day.Text = "2";
            // 
            // tb_StressedPipeLineSubsidence_PipelineMillimeter
            // 
            this.tb_StressedPipeLineSubsidence_PipelineMillimeter.Location = new System.Drawing.Point(196, 20);
            this.tb_StressedPipeLineSubsidence_PipelineMillimeter.Name = "tb_StressedPipeLineSubsidence_PipelineMillimeter";
            this.tb_StressedPipeLineSubsidence_PipelineMillimeter.Size = new System.Drawing.Size(16, 21);
            this.tb_StressedPipeLineSubsidence_PipelineMillimeter.TabIndex = 140;
            this.tb_StressedPipeLineSubsidence_PipelineMillimeter.Text = "2";
            // 
            // tb_PipeLineSubsidence_Day
            // 
            this.tb_PipeLineSubsidence_Day.AutoSize = true;
            this.tb_PipeLineSubsidence_Day.Location = new System.Drawing.Point(7, 23);
            this.tb_PipeLineSubsidence_Day.Name = "tb_PipeLineSubsidence_Day";
            this.tb_PipeLineSubsidence_Day.Size = new System.Drawing.Size(485, 12);
            this.tb_PipeLineSubsidence_Day.TabIndex = 139;
            this.tb_PipeLineSubsidence_Day.Text = "报警值：日报警值连续     天±     mm（有压）、±      mm（自流井）；累计      mm";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.groupBox2.Controls.Add(this.tb_SurfaceSubsidence_SumMillimeter);
            this.groupBox2.Controls.Add(this.tb_SurfaceSubsidence_Day);
            this.groupBox2.Controls.Add(this.tb_SurfaceSubsidence_DailyMillimeter);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(12, 66);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(529, 48);
            this.groupBox2.TabIndex = 151;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "地表沉降";
            // 
            // tb_SurfaceSubsidence_SumMillimeter
            // 
            this.tb_SurfaceSubsidence_SumMillimeter.Location = new System.Drawing.Point(274, 20);
            this.tb_SurfaceSubsidence_SumMillimeter.Name = "tb_SurfaceSubsidence_SumMillimeter";
            this.tb_SurfaceSubsidence_SumMillimeter.Size = new System.Drawing.Size(20, 21);
            this.tb_SurfaceSubsidence_SumMillimeter.TabIndex = 141;
            this.tb_SurfaceSubsidence_SumMillimeter.Text = "23";
            // 
            // tb_SurfaceSubsidence_Day
            // 
            this.tb_SurfaceSubsidence_Day.Location = new System.Drawing.Point(141, 20);
            this.tb_SurfaceSubsidence_Day.Name = "tb_SurfaceSubsidence_Day";
            this.tb_SurfaceSubsidence_Day.Size = new System.Drawing.Size(16, 21);
            this.tb_SurfaceSubsidence_Day.TabIndex = 138;
            this.tb_SurfaceSubsidence_Day.Text = "2";
            // 
            // tb_SurfaceSubsidence_DailyMillimeter
            // 
            this.tb_SurfaceSubsidence_DailyMillimeter.Location = new System.Drawing.Point(196, 20);
            this.tb_SurfaceSubsidence_DailyMillimeter.Name = "tb_SurfaceSubsidence_DailyMillimeter";
            this.tb_SurfaceSubsidence_DailyMillimeter.Size = new System.Drawing.Size(16, 21);
            this.tb_SurfaceSubsidence_DailyMillimeter.TabIndex = 140;
            this.tb_SurfaceSubsidence_DailyMillimeter.Text = "2";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(305, 12);
            this.label6.TabIndex = 139;
            this.label6.Text = "报警值：日报警值连续     天±     mm；累计      mm";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tb_BuildingSubsidence_SumMillimeter);
            this.groupBox1.Controls.Add(this.tb_BuildingSubsidence_Day);
            this.groupBox1.Controls.Add(this.tb_BuildingSubsidence_DailyMillimeter);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(529, 48);
            this.groupBox1.TabIndex = 150;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "建筑物沉降";
            // 
            // tb_BuildingSubsidence_SumMillimeter
            // 
            this.tb_BuildingSubsidence_SumMillimeter.Location = new System.Drawing.Point(274, 20);
            this.tb_BuildingSubsidence_SumMillimeter.Name = "tb_BuildingSubsidence_SumMillimeter";
            this.tb_BuildingSubsidence_SumMillimeter.Size = new System.Drawing.Size(20, 21);
            this.tb_BuildingSubsidence_SumMillimeter.TabIndex = 141;
            this.tb_BuildingSubsidence_SumMillimeter.Text = "20";
            // 
            // tb_BuildingSubsidence_Day
            // 
            this.tb_BuildingSubsidence_Day.Location = new System.Drawing.Point(141, 20);
            this.tb_BuildingSubsidence_Day.Name = "tb_BuildingSubsidence_Day";
            this.tb_BuildingSubsidence_Day.Size = new System.Drawing.Size(16, 21);
            this.tb_BuildingSubsidence_Day.TabIndex = 138;
            this.tb_BuildingSubsidence_Day.Text = "2";
            // 
            // tb_BuildingSubsidence_DailyMillimeter
            // 
            this.tb_BuildingSubsidence_DailyMillimeter.Location = new System.Drawing.Point(196, 20);
            this.tb_BuildingSubsidence_DailyMillimeter.Name = "tb_BuildingSubsidence_DailyMillimeter";
            this.tb_BuildingSubsidence_DailyMillimeter.Size = new System.Drawing.Size(16, 21);
            this.tb_BuildingSubsidence_DailyMillimeter.TabIndex = 140;
            this.tb_BuildingSubsidence_DailyMillimeter.Text = "1";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(305, 12);
            this.label5.TabIndex = 139;
            this.label5.Text = "报警值：日报警值连续     天±     mm；累计      mm";
            // 
            // WarnSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 368);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Submit);
            this.Controls.Add(this.GroupBox_SkewBack);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "WarnSettingsForm";
            this.Text = "报警值设定";
            this.GroupBox_SkewBack.ResumeLayout(false);
            this.GroupBox_SkewBack.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_Submit;
        private System.Windows.Forms.GroupBox GroupBox_SkewBack;
        private System.Windows.Forms.TextBox tb_SkewBack_Speed;
        private System.Windows.Forms.TextBox tb_SkewBack_StandardMillimeter;
        private System.Windows.Forms.TextBox tb_SkewBack_WellMillimeter;
        private System.Windows.Forms.TextBox tb_SkewBack_Day;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox tb_STBAP_MaxAxle;
        private System.Windows.Forms.TextBox tb_STBAP_MinAxle;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox tb_UnstressedPipeLineSubsidence_SumMillimeter;
        private System.Windows.Forms.TextBox tb_UnstressedPipeLineSubsidence_WellMillimeter;
        private System.Windows.Forms.TextBox tb_UnstressedPipeLineSubsidence_Day;
        private System.Windows.Forms.TextBox tb_UnstressedPipeLineSubsidence_PipelineMillimeter;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox tb_StressedPipeLineSubsidence_SumMillimeter;
        private System.Windows.Forms.TextBox tb_StressedPipeLineSubsidence_WellMillimeter;
        private System.Windows.Forms.TextBox tb_StressedPipeLineSubsidence_Day;
        private System.Windows.Forms.TextBox tb_StressedPipeLineSubsidence_PipelineMillimeter;
        private System.Windows.Forms.Label tb_PipeLineSubsidence_Day;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tb_SurfaceSubsidence_SumMillimeter;
        private System.Windows.Forms.TextBox tb_SurfaceSubsidence_Day;
        private System.Windows.Forms.TextBox tb_SurfaceSubsidence_DailyMillimeter;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tb_BuildingSubsidence_SumMillimeter;
        private System.Windows.Forms.TextBox tb_BuildingSubsidence_Day;
        private System.Windows.Forms.TextBox tb_BuildingSubsidence_DailyMillimeter;
        private System.Windows.Forms.Label label5;
    }
}