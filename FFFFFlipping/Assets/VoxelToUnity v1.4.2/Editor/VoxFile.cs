namespace Voxel2Unity {

	using UnityEngine;
	using System.Collections;
	using System.IO;
	using System.Collections.Generic;
	using System;
	using System.Text;




	public static class VoxFile {






		private static Color[] DefaultPallete {
			get {

				Color[] c = new Color[256];
				int index = 0;
				for (int r = 10; r >= 0; r -= 2) {
					for (int g = 10; g >= 0; g -= 2) {
						for (int b = 10; b >= 0; b -= 2) {
							if (r + g + b != 0) {
								c[index] = new Color((float)r / 10f, (float)g / 10f, (float)b / 10f);
								index++;
							}
						}
					}
				}

				for (int i = 14; i > 0; i--) {
					if (i % 3 == 0) {
						continue;
					}
					c[index] = new Color((float)i / 15f, 0f, 0f);
					index++;
				}

				for (int i = 14; i > 0; i--) {
					if (i % 3 == 0) {
						continue;
					}
					c[index] = new Color(0f, (float)i / 15f, 0f);
					index++;
				}

				for (int i = 14; i > 0; i--) {
					if (i % 3 == 0) {
						continue;
					}
					c[index] = new Color(0f, 0f, (float)i / 15f);
					index++;
				}

				for (int i = 14; i > 0; i--) {
					if (i % 3 == 0) {
						continue;
					}
					c[index] = new Color((float)i / 15f, (float)i / 15f, (float)i / 15f);
					index++;
				}

				c[255] = new Color(0f, 0f, 0f, 1f);

				return c;
			}
		}


		public static byte[] GetVoxByte (VoxData _data, Color[] _palatte) {

			List<byte> _byte = new List<byte>();

			///*

			// content
			for (int index = 0; index < _data.Voxels.Length; index++) {
				// --- SIZE ---
				//size / children size
				_byte.AddRange(Encoding.Default.GetBytes("SIZE"));
				_byte.AddRange(BitConverter.GetBytes(12));
				_byte.AddRange(BitConverter.GetBytes(0));

				_byte.AddRange(BitConverter.GetBytes(_data.SizeX[index]));
				_byte.AddRange(BitConverter.GetBytes(_data.SizeY[index]));
				_byte.AddRange(BitConverter.GetBytes(_data.SizeZ[index]));

				// --- XYZI ---
				//size / children size
				_byte.AddRange(Encoding.Default.GetBytes("XYZI"));
				_byte.AddRange(BitConverter.GetBytes(_data.VoxelNum(index) * 4 + 4));
				_byte.AddRange(BitConverter.GetBytes(0));
				_byte.AddRange(BitConverter.GetBytes(_data.VoxelNum(index)));

				for (int i = 0; i < _data.SizeX[index]; i++) {
					for (int j = 0; j < _data.SizeY[index]; j++) {
						for (int k = 0; k < _data.SizeZ[index]; k++) {
							if (_data.Voxels[index][i, j, k] != 0) {
								_byte.Add((byte)i);
								_byte.Add((byte)j);
								_byte.Add((byte)k);
								_byte.Add((byte)_data.Voxels[index][i, j, k]);
							}
						}
					}
				}

			}


			// --- RGBA ---
			//size / children size
			_byte.AddRange(Encoding.Default.GetBytes("RGBA"));
			_byte.AddRange(BitConverter.GetBytes(1024));
			_byte.AddRange(BitConverter.GetBytes(0));

			for (int i = 0; i < 256; i++) {
				Color _color = i < _palatte.Length ? _palatte[i] : new Color();
				_byte.Add((byte)(_color.r * 255.0f));
				_byte.Add((byte)(_color.g * 255.0f));
				_byte.Add((byte)(_color.b * 255.0f));
				_byte.Add((byte)(_color.a * 255.0f));
			}

			//*/

			// --- Final ---
			byte[] _ans = new byte[_byte.Count];
			_byte.CopyTo(_ans);
			return _ans;
		}



		public static byte[] GetMainByte (VoxData _voxData) {

			List<byte> _byte = new List<byte>();

			///*

			// "VOX "
			_byte.AddRange(Encoding.Default.GetBytes("VOX "));

			// "VERSION "
			_byte.AddRange(_voxData.Version);

			// ID --> MAIN
			_byte.AddRange(Encoding.Default.GetBytes("MAIN"));

			// Main Chunk Size
			_byte.AddRange(BitConverter.GetBytes(0));

			// Main Chunk Children Size
			byte[] _vox = GetVoxByte(_voxData, _voxData.Palatte);
			_byte.AddRange(BitConverter.GetBytes(_vox.Length));

			// Vox Chunk
			_byte.AddRange(_vox);

			//*/

			byte[] _ans = new byte[_byte.Count];
			_byte.CopyTo(_ans);
			return _ans;
		}


		public static VoxData LoadVoxel (string path) {
			return LoadVoxel(Util.FileToByte(path));
		}


		public static VoxData LoadVoxel (byte[] _data) {

			if (_data[0] != 'V' || _data[1] != 'O' || _data[2] != 'X' || _data[3] != ' ') {
				Debug.LogError("Error Magic Number");
				return null;
			}

			using (MemoryStream _ms = new MemoryStream(_data)) {

				using (BinaryReader _br = new BinaryReader(_ms)) {

					VoxData _mainData = new VoxData();
					List<int[,,]> tempVoxels = new List<int[,,]>();
					List<Vector3> tempSizes = new List<Vector3>();
					///*

					// VOX_
					_br.ReadInt32();

					// VERSION
					_mainData.Version = _br.ReadBytes(4);

					byte[] _chunkId = _br.ReadBytes(4);
					if (_chunkId[0] != 'M' || _chunkId[1] != 'A' || _chunkId[2] != 'I' || _chunkId[3] != 'N') {
						Debug.LogError("Error main ID");
						return null;
					}

					int _chunkSize = _br.ReadInt32();
					int _childrenSize = _br.ReadInt32();

					_br.ReadBytes(_chunkSize);

					int _readSize = 0;
					while (_readSize < _childrenSize) {
						_chunkId = _br.ReadBytes(4);
						if (_chunkId[0] == 'S' && _chunkId[1] == 'I' && _chunkId[2] == 'Z' && _chunkId[3] == 'E') {

							_readSize += ReadSizeChunk(_br, ref tempSizes);

						} else if (_chunkId[0] == 'X' && _chunkId[1] == 'Y' && _chunkId[2] == 'Z' && _chunkId[3] == 'I') {

							_readSize += ReadVoxelChunk(_br, _mainData, tempSizes, ref tempVoxels);

						} else if (_chunkId[0] == 'R' && _chunkId[1] == 'G' && _chunkId[2] == 'B' && _chunkId[3] == 'A') {

							_mainData.Palatte = new Color[256];
							_readSize += ReadPalattee(_br, _mainData.Palatte);

						} else {
							int chunkSize = _br.ReadInt32();
							int childrenSize = _br.ReadInt32();
							_br.ReadBytes(chunkSize + childrenSize);
							_readSize += chunkSize + childrenSize + 4 + 4 + 4;
						}
					}

					// Add Voxels
					_mainData.Voxels = tempVoxels.ToArray();

					// Add Sizes
					_mainData.SizeX = new int[tempSizes.Count];
					_mainData.SizeY = new int[tempSizes.Count];
					_mainData.SizeZ = new int[tempSizes.Count];
					for (int i = 0; i < tempSizes.Count; i++) {
						_mainData.SizeX[i] = (int)tempSizes[i].x;
						_mainData.SizeY[i] = (int)tempSizes[i].y;
						_mainData.SizeZ[i] = (int)tempSizes[i].z;
					}

					if (_mainData.Palatte == null) {
						_mainData.Palatte = DefaultPallete;
					}

					return _mainData;
				}
			}
		}






		static int ReadSizeChunk (BinaryReader _br, ref List<Vector3> sizeList) {
			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();
			int x = _br.ReadInt32();
			int y = _br.ReadInt32();
			int z = _br.ReadInt32();
			sizeList.Add(new Vector3(x, y, z));
			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}
			return chunkSize + childrenSize + 4 * 3;
		}




		static int ReadVoxelChunk (BinaryReader _br, VoxData _mainData, List<Vector3> tempSize, ref List<int[,,]> tempVoxels) {
			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();
			int numVoxels = _br.ReadInt32();
			int[,,] temp = new int[
				(int)tempSize[tempVoxels.Count].x,
				(int)tempSize[tempVoxels.Count].y,
				(int)tempSize[tempVoxels.Count].z
			];

			for (int i = 0; i < numVoxels; ++i) {
				int x = (int)_br.ReadByte();
				int y = (int)_br.ReadByte();
				int z = (int)_br.ReadByte();
				temp[x, y, z] = _br.ReadByte();
			}

			tempVoxels.Add(temp);

			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}

			return chunkSize + childrenSize + 4 * 3;
		}




		static int ReadPalattee (BinaryReader _br, Color[] _colors) {
			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();
			for (int i = 0; i < 256; ++i) {
				_colors[i] = new Color((float)_br.ReadByte() / 255.0f, (float)_br.ReadByte() / 255.0f, (float)_br.ReadByte() / 255.0f, (float)_br.ReadByte() / 255.0f);
			}
			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}
			return chunkSize + childrenSize + 4 * 3;
		}






	}

}