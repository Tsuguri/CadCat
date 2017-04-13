namespace CadCat.DataStructures
{
	struct ClickData
	{
		public double Distance;
		public CatPoint ClickedModel;

		public ClickData(double distance, CatPoint clicked)
		{
			Distance = distance;
			ClickedModel = clicked;
		}
	}
}
