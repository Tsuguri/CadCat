using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadCat.DataStructures;

namespace CadCat.Rendering
{
    public class ModelData
    {
        public DataStructures.SpatialData.Transform transform;
		public int ModelID = 0;

        public void Clear()
        {
            transform = null;
			ModelID = -1;
        }
    }
}
