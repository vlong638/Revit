namespace MyRevit.SubsidenceMonitor.UI
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_ReportName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_WarnArgs = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgv_left = new MyRevit.MyDGV0427();
            this.dgv_right = new MyRevit.MyDGV0427();
            this.tb_InstrumentCode = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tb_InstrumentName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tb_Time = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tb_Date = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tb_Monitor = new System.Windows.Forms.TextBox();
            this.tb_Supervisor = new System.Windows.Forms.TextBox();
            this.tb_Contractor = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btn_RenderComponent = new System.Windows.Forms.Button();
            this.btn_DeleteComponent = new System.Windows.Forms.Button();
            this.btn_AddComponent = new System.Windows.Forms.Button();
            this.btn_LoadExcel = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btn_ViewAll = new System.Windows.Forms.Button();
            this.btn_ViewSelection = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btn_ViewCurrentMax_All = new System.Windows.Forms.Button();
            this.btn_ViewCurrentMax_Red = new System.Windows.Forms.Button();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.btn_ViewSumMax_All = new System.Windows.Forms.Button();
            this.btn_ViewSumMax_Red = new System.Windows.Forms.Button();
            this.btn_Submit = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.btn_Delete = new System.Windows.Forms.Button();
            this.btn_Next = new System.Windows.Forms.Button();
            this.btn_Previous = new System.Windows.Forms.Button();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.btn_ViewOverWarn = new System.Windows.Forms.Button();
            this.btn_OverWarnSettings = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.btn_ViewCloseWarn = new System.Windows.Forms.Button();
            this.btn_CloseWarnSettings = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.tb_IssueType = new System.Windows.Forms.TextBox();
            this.btn_CreateNew = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_left)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_right)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(96, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "报告名称:";
            // 
            // tb_ReportName
            // 
            this.tb_ReportName.Location = new System.Drawing.Point(161, 10);
            this.tb_ReportName.Name = "tb_ReportName";
            this.tb_ReportName.Size = new System.Drawing.Size(493, 21);
            this.tb_ReportName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(671, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "沉降类型:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(868, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "报警值:";
            // 
            // tb_WarnArgs
            // 
            this.tb_WarnArgs.Location = new System.Drawing.Point(921, 10);
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
            this.groupBox2.Controls.Add(this.dgv_left);
            this.groupBox2.Controls.Add(this.dgv_right);
            this.groupBox2.Controls.Add(this.tb_InstrumentCode);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.tb_InstrumentName);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.tb_Time);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.tb_Date);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.tb_Monitor);
            this.groupBox2.Controls.Add(this.tb_Supervisor);
            this.groupBox2.Controls.Add(this.tb_Contractor);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 33);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1140, 526);
            this.groupBox2.TabIndex = 22;
            this.groupBox2.TabStop = false;
            // 
            // dgv_left
            // 
            this.dgv_left.AllowUserToAddRows = false;
            this.dgv_left.AllowUserToDeleteRows = false;
            this.dgv_left.AllowUserToResizeColumns = false;
            this.dgv_left.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_left.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_left.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_left.HeaderNodes = null;
            this.dgv_left.Location = new System.Drawing.Point(571, 108);
            this.dgv_left.Name = "dgv_left";
            this.dgv_left.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgv_left.RowTemplate.Height = 23;
            this.dgv_left.Size = new System.Drawing.Size(568, 418);
            this.dgv_left.TabIndex = 36;
            // 
            // dgv_right
            // 
            this.dgv_right.AllowUserToAddRows = false;
            this.dgv_right.AllowUserToDeleteRows = false;
            this.dgv_right.AllowUserToResizeColumns = false;
            this.dgv_right.AllowUserToResizeRows = false;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_right.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgv_right.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_right.HeaderNodes = null;
            this.dgv_right.Location = new System.Drawing.Point(1, 108);
            this.dgv_right.Name = "dgv_right";
            this.dgv_right.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgv_right.RowTemplate.Height = 23;
            this.dgv_right.Size = new System.Drawing.Size(568, 418);
            this.dgv_right.TabIndex = 35;
            // 
            // tb_InstrumentCode
            // 
            this.tb_InstrumentCode.Location = new System.Drawing.Point(726, 86);
            this.tb_InstrumentCode.Name = "tb_InstrumentCode";
            this.tb_InstrumentCode.Size = new System.Drawing.Size(210, 21);
            this.tb_InstrumentCode.TabIndex = 34;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("宋体", 14F);
            this.label9.Location = new System.Drawing.Point(625, 86);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(95, 19);
            this.label9.TabIndex = 33;
            this.label9.Text = "仪器编号:";
            // 
            // tb_InstrumentName
            // 
            this.tb_InstrumentName.Location = new System.Drawing.Point(264, 86);
            this.tb_InstrumentName.Name = "tb_InstrumentName";
            this.tb_InstrumentName.Size = new System.Drawing.Size(210, 21);
            this.tb_InstrumentName.TabIndex = 32;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("宋体", 14F);
            this.label10.Location = new System.Drawing.Point(163, 86);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(95, 19);
            this.label10.TabIndex = 31;
            this.label10.Text = "仪器名称:";
            // 
            // tb_Time
            // 
            this.tb_Time.Location = new System.Drawing.Point(726, 59);
            this.tb_Time.Name = "tb_Time";
            this.tb_Time.Size = new System.Drawing.Size(210, 21);
            this.tb_Time.TabIndex = 30;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 14F);
            this.label8.Location = new System.Drawing.Point(625, 59);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 19);
            this.label8.TabIndex = 29;
            this.label8.Text = "监测时间:";
            // 
            // tb_Date
            // 
            this.tb_Date.Location = new System.Drawing.Point(264, 59);
            this.tb_Date.Name = "tb_Date";
            this.tb_Date.Size = new System.Drawing.Size(210, 21);
            this.tb_Date.TabIndex = 28;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 14F);
            this.label7.Location = new System.Drawing.Point(163, 59);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(95, 19);
            this.label7.TabIndex = 27;
            this.label7.Text = "监测日期:";
            // 
            // tb_Monitor
            // 
            this.tb_Monitor.Location = new System.Drawing.Point(799, 20);
            this.tb_Monitor.Name = "tb_Monitor";
            this.tb_Monitor.Size = new System.Drawing.Size(230, 21);
            this.tb_Monitor.TabIndex = 26;
            // 
            // tb_Supervisor
            // 
            this.tb_Supervisor.Location = new System.Drawing.Point(480, 20);
            this.tb_Supervisor.Name = "tb_Supervisor";
            this.tb_Supervisor.Size = new System.Drawing.Size(230, 21);
            this.tb_Supervisor.TabIndex = 25;
            // 
            // tb_Contractor
            // 
            this.tb_Contractor.Location = new System.Drawing.Point(165, 20);
            this.tb_Contractor.Name = "tb_Contractor";
            this.tb_Contractor.Size = new System.Drawing.Size(230, 21);
            this.tb_Contractor.TabIndex = 24;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 10F);
            this.label6.Location = new System.Drawing.Point(723, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 14);
            this.label6.TabIndex = 23;
            this.label6.Text = "监测单位:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 10F);
            this.label5.Location = new System.Drawing.Point(407, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 14);
            this.label5.TabIndex = 22;
            this.label5.Text = "监理单位:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 10F);
            this.label4.Location = new System.Drawing.Point(89, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 14);
            this.label4.TabIndex = 21;
            this.label4.Text = "承包单位:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btn_RenderComponent);
            this.groupBox3.Controls.Add(this.btn_DeleteComponent);
            this.groupBox3.Controls.Add(this.btn_AddComponent);
            this.groupBox3.Controls.Add(this.btn_LoadExcel);
            this.groupBox3.Location = new System.Drawing.Point(12, 590);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(182, 79);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "测点编辑";
            // 
            // btn_RenderComponent
            // 
            this.btn_RenderComponent.Location = new System.Drawing.Point(132, 20);
            this.btn_RenderComponent.Name = "btn_RenderComponent";
            this.btn_RenderComponent.Size = new System.Drawing.Size(44, 55);
            this.btn_RenderComponent.TabIndex = 4;
            this.btn_RenderComponent.Text = "测点赋值";
            this.btn_RenderComponent.UseVisualStyleBackColor = true;
            // 
            // btn_DeleteComponent
            // 
            this.btn_DeleteComponent.Image = global::MyRevit.Properties.Resources.AddElement;
            this.btn_DeleteComponent.Location = new System.Drawing.Point(94, 20);
            this.btn_DeleteComponent.Name = "btn_DeleteComponent";
            this.btn_DeleteComponent.Size = new System.Drawing.Size(32, 55);
            this.btn_DeleteComponent.TabIndex = 3;
            this.btn_DeleteComponent.UseVisualStyleBackColor = true;
            this.btn_DeleteComponent.Click += new System.EventHandler(this.btn_DeleteComponent_Click);
            // 
            // btn_AddComponent
            // 
            this.btn_AddComponent.Image = global::MyRevit.Properties.Resources.AddElement;
            this.btn_AddComponent.Location = new System.Drawing.Point(6, 20);
            this.btn_AddComponent.Name = "btn_AddComponent";
            this.btn_AddComponent.Size = new System.Drawing.Size(32, 55);
            this.btn_AddComponent.TabIndex = 2;
            this.btn_AddComponent.UseVisualStyleBackColor = true;
            this.btn_AddComponent.Click += new System.EventHandler(this.btn_AddComponent_Click);
            // 
            // btn_LoadExcel
            // 
            this.btn_LoadExcel.Location = new System.Drawing.Point(44, 20);
            this.btn_LoadExcel.Name = "btn_LoadExcel";
            this.btn_LoadExcel.Size = new System.Drawing.Size(44, 55);
            this.btn_LoadExcel.TabIndex = 1;
            this.btn_LoadExcel.Text = "录入Excel";
            this.btn_LoadExcel.UseVisualStyleBackColor = true;
            this.btn_LoadExcel.Click += new System.EventHandler(this.btn_LoadExcel_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btn_ViewAll);
            this.groupBox4.Controls.Add(this.btn_ViewSelection);
            this.groupBox4.Location = new System.Drawing.Point(200, 590);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(106, 79);
            this.groupBox4.TabIndex = 24;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "测点查看";
            // 
            // btn_ViewAll
            // 
            this.btn_ViewAll.Location = new System.Drawing.Point(56, 18);
            this.btn_ViewAll.Name = "btn_ViewAll";
            this.btn_ViewAll.Size = new System.Drawing.Size(44, 55);
            this.btn_ViewAll.TabIndex = 6;
            this.btn_ViewAll.Text = "查看全部";
            this.btn_ViewAll.UseVisualStyleBackColor = true;
            // 
            // btn_ViewSelection
            // 
            this.btn_ViewSelection.Location = new System.Drawing.Point(6, 18);
            this.btn_ViewSelection.Name = "btn_ViewSelection";
            this.btn_ViewSelection.Size = new System.Drawing.Size(44, 55);
            this.btn_ViewSelection.TabIndex = 5;
            this.btn_ViewSelection.Text = "选中查看";
            this.btn_ViewSelection.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.btn_ViewCurrentMax_All);
            this.groupBox5.Controls.Add(this.btn_ViewCurrentMax_Red);
            this.groupBox5.Location = new System.Drawing.Point(312, 590);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(122, 79);
            this.groupBox5.TabIndex = 25;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "本次最大变量查看";
            // 
            // btn_ViewCurrentMax_All
            // 
            this.btn_ViewCurrentMax_All.Location = new System.Drawing.Point(66, 17);
            this.btn_ViewCurrentMax_All.Name = "btn_ViewCurrentMax_All";
            this.btn_ViewCurrentMax_All.Size = new System.Drawing.Size(44, 55);
            this.btn_ViewCurrentMax_All.TabIndex = 6;
            this.btn_ViewCurrentMax_All.Text = "整体查看";
            this.btn_ViewCurrentMax_All.UseVisualStyleBackColor = true;
            // 
            // btn_ViewCurrentMax_Red
            // 
            this.btn_ViewCurrentMax_Red.Location = new System.Drawing.Point(16, 17);
            this.btn_ViewCurrentMax_Red.Name = "btn_ViewCurrentMax_Red";
            this.btn_ViewCurrentMax_Red.Size = new System.Drawing.Size(44, 55);
            this.btn_ViewCurrentMax_Red.TabIndex = 5;
            this.btn_ViewCurrentMax_Red.Text = "红色显示";
            this.btn_ViewCurrentMax_Red.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.btn_ViewSumMax_All);
            this.groupBox6.Controls.Add(this.btn_ViewSumMax_Red);
            this.groupBox6.Location = new System.Drawing.Point(440, 590);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(122, 79);
            this.groupBox6.TabIndex = 26;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "累计最大变量查看";
            // 
            // btn_ViewSumMax_All
            // 
            this.btn_ViewSumMax_All.Location = new System.Drawing.Point(66, 17);
            this.btn_ViewSumMax_All.Name = "btn_ViewSumMax_All";
            this.btn_ViewSumMax_All.Size = new System.Drawing.Size(44, 55);
            this.btn_ViewSumMax_All.TabIndex = 6;
            this.btn_ViewSumMax_All.Text = "整体查看";
            this.btn_ViewSumMax_All.UseVisualStyleBackColor = true;
            // 
            // btn_ViewSumMax_Red
            // 
            this.btn_ViewSumMax_Red.Location = new System.Drawing.Point(16, 17);
            this.btn_ViewSumMax_Red.Name = "btn_ViewSumMax_Red";
            this.btn_ViewSumMax_Red.Size = new System.Drawing.Size(44, 55);
            this.btn_ViewSumMax_Red.TabIndex = 5;
            this.btn_ViewSumMax_Red.Text = "红色显示";
            this.btn_ViewSumMax_Red.UseVisualStyleBackColor = true;
            // 
            // btn_Submit
            // 
            this.btn_Submit.Location = new System.Drawing.Point(988, 633);
            this.btn_Submit.Name = "btn_Submit";
            this.btn_Submit.Size = new System.Drawing.Size(67, 38);
            this.btn_Submit.TabIndex = 33;
            this.btn_Submit.Text = "保存";
            this.btn_Submit.UseVisualStyleBackColor = true;
            this.btn_Submit.Click += new System.EventHandler(this.btn_Submit_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(1061, 633);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(67, 38);
            this.btn_Cancel.TabIndex = 34;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 562);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(137, 12);
            this.label13.TabIndex = 35;
            this.label13.Text = "注:+为向坑内,-为向坑外";
            // 
            // btn_Delete
            // 
            this.btn_Delete.Location = new System.Drawing.Point(1061, 580);
            this.btn_Delete.Name = "btn_Delete";
            this.btn_Delete.Size = new System.Drawing.Size(67, 38);
            this.btn_Delete.TabIndex = 36;
            this.btn_Delete.Text = "删除";
            this.btn_Delete.UseVisualStyleBackColor = true;
            this.btn_Delete.Click += new System.EventHandler(this.btn_Delete_Click);
            // 
            // btn_Next
            // 
            this.btn_Next.Location = new System.Drawing.Point(872, 637);
            this.btn_Next.Name = "btn_Next";
            this.btn_Next.Size = new System.Drawing.Size(80, 28);
            this.btn_Next.TabIndex = 37;
            this.btn_Next.Text = "下一份";
            this.btn_Next.UseVisualStyleBackColor = true;
            this.btn_Next.Click += new System.EventHandler(this.btn_Next_Click);
            // 
            // btn_Previous
            // 
            this.btn_Previous.Location = new System.Drawing.Point(872, 598);
            this.btn_Previous.Name = "btn_Previous";
            this.btn_Previous.Size = new System.Drawing.Size(80, 29);
            this.btn_Previous.TabIndex = 38;
            this.btn_Previous.Text = "上一份";
            this.btn_Previous.UseVisualStyleBackColor = true;
            this.btn_Previous.Click += new System.EventHandler(this.btn_Previous_Click);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.btn_ViewOverWarn);
            this.groupBox7.Controls.Add(this.btn_OverWarnSettings);
            this.groupBox7.Controls.Add(this.label12);
            this.groupBox7.Controls.Add(this.btn_ViewCloseWarn);
            this.groupBox7.Controls.Add(this.btn_CloseWarnSettings);
            this.groupBox7.Controls.Add(this.label11);
            this.groupBox7.Location = new System.Drawing.Point(568, 590);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(272, 79);
            this.groupBox7.TabIndex = 24;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "预警编辑/查看";
            // 
            // btn_ViewOverWarn
            // 
            this.btn_ViewOverWarn.Location = new System.Drawing.Point(172, 46);
            this.btn_ViewOverWarn.Name = "btn_ViewOverWarn";
            this.btn_ViewOverWarn.Size = new System.Drawing.Size(85, 23);
            this.btn_ViewOverWarn.TabIndex = 38;
            this.btn_ViewOverWarn.Text = "接近预警预览";
            this.btn_ViewOverWarn.UseVisualStyleBackColor = true;
            // 
            // btn_OverWarnSettings
            // 
            this.btn_OverWarnSettings.Location = new System.Drawing.Point(87, 46);
            this.btn_OverWarnSettings.Name = "btn_OverWarnSettings";
            this.btn_OverWarnSettings.Size = new System.Drawing.Size(79, 23);
            this.btn_OverWarnSettings.TabIndex = 37;
            this.btn_OverWarnSettings.Text = "颜色/透明度";
            this.btn_OverWarnSettings.UseVisualStyleBackColor = true;
            this.btn_OverWarnSettings.Click += new System.EventHandler(this.btn_OverWarn_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(10, 51);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(71, 12);
            this.label12.TabIndex = 36;
            this.label12.Text = "超出预警值:";
            // 
            // btn_ViewCloseWarn
            // 
            this.btn_ViewCloseWarn.Location = new System.Drawing.Point(172, 20);
            this.btn_ViewCloseWarn.Name = "btn_ViewCloseWarn";
            this.btn_ViewCloseWarn.Size = new System.Drawing.Size(85, 23);
            this.btn_ViewCloseWarn.TabIndex = 35;
            this.btn_ViewCloseWarn.Text = "接近预警预览";
            this.btn_ViewCloseWarn.UseVisualStyleBackColor = true;
            // 
            // btn_CloseWarnSettings
            // 
            this.btn_CloseWarnSettings.Location = new System.Drawing.Point(87, 20);
            this.btn_CloseWarnSettings.Name = "btn_CloseWarnSettings";
            this.btn_CloseWarnSettings.Size = new System.Drawing.Size(79, 23);
            this.btn_CloseWarnSettings.TabIndex = 34;
            this.btn_CloseWarnSettings.Text = "颜色/透明度";
            this.btn_CloseWarnSettings.UseVisualStyleBackColor = true;
            this.btn_CloseWarnSettings.Click += new System.EventHandler(this.btn_CloseWarn_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(10, 25);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(71, 12);
            this.label11.TabIndex = 33;
            this.label11.Text = "接近预警值:";
            // 
            // tb_IssueType
            // 
            this.tb_IssueType.Location = new System.Drawing.Point(727, 10);
            this.tb_IssueType.Name = "tb_IssueType";
            this.tb_IssueType.Size = new System.Drawing.Size(135, 21);
            this.tb_IssueType.TabIndex = 39;
            // 
            // btn_CreateNew
            // 
            this.btn_CreateNew.Location = new System.Drawing.Point(988, 580);
            this.btn_CreateNew.Name = "btn_CreateNew";
            this.btn_CreateNew.Size = new System.Drawing.Size(67, 38);
            this.btn_CreateNew.TabIndex = 40;
            this.btn_CreateNew.Text = "新建";
            this.btn_CreateNew.UseVisualStyleBackColor = true;
            this.btn_CreateNew.Click += new System.EventHandler(this.btn_Create_Click);
            // 
            // SubsidenceMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1164, 681);
            this.Controls.Add(this.btn_CreateNew);
            this.Controls.Add(this.tb_IssueType);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.btn_Previous);
            this.Controls.Add(this.btn_Next);
            this.Controls.Add(this.btn_Delete);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Submit);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tb_WarnArgs);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_ReportName);
            this.Controls.Add(this.label1);
            this.Name = "SubsidenceMonitorForm";
            this.Text = "沉降监测";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SubsidenceMonitorForm_FormClosing);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_left)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_right)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_ReportName;
        private System.Windows.Forms.Label label2;
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
        private System.Windows.Forms.TextBox tb_Monitor;
        private System.Windows.Forms.TextBox tb_Supervisor;
        private System.Windows.Forms.TextBox tb_Contractor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btn_AddComponent;
        private System.Windows.Forms.Button btn_LoadExcel;
        private System.Windows.Forms.Button btn_RenderComponent;
        private System.Windows.Forms.Button btn_DeleteComponent;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btn_ViewAll;
        private System.Windows.Forms.Button btn_ViewSelection;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btn_ViewCurrentMax_All;
        private System.Windows.Forms.Button btn_ViewCurrentMax_Red;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button btn_ViewSumMax_All;
        private System.Windows.Forms.Button btn_ViewSumMax_Red;
        private System.Windows.Forms.Button btn_Submit;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btn_Delete;
        private System.Windows.Forms.Button btn_Next;
        private System.Windows.Forms.Button btn_Previous;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button btn_ViewOverWarn;
        private System.Windows.Forms.Button btn_OverWarnSettings;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btn_ViewCloseWarn;
        private System.Windows.Forms.Button btn_CloseWarnSettings;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tb_IssueType;
        private System.Windows.Forms.Button btn_CreateNew;
        private MyDGV0427 dgv_left;
        private MyDGV0427 dgv_right;
    }
}