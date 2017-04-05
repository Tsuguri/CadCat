using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CadCat
{
	class DropDownMenuSelector: DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var myObj = item as string;
			var frameworkElement = (FrameworkElement)container;

			if (myObj != null)
			{
				switch (myObj)
				{
					case "CadCat.ModelInterfaces.Test":
						return (DataTemplate)frameworkElement.FindResource("Test");
					case "CadCat.ModelInterfaces.IChangeablePointCount":
						return (DataTemplate)frameworkElement.FindResource("ChangeablePointCount");
					case "CadCat.ModelInterfaces.ITypeChangeable":
						return (DataTemplate)frameworkElement.FindResource("TypeChangeable");
					default:
						return (DataTemplate)frameworkElement.FindResource("Default");
				}
			}

			return (DataTemplate)frameworkElement.FindResource("Default");
		}
	}
}
