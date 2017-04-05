using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;
using CadCat.Rendering;

namespace CadCat.GeometryModels
{
	class Cube : ParametrizedModel
	{
		private List<Math.Vector3> points;
		private List<int> indices;

		public Cube()
		{
			points = new List<Math.Vector3>();
			points.Add(new Math.Vector3(0, 0, 0));
			points.Add(new Math.Vector3(1, 0, 0));
			points.Add(new Math.Vector3(1, 0, 1));
			points.Add(new Math.Vector3(0, 0, 1));

			points.Add(new Math.Vector3(0, 1, 0));
			points.Add(new Math.Vector3(1, 1, 0));
			points.Add(new Math.Vector3(1, 1, 1));
			points.Add(new Math.Vector3(0, 1, 1));

			indices = new List<int>();
			//floor
			indices.Add(0);
			indices.Add(1);

			indices.Add(1);
			indices.Add(2);

			indices.Add(2);
			indices.Add(3);

			indices.Add(3);
			indices.Add(0);

			//ceiling
			indices.Add(4);
			indices.Add(5);

			indices.Add(5);
			indices.Add(6);

			indices.Add(6);
			indices.Add(7);

			indices.Add(7);
			indices.Add(4);
			
			//walls
			indices.Add(0);
			indices.Add(4);

			indices.Add(1);
			indices.Add(5);

			indices.Add(2);
			indices.Add(6);

			indices.Add(3);
			indices.Add(7);

		}

		public override void Render(BaseRenderer renderer)
		{
			base.Render(renderer);
			renderer.UseIndices = true;
			renderer.Points = points;
			renderer.Indices = indices;
			renderer.ModelMatrix = transform.CreateTransformMatrix();
			renderer.Transform();
			renderer.DrawLines();
		}
	}
}
