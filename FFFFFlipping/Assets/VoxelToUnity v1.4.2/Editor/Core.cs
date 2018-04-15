namespace Voxel2Unity {
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using UnityEditor;


	public enum Direction {
		Up = 0,
		Down = 1,
		Front = 2,
		Back = 3,
		Left = 4,
		Right = 5,
	}


	public enum Axis {
		X = 0,
		Y = 1,
		Z = 2,
	}




	public struct Voxel {

		public int ColorIndex {
			get {
				return colorIndex;
			}
			set {
				colorIndex = value;
			}
		}
		public bool IsEmpty {
			get {
				return ColorIndex == 0;
			}
		}

		public bool IsVisible {
			get {
				return visible != null && visible.Length > 5 && (visible[0] || visible[1] || visible[2] || visible[3] || visible[4] || visible[5]);
			}
			set {
				visible[0] = value;
				visible[1] = value;
				visible[2] = value;
				visible[3] = value;
				visible[4] = value;
				visible[5] = value;
			}
		}
		public bool AllVisible {
			get {
				return visible != null && visible.Length > 5 && (visible[0] && visible[1] && visible[2] && visible[3] && visible[4] && visible[5]);
			}
			set {
				visible[0] = value;
				visible[1] = value;
				visible[2] = value;
				visible[3] = value;
				visible[4] = value;
				visible[5] = value;
			}
		}

		public bool VisibleLeft {
			get {
				return visible[(int)Direction.Left];
			}
			set {
				visible[(int)Direction.Left] = value;
			}
		}
		public bool VisibleRight {
			get {
				return visible[(int)Direction.Right];
			}
			set {
				visible[(int)Direction.Right] = value;
			}
		}
		public bool VisibleUp {
			get {
				return visible[(int)Direction.Up];
			}
			set {
				visible[(int)Direction.Up] = value;
			}
		}
		public bool VisibleDown {
			get {
				return visible[(int)Direction.Down];
			}
			set {
				visible[(int)Direction.Down] = value;
			}
		}
		public bool VisibleFront {
			get {
				return visible[(int)Direction.Front];
			}
			set {
				visible[(int)Direction.Front] = value;
			}
		}
		public bool VisibleBack {
			get {
				return visible[(int)Direction.Back];
			}
			set {
				visible[(int)Direction.Back] = value;
			}
		}
		public bool[] Visible {
			get {
				return visible;
			}
		}

		private bool[] visible;
		private int colorIndex;

		public void Init () {
			colorIndex = 0;
			visible = new bool[6] { false, false, false, false, false, false };
		}

	}


	public struct VoxelFace {

		public Vector3 MaxPoint {
			get {
				switch (direction) {
					case Direction.Left:
					case Direction.Right:
						return new Vector3(X, Y + Width, Z + Height);
					case Direction.Front:
					case Direction.Back:
						return new Vector3(X + Width, Y, Z + Height);
					case Direction.Up:
					case Direction.Down:
						return new Vector3(X + Width, Y + Height, Z);
					default:
						return Vector3.zero;
				}
			}
		}
		public Vector2 MaxPoints2D {
			get {
				switch (direction) {
					case Direction.Left:
					case Direction.Right:
						return new Vector2(Y + Width, Z + Height);
					case Direction.Front:
					case Direction.Back:
						return new Vector2(X + Width, Z + Height);
					case Direction.Up:
					case Direction.Down:
						return new Vector2(X + Width, Y + Height);
					default:
						return Vector2.zero;
				}
			}
		}
		public Vector3[] Points {
			get {
				return new Vector3[4] {
				new Vector3(X, Z, Y),
				direction == Direction.Left || direction == Direction.Right ?
				new Vector3(X, Z, Y + Width) : new Vector3(X + Width, Z, Y),
				direction == Direction.Up || direction == Direction.Down ?
				new Vector3(X, Z, Y + Height) : new Vector3(X, Z+ Height, Y),
				direction == Direction.Left || direction == Direction.Right ?
				new Vector3(X, Z + Height ,Y + Width ):
				direction == Direction.Up || direction == Direction.Down ?
				new Vector3(X + Width, Z, Y + Height):
				new Vector3(X + Width, Z + Height, Y)
			};
			}
		}
		public int VoxelX {
			get {
				return direction == Direction.Right ? X - 1 : X;
			}
		}
		public int VoxelY {
			get {
				return direction == Direction.Back ? Y - 1 : Y;
			}
		}
		public int VoxelZ {
			get {
				return direction == Direction.Up ? Z - 1 : Z;
			}
		}

		public int X, Y, Z;
		public int Width;
		public int Height;
		public Direction direction;

		public VoxelFace (int x, int y, int z, int w, int h, Direction d) {
			X = d == Direction.Right ? x + 1 : x;
			Y = d == Direction.Back ? y + 1 : y;
			Z = d == Direction.Up ? z + 1 : z;
			Width = w;
			Height = h;
			direction = d;
		}

	}


	public class VoxelMesh {

		private Voxel[,,] Voxels;
		private VoxData MainVoxelData;
		private List<VoxelFace> VoxelFaces = new List<VoxelFace>();
		private Vector3 Pivot = new Vector3(0.5f, 0f, 0.5f);
		private int SizeX = 0, SizeY = 0, SizeZ = 0;
		private float Scale = 0.01f;
		private int MaxFacesInOneMesh = 16200;


		public void CreateVoxelMesh (
			VoxData voxelData, int frame, float scale, Vector3 pivot, bool lightMapSupport,
			ref Mesh[] meshs, ref Texture2D texture
		) {

			// Init Voxels
			InitVoxels(voxelData, frame, scale, pivot);

			// Visibility
			FixVisible();

			// Faces
			Getfaces();

			// Mesh
			CreateMesh(voxelData, lightMapSupport, ref meshs, ref texture);

		}



		public void CreateVoxelMesh (
			VoxData voxelData, int frame, float scale, Vector3 pivot, bool lightMapSupport,
			ref Texture2D texture, ref string objFile
		) {

			// Init Voxels
			InitVoxels(voxelData, frame, scale, pivot);

			// Visibility
			FixVisible();

			// Faces
			Getfaces();

			// Mesh
			Mesh[] meshs = null;
			CreateMesh(voxelData, lightMapSupport, ref meshs, ref texture);

			// Obj
			objFile = CreateObj(meshs, objFile);

		}


		private void InitVoxels (VoxData voxelData, int frame, float scale, Vector3 pivot) {
			Scale = scale;
			Pivot = pivot;
			SizeX = voxelData.SizeX[frame];
			SizeY = voxelData.SizeY[frame];
			SizeZ = voxelData.SizeZ[frame];
			MainVoxelData = voxelData;
			Voxels = new Voxel[SizeX, SizeY, SizeZ];
			for (int i = 0; i < SizeX; i++) {
				for (int j = 0; j < SizeY; j++) {
					for (int k = 0; k < SizeZ; k++) {
						Voxels[i, j, k].Init();
						Voxels[i, j, k].ColorIndex = MainVoxelData.Voxels[frame][i, j, k];
					}
				}
			}
		}


		private void FixVisible () {
			for (int i = 0; i < SizeX; i++) {
				for (int j = 0; j < SizeY; j++) {
					for (int k = 0; k < SizeZ; k++) {
						if (Voxels[i, j, k].IsEmpty) {
							Voxels[i, j, k].IsVisible = true;
							continue;
						}
						Voxels[i, j, k].VisibleLeft = i > 0 ? Voxels[i - 1, j, k].IsEmpty : true;
						Voxels[i, j, k].VisibleRight = i < SizeX - 1 ? Voxels[i + 1, j, k].IsEmpty : true;
						Voxels[i, j, k].VisibleFront = j > 0 ? Voxels[i, j - 1, k].IsEmpty : true;
						Voxels[i, j, k].VisibleBack = j < SizeY - 1 ? Voxels[i, j + 1, k].IsEmpty : true;
						Voxels[i, j, k].VisibleDown = k > 0 ? Voxels[i, j, k - 1].IsEmpty : true;
						Voxels[i, j, k].VisibleUp = k < SizeZ - 1 ? Voxels[i, j, k + 1].IsEmpty : true;
					}
				}
			}

		}


		private void Getfaces () {

			VoxelFaces.Clear();

			bool[,,,] isFixed = new bool[SizeX, SizeY, SizeZ, 6];

			int unFixedNum = SizeX * SizeY * SizeZ * 6;

			while (unFixedNum > 0) {
				for (int x = 0; x < SizeX; x++) {
					for (int y = 0; y < SizeY; y++) {
						for (int z = 0; z < SizeZ; z++) {
							for (int facing = 0; facing < 6; facing++) {

								if (isFixed[x, y, z, facing]) {
									continue;
								}

								if (Voxels[x, y, z].IsEmpty || !Voxels[x, y, z].Visible[facing]) {
									isFixed[x, y, z, facing] = true;
									unFixedNum--;
									continue;
								}

								isFixed[x, y, z, facing] = true;
								unFixedNum--;

								int minX = x, minY = y, minZ = z;
								int maxX = x, maxY = y, maxZ = z;

								#region --- Moving ---

								for (int moving = 0; moving < 6; moving++) {
									if (moving / 2 != facing / 2) {
										bool done = false;
										int temp;
										switch ((Direction)moving) {
											default:
												break;
											case Direction.Front:
												temp = minY;
												while (!done && minY > 0) {
													temp--;
													for (int i = minX; !done && i <= maxX; i++) {
														for (int k = minZ; k <= maxZ; k++) {
															if (isFixed[i, temp, k, facing] || Voxels[i, temp, k].IsEmpty || !Voxels[i, temp, k].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														minY = temp;
														for (int i = minX; i <= maxX; i++) {
															for (int k = minZ; k <= maxZ; k++) {
																if (!isFixed[i, temp, k, facing]) {
																	isFixed[i, temp, k, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
											case Direction.Back:
												temp = maxY;
												while (!done && maxY < SizeY - 1) {
													temp++;
													for (int i = minX; !done && i <= maxX; i++) {
														for (int k = minZ; k <= maxZ; k++) {
															if (isFixed[i, temp, k, facing] || Voxels[i, temp, k].IsEmpty || !Voxels[i, temp, k].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														maxY = temp;
														for (int i = minX; i <= maxX; i++) {
															for (int k = minZ; k <= maxZ; k++) {
																if (!isFixed[i, temp, k, facing]) {
																	isFixed[i, temp, k, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
											case Direction.Left:
												temp = minX;
												while (!done && minX > 0) {
													temp--;
													for (int j = minY; !done && j <= maxY; j++) {
														for (int k = minZ; k <= maxZ; k++) {
															if (isFixed[temp, j, k, facing] || Voxels[temp, j, k].IsEmpty || !Voxels[temp, j, k].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														minX = temp;
														for (int j = minY; j <= maxY; j++) {
															for (int k = minZ; k <= maxZ; k++) {
																if (!isFixed[temp, j, k, facing]) {
																	isFixed[temp, j, k, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
											case Direction.Right:
												temp = maxX;
												while (!done && maxX < SizeX - 1) {
													temp++;
													for (int j = minY; !done && j <= maxY; j++) {
														for (int k = minZ; k <= maxZ; k++) {
															if (isFixed[temp, j, k, facing] || Voxels[temp, j, k].IsEmpty || !Voxels[temp, j, k].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														maxX = temp;
														for (int j = minY; j <= maxY; j++) {
															for (int k = minZ; k <= maxZ; k++) {
																if (!isFixed[temp, j, k, facing]) {
																	isFixed[temp, j, k, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
											case Direction.Up:
												temp = maxZ;
												while (!done && maxZ < SizeZ - 1) {
													temp++;
													for (int i = minX; !done && i <= maxX; i++) {
														for (int j = minY; j <= maxY; j++) {
															if (isFixed[i, j, temp, facing] || Voxels[i, j, temp].IsEmpty || !Voxels[i, j, temp].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														maxZ = temp;
														for (int i = minX; i <= maxX; i++) {
															for (int j = minY; j <= maxY; j++) {
																if (!isFixed[i, j, temp, facing]) {
																	isFixed[i, j, temp, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
											case Direction.Down:
												temp = minZ;
												while (!done && minZ > 0) {
													temp--;
													for (int i = minX; !done && i <= maxX; i++) {
														for (int j = minY; j <= maxY; j++) {
															if (isFixed[i, j, temp, facing] || Voxels[i, j, temp].IsEmpty || !Voxels[i, j, temp].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														minZ = temp;
														for (int i = minX; i <= maxX; i++) {
															for (int j = minY; j <= maxY; j++) {
																if (!isFixed[i, j, temp, facing]) {
																	isFixed[i, j, temp, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
										}
									}
								}

								#endregion

								// Add The Face

								VoxelFaces.Add(new VoxelFace(
									minX, minY, minZ,
									(Direction)facing == Direction.Left || (Direction)facing == Direction.Right ? maxY - minY + 1 : maxX - minX + 1,
									(Direction)facing == Direction.Up || (Direction)facing == Direction.Down ? maxY - minY + 1 : maxZ - minZ + 1,
									(Direction)facing
								));

							}
						}
					}
				}
			}

		}


		private void CreateMesh (VoxData voxelData, bool lightMapSupport, ref Mesh[] meshs, ref Texture2D aimTexture) {

			int num = VoxelFaces.Count / MaxFacesInOneMesh + 1;
			if (VoxelFaces.Count % MaxFacesInOneMesh == 0) {
				num--;
			}

			meshs = new Mesh[num];
			List<Dictionary<int, int>> uvIdMaps = new List<Dictionary<int, int>>();
			List<Texture2D[]> temptxtList = new List<Texture2D[]>();

			for (int index = 0; index < num; index++) {

				List<VoxelFace> Faces = new List<VoxelFace>(VoxelFaces.GetRange(index * MaxFacesInOneMesh, Mathf.Min(MaxFacesInOneMesh, VoxelFaces.Count - index * MaxFacesInOneMesh)));


				#region --- Vertices ---

				List<Vector3> voxelVertices = new List<Vector3>();

				for (int i = 0; i < Faces.Count; i++) {
					Vector3[] points = Faces[i].Points;
					for (int j = 0; j < 4; j++) {
						points[j] -= new Vector3(SizeX * Pivot.x, SizeZ * Pivot.y, SizeY * Pivot.z);
						points[j] *= Scale;
					}
					voxelVertices.AddRange(points);
				}

				#endregion


				#region --- Triangles ---

				List<int> voxelTriangles = new List<int>();

				for (int i = 0; i < Faces.Count; i++) {
					if (Faces[i].direction == Direction.Front || Faces[i].direction == Direction.Up || Faces[i].direction == Direction.Right) {
						voxelTriangles.AddRange(new int[6] {
						i * 4 + 2,
						i * 4 + 1,
						i * 4 + 0,
						i * 4 + 3,
						i * 4 + 1,
						i * 4 + 2
					});
					} else {
						voxelTriangles.AddRange(new int[6] {
						i * 4 + 2,
						i * 4 + 0,
						i * 4 + 1,
						i * 4 + 3,
						i * 4 + 2,
						i * 4 + 1
					});
					}
				}

				#endregion


				meshs[index] = new Mesh();
				meshs[index].Clear();
				meshs[index].vertices = voxelVertices.ToArray();
				meshs[index].triangles = voxelTriangles.ToArray();


				#region --- Texture ---


				List<Texture2D> tempTextureList = new List<Texture2D>();
				List<int> tempColorIndexs = new List<int>();
				Dictionary<int, int> UVidMap = new Dictionary<int, int>();


				for (int i = 0; i < Faces.Count; i++) {
					int width = Faces[i].Width + 2;
					int height = Faces[i].Height + 2;

					Color[] _colors = new Color[width * height];

					bool sameIndex = !lightMapSupport;
					int currentIndex = -1;

					for (int u = 0; u < Faces[i].Width; u++) {
						for (int v = 0; v < Faces[i].Height; v++) {
							int colorIndex = 0;
							switch (Faces[i].direction) {
								case Direction.Front:
								case Direction.Back:
									colorIndex = Voxels[u + Faces[i].VoxelX, Faces[i].VoxelY, v + Faces[i].VoxelZ].ColorIndex - 1;
									break;
								case Direction.Left:
								case Direction.Right:
									colorIndex = Voxels[Faces[i].VoxelX, u + Faces[i].VoxelY, v + Faces[i].VoxelZ].ColorIndex - 1;
									break;
								case Direction.Up:
								case Direction.Down:
									colorIndex = Voxels[u + Faces[i].VoxelX, v + Faces[i].VoxelY, Faces[i].VoxelZ].ColorIndex - 1;
									break;
							}
							Color _c = MainVoxelData.Palatte[colorIndex];
							_colors[(v + 1) * width + u + 1] = _c;


							if (currentIndex == -1) {
								currentIndex = colorIndex;
							}
							if (sameIndex && currentIndex != colorIndex) {
								sameIndex = false;
							}


							#region --- Side ---

							if (u == 0) {
								_colors[(v + 1) * width + u] = _c;
							}

							if (u == Faces[i].Width - 1) {
								_colors[(v + 1) * width + u + 2] = _c;
							}

							if (v == 0) {
								_colors[v * width + u + 1] = _c;
							}

							if (v == Faces[i].Height - 1) {
								_colors[(v + 2) * width + u + 1] = _c;
							}

							if (u == 0 && v == 0) {
								_colors[0] = _c;
							}

							if (u == 0 && v == Faces[i].Height - 1) {
								_colors[(v + 2) * width + u] = _c;
							}

							if (u == Faces[i].Width - 1 && v == 0) {
								_colors[v * width + u + 2] = _c;
							}

							if (u == Faces[i].Width - 1 && v == Faces[i].Height - 1) {
								_colors[(v + 2) * width + u + 2] = _c;
							}

							#endregion

						}
					}

					Texture2D _texture = null;
					int oldID = i;
					if (sameIndex && currentIndex > 0) {
						if (tempColorIndexs.Contains(currentIndex)) {
							oldID = tempColorIndexs.IndexOf(currentIndex);
						} else {
							Color c = MainVoxelData.Palatte[currentIndex];
							_texture = new Texture2D(3, 3, TextureFormat.ARGB32, false, false);
							_texture.SetPixels(new Color[9] { c, c, c, c, c, c, c, c, c });
							tempTextureList.Add(_texture);
							tempColorIndexs.Add(currentIndex);
							oldID = tempTextureList.Count - 1;
						}
					} else {
						_texture = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
						_texture.SetPixels(_colors);
						tempTextureList.Add(_texture);
						tempColorIndexs.Add(-1);
						oldID = tempTextureList.Count - 1;
					}
					if (_texture) {
						_texture.wrapMode = TextureWrapMode.Clamp;
						_texture.filterMode = FilterMode.Point;
						_texture.mipMapBias = 0f;
						_texture.Apply();
					}
					UVidMap.Add(i, oldID);

				}

				temptxtList.Add(tempTextureList.ToArray());
				tempTextureList.Clear();
				uvIdMaps.Add(UVidMap);

				#endregion


			}

			// Combine Textures && Reset UV

			List<Texture2D> txtList = new List<Texture2D>();
			for (int i = 0; i < temptxtList.Count; i++) {
				txtList.AddRange(temptxtList[i]);
			}


			//aimTexture = new Texture2D(1, 1);
			//Rect[] tempUVs = aimTexture.PackTextures(txtList.ToArray(), 0);
			Rect[] tempUVs = RectPacking.PackTextures(out aimTexture, txtList);

			CutTexture(ref aimTexture, ref tempUVs);

			List<Rect> aimUVs = new List<Rect>(tempUVs);

			aimTexture.wrapMode = TextureWrapMode.Clamp;
			aimTexture.filterMode = FilterMode.Point;
			aimTexture.mipMapBias = 0f;
			aimTexture.Apply();


			#region --- UV ---

			int rectIndexOffset = 0;
			for (int index = 0; index < num; index++) {

				List<VoxelFace> Faces = new List<VoxelFace>(VoxelFaces.GetRange(
					index * MaxFacesInOneMesh,
					Mathf.Min(MaxFacesInOneMesh, VoxelFaces.Count - index * MaxFacesInOneMesh)
				));

				Rect[] rects = aimUVs.GetRange(
					rectIndexOffset,
					temptxtList[index].Length
				).ToArray();

				float gapX = 1f / aimTexture.width;
				float gapY = 1f / aimTexture.height;

				Vector2[] voxelUV = new Vector2[Faces.Count * 4];

				for (int i = 0; i < Faces.Count; i++) {
					int rectID = uvIdMaps[index][i];
					if (rectID >= rects.Length) {
						continue;
					}
					voxelUV[i * 4 + 0] = new Vector2(rects[rectID].xMin + gapX, rects[rectID].yMin + gapY);
					voxelUV[i * 4 + 1] = new Vector2(rects[rectID].xMax - gapX, rects[rectID].yMin + gapY);
					voxelUV[i * 4 + 2] = new Vector2(rects[rectID].xMin + gapX, rects[rectID].yMax - gapY);
					voxelUV[i * 4 + 3] = new Vector2(rects[rectID].xMax - gapX, rects[rectID].yMax - gapY);
				}
				uvIdMaps[index].Clear();

				rectIndexOffset += temptxtList[index].Length;

				meshs[index].uv = voxelUV;
				meshs[index].RecalculateNormals();

			}

			uvIdMaps.Clear();
			aimUVs.Clear();

			#endregion


		}



		private string CreateObj (Mesh[] meshs, string name) {

			StringBuilder sb = new StringBuilder();
			sb.Append("mtllib " + name + ".mtl\n");
			sb.Append("usemtl " + name + "\n");

			int offset = 0;

			for (int index = 0; index < meshs.Length; index++) {

				Vector3[] vs = meshs[index].vertices;
				Vector2[] uvs = meshs[index].uv;
				int[] trs = meshs[index].triangles;

				sb.Append("g " + name + "_" + index.ToString() + "_\n");

				foreach (Vector3 v in vs) {
					sb.Append(string.Format("v {0} {1} {2}\n", -v.x, v.y, v.z));
				}
				sb.Append("\n");

				foreach (Vector3 v in uvs) {
					sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
				}
				sb.Append("\n");

				for (int i = 0; i < trs.Length; i += 3) {
					sb.Append(string.Format("f {2}/{2} {1}/{1} {0}/{0}\n",
						trs[i] + 1 + offset, trs[i + 1] + 1 + offset, trs[i + 2] + 1 + offset));
				}
				sb.Append("\n");

				offset += vs.Length;

			}
			return sb.ToString();
		}



		public static void CutTexture (ref Texture2D texture, ref Rect[] rects) {

			if (texture == null) {
				return;
			}

			float scaleX = 0f;
			float scaleY = 0f;

			for (int i = 0; i < rects.Length; i++) {
				scaleX = Mathf.Max(scaleX, rects[i].xMax);
				scaleY = Mathf.Max(scaleY, rects[i].yMax);
			}

			for (int i = 0; i < rects.Length; i++) {
				rects[i] = new Rect(
					rects[i].x / scaleX,
					rects[i].y / scaleY,
					rects[i].width / scaleX,
					rects[i].height / scaleY
				);
			}

			int newWidth = Mathf.CeilToInt(texture.width * scaleX);
			int newHeight = Mathf.CeilToInt(texture.height * scaleY);

			Texture2D newTexture = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false, false);
			newTexture.SetPixels(texture.GetPixels(0, 0, newWidth, newHeight));
			newTexture.wrapMode = TextureWrapMode.Clamp;
			newTexture.filterMode = FilterMode.Point;
			newTexture.mipMapBias = 0f;
			newTexture.Apply();
			texture = newTexture;

		}



	}


	public class _25DSprite {



		#region --- SUB ---


		public class SpriteResult {
			public int Width;
			public int Height;
			public Texture2D Texture;
			public Vector2[] Pivots;
			public Rect[] Rects;
		}


		private struct VoxelFace {
			public Vector3 Position;
			public Vector3 Normal;
			public Color Color;
			public FaceType Type;

			public static FaceType GetFaceType (int spriteAngleIndex, Direction dir, float normalX) {
				int type = dir == Direction.Up ? 0 : normalX < 0f ? 4 : 8;
				type +=
					spriteAngleIndex >= 0 && spriteAngleIndex <= 3 ? 2 :
					(spriteAngleIndex >= 4 && spriteAngleIndex <= 7 ? 0 :
					(spriteAngleIndex >= 8 && spriteAngleIndex <= 11 ? 1 : 3));
				if (type == (int)FaceType.Left_0) {
					type = (int)FaceType.Front_0;
				}
				return (FaceType)type;
			}

		}


		private struct Pixel {
			public Color Color;
			public int X;
			public int Y;
		}


		private class FaceSorter : IComparer<VoxelFace> {
			public int Compare (VoxelFace x, VoxelFace y) {
				return y.Position.z.CompareTo(x.Position.z);
			}
		}


		public enum FaceType {
			Up_0 = 0,
			Up_22 = 1,
			Up_45 = 2,
			Up_67 = 3,
			Left_0 = 4,
			Left_22 = 5,
			Left_45 = 6,
			Left_67 = 7,
			Front_0 = 8,
			Front_22 = 9,
			Front_45 = 10,
			Front_67 = 11,
		}



		#endregion



		#region --- VAR ---



		public static readonly float[] SPRITE_ANGLE = new float[] {
			45f, 135f, 225f, 315f,
			0f, 90f, 180f, 270f,
			22.5f, 112.5f, 202.5f, 292.5f,
			67.5f, 157.5f, 247.5f, 337.5f,
		};
		public static readonly float[] SPRITE_ANGLE_SIMPLE = new float[] {
			45f, 135f, 225f, 315f,
			0f, 90.001f, 180f, 270.001f,
			22.5f, 112.5f, 202.5f, 292.5f,
			67.5f, 157.5f, 247.5f, 337.5f,
		};
		private static readonly Vector3[] VOX_CENTER_OFFSET = new Vector3[] {
			Vector3.up,
			Vector3.down,
			Vector3.back ,
			Vector3.forward,
			Vector3.left,
			Vector3.right,
		};
		private const int MAX_ROOM_NUM = 80 * 80 * 80;
		private static readonly float CAMERA_ANGLE = 35f;



		#endregion




		public static SpriteResult CreateSprite (
			VoxData voxelData, int frame, Vector3 pivot, int spriteNum, float lightIntensity, bool simple, bool spriteOutline
		) {

			// Voxels
			Voxel[,,] voxels = GetVoxels(voxelData, frame);

			// Colorss
			int[] widths;
			int[] heights;
			Vector2[] pivots;
			Color[][] colorss = GetColorss(
				voxels, voxelData.Palatte,
				pivot, spriteNum, lightIntensity, simple, spriteOutline,
				out widths, out heights, out pivots
			);

			// Result
			SpriteResult result = PackTextures(colorss, widths, heights, pivots);

			return result;
		}



		private static Voxel[,,] GetVoxels (VoxData voxelData, int frame) {
			Voxel[,,] voxels = new Voxel[voxelData.SizeX[frame], voxelData.SizeY[frame], voxelData.SizeZ[frame]];
			for (int i = 0; i < voxelData.SizeX[frame]; i++) {
				for (int j = 0; j < voxelData.SizeY[frame]; j++) {
					for (int k = 0; k < voxelData.SizeZ[frame]; k++) {
						voxels[i, j, k].Init();
						voxels[i, j, k].ColorIndex = voxelData.Voxels[frame][i, j, k];
					}
				}
			}
			for (int i = 0; i < voxelData.SizeX[frame]; i++) {
				for (int j = 0; j < voxelData.SizeY[frame]; j++) {
					for (int k = 0; k < voxelData.SizeZ[frame]; k++) {
						if (voxels[i, j, k].IsEmpty) {
							voxels[i, j, k].IsVisible = true;
							continue;
						}
						voxels[i, j, k].VisibleLeft = i > 0 ? voxels[i - 1, j, k].IsEmpty : true;
						voxels[i, j, k].VisibleRight = i < voxelData.SizeX[frame] - 1 ? voxels[i + 1, j, k].IsEmpty : true;
						voxels[i, j, k].VisibleFront = j > 0 ? voxels[i, j - 1, k].IsEmpty : true;
						voxels[i, j, k].VisibleBack = j < voxelData.SizeY[frame] - 1 ? voxels[i, j + 1, k].IsEmpty : true;
						voxels[i, j, k].VisibleDown = k > 0 ? voxels[i, j, k - 1].IsEmpty : true;
						voxels[i, j, k].VisibleUp = k < voxelData.SizeZ[frame] - 1 ? voxels[i, j, k + 1].IsEmpty : true;
					}
				}
			}
			return voxels;
		}



		private static Color[][] GetColorss (
			Voxel[,,] voxels, Color[] palette,
			Vector3 mainPivot, int spriteNum, float lightIntensity, bool simple, bool spriteOutline,
			out int[] widths, out int[] heights, out Vector2[] pivots
		) {

			int voxelSizeX = voxels.GetLength(0);
			int voxelSizeY = voxels.GetLength(1);
			int voxelSizeZ = voxels.GetLength(2);
			widths = new int[spriteNum];
			heights = new int[spriteNum];
			pivots = new Vector2[spriteNum];

			if (voxelSizeX * voxelSizeY * voxelSizeZ > MAX_ROOM_NUM) {
				if (!Util.Dialog(
					"Warning",
					"Model Is Too Large !\nIt may takes very long time to create this sprite.\nAre you sure to do that?",
					"Just Go ahead!",
					"Cancel"
				)) {
					return null;
				}
			}

			Color[][] colorss = new Color[spriteNum][];
			Vector3 pivotOffset = new Vector3(
				voxelSizeX * mainPivot.x,
				voxelSizeZ * mainPivot.y,
				voxelSizeY * mainPivot.z
			);


			for (int index = 0; index < spriteNum; index++) {

				float angleY = simple ? SPRITE_ANGLE_SIMPLE[index] : SPRITE_ANGLE[index];
				Quaternion cameraRot = Quaternion.Inverse(Quaternion.Euler(CAMERA_ANGLE, angleY, 0f));
				Vector3 minPos;
				Vector3 maxPos;


				// Get faces
				List<VoxelFace> faces = GetFaces(
					voxels, palette,
					new Vector3(voxelSizeX, voxelSizeY, voxelSizeZ), mainPivot, cameraRot, pivotOffset,
					index, lightIntensity,
					out minPos, out maxPos
				);


				// Get Pivot01
				Vector3 pivot = new Vector3(
					Mathf.LerpUnclamped(0f, voxelSizeX, mainPivot.x),
					Mathf.LerpUnclamped(0f, voxelSizeZ, mainPivot.y),
					Mathf.LerpUnclamped(0f, voxelSizeY, mainPivot.z)
				);

				pivot = cameraRot * new Vector3(
					pivot.x - voxelSizeX * mainPivot.x,
					pivot.y - voxelSizeZ * mainPivot.y,
					pivot.z - voxelSizeY * mainPivot.z
				) + pivotOffset;

				pivot = new Vector3(
					(pivot.x - minPos.x) / (maxPos.x - minPos.x),
					(pivot.y - minPos.y) / (maxPos.y - minPos.y),
					(pivot.z - minPos.z) / (maxPos.z - minPos.z)
				);


				// Get Pixels

				int minPixelX;
				int minPixelY;
				int maxPixelX;
				int maxPixelY;

				// Get Pixels
				List<Pixel> pixels = GetPixels(
					faces, simple,
					out minPixelX, out minPixelY, out maxPixelX, out maxPixelY
				);

				// W and H
				int width = maxPixelX - minPixelX + 1 + 2;
				int height = maxPixelY - minPixelY + 1 + 2;

				// Get Colorss
				colorss[index] = new Color[width * height];
				int len = pixels.Count;
				for (int i = 0; i < len; i++) {
					int id = (pixels[i].Y - minPixelY + 1) * width + (pixels[i].X - minPixelX + 1);
					colorss[index][id] = pixels[i].Color;
				}

				// Do some Cheat (｡･∀･)ﾉﾞ
				{
					List<int> cheatPixels = new List<int>();
					List<Color> cheatColors = new List<Color>();
					for (int x = 0; x < width; x++) {
						for (int y = 0; y < height; y++) {
							Color c = CheckPixelsAround_Cheat(colorss[index], x, y, width, height);
							if (c != Color.clear) {
								cheatPixels.Add(y * width + x);
								cheatColors.Add(c);
							}
						}
					}
					int cheatCount = cheatPixels.Count;
					for (int i = 0; i < cheatCount; i++) {
						colorss[index][cheatPixels[i]] = cheatColors[i];
					}
				}

				// Outline
				if (!simple && spriteOutline) {
					Color outlineColor = Color.black;
					List<int> outlinePixels = new List<int>();
					for (int x = 0; x < width; x++) {
						for (int y = 0; y < height; y++) {
							if (CheckPixelsAround(colorss[index], x, y, width, height)) {
								outlinePixels.Add(y * width + x);
							}
						}
					}
					int outlineCount = outlinePixels.Count;
					for (int i = 0; i < outlineCount; i++) {
						colorss[index][outlinePixels[i]] = outlineColor;
					}
				}

				// Final
				widths[index] = width;
				heights[index] = height;
				pivots[index] = pivot;

			}
			return colorss;
		}



		private static List<VoxelFace> GetFaces (
			Voxel[,,] voxels, Color[] palette,
			Vector3 voxelSize, Vector3 mainPivot, Quaternion cameraRot, Vector3 pivotOffset,
			int cameraAngleIndex, float lightIntensity,
			out Vector3 minPos, out Vector3 maxPos
		) {
			lightIntensity = Mathf.Lerp(0f, 0.7f, lightIntensity * 0.2f);
			minPos = Vector3.one * float.MaxValue;
			maxPos = Vector3.one * float.MinValue;
			List<VoxelFace> faces = new List<VoxelFace>();
			for (int x = 0; x < voxelSize.x; x++) {
				for (int y = 0; y < voxelSize.y; y++) {
					for (int z = 0; z < voxelSize.z; z++) {
						Voxel vox = voxels[x, y, z];
						Color color = palette[vox.ColorIndex <= 0 ? 0 : vox.ColorIndex - 1];
						for (int i = 0; i < 6; i++) {
							if (!vox.IsEmpty && vox.Visible[i]) {
								Vector3 pos = cameraRot * (new Vector3(
									x - voxelSize.x * mainPivot.x,
									z - voxelSize.z * mainPivot.y,
									y - voxelSize.y * mainPivot.z
								) + VOX_CENTER_OFFSET[i] * 0.5f) + pivotOffset;
								minPos = Vector3.Min(minPos, pos);
								maxPos = Vector3.Max(maxPos, pos);
								Vector3 worldNormal = cameraRot * VOX_CENTER_OFFSET[i];
								// Normal Check
								if (Vector3.Angle(worldNormal, Vector3.back) >= 90f) {
									continue;
								}
								VoxelFace face = new VoxelFace() {
									Position = pos,
									Normal = worldNormal,
									Type = VoxelFace.GetFaceType(cameraAngleIndex, (Direction)i, worldNormal.x),
								};
								face.Color = Color.Lerp(
									color,
									(int)face.Type > 3 ? Color.black : color,
									(int)face.Type > 3 ? (int)face.Type > 7 ? lightIntensity * 0.5f : lightIntensity : 1f
								);
								faces.Add(face);
							}
						}
					}
				}
			}
			faces.Sort(new FaceSorter());
			return faces;
		}




		#region --- Colorss ---



		private static readonly Vector2[][] TYPE_PIXEL_OFFSETSS = new Vector2[12][] {


			new Vector2[]{// U 0 ※
				new Vector2 (-2,2),new Vector2 (-1,2),new Vector2 (0,2),new Vector2 (1,2),
				new Vector2 (-2,1),new Vector2 (-1,1),new Vector2 (0,1),new Vector2 (1,1),
				new Vector2 (-2,0),new Vector2 (-1,0),new Vector2 (0,0),new Vector2 (1,0),
			},
			new Vector2[]{// U 22
				new Vector2 (0,2),new Vector2 (1,2),new Vector2 (2,2),new Vector2 (3,2),
				new Vector2 (0,1),new Vector2 (1,1),new Vector2 (2,1),new Vector2 (3,1), new Vector2 (4,1),
								  new Vector2 (1,0),new Vector2 (2,0),new Vector2 (3,0), new Vector2 (4,0),
													new Vector2 (2,-1),
			},
			new Vector2[]{// U 45
								   new Vector2 (-1,4),new Vector2 (0,4),
				new Vector2 (-2,3),new Vector2 (-1,3),new Vector2 (0,3),new Vector2 (1,3),
				new Vector2 (-2,2),new Vector2 (-1,2),new Vector2 (0,2),new Vector2 (1,2),new Vector2 (2,2),
								   new Vector2 (-1,1),new Vector2 (0,1),new Vector2 (1,1),new Vector2 (2,1),
													  new Vector2 (0,0),new Vector2 (1,0),
			},
			new Vector2[]{// U 67
				new Vector2 (0,2),new Vector2 (-1,2),new Vector2 (-2,2),new Vector2 (-3,2),
				new Vector2 (0,1),new Vector2 (-1,1),new Vector2 (-2,1),new Vector2 (-3,1), new Vector2 (-4,1),
								  new Vector2 (-1,0),new Vector2 (-2,0),new Vector2 (-3,0), new Vector2 (-4,0),
													new Vector2 (-2,-1),
			},


			new Vector2[]{},
			new Vector2[]{// L 22
				new Vector2 (-1,4),
				new Vector2 (-1,3),new Vector2 (0,3),new Vector2 (1,3),
				new Vector2 (-1,2),new Vector2 (0,2),new Vector2 (1,2),new Vector2 (2,2),
				new Vector2 (-1,1),new Vector2 (0,1),new Vector2 (1,1),new Vector2 (2,1),
								   new Vector2 (0,0),new Vector2 (1,0),new Vector2 (2,0),
			},
			new Vector2[]{// L 45
				new Vector2 (-1,4),new Vector2 (0,4),new Vector2 (1,3),
				new Vector2 (-1,3),new Vector2 (0,3),new Vector2 (1,2),
				new Vector2 (-1,2),new Vector2 (0,2),new Vector2 (1,1),
				new Vector2 (-1,1),new Vector2 (0,1),new Vector2 (1,0),
			},
			new Vector2[]{// L 67
											  
													 new Vector2 (-2,3),new Vector2 (-3,3),
				new Vector2 (0,2),new Vector2 (-1,2),new Vector2 (-2,2),new Vector2 (-3,2),
				new Vector2 (0,1),new Vector2 (-1,1),new Vector2 (-2,1),new Vector2 (-3,1),
				new Vector2 (0,0),new Vector2 (-1,0),new Vector2 (-2,0),new Vector2 (-3,0),
				new Vector2 (0,-1),new Vector2 (-1,-1),
			},


			new Vector2[]{// F 0
				new Vector2 (-2,3),new Vector2 (-1,3),new Vector2 (0,3),new Vector2 (1,3),
				new Vector2 (-2,2),new Vector2 (-1,2),new Vector2 (0,2),new Vector2 (1,2),
				new Vector2 (-2,1),new Vector2 (-1,1),new Vector2 (0,1),new Vector2 (1,1),
				new Vector2 (-2,0),new Vector2 (-1,0),new Vector2 (0,0),new Vector2 (1,0),
			},
			new Vector2[]{// F 22
												   
												    new Vector2 (2,3),new Vector2 (3,3),
				new Vector2 (0,2),new Vector2 (1,2),new Vector2 (2,2),new Vector2 (3,2),
				new Vector2 (0,1),new Vector2 (1,1),new Vector2 (2,1),new Vector2 (3,1),
				new Vector2 (0,0),new Vector2 (1,0),new Vector2 (2,0),new Vector2 (3,0),
				new Vector2 (0,-1),new Vector2 (1,-1),
			},
			new Vector2[]{// F 45
				new Vector2 (-1,3),new Vector2 (0,4),new Vector2 (1,4),
				new Vector2 (-1,2),new Vector2 (0,3),new Vector2 (1,3),
				new Vector2 (-1,1),new Vector2 (0,2),new Vector2 (1,2),
				new Vector2 (-1,0),new Vector2 (0,1),new Vector2 (1,1),
			},
			new Vector2[]{// F 67
				new Vector2 (1,4),
				new Vector2 (1,3),new Vector2 (0,3),new Vector2 (-1,3),
				new Vector2 (1,2),new Vector2 (0,2),new Vector2 (-1,2),new Vector2 (-2,2),
				new Vector2 (1,1),new Vector2 (0,1),new Vector2 (-1,1),new Vector2 (-2,1),
								  new Vector2 (0,0),new Vector2 (-1,0),new Vector2 (-2,0),
			},

		};



		private static List<Pixel> GetPixels (
			List<VoxelFace> faces, bool simple,
			out int minPixelX, out int minPixelY, out int maxPixelX, out int maxPixelY
		) {
			minPixelX = int.MaxValue;
			minPixelY = int.MaxValue;
			maxPixelX = 0;
			maxPixelY = 0;
			List<Pixel> pixels = new List<Pixel>();
			int count = faces.Count;
			for (int index = 0; index < count; index++) {
				VoxelFace face = faces[index];
				float pixelSize = simple ? 1f : 4f;
				Vector2 pos = new Vector2(
					(face.Position.x * pixelSize),
					(face.Position.y * pixelSize)
				);
				pos.x = Mathf.Round((pos.x / pixelSize) * pixelSize);
				pos.y = Mathf.Round((pos.y / pixelSize) * pixelSize);
				int len = simple ? 1 : TYPE_PIXEL_OFFSETSS[(int)face.Type].Length;
				for (int i = 0; i < len; i++) {
					Vector2 p = pos + (simple ? Vector2.zero : (
						TYPE_PIXEL_OFFSETSS[(int)face.Type][i]
					));
					minPixelX = Mathf.Min(minPixelX, (int)p.x);
					minPixelY = Mathf.Min(minPixelY, (int)p.y);
					maxPixelX = Mathf.Max((int)p.x, maxPixelX);
					maxPixelY = Mathf.Max((int)p.y, maxPixelY);
					pixels.Add(new Pixel() {
						Color = face.Color,
						X = (int)p.x,
						Y = (int)p.y,
					});
				}
			}
			return pixels;
		}



		private static bool CheckPixelsAround (Color[] colors, int x, int y, int w, int h) {

			// Self
			if (colors[y * w + x] != Color.clear) {
				return false;
			}

			// U
			if (y < h - 1 && colors[(y + 1) * w + x] != Color.clear) {
				return true;
			}

			// D
			if (y > 0 && colors[(y - 1) * w + x] != Color.clear) {
				return true;
			}

			// L
			if (x < w - 1 && colors[y * w + x + 1] != Color.clear) {
				return true;
			}

			// R
			if (x > 0 && colors[y * w + x - 1] != Color.clear) {
				return true;
			}

			return false;
		}



		private static Color CheckPixelsAround_Cheat (Color[] colors, int x, int y, int w, int h) {
			// Self
			if (colors[y * w + x] != Color.clear) {
				return Color.clear;
			}

			Color c, color = Color.clear;

			// U
			if (y < h - 1) {
				c = colors[(y + 1) * w + x];
				if (c == Color.clear) {
					return Color.clear;
				} else {
					color = c;
				}
			}

			// D
			if (y > 0) {
				c = colors[(y - 1) * w + x];
				if (c == Color.clear) {
					return Color.clear;
				} else {
					color = c;
				}
			}

			// L
			if (x < w - 1) {
				c = colors[y * w + x + 1];
				if (c == Color.clear) {
					return Color.clear;
				} else {
					color = c;
				}
			}

			// R
			if (x > 0) {
				c = colors[y * w + x - 1];
				if (c == Color.clear) {
					return Color.clear;
				} else {
					color = c;
				}
			}

			return color;
		}


		#endregion



		private static SpriteResult PackTextures (Color[][] colorss, int[] widths, int[] heights, Vector2[] pivots) {

			int tCount = colorss.Length;
			int gapSize = 1;
			SpriteResult resultInfo = new SpriteResult() {
				Pivots = pivots,
			};

			// Single Size
			int singleWidth = 0;
			int singleHeight = 0;
			for (int i = 0; i < tCount; i++) {
				singleWidth = Mathf.Max(singleWidth, widths[i]);
				singleHeight = Mathf.Max(singleHeight, heights[i]);
			}

			// Size All
			int aimCountX = tCount > 4 ? 4 : tCount;
			int aimCountY = ((tCount - 1) / 4) + 1;
			int aimWidth = aimCountX * singleWidth + gapSize * (aimCountX + 1);
			int aimHeight = aimCountY * singleHeight + gapSize * (aimCountY + 1);

			resultInfo.Width = aimWidth;
			resultInfo.Height = aimHeight;
			resultInfo.Texture = new Texture2D(aimWidth, aimHeight, TextureFormat.ARGB32, false);
			resultInfo.Texture.SetPixels(new Color[aimWidth * aimHeight]);

			Rect[] spriteRects = new Rect[tCount];
			for (int i = 0; i < tCount; i++) {
				int width = widths[i];
				int height = heights[i];
				int globalOffsetX = (i % 4) * singleWidth + ((i % 4) + 1) * gapSize;
				int globalOffsetY = (i / 4) * singleHeight + ((i / 4) + 1) * gapSize;
				int offsetX = globalOffsetX + (singleWidth - width) / 2;
				int offsetY = globalOffsetY + (singleHeight - height) / 2;
				// Rect
				spriteRects[i] = new Rect(globalOffsetX, globalOffsetY, singleWidth, singleHeight);
				// Pivot
				resultInfo.Pivots[i] = new Vector2(
					(resultInfo.Pivots[i].x * (float)width + (float)(singleWidth - width) / 2f) / (float)singleWidth,
					(resultInfo.Pivots[i].y * (float)height + (float)(singleHeight - height) / 2f) / (float)singleHeight
				);
				resultInfo.Texture.SetPixels(offsetX, offsetY, width, height, colorss[i]);
			}
			resultInfo.Texture.Apply();
			resultInfo.Rects = spriteRects;

			return resultInfo;
		}




	}


	public class MaterialCombiner {


		public static void CombineMaterials (List<MeshRenderer> meshRenderers, string exportPath) {

			// Warning
			if (!Util.Dialog(
				"Warning",
				"This operation will replace the current materials.\nPlease make a back up for these prefabs.",
				"I made a backup, Go ahead",
				"Cancel"
			)) {
				return;
			}

			// Result
			Material resultMat = null;
			for (int i = 0; i < meshRenderers.Count; i++) {
				Material mat = meshRenderers[i].sharedMaterial;
				if (mat && mat.HasProperty("_MainTex") && mat.mainTexture) {
					if (!resultMat) {
						resultMat = new Material(mat);
					}
				} else {
					meshRenderers.RemoveAt(i);
					i--;
				}
			}
			if (meshRenderers.Count == 0) {
				Util.Dialog(
					"Warning",
					"Can NOT combine material.\nBecause no meshRenderer has material with _MainTex property.",
					"OK"
				);
				return;
			}
			if (!resultMat) {
				resultMat = new Material(Shader.Find("Mobile/Diffuse"));
			}

			// Textures
			List<Texture2D> textures = new List<Texture2D>();
			List<Mesh> meshes = new List<Mesh>();

			// Get textures and meshes
			for (int i = 0; i < meshRenderers.Count; i++) {
				MeshFilter mf = meshRenderers[i].GetComponent<MeshFilter>();
				Mesh mesh = mf ? mf.sharedMesh : null;
				Texture2D texture = meshRenderers[i].sharedMaterial.mainTexture as Texture2D;
				if (mesh && texture) {
					textures.Add(texture);
					meshes.Add(mesh);
				}
			}

			// Pack Textures
			Texture2D resultTexture;// = new Texture2D(1, 1);
			//Rect[] rects = resultTexture.PackTextures(textures.ToArray(), 0);
			Rect[] rects = RectPacking.PackTextures(out resultTexture, textures);
			VoxelMesh.CutTexture(ref resultTexture, ref rects);

			// Fix Mesh UV
			//Vector2 resultTextureSize = resultTexture.texelSize;
			for (int i = 0; i < meshes.Count; i++) {
				Vector2[] uvs = meshes[i].uv;
				for (int j = 0; j < uvs.Length; j++) {
					uvs[j] = new Vector2(
						rects[i].x + uvs[j].x * rects[i].width,
						rects[i].y + uvs[j].y * rects[i].height
					);
				}
				meshes[i].uv = uvs;
				meshes[i].UploadMeshData(false);
			}

			// Get Mat File Path
			string _name = "Mat(";
			for (int i = 0; i < meshRenderers.Count && i < 2; i++) {
				_name += GetRootName(meshRenderers[i].gameObject.transform) + " ";
			}
			_name += meshRenderers.Count > 2 ? " etc)" : ")";

			// Save Mat Asset
			AssetDatabase.CreateAsset(resultMat, Util.CombinePaths(exportPath, _name + ".mat"));


			// Save Texture
			string texturePath = Util.CombinePaths(exportPath, _name + ".png");
			Util.ByteToFile(resultTexture.EncodeToPNG(), texturePath);
			Postprocessor.AddToQueue(texturePath);
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			AssetDatabase.SaveAssets();
			resultTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
			resultMat.mainTexture = resultTexture;

			// Set Materials
			for (int i = 0; i < meshRenderers.Count; i++) {
				meshRenderers[i].sharedMaterial = resultMat;
			}

		}


		private static string GetRootName (Transform tf) {
			while (tf.parent) {
				tf = tf.parent;
			}
			return tf.name;
		}


	}



}