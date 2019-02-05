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
        //======================== задать входящие вручную!
        private static readonly bool lattitude = false;
        public static int StepLimit = 8; // количество шагов
        private static readonly string sourceFilePath = @"E:\Dropbox\WORK\154_ROBOT\04_Grasshopper\Source\flats-lon-01.json";
        private static readonly string outputFilePath = @"E:\Dropbox\WORK\154_ROBOT\07_Import-Export\flats-lon-01.txt";
        //====================================================================

        public static List<FlatContainer> BottomFlats { get; set; }
        public static List<FlatContainer> TopFlats { get; set; }
        public static int ValidateCounter { get; private set; }
        public static List<List<string>> SuccessfulCombinations { get; private set; }

        private static int TopLeftLength = 0;
        private static int TopRightLength = 0;

        static void Main(string[] args)
        {

            if (!lattitude)
            {
                // врехние шаги для меридианалок
                switch (StepLimit)
                {
                    case 8:
                        TopLeftLength = 3;
                        TopRightLength = 3;
                        break;
                    case 9:
                        TopLeftLength = 3;
                        TopRightLength = 4;
                        break;
                    case 10:
                        TopLeftLength = 4;
                        TopRightLength = 4;
                        break;
                    default:
                        break;
                }
            }
                
                
            string json = System.IO.File.ReadAllText(sourceFilePath);
            List<FlatContainer> AllFlats = JsonConvert.DeserializeObject<List<FlatContainer>>(json);
            AllFlats.RemoveAll(item => item == null);

            SplitFlats(AllFlats);

            int stepCount = 0;

            Stack<FlatContainer> stack = new Stack<FlatContainer>();
            SuccessfulCombinations = new List<List<string>>();

            // запуск действа
            TryAddBottomFlat(stack, stepCount);

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


        private static void LongitudeBlock()
        {
            throw new NotImplementedException();
        }

        private static void LattitudeSection(string sourceFilePath, string outputFilePath)
        {
            
        }

        /// <summary>
        /// Разделяет квартиры на верхние и нижние
        /// </summary>
        /// <param name="allFlats"></param>
        private static void SplitFlats(List<FlatContainer> allFlats)
        {
            TopFlats = new List<FlatContainer>();
            BottomFlats = new List<FlatContainer>();
            foreach (var item in allFlats)
            {
                if (item.FType == FlatContainer.FlatLocattionType.MiddleUp ||
                    item.FType == FlatContainer.FlatLocattionType.CornerLeftUp ||
                    item.FType == FlatContainer.FlatLocattionType.CornerRightUp)
                    TopFlats.Add(item);
                else
                    BottomFlats.Add(item);
            }
            if (TopFlats !=null) // добавить ЛЛУ в верхние квартиры
            {
                var llu = new FlatContainer()
                {
                    Id = "llu",
                    TopSteps = 2,
                    FType = FlatContainer.FlatLocattionType.MiddleUp
                };
                TopFlats.Add(llu); 
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

        private static void TryAddBottomFlat(Stack<FlatContainer> stack, int stepCount)
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
                    TryAddBottomFlat(stack, totalSteps);
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
            if (rightFlat.FType != FlatContainer.FlatLocattionType.CornerRight || rightFlat.FType != FlatContainer.FlatLocattionType.CornerRightDown) return;
            //проверка левого элемента
            var leftFlat = arr[arr.Length-1];
            if (leftFlat.FType != FlatContainer.FlatLocattionType.CornerLeft || leftFlat.FType != FlatContainer.FlatLocattionType.CornerLeftDown) return;

            //проверка средних элементов
             
            for (int i=1; i<arr.Length-1; i++)
            {
                if (arr[i].FType != FlatContainer.FlatLocattionType.MiddleDown) return;
            }

            // проверка верхних шагов
            if (lattitude)
            {
                if (!CheckTopLat(arr)) return;
                
                //все тесты пройдены!!!
                stack.Push(rightFlat);
                SuccessfulCombinations.Add(stack.Select(t => t.Id).ToList()); //сохранение id квартир

                stack.Pop();
                return;
            }
            else // меридианалка
            {
                AddTopLong(arr);
                return;
            }
        }

        private static void AddTopLong(FlatContainer[] arr)
        {
            
            var leftFlat = arr[arr.Length - 1];
            var rightFlat = arr[0];

            //если есть короткие торцевые, то отдельный подсчет
            if (leftFlat.FType != FlatContainer.FlatLocattionType.CornerLeft ||
                rightFlat.FType != FlatContainer.FlatLocattionType.CornerRight)
            {
                TryAddTopFlat(arr);
                return;
            }

            //если нет коротких торцевых
                        
            if (TopLeftLength - leftFlat.TopSteps == 1 || TopRightLength - rightFlat.TopSteps == 1)
                return; // одного шага недостаточно для того чтобы всунуть квартиру

            FlatContainer topLeftFlat = null;
           

            //если левая угловая квартира до лестницы, то сразу смотри правые 
            if (leftFlat.TopSteps == TopLeftLength)
                TryAddTopRight(arr, topLeftFlat);
            else // добавить левые верхние квартиры
            {
                foreach (var item in TopFlats)
                {
                    if (leftFlat.TopSteps + item.TopSteps == TopLeftLength)
                    {
                        TryAddTopRight(arr, topLeftFlat);
                    }
                }
            }

        }

        private static void TryAddTopRight(FlatContainer[] arr, FlatContainer topLeftFlat)
        {

            FlatContainer topRightFlat = null;
            if (arr[0].TopSteps == TopRightLength)
                AddSuccessfullLongitudeCombination(arr,
                    new List<FlatContainer>() { topLeftFlat },
                    new List<FlatContainer>() { topRightFlat });
            else
            {
                foreach (var item in TopFlats)
                {
                    if (arr[0].TopSteps + item.TopSteps == TopRightLength)
                    {
                        AddSuccessfullLongitudeCombination(arr, 
                            new List<FlatContainer>() { topLeftFlat }, 
                            new List<FlatContainer>() { item });
                    }
                }
            }
        }

        private static void AddSuccessfullLongitudeCombination(FlatContainer[] arr, List<FlatContainer> topLeftFlats, List<FlatContainer> topRightFlats)
        {
            var successfulComb = new List<FlatContainer>();
            if (topRightFlats != null)
                successfulComb.AddRange(topRightFlats);
            if (topLeftFlats != null)
                successfulComb.AddRange(topLeftFlats);            
            successfulComb.AddRange(arr);

            //добавить в основной список
            SuccessfulCombinations.Add(successfulComb.Select(t => t.Id).ToList());
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

        private static void TryAddTopFlat(FlatContainer[] arr)
        {

            var TopLeftFlats = new List<FlatContainer>();
            var TopRightFlats = new List<FlatContainer>();

            //левые хаты
            var leftFlat = arr[arr.Length - 1];
            if (leftFlat.FType == FlatContainer.FlatLocattionType.CornerLeft)
            {
                if (leftFlat.TopSteps != TopLeftLength) // левая торцевая не до лестницы
                {
                    foreach (var item in TopFlats)
                    {

                    }
                    var leftResults = FitFlats(TopFlats, TopLeftLength - leftFlat.TopSteps);
                    if (TopLeftFlats == null) return; // нельзя добавить левые верхние квартиры
                }
            }
            else //левая верхняя короткая
            {
                TopLeftFlats = FitFlats(TopFlats, TopLeftLength);
                if (TopLeftFlats == null) return; // нельзя добавить левые верхние квартиры
            }


            foreach (var flat in TopFlats)
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
                    TryAddTopFlat(stack, totalSteps);
                }
            }
            if (stack.Count > 0) stack.Pop();

        }

        private static List<FlatContainer> FitFlats(List<FlatContainer> topFlats, int size)
        {
            if (topFlats == null || size <= 0) return null;

        }
    }
}
