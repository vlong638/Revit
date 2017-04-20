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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.btn_Preview = new System.Windows.Forms.Button();
            this.btn_Save = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.btn_Commit = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Blocks)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
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
            this.dgv_Blocks.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_Blocks_CellClick);
            this.dgv_Blocks.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_Blocks_CellDoubleClick);
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
            this.Node_Description.Width = 300;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridView1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(575, 326);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "实际施工节点信息";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(575, 330);
            this.dataGridView1.TabIndex = 0;
            // 
            // btn_Preview
            // 
            this.btn_Preview.Location = new System.Drawing.Point(317, 370);
            this.btn_Preview.Name = "btn_Preview";
            this.btn_Preview.Size = new System.Drawing.Size(66, 23);
            this.btn_Preview.TabIndex = 1;
            this.btn_Preview.Text = "预览";
            this.btn_Preview.UseVisualStyleBackColor = true;
            this.btn_Preview.Click += new System.EventHandler(this.btn_Preview_Click);
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(533, 370);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(66, 23);
            this.btn_Save.TabIndex = 2;
            this.btn_Save.Text = "应用";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Location = new System.Drawing.Point(461, 370);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(66, 23);
            this.btn_cancel.TabIndex = 3;
            this.btn_cancel.Text = "取消";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_Commit
            // 
            this.btn_Commit.Location = new System.Drawing.Point(389, 370);
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
            // EarthworkBlockingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 399);
            this.Controls.Add(this.btn_Commit);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.btn_Preview);
            this.Controls.Add(this.tabControl1);
            this.Name = "EarthworkBlockingForm";
            this.Text = "土方";
            this.Load += new System.EventHandler(this.EarthworkBlockingForm_Load);
            this.Shown += new System.EventHandler(this.EarthworkBlockingForm_Shown);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Blocks)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btn_Preview;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Button btn_cancel;
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
        private System.Windows.Forms.DataGridViewTextBoxColumn Node_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn Node_Description;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}