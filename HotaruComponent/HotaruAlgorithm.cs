/**
Copyright (c) 2019 hiron_rgkr

This software is released under the MIT License.
See LICENSE
**/

using System;
using System.Drawing;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI;
using Grasshopper.GUI.HTML;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Gradient;

using GH_IO.Serialization;

using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;

namespace HotaruComponent {
    public class HotaruAlgorithm : GH_Component {
        // input
        private int numFireflies, dim, maxEpochs, seed;

        // サブカテゴリ内の配置
        public override GH_Exposure Exposure {
            get {
                return GH_Exposure.primary;
            }
        }
        // コンポーネント名
        public HotaruAlgorithm()
            : base("HotaruAlgorithm",
                   "Hotaru",
                   "Firefly algorithm optimization compnent\n Goal is to solve the Michalewicz benchmark function. The function has a known minimum value of -4.687658. x = 2.2029 1.5707 1.2850 1.9231 1.7205",
                   "Hotaru",
                   "Hotaru") {
        }
        // データのクリア
        public override void ClearData() {
            base.ClearData();
        }
        // ジオメトリなどを出力しなくてもPreviewを有効にする。
        public override bool IsPreviewCapable {
            get {
                return true;
            }
        }
        /// <summary>
        /// インプットパラメータの登録
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddIntegerParameter("numFireflies", "N", "Input numFireflie", GH_ParamAccess.item, 40);// typically 15-40  ホタルの数
            pManager.AddIntegerParameter("dim", "dim", "dimention", GH_ParamAccess.item, 5);// 変数の数
            pManager.AddIntegerParameter("maxEpochs", "MaxE", "maxEpochs", GH_ParamAccess.item, 100); // 世代数
            pManager.AddIntegerParameter("seed", "seed", "seed", GH_ParamAccess.item, 0);// 乱数のシード値
        }
        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddNumberParameter("X1", "X1", "Output X1", GH_ParamAccess.item);
            pManager.AddNumberParameter("X2", "X2", "Output X2", GH_ParamAccess.item);
            pManager.AddNumberParameter("X3", "X3", "Output X3", GH_ParamAccess.item);
            pManager.AddNumberParameter("X4", "X4", "Output X4", GH_ParamAccess.item);
            pManager.AddNumberParameter("X5", "X5", "Output X5", GH_ParamAccess.item);
            pManager.AddNumberParameter("Z", "Z", "Output Z", GH_ParamAccess.item);
            pManager.AddNumberParameter("Error", "error", "Output error", GH_ParamAccess.item);
        }
        /// <summary>
        /// 計算部分
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 入力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetData(0, ref numFireflies)) { return; }
            if (!DA.GetData(1, ref dim)) { return; }
            if (!DA.GetData(2, ref maxEpochs)) { return; }
            if (!DA.GetData(3, ref seed)) { return; }

            // FA を 呼び出し
            double[] bestPosition = Solve(numFireflies, dim, seed, maxEpochs);

            // Main メソッドは FA の結果を表示して終了
            double z = Michalewicz(bestPosition);
            double error = Error(bestPosition);

            // grassshopper へのデータ出力　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetData(0, bestPosition[0]);
            DA.SetData(1, bestPosition[1]);
            DA.SetData(2, bestPosition[2]);
            DA.SetData(3, bestPosition[3]);
            DA.SetData(4, bestPosition[4]);
            DA.SetData(5, z);
            DA.SetData(6, error);
        }

        // ここがsolverのメイン
        static double[] Solve(int numFireflies, int dim, int seed, int maxEpochs) {
            Random rnd = new Random(seed);
            double minX = 0.0; // Michalewicz 関数 固有の値
            double maxX = 3.2; // 通常は -10.0 ~ +10.0

            // あるホタルが別のホタルを引き寄せる力の制御値
            double B0 = 1.0;   // 引用している  
            double g = 1.0;    // 研究論文の
            double a = 0.20;   // 推奨値

            int displayInterval = maxEpochs / 10;

            // 空のホタルの群れを作成
            double bestError = double.MaxValue;
            double[] bestPosition = new double[dim]; // best ever

            Firefly[] swarm = new Firefly[numFireflies]; // all null

            // ホタルの群れ(swarm)をランダムな位置で初期化
            for (int i = 0; i < numFireflies; ++i) {
                swarm[i] = new Firefly(dim); // 初期値として 0.0 で定義
                for (int k = 0; k < dim; ++k) // ランダムに位置を設定
                    swarm[i].position[k] = (maxX - minX) * rnd.NextDouble() + minX;
                swarm[i].error = Error(swarm[i].position); // 誤差の計算
                                                           // ホタルの明るさ(intensity)を 誤差の 逆数になるように定義
                swarm[i].intensity = 1 / (swarm[i].error + 1); // 0除算 を避けるため +1

                // 初期化の最後に 新しく作成したホタルを チェックして 最適な位置か確認
                if (swarm[i].error < bestError) {
                    bestError = swarm[i].error;
                    for (int k = 0; k < dim; ++k)
                        bestPosition[k] = swarm[i].position[k];
                }
            }

            // メイン処理のループ
            int epoch = 0;
            while (epoch < maxEpochs) // main processing
            {
                //if (bestError < errThresh) break; // are we good?
                if (epoch % displayInterval == 0 && epoch < maxEpochs) // show progress?
                {
                    string sEpoch = epoch.ToString().PadLeft(6);
                    Console.Write("epoch = " + sEpoch);
                    Console.WriteLine("   error = " + bestError.ToString("F14"));
                }

                // 各ホタルを入れ子にした for ループ を使用して別のホタルと比較
                for (int i = 0; i < numFireflies; ++i) // each firefly
                {
                    for (int j = 0; j < numFireflies; ++j) // each other firefly. weird!
                    {
                        if (swarm[i].intensity < swarm[j].intensity) {
                            // より明るいホタルへの 引き寄せる力の変数 beta の計算
                            double r = Distance(swarm[i].position, swarm[j].position);
                            double beta = B0 * Math.Exp(-g * r * r); // original 
                                                                     //double beta = (B0 - betaMin) * Math.Exp(-g * r * r) + betaMin; // better
                                                                     //double a = a0 * Math.Pow(0.98, epoch); // better

                            for (int k = 0; k < dim; ++k) {
                                swarm[i].position[k] += beta * (swarm[j].position[k] - swarm[i].position[k]); // beta分だけ移動
                                swarm[i].position[k] += a * (rnd.NextDouble() - 0.5); // 移動量に微小なランダム要素を混ぜる

                                // 各位置を確認して範囲外であれば 範囲に収まるよう ランダム値 を入れる
                                if (swarm[i].position[k] < minX)
                                    swarm[i].position[k] = (maxX - minX) * rnd.NextDouble() + minX;
                                if (swarm[i].position[k] > maxX)
                                    swarm[i].position[k] = (maxX - minX) * rnd.NextDouble() + minX;
                            }

                            // 入れ子移動したばかりのホタルの誤差と明るさを更新して終了
                            swarm[i].error = Error(swarm[i].position);
                            swarm[i].intensity = 1 / (swarm[i].error + 1);
                        }
                    } // j
                } // i each firefly

                // 一番誤差の小さなホタルが sworm[0] になるよう ソート
                Array.Sort(swarm); // low error to high
                if (swarm[0].error < bestError) // new best?
                {
                    bestError = swarm[0].error;
                    for (int k = 0; k < dim; ++k)
                        bestPosition[k] = swarm[0].position[k];
                }
                ++epoch;
            } // while
            return bestPosition;
        } // Solve
        // ホタル間のユークリッド空間での距離を２乗和平方で計算
        static double Distance(double[] posA, double[] posB) {
            double ssd = 0.0; // sum squared diffrences (Euclidean)
            for (int i = 0; i < posA.Length; ++i)
                ssd += (posA[i] - posB[i]) * (posA[i] - posB[i]);
            return Math.Sqrt(ssd);
        }
        // 評価関数のMichalewiczの作成
        static double Michalewicz(double[] xValues) {
            double result = 0.0;
            for (int i = 0; i < xValues.Length; ++i) {
                double a = Math.Sin(xValues[i]);
                double b = Math.Sin(((i + 1) * xValues[i] * xValues[i]) / Math.PI);
                double c = Math.Pow(b, 20);
                result += a * c;
            }
            return -1.0 * result;
        } // Michalewicz
        //誤差計算
        static double Error(double[] xValues) {
            int dim = xValues.Length;
            double trueMin = 0.0;
            if (dim == 2)
                trueMin = -1.8013; // approx.
            else if (dim == 5)
                trueMin = -4.687658; // approx.
            else if (dim == 10)
                trueMin = -9.66015; // approx.
            double calculated = Michalewicz(xValues);
            return (trueMin - calculated) * (trueMin - calculated);
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args) {
        }
        /// <summary>
        /// アイコンの設定。24x24 pixelsが推奨
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
              return null;
            }
        }
        /// <summary>
        /// GUIDの設定
        /// </summary>
        public override Guid ComponentGuid {
            get {
                return new Guid("621eac03-23fb-445c-9430-44ce37bf9023");
            }
        }
    }

    // Firefly クラスの定義
    public class Firefly : IComparable<Firefly> {
        public double[] position;
        public double error;
        public double intensity;

        public Firefly(int dim) {
            this.position = new double[dim];
            this.error = 0.0;
            this.intensity = 0.0;
        }

        // 誤差の小さいホタルから大きいホタルへと Firefly オブジェクトを並び替え
        public int CompareTo(Firefly other) {
            // allow auto sort low error to high
            if (this.error < other.error)
                return -1;
            else if (this.error > other.error)
                return +1;
            else
                return 0;
        }
    } // class Firefly
}

