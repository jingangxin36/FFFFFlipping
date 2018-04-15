namespace Voxel2Unity {

	using UnityEngine;
	using System.Collections.Generic;


	public class QbData {


		public struct QbMatrix {

			public int this[int x, int y, int z] {
				get {
					return Voxels[x, y, z];
				}
			}
			public byte NameLength {
				get {
					return (byte)(string.IsNullOrEmpty(Name) ? 0 : Name.Length);
				}
			}
			public string Name;
			public int SizeX, SizeY, SizeZ;
			public int PosX, PosY, PosZ;
			public int[,,] Voxels;

		}



		public uint Version;
		public uint ColorFormat; // 0->RGBA 1->BGRA
		public uint ZAxisOrientation; // 0->Left Handed  1->Right Handed
		public uint Compressed; // 0->Normal  1->WithNumbers
		public uint NumMatrixes;
		public uint VisibleMask;


		public List<QbMatrix> MatrixList;

		public Vector3 SizeAll {
			get {
				int SizeX = 0, SizeY = 0, SizeZ = 0;
				for (int i = 0; i < MatrixList.Count; i++) {
					if (i == 0) {
						SizeX = MatrixList[i].SizeX + MatrixList[i].PosX;
						SizeY = MatrixList[i].SizeY + MatrixList[i].PosY;
						SizeZ = MatrixList[i].SizeZ + MatrixList[i].PosZ;
					} else {
						SizeX = Mathf.Max(SizeX, MatrixList[i].SizeX + MatrixList[i].PosX);
						SizeY = Mathf.Max(SizeY, MatrixList[i].SizeY + MatrixList[i].PosY);
						SizeZ = Mathf.Max(SizeZ, MatrixList[i].SizeZ + MatrixList[i].PosZ);
					}
				}
				return new Vector3(SizeX, SizeY, SizeZ);
			}
		}

		public VoxData GetVoxData () {

			VoxData vox = new VoxData();

			Vector3 sizeAll = SizeAll;
			vox.SizeX = new int[1] { (int)sizeAll.x };
			vox.SizeY = new int[1] { (int)sizeAll.y };
			vox.SizeZ = new int[1] { (int)sizeAll.z };

			vox.Voxels = new int[1][,,];
			vox.Voxels[0] = new int[vox.SizeX[0], vox.SizeY[0], vox.SizeZ[0]];
			List<Color> pl = new List<Color> {
				new Color()
			};

			for (int i = 0; i < MatrixList.Count; i++) {
				for (int x = 0; x < MatrixList[i].SizeX; x++) {
					for (int y = 0; y < MatrixList[i].SizeY; y++) {
						for (int z = 0; z < MatrixList[i].SizeZ; z++) {
							int v = MatrixList[i][x, y, z];
							if (v != 0) {
								Color color = ColorFormat == 0 ? RGB(v) : BGR(v);
								int index = pl.IndexOf(color);
								if (index == -1) {
									pl.Add(color);
									index = pl.Count - 1;
								}
								vox.Voxels[0][x + MatrixList[i].PosX, y + MatrixList[i].PosY, z + MatrixList[i].PosZ] = index + 1;
							}
						}
					}
				}
			}

			vox.Palatte = pl.ToArray();

			return vox;
		}


		public void Flip (Axis _axis) {
			Vector3 sizeAll = SizeAll;
			for (int i = 0; i < MatrixList.Count; i++) {
				QbMatrix qm = MatrixList[i];

				for (int x = 0; x < (_axis == Axis.X ? qm.SizeX * 0.5f : qm.SizeX); x++) {
					for (int y = 0; y < (_axis == Axis.Y ? qm.SizeY * 0.5f : qm.SizeY); y++) {
						for (int z = 0; z < (_axis == Axis.Z ? qm.SizeZ * 0.5f : qm.SizeZ); z++) {
							int ii = _axis == Axis.X ? (int)qm.SizeX - x - 1 : x;
							int jj = _axis == Axis.Y ? (int)qm.SizeY - y - 1 : y;
							int kk = _axis == Axis.Z ? (int)qm.SizeZ - z - 1 : z;
							int _b = qm.Voxels[x, y, z];
							qm.Voxels[x, y, z] = qm.Voxels[ii, jj, kk];
							qm.Voxels[ii, jj, kk] = _b;
						}
					}
				}
				qm.PosX = _axis == Axis.X ? (int)sizeAll.x - qm.PosX - (int)qm.SizeX : qm.PosX;
				qm.PosY = _axis == Axis.Y ? (int)sizeAll.y - qm.PosY - (int)qm.SizeY : qm.PosY;
				qm.PosZ = _axis == Axis.Z ? (int)sizeAll.z - qm.PosZ - (int)qm.SizeZ : qm.PosZ;
				MatrixList[i] = qm;

			}

		}

		public void Rotate (Axis _axis, bool reverse = false) {

			for (int index = 0; index < MatrixList.Count; index++) {

				QbMatrix qm = MatrixList[index];

				int _newSizeX = qm.SizeX;
				int _newSizeY = qm.SizeY;
				int _newSizeZ = qm.SizeZ;
				int _newPosX = qm.PosX;
				int _newPosY = qm.PosY;
				int _newPosZ = qm.PosZ;
				int[,,] _newByte = null;

				switch (_axis) {
					case Axis.X:
						_newSizeY = qm.SizeZ;
						_newSizeZ = qm.SizeY;
						_newPosY = qm.PosZ;
						_newPosZ = qm.PosY;
						_newByte = new int[_newSizeX, _newSizeY, _newSizeZ];
						for (int i = 0; i < qm.SizeX; i++) {
							for (int j = 0; j < qm.SizeY; j++) {
								for (int k = 0; k < qm.SizeZ; k++) {
									_newByte[i, k, j] = qm.Voxels[i, j, k];
								}
							}
						}

						qm.SizeY = _newSizeY;
						qm.SizeZ = _newSizeZ;
						qm.PosY = _newPosY;
						qm.PosZ = _newPosZ;
						qm.Voxels = _newByte;
						MatrixList[index] = qm;
						break;
					case Axis.Y:
						_newSizeX = qm.SizeZ;
						_newSizeZ = qm.SizeX;
						_newPosX = qm.PosZ;
						_newPosZ = qm.PosX;
						_newByte = new int[_newSizeX, _newSizeY, _newSizeZ];
						for (int i = 0; i < qm.SizeX; i++) {
							for (int j = 0; j < qm.SizeY; j++) {
								for (int k = 0; k < qm.SizeZ; k++) {
									_newByte[k, j, i] = qm.Voxels[i, j, k];
								}
							}
						}
						qm.SizeX = _newSizeX;
						qm.SizeZ = _newSizeZ;
						qm.PosX = _newPosX;
						qm.PosZ = _newPosZ;
						qm.Voxels = _newByte;
						MatrixList[index] = qm;
						break;
					case Axis.Z:
						_newSizeX = qm.SizeY;
						_newSizeY = qm.SizeX;
						_newPosX = qm.PosY;
						_newPosY = qm.PosX;
						_newByte = new int[_newSizeX, _newSizeY, _newSizeZ];
						for (int i = 0; i < qm.SizeX; i++) {
							for (int j = 0; j < qm.SizeY; j++) {
								for (int k = 0; k < qm.SizeZ; k++) {
									_newByte[j, i, k] = qm.Voxels[i, j, k];
								}
							}
						}
						qm.SizeX = _newSizeX;
						qm.SizeY = _newSizeY;
						qm.PosX = _newPosX;
						qm.PosY = _newPosY;
						qm.Voxels = _newByte;
						MatrixList[index] = qm;
						break;
				}
			}


			switch (_axis) {
				case Axis.X:
					if (reverse) {
						Flip(Axis.Z);
					} else {
						Flip(Axis.Y);
					}
					break;
				case Axis.Y:
					if (reverse) {
						Flip(Axis.X);
					} else {
						Flip(Axis.Z);
					}
					break;
				case Axis.Z:
					if (reverse) {
						Flip(Axis.Y);
					} else {
						Flip(Axis.X);
					}
					break;
			}


		}


		private Color RGB (int color) {
			int r = 0xFF & color;
			int g = 0xFF00 & color;
			g >>= 8;
			int b = 0xFF0000 & color;
			b >>= 16;
			return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f);
		}

		private Color BGR (int color) {
			int b = 0xFF & color;
			int g = 0xFF00 & color;
			g >>= 8;
			int r = 0xFF0000 & color;
			r >>= 16;
			return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f);
		}



	}



}