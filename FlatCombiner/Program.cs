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
        public static List<FlatContainer> Flats { get; set; }
        public static int ValicateCounter { get; private set; }
        public static List<List<string>> SuccessfulCombinations { get; private set; }

        public static int StepLimit = 8;

        static void Main(string[] args)
        {
            string json = System.IO.File.ReadAllText(@"E:\Dropbox\WORK\154_ROBOT\04_Grasshopper\Source\flats-lat-02.json");            
            Flats = JsonConvert.DeserializeObject<List<FlatContainer>>(json);
            Flats.RemoveAll(item => item == null);

            int stepCount = 0;
            

            Stack<FlatContainer> stack = new Stack<FlatContainer>();
            SuccessfulCombinations = new List<List<string>>();

            // запуск действа
            TryAddFlat(stack, stepCount); 

            // сохранение файла
            string savePath = @"E:\Dropbox\WORK\154_ROBOT\07_Import-Export\names_lat_8step-02.txt";
            SaveFile(savePath);

            foreach (var flat in Flats)
            {
                Console.WriteLine(flat.ToString());
            }
            

            
            Console.WriteLine(ValicateCounter.ToString());
            Console.WriteLine(SuccessfulCombinations.Count);
            Console.ReadKey();
        }

        private static void SaveFile(string savePath)
        {
            List<string> combinations = new List<string>();
            foreach (var success in SuccessfulCombinations)
            {
                success.Reverse();                
                var line = string.Join(",", success);
                combinations.Add(line);                
            }
            //List<string> unique = combinations.Distinct().ToList();
            System.IO.File.WriteAllLines(savePath, combinations);
        }

        private static void TryAddFlat(Stack<FlatContainer> stack, int stepCount)
        {
            foreach (var flat in Flats)
            {
                var totalSteps = stepCount + flat.BottomSteps;
                if (totalSteps > StepLimit) continue; //возврат если превышен лимит
                if (totalSteps == StepLimit)
                {
                    stack.Push(flat);
                    Validate(stack); // проверка на подходимость
                }
                else
                {
                    stack.Push(flat);
                    TryAddFlat(stack, totalSteps);
                }
            }
            if(stack.Count > 0) stack.Pop();
           
        }

        private static void Validate(Stack<FlatContainer> stack)
        {
            ValicateCounter ++;

            var arr = stack.ToArray(); // массив для удобства (копия?)
            var rightFlat = stack.Pop(); //вытащить последний            

            //проверка последнего элемента
            if (rightFlat.FType != FlatContainer.FlatLocattionType.CornerRight) return;
            //проверка правого элемента
            var leftFlat = arr[arr.Length-1];
            if (leftFlat.FType != FlatContainer.FlatLocattionType.CornerLeft) return;

            //проверка средних элементов
             
            for (int i=1; i<arr.Length-1; i++)
            {
                if (arr[i].FType != FlatContainer.FlatLocattionType.Middle) return;
            }

            // проверка верхних шагов
            if (!CheckTop(arr)) return;

            //все тесты пройдены!!!
            stack.Push(rightFlat);
            SuccessfulCombinations.Add(stack.Select(t=> t.Id).ToList()); //сохранение id квартир

            stack.Pop();
            return;
        }

        private static bool CheckTop(FlatContainer[] arr)
        {
            switch(StepLimit)
            {
                case 7:
                    if (arr[0].TopSteps != 3) return false; //крайний правый
                    if (arr[arr.Length - 1].TopSteps != 2) return false;
                    return true;
                case 8:
                    if (arr[0].TopSteps != 3) return false; //крайний правый
                    if (arr[arr.Length - 1].TopSteps != 3) return false;
                    return true;
                default:
                    return false;
            }
        }
    }
}
