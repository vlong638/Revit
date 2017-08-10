//namespace MyRevit.MyTests.UI
//{
//    partial class PipeAnnotationForm
//    {
//        /// <summary>
//        /// Required designer variable.
//        /// </summary>
//        private System.ComponentModel.IContainer components = null;

//        /// <summary>
//        /// Clean up any resources being used.
//        /// </summary>
//        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && (components != null))
//            {
//                components.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        #region Windows Form Designer generated code

//        /// <summary>
//        /// Required method for Designer support - do not modify
//        /// the contents of this method with the code editor.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            this.group_Single = new System.Windows.Forms.GroupBox();
//            this.label2 = new System.Windows.Forms.Label();
//            this.tb_LengthFromLine = new System.Windows.Forms.TextBox();
//            this.label1 = new System.Windows.Forms.Label();
//            this.cb_Lead = new System.Windows.Forms.CheckBox();
//            this.checkBox1 = new System.Windows.Forms.CheckBox();
//            this.btn_AllSinglePipe = new System.Windows.Forms.Button();
//            this.btn_PickSinglePipe = new System.Windows.Forms.Button();
//            this.rb_AbovePipe = new System.Windows.Forms.RadioButton();
//            this.rb_OnPipe = new System.Windows.Forms.RadioButton();
//            this.groupBox1 = new System.Windows.Forms.GroupBox();
//            this.label3 = new System.Windows.Forms.Label();
//            this.tb_LengthBetweenPipe = new System.Windows.Forms.TextBox();
//            this.label4 = new System.Windows.Forms.Label();
//            this.checkBox3 = new System.Windows.Forms.CheckBox();
//            this.btn_AllMulltiplePipe = new System.Windows.Forms.Button();
//            this.btn_PickMultiPipe = new System.Windows.Forms.Button();
//            this.rb_OnLine = new System.Windows.Forms.RadioButton();
//            this.rb_OnLineEdge = new System.Windows.Forms.RadioButton();
//            this.label5 = new System.Windows.Forms.Label();
//            this.checkBox4 = new System.Windows.Forms.CheckBox();
//            this.checkBox5 = new System.Windows.Forms.CheckBox();
//            this.group_Single.SuspendLayout();
//            this.groupBox1.SuspendLayout();
//            this.SuspendLayout();
//            // 
//            // group_Single
//            // 
//            this.group_Single.Controls.Add(this.label2);
//            this.group_Single.Controls.Add(this.tb_LengthFromLine);
//            this.group_Single.Controls.Add(this.label1);
//            this.group_Single.Controls.Add(this.cb_Lead);
//            this.group_Single.Controls.Add(this.checkBox1);
//            this.group_Single.Controls.Add(this.btn_AllSinglePipe);
//            this.group_Single.Controls.Add(this.btn_PickSinglePipe);
//            this.group_Single.Controls.Add(this.rb_AbovePipe);
//            this.group_Single.Controls.Add(this.rb_OnPipe);
//            this.group_Single.Location = new System.Drawing.Point(12, 12);
//            this.group_Single.Name = "group_Single";
//            this.group_Single.Size = new System.Drawing.Size(260, 98);
//            this.group_Single.TabIndex = 0;
//            this.group_Single.TabStop = false;
//            this.group_Single.Text = "单管尺寸标注";
//            // 
//            // label2
//            // 
//            this.label2.AutoSize = true;
//            this.label2.Location = new System.Drawing.Point(213, 74);
//            this.label2.Name = "label2";
//            this.label2.Size = new System.Drawing.Size(17, 12);
//            this.label2.TabIndex = 8;
//            this.label2.Text = "mm";
//            // 
//            // tb_LengthFromLine
//            // 
//            this.tb_LengthFromLine.Location = new System.Drawing.Point(168, 71);
//            this.tb_LengthFromLine.Name = "tb_LengthFromLine";
//            this.tb_LengthFromLine.Size = new System.Drawing.Size(39, 21);
//            this.tb_LengthFromLine.TabIndex = 7;
//            // 
//            // label1
//            // 
//            this.label1.AutoSize = true;
//            this.label1.Location = new System.Drawing.Point(63, 74);
//            this.label1.Name = "label1";
//            this.label1.Size = new System.Drawing.Size(101, 12);
//            this.label1.TabIndex = 6;
//            this.label1.Text = "标记距离管道边缘";
//            // 
//            // cb_Lead
//            // 
//            this.cb_Lead.AutoSize = true;
//            this.cb_Lead.Location = new System.Drawing.Point(9, 73);
//            this.cb_Lead.Name = "cb_Lead";
//            this.cb_Lead.Size = new System.Drawing.Size(48, 16);
//            this.cb_Lead.TabIndex = 5;
//            this.cb_Lead.Text = "引线";
//            this.cb_Lead.UseVisualStyleBackColor = true;
//            // 
//            // checkBox1
//            // 
//            this.checkBox1.AutoSize = true;
//            this.checkBox1.Enabled = false;
//            this.checkBox1.Location = new System.Drawing.Point(168, 46);
//            this.checkBox1.Name = "checkBox1";
//            this.checkBox1.Size = new System.Drawing.Size(72, 16);
//            this.checkBox1.TabIndex = 4;
//            this.checkBox1.Text = "背景透明";
//            this.checkBox1.UseVisualStyleBackColor = true;
//            // 
//            // btn_AllSinglePipe
//            // 
//            this.btn_AllSinglePipe.Location = new System.Drawing.Point(87, 42);
//            this.btn_AllSinglePipe.Name = "btn_AllSinglePipe";
//            this.btn_AllSinglePipe.Size = new System.Drawing.Size(75, 23);
//            this.btn_AllSinglePipe.TabIndex = 3;
//            this.btn_AllSinglePipe.Text = "一键标注";
//            this.btn_AllSinglePipe.UseVisualStyleBackColor = true;
//            // 
//            // btn_PickSinglePipe
//            // 
//            this.btn_PickSinglePipe.Location = new System.Drawing.Point(6, 42);
//            this.btn_PickSinglePipe.Name = "btn_PickSinglePipe";
//            this.btn_PickSinglePipe.Size = new System.Drawing.Size(75, 23);
//            this.btn_PickSinglePipe.TabIndex = 2;
//            this.btn_PickSinglePipe.Text = "选管标注";
//            this.btn_PickSinglePipe.UseVisualStyleBackColor = true;
//            // 
//            // rb_AbovePipe
//            // 
//            this.rb_AbovePipe.AutoSize = true;
//            this.rb_AbovePipe.Location = new System.Drawing.Point(121, 20);
//            this.rb_AbovePipe.Name = "rb_AbovePipe";
//            this.rb_AbovePipe.Size = new System.Drawing.Size(119, 16);
//            this.rb_AbovePipe.TabIndex = 1;
//            this.rb_AbovePipe.TabStop = true;
//            this.rb_AbovePipe.Text = "文字位于管道上方";
//            this.rb_AbovePipe.UseVisualStyleBackColor = true;
//            // 
//            // rb_OnPipe
//            // 
//            this.rb_OnPipe.AutoSize = true;
//            this.rb_OnPipe.Location = new System.Drawing.Point(6, 20);
//            this.rb_OnPipe.Name = "rb_OnPipe";
//            this.rb_OnPipe.Size = new System.Drawing.Size(95, 16);
//            this.rb_OnPipe.TabIndex = 0;
//            this.rb_OnPipe.TabStop = true;
//            this.rb_OnPipe.Text = "文字位于管道";
//            this.rb_OnPipe.UseVisualStyleBackColor = true;
//            // 
//            // groupBox1
//            // 
//            this.groupBox1.Controls.Add(this.label3);
//            this.groupBox1.Controls.Add(this.tb_LengthBetweenPipe);
//            this.groupBox1.Controls.Add(this.label4);
//            this.groupBox1.Controls.Add(this.checkBox3);
//            this.groupBox1.Controls.Add(this.btn_AllMulltiplePipe);
//            this.groupBox1.Controls.Add(this.btn_PickMultiPipe);
//            this.groupBox1.Controls.Add(this.rb_OnLine);
//            this.groupBox1.Controls.Add(this.rb_OnLineEdge);
//            this.groupBox1.Location = new System.Drawing.Point(12, 116);
//            this.groupBox1.Name = "groupBox1";
//            this.groupBox1.Size = new System.Drawing.Size(260, 98);
//            this.groupBox1.TabIndex = 1;
//            this.groupBox1.TabStop = false;
//            this.groupBox1.Text = "多管尺寸标注";
//            // 
//            // label3
//            // 
//            this.label3.AutoSize = true;
//            this.label3.Location = new System.Drawing.Point(175, 73);
//            this.label3.Name = "label3";
//            this.label3.Size = new System.Drawing.Size(53, 12);
//            this.label3.TabIndex = 16;
//            this.label3.Text = "mm时生成";
//            // 
//            // tb_LengthBetweenPipe
//            // 
//            this.tb_LengthBetweenPipe.Location = new System.Drawing.Point(130, 70);
//            this.tb_LengthBetweenPipe.Name = "tb_LengthBetweenPipe";
//            this.tb_LengthBetweenPipe.Size = new System.Drawing.Size(39, 21);
//            this.tb_LengthBetweenPipe.TabIndex = 15;
//            // 
//            // label4
//            // 
//            this.label4.AutoSize = true;
//            this.label4.Location = new System.Drawing.Point(11, 73);
//            this.label4.Name = "label4";
//            this.label4.Size = new System.Drawing.Size(113, 12);
//            this.label4.TabIndex = 14;
//            this.label4.Text = "平行多管尺寸净距≤";
//            // 
//            // checkBox3
//            // 
//            this.checkBox3.AutoSize = true;
//            this.checkBox3.Enabled = false;
//            this.checkBox3.Location = new System.Drawing.Point(175, 45);
//            this.checkBox3.Name = "checkBox3";
//            this.checkBox3.Size = new System.Drawing.Size(72, 16);
//            this.checkBox3.TabIndex = 13;
//            this.checkBox3.Text = "背景透明";
//            this.checkBox3.UseVisualStyleBackColor = true;
//            // 
//            // btn_AllMulltiplePipe
//            // 
//            this.btn_AllMulltiplePipe.Location = new System.Drawing.Point(94, 41);
//            this.btn_AllMulltiplePipe.Name = "btn_AllMulltiplePipe";
//            this.btn_AllMulltiplePipe.Size = new System.Drawing.Size(75, 23);
//            this.btn_AllMulltiplePipe.TabIndex = 12;
//            this.btn_AllMulltiplePipe.Text = "一键标注";
//            this.btn_AllMulltiplePipe.UseVisualStyleBackColor = true;
//            // 
//            // btn_PickMultiPipe
//            // 
//            this.btn_PickMultiPipe.Location = new System.Drawing.Point(13, 41);
//            this.btn_PickMultiPipe.Name = "btn_PickMultiPipe";
//            this.btn_PickMultiPipe.Size = new System.Drawing.Size(75, 23);
//            this.btn_PickMultiPipe.TabIndex = 11;
//            this.btn_PickMultiPipe.Text = "选管标注";
//            this.btn_PickMultiPipe.UseVisualStyleBackColor = true;
//            // 
//            // rb_OnLine
//            // 
//            this.rb_OnLine.AutoSize = true;
//            this.rb_OnLine.Location = new System.Drawing.Point(128, 19);
//            this.rb_OnLine.Name = "rb_OnLine";
//            this.rb_OnLine.Size = new System.Drawing.Size(95, 16);
//            this.rb_OnLine.TabIndex = 10;
//            this.rb_OnLine.TabStop = true;
//            this.rb_OnLine.Text = "文字位于线上";
//            this.rb_OnLine.UseVisualStyleBackColor = true;
//            // 
//            // rb_OnLineEdge
//            // 
//            this.rb_OnLineEdge.AutoSize = true;
//            this.rb_OnLineEdge.Location = new System.Drawing.Point(13, 19);
//            this.rb_OnLineEdge.Name = "rb_OnLineEdge";
//            this.rb_OnLineEdge.Size = new System.Drawing.Size(95, 16);
//            this.rb_OnLineEdge.TabIndex = 9;
//            this.rb_OnLineEdge.TabStop = true;
//            this.rb_OnLineEdge.Text = "文字位于线端";
//            this.rb_OnLineEdge.UseVisualStyleBackColor = true;
//            // 
//            // label5
//            // 
//            this.label5.AutoSize = true;
//            this.label5.ForeColor = System.Drawing.Color.Red;
//            this.label5.Location = new System.Drawing.Point(13, 217);
//            this.label5.Name = "label5";
//            this.label5.Size = new System.Drawing.Size(227, 12);
//            this.label5.TabIndex = 2;
//            this.label5.Text = "注:一键标注对象为当前视图中可见的管道";
//            // 
//            // checkBox4
//            // 
//            this.checkBox4.AutoSize = true;
//            this.checkBox4.Location = new System.Drawing.Point(15, 234);
//            this.checkBox4.Name = "checkBox4";
//            this.checkBox4.Size = new System.Drawing.Size(132, 16);
//            this.checkBox4.TabIndex = 9;
//            this.checkBox4.Text = "包括链接进来的管道";
//            this.checkBox4.UseVisualStyleBackColor = true;
//            // 
//            // checkBox5
//            // 
//            this.checkBox5.AutoSize = true;
//            this.checkBox5.Enabled = false;
//            this.checkBox5.Location = new System.Drawing.Point(153, 234);
//            this.checkBox5.Name = "checkBox5";
//            this.checkBox5.Size = new System.Drawing.Size(96, 16);
//            this.checkBox5.TabIndex = 10;
//            this.checkBox5.Text = "标注自动避让";
//            this.checkBox5.UseVisualStyleBackColor = true;
//            // 
//            // PipeAnnotationForm
//            // 
//            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
//            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//            this.ClientSize = new System.Drawing.Size(284, 262);
//            this.Controls.Add(this.checkBox5);
//            this.Controls.Add(this.checkBox4);
//            this.Controls.Add(this.label5);
//            this.Controls.Add(this.groupBox1);
//            this.Controls.Add(this.group_Single);
//            this.Name = "PipeAnnotationForm";
//            this.Text = "管道尺寸标注";
//            this.group_Single.ResumeLayout(false);
//            this.group_Single.PerformLayout();
//            this.groupBox1.ResumeLayout(false);
//            this.groupBox1.PerformLayout();
//            this.ResumeLayout(false);
//            this.PerformLayout();

//        }

//        #endregion

//        private System.Windows.Forms.GroupBox group_Single;
//        private System.Windows.Forms.RadioButton rb_AbovePipe;
//        private System.Windows.Forms.RadioButton rb_OnPipe;
//        private System.Windows.Forms.Button btn_PickSinglePipe;
//        private System.Windows.Forms.Button btn_AllSinglePipe;
//        private System.Windows.Forms.CheckBox checkBox1;
//        private System.Windows.Forms.CheckBox cb_Lead;
//        private System.Windows.Forms.Label label1;
//        private System.Windows.Forms.TextBox tb_LengthFromLine;
//        private System.Windows.Forms.Label label2;
//        private System.Windows.Forms.GroupBox groupBox1;
//        private System.Windows.Forms.Label label3;
//        private System.Windows.Forms.TextBox tb_LengthBetweenPipe;
//        private System.Windows.Forms.Label label4;
//        private System.Windows.Forms.CheckBox checkBox3;
//        private System.Windows.Forms.Button btn_AllMulltiplePipe;
//        private System.Windows.Forms.Button btn_PickMultiPipe;
//        private System.Windows.Forms.RadioButton rb_OnLine;
//        private System.Windows.Forms.RadioButton rb_OnLineEdge;
//        private System.Windows.Forms.Label label5;
//        private System.Windows.Forms.CheckBox checkBox4;
//        private System.Windows.Forms.CheckBox checkBox5;
//    }
//}