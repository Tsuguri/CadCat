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
using CadCat.GeometryModels;
using System.Collections.ObjectModel;

namespace CadCat.DataStructures
{
	public class Line
	{
		public Vector3 from;
		public Vector3 to;
	}
	public class SceneData : Utilities.BindableObject
	{
		public RendererType RenderingMode
		{
			get;set;
		}
		private BaseRenderer renderer = null;
		public BaseRenderer Renderer
		{
			get
			{
				return renderer;
			}
			set
			{
				renderer = value;
				OnPropertyChanged();
			}
		}




		#region Models

		private ObservableCollection<Model> models = new ObservableCollection<Model>();
		public ObservableCollection<Model> Models { get { return models; } }

		private Model selectedModel = null;
		public Model SelectedModel
		{
			get
			{
				return selectedModel;
			}
			set
			{
				selectedModel = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region MouseData

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
				OnPropertyChanged();
			}
		}
		public Vector Delta
		{
			get; set;
		}
		private Point mousePosition;
		private Point previousMousePosition;

		#endregion

		#region Commands

		private ICommand createTorusCommand;
		private ICommand createCubeCommand;
		private ICommand goToSelectedCommand;

		public ICommand CreateTorusCommand
		{
			get
			{
				return createTorusCommand ?? (createTorusCommand = new Utilities.CommandHandler(CreateTorus));
			}
		}

		public ICommand CreateCubeCommand
		{
			get
			{
				return createCubeCommand ?? (createCubeCommand = new Utilities.CommandHandler(CreateCube));
			}
		}

		public ICommand GoToSelectedCommand
		{
			get
			{
				return goToSelectedCommand ?? (goToSelectedCommand = new Utilities.CommandHandler(GoToSelected));
			}
		}

		#endregion

		#region CreatingModels

		private void AddNewModel(Model model)
		{
			model.transform.Position = ActiveCamera.LookingAt;
			models.Add(model);
			SelectedModel = model;
		}
		private void CreateTorus()
		{
			AddNewModel(new Torus());
		}

		private void CreateCube()
		{
			AddNewModel(new Cube());
		}

		#endregion

		double rotateSpeed = 0.2;


		public Camera ActiveCamera
		{
			get; set;
		}

		public SceneData()
		{
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


		internal void UpdateFrameData()
		{
			var delta = Delta;
			if (delta.X != -1 && delta.Length > 0.001 && Mouse.LeftButton == MouseButtonState.Pressed)
			{
				if (Keyboard.IsKeyDown(Key.A))
				{
					ActiveCamera.Radius = ActiveCamera.Radius + delta.Y * 0.05;
				}
				else if (Keyboard.IsKeyDown(Key.LeftCtrl))
				{
					ActiveCamera.Rotate(delta.Y * rotateSpeed, delta.X * rotateSpeed);

				}
				else if (Keyboard.IsKeyDown(Key.LeftAlt))
				{
					ActiveCamera.Move(0, 0, delta.Y);
				}
				else
				{
					ActiveCamera.Move(delta.X, delta.Y);
				}
			}

		}

		private void GoToSelected()
		{
			if (SelectedModel != null)
			{
				ActiveCamera.LookingAt = SelectedModel.transform.Position;
			}
		}
	}
}
