using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;

using System;
using System.Windows.Forms;
using System.Collections.Generic;

// Refer to https://discourse.mcneel.com/t/optimization-plug-in-for-grasshopper-how-to-use-galapagos-interface-and-gene-pool/27267/17

namespace GHOptimizationTest {
    public class ChangeNumSlider : GH_Component {
        GH_Document doc;
        IGH_Component Component;
        public int Value { get; set; }

        HotaruComponent.Utilities.InputForm _form;
        public void DisplayForm() {

            _form = new HotaruComponent.Utilities.InputForm();
            _form.ValueTrackBar.Value = Value;

            _form.FormClosed += OnFormClosed;
            _form.ValueTrackBar.ValueChanged += ValueTrackBar_ValueChanged;

            GH_WindowsFormUtil.CenterFormOnCursor(_form, true);
            _form.Show(Grasshopper.Instances.DocumentEditor);
        }

        private void ValueTrackBar_ValueChanged(object sender, EventArgs e) {
            TrackBar trackBar = sender as TrackBar;
            if (trackBar != null) {
                Value = trackBar.Value;
                ExpireSolution(true);
            }
        }

        private void OnFormClosed(object sender, FormClosedEventArgs formClosedEventArgs) {
            _form = null;
        }

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ChangeNumSlider()
            : base("GHOptimizationTest", "Nickname",
                "Description",
                "Hotaru", "Hotaru") {
        }

        /// <summary>
        /// カスタムの設定
        /// </summary>
        public override void CreateAttributes() {
            m_attributes = new Attributes_Custom(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddIntegerParameter("f(x*)", "f(x*)", "optimum", GH_ParamAccess.item);
            pManager.AddIntegerParameter("x", "x", "decision variable", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddIntegerParameter("Value", "V", "Value via UI.", GH_ParamAccess.item);
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

            sliders[1].SetSliderValue(Value);

            // 出力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetData(0, Value + fopt);
        }

        public class Attributes_Custom : GH_ComponentAttributes {
            public Attributes_Custom(IGH_Component ChangeNumberSliderTestComponent)
                : base(ChangeNumberSliderTestComponent) { }

            public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e) {
                (Owner as ChangeNumSlider)?.DisplayForm();
                return GH_ObjectResponse.Handled;
            }
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
    }
}
