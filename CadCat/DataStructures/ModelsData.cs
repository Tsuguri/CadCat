using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CadCat.Math;

namespace CadCat.DataStructures
{
	public class Line
	{
		public Vector3 from;
		public Vector3 to;
	}
	public class ModelsData
	{
        List<Model> models = new List<Model>();
		public ModelsData()
		{
		}

        public void AddModel(Model model)
        {
			models.Add(model);
        }

        public void RemoveModel(Model model)
        {
			models.Remove(model);
        }

		public IEnumerable<Line> GetLines(Rendering.ModelData modelData)
		{


			foreach(var model in models)
			{
				modelData.transform = model.transform;
				modelData.ModelID = model.ModelID;
				foreach (var line in model.GetLines())
					yield return line;
			}
			yield break;

		}


	}
}
