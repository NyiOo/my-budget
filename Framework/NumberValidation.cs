using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyBudget.Framework
{
    public class NumberValidation 
    {
        public static Boolean Validate(String msg)
        {
            Decimal kyats;

            var flag = Decimal.TryParse(msg, out kyats);
           
            return !flag;
          

        }
    }

    public class ConvertBurmeseToEnglish
    {
        public static String ConvertFromMyanmarNumbers(String nums)
        {          

            Decimal result = 0;
            StringBuilder builder = new StringBuilder();

            Boolean flag = Decimal.TryParse(nums, out result);

            if (!flag)     // need to convert from myanmar to english
            {
                Char[] myanmar_chars = nums.ToCharArray();

                builder.Clear();

                for (int i = 0; i < myanmar_chars.Length; i++)
                {
                    switch (myanmar_chars[i])
                    {
                        case '0': builder.Append(0); break;
                        case '1': builder.Append(1); break;
                        case '2': builder.Append(2); break;
                        case '3': builder.Append(3); break;
                        case '4': builder.Append(4); break;
                        case '5': builder.Append(5); break;
                        case '6': builder.Append(6); break;
                        case '7': builder.Append(7); break;
                        case '8': builder.Append(8); break;
                        case '9': builder.Append(9); break;
                        case '၀': builder.Append(0); break;
                        case '၁': builder.Append(1); break;
                        case '၂': builder.Append(2); break;
                        case '၃': builder.Append(3); break;
                        case '၄': builder.Append(4); break;
                        case '၅': builder.Append(5); break;
                        case '၆': builder.Append(6); break;
                        case '၇': builder.Append(7); break;
                        case '၈': builder.Append(8); break;
                        case '၉': builder.Append(9); break;
                        case ',': builder.Append(',');break;
                        default: return "";                      // if there is another letter not numbers then  return "0"
                    }
                }
                if (builder.Length > 0)
                    return String.Format("{0:#,##}", Double.Parse(builder.ToString()));
                else
                    return null;
            }
            else
                return String.Format("{0:#,##}", Double.Parse(nums));
        }
    }



}
