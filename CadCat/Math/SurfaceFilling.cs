using System;
using System.Collections.Generic;

namespace CadCat.Math
{
	class SurfaceFilling
	{
		struct QuadData
		{
			public int Down;
			public int Right;
			public int Corner;
		}

		private static int iterations = 5;
		private static Vector2 FindPoint(Vector2 from, Vector2 to, Func<Vector2, bool> check)
		{
			var left = from;
			var right = to;
			var leftBelongs = check(left);
			Vector2 last = left;
			for (int i = 0; i < iterations; i++)
			{
				last = (left + right) * 0.5;
				var afgBelongs = check(last);
				if (leftBelongs == afgBelongs)
				{
					left = last;
				}
				else
					right = last;
			}
			return last;
		}

		public static Tuple<List<Vector2>, List<int>> MarchingAszklars(bool[,] ptsAvaiable, double uWidth, double vWidth, bool uCycled, bool vCycled, Func<Vector2, bool> check)
		{
			var vertices = new List<Vector2>();
			var indices = new List<int>();

			int uDensity = ptsAvaiable.GetLength(1);
			double uStep = uWidth / (uDensity - 1);
			int vDensity = ptsAvaiable.GetLength(0);
			double vStep = vWidth / (vDensity - 1);

			var vertAvai = new QuadData[vDensity, uDensity];

			{
				var vert = new QuadData() { Corner = -1, Down = -1, Right = -1 };
				for (int i = 0; i < vDensity; i++)
					for (int j = 0; j < uDensity; j++)
						vertAvai[i, j] = vert;
			}

			for (int i = 0; i < uDensity - 1; i++)
				for (int j = 0; j < vDensity - 1; j++)
				{
					if (ptsAvaiable[j, i])
					{
						vertAvai[j, i].Corner = vertices.Count;
						vertices.Add(new Vector2(i * uStep, j * vStep));

					}

					if (ptsAvaiable[j, i] != ptsAvaiable[j + 1, i])
					{
						vertAvai[j, i].Down = vertices.Count;
						vertices.Add(FindPoint(new Vector2(i * uStep, j * vStep), new Vector2(i * uStep, (j + 1) * vStep), check));
					}

					if (ptsAvaiable[j, i] != ptsAvaiable[j, i + 1])
					{
						vertAvai[j, i].Right = vertices.Count;
						vertices.Add(FindPoint(new Vector2(i * uStep, j * vStep), new Vector2((i + 1) * uStep, j * vStep), check));
					}
				}

			for (int i = 0; i < uDensity - 1; i++)
			{
				if (ptsAvaiable[vDensity - 1, i])
				{
					vertAvai[vDensity - 1, i].Corner = vertices.Count;
					vertices.Add(new Vector2(i * uStep, (vDensity - 1) * vStep));
				}
				if (ptsAvaiable[vDensity - 1, i] != ptsAvaiable[vDensity - 1, i + 1])
				{
					vertAvai[vDensity - 1, i].Right = vertices.Count;
					vertices.Add(FindPoint(new Vector2(i * uStep, (vDensity - 1) * vStep), new Vector2((i + 1) * uStep, (vDensity - 1) * vStep), check));
				}
			}

			for (int i = 0; i < vDensity - 1; i++)
			{
				if (ptsAvaiable[i, uDensity - 1])
				{
					vertAvai[i, uDensity - 1].Corner = vertices.Count;
					vertices.Add(new Vector2((uDensity - 1) * uStep, i * vStep));
				}

				if (ptsAvaiable[i, uDensity - 1] != ptsAvaiable[i + 1, uDensity - 1])
				{
					vertAvai[i, uDensity - 1].Down = vertices.Count;
					vertices.Add(FindPoint(new Vector2((uDensity - 1) * uStep, i * vStep), new Vector2((uDensity - 1) * uStep, (i + 1) * vStep), check));
				}
			}

			if (ptsAvaiable[vDensity - 1, uDensity - 1])
			{
				vertAvai[vDensity - 1, uDensity - 1].Corner = vertices.Count;
				vertices.Add(new Vector2(uWidth, vWidth));
			}
			for (int i = 0; i < uDensity - 1; i++)
				for (int j = 0; j < vDensity - 1; j++)
				{
					var lu = ptsAvaiable[j, i];
					var ru = ptsAvaiable[j, i + 1];
					var ld = ptsAvaiable[j + 1, i];
					var rd = ptsAvaiable[j + 1, i + 1];


					if (lu && ld)
					{
						indices.Add(vertAvai[j, i].Corner);
						indices.Add(vertAvai[j + 1, i].Corner);
					}

					if (lu && !ld)
					{
						indices.Add(vertAvai[j, i].Corner);
						indices.Add(vertAvai[j, i].Down);
					}
					if (!lu && ld)
					{
						indices.Add(vertAvai[j, i].Down);
						indices.Add(vertAvai[j + 1, i].Corner);
					}
					if (lu && ru)
					{
						indices.Add(vertAvai[j, i].Corner);
						indices.Add(vertAvai[j, i + 1].Corner);
					}

					if (lu && !ru)
					{
						indices.Add(vertAvai[j, i].Corner);
						indices.Add(vertAvai[j, i].Right);
					}

					if (!lu && ru)
					{
						indices.Add(vertAvai[j, i].Right);
						indices.Add(vertAvai[j, i + 1].Corner);
					}

					if (lu != ru && ru == ld)
					{
						indices.Add(vertAvai[j, i].Right);
						indices.Add(vertAvai[j, i].Down);
					}

					if (lu == rd && lu != ld)
					{
						indices.Add(vertAvai[j, i].Down);
						indices.Add(vertAvai[j + 1, i].Right);
					}

					if (ru != lu && lu == rd)
					{
						indices.Add(vertAvai[j, i].Right);
						indices.Add(vertAvai[j, i + 1].Down);
					}

					if (ld == ru && ld != rd)
					{
						indices.Add(vertAvai[j, i + 1].Down);
						indices.Add(vertAvai[j + 1, i].Right);
					}

					if (lu == ld && ru == rd && lu != ru)
					{
						indices.Add(vertAvai[j, i].Right);
						indices.Add(vertAvai[j + 1, i].Right);
					}

					if (lu == ru && ld == rd && lu != ld)
					{
						indices.Add(vertAvai[j, i].Down);
						indices.Add(vertAvai[j, i + 1].Down);
					}
				}

			if (!uCycled)
			{
				for (int j = 0; j < vDensity - 1; j++)
				{
					var lu = ptsAvaiable[j, uDensity - 1];
					var ld = ptsAvaiable[j + 1, uDensity - 1];

					if (lu && ld)
					{
						indices.Add(vertAvai[j, uDensity - 1].Corner);
						indices.Add(vertAvai[j + 1, uDensity - 1].Corner);
					}

					if (lu && !ld)
					{
						indices.Add(vertAvai[j, uDensity - 1].Corner);
						indices.Add(vertAvai[j, uDensity - 1].Down);
					}
					if (!lu && ld)
					{
						indices.Add(vertAvai[j, uDensity - 1].Down);
						indices.Add(vertAvai[j + 1, uDensity - 1].Corner);
					}
				}
			}

			if (!vCycled)
			{
				for (int i = 0; i < uDensity - 1; i++)
				{
					var lu = ptsAvaiable[vDensity - 1, i];
					var ru = ptsAvaiable[vDensity - 1, i + 1];
					if (lu && ru)
					{
						indices.Add(vertAvai[vDensity - 1, i].Corner);
						indices.Add(vertAvai[vDensity - 1, i + 1].Corner);
					}

					if (lu && !ru)
					{
						indices.Add(vertAvai[vDensity - 1, i].Corner);
						indices.Add(vertAvai[vDensity - 1, i].Right);
					}

					if (!lu && ru)
					{
						indices.Add(vertAvai[vDensity - 1, i].Right);
						indices.Add(vertAvai[vDensity - 1, i + 1].Corner);
					}
				}
			}



			return new Tuple<List<Vector2>, List<int>>(vertices, indices);
		}
	}
}
