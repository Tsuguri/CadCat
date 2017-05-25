using System.Collections.Generic;
using CadCat.GeometryModels;
using CadCat.Math;

namespace CadCat.ModelInterfaces
{
	public struct ParametrizedPoint
	{
		public Vector3 Position;
		public Vector2 Parametrization;
	}

	public interface IIntersectable
	{
		float FirstParamLimit { get; }
		float SecondParamLimit { get; }
		bool FirstParamLooped { get; }
		bool SecondParamLooped { get; }

		Vector3 GetPosition(double firstParam, double secondParam);
		Vector3 GetFirstParamDerivative(double firstParam, double secondParam);
		Vector3 GetSecondParamDerivative(double firstParam, double secondParam);

		ParametrizedPoint GetClosestPointParams(Vector3 point);
		Vector2? ConfirmParams(double u, double v);
		Vector2 ClipParams(double u, double v);

		IEnumerable<ParametrizedPoint> GetPointsForSearch(int firstParamDiv, int secondParamDiv);

		void SetCuttingCurve(CuttingCurve curve);

	}
}
