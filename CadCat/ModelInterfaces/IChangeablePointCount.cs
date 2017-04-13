namespace CadCat.ModelInterfaces
{
	interface IChangeablePointCount
	{
		void AddPoint(DataStructures.CatPoint point);
		void RemovePoint(DataStructures.CatPoint point);
	}
}
