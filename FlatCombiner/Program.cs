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
        public static List<FlatContainer> BottomFlats { get; set; }
        public static List<FlatContainer> TopFlats { get; set; }
        public static int ValidateCounter { get; private set; }
        public static List<List<string>> SuccessfulCombinations { get; private set; }

        
        // определить широтные или меридианальные секции!!!
        private static readonly bool lattitude = true;
        // количество шагов
        public static int StepLimit = 8;

        static void Main(string[] args)
        {
            if (lattitude)
                LattitudeSection(@"E:\Dropbox\WORK\154_ROBOT\04_Grasshopper\Source\flats03.json", @"E:\Dropbox\WORK\154_ROBOT\07_Import-Export\names_01.txt");
            else
                LongitudeBlock();
        }

        private static void LongitudeBlock()
        {
            throw new NotImplementedException();
        }

        private static void LattitudeSection(string sourceFilePath, string outputFilePath)
        {
            string json = System.IO.File.ReadAllText(sourceFilePath);
            List<FlatContainer> AllFlats = JsonConvert.DeserializeObject<List<FlatContainer>>(json);
            AllFlats.RemoveAll(item => item == null);

            SplitFlats(AllFlats);

            int stepCount = 0;
            

            Stack<FlatContainer> stack = new Stack<FlatContainer>();
            SuccessfulCombinations = new List<List<string>>();

            // запуск действа
            TryAddFlat(stack, stepCount);

            // сохранение файла            
            //SaveFile(outputFilePath);

            // всякая хрень для проверки
            foreach (var flat in AllFlats)
            {
                Console.WriteLine(flat.ToString());
            }
            Console.WriteLine(ValidateCounter.ToString());
            Console.WriteLine(SuccessfulCombinations.Count);
            Console.ReadKey();
        }

        /// <summary>
        /// Разделяет квартиры на верхние и нижние
        /// </summary>
        /// <param name="allFlats"></param>
        private static void SplitFlats(List<FlatContainer> allFlats)
        {
            foreach (var item in allFlats)
            {
                if (item.FType == FlatContainer.FlatLocattionType.MiddleUp)
                    TopFlats.Add(item);
                else
                    BottomFlats.Add(item);
            }
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
            foreach (var flat in BottomFlats)
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
            ValidateCounter ++;

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
                if (arr[i].FType != FlatContainer.FlatLocattionType.MiddleDown) return;
            }

            // проверка верхних шагов
            if (lattitude)
            {
                if (!CheckTopLat(arr)) return;
            }
            else // меридианалка
            {
                if (!CheckTopLong(arr)) return;
            }


            //все тесты пройдены!!!
            stack.Push(rightFlat);
            SuccessfulCombinations.Add(stack.Select(t=> t.Id).ToList()); //сохранение id квартир

            stack.Pop();
            return;
        }

        private static bool CheckTopLong(FlatContainer[] arr)
        {
            throw new NotImplementedException();
        }

        private static bool CheckTopLat(FlatContainer[] arr)
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
