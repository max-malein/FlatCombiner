using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robot;
using Newtonsoft.Json;
using Rhino;
using Rhino.Geometry;


namespace FlatCombiner
{
    class Program
    {
        static void Main(string[] args)
        {            
            string json = System.IO.File.ReadAllText(@"D:\Dropbox\WORK\154_ROBOT\04_Grasshopper\Source\flats03.json");
            var flats = JsonConvert.DeserializeObject<List<FlatContainer>>(json);
            flats.RemoveAll(item => item == null);



            foreach (var flat in flats)
            {                
                Console.WriteLine(flat.ToString());
            }
            Console.ReadKey();
        }
    }
}
