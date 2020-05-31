using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatCombiner
{
    public class FlatContainer
    {
        private string id="";
        private readonly bool createdByIdCode = false;

        public string Id { get { return id; } set
            {
                id = value;

                if (createdByIdCode) return; // id задается кодом, а не читается из квартиры

                //скрипт ниже просто чтобы сохранить возможность создавать FlatContainer из Flat
                //но там нужно shortSide добавлять и вообще вряд ли это еще пригодится.
                if (value == null) return;
                string pat = @".{3}_[UD]";
                string pat2 = @"shortSide";
                var code = System.Text.RegularExpressions.Regex.Match(id, pat).Value;
                bool shortSide = System.Text.RegularExpressions.Regex.Match(id, pat2).Success;

                if (shortSide) //короткие торцевые квартиры
                {
                    id = System.Text.RegularExpressions.Regex.Replace(value, pat2, ""); // убрать хрень из имени
                    switch (code)
                    {
                        case "b_R_U":
                            FType = FlatLocattionType.CornerRightUp;
                            break;
                        case "b_R_D":
                            FType = FlatLocattionType.CornerRightDown;
                            break;
                        case "b_L_U":
                            FType = FlatLocattionType.CornerLeftUp;
                            break;
                        case "b_L_D":
                            FType = FlatLocattionType.CornerLeftDown;
                            break;
                        default:
                            break;
                    }
                }
                else 
                {
                    switch (code)
                    {
                        case "b_R_U":
                        case "b_R_D":
                            FType = FlatLocattionType.CornerRight;
                            break;

                        case "b_L_U":
                        case "b_L_D":
                            FType = FlatLocattionType.CornerLeft;
                            break;

                        case "m_R_D":
                        case "m_L_D":
                            FType = FlatLocattionType.MiddleDown;
                            break;

                        default:
                            FType = FlatLocattionType.MiddleUp;
                            break;
                    }
                }
                
                return;
            }
        }
        public int BottomSteps { get; set; }
        public int TopSteps { get; set; }
        public FlatLocattionType FType {get; set;}

        public FlatContainer() { }

        public FlatContainer(string code)
        {
            if (string.IsNullOrEmpty(code)) return;

            var parts = code.Split('_');
            if (parts.Length < 3) return;

            createdByIdCode = true;
            Id = code;

            var location = parts[0];
            TopSteps = int.Parse(parts[1]);
            BottomSteps = int.Parse(parts[2]);

            switch (location)
            {
                case "MU":
                    FType = FlatLocattionType.MiddleUp;
                    break;

                case "MD":
                    FType = FlatLocattionType.MiddleDown;
                    break;

                case "CL":
                    if (BottomSteps == 0)
                        FType = FlatLocattionType.CornerLeftUp;
                    else if (TopSteps == 0)
                        FType = FlatLocattionType.CornerLeftDown;
                    else
                        FType = FlatLocattionType.CornerLeft;
                    break;

                case "CR":
                    if (BottomSteps == 0)
                        FType = FlatLocattionType.CornerRightUp;
                    else if (TopSteps == 0)
                        FType = FlatLocattionType.CornerRightDown;
                    else
                        FType = FlatLocattionType.CornerRight;
                    break;

                default:
                    break;
            }


        }

        public override string ToString()
        {
            return string.Format("{0}, bSteps = {1}, tSteps = {2}, {3}", Id, BottomSteps, TopSteps, FType);
        }

        public enum FlatLocattionType
        {
            CornerLeft, //распашонка
            CornerLeftDown,
            CornerLeftUp,
            CornerRight, //распашонка
            CornerRightUp,
            CornerRightDown,
            MiddleDown,
            MiddleUp,

            }
    }

    
}

