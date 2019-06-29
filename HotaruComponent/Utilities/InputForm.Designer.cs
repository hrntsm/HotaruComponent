namespace HotaruComponent.Utilities {
    partial class InputForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.ValueTrackBar = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.ValueTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // ValueTrackBar
            // 
            this.ValueTrackBar.Location = new System.Drawing.Point(12, 45);
            this.ValueTrackBar.Maximum = 100;
            this.ValueTrackBar.Name = "ValueTrackBar";
            this.ValueTrackBar.Size = new System.Drawing.Size(339, 56);
            this.ValueTrackBar.TabIndex = 0;
            // 
            // InputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(363, 142);
            this.Controls.Add(this.ValueTrackBar);
            this.Name = "InputForm";
            this.Text = "ControlValue";
            this.Load += new System.EventHandler(this.InputForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ValueTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TrackBar ValueTrackBar;
    }
}