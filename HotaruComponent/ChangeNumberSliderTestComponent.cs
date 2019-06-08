using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;

using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// Refer to https://discourse.mcneel.com/t/optimization-plug-in-for-grasshopper-how-to-use-galapagos-interface-and-gene-pool/27267/17

namespace GHOptimizationTest {
    public class ChangeNumberSliderTestComponent : GH_Component {
        GH_Document doc;
        IGH_Component Component;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ChangeNumberSliderTestComponent()
            : base("GHOptimizationTest", "Nickname",
                "Description",
                "Hotaru", "Hotaru") {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddIntegerParameter("f(x*)", "f(x*)", "optimum", GH_ParamAccess.item);
            pManager.AddIntegerParameter("x", "x", "decision variable", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            Component = this;
            doc = Component.OnPingDocument();

            //component input
            int fopt = new int();                   //expected optimum
            int x = new int();                      //decision variable
            if (!DA.GetData(0, ref fopt)) { return; }
            if (!DA.GetData(1, ref x)) { return; }

            //getting number slider for decision variable, which also updates objective function value once changed
            List<Grasshopper.Kernel.Special.GH_NumberSlider> sliders = new List<Grasshopper.Kernel.Special.GH_NumberSlider>();
            foreach (IGH_Param param in Component.Params.Input) {
                Grasshopper.Kernel.Special.GH_NumberSlider slider = param.Sources[0] as Grasshopper.Kernel.Special.GH_NumberSlider;
                if (slider != null)
                    sliders.Add(slider);
            }

            // エラーはここが参考になる？
            // https://www.grasshopper3d.com/forum/topics/changing-sliders-upstream-causes-an-object-expired-during-a
            // ghは解析中に値の変更を許してないので、上のようにSolveInstance内で直接値を変えようとすると stackoverflow するっぽいので
            // 以下のように遅らせるといいらしいけどうまくいかず
            doc.ScheduleSolution(5);


            sliders[1].SetSliderValue(fopt);

            EnsurePaintHandler();
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("{50084e0a-caa3-472e-8e9a-a680604444d2}"); }
        }
        private bool _paintHandlerAssigned = false;

        private void EnsurePaintHandler() {
            if (_paintHandlerAssigned)
                return;

            Grasshopper.Instances.ActiveCanvas.CanvasPrePaintWires += PrePaintWires;
            _paintHandlerAssigned = true;
        }

        private void PrePaintWires(Grasshopper.GUI.Canvas.GH_Canvas canvas) {
            //// We should only draw wires if the document loaded in the canvas is the document we're in.
            //if (!ReferenceEquals(GrasshopperDocument, canvas.Document))
            //    return;

            // Find all sliders that plug into the first component input.
            var first = Component.Params.Input[0];
            if (first.SourceCount == 0)
                return;

            foreach (var source in first.Sources) {
                var slider = source as Grasshopper.Kernel.Special.GH_NumberSlider;
                if (slider == null)
                    continue;

                var input = first.Attributes.InputGrip;
                var output = slider.Attributes.OutputGrip;

                var path = Grasshopper.GUI.Canvas.GH_Painter.ConnectionPath(
                  input, output,
                  Grasshopper.GUI.Canvas.GH_WireDirection.left,
                  Grasshopper.GUI.Canvas.GH_WireDirection.right);

                var edge = new System.Drawing.Pen(System.Drawing.Color.DeepPink, 8);
                edge.DashCap = System.Drawing.Drawing2D.DashCap.Round;
                edge.DashPattern = new float[] { 0.1f, 2f };

                var edge2 = new System.Drawing.Pen(System.Drawing.Color.DeepPink, 8);

                //canvas.Graphics.DrawPath(edge, path);

                var test = new PointF[] {input, output};
                var input2 = new PointF(input.X -50, input.Y);
                var output2 = new PointF(output.X +50, output.Y);
                //canvas.Graphics.DrawCurve(edge2, test);
                canvas.Graphics.DrawBezier(edge2, input, input2, output2, output);

                edge.Dispose();
                path.Dispose();
            }
        }
    }
}
