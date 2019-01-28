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

        public string Id { get { return id; } set
            {
                id = value;
                if (value == null) return;
                string pat = @".{3}_[UD]";
                var code = System.Text.RegularExpressions.Regex.Match(id, pat).Value;
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
                return;
            }
        }
        public int BottomSteps { get; set; }
        public int TopSteps { get; set; }
        public FlatLocattionType FType {get; set;}

        public FlatContainer() { }

        public override string ToString()
        {
            return string.Format("{0}, bSteps = {1}, tSteps = {2}, {3}", Id, BottomSteps, TopSteps, FType);
        }

        public enum FlatLocattionType
        {
            CornerLeft,
            CornerRight,
            MiddleDown,
            MiddleUp
        }
    }

    
}

