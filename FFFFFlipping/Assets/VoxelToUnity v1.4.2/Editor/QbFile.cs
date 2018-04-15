namespace Voxel2Unity {

	using UnityEngine;
	using System.Collections;
	using System.IO;
	using System.Collections.Generic;
	using System;
	using System.Text;


	public static class QbFile {



		public static byte[] GetQbByte (QbData _data) {

			List<byte> mainBytes = new List<byte>();

			mainBytes.AddRange(BitConverter.GetBytes(_data.Version));
			mainBytes.AddRange(BitConverter.GetBytes(_data.ColorFormat));
			mainBytes.AddRange(BitConverter.GetBytes(_data.ZAxisOrientation));
			mainBytes.AddRange(BitConverter.GetBytes(0));
			mainBytes.AddRange(BitConverter.GetBytes(_data.VisibleMask));
			mainBytes.AddRange(BitConverter.GetBytes(_data.NumMatrixes));

			switch (_data.ZAxisOrientation) {
				default:
				case 0: // Left Handed
					_data.Flip(Axis.X);
					_data.Rotate(Axis.X);
					_data.Flip(Axis.Y);
					break;
				case 1: // Right Handed 
					_data.Flip(Axis.X);
					_data.Rotate(Axis.X, true);
					break;
			}

			for (int i = 0; i < _data.NumMatrixes; i++) {

				// name
				mainBytes.Add(_data.MatrixList[i].NameLength);
				mainBytes.AddRange(Encoding.Default.GetBytes(_data.MatrixList[i].Name));

				// size
				mainBytes.AddRange(BitConverter.GetBytes(_data.MatrixList[i].SizeX));
				mainBytes.AddRange(BitConverter.GetBytes(_data.MatrixList[i].SizeY));
				mainBytes.AddRange(BitConverter.GetBytes(_data.MatrixList[i].SizeZ));

				// pos
				mainBytes.AddRange(BitConverter.GetBytes(_data.MatrixList[i].PosX));
				mainBytes.AddRange(BitConverter.GetBytes(_data.MatrixList[i].PosY));
				mainBytes.AddRange(BitConverter.GetBytes(_data.MatrixList[i].PosZ));

				// voxels
				for (int z = 0; z < _data.MatrixList[i].SizeZ; z++) {
					for (int y = 0; y < _data.MatrixList[i].SizeY; y++) {
						for (int x = 0; x < _data.MatrixList[i].SizeX; x++) {
							mainBytes.AddRange(BitConverter.GetBytes(_data.MatrixList[i].Voxels[x, y, z]));
						}
					}
				}

			}

			return mainBytes.ToArray();
		}


		public static QbData LoadQb (string path) {
			return LoadQb(Util.FileToByte(path));
		}

		public static QbData LoadQb (byte[] _data) {

			using (MemoryStream _ms = new MemoryStream(_data)) {

				using (BinaryReader _br = new BinaryReader(_ms)) {

					QbData _mainData = new QbData();


					#region --- From Wiki ---

					int index;
					int data;
					uint count;
					const uint CODEFLAG = 2;
					const uint NEXTSLICEFLAG = 6;

					_mainData.Version = _br.ReadUInt32();
					_mainData.ColorFormat = _br.ReadUInt32();
					_mainData.ZAxisOrientation = _br.ReadUInt32();
					_mainData.Compressed = _br.ReadUInt32();
					_br.ReadUInt32(); // Visible Mask
					_mainData.NumMatrixes = _br.ReadUInt32();

					_mainData.MatrixList = new List<QbData.QbMatrix>();


					for (int i = 0; i < _mainData.NumMatrixes; i++) {

						QbData.QbMatrix qm = new QbData.QbMatrix();

						// read matrix name
						int nameLength = _br.ReadByte();
						qm.Name = _br.ReadChars(nameLength).ToString(); // Name

						// read matrix size
						qm.SizeX = _br.ReadInt32();
						qm.SizeY = _br.ReadInt32();
						qm.SizeZ = _br.ReadInt32();

						// read matrix position (in this example the position is irrelevant)
						qm.PosX = _br.ReadInt32();
						qm.PosY = _br.ReadInt32();
						qm.PosZ = _br.ReadInt32();

						// create matrix and add to matrix list
						qm.Voxels = new int[qm.SizeX, qm.SizeY, qm.SizeZ];

						int x;
						int y;
						int z;

						if (_mainData.Compressed == 0) {
							for (z = 0; z < qm.SizeZ; z++) {
								for (y = 0; y < qm.SizeY; y++) {
									for (x = 0; x < qm.SizeX; x++) {
										qm.Voxels[x, y, z] = (int)_br.ReadUInt32();
									}
								}
							}
						} else {
							z = 0;
							while (z < qm.SizeZ) {

								index = 0;

								while (true) {
									data = (int)_br.ReadUInt32();

									if (data == NEXTSLICEFLAG)
										break;
									else if (data == CODEFLAG) {
										count = _br.ReadUInt32();

										data = (int)_br.ReadUInt32();

										for (int j = 0; j < count; j++) {
											x = index % qm.SizeX;
											y = index / qm.SizeX;
											index++;
											qm.Voxels[x, y, z] = data;
										}
									} else {
										x = index % qm.SizeX;
										y = index / qm.SizeX;
										index++;
										qm.Voxels[x, y, z] = data;
									}
								}

								z++;

							}
						}

						_mainData.MatrixList.Add(qm);

					}

					#endregion

					Vector3 minPos = Vector3.zero;

					for (int i = 0; i < _mainData.NumMatrixes; i++) {
						QbData.QbMatrix qm = _mainData.MatrixList[i];
						if (i == 0) {
							minPos = new Vector3(qm.PosX, qm.PosY, qm.PosZ);
						} else {
							minPos = new Vector3(
								Mathf.Min(qm.PosX, minPos.x),
								Mathf.Min(qm.PosY, minPos.y),
								Mathf.Min(qm.PosZ, minPos.z)
							);
						}
					}

					for (int i = 0; i < _mainData.NumMatrixes; i++) {
						QbData.QbMatrix qm = _mainData.MatrixList[i];
						qm.PosX -= (int)minPos.x;
						qm.PosY -= (int)minPos.y;
						qm.PosZ -= (int)minPos.z;
						_mainData.MatrixList[i] = qm;
					}


					#region --- Fix Axis Orientation ---


					switch (_mainData.ZAxisOrientation) {
						default:
						case 0: // Left Handed
							_mainData.Flip(Axis.X);
							_mainData.Rotate(Axis.X);
							_mainData.Flip(Axis.Y);
							break;
						case 1: // Right Handed 
							_mainData.Flip(Axis.X);
							_mainData.Rotate(Axis.X);
							break;
					}


					#endregion


					return _mainData;

				}
			}
		}



	}

}