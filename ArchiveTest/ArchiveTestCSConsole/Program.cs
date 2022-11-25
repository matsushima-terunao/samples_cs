// SharpCompress のテスト
// 2022/11/13 matsushima

using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using System.Text;

string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/archivetest/"; // マイドキュメント
string srcDir = baseDir + "test/"; // 圧縮したいファイルのあるフォルダー
string dstFile = baseDir + "test.zip"; // 圧縮ファイル名
string dstFile2 = baseDir + "test.tar.gz"; // 圧縮ファイル名
string dstDir = baseDir + "test2"; // 展開先フォルダー

Console.WriteLine("新規 archive, writer");
using (var archive11 = ZipArchive.Create())
using (var archive12 = ArchiveFactory.Create(ArchiveType.Zip))
using (var stream11 = File.Create(dstFile))
using (var writer11 = new ZipWriter(stream11,
	new(CompressionType.Deflate) { LeaveStreamOpen = true,
		ArchiveEncoding = new() { Default = Encoding.UTF8 } })) // ファイル名の文字化け対応
using (var writer12 = WriterFactory.Open(stream11, ArchiveType.Zip,
	new(CompressionType.Deflate) { LeaveStreamOpen = true,
		ArchiveEncoding = new() { Default = Encoding.UTF8 } })) // ファイル名の文字化け対応
{
	Console.WriteLine(archive11);
	Console.WriteLine(archive12);
	Console.WriteLine(stream11);
	Console.WriteLine(writer11);
	Console.WriteLine(writer12);
}
Console.WriteLine("読み込み archive, reader");
using (var archive21 = ArchiveFactory.Open(dstFile))
using (var stream21 = File.OpenRead(dstFile))
using (var archive22 = ArchiveFactory.Open(stream21))
using (var reader21 = ReaderFactory.Open(stream21))
{
	Console.WriteLine(archive21);
	Console.WriteLine(stream21);
	Console.WriteLine(archive22);
	Console.WriteLine(reader21);
}
Console.WriteLine("更新 archive");
using (var archive31 = (IWritableArchive)ArchiveFactory.Open(dstFile))
{
	Console.WriteLine(archive31);
}

Console.WriteLine($"フォルダー配下を圧縮 {srcDir} {dstFile}");
using (var archive = ZipArchive.Create())
{
	archive.AddAllFromDirectory(srcDir);
	archive.SaveTo(dstFile,
		new(CompressionType.Deflate) { ArchiveEncoding = new() { Default = Encoding.UTF8 } }); // ファイル名の文字化け対応
}

Console.WriteLine($"指定ファイルを圧縮 {srcDir} {dstFile2}");
using (var stream = File.Create(dstFile2))
using (var writer = WriterFactory.Open(stream, ArchiveType.Tar,
	new(CompressionType.GZip) { LeaveStreamOpen = true,
		ArchiveEncoding = new() { Default = Encoding.UTF8 } })) // ファイル名の文字化け対応
{
	foreach (var entry in new DirectoryInfo(srcDir).EnumerateFiles("*", SearchOption.AllDirectories))
	{
		writer.Write(Path.GetRelativePath(srcDir, entry.FullName), entry);
	}
}

Console.WriteLine($"全て展開 {dstFile} {dstDir}");
if (Directory.Exists(dstDir))
{
	Directory.Delete(dstDir, true);
}
Directory.CreateDirectory(dstDir);
using (var archive = ArchiveFactory.Open(dstFile))
{
	archive.WriteToDirectory(dstDir,
		new ExtractionOptions() { Overwrite = true, ExtractFullPath = true, PreserveFileTime = true, PreserveAttributes = false });
}

Console.WriteLine($"指定ファイルを展開 {dstFile2} {dstDir}");
if (Directory.Exists(dstDir))
{
	Directory.Delete(dstDir, true);
}
Directory.CreateDirectory(dstDir);
using (var stream = File.OpenRead(dstFile2))
using (var reader = ReaderFactory.Open(stream))
{
	while (reader.MoveToNextEntry())
	{
		if (!reader.Entry.IsDirectory)
		{
			reader.WriteEntryToDirectory(dstDir,
				new ExtractionOptions() { Overwrite = true, ExtractFullPath = true, PreserveFileTime = true, PreserveAttributes = false });
		}
	}
}

Console.WriteLine($"一覧 {dstFile}");
using (var archive = ArchiveFactory.Open(dstFile))
{
	foreach (var entry in archive.Entries)
	{
		Console.WriteLine(entry);
	}
}

Console.WriteLine($"一覧 {dstFile2}");
using (var stream = File.OpenRead(dstFile2))
using (var reader = ReaderFactory.Open(stream))
{
	while (reader.MoveToNextEntry())
	{
		Console.WriteLine(reader.Entry);
	}
}

Console.WriteLine($"追加 {dstFile} {dstFile + "add.zip"}");
using (var archive = (IWritableArchive)ArchiveFactory.Open(dstFile))
{
	foreach (var entry in new DirectoryInfo(srcDir).EnumerateFiles("*", SearchOption.AllDirectories))
	{
		archive.AddEntry("newfolder/" + entry.Name, entry);
		break;
	}
	archive.SaveTo(dstFile + "add.zip",
		new(CompressionType.Deflate) { ArchiveEncoding = new() { Default = Encoding.UTF8 } }); // ファイル名の文字化け対応
}

Console.WriteLine($"追加 {dstFile2} {dstFile2 + "add.tar.gz"}");
// tar を一時ファイルとして展開
using (var archive = ArchiveFactory.Open(dstFile2))
{
	archive.Entries.First().WriteToFile(dstFile2 + "add.tar.tmp",
		new ExtractionOptions() { Overwrite = true, ExtractFullPath = true, PreserveFileTime = true, PreserveAttributes = false });
}
// tar を開く
using (var archive = (IWritableArchive)ArchiveFactory.Open(dstFile2 + "add.tar.tmp"))
{
	// tar にファイル追加
	foreach (var entry in new DirectoryInfo(srcDir).EnumerateFiles("*", SearchOption.AllDirectories))
	{
		archive.AddEntry("newfolder/" + entry.Name, entry);
		break;
	}
	// tar.gz に圧縮
	archive.SaveTo(dstFile2 + "add.tar.gz",
		new(CompressionType.GZip) { ArchiveEncoding = new() { Default = Encoding.UTF8 } }); // ファイル名の文字化け対応
}
File.Delete(dstFile2 + "add.tar.tmp");

Console.WriteLine($"削除 {dstFile} {dstFile + "del.zip"}");
using (var archive = (IWritableArchive)ArchiveFactory.Open(dstFile))
{
	foreach (var entry in archive.Entries)
	{
		archive.RemoveEntry(entry);
		break;
	}
	archive.SaveTo(dstFile + "del.zip",
		new(CompressionType.Deflate) { ArchiveEncoding = new() { Default = Encoding.UTF8 } }); // ファイル名の文字化け対応
}

Console.WriteLine($"削除 {dstFile2} {dstFile2 + "del.tar.gz"}");
// tar を一時ファイルとして展開
using (var archive = ArchiveFactory.Open(dstFile2))
{
	archive.Entries.First().WriteToFile(dstFile2 + "del.tar.tmp",
		new ExtractionOptions() { Overwrite = true, ExtractFullPath = true, PreserveFileTime = true, PreserveAttributes = false });
}
// tar を開く
using (var archive = (IWritableArchive)ArchiveFactory.Open(dstFile2 + "del.tar.tmp"))
{
	// tar からファイル削除
	foreach (var entry in archive.Entries)
	{
		archive.RemoveEntry(entry);
		break;
	}
	// tar.zip に圧縮
	archive.SaveTo(dstFile2 + "del.tar.gz",
		new(CompressionType.GZip) { ArchiveEncoding = new() { Default = Encoding.UTF8 } }); // ファイル名の文字化け対応
}
File.Delete(dstFile2 + "del.tar.tmp");
