namespace Voxel2Unity {

	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class SpritePostprocessor : AssetPostprocessor {



		public class QueueData {

			public string path;
			public int width;
			public int height;
			public Vector2[] Pivots;
			public Rect[] spriteRects;

		}



		public static List<QueueData> PathQueue = new List<QueueData>();



		#region --- API ---



		public static void AddToQueue (QueueData data) {
			PathQueue.Add(data);
		}



		public static void ClearQueue () {
			PathQueue.Clear();
		}


		#endregion



		#region --- MSG ---






		void OnPostprocessTexture (Texture2D texture) {
			string path = Util.RelativePath(assetPath);
			int index = IndexFor(path);
			if (index >= 0) {

				// Impoert
				TextureImporter ti = assetImporter as TextureImporter;
				ti.isReadable = true;
				ti.alphaIsTransparency = true;
				ti.filterMode = FilterMode.Point;
				ti.mipmapEnabled = false;
				ti.textureType = TextureImporterType.Sprite;
				ti.spriteImportMode = SpriteImportMode.Multiple;
				ti.maxTextureSize = 8192;

#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
				ti.textureFormat = TextureImporterFormat.AutomaticTruecolor;
#else
				ti.textureCompression = TextureImporterCompression.Uncompressed;
#endif

				// Sprites
				Rect[] rects = PathQueue[index].spriteRects;

				List<SpriteMetaData> newData = new List<SpriteMetaData>();

				for (int i = 0; i < rects.Length; i++) {
					SpriteMetaData smd = new SpriteMetaData() {
						pivot = PathQueue[index].Pivots[i],
						alignment = 9,
						name = Util.GetName(path) + "_" + _25DSprite.SPRITE_ANGLE[i].ToString("0"), 
						rect = rects[i]
					};
					newData.Add(smd);
				}

				ti.spritesheet = newData.ToArray();

				// Final
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
				PathQueue.RemoveAt(index);

			}
		}



		int IndexFor (string path) {
			for (int i = 0; i < PathQueue.Count; i++) {
				if (PathQueue[i].path == path) {
					return i;
				}
			}
			return -1;
		}



		#endregion



	}
}
