using System.Windows;
using System.Windows.Input;

namespace CadCat.DataStructures
{
	public class ImageMouseController
	{
		SceneData data;
		Point mousePos;
		//bool clickedOnImage;
		private ICommand leftMouseDown;
		private ICommand leftMouseUp;

		public ICommand LeftMouseDownCommand
		{
			get
			{
				return leftMouseDown ?? (leftMouseDown = new Utilities.CommandHandler(LeftMouseDown));
			}
		}

		public ICommand LeftMouseUpCommand
		{
			get
			{
				return leftMouseUp ?? (leftMouseUp = new Utilities.CommandHandler(LeftMouseUp));
			}
		}

		public bool ClickedOnImage { get; set; } = false;

		public ImageMouseController(SceneData data)
		{
			this.data = data;
		}



		private void LeftMouseDown()
		{
			ClickedOnImage = true;
			mousePos = data.MousePosition;
			data.OnLeftMousePressed(new Math.Vector2(mousePos.X / data.ScreenSize.X, mousePos.Y / data.ScreenSize.Y) * 2 - 1);
		}

		private void LeftMouseUp()
		{
			var pos = data.MousePosition;
			var dt = pos - mousePos;
			if(dt.LengthSquared < 10)
			{
				data.SceneClicked(new Math.Vector2(pos.X/data.ScreenSize.X, pos.Y/data.ScreenSize.Y) *2 -1);
			}
			ClickedOnImage = false;
			data.OnLeftMouseReleased();
		}
	}
}
