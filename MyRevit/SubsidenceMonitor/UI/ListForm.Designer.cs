namespace MyRevit.SubsidenceMonitor.UI
{
    partial class ListForm
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
            this.cb_IssueType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_IssueMonth = new System.Windows.Forms.Button();
            this.dgv = new System.Windows.Forms.DataGridView();
            this.dgv_Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgv_Imported = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgv_Operation = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();
            // 
            // cb_IssueType
            // 
            this.cb_IssueType.FormattingEnabled = true;
            this.cb_IssueType.Location = new System.Drawing.Point(74, 17);
            this.cb_IssueType.Name = "cb_IssueType";
            this.cb_IssueType.Size = new System.Drawing.Size(110, 20);
            this.cb_IssueType.TabIndex = 0;
            this.cb_IssueType.SelectedIndexChanged += new System.EventHandler(this.cb_IssueType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "报告类型:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(190, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "报告月份:";
            // 
            // btn_IssueMonth
            // 
            this.btn_IssueMonth.Location = new System.Drawing.Point(254, 14);
            this.btn_IssueMonth.Name = "btn_IssueMonth";
            this.btn_IssueMonth.Size = new System.Drawing.Size(100, 23);
            this.btn_IssueMonth.TabIndex = 3;
            this.btn_IssueMonth.UseVisualStyleBackColor = true;
            this.btn_IssueMonth.Click += new System.EventHandler(this.btn_IssueMonth_Click);
            // 
            // dgv
            // 
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgv_Date,
            this.dgv_Imported,
            this.dgv_Operation});
            this.dgv.Location = new System.Drawing.Point(19, 44);
            this.dgv.Name = "dgv";
            this.dgv.RowTemplate.Height = 23;
            this.dgv.Size = new System.Drawing.Size(335, 738);
            this.dgv.TabIndex = 4;
            this.dgv.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellContentClick);
            // 
            // dgv_Date
            // 
            this.dgv_Date.HeaderText = "日期";
            this.dgv_Date.Name = "dgv_Date";
            this.dgv_Date.ReadOnly = true;
            this.dgv_Date.Width = 120;
            // 
            // dgv_Imported
            // 
            this.dgv_Imported.HeaderText = "数据状态";
            this.dgv_Imported.Name = "dgv_Imported";
            this.dgv_Imported.ReadOnly = true;
            this.dgv_Imported.Width = 90;
            // 
            // dgv_Operation
            // 
            this.dgv_Operation.HeaderText = "操作";
            this.dgv_Operation.Name = "dgv_Operation";
            this.dgv_Operation.Width = 80;
            // 
            // ListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 794);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.btn_IssueMonth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_IssueType);
            this.Name = "ListForm";
            this.Text = "监测列表";
            this.Click += new System.EventHandler(this.ListForm_Click);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cb_IssueType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_IssueMonth;
        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgv_Date;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgv_Imported;
        private System.Windows.Forms.DataGridViewButtonColumn dgv_Operation;
    }
}