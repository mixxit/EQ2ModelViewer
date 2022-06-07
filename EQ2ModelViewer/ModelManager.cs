using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQ2ModelViewer
{
    public class ModelManager
    {
        private UInt32 id;
        private Dictionary<UInt32, Model> model_list = new Dictionary<UInt32, Model>();
        private Dictionary<string, UInt32> id_lookup_list = new Dictionary<string, UInt32>();

        ModelManager()
        {
            id = 0;
        }

        /*public UInt32 AddModel(string file)
        {
            if (id_lookup_list.ContainsKey(file))
                return id_lookup_list[file];

            id_lookup_list[file] = id;
            Model model = new Model();
            //model.Initialize
        }*/
    }
}
