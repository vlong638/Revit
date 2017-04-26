namespace MyRevit.EarthWork.UI
{
    partial class EarthworkBlockingForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EarthworkBlockingForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btn_DownNode = new System.Windows.Forms.Button();
            this.btn_UpNode = new System.Windows.Forms.Button();
            this.btn_DeleteNode = new System.Windows.Forms.Button();
            this.btn_AddNode = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btn_DeleteElement = new System.Windows.Forms.Button();
            this.btn_AddElement = new System.Windows.Forms.Button();
            this.btn_CPSettings = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_CombineAfter = new System.Windows.Forms.Button();
            this.btn_CombineBefore = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_InsertAfter = new System.Windows.Forms.Button();
            this.btn_InsertBefore = new System.Windows.Forms.Button();
            this.dgv_Blocks = new System.Windows.Forms.DataGridView();
            this.Node_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Node_Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgv_ImplementationInfo = new System.Windows.Forms.DataGridView();
            this.ConstructionNode_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConstructionNode_StartTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConstructionNode_ExposureTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConstructionNode_EndTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btn_Preview = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_Commit = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.lable_OrderByTime = new System.Windows.Forms.Label();
            this.btn_SortByDate = new System.Windows.Forms.Button();
            this.lbl_BlockingColor = new System.Windows.Forms.Label();
            this.lbl_Completed = new System.Windows.Forms.Label();
            this.btn_Completed = new System.Windows.Forms.Button();
            this.lbl_Uncompleted = new System.Windows.Forms.Label();
            this.btn_Uncompleted = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.btn_ViewGt6 = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Blocks)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ImplementationInfo)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(583, 352);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox4);
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.dgv_Blocks);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(575, 326);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "土方分块";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btn_DownNode);
            this.groupBox4.Controls.Add(this.btn_UpNode);
            this.groupBox4.Controls.Add(this.btn_DeleteNode);
            this.groupBox4.Controls.Add(this.btn_AddNode);
            this.groupBox4.Location = new System.Drawing.Point(449, 276);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(123, 49);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "修改节点";
            // 
            // btn_DownNode
            // 
            this.btn_DownNode.Image = ((System.Drawing.Image)(resources.GetObject("btn_DownNode.Image")));
            this.btn_DownNode.Location = new System.Drawing.Point(90, 17);
            this.btn_DownNode.Name = "btn_DownNode";
            this.btn_DownNode.Size = new System.Drawing.Size(28, 28);
            this.btn_DownNode.TabIndex = 5;
            this.btn_DownNode.UseVisualStyleBackColor = true;
            this.btn_DownNode.Click += new System.EventHandler(this.btn_DownNode_Click);
            // 
            // btn_UpNode
            // 
            this.btn_UpNode.Image = ((System.Drawing.Image)(resources.GetObject("btn_UpNode.Image")));
            this.btn_UpNode.Location = new System.Drawing.Point(62, 17);
            this.btn_UpNode.Name = "btn_UpNode";
            this.btn_UpNode.Size = new System.Drawing.Size(28, 28);
            this.btn_UpNode.TabIndex = 4;
            this.btn_UpNode.UseVisualStyleBackColor = true;
            this.btn_UpNode.Click += new System.EventHandler(this.btn_UpNode_Click);
            // 
            // btn_DeleteNode
            // 
            this.btn_DeleteNode.Image = ((System.Drawing.Image)(resources.GetObject("btn_DeleteNode.Image")));
            this.btn_DeleteNode.Location = new System.Drawing.Point(34, 17);
            this.btn_DeleteNode.Name = "btn_DeleteNode";
            this.btn_DeleteNode.Size = new System.Drawing.Size(28, 28);
            this.btn_DeleteNode.TabIndex = 3;
            this.btn_DeleteNode.UseVisualStyleBackColor = true;
            this.btn_DeleteNode.Click += new System.EventHandler(this.btn_DeleteNode_Click);
            // 
            // btn_AddNode
            // 
            this.btn_AddNode.Image = ((System.Drawing.Image)(resources.GetObject("btn_AddNode.Image")));
            this.btn_AddNode.Location = new System.Drawing.Point(6, 17);
            this.btn_AddNode.Name = "btn_AddNode";
            this.btn_AddNode.Size = new System.Drawing.Size(28, 28);
            this.btn_AddNode.TabIndex = 2;
            this.btn_AddNode.UseVisualStyleBackColor = true;
            this.btn_AddNode.Click += new System.EventHandler(this.btn_AddNode_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btn_DeleteElement);
            this.groupBox3.Controls.Add(this.btn_AddElement);
            this.groupBox3.Controls.Add(this.btn_CPSettings);
            this.groupBox3.Location = new System.Drawing.Point(449, 186);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(120, 84);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "构件设置";
            // 
            // btn_DeleteElement
            // 
            this.btn_DeleteElement.Image = ((System.Drawing.Image)(resources.GetObject("btn_DeleteElement.Image")));
            this.btn_DeleteElement.Location = new System.Drawing.Point(64, 17);
            this.btn_DeleteElement.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.btn_DeleteElement.Name = "btn_DeleteElement";
            this.btn_DeleteElement.Size = new System.Drawing.Size(35, 33);
            this.btn_DeleteElement.TabIndex = 3;
            this.btn_DeleteElement.UseVisualStyleBackColor = true;
            this.btn_DeleteElement.Click += new System.EventHandler(this.btn_DeleteElement_Click);
            // 
            // btn_AddElement
            // 
            this.btn_AddElement.Image = ((System.Drawing.Image)(resources.GetObject("btn_AddElement.Image")));
            this.btn_AddElement.Location = new System.Drawing.Point(23, 17);
            this.btn_AddElement.Name = "btn_AddElement";
            this.btn_AddElement.Size = new System.Drawing.Size(35, 33);
            this.btn_AddElement.TabIndex = 2;
            this.btn_AddElement.UseVisualStyleBackColor = true;
            this.btn_AddElement.Click += new System.EventHandler(this.btn_AddElement_Click);
            // 
            // btn_CPSettings
            // 
            this.btn_CPSettings.Location = new System.Drawing.Point(23, 52);
            this.btn_CPSettings.Name = "btn_CPSettings";
            this.btn_CPSettings.Size = new System.Drawing.Size(76, 23);
            this.btn_CPSettings.TabIndex = 1;
            this.btn_CPSettings.Text = "颜色/透明度";
            this.btn_CPSettings.UseVisualStyleBackColor = true;
            this.btn_CPSettings.Click += new System.EventHandler(this.btn_CPSettings_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btn_CombineAfter);
            this.groupBox2.Controls.Add(this.btn_CombineBefore);
            this.groupBox2.Location = new System.Drawing.Point(447, 96);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(120, 84);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "合并对象";
            // 
            // btn_CombineAfter
            // 
            this.btn_CombineAfter.Location = new System.Drawing.Point(7, 50);
            this.btn_CombineAfter.Name = "btn_CombineAfter";
            this.btn_CombineAfter.Size = new System.Drawing.Size(106, 23);
            this.btn_CombineAfter.TabIndex = 1;
            this.btn_CombineAfter.Text = "与下一个合并(N)";
            this.btn_CombineAfter.UseVisualStyleBackColor = true;
            this.btn_CombineAfter.Click += new System.EventHandler(this.btn_CombineAfter_Click);
            // 
            // btn_CombineBefore
            // 
            this.btn_CombineBefore.Location = new System.Drawing.Point(7, 20);
            this.btn_CombineBefore.Name = "btn_CombineBefore";
            this.btn_CombineBefore.Size = new System.Drawing.Size(106, 23);
            this.btn_CombineBefore.TabIndex = 0;
            this.btn_CombineBefore.Text = "与上一个合并(P)";
            this.btn_CombineBefore.UseVisualStyleBackColor = true;
            this.btn_CombineBefore.Click += new System.EventHandler(this.btn_CombineBefore_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_InsertAfter);
            this.groupBox1.Controls.Add(this.btn_InsertBefore);
            this.groupBox1.Location = new System.Drawing.Point(447, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(120, 84);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "插入";
            // 
            // btn_InsertAfter
            // 
            this.btn_InsertAfter.Location = new System.Drawing.Point(7, 50);
            this.btn_InsertAfter.Name = "btn_InsertAfter";
            this.btn_InsertAfter.Size = new System.Drawing.Size(106, 23);
            this.btn_InsertAfter.TabIndex = 1;
            this.btn_InsertAfter.Text = "在后面插入(F)";
            this.btn_InsertAfter.UseVisualStyleBackColor = true;
            this.btn_InsertAfter.Click += new System.EventHandler(this.btn_InsertAfter_Click);
            // 
            // btn_InsertBefore
            // 
            this.btn_InsertBefore.Location = new System.Drawing.Point(7, 20);
            this.btn_InsertBefore.Name = "btn_InsertBefore";
            this.btn_InsertBefore.Size = new System.Drawing.Size(106, 23);
            this.btn_InsertBefore.TabIndex = 0;
            this.btn_InsertBefore.Text = "在前面插入(B)";
            this.btn_InsertBefore.UseVisualStyleBackColor = true;
            this.btn_InsertBefore.Click += new System.EventHandler(this.btn_InsertBefore_Click);
            // 
            // dgv_Blocks
            // 
            this.dgv_Blocks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Blocks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Node_Name,
            this.Node_Description});
            this.dgv_Blocks.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgv_Blocks.Location = new System.Drawing.Point(-4, 0);
            this.dgv_Blocks.Name = "dgv_Blocks";
            this.dgv_Blocks.RowTemplate.Height = 23;
            this.dgv_Blocks.Size = new System.Drawing.Size(443, 325);
            this.dgv_Blocks.TabIndex = 0;
            this.dgv_Blocks.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dgv_Blocks_CellBeginEdit);
            this.dgv_Blocks.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_Blocks_CellClick);
            this.dgv_Blocks.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_Blocks_CellDoubleClick);
            this.dgv_Blocks.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_Blocks_CellEndEdit);
            this.dgv_Blocks.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgv_Blocks_RowPostPaint);
            // 
            // Node_Name
            // 
            this.Node_Name.HeaderText = "名称";
            this.Node_Name.Name = "Node_Name";
            // 
            // Node_Description
            // 
            this.Node_Description.HeaderText = "说明";
            this.Node_Description.Name = "Node_Description";
            this.Node_Description.Width = 280;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgv_ImplementationInfo);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(575, 326);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "实际施工节点信息管理";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgv_ImplementationInfo
            // 
            this.dgv_ImplementationInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_ImplementationInfo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ConstructionNode_Name,
            this.ConstructionNode_StartTime,
            this.ConstructionNode_ExposureTime,
            this.ConstructionNode_EndTime});
            this.dgv_ImplementationInfo.Location = new System.Drawing.Point(-4, 0);
            this.dgv_ImplementationInfo.Name = "dgv_ImplementationInfo";
            this.dgv_ImplementationInfo.RowTemplate.Height = 23;
            this.dgv_ImplementationInfo.Size = new System.Drawing.Size(579, 330);
            this.dgv_ImplementationInfo.TabIndex = 1;
            this.dgv_ImplementationInfo.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dgv_ImplementationInfo_CellBeginEdit);
            this.dgv_ImplementationInfo.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_ImplementationInfo_CellEndEdit);
            this.dgv_ImplementationInfo.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgv_ImplementationInfo_RowPostPaint);
            this.dgv_ImplementationInfo.SelectionChanged += new System.EventHandler(this.dgv_ImplementationInfo_SelectionChanged);
            // 
            // ConstructionNode_Name
            // 
            this.ConstructionNode_Name.HeaderText = "名称";
            this.ConstructionNode_Name.Name = "ConstructionNode_Name";
            // 
            // ConstructionNode_StartTime
            // 
            this.ConstructionNode_StartTime.HeaderText = "节点开始";
            this.ConstructionNode_StartTime.Name = "ConstructionNode_StartTime";
            this.ConstructionNode_StartTime.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ConstructionNode_StartTime.Width = 140;
            // 
            // ConstructionNode_ExposureTime
            // 
            this.ConstructionNode_ExposureTime.HeaderText = "无支撑暴露时间(h)";
            this.ConstructionNode_ExposureTime.Name = "ConstructionNode_ExposureTime";
            this.ConstructionNode_ExposureTime.Width = 130;
            // 
            // ConstructionNode_EndTime
            // 
            this.ConstructionNode_EndTime.HeaderText = "节点完成";
            this.ConstructionNode_EndTime.Name = "ConstructionNode_EndTime";
            this.ConstructionNode_EndTime.Width = 140;
            // 
            // btn_Preview
            // 
            this.btn_Preview.Location = new System.Drawing.Point(382, 372);
            this.btn_Preview.Name = "btn_Preview";
            this.btn_Preview.Size = new System.Drawing.Size(66, 23);
            this.btn_Preview.TabIndex = 1;
            this.btn_Preview.Text = "查看";
            this.btn_Preview.UseVisualStyleBackColor = true;
            this.btn_Preview.Click += new System.EventHandler(this.btn_Preview_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(526, 372);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(66, 23);
            this.btn_Cancel.TabIndex = 3;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_Commit
            // 
            this.btn_Commit.Location = new System.Drawing.Point(454, 372);
            this.btn_Commit.Name = "btn_Commit";
            this.btn_Commit.Size = new System.Drawing.Size(66, 23);
            this.btn_Commit.TabIndex = 4;
            this.btn_Commit.Text = "确定";
            this.btn_Commit.UseVisualStyleBackColor = true;
            this.btn_Commit.Click += new System.EventHandler(this.btn_Commit_Click);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // lable_OrderByTime
            // 
            this.lable_OrderByTime.AutoSize = true;
            this.lable_OrderByTime.Location = new System.Drawing.Point(444, 13);
            this.lable_OrderByTime.Name = "lable_OrderByTime";
            this.lable_OrderByTime.Size = new System.Drawing.Size(71, 12);
            this.lable_OrderByTime.TabIndex = 1;
            this.lable_OrderByTime.Text = "按时间查看:";
            // 
            // btn_SortByDate
            // 
            this.btn_SortByDate.Location = new System.Drawing.Point(517, 8);
            this.btn_SortByDate.Name = "btn_SortByDate";
            this.btn_SortByDate.Size = new System.Drawing.Size(75, 23);
            this.btn_SortByDate.TabIndex = 5;
            this.btn_SortByDate.UseVisualStyleBackColor = true;
            this.btn_SortByDate.TextChanged += new System.EventHandler(this.btn_SortByDate_TextChanged);
            this.btn_SortByDate.Click += new System.EventHandler(this.btn_SortByTime_Click);
            // 
            // lbl_BlockingColor
            // 
            this.lbl_BlockingColor.AutoSize = true;
            this.lbl_BlockingColor.Location = new System.Drawing.Point(10, 375);
            this.lbl_BlockingColor.Name = "lbl_BlockingColor";
            this.lbl_BlockingColor.Size = new System.Drawing.Size(59, 12);
            this.lbl_BlockingColor.TabIndex = 6;
            this.lbl_BlockingColor.Text = "分块颜色:";
            // 
            // lbl_Completed
            // 
            this.lbl_Completed.AutoSize = true;
            this.lbl_Completed.Location = new System.Drawing.Point(75, 375);
            this.lbl_Completed.Name = "lbl_Completed";
            this.lbl_Completed.Size = new System.Drawing.Size(41, 12);
            this.lbl_Completed.TabIndex = 7;
            this.lbl_Completed.Text = "已完工";
            // 
            // btn_Completed
            // 
            this.btn_Completed.Location = new System.Drawing.Point(122, 372);
            this.btn_Completed.Name = "btn_Completed";
            this.btn_Completed.Size = new System.Drawing.Size(18, 18);
            this.btn_Completed.TabIndex = 8;
            this.btn_Completed.UseVisualStyleBackColor = true;
            this.btn_Completed.Click += new System.EventHandler(this.btn_Completed_Click);
            // 
            // lbl_Uncompleted
            // 
            this.lbl_Uncompleted.AutoSize = true;
            this.lbl_Uncompleted.Location = new System.Drawing.Point(146, 375);
            this.lbl_Uncompleted.Name = "lbl_Uncompleted";
            this.lbl_Uncompleted.Size = new System.Drawing.Size(41, 12);
            this.lbl_Uncompleted.TabIndex = 9;
            this.lbl_Uncompleted.Text = "未完工";
            // 
            // btn_Uncompleted
            // 
            this.btn_Uncompleted.Location = new System.Drawing.Point(193, 372);
            this.btn_Uncompleted.Name = "btn_Uncompleted";
            this.btn_Uncompleted.Size = new System.Drawing.Size(18, 18);
            this.btn_Uncompleted.TabIndex = 11;
            this.btn_Uncompleted.UseVisualStyleBackColor = true;
            this.btn_Uncompleted.Click += new System.EventHandler(this.btn_Uncompleted_Click);
            // 
            // btn_ViewGt6
            // 
            this.btn_ViewGt6.Location = new System.Drawing.Point(310, 372);
            this.btn_ViewGt6.Name = "btn_ViewGt6";
            this.btn_ViewGt6.Size = new System.Drawing.Size(66, 23);
            this.btn_ViewGt6.TabIndex = 12;
            this.btn_ViewGt6.Text = "查看(>6)";
            this.btn_ViewGt6.UseVisualStyleBackColor = true;
            this.btn_ViewGt6.Click += new System.EventHandler(this.btn_ViewGt6_Click);
            // 
            // EarthworkBlockingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 399);
            this.Controls.Add(this.btn_ViewGt6);
            this.Controls.Add(this.btn_Uncompleted);
            this.Controls.Add(this.lbl_Uncompleted);
            this.Controls.Add(this.btn_Completed);
            this.Controls.Add(this.lbl_Completed);
            this.Controls.Add(this.lbl_BlockingColor);
            this.Controls.Add(this.btn_SortByDate);
            this.Controls.Add(this.lable_OrderByTime);
            this.Controls.Add(this.btn_Commit);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Preview);
            this.Controls.Add(this.tabControl1);
            this.Name = "EarthworkBlockingForm";
            this.Text = "土方";
            this.Load += new System.EventHandler(this.EarthworkBlockingForm_Load);
            this.Shown += new System.EventHandler(this.EarthworkBlockingForm_Shown);
            this.Click += new System.EventHandler(this.EarthworkBlockingForm_Click);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Blocks)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ImplementationInfo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btn_Preview;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_Commit;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_InsertBefore;
        private System.Windows.Forms.DataGridView dgv_Blocks;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btn_DeleteElement;
        private System.Windows.Forms.Button btn_AddElement;
        private System.Windows.Forms.Button btn_CPSettings;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btn_CombineAfter;
        private System.Windows.Forms.Button btn_CombineBefore;
        private System.Windows.Forms.Button btn_InsertAfter;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btn_AddNode;
        private System.Windows.Forms.Button btn_DownNode;
        private System.Windows.Forms.Button btn_UpNode;
        private System.Windows.Forms.Button btn_DeleteNode;
        private System.Windows.Forms.Label lable_OrderByTime;
        private System.Windows.Forms.Button btn_SortByDate;
        private System.Windows.Forms.Label lbl_BlockingColor;
        private System.Windows.Forms.Label lbl_Completed;
        private System.Windows.Forms.Button btn_Completed;
        private System.Windows.Forms.Label lbl_Uncompleted;
        private System.Windows.Forms.Button btn_Uncompleted;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.DataGridView dgv_ImplementationInfo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Node_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn Node_Description;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConstructionNode_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConstructionNode_StartTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConstructionNode_ExposureTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConstructionNode_EndTime;
        private System.Windows.Forms.Button btn_ViewGt6;
    }
}