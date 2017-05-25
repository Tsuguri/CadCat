using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using CadCat.Math;
using CadCat.ModelInterfaces;
using CadCat.Rendering;
using CadCat.Utilities;

namespace CadCat.GeometryModels
{

	using Real = System.Double;
	class Torus : ParametrizedModel, IIntersectable
	{
		//private List<ModelLine> lines;
		private bool eachOrAny;

		public bool EachOrAny
		{
			get { return eachOrAny; }
			set
			{
				if (value != eachOrAny)
				{
					modelReady = false;
					eachOrAny = value;
					OnPropertyChanged();
				}
			}
		}
		private readonly ObservableCollection<CuttingCurveWrapper> cuttingCurves = new ObservableCollection<CuttingCurveWrapper>();

		public ObservableCollection<CuttingCurveWrapper> CuttingCurves => cuttingCurves;
		private List<int> indices;
		private List<Math.Vector3> points;

		private bool modelReady;

		private Real bigRadius = 5.0;
		private Real smallRadius = 1.0;
		private int bigAngleDensity = 16;
		private int smallAngleDensity = 16;

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


		public Torus()
		{
		}


		private void GenerateModel(Real bigRadius, Real smallRadius, int bigAngleDensity, int smallAngleDensity)
		{
			var ptsAvaiable = GetAvaiablePoints(bigAngleDensity, smallAngleDensity);


			var tup = SurfaceFilling.MarchingAszklars(ptsAvaiable, FirstParamLimit, SecondParamLimit, true, true);

			this.indices = tup.Item2;
			points = tup.Item1.Select(x => CalculatePoint(x.X, x.Y, bigRadius, smallRadius)).ToList();
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
			renderer.SelectedColor = IsSelected ? Colors.LimeGreen : Colors.White;
			renderer.ModelMatrix = Transform.CreateTransformMatrix();
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

		public void SideChanged()
		{
			modelReady = false;
		}

		public void RemoveCurve(CuttingCurve curve)
		{
			var p = CuttingCurves.Where(x => x.curve == curve);
			var cur = p.FirstOrDefault();
			if (cur != null)
				cuttingCurves.Remove(cur);
			SideChanged();
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
			bool val = eachOrAny || cuttingCurves.Count == 0;
			for (int i = 0; i < uDiv; i++)
			{
				for (int j = 0; j < vDiv; j++)
					pts[j, i] = val;
			}

			foreach (var cuttingCurveWrapper in cuttingCurves)
			{
				cuttingCurveWrapper.curve.PointsContainedByCurve(pts, cuttingCurves[0].Side, this, 0, FirstParamLimit, 0, SecondParamLimit, eachOrAny);
			}
			return pts;
		}

		public void SetCuttingCurve(CuttingCurve curve)
		{
			if (curve.IsIntersectable(this))
			{
				cuttingCurves.Add(new CuttingCurveWrapper(curve, this));
				modelReady = false;
			}
		}
	}
}
