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
	/// ZIP �A�[�J�C�u�T���v��
	/// 2022/11/15 matsushima
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>���݊J���Ă���A�[�J�C�u�̃p�X</summary>
		private string openArchivePath = null!;
		/// <summary>�t�@�C���I���_�C�A���O</summary>
		private readonly OpenFileDialog openFileDialog = new() { Filter = "���ׂẴt�@�C�� (*.*)|*.*" };
		/// <summary>�t�@�C���ۑ��_�C�A���O</summary>
		private readonly SaveFileDialog saveFileDialog = new() { Filter = "Zip �t�@�C�� (*.zip)|*.zip|Tar �t�@�C�� (*.tar)|*.tar|GZip �t�@�C�� (*.gz;*.gzip;*.tar.gz;*.tar.gzip;*.tgz)|*.gz;*.gzip;*.tar.gz;*.tar.gzip;*.tgz|BZip2 �t�@�C�� (*.bz2;*.bzip2;*.tar.bz2;*.tar.bzip2;*.tbz2)|*.bz2;*.bzip2;*.tar.bz2;*.tar.bzip2;*.tbz2|LZip �t�@�C�� (*.lz;*.tar.lz)|*.lz;*.tar.lz|XZ �t�@�C�� (*.xz;*.tar.xz)|*.xz;*.tar.xz" };
		/// <summary>�t�H���_�[�I���_�C�A���O</summary>
		private readonly FolderBrowserDialog folderBrowserDialog = new();

		/// <summary>
		/// �A�[�J�C�u�ƈ��k�����̑Ή�
		/// </summary>
		struct ArchiveMapping
		{
			public ArchiveType ArchiveType;
			public CompressionType CompressionType;
		};
		/// <summary>
		/// �A�[�J�C�u�ƈ��k�����̑Ή�
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

			// ListView ������
			listView1.View = View.Details;
			listView1.Columns.Add("���O", 180, HorizontalAlignment.Left);
			listView1.Columns.Add("�ꏊ", 160, HorizontalAlignment.Left);
			listView1.Columns.Add("�X�V����", 120, HorizontalAlignment.Left);
			listView1.Columns.Add("�T�C�Y", 80, HorizontalAlignment.Right);
			listView1.Columns.Add("���k����", 60, HorizontalAlignment.Left);
			listView1.SelectedIndexChanged += (object? sender, EventArgs e) =>
			{
				UpdateControls();
			};

			// �R���g���[���̍X�V�B
			UpdateControls();

			// �{�^��
			// �A�[�J�C�u���J��
			buttonOpenArchive.Click += (object? sender, EventArgs e) =>
			{
				if (DialogResult.OK == openFileDialog.ShowDialog())
				{
					// ListView �ɃA�[�J�C�u�̃t�@�C���ꗗ�ǉ��B
					GetArchiveEntries(openFileDialog.FileName);
				}
			};
			// �t�H���_�[�����k
			buttonCompressFolder.Click += (object? sender, EventArgs e) =>
			{
				// �t�H���_�[�����k�B
				CompressFolder();
			};
			// ����
			buttonClose.Click += (object? sender, EventArgs e) =>
			{
				listView1.Items.Clear();
				openArchivePath = null!;
				UpdateControls();
			};
			// ���ׂēW�J
			buttonExtractAll.Click += (object? sender, EventArgs e) =>
			{
				// ���ׂēW�J�B
				ExtractArchive();
			};
			// �W�J
			buttonExtract.Click += (object? sender, EventArgs e) =>
			{
				// �I���t�@�C����W�J�B
				ExtractSelectedArchiveEntries();
			};
			// �폜
			buttonDelete.Click += (object? sender, EventArgs e) =>
			{
				if (null != openArchivePath)
				{
					if (DialogResult.Yes == MessageBox.Show("�I���t�@�C�����폜���ăA�[�J�C�u���X�V���܂���?",
						Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					{
						// �A�[�J�C�u����I���t�@�C�����폜�B
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
			// ���ׂĈ��k
			buttonCompressAll.Click += (object? sender, EventArgs e) =>
			{
				// �w��t�@�C�������k�B
				Compress(listView1.Items.Cast<ListViewItem>());
			};
			// ���k
			buttonCompress.Click += (object? sender, EventArgs e) =>
			{
				// �w��t�@�C�������k�B
				Compress(listView1.SelectedItems.Cast<ListViewItem>());
			};

			// Form �h���b�O&�h���b�v
			DragDrop += (object? sender, DragEventArgs e) =>
			{
				// �t�@�C�������B
				string[] files = (string[])e.Data?.GetData(DataFormats.FileDrop, false)!;
				ProcessFiles(files);
			};
			DragEnter += (object? sender, DragEventArgs e) =>
			{
				e.Effect = ((e.Data?.GetDataPresent(DataFormats.FileDrop)) ?? false) ? DragDropEffects.All : DragDropEffects.None;
			};
			AllowDrop = true;

			// �R�}���h���C������
			string[] files = System.Environment.GetCommandLineArgs();
			if (files.Length >= 2)
			{
				// �t�@�C�������B
				ProcessFiles(files[1..]);
			}
		}

		/// <summary>
		/// �R���g���[���̍X�V�B
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
		/// �t�@�C�������B
		/// </summary>
		/// <param name="files"></param>
		private void ProcessFiles(string[] files)
		{
			DialogResult addFiles = DialogResult.Yes; // yes:�ǉ� no:�V�K cancel:�L�����Z��
			// �A�[�J�C�u���J��
			if (1 == files.Length && File.Exists(files[0]))
			{
				try
				{
					// �A�[�J�C�u�����肷�邽�߂ɊJ��
					using (var archive = ArchiveFactory.Open(files[0]))
					{
					}
					addFiles = MessageBox.Show("�t�@�C����ǉ����܂���?\n�܂��̓A�[�J�C�u���J���܂���?",
						Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (DialogResult.No == addFiles)
					{
						// ListView �ɃA�[�J�C�u�̃t�@�C���ꗗ�ǉ��B
						GetArchiveEntries(files[0]);
					}
				}
				catch (Exception)
				{
					// �A�[�J�C�u�łȂ�
					addFiles = DialogResult.Yes;
					Debug.WriteLine("not an archive");
				}
			}
			// �t�@�C����ǉ�
			if (DialogResult.Yes == addFiles)
			{
				if (null != openArchivePath)
				{
					addFiles = MessageBox.Show("�t�@�C����ǉ����ăA�[�J�C�u���X�V���܂���?",
						Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (DialogResult.Yes == addFiles)
					{
						// �A�[�J�C�u�Ƀt�@�C����ǉ��B
						AddArchiveEntries(files);
					}
				}
				else
				{
					// ListView �Ƀt�@�C���ꗗ�ǉ��B
					AddFolderFiles(files);
				}
			}
		}

		/// <summary>
		/// ListView �ɃA�[�J�C�u�̃t�@�C���ꗗ�ǉ��B
		/// </summary>
		/// <param name="path"></param>
		private void GetArchiveEntries(string path)
		{
			openArchivePath = path;
			// ListViewItem �ǉ�
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
		/// ListView �Ƀt�@�C���ꗗ�ǉ��B
		/// </summary>
		/// <param name="paths"></param>
		private void AddFolderFiles(string[] paths)
		{
			// ListViewItem �ǉ�
			foreach (string path in paths)
			{
				if (Directory.Exists(path))
				{
					string basePath = Path.GetDirectoryName(path) ?? "";
					foreach (var file in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
					{
						// �I���t�H���_�[���N�_�Ƃ������΃p�X�ŃG���g���[���쐬
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
		/// ���ׂēW�J�B
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
		/// �I���t�@�C����W�J�B
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
		/// �t�H���_�[�����k�B
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
							ArchiveEncoding = new() { Default = Encoding.UTF8 } })) // �t�@�C�����̕��������Ή�
					{
						writer.WriteAll(folderBrowserDialog.SelectedPath, "*", SearchOption.AllDirectories);
					}
					Debug.WriteLine("WriteAll " + mapping.ArchiveType + " " + mapping.CompressionType + " " + (DateTime.Now - dt).TotalSeconds);
					// ListView �ɃA�[�J�C�u�̃t�@�C���ꗗ�ǉ��B
					GetArchiveEntries(saveFileDialog.FileName);
				}
			}
		}

		/// <summary>
		/// �w��t�@�C�������k�B
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
						ArchiveEncoding = new() { Default = Encoding.UTF8 } })) // �t�@�C�����̕��������Ή�
				{
					foreach (ListViewItem item in items)
					{
						writer.Write(Path.Combine(item.SubItems[1].Text, item.Text), (FileInfo)item.Tag);
					}
				}
				Debug.WriteLine("Write " + mapping.ArchiveType + " " + mapping.CompressionType + " " + (DateTime.Now - dt).TotalSeconds);
				// ListView �ɃA�[�J�C�u�̃t�@�C���ꗗ�ǉ��B
				GetArchiveEntries(saveFileDialog.FileName);
			}
		}

		/// <summary>
		/// �ǉ��E�폜�p�ɃA�[�J�C�u���J���B
		/// </summary>
		/// <param name="func"></param>
		private void OpenArchiveForModify(Action<IWritableArchive> func)
		{
			using (var archive = (IWritableArchive)ArchiveFactory.Open(openArchivePath))
			{
				// tar ��1�����܂܂�Ă��邩����
				if (ArchiveType.Tar == archive.Type && 1 == archive.Entries.Count() && null == archive.Entries.First().Key)
				{
					// tar ���ꎞ�t�@�C���Ƃ��ēW�J
					string tarPath = openArchivePath + ".tar.tmp";
					archive.Entries.First().WriteToFile(tarPath,
						new ExtractionOptions() { Overwrite = true, ExtractFullPath = true, PreserveFileTime = true, PreserveAttributes = false });
					var mapping = archiveMappings.Where(m => m.CompressionType == archive.Entries.First().CompressionType).First();
					archive.Dispose();

					// tar ���J��
					using (var tarArchive = (IWritableArchive)ArchiveFactory.Open(tarPath))
					{
						func(tarArchive);
						// tar.xxx �Ɉ��k
						tarArchive.SaveTo(openArchivePath + ".tmp",
							new(mapping.CompressionType) { ArchiveEncoding = new() { Default = Encoding.UTF8 } }); // �t�@�C�����̕��������Ή�
					}
					File.Delete(tarPath);
				}
				else
				{
					// tar.xxx �ȊO
					func(archive);
					// �ꎞ�t�@�C���ɏ����o��
					var mapping = archiveMappings.Where(m => m.ArchiveType == archive.Type).First();
					archive.SaveTo(openArchivePath + ".tmp",
						new(mapping.CompressionType) { ArchiveEncoding = new() { Default = Encoding.UTF8 } }); // �t�@�C�����̕��������Ή�
				}
			}
			File.Move(openArchivePath + ".tmp", openArchivePath, true);
			// ListView �ɃA�[�J�C�u�̃t�@�C���ꗗ�ǉ��B
			GetArchiveEntries(openArchivePath);
		}

		/// <summary>
		/// �A�[�J�C�u�Ƀt�@�C����ǉ��B
		/// </summary>
		/// <param name="paths"></param>
		private void AddArchiveEntries(string[] paths)
		{
			// �ǉ��E�폜�p�ɃA�[�J�C�u���J���B
			OpenArchiveForModify(archive =>
			{
				foreach (string path in paths)
				{
					if (Directory.Exists(path))
					{
						string basePath = Path.GetDirectoryName(path) ?? "";
						foreach (var file in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
						{
							// �I���t�H���_�[���N�_�Ƃ������΃p�X�ŃG���g���[��ǉ�
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
		/// �A�[�J�C�u����I���t�@�C�����폜�B
		/// </summary>
		private void DeleteArchiveEntries()
		{
			// �ǉ��E�폜�p�ɃA�[�J�C�u���J���B
			OpenArchiveForModify(archive =>
			{
				foreach (ListViewItem item in listView1.SelectedItems)
				{
					foreach (var entry in archive.Entries)
					{
						if (entry.Key.Equals(((IEntry)item.Tag).Key))
						{
							// �I���t�@�C���ƈ�v����G���g���[���폜
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