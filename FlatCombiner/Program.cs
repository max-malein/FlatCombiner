using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robot;
using Newtonsoft.Json;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel.Special;

namespace FlatCombiner
{
    class Program
    {
        //======================== задать входящие вручную!
        private static readonly bool lattitude = false;
        public static int StepLimit = 5; // количество шагов
        private static readonly string sourceFilePath = @"E:\Dropbox\WORK\154_ROBOT\04_Grasshopper\Source\flats-lon-01.json";
        private static readonly string outputFilePath = @"E:\Dropbox\WORK\154_ROBOT\07_Exchange\test_5.txt";
        private static readonly bool save = true;
        //====================================================================

        //коды:
        private static string inputCode = "MU_1_0;MU_2_0;MU_3_0;MD_0_1;MD_0_2;MD_0_3;CL_1_1;CL_1_2;CL_1_3;CL_2_1;CL_2_2;CL_2_3;CL_3_1;CL_3_2;CR_1_1;CR_1_2;CR_1_3;CR_2_1;CR_2_2;CR_2_3;CR_3_1;CR_3_2;CL_1_0;CL_2_0;CL_3_0;CL_0_1;CL_0_2;CL_0_3;CR_1_0;CR_2_0;CR_3_0;CR_0_1;CR_0_2;CR_0_3";

        private static int lluSteps = 2;

        private static List<FlatContainer> BottomFlats = new List<FlatContainer>();
        private static List<FlatContainer> TopFlats = new List<FlatContainer>();
        private static List<FlatContainer> LeftCornerFlats = new List<FlatContainer>();
        private static List<FlatContainer> RightCornerFlats = new List<FlatContainer>();
        private static List<FlatContainer> TopLeftCornerFlats = new List<FlatContainer>();
        private static List<FlatContainer> TopRightCornerFlats = new List<FlatContainer>();
        public static int ValidateCounter { get; private set; }
        public static List<List<string>> SuccessfulCombinations = new List<List<string>>();

        //длина слева и справа от ллу
        private static int TopLeftLength = (int) ((StepLimit - lluSteps) / 2);
        private static int TopRightLength = StepLimit - lluSteps - TopLeftLength;

        private static List<FlatContainer> currentBottomCorners;
        private static FlatContainer currentBottomRightCorner;
        private static Stack<FlatContainer> currentBottomStack;
        private static FlatContainer currentBottomLeftCorner;
        private static Stack<FlatContainer> tempStack;
        private static List<List<FlatContainer>> tempCombinations;
        private static Dictionary<int, List<List<FlatContainer>>> topCombinations = new Dictionary<int, List<List<FlatContainer>>>();
        private static Dictionary<int, List<List<FlatContainer>>> bottomCombinations = new Dictionary<int, List<List<FlatContainer>>>();

        //Создать пустой список для заполнения
        private static List<FlatContainer> nothingList = new List<FlatContainer>()
        {
            new FlatContainer()
            {
                TopSteps = 0,
                Id = "nothing",
                FType = FlatContainer.FlatLocattionType.MiddleUp
            }
        };


        static void Main(string[] args)
        {
           

            /*    
            string json = System.IO.File.ReadAllText(sourceFilePath);
            List<FlatContainer> AllFlats = JsonConvert.DeserializeObject<List<FlatContainer>>(json);
            AllFlats.RemoveAll(item => item == null);
            */


            //Создать FlatContainer
            var AllFlats = inputCode.Split(';').Select(c => new FlatContainer(c)).ToList();

            SplitFlats(AllFlats);

            int stepCount = 0;

            //Найти комбинации снизу
            for (int i = 0; i <= StepLimit; i++)
            {
                tempStack = new Stack<FlatContainer>();
                tempCombinations = new List<List<FlatContainer>>();
                GetAllCombinations(BottomFlats, i, true);

                bottomCombinations.Add(i, tempCombinations);
            }

            //Найти комбинации сверху
            for (int i = 0; i <= TopRightLength; i++)
            {
                tempStack = new Stack<FlatContainer>();
                tempCombinations = new List<List<FlatContainer>>();
                GetAllCombinations(TopFlats, i, false);

                topCombinations.Add(i, tempCombinations);
            }


            // запуск действа

            foreach (var leftCorner in LeftCornerFlats)
            {
                currentBottomLeftCorner = leftCorner;
                foreach (var rigthCorner in RightCornerFlats)
                {
                    int bottomSteps = StepLimit - leftCorner.BottomSteps - rigthCorner.BottomSteps;
                    if (bottomSteps < 0) continue;
                    foreach (var bottomMid in bottomCombinations[bottomSteps])
                    {                        
                        var realTopLeftCornerFlats = TopLeftCornerFlats;

                        //Если верхняя угловая не нужна, то ее нужно заменить на пустышку
                        if (leftCorner.FType == FlatContainer.FlatLocattionType.CornerLeft) 
                            realTopLeftCornerFlats = nothingList;

                        foreach (var leftTopCorner in realTopLeftCornerFlats)
                        {
                            var leftLluSize = TopLeftLength - leftCorner.TopSteps - leftTopCorner.TopSteps;

                            var leftTopFlats = new List<List<FlatContainer>>();
                            if (leftLluSize < 0)
                                continue;
                            else if (leftLluSize == 0)
                                leftTopFlats.Add(nothingList);
                            else
                                leftTopFlats = topCombinations[leftLluSize];

                            foreach (var leftTopFlat in leftTopFlats)
                            {
                                //Взять правый угол
                                var realTopRightCornerFlats = TopRightCornerFlats;

                                //Если верхняя угловая не нужна, то ее нужно заменить на пустышку
                                if (rigthCorner.FType == FlatContainer.FlatLocattionType.CornerRight)
                                    realTopRightCornerFlats = nothingList;

                                foreach (var rightTopCorner in realTopRightCornerFlats)
                                {
                                    var rightLluSize = TopRightLength - rightTopCorner.TopSteps - rigthCorner.TopSteps;

                                    var rightTopFlats = new List<List<FlatContainer>>();
                                    if (rightLluSize < 0)
                                        continue;
                                    else if (rightLluSize == 0)
                                        rightTopFlats.Add(nothingList);
                                    else
                                        rightTopFlats = topCombinations[rightLluSize];

                                    foreach (var rightTopFlat in rightTopFlats)
                                    {
                                        //Все готово, можно собирать комбинацию!
                                        var successfulCombination = new List<FlatContainer>();

                                        successfulCombination.Add(leftCorner);
                                        
                                        if (!bottomMid.Where(f => f.Id == "nothing").Any())
                                            successfulCombination.AddRange(bottomMid);

                                        successfulCombination.Add(rigthCorner);

                                        if (leftTopCorner.Id != "nothing")
                                            successfulCombination.Add(leftTopCorner);

                                        if (!leftTopFlat.Where(f => f.Id == "nothing").Any())
                                            successfulCombination.AddRange(leftTopFlat);

                                        var llu = new FlatContainer()
                                        {
                                            Id = "llu",
                                            TopSteps = lluSteps,
                                            BottomSteps = 0,
                                            FType = FlatContainer.FlatLocattionType.MiddleUp
                                        };
                                        successfulCombination.Add(llu);

                                        if (!rightTopFlat.Where(f => f.Id == "nothing").Any())
                                            successfulCombination.AddRange(rightTopFlat);

                                        if (rightTopCorner.Id != "nothing")
                                            successfulCombination.Add(rightTopCorner);

                                        var combIds = successfulCombination.Select(f => f.Id).ToList();
                                        Console.WriteLine(string.Join(" ", combIds));
                                        SuccessfulCombinations.Add(combIds);
                                    }
                                }
                            }
                            
                        }
                    }
                }
            }

            
            // сохранение файла            
            if(save) SaveFile(outputFilePath);

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
                switch (item.FType)
                {
                    case FlatContainer.FlatLocattionType.CornerLeft:
                    case FlatContainer.FlatLocattionType.CornerLeftDown:
                        LeftCornerFlats.Add(item);
                        break;

                    case FlatContainer.FlatLocattionType.CornerRight:
                    case FlatContainer.FlatLocattionType.CornerRightDown:
                        RightCornerFlats.Add(item);
                        break;

                    case FlatContainer.FlatLocattionType.CornerLeftUp:
                        TopLeftCornerFlats.Add(item);
                        break;

                    case FlatContainer.FlatLocattionType.CornerRightUp:
                        TopRightCornerFlats.Add(item);
                        break;            
                        
                    case FlatContainer.FlatLocattionType.MiddleUp:
                        TopFlats.Add(item);
                        break;

                    case FlatContainer.FlatLocattionType.MiddleDown:
                        BottomFlats.Add(item);
                        break;

                    default:
                        break;
                }
            }
            /*
            if (TopFlats !=null) // добавить ЛЛУ в верхние квартиры
            {
                var llu = new FlatContainer()
                {
                    Id = "llu",
                    TopSteps = lluSteps,
                    FType = FlatContainer.FlatLocattionType.MiddleUp
                };
                TopFlats.Add(llu); 
            }
                */
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
        /// <summary>
        /// Подбирает все возможные комбинации. Нужно перед вызовом очисить tempStack и tempCominations
        /// </summary>
        /// <param name="flats"></param>
        /// <param name="stepLimit"></param>
        /// <param name="bottom"></param>
        private static void GetAllCombinations(List<FlatContainer> flats, int stepLimit, bool bottom)
        {
            if (stepLimit ==  0)
            {
                tempCombinations.Add(nothingList);
                return;
            }

            foreach (var flat in flats)
            {
                int flatSteps = 0;
                if (bottom)
                    flatSteps = flat.BottomSteps;
                else
                    flatSteps = flat.TopSteps;

                if (flatSteps == stepLimit) //добавить в результаты
                {
                    tempStack.Push(flat);
                    tempCombinations.Add(tempStack.ToList()); 
                    tempStack.Pop();
                }
                else if (flatSteps > stepLimit) // квартира не подходит
                {
                    continue;
                }
                else //можно всунуть еще квартирку
                {
                    tempStack.Push(flat);
                    GetAllCombinations(flats, stepLimit - flatSteps, bottom);
                }
            }
            if (tempStack.Count > 0) tempStack.Pop();
        }


        private static void AddTopLong(FlatContainer[] arr)
        {


            var leftFlat = arr[arr.Length - 1];
            var rightFlat = arr[0];
            var topLength = StepLimit;
            var lluLength = TopLeftLength; // расстояние до ЛЛУ
            bool rightCorner = false; // наличие распашенки справа
            bool leftCorner = false; // наличие распашенки слева

            //если есть распашенки, то нужно сократить количество шагов сверху
            if (leftFlat.FType == FlatContainer.FlatLocattionType.CornerLeft)
            {
                leftCorner = true;

                topLength -= leftFlat.TopSteps;
                lluLength -= leftFlat.TopSteps;
            }
            if (rightFlat.FType == FlatContainer.FlatLocattionType.CornerRight)
            {
                rightCorner = true;
                topLength -= rightFlat.TopSteps;                
            }

            if (topLength < 2 || lluLength < 0) return;

            Stack<FlatContainer> topStack = new Stack<FlatContainer>();
            AddTopFuckers(arr, topStack, topLength, lluLength, leftCorner, rightCorner); //хуячить верхние квартиры у меридианалок
        }

        private static void AddTopFuckers(FlatContainer[] arr, Stack<FlatContainer> topStack, int topLength, int lluLength, bool leftCorner, bool rightCorner)
        {
            foreach (var flat in TopFlats)
            {
                if (flat.TopSteps < topLength)
                {
                    topStack.Push(flat);
                    AddTopFuckers(arr, topStack, topLength - flat.TopSteps, lluLength, leftCorner, rightCorner);
                    continue;
                }
                else if (flat.TopSteps > topLength) continue;

                //шаги совпадают с нужными
                topStack.Push(flat);
                ValidateTopSteps(arr, topStack, lluLength, leftCorner, rightCorner);
                topStack.Pop();
            }
            if (topStack != null && topStack.Count >0) topStack.Pop();
        }

        private static void ValidateTopSteps(FlatContainer[] arr, Stack<FlatContainer> topStack, int lluLength, bool leftCorner, bool rightCorner)
        {
            if (lluLength < 0) return;

            var topArray = topStack.ToList();
            topArray.Reverse();
            var leftFlat = topArray[0];
            var rightFlat = topArray[topArray.Count - 1];

            // проверить углы
            if (leftCorner && leftFlat.FType != FlatContainer.FlatLocattionType.MiddleUp) return;
            if (!leftCorner && leftFlat.FType != FlatContainer.FlatLocattionType.CornerLeftUp) return;
            if (rightCorner && rightFlat.FType != FlatContainer.FlatLocattionType.MiddleUp) return;
            if (!rightCorner && rightFlat.FType != FlatContainer.FlatLocattionType.CornerRightUp) return;
            
            int steps = 0;
            int lluCouner = 0;
            int cornerLeftUpCounter = 0;
            int cornerRightUpCounter = 0;
            foreach (var item in topArray)
            {
                if (item.FType == FlatContainer.FlatLocattionType.CornerLeftUp && leftCorner) return;
                if (item.FType == FlatContainer.FlatLocattionType.CornerRightUp && rightCorner) return;

                if (steps == lluLength && item.Id != "llu") return;
                if (item.Id == "llu" && steps != lluLength) return;
                steps += item.TopSteps;

                if (item.Id == "llu") lluCouner++;
                if (item.FType == FlatContainer.FlatLocattionType.CornerLeftUp) cornerLeftUpCounter++;
                if (item.FType == FlatContainer.FlatLocattionType.CornerRightUp) cornerRightUpCounter++;
            }
            if (lluCouner != 1) return;
            if (cornerLeftUpCounter > 1 || cornerRightUpCounter > 1) return;

            // все ок добавляй хуйню
            AddSuccessfullLongitudeCombination(arr, topArray);
           // var successCounter++;

        }

        private static void AddSuccessfullLongitudeCombination(FlatContainer[] arr, List<FlatContainer> topArray) // главная добавлялка!
        {
            var success = new List<FlatContainer>();
            topArray.Reverse();
            success.AddRange(topArray);
            success.AddRange(arr);
            SuccessfulCombinations.Add(success.Select(t=> t.Id).ToList());

            Console.Write(SuccessfulCombinations.Count + ": ");
            success.ForEach(s => Console.Write(s.Id + " "));
            Console.WriteLine();
        }

        private static void TryAddTopRight(FlatContainer[] arr, FlatContainer topLeftFlat) // убрать начиная отсюда!
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

        private static void AddSuccessfullLongitudeCombination(FlatContainer[] arr, List<FlatContainer> topLeftFlats, List<FlatContainer> topRightFlats) // может ее удалить?
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
        
        //private static void TryAddTopFlat(FlatContainer[] arr)
        //{

        //    var TopLeftFlats = new List<FlatContainer>();
        //    var TopRightFlats = new List<FlatContainer>();

        //    //левые хаты
        //    var leftFlat = arr[arr.Length - 1];
        //    if (leftFlat.FType == FlatContainer.FlatLocattionType.CornerLeft)
        //    {
        //        if (leftFlat.TopSteps != TopLeftLength) // левая торцевая не до лестницы
        //        {
        //            foreach (var item in TopFlats)
        //            {

        //            }
        //            var leftResults = FitFlats(TopFlats, TopLeftLength - leftFlat.TopSteps);
        //            if (TopLeftFlats == null) return; // нельзя добавить левые верхние квартиры
        //        }
        //    }
        //    else //левая верхняя короткая
        //    {
        //        TopLeftFlats = FitFlats(TopFlats, TopLeftLength);
        //        if (TopLeftFlats == null) return; // нельзя добавить левые верхние квартиры
        //    }


        //    foreach (var flat in TopFlats)
        //    {
        //        var totalSteps = stepCount + flat.BottomSteps;
        //        if (totalSteps > StepLimit) continue; //возврат если превышен лимит
        //        if (totalSteps == StepLimit)
        //        {
        //            stack.Push(flat);
        //            Validate(stack); // проверка на подходимость
        //        }
        //        else
        //        {
        //            stack.Push(flat);
        //            TryAddTopFlat(stack, totalSteps);
        //        }
        //    }
        //    if (stack.Count > 0) stack.Pop();

        //}

        //private static List<FlatContainer> FitFlats(List<FlatContainer> topFlats, int size)
        //{
        //    if (topFlats == null || size <= 0) return null;

        //}
    }
}
