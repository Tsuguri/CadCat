using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadCat.Math
{
	class SurfaceFilling
	{
		struct QuadData
		{
			public int down;
			public int right;
			public int corner;
		}

		public static Tuple<List<Vector2>, List<int>> MarchingAszklars(bool[,] ptsAvaiable, double uLimit, double vLimit)
		{
			var vertices = new List<Vector2>();
			var indices = new List<int>();

			int uDensity = ptsAvaiable.GetLength(1);
			double uStep = uLimit / (uDensity - 1);
			int vDensity = ptsAvaiable.GetLength(0);
			double vStep = vLimit / (vDensity - 1);

			var vertAvai = new QuadData[vDensity, uDensity];

			{
				var vert = new QuadData() { corner = -1, down = -1, right = -1 };
				for (int i = 0; i < vDensity; i++)
				for (int j = 0; j < uDensity; j++)
					vertAvai[i, j] = vert;
			}

			for (int i = 0; i < uDensity - 1; i++)
			for (int j = 0; j < vDensity - 1; j++)
			{
				if (ptsAvaiable[j, i])
				{
					vertAvai[j, i].corner = vertices.Count;
					vertices.Add(new Vector2(i * uStep, j * vStep));

				}

				if (ptsAvaiable[j, i] != ptsAvaiable[j + 1, i])
				{
					vertAvai[j, i].down = vertices.Count;
					vertices.Add(new Vector2(i * uStep, (j + 0.5) * vStep));
				}

				if (ptsAvaiable[j, i] != ptsAvaiable[j, i + 1])
				{
					vertAvai[j, i].right = vertices.Count;
					vertices.Add(new Vector2((i + 0.5) * uStep, j * vStep));
				}
			}

			for (int i = 0; i < uDensity - 1; i++)
			{
				if (ptsAvaiable[vDensity - 1, i])
				{
					vertAvai[vDensity - 1, i].corner = vertices.Count;
					vertices.Add(new Vector2(i * uStep, (vDensity - 1) * vStep));
				}
				if (ptsAvaiable[vDensity - 1, i] != ptsAvaiable[vDensity - 1, i + 1])
				{
					vertAvai[vDensity - 1, i].right = vertices.Count;
					vertices.Add(new Vector2((i + 0.5) * uStep, (vDensity - 1) * vStep));
				}
			}

			for (int i = 0; i < vDensity - 1; i++)
			{
				if (ptsAvaiable[i, uDensity - 1])
				{
					vertAvai[i, uDensity - 1].corner = vertices.Count;
					vertices.Add(new Vector2((uDensity - 1) * uStep, i * vStep));
				}

				if (ptsAvaiable[i, uDensity - 1] != ptsAvaiable[i + 1, uDensity - 1])
				{
					vertAvai[i, uDensity - 1].down = vertices.Count;
					vertices.Add(new Vector2((uDensity - 1) * uStep, (i + 0.5) * vStep));
				}
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
					indices.Add(vertAvai[j, i].corner);
					indices.Add(vertAvai[j + 1, i].corner);
				}

				if (lu && ru)
				{
					indices.Add(vertAvai[j, i].corner);
					indices.Add(vertAvai[j, i + 1].corner);
				}

				if (lu && !ld)
				{
					indices.Add(vertAvai[j, i].corner);
					indices.Add(vertAvai[j, i].down);
				}
				if (!lu && ld)
				{
					indices.Add(vertAvai[j, i].down);
					indices.Add(vertAvai[j + 1, i].corner);
				}

				if (lu && !ru)
				{
					indices.Add(vertAvai[j, i].corner);
					indices.Add(vertAvai[j, i].right);
				}

				if (!lu && ru)
				{
					indices.Add(vertAvai[j, i].right);
					indices.Add(vertAvai[j, i + 1].corner);
				}

				if (lu != ru && ru == ld)
				{
					indices.Add(vertAvai[j, i].right);
					indices.Add(vertAvai[j, i].down);
				}

				if (lu == rd && lu != ld)
				{
					indices.Add(vertAvai[j, i].down);
					indices.Add(vertAvai[j + 1, i].right);
				}

				if (ru != lu && lu == rd)
				{
					indices.Add(vertAvai[j, i].right);
					indices.Add(vertAvai[j, i + 1].down);
				}

				if (ld == ru && ld != rd)
				{
					indices.Add(vertAvai[j, i + 1].down);
					indices.Add(vertAvai[j + 1, i].right);
				}

				if (lu == ld && ru == rd && lu != ru)
				{
					indices.Add(vertAvai[j, i].right);
					indices.Add(vertAvai[j + 1, i].right);
				}

				if (lu == ru && ld == rd && lu != ld)
				{
					indices.Add(vertAvai[j, i].down);
					indices.Add(vertAvai[j, i + 1].down);
				}
			}



			return new Tuple<List<Vector2>, List<int>>(vertices, indices);
		}
	}
}
