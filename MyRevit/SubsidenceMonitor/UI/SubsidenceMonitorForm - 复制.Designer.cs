using System.Collections.Generic;
using WinformTests.SubsidenceMonitor.Controls;

namespace WinformTests
{
    partial class SubsidenceMonitorForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_ReportName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_SubsidenceType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_WarnArgs = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tb_InstrumentCode = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tb_InstrumentName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tb_Time = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tb_Date = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_LoadExcel = new System.Windows.Forms.Button();
            this.myDGV04271 = new WinformTests.SubsidenceMonitor.Controls.MyDGV0427();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.myDGV04271)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "报告名称:";
            // 
            // tb_ReportName
            // 
            this.tb_ReportName.Location = new System.Drawing.Point(77, 6);
            this.tb_ReportName.Name = "tb_ReportName";
            this.tb_ReportName.Size = new System.Drawing.Size(493, 21);
            this.tb_ReportName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(587, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "沉降类型:";
            // 
            // cb_SubsidenceType
            // 
            this.cb_SubsidenceType.FormattingEnabled = true;
            this.cb_SubsidenceType.Location = new System.Drawing.Point(652, 7);
            this.cb_SubsidenceType.Name = "cb_SubsidenceType";
            this.cb_SubsidenceType.Size = new System.Drawing.Size(121, 20);
            this.cb_SubsidenceType.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(784, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "报警值:";
            // 
            // tb_WarnArgs
            // 
            this.tb_WarnArgs.Location = new System.Drawing.Point(837, 6);
            this.tb_WarnArgs.Name = "tb_WarnArgs";
            this.tb_WarnArgs.Size = new System.Drawing.Size(135, 21);
            this.tb_WarnArgs.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(405, 377);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(8, 8);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dataGridView1);
            this.groupBox2.Controls.Add(this.myDGV04271);
            this.groupBox2.Controls.Add(this.tb_InstrumentCode);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.tb_InstrumentName);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.tb_Time);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.tb_Date);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.textBox3);
            this.groupBox2.Controls.Add(this.textBox2);
            this.groupBox2.Controls.Add(this.textBox1);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 33);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(959, 526);
            this.groupBox2.TabIndex = 22;
            this.groupBox2.TabStop = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(483, 122);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(476, 404);
            this.dataGridView1.TabIndex = 36;
            // 
            // tb_InstrumentCode
            // 
            this.tb_InstrumentCode.Location = new System.Drawing.Point(647, 86);
            this.tb_InstrumentCode.Name = "tb_InstrumentCode";
            this.tb_InstrumentCode.Size = new System.Drawing.Size(210, 21);
            this.tb_InstrumentCode.TabIndex = 34;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("宋体", 14F);
            this.label9.Location = new System.Drawing.Point(546, 86);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(95, 19);
            this.label9.TabIndex = 33;
            this.label9.Text = "仪器编号:";
            // 
            // tb_InstrumentName
            // 
            this.tb_InstrumentName.Location = new System.Drawing.Point(185, 86);
            this.tb_InstrumentName.Name = "tb_InstrumentName";
            this.tb_InstrumentName.Size = new System.Drawing.Size(210, 21);
            this.tb_InstrumentName.TabIndex = 32;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("宋体", 14F);
            this.label10.Location = new System.Drawing.Point(84, 86);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(95, 19);
            this.label10.TabIndex = 31;
            this.label10.Text = "仪器名称:";
            // 
            // tb_Time
            // 
            this.tb_Time.Location = new System.Drawing.Point(647, 59);
            this.tb_Time.Name = "tb_Time";
            this.tb_Time.Size = new System.Drawing.Size(210, 21);
            this.tb_Time.TabIndex = 30;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 14F);
            this.label8.Location = new System.Drawing.Point(546, 59);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 19);
            this.label8.TabIndex = 29;
            this.label8.Text = "监测时间:";
            // 
            // tb_Date
            // 
            this.tb_Date.Location = new System.Drawing.Point(185, 59);
            this.tb_Date.Name = "tb_Date";
            this.tb_Date.Size = new System.Drawing.Size(210, 21);
            this.tb_Date.TabIndex = 28;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 14F);
            this.label7.Location = new System.Drawing.Point(84, 59);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(95, 19);
            this.label7.TabIndex = 27;
            this.label7.Text = "监测日期:";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(720, 20);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(230, 21);
            this.textBox3.TabIndex = 26;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(401, 20);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(230, 21);
            this.textBox2.TabIndex = 25;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(86, 20);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(230, 21);
            this.textBox1.TabIndex = 24;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 10F);
            this.label6.Location = new System.Drawing.Point(644, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 14);
            this.label6.TabIndex = 23;
            this.label6.Text = "监测单位:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 10F);
            this.label5.Location = new System.Drawing.Point(328, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 14);
            this.label5.TabIndex = 22;
            this.label5.Text = "监理单位:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 10F);
            this.label4.Location = new System.Drawing.Point(10, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 14);
            this.label4.TabIndex = 21;
            this.label4.Text = "承包单位:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.btn_LoadExcel);
            this.groupBox3.Location = new System.Drawing.Point(14, 565);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(188, 68);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "测点编辑";
            // 
            // button1
            // 
            this.button1.Image = global::WinformTests.Properties.Resources.AddElement;
            this.button1.Location = new System.Drawing.Point(11, 14);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(32, 44);
            this.button1.TabIndex = 2;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btn_LoadExcel
            // 
            this.btn_LoadExcel.Location = new System.Drawing.Point(44, 14);
            this.btn_LoadExcel.Name = "btn_LoadExcel";
            this.btn_LoadExcel.Size = new System.Drawing.Size(44, 44);
            this.btn_LoadExcel.TabIndex = 1;
            this.btn_LoadExcel.Text = "导入Excel";
            this.btn_LoadExcel.UseVisualStyleBackColor = true;
            // 
            // myDGV04271
            // 
            this.myDGV04271.AllowUserToAddRows = false;
            this.myDGV04271.AllowUserToDeleteRows = false;
            this.myDGV04271.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.myDGV04271.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.myDGV04271.HeaderNodes = null;
            this.myDGV04271.Location = new System.Drawing.Point(0, 122);
            this.myDGV04271.Name = "myDGV04271";
            this.myDGV04271.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.myDGV04271.RowTemplate.Height = 23;
            this.myDGV04271.Size = new System.Drawing.Size(477, 404);
            this.myDGV04271.TabIndex = 35;
            // 
            // SubsidenceMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(985, 645);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tb_WarnArgs);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cb_SubsidenceType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_ReportName);
            this.Controls.Add(this.label1);
            this.Name = "SubsidenceMonitorForm";
            this.Text = "SubsidenceMonitorForm";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.myDGV04271)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_ReportName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cb_SubsidenceType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_WarnArgs;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tb_InstrumentCode;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tb_InstrumentName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tb_Time;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tb_Date;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_LoadExcel;
        private MyDGV0427 myDGV04271;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}