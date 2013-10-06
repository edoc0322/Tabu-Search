using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace TS_TsaiHung_Express
{
    class TS
    {
        /**Initial*/
        public static double[][] distance;//距離陣列
        public double GbestFitness;//紀錄最佳適應值
        private int[] Gbest;  //紀錄最佳旅程
        private int candidate;//候選解數量
        private int tabuListLength;//禁忌清單長度
        private int Dimension;//問題的維度(TSP 節點 數)
        public int NowEvalution;//疊代次數上限
        private int LimitFit = 3, Limit = 2;//當經過一段時間值沒有改變則透過當前解重新產生初始解
        private int newInit = 0, newInitFit=0;//Limit的count次數
        private Random R = new Random(Guid.NewGuid().GetHashCode());

        private List<int> InitTemp = new List<int>(); //使用貪婪法產生初始解時 使用的變數
        private List<int> NeighborTabuList = new List<int>();//紀錄當次產生的候選解，不重複找到相同的組合 (隨機兩點的組合)
        private List<int> tabuList = new List<int>();//紀錄疊代中 以選過的隨機兩點組合  (大小為 禁忌清單長度*2  因為一次兩個值)
        private List<int> population = new List<int>();//產生旅程暫存
        private List<int> TempTour = new List<int>();//產生旅程暫存
        private int[] currentTour;//目前解
        private double currentFitness;//目前解的適合度

        private int[,] candidateList;//紀錄當候選解的每個候選解 交換的兩點
        private int[,] sortCandidateList;//排序候選清單
        private int[,] candidateTour;//該候選解的旅程
        private int[,] sortCandidateTour;//排序後的旅程
        private double[] candidateFitness;//該候選解的適合度
        private double[] sortCandidateFitness;//排序後的適合度
     
        private int[] tempInitTour;//暫存的旅程陣列
        /****************************************************************************************************/
        /*Initial Method*/
        public void Init(int num, int dimension, int lowerbound, int upperbound, int TabuListSize)
        {
            //Initial every Global Variable;
            NowEvalution = 0;
            candidate = num;
            Dimension = dimension;
            tabuListLength = TabuListSize;
            GbestFitness = double.MaxValue;
            int[] tempSolution = new int[dimension];
            currentFitness = double.MaxValue;
            currentTour = new int[dimension];
            candidateList = new int[candidate, 2];
            sortCandidateList = new int[candidate, 2];

            candidateTour = new int[candidate,dimension];
            sortCandidateTour = new int[candidate, dimension];

            candidateFitness = new double[candidate];
            sortCandidateFitness = new double[candidate];
            Gbest = new int[dimension];
            tabuList.Clear();//Tabu清單清空

            //GreedyInitial();//貪婪產生初始解
            InsertInitial();//插入法產生初始解   // 兩者差不多
            //RandomInitial();//隨機產生初始解   // 還沒寫 
            //evalution();
        }
        private void RandomInitial()
        { 
        //
        }      //隨機產生初始解
        private void GreedyInitial()
        { 
            for (int initC = 0; initC < candidate; initC++)
            {
                tempInitTour = new int[Dimension];
                double temp_fitness;
                InitTemp.Clear();
                for (int i = 0; i < Dimension; i++)
                { InitTemp.Add(i); }
                tempInitTour[0] = R.Next(Dimension);
                int now = tempInitTour[0];
                int chose = 0;
                for (int i = 0; i < Dimension - 1; i++)
                {
                    UpdateAllowNode(i);
                    double Max = double.MaxValue;
                    for (int j = 0; j < InitTemp.Count; j++)
                    {
                        int k = InitTemp[j];
                        if (Max > distance[now][k])
                        {
                            Max = distance[now][k];
                            chose = k;
                        }
                    }
                    tempInitTour[i + 1] = chose;
                    now = chose;
                }
                temp_fitness = Fitness(tempInitTour);
                if (temp_fitness < currentFitness)
                {
                    currentFitness = temp_fitness;
                    for (int i = 0; i < Dimension; i++)
                    { currentTour[i] = tempInitTour[i]; }
                }
            }
            evalution();//評估是否需要更新Gbest
        }      //根據各點最短距離產生初始解
        private void InsertInitial()
        {
            /*  step 1.隨機產生兩個城市作為起始路徑 設為 T={V0, V1}
             *  step 2.當 T=V則停止，使得dij+dji-dik 為最小 即為插入位置
             *  step 3.將點插入 Vi和Vi+1(Vk)之間 執行 step 2.
             * */
            for (int count = 0; count < 50; count++)//產生50個初始候選解 選出最好的作為初始解
            {
                population.Clear();//清空
                TempTour.Clear();//清空

                tempInitTour = new int[Dimension];//初始化暫存
                double temp_fitness;//初始化暫存Finess 
       
                for (int j = 0; j < Dimension; j++)
                { population.Add(j); }
                //隨機前兩個城市編號 // 準備進行TIS初始化工作  
                int Random_0, Random_1, temp0, temp1;
                do
                {
                    Random_0 = R.Next(0, Dimension);
                    Random_1 = R.Next(0, Dimension);
                } while (Random_0 == Random_1); //step 1 
                temp0 = population[Random_0];
                temp1 = population[Random_1];

                TempTour.Add(temp0);
                TempTour.Add(temp1);

                population.Remove(temp0);
                population.Remove(temp1);

                InitialofInsert();
                //InitialofTestInsert();
                temp_fitness = Fitness(tempInitTour);
                if (temp_fitness < currentFitness)
                {
                    currentFitness = temp_fitness;
                    for (int p = 0; p < Dimension; p++)
                    { 
                    currentTour[p] = tempInitTour[p];
                    }
                }
            }
            evalution();//評估是否需要更新Gbest
        }      //插入法產生初始解
        private void InitialofTestInsert()
        {
            /*
       * 根據目前母體旅程中有的點計算，接下來要插入的點的最佳位置 並插入
       * */
            int tempC = TempTour.Count;
            int Vi = 0, Vj = 0;
            for (int j = 0; j < tempC; j++)
            {
                double serchDistance = double.MaxValue; //宣告最小距離為最大值
                for (int i = 0; i < population.Count; i++)
                {
                    if (i == population.Count - 1)
                    {
                        if (distance[population[i]][TempTour[j]] + distance[TempTour[j]][population[0]] - distance[population[i]][population[0]] < serchDistance)
                        {
                            serchDistance = distance[population[i]][TempTour[j]] + distance[TempTour[j]][population[0]] - distance[population[i]][population[0]];
                            Vi = i + 1;
                            Vj = j;
                        }
                    }
                    else
                    {
                        if (distance[population[i]][TempTour[j]] + distance[TempTour[j]][population[i + 1]] - distance[population[i]][population[i + 1]] < serchDistance)
                        {
                            serchDistance = distance[population[i]][TempTour[j]] + distance[TempTour[j]][population[i + 1]] - distance[population[i]][population[i + 1]];
                            Vi = i + 1;
                            Vj = j;
                        }
                    }
                }
                population.Insert(Vi, TempTour[Vj]);    //將選到的點插入暫存清單
            }
            for (int i = 0; i < Dimension; i++)    //將暫存清單丟回目前解清單
            {
                tempInitTour[i] = population[i];
            }
        }//改良後的插入初始化法
        private void InitialofInsert()
        {
            /*  
             * 說明在InsertInitial();
             * */
            int Vi = 0,Vj=0;
            int tempDimension = population.Count;
            for (int c = 0; c < tempDimension; c++)
            {//需要插入的節點數量
                double serchDistance = double.MaxValue; //宣告最小距離為最大值
                for (int j = 0; j < population.Count; j++)
                {//population 遞增                 
                    for (int i = 0; i < TempTour.Count; i++)  //計算插入點與任兩點的距離
                    {//TempTour 遞減
                        if (i == TempTour.Count - 1)
                        {
                            if (distance[TempTour[i]][population[j]] + distance[population[j]][TempTour[0]] - distance[TempTour[i]][TempTour[0]] < serchDistance)
                            {//計算 dij +  dji - dik
                                serchDistance = distance[TempTour[i]][population[j]] + distance[population[j]][TempTour[0]] - distance[TempTour[i]][TempTour[0]];
                                Vi = i + 1;//紀錄插入點位置
                                Vj = j;//紀錄要插入的點位置
                            }
                        }
                        else
                        {
                            if (distance[TempTour[i]][population[j]] + distance[population[j]][TempTour[i + 1]] - distance[TempTour[i]][TempTour[i + 1]] < serchDistance)
                            {//計算 dij +  dji - dik
                                serchDistance = distance[TempTour[i]][population[j]] + distance[population[j]][TempTour[i + 1]] - distance[TempTour[i]][TempTour[i + 1]];
                                Vi = i + 1;//紀錄插入點位置
                                Vj = j;//紀錄要插入的點位置
                            }
                        }
                    }
                }
                TempTour.Insert(Vi, population[Vj]);    //將選到的點插入暫存清單
                population.RemoveAt(Vj); //將選到的點從目前解名單刪除
            }
            for (int i = 0; i < Dimension; i++)
            {//將暫存清單丟回目前解清單
                tempInitTour[i] = TempTour[i];
            }
        }    //插入初始化法 完整版(與paper相符)
        private void UpdateAllowNode(int city)
        {//city = 選到第幾個
            int k = 0;
            while (k < InitTemp.Count)
            {//將走過的城市 從arraylist中刪除
                if (tempInitTour[city] == InitTemp[k])
                {
                    InitTemp.RemoveAt(k);
                    break;
                }
                k++;
            }
        } //還沒選過的節點(用於貪婪搜尋初始解方法)
        /****************************************************************************************************/

        public void Run(int MaxFEC)
        {
            //for (int run = 0; run < (MaxFEC / candidate); run++)//這回圈有問題
            while (NowEvalution < MaxFEC)
            {
                NeighborTabuList.Clear();
                for (int can = 0; can < candidate; can++)
                {//每次產生 candidate 個候選解
                    for (int i = 0; i < Dimension; i++)
                        candidateTour[can, i] = currentTour[i];
                    InitialNeighborList(can);//初始化第 can 的鄰居清單
                    two_swap(can, candidateList[can, 0], candidateList[can, 1]);//兩點交換
                    GetCandidateFitness(can);//計算該候選解的適合度
                }
                sortCandidate();//排序候選解
                selectJudgment();//依序選擇
                // a= true , have a move solution ; newInit=0; 
                if (newInit == Limit)
                {//這邊似乎影響極度不大
                    Diversification();//多樣化策略(根據目前解重新產生 目前解)
                    newInit = 0;//歸零
                }
            }
            //Console.WriteLine(NowEvalution);
        } //運行
        private void two_swap(int can, int a, int b)
        {//兩點交換 // 從旅程中 找到 node a and node b 將兩個城市的順序對調
            int[] tempSolution = new int[Dimension];
            int ta = 0, tb = 0;
            //ta = a; tb = b;      
            for (int i = 0; i < Dimension; i++)
            {//找出該點在 候選旅程中的位置 並相互交換
                if (candidateTour[can, i] == a)
                { ta = i; }
                else if (candidateTour[can, i] == b)
                { tb = i; }
            }
            //swap
            int tempA = candidateTour[can, ta];
            candidateTour[can, ta] = candidateTour[can, tb];
            candidateTour[can, tb] = tempA;
        }  //兩點交換
        private void InitialNeighborList(int NumofCandidate)
        {//隨機產生兩點為該候選解準備交換的城市
         
            int randomPoint_a, randomPoint_b;
                 do
                {
                    randomPoint_a = R.Next(0, Dimension);
                    randomPoint_b = R.Next(0, Dimension);
                }while (randomPoint_a == randomPoint_b && checkNeighborList(randomPoint_a,randomPoint_b) );
                NeighborTabuList.Add(randomPoint_a);
                NeighborTabuList.Add(randomPoint_b);

                candidateList[NumofCandidate, 0] = randomPoint_a;
                candidateList[NumofCandidate, 1] = randomPoint_b;
        }//初始化各候選解欲交換的點
        private void sortCandidate()
        {//排序候選解
            for (int i = 0; i < candidate; i++)//複製
            { sortCandidateFitness[i] = candidateFitness[i]; }
            
            Array.Sort(sortCandidateFitness);//排序
            for (int i = 0; i < candidate; i++)//依據值丟入陣列中            
            {
                for (int j = 0; j < candidate; j++)
                    if (sortCandidateFitness[i] == candidateFitness[j])
                    {
                        for (int k = 0; k < Dimension; k++)
                        { sortCandidateTour[i, k] = candidateTour[j, k]; }
                        for (int k = 0; k < 2; k++)
                        { sortCandidateList[i, k] = candidateList[j, k]; }
                    }
            }
        }//排序候選解
        /****************************************************************************************************/
        /* Diversification Method*/       
        private void Diversification()
        {//多樣化   -> 分散策略
            int[,] tempCandidate = new int[candidate, Dimension];//暫存候選解
            double[] tempCandidateFitness = new double[candidate];//暫存適合度
            for (int k = 0; k < candidate; k++)//產生50個新解 取最好的丟回
            {
                    population.Clear();
                    TempTour.Clear();

                    tempInitTour = new int[Dimension];

                    for (int i = 0; i < Dimension; i++)
                    { population.Add(currentTour[i]); }

                    for (int i = 0; i < Dimension * 0.7; i++)
                    {//取出Dimension /2 個  目前解中的
                        int ran = R.Next(population.Count);//隨機一個起始點 
                        int inn = population[ran];
                        TempTour.Add(population[ran]);//bcd
                        population.Remove(population[ran]);//population
                    }

                    InsertofRenewTour();//使用插入法

                    tempCandidateFitness[k] = Fitness(tempInitTour);//適合度計算
                    for (int i = 0; i < Dimension; i++)//存入值
                    { tempCandidate[k, i] = tempInitTour[i]; }
                }
            for (int i = 0; i < candidate; i++)
            {//找出最好的丟回目前解 並比較GbestFiness
                if (tempCandidateFitness[i] < currentFitness)
                {
                    currentFitness = tempCandidateFitness[i];
                    for (int j = 0; j < Dimension; j++)
                    {
                        currentTour[j] = tempCandidate[i, j];
                    }
                }
            }
        }    //多樣化策略 用於當目前解無法變更最佳解太長時間(i.e. 運算停滯)
        private void InsertofRenewTour()
        {//類似初始化時用的插入法，
            /*
             * 根據目前母體旅程中有的點計算，接下來要插入的點的最佳位置 並插入
             * */
            int tempC = TempTour.Count;
            int Vi=0, Vj=0;
            for (int j = 0; j < tempC; j++)
            {
                double serchDistance = double.MaxValue; //宣告最小距離為最大值
                for (int i = 0; i < population.Count; i++)
                {                        
                        if (i == population.Count - 1)
                        {
                            if (distance[population[i]][TempTour[j]] + distance[TempTour[j]][population[0]] - distance[population[i]][population[0]] < serchDistance)
                            {
                                serchDistance = distance[population[i]][TempTour[j]] + distance[TempTour[j]][population[0]] - distance[population[i]][population[0]];
                                Vi = i + 1;
                                Vj = j;
                            }
                        }
                        else
                        {
                            if (distance[population[i]][TempTour[j]] + distance[TempTour[j]][population[i + 1]] - distance[population[i]][population[i + 1]] < serchDistance)
                            {
                                serchDistance = distance[population[i]][TempTour[j]] + distance[TempTour[j]][population[i + 1]] - distance[population[i]][population[i + 1]];
                                Vi = i + 1;
                                Vj = j;
                            }
                        }               
                }
                population.Insert(Vi, TempTour[Vj]);    //將選到的點插入暫存清單
            }
            for (int i = 0; i < Dimension; i++)    //將暫存清單丟回目前解清單
            {
                tempInitTour[i] = population[i];
            }
        }  //多樣化根據目前解重新產生解
        /****************************************************************************************************/

        private void selectJudgment()
        {//從候選解中 選出旅程取代目前旅程
            bool a = false;
            for (int can = 0; can < candidate; can++)
            {
                if (checkTabuList(sortCandidateList[can, 0], sortCandidateList[can, 1]) == false)//not tabu
                {//如果不再禁忌清單內直接選取
                    for (int i = 0; i < Dimension; i++)
                    { currentTour[i] = sortCandidateTour[can, i]; }

                    currentFitness = sortCandidateFitness[can];
                    a = true;
                    evalution();
                    UpdateTabuList(sortCandidateList[can, 0], sortCandidateList[can, 1]);
                    break;
                }
                else if (sortCandidateFitness[can] < GbestFitness)
                {//如果在禁忌清單內 ，且小於GbestFitness  符合免禁準則 直接選取
                    for (int i = 0; i < Dimension; i++)
                    { currentTour[i] = sortCandidateTour[can, i]; }
                    currentFitness = sortCandidateFitness[can];
                    evalution();
                    a = true;
                    UpdateTabuList(sortCandidateList[can, 0], sortCandidateList[can, 1]);
                    break;
                }
                else
                {//如果在禁忌清單內 ，但沒有比GbestFitness好的話
                    double Aspiration = R.NextDouble();
                    if (Aspiration <= 0.3)//如果符合勇氣程度則依然選該條旅程
                    {
                        if (sortCandidateFitness[can] < currentFitness)
                        {
                            for (int i = 0; i < Dimension; i++)
                                currentTour[i] = sortCandidateTour[can, i];
                            currentFitness = sortCandidateFitness[can];
                            evalution();
                            a = true;
                            UpdateTabuList(sortCandidateList[can, 0], sortCandidateList[can, 1]);
                            break;
                        }
                    }
                    //else
                    //{//如果都沒有選到 則 找不到解的計數器++  
                    //    newInit++; 
                    //}             
                }
            }
            if (a == false)
                newInit++;
        }     //選擇候選解
        private void Intensification()
        {
            tabuList.Clear();
        }    //集中化策略
        /****************************************************************************************************/
        /* Tabu Method*/       
        private void UpdateTabuList(int a, int b)
        {//更新禁忌清單
            if (tabuListLength * 2 == tabuList.Count)
            {
                tabuList.RemoveAt(0);//刪除前兩個值
                tabuList.RemoveAt(0);//刪除前兩個值
                if (checkTabuList(a, b) == false)
                {//當兩點不存在於禁忌清單內(重複禁忌)  直接加入值
                    tabuList.Add(a);
                    tabuList.Add(b);
                }
                else
                {//否則 在禁忌清單內找出重複的值刪除，並加入值
                    for (int i = 0; i < tabuList.Count; i += 2)
                    {
                        if (tabuList[i] == a && tabuList[i + 1] == b || tabuList[i] == b && tabuList[i + 1] == a)
                        {
                            tabuList.RemoveAt(i);
                            tabuList.RemoveAt(i);
                            break;
                        }
                    }
                    tabuList.Add(a);
                    tabuList.Add(b);
                }
            }
            else
            {//清單尚未填滿 直接加入
                tabuList.Add(a);
                tabuList.Add(b);
            }
        }//更新禁忌清單       
        private bool checkTabuList(int a, int b)
        {//查看丟入的node a and node b wether on TabuList 
            bool tabuOrNot = false;
            for (int c = 0; c < tabuList.Count; c +=2)
            {
                //Console.WriteLine("a="+a+"  b="+b );
                if ((a == tabuList[c] && b == tabuList[c+1]) || (a == tabuList[c+1] && b == tabuList[c]))
                {
                    tabuOrNot = true;
                    break;
                }
            }
            return tabuOrNot;
        } //查看是否在禁忌清單內(僅查看)
        private bool checkNeighborList(int a, int b)
        {//判斷是否重複選到相同的組合?
            bool NeighborOrNot = false;
            for (int i = 0; i < NeighborTabuList.Count; i += 2)
            {
                if ((a == NeighborTabuList[i] && b == NeighborTabuList[i + 1]) || (a == NeighborTabuList[i + 1] && b == NeighborTabuList[i]))
                {
                    NeighborOrNot = true;
                    break;
                }
            }
            return NeighborOrNot;
        }

        /****************************************************************************************************/
        /* Evalution Method*/
        private void evalution()
        {
            if (currentFitness < GbestFitness)
            {
                GbestFitness = currentFitness;
                for (int i = 0; i < Dimension; i++)
                {
                    Gbest[i] = currentTour[i];
                }
                newInitFit = 0;
                Intensification();//集中化策略
            }
            else
            {
                newInitFit++;
            }

            if (newInitFit >= LimitFit)//GBESTFITNESS沒被改變過 所以重新產生初始解
            {
                Diversification();//清空塔布、重新產生初始解
                newInitFit = 0;
                evalution();
            }
        }
        private void GetCandidateFitness(int CandidateNumber)
        {
            int[] temp = new int[Dimension];
            for (int i = 0; i < Dimension; i++)
            { temp[i] = candidateTour[CandidateNumber, i]; }
            candidateFitness[CandidateNumber] = Fitness(temp);
        }       
        public virtual double Fitness(int[] f)
        {
            return -1;
        }
        /****************************************************************************************************/
    }
}
