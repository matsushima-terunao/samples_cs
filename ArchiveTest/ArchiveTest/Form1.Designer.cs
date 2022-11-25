namespace ArchiveTest
{
	partial class Form1
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.listView1 = new System.Windows.Forms.ListView();
			this.buttonExtract = new System.Windows.Forms.Button();
			this.buttonExtractAll = new System.Windows.Forms.Button();
			this.buttonCompress = new System.Windows.Forms.Button();
			this.buttonOpenArchive = new System.Windows.Forms.Button();
			this.buttonDelete = new System.Windows.Forms.Button();
			this.buttonOpenFolder = new System.Windows.Forms.Button();
			this.buttonClose = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.Location = new System.Drawing.Point(12, 12);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(620, 426);
			this.listView1.TabIndex = 0;
			this.listView1.UseCompatibleStateImageBehavior = false;
			// 
			// buttonExtract
			// 
			this.buttonExtract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonExtract.Location = new System.Drawing.Point(638, 147);
			this.buttonExtract.Name = "buttonExtract";
			this.buttonExtract.Size = new System.Drawing.Size(150, 23);
			this.buttonExtract.TabIndex = 1;
			this.buttonExtract.Text = "フォルダに展開";
			this.buttonExtract.UseVisualStyleBackColor = true;
			// 
			// buttonExtractAll
			// 
			this.buttonExtractAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonExtractAll.Location = new System.Drawing.Point(638, 118);
			this.buttonExtractAll.Name = "buttonExtractAll";
			this.buttonExtractAll.Size = new System.Drawing.Size(150, 23);
			this.buttonExtractAll.TabIndex = 2;
			this.buttonExtractAll.Text = "全てフォルダに展開";
			this.buttonExtractAll.UseVisualStyleBackColor = true;
			// 
			// buttonCompress
			// 
			this.buttonCompress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCompress.Location = new System.Drawing.Point(638, 224);
			this.buttonCompress.Name = "buttonCompress";
			this.buttonCompress.Size = new System.Drawing.Size(150, 23);
			this.buttonCompress.TabIndex = 5;
			this.buttonCompress.Text = "圧縮";
			this.buttonCompress.UseVisualStyleBackColor = true;
			// 
			// buttonOpenArchive
			// 
			this.buttonOpenArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOpenArchive.Location = new System.Drawing.Point(638, 12);
			this.buttonOpenArchive.Name = "buttonOpenArchive";
			this.buttonOpenArchive.Size = new System.Drawing.Size(150, 23);
			this.buttonOpenArchive.TabIndex = 7;
			this.buttonOpenArchive.Text = "ZIP ファイルを開く";
			this.buttonOpenArchive.UseVisualStyleBackColor = true;
			// 
			// buttonDelete
			// 
			this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonDelete.Location = new System.Drawing.Point(638, 176);
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.Size = new System.Drawing.Size(150, 23);
			this.buttonDelete.TabIndex = 8;
			this.buttonDelete.Text = "削除";
			this.buttonDelete.UseVisualStyleBackColor = true;
			// 
			// buttonOpenFolder
			// 
			this.buttonOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOpenFolder.Location = new System.Drawing.Point(638, 41);
			this.buttonOpenFolder.Name = "buttonOpenFolder";
			this.buttonOpenFolder.Size = new System.Drawing.Size(150, 23);
			this.buttonOpenFolder.TabIndex = 9;
			this.buttonOpenFolder.Text = "圧縮するフォルダを選択";
			this.buttonOpenFolder.UseVisualStyleBackColor = true;
			// 
			// buttonClose
			// 
			this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonClose.Location = new System.Drawing.Point(638, 70);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size(150, 23);
			this.buttonClose.TabIndex = 10;
			this.buttonClose.Text = "閉じる";
			this.buttonClose.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.buttonClose);
			this.Controls.Add(this.buttonOpenFolder);
			this.Controls.Add(this.buttonDelete);
			this.Controls.Add(this.buttonOpenArchive);
			this.Controls.Add(this.buttonCompress);
			this.Controls.Add(this.buttonExtractAll);
			this.Controls.Add(this.buttonExtract);
			this.Controls.Add(this.listView1);
			this.Name = "Form1";
			this.Text = "ArchiveTest";
			this.ResumeLayout(false);

		}

		#endregion

		private ListView listView1;
		private Button buttonExtract;
		private Button buttonExtractAll;
		private Button buttonCompress;
		private Button buttonOpenArchive;
		private Button buttonDelete;
		private Button buttonOpenFolder;
		private Button buttonClose;
	}
}