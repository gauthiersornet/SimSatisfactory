using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimSatisfactory
{
    public class Lien
    {
        public Producteur ProducteurSource;
        public Pièce.EPièce EPièce;
        public Producteur ProducteurDestinataire;
        public double Facteur;
        public double Quantité;
        public double QuantitéThéorique;
        public double QuantitéRéel;
        //public double QuantitéRéel { get => Math.Min(Facteur, QuantitéThéorique); }

        public Lien()
        {
            Facteur = 780.0;
            RéinitialiserCalcul();
        }

        public bool EstLienOptimisable
        {
            get
            {
                return ProducteurSource != null && ProducteurDestinataire != null && ProducteurSource.EstTypeOptimisable;
            }
        }

        public bool EstLienRéductible
        {
            get
            {
                return EstLienOptimisable || (ProducteurSource != null && ProducteurSource.EstLienDestReductible(this));
            }
        }

        public void RéinitialiserCalcul()
        {
            Quantité = 0.0;
            QuantitéThéorique = 0.0;
            QuantitéRéel = 0.0;
        }

        public Lien(Producteur producteurSource, Pièce.EPièce ePièce, Producteur producteurDestinataire, double facteur, double quantité, double quantitéThéo, double quantitéRéel)
        {
            ProducteurSource = producteurSource;
            EPièce = ePièce;
            ProducteurDestinataire = producteurDestinataire;
            Facteur = facteur;
            Quantité = quantité;
            QuantitéThéorique = quantitéThéo;
            QuantitéRéel = quantitéRéel;
        }

        public XmlNode Sérialiser(XmlDocument doc)
        {
            XmlNode lien = doc.CreateNode(XmlNodeType.Element, "LienSortant", "");
            XmlAttribute att;
            att = doc.CreateAttribute("id"); att.Value = ProducteurDestinataire.id.ToString(); lien.Attributes.Append(att);
            att = doc.CreateAttribute("pièce"); att.Value = EPièce.ToString(); lien.Attributes.Append(att);
            att = doc.CreateAttribute("facteur"); att.Value = Facteur.ToString(); lien.Attributes.Append(att);
            att = doc.CreateAttribute("quantité"); att.Value = Quantité.ToString(); lien.Attributes.Append(att);
            att = doc.CreateAttribute("quantitéThéo"); att.Value = QuantitéThéorique.ToString(); lien.Attributes.Append(att);
            att = doc.CreateAttribute("quantitéRéel"); att.Value = QuantitéRéel.ToString(); lien.Attributes.Append(att);
            return lien;
        }

        public override int GetHashCode()
        {
            return (ProducteurSource?.GetHashCode() ?? 0) + EPièce.GetHashCode() << 1 + (ProducteurDestinataire?.GetHashCode() ?? 0) << 2;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj)) return true;
            else if(obj is Lien)
            {
                Lien l = obj as Lien;
                return (l.ProducteurSource == ProducteurSource && l.EPièce == EPièce && l.ProducteurDestinataire == ProducteurDestinataire);
            }
            else return false;
        }
    }

    public class Producteur
    {
        public enum SelectionMode
        {
            None = 0,
            Déplacement,
            LienEntrant,
            LienSortant,
            SelectMultiple,
        }

        static readonly public Color[] Couleur = new Color[]
        {
            Color.LightGray,//Vide = 0,
            Color.Maroon,//Centrale_Biomasse = 1,
            Color.Black,//Centrale_Charbon = 2,
            Color.OrangeRed,//Centrale_Carburant = 3,
            Color.Green,//Centrale_Nucléaire = 4,
            Color.DarkGray,//Foreuse = 5,
            Color.DarkRed,//Fonderie = 6,
            Color.Red,//Fonderie_Avancée = 7,
            Color.Blue,//Pompe_Eau = 8,
            Color.Black,//Pompe_Pétrole = 9,
            Color.DarkViolet,//Rafinerie = 10,
            Color.Black,//Packageur = 11,
            Color.Cyan,//Pompe_Azote = 12,
            Color.Black,//Constructeur = 13,
            Color.Black,//Assembleuse = 14,
            Color.Black,//Façonneuse = 15,
            Color.Violet,//Mélangeur = 16
            Color.Black,//Conteneur = 17
            Color.BlueViolet//Accélérateur de particule = 18
        };

        static public readonly int Img_NbProd_Colonne = 5;

        [Flags]
        public enum EEtat : byte
        {
            Vide = 0,
            Sous_Dimentionné = (1 << 0),
            Sur_Dimentionné = (1 << 1),
            Est_Calculé = (1 << 2),
            Est_Optimisé = (1 << 3),
            Est_Flux_Optimisé = (1 << 4)
        }

        static public Image ProducteurImages = Bitmap.FromFile("Batiments.png");

        static readonly public float HauteurBase = 105.0f;
        static readonly public float HauteurUnitaire = 60.0f;

        static readonly public float LargeurMin = 220.0f;
        static readonly public float LargeurBase = 20.0f;
        static readonly public float LargeurUnitaire = 100.0f;

        private static readonly PointF deltaPointRecette = new PointF(0.0f, 5.0f);

        static readonly public float[] CoefLargeur = new float[]
        {
            50.0f,//Vide = 0,
            1.0f,//Centrale_Biomasse = 1,
            2.0f,//Centrale_Charbon = 2,
            1.0f,//Centrale_Carburant = 3,
            2.0f,//Centrale_Nuclaire = 4,
            1.5f,//Foreuse = 5,
            1.5f,//Fonderie = 6,
            2.0f,//Fonderie_Avancée = 7,
            1.5f,//Pompe_Eau = 8,
            1.0f,//Pompe_Pétrole = 9,
            2.0f,//Rafinerie = 10,
            2.0f,//Packageur = 11,
            1.0f,//Pompe_Azote = 12,
            1.8f,//Constructeur = 13,
            2.0f,//Assembleuse = 14,
            4.0f,//Façonneuse = 15,
            4.0f,//Mélangeur = 16
            1.0f,//Conteneur = 17
            3.0f,//Accélérateur de particules = 18
        };

        static public void RelierProducteur(Producteur source, Producteur destinataire, Pièce.EPièce epièce, double facteur = -1.0, double quantité = 0.0, double quantitéThéo = 0.0, double quantitéRéel = 0.0)
        {
            if (source.ProduitPièce(epièce) && destinataire.RequièrePièce(epièce))
            {
                if (facteur < 0.0) facteur = Pièce.ObtenirLimiteLogistique(epièce);
                /*{
                    if (epièce.ToString().StartsWith("fluide_")) facteur = 600.0;
                    else facteur = 780.0;
                }*/
                Lien lien = new Lien(source, epièce, destinataire, facteur, quantité, quantitéThéo, quantitéRéel);
                if (source.Destinataires == null) source.Destinataires = new List<Lien>() { lien };
                else if (!source.Destinataires.Contains(lien)) source.Destinataires.Add(lien);
                if (destinataire.Sources == null) destinataire.Sources = new List<Lien>() { lien };
                else if (!destinataire.Sources.Contains(lien)) destinataire.Sources.Add(lien);
            }
        }

        static public void SupprimerLien(Lien ln)
        {
            if (ln.ProducteurSource.Destinataires != null)
            {
                ln.ProducteurSource.Destinataires.Remove(ln);
                if (ln.ProducteurSource.Destinataires.Any() == false) ln.ProducteurSource.Destinataires = null;
            }
            if (ln.ProducteurDestinataire.Sources != null)
            {
                ln.ProducteurDestinataire.Sources.Remove(ln);
                if (ln.ProducteurDestinataire.Sources.Any() == false) ln.ProducteurDestinataire.Sources = null;
            }
        }

        static public void SupprimerLien(Producteur source, Producteur destinataire, Pièce.EPièce epièce)
        {
            if(source.Destinataires != null)
            {
                source.Destinataires.RemoveAll(x => x.ProducteurSource == source && x.ProducteurDestinataire == destinataire && x.EPièce == epièce);
                if (source.Destinataires.Any() == false) source.Destinataires = null;
            }
            if (destinataire.Sources != null)
            {
                destinataire.Sources.RemoveAll(x => x.ProducteurSource == source && x.ProducteurDestinataire == destinataire && x.EPièce == epièce);
                if (destinataire.Sources.Any() == false) destinataire.Sources = null;
            }
        }

        public uint id;
        public PointF P;
        public List<Lien> Sources;
        public Recette ProgRecette;
        public double Facteur, FacteurAppro, FacteurThéo/*, FacteurRéel*/;
        public Pièce.EPièce PièceLimit;
        public double FacteurRéel { get => Math.Min(FacteurThéo, Facteur); }
        public double Consommation { get => (ProgRecette != null ? ProgRecette.énergie : 0.0); }
        public double ConsommationRéel { get => Consommation * FacteurRéel; }
        public double ConsommationMax { get => Consommation * Facteur; }
        public List<Lien> Destinataires;
        public EEtat Etat;

        //public int NbMachines { get => (int)(Facteur + 0.99999999999); }

        public bool RequièrePièce(Pièce.EPièce pièce)
        {
            if (ProgRecette != null) return ProgRecette.Entrants.ContientPièce(pièce);
            else return false;
        }

        public bool ProduitPièce(Pièce.EPièce pièce)
        {
            if (ProgRecette != null) return ProgRecette.Sortants.ContientPièce(pièce);
            else return false;
        }

        public HashSet<Pièce.EPièce> EPièceNonAppro
        {
            get
            {
                if (ProgRecette == null || ProgRecette.Entrants.ingrédients == null || !ProgRecette.Entrants.ingrédients.Any()) return new HashSet<Pièce.EPièce>();
                else
                {
                    HashSet<Pièce.EPièce> resources = ProgRecette.EnssembleEntrantsEPièces;
                    if (Sources != null && Sources.Any()) Sources.ForEach(p => resources.Remove(p.EPièce));
                    return resources;
                }
            }
        }

        public HashSet<Pièce.EPièce> EPièceNonApproQt
        {
            get
            {
                if (ProgRecette == null || ProgRecette.Entrants.ingrédients == null || !ProgRecette.Entrants.ingrédients.Any()) return new HashSet<Pièce.EPièce>();
                else
                {
                    HashSet<Pièce.EPièce> resources = ProgRecette.EnssembleEntrantsEPièces;
                    if (Sources != null && Sources.Any()) Sources.ForEach(p => { if (p.Quantité != 0.0) resources.Remove(p.EPièce); });
                    return resources;
                }
            }
        }

        public bool EstApprovisioné
        {
            get
            {
                HashSet<Pièce.EPièce> resources = this.EPièceNonAppro;
                return resources != null && !resources.Any();
            }
        }

        public bool EstApproCalculable
        {
            get
            {
                HashSet<Pièce.EPièce> resources = EPièceNonApproQt;
                return resources != null && !resources.Any();
            }
        }

        public bool EstCalculable
        {
            get
            {
                if (ProgRecette != null)
                {
                    if (Sources != null)
                        return Sources.All(s => s.ProducteurSource == null || s.ProducteurSource.Etat.HasFlag(EEtat.Est_Calculé));
                    else return true;
                }
                else return false;
            }
        }

        public bool EstOptimisable
        {
            get
            {
                if (ProgRecette != null && EstTypeOptimisable)
                {
                    if (Sources != null)
                        return Sources.All(s => s.ProducteurSource == null || s.ProducteurSource.Etat.HasFlag(EEtat.Est_Optimisé));
                    else return true;
                }
                else return false;
            }
        }

        public bool EstTypeOptimisable
        {
            get => ProgRecette.TypeProducteur != Recette.EProducteur.Conteneur && ProgRecette.TypeProducteur != Recette.EProducteur.Foreuse && ProgRecette.TypeProducteur != Recette.EProducteur.Pompe_Eau && ProgRecette.TypeProducteur != Recette.EProducteur.Pompe_Pétrole && ProgRecette.TypeProducteur != Recette.EProducteur.Pompe_Azote;
        }

        public bool EstFluxOptimisable
        {
            get
            {
                if (ProgRecette != null && EstTypeOptimisable)
                {
                    if (Destinataires != null)
                        return Destinataires.All(s => s.ProducteurDestinataire == null || s.ProducteurDestinataire.Etat.HasFlag(EEtat.Est_Flux_Optimisé));
                    else return true;
                }
                else return false;
            }
        }

        public Recette.EProducteur TypeProducteur { get => ProgRecette?.TypeProducteur ?? Recette.EProducteur.Vide; }
        public SizeF Taille { get => new SizeF(Math.Max(LargeurMin, LargeurBase + LargeurUnitaire * CoefLargeur[(int)TypeProducteur]), HauteurBase + HauteurUnitaire * (ProgRecette != null ? (ProgRecette.Entrants.EstVide ? 0 : 1) + (ProgRecette.Sortants.EstVide ? 0 : 1) : 0)); }

        public Producteur()
        {
            Sources = null;
            ProgRecette = null;
            Facteur = 1.0;
            Destinataires = null;
            RéinitialiserCalcul();
        }

        public void RéinitialiserCalcul()
        {
            FacteurAppro = 0.0;
            FacteurThéo = 0.0;
            Etat &= ~EEtat.Sous_Dimentionné;
            Etat &= ~EEtat.Sur_Dimentionné;
            Etat &= ~EEtat.Est_Calculé;
            PièceLimit = Pièce.EPièce.vide;
        }

        public Producteur(Recette recette, double facteur = -1.0)
        {
            Sources = null;
            ProgRecette = recette;
            if(facteur < 0.0)
            {
                if (TypeProducteur == Recette.EProducteur.Foreuse) facteur = 240.0;
                else if(TypeProducteur == Recette.EProducteur.Pompe_Pétrole) facteur = 120.0;
                else if(TypeProducteur == Recette.EProducteur.Conteneur) facteur = 780.0;
                else facteur = 1.0;
            }
            Facteur = facteur;
            Destinataires = null;
        }

        public Producteur(XmlNode nd, PointF dropP)
        {
            id = uint.Parse(nd.Attributes.GetNamedItem("id").Value);
            P.X = dropP.X + float.Parse(nd.Attributes.GetNamedItem("x").Value);
            P.Y = dropP.Y + float.Parse(nd.Attributes.GetNamedItem("y").Value);
            ProgRecette = Recette.Parse(nd.Attributes.GetNamedItem("recette").Value);
            Facteur = double.Parse(nd.Attributes.GetNamedItem("facteur").Value);
            FacteurAppro = double.Parse(nd.Attributes.GetNamedItem("facteurAppro").Value);
            FacteurThéo = double.Parse(nd.Attributes.GetNamedItem("facteurThéo").Value);
            //FacteurRéel = double.Parse(nd.Attributes.GetNamedItem("facteurRéel").Value);
            //PièceLimit = (Pièce.EPièce)int.Parse(nd.Attributes.GetNamedItem("pièceLimit").Value);
            PièceLimit = (Pièce.EPièce)Enum.Parse(typeof(Pièce.EPièce), nd.Attributes.GetNamedItem("pièceLimit").Value);
            Etat = (EEtat)int.Parse(nd.Attributes.GetNamedItem("état").Value);
        }

        public XmlNode Sérialiser(XmlDocument doc, PointF dropP)
        {
            XmlNode res = doc.CreateNode(XmlNodeType.Element, "Producteur", "");
            {
                XmlAttribute att;
                att = doc.CreateAttribute("id"); att.Value = id.ToString(); res.Attributes.Append(att);
                att = doc.CreateAttribute("x"); att.Value = (P.X - dropP.X).ToString(); res.Attributes.Append(att);
                att = doc.CreateAttribute("y"); att.Value = (P.Y - dropP.Y).ToString(); res.Attributes.Append(att);
                att = doc.CreateAttribute("recette"); att.Value = ProgRecette?.ToString() ?? ""; res.Attributes.Append(att);
                att = doc.CreateAttribute("facteur"); att.Value = Facteur.ToString(); res.Attributes.Append(att);
                att = doc.CreateAttribute("facteurAppro"); att.Value = FacteurAppro.ToString(); res.Attributes.Append(att);
                att = doc.CreateAttribute("facteurThéo"); att.Value = FacteurThéo.ToString(); res.Attributes.Append(att);
                //att = doc.CreateAttribute("facteurRéel"); att.Value = FacteurRéel.ToString(); res.Attributes.Append(att);
                att = doc.CreateAttribute("pièceLimit"); att.Value = PièceLimit.ToString(); res.Attributes.Append(att);
                att = doc.CreateAttribute("état"); att.Value = ((int)Etat).ToString(); res.Attributes.Append(att);
            }
            /*if (Destinataires != null) Destinataires.ForEach(d =>
            {
                XmlNode lien = doc.CreateNode(XmlNodeType.Element, "LienSortant", "");
                XmlAttribute att;
                att = doc.CreateAttribute("id"); att.Value = d.Item1.id.ToString(); lien.Attributes.Append(att);
                att = doc.CreateAttribute("pièce"); att.Value = d.Item2.ToString(); lien.Attributes.Append(att);
                res.AppendChild(lien);
            });*/
            if (Destinataires != null)
            {
                Destinataires.ForEach(d => res.AppendChild(d.Sérialiser(doc)));
            }
            return res;
        }

        public void ChargerLiensSortants(XmlNodeList liens, Dictionary<uint, Producteur> mapProd)
        {
            if(liens != null)
            {
                foreach(XmlNode nd in liens)
                {
                    uint idDest = uint.Parse(nd.Attributes.GetNamedItem("id").Value);
                    Pièce.EPièce pc = (Pièce.EPièce)Enum.Parse(typeof(Pièce.EPièce), nd.Attributes.GetNamedItem("pièce").Value);
                    double facteur = double.Parse(nd.Attributes.GetNamedItem("facteur").Value);
                    double quantité = double.Parse(nd.Attributes.GetNamedItem("quantité").Value);
                    double quantitéThéo = double.Parse(nd.Attributes.GetNamedItem("quantitéThéo").Value);
                    double quantitéRéel = double.Parse(nd.Attributes.GetNamedItem("quantitéRéel").Value);
                    Producteur.RelierProducteur(this, mapProd[idDest], pc, facteur, quantité, quantitéThéo, quantitéRéel);
                }
            }
        }

        public int NbLienEntrant(Pièce.EPièce epièce)
        {
            return (Sources != null ? Sources.Count(s => s.EPièce == epièce) : 0);
        }

        public int NbLienSortant(Pièce.EPièce epièce)
        {
            return (Destinataires != null ? Destinataires.Count(s => s.EPièce == epièce) : 0);
        }

        public bool EstSourceDe(Producteur prod, HashSet<Producteur> fermé = null)
        {
            if (prod == this) return true;
            else if (Destinataires != null)
            {
                if (Destinataires.Any(d => d.ProducteurDestinataire == prod)) return true;
                else
                {
                    if(fermé == null) fermé = new HashSet<Producteur>();
                    fermé.Add(this);
                    return Destinataires.Any(d => !fermé.Contains(d.ProducteurDestinataire) && d.ProducteurDestinataire.EstSourceDe(prod, fermé));
                }
            }
            else return false;
        }

        public bool EstDestinataireDe(Producteur prod, HashSet<Producteur> fermé = null)
        {
            if (Sources != null)
            {
                if (Sources.Any(d => d.ProducteurSource == prod)) return true;
                else
                {
                    if (fermé == null) fermé = new HashSet<Producteur>();
                    fermé.Add(this);
                    return Sources.Any(d => !fermé.Contains(d.ProducteurSource) && d.ProducteurSource.EstDestinataireDe(prod));
                }
            }
            else return false;
        }

        public void Dessiner(Graphics g)
        {
            Matrix m = g.Transform;
            g.TranslateTransform(P.X, P.Y);
            Color coul = Couleur[(int)TypeProducteur];
            Pen pn = new Pen(coul, 5.0f);

            SizeF taille = Taille;
            g.DrawRectangle(pn, - taille.Width / 2.0f, -taille.Height / 2.0f, taille.Width, taille.Height);

            int lig = ((int)TypeProducteur) / Img_NbProd_Colonne;
            int col = ((int)TypeProducteur) % Img_NbProd_Colonne;
            g.DrawImage(ProducteurImages, new RectangleF(-taille.Width / 2.0f, -taille.Height / 2.0f, 50.0f, 50.0f), new RectangleF(col * 50.0f, lig * 50.0f, 50.0f, 50.0f), GraphicsUnit.Pixel);
            Brush brsh = new SolidBrush(coul);
            g.DrawString(TypeProducteur.ToString() + "\nfacteur " + Math.Round(Facteur, 3) + (Etat.HasFlag(EEtat.Sous_Dimentionné) ? "+" : (Etat.HasFlag(EEtat.Sur_Dimentionné) ? "-" : "")), new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, 51.0f - taille.Width / 2.0f, 2.0f-taille.Height / 2.0f);

            if (ProgRecette != null)
            {
                ProgRecette.Dessiner(g, deltaPointRecette, Facteur, Math.Min(Facteur, FacteurThéo), PièceLimit);
                if (ProgRecette.Entrants.EstVide == false)
                {
                    g.DrawString($"FacAppro\n{Math.Round(FacteurAppro, 3)}", new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, 5.0f - taille.Width / 2.0f, -6.0f);
                    g.DrawString($"FacThéo\n{Math.Round(FacteurThéo, 3)}", new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, 15.0f, -6.0f);
                }
            }
            g.Transform = m;
        }

        public void DessinerAllow(Graphics g)
        {
            Pen pn = new Pen(Color.Yellow, 6.0f);
            SizeF taille = Taille;
            taille.Width += 14.0f;
            taille.Height += 14.0f;
            g.DrawRectangle(pn, P.X - taille.Width / 2.0f, P.Y - taille.Height / 2.0f, taille.Width, taille.Height);
        }

        public void DessinerLiensSortant(Graphics g)
        {
            if(Destinataires != null)
            {
                Pen pn = new Pen(Couleur[(int)TypeProducteur], 5.0f);
                PointF pSortant = new PointF(P.X, P.Y + Taille.Height / 2.0f);
                foreach(var lien in Destinataires)
                {
                    PointF pEntrant = lien.ProducteurDestinataire.P;
                    pEntrant.Y -= lien.ProducteurDestinataire.Taille.Height / 2.0f;
                    PointF pMid = new PointF((pSortant.X + pEntrant.X) / 2.0f, (pSortant.Y + pEntrant.Y) / 2.0f);
                    g.DrawLine(pn, pSortant, pEntrant);
                    Ingrédient.Dessinner(g, pMid, lien.EPièce);
                    Brush brsh = new SolidBrush(Color.Black);
                    g.DrawString("F" + Math.Round(lien.Facteur, 3).ToString(), new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, pMid.X - 25.0f, pMid.Y - 48.0f);
                    g.DrawString("A" + Math.Round(lien.Quantité, 3).ToString(), new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, pMid.X - 25.0f, pMid.Y - 38.0f);
                    g.DrawString("T" + Math.Round(lien.QuantitéThéorique, 3).ToString(), new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, pMid.X - 25.0f, pMid.Y + 25.0f);
                    g.DrawString("R" + Math.Round(lien.QuantitéRéel, 3).ToString(), new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, pMid.X - 25.0f, pMid.Y + 35.0f);
                }
            }
        }

        public Lien SelectionLienSortant(PointF p)
        {
            if (Destinataires != null)
            {
                Pen pn = new Pen(Couleur[(int)TypeProducteur], 5.0f);
                PointF pSortant = new PointF(P.X, P.Y + Taille.Height / 2.0f);
                p.X -= pSortant.X;
                p.Y -= pSortant.Y;
                for(int i = Destinataires.Count-1; i >= 0; --i)
                {
                    Lien lien = Destinataires[i];
                    PointF pEntrant = lien.ProducteurDestinataire.P;
                    pEntrant.Y -= lien.ProducteurDestinataire.Taille.Height / 2.0f;
                    pEntrant.X -= pSortant.X;
                    pEntrant.Y -= pSortant.Y;
                    float scal = (p.X * pEntrant.X + p.Y * pEntrant.Y);
                    float sqMag = (pEntrant.X * pEntrant.X + pEntrant.Y * pEntrant.Y);
                    if (0.0f <= scal && scal <= sqMag)
                    {
                        float hypo = (float)Math.Sqrt(sqMag);
                        if(hypo > 0.000001f)
                        {
                            float d = (p.X * -pEntrant.Y + p.Y * pEntrant.X) / hypo;
                            if (Math.Abs(d) <= 5.0f) return lien;
                        }
                    }
                }
                return null;
            }
            else return null;
        }

        public void DessinerLiens(Graphics g, PointF pd, PointF pf)
        {
            Pen pn = new Pen(Couleur[(int)TypeProducteur], 5.0f);
            g.DrawLine(pn, pd, pf);
        }

        public bool EstDans(PointF p)
        {
            SizeF taille = Taille;
            taille.Width /= 2.0f;
            taille.Height /= 2.0f;
            p.X -= P.X;
            p.Y -= P.Y;
            return (-taille.Width <= p.X && p.X <= taille.Width && -taille.Height <= p.Y && p.Y <= taille.Height);
        }

        public (SelectionMode, Pièce.EPièce) ObtenirMode(PointF p)
        {
            SizeF taille = Taille;
            taille.Width /= 2.0f;
            taille.Height /= 2.0f;
            p.X -= P.X;
            p.Y -= P.Y;
            if (-taille.Width <= p.X && p.X <= taille.Width && -taille.Height <= p.Y && p.Y <= taille.Height)
            {
                if (ProgRecette != null)
                {
                    p.X -= deltaPointRecette.X;
                    p.Y -= deltaPointRecette.Y;
                    var res = ProgRecette.Selection(p);
                    if (res.Item2 == Pièce.EPièce.vide) res.Item1 = SelectionMode.Déplacement;
                    return res;
                }
                else return (SelectionMode.Déplacement, Pièce.EPièce.vide);
            }
            else return (SelectionMode.None, Pièce.EPièce.vide);
        }

        public void SupprimerLiens()
        {
            if(Sources != null)
            {
                Sources.ForEach(l => {
                    if (l.ProducteurSource.Destinataires != null)
                    {
                        l.ProducteurSource.Destinataires.RemoveAll(ld => ld.ProducteurDestinataire == this);
                        if (l.ProducteurSource.Destinataires.Any() == false)
                            l.ProducteurSource.Destinataires = null;
                    }
                });
                Sources = null;
            }
            if (Destinataires != null)
            {
                Destinataires.ForEach(l => {
                    if (l.ProducteurDestinataire.Sources != null)
                    {
                        l.ProducteurDestinataire.Sources.RemoveAll(ld => ld.ProducteurSource == this);
                        if (l.ProducteurDestinataire.Sources.Any() == false)
                            l.ProducteurDestinataire.Sources = null;
                    }
                });
                Destinataires = null;
            }
        }

        public override string ToString()
        {
            return $"{TypeProducteur}({Math.Round(Facteur, 3)}) : {(ProgRecette != null ? ProgRecette.ToString() : "()=>()")}";
        }

        public void CalculerLiensSortants(Ingrédient ing)
        {
            IEnumerable<Lien> lings = Destinataires.Where(src => src.EPièce == ing.pièce.PType);
            if (lings != null && lings.Any())
            {
                double sumIngQt = Facteur * ing.parMinute;
                double sumIngQtRl = Math.Min(Facteur, FacteurThéo) * ing.parMinute;

                double sumFacLn = lings.Sum(l => l.Facteur);
                if (sumFacLn > 0.00000001)
                {
                    double rest = 0.0;
                    foreach (Lien l in lings)
                    {
                        double facLn = l.Facteur / sumFacLn;
                        l.Quantité = sumIngQt * facLn;
                        l.QuantitéThéorique = sumIngQtRl * facLn;
                        if (l.QuantitéThéorique > l.Facteur)
                        {
                            rest += l.QuantitéThéorique - l.Facteur;
                            l.QuantitéRéel = l.Facteur;
                        }
                        else l.QuantitéRéel = l.QuantitéThéorique;
                    }
                    sumIngQtRl = rest;
                    for (int i = 0; i < 1000 && (sumIngQtRl > 0.0001); ++i)
                    {
                        rest = 0.0;
                        List<Lien> lns = lings.Where(l => l.QuantitéRéel < l.Facteur).ToList();
                        if (lns.Any())
                        {
                            sumFacLn = lns.Sum(l => l.Facteur);
                            if (sumFacLn > 0.00000001)
                            {
                                foreach (Lien l in lns)
                                {
                                    double facLn = l.Facteur / sumFacLn;
                                    l.QuantitéRéel += sumIngQtRl * facLn;
                                    if (l.QuantitéRéel > l.Facteur)
                                    {
                                        rest += l.QuantitéRéel - l.Facteur;
                                        l.QuantitéRéel = l.Facteur;
                                    }
                                }
                            }
                            else break;
                        }
                        sumIngQtRl = rest;
                    }
                }
            }
        }

        public void Calculer()
        {
            Etat &= ~EEtat.Sous_Dimentionné;
            Etat &= ~EEtat.Sur_Dimentionné;
            Etat |= EEtat.Est_Calculé;
            PièceLimit = Pièce.EPièce.vide;
            if (ProgRecette != null)
            {
                if (ProgRecette.Entrants.EstVide)
                {
                    FacteurAppro = Facteur;
                    FacteurThéo = Facteur;
                }
                else
                {
                    if (Sources != null && Sources.Any())
                    {
                        double appro = double.MaxValue;
                        double réel = double.MaxValue;
                        double dbl;
                        PièceLimit = ProgRecette.Entrants.ingrédients.First().pièce.PType;
                        foreach (Ingrédient ing in ProgRecette.Entrants.ingrédients)
                        {
                            IEnumerable<Lien> lings = Sources.Where(src => src.EPièce == ing.pièce.PType);
                            if (lings != null && lings.Any())
                            {
                                double sumIngQt = lings.Sum(l => l.Quantité);
                                double sumIngQtRl = lings.Sum(l => l.QuantitéRéel);
                                appro = Math.Min(appro, sumIngQt / ing.parMinute);
                                dbl = Math.Min(réel, sumIngQtRl / ing.parMinute);
                                if(dbl < réel)
                                {
                                    réel = dbl;
                                    PièceLimit = ing.pièce.PType;
                                }
                            }
                            else
                            {
                                FacteurAppro = 0.0;
                                FacteurThéo = 0.0;
                                return;
                            }
                        }
                        if (réel > Facteur) Etat |= EEtat.Sous_Dimentionné;
                        else if(réel < Facteur) Etat |= EEtat.Sur_Dimentionné;
                        FacteurAppro = appro;
                        FacteurThéo = réel;
                    }
                    else
                    {
                        FacteurAppro = 0.0;
                        FacteurThéo = 0.0;
                    }
                }

                if (Destinataires != null && Destinataires.Any() && !ProgRecette.Sortants.EstVide)
                {
                    foreach (Ingrédient ing in ProgRecette.Sortants.ingrédients)
                        CalculerLiensSortants(ing);
                }
            }
            else
            {
                FacteurAppro = 0.0;
                FacteurThéo = 0.0;
            }
        }

        public void Optimiser()
        {
            Etat &= ~EEtat.Sous_Dimentionné;
            Etat &= ~EEtat.Sur_Dimentionné;
            Etat |= EEtat.Est_Calculé;
            Etat |= EEtat.Est_Optimisé;
            PièceLimit = Pièce.EPièce.vide;
            if (ProgRecette != null)
            {
                if (ProgRecette.Entrants.EstVide)
                {
                    FacteurAppro = Facteur;
                    FacteurThéo = Facteur;
                }
                else
                {
                    //On regarde ce que l'on peut produire selon les appro et on adapte le facteur de prod en conséquence
                    if (Sources != null && Sources.Any())
                    {
                        double appro = double.MaxValue;
                        double réel = double.MaxValue;
                        //PièceLimit = ProgRecette.Entrants.ingrédients.First().pièce.PType;
                        PièceLimit = Pièce.EPièce.vide;
                        foreach (Ingrédient ing in ProgRecette.Entrants.ingrédients)
                        {
                            IEnumerable<Lien> lings = Sources.Where(src => src.EPièce == ing.pièce.PType);
                            if (lings != null && lings.Any())
                            {
                                double sumIngQt = lings.Sum(l => l.Quantité);
                                double sumIngQtRl = lings.Sum(l => l.QuantitéRéel);
                                appro = Math.Min(appro, sumIngQt / ing.parMinute);
                                double dbl = sumIngQtRl / ing.parMinute;
                                if (dbl < réel)
                                {
                                    réel = dbl;
                                    PièceLimit = ing.pièce.PType;
                                }
                            }
                            else
                            {
                                FacteurAppro = 0.0;
                                FacteurThéo = 0.0;
                                Facteur = 0.0;
                                return;
                            }
                        }
                        //if (réel > Facteur) Etat |= EEtat.Sous_Dimentionné;
                        //else if(réel < Facteur) Etat |= EEtat.Sur_Dimentionné;
                        FacteurAppro = appro;
                        FacteurThéo = réel;
                        Facteur = réel;
                    }
                    else
                    {
                        FacteurAppro = 0.0;
                        FacteurThéo = 0.0;
                    }
                }

                if (Destinataires != null && Destinataires.Any() && !ProgRecette.Sortants.EstVide)
                {
                    //L'ingrédient qui produit le moins de résidu sur lequel on peut optimiser
                    double minRésidu = double.MaxValue;
                    foreach (Ingrédient ing in ProgRecette.Sortants.ingrédients)
                    {
                        IEnumerable<Lien> lings = Destinataires.Where(src => src.EPièce == ing.pièce.PType);
                        if (lings != null && lings.Any())
                        {
                            double sumFacLn = lings.Sum(l => l.Facteur);//attentes
                            //Y a t'il des attentes ? => On calcul les valeurs des flux sortant
                            if (sumFacLn > 0.00000001)
                            {
                                double sumIngQt = Facteur * ing.parMinute;
                                double sumIngQtRl = FacteurRéel * ing.parMinute;//ce que l'on produit

                                foreach (Lien l in lings)
                                {
                                    if (l.ProducteurDestinataire != null)
                                    {
                                        double facLn = l.Facteur / sumFacLn;
                                        l.Facteur = facLn * sumIngQtRl;
                                    }
                                    else l.Facteur = 0.0;
                                }
                                sumFacLn = sumIngQtRl;

                                foreach (Lien l in lings)
                                {
                                    double facLn = l.Facteur / sumFacLn;
                                    l.Quantité = sumIngQt * facLn;
                                    l.QuantitéThéorique = sumIngQtRl * facLn;
                                    if (l.QuantitéThéorique > l.Facteur) l.QuantitéRéel = l.Facteur;
                                    else l.QuantitéRéel = l.QuantitéThéorique;
                                }
                                minRésidu = Math.Min(minRésidu, (sumIngQtRl - sumFacLn) / ing.parMinute);
                                /*double rest = 0.0;
                                foreach (Lien l in lings)
                                {
                                    double facLn = l.Facteur / sumFacLn;
                                    l.Quantité = sumIngQt * facLn;
                                    l.QuantitéThéorique = sumIngQtRl * facLn;
                                    if (l.QuantitéThéorique > l.Facteur)
                                    {
                                        rest += l.QuantitéThéorique - l.Facteur;
                                        l.QuantitéRéel = l.Facteur;
                                    }
                                    else l.QuantitéRéel = l.QuantitéThéorique;
                                }
                                sumIngQtRl = rest;
                                //Mise à jour et équilibrage des quantités d'appro dans les liens destinataires
                                for (int i = 0; i < 1000 && (sumIngQtRl > 0.0001); ++i)
                                {
                                    rest = 0.0;
                                    List<Lien> lns = lings.Where(l => l.QuantitéRéel < l.Facteur).ToList();
                                    if (lns.Any())
                                    {
                                        sumFacLn = lns.Sum(l => l.Facteur);
                                        if (sumFacLn > 0.00000001)
                                        {
                                            foreach (Lien l in lns)
                                            {
                                                double facLn = l.Facteur / sumFacLn;
                                                l.QuantitéRéel += sumIngQtRl * facLn;
                                                if (l.QuantitéRéel > l.Facteur)
                                                {
                                                    rest += l.QuantitéRéel - l.Facteur;
                                                    l.QuantitéRéel = l.Facteur;
                                                }
                                            }
                                        }
                                        else break;
                                    }
                                    else break;
                                    sumIngQtRl = rest;
                                }
                                //Résidu = sumIngQtRl
                                minRésidu = Math.Min(minRésidu, sumIngQtRl / ing.parMinute);*/
                            }
                            else
                            {
                                foreach (Lien l in lings)
                                {
                                    l.Quantité = 0.0;
                                    l.QuantitéThéorique = 0.0;
                                    l.QuantitéRéel = 0.0;
                                }
                            }
                        }
                    }
                    if(0.0001 <= minRésidu && minRésidu < double.MaxValue)//On a un résidu minimal ?
                    {
                        Facteur -= minRésidu;
                        if (Facteur < 0.0) Facteur = 0.0;//garde foux

                        foreach (Ingrédient ing in ProgRecette.Sortants.ingrédients)
                        {
                            IEnumerable<Lien> lings = Destinataires.Where(src => src.EPièce == ing.pièce.PType);
                            if (lings != null && lings.Any())
                            {
                                double sumIngQt = Facteur * ing.parMinute;
                                double sumIngQtRl = FacteurRéel * ing.parMinute;//ce que l'on produit
                                double sumFacLn = lings.Sum(l => l.Facteur);

                                if (sumFacLn > 0.00000001)
                                {
                                    foreach (Lien l in lings)
                                    {
                                        double facLn = l.Facteur / sumFacLn;
                                        l.Quantité = sumIngQt * facLn;
                                        l.QuantitéThéorique = sumIngQtRl * facLn;
                                        if (l.QuantitéThéorique > l.Facteur) l.QuantitéRéel = l.Facteur;
                                        else l.QuantitéRéel = l.QuantitéThéorique;
                                    }
                                }
                            }
                        }

                    }
                }

            }
            else
            {
                FacteurAppro = 0.0;
                FacteurThéo = 0.0;
            }
        }

        /// <summary>
        /// Retourne la quantité de production d'une pièce pouvant être réduite
        /// avant que les autres pièces soient limitantes
        /// </summary>
        /// <param name="epièce"></param>
        /// <returns></returns>
        /*public double MargeDeRéductionSortant(Pièce.EPièce epièce)
        {
            if (ProgRecette != null && !ProgRecette.Sortants.EstVide && Destinataires != null && Destinataires.Any())
            {
                Ingrédient ingRed = ProgRecette.Sortants.ingrédients.FirstOrDefault(i => i.pièce.PType == epièce);
                if (ingRed.pièce.PType != Pièce.EPièce.vide)
                {
                    double prodRed = FacteurRéel * ingRed.parMinute;
                    IEnumerable<Lien> lingsRed = Destinataires.Where(src => src.EPièce == epièce);
                    double consoRed = lingsRed.Sum(l => l.QuantitéRéel);
                    double capaRed = consoRed - prodRed;

                    Ingrédient ingMinRésidu = new Ingrédient(Pièce.EPièce.vide, 0.0);
                    double minRésidu = double.MaxValue;
                    foreach (Ingrédient ing in ProgRecette.Sortants.ingrédients)
                    {
                        if (ing.pièce.PType != epièce)
                        {
                            IEnumerable<Lien> lings = Destinataires.Where(src => src.EPièce == ing.pièce.PType);
                            double sommeConso = lings.Sum(l => l.QuantitéRéel);
                            double production = FacteurRéel * ing.parMinute;
                            if (production > sommeConso) // On produit trop celui-ci ?
                            {
                                double resi = production - sommeConso;
                                if (resi < minRésidu)
                                {
                                    minRésidu = resi;
                                    ingMinRésidu = ing;
                                }
                            }
                            else // On produit juste ou pas assez celui-ci ?
                            {
                                minRésidu = 0.0;
                                ingMinRésidu = ing;
                                break;
                            }
                        }
                    }
                    if (ingMinRésidu.pièce.PType != Pièce.EPièce.vide && minRésidu > 0.0)
                    {
                        //Facteur -= minRésidu / ingMinRésidu.parMinute;
                        double factRed = minRésidu / ingMinRésidu.parMinute;//facteur réducteur du taux de fabrication
                        if (factRed < FacteurRéel)//garde foux
                        {
                            return capaRed + ingRed.parMinute * factRed;
                        }
                        else return capaRed;
                    }
                    else return capaRed;
                }
                else return 0.0;
            }
            else return 0.0;
        }*/

        public void OptimiserFlux(double stepFact)
        {
            if (ProgRecette == null) return;
            Etat |= EEtat.Est_Flux_Optimisé;

            bool cheminCritique; // vrai ou faux ?
            if (Destinataires != null && Destinataires.Any())
            {
                double minRésidu = double.MaxValue;
                foreach (Ingrédient ing in ProgRecette.Sortants.ingrédients)
                {
                    //Réévaluation des destinataires en comparaison à la production
                    IEnumerable<Lien> lings = Destinataires.Where(src => src.EPièce == ing.pièce.PType);
                    if (lings != null && lings.Any())
                    {
                        double sumIngQtRl = FacteurRéel * ing.parMinute;
                        double sumFacLn = lings.Sum(l => l.Facteur);

                        if (sumFacLn > 0.00000001)
                        {
                            foreach (Lien l in lings)
                            {
                                double facLn = l.Facteur / sumFacLn;
                                l.QuantitéThéorique = sumIngQtRl * facLn;
                                if (l.QuantitéThéorique > l.Facteur) l.QuantitéRéel = l.Facteur;
                                else l.QuantitéRéel = l.QuantitéThéorique;
                            }
                            minRésidu = Math.Min(minRésidu, (sumIngQtRl - sumFacLn) / ing.parMinute);

                            /*double rest = 0.0;
                            foreach (Lien l in lings)
                            {
                                double facLn = l.Facteur / sumFacLn;
                                l.QuantitéThéorique = sumIngQtRl * facLn;
                                if (l.QuantitéThéorique > l.Facteur)
                                {
                                    rest += l.QuantitéThéorique - l.Facteur;
                                    l.QuantitéRéel = l.Facteur;
                                }
                                else l.QuantitéRéel = l.QuantitéThéorique;
                            }
                            sumIngQtRl = rest;
                            for (int i = 0; i < 1000 && (sumIngQtRl > 0.0001); ++i)
                            {
                                rest = 0.0;
                                List<Lien> lns = lings.Where(l => l.QuantitéRéel < l.Facteur).ToList();
                                if (lns.Any())
                                {
                                    sumFacLn = lns.Sum(l => l.Facteur);
                                    if (sumFacLn > 0.00000001)
                                    {
                                        foreach (Lien l in lns)
                                        {
                                            double facLn = l.Facteur / sumFacLn;
                                            l.QuantitéRéel += sumIngQtRl * facLn;
                                            if (l.QuantitéRéel > l.Facteur)
                                            {
                                                rest += l.QuantitéRéel - l.Facteur;
                                                l.QuantitéRéel = l.Facteur;
                                            }
                                        }
                                    }
                                    else break;
                                }
                                else break;
                                sumIngQtRl = rest;
                            }
                            minRésidu = Math.Min(minRésidu, sumIngQtRl / ing.parMinute);*/
                            //Résidu = sumIngQtRl
                        }
                    }
                }

                if (0.0001 <= minRésidu && minRésidu < double.MaxValue)//On a un résidu minimal ? => Ajuster le facteur de prod
                {
                    cheminCritique = false;
                    Facteur -= minRésidu * stepFact;
                    if (Facteur < 0.0) Facteur = 0.0;//garde foux

                    //Mise à jour des liens de quantité d'approvisionnement
                    foreach (Ingrédient ing in ProgRecette.Sortants.ingrédients)
                    {
                        IEnumerable<Lien> lings = Destinataires.Where(src => src.EPièce == ing.pièce.PType);
                        if (lings != null && lings.Any())
                        {
                            double sumIngQt = Facteur * ing.parMinute;
                            double sumFacLn = lings.Sum(l => l.Facteur);

                            if (sumFacLn > 0.00000001)
                            {
                                foreach (Lien l in lings)
                                {
                                    double facLn = l.Facteur / sumFacLn;
                                    l.Quantité = sumIngQt * facLn;
                                }
                            }
                        }
                    }
                }
                else cheminCritique = true;
            }
            else cheminCritique = true;

            if (Sources != null && Sources.Any())
            {
                //Calculer les appro critiques ou pas...
                //(Ingrédient, approSum, besoin, approSum - besoin)
                (Ingrédient, double, double, double)[] ingAppros = new (Ingrédient, double, double, double)[ProgRecette.Entrants.ingrédients.Length];
                for(int i = 0; i < ProgRecette.Entrants.ingrédients.Length; ++i)
                {
                    Ingrédient ing = ProgRecette.Entrants.ingrédients[i];
                    double besoin = FacteurRéel * ing.parMinute;

                    double approSum;
                    IEnumerable<Lien> lings = Sources.Where(src => src.EPièce == ing.pièce.PType);
                    approSum = lings.Sum(l => l.QuantitéRéel);

                    ingAppros[i] = (ing, approSum, besoin, (approSum - besoin) * stepFact);
                }

                if (cheminCritique)
                {
                    //Rechercher à récupérer sur les appro critique (sources) (à faire un coef de progressivité par la suite)
                    //(Ingrédient, approSum, besoin, approSum - besoin)
                    foreach (var appro in ingAppros)
                    {
                        if (appro.Item4 < 0.000000001)
                        {
                            var lings = Sources.Where(src => src.EPièce == appro.Item1.pièce.PType /*&& src.EstLienOptimisable*/);
                            foreach (Lien ln in lings)
                            {
                                ln.Facteur += ln.ProducteurSource.Récupérer(appro.Item1.pièce.PType, stepFact, ln);
                            }
                        }
                    }

                    //Réhausser le facteur si possible
                    {
                        double fappro = double.MaxValue;
                        double fréel = double.MaxValue;
                        //PièceLimit = ProgRecette.Entrants.ingrédients.First().pièce.PType;
                        PièceLimit = Pièce.EPièce.vide;
                        //(Ingrédient, approSum, besoin, approSum - besoin)
                        foreach (var appro in ingAppros)
                        {
                            if (appro.Item3 > 0.0)
                            {
                                double sumIngQt = Sources.Where(src => src.EPièce == appro.Item1.pièce.PType).Sum(l => l.Quantité);
                                //double sumIngQtRl = lings.Sum(l => l.QuantitéRéel); // appro.Item2
                                fappro = Math.Min(fappro, sumIngQt / appro.Item1.parMinute);
                                double dbl = appro.Item2 / appro.Item1.parMinute;
                                if (dbl < fréel)
                                {
                                    fréel = dbl;
                                    PièceLimit = appro.Item1.pièce.PType;
                                }
                            }
                            else
                            {
                                fappro = 0.0;
                                fréel = 0.0;
                                break;
                            }
                        }
                        //if (réel > Facteur) Etat |= EEtat.Sous_Dimentionné;
                        //else if(réel < Facteur) Etat |= EEtat.Sur_Dimentionné;
                        FacteurAppro = fappro;
                        FacteurThéo = fréel;
                        Facteur = fréel;
                    }

                    //minimiser les autres appro pas critique (source)... (à faire un coef de progressivité par la suite)
                    //(Ingrédient, approSum, besoin, approSum - besoin)
                    foreach (var appro in ingAppros)
                    {
                        if (appro.Item4 >= 0.000000001)
                        {
                            var lings = Sources.Where(src => src.EPièce == appro.Item1.pièce.PType && src.EstLienRéductible);
                            double sumFact = lings.Sum(l => l.Facteur);
                            if (sumFact >= 0.000000001)
                            {
                                foreach (Lien ln in lings)
                                {
                                    double factRed = appro.Item4 * (ln.Facteur / sumFact);
                                    if (!ln.EstLienOptimisable) ln.ProducteurSource.Redistribuer(appro.Item1.pièce.PType, factRed, ln);
                                    ln.Facteur -= factRed;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Le facteur a forcément réduit
                    //donc, on réduit les demandes (sources)...
                    //(Ingrédient, approSum, besoin, approSum - besoin)
                    foreach (var appro in ingAppros)
                    {
                        if(appro.Item4 >= 0.000000001)
                        {
                            var lings = Sources.Where(src => src.EPièce == appro.Item1.pièce.PType && src.EstLienRéductible);//src.EstLienRéductible
                            double sumFact = lings.Sum(l => l.Facteur);
                            if (sumFact >= 0.000000001)
                            {
                                foreach (Lien ln in lings)
                                {
                                    double factRed = appro.Item4 * (ln.Facteur / sumFact);
                                    if(!ln.EstLienOptimisable) ln.ProducteurSource.Redistribuer(appro.Item1.pièce.PType, factRed, ln);
                                    ln.Facteur -= factRed;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Réduit les liens entrants suivant la consomation actuellement necessaire du producteur de l'ingrédient
        /// </summary>
        /*public void OptimiserFlux()
        {
            if (ProgRecette == null) return;
            Etat |= EEtat.Est_Flux_Optimisé;

            //Réduire le facteur prod en fonction des besoins sortant...déjà fait dans l'optimisation
            if (Destinataires != null && Destinataires.Any())
            {
                Ingrédient ingMinRésidu = new Ingrédient(Pièce.EPièce.vide, 0.0);
                double minRésidu = double.MaxValue;
                foreach (Ingrédient ing in ProgRecette.Sortants.ingrédients)
                {
                    IEnumerable<Lien> lings = Destinataires.Where(src => src.EPièce == ing.pièce.PType);
                    if (lings != null && lings.Any())
                    {
                        double sumIngQtRl = FacteurRéel * ing.parMinute;
                        double sumFacLn = lings.Sum(l => l.Facteur);

                        if (sumFacLn > 0.00000001)
                        {
                            double rest = 0.0;
                            foreach (Lien l in lings)
                            {
                                double facLn = l.Facteur / sumFacLn;
                                l.QuantitéThéorique = sumIngQtRl * facLn;
                                if (l.QuantitéThéorique > l.Facteur)
                                {
                                    rest += l.QuantitéThéorique - l.Facteur;
                                    l.QuantitéRéel = l.Facteur;
                                }
                                else l.QuantitéRéel = l.QuantitéThéorique;
                            }
                            sumIngQtRl = rest;
                            for (int i = 0; i < 1000 && (sumIngQtRl > 0.0001); ++i)
                            {
                                rest = 0.0;
                                List<Lien> lns = lings.Where(l => l.QuantitéRéel < l.Facteur).ToList();
                                if (lns.Any())
                                {
                                    sumFacLn = lns.Sum(l => l.Facteur);
                                    if (sumFacLn > 0.00000001)
                                    {
                                        foreach (Lien l in lns)
                                        {
                                            double facLn = l.Facteur / sumFacLn;
                                            l.QuantitéRéel += sumIngQtRl * facLn;
                                            if (l.QuantitéRéel > l.Facteur)
                                            {
                                                rest += l.QuantitéRéel - l.Facteur;
                                                l.QuantitéRéel = l.Facteur;
                                            }
                                        }
                                    }
                                    else break;
                                }
                                else break;
                                sumIngQtRl = rest;
                            }
                            //Résidu = sumIngQtRl
                            if (sumIngQtRl < minRésidu)
                            {
                                minRésidu = sumIngQtRl;
                                ingMinRésidu = ing;
                            }
                        }
                    }
                }

                if (ingMinRésidu.pièce.PType != Pièce.EPièce.vide)//On a un résidu minimal ? => Ajuster le facteur de prod
                {
                    double factRed = minRésidu / (FacteurRéel * ingMinRésidu.parMinute);//facteur réducteur du taux de fabrication
                    Facteur *= (1.0 - factRed);
                    if (Facteur < 0.0) Facteur = 0.0;//garde foux

                    //Mise à jour des liens de quantité d'approvisionnement
                    foreach (Ingrédient ing in ProgRecette.Sortants.ingrédients)
                    {
                        IEnumerable<Lien> lings = Destinataires.Where(src => src.EPièce == ing.pièce.PType);
                        if (lings != null && lings.Any())
                        {
                            double sumIngQt = Facteur * ing.parMinute;
                            double sumFacLn = lings.Sum(l => l.Facteur);

                            if (sumFacLn > 0.00000001)
                            {
                                foreach (Lien l in lings)
                                {
                                    double facLn = l.Facteur / sumFacLn;
                                    l.Quantité = sumIngQt * facLn;
                                }
                            }
                        }
                    }

                }
            }

            if (Sources != null && Sources.Any())
            {
                foreach (Ingrédient ing in ProgRecette.Entrants.ingrédients)
                {
                    IEnumerable<Lien> lings = Sources.Where(src => src.EPièce == ing.pièce.PType && src.EstLienOptimisable);
                    if (lings != null && lings.Any())
                    {
                        double approSum = lings.Sum(l => l.QuantitéRéel);
                        double besoin = FacteurRéel * ing.parMinute;
                        if(approSum > besoin)//trop d'appro
                        {
                            double limiteTech = ing.pièce.LimiteLogistique;
                            List<(Lien, double)> lingsMarg = lings.Select(l => (l, l.ProducteurSource.MargeDeRéductionSortant(ing.pièce.PType))).ToList();
                            //IEnumerable<Lien> lingsAOpti = lings.Where(l => l.ProducteurSource.NbLienSortant(ing.pièce.PType) > 1);
                            double surplus = approSum - besoin;
                            double sumMargePos = lingsMarg.Sum(l => (l.Item2 > 0.0 ? l.Item2 : 0.0));
                            if (sumMargePos > 0.0)
                            {
                                for (int i = 0; i < 100 && surplus > 0.00001 && lingsMarg.Any(); ++i)
                                {
                                    double resid = 0.0;
                                    List<(Lien, double)> lingsMargNXT = new List<(Lien, double)>();
                                    foreach ((Lien, double) ln in lingsMarg)
                                    {
                                        if (ln.Item2 > 0.0)
                                        {
                                            double surpf = surplus * (ln.Item2 / sumMargePos);
                                            if (surpf >= ln.Item2)
                                            {
                                                resid += surpf - ln.Item2;
                                                ln.Item1.ProducteurSource.Redistribuer(ln.Item1.EPièce, ln.Item2, ln.Item1);
                                                ln.Item1.Facteur -= ln.Item2;
                                            }
                                            else
                                            {
                                                ln.Item1.Facteur -= surpf;
                                                ln.Item1.ProducteurSource.Redistribuer(ln.Item1.EPièce, surpf, ln.Item1);
                                                lingsMargNXT.Add(ln);
                                            }
                                        }
                                    }
                                    surplus = resid;
                                    lingsMarg = lingsMargNXT;
                                }
                            }

                            if(surplus > 0.00001)
                            {
                                sumMargePos = lings.Sum(l => l.Facteur);
                                List<Lien> lingsSP = lings.ToList();
                                for (int i = 0; i < 100 && surplus > 0.00001 && lingsSP.Any(); ++i)
                                {
                                    double resid = 0.0;
                                    List<Lien> lingsMarSPNXT = new List<Lien>();
                                    foreach (Lien ln in lingsSP)
                                    {
                                        double surpf = surplus * (ln.Facteur / sumMargePos);
                                        if(ln.Facteur < surpf)
                                        {
                                            resid += surpf - ln.Facteur;
                                            ln.ProducteurSource.Redistribuer(ln.EPièce, ln.Facteur, ln);
                                            ln.Facteur = 0.0;
                                        }
                                        else
                                        {
                                            ln.ProducteurSource.Redistribuer(ln.EPièce, surpf, ln);
                                            ln.Facteur -= surpf;
                                            lingsMarSPNXT.Add(ln);
                                        }
                                    }
                                    surplus = resid;
                                    lingsSP = lingsMarSPNXT;
                                }
                            }

                            //Recalculer les liens des sources !
                            foreach(Lien ln in lings)
                            {
                                if (ln.ProducteurSource != null)
                                    ln.ProducteurSource.CalculerLiensSortants(ing);
                            }
                            Usine.ReOptimiser(lings.Where(l => l.ProducteurSource.Destinataires != null && l.ProducteurSource.Destinataires.Any()).SelectMany(l => l.ProducteurSource.Destinataires.Select(d => d.ProducteurDestinataire)).ToList());
                        }
                    }
                }
            }
        }*/

        public double Récupérer(Pièce.EPièce epièce, double coef, Lien autre)
        {
            if (Destinataires != null && ProgRecette != null && !ProgRecette.Sortants.EstVide)
            {
                Ingrédient ing = ProgRecette.Sortants.ingrédients.FirstOrDefault(i => i.pièce.PType == epièce);
                if (ing.pièce.PType != Pièce.EPièce.vide)
                {
                    double qtProdSup = (ing.parMinute * FacteurRéel) - Destinataires.Sum(l => l.Facteur);
                    if (qtProdSup > 0.0001) return (qtProdSup * coef);
                    else return 0.0;
                }
                else return 0.0;
            }
            else return 0.0;
        }

        public void Redistribuer(Pièce.EPièce epièce, double qt, Lien autre)
        {
            if(Destinataires != null)
            {
                IEnumerable<Lien> autres = Destinataires.Where(d => d != autre && d.ProducteurDestinataire != null);
                double sumFact = autres.Sum(a => a.Facteur);
                foreach (Lien ln in autres)
                {
                    double qtRe = qt * (ln.Facteur / sumFact);
                    ln.Facteur += qtRe;
                }

                /*double logi = Pièce.ObtenirLimiteLogistique(epièce);
                List<Lien> autres = Destinataires.Where(d => d != autre && d.ProducteurDestinataire != null).ToList();
                for(int i = 0; i < 100 && autres.Any() && qt > 0.00001; ++i)
                {
                    double rest = 0.0;
                    List<Lien> autresNXT = new List<Lien>();
                    double sumFact = autres.Sum(a => a.Facteur);
                    foreach(Lien ln in autres)
                    {
                        double qtRe = qt * (ln.Facteur / sumFact);
                        double logiMac = ln.ProducteurDestinataire.NbMachines * logi;

                        if (ln.Facteur < logiMac)
                        {
                            ln.Facteur += qtRe;
                            if (ln.Facteur > logiMac)
                            {
                                rest += ln.Facteur - logiMac;
                                ln.Facteur = logiMac;
                            }
                            else autresNXT.Add(ln);
                        }
                        else rest += qtRe;
                    }
                    qt = rest;
                    autres = autresNXT;
            }*/
            }
        }

        public void Recalculer()
        {
            IEnumerable<Producteur> producteurs = new List<Producteur>() { this };
            while (producteurs != null && producteurs.Any())
            {
                foreach (Producteur p in producteurs)
                {
                    if (p.Destinataires != null) p.Destinataires.ForEach(l => { l.RéinitialiserCalcul(); l.ProducteurDestinataire.RéinitialiserCalcul(); });
                    p.RéinitialiserCalcul();
                    /*p.FacteurAppro = 0.0;
                    p.FacteurThéo = 0.0;
                    p.Etat &= ~Producteur.EEtat.Est_Calculé;*/
                    p.Calculer();
                }
                producteurs = producteurs.Where(prd => prd.Destinataires != null).SelectMany(prd => prd.Destinataires.Select(d => d.ProducteurDestinataire)).Where(prd => !prd.Etat.HasFlag(Producteur.EEtat.Est_Calculé) && prd.EstApproCalculable).ToList();
            }
        }

        public void Reoptimiser()
        {
            IEnumerable<Producteur> producteurs = new List<Producteur>() { this };
            while (producteurs != null && producteurs.Any())
            {
                foreach (Producteur p in producteurs)
                {
                    if (p.Destinataires != null) p.Destinataires.ForEach(l => { l.RéinitialiserCalcul(); l.ProducteurDestinataire.RéinitialiserCalcul(); });
                    p.RéinitialiserCalcul();
                    /*p.FacteurAppro = 0.0;
                    p.FacteurThéo = 0.0;
                    p.Etat &= ~Producteur.EEtat.Est_Calculé;*/
                    p.Optimiser();
                }
                producteurs = producteurs.Where(prd => prd.Destinataires != null).SelectMany(prd => prd.Destinataires.Select(d => d.ProducteurDestinataire)).Where(prd => !prd.Etat.HasFlag(Producteur.EEtat.Est_Calculé) && prd.EstApproCalculable).ToList();
            }
        }

        public bool EstLienDestReductible(Lien ln)
        {
            return Destinataires != null && Destinataires.Any(l => l != ln && l.EPièce == ln.EPièce);
        }

        /*static public void Calculer(IEnumerable<Producteur> producteurs)
        {
            while (producteurs != null && producteurs.Any())
            {
                foreach (Producteur p in producteurs)
                {
                    if (p.Destinataires != null) p.Destinataires.ForEach(l => { l.Quantité = 0.0; l.ProducteurDestinataire.Etat &= ~EEtat.Est_Calculé;  });
                    p.FacteurAppro = 0.0;
                    p.FacteurRéel = 0.0;
                    p.Etat &= ~Producteur.EEtat.Est_Calculé;
                    p.Calculer();
                }
                producteurs = producteurs.Where(prd => prd.Destinataires != null).SelectMany(prd => prd.Destinataires.Select(d => d.ProducteurDestinataire)).Where(prd => !prd.Etat.HasFlag(Producteur.EEtat.Est_Calculé) && prd.EstApproCalculable).ToList();
            }
        }*/


    }
}
