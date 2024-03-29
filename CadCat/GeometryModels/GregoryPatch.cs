﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using CadCat.DataStructures;
using CadCat.Math;
using CadCat.Rendering;

namespace CadCat.GeometryModels
{
	class PatchCycle
	{
		public List<CatPoint> Points;
		public List<BezierPatch> Patches;

		public PatchCycle(List<BezierPatch> patches, List<CatPoint> points)
		{
			this.Patches = patches;
			this.Points = points;
		}
	}

	class AdjacentHalfPatch
	{

		private bool changed;
		public CatPoint[] LeftNearest;
		public CatPoint[] LeftBack;
		public CatPoint[] RightNearest;
		public CatPoint[] RightBack;

		public CatPoint P1I;
		public Vector3 Q;
		public CatPoint leftP1;
		public CatPoint rightP1;

		//public CatPoint NormalL2;
		public CatPoint NormalL1;
		//public CatPoint NormalR2;
		public CatPoint NormalR1;

		private List<CatPoint> renderPoints;



		private static List<int> indices = new List<int>
		{
			0,1,
			1,2,
			1,3,
			3,4,
			3,5,
			5,6,
			5,7,
			7,8,
			7,9,
			9,10,
			9,11,
			6,12,
			12,13,
			12,14,
			12,15,
			6,4,
			6,8
		};

		private CatPoint centerPoint;

		private HalfPatchData data;
		private SceneData scene;

		public AdjacentHalfPatch(HalfPatchData halfPatch, SceneData scene, CatPoint centerPoint)
		{
			this.data = halfPatch;
			this.scene = scene;
			this.centerPoint = centerPoint;
			LeftNearest = new CatPoint[4];
			LeftBack = new CatPoint[3];
			RightNearest = new CatPoint[4];
			RightBack = new CatPoint[3];

			LeftNearest[0] = halfPatch.Nearest[0];

			RightNearest[3] = halfPatch.Nearest[3];

			for (int i = 0; i < 3; i++)
			{
				LeftNearest[i + 1] = scene.CreateHiddenCatPoint(new Vector3());
			}

			RightNearest[0] = LeftNearest[3];

			for (int i = 0; i < 2; i++)
				RightNearest[i + 1] = scene.CreateHiddenCatPoint(new Vector3());

			for (int i = 0; i < 2; i++)
			{
				LeftBack[i] = scene.CreateHiddenCatPoint(new Vector3());
				RightBack[i + 1] = scene.CreateHiddenCatPoint(new Vector3());
			}

			LeftBack[2] = RightBack[0] = scene.CreateHiddenCatPoint(new Vector3());

			for (int i = 0; i < 4; i++)
			{
				halfPatch.Nearest[i].OnChanged += AdjacentHalfPatch_OnChanged;
				halfPatch.Back[i].OnChanged += AdjacentHalfPatch_OnChanged;
			}
			centerPoint.OnChanged += AdjacentHalfPatch_OnChanged;
			P1I = scene.CreateHiddenCatPoint(new Vector3());
			//P1I.W = 6;
			changed = true;
			ActualizePositions();

			NormalL1 = scene.CreateHiddenCatPoint(new Vector3());
			NormalR1 = scene.CreateHiddenCatPoint(new Vector3());

			renderPoints = new List<CatPoint>
			{
				LeftNearest[0],
				LeftNearest[1],
				LeftBack[0],
				LeftNearest[2],
				LeftBack[1],
				LeftNearest[3],
				LeftBack[2],
				RightNearest[1],
				RightBack[1],
				RightNearest[2],
				RightBack[2],
				RightNearest[3],
				P1I,centerPoint,
				NormalL1,
				NormalR1,
			};



		}

		private void AdjacentHalfPatch_OnChanged(CatPoint sender)
		{
			changed = true;
		}

		private void ActualizePositions()
		{
			changed = false;

			LeftNearest[1].Position = (data.Nearest[0].Position + data.Nearest[1].Position) / 2;
			RightNearest[2].Position = (data.Nearest[2].Position + data.Nearest[3].Position) / 2;

			LeftBack[0].Position = LeftNearest[1].Position * 2 - (data.Back[0].Position + data.Back[1].Position) / 2;
			RightBack[2].Position = RightNearest[2].Position * 2 - (data.Back[3].Position + data.Back[2].Position) / 2;

			var tmpNrCenter = (data.Nearest[1].Position + data.Nearest[2].Position) / 2;
			var tmpBckCenter = (data.Back[1].Position + data.Back[2].Position) / 2;

			LeftNearest[2].Position = (LeftNearest[1].Position + tmpNrCenter) / 2;
			RightNearest[1].Position = (RightNearest[2].Position + tmpNrCenter) / 2;
			LeftBack[1].Position = (LeftBack[0].Position + tmpNrCenter * 2 - tmpBckCenter) / 2;
			RightBack[1].Position = (RightBack[2].Position + tmpNrCenter * 2 - tmpBckCenter) / 2;

			LeftNearest[3].Position = (LeftNearest[2].Position + RightNearest[1].Position) / 2;
			LeftBack[2].Position = (LeftBack[1].Position + RightBack[1].Position) / 2;

			Q = (LeftBack[2].Position * 3 - LeftNearest[3].Position) / 2;
		}

		public void Update()
		{
			if (changed)
				ActualizePositions();
		}

		public void UpdateNormals()
		{
			var g0 = (rightP1.Position - leftP1.Position) / 2;
			var g2 = (RightNearest[1].Position - LeftNearest[2].Position) / 2;
			var g1 = (g0 + g2) / 2;

			var second = (g0 * (1 / 3.0f) + g1 * (2 / 3.0f)) * (1 / 3.0f) + (g1 * (1 / 3.0f) + g2 * (2 / 3.0f)) * (2 / 3.0f);

			NormalL1.Position = P1I.Position - second;
			NormalR1.Position = P1I.Position + second;

		}

		public void CalculateP1()
		{
			P1I.Position = (centerPoint.Position + Q * 2) / 3;
		}

		public void Render(BaseRenderer renderer)
		{
			renderer.UseIndices = true;
			renderer.SelectedColor = Colors.BurlyWood;
			renderer.Points = renderPoints.Select(x => x.Position).ToList();
			renderer.Indices = indices;


			renderer.ModelMatrix = Matrix4.CreateIdentity();
			renderer.Transform();
			renderer.DrawLines();
		}

		public void UpdatePointVisibility(bool value)
		{
			foreach (var catPoint in LeftNearest.Concat(LeftBack).Concat(RightNearest).Concat(RightBack))
			{
				catPoint.Visible = value;
			}

			P1I.Visible = value;
			NormalL1.Visible = value;
			NormalR1.Visible = value;
		}

		public void CleanUp()
		{
			scene.RemovePoint(NormalL1);
			scene.RemovePoint(NormalR1);
			scene.RemovePoint(P1I);

			foreach (var catPoint in LeftBack.Concat(RightBack))
			{
				scene.RemovePoint(catPoint);
			}

			scene.RemovePoint(centerPoint);
			scene.RemovePoint(LeftNearest[1]);
			scene.RemovePoint(LeftNearest[2]);
			scene.RemovePoint(LeftNearest[3]);
			scene.RemovePoint(RightNearest[1]);
			scene.RemovePoint(RightNearest[2]);


		}
	}

	class SingleGregoryPatch
	{

		private bool changed = false;

		private GregoryPatch parent;

		private CatPoint p00;
		private CatPoint p01;
		private CatPoint p02;
		private CatPoint p03;

		private CatPoint p10;
		private CatPoint p20;
		private CatPoint p30;

		private CatPoint p13;
		private CatPoint p23;
		private CatPoint p31;
		private CatPoint p32;
		private CatPoint p33;

		private CatPoint p01P;
		private CatPoint p02P;
		private CatPoint p13P;
		private CatPoint p23P;
		private CatPoint p10P;
		private CatPoint p20P;
		private CatPoint p31P;
		private CatPoint p32P;

		private readonly List<Vector3> mesh = new List<Vector3>();
		private readonly List<int> meshIndices = new List<int>();

		public SingleGregoryPatch(GregoryPatch parent, AdjacentHalfPatch left, AdjacentHalfPatch right, CatPoint centerPoint)
		{
			p00 = left.RightNearest[3];
			p00.OnChanged += PointOnChanged;
			p10 = left.RightNearest[2];
			p10.OnChanged += PointOnChanged;
			p20 = left.RightNearest[1];
			p20.OnChanged += PointOnChanged;
			p30 = left.RightNearest[0];
			p30.OnChanged += PointOnChanged;

			p01 = right.LeftNearest[1];
			p01.OnChanged += PointOnChanged;
			p02 = right.LeftNearest[2];
			p02.OnChanged += PointOnChanged;
			p03 = right.LeftNearest[3];
			p03.OnChanged += PointOnChanged;

			p13 = right.LeftBack[2];
			p13.OnChanged += PointOnChanged;
			p23 = right.P1I;
			p23.OnChanged += PointOnChanged;
			p33 = centerPoint;
			p33.OnChanged += PointOnChanged;

			p31 = left.RightBack[0];
			p31.OnChanged += PointOnChanged;
			p32 = left.P1I;
			p32.OnChanged += PointOnChanged;

			p01P = right.LeftBack[0];
			p01P.OnChanged += PointOnChanged;
			p02P = right.LeftBack[1];
			p02P.OnChanged += PointOnChanged;
			p13P = right.LeftBack[1];
			p13P.OnChanged += PointOnChanged;
			p23P = right.NormalL1;
			p23P.OnChanged += PointOnChanged;

			p10P = left.RightBack[2];
			p10P.OnChanged += PointOnChanged;
			p20P = left.RightBack[1];
			p20P.OnChanged += PointOnChanged;
			p31P = left.RightBack[1];
			p31P.OnChanged += PointOnChanged;
			p32P = left.NormalR1;
			p32P.OnChanged += PointOnChanged;

			this.parent = parent;
			parent.PropertyChanged += Parent_PropertyChanged;

			RecalculateMesh();
		}

		private void Parent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(GregoryPatch.HeightDiv) || e.PropertyName == nameof(GregoryPatch.WidthDiv))
				changed = true;
		}

		private void RecalculateMesh()
		{
			changed = false;
			int widthdiv = parent.WidthDiv;
			int heightdiv = parent.HeightDiv;

			mesh.Clear();
			mesh.Capacity = System.Math.Max(mesh.Capacity, (widthdiv + 1) * (heightdiv + 1));
			meshIndices.Clear();
			double widthStep = 1.0 / widthdiv;
			double heightStep = 1.0 / heightdiv;
			int widthPoints = widthdiv + 1;
			int heightPoints = heightdiv + 1;

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


		}

		private Vector4 EvaluateH(double t)
		{
			return new Vector4(
				1 + t * t * (2 * t - 3),
				t * t * (3 - 2 * t),
				t * (1 + t * (t - 2)),
				t * t * (t - 1)
				);
		}

		private Vector4 EvaluateB(double t)
		{
			var tm = 1 - t;
			return new Vector4(
				tm * tm * tm,
				3 * t * tm * tm,
				3 * t * t * tm,
				t * t * t
				);
		}

		private Vector3 EvaluatePointValue(double u, double v)
		{
			if (System.Math.Abs(System.Math.Abs(u - 0.5) - 0.5) < double.Epsilon && System.Math.Abs(System.Math.Abs(v - 0.5) - 0.5) < double.Epsilon)
			{
				if (u > 0.5)
				{
					if (v > 0.5)
						return p33.Position;
					return p03.Position;
				}
				if (v > 0.5)
					return p30.Position;
				return p00.Position;
			}

			var uh = EvaluateB(u);
			var vh = EvaluateB(v);

			// baza berensteina:

			Vector4 firstRow = p00.WPos * vh.X + p10.WPos * vh.Y + p20.WPos * vh.Z + p30.WPos * vh.W;
			Vector4 secondRow = p01.WPos * vh.X + (p01P.WPos * u + p10P.WPos * v) / (u + v) * vh.Y + (p20P.WPos * (1 - v) + p31P.WPos * u) / (1 - v + u) * vh.Z + p31.WPos * vh.W;
			Vector4 thirdRow = p02.WPos * vh.X + (p02P.WPos * (1 - u) + p13P.WPos * v) / (1 - u + v) * vh.Y + (p23P.WPos * (1 - v) + p32P.WPos * (1 - u)) / (2 - u - v) * vh.Z + p32.WPos * vh.W;

			Vector4 fourthRow = p03.WPos * vh.X + p13.WPos * vh.Y + p23.WPos * vh.Z + p33.WPos * vh.W;

			var result = firstRow * uh.X + secondRow * uh.Y + thirdRow * uh.Z + fourthRow * uh.W;
			return result.ToNormalizedVector3();
		}
		private void PointOnChanged(CatPoint sender)
		{
			changed = true;
		}

		public void Render(BaseRenderer renderer)
		{
			if (changed)
				RecalculateMesh();

			renderer.Points = mesh;
			renderer.Indices = meshIndices;
			renderer.UseIndices = true;
			renderer.SelectedColor = Colors.Crimson;

			renderer.Transform();
			renderer.DrawLines();
		}
	}

	class GregoryPatch : Model
	{
		private PatchCycle cycle;
		private SceneData data;
		private readonly List<AdjacentHalfPatch> adjacentPatches;
		private readonly CatPoint centerPoint;
		private readonly List<SingleGregoryPatch> gregoryPatches;

		private bool showPoints = true;

		public bool ShowPoints
		{
			get
			{
				return showPoints;
			}
			set
			{
				showPoints = value;
				OnPropertyChanged();
				adjacentPatches.ForEach(x => x.UpdatePointVisibility(value));
				centerPoint.Visible = value;

			}
		}

		private bool showPolygon = true;

		public bool ShowPolygon
		{
			get { return showPolygon; }
			set
			{
				showPolygon = value;
				OnPropertyChanged();
			}
		}

		private int widthDiv = 2;
		private int heightDiv = 2;

		public int WidthDiv
		{
			get { return widthDiv; }
			set
			{
				if (value < 1 || value > 50)
					return;
				widthDiv = value;
				OnPropertyChanged();
			}
		}

		public int HeightDiv
		{
			get { return heightDiv; }
			set
			{
				if (value < 1 || value > 50)
					return;
				heightDiv = value;
				OnPropertyChanged();
			}
		}

		private double lw = 1.0;
		public double LW
		{
			get { return lw; }
			set
			{
				lw = value;
				adjacentPatches.ForEach(x =>
				{
					x.NormalL1.W = lw;
				});
				OnPropertyChanged();
			}
		}
		private double rw = 1.0;
		public double RW
		{
			get { return rw; }
			set
			{
				rw = value;
				adjacentPatches.ForEach(x =>
				{
					x.NormalR1.W = rw;
				});
				OnPropertyChanged();
			}
		}

		private double w2 = 1.0;
		public double W2
		{
			get { return w2; }
			set
			{
				w2 = value;
				adjacentPatches.ForEach(x =>
				{
					x.P1I.W = w2;
				});
				OnPropertyChanged();
			}
		}
		public GregoryPatch(PatchCycle cycle, SceneData data)
		{
			this.cycle = cycle;
			this.data = data;
			centerPoint = data.CreateHiddenCatPoint(new Vector3());

			adjacentPatches = new List<AdjacentHalfPatch>(cycle.Patches.Count);

			for (int i = 0; i < cycle.Patches.Count; i++)
			{
				adjacentPatches.Add(new AdjacentHalfPatch(cycle.Patches[i].GetDataBetweenPoints(cycle.Points[i], cycle.Points[(i + 1) % cycle.Points.Count]), data, centerPoint));
			}

			for (int i = 0; i < adjacentPatches.Count; i++)
			{
				int l = i - 1;
				if (l < 0)
					l += adjacentPatches.Count;
				adjacentPatches[i].leftP1 = adjacentPatches[l].P1I;
				adjacentPatches[i].rightP1 = adjacentPatches[(i + 1) % adjacentPatches.Count].P1I;
			}

			Vector3 sum = new Vector3();
			adjacentPatches.ForEach(x => sum += x.Q);
			sum /= adjacentPatches.Count;
			centerPoint.Position = sum;
			adjacentPatches.ForEach(x => x.CalculateP1());
			gregoryPatches = new List<SingleGregoryPatch>(cycle.Patches.Count);


			for (int i = 0; i < adjacentPatches.Count; i++)
			{
				gregoryPatches.Add(new SingleGregoryPatch(this, adjacentPatches[i], adjacentPatches[(i + 1) % adjacentPatches.Count], centerPoint));
			}

		}

		public override void Render(BaseRenderer renderer)
		{
			adjacentPatches.ForEach(x => x.Update());
			Vector3 sum = new Vector3();
			adjacentPatches.ForEach(x => sum += x.Q);
			sum /= adjacentPatches.Count;
			centerPoint.Position = sum;
			adjacentPatches.ForEach(x => x.CalculateP1());
			adjacentPatches.ForEach(x => x.UpdateNormals());

			base.Render(renderer);
			if (ShowPolygon)
				adjacentPatches.ForEach(x => x.Render(renderer));

			gregoryPatches.ForEach(x => x.Render(renderer));
		}

		public override void CleanUp()
		{
			base.CleanUp();
			adjacentPatches.ForEach(x => x.CleanUp());
		}

		public override string GetName()
		{
			return "Gregory patch: " + base.GetName();
		}
	}
}
