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
using System.Diagnostics;

namespace FlatCombiner
{
    class Program
    {
        //======================== задать входящие вручную!
        public static int StepLimit = 5; // количество шагов
        private static readonly string outputFilePath = @"E:\Dropbox\WORK\154_ROBOT\07_Exchange\combinations_5.txt";
        private static readonly bool save = true;
        private static int lluSteps = 2;

        //коды:
        private static string inputCode = "MU_1_0;MU_2_0;MU_3_0;MD_0_1;MD_0_2;MD_0_3;CL_1_1;CL_1_2;CL_1_3;CL_2_1;CL_2_2;CL_2_3;CL_3_1;CL_3_2;CR_1_1;CR_1_2;CR_1_3;CR_2_1;CR_2_2;CR_2_3;CR_3_1;CR_3_2;CL_1_0;CL_2_0;CL_3_0;CL_0_1;CL_0_2;CL_0_3;CR_1_0;CR_2_0;CR_3_0;CR_0_1;CR_0_2;CR_0_3";
        //====================================================================


        private static List<FlatContainer> BottomFlats = new List<FlatContainer>();
        private static List<FlatContainer> TopFlats = new List<FlatContainer>();
        private static List<FlatContainer> LeftCornerFlats = new List<FlatContainer>();
        private static List<FlatContainer> RightCornerFlats = new List<FlatContainer>();
        private static List<FlatContainer> TopLeftCornerFlats = new List<FlatContainer>();
        private static List<FlatContainer> TopRightCornerFlats = new List<FlatContainer>();

        public static List<List<string>> SuccessfulCombinations = new List<List<string>>();

        //длина слева и справа от ллу
        private static int TopLeftLength = (int) ((StepLimit - lluSteps) / 2);
        private static int TopRightLength = StepLimit - lluSteps - TopLeftLength;

        private static Stack<FlatContainer> tempStack;
        private static List<List<FlatContainer>> tempCombinations;
        private static Dictionary<int, List<List<FlatContainer>>> topCombinations = new Dictionary<int, List<List<FlatContainer>>>();
        private static Dictionary<int, List<List<FlatContainer>>> bottomCombinations = new Dictionary<int, List<List<FlatContainer>>>();

        //Создать пустой список для заполнения
        private static readonly List<FlatContainer> nothingList = new List<FlatContainer>()
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
            //Создать FlatContainer
            var AllFlats = inputCode.Split(';').Select(c => new FlatContainer(c)).ToList();

            //разбить на списки по расположению
            SplitFlats(AllFlats);

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
            int counter = 0;
            int num = 1;


            foreach (var leftCorner in LeftCornerFlats)
            {
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
                                        //Console.WriteLine(string.Join(" ", combIds));
                                        SuccessfulCombinations.Add(combIds);
                                        
                                        if (++counter >= 15000000)
                                        {
                                            SaveFile(outputFilePath + num++);
                                            counter = 0;
                                            SuccessfulCombinations.Clear();
                                        }
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
            Console.WriteLine(SuccessfulCombinations.Count);
            Console.ReadKey();
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
    }    
}
