using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using Gurobi;

namespace Popular_Matchings
{
    /// <summary>
    /// Form for user input and displaying results.
    /// </summary>
    public partial class Form1 : Form
    {
        string Path = Application.StartupPath + "\\MatchingResult";
        string PathLP = Application.StartupPath + "\\MatchingResult";
        Instance GlobalResult;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Parse the given string into a matching instance.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        private Instance Parse(string Input)
        {
            string[] Prios = Input.Split('-');
            List<Tuple<int, int>>[] PrioList = new List<Tuple<int, int>>[Prios.Length - 1];
            List<int> Posts = new List<int>();
            for (int i = 0; i < Prios.Length - 1; i++)
            {
                PrioList[i] = new List<Tuple<int, int>>();
                string[] Targets = Prios[i].Split(',');
                int RankCounter = 0;
                foreach (string Target in Targets)
                {
                    string[] Parts = Target.Split('.');
                    int ID = int.Parse(Parts[0]);
                    int Rank;
                    if (Parts.Length == 1)
                        Rank = RankCounter++;
                    else
                        Rank = int.Parse(Parts[1]);
                    PrioList[i].Add(new Tuple<int, int>(ID, Rank));
                    if (!Posts.Contains(ID))
                        Posts.Add(ID);
                }
            }
            int Capacity = int.Parse(Prios[Prios.Length - 1]);
            Instance Result = new Instance(Prios.Length - 1, Posts.Count, Capacity, PrioList);

            return Result;
        }

        /// <summary>
        /// Prints the given instance into a string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string Output(Instance input)
        {
            string Result = "";
            if (input == null)
                Result = "No popular matching.";
            else
            {
                int[] Matching = new int[input.Applicants.Length];
                for (int i = 0; i < input.Posts.Length; i++)
                {
                    if (input.Posts[i].NrMatchings > 0)
                        Matching[input.Posts[i].Matchings[0]] = i;
                }

                for (int j = 0; j < Matching.Length; j++)
                {
                    Result += j.ToString() + " -> " + Matching[j].ToString() + ";  ";
                }
            }

            return Result;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            int Solver = 0;
            if (radioButton3.Checked)
                Solver = 0;
            if (radioButton4.Checked)
                Solver = 1;
            if (radioButton5.Checked)
                Solver = 2;
            Simulator Sim = new Simulator(Solver);
            Tuple<long, long> Result = Sim.FindAllMatchings(int.Parse(textBox3.Text), false, "");
            textBox4.Text = "#Not Popular: " + Result.Item1.ToString() + "  #Total: " + Result.Item2.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int Solver = 0;
            if (radioButton3.Checked)
                Solver = 0;
            if (radioButton4.Checked)
                Solver = 1;
            if (radioButton5.Checked)
                Solver = 2;
            Simulator Sim = new Simulator(Solver);
            List<double> Result = Sim.SimulateRandomInstances(int.Parse(textBox6.Text), int.Parse(textBox7.Text));
            textBox5.Text = "Conf. Lower: " + Result[0].ToString() + "  Conf. Upper: " + Result[1].ToString() + "  #Total: " + Result[2].ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int Solver = 0;
            if (radioButton3.Checked)
                Solver = 0;
            if (radioButton4.Checked)
                Solver = 1;
            if (radioButton5.Checked)
                Solver = 2;
            Simulator Sim = new Simulator(Solver);
            Sim.TestCalcConfidenceRow(int.Parse(textBox10.Text), int.Parse(textBox11.Text), int.Parse(textBox8.Text));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            string Input = textBox1.Text.Replace(" ", "");

            Instance Test = Parse(Input);

            Instance Result;
            if (radioButton1.Checked)
            {
                PopSolver poppi = new PopSolver();
                Result = poppi.Match(Test, Path, checkBox1.Checked);
            }
            else
            {
                PopSolver2 poppi = new PopSolver2();
                Result = poppi.Match(Test, Path, checkBox1.Checked);
            }
            textBox2.Text = Output(Result);
            GlobalResult = Result;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Path);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = Path;
            DialogResult objResult = folderBrowserDialog1.ShowDialog(this);
            if (objResult == DialogResult.OK)
                Path = folderBrowserDialog1.SelectedPath;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SwitchingGraph SwGraph = new SwitchingGraph(GlobalResult);
            if (checkBox1.Checked)
                SwGraph.Draw().Save(Path + "\\SwitchingGraph.jpg");
            textBox9.Text = SwGraph.Count().ToString();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SwitchingGraph SwGraph = new SwitchingGraph(GlobalResult);
            if (checkBox1.Checked)
                SwGraph.Draw().Save(Path + "\\SwitchingGraph.jpg");
            textBox9.Text = SwGraph.Count().ToString();
            SwGraph.Enumerate(Path + "\\Enumerate\\");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Instance Test = Parse(textBox13.Text);
            LPSolver MySolver = new LPSolver();
            Instance Result = MySolver.Match(Test, PathLP, checkBox2.Checked);

            textBox12.Text = Output(Result);
            textBox14.Text = MySolver.LP;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = PathLP;
            DialogResult objResult = folderBrowserDialog1.ShowDialog(this);
            if (objResult == DialogResult.OK)
                PathLP = folderBrowserDialog1.SelectedPath;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(PathLP);
        }

    }

    /// <summary>
    /// Class for creating and testing multiple (random) instances
    /// </summary>
    public class Simulator
    {

        Solver ChosenSolver; // Solver which should be used 

        List<List<int>> Permutations;      
        List<List<int>> Perms;
        long CC; // Counter of total created instances
        long NotPop; // Counter of instances not allowing a popular matching

        public Simulator(int solver)
        {
            switch (solver)
            {
                case 0:
                    ChosenSolver = new PopSolver2();
                    break;
                case 1:
                    ChosenSolver = new PopSolver();
                    break;
                case 2:
                    ChosenSolver = new LPSolver();
                    break;
            }
        }

        public List<List<int>> CreatePermutation(List<int> elements)
         {
            Permutations = new List<List<int>>();
            RecCreatePermutation(new List<int>(), elements);
            return Permutations;
        }

        /// <summary>
        /// Creates a deep copy of the given list.
        /// </summary>
        /// <param name="dummy"></param>
        /// <returns></returns>
        private List<int> Copy(List<int> dummy)
        {
            List<int> Result = new List<int>();
            foreach (int i in dummy)
                Result.Add(i);
            return Result;
        }

        /// <summary>
        /// Create all permutations of the given elements.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="elements"></param>
        public void RecCreatePermutation(List<int> current, List<int> elements)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                List<int> NewCurrent = Copy(current);
                List<int> NewElements = Copy(elements);
                NewCurrent.Add(elements[i]);
                NewElements.RemoveAt(i);
                if (NewElements.Count == 0)
                {
                    Permutations.Add(NewCurrent);
                }
                else
                    RecCreatePermutation(NewCurrent, NewElements);
            }
        }

        /// <summary>
        /// Calculates the factorial of n.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public ulong Factorial(ulong n)
        {
            ulong Result = 1;
            for (ulong i = 1; i <= n; i++)
            {
                Result *= i;
            }
            return Result;
        }

        /// <summary>
        /// Recursively create all possible instances by creating all possible combinations of priority lists.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="n"></param>
        /// <param name="seeds"></param>
        /// <param name="start"></param>
        /// <param name="print"></param>
        /// <param name="path"></param>
        private void RecPrios(int depth, int n, List<int> seeds, int start, bool print, string path)
        {
            if (depth < n)
            {
                // call the next level with every possible priority configuration
                for (int a = 0; a < (int)Factorial((ulong)(n)); a++)
                {
                    List<int> dummy = Copy(seeds);
                    dummy.Add(a);
                    RecPrios(depth + 1, n, dummy, a, print, path);
                }
            }
            else
            {
                // if final level reached, create instance
                List<Tuple<int, int>>[] Prios = new List<Tuple<int, int>>[n];
                for (int i = 0; i < n; i++)
                {
                    Prios[i] = new List<Tuple<int, int>>();
                }

                for (int i = 0; i < n; i++)
                {
                    for (int x = 0; x < Perms[seeds[i]].Count; x++)
                    {
                        Prios[i].Add(new Tuple<int, int>(Perms[seeds[i]][x], x));
                    }
                }

                Instance Temp = new Instance(n, n, 1, Prios);


                // check for popular matching
                Temp = ChosenSolver.Match(Temp, path, print);

                // multiply this value by Factor, since the first applicant always gets the same priority seed - this way, a permutation less has to be examined
                double Factor = Factorial((ulong)n);

                if (Temp == null)
                {
                    NotPop += (long)Factor;
                }

                CC += (long)Factor;
            }
        }

        /// <summary>
        /// Do SimulateRandomInstances(n, a) for each n between start and end
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="a"></param>
        public void TestCalcConfidenceRow(int start, int end, int a)
        {
            List<double>[] Data = new List<double>[end - start + 1];
            for (int i = start; i <= end; i++)
            {
                List<double> Values = SimulateRandomInstances(i, a);
                Data[i - start] = Values;
            }
            ExcelDisplayer.Print(Application.StartupPath + "\\confidence.xls", Data);
        }

        /// <summary>
        /// Simulate a random instances of size n and calculate the percentage of instances allowing a popular matching by specifying a confidence interval
        /// </summary>
        /// <param name="n"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public List<double> SimulateRandomInstances(int n, int a)
        {
            double NotPop = 0;
            double CC = 0;

            double[] Results = new double[a];
            for (int i = 0; i < a; i++)
            {
                // create a random instances of size n and check for popularity
                Instance Test = Instance.GetRandom(n, n, n, 1, -1);

                Test = ChosenSolver.Match(Test, "", false);
                if (Test == null)
                {
                    NotPop++;
                    Results[i] = 1;
                }
                else
                    Results[i] = 0;
                CC++;
            }

            double mu = (NotPop) / a; // average

            double std = 0; // std deviation
            for (int i = 0; i < a; i++)
            {
                std += Math.Pow(Results[i] - mu, 2);
            }
            std /= (double)(a - 1);

            std = Math.Sqrt(std);
            double EmpResult = NotPop / (double)a;
            double TrueCC = Math.Pow((double)Factorial((ulong)n), n);

            double Result = EmpResult * TrueCC;

            // 95% confidence interval
            double Lower = ((mu - 1.96 * std / Math.Sqrt(a))) * TrueCC;
            double Upper = ((mu + 1.96 * std / Math.Sqrt(a))) * TrueCC;

            List<double> Values = new List<double>();
            Values.Add(Lower);
            Values.Add(Upper);
            Values.Add(TrueCC);
            return Values;
        }

        /// <summary>
        /// Calculates all matching instances without ties for the given size and calculates the percentage of instances which allow a popular matching.
        /// </summary>
        /// <param name="n">The instance size = number of applicants, posts.</param>
        /// <param name="print">True if the algorithm should print debug pictures in the folder path.</param>
        /// <returns>A tuple consisting of the number of instances not allowing a popular matching and the total number of instances.</returns>
        public Tuple<long, long> FindAllMatchings(int n, bool print, string path)
        {
            List<Instance> FoundMatchings = new List<Instance>();

            List<int> dummy = new List<int>();
            for (int i = 0; i < n; i++)
            {
                dummy.Add(i);

            }
            Perms = CreatePermutation(dummy);

            List<int> Start = new List<int>();
            Start.Add(0);

            RecPrios(1, n, Start, 0, print, path); // Recursively iterate over all possible instances.

            return new Tuple<long, long>(NotPop, CC);
        }
    }

    /// <summary>
    /// For displaying data in Excel.
    /// </summary>
    public class ExcelDisplayer
    {
        public static void Print(string file, List<double>[] data)
        {
            Excel.Application myExcelApplication;
            Excel.Workbook myExcelWorkbook;
            Excel.Worksheet myExcelWorkSheet;
            myExcelApplication = null;

            myExcelApplication = new Excel.Application();
            myExcelApplication.Visible = true;
            myExcelApplication.ScreenUpdating = true;

            var myCount = myExcelApplication.Workbooks.Count;
            myExcelWorkbook = (Excel.Workbook)(myExcelApplication.Workbooks.Add(System.Reflection.Missing.Value));
            myExcelWorkSheet = (Excel.Worksheet)myExcelWorkbook.ActiveSheet;

            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    myExcelWorkSheet.Cells[j + 3, i + 3] = data[i][j].ToString().Replace(',', '.');
                }
            }

            myExcelWorkbook.SaveAs(file, Excel.XlFileFormat.xlWorkbookNormal);
            myExcelWorkbook.Close(true, System.Reflection.Missing.Value, System.Reflection.Missing.Value);

            myExcelApplication.Quit();
        }
    }

    public class Instance
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">Numer of applicants</param>
        /// <param name="m">Number of posts</param>
        /// <param name="c">Capacity of the posts</param>
        /// <param name="prios"></param>
        public Instance(int n, int m, int c, List<Tuple<int, int>>[] prios)
        {
            Applicants = new Applicant[n];
            Posts = new Post[m + n];
            this.n = n;
            this.m = m;

            for (int i = 0; i < n; i++)
            {
                Applicants[i] = new Applicant(i);
                Applicants[i].Priorities = new List<Instance.Priority>();
                for (int j = 0; j < prios[i].Count; j++)
                {
                    Applicants[i].Priorities.Add(new Instance.Priority(prios[i].ElementAt(j).Item1, prios[i].ElementAt(j).Item2));
                }
                Applicants[i].Priorities.Add(new Instance.Priority(m + i, prios[i].Count));
            }

            for (int i = 0; i < m; i++)
            {
                Posts[i] = new Post(i, c);
            }
            for (int i = m; i < m + n; i++)
            {
                Posts[i] = new Post(i, 1);
            }

        }

        public Instance Copy()
        {
            Instance result = new Instance(n, m);
            for (int i = 0; i < Applicants.Length; i++)
            {
                result.Applicants[i] = new Applicant(i);
                for (int j = 0; j < Applicants[i].Priorities.Count; j++)
                {
                    result.Applicants[i].Priorities.Add(new Priority(Applicants[i].Priorities[j].Target, Applicants[i].Priorities[j].Rank));
                }
            }
            for (int i = 0; i < Posts.Length; i++)
            {
                result.Posts[i] = new Post(i, Posts[i].Capacity);
            }
            return result;
        }

        public Instance CopyWithMatching()
        {
            Instance result = new Instance(n, m);
            for (int i = 0; i < Applicants.Length; i++)
            {
                result.Applicants[i] = new Applicant(i);
                for (int j = 0; j < Applicants[i].Priorities.Count; j++)
                {
                    result.Applicants[i].Priorities.Add(new Priority(Applicants[i].Priorities[j].Target, Applicants[i].Priorities[j].Rank));
                }
                result.Applicants[i].Matched = Applicants[i].Matched;
            }
            for (int i = 0; i < Posts.Length; i++)
            {
                result.Posts[i] = new Post(i, Posts[i].Capacity);
                result.Posts[i].Matched = Posts[i].Matched;
                result.Posts[i].Full = Posts[i].Full;
                result.Posts[i].NrMatchings = Posts[i].NrMatchings;
                result.Posts[i].Matchings = Copy(Posts[i].Matchings);
            }
            return result;
        }

        public void AddMatch(int tid, int sid)
        {
            Posts[tid].Matchings.Add(sid);
            Applicants[sid].Matched = true;
            if (++Posts[tid].NrMatchings >= Posts[tid].Capacity)
                Posts[tid].Full = true;
        }

        public void MatchPath(List<int> path, bool startApplicant)
        {
            if (startApplicant)
            {
                for (int i = 0; i < path.Count; i += 2)
                {
                    DeleteMatch(path[i]);
                    Applicants[path[i]].Matched = true;
                    Posts[path[i + 1]].Matchings.Add(path[i]);
                    if (++Posts[path[i + 1]].NrMatchings >= Posts[path[i + 1]].Capacity)
                        Posts[path[i + 1]].Full = true;
                }
            }


        }

        public void DeleteMatch(int sid)
        {
            for (int i = 0; i < Posts.Length; i++)
            {
                if (Posts[i].Matchings.Contains(sid))
                {
                    Posts[i].Matchings.Remove(sid);
                    Posts[i].NrMatchings--;

                    Applicants[sid].Matched = false;
                }
                if (Posts[i].NrMatchings < Posts[i].Capacity)
                    Posts[i].Full = false;
            }
        }

        public class Post
        {
            public int ID;
            public List<int> Matchings;
            public int Capacity;
            public bool Matched;
            public int NrMatchings;
            public bool Full;
            public int Type; // 0 = Even, 1 = Odd, 2 = Unreachable
            public int IsF; // 1 if f-post

            public Post(int id, int capacity)
            {
                ID = id;
                Capacity = capacity;
                Matched = false;
                Matchings = new List<int>();
                NrMatchings = 0;
                Type = -1;
                IsF = 0;
            }

        }

        public class Priority
        {
            public int Target;
            public int Rank;

            public Priority(int target, int rank)
            {
                Target = target;
                Rank = rank;
            }
        }

        public class Applicant
        {
            public int ID;
            public List<Priority> Priorities;
            public bool Matched;
            public int Type; // 0 = Odd, 1 = Even, 2 = Unreachable

            public Applicant(int id)
            {
                ID = id;
                Matched = false;
                Priorities = new List<Priority>();
                Type = -1;
            }
        }

        public Instance(int n, int m)
        {
            Applicants = new Applicant[n];
            Posts = new Post[m + n];
            this.n = n;
            this.m = m;
        }

        private List<int> Copy(List<int> dummy)
        {
            List<int> Result = new List<int>();
            foreach (int i in dummy)
                Result.Add(i);
            return Result;
        }

        public int n, m;
        public Applicant[] Applicants;
        public Post[] Posts;
        static Random Rnd = new Random();

        /// <summary>
        /// Returns a random instance.
        /// </summary>
        /// <param name="n">Number of Applicants</param>
        /// <param name="m">Number of Posts</param>
        /// <param name="k">Length of the priority list</param>
        /// <param name="c">capacity of the Posts</param>
        /// <param name="seed">starting seed for random</param>
        /// <returns></returns>
        /// 
        public static Instance GetRandom(int n, int m, int k, int c, int seed)
        {
            Instance Result = new Instance(n, m);

            for (int i = 0; i < n; i++)
            {
                Result.Applicants[i] = new Applicant(i);
                Result.Applicants[i].Priorities = new List<Instance.Priority>();
                for (int j = 0; j < k; j++)
                {
                    int NextPrio = Rnd.Next(m);
                    bool Existing = true;
                    while (Existing)
                    {
                        Existing = false;
                        for (int z = 0; z < Result.Applicants[i].Priorities.Count; z++)
                        {
                            if (NextPrio == Result.Applicants[i].Priorities[z].Target)
                            {
                                Existing = true;
                                break;
                            }
                        }
                        if (Existing)
                            NextPrio = Rnd.Next(m);
                    }
                    Result.Applicants[i].Priorities.Add(new Instance.Priority(NextPrio, j));
                }
                Result.Applicants[i].Priorities.Add(new Instance.Priority(m + i, k));
            }

            for (int i = 0; i < m; i++)
            {
                Result.Posts[i] = new Post(i, c);
            }
            for (int i = m; i < m + n; i++)
            {
                Result.Posts[i] = new Post(i, 1);
            }

            return Result;
        }

        /// <summary>
        /// Represents the instance as a bitmap.
        /// </summary>
        /// <returns></returns>
        public Bitmap Draw()
        {
            double Scale = (double)n / 10;

            Bitmap Result = new Bitmap((int)(1000 * Scale), (int)(1000 * Scale));
            Graphics G = Graphics.FromImage(Result);
            Pen P = new Pen(new SolidBrush(Color.Black));

            int HeightN = (int)((int)(1000 * Scale) / (double)(n + 2));

            int Capacity = 0;
            for (int i = 0; i < Posts.Length - n; i++)
            {
                Capacity += Posts[i].Capacity;
            }

            int HeightM;
            if (Capacity > n)
                HeightM = (int)((int)(1000 * Scale) / (double)(Capacity + 2));
            else
                HeightM = (int)((int)(1000 * Scale) / (double)(n + 2));

            int Left = (int)(800 * Scale);
            int TopOffset = 0;
            for (int i = 0; i < n; i++)
            {
                Pen P1 = new Pen(new SolidBrush(Color.Black));
                string Type = "-1";

                switch (Applicants[i].Type)
                {
                    case 0:
                        P1.Color = Color.Cyan;

                        break;
                    case 1:
                        P1.Color = Color.Yellow;
                        break;
                    case 2:
                        P1.Color = Color.Gray;
                        break;
                }
                Type = Applicants[i].Type.ToString();

                G.DrawEllipse(P1, (int)(300 * Scale), (i + 1) * HeightN, (int)(Scale * 100), (int)(HeightN * 0.8));
                Font drawFont = new Font("Arial", 16);
                SolidBrush drawBrush = new SolidBrush(Color.Black);
                G.DrawString(Type, drawFont, drawBrush, (int)(300 * Scale), (i + 1) * HeightN);
                Color PenColor = Color.Black;
                for (int j = 0; j < Applicants[i].Priorities.Count; j++)
                {
                    if (Applicants[i].Priorities[j].Target >= m)
                    {
                        Left = (int)(100 * Scale);
                        TopOffset = m;
                    }
                    else
                    {
                        Left = (int)(800 * Scale);
                        TopOffset = 0;
                    }

                    if (Applicants[i].Priorities[j].Rank % 2 == 0)
                        PenColor = Color.Black;
                    else
                        PenColor = Color.DarkGreen;

                    G.DrawLine(new Pen(new SolidBrush(PenColor), ((int)(10) - (int)(Applicants[i].Priorities[j].Rank * 2))), (int)(Scale * 300), (i + 1) * HeightN + HeightN / 2, Left, (Applicants[i].Priorities[j].Target - TopOffset + 1) * HeightM + HeightM / 2);
                }
            }

            Left = (int)(800 * Scale);
            TopOffset = 0;

            for (int i = 0; i < m + n; i++)
            {
                if (i >= m)
                {
                    Left = (int)(100 * Scale);
                    TopOffset = m;
                }
                else
                {
                    Left = (int)(800 * Scale);
                    TopOffset = 0;
                }

                Pen P1 = new Pen(new SolidBrush(Color.Black));
                string Type = "-1";
                switch (Posts[i].Type)
                {
                    case 0:
                        P1.Color = Color.Cyan;
                        break;
                    case 1:
                        P1.Color = Color.Yellow;
                        break;
                    case 2:
                        P1.Color = Color.Gray;
                        break;

                }
                Type = Posts[i].Type.ToString();
                Font drawFont = new Font("Arial", 16);
                SolidBrush drawBrush = new SolidBrush(Color.Black);
                G.DrawRectangle(P1, Left, (i + 1 - TopOffset) * HeightM, (int)(Scale * 100), (int)(HeightM * 0.8));
                G.DrawString(Type, drawFont, drawBrush, Left, (i + 1 - TopOffset) * HeightM);
                for (int j = 0; j < Posts[i].Matchings.Count; j++)
                {
                    G.DrawLine(new Pen(new SolidBrush(Color.Red), (int)(10)), Left, (i + 1 - TopOffset) * HeightM + HeightM / 2, (int)(Scale * 300), (Posts[i].Matchings[j] + 1) * HeightN + HeightN / 2);
                }
            }
            G.Dispose();
            return Result;
        }
    }

    /// <summary>
    /// Tries to find a popular matching without ties.
    /// </summary>
    public class PopSolver : Solver
    {
        public Instance Match(Instance instance, string path, bool print)
        {
            Instance oldcopy = instance.Copy();

            if (!System.IO.Directory.Exists(path) && print)
                System.IO.Directory.CreateDirectory(path);

            if (print)
            {
                Bitmap Bmp1 = instance.Draw();
                Bmp1.Save(path + "/0Start.bmp");
                Bmp1.Dispose();
            }

            instance = GetReduced(instance);

            if (print)
            {
                Bitmap Bmp2 = instance.Draw();
                Bmp2.Save(path + "/1Reduced.bmp");
                Bmp2.Dispose();
            }

            if (print)
            {
                Bitmap Bmp3 = instance.Draw();
                Bmp3.Save(path + "/2Inflated.bmp");
                Bmp3.Dispose();
            }


            // check for applicant-complete matching
            instance = GetApplicantComplete(instance);

            if (instance == null)
            {

                if (print)
                {
                    Bitmap Result = new Bitmap(1000, 1000);
                    Graphics G = Graphics.FromImage(Result);
                    Pen P = new Pen(new SolidBrush(Color.Black));
                    G.DrawString("No Popular Matching", new Font("Arial", 36), new SolidBrush(Color.Black), 100, 100);
                    Result.Save(path + "/3NoPop.bmp");
                    G.Dispose();
                    Result.Dispose();
                }
                return null;
            }
            else
            {
                if (print)
                {
                    Bitmap Bmp4 = instance.Draw();
                    Bmp4.Save(path + "/3ApplicantComplete.bmp");
                    Bmp4.Dispose();
                }
            }

            List<int>[] FPosts = new List<int>[instance.Posts.Length];

            for (int i = 0; i < instance.Posts.Length; i++)
            {
                FPosts[i] = new List<int>();
            }

            for (int i = 0; i < instance.Applicants.Length; i++)
            {
                FPosts[instance.Applicants[i].Priorities.ElementAt(0).Target].Add(i);
            }

            for (int i = 0; i < instance.Posts.Length; i++)
            {
                if (FPosts[i].Count > 0 && instance.Posts[i].NrMatchings == 0)
                {
                    instance.DeleteMatch(FPosts[i][0]);
                    instance.AddMatch(i, FPosts[i][0]);
                }
            }

            if (print)
            {
                Bitmap Bmp4 = instance.Draw();
                Bmp4.Save(path + "/4AllFPostsMatched.bmp");
                Bmp4.Dispose();
            }

            return instance;
        }

        /// <summary>
        /// Creates the reduced subgraph.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private Instance GetReduced(Instance instance)
        {
            // Determine the f-posts
            List<int> FirstPosts = new List<int>();
            for (int i = 0; i < instance.n; i++)
            {
                if (!FirstPosts.Contains(instance.Applicants[i].Priorities.ElementAt(0).Target))
                    FirstPosts.Add(instance.Applicants[i].Priorities.ElementAt(0).Target);
            }

            for (int i = 0; i < instance.n; i++)
            {
                bool SecondFound = false;
                int j;

                for (j = 1; j < instance.Applicants[i].Priorities.Count - 1; j++)
                {

                    if (FirstPosts.Contains(instance.Applicants[i].Priorities.ElementAt(j).Target))
                    {
                        // delete all f-posts except the first one from priority list
                        instance.Applicants[i].Priorities.Remove(instance.Applicants[i].Priorities.ElementAt(j));
                        j--;
                    }
                    else
                    {
                        // look if there is some not f-post before l on the list
                        SecondFound = true;
                        break;
                    }
                }
                if (SecondFound)
                {
                    // if some second priority has been found before l, delete all posts behind it
                    for (j = j + 1; j < instance.Applicants[i].Priorities.Count; j++)
                    {
                        instance.Applicants[i].Priorities.Remove(instance.Applicants[i].Priorities.ElementAt(j));
                        j--;
                    }
                }


                if (instance.Applicants[i].Priorities.Count == 1)
                    return null;
            }

            // now only applicant has 2 edges, to f- and s-post
            return instance;
        }

        /// <summary>
        /// Replace posts with capacity c by c posts
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private Instance Inflate(Instance instance)
        {
            int Capacity = 0;

            // determine total complexity of the posts
            for (int i = 0; i < instance.m; i++)
            {
                Capacity += instance.Posts[i].Capacity;
            }

            Instance NewInstance = new Instance(instance.n, Capacity);

            for (int i = 0; i < instance.n; i++)
            {
                NewInstance.Applicants[i] = new Instance.Applicant(i);
            }

            int Counter = 0;
            for (int i = 0; i < instance.m; i++)
            {
                // traverse all old posts
                List<Tuple<int, int>> Suitors = new List<Tuple<int, int>>();
                for (int j = 0; j < instance.n; j++)
                {
                    // add all applicants connected to post j to the list of interested people
                    for (int z = 0; z < instance.Applicants[j].Priorities.Count; z++)
                    {
                        if (instance.Applicants[j].Priorities.ElementAt(z).Target == i)
                        {
                            Suitors.Add(new Tuple<int, int>(j, z));
                        }
                        // in the last iteration of the loop maybe add an edge to the l post of the applicant and mind the changed ID
                        if (i == instance.m - 1 && instance.Applicants[j].Priorities.ElementAt(z).Target >= instance.m)
                            NewInstance.Applicants[j].Priorities.Add(new Instance.Priority(instance.Applicants[j].Priorities.ElementAt(z).Target + (Capacity - instance.m), 0));

                    }
                }

                // more applicants are interested in the post than there is capacity
                if (Suitors.Count >= instance.Posts[i].Capacity)
                {
                    // fill every new posts with about equally many interested people
                    // calculate current interested people per space
                    int Factor = (int)((Suitors.Count) / (decimal)instance.Posts[i].Capacity);
                    int SCounter = 0;
                    for (int j = 0; j < instance.Posts[i].Capacity; j++)
                    {
                        // traverse all posts
                        NewInstance.Posts[Counter] = new Instance.Post(Counter, 1);

                        // adapt factor depending on still existing interested people and remaining new posts
                        Factor = (int)Math.Ceiling((Suitors.Count - SCounter) / ((decimal)instance.Posts[i].Capacity - j));

                        // add according to the factor calculated above many people to the new post
                        for (int z = 0; z < Factor; z++)
                        {
                            // mind the priority list
                            if (Suitors.ElementAt(SCounter).Item2 == 0 || NewInstance.Applicants[Suitors.ElementAt(SCounter).Item1].Priorities.Count == 0)
                                NewInstance.Applicants[Suitors.ElementAt(SCounter++).Item1].Priorities.Insert(0, new Instance.Priority(Counter, 0));
                            else
                            {
                                NewInstance.Applicants[Suitors.ElementAt(SCounter++).Item1].Priorities.Insert(1, new Instance.Priority(Counter, 1));
                            }
                        }

                        Counter++;
                    }
                }
                else
                {
                    // other more available places than interested people exist, just fill the places
                    int Next = Counter + instance.Posts[i].Capacity;
                    for (int j = 0; j < Suitors.Count; j++)
                    {
                        NewInstance.Posts[Counter] = new Instance.Post(Counter, 1);

                        if (Suitors.ElementAt(j).Item2 == 0)
                            NewInstance.Applicants[Suitors.ElementAt(j).Item1].Priorities.Insert(0, new Instance.Priority(Counter, 0));
                        else
                            NewInstance.Applicants[Suitors.ElementAt(j).Item1].Priorities.Insert(1, new Instance.Priority(Counter, 1));

                        Counter++;
                    }
                    for (; Counter < Next; Counter++)
                    {
                        NewInstance.Posts[Counter] = new Instance.Post(Counter, 1);
                    }
                }

            }
            for (int j = Counter; j < NewInstance.Posts.Length; j++)
            {
                NewInstance.Posts[j] = new Instance.Post(j, 1);
            }
            return NewInstance;
        }

        /// <summary>
        /// Check for applicant complete matching
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private Instance GetApplicantComplete(Instance instance)
        {
            List<int>[] Suitors = new List<int>[instance.Posts.Length];
            for (int i = 0; i < instance.Posts.Length; i++)
            {
                Suitors[i] = new List<int>();
            }

            // create suitors list
            for (int i = 0; i < instance.Applicants.Length; i++)
            {
                for (int j = 0; j < instance.Applicants[i].Priorities.Count; j++)
                {
                    Suitors[instance.Applicants[i].Priorities.ElementAt(j).Target].Add(i);
                }
            }

            int Unmatched = instance.Posts.Length; // # of not full posts
            int UnmatchedApplicants = instance.Applicants.Length; // # unmatched applicants

            for (int i = 0; i < instance.Posts.Length; i++)
            {
                // if a post is connected to only one not matched applicant, match this
                if (Suitors[i].Count == 1 && !instance.Posts[i].Matched && !instance.Applicants[Suitors[i].ElementAt(0)].Matched)
                {
                    instance.Posts[i].Matched = true;
                    instance.AddMatch(i, Suitors[i].ElementAt(0));
                    UnmatchedApplicants--;
                    int Applicant = Suitors[i].ElementAt(0);
                    for (int j = 0; j < instance.Posts.Length; j++)
                    {
                        Suitors[j].Remove(Applicant);
                    }
                }
            }

            for (int i = 0; i < instance.Posts.Length; i++)
            {
                // delete not connected posts
                if (Suitors[i].Count == 0)
                {
                    instance.Posts[i].Matched = true;
                    Unmatched--;
                }
            }


            // check if there is enough space for the remaining applicants
            if (Unmatched < UnmatchedApplicants)
                return null;
            else
            {
                // if yes, walk over the disjoint cycle and take every second edge
                for (int i = 0; i < instance.Applicants.Length; i++)
                {
                    instance = GoPath(instance, i);
                    if (instance == null)
                        instance = null;
                }
            }

            return instance;
        }

        /// <summary>
        /// Completess the applicant-complete matching, by traversing the disjoint cycle and taking every second edge.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private Instance GoPath(Instance instance, int start)
        {
            if (instance.Applicants[start].Matched)
                return instance;
            else
            {
                int Target = -1;
                if (!instance.Posts[instance.Applicants[start].Priorities.ElementAt(0).Target].Full)
                {
                    instance.AddMatch(instance.Applicants[start].Priorities.ElementAt(0).Target, start);
                    Target = instance.Applicants[start].Priorities.ElementAt(0).Target;

                }
                else
                {
                    if (!instance.Posts[instance.Applicants[start].Priorities.ElementAt(1).Target].Full)
                    {
                        instance.AddMatch(instance.Applicants[start].Priorities.ElementAt(1).Target, start);
                        Target = instance.Applicants[start].Priorities.ElementAt(1).Target;
                    }
                    else
                        return instance;
                }

                for (int j = 0; j < instance.Applicants.Length; j++)
                {
                    if (instance.Applicants[j].Priorities.ElementAt(0).Target == Target || instance.Applicants[j].Priorities.ElementAt(1).Target == Target)
                    {
                        if (!instance.Applicants[j].Matched)
                            instance = GoPath(instance, j);
                    }
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Solver for matchings with ties
    /// </summary>
    public class PopSolver2 : Solver
    {
        /// <summary>
        /// used for determining a maximum matching
        /// </summary>
        public class DirectedGraph
        {
            public class Node
            {
                public int Type;
                public int ID; // 0 = Applicant, 1 = Post
                public List<int> Outgoing;
                public int PrevId;
                public bool Marked;

                public Node(int type, int id)
                {
                    Type = type;
                    ID = id;
                    Outgoing = new List<int>();
                    PrevId = -1;
                    Marked = false;
                }
            }

            public List<Node> Nodes;

            public DirectedGraph(Instance inst, int useOnly)
            {
                Nodes = new List<Node>();

                for (int j = 0; j < inst.Posts.Length; j++)
                {
                    Nodes.Add(new Node(1, j));
                }

                for (int i = 0; i < inst.Applicants.Length; i++)
                {
                    Nodes.Add(new Node(0, inst.Posts.Length + i));
                    for (int k = 0; k < inst.Applicants[i].Priorities.Count; k++)
                    {
                        if (useOnly != -1 && inst.Applicants[i].Priorities[k].Rank != useOnly)
                            continue;
                        if (inst.Posts[inst.Applicants[i].Priorities[k].Target].Matchings.Contains(i))
                        {
                            Nodes.ElementAt(inst.Applicants[i].Priorities[k].Target).Outgoing.Add(inst.Posts.Length + i);
                        }
                        else
                        {
                            Nodes.ElementAt(inst.Posts.Length + i).Outgoing.Add(inst.Applicants[i].Priorities[k].Target);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// DFS to find an augmenting path
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="inst"></param>
        /// <returns></returns>
        private List<int> DFS(DirectedGraph graph, Instance inst)
        {
            Stack<DirectedGraph.Node> Stack = new Stack<DirectedGraph.Node>();

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (graph.Nodes[i].Type == 0 && !inst.Applicants[graph.Nodes[i].ID - inst.Posts.Length].Matched)
                {
                    Stack.Push(graph.Nodes[i]);
                }
            }

            while (Stack.Count > 0)
            {
                DirectedGraph.Node n = Stack.Pop();
                if (n.Type == 1 && inst.Posts[n.ID].NrMatchings == 0)
                {
                    List<int> Path = new List<int>();
                    DirectedGraph.Node Current = n;
                    Path.Insert(0, n.ID);
                    while (Current.PrevId != -1)
                    {
                        Current = graph.Nodes[Current.PrevId];
                        if (Current.Type == 0)
                            Path.Insert(0, Current.ID - inst.Posts.Length);
                        else
                            Path.Insert(0, Current.ID);
                    }
                    return Path;
                }
                for (int j = 0; j < n.Outgoing.Count; j++)
                {
                    int Target = n.Outgoing[j];


                    int Prev = n.ID;

                    if (!graph.Nodes[Target].Marked)
                    {
                        graph.Nodes[Target].PrevId = Prev;
                        Stack.Push(graph.Nodes[Target]);
                        graph.Nodes[Target].Marked = true;
                    }
                }

            }

            return null;
        }

        public Instance Match(Instance instance, string path, bool print)
        {
            if (!System.IO.Directory.Exists(path) && print)
                System.IO.Directory.CreateDirectory(path);

            if (print)
            {
                Bitmap Bmp1 = instance.Draw();
                Bmp1.Save(path + "/0Start.bmp");
                Bmp1.Dispose();
            }

            instance = Inflate(instance);

            if (print)
            {
                Bitmap Bmp1 = instance.Draw();
                Bmp1.Save(path + "/1Inflated.bmp");
                Bmp1.Dispose();
            }

            instance = MaxMatching(instance, 0);

            if (print)
            {
                Bitmap Bmp1 = instance.Draw();
                Bmp1.Save(path + "/2FirstMax.bmp");
                Bmp1.Dispose();
            }

            instance = DetermineTypes(instance);

            if (print)
            {
                Bitmap Bmp3 = instance.Draw();
                Bmp3.Save(path + "/3Types.bmp");
                Bmp3.Dispose();

            }

            instance = GetReduced(instance);

            if (print)
            {
                Bitmap Bmp4 = instance.Draw();
                Bmp4.Save(path + "/4Reduced.bmp");
                Bmp4.Dispose();

            }

            instance = DeleteEdges(instance);

            if (print)
            {
                Bitmap Bmp5 = instance.Draw();
                Bmp5.Save(path + "/5EdgesDeleted.bmp");
                Bmp5.Dispose();
            }

            instance = MaxMatching(instance, -1);

            if (print)
            {
                Bitmap Bmp6 = instance.Draw();
                Bmp6.Save(path + "/6GeneralMaxMatching.bmp");
                Bmp6.Dispose();

            }

            for (int i = 0; i < instance.Applicants.Length; i++)
            {
                if (!instance.Applicants[i].Matched)
                {
                    if (print)
                    {
                        Bitmap Bmp7 = instance.Draw();
                        Bmp7.Save(path + "/7NoPopularMatching.bmp");
                        Bmp7.Dispose();
                    }
                    return null;
                }
            }

            return instance;
        }

        private Instance DeleteEdges(Instance instance)
        {
            for (int i = 0; i < instance.Applicants.Length; i++)
            {
                for (int j = 0; j < instance.Applicants[i].Priorities.Count; j++)
                {
                    if ((instance.Applicants[i].Type == 0 && instance.Posts[instance.Applicants[i].Priorities[j].Target].Type == 0) || (instance.Applicants[i].Type == 0 && instance.Posts[instance.Applicants[i].Priorities[j].Target].Type == 2) || (instance.Applicants[i].Type == 2 && instance.Posts[instance.Applicants[i].Priorities[j].Target].Type == 0))
                    {
                        instance.Applicants[i].Priorities.RemoveAt(j);
                        j--;
                    }

                }
            }
            return instance;
        }

        private Instance GetReduced(Instance instance)
        {
            for (int i = 0; i < instance.n; i++)
            {
                int HighestSecond = int.MaxValue;

                for (int j = 0; j < instance.Applicants[i].Priorities.Count; j++)
                {
                    if (instance.Applicants[i].Priorities.ElementAt(j).Rank != 0 && instance.Posts[instance.Applicants[i].Priorities.ElementAt(j).Target].Type != 1)
                    {
                        instance.Applicants[i].Priorities.RemoveAt(j);
                        j--;
                    }
                    else
                    {
                        if (instance.Applicants[i].Priorities[j].Rank < HighestSecond && instance.Posts[instance.Applicants[i].Priorities[j].Target].Type == 1)
                        {
                            HighestSecond = (instance.Applicants[i].Priorities[j].Rank);
                        }
                    }
                }

                for (int j = 0; j < instance.Applicants[i].Priorities.Count; j++)
                {
                    if (instance.Applicants[i].Priorities.ElementAt(j).Rank > HighestSecond)
                    {
                        instance.Applicants[i].Priorities.RemoveAt(j);
                        j--;
                    }
                }

            }

            // now every applicant has only 2 edges, to his f- and s-post
            return instance;

        }
        List<int> FinalPath = null;

        private void GoTypePath(Instance instance, bool Applicant, bool match, bool odd, int id)
        {
            if (Applicant)
            {
                if (odd)
                {
                    instance.Applicants[id].Type = 0;
                }
                else
                {
                    instance.Applicants[id].Type = 1;
                }
                if (!match)
                {
                    for (int j = 0; j < instance.Applicants[id].Priorities.Count; j++)
                    {
                        if (instance.Posts[instance.Applicants[id].Priorities[j].Target].Type == -1 && instance.Applicants[id].Priorities[j].Rank == 0)
                            GoTypePath(instance, false, true, !odd, instance.Applicants[id].Priorities[j].Target);
                    }
                }
                if (match)
                {
                    for (int j = 0; j < instance.Applicants[id].Priorities.Count; j++)
                    {
                        if (instance.Posts[instance.Applicants[id].Priorities[j].Target].Matchings.Contains(id))
                        {
                            if (instance.Posts[instance.Applicants[id].Priorities[j].Target].Type == -1 && instance.Applicants[id].Priorities[j].Rank == 0)
                                GoTypePath(instance, false, false, !odd, instance.Applicants[id].Priorities[j].Target);

                        }
                    }
                }

            }
            else
            {
                if (odd)
                {
                    instance.Posts[id].Type = 0;
                }
                else
                {
                    instance.Posts[id].Type = 1;
                }
                if (match)
                {
                    for (int i = 0; i < instance.Posts[id].Matchings.Count; i++)
                    {
                        if (instance.Applicants[instance.Posts[id].Matchings[i]].Type == -1)
                            GoTypePath(instance, true, false, !odd, instance.Posts[id].Matchings[i]);
                    }
                }
                if (!match)
                {
                    for (int i = 0; i < instance.Applicants.Length; i++)
                    {
                        for (int j = 0; j < instance.Applicants[i].Priorities.Count; j++)
                        {
                            if (instance.Applicants[i].Priorities[j].Target == id && instance.Applicants[i].Priorities[j].Rank == 0)
                            {
                                if (instance.Applicants[i].Type == -1)
                                    GoTypePath(instance, true, true, !odd, i);

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determine the types of the nodes.
        /// 0 = Odd
        /// 1 = Even
        /// 2 = Unreachable
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private Instance DetermineTypes(Instance instance)
        {
            // Unmatched nodes are directly even, go possible paths to determine the other paths.
            for (int i = 0; i < instance.Applicants.Length; i++)
            {
                if (!instance.Applicants[i].Matched)
                {
                    instance.Applicants[i].Type = 1;
                    for (int j = 0; j < instance.Applicants[i].Priorities.Count; j++)
                    {
                        if (instance.Posts[instance.Applicants[i].Priorities[j].Target].Type == -1 && instance.Applicants[i].Priorities[j].Rank == 0)
                            GoTypePath(instance, false, true, true, instance.Applicants[i].Priorities[j].Target);
                    }
                }
            }
            for (int i = 0; i < instance.Posts.Length; i++)
            {
                if (!instance.Posts[i].Full)
                {
                    instance.Posts[i].Type = 1;

                    for (int j = 0; j < instance.Applicants.Length; j++)
                    {
                        for (int k = 0; k < instance.Applicants[j].Priorities.Count; k++)
                        {
                            if (instance.Applicants[j].Priorities[k].Target == i && instance.Applicants[j].Type == -1 && instance.Applicants[j].Priorities[k].Rank == 0)
                                GoTypePath(instance, true, true, true, j);
                        }
                    }
                }
            }


            // unreachable nodes
            for (int i = 0; i < instance.Applicants.Length; i++)
            {
                if (instance.Applicants[i].Type == -1)
                    instance.Applicants[i].Type = 2;
            }

            for (int i = 0; i < instance.Posts.Length; i++)
            {
                if (instance.Posts[i].Type == -1)
                    instance.Posts[i].Type = 2;
            }

            return instance;
        }

        /// <summary>
        /// Uses a basic augmenting path algorithm to find a maximum matching.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="useOnly"></param>
        /// <returns></returns>
        private Instance MaxMatching(Instance instance, int useOnly)
        {
            FinalPath = new List<int>();
            while (FinalPath != null)
            {
                FinalPath = DFS(new DirectedGraph(instance, useOnly), instance);
                if (FinalPath != null)
                    instance.MatchPath(FinalPath, true);

            }
            return instance;
        }

        private List<int> Copy(List<int> dummy)
        {
            List<int> Result = new List<int>();
            foreach (int i in dummy)
                Result.Add(i);
            return Result;
        }
        /// <summary>
        /// Replace posts with capacity c by c posts.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private Instance Inflate(Instance instance)
        {
            int Capacity = 0;

            for (int i = 0; i < instance.m; i++)
            {
                Capacity += instance.Posts[i].Capacity;
            }

            Instance NewInstance = new Instance(instance.n, Capacity);

            for (int i = 0; i < instance.n; i++)
            {
                NewInstance.Applicants[i] = new Instance.Applicant(i);
            }

            int Counter = 0;
            for (int i = 0; i < instance.m + instance.n; i++)
            {
                for (int k = 0; k < instance.Posts[i].Capacity; k++)
                {
                    NewInstance.Posts[Counter] = new Instance.Post(Counter, 1);

                    for (int j = 0; j < instance.n; j++)
                    {
                        for (int z = 0; z < instance.Applicants[j].Priorities.Count; z++)
                        {
                            if (instance.Applicants[j].Priorities[z].Target == i)
                            {

                                NewInstance.Applicants[j].Priorities.Add(new Instance.Priority(Counter, instance.Applicants[j].Priorities[z].Rank));
                            }
                        }
                    }

                    Counter++;
                }
            }
            return NewInstance;
        }

    }

    public class SwitchingGraph
    {
        public class Node
        {
            public Instance.Post Post;
            public List<Edge> Outgoing;
            public Node Prev;
            public bool IsS;
            public bool Found;

            public Node(Instance.Post p)
            {
                Post = p;
                Outgoing = new List<Edge>();
                Component = -1;
                Prev = null;
                IsS = true;
                Found = false;
            }

            public int X, Y;

            public int Component;
        }

        public class Edge
        {
            public Instance.Applicant Applicant;
            public Node Start;
            public Node End;

            public Edge(Node start, Node end, Instance.Applicant app)
            {
                Applicant = app;
                Start = start;
                End = end;
            }
        }

        public class Component
        {
            public static int statID = 0;
            public List<Node> Nodes;
            public bool IsCycle;
            public int ID;
            public Node Sink;
            public int S;
            public List<Node> Cycle;
            public List<List<Node>> Paths;

            public Component()
            {
                Nodes = new List<Node>();
                ID = statID++;
                IsCycle = false;
                Sink = null;
                S = 0;
                Cycle = new List<Node>();
                Paths = new List<List<Node>>();
            }

            public List<Node> GetPath(int id)
            {
                if (id == 0)
                    return null;
                else
                {
                    if (IsCycle)
                        return Cycle;
                    else
                        return Paths[id];
                }
            }
        }

        List<Node> Nodes;
        List<Component> Components;
        Instance BaseInstance;

        public SwitchingGraph(Instance instance)
        {
            BaseInstance = instance;
            Component.statID = 0;
            Nodes = new List<Node>();
            foreach (Instance.Post p in instance.Posts)
            {
                Node Temp = new Node(p);
                Nodes.Add(Temp);
            }

            int Counter = 0;
            foreach (Instance.Post p in instance.Posts)
            {
                // create edges in the switching graph
                if (p.Matchings.Count > 0)
                {
                    Instance.Applicant Partner = instance.Applicants[p.Matchings[0]];
                    Node Start;
                    Node End;
                    if (instance.Posts[Partner.Priorities[0].Target].ID == p.ID && Partner.Priorities[0].Rank == 0)
                    {
                        Start = Nodes[Counter];
                        Start.IsS = false;
                        End = Nodes[Partner.Priorities[1].Target];
                        End.IsS = true;
                    }
                    else
                    {
                        Start = Nodes[Counter];
                        Start.IsS = true;
                        End = Nodes[Partner.Priorities[0].Target];
                        End.IsS = false;
                    }
                    Nodes[Counter].Outgoing.Add(new Edge(Start, End, Partner));
                }
                Counter++;
            }

            IdentifiyComponents();
        }


        /// <summary>
        /// DFS for determining component types
        /// </summary>
        private void IdentifiyComponents()
        {
            Stack<Node> Stack = new Stack<Node>();

            Components = new List<Component>();

            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Component == -1)
                {
                    Stack.Push(Nodes[i]);
                    while (Stack.Count > 0)
                    {
                        Node Top = Stack.Pop();
                        if (Top.Component == -1)
                        {
                            if (Top.Prev == null)
                            {
                                Component Temp = new Component();
                                Temp.Nodes.Add(Top);
                                if (Top.IsS)
                                    Temp.S++;
                                Components.Add(Temp);
                                Top.Component = Temp.ID;


                            }
                            else
                            {
                                Components[Top.Prev.Component].Nodes.Add(Top);
                                if (Top.IsS)
                                    Components[Top.Prev.Component].S++;
                                Top.Component = Top.Prev.Component;

                            }

                            if (Top.Outgoing.Count == 0)
                                Components[Top.Component].Sink = Top;

                            foreach (Edge e in Top.Outgoing)
                            {
                                e.End.Prev = Top;

                                if (e.End.Component == -1) // path continues
                                    Stack.Push(e.End);

                                if (e.End.Component == Top.Component) // cycle found
                                {
                                    e.End.Prev = Top;
                                    Components[Top.Component].IsCycle = true;

                                    Node Current = Top;

                                    do
                                    {
                                        Components[Top.Component].Cycle.Add(Current);
                                        Current = Current.Prev;
                                    } while (Current.Post.ID != Top.Post.ID);
                                }

                                if (e.End.Component != Top.Component && e.End.Component != -1) // two components have to be melted
                                {
                                    int OldComponent = e.End.Component;
                                    e.End.Prev = Top;
                                    foreach (Node n in Components[OldComponent].Nodes)
                                    {
                                        n.Component = Top.Component;
                                        Components[Top.Component].Nodes.Add(n);
                                    }
                                    Components[Top.Component].S += Components[OldComponent].S;
                                    if (Components[OldComponent].IsCycle)
                                        Components[Top.Component].IsCycle = true;

                                    Components[Top.Component].Cycle = Components[OldComponent].Cycle;

                                    Components[OldComponent] = null;
                                    foreach (Node n in Stack)
                                    {
                                        if (n.Component == OldComponent)
                                            n.Component = Top.Component;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the switching graph in a bitmap.
        /// </summary>
        /// <returns></returns>
        public Bitmap Draw()
        {
            Bitmap Result = new Bitmap(1000, 1000);
            Graphics G = Graphics.FromImage(Result);
            Pen TreeP = new Pen(new SolidBrush(Color.Red));
            Pen CycleP = new Pen(new SolidBrush(Color.Blue));

            double Angle = 2 * Math.PI / Nodes.Count;
            double Radius = 400;

            for (int i = 0; i < Nodes.Count; i++)
            {
                double CurrentAngle = Angle * i;
                double X = 500 + Radius * Math.Cos(CurrentAngle);
                double Y = 500 + Radius * Math.Sin(CurrentAngle);

                Pen P;
                if (Components[Nodes[i].Component].IsCycle)
                    P = CycleP;
                else
                    P = TreeP;

                G.DrawRectangle(P, (int)X, (int)Y, 20, 20);

                Nodes[i].X = (int)X;
                Nodes[i].Y = (int)Y;

                G.DrawString(Nodes[i].Post.ID.ToString(), new Font("Arial", 16), new SolidBrush(Color.Black), (float)X, (float)Y);
            }


            System.Drawing.Drawing2D.AdjustableArrowCap bigArrow = new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);


            for (int i = 0; i < Nodes.Count; i++)
            {
                Pen P;
                if (Components[Nodes[i].Component].IsCycle)
                    P = CycleP;
                else
                    P = TreeP;
                P.CustomEndCap = bigArrow;

                foreach (Edge e in Nodes[i].Outgoing)
                {
                    G.DrawLine(P, e.Start.X, e.Start.Y, e.End.X, e.End.Y);

                    G.DrawString(e.Applicant.ID.ToString(), new Font("Arial", 16), new SolidBrush(Color.Black), (float)(e.Start.X + e.End.X) / 2, (float)(e.Start.Y + e.End.Y) / 2);
                }
            }
            return Result;
        }

        /// <summary>
        /// Counts the number of possible popular matchings.
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            int l = 0;
            int PS = 1;
            foreach (Component comp in Components)
            {
                if (comp == null)
                    continue;
                if (comp.IsCycle)
                    l++;
                else
                    PS *= comp.S;
            }
            return (int)(Math.Pow(2, l) * PS);
        }

        /// <summary>
        /// Finds all paths from all s-posts to the sink post in a tree component.
        /// </summary>
        private void GetSPaths()
        {
            foreach (Component c in Components)
            {
                if (c == null)
                    continue;
                if (!c.IsCycle)
                {
                    foreach (Node n in c.Nodes)
                    {
                        if (n.IsS)
                        {
                            List<Node> Temp = new List<Node>();
                            Temp.Add(n);
                            Node Current = n;
                            while (Current != c.Sink && Current.Outgoing.Count > 0)
                            {
                                Current = Current.Outgoing[0].End;
                                Temp.Add(Current);
                            }
                            if (Temp.Count > 1)
                                c.Paths.Add(Temp);
                        }
                    }
                }
            }
        }

        private List<int> Copy(List<int> dummy)
        {
            List<int> Result = new List<int>();
            foreach (int i in dummy)
                Result.Add(i);
            return Result;
        }

        List<List<int>> Vectors;
        public List<List<int>> CreateVectors()
        {
            Vectors = new List<List<int>>();
            RecCreateVector(new List<int>(), 0);
            return Vectors;
        }

        /// <summary>
        /// Recursively creates all possible vectors representing applications of switching cycles and paths
        /// </summary>
        /// <param name="current"></param>
        /// <param name="index"></param>
        private void RecCreateVector(List<int> current, int index)
        {
            int Max = 0;
            if (Components[index] == null)
            {
                Max = 0;
            }
            else
            {
                if (Components[index].IsCycle)
                    Max = 1;
                else
                    Max = Components[index].S - 1;
            }

            for (int i = 0; i <= Max; i++)
            {
                List<int> NewCurrent = Copy(current);
                NewCurrent.Add(i);

                if (index == Components.Count - 1)
                {
                    Vectors.Add(NewCurrent);
                }
                else
                    RecCreateVector(NewCurrent, index + 1);
            }
        }

        /// <summary>
        /// Converts the given path of Nodes into a path of corresponding integer IDs
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<int> GetPath(List<Node> path)
        {
            List<int> Result = new List<int>();
            if (path != null && path.Count > 0)
            {
                Node n = path[0];
                do
                {
                    if (n.Outgoing.Count > 0)
                    {
                        Result.Add(n.Outgoing[0].Applicant.ID);
                        Result.Add(n.Outgoing[0].End.Post.ID);
                        n = n.Outgoing[0].End;
                    }
                } while (n != path[0]);
            }
            return Result;
        }

        /// <summary>
        /// Enumerates all possible popular matchings.
        /// </summary>
        /// <param name="path"></param>
        public void Enumerate(string path)
        {
            GetSPaths();
            List<List<int>> PossibleVectors = CreateVectors();

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            for (int i = 0; i < PossibleVectors.Count; i++)
            {
                Instance NewInstance = BaseInstance.CopyWithMatching();
                for (int j = 0; j < PossibleVectors[i].Count; j++)
                {
                    List<Node> NodePath = null;
                    if (Components[j] != null)
                        NodePath = Components[j].GetPath(PossibleVectors[i][j]);
                    List<int> IntPath = GetPath(NodePath);
                    NewInstance.MatchPath(IntPath, true);
                    NewInstance.Draw().Save(path + i.ToString() + ".jpg");
                }
            }
        }
    }

    public class LPSolver : Solver
    {
        public class LPEdge
        {
            public Instance.Applicant Applicant;
            public Instance.Post Post;
            public int Rank;

            public LPEdge(Instance.Applicant app, Instance.Post post, int rank)
            {
                Applicant = app;
                Post = post;
                Rank = rank;
            }
        }

        public string LP;

        public Instance Match(Instance instance, string path, bool print)
        {
            try
            {
                LP = "";
                if (!System.IO.Directory.Exists(path) && print)
                    System.IO.Directory.CreateDirectory(path);

                GRBEnv env = new GRBEnv("mip1.log");
                GRBModel model = new GRBModel(env);

                List<LPEdge> LPEdges = new List<LPEdge>();


                if (print)
                {
                    instance.Draw().Save(path + "/0Start.bmp");
                }

                int EdgeCounter = 0;
                foreach (Instance.Applicant a in instance.Applicants)
                {
                    EdgeCounter += a.Priorities.Count;
                    foreach (Instance.Priority Prio in a.Priorities)
                    {
                        {
                            LPEdges.Add(new LPEdge(a, instance.Posts[Prio.Target], Prio.Rank));
                            if (Prio.Rank == 0)
                                instance.Posts[Prio.Target].IsF = 1;
                        }
                    }

                }
                // Create variables

                GRBVar[] Edges = new GRBVar[EdgeCounter];

                for (int i = 0; i < Edges.Length; i++)
                {
                    Edges[i] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "ve" + i.ToString());
                }

                // Integrate new variables

                model.Update();

                if (print)
                    LP += "Applicant Matching Conditions:" + Environment.NewLine;

                foreach (Instance.Applicant a in instance.Applicants)
                {
                    GRBLinExpr Temp = new GRBLinExpr();
                    for (int i = 0; i < LPEdges.Count; i++)
                    {
                        if (LPEdges[i].Applicant == a)
                        {
                            Temp += Edges[i];
                            if (print)
                                LP += "(a" + LPEdges[i].Applicant.ID + ", p" + LPEdges[i].Post.ID + ") + ";
                        }
                    }
                    model.AddConstr(Temp == 1.0, "a" + a.ID.ToString());
                    if (print)
                        LP += " = 1;" + Environment.NewLine;
                }

                if (print)
                    LP += Environment.NewLine + "Post Matching Conditions:" + Environment.NewLine;
                
                foreach (Instance.Post p in instance.Posts)
                {
                    GRBLinExpr Temp = new GRBLinExpr();
                    for (int i = 0; i < LPEdges.Count; i++)
                    {
                        if (LPEdges[i].Post == p)
                        {
                            Temp += Edges[i];
                            if (print)
                                LP += "(a" + LPEdges[i].Applicant.ID + ", p" + LPEdges[i].Post.ID + ") + ";
                        }
                    }
                    model.AddConstr(Temp <= 1.0, "p" + p.ID.ToString());
                    if (print)
                        LP += " <= 1;" + Environment.NewLine;
                }

                if (print)
                    LP += Environment.NewLine + "First Choice Conditions:" + Environment.NewLine;

                for (int i = 0; i < LPEdges.Count; i++)
                {
                    LPEdge le1 = LPEdges[i];

                    if (le1.Post.IsF == 1 && le1.Rank != 0)
                    {
                        model.AddConstr(Edges[i] <= 0, "s" + i.ToString());
                        if (print)
                            LP += "(a" + LPEdges[i].Applicant.ID + ", p" + LPEdges[i].Post.ID + ") <= 0;" + Environment.NewLine;

                        for (int j = 0; j < LPEdges[i].Applicant.Priorities.Count; j++)
                        {
                            if (LPEdges[i].Applicant.Priorities[j].Target == LPEdges[i].Post.ID && LPEdges[i].Rank == LPEdges[i].Applicant.Priorities[j].Rank)
                            {
                                LPEdges[i].Applicant.Priorities.RemoveAt(j);
                            }
                        }
                    }
                }

                if (print)
                    LP += Environment.NewLine + "Second Choice Conditions:" + Environment.NewLine;

                for (int i = 0; i < LPEdges.Count; i++)
                {
                    LPEdge le1 = LPEdges[i];

                    foreach (LPEdge le2 in LPEdges)
                    {
                        if (le2 != le1 && le2.Post.IsF == 0 && le1.Applicant == le2.Applicant && le2.Rank != 0 && le2.Rank < le1.Rank)
                        {
                            model.AddConstr(Edges[i] <= 0, "s" + i.ToString());
                            if (print)
                                LP += "(a" + LPEdges[i].Applicant.ID + ", p" + LPEdges[i].Post.ID + ") <= 0;" + Environment.NewLine;
                            for (int j = 0; j < LPEdges[i].Applicant.Priorities.Count; j++)
                            {
                                if (LPEdges[i].Applicant.Priorities[j].Target == LPEdges[i].Post.ID && LPEdges[i].Rank == LPEdges[i].Applicant.Priorities[j].Rank)
                                {
                                    LPEdges[i].Applicant.Priorities.RemoveAt(j);
                                }
                            }
                            break;
                        }
                    }
                }

                if (print)
                    LP += Environment.NewLine + "First Post Conditions:" + Environment.NewLine;

                foreach (Instance.Post p in instance.Posts)
                {
                    if (p.IsF == 1)
                    {
                        GRBLinExpr Temp = new GRBLinExpr();
                        for (int i = 0; i < LPEdges.Count; i++)
                        {
                            if (LPEdges[i].Post == p)
                            {
                                Temp += Edges[i];
                                if (print)
                                    LP += "(a" + LPEdges[i].Applicant.ID + ", p" + LPEdges[i].Post.ID + ") + ";
                            }
                        }
                        model.AddConstr(Temp >= 1.0, "f" + p.ID.ToString());
                        if (print)
                            LP += ">= 1;" + Environment.NewLine;
                    }
                }


                // Optimize model

                model.Optimize();

                if (print)
                {
                    instance.Draw().Save(path + "/1Reduced.bmp");
                }

                for (int i = 0; i < Edges.Length; i++)
                {
                    if (Edges[i].Get(GRB.DoubleAttr.X) == 1)
                    {
                        instance.AddMatch(LPEdges[i].Post.ID, LPEdges[i].Applicant.ID);
                    }
                }

                if (print)
                {
                    instance.Draw().Save(path + "/2Matched.bmp");
                }

                // Dispose of model and env

                model.Dispose();
                env.Dispose();

                return instance;

            }
            catch (GRBException e)
            {
                Console.WriteLine("Error code: " + e.ErrorCode + ". " + e.Message);
                return null;
            }
        }
    }

    public interface Solver
    {
        Instance Match(Instance instance, string path, bool print);
    }
}
