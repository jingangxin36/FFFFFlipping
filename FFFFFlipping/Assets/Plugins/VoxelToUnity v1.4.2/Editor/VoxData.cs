namespace Voxel2Unity {

	using UnityEngine;


	public enum VoxelFileFormat {
		Vox = 0,
		Qb = 1,
	}



	public class VoxData {

		public string Name;

		public Color[] Palatte;

		public int[] SizeX, SizeY, SizeZ;

		public byte[] Version;

		public int[][,,] Voxels;

		public int VoxelNum (int index) {
			int num = 0;
			for (int i = 0; i < SizeX[index]; i++) {
				for (int j = 0; j < SizeY[index]; j++) {
					for (int k = 0; k < SizeZ[index]; k++) {
						if (Voxels[index][i, j, k] != 0) {
							num++;
						}
					}
				}
			}
			return num;
		}

		public VoxData GetLODVoxelData (int lodLevel) {
			VoxData data = new VoxData() {
				SizeX = new int[Voxels.Length],
				SizeY = new int[Voxels.Length],
				SizeZ = new int[Voxels.Length],
				Version = Version,
				Palatte = Palatte,
				Voxels = new int[Voxels.Length][,,],
			};
			for (int index = 0; index < Voxels.Length; index++) {
				
				if (SizeX[index] <= 1 || SizeY[index] <= 1 || SizeZ[index] <= 1) {
					data.Voxels[index] = null;
					continue;
				}

				lodLevel = Mathf.Clamp(lodLevel, 0, 8);
				if (lodLevel <= 1) {
					data.SizeX[index] = SizeX[index];
					data.SizeY[index] = SizeY[index];
					data.SizeZ[index] = SizeZ[index];
					data.Voxels[index] = Voxels[index];
					continue;
				}

				if (SizeX[index] <= lodLevel && SizeY[index] <= lodLevel && SizeZ[index] <= lodLevel) {
					data.Voxels[index] = null;
					continue;
				}

				data.SizeX[index] = Mathf.Max(Mathf.CeilToInt((float)SizeX[index] / lodLevel), 1);
				data.SizeY[index] = Mathf.Max(Mathf.CeilToInt((float)SizeY[index] / lodLevel));
				data.SizeZ[index] = Mathf.Max(Mathf.CeilToInt((float)SizeZ[index] / lodLevel));
				data.Voxels[index] = new int[data.SizeX[index], data.SizeY[index], data.SizeZ[index]];
				for (int x = 0; x < data.SizeX[index]; x++) {
					for (int y = 0; y < data.SizeY[index]; y++) {
						for (int z = 0; z < data.SizeZ[index]; z++) {
							data.Voxels[index][x, y, z] = GetMajorityColorIndex(index, x * lodLevel, y * lodLevel, z * lodLevel, lodLevel);
						}
					}
				}
			}
			return data;
		}




		public void Flip (Axis _axis) {
			for (int index = 0; index < Voxels.Length; index++) {
				for (int i = 0; i < (_axis == Axis.X ? SizeX[index] * 0.5f : SizeX[index]); i++) {
					for (int j = 0; j < (_axis == Axis.Y ? SizeY[index] * 0.5f : SizeY[index]); j++) {
						for (int k = 0; k < (_axis == Axis.Z ? SizeZ[index] * 0.5f : SizeZ[index]); k++) {
							int ii = _axis == Axis.X ? SizeX[index] - i - 1 : i;
							int jj = _axis == Axis.Y ? SizeY[index] - j - 1 : j;
							int kk = _axis == Axis.Z ? SizeZ[index] - k - 1 : k;
							int _b = Voxels[index][i, j, k];
							Voxels[index][i, j, k] = Voxels[index][ii, jj, kk];
							Voxels[index][ii, jj, kk] = _b;
						}
					}
				}
			}

		}

		public void Rotate (Axis _axis, bool reverse = false) {

			int[][,,] _newByte = new int[Voxels.Length][,,];
			for (int index = 0; index < Voxels.Length; index++) {
				int _newSizeX = SizeX[index];
				int _newSizeY = SizeY[index];
				int _newSizeZ = SizeZ[index];
				switch (_axis) {
					case Axis.X:
						_newSizeY = SizeZ[index];
						_newSizeZ = SizeY[index];
						_newByte[index] = new int[_newSizeX, _newSizeY, _newSizeZ];
						for (int i = 0; i < SizeX[index]; i++) {
							for (int j = 0; j < SizeY[index]; j++) {
								for (int k = 0; k < SizeZ[index]; k++) {
									_newByte[index][i, k, j] = Voxels[index][i, j, k];
								}
							}
						}
						SizeY[index] = _newSizeY;
						SizeZ[index] = _newSizeZ;
						Voxels = _newByte;

						if (reverse) {
							Flip(Axis.Z);
						} else {
							Flip(Axis.Y);
						}


						break;
					case Axis.Y:
						_newSizeX = SizeZ[index];
						_newSizeZ = SizeX[index];
						_newByte[index] = new int[_newSizeX, _newSizeY, _newSizeZ];
						for (int i = 0; i < SizeX[index]; i++) {
							for (int j = 0; j < SizeY[index]; j++) {
								for (int k = 0; k < SizeZ[index]; k++) {
									_newByte[index][k, j, i] = Voxels[index][i, j, k];
								}
							}
						}
						SizeX[index] = _newSizeX;
						SizeZ[index] = _newSizeZ;
						Voxels = _newByte;


						if (reverse) {
							Flip(Axis.X);
						} else {
							Flip(Axis.Z);
						}

						break;
					case Axis.Z:
						_newSizeX = SizeY[index];
						_newSizeY = SizeX[index];
						_newByte[index] = new int[_newSizeX, _newSizeY, _newSizeZ];
						for (int i = 0; i < SizeX[index]; i++) {
							for (int j = 0; j < SizeY[index]; j++) {
								for (int k = 0; k < SizeZ[index]; k++) {
									_newByte[index][j, i, k] = Voxels[index][i, j, k];
								}
							}
						}
						SizeX[index] = _newSizeX;
						SizeY[index] = _newSizeY;
						Voxels = _newByte;


						if (reverse) {
							Flip(Axis.Y);
						} else {
							Flip(Axis.X);
						}

						break;
				}
			}
		}


		public int GetMajorityColorIndex (int _index, int x, int y, int z, int lodLevel) {
			x = Mathf.Min(x, SizeX[_index] - 2);
			y = Mathf.Min(y, SizeY[_index] - 2);
			z = Mathf.Min(z, SizeZ[_index] - 2);
			int cubeNum = (int)Mathf.Pow(lodLevel, 3);
			int[] index = new int[cubeNum];
			for (int i = 0; i < lodLevel; i++) {
				for (int j = 0; j < lodLevel; j++) {
					for (int k = 0; k < lodLevel; k++) {
						if (x + i > SizeX[_index] - 1 || y + j > SizeY[_index] - 1 || z + k > SizeZ[_index] - 1) {
							index[i * lodLevel * lodLevel + j * lodLevel + k] = 0;
						} else {
							index[i * lodLevel * lodLevel + j * lodLevel + k] = Voxels[_index][x + i, y + j, z + k];
						}
					}
				}
			}

			int[] numIndex = new int[cubeNum];
			int maxNum = 1;
			int maxNumIndex = 0;
			for (int i = 0; i < cubeNum; i++) {
				numIndex[i] = index[i] == 0 ? 0 : 1;
			}
			for (int i = 0; i < cubeNum; i++) {
				for (int j = 0; j < cubeNum; j++) {
					if (i != j && index[i] != 0 && index[i] == index[j]) {
						numIndex[i]++;
						if (numIndex[i] > maxNum) {
							maxNum = numIndex[i];
							maxNumIndex = i;
						}
					}
				}
			}
			return index[maxNumIndex];
		}



	}



}
