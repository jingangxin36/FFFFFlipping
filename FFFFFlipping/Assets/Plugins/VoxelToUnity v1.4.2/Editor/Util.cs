namespace Voxel2Unity {

	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;

	public struct Util {



		#region --- File ---



		public static string Load (string _path) {
			try {
				StreamReader _sr = File.OpenText(_path);
				string _data = _sr.ReadToEnd();
				_sr.Close();
				return _data;
			} catch (System.Exception) {
				return "";
			}
		}



		public static void Save (string _data, string _path) {
			try {
				FileStream fs = new FileStream(_path, FileMode.Create);
				StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
				sw.Write(_data);
				sw.Close();
				fs.Close();
			} catch (System.Exception) {
				return;
			}
		}



		public static byte[] FileToByte (string path) {
			if (File.Exists(path)) {
				byte[] bytes = null;
				try {
					bytes = File.ReadAllBytes(path);
				} catch {
					return null;
				}
				return bytes;
			} else {
				return null;
			}
		}



		public static bool ByteToFile (byte[] bytes, string path) {
			try {
				string parentPath = new FileInfo(path).Directory.FullName;
				CreateFolder(parentPath);
				FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
				fs.Write(bytes, 0, bytes.Length);
				fs.Close();
				fs.Dispose();
				return true;
			} catch {
				return false;
			}
		}



		public static void CreateFolder (string _path) {
			_path = GetFullPath(_path);
			if (Directory.Exists(_path))
				return;
			string _parentPath = new FileInfo(_path).Directory.FullName;
			if (Directory.Exists(_parentPath)) {
				Directory.CreateDirectory(_path);
			} else {
				CreateFolder(_parentPath);
				Directory.CreateDirectory(_path);
			}
		}



		#endregion



		#region --- Path ---



		public static string FixPath (string _path) {
			_path = _path.Replace('\\', '/');
			_path = _path.Replace("//", "/");
			while (_path.Length > 0 && _path[0] == '/') {
				_path = _path.Remove(0, 1);
			}
			return _path;
		}



		public static string GetFullPath (string path) {
			return new FileInfo(path).FullName;
		}



		public static string RelativePath (string path) {
			path = FixPath(path);
			if (path.StartsWith("Assets")) {
				return path;
			}
			if (path.StartsWith(FixPath(Application.dataPath))) {
				return "Assets" + path.Substring(FixPath(Application.dataPath).Length);
			} else {
				return "";
			}
		}



		public static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, FixPath(paths[i]));
			}
			return FixPath(path);
		}



		public static string GetExtension (string path) {
			return Path.GetExtension(path);
		}



		public static string GetName (string path) {
			return Path.GetFileNameWithoutExtension(path);
		}



		public static string ChangeExtension (string path, string newEx) {
			return Path.ChangeExtension(path, newEx);
		}



		public static bool PathIsDirectory (string path) {
			FileAttributes attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;
		}



		#endregion



		#region --- MSG ---


		public static bool Dialog (string title, string msg, string ok, string cancel = "") {
			EditorApplication.Beep();
			PauseWatch();
			if (string.IsNullOrEmpty(cancel)) {
				bool sure = EditorUtility.DisplayDialog(title, msg, ok);
				RestartWatch();
				return sure;
			} else {
				bool sure = EditorUtility.DisplayDialog(title, msg, ok, cancel);
				RestartWatch();
				return sure;
			}
		}


		public static void ProgressBar (string title, string msg, float value) {
			value = Mathf.Clamp01(value);
			EditorUtility.DisplayProgressBar(title, msg, value);
		}


		public static void ClearProgressBar () {
			EditorUtility.ClearProgressBar();
		}


		#endregion



		#region --- Watch ---


		private static System.Diagnostics.Stopwatch TheWatch;


		public static void StartWatch () {
			TheWatch = new System.Diagnostics.Stopwatch();
			TheWatch.Start();
		}


		public static void PauseWatch () {
			if (TheWatch != null) {
				TheWatch.Stop();
			}
		}


		public static void RestartWatch () {
			if (TheWatch != null) {
				TheWatch.Start();
			}
		}


		public static double StopWatchAndGetTime () {
			if (TheWatch != null) {
				TheWatch.Stop();
				return TheWatch.Elapsed.TotalSeconds;
			}
			return 0f;
		}


		#endregion



	}


	public struct RectPacking {



		private class ItemSorter : IComparer<Item> {
			bool SortWithIndex;
			public ItemSorter (bool sortWithIndex) {
				SortWithIndex = sortWithIndex;
			}
			public int Compare (Item x, Item y) {
				return SortWithIndex ?
					x.Index.CompareTo(y.Index) :
					y.Height.CompareTo(x.Height);
			}
		}



		private struct Item {
			public int Index;
			public int X, Y;
			public int Width, Height;
			public Texture2D Texture;
		}


		private class Shelf {

			public int Y;
			public int Width;
			public int Height;
			public int[] RoomHeight;


			public bool AddItem (ref Item item, ref int width, ref int height) {

				int currentFitWidth = 0;
				int maxRoomY = 0;
				for (int i = 0; i < RoomHeight.Length; i++) {
					if (RoomHeight[i] >= item.Height) {
						// fit
						currentFitWidth++;
						maxRoomY = Mathf.Max(maxRoomY, Height - RoomHeight[i]);
						if (currentFitWidth >= item.Width) {
							item.Y = Y + maxRoomY;
							item.X = i - currentFitWidth + 1;
							// set width height
							width = Mathf.Max(width, item.X + item.Width);
							height = Mathf.Max(height, item.Y + item.Height);
							// Set room height
							for (int j = item.X; j < item.X + item.Width; j++) {
								RoomHeight[j] = Height - maxRoomY - item.Height;
							}
							return true;
						}
					} else {
						// not fit
						currentFitWidth = 0;
						maxRoomY = 0;
					}
				}
				return false;
			}

		}



		public static Rect[] PackTextures (out Texture2D texture, List<Texture2D> textures) {
			
			// Check
			if (textures.Count == 0) {
				texture = Texture2D.whiteTexture;
				return new Rect[1] { new Rect(0, 0, 1, 1) };
			}

			// Init
			int aimWidth = 0;
			int maxWidth = 0;
			List<Item> items = new List<Item>();
			for (int i = 0; i < textures.Count; i++) {
				int w = textures[i].width;
				int h = textures[i].height;
				items.Add(new Item() {
					Index = i,
					Width = w,
					Height = h,
					Texture = textures[i],
				});
				aimWidth += items[i].Width * items[i].Height;
				maxWidth = Mathf.Max(maxWidth, items[i].Width);
			}
			aimWidth = Mathf.Max(maxWidth, Mathf.CeilToInt(Mathf.Sqrt(aimWidth)));

			//Sort With Height
			items.Sort(new ItemSorter(false));

			// Pack
			int width = 0;
			int height = 0;

			List<Shelf> shelfs = new List<Shelf>();
			for (int i = 0; i < items.Count; i++) {

				// Try Add
				bool success = false;
				Item item = items[i];
				for (int j = 0; j < shelfs.Count; j++) {
					success = shelfs[j].AddItem(
						ref item, ref width, ref height
					);
					if (success) {
						items[i] = item;
						break;
					}
				}

				// Fail to Add
				if (!success) {

					// New shelf
					Shelf s = new Shelf() {
						Y = shelfs.Count == 0 ? 0 : shelfs[shelfs.Count - 1].Y + shelfs[shelfs.Count - 1].Height,
						Width = aimWidth,
						Height = items[i].Height,
						RoomHeight = new int[aimWidth],
					};
					for (int j = 0; j < aimWidth; j++) {
						s.RoomHeight[j] = s.Height;
					}
					shelfs.Add(s);

					// Add Again
					success = shelfs[shelfs.Count - 1].AddItem(
						ref item, ref width, ref height
					);
					items[i] = item;

					// Error, this shouldn't be happen...
					if (!success) {
						Debug.LogWarning("[Voxel To Unity] Failed to pack textures, use Unity default packing algorithm instead. Please contact the programmer to report the bug: QQ 1182032752, Email moenenn@163.com");
						texture = new Texture2D(1, 1);
						Rect[] _uvs = texture.PackTextures(textures.ToArray(), 0);
						return _uvs;
					}
				}

			}

			// Set Texture
			texture = new Texture2D(width, height);

			// Temp
			//texture.SetPixels(new Color[width * height]);

			for (int i = 0; i < items.Count; i++) {
				texture.SetPixels(
					items[i].X,
					items[i].Y,
					items[i].Width,
					items[i].Height,
					items[i].Texture.GetPixels()
				);
			}
			texture.Apply();

			// Sort with Index
			items.Sort(new ItemSorter(true));
			Rect[] uvs = new Rect[items.Count];
			for (int i = 0; i < items.Count; i++) {
				uvs[i] = new Rect(
					(float)items[i].X / width,
					(float)items[i].Y / height,
					(float)items[i].Width / width,
					(float)items[i].Height / height
				);
			}

			// Temp
			//uvs = texture.PackTextures(textures.ToArray(), 0);
			return uvs;
		}


	}

}
