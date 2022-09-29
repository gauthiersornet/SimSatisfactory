using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimSatisfactory
{
    public partial class FrmConfig : Form
    {
        private Producteur prod;
        private Lien ln;

        public FrmConfig(Producteur prod)
        {
            this.prod = prod;
            InitializeComponent();
            txtFacteur.Text = prod.Facteur.ToString();
            //chkBloquer.Checked = prod.Etat.HasFlag(Producteur.EEtat.Facteur_bloqué);
            this.Text = "Config producteur";
        }

        public FrmConfig(Lien ln)
        {
            this.ln = ln;
            InitializeComponent();
            txtFacteur.Text = ln.Facteur.ToString();
            //chkBloquer.Checked = prod.Etat.HasFlag(Producteur.EEtat.Facteur_bloqué);
            this.Text = "Config lien";
        }

        private void btValid_Click(object sender, EventArgs e)
        {
            if (prod != null) prod.Facteur = double.Parse(txtFacteur.Text.Replace(".", ","));
            if (ln != null) ln.Facteur = double.Parse(txtFacteur.Text);
            /*if (chkBloquer.Checked) prod.Etat |= Producteur.EEtat.Facteur_bloqué;
            else prod.Etat &= ~Producteur.EEtat.Facteur_bloqué;*/
            DialogResult = DialogResult.Yes;
        }

        private void btAnnuler_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void txtFacteur_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) btValid_Click(sender, e);
        }

        private void FrmConfig_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) btAnnuler_Click(sender, e);
        }

        private void txtFacteur_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) btAnnuler_Click(sender, e);
        }
    }
}
