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
        public Transform transform;

        public void Clear()
        {
            transform = null;
        }
    }
}
