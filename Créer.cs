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
    public partial class FrmCréer : Form
    {
        private string[] choix;
        public string valeur;

        public FrmCréer(string titre, string[] choix)
        {
            this.choix = choix;
            InitializeComponent();
            this.Text = "Créer " + titre;
            lstBxPropositions.Items.AddRange(choix);
        }

        private void btValid_Click(object sender, EventArgs e)
        {
            if (lstBxPropositions.SelectedIndex < 0) lstBxPropositions.SelectedIndex = 0;
             valeur = lstBxPropositions.SelectedItem.ToString();
            DialogResult = DialogResult.Yes;
        }

        private void btAnnuler_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void txtRecherche_TextChanged(object sender, EventArgs e)
        {
            lstBxPropositions.Items.Clear();
            if(string.IsNullOrWhiteSpace(txtRecherche.Text)) lstBxPropositions.Items.AddRange(choix); 
            else lstBxPropositions.Items.AddRange(choix.Where(c => c.ToUpper().Contains(txtRecherche.Text.ToUpper())).ToArray());
        }

        private void lstBxPropositions_DoubleClick(object sender, EventArgs e)
        {
            btValid_Click(sender, e);
        }

        private void FrmCréer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) btAnnuler_Click(sender, e);
        }

        private void txtRecherche_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) btAnnuler_Click(sender, e);
            else if (e.KeyCode == Keys.Enter) btValid_Click(sender, e);
        }

        private void lstBxPropositions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) btAnnuler_Click(sender, e);
            else if(e.KeyCode == Keys.Enter) btValid_Click(sender, e);
        }
    }
}
