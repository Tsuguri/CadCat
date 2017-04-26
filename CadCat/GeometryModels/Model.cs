using System;
using System.Collections.Generic;
using System.Linq;
using CadCat.Math;
using CadCat.DataStructures;

namespace CadCat.GeometryModels
{
	public class Model : Utilities.BindableObject
	{

		public int ModelId
		{
			get;
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
		private bool isSelected;
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

		public IEnumerable<Type> Interfaces => this.GetType().GetInterfaces();

		private static int idCounter = 0;

		public static void ResetId()
		{
			idCounter = 0;
		}

		public Model()
		{
			ModelId = idCounter;
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
			return ModelId.ToString();
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
