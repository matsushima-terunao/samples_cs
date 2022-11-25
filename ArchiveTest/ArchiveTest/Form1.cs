using System.Diagnostics;
using System.IO.Compression;
using FileIO = Microsoft.VisualBasic.FileIO;

namespace ArchiveTest
{
	/// <summary>
	/// ZIP アーカイブサンプル
	/// 2022/11/11 matsushima
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>現在開いているアーカイブのパス</summary>
		private string openArchivePath = null!;
		/// <summary>現在開いているフォルダーのパス</summary>
		private string openFolderPath = null!;
		/// <summary>ファイル選択ダイアログ</summary>
		private readonly OpenFileDialog openFileDialog = new() { Filter = "ZIP ファイル (*.zip)|*.zip|すべてのファイル (*.*)|*.*" };
		/// <summary>ファイル保存ダイアログ</summary>
		private readonly SaveFileDialog saveFileDialog = new() { Filter = "ZIP ファイル (*.zip)|*.zip|すべてのファイル (*.*)|*.*" };
		/// <summary>フォルダー選択ダイアログ</summary>
		private readonly FolderBrowserDialog folderBrowserDialog = new();

		public Form1()
		{
			InitializeComponent();

			// ListView 初期化
			listView1.View = View.Details;
			listView1.Columns.Add("名前", 200, HorizontalAlignment.Left);
			listView1.Columns.Add("場所", 200, HorizontalAlignment.Left);
			listView1.Columns.Add("更新日時", 120, HorizontalAlignment.Left);
			listView1.Columns.Add("サイズ", 80, HorizontalAlignment.Right);
			listView1.SelectedIndexChanged += (object? sender, EventArgs e) =>
			{
				UpdateControls();
			};

			// Form ドラッグ&ドロップ
			DragDrop += (object? sender, DragEventArgs e) =>
			{
				string[] files = (string[])e.Data?.GetData(DataFormats.FileDrop, false)!;
				DialogResult addFiles = DialogResult.No; // yes:追加 no:新規 cancel:キャンセル
				if (null != openArchivePath)
				{
					// アーカイブにファイルを追加。
					addFiles = MessageBox.Show("ファイルを追加して ZIP ファイルを更新しますか?\nまたは新規に開きますか?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (DialogResult.Yes == addFiles)
					{
						AddArchiveEntries(files);
					}
				}
				else if (null != openFolderPath)
				{
					// フォルダーにファイルをコピー。
					addFiles = MessageBox.Show("フォルダーにファイルをコピーしますか?\nまたは新規に開きますか?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (DialogResult.Yes == addFiles)
					{
						CopyFiles(files);
					}
				}
				if (DialogResult.No == addFiles)
				{
					if (1 == files.Length && ".zip" == Path.GetExtension(files[0]).ToLower() && File.Exists(files[0]))
					{
						// ListView にアーカイブのファイル一覧追加。
						openFolderPath = null!;
						GetArchiveEntries(files[0]);
					}
					else if (1 == files.Length && Directory.Exists(files[0]))
					{
						// ListView にファイル一覧追加。
						openArchivePath = null!;
						GetFolderFiles(files[0]);
					}
					else
					{
						MessageBox.Show("展開したい ZIP ファイルまたは圧縮したいフォルダーを1つだけドロップしてください。", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				}
			};
			DragEnter += (object? sender, DragEventArgs e) =>
			{
				e.Effect = ((e.Data?.GetDataPresent(DataFormats.FileDrop)) ?? false) ? DragDropEffects.All : DragDropEffects.None;
			};
			AllowDrop = true;

			// ListView にアーカイブのファイル一覧追加。
			buttonOpenArchive.Click += (object? sender, EventArgs e) =>
			{
				if (DialogResult.OK == openFileDialog.ShowDialog())
				{
					openFolderPath = null!;
					GetArchiveEntries(openFileDialog.FileName);
				}
			};
			// ListView にファイル一覧追加。
			buttonOpenFolder.Click += (object? sender, EventArgs e) =>
			{
				if (DialogResult.OK == folderBrowserDialog.ShowDialog())
				{
					openArchivePath = null!;
					GetFolderFiles(folderBrowserDialog.SelectedPath);
				}
			};
			// 閉じる
			buttonClose.Click += (object? sender, EventArgs e) =>
			{
				listView1.Items.Clear();
				openArchivePath = null!;
				openFolderPath = null!;
				UpdateControls();
			};
			// 全て展開。
			buttonExtractAll.Click += (object? sender, EventArgs e) =>
			{
				ExtractArchive();
			};
			// 選択ファイルを展開。
			buttonExtract.Click += (object? sender, EventArgs e) =>
			{
				ExtractSelectedArchiveEntries();
			};
			// アーカイブから選択ファイルを削除。
			buttonDelete.Click += (object? sender, EventArgs e) =>
			{
				if (DialogResult.Yes == MessageBox.Show("選択ファイルを削除して ZIP ファイルを更新しますか？", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				{
					DeleteArchiveEntries();
				}
			};
			// 圧縮。
			buttonCompress.Click += (object? sender, EventArgs e) =>
			{
				CompressFolder();
			};

			// コントロールの更新。
			UpdateControls();
		}

		/// <summary>
		/// コントロールの更新。
		/// </summary>
		private void UpdateControls()
		{
			buttonClose.Enabled = (null != openArchivePath || null != openFolderPath);
			buttonExtractAll.Enabled = (null != openArchivePath);
			buttonExtract.Enabled = (null != openArchivePath && listView1.SelectedIndices.Count >= 1);
			buttonDelete.Enabled = (null != openArchivePath && listView1.SelectedIndices.Count >= 1);
			buttonCompress.Enabled = (null != openFolderPath);
		}

		/// <summary>
		/// ListView にアーカイブのファイル一覧追加。
		/// </summary>
		/// <param name="path"></param>
		private void GetArchiveEntries(string path)
		{
			// ListView クリア
			listView1.Items.Clear();
			// 読み取り専用でアーカイブを開く
			openArchivePath = path;
			using (ZipArchive archive = ZipFile.OpenRead(openArchivePath))
			{
				// ListViewItem 追加
				foreach (var entry in archive.Entries)
				{
					var item = new ListViewItem(entry.Name);
					item.SubItems.Add(Path.GetDirectoryName(entry.FullName));
					item.SubItems.Add(entry.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
					item.SubItems.Add(entry.Length.ToString());
					item.Tag = entry;
					listView1.Items.Add(item);
				}
			}
			UpdateControls();
		}

		/// <summary>
		/// ListView にファイル一覧追加。
		/// </summary>
		/// <param name="paths"></param>
		private void GetFolderFiles(string path)
		{
			openFolderPath = path;
			// ListView クリア
			listView1.Items.Clear();
			// ListViewItem 追加
			foreach (var entry in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
			{
				var item = new ListViewItem(entry.Name);
				item.SubItems.Add(Path.GetRelativePath(path, entry.DirectoryName!));
				item.SubItems.Add(entry.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
				item.SubItems.Add(entry.Length.ToString());
				item.Tag = entry;
				listView1.Items.Add(item);
			}
			UpdateControls();
		}

		/// <summary>
		/// 全て展開。
		/// </summary>
		private void ExtractArchive()
		{
			if (DialogResult.OK == folderBrowserDialog.ShowDialog())
			{
				DateTime dt = DateTime.Now;
				ZipFile.ExtractToDirectory(openArchivePath, folderBrowserDialog.SelectedPath, true);
				Debug.WriteLine("ExtractToDirectory " + (DateTime.Now - dt).TotalSeconds);
			}
		}

		/// <summary>
		/// 選択ファイルを展開。
		/// </summary>
		private void ExtractSelectedArchiveEntries()
		{
			if (DialogResult.OK == folderBrowserDialog.ShowDialog())
			{
				// 読み取り専用でアーカイブを開く
				using (ZipArchive archive = ZipFile.OpenRead(openArchivePath))
				{
					foreach (ListViewItem item in listView1.SelectedItems)
					{
						// 選択ファイルと一致するエントリーを展開
						var entry = (ZipArchiveEntry)item.Tag;
						string path = Path.Combine(folderBrowserDialog.SelectedPath, entry.Name);
						var extractEntry = archive.GetEntry(entry.Name);
						extractEntry?.ExtractToFile(path, true);
					}
				}
			}
		}

		/// <summary>
		/// 圧縮。
		/// </summary>
		private void CompressFolder()
		{
			saveFileDialog.FileName = Path.GetFileName(openFolderPath) + ".zip";
			if (DialogResult.OK == saveFileDialog.ShowDialog())
			{
				if (File.Exists(saveFileDialog.FileName))
				{
					File.Delete(saveFileDialog.FileName);
				}
				DateTime dt = DateTime.Now;
				ZipFile.CreateFromDirectory(openFolderPath, saveFileDialog.FileName);
				Debug.WriteLine("CreateFromDirectory " + (DateTime.Now - dt).TotalSeconds);
			}
		}

		/// <summary>
		/// アーカイブにファイルを追加。
		/// </summary>
		/// <param name="paths"></param>
		private void AddArchiveEntries(string[] paths)
		{
			// 読み書き用にアーカイブを開く
			using (ZipArchive archive = ZipFile.Open(openArchivePath, ZipArchiveMode.Update))
			{
				foreach (string path in paths)
				{
					string dir = Path.GetDirectoryName(path) ?? "";
					foreach (var file in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
					{
						// 選択フォルダーを起点とした相対パスでエントリーを作成
						archive.CreateEntryFromFile(file.FullName, Path.GetRelativePath(dir, file.FullName));
					}
				}
			}
			// ListView にアーカイブのファイル一覧追加。
			GetArchiveEntries(openArchivePath);
		}

		/// <summary>
		/// アーカイブから選択ファイルを削除。
		/// </summary>
		private void DeleteArchiveEntries()
		{
			// 読み書き用にアーカイブを開く
			using (ZipArchive archive = ZipFile.Open(openArchivePath, ZipArchiveMode.Update))
			{
				foreach (ListViewItem item in listView1.SelectedItems)
				{
					// 選択ファイルと一致するエントリーを削除
					var entry = (ZipArchiveEntry)item.Tag;
					var deleteEntry = archive.GetEntry(entry.Name);
					deleteEntry?.Delete();
				}
			}
			// ListView にアーカイブのファイル一覧追加。
			GetArchiveEntries(openArchivePath);
		}

		/// <summary>
		/// フォルダーにファイルをコピー。
		/// </summary>
		/// <param name="paths"></param>
		private void CopyFiles(string[] paths)
		{
			foreach (string path in paths)
			{
				string dst = Path.Combine(openFolderPath, Path.GetFileName(path));
				if (Directory.Exists(path))
				{
					FileIO.FileSystem.CopyDirectory(path, dst, FileIO.UIOption.AllDialogs, FileIO.UICancelOption.DoNothing);
				}
				else
				{
					FileIO.FileSystem.CopyFile(path, dst, FileIO.UIOption.AllDialogs, FileIO.UICancelOption.DoNothing);
				}
			}
			// ListView にファイル一覧追加。
			GetFolderFiles(openFolderPath);
		}
	}
}