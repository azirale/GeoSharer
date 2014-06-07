namespace GeoSharerWinForm
{
    partial class MainForm
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
            this.PanelA = new System.Windows.Forms.Panel();
            this.PanelB = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.OutputFolderLabel = new System.Windows.Forms.Label();
            this.OutputFolderText = new System.Windows.Forms.TextBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusSpringlabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusMessagelabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusProgressbar = new System.Windows.Forms.ToolStripProgressBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pickOutputDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addInputFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.processToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeToWorldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createTrimmedDatafileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PanelA.SuspendLayout();
            this.PanelB.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelA
            // 
            this.PanelA.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelA.Controls.Add(this.PanelB);
            this.PanelA.Location = new System.Drawing.Point(0, 27);
            this.PanelA.Name = "PanelA";
            this.PanelA.Size = new System.Drawing.Size(784, 510);
            this.PanelA.TabIndex = 0;
            // 
            // PanelB
            // 
            this.PanelB.ColumnCount = 2;
            this.PanelB.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46.89119F));
            this.PanelB.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 53.10881F));
            this.PanelB.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.PanelB.Controls.Add(this.richTextBox1, 1, 0);
            this.PanelB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelB.Location = new System.Drawing.Point(0, 0);
            this.PanelB.Name = "PanelB";
            this.PanelB.RowCount = 1;
            this.PanelB.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.PanelB.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 510F));
            this.PanelB.Size = new System.Drawing.Size(784, 510);
            this.PanelB.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.OutputFolderLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.OutputFolderText, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.listBox1, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(361, 504);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // OutputFolderLabel
            // 
            this.OutputFolderLabel.AutoSize = true;
            this.OutputFolderLabel.Location = new System.Drawing.Point(3, 0);
            this.OutputFolderLabel.Name = "OutputFolderLabel";
            this.OutputFolderLabel.Size = new System.Drawing.Size(71, 13);
            this.OutputFolderLabel.TabIndex = 1;
            this.OutputFolderLabel.Text = "Output Folder";
            // 
            // OutputFolderText
            // 
            this.OutputFolderText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OutputFolderText.Location = new System.Drawing.Point(3, 20);
            this.OutputFolderText.Name = "OutputFolderText";
            this.OutputFolderText.Size = new System.Drawing.Size(355, 20);
            this.OutputFolderText.TabIndex = 2;
            this.OutputFolderText.TextChanged += new System.EventHandler(this.OutputFolderText_TextChanged);
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.IntegralHeight = false;
            this.listBox1.Location = new System.Drawing.Point(3, 64);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(355, 437);
            this.listBox1.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "label2";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.HideSelection = false;
            this.richTextBox1.Location = new System.Drawing.Point(370, 3);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(411, 504);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusSpringlabel,
            this.StatusMessagelabel,
            this.StatusProgressbar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 540);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(784, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.Resize += new System.EventHandler(this.statusStrip1_Resize);
            // 
            // StatusSpringlabel
            // 
            this.StatusSpringlabel.Name = "StatusSpringlabel";
            this.StatusSpringlabel.Size = new System.Drawing.Size(328, 17);
            this.StatusSpringlabel.Spring = true;
            this.StatusSpringlabel.Text = " ";
            // 
            // StatusMessagelabel
            // 
            this.StatusMessagelabel.Name = "StatusMessagelabel";
            this.StatusMessagelabel.Size = new System.Drawing.Size(39, 17);
            this.StatusMessagelabel.Text = "Ready";
            this.StatusMessagelabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // StatusProgressbar
            // 
            this.StatusProgressbar.AutoSize = false;
            this.StatusProgressbar.Name = "StatusProgressbar";
            this.StatusProgressbar.Size = new System.Drawing.Size(400, 16);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.processToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(784, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pickOutputDirectoryToolStripMenuItem,
            this.addInputFilesToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // pickOutputDirectoryToolStripMenuItem
            // 
            this.pickOutputDirectoryToolStripMenuItem.Name = "pickOutputDirectoryToolStripMenuItem";
            this.pickOutputDirectoryToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.pickOutputDirectoryToolStripMenuItem.Text = "Pick Output Directory";
            this.pickOutputDirectoryToolStripMenuItem.Click += new System.EventHandler(this.pickOutputDirectoryToolStripMenuItem_Click);
            // 
            // addInputFilesToolStripMenuItem
            // 
            this.addInputFilesToolStripMenuItem.Name = "addInputFilesToolStripMenuItem";
            this.addInputFilesToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.addInputFilesToolStripMenuItem.Text = "Add Input File(s)";
            this.addInputFilesToolStripMenuItem.Click += new System.EventHandler(this.addInputFilesToolStripMenuItem_Click);
            // 
            // processToolStripMenuItem
            // 
            this.processToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mergeToWorldToolStripMenuItem,
            this.createTrimmedDatafileToolStripMenuItem});
            this.processToolStripMenuItem.Name = "processToolStripMenuItem";
            this.processToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.processToolStripMenuItem.Text = "Process";
            // 
            // mergeToWorldToolStripMenuItem
            // 
            this.mergeToWorldToolStripMenuItem.Name = "mergeToWorldToolStripMenuItem";
            this.mergeToWorldToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.mergeToWorldToolStripMenuItem.Text = "Merge/Create World Data";
            this.mergeToWorldToolStripMenuItem.Click += new System.EventHandler(this.mergeToWorldToolStripMenuItem_Click);
            // 
            // createTrimmedDatafileToolStripMenuItem
            // 
            this.createTrimmedDatafileToolStripMenuItem.Name = "createTrimmedDatafileToolStripMenuItem";
            this.createTrimmedDatafileToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.createTrimmedDatafileToolStripMenuItem.Text = "Create Trimmed Datafile";
            this.createTrimmedDatafileToolStripMenuItem.Click += new System.EventHandler(this.createTrimmedDatafileToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.PanelA);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "MainForm";
            this.Text = "GeoSharer Utility";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.PanelA.ResumeLayout(false);
            this.PanelB.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel PanelA;
        private System.Windows.Forms.TableLayoutPanel PanelB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label OutputFolderLabel;
        private System.Windows.Forms.TextBox OutputFolderText;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusSpringlabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusMessagelabel;
        private System.Windows.Forms.ToolStripProgressBar StatusProgressbar;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pickOutputDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addInputFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem processToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeToWorldToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createTrimmedDatafileToolStripMenuItem;
    }
}

