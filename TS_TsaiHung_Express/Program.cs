using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace TS_TsaiHung_Express
{
    class Program
    {
        static void Main(string[] args)
        {
            // runLength = 一個問題做的次數
            // MaxFuntionEvaluation = 適合度最大評估次數
            // num = 粒子數 //候選解數
            // TabuListSize = 禁忌名單的長度

            int runLength = 5, MaxFuntionEvaluation = 40000, num = 50, TabuListSize = 12;
            TSP tabutsp = new TSP();

            //將資料寫入到EXCEL上
            StreamWriter sw = new StreamWriter("Tabu_Self-1025.csv");

            // Function name ; 最好的值 (越小越好) ; 平均runLength次的值 ; 標準差 ; 平均時間
            sw.WriteLine("Case , Best Value , Average , STD ,Average time");

            //碼表
            System.Diagnostics.Stopwatch swt = new System.Diagnostics.Stopwatch();

            double[] best = new double[runLength];

            for (int i = 0; i < tabutsp.problem.Length; i++)
            {
                //Console.WriteLine("目前是第" + i + "個問題");
                double TSbest = double.MaxValue;

                double bestsum = 0.0;
                double average_best, average_time;

                for (int k = 0; k < runLength; k++)
                {
                    // Console.WriteLine("k = " + k);
                    swt.Reset();
                    swt.Start();

                    //problem init
                    tabutsp.initDistance(i);

                    //方法初始化
                    tabutsp.Init(num, tabutsp.distanceLength, 0, tabutsp.distanceLength - 1, TabuListSize);
                    //讀入 (粒子數 ; 維度數 ; 下限 ; 上限 ; 禁忌名單長度)

                    tabutsp.Run(MaxFuntionEvaluation);

                    best[k] = tabutsp.GbestFitness;
                    bestsum += best[k];

                    if (best[k] < TSbest)
                    {
                        TSbest = best[k];
                    }
                }

                swt.Stop();
                //碼錶出來的時間是毫秒，要轉成秒，除以1000。
                double time = (double)swt.Elapsed.TotalMilliseconds / 1000;

                average_best = bestsum / runLength;
                average_time = time / runLength;

                Console.WriteLine(tabutsp.problem[i] + ", " + TSbest + "," + average_best + "," + std(best) + "," + average_time);
                sw.WriteLine(tabutsp.problem[i] + ", " + TSbest + "," + average_best + "," + std(best) + "," + average_time);
            }
            Console.Read();
            sw.Close();
        }

        public static double std(double[] fit)
        {
            double sum = 0.0, average;
            for (int i = 0; i < fit.Length; i++)
                sum += fit[i];
            average = sum / fit.Length;
            sum = 0.0;
            for (int i = 0; i < fit.Length; i++)
                sum += (Math.Pow(fit[i] - average, 2));
            return Math.Pow(sum / fit.Length, 0.5);

        }

    }

    class TSP : TS
    {

        public string[] problem = new string[] { "Bays29", "Berlin52", "Eil51", "Eil76", "Pr76", "St70", "Oliver30" };
        public int distanceLength;

        public void initDistance(int i)
        {
            if (i == 0)
            {
                distance = new double[29][];
                for (int k = 0; k < distance.Length; k++)
                    distance[k] = new double[29];
                readStreetDistance("TSP\\"+problem[i] + ".txt");
            }
            else
                readGeographicalDistance("TSP\\" + problem[i] + ".txt");
            distanceLength = distance[0].Length;
        }
        public static void readGeographicalDistance(string file)
        {
            StreamReader str = new StreamReader(file);
            string all = str.ReadToEnd();
            string[] c = all.Split(new char[] { ' ', '\n' });
            distance = new double[int.Parse(c[0])][];
            for (int i = 0; i < int.Parse(c[0]); i++)
                distance[i] = new double[int.Parse(c[0])];
            for (int i = 0; i < int.Parse(c[0]); i++)
            {
                for (int j = 0; j < int.Parse(c[0]); j++)
                {
                    int t1 = 3 * i + 1, t2 = 3 * j + 1;
                    double x1 = Double.Parse(c[t1 + 1]), y1 = Double.Parse(c[t1 + 2]);
                    double x2 = Double.Parse(c[t2 + 1]), y2 = Double.Parse(c[t2 + 2]);
                    distance[i][j] = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
                }
            }
        }

        public static void readStreetDistance(string file)
        {
            StreamReader str = new StreamReader(file);
            string all = str.ReadToEnd();
            string[] c = all.Split(new char[] { ' ', '\n' });
            int index = 0;
            for (int i = 0; i < 29; i++)
            {
                for (int j = 0; j < 29; j++)
                {
                    for (int k = index; k < c.Length; k++)
                    {
                        if (c[k] != "")
                        {
                            index = k + 1;
                            break;
                        }
                    }
                    distance[i][j] = Double.Parse(c[index - 1]);
                }
            }
        }

        public override double Fitness(int[] solution)
        {
            double sum = 0;
            NowEvalution++;
            for (int j = 0; j < solution.Length - 1; j++)
                sum = sum + distance[solution[j]][solution[j + 1]];
            sum = sum + distance[solution[solution.Length - 1]][solution[0]];
            return sum;
        }
    }

}
