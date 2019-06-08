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
    public class MichalewiczFunction : GH_Component {
        // 引数
        private double[] xValues = new double[5];

        // サブカテゴリ内の配置
        public override GH_Exposure Exposure {
            get {
                return GH_Exposure.primary;
            }
        }
        // コンポーネント名
        public MichalewiczFunction()
            : base("MichalewiczFunction",
                   "Michalewicz",
                   "benchmark function",
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
            pManager.AddNumberParameter("X1", "X1", "parameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("X2", "X2", "parameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("X3", "X3", "parameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("X4", "X4", "parameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("X5", "X5", "parameter", GH_ParamAccess.item);
        }
        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddNumberParameter("Z", "Z", "Output Z", GH_ParamAccess.item);
        }
        /// <summary>
        /// 計算部分
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 入力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetData(0, ref xValues[0])) { return; }
            if (!DA.GetData(1, ref xValues[1])) { return; }
            if (!DA.GetData(2, ref xValues[2])) { return; }
            if (!DA.GetData(3, ref xValues[3])) { return; }
            if (!DA.GetData(4, ref xValues[4])) { return; }

            double z = Michalewicz(xValues);

            // 評価関数のMichalewiczの作成
            double Michalewicz(double[] xValues) {
                double result = 0.0;
                for (int i = 0; i < xValues.Length; ++i) {
                    double a = Math.Sin(xValues[i]);
                    double b = Math.Sin(((i + 1) * xValues[i] * xValues[i]) / Math.PI);
                    double c = Math.Pow(b, 20);
                    result += a * c;
                }
                return -1.0 * result;
            }

            // grassshopper へのデータ出力　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetData(0, z);
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
                return new Guid("621eac03-23fb-445c-9430-44ce37bf9024");
            }
        }
    }
}
