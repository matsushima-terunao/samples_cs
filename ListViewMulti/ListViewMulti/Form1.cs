using System.Diagnostics;

//2022/11/09
//31.4
//17.8
//20.3
//22.5
namespace ListViewMulti
{
	public partial class Form1 : Form
	{
		/// <summary>サムネイル ListViewItem</summary>
		private List<ListViewItem> listViewItems = new();
		/// <summary>サムネイル ImageList</summary>
		private ImageList imageList = new();
		/// <summary>タスクキャンセル</summary>
		private CancellationTokenSource cancellationTokenSource = null!;

		public Form1()
		{
			InitializeComponent();

			// ドラッグアンドドロップでアーカイブを開く
			DragDrop += (object? sender, DragEventArgs e) =>
			{
				string[] files = (string[])e.Data?.GetData(DataFormats.FileDrop, false)!;
				ReadFolder(files[0]);
			};
			DragEnter += (object? sender, DragEventArgs e) =>
			{
				e.Effect = ((e.Data?.GetDataPresent(DataFormats.FileDrop)) ?? false) ? DragDropEffects.All : DragDropEffects.None;
			};
			AllowDrop = true;

			// 仮想 ListView
			listView1.RetrieveVirtualItem += (object? sender, RetrieveVirtualItemEventArgs e) =>
			{
				e.Item = listViewItems[e.ItemIndex];
			};
			listView1.VirtualListSize = 0;
			listView1.VirtualMode = true;
		}

		/// <summary>
		/// フォルダー内のファイル情報を ListView に追加。
		/// </summary>
		/// <param name="path"></param>
		public void ReadFolder(string path)
		{
			// タスクキャンセル
			if (null != cancellationTokenSource)
			{
				Debug.WriteLine("cancel");
				cancellationTokenSource.Cancel();
			}

			DateTime dt = DateTime.Now;

			// ListView 更新
			listView1.Items.Clear();
			// サムネイル ImageList
			imageList = new();
			imageList.ImageSize = new Size(100, 100);
			// ダミーイメージ
			var bitmap = new Bitmap(1, 1);
			imageList.Images.Add(bitmap);
			listView1.LargeImageList = imageList;
			// サムネイル ListViewItem
			listViewItems = new();
			foreach (var file in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
			{
				var item = new ListViewItem(file.Name);
				item.ImageIndex = 0;
				item.Tag = file;
				listViewItems.Add(item);
			}
			// 仮想 ListView 更新
			listView1.VirtualListSize = listViewItems.Count;
			// 読み込み進捗 ToolStripProgressBar
			toolStripProgressBar1.Minimum = 0;
			toolStripProgressBar1.Maximum = listViewItems.Count;
			toolStripProgressBar1.Value = 0;

			// 画像読み込み処理
			cancellationTokenSource = new();
			var cancelToken = cancellationTokenSource.Token; // タスク開始前に退避
#if false
			// 画像読み込み処理を実行
			AddThumbnails(cancelToken);
#else
			// 画像読み込み処理を別スレッドで実行
			new Task(() => AddThumbnails(cancelToken), cancelToken).Start();
#endif
			Debug.WriteLine("ReadFolder " + (DateTime.Now - dt).TotalSeconds);
		}

		/// <summary>
		/// 画像読み込み処理。
		/// </summary>
		private void AddThumbnails(CancellationToken cancelToken)
		{
			DateTime dt = DateTime.Now;
#if false
			// 画像読み込み処理を実行
			for (int fileIdx = 0; fileIdx < listViewItems.Count; ++fileIdx)
			{
				AddThumbnail(fileIdx, cancelToken);
			}
#else
			// 並列処理で画像読み込み処理を実行
			var parallelOptions = new ParallelOptions() { CancellationToken = cancelToken };
			try
			{
				Parallel.For(0, listViewItems.Count, parallelOptions, fileIdx =>
				{
					AddThumbnail(fileIdx, cancelToken);
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine("catch in parallel");
				Debug.WriteLine(ex);
			}
#endif
			Invoke((Action)(() =>
			{
				toolStripProgressBar1.Value = 0;
				toolStripStatusLabel1.Text = "AddThumbnails " + (DateTime.Now - dt).TotalSeconds;
			}));
			Debug.WriteLine("AddThumbnails " + (DateTime.Now - dt).TotalSeconds);
		}

		/// <summary>
		/// 画像読み込み処理。
		/// </summary>
		/// <param name="fileIdx"></param>
		private void AddThumbnail(int fileIdx, CancellationToken cancelToken)
		{
			try
			{
				// 画像ファイルのサムネイル作成。
				var fileInfo = (FileInfo)listViewItems[fileIdx].Tag;
				var bitmap = CreateThmbnail(fileInfo, imageList.ImageSize.Width);
				{
					// Form のスレッドで ListViewItem 更新
					Invoke((Action<int, Image>)((_fileIdx, _bitmap) =>
					{
						if (!cancelToken.IsCancellationRequested)
						{
							imageList.Images.Add(_bitmap);
							_bitmap.Dispose();
							listViewItems[_fileIdx].ImageIndex = imageList.Images.Count - 1;
							if (listView1.ClientRectangle.IntersectsWith(listViewItems[_fileIdx].Bounds))
							{
								listView1.RedrawItems(_fileIdx, _fileIdx, true);
							}
							++toolStripProgressBar1.Value;
						}
					}), fileIdx, bitmap);
					//Task.Delay(1).Wait();
				}
			}
			catch (Exception ex)
			{
				// 画像ファイルでない
				Debug.WriteLine(ex);
				Debug.WriteLine(listViewItems[fileIdx].Tag);
				Invoke((Action)(() => {
					++toolStripProgressBar1.Value;
				}));
			}
		}

		/// <summary>
		/// アーカイブ内画像ファイルのサムネイル作成。
		/// archiveEntry アクセス時に例外が発生しないようにロックをかける。
		/// </summary>
		/// <param name="archiveEntry"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		private Image CreateThmbnail(FileInfo fileInfo, int distance)
		{
			using var image = Image.FromFile(fileInfo.FullName);
			var rect = new Rectangle();
			if (image.Width > image.Height)
			{
				rect.Width = distance;
				rect.Height = distance * image.Height / image.Width;
				rect.X = 0;
				rect.Y = (distance - rect.Height) / 2;
			}
			else
			{
				rect.Height = distance;
				rect.Width = distance * image.Width / image.Height;
				rect.Y = 0;
				rect.X = (distance - rect.Width) / 2;
			}
			var bitmap = new Bitmap(distance, distance);
			using var graphics = Graphics.FromImage(bitmap);
			graphics.DrawImage(image, rect);
			return bitmap;
		}
	}
}
