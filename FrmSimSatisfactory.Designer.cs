
namespace SimSatisfactory
{
    partial class FrmSimSatisfactory
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FrmSimSatisfactory
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1324, 654);
            this.DoubleBuffered = true;
            this.Name = "FrmSimSatisfactory";
            this.Text = "SimSatisfactory";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.SizeChanged += new System.EventHandler(this.FrmSimSatisfactory_SizeChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FrmSimSatisfactory_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FrmSimSatisfactory_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmSimSatisfactory_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FrmSimSatisfactory_KeyUp);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.FrmSimSatisfactory_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.FrmSimSatisfactory_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FrmSimSatisfactory_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FrmSimSatisfactory_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FrmSimSatisfactory_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

