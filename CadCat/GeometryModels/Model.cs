using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.Math;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{


	internal struct ModelLine
	{
		public int from;
		public int to;

		public ModelLine(int from, int to)
		{
			this.from = from;
			this.to = to;
		}
	}
	public class Model : Utilities.BindableObject
	{

		public int ModelID
		{
			get;
			private set;
		}
		private string name;
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
				OnPropertyChanged();
			}
		}
		private bool isSelected = false;
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
				OnPropertyChanged();
			}
		}

		public IEnumerable<string> Interfaces
		{
			get
			{
				return this.GetType().GetInterfaces().Select(x=> x.ToString());
			}
		}

		private static int idCounter = 0;

		public Model()
		{
			ModelID = idCounter;
			name = GetName();
			idCounter++;
		}
		public virtual void Render(Rendering.BaseRenderer renderer)
		{
			renderer.UseIndices = false;
		}

		public virtual bool Collide(Ray ray, out double distance)
		{
			distance = 0.0;
			return false;
		}

		public virtual IEnumerable<CatPoint> EnumerateCatPoints()
		{
			return Enumerable.Empty<CatPoint>();
		}

		public virtual string GetName()
		{
			return ModelID.ToString();
		}

		public virtual Matrix4 GetMatrix(bool overrideScale, Vector3 newScale)
		{
			return Matrix4.CreateIdentity();
		}

		public virtual void CleanUp()
		{

		}
	}
}
