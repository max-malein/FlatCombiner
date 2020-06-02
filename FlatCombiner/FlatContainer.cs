using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatCombiner
{
    public class FlatContainer
    {
        public string Id { get; set; }
        public int BottomSteps { get; set; }
        public int TopSteps { get; set; }
        public FlatLocattionType FType {get; set;}

        public FlatContainer() { }

        public FlatContainer(string code)
        {
            if (string.IsNullOrEmpty(code)) return;

            var parts = code.Split('_');
            if (parts.Length < 3) return;

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
            MiddleUp
        }
    }

    
}

