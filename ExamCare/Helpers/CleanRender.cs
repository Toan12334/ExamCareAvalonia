using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace ExamCare.Helpers
{
    public class CleanRender
    {


        public static string CleanMathSpaces(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var result = new System.Text.StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '$')
                {
                    // kiểm tra $$ hay $
                    string delimiter = "$";
                    if (i + 1 < input.Length && input[i + 1] == '$')
                    {
                        delimiter = "$$";
                        i++;
                    }

                    int start = i + 1;
                    int end = input.IndexOf(delimiter, start);

                    if (end != -1)
                    {
                        string inner = input.Substring(start, end - start).Trim();

                        result.Append(delimiter + inner + delimiter);

                        i = end + delimiter.Length - 1;
                    }
                    else
                    {
                        // không có đóng → giữ nguyên
                        result.Append(delimiter);
                    }
                }
                else
                {
                    result.Append(input[i]);
                }
            }

            return result.ToString();
        }


    }

}



