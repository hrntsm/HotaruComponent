using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace HotaruComponent {
    public class HotaruAlgorithmInfo : GH_AssemblyInfo {
        public override string Name {
            get {
                return "MISE";
            }
        }
        //public override Bitmap Icon {
        //    get {
        //        //Return a 24x24 pixel bitmap to represent this GHA library.
        //        return;
        //    }
        //}
        public override string Description {
            get {
                //Return a short string describing the purpose of this GHA library.
                return "Stress Analysis of the Simple Beam";
            }
        }
        public override Guid Id {
            get {
                return new Guid("8b3cd60f-4aaa-4723-a285-ce5590a58ade");
            }
        }
        public override string AuthorName {
            get {
                //Return a string identifying you or your company.
                return "hiron_rgrk";
            }
        }
        public override string AuthorContact {
            get {
                //Return a string representing your preferred contact details.
                return "hiro.n.rgrk@gmail.com";
            }
        }
    }
}