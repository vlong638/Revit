﻿using System.Collections.Generic;
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_ReportName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_SubsidenceType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_WarnArgs = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
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
            this.dgv_right = new WinformTests.SubsidenceMonitor.Controls.MyDGV0427();
            this.dgv_left = new WinformTests.SubsidenceMonitor.Controls.MyDGV0427();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.button14 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_right)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_left)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
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
            this.groupBox2.Controls.Add(this.dgv_right);
            this.groupBox2.Controls.Add(this.dgv_left);
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
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.btn_LoadExcel);
            this.groupBox3.Location = new System.Drawing.Point(14, 565);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(182, 68);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "测点编辑";
            // 
            // button1
            // 
            this.button1.Image = global::WinformTests.Properties.Resources.AddElement;
            this.button1.Location = new System.Drawing.Point(6, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(32, 44);
            this.button1.TabIndex = 2;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btn_LoadExcel
            // 
            this.btn_LoadExcel.Location = new System.Drawing.Point(44, 20);
            this.btn_LoadExcel.Name = "btn_LoadExcel";
            this.btn_LoadExcel.Size = new System.Drawing.Size(44, 44);
            this.btn_LoadExcel.TabIndex = 1;
            this.btn_LoadExcel.Text = "录入Excel";
            this.btn_LoadExcel.UseVisualStyleBackColor = true;
            this.btn_LoadExcel.Click += new System.EventHandler(this.btn_LoadExcel_Click);
            // 
            // dgv_right
            // 
            this.dgv_right.AllowUserToAddRows = false;
            this.dgv_right.AllowUserToDeleteRows = false;
            this.dgv_right.AllowUserToResizeColumns = false;
            this.dgv_right.AllowUserToResizeRows = false;
            dataGridViewCellStyle25.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomCenter;
            dataGridViewCellStyle25.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle25.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle25.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle25.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle25.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle25.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_right.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle25;
            this.dgv_right.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgv_right.HeaderNodes = null;
            this.dgv_right.Location = new System.Drawing.Point(480, 122);
            this.dgv_right.Name = "dgv_right";
            this.dgv_right.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgv_right.RowTemplate.Height = 23;
            this.dgv_right.Size = new System.Drawing.Size(479, 404);
            this.dgv_right.TabIndex = 36;
            // 
            // dgv_left
            // 
            this.dgv_left.AllowUserToAddRows = false;
            this.dgv_left.AllowUserToDeleteRows = false;
            this.dgv_left.AllowUserToResizeColumns = false;
            this.dgv_left.AllowUserToResizeRows = false;
            dataGridViewCellStyle26.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomCenter;
            dataGridViewCellStyle26.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle26.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle26.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle26.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle26.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle26.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_left.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle26;
            this.dgv_left.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgv_left.HeaderNodes = null;
            this.dgv_left.Location = new System.Drawing.Point(0, 122);
            this.dgv_left.Name = "dgv_left";
            this.dgv_left.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgv_left.RowTemplate.Height = 23;
            this.dgv_left.Size = new System.Drawing.Size(479, 404);
            this.dgv_left.TabIndex = 35;
            // 
            // button2
            // 
            this.button2.Image = global::WinformTests.Properties.Resources.AddElement;
            this.button2.Location = new System.Drawing.Point(94, 20);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(32, 44);
            this.button2.TabIndex = 3;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(132, 20);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(44, 44);
            this.button3.TabIndex = 4;
            this.button3.Text = "测点赋值";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.button5);
            this.groupBox4.Controls.Add(this.button4);
            this.groupBox4.Location = new System.Drawing.Point(202, 565);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(106, 68);
            this.groupBox4.TabIndex = 24;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "测点查看";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(6, 18);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(44, 44);
            this.button4.TabIndex = 5;
            this.button4.Text = "选中查看";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(56, 18);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(44, 44);
            this.button5.TabIndex = 6;
            this.button5.Text = "查看全部";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.button6);
            this.groupBox5.Controls.Add(this.button7);
            this.groupBox5.Location = new System.Drawing.Point(314, 565);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(122, 68);
            this.groupBox5.TabIndex = 25;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "本次最大变量查看";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(66, 17);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(44, 44);
            this.button6.TabIndex = 6;
            this.button6.Text = "整体查看";
            this.button6.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(16, 17);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(44, 44);
            this.button7.TabIndex = 5;
            this.button7.Text = "红色显示";
            this.button7.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.button8);
            this.groupBox6.Controls.Add(this.button9);
            this.groupBox6.Location = new System.Drawing.Point(442, 565);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(122, 68);
            this.groupBox6.TabIndex = 26;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "累计最大变量查看";
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(66, 17);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(44, 44);
            this.button8.TabIndex = 6;
            this.button8.Text = "整体查看";
            this.button8.UseVisualStyleBackColor = true;
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(16, 17);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(44, 44);
            this.button9.TabIndex = 5;
            this.button9.Text = "红色显示";
            this.button9.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(570, 582);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(71, 12);
            this.label11.TabIndex = 27;
            this.label11.Text = "接近预警值:";
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(647, 577);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(79, 23);
            this.button10.TabIndex = 28;
            this.button10.Text = "颜色/透明度";
            this.button10.UseVisualStyleBackColor = true;
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(732, 577);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(85, 23);
            this.button11.TabIndex = 29;
            this.button11.Text = "接近预警预览";
            this.button11.UseVisualStyleBackColor = true;
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(732, 603);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(85, 23);
            this.button12.TabIndex = 32;
            this.button12.Text = "接近预警预览";
            this.button12.UseVisualStyleBackColor = true;
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(647, 603);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(79, 23);
            this.button13.TabIndex = 31;
            this.button13.Text = "颜色/透明度";
            this.button13.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(570, 608);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(71, 12);
            this.label12.TabIndex = 30;
            this.label12.Text = "接近预警值:";
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(887, 577);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(86, 23);
            this.button14.TabIndex = 33;
            this.button14.Text = "保存";
            this.button14.UseVisualStyleBackColor = true;
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(888, 603);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(85, 23);
            this.button15.TabIndex = 34;
            this.button15.Text = "取消";
            this.button15.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(18, 646);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(137, 12);
            this.label13.TabIndex = 35;
            this.label13.Text = "注:+为向坑内,-为向坑外";
            // 
            // SubsidenceMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(985, 670);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.button15);
            this.Controls.Add(this.button14);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.button13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
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
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_right)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_left)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
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
        private MyDGV0427 dgv_left;
        private MyDGV0427 dgv_right;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Label label13;
    }
}