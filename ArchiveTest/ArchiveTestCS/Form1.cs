using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System.Diagnostics;
using System.Text;

namespace ArchiveTestCS
{
	/// <summary>
	/// ZIP アーカイブサンプル
	/// 2022/11/15 matsushima
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>現在開いているアーカイブのパス</summary>
		private string openArchivePath = null!;
		/// <summary>ファイル選択ダイアログ</summary>
		private readonly OpenFileDialog openFileDialog = new() { Filter = "すべてのファイル (*.*)|*.*" };
		/// <summary>ファイル保存ダイアログ</summary>
		private readonly SaveFileDialog saveFileDialog = new() { Filter = "Zip ファイル (*.zip)|*.zip|Tar ファイル (*.tar)|*.tar|GZip ファイル (*.gz;*.gzip;*.tar.gz;*.tar.gzip;*.tgz)|*.gz;*.gzip;*.tar.gz;*.tar.gzip;*.tgz|BZip2 ファイル (*.bz2;*.bzip2;*.tar.bz2;*.tar.bzip2;*.tbz2)|*.bz2;*.bzip2;*.tar.bz2;*.tar.bzip2;*.tbz2|LZip ファイル (*.lz;*.tar.lz)|*.lz;*.tar.lz|XZ ファイル (*.xz;*.tar.xz)|*.xz;*.tar.xz" };
		/// <summary>フォルダー選択ダイアログ</summary>
		private readonly FolderBrowserDialog folderBrowserDialog = new();

		/// <summary>
		/// アーカイブと圧縮方式の対応
		/// </summary>
		struct ArchiveMapping
		{
			public ArchiveType ArchiveType;
			public CompressionType CompressionType;
		};
		/// <summary>
		/// アーカイブと圧縮方式の対応
		/// </summary>
		private static readonly ArchiveMapping[] archiveMappings =
		{
			new(),
			new() { ArchiveType = ArchiveType.Zip, CompressionType = CompressionType.Deflate },
			new() { ArchiveType = ArchiveType.Tar, CompressionType = CompressionType.None },
			new() { ArchiveType = ArchiveType.Tar, CompressionType = CompressionType.GZip },
			new() { ArchiveType = ArchiveType.Tar, CompressionType = CompressionType.BZip2 },
			new() { ArchiveType = ArchiveType.Tar, CompressionType = CompressionType.LZMA },
		};


		public Form1()
		{
			InitializeComponent();

			// ListView 初期化
			listView1.View = View.Details;
			listView1.Columns.Add("名前", 180, HorizontalAlignment.Left);
			listView1.Columns.Add("場所", 160, HorizontalAlignment.Left);
			listView1.Columns.Add("更新日時", 120, HorizontalAlignment.Left);
			listView1.Columns.Add("サイズ", 80, HorizontalAlignment.Right);
			listView1.Columns.Add("圧縮方式", 60, HorizontalAlignment.Left);
			listView1.SelectedIndexChanged += (object? sender, EventArgs e) =>
			{
				UpdateControls();
			};

			// コントロールの更新。
			UpdateControls();

			// ボタン
			// アーカイブを開く
			buttonOpenArchive.Click += (object? sender, EventArgs e) =>
			{
				if (DialogResult.OK == openFileDialog.ShowDialog())
				{
					// ListView にアーカイブのファイル一覧追加。
					GetArchiveEntries(openFileDialog.FileName);
				}
			};
			// フォルダーを圧縮
			buttonCompressFolder.Click += (object? sender, EventArgs e) =>
			{
				// フォルダーを圧縮。
				CompressFolder();
			};
			// 閉じる
			buttonClose.Click += (object? sender, EventArgs e) =>
			{
				listView1.Items.Clear();
				openArchivePath = null!;
				UpdateControls();
			};
			// すべて展開
			buttonExtractAll.Click += (object? sender, EventArgs e) =>
			{
				// すべて展開。
				ExtractArchive();
			};
			// 展開
			buttonExtract.Click += (object? sender, EventArgs e) =>
			{
				// 選択ファイルを展開。
				ExtractSelectedArchiveEntries();
			};
			// 削除
			buttonDelete.Click += (object? sender, EventArgs e) =>
			{
				if (null != openArchivePath)
				{
					if (DialogResult.Yes == MessageBox.Show("選択ファイルを削除してアーカイブを更新しますか?",
						Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					{
						// アーカイブから選択ファイルを削除。
						DeleteArchiveEntries();
					}
				}
				else
				{
					foreach (var item in listView1.SelectedItems)
					{
						listView1.Items.Remove((ListViewItem)item);
					}
				}
			};
			// すべて圧縮
			buttonCompressAll.Click += (object? sender, EventArgs e) =>
			{
				// 指定ファイルを圧縮。
				Compress(listView1.Items.Cast<ListViewItem>());
			};
			// 圧縮
			buttonCompress.Click += (object? sender, EventArgs e) =>
			{
				// 指定ファイルを圧縮。
				Compress(listView1.SelectedItems.Cast<ListViewItem>());
			};

			// Form ドラッグ&ドロップ
			DragDrop += (object? sender, DragEventArgs e) =>
			{
				// ファイル処理。
				string[] files = (string[])e.Data?.GetData(DataFormats.FileDrop, false)!;
				ProcessFiles(files);
			};
			DragEnter += (object? sender, DragEventArgs e) =>
			{
				e.Effect = ((e.Data?.GetDataPresent(DataFormats.FileDrop)) ?? false) ? DragDropEffects.All : DragDropEffects.None;
			};
			AllowDrop = true;

			// コマンドライン引数
			string[] files = System.Environment.GetCommandLineArgs();
			if (files.Length >= 2)
			{
				// ファイル処理。
				ProcessFiles(files[1..]);
			}
		}

		/// <summary>
		/// コントロールの更新。
		/// </summary>
		private void UpdateControls()
		{
			Text = (null != openArchivePath) ? "ArchiveTestCS - " + openArchivePath : "ArchiveTestCS";
			buttonClose.Enabled = (null != openArchivePath);
			buttonExtractAll.Enabled = (null != openArchivePath);
			buttonExtract.Enabled = (null != openArchivePath && listView1.SelectedIndices.Count >= 1);
			buttonDelete.Enabled = (listView1.SelectedIndices.Count >= 1);
			buttonCompressAll.Enabled = (null == openArchivePath && listView1.Items.Count >= 1);
			buttonCompress.Enabled = (null == openArchivePath && listView1.SelectedItems.Count >= 1);
		}

		/// <summary>
		/// ファイル処理。
		/// </summary>
		/// <param name="files"></param>
		private void ProcessFiles(string[] files)
		{
			DialogResult addFiles = DialogResult.Yes; // yes:追加 no:新規 cancel:キャンセル
			// アーカイブを開く
			if (1 == files.Length && File.Exists(files[0]))
			{
				try
				{
					// アーカイブか判定するために開く
					using (var archive = ArchiveFactory.Open(files[0]))
					{
					}
					addFiles = MessageBox.Show("ファイルを追加しますか?\nまたはアーカイブを開きますか?",
						Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (DialogResult.No == addFiles)
					{
						// ListView にアーカイブのファイル一覧追加。
						GetArchiveEntries(files[0]);
					}
				}
				catch (Exception)
				{
					// アーカイブでない
					addFiles = DialogResult.Yes;
					Debug.WriteLine("not an archive");
				}
			}
			// ファイルを追加
			if (DialogResult.Yes == addFiles)
			{
				if (null != openArchivePath)
				{
					addFiles = MessageBox.Show("ファイルを追加してアーカイブを更新しますか?",
						Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (DialogResult.Yes == addFiles)
					{
						// アーカイブにファイルを追加。
						AddArchiveEntries(files);
					}
				}
				else
				{
					// ListView にファイル一覧追加。
					AddFolderFiles(files);
				}
			}
		}

		/// <summary>
		/// ListView にアーカイブのファイル一覧追加。
		/// </summary>
		/// <param name="path"></param>
		private void GetArchiveEntries(string path)
		{
			openArchivePath = path;
			// ListViewItem 追加
			listView1.Items.Clear();
			using (var stream = File.OpenRead(path))
			using (var reader = ReaderFactory.Open(stream))
			{
				while (reader.MoveToNextEntry())
				{
					if (!reader.Entry.IsDirectory)
					{
						var item = new ListViewItem(Path.GetFileName(reader.Entry.Key ?? Path.GetFileNameWithoutExtension(path)));
						item.SubItems.Add(Path.GetDirectoryName(reader.Entry.Key));
						item.SubItems.Add(reader.Entry.LastModifiedTime?.ToString("yyyy/MM/dd HH:mm:ss"));
						item.SubItems.Add(reader.Entry.Size.ToString("#,0"));
						item.SubItems.Add(reader.Entry.CompressionType.ToString());
						item.Tag = reader.Entry;
						listView1.Items.Add(item);
					}
				}
			}
			UpdateControls();
		}

		/// <summary>
		/// ListView にファイル一覧追加。
		/// </summary>
		/// <param name="paths"></param>
		private void AddFolderFiles(string[] paths)
		{
			// ListViewItem 追加
			foreach (string path in paths)
			{
				if (Directory.Exists(path))
				{
					string basePath = Path.GetDirectoryName(path) ?? "";
					foreach (var file in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
					{
						// 選択フォルダーを起点とした相対パスでエントリーを作成
						var item = new ListViewItem(file.Name);
						item.SubItems.Add(Path.GetRelativePath(basePath, file.DirectoryName!));
						item.SubItems.Add(file.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
						item.SubItems.Add(file.Length.ToString("#,0"));
						item.Tag = file;
						listView1.Items.Add(item);
					}
				}
				else
				{
					var file = new FileInfo(path);
					var item = new ListViewItem(file.Name);
					item.SubItems.Add(file.Name);
					item.SubItems.Add(file.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
					item.SubItems.Add(file.Length.ToString("#,0"));
					item.Tag = file;
					listView1.Items.Add(item);
				}
			}
			UpdateControls();
		}

		/// <summary>
		/// すべて展開。
		/// </summary>
		private void ExtractArchive()
		{
			if (DialogResult.OK == folderBrowserDialog.ShowDialog())
			{
				DateTime dt = DateTime.Now;
				using (var stream = File.OpenRead(openArchivePath))
				using (var reader = ReaderFactory.Open(stream))
				{
					reader.WriteAllToDirectory(folderBrowserDialog.SelectedPath,
						new ExtractionOptions() { Overwrite = true, ExtractFullPath = true, PreserveFileTime = true, PreserveAttributes = false });
				}
				Debug.WriteLine("WriteAllToDirectory " + (DateTime.Now - dt).TotalSeconds);
			}
		}

		/// <summary>
		/// 選択ファイルを展開。
		/// </summary>
		private void ExtractSelectedArchiveEntries()
		{
			if (DialogResult.OK == folderBrowserDialog.ShowDialog())
			{
				DateTime dt = DateTime.Now;
				using (var stream = File.OpenRead(openArchivePath))
				using (var reader = ReaderFactory.Open(stream))
				{
					while (reader.MoveToNextEntry())
					{
						foreach (ListViewItem item in listView1.SelectedItems)
						{
							if (reader.Entry.Key.Equals(((IEntry)item.Tag).Key))
							{
								reader.WriteEntryToDirectory(folderBrowserDialog.SelectedPath,
									new ExtractionOptions() { Overwrite = true, ExtractFullPath = true, PreserveFileTime = true, PreserveAttributes = false });
								break;
							}
						}
					}
				}
				Debug.WriteLine("WriteEntryToDirectory " + (DateTime.Now - dt).TotalSeconds);
			}
		}

		/// <summary>
		/// フォルダーを圧縮。
		/// </summary>
		private void CompressFolder()
		{
			if (DialogResult.OK == folderBrowserDialog.ShowDialog())
			{
				saveFileDialog.FileName = Path.GetFileName(folderBrowserDialog.SelectedPath) + ".zip";
				if (0 == saveFileDialog.InitialDirectory.Length)
				{
					saveFileDialog.InitialDirectory = Path.GetDirectoryName(folderBrowserDialog.SelectedPath);
				}
				if (DialogResult.OK == saveFileDialog.ShowDialog())
				{
					DateTime dt = DateTime.Now;
					var mapping = archiveMappings[saveFileDialog.FilterIndex];
					using (var stream = File.Create(saveFileDialog.FileName))
					using (var writer = WriterFactory.Open(stream, mapping.ArchiveType,
						new(mapping.CompressionType) { LeaveStreamOpen = true,
							ArchiveEncoding = new() { Default = Encoding.UTF8 } })) // ファイル名の文字化け対応
					{
						writer.WriteAll(folderBrowserDialog.SelectedPath, "*", SearchOption.AllDirectories);
					}
					Debug.WriteLine("WriteAll " + mapping.ArchiveType + " " + mapping.CompressionType + " " + (DateTime.Now - dt).TotalSeconds);
					// ListView にアーカイブのファイル一覧追加。
					GetArchiveEntries(saveFileDialog.FileName);
				}
			}
		}

		/// <summary>
		/// 指定ファイルを圧縮。
		/// </summary>
		/// <param name="items"></param>
		private void Compress(IEnumerable<ListViewItem> items)
		{
			if (DialogResult.OK == saveFileDialog.ShowDialog())
			{
				DateTime dt = DateTime.Now;
				var mapping = archiveMappings[saveFileDialog.FilterIndex];
				using (var stream = File.Create(saveFileDialog.FileName))
				using (var writer = WriterFactory.Open(stream, mapping.ArchiveType,
					new(mapping.CompressionType) { LeaveStreamOpen = true,
						ArchiveEncoding = new() { Default = Encoding.UTF8 } })) // ファイル名の文字化け対応
				{
					foreach (ListViewItem item in items)
					{
						writer.Write(Path.Combine(item.SubItems[1].Text, item.Text), (FileInfo)item.Tag);
					}
				}
				Debug.WriteLine("Write " + mapping.ArchiveType + " " + mapping.CompressionType + " " + (DateTime.Now - dt).TotalSeconds);
				// ListView にアーカイブのファイル一覧追加。
				GetArchiveEntries(saveFileDialog.FileName);
			}
		}

		/// <summary>
		/// 追加・削除用にアーカイブを開く。
		/// </summary>
		/// <param name="func"></param>
		private void OpenArchiveForModify(Action<IWritableArchive> func)
		{
			using (var archive = (IWritableArchive)ArchiveFactory.Open(openArchivePath))
			{
				// tar が1つだけ含まれているか判定
				if (ArchiveType.Tar == archive.Type && 1 == archive.Entries.Count() && null == archive.Entries.First().Key)
				{
					// tar を一時ファイルとして展開
					string tarPath = openArchivePath + ".tar.tmp";
					archive.Entries.First().WriteToFile(tarPath,
						new ExtractionOptions() { Overwrite = true, ExtractFullPath = true, PreserveFileTime = true, PreserveAttributes = false });
					var mapping = archiveMappings.Where(m => m.CompressionType == archive.Entries.First().CompressionType).First();
					archive.Dispose();

					// tar を開く
					using (var tarArchive = (IWritableArchive)ArchiveFactory.Open(tarPath))
					{
						func(tarArchive);
						// tar.xxx に圧縮
						tarArchive.SaveTo(openArchivePath + ".tmp",
							new(mapping.CompressionType) { ArchiveEncoding = new() { Default = Encoding.UTF8 } }); // ファイル名の文字化け対応
					}
					File.Delete(tarPath);
				}
				else
				{
					// tar.xxx 以外
					func(archive);
					// 一時ファイルに書き出し
					var mapping = archiveMappings.Where(m => m.ArchiveType == archive.Type).First();
					archive.SaveTo(openArchivePath + ".tmp",
						new(mapping.CompressionType) { ArchiveEncoding = new() { Default = Encoding.UTF8 } }); // ファイル名の文字化け対応
				}
			}
			File.Move(openArchivePath + ".tmp", openArchivePath, true);
			// ListView にアーカイブのファイル一覧追加。
			GetArchiveEntries(openArchivePath);
		}

		/// <summary>
		/// アーカイブにファイルを追加。
		/// </summary>
		/// <param name="paths"></param>
		private void AddArchiveEntries(string[] paths)
		{
			// 追加・削除用にアーカイブを開く。
			OpenArchiveForModify(archive =>
			{
				foreach (string path in paths)
				{
					if (Directory.Exists(path))
					{
						string basePath = Path.GetDirectoryName(path) ?? "";
						foreach (var file in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
						{
							// 選択フォルダーを起点とした相対パスでエントリーを追加
							archive.AddEntry(Path.GetRelativePath(basePath, file.FullName), file);
						}
					}
					else
					{
						var file = new FileInfo(path);
						archive.AddEntry(file.Name, file);
					}
				}
			});
		}

		/// <summary>
		/// アーカイブから選択ファイルを削除。
		/// </summary>
		private void DeleteArchiveEntries()
		{
			// 追加・削除用にアーカイブを開く。
			OpenArchiveForModify(archive =>
			{
				foreach (ListViewItem item in listView1.SelectedItems)
				{
					foreach (var entry in archive.Entries)
					{
						if (entry.Key.Equals(((IEntry)item.Tag).Key))
						{
							// 選択ファイルと一致するエントリーを削除
							archive.RemoveEntry(entry);
							Debug.WriteLine(entry);
							break;
						}
					}
				}
			});
		}
	}
}