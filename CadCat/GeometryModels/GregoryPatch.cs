using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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

		public CatPoint NormalL2;
		public CatPoint NormalL1;
		public CatPoint NormalR2;
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
			12,16,
			6,15,
			6,17
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
			LeftBack = new CatPoint[4];
			RightNearest = new CatPoint[4];
			RightBack = new CatPoint[4];

			LeftNearest[0] = halfPatch.Nearest[0];

			RightNearest[3] = halfPatch.Nearest[3];

			for (int i = 0; i < 3; i++)
			{
				LeftNearest[i + 1] = scene.CreateHiddenCatPoint(new Vector3());
			}

			RightNearest[0] = LeftNearest[3];

			for (int i = 0; i < 2; i++)
				RightNearest[i + 1] = scene.CreateHiddenCatPoint(new Vector3());

			for (int i = 0; i < 3; i++)
			{
				LeftBack[i] = scene.CreateHiddenCatPoint(new Vector3());
				RightBack[i + 1] = scene.CreateHiddenCatPoint(new Vector3());
			}

			LeftBack[3] = RightBack[0] = scene.CreateHiddenCatPoint(new Vector3());

			for (int i = 0; i < 4; i++)
			{
				halfPatch.Nearest[i].OnChanged += AdjacentHalfPatch_OnChanged;
				halfPatch.Back[i].OnChanged += AdjacentHalfPatch_OnChanged;
			}
			centerPoint.OnChanged += AdjacentHalfPatch_OnChanged;
			P1I = scene.CreateHiddenCatPoint(new Vector3());
			changed = true;
			ActualizePositions();

			NormalL1 = scene.CreateHiddenCatPoint(new Vector3());
			NormalL2 = scene.CreateHiddenCatPoint(new Vector3());
			NormalR1 = scene.CreateHiddenCatPoint(new Vector3());
			NormalR2 = scene.CreateHiddenCatPoint(new Vector3());


			renderPoints = new List<CatPoint>
			{
				LeftNearest[0],
				LeftNearest[1],
				LeftBack[1],
				LeftNearest[2],
				LeftBack[2],
				LeftNearest[3],
				LeftBack[3],
				RightNearest[1],
				RightBack[1],
				RightNearest[2],
				RightBack[2],
				RightNearest[3],
				P1I,centerPoint,
				NormalL1,
				NormalL2,
				NormalR1,
				NormalR2
			};



		}

		private void AdjacentHalfPatch_OnChanged(CatPoint sender)
		{
			changed = true;
		}

		private void ActualizePositions()
		{
			changed = false;

			LeftBack[0].Position = data.Nearest[0].Position * 2 - data.Back[0].Position;
			RightBack[3].Position = data.Nearest[3].Position * 2 - data.Back[3].Position;

			LeftNearest[1].Position = (data.Nearest[0].Position + data.Nearest[1].Position) / 2;
			RightNearest[2].Position = (data.Nearest[2].Position + data.Nearest[3].Position) / 2;

			LeftBack[1].Position = LeftNearest[1].Position * 2 - (data.Back[0].Position + data.Back[1].Position) / 2;
			RightBack[2].Position = RightNearest[2].Position * 2 - (data.Back[3].Position + data.Back[2].Position) / 2;

			var tmpNrCenter = (data.Nearest[1].Position + data.Nearest[2].Position) / 2;
			var tmpBckCenter = (data.Back[1].Position + data.Back[2].Position) / 2;

			LeftNearest[2].Position = (LeftNearest[1].Position + tmpNrCenter) / 2;
			RightNearest[1].Position = (RightNearest[2].Position + tmpNrCenter) / 2;
			LeftBack[2].Position = (LeftBack[1].Position + tmpNrCenter * 2 - tmpBckCenter) / 2;
			RightBack[1].Position = (RightBack[2].Position + tmpNrCenter * 2 - tmpBckCenter) / 2;

			LeftNearest[3].Position = (LeftNearest[2].Position + RightNearest[1].Position) / 2;
			LeftBack[3].Position = (LeftBack[2].Position + RightBack[1].Position) / 2;

			Q = (LeftBack[3].Position * 3 - LeftNearest[3].Position) / 2;
			CalculateP1();
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

			var first = (g0 * (2 / 3.0f) + g1 * (1 / 3.0f)) * (2 / 3.0f) + (g1 * (2 / 3.0f) + g2 * (1 / 3.0f)) * (1 / 3.0f);
			var second = (g0 * (1 / 3.0f) + g1 * (2 / 3.0f)) * (1 / 3.0f) + (g1 * (1 / 3.0f) + g2 * (2 / 3.0f)) * (2 / 3.0f);

			NormalL2.Position = LeftBack[3].Position - first;
			NormalR2.Position = LeftBack[3].Position + first;

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

	}



	class GregoryPatch : Patch
	{
		private PatchCycle cycle;
		private SceneData data;
		private List<AdjacentHalfPatch> adjacentPatches;
		private CatPoint centerPoint;
		public GregoryPatch(PatchCycle cycle, SceneData data)
		{
			this.cycle = cycle;
			this.data = data;
			centerPoint = data.CreateCatPoint(new Vector3(), false);

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



		}

		public override void Render(BaseRenderer renderer)
		{
			adjacentPatches.ForEach(x => x.CalculateP1());
			adjacentPatches.ForEach(x => x.Update());
			adjacentPatches.ForEach(x => x.UpdateNormals());

			base.Render(renderer);

			adjacentPatches.ForEach(x => x.Render(renderer));
		}

		public override string GetName()
		{
			return "Gregory patch: " + base.GetName();
		}
	}
}
