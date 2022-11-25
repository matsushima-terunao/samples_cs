namespace ArchiveTestCS
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
			this.buttonDelete = new System.Windows.Forms.Button();
			this.buttonOpenArchive = new System.Windows.Forms.Button();
			this.buttonCompressFolder = new System.Windows.Forms.Button();
			this.buttonExtractAll = new System.Windows.Forms.Button();
			this.buttonExtract = new System.Windows.Forms.Button();
			this.listView1 = new System.Windows.Forms.ListView();
			this.buttonClose = new System.Windows.Forms.Button();
			this.buttonCompressAll = new System.Windows.Forms.Button();
			this.buttonCompress = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonDelete
			// 
			this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonDelete.Location = new System.Drawing.Point(638, 173);
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.Size = new System.Drawing.Size(150, 23);
			this.buttonDelete.TabIndex = 16;
			this.buttonDelete.Text = "削除";
			this.buttonDelete.UseVisualStyleBackColor = true;
			// 
			// buttonOpenArchive
			// 
			this.buttonOpenArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOpenArchive.Location = new System.Drawing.Point(638, 12);
			this.buttonOpenArchive.Name = "buttonOpenArchive";
			this.buttonOpenArchive.Size = new System.Drawing.Size(150, 23);
			this.buttonOpenArchive.TabIndex = 15;
			this.buttonOpenArchive.Text = "アーカイブを開く";
			this.buttonOpenArchive.UseVisualStyleBackColor = true;
			// 
			// buttonCompressFolder
			// 
			this.buttonCompressFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCompressFolder.Location = new System.Drawing.Point(638, 70);
			this.buttonCompressFolder.Name = "buttonCompressFolder";
			this.buttonCompressFolder.Size = new System.Drawing.Size(150, 23);
			this.buttonCompressFolder.TabIndex = 14;
			this.buttonCompressFolder.Text = "フォルダーを圧縮";
			this.buttonCompressFolder.UseVisualStyleBackColor = true;
			// 
			// buttonExtractAll
			// 
			this.buttonExtractAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonExtractAll.Location = new System.Drawing.Point(638, 115);
			this.buttonExtractAll.Name = "buttonExtractAll";
			this.buttonExtractAll.Size = new System.Drawing.Size(150, 23);
			this.buttonExtractAll.TabIndex = 13;
			this.buttonExtractAll.Text = "全て展開";
			this.buttonExtractAll.UseVisualStyleBackColor = true;
			// 
			// buttonExtract
			// 
			this.buttonExtract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonExtract.Location = new System.Drawing.Point(638, 144);
			this.buttonExtract.Name = "buttonExtract";
			this.buttonExtract.Size = new System.Drawing.Size(150, 23);
			this.buttonExtract.TabIndex = 12;
			this.buttonExtract.Text = "展開";
			this.buttonExtract.UseVisualStyleBackColor = true;
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.Location = new System.Drawing.Point(12, 12);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(620, 426);
			this.listView1.TabIndex = 11;
			this.listView1.UseCompatibleStateImageBehavior = false;
			// 
			// buttonClose
			// 
			this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonClose.Location = new System.Drawing.Point(638, 41);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size(150, 23);
			this.buttonClose.TabIndex = 18;
			this.buttonClose.Text = "閉じる";
			this.buttonClose.UseVisualStyleBackColor = true;
			// 
			// buttonCompressAll
			// 
			this.buttonCompressAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCompressAll.Location = new System.Drawing.Point(638, 202);
			this.buttonCompressAll.Name = "buttonCompressAll";
			this.buttonCompressAll.Size = new System.Drawing.Size(150, 23);
			this.buttonCompressAll.TabIndex = 19;
			this.buttonCompressAll.Text = "全て圧縮";
			this.buttonCompressAll.UseVisualStyleBackColor = true;
			// 
			// buttonCompress
			// 
			this.buttonCompress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCompress.Location = new System.Drawing.Point(638, 231);
			this.buttonCompress.Name = "buttonCompress";
			this.buttonCompress.Size = new System.Drawing.Size(150, 23);
			this.buttonCompress.TabIndex = 20;
			this.buttonCompress.Text = "圧縮";
			this.buttonCompress.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.buttonCompress);
			this.Controls.Add(this.buttonCompressAll);
			this.Controls.Add(this.buttonClose);
			this.Controls.Add(this.buttonDelete);
			this.Controls.Add(this.buttonOpenArchive);
			this.Controls.Add(this.buttonCompressFolder);
			this.Controls.Add(this.buttonExtractAll);
			this.Controls.Add(this.buttonExtract);
			this.Controls.Add(this.listView1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion
		private Button buttonDelete;
		private Button buttonOpenArchive;
		private Button buttonCompressFolder;
		private Button buttonExtractAll;
		private Button buttonExtract;
		private ListView listView1;
		private Button buttonClose;
		private Button buttonCompressAll;
		private Button buttonCompress;
	}
}