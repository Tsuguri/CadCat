using CadCat.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CadCat.Rendering;
using System.Windows.Media;
using CadCat.Math;

namespace CadCat.GeometryModels
{
	class BezierPatch : Model
	{
		private CatPoint[] points = new CatPoint[16];
		private List<Vector3> mesh = new List<Vector3>();
		private List<int> meshIndices = new List<int>();
		private SceneData scene;

		private bool changed = false;
		private bool owner = false;

		public bool ShowPolygon
		{
			get; set;
		} = true;

		public int WidthDiv
		{ get; set; } = 10;

		public int HeightDiv
		{ get; set; } = 10;

		private static List<int> indices = new List<int>()
		{
			0,1,
			1,2,
			2,3,

			4,5,
			5,6,
			6,7,

			8,9,
			9,10,
			10,11,

			12,13,
			13,14,
			14,15,

			0,4,
			4,8,
			8,12,

			1,5,
			5,9,
			9,13,

			2,6,
			6,10,
			10,14,

			3,7,
			7,11,
			11,15
		};

		public BezierPatch(SceneData scene)
		{
			this.scene = scene;

			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
				{
					var pt = scene.CreateHiddenCatPoint(new Math.Vector3(i, System.Math.Sin(Math.Utils.Pi * (i * 0.5 + j * 0.1) / 2.0), j));
					points[i * 4 + j] = pt;
					pt.OnChanged += OnBezierPointChanged;
				}
			changed = true;
			owner = true;
		}

		public BezierPatch(CatPoint[,] pts)
		{
			Debug.Assert(points.Length == 16);

			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
				{
					points[i * 4 + j] = pts[i,j];
					pts[i,j].OnChanged += OnBezierPointChanged;
				}
			changed = true;
			owner = false;
		}

		private void OnBezierPointChanged(CatPoint point)
		{
			changed = true;
		}



		public override void Render(BaseRenderer renderer)
		{

			if (changed)
				RecalculatePoints();
			base.Render(renderer);

			renderer.ModelMatrix = GetMatrix(false, new Math.Vector3());
			renderer.SelectedColor = IsSelected ? Colors.LimeGreen : Colors.White;
			renderer.UseIndices = true;

			if (ShowPolygon)
			{
				renderer.Indices = indices;
				renderer.Points = points.Select(x => x.Position).ToList();


				renderer.Transform();
				renderer.DrawLines();
			}

			renderer.Indices = meshIndices;
			renderer.Points = mesh;

			renderer.Transform();
			renderer.DrawLines();
		}

		private void RecalculatePoints()
		{
			mesh.Clear();
			mesh.Capacity = System.Math.Max(mesh.Capacity, WidthDiv * HeightDiv);
			meshIndices.Clear();
			double widthStep = 1.0 / (WidthDiv - 1);
			double heightStep = 1.0 / (HeightDiv - 1);

			for (int i = 0; i < WidthDiv; i++)
				for (int j = 0; j < HeightDiv; j++)
				{
					mesh.Insert(i * WidthDiv + j, EvaluatePointValue(i * widthStep, j * heightStep));
				}

			for (int i = 0; i < WidthDiv - 1; i++)
				for (int j = 0; j < HeightDiv - 1; j++)
				{
					meshIndices.Add(i * WidthDiv + j);
					meshIndices.Add(i * WidthDiv + j + 1);
					meshIndices.Add(i * WidthDiv + j);
					meshIndices.Add((i + 1) * WidthDiv + j);
				}
			for (int i = 0; i < WidthDiv - 1; i++)
			{
				meshIndices.Add(WidthDiv * (i + 1) - 1);
				meshIndices.Add(WidthDiv * (i + 2) - 1);
			}
			for (int j = 0; j < HeightDiv - 1; j++)
			{
				meshIndices.Add((HeightDiv - 1) * WidthDiv + j);
				meshIndices.Add((HeightDiv - 1) * WidthDiv + j + 1);
			}
			changed = false;
		}

		private static double[,] temp = new double[2, 4];
		private Vector3 EvaluatePointValue(double u, double v)
		{
			temp[0, 0] = EvaluateBerenstein(0, u);
			temp[0, 1] = EvaluateBerenstein(1, u);
			temp[0, 2] = EvaluateBerenstein(2, u);
			temp[0, 3] = EvaluateBerenstein(3, u);

			temp[1, 0] = EvaluateBerenstein(0, v);
			temp[1, 1] = EvaluateBerenstein(1, v);
			temp[1, 2] = EvaluateBerenstein(2, v);
			temp[1, 3] = EvaluateBerenstein(3, v);

			var sum = new Vector3();
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
					sum += points[i * 4 + j].Position * temp[0, i] * temp[1, j];


			return sum;
		}

		private double EvaluateBerenstein(int n, double val)
		{
			double neg = 1 - val;
			switch (n)
			{
				case 0:
					return neg * neg * neg;
				case 1:
					return 3 * neg * neg * val;
				case 2:
					return 3 * neg * val * val;
				case 3:
					return val * val * val;
				default:
					throw new ArgumentException("bad n value");
			}
		}

		private Vector3 EvaluateBezierCurve(int startPoint, int step, double x)
		{
			double x2 = x * x;
			double x3 = x2 * x;
			double x11 = (1 - x);
			double x12 = x11 * (1 - x);
			double x13 = x12 * (1 - x);
			Vector3 tempVec = new Vector3();
			tempVec = points[startPoint + step * 0].Position * x13 + points[startPoint + step * 1].Position * x12 * x * 3
				+ points[startPoint + step * 2].Position * x2 * x11 * 3 + points[startPoint + step * 3].Position * x3;

			return tempVec;
		}

		public override void CleanUp()
		{
			base.CleanUp();
			if (owner)
			{
				foreach (var pt in points)
				{
					scene.RemovePoint(pt);
				}
			}
		}

		public override string GetName()
		{
			return "Bezier patch "+base.GetName();
		}
	}
}
