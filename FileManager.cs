using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200033D RID: 829
	public class FileManager
	{
		// Token: 0x17000251 RID: 593
		// (get) Token: 0x060026D3 RID: 9939 RVA: 0x0010C0CE File Offset: 0x0010A2CE
		public static string aaDefaultPath
		{
			get
			{
				return FileManager.GetFullPath(FileManager.Type.AddressableAssets, FileManager.Source.Default, "");
			}
		}

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x060026D4 RID: 9940 RVA: 0x0010C0DC File Offset: 0x0010A2DC
		public static string aaModPath
		{
			get
			{
				return FileManager.GetFullPath(FileManager.Type.AddressableAssets, FileManager.Source.Mods, "");
			}
		}

		// Token: 0x060026D5 RID: 9941 RVA: 0x0010C0EC File Offset: 0x0010A2EC
		public static string GetFullPath(FileManager.Type type, FileManager.Source source, string relativePath = "")
		{
			if (Application.isEditor)
			{
				if ((source == FileManager.Source.Default || source == FileManager.Source.Mods) && type != FileManager.Type.AddressableAssets && type == FileManager.Type.JSONCatalog)
				{
					string path = Path.Combine(Application.dataPath.Replace("/Assets", ""), ThunderRoadSettings.current.catalogsEditorPath);
					if (source == FileManager.Source.Default)
					{
						path = Path.Combine(path, FileManager.defaultFolderName);
					}
					else if (source == FileManager.Source.Mods)
					{
						path = Path.Combine(path, ModManager.modFolderName);
					}
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
					return Path.Combine(path, relativePath);
				}
			}
			else if (Application.platform == RuntimePlatform.Android)
			{
				if (source == FileManager.Source.Default)
				{
					return Path.Combine("/sdcard/Android/obb/" + Application.identifier, relativePath);
				}
				if (source == FileManager.Source.Mods)
				{
					return Path.Combine(Application.persistentDataPath, ModManager.modFolderName, relativePath);
				}
				if (source == FileManager.Source.Logs)
				{
					return Path.Combine(Application.persistentDataPath, FileManager.logFolderName, relativePath);
				}
			}
			else
			{
				if (source == FileManager.Source.Default)
				{
					string pathFolder = Path.Combine(Application.streamingAssetsPath, FileManager.defaultFolderName);
					string text = Path.Combine(pathFolder, relativePath);
					if (!Directory.Exists(text))
					{
						Directory.CreateDirectory(pathFolder);
					}
					return text;
				}
				if (source == FileManager.Source.Mods)
				{
					if (Application.platform == RuntimePlatform.PS5)
					{
						return Path.Combine(Application.persistentDataPath, ModManager.modFolderName, relativePath);
					}
					string pathFolder2 = Path.Combine(Application.streamingAssetsPath, ModManager.modFolderName);
					string text2 = Path.Combine(pathFolder2, relativePath);
					if (!Directory.Exists(text2))
					{
						Directory.CreateDirectory(pathFolder2);
					}
					return text2;
				}
				else if (source == FileManager.Source.Logs)
				{
					if (Application.platform == RuntimePlatform.PS5)
					{
						return Path.Combine(Application.persistentDataPath, FileManager.logFolderName, relativePath);
					}
					string pathFolder3 = Path.Combine(Application.streamingAssetsPath, FileManager.logFolderName);
					string text3 = Path.Combine(pathFolder3, relativePath);
					if (!Directory.Exists(text3))
					{
						Directory.CreateDirectory(pathFolder3);
					}
					return text3;
				}
			}
			return null;
		}

		// Token: 0x060026D6 RID: 9942 RVA: 0x0010C277 File Offset: 0x0010A477
		public static string[] GetFolderNames(FileManager.Type type, FileManager.Source source, string localPath = "")
		{
			return Directory.GetDirectories(FileManager.GetFullPath(type, source, localPath)).Select(new Func<string, string>(Path.GetFileName)).ToArray<string>();
		}

		// Token: 0x060026D7 RID: 9943 RVA: 0x0010C29C File Offset: 0x0010A49C
		public static FileManager.ReadFile[] ReadFiles(FileManager.Type type, FileManager.Source source, string localPath = "", string searchPattern = "*.*")
		{
			string folderPath = FileManager.GetFullPath(type, source, localPath);
			if (!Directory.Exists(folderPath))
			{
				return Array.Empty<FileManager.ReadFile>();
			}
			string[] filePaths = Directory.GetFiles(folderPath, searchPattern, SearchOption.AllDirectories);
			FileManager.ReadFile[] readFiles = new FileManager.ReadFile[filePaths.Length];
			for (int i = 0; i < filePaths.Length; i++)
			{
				string filePath = filePaths[i];
				readFiles[i] = new FileManager.ReadFile(File.ReadAllText(filePath), filePath);
			}
			return readFiles;
		}

		// Token: 0x060026D8 RID: 9944 RVA: 0x0010C2F8 File Offset: 0x0010A4F8
		public static string[] GetFilePaths(FileManager.Type type, FileManager.Source source, string localPath = "", string searchPattern = "*.*")
		{
			List<string> relativePaths = new List<string>();
			string fullPath = FileManager.GetFullPath(type, source, localPath);
			foreach (string filePath in Directory.GetFiles(fullPath, searchPattern, SearchOption.AllDirectories))
			{
				string relativePath = FileManager.GetRelativePath(fullPath, filePath);
				relativePaths.Add(relativePath);
			}
			return relativePaths.ToArray();
		}

		// Token: 0x060026D9 RID: 9945 RVA: 0x0010C348 File Offset: 0x0010A548
		public static string GetRelativePath(string relativeTo, string path)
		{
			string rel = Uri.UnescapeDataString(new Uri(relativeTo).MakeRelativeUri(new Uri(path)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			if (!rel.Contains(Path.DirectorySeparatorChar.ToString()))
			{
				rel = string.Format(".{0}{1}", Path.DirectorySeparatorChar, rel);
			}
			return rel;
		}

		// Token: 0x060026DA RID: 9946 RVA: 0x0010C3AC File Offset: 0x0010A5AC
		public static string[] GetFullFilePaths(FileManager.Type type, FileManager.Source source, string localPath = "", string searchPattern = "*.*")
		{
			string fullPath = FileManager.GetFullPath(type, source, localPath);
			try
			{
				return Directory.GetFiles(fullPath, searchPattern, SearchOption.AllDirectories);
			}
			catch (DirectoryNotFoundException e)
			{
				Debug.LogWarning(string.Format("Directory Not Found: {0}", e));
			}
			return new string[0];
		}

		// Token: 0x060026DB RID: 9947 RVA: 0x0010C3F8 File Offset: 0x0010A5F8
		public static bool FileExist(FileManager.Type type, FileManager.Source source, string localPath)
		{
			return File.Exists(FileManager.GetFullPath(type, source, localPath));
		}

		// Token: 0x060026DC RID: 9948 RVA: 0x0010C407 File Offset: 0x0010A607
		public static string ReadAllText(FileManager.Type type, FileManager.Source source, string localPath)
		{
			return File.ReadAllText(FileManager.GetFullPath(type, source, localPath));
		}

		// Token: 0x04002627 RID: 9767
		public static string defaultFolderName = "Default";

		// Token: 0x04002628 RID: 9768
		public static string logFolderName = "Logs";

		// Token: 0x04002629 RID: 9769
		public static bool useObb = false;

		// Token: 0x02000A2D RID: 2605
		public class ReadFile
		{
			// Token: 0x0600457F RID: 17791 RVA: 0x00195EA7 File Offset: 0x001940A7
			public ReadFile(string text, string path)
			{
				this.text = text;
				this.path = path;
			}

			// Token: 0x0400474C RID: 18252
			public string text;

			// Token: 0x0400474D RID: 18253
			public string path;
		}

		// Token: 0x02000A2E RID: 2606
		public enum Source
		{
			// Token: 0x0400474F RID: 18255
			Default,
			// Token: 0x04004750 RID: 18256
			Mods,
			// Token: 0x04004751 RID: 18257
			Logs
		}

		// Token: 0x02000A2F RID: 2607
		public enum Type
		{
			// Token: 0x04004753 RID: 18259
			AddressableAssets,
			// Token: 0x04004754 RID: 18260
			JSONCatalog
		}
	}
}
