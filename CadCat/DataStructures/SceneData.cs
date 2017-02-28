using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CadCat.Math;
using CadCat.Rendering;
using System.Windows.Input;
using System.Windows.Controls;

namespace CadCat.DataStructures
{
	public class Line
	{
		public Vector3 from;
		public Vector3 to;
	}
	public class SceneData
	{
		public Point MousePosition
		{
			get
			{
				return mousePosition;
			}
			set
			{
				previousMousePosition = mousePosition;
				mousePosition = value;
				Delta = mousePosition - previousMousePosition;
			}
		}
		public Vector Delta
		{
			get; set;
		}
		private Point mousePosition;
		private Point previousMousePosition;
		public Camera ActiveCamera
		{
			get; set;
		}
		List<Model> models = new List<Model>();

		public SceneData()
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


			foreach (var model in models)
			{
				modelData.transform = model.transform;
				modelData.ModelID = model.ModelID;
				foreach (var line in model.GetLines())
					yield return line;
			}
			yield break;

		}
		double rotateSpeed = 0.2;
		internal void UpdateFrameData(MenuItem control)
		{
			var delta = Delta;
			control.Header = Delta;
			if (delta.Length > 0.001 && Mouse.LeftButton == MouseButtonState.Pressed)
			{
				if (Keyboard.IsKeyDown(Key.A))
				{
					ActiveCamera.Radius = ActiveCamera.Radius + delta.Y*0.05;
				}
				else if (Keyboard.IsKeyDown(Key.LeftCtrl))
				{
					ActiveCamera.Rotate(delta.Y * rotateSpeed, delta.X * rotateSpeed);

				}
			}

		}
	}
}
