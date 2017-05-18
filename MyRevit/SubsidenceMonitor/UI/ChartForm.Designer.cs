namespace MyRevit.SubsidenceMonitor.UI
{
    partial class ChartForm
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_ReportName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_StartDate = new System.Windows.Forms.Button();
            this.cb_IssueType = new System.Windows.Forms.ComboBox();
            this.cb_Y = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btn_Export = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_Submit = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_NodeCode = new System.Windows.Forms.ComboBox();
            this.lbl_Depth = new System.Windows.Forms.Label();
            this.cb_Depth = new System.Windows.Forms.ComboBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "图表名称:";
            // 
            // tb_ReportName
            // 
            this.tb_ReportName.Location = new System.Drawing.Point(78, 10);
            this.tb_ReportName.Name = "tb_ReportName";
            this.tb_ReportName.Size = new System.Drawing.Size(331, 21);
            this.tb_ReportName.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(415, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "报告类型:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(415, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "日期起点:";
            // 
            // btn_StartDate
            // 
            this.btn_StartDate.Location = new System.Drawing.Point(480, 40);
            this.btn_StartDate.Name = "btn_StartDate";
            this.btn_StartDate.Size = new System.Drawing.Size(133, 23);
            this.btn_StartDate.TabIndex = 6;
            this.btn_StartDate.UseVisualStyleBackColor = true;
            this.btn_StartDate.TextChanged += new System.EventHandler(this.btn_StartDate_TextChanged);
            this.btn_StartDate.Click += new System.EventHandler(this.btn_StartDate_Click);
            // 
            // cb_IssueType
            // 
            this.cb_IssueType.FormattingEnabled = true;
            this.cb_IssueType.Location = new System.Drawing.Point(480, 10);
            this.cb_IssueType.Name = "cb_IssueType";
            this.cb_IssueType.Size = new System.Drawing.Size(133, 20);
            this.cb_IssueType.TabIndex = 7;
            this.cb_IssueType.TextChanged += new System.EventHandler(this.cb_IssueType_TextChanged);
            // 
            // cb_Y
            // 
            this.cb_Y.FormattingEnabled = true;
            this.cb_Y.Location = new System.Drawing.Point(72, 441);
            this.cb_Y.Name = "cb_Y";
            this.cb_Y.Size = new System.Drawing.Size(133, 20);
            this.cb_Y.TabIndex = 10;
            this.cb_Y.TextChanged += new System.EventHandler(this.cb_Y_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 444);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "Y轴数据:";
            // 
            // btn_Export
            // 
            this.btn_Export.Location = new System.Drawing.Point(489, 439);
            this.btn_Export.Name = "btn_Export";
            this.btn_Export.Size = new System.Drawing.Size(75, 23);
            this.btn_Export.TabIndex = 11;
            this.btn_Export.Text = "导出";
            this.btn_Export.UseVisualStyleBackColor = true;
            this.btn_Export.Click += new System.EventHandler(this.btn_Export_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(408, 439);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 12;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_Submit
            // 
            this.btn_Submit.Location = new System.Drawing.Point(327, 439);
            this.btn_Submit.Name = "btn_Submit";
            this.btn_Submit.Size = new System.Drawing.Size(75, 23);
            this.btn_Submit.TabIndex = 13;
            this.btn_Submit.Text = "确定";
            this.btn_Submit.UseVisualStyleBackColor = true;
            this.btn_Submit.Click += new System.EventHandler(this.btn_Submit_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "测点名称:";
            // 
            // cb_NodeCode
            // 
            this.cb_NodeCode.FormattingEnabled = true;
            this.cb_NodeCode.Location = new System.Drawing.Point(78, 42);
            this.cb_NodeCode.Name = "cb_NodeCode";
            this.cb_NodeCode.Size = new System.Drawing.Size(127, 20);
            this.cb_NodeCode.TabIndex = 14;
            this.cb_NodeCode.TextChanged += new System.EventHandler(this.cb_NodeCode_TextChanged);
            // 
            // lbl_Depth
            // 
            this.lbl_Depth.AutoSize = true;
            this.lbl_Depth.Location = new System.Drawing.Point(217, 45);
            this.lbl_Depth.Name = "lbl_Depth";
            this.lbl_Depth.Size = new System.Drawing.Size(35, 12);
            this.lbl_Depth.TabIndex = 15;
            this.lbl_Depth.Text = "深度:";
            // 
            // cb_Depth
            // 
            this.cb_Depth.FormattingEnabled = true;
            this.cb_Depth.Location = new System.Drawing.Point(258, 42);
            this.cb_Depth.Name = "cb_Depth";
            this.cb_Depth.Size = new System.Drawing.Size(127, 20);
            this.cb_Depth.TabIndex = 16;
            this.cb_Depth.TextChanged += new System.EventHandler(this.cb_Depth_TextChanged);
            // 
            // chart1
            // 
            chartArea4.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea4);
            legend4.Name = "Legend1";
            this.chart1.Legends.Add(legend4);
            this.chart1.Location = new System.Drawing.Point(12, 68);
            this.chart1.Name = "chart1";
            series4.ChartArea = "ChartArea1";
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            this.chart1.Series.Add(series4);
            this.chart1.Size = new System.Drawing.Size(601, 367);
            this.chart1.TabIndex = 17;
            this.chart1.Text = "chart1";
            // 
            // ChartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 471);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.cb_Depth);
            this.Controls.Add(this.lbl_Depth);
            this.Controls.Add(this.cb_NodeCode);
            this.Controls.Add(this.btn_Submit);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Export);
            this.Controls.Add(this.cb_Y);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cb_IssueType);
            this.Controls.Add(this.btn_StartDate);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_ReportName);
            this.Controls.Add(this.label1);
            this.Name = "ChartForm";
            this.Text = "ChartForm";
            this.Click += new System.EventHandler(this.ChartForm_Click);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_ReportName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btn_StartDate;
        private System.Windows.Forms.ComboBox cb_IssueType;
        private System.Windows.Forms.ComboBox cb_Y;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btn_Export;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_Submit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cb_NodeCode;
        private System.Windows.Forms.Label lbl_Depth;
        private System.Windows.Forms.ComboBox cb_Depth;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
    }
}