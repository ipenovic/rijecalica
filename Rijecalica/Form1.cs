using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Rijecalica
{
    public partial class Form1 : Form
    {
        static List<string> rjecnik;
        static List<Rijec> nadeneRijeci = new List<Rijec>();
        static List<string> putanja = new List<string>();
        static int M = 4, N = 4;
        string slova = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void txtUnos_TextChanged(object sender, EventArgs e)
        {
            slova = txtUnos.Text;

            for (int i = 0; i < 16; ++i)
            {
                int red = i % 4;
                int stupac = i / 4;

                Control textBox = tablica.GetControlFromPosition(red, stupac);

                string letter;
                if (i < slova.Length)
                    letter = slova[i].ToString();
                else
                    letter = "";

                textBox.Text = letter;
            }
        }

        private void btnRiješi_Click(object sender, EventArgs e)
        {
            try
            {
                slova = txtUnos.Text;

                var matrica = UnosMatrice(slova, M, N);
                M -= 1; N -= 1;

                rjecnik = Rjecnik(slova);

                Stopwatch sw = new Stopwatch();
                sw.Start();

                NadiRijeci(matrica);
                IspisiRijeci(nadeneRijeci);

                sw.Stop();
                toolStripStatusLabel.Text += "   |   Vrijeme izvođenja: " + sw.Elapsed;
                statusStrip1.Refresh();
            }
            catch
            {
                toolStripStatusLabel.Text = "Niste unijeli dovoljno slova!";
                statusStrip1.Refresh();
            }
        }

        private void btnRandom_Click(object sender, EventArgs e)
        {
            ObojajMatricu();
            lstRijeci.Items.Clear();
            slova = "";
            nadeneRijeci.Clear();
            M = 4;
            N = 4;
            toolStripStatusLabel.Text = "";
            statusStrip1.Refresh();

            Random r = new Random();
            string svaslova = "abcčćdđefghijklmnoprsštuvzž".ToUpper();
            for (int i = 0; i < 16; i++)
            {
                int index = r.Next(svaslova.Length);
                slova += svaslova[index];
            }
            txtUnos.Text = slova;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                txtUnos.Text = "";
                lstRijeci.Items.Clear();
                slova = "";
                nadeneRijeci.Clear();
                rjecnik.Clear();
                M = 4;
                N = 4;
                toolStripStatusLabel.Text = "";
                statusStrip1.Refresh();
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        tablica.GetControlFromPosition(i, j).BackColor = Color.WhiteSmoke;
            }
            catch
            {
                toolStripStatusLabel.Text = "Resetirano je već!";
                statusStrip1.Refresh();
            }
        }

        private void lstRijeci_SelectedIndexChanged(object sender, EventArgs e)
        {
            ObojajMatricu();

            try
            {
                string odabranaRijec = lstRijeci.SelectedItem.ToString();
                string[] cijelarijec = odabranaRijec.Split('>');
                string[] putanja = cijelarijec[1].Split(' ');
                string[] redak = new string[putanja.Length];
                string[] stupac = new string[putanja.Length];
                for (int i = 0; i < putanja.Length - 1; i++)
                {
                    stupac[i] = putanja[i].Split(',')[0];
                    redak[i] = putanja[i].Split(',')[1];
                }
                for (int i = 0; i < stupac.Length - 1; i++)
                    tablica.GetControlFromPosition(int.Parse(redak[i]), int.Parse(stupac[i])).BackColor = Color.LightBlue;
            }
            catch
            {
                ObojajMatricu();
            }
        }

        static char[,] UnosMatrice(string slova, int M, int N)
        {
            char[,] matrica = new char[M, N];

            int br = 0;
            for (int i = 0; i < M; i++)
                for (int j = 0; j < N; j++)
                {
                    matrica[i, j] = slova[br];
                    br++;
                }
            Console.WriteLine();

            return matrica;
        }

        static List<string> Rjecnik(string slova)
        {
            List<string> cijelirjecnik = UcitajRjecnik();
            List<string> visakrijeci = new List<string>();
            List<char> nekoristeni = VratiSlova(slova);
            foreach (string w in cijelirjecnik)
                foreach (char c in nekoristeni)
                    if (w.ToString().Contains(c.ToString()))
                    {
                        visakrijeci.Add(w);
                        break;
                    }
            List<string> rjecnik = cijelirjecnik.Except(visakrijeci).ToList();

            return rjecnik;
        }

        static List<string> UcitajRjecnik()
        {
            List<string> rjecnik = new List<string>();
            string line;
            StreamReader file = new StreamReader("../../rjecnik.txt", Encoding.GetEncoding("windows-1250"));
            while ((line = file.ReadLine()) != null)
                rjecnik.Add(line.ToUpper().ToString());
            file.Close();

            return rjecnik;
        }

        static List<char> VratiSlova(string slova)
        {
            string svaslova = "abcčćdđefghijklmnoprsštuvzžqwyxö".ToUpper();
            List<char> abeceda = new List<char>();
            foreach (char c in svaslova)
                abeceda.Add((char)c);

            string rjecalica = slova;
            List<char> matrica = new List<char>();
            foreach (char c in rjecalica)
                matrica.Add((char)c);

            List<char> nekoristeni = abeceda.Except(matrica).ToList();

            return nekoristeni;
        }

        private static void NadiRijeci(char[,] matrica)
        {
            var posjecen = new bool[(M + 1), (N + 1)];
            var rijec = string.Empty;

            for (int row = 0; row <= M; row++)
                for (int col = 0; col <= N; col++)
                    Rekurzija(matrica, row, col, posjecen, rijec);
        }

        private static void Rekurzija(char[,] matrica, int row, int col, bool[,] posjecen, string rijec)
        {
            posjecen[row, col] = true;
            rijec += matrica[row, col];
            putanja.Add(row + "," + col);

            bool provjera = false;
            foreach (string w in rjecnik)
                if (w.Contains(rijec))
                    provjera = true;

            if (provjera == true)
            {
                if (rjecnik.Contains(rijec.ToString()) && rijec.Length > 2)
                    nadeneRijeci.Add(new Rijec(rijec, putanja));

                for (int r = row - 1; r <= (row + 1) && r <= M; r++)
                    for (int c = col - 1; c <= (col + 1) && c <= N; c++)
                        if (r >= 0 && c >= 0 && !posjecen[r, c])
                            Rekurzija(matrica, r, c, posjecen, rijec);
            }
            rijec = rijec.Remove(rijec.Length - 1, 1);
            putanja.RemoveAt(putanja.Count - 1);
            posjecen[row, col] = false;
        }

        private void ObojajMatricu()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    tablica.GetControlFromPosition(i, j).BackColor = Color.WhiteSmoke;
        }

        private void IspisiRijeci(List<Rijec> rijeci)
        {
            List<Rijec> bezDuplih = nadeneRijeci.Distinct().ToList();
            var sorted = bezDuplih.OrderByDescending(x => x.rijec.Length).ThenBy(x => x.rijec).ToList();

            int brojac = 0;
            int duzina = sorted[0].rijec.Length;
            lstRijeci.Items.Add("RIJEČI SA " + duzina + " SLOVA:\n");
            foreach (Rijec r in sorted)
            {
                brojac++;
                if (r.rijec.Length < duzina)
                {
                    duzina = r.rijec.Length;
                    lstRijeci.Items.Add("\n");
                    lstRijeci.Items.Add("RIJEČI SA " + duzina + " SLOVA:\n");
                    lstRijeci.Items.Add(r.rijec + "==>" + IspisListe(r.putanja) + "\n");
                }
                else
                {
                    lstRijeci.Items.Add(r.rijec + "==>" + IspisListe(r.putanja) + "\n");
                }
            }
            toolStripStatusLabel.Text = "Pronađeno je " + brojac + " riječi".ToString();
            statusStrip1.Refresh();
        }

        string IspisListe(List<string> lista)
        {
            string tekst = "";
            foreach (string s in lista)
                tekst += s + " ";
            return tekst;
        }
    }
}
