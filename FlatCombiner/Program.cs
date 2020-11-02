using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace FlatCombiner
{
    class Program
    {
        //======================== задать входящие вручную!
        public static int StepLimit = 13; // количество шагов
        private static readonly string outputFolderPath = @"E:\Dropbox\WORK\154_ROBOT\07_Exchange";
        private static readonly bool save = true;
        private static int lluSteps = 2;
        private static bool cornerHblock = true;

        //коды:
        private static string inputCode = "MU_1_0;MU_2_0;MU_3_0;MD_0_1;MD_0_2;MD_0_3;CL_1_1;CL_1_2;CL_1_3;CL_2_1;CL_2_2;CL_2_3;CL_3_1;CL_3_2;CR_1_1;CR_1_2;CR_1_3;CR_2_1;CR_2_2;CR_2_3;CR_3_1;CR_3_2;CL_1_0;CL_2_0;CL_3_0;CL_0_1;CL_0_2;CL_0_3;CR_1_0;CR_2_0;CR_3_0;CR_0_1;CR_0_2;CR_0_3";
        private static string inputCodeCorner = "MU_1_0;MU_2_0;MU_3_0;MD_0_1;MD_0_2;MD_0_3;CR_1_1;CR_1_2;CR_1_3;CR_2_1;CR_2_2;CR_2_3;CR_3_1;CR_3_2;CR_1_0;CR_2_0;CR_3_0;CR_0_1;CR_0_2;CR_0_3";
        //====================================================================


        private static List<FlatContainer> BottomFlats = new List<FlatContainer>();
        private static List<FlatContainer> TopFlats = new List<FlatContainer>();
        private static List<FlatContainer> LeftCornerFlats = new List<FlatContainer>();
        private static List<FlatContainer> RightCornerFlats = new List<FlatContainer>();
        private static List<FlatContainer> TopLeftCornerFlats = new List<FlatContainer>();
        private static List<FlatContainer> TopRightCornerFlats = new List<FlatContainer>();

        public static List<List<string>> SuccessfulCombinations = new List<List<string>>();

        private static string FilePath = string.Empty;
        private static string FilePathRightCorner = string.Empty;

        //длина слева и справа от ллу
        private static int TopLeftLength = (int)((StepLimit - lluSteps) / 2);
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //создать путь для сохранения
            if (save)
            {
                string corner = cornerHblock ? "_cornerLeft" : "";
                var fileName = $"combinations{corner}_llu{lluSteps}_{StepLimit}.txt";
                FilePath = Path.Combine(outputFolderPath, fileName);
                File.Delete(FilePath);
            }

            //поправить размеры верхнего ряда, если угловая секция
            if (cornerHblock)
            {
                //AL_3_2 - угловая
                //BL_2_1 - стыковка с широткой
                var al = new FlatContainer
                {
                    Id = "AL_3_2",
                    BottomSteps = 2,
                    FType = FlatContainer.FlatLocattionType.CornerLeftDown
                };
                var bl = new FlatContainer
                {
                    Id = "BL_2_1",
                    TopSteps = 3,
                    FType = FlatContainer.FlatLocattionType.CornerLeftUp
                };
                LeftCornerFlats.Add(al);
                TopLeftCornerFlats.Add(bl);
                TopLeftLength = 3;
                TopRightLength = StepLimit - 3 - lluSteps;

                //Добавить путь для правого угла
                FilePathRightCorner = FilePath.Replace("_cornerLeft", "_cornerRight");
                File.Delete(FilePathRightCorner);
            }
                

            //Создать FlatContainer
            List<FlatContainer> AllFlats = null;
            if (cornerHblock)
                AllFlats = inputCodeCorner.Split(';').Select(c => new FlatContainer(c)).ToList();
            else
                AllFlats = inputCode.Split(';').Select(c => new FlatContainer(c)).ToList();

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
            int totalCounter = 0;
            int num = 1;
            int progressCounter = 0;
            int totalCombinations = LeftCornerFlats.Count * RightCornerFlats.Count * TopRightCornerFlats.Count * TopLeftCornerFlats.Count;


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
                                    progressCounter++;

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

                                        successfulCombination.Add(rigthCorner);

                                        if (!bottomMid.Where(f => f.Id == "nothing").Any())
                                            successfulCombination.AddRange(bottomMid);

                                        var combIds = successfulCombination.Select(f => f.Id).ToList();                                                                                
                                        SuccessfulCombinations.Add(combIds);

                                        //Сохранять каждые 10000 строк
                                        if (SuccessfulCombinations.Count >= 10000)
                                        {
                                            SaveTextLines(SuccessfulCombinations);
                                            //Console.Write("\r");
                                            //Console.Write($"progress: {(int)((progressCounter/totalCombinations) * 100)}%");
                                        }
                                        
                                        
                                        /*if (++totalCounter >= 15000000)
                                        {
                                            SaveFile(outputFolderPath + num++);
                                            totalCounter = 0;
                                            SuccessfulCombinations.Clear();
                                        }*/
                                    }
                                }
                            }
                            
                        }
                    }
                }
            }


            // сохранение файла            
            //if(save) SaveFile(outputFolderPath);

            //сохранить остатки
            SaveTextLines(SuccessfulCombinations);


            stopwatch.Stop();
            Console.WriteLine($"выполнено за {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds}:{stopwatch.Elapsed.Milliseconds}");
            Console.ReadKey();
        }

        private static void SaveTextLines(List<List<string>> successfulCombinations)
        {
            if (!successfulCombinations.Any()) return;

            var lines = successfulCombinations.Select(comb => string.Join(",", comb));
            File.AppendAllLines(FilePath, lines);

            //Добавить правый угол
            if (cornerHblock)
            {
                var rightCornerLines = new List<string>();
                foreach (var line in lines)
                {
                    var rc = line.Replace("AL", "AR");
                    rc = rc.Replace("BL", "BR");
                    rightCornerLines.Add(rc);
                }

                File.AppendAllLines(FilePathRightCorner, rightCornerLines);
            }

            successfulCombinations.Clear();
        }

        private static void SaveTextLine(List<string> combIds)
        {           
            File.AppendAllText(FilePath, string.Join(",", combIds));
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

        private static void SaveFile(string saveFolder)
        {
            List<string> combinations = new List<string>();
            List<string> combinationsCornerRight = new List<string>(); //только для угловых

            string corner = cornerHblock ? "_cornerLeft" : "";
            var fileName = $"combinations{corner}_llu{lluSteps}_{StepLimit}.txt";
            var filePath = Path.Combine(saveFolder, fileName);

            foreach (var success in SuccessfulCombinations)
            {                               
                var line = string.Join(",", success);
                line = SortCodeClockwise(line);
                combinations.Add(line);
                
                //для угловых нужно создать еще правую секцию
                //правая секция записана в обратном направлении
                if (cornerHblock)
                {
                    line = line.Replace("AL", "AR");
                    line = line.Replace("BL", "BR");
                    combinationsCornerRight.Add(line);
                }
            }
            //List<string> unique = combinations.Distinct().ToList();
            
            if (combinations.Any())
                System.IO.File.WriteAllLines(filePath, combinations);
            
            //Сохранить углы, если есть
            if (combinationsCornerRight.Any())
            {
                filePath = filePath.Replace("_cornerLeft_", "_cornerRight_");
                File.WriteAllLines(filePath, combinationsCornerRight);
            }    
                
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

        public static string SortCodeClockwise(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;

            var codes = code.Trim().Split(',').ToList();
            var sorted = new List<string>();

            //add CL or AL
            sorted.Add(codes[0]);

            var bottomRow = new List<string>();
            var topRow = new List<string>();
            for (int i = 1; i < codes.Count; i++)
            {
                bottomRow.Add(codes[i]);
                if (codes[i].Contains("CR"))
                {
                    bottomRow.Reverse();
                    topRow = codes.Skip(i + 1).ToList();
                    break;
                }
            }

            sorted.AddRange(topRow);
            sorted.AddRange(bottomRow);

            return string.Join(",", sorted);
        }
    }    
}
