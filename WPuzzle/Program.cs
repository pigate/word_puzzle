using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace WordP
{
    class Program
    {
        public static class PuzzleSolver
        {
            public static string[] DICTIONARY = { "OX", "CAT", "TOY", "AT", "DOG", "CATAPULT", "T" };

            //word length maps to set of words
            public static Dictionary<int, HashSet<string>> appDict;

            /// <summary>
            /// returns the input string in reverse form
            /// </summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public static string ReverseString(string s)
            {
                if (string.IsNullOrEmpty(s)) return s;
                char[] charArray = s.ToCharArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }

            /// <summary>
            /// updates appDict with words from DICTIONARY
            /// if option_rev == true, includes reversed versions of the word
            /// </summary>
            static void PrepDict()
            {
                appDict = new Dictionary<int, HashSet<string>>();
                int ct = DICTIONARY.Count();
                for (int i = 0; i < ct; i++)
                {
                    int len = DICTIONARY[i].Length;
                    if (!appDict.ContainsKey(len))
                    {
                        appDict[len] = new HashSet<string>();
                    }

                    appDict[len].Add(DICTIONARY[i]);
                    //if needed, add reverse of word
                    if (option_rev)
                    {
                        appDict[len].Add(ReverseString(DICTIONARY[i]));
                    }
                }

                if (debug_print)
                {
                    Console.WriteLine("appDict is prepped with: ");
                    foreach (KeyValuePair<int, HashSet<string>> kvp in appDict)
                    {
                        Console.WriteLine("{0}: ", kvp.Key);
                        foreach (string s in kvp.Value)
                        {
                            Console.Write("{0}, ", s);
                        }
                        Console.WriteLine();
                    }
                }
            }

            //custom options for which directions should be checked by puzzle
            public static bool option_horiz = true;
            public static bool option_vert = true;
            public static bool option_diag_for = true;
            public static bool option_diag_back = true;

            //custom option for whether to include check for reverse words
            public static bool option_rev = true;
            public static bool debug_print = false;

            //stats
            static int ct_rows = 0;
            static int ct_cols = 0;
            static int ct_diags = 0;

            /// <summary>
            /// returns the ith horizontal segment in puzzle
            /// i = 0 corresponds to row[0]
            /// </summary>
            /// <param name="puzzle"></param>
            /// <param name="i"></param>
            /// <returns></returns>
            static string GetHorizRow(char[][] puzzle, int i)
            {
                if (ct_rows == 0 || ct_cols == 0 || i < 0 || i > ct_rows) return null;
                return new string(puzzle[i]);
            }

            /// <summary>
            /// returns the ith vertical segment in puzzle
            /// i = 0 corresponds to col[0]
            /// </summary>
            /// <param name="puzzle"></param>
            /// <param name="i"></param>
            /// <returns></returns>
            static string GetVert(char[][] puzzle, int i)
            {
                if (ct_rows == 0 || ct_cols == 0 || i < 0 || i > ct_cols) return null;

                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < ct_rows; j++)
                {
                    sb.Append(puzzle[j][i]);
                }
                return sb.ToString();
            }

            /// <summary>
            /// returns the ith forward diagonal segment in puzzle
            /// i = 0 corresponds to top right corner
            /// </summary>
            /// <param name="puzzle"></param>
            /// <param name="i"></param>
            /// <returns></returns>
            static string GetDiagFor(char[][] puzzle, int i)
            {
                if (ct_rows == 0 || ct_cols == 0 || i < 0 || i >= ct_diags) return null;

                StringBuilder sb = new StringBuilder();
                int col_offset = GetColOffset_DiagFor(i);
                int row_offset = GetRowOffset(i);
                int len = GetLengthDiag(i);
                int iter = 0;
                while (iter < len)
                {
                    sb.Append(puzzle[row_offset][col_offset]);
                    col_offset++;
                    row_offset++;
                    iter++;
                }
                return sb.ToString();
            }

            /// <summary>
            /// returns the ith backward diagonal segment in puzzle
            /// i = 0 corresponds to bottom right corner
            /// </summary>
            /// <param name="puzzle"></param>
            /// <param name="i"></param>
            /// <returns></returns>
            static string GetDiagRev(char[][] puzzle, int i)
            {
                if (ct_rows == 0 || ct_cols == 0 || i < 0 || i >= ct_diags) return null;

                StringBuilder sb = new StringBuilder();
                int len = GetLengthDiag(i);
                int col_offset = GetColOffset_DiagRev(i);
                int row_offset = GetRowOffset(i);

                int iter = 0;
                while (iter < len)
                {
                    sb.Append(puzzle[row_offset][col_offset]);
                    col_offset--;
                    row_offset++;
                    iter++;
                }
                return sb.ToString();
            }

            /// <summary>
            /// returns the column offset for ith forward diagonal segment in puzzle
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            static int GetColOffset_DiagFor(int i)
            {
                int col_offset = 0;
                if (i < ct_rows)
                {
                    col_offset = 0;
                }
                else
                {
                    col_offset = i - (ct_rows - 1);
                }
                if (debug_print)
                {
                    Console.WriteLine("ColOffset for '{0}':'{1}'", i, col_offset);
                }
                return col_offset;
            }

            /// <summary>
            /// returns the column offset for ith backward diagonal segment in puzzle
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            static int GetColOffset_DiagRev(int i)
            {
                int min = ct_cols > ct_rows ? ct_rows : ct_cols;
                int abs_diff = ct_cols > ct_rows ? ct_cols - ct_rows : ct_rows - ct_cols;
                int col_offset = ct_cols - 1;

                //case n > m
                if (ct_rows > ct_cols)
                {
                    if (i >= ct_rows)
                        col_offset = ct_diags - i;
                }
                //case n < m
                else if (ct_rows < ct_cols)
                {
                    if (i >= ct_rows)
                        col_offset = ct_diags - i - 1;
                }
                //case n == m
                else
                {
                    if (i >= ct_rows)
                        col_offset = ct_diags - i - 1;
                }
                return col_offset;
            }

            /// <summary>
            /// returns row offset for ith diagonal segment in puzzle (both forward and backward)
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            static int GetRowOffset(int i)
            {
                if (i < ct_rows - 1)
                {
                    return ct_rows - i - 1;
                }
                return 0;
            }

            /// <summary>
            /// returns number of diagonals in puzzle
            /// </summary>
            /// <returns></returns>
            static int GetNumDiags()
            {
                if (ct_cols == ct_rows)
                {
                    if (ct_cols == 0) return 0;
                    if (ct_cols == 1) return 1;
                }

                int min = ct_cols > ct_rows ? ct_rows : ct_cols;
                int len = 2 * min - 1;
                if (ct_cols == ct_rows)
                    return len;
                int abs_difference = ct_cols > ct_rows ? ct_cols - ct_rows : ct_rows - ct_cols;
                return len + abs_difference;
            }

            /// <summary>
            /// returns length of ith diagonal (both forward and backward) in puzzle
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            static int GetLengthDiag(int i)
            {
                int max_length = ct_cols > ct_rows ? ct_rows : ct_cols;

                if (i < 0 || i > ct_diags) return 0;

                int lower_tri_boundary = -1; 
                int upper_tri_boundary = ct_diags;

                if (ct_cols == ct_rows) //case nxn
                 {
                    lower_tri_boundary = ct_rows;
                    upper_tri_boundary = ct_diags / 2;

                    if (i < lower_tri_boundary) //lower triangle (non inclusive)
                    {
                        return i + 1;
                    }
                    else if (i > upper_tri_boundary) //upper triangle (non inclusive)
                    {
                        return ct_diags - i;
                    }
                }
                else if (ct_rows < ct_cols) //case n < m
                {
                    lower_tri_boundary = -1;
                    upper_tri_boundary = ct_diags;

                    if (ct_rows != 1)
                    {
                        lower_tri_boundary = ct_rows - 2;
                        upper_tri_boundary = ct_diags - (1 + lower_tri_boundary);
                    }

                    if (i <= lower_tri_boundary)
                    {
                        return i + 1;
                    }
                    else if (i >= upper_tri_boundary)
                    {
                        return ct_diags - i;
                    }
                }
                else //case n > m
                {
                    lower_tri_boundary = max_length - 1;
                    upper_tri_boundary = ct_diags - 1 - lower_tri_boundary;

                    if (i <= lower_tri_boundary)
                    {
                        return i + 1;
                    }
                    else if (i > upper_tri_boundary)
                    {
                        return ct_diags - i;
                    }
                }
                return max_length;
            }

            /// <summary>
            /// returns count of nondistinct words from appDict found in puzzle
            /// check options for which directions to check
            /// </summary>
            /// <param name="puzzle"></param>
            /// <returns></returns>
            public static int FindWords(char[][] puzzle)
            {
                int count = 0;
                PrepDict();
                ct_rows = puzzle.Count();
                if (ct_rows == 0) return count;
                ct_cols = puzzle[0].Count();
                if (ct_cols == 0) return count;
                ct_diags = GetNumDiags();
                //if set to true, ignore 1 letter matches if has already been counted
                bool hasCountedSingles = false;
                
                //mode reflects direction to check puzzle with
                //0 = horiz
                //1 = vert
                //2 = diag_forw
                //3 = diag_rev
                //each direction has a similar procedure.
                //the only difference between the modes is how you set up the words and iter counts
                int mode = 0;
                int iter = ct_rows;
                int str_length = ct_rows;

                while (mode < 4)
                {
                    //check contig combinations of puzzle in specified direction for match
                    int start = 0;

                    if (mode == 0)
                    {
                        if (option_horiz)
                        {
                            if (debug_print) Console.WriteLine("HORIZ: ");
                            iter = ct_rows;
                        }
                        else
                        {
                            mode++;
                            continue;
                        }
                    }
                    else if (mode == 1)
                    {
                        if (option_vert)
                        {
                            if (debug_print) Console.WriteLine("VERT: ");
                            iter = ct_cols;
                        }
                        else
                        {
                            mode++;
                            continue;
                        }
                    }
                    else //both mode 2/3
                    {
                        if (mode == 2 && option_diag_for || mode == 3 && option_diag_back)
                        {
                            if (debug_print) Console.Write("DIAG_");
                            iter = ct_diags;
                        }
                        else
                        {
                            mode++;
                            continue;
                        }
                        //set iter2 = GetLengthDiag(i) when know value of iter
                    }

                    for (int i = 0; i < iter; i++)
                    {
                        List<string> foundWords = new List<string>();
                        string s = "";

                        //some overrides for iter2/iter3 in case of diagonal mode
                        if (mode == 0)
                        {
                            s = GetHorizRow(puzzle, i);
                        }
                        else if (mode == 1)
                        {
                            s = GetVert(puzzle, i);
                        }
                        else if (mode == 2)
                        {
                            if (debug_print) Console.WriteLine("FOR: ");
                            s = GetDiagFor(puzzle, i);
                        }
                        else //mode = 3
                        {
                            if (debug_print) Console.WriteLine("Rev: ");
                            s = GetDiagRev(puzzle, i);
                        }
                        str_length = s.Length;

                        while (start < str_length)
                        {
                            int howMany = 1;
                            if (debug_print)
                            {
                                Console.Write("row {0}: ", i);
                            }
                            while (start + howMany <= str_length)
                            {
                                string substring = s.Substring(start, howMany);
                                if (debug_print)
                                {
                                    Console.Write("{0}, ", substring);
                                }

                                if (howMany > 1 || howMany == 1 && !hasCountedSingles)
                                {
                                    if (appDict.ContainsKey(howMany))
                                    {
                                        if (appDict[howMany].Contains(substring))
                                        {
                                            count++;
                                            foundWords.Add(substring);
                                        }
                                    }
                                }
                                howMany++;
                            }
                            if (debug_print)
                            {
                                Console.WriteLine();
                            }
                            start++;
                        }
                        if (debug_print)
                        {
                            Console.WriteLine();
                            Console.Write("Found: {");
                            foreach (string match in foundWords)
                            {
                                Console.Write("{0}, ", match);
                            }
                            Console.WriteLine("}");
                        }

                        start = 0;
                    }
                    if (!hasCountedSingles) hasCountedSingles = true;
                    mode++;
                }

                //reset for next count
                mode = 0;
                hasCountedSingles = false;

                if (debug_print)
                {
                    Console.WriteLine("Count: {0}", count);
                }
                return count;
            }

            public static void Test()
            {
                //n = m
                ct_cols = ct_rows = 0;
                Debug.Assert(GetNumDiags() == 0);
                ct_cols = ct_rows = 1;
                Debug.Assert(GetNumDiags() == 1);
                ct_cols = ct_rows = 2;
                Debug.Assert(GetNumDiags() == 3);
                ct_cols = ct_rows = 3;
                Debug.Assert(GetNumDiags() == 5);

                //n > m
                //special case: m = 1
                ct_cols = 1; ct_rows = 3;
                Debug.Assert(GetNumDiags() == 3);
                ct_cols = 1; ct_rows = 4;
                Debug.Assert(GetNumDiags() == 4);
                ct_cols = 1; ct_rows = 5;
                Debug.Assert(GetNumDiags() == 5);

                ct_cols = 2; ct_rows = 3;
                Debug.Assert(GetNumDiags() == 4);
                ct_cols = 3; ct_rows = 4;
                Debug.Assert(GetNumDiags() == 6);
                ct_cols = 3; ct_rows = 5;
                Debug.Assert(GetNumDiags() == 7);

                //n < m
                ct_cols = 3; ct_rows = 1;
                Debug.Assert(GetNumDiags() == 3);
                ct_cols = 4; ct_rows = 1;
                Debug.Assert(GetNumDiags() == 4);
                ct_cols = 5; ct_rows = 1;
                Debug.Assert(GetNumDiags() == 5);

                ct_cols = ct_rows = 2;
                ct_diags = GetNumDiags();
                Debug.Assert(GetLengthDiag(0) == 1);
                Debug.Assert(GetLengthDiag(1) == 2);
                Debug.Assert(GetLengthDiag(2) == 1);

                ct_cols = ct_rows = 2;
                ct_diags = GetNumDiags();
                Debug.Assert(GetRowOffset(0) == 1);
                Debug.Assert(GetRowOffset(1) == 0);
                Debug.Assert(GetRowOffset(2) == 0);

                ct_cols = ct_rows = 2;
                ct_diags = GetNumDiags();
                Debug.Assert(GetColOffset_DiagFor(0) == 0);
                Debug.Assert(GetColOffset_DiagFor(1) == 0);
                Debug.Assert(GetColOffset_DiagFor(2) == 1);

                ct_cols = ct_rows = 3;
                ct_diags = GetNumDiags();
                Debug.Assert(GetLengthDiag(0) == 1);
                Debug.Assert(GetLengthDiag(1) == 2);
                Debug.Assert(GetLengthDiag(2) == 3);
                Debug.Assert(GetLengthDiag(3) == 2);
                Debug.Assert(GetLengthDiag(4) == 1);

                ct_cols = ct_rows = 3;
                ct_diags = GetNumDiags();
                Debug.Assert(GetRowOffset(0) == 2);
                Debug.Assert(GetRowOffset(1) == 1);
                Debug.Assert(GetRowOffset(2) == 0);
                Debug.Assert(GetRowOffset(3) == 0);
                Debug.Assert(GetRowOffset(4) == 0);

                ct_cols = ct_rows = 3;
                ct_diags = GetNumDiags();
                Debug.Assert(GetColOffset_DiagFor(0) == 0);
                Debug.Assert(GetColOffset_DiagFor(1) == 0);
                Debug.Assert(GetColOffset_DiagFor(2) == 0);
                Debug.Assert(GetColOffset_DiagFor(3) == 1);
                Debug.Assert(GetColOffset_DiagFor(4) == 2);

                ct_cols = 8;
                ct_rows = 3;
                ct_diags = GetNumDiags();
                Debug.Assert(GetLengthDiag(0) == 1);
                Debug.Assert(GetLengthDiag(1) == 2);
                Debug.Assert(GetLengthDiag(2) == 3);
                Debug.Assert(GetLengthDiag(3) == 3);
                Debug.Assert(GetLengthDiag(4) == 3);
                Debug.Assert(GetLengthDiag(5) == 3);
                Debug.Assert(GetLengthDiag(6) == 3);
                Debug.Assert(GetLengthDiag(7) == 3);
                Debug.Assert(GetLengthDiag(8) == 2);
                Debug.Assert(GetLengthDiag(9) == 1);
                Debug.Assert(GetLengthDiag(10) == 0);
            }
        }


        static void Main(string[] args)
        {
            //read puzzle from file. construct char[,]
            StreamReader objReader = new StreamReader("c:\\Users\\User\\Documents\\test.txt");
            string line = "";

            List<char[]> puzzle = new List<char[]>();
            line = objReader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                //convert string to char array
                char[] charArray = line.ToCharArray();
                puzzle.Add(charArray);
                line = objReader.ReadLine();
            }

            char[][] charPuzzle = puzzle.ToArray();
            Console.WriteLine("Count: " + PuzzleSolver.FindWords(charPuzzle));
            Console.Read();
        }
    }
}
