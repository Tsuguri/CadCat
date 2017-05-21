﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		IEnumerable<ParametrizedPoint> GetPointsForSearch(int firstParamDiv, int secondParamDiv);

	}
}