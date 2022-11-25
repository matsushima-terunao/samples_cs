using System.Diagnostics;
using System.IO.Compression;
using FileIO = Microsoft.VisualBasic.FileIO;

namespace ArchiveTest
{
	/// <summary>
	/// ZIP �A�[�J�C�u�T���v��
	/// 2022/11/11 matsushima
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>���݊J���Ă���A�[�J�C�u�̃p�X</summary>
		private string openArchivePath = null!;
		/// <summary>���݊J���Ă���t�H���_�[�̃p�X</summary>
		private string openFolderPath = null!;
		/// <summary>�t�@�C���I���_�C�A���O</summary>
		private readonly OpenFileDialog openFileDialog = new() { Filter = "ZIP �t�@�C�� (*.zip)|*.zip|���ׂẴt�@�C�� (*.*)|*.*" };
		/// <summary>�t�@�C���ۑ��_�C�A���O</summary>
		private readonly SaveFileDialog saveFileDialog = new() { Filter = "ZIP �t�@�C�� (*.zip)|*.zip|���ׂẴt�@�C�� (*.*)|*.*" };
		/// <summary>�t�H���_�[�I���_�C�A���O</summary>
		private readonly FolderBrowserDialog folderBrowserDialog = new();

		public Form1()
		{
			InitializeComponent();

			// ListView ������
			listView1.View = View.Details;
			listView1.Columns.Add("���O", 200, HorizontalAlignment.Left);
			listView1.Columns.Add("�ꏊ", 200, HorizontalAlignment.Left);
			listView1.Columns.Add("�X�V����", 120, HorizontalAlignment.Left);
			listView1.Columns.Add("�T�C�Y", 80, HorizontalAlignment.Right);
			listView1.SelectedIndexChanged += (object? sender, EventArgs e) =>
			{
				UpdateControls();
			};

			// Form �h���b�O&�h���b�v
			DragDrop += (object? sender, DragEventArgs e) =>
			{
				string[] files = (string[])e.Data?.GetData(DataFormats.FileDrop, false)!;
				DialogResult addFiles = DialogResult.No; // yes:�ǉ� no:�V�K cancel:�L�����Z��
				if (null != openArchivePath)
				{
					// �A�[�J�C�u�Ƀt�@�C����ǉ��B
					addFiles = MessageBox.Show("�t�@�C����ǉ����� ZIP �t�@�C�����X�V���܂���?\n�܂��͐V�K�ɊJ���܂���?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (DialogResult.Yes == addFiles)
					{
						AddArchiveEntries(files);
					}
				}
				else if (null != openFolderPath)
				{
					// �t�H���_�[�Ƀt�@�C�����R�s�[�B
					addFiles = MessageBox.Show("�t�H���_�[�Ƀt�@�C�����R�s�[���܂���?\n�܂��͐V�K�ɊJ���܂���?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (DialogResult.Yes == addFiles)
					{
						CopyFiles(files);
					}
				}
				if (DialogResult.No == addFiles)
				{
					if (1 == files.Length && ".zip" == Path.GetExtension(files[0]).ToLower() && File.Exists(files[0]))
					{
						// ListView �ɃA�[�J�C�u�̃t�@�C���ꗗ�ǉ��B
						openFolderPath = null!;
						GetArchiveEntries(files[0]);
					}
					else if (1 == files.Length && Directory.Exists(files[0]))
					{
						// ListView �Ƀt�@�C���ꗗ�ǉ��B
						openArchivePath = null!;
						GetFolderFiles(files[0]);
					}
					else
					{
						MessageBox.Show("�W�J������ ZIP �t�@�C���܂��͈��k�������t�H���_�[��1�����h���b�v���Ă��������B", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				}
			};
			DragEnter += (object? sender, DragEventArgs e) =>
			{
				e.Effect = ((e.Data?.GetDataPresent(DataFormats.FileDrop)) ?? false) ? DragDropEffects.All : DragDropEffects.None;
			};
			AllowDrop = true;

			// ListView �ɃA�[�J�C�u�̃t�@�C���ꗗ�ǉ��B
			buttonOpenArchive.Click += (object? sender, EventArgs e) =>
			{
				if (DialogResult.OK == openFileDialog.ShowDialog())
				{
					openFolderPath = null!;
					GetArchiveEntries(openFileDialog.FileName);
				}
			};
			// ListView �Ƀt�@�C���ꗗ�ǉ��B
			buttonOpenFolder.Click += (object? sender, EventArgs e) =>
			{
				if (DialogResult.OK == folderBrowserDialog.ShowDialog())
				{
					openArchivePath = null!;
					GetFolderFiles(folderBrowserDialog.SelectedPath);
				}
			};
			// ����
			buttonClose.Click += (object? sender, EventArgs e) =>
			{
				listView1.Items.Clear();
				openArchivePath = null!;
				openFolderPath = null!;
				UpdateControls();
			};
			// �S�ēW�J�B
			buttonExtractAll.Click += (object? sender, EventArgs e) =>
			{
				ExtractArchive();
			};
			// �I���t�@�C����W�J�B
			buttonExtract.Click += (object? sender, EventArgs e) =>
			{
				ExtractSelectedArchiveEntries();
			};
			// �A�[�J�C�u����I���t�@�C�����폜�B
			buttonDelete.Click += (object? sender, EventArgs e) =>
			{
				if (DialogResult.Yes == MessageBox.Show("�I���t�@�C�����폜���� ZIP �t�@�C�����X�V���܂����H", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				{
					DeleteArchiveEntries();
				}
			};
			// ���k�B
			buttonCompress.Click += (object? sender, EventArgs e) =>
			{
				CompressFolder();
			};

			// �R���g���[���̍X�V�B
			UpdateControls();
		}

		/// <summary>
		/// �R���g���[���̍X�V�B
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
		/// ListView �ɃA�[�J�C�u�̃t�@�C���ꗗ�ǉ��B
		/// </summary>
		/// <param name="path"></param>
		private void GetArchiveEntries(string path)
		{
			// ListView �N���A
			listView1.Items.Clear();
			// �ǂݎ���p�ŃA�[�J�C�u���J��
			openArchivePath = path;
			using (ZipArchive archive = ZipFile.OpenRead(openArchivePath))
			{
				// ListViewItem �ǉ�
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
		/// ListView �Ƀt�@�C���ꗗ�ǉ��B
		/// </summary>
		/// <param name="paths"></param>
		private void GetFolderFiles(string path)
		{
			openFolderPath = path;
			// ListView �N���A
			listView1.Items.Clear();
			// ListViewItem �ǉ�
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
		/// �S�ēW�J�B
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
		/// �I���t�@�C����W�J�B
		/// </summary>
		private void ExtractSelectedArchiveEntries()
		{
			if (DialogResult.OK == folderBrowserDialog.ShowDialog())
			{
				// �ǂݎ���p�ŃA�[�J�C�u���J��
				using (ZipArchive archive = ZipFile.OpenRead(openArchivePath))
				{
					foreach (ListViewItem item in listView1.SelectedItems)
					{
						// �I���t�@�C���ƈ�v����G���g���[��W�J
						var entry = (ZipArchiveEntry)item.Tag;
						string path = Path.Combine(folderBrowserDialog.SelectedPath, entry.Name);
						var extractEntry = archive.GetEntry(entry.Name);
						extractEntry?.ExtractToFile(path, true);
					}
				}
			}
		}

		/// <summary>
		/// ���k�B
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
		/// �A�[�J�C�u�Ƀt�@�C����ǉ��B
		/// </summary>
		/// <param name="paths"></param>
		private void AddArchiveEntries(string[] paths)
		{
			// �ǂݏ����p�ɃA�[�J�C�u���J��
			using (ZipArchive archive = ZipFile.Open(openArchivePath, ZipArchiveMode.Update))
			{
				foreach (string path in paths)
				{
					string dir = Path.GetDirectoryName(path) ?? "";
					foreach (var file in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
					{
						// �I���t�H���_�[���N�_�Ƃ������΃p�X�ŃG���g���[���쐬
						archive.CreateEntryFromFile(file.FullName, Path.GetRelativePath(dir, file.FullName));
					}
				}
			}
			// ListView �ɃA�[�J�C�u�̃t�@�C���ꗗ�ǉ��B
			GetArchiveEntries(openArchivePath);
		}

		/// <summary>
		/// �A�[�J�C�u����I���t�@�C�����폜�B
		/// </summary>
		private void DeleteArchiveEntries()
		{
			// �ǂݏ����p�ɃA�[�J�C�u���J��
			using (ZipArchive archive = ZipFile.Open(openArchivePath, ZipArchiveMode.Update))
			{
				foreach (ListViewItem item in listView1.SelectedItems)
				{
					// �I���t�@�C���ƈ�v����G���g���[���폜
					var entry = (ZipArchiveEntry)item.Tag;
					var deleteEntry = archive.GetEntry(entry.Name);
					deleteEntry?.Delete();
				}
			}
			// ListView �ɃA�[�J�C�u�̃t�@�C���ꗗ�ǉ��B
			GetArchiveEntries(openArchivePath);
		}

		/// <summary>
		/// �t�H���_�[�Ƀt�@�C�����R�s�[�B
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
			// ListView �Ƀt�@�C���ꗗ�ǉ��B
			GetFolderFiles(openFolderPath);
		}
	}
}