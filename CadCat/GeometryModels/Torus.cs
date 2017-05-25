using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CadCat.Math;
using CadCat.ModelInterfaces;
using CadCat.Rendering;

namespace CadCat.GeometryModels
{

	using Real = System.Double;
	class Torus : ParametrizedModel, IIntersectable
	{
		//private List<ModelLine> lines;

		private List<CuttingCurve> cuttingCurves = new List<CuttingCurve>();
		private List<int> indices;
		private List<Math.Vector3> points;
		private readonly List<Vector3> normalMesh = new List<Vector3>();
		private readonly List<int> normalIndices = new List<int>();

		private bool modelReady;

		private Real bigRadius = 5.0;
		private Real smallRadius = 1.0;
		private int bigAngleDensity = 16;
		private int smallAngleDensity = 16;

		private bool drawNormals;

		public bool DrawNormals
		{
			get
			{
				return drawNormals;

			}
			set
			{
				drawNormals = value;
				OnPropertyChanged();
			}
		}

		public Real R
		{
			get
			{
				return bigRadius;
			}
			set
			{
				if (System.Math.Abs(bigRadius - value) > Math.Utils.Eps)
				{
					bigRadius = value;
					modelReady = false;
					OnPropertyChanged();
				}
			}
		}

		public Real r
		{
			get
			{
				return smallRadius;
			}
			set
			{
				if (bigRadius != value)
				{
					smallRadius = value;
					modelReady = false;
					OnPropertyChanged();
				}
			}
		}

		public int RDensity
		{
			get
			{
				return bigAngleDensity;
			}
			set
			{
				if (bigAngleDensity != value)
				{
					bigAngleDensity = value;
					modelReady = false;
					OnPropertyChanged();
				}
			}
		}

		public int rDensity
		{
			get
			{
				return smallAngleDensity;
			}
			set
			{
				if (smallAngleDensity != value)
				{
					smallAngleDensity = value;
					modelReady = false;
					OnPropertyChanged();
				}
			}
		}


		//protected override void PositionChanged()
		//{
		//	base.PositionChanged();
		//	modelReady = false;
		//}

		public Torus()
		{
		}

		struct QuadData
		{
			public int down;
			public int right;
			public int corner;
		}

		private void GenerateModel(Real bigRadius, Real smallRadius, int bigAngleDensity, int smallAngleDensity)
		{
			normalIndices.Clear();
			normalMesh.Clear();
			Real bigStep = Math.Utils.Pi * 2 / (bigAngleDensity - 1);
			Real smallStep = Math.Utils.Pi * 2 / (smallAngleDensity - 1);
			points = new List<Math.Vector3>(bigAngleDensity * smallAngleDensity);

			var ptsAvaiable = GetAvaiablePoints(bigAngleDensity, smallAngleDensity);

			var vertices = new List<Vector2>();
			var vertAvai = new QuadData[smallAngleDensity, bigAngleDensity];

			{
				var vert = new QuadData() { corner = -1, down = -1, right = -1 };
				for (int i = 0; i < smallAngleDensity; i++)
					for (int j = 0; j < bigAngleDensity; j++)
						vertAvai[i, j] = vert;
			}

			for (int i = 0; i < bigAngleDensity - 1; i++)
				for (int j = 0; j < smallAngleDensity - 1; j++)
				{
					if (ptsAvaiable[j, i])
					{
						vertAvai[j, i].corner = vertices.Count;
						vertices.Add(new Vector2(i * bigStep, j * smallStep));

					}

					if (ptsAvaiable[j, i] != ptsAvaiable[j + 1, i])
					{
						vertAvai[j, i].down = vertices.Count;
						vertices.Add(new Vector2(i * bigStep, (j + 0.5) * smallStep));
					}

					if (ptsAvaiable[j, i] != ptsAvaiable[j, i + 1])
					{
						vertAvai[j, i].right = vertices.Count;
						vertices.Add(new Vector2((i + 0.5) * bigStep, j * smallStep));
					}
				}

			for (int i = 0; i < bigAngleDensity - 1; i++)
			{
				if (ptsAvaiable[smallAngleDensity - 1, i])
				{
					vertAvai[smallAngleDensity - 1, i].corner = vertices.Count;
					vertices.Add(new Vector2(i * bigStep, (smallAngleDensity - 1) * smallStep));
				}
				if (ptsAvaiable[smallAngleDensity - 1, i] != ptsAvaiable[smallAngleDensity - 1, i + 1])
				{
					vertAvai[smallAngleDensity - 1, i].right = vertices.Count;
					vertices.Add(new Vector2((i + 0.5) * bigStep, (smallAngleDensity - 1) * smallStep));
				}
			}

			for (int i = 0; i < smallAngleDensity - 1; i++)
			{
				if (ptsAvaiable[i, bigAngleDensity - 1])
				{
					vertAvai[i, bigAngleDensity - 1].corner = vertices.Count;
					vertices.Add(new Vector2((bigAngleDensity - 1) * bigStep, i * smallStep));
				}

				if (ptsAvaiable[i, bigAngleDensity - 1] != ptsAvaiable[i + 1, bigAngleDensity - 1])
				{
					vertAvai[i, bigAngleDensity - 1].down = vertices.Count;
					vertices.Add(new Vector2((bigAngleDensity - 1) * bigStep, (i + 0.5) * smallStep));
				}
			}

			points = vertices.Select(x => CalculatePoint(x.X, x.Y, bigRadius, smallRadius)).ToList();

			indices = new List<int>(bigAngleDensity * smallAngleDensity * 2);

			for (int i = 0; i < bigAngleDensity - 1; i++)
				for (int j = 0; j < smallAngleDensity - 1; j++)
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

					int pa = 0;
				}

			int z = 0;
















			//for (int i = 0; i < bigAngleDensity; i++)
			//{
			//	Real bigAngle = bigStep * i;
			//	for (int j = 0; j < smallAngleDensity; j++)
			//	{
			//		Real smallAngle = j * smallStep;
			//		var pt = CalculatePoint(bigAngle, smallAngle, bigRadius, smallRadius);
			//		points.Insert(i * smallAngleDensity + j, pt);

			//		var normal = Vector3.CrossProduct(UDeriv(bigAngle, smallAngle), VDeriv(bigAngle, smallAngle)).Normalized();
			//		normalMesh.Add(pt);
			//		normalMesh.Add(pt + normal);
			//		normalIndices.Add((i * smallAngleDensity + j) * 2);
			//		normalIndices.Add((i * smallAngleDensity + j) * 2 + 1);
			//	}
			//}
			//for (int i = 0; i < bigAngleDensity; i++)
			//{
			//	int circleStart = i * smallAngleDensity;
			//	for (int j = 0; j < smallAngleDensity - 1; j++)
			//	{
			//		if (ptsAvaiable[j, i] && ptsAvaiable[j + 1, i])
			//		{
			//			indices.Add(circleStart + j);
			//			indices.Add(circleStart + j + 1);
			//		}

			//	}
			//	if (ptsAvaiable[0, i] && ptsAvaiable[ptsAvaiable.GetLength(0) - 1, i])
			//	{
			//		indices.Add(circleStart + smallAngleDensity - 1);
			//		indices.Add(circleStart);
			//	}
			//}

			//int vertexCount = points.Count;
			//for (int i = 0; i < bigAngleDensity; i++)
			//{
			//	int circleStart = i * smallAngleDensity;
			//	for (int j = 0; j < smallAngleDensity; j++)
			//	{
			//		if (ptsAvaiable[j, i] && ptsAvaiable[j, (i + 1) % bigAngleDensity])
			//		{
			//			indices.Add(circleStart + j);
			//			indices.Add((circleStart + j + smallAngleDensity) % vertexCount);
			//		}
			//	}
			//}
			modelReady = true;
		}

		private Math.Vector3 CalculatePoint(Real bigAngle, Real smallAngle, Real bigRadius, Real smallRadius)
		{
			Math.Vector3 ret = new Math.Vector3
			{
				X = System.Math.Cos(bigAngle) * (bigRadius + smallRadius * System.Math.Cos(smallAngle)),
				Y = System.Math.Sin(bigAngle) * (bigRadius + smallRadius * System.Math.Cos(smallAngle)),
				Z = smallRadius * System.Math.Sin(smallAngle)
			};
			return ret;
		}

		private Vector3 CalculateWorldPoint(Real bigAngle, Real smallAngle, Real bigRadius, Real smallRadius)
		{
			return (modelMat * new Vector4(CalculatePoint(bigAngle, smallAngle, bigRadius, smallRadius), 1.0)).ClipToVector3();
		}

		private Matrix4 modelMat = Matrix4.CreateIdentity();
		private Matrix4 normalMat = Matrix4.CreateIdentity();

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);
			if (!modelReady)
				GenerateModel(bigRadius, smallRadius, bigAngleDensity, smallAngleDensity);
			modelMat = Transform.CreateTransformMatrix();
			normalMat = modelMat.Inversed().Transposed();
			renderer.UseIndices = true;

			renderer.ModelMatrix = Transform.CreateTransformMatrix();

			if (DrawNormals)
			{
				renderer.Points = normalMesh;
				renderer.Indices = normalIndices;
				renderer.Transform();
				renderer.DrawLines();
			}
			renderer.Points = points;
			renderer.Indices = indices;
			renderer.Transform();
			renderer.DrawLines();
		}
		public override string GetName()
		{
			return "Torus" + base.GetName();
		}

		public float FirstParamLimit => (float)(Math.Utils.Pi * 2.0);
		public float SecondParamLimit => (float)(Math.Utils.Pi * 2.0);
		public bool FirstParamLooped => true;
		public bool SecondParamLooped => true;

		public Vector3 GetPosition(double firstParam, double secondParam)
		{
			return CalculateWorldPoint(firstParam, secondParam, bigRadius, smallRadius);
		}

		private Vector3 UDeriv(double firstParam, double secondParam)
		{
			Math.Vector3 ret = new Math.Vector3
			{
				X = -System.Math.Sin(firstParam) * (bigRadius + smallRadius * System.Math.Cos(secondParam)),
				Y = System.Math.Cos(firstParam) * (bigRadius + smallRadius * System.Math.Cos(secondParam)),
				Z = 0
			};
			return ret;
		}

		public Vector3 GetFirstParamDerivative(double firstParam, double secondParam)
		{
			return normalMat * UDeriv(firstParam, secondParam);
		}

		private Vector3 VDeriv(double firstParam, double secondParam)
		{
			Math.Vector3 ret = new Math.Vector3
			{
				X = System.Math.Cos(firstParam) * (-smallRadius * System.Math.Sin(secondParam)),
				Y = System.Math.Sin(firstParam) * (-smallRadius * System.Math.Sin(secondParam)),
				Z = smallRadius * System.Math.Cos(secondParam)
			};
			return ret;
		}

		public Vector3 GetSecondParamDerivative(double firstParam, double secondParam)
		{
			return normalMat * VDeriv(firstParam, secondParam);
		}

		public ParametrizedPoint GetClosestPointParams(Vector3 point)
		{
			throw new System.NotImplementedException();
		}

		public Vector2? ConfirmParams(double u, double v)
		{
			if (u > FirstParamLimit)
				u -= FirstParamLimit;
			if (u < 0)
				u += FirstParamLimit;
			if (v > SecondParamLimit)
				v -= SecondParamLimit;
			if (v < 0)
				v += SecondParamLimit;
			return new Vector2(u, v);
		}

		public Vector2 ClipParams(double u, double v)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<ParametrizedPoint> GetPointsForSearch(int firstParamDiv, int secondParamDiv)
		{
			Real bigStep = Math.Utils.Pi * 2 / firstParamDiv;
			Real smallStep = Math.Utils.Pi * 2 / secondParamDiv;
			for (int i = 0; i < bigAngleDensity; i++)
			{
				Real bigAngle = bigStep * i;
				for (int j = 0; j < smallAngleDensity; j++)
				{
					Real smallAngle = j * smallStep;
					yield return new ParametrizedPoint { Parametrization = new Vector2(bigAngle, smallAngle), Position = CalculateWorldPoint(bigAngle, smallAngle, bigRadius, smallRadius) };
				}
			}
		}

		private bool[,] GetAvaiablePoints(int uDiv, int vDiv)
		{
			var pts = new bool[vDiv, uDiv];
			for (int i = 0; i < uDiv; i++)
			{
				for (int j = 0; j < vDiv; j++)
					pts[j, i] = true;
			}
			//var uStep = FirstParamLimit / (uDiv - 1);
			//var vStep = SecondParamLimit / (vDiv - 1);


			//for (int i = 0; i < uDiv; i++)
			//	for (int j = 0; j < vDiv; j++)
			//	{
			//		pts[j, i] = CheckPoint(i * uStep, j * vStep);
			//	}

			//return pts;
			if (cuttingCurves.Count > 0)
				cuttingCurves[0].PointsContainedByCurve(pts, true, this);
			return pts;
		}

		private bool CheckPoint(double u, double v)
		{
			if (cuttingCurves.Count == 0)
				return true;

			return cuttingCurves[0].PointBelongs(false, this, new Vector2(u, v));
		}


		public void SetCuttingCurve(CuttingCurve curve)
		{
			modelReady = false;
			cuttingCurves.Add(curve);
		}
	}
}
