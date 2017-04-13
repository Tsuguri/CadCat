using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CadCat.ModelInterfaces;

namespace CadCat
{
	class DropDownMenuSelector: DataTemplateSelector
	{

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var myObj = item as Type;
			var frameworkElement = (FrameworkElement)container;

			if (myObj != null)
			{
				var name = myObj.Name;


				switch (name)
				{
					case nameof(Test):
						return (DataTemplate)frameworkElement.FindResource("Test");
					case nameof(IChangeablePointCount):
						return (DataTemplate)frameworkElement.FindResource("ChangeablePointCount");
					case nameof(ITypeChangeable):
						return (DataTemplate)frameworkElement.FindResource("TypeChangeable");
					case nameof(IConvertibleToPoints):
						return (DataTemplate) frameworkElement.FindResource("ConvertibleToPoints");
					default:
						return (DataTemplate)frameworkElement.FindResource("Default");
				}
			}

			return (DataTemplate)frameworkElement.FindResource("Default");
		}
	}
}
