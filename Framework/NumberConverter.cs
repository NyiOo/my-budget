using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace MyBudget.Framework
{
    [ValueConversion(typeof(string),typeof(string))]
    class EnglishNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
          
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            Decimal result = 0;

            StringBuilder builder = new StringBuilder();

            Boolean flag = Decimal.TryParse(value.ToString(), out result);

            if (!flag)     // need to convert from myanmar to english
            {
                Char[] myanmar_chars = value.ToString().ToCharArray();

                builder.Clear();

                for (int i = 0; i < myanmar_chars.Length; i++)
                {
                    switch (myanmar_chars[i])
                    {
                        
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
                        default: return "";                      // if there is another letter not numbers then  return ""
                    }
                }
                if (builder.Length > 0)
                    return builder.ToString();
                else
                    return null;
            }
            else
                return value.ToString();
            
        }
    }
}
