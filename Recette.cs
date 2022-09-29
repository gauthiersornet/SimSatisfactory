using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimSatisfactory
{
    public struct Ingrédient
    {
        static public readonly int NbNumSignif = 3;
        static public readonly int nbChiffreCar = 6;

        public Pièce pièce;
        public double parMinute; // unité pour les éléments et m3 pour les fluides

        public Ingrédient(Pièce.EPièce epièce, double parMinute) { this.pièce = new Pièce(epièce); this.parMinute = parMinute; }
        public Ingrédient(Pièce pièce, double parMinute) { this.pièce = pièce; this.parMinute = parMinute; }

        public Ingrédient(XmlNode nd)
        {
            pièce = new Pièce((Pièce.EPièce)Enum.Parse(typeof(Pièce.EPièce), nd.Attributes.GetNamedItem("pièce").Value));
            parMinute = double.Parse(nd.Attributes.GetNamedItem("parMinute").Value);
        }

        public override string ToString()
        {
            return $"{pièce}({Math.Round(parMinute, 3)})";
        }

        static public string TaillerNum(string numStr, int nbCar)
        {
            if (numStr.Length > nbCar)
            {
                int virg = numStr.IndexOf(',');
                if (0 <= virg && virg < nbCar) return numStr.Substring(0, nbCar);
                else return numStr.Substring(0, nbCar - 1) + '!';
            }
            else return numStr;
        }

        static public string TaillerNum(double num, int nbCar)
        {
            return TaillerNum(Math.Round(num, NbNumSignif).ToString(), nbCar);
        }

        public void DessinerEntrant(Graphics g, PointF p, double Facteur, Pièce.EPièce PièceLimit)
        {
            //Pen pn = new Pen(Color.Black);
            int lig = ((int)pièce.PType) / Pièce.Img_NbElm_Colonne;
            int col = ((int)pièce.PType) % Pièce.Img_NbElm_Colonne;
            g.DrawImage(Recette.PiècesImages, new RectangleF(p.X - 25.0f, p.Y - 25.0f, 50.0f, 50.0f), new RectangleF(col * 50.0f, lig * 50.0f, 50.0f, 50.0f), GraphicsUnit.Pixel);
            if(pièce.PType == PièceLimit)
            {
                Pen pn = new Pen(Color.Red, 2.0f);
                g.DrawRectangle(pn, p.X - 25.0f, p.Y - 25.0f, 50.0f, 50.0f);
            }

            //g.DrawRectangle(pn, p.X - 10.0f, p.Y - 10.0f, 20.0f, 20.0f);
            Brush brsh = new SolidBrush(Color.Black);
            g.DrawString(TaillerNum(parMinute, nbChiffreCar).ToString(), new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, p.X - 25.0f, p.Y + 24.0f);
            g.DrawString("A" + TaillerNum(Facteur * parMinute, nbChiffreCar).ToString(), new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, p.X - 25.0f, p.Y - 39.0f);
        }

        public void DessinerSortant(Graphics g, PointF p, double Facteur, double Réel)
        {
            //Pen pn = new Pen(Color.Black);
            int lig = ((int)pièce.PType) / Pièce.Img_NbElm_Colonne;
            int col = ((int)pièce.PType) % Pièce.Img_NbElm_Colonne;
            g.DrawImage(Recette.PiècesImages, new RectangleF(p.X - 25.0f, p.Y - 25.0f, 50.0f, 50.0f), new RectangleF(col * 50.0f, lig *50.0f, 50.0f, 50.0f), GraphicsUnit.Pixel);

            //g.DrawRectangle(pn, p.X - 10.0f, p.Y - 10.0f, 20.0f, 20.0f);
            Brush brsh = new SolidBrush(Color.Black);
            g.DrawString(TaillerNum(parMinute, nbChiffreCar).ToString(), new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, p.X - 25.0f, p.Y + 24.0f);
            g.DrawString("A" + TaillerNum(Facteur * parMinute, nbChiffreCar).ToString(), new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, p.X - 25.0f, p.Y + 34.0f);
            g.DrawString("R" + TaillerNum(Réel * parMinute, nbChiffreCar).ToString(), new Font(FontFamily.GenericMonospace, 10.0f, FontStyle.Bold), brsh, p.X - 25.0f, p.Y + 44.0f);
        }

        static public void Dessinner(Graphics g, PointF p, Pièce.EPièce pièce)
        {
            int lig = ((int)pièce) / Pièce.Img_NbElm_Colonne;
            int col = ((int)pièce) % Pièce.Img_NbElm_Colonne;
            g.DrawImage(Recette.PiècesImages, new RectangleF(p.X - 25.0f, p.Y - 25.0f, 50.0f, 50.0f), new RectangleF(col * 50.0f, lig * 50.0f, 50.0f, 50.0f), GraphicsUnit.Pixel);
        }

        public XmlNode VersXMLDom(XmlDocument doc)
        {
            XmlNode res = doc.CreateNode(XmlNodeType.Element, "Ingrédient", "");
            XmlAttribute att;
            att = doc.CreateAttribute("pièce"); att.Value = pièce.ToString(); res.Attributes.Append(att);
            att = doc.CreateAttribute("parMinute"); att.Value = parMinute.ToString(); res.Attributes.Append(att);
            return res;
        }
    }

    public struct Ingrédients 
    {
        static public readonly float Espassement = 15.0f;

        public Ingrédient[] ingrédients;
        public bool EstVide { get => ingrédients == null || !ingrédients.Any(); }

        public Ingrédients(Ingrédient[] ingrédients) { this.ingrédients = ingrédients; }

        public Ingrédients(XmlNode ndIngs)
        {
            List<Ingrédient> ings = new List<Ingrédient>();
            foreach (XmlNode nd in ndIngs.ChildNodes) ings.Add(new Ingrédient(nd));
            ingrédients = ings.ToArray();
        }

        public bool ContientPièce(Pièce.EPièce pièce)
        {
            return (ingrédients != null && ingrédients.Any(i => i.pièce.PType == pièce));
        }

        public void DessinerEntrant(Graphics g, PointF p, double Facteur, Pièce.EPièce PièceLimit)
        {
            if (ingrédients != null)
            {
                p.X -= ((ingrédients.Length - 1) * (50.0f + Espassement)) / 2.0f;
                for (int i = 0; i < ingrédients.Length; ++i)
                {
                    ingrédients[i].DessinerEntrant(g, p, Facteur, PièceLimit);
                    p.X += (50.0f + Espassement);
                }
            }
        }

        public void DessinerSortant(Graphics g, PointF p, double Facteur, double Réel)
        {
            if (ingrédients != null)
            {
                p.X -= ((ingrédients.Length - 1) * (50.0f + Espassement)) / 2.0f;
                for (int i = 0; i < ingrédients.Length; ++i)
                {
                    ingrédients[i].DessinerSortant(g, p, Facteur, Réel);
                    p.X += (50.0f + Espassement);
                }
            }
        }

        public Pièce.EPièce Selection(PointF p)
        {
            if (ingrédients != null)
            {
                p.X += ((ingrédients.Length - 1) * (50.0f + Espassement)) / 2.0f;
                for (int i = 0; i < ingrédients.Length; ++i)
                {
                    //ingrédients[i].Dessiner(g, p);
                    if (-25.0f <= p.X && p.X <= 25.0f) return ingrédients[i].pièce.PType;
                    p.X -= (50.0f + Espassement);
                }
                return Pièce.EPièce.vide;
            }
            else return Pièce.EPièce.vide;
        }

        public XmlNode VersXMLDom(XmlDocument doc, string nom)
        {
            XmlNode res = doc.CreateNode(XmlNodeType.Element, nom, "");
            foreach (Ingrédient ing in ingrédients) res.AppendChild(ing.VersXMLDom(doc));
            return res;
        }

        public override int GetHashCode()
        {
            return ingrédients.Sum(x => x.pièce.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj)) return true;
            else if (obj is Ingrédients)
            {
                Ingrédients other = (Ingrédients)obj;
                IEnumerable<Pièce> ingres = other.ingrédients.Select(x => x.pièce);
                return (other.ingrédients.Length == ingrédients.Length && ingrédients.All(x => ingres.Contains(x.pièce)));
            }
            else return false;
        }

        public override string ToString()
        {
            return (ingrédients != null && ingrédients.Any() ? "(" + String.Join(",", ingrédients) + ")" : "");
        }
    }

    public class Recette
    {
        public enum EProducteur
        {
            Vide = 0,
            Centrale_Biomasse = 1,
            Centrale_Charbon = 2,
            Centrale_Carburant = 3,
            Centrale_Nucléaire = 4,
            Foreuse = 5,
            Fonderie = 6,
            Fonderie_Avancée = 7,
            Pompe_Eau = 8,
            Pompe_Pétrole = 9,
            Rafinerie = 10,
            Packageur = 11,
            Pompe_Azote = 12,
            Constructeur = 13,
            Assembleuse = 14,
            Façonneuse = 15,
            Mélangeur = 16,
            Conteneur = 17,
            Accélérateur_de_particules = 18
        }

        static public Image PiècesImages = Bitmap.FromFile("Pieces.png");

        /*static private double[] ConsoForeuse = new double[] { 0.0, -5.0, -12.0, -30.0 };
        static private Recette[] InitForeuse(double minerai_pm, Pièce.EPièce minerai)
        {
            Recette[] rct = new Recette[4];
            rct[0] = new Recette(ConsoForeuse[0], new Ingrédient[] { }, // ->
                                 new Ingrédient[] { });
            for (int i = 1; i < rct.Length; ++i)
                rct[i] = new Recette(ConsoForeuse[i],
                                        new Ingrédient[] { }, // ->
                                        new Ingrédient[] { new Ingrédient(new Pièce(minerai), (1 << (i - 1)) * minerai_pm) });
            return rct;
        }

        static public readonly Recette[] ForeuseFer_Impure = InitForeuse(30.0, Pièce.EPièce.minerai_fer);
        static public readonly Recette[] ForeuseFer_Normale = InitForeuse(60.0, Pièce.EPièce.minerai_fer);
        static public readonly Recette[] ForeuseFer_Pure = InitForeuse(120.0, Pièce.EPièce.minerai_fer);

        static public readonly Recette[] ForeuseCuivre_Impure = InitForeuse(30.0, Pièce.EPièce.minerai_cuivre);
        static public readonly Recette[] ForeuseCuivre_Normale = InitForeuse(60.0, Pièce.EPièce.minerai_cuivre);
        static public readonly Recette[] ForeuseCuivre_Pure = InitForeuse(120.0, Pièce.EPièce.minerai_cuivre);*/

        static public Recette[] Extracteurs = new Recette[]
        {
            new Recette(EProducteur.Foreuse, -30/240.0, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.minerai_fer), 1) }),
            new Recette(EProducteur.Foreuse, -30/240.0, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.minerai_cuivre), 1) }),
            new Recette(EProducteur.Foreuse, -30/240.0, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.minerai_calcaire), 1) }),
            new Recette(EProducteur.Foreuse, -30/240.0, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.minerai_charbon), 1) }),
            new Recette(EProducteur.Foreuse, -30/240.0, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.minerai_catérium), 1) }),
            new Recette(EProducteur.Foreuse, -30/240.0, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.minerai_soufre), 1) }),
            new Recette(EProducteur.Foreuse, -30/240.0, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.minerai_quartz), 1) }),
            new Recette(EProducteur.Foreuse, -30/240.0, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.minerai_bauxite), 1) }),
            new Recette(EProducteur.Foreuse, -30/240.0, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.minerai_uranium), 1) }),
            new Recette(EProducteur.Pompe_Pétrole, -40/120.0, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.fluide_pétrole_brut), 1) }),//Normale = 120.0
            new Recette(EProducteur.Pompe_Azote, -150, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.fluide_azote), 1200) }),
            new Recette(EProducteur.Pompe_Eau ,- 20, new Ingrédient[] { }, new Ingrédient[] { new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 120.0) })
        };

        static public Recette[] Centrales = new Recette[]
        {
            new Recette(EProducteur.Centrale_Biomasse, 30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bio_carburant_solide), 4.0) }, // ->
                           new Ingrédient[]{  }),
            new Recette(EProducteur.Centrale_Charbon, 75, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_charbon), 15.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 45.0) }, // ->
                           new Ingrédient[]{  }),
            new Recette(EProducteur.Centrale_Carburant, 150, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_carburant), 12.0) }, // ->
                           new Ingrédient[]{  }),
        };

        //4 Mw
        static public Recette[] Fonderies =
        {
            new Recette(EProducteur.Fonderie, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_fer), 30.0) }, // ->
                           new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_fer), 30.0) }),
            new Recette(EProducteur.Fonderie, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_cuivre), 30.0) }, // ->
                           new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_cuivre), 30.0) }),
            new Recette(EProducteur.Fonderie, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_catérium), 45.0) }, // ->
                           new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_catérium), 15.0) }),
        };

        //16 Mw
        static public Recette[] Fonderies_avancées =
        {
            new Recette(EProducteur.Fonderie_Avancée, -16, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_fer), 45.0), new Ingrédient(new Pièce(Pièce.EPièce.minerai_charbon), 45.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_acier), 45.0) }),
            new Recette(EProducteur.Fonderie_Avancée, -16, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.rebus_aluminium), 90.0), new Ingrédient(new Pièce(Pièce.EPièce.silice), 75.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_aluminium), 60.0) }),
        };

        //4 Mw
        static public Recette[] Constructeurs =
        {
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_fer), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plaque_fer), 20.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_fer), 15.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tige_fer), 15.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tige_fer), 10.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.vis), 40.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_fer), 12.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.vis), 50.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_cuivre), 20.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tôle_cuivre), 10.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_cuivre), 15.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fil_électrique), 30.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fil_électrique), 60.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.câble), 30.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_catérium), 12.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.filactif), 60.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_calcaire), 45.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.béton), 15.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_quartz), 37.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cristal_quartz), 22.5) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_quartz), 22.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.silice), 37.5) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_acier), 60.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.poutre_acier), 15.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_acier), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tuyau_acier), 20.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.feuille), 120.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.biomasse), 60.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bois), 60.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.biomasse), 300.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.carapasse), 15.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.biomasse), 1500.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.organe), 7.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.biomasse), 1500.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.mycélium), 150.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.biomasse), 150.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.biomasse), 120.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bio_carburant_solide), 60.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_aluminium), 90.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.boitier_aluminium), 60.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tige_fer), 15.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.barre_pointe), 15.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.pétales), 37.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cartouche), 75.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plastique), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 60.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_aluminium), 60.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.réservoir), 60.0) }),
            new Recette(EProducteur.Constructeur, -4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_cuivre), 300.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.poudre_de_cuivre), 50.0) }),
            /*
            new Recette(-4, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.vide), .0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.vide), .0) }),
            */
        };

        //15 Mw
        static public Recette[] Assembleuses =
        {
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plaque_fer), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.vis), 60.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plaque_fer_renforcée), 5.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plaque_fer), 10.0), new Ingrédient(new Pièce(Pièce.EPièce.fil_électrique), 20.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plaque_fer_renforcée), 5.625) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plaque_fer_renforcée), 3.0), new Ingrédient(new Pièce(Pièce.EPièce.tige_fer), 12.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cadre_modulaire), 2.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.poutre_acier), 24.0), new Ingrédient(new Pièce(Pièce.EPièce.béton), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.poutre_béton_armé), 6.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tuyau_acier), 28.0), new Ingrédient(new Pièce(Pièce.EPièce.béton), 20.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.poutre_béton_armé), 4.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_aluminium), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.lingo_cuivre), 10.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tôle_aluminium), 30.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tige_fer), 20.0), new Ingrédient(new Pièce(Pièce.EPièce.vis), 100.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.rotor), 4.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tôle_cuivre), 22.5), new Ingrédient(new Pièce(Pièce.EPièce.vis), 195.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.rotor), 11.25) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tuyau_acier), 10.0), new Ingrédient(new Pièce(Pièce.EPièce.fil_électrique), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.rotor), 5.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tuyau_acier), 15.0), new Ingrédient(new Pièce(Pièce.EPièce.fil_électrique), 40.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.stator), 5.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.rotor), 10.0), new Ingrédient(new Pièce(Pièce.EPièce.stator), 10.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.moteur), 5.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tôle_aluminium), 37.5), new Ingrédient(new Pièce(Pièce.EPièce.tôle_cuivre), 22.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.dissipateur), 7.5) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plaque_fer_renforcée), 2.0), new Ingrédient(new Pièce(Pièce.EPièce.rotor), 2.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.placage_intélligent), 2.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cadre_modulaire), 2.5), new Ingrédient(new Pièce(Pièce.EPièce.poutre_acier), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.structure_polyvalente), 5.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.stator), 2.5), new Ingrédient(new Pièce(Pièce.EPièce.câble), 50.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.câblage_automatisé), 2.5) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.unité_contrôle_adaptative), 1.5), new Ingrédient(new Pièce(Pièce.EPièce.supercalculateur), 0.75) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.système_directeur_assemblage), 0.75) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tôle_cuivre), 25.0), new Ingrédient(new Pièce(Pièce.EPièce.filactif), 100.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.contrôleur_ia), 5.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tôle_cuivre), 15.0), new Ingrédient(new Pièce(Pièce.EPièce.plastique), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.circuit_imprimé), 7.5) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plastique), 12.5), new Ingrédient(new Pièce(Pièce.EPièce.filactif), 37.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.circuit_imprimé), 8.75) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.circuit_imprimé), 7.5), new Ingrédient(new Pièce(Pièce.EPièce.oscillateur_cristal), 2.813) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.ordinateur), 2.813) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_charbon), 7.5), new Ingrédient(new Pièce(Pièce.EPièce.minerai_soufre), 15.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.poudre_noire), 7.5) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.poudre_noire), 15.0), new Ingrédient(new Pièce(Pièce.EPièce.tuyau_acier), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.nobelisk), 3.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.mycélium), 15.0), new Ingrédient(new Pièce(Pièce.EPièce.biomasse), 75.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tissu), 15.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.stator), 6.0), new Ingrédient(new Pièce(Pièce.EPièce.contrôleur_ia), 4.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tige_contrôle_électromagnétique), 4.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.stator), 8.0), new Ingrédient(new Pièce(Pièce.EPièce.connecteur_haute_vitesse), 4.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.tige_contrôle_électromagnétique), 8.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cadre_modulaire_fusionné), 1.0), new Ingrédient(new Pièce(Pièce.EPièce.unité_contrôle_radio), 2.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cube_conversion_pression), 1.0) }),
            new Recette(EProducteur.Assembleuse, -15, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.granulé_de_plutonium), 10.0), new Ingrédient(new Pièce(Pièce.EPièce.béton), 20.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cellule_de_plutonium), 5.0) }),
        };

        //55 Mw
        static public Recette[] Façonneuses =
        {
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plaque_fer), 22.5), new Ingrédient(new Pièce(Pièce.EPièce.tige_fer), 7.5), new Ingrédient(new Pièce(Pièce.EPièce.fil_électrique), 112.5), new Ingrédient(new Pièce(Pièce.EPièce.câble), 15.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.balise), 7.5) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cadre_modulaire), 10.0), new Ingrédient(new Pièce(Pièce.EPièce.tuyau_acier), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.poutre_béton_armé), 10.0), new Ingrédient(new Pièce(Pièce.EPièce.vis), 200.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cadre_modulaire_lourd), 2.0) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cadre_modulaire), 18.75), new Ingrédient(new Pièce(Pièce.EPièce.poutre_béton_armé), 11.25), new Ingrédient(new Pièce(Pièce.EPièce.caoutchouc), 75.0), new Ingrédient(new Pièce(Pièce.EPièce.vis), 390.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cadre_modulaire_lourd), 3.75) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.circuit_imprimé), 25.0), new Ingrédient(new Pièce(Pièce.EPièce.câble), 22.5), new Ingrédient(new Pièce(Pièce.EPièce.plastique), 45.0), new Ingrédient(new Pièce(Pièce.EPièce.vis), 130.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.ordinateur), 2.5) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.ordinateur), 3.75), new Ingrédient(new Pièce(Pièce.EPièce.contrôleur_ia), 3.75), new Ingrédient(new Pièce(Pièce.EPièce.connecteur_haute_vitesse), 5.625), new Ingrédient(new Pièce(Pièce.EPièce.plastique), 52.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.supercalculateur), 1.875) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cristal_quartz), 18.0), new Ingrédient(new Pièce(Pièce.EPièce.câble), 14.0), new Ingrédient(new Pièce(Pièce.EPièce.plaque_fer_renforcée), 2.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.oscillateur_cristal), 1.0) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.boitier_aluminium), 40.0), new Ingrédient(new Pièce(Pièce.EPièce.oscillateur_cristal), 1.25), new Ingrédient(new Pièce(Pièce.EPièce.ordinateur), 1.25) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.unité_contrôle_radio), 2.5) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.moteur), 2.0), new Ingrédient(new Pièce(Pièce.EPièce.caoutchouc), 15.0), new Ingrédient(new Pièce(Pièce.EPièce.placage_intélligent), 2.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.moteur_modulaire), 1.0) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.câblage_automatisé), 7.5), new Ingrédient(new Pièce(Pièce.EPièce.circuit_imprimé), 5.0), new Ingrédient(new Pièce(Pièce.EPièce.cadre_modulaire_lourd), 1.0), new Ingrédient(new Pièce(Pièce.EPièce.ordinateur), 1.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.unité_contrôle_adaptative), 1.0) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.structure_polyvalente), 2.5), new Ingrédient(new Pièce(Pièce.EPièce.tige_contrôle_électromagnétique), 1.0), new Ingrédient(new Pièce(Pièce.EPièce.batterie), 5.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.générateur_champ_magnétique), 1.0) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.moteur_modulaire), 2.5), new Ingrédient(new Pièce(Pièce.EPièce.moteur_turbo), 1.0), new Ingrédient(new Pièce(Pièce.EPièce.système_refroidissement), 3.0), new Ingrédient(new Pièce(Pièce.EPièce.cadre_modulaire_lourd), 1.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fusée_propulsion_thermique), 1.0) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_charbon), 37.5), new Ingrédient(new Pièce(Pièce.EPièce.caoutchouc), 15.0), new Ingrédient(new Pièce(Pièce.EPièce.tissu), 15.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.filtre_gaz), 7.5) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.filtre_gaz), 3.75), new Ingrédient(new Pièce(Pièce.EPièce.filactif), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.boitier_aluminium), 3.375) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.filtre_infusion_iode), 3.75) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.balise), 3.0), new Ingrédient(new Pièce(Pièce.EPièce.tuyau_acier), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.poudre_noire), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.caoutchouc), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cartouche_fusil), 15.0) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.filactif), 210.0), new Ingrédient(new Pièce(Pièce.EPièce.câble), 37.5), new Ingrédient(new Pièce(Pièce.EPièce.circuit_imprimé), 3.75) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.connecteur_haute_vitesse), 3.75) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.système_refroidissement), 7.5), new Ingrédient(new Pièce(Pièce.EPièce.unité_contrôle_radio), 3.75), new Ingrédient(new Pièce(Pièce.EPièce.moteur), 7.5), new Ingrédient(new Pièce(Pièce.EPièce.caoutchouc), 45.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.moteur_turbo), 1.875) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cellule_uranium), 20.0), new Ingrédient(new Pièce(Pièce.EPièce.poutre_béton_armé), 1.2), new Ingrédient(new Pièce(Pièce.EPièce.tige_contrôle_électromagnétique), 2.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.barre_combustible_uranium), 0.4) }),
            new Recette(EProducteur.Façonneuse, -55, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cellule_de_plutonium), 7.5), new Ingrédient(new Pièce(Pièce.EPièce.poutre_acier), 4.5), new Ingrédient(new Pièce(Pièce.EPièce.tige_contrôle_électromagnétique), 1.5), new Ingrédient(new Pièce(Pièce.EPièce.dissipateur), 2.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.barre_de_plutonium), 0.25) }),
        };

        //30 Mw
        static public Recette[] Rafineries =
        {
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_pétrole_brut), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plastique), 20.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_résidu_pétrole_lourd), 10.0) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.résine_polymère), 60.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 20.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.plastique), 20.0) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_pétrole_brut), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.caoutchouc), 20.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_résidu_pétrole_lourd), 20.0) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.résine_polymère), 40.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 40.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.caoutchouc), 20.0) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_pétrole_brut), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_résidu_pétrole_lourd), 40.0), new Ingrédient(new Pièce(Pièce.EPièce.résine_polymère), 20.0) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_résidu_pétrole_lourd), 40.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.coke_pétrole), 120.0) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_pétrole_brut), 60.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_carburant), 40.0), new Ingrédient(new Pièce(Pièce.EPièce.résine_polymère), 30.0) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_résidu_pétrole_lourd), 60.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_carburant), 40.0) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bio_carburant_solide), 90.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 45.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_biocarburant), 60.0) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_cuivre), 15.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 10.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.lingo_cuivre), 37.5) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_bauxite), 120.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 180.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_alumine), 120.0), new Ingrédient(new Pièce(Pièce.EPièce.silice), 50.0) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_alumine), 240.0), new Ingrédient(new Pièce(Pièce.EPièce.minerai_charbon), 120.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.rebus_aluminium), 360.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 120.0) }),
            new Recette(EProducteur.Rafinerie, -30, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_soufre), 50.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 50.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_sulfurique), 50.0) }),
        };

        //10 Mw
        static public readonly Recette[] Packageurs =
        {
            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 60.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 60.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_eau), 60.0) }),
            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_eau), 60.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 60.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 60.0) }),

            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_pétrole_brut), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_pétrole), 30.0) }),
            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_pétrole), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_pétrole_brut), 30.0) }),

            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_résidu_pétrole_lourd), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_résidu_pétrole_lourd), 30.0) }),
            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_résidu_pétrole_lourd), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_résidu_pétrole_lourd), 30.0) }),

            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 40.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_biocarburant), 40.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_biocarburant), 40.0) }),
            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_biocarburant), 40.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 40.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_biocarburant), 40.0) }),

            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 120.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_alumine), 120.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_alumine), 120.0) }),
            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_alumine), 120.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 120.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_alumine), 120.0) }),

            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 40.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_sulfurique), 40.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_acide_sulfurique), 40.0) }),
            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_acide_sulfurique), 40.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 40.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_sulfurique), 40.0) }),

            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.réservoir), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_nitrique), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.réservoir_acide_nitrique), 30.0) }),
            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.réservoir_acide_nitrique), 30.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_nitrique), 30.0) }),

            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 40.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_carburant), 40.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_carburant), 40.0) }),
            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon_carburant), 40.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.bidon), 40.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_carburant), 40.0) }),

            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.réservoir), 60.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_azote), 240.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.réservoir_azote), 60.0) }),
            new Recette(EProducteur.Packageur, -10, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.réservoir_azote), 60.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.réservoir), 60.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_azote), 240.0) }),
        };

        //75 Mw
        static public Recette[] Mélangeurs =
        {
            new Recette(EProducteur.Mélangeur, -75, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cadre_modulaire_lourd), 1.5), new Ingrédient(new Pièce(Pièce.EPièce.boitier_aluminium), 75.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_azote), 37.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cadre_modulaire_fusionné), 1.5) }),
            new Recette(EProducteur.Mélangeur, -75, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.dissipateur), 12.0), new Ingrédient(new Pièce(Pièce.EPièce.caoutchouc), 12.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_azote), 150.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.système_refroidissement), 6.0) }),
            new Recette(EProducteur.Mélangeur, -75, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_sulfurique), 50.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_alumine), 40.0), new Ingrédient(new Pièce(Pièce.EPièce.boitier_aluminium), 20.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.batterie), 20.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 30.0) }),
            new Recette(EProducteur.Mélangeur, -75, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_uranium), 50.0), new Ingrédient(new Pièce(Pièce.EPièce.béton), 15.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_sulfurique), 40.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.cellule_uranium), 25.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_sulfurique), 10.0) }),
            new Recette(EProducteur.Mélangeur, -75, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.déchets_uranium), 37.5), new Ingrédient(new Pièce(Pièce.EPièce.silice), 25.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_nitrique), 15), new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_sulfurique), 15) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.uranium_non_fissile), 50.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 15.0) }),
            new Recette(EProducteur.Mélangeur, -75, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.minerai_bauxite), 150.0), new Ingrédient(new Pièce(Pièce.EPièce.minerai_charbon), 100.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_sulfurique), 50), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 60) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.rebus_aluminium), 300.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 50.0) }),
            new Recette(EProducteur.Mélangeur, -75, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_azote), 120.0), new Ingrédient(new Pièce(Pièce.EPièce.fluide_eau), 30.0), new Ingrédient(new Pièce(Pièce.EPièce.plaque_fer), 10) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.fluide_acide_nitrique), 30.0) }),
        };

        //1000 Mw
        static public Recette[] Accélérateur_de_particules =
        {
            new Recette(EProducteur.Accélérateur_de_particules, -500, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.uranium_non_fissile), 100.0), new Ingrédient(new Pièce(Pièce.EPièce.déchets_uranium), 25.0) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.granulé_de_plutonium), 30.0) }),
            new Recette(EProducteur.Accélérateur_de_particules, -1000, new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.poudre_de_cuivre), 100.0), new Ingrédient(new Pièce(Pièce.EPièce.cube_conversion_pression), 0.5) }, // ->
                        new Ingrédient[]{ new Ingrédient(new Pièce(Pièce.EPièce.pâte_nucléaire), 0.5) })
        };

        static public Recette[][] Recettes = new Recette[][]{
            Extracteurs,
            Centrales,
            Fonderies,
            Fonderies_avancées,
            Constructeurs,
            Assembleuses,
            Façonneuses,
            Rafineries,
            Packageurs,
            Mélangeurs,
            Accélérateur_de_particules
        };

        public XmlNode VersXMLDom(XmlDocument doc)
        {
            XmlNode res = doc.CreateNode(XmlNodeType.Element, "Recette", "");
            XmlAttribute att;
            att = doc.CreateAttribute("TypeProducteur"); att.Value = TypeProducteur.ToString(); res.Attributes.Append(att);
            att = doc.CreateAttribute("énergie"); att.Value = énergie.ToString(); res.Attributes.Append(att);
            res.AppendChild(Entrants.VersXMLDom(doc, "Entrants"));
            res.AppendChild(Sortants.VersXMLDom(doc, "Sortants"));
            return res;
        }

        static public void RecettesVersXML(string fichier, Recette[] recets)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode racine = doc.CreateNode(XmlNodeType.Element, "Recettes", "");
            doc.AppendChild(racine);
            if (recets != null && recets.Any())
            {
                foreach (Recette rct in recets)
                    racine.AppendChild(rct.VersXMLDom(doc));
            }
            doc.Save(fichier);
        }

        static public Recette[] RecettesDepuisXML(string fichier)
        {
            XmlDocument doc = new XmlDocument();
            try { doc.Load(fichier); }
            catch (System.IO.FileNotFoundException ex)
            {
                return null;
            }
            catch (System.Xml.XmlException ex)
            {
                return null;
            }

            if (doc.ChildNodes != null && doc.ChildNodes.Count == 1)
            {
                XmlNode root = doc.ChildNodes.Item(0);
                if (root != null)
                {
                    List<Recette> rcts = new List<Recette>();
                    foreach (XmlNode nd in root.ChildNodes)
                        rcts.Add(new Recette(nd));
                    return rcts.ToArray();
                }
                else return null;
            }
            else return null;
        }

        /*static readonly public Recette[][] RecettesAll = new Recette[][]{
            Extracteurs,
            Centrales,
            Fonderies,
            Fonderies_avancées,
            Constructeurs,
            Assembleuses,
            Façonneuses,
            Rafineries,
            Packageurs,
            Mélangeurs
        };*/

        /*static private Dictionary<Ingrédients, Recette> calculer_EntrantRecette()
        {
            Dictionary<Ingrédients, Recette> dico = new Dictionary<Ingrédients, Recette>();

            foreach (Recette[] recettes in Recettes)
            {
                foreach (Recette rct in recettes)
                    dico.Add(rct.Entrants, rct);
            }

            return dico;
        }*/

        /*static private Dictionary<Ingrédients, Recette> calculer_SortantRecette()
        {
            Dictionary<Ingrédients, Recette> dico = new Dictionary<Ingrédients, Recette>();

            foreach (Recette[] recettes in Recettes)
            {
                foreach (Recette rct in recettes)
                    dico.Add(rct.Sortants, rct);
            }

            return dico;
        }*/

        //static public readonly Dictionary<Ingrédients, Recette> EntrantRecette = calculer_EntrantRecette();
        //static public readonly Dictionary<Ingrédients, Recette> SortantRecette = calculer_SortantRecette();

        static public Recette Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            else
            {
                Recette rct;
                if(ParseStrRecette.TryGetValue(s, out rct)) return rct;
                else
                {
                    int idx = s.IndexOf(">Conteneur>");
                    if (idx >= 0)
                    {
                        idx += ">Conteneur>".Length;
                        s = s.Substring(idx);
                        //if (s.StartsWith("(")) s = s.Substring(1);
                        if (s.Length <= 4) return null;
                        idx = s.LastIndexOf('(');
                        if (idx < 0) idx = s.Length;
                        if (s.StartsWith("(")) s = s.Substring(1, idx-1);
                        else s = s.Substring(0, idx);
                        Pièce.EPièce ep = (Pièce.EPièce)Enum.Parse(typeof(Pièce.EPièce), s);
                        if (ep != Pièce.EPièce.vide)
                            return new Recette(EProducteur.Conteneur, 0.0, new Ingrédient[0], new Ingrédient[] { new Ingrédient(ep, 1.0) });
                        else return null;
                    }
                    else return null;
                }
            }
        }

        static private Dictionary<Pièce.EPièce, List<Recette>> calculer_EPièceEntrantRecette()
        {
            Dictionary<Pièce.EPièce, List<Recette>> dico = new Dictionary<Pièce.EPièce, List<Recette>>();

            foreach (Recette[] recettes in Recettes)
            {
                foreach (Recette rct in recettes)
                {
                    foreach(Ingrédient ing in rct.Entrants.ingrédients)
                    {
                        List<Recette> lstRct;
                        if(!dico.TryGetValue(ing.pièce.PType, out lstRct))
                        {
                            lstRct = new List<Recette>();
                            dico.Add(ing.pièce.PType, lstRct);
                        }
                        if(!lstRct.Contains(rct)) lstRct.Add(rct);
                    }
                }
            }

            return dico;
        }

        static private Dictionary<Pièce.EPièce, List<Recette>> calculer_EPièceSortantRecette()
        {
            Dictionary<Pièce.EPièce, List<Recette>> dico = new Dictionary<Pièce.EPièce, List<Recette>>();

            foreach (Recette[] recettes in Recettes)
            {
                foreach (Recette rct in recettes)
                {
                    foreach (Ingrédient ing in rct.Sortants.ingrédients)
                    {
                        List<Recette> lstRct;
                        if (!dico.TryGetValue(ing.pièce.PType, out lstRct))
                        {
                            lstRct = new List<Recette>();
                            dico.Add(ing.pièce.PType, lstRct);
                        }
                        lstRct.Add(rct);
                    }
                }
            }

            return dico;
        }

        static public void InitialiserRecettes()
        {
            /*foreach(object tprod in Enum.GetValues(typeof(EProducteur)))
            {

            }*/
            for(int i = 0; i < Recettes.Length; ++i)
            {
                if (Recettes[i].Any())
                {
                    string nomProd = Recettes[i].First().TypeProducteur.ToString() + ".xml";
                    if (File.Exists(nomProd))
                    {
                        Recette[] rcts = RecettesDepuisXML(nomProd);
                        if (rcts != null) Recettes[i] = rcts;
                    }
                    else RecettesVersXML(nomProd, Recettes[i]);
                }
            }

            EPièceEntrantRecette = calculer_EPièceEntrantRecette();
            EPièceSortantRecette = calculer_EPièceSortantRecette();
            ParseStrRecette = Recettes.SelectMany(x => x).ToDictionary(x => x.ToString());
        }

        static public Dictionary<Pièce.EPièce, List<Recette>> EPièceEntrantRecette = new Dictionary<Pièce.EPièce, List<Recette>>();
        static public Dictionary<Pièce.EPièce, List<Recette>> EPièceSortantRecette = new Dictionary<Pièce.EPièce, List<Recette>>();
        static public Dictionary<string, Recette> ParseStrRecette = new Dictionary<string, Recette>();

        static public readonly SizeF Espassement = new SizeF(5.0f, 25.0f);
        static public readonly float EspaceEntrant = 20.0f;

        public EProducteur TypeProducteur;
        public double énergie;
        public Ingrédients Entrants;
        public Ingrédients Sortants;

        public HashSet<Pièce.EPièce> EnssembleEntrantsEPièces
        { 
            get
            {
                return new HashSet<Pièce.EPièce>(Entrants.ingrédients.Select(i => i.pièce.PType));
            }
        }

        public Recette() { }
        public Recette(EProducteur typeProducteur, double énergie, Ingrédients entrants, Ingrédients sortants) { this.TypeProducteur = typeProducteur; this.énergie = énergie; this.Entrants = entrants; this.Sortants = sortants; }
        public Recette(EProducteur typeProducteur, double énergie, Ingrédient[] entrants, Ingrédient[] sortants) { this.TypeProducteur = typeProducteur; this.énergie = énergie; this.Entrants = new Ingrédients(entrants); this.Sortants = new Ingrédients(sortants); }

        public Recette(XmlNode nd)
        {
            TypeProducteur = (EProducteur)Enum.Parse(typeof(EProducteur), nd.Attributes.GetNamedItem("TypeProducteur").Value);
            énergie = double.Parse(nd.Attributes.GetNamedItem("énergie").Value);
            Entrants = new Ingrédients();
            Sortants = new Ingrédients();
            foreach(XmlNode cnd in nd.ChildNodes)
            {
                if(Entrants.ingrédients == null && cnd.Name.Equals("Entrants"))
                {
                    Entrants = new Ingrédients(cnd);
                    if (Sortants.ingrédients != null) break;
                }
                else if(Sortants.ingrédients == null && cnd.Name.Equals("Sortants"))
                {
                    Sortants = new Ingrédients(cnd);
                    if (Entrants.ingrédients != null) break;
                }
            }
            if (Entrants.ingrédients == null) Entrants.ingrédients = new Ingrédient[0];
            if (Sortants.ingrédients == null) Sortants.ingrédients = new Ingrédient[0];
        }

        public void Dessiner(Graphics g, PointF p, double Facteur, double FacteurRéel, Pièce.EPièce PièceLimit)
        {
            p.Y -= ((Entrants.EstVide ? 0.0f : 50.0f / 2.0f + EspaceEntrant + Espassement.Height) + (Sortants.EstVide ? 0.0f : (50.0f / 2.0f) + Espassement.Height)) / 2.0f;
            if(!Entrants.EstVide)
            {
                p.Y += (Espassement.Height / 2.0f);
                Entrants.DessinerEntrant(g, p, Facteur, PièceLimit);
                p.Y += 25.0f + EspaceEntrant + (Espassement.Height / 2.0f);
            }
            Pen pn = new Pen(Producteur.Couleur[(int)TypeProducteur], 2.0f);
            //p.Y += 5.0f;
            g.DrawLine(pn, new PointF(p.X - 10.0f, p.Y - 10.0f), p);
            g.DrawLine(pn, p, new PointF(p.X + 10.0f, p.Y - 10.0f));
            //p.Y -= 5.0f;
            if (!Sortants.EstVide)
            {
                p.Y += 25.0f + (Espassement.Height / 2.0f);
                Sortants.DessinerSortant(g, p, Facteur, FacteurRéel);
                //p.Y += (Espassement.Height / 2.0f);
            }
        }

        public (Producteur.SelectionMode, Pièce.EPièce) Selection(PointF p)
        {
            p.Y += ((Entrants.EstVide ? 0.0f : 50.0f / 2.0f + EspaceEntrant + Espassement.Height) + (Sortants.EstVide ? 0.0f : (50.0f / 2.0f) + Espassement.Height)) / 2.0f;
            if (!Entrants.EstVide)
            {
                p.Y -= (Espassement.Height / 2.0f);
                if (-25.0 <= p.Y && p.Y <= 25.0) return (Producteur.SelectionMode.LienEntrant, Entrants.Selection(p));
                p.Y -= 25.0f + EspaceEntrant + (Espassement.Height / 2.0f);
            }
            if (!Sortants.EstVide)
            {
                p.Y -= 25.0f + (Espassement.Height / 2.0f);
                if (-25.0 <= p.Y && p.Y <= 25.0) return (Producteur.SelectionMode.LienEntrant, Sortants.Selection(p));
                //p.Y -= Espassement.Height / 2.0f;
            }
            return (Producteur.SelectionMode.Déplacement, Pièce.EPièce.vide);
        }

        public override string ToString()
        {
            return $"{Entrants}>{TypeProducteur}>{Sortants}";
        }

    }
}
