﻿using CadCat.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CadCat.Rendering;
using System.Windows.Media;
using CadCat.Math;

namespace CadCat.GeometryModels
{
	class BezierPatch : Patch
	{
		private readonly CatPoint[] points = new CatPoint[16];
		private readonly List<Vector3> mesh = new List<Vector3>();
		private readonly List<int> meshIndices = new List<int>();
		private readonly SceneData scene;


		private readonly bool owner;


		private static readonly List<int> Indices = new List<int>()
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
					pt.DependentUnremovable += 1;
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
					points[i  + j * 4] = pts[i, j];
					//pts[i, j].DependentUnremovable += 1;

					pts[i, j].OnChanged += OnBezierPointChanged;
					pts[i, j].OnReplace += OnBezierPointReplaced;
				}
			changed = true;
			owner = false;
		}

		private void OnBezierPointChanged(CatPoint point)
		{
			changed = true;
		}

		private void OnBezierPointReplaced(CatPoint point, CatPoint newPoint)
		{
			for (int i = 0; i < points.Length; i++)
				if (points[i] == point)
					points[i] = newPoint;
			changed = true;
			point.OnChanged -= OnBezierPointChanged;
			if (owner)
				point.DependentUnremovable -= 1;
			newPoint.OnChanged += OnBezierPointChanged;
			if (owner)
				newPoint.DependentUnremovable += 1;
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
				renderer.Indices = Indices;
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
			mesh.Capacity = System.Math.Max(mesh.Capacity, (WidthDiv + 1) * (HeightDiv + 1));
			meshIndices.Clear();
			double widthStep = 1.0 / WidthDiv;
			double heightStep = 1.0 / HeightDiv;
			int widthPoints = WidthDiv + 1;
			int heightPoints = HeightDiv + 1;

			for (int i = 0; i < widthPoints; i++)
				for (int j = 0; j < heightPoints; j++)
				{
					mesh.Insert(i * heightPoints + j, EvaluatePointValue(i * widthStep, j * heightStep));
				}

			for (int i = 0; i < widthPoints - 1; i++)
				for (int j = 0; j < heightPoints - 1; j++)
				{
					meshIndices.Add(i * heightPoints + j);
					meshIndices.Add(i * heightPoints + j + 1);
					meshIndices.Add(i * heightPoints + j);
					meshIndices.Add((i + 1) * heightPoints + j);
				}
			for (int i = 0; i < widthPoints - 1; i++)
			{
				meshIndices.Add(heightPoints * (i + 1) - 1);
				meshIndices.Add(heightPoints * (i + 2) - 1);
			}
			for (int j = 0; j < heightPoints - 1; j++)
			{
				meshIndices.Add((widthPoints - 1) * heightPoints + j);
				meshIndices.Add((widthPoints - 1) * heightPoints + j + 1);
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

			return SumFunctions();
		}

		private Vector3 EvaluateUDerivative(double u, double v)
		{
			temp[0, 0] = EvalueateBerensteinDerivative(0, u);
			temp[0, 1] = EvalueateBerensteinDerivative(1, u);
			temp[0, 2] = EvalueateBerensteinDerivative(2, u);
			temp[0, 3] = EvalueateBerensteinDerivative(3, u);

			temp[1, 0] = EvaluateBerenstein(0, v);
			temp[1, 1] = EvaluateBerenstein(1, v);
			temp[1, 2] = EvaluateBerenstein(2, v);
			temp[1, 3] = EvaluateBerenstein(3, v);

			return SumFunctions() * 3;
		}

		private Vector3 EvaluateVDerivative(double u, double v)
		{
			temp[0, 0] = EvaluateBerenstein(0, u);
			temp[0, 1] = EvaluateBerenstein(1, u);
			temp[0, 2] = EvaluateBerenstein(2, u);
			temp[0, 3] = EvaluateBerenstein(3, u);

			temp[1, 0] = EvalueateBerensteinDerivative(0, v);
			temp[1, 1] = EvalueateBerensteinDerivative(1, v);
			temp[1, 2] = EvalueateBerensteinDerivative(2, v);
			temp[1, 3] = EvalueateBerensteinDerivative(3, v);

			return SumFunctions() * 3;
		}

		private Vector3 SumFunctions()
		{
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

		private double EvalueateBerensteinDerivative(int n, double val)
		{
			double neg = 1 - val;
			switch (n)
			{
				case 0:
					return -3 * neg * neg;
				case 1:
					return 3 * (1 - 4 * val + 3 * val * val);
				case 2:
					return 3 * (2 * val - 3 * val * val);
				case 3:
					return 3 * val * val;
				default:
					throw new ArgumentException("bad n value");
			}
		}

		public override void CleanUp()
		{
			base.CleanUp();
			if (owner)
			{
				foreach (var pt in points)
				{
					pt.DependentUnremovable -= 1;
					scene.RemovePoint(pt);
				}
			}
		}

		public override string GetName()
		{
			return "Bezier patch " + base.GetName();
		}

		public override IEnumerable<CatPoint> EnumerateCatPoints()
		{
			return points;
		}

		public bool ContainsTwoInCorners(List<CatPoint> catPoints)
		{
			int contained = 0;

			if (catPoints.Contains(points[0]))
				contained++;
			if (catPoints.Contains(points[3]))
				contained++;
			if (catPoints.Contains(points[12]))
				contained++;
			if (catPoints.Contains(points[15]))
				contained++;

			return contained >= 2;
		}

		public bool ContainsInCorner(CatPoint catPoint)
		{
			return points[0] == catPoint || points[3] == catPoint || points[12] == catPoint || points[15] == catPoint;
		}

		public bool ContainsNearby(CatPoint prev, CatPoint next)
		{
			int ind;
			if (points[0] == prev)
				ind = 0;
			else if (points[3] == prev)
				ind = 3;
			else if (points[12] == prev)
				ind = 12;
			else if (points[15] == prev)
				ind = 15;
			else
				throw new Exception("Something failed");

			switch (ind)
			{
				case 0:
					return points[3] == next || points[12] == next;
				case 3:
					return points[0] == next || points[15] == next;
				case 12:
					return points[0] == next || points[15] == next;
				case 15:
					return points[3] == next || points[12] == next;

			}
			return false;
		}

		private int IndexOf(CatPoint point)
		{
			for (int i = 0; i < points.Length; i++)
				if (points[i] == point)
					return i;
			return -1;
		}



		public HalfPatchData GetDataBetweenPoints(CatPoint first, CatPoint second)
		{
			var firstInd = IndexOf(first);
			var secondInd = IndexOf(second);
			var back = new CatPoint[4];
			var nearest = new CatPoint[4];

			switch (firstInd)
			{
				case 0:
					switch (secondInd)
					{
						case 3:
							nearest[0] = points[0];
							nearest[1] = points[1];
							nearest[2] = points[2];
							nearest[3] = points[3];
							back[0] = points[4];
							back[1] = points[5];
							back[2] = points[6];
							back[3] = points[7];
							break;
						case 12:
							nearest[0] = points[0];
							nearest[1] = points[4];
							nearest[2] = points[8];
							nearest[3] = points[12];
							back[0] = points[1];
							back[1] = points[5];
							back[2] = points[9];
							back[3] = points[13];
							break;
					}
					break;
				case 3:
					switch (secondInd)
					{
						case 0:
							nearest[0] = points[3];
							nearest[1] = points[2];
							nearest[2] = points[1];
							nearest[3] = points[0];
							back[0] = points[7];
							back[1] = points[6];
							back[2] = points[5];
							back[3] = points[4];
							break;
						case 15:
							nearest[0] = points[3];
							nearest[1] = points[7];
							nearest[2] = points[11];
							nearest[3] = points[15];
							back[0] = points[2];
							back[1] = points[6];
							back[2] = points[10];
							back[3] = points[14];
							break;
					}
					break;
				case 12:
					switch (secondInd)
					{
						case 0:
							nearest[0] = points[12];
							nearest[1] = points[8];
							nearest[2] = points[4];
							nearest[3] = points[0];
							back[0] = points[13];
							back[1] = points[9];
							back[2] = points[5];
							back[3] = points[1];
							break;
						case 15:
							nearest[0] = points[12];
							nearest[1] = points[13];
							nearest[2] = points[14];
							nearest[3] = points[15];
							back[0] = points[8];
							back[1] = points[9];
							back[2] = points[10];
							back[3] = points[11];
							break;
					}
					break;
				case 15:
					switch (secondInd)
					{
						case 3:
							nearest[0] = points[15];
							nearest[1] = points[11];
							nearest[2] = points[7];
							nearest[3] = points[3];
							back[0] = points[14];
							back[1] = points[10];
							back[2] = points[6];
							back[3] = points[2];
							break;
						case 12:
							nearest[0] = points[15];
							nearest[1] = points[14];
							nearest[2] = points[13];
							nearest[3] = points[12];
							back[0] = points[11];
							back[1] = points[10];
							back[2] = points[9];
							back[3] = points[8];
							break;
					}
					break;
			}


			return new HalfPatchData(back, nearest);
		}

		public override Vector3 GetPoint(double u, double v)
		{
			return EvaluatePointValue(u, v);
		}

		public override Vector3 GetUDerivative(double u, double v)
		{
			return EvaluateUDerivative(u, v);
		}

		public override Vector3 GetVDerivative(double u, double v)
		{
			return EvaluateVDerivative(u, v);
		}

		public override CatPoint GetCatPoint(int u, int v)
		{
			return points[v * 4 + u];
		}
	}
}
