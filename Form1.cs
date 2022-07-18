using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OSSystemsQuiz
{
    public partial class Form1 : Form
    {
        //Statik tanımlamalar ve değişkenler
        private static int PCount = 0; // Process sayısı
        private static readonly int RCount = 3; // Kaynak sayısı (A,B,C)
        private static int MCount = 0; // Max matrisi için process sayısı (dengeli girdi kontrolü için)
        private static int total = 0; // Kod içerisinde kullanılan ve değeri saklanan sayaçlar
        private static int cntr = 0;
        private static int countavail = 0;
        private static readonly List<int> safe = new List<int>();              //Güvenli işlemlerin indisini saklayan list yapısı
        private static readonly List<string> comperator = new List<string>();  //Güvenli sekansların kaydını ve kullanıcı güvenli yol karşılaştırmasını oluşturan liste.
        private static readonly int[] resources = new int[RCount];             //Mevcut kaynak girişi


        //İşlemleri ve kaynak değerlerini saklayan 2D array tanımlamaları
        private static int[,] temp = new int[PCount, RCount]; 
        private static int[,] need = new int[PCount, RCount];
        private static int[,] max = new int[MCount, RCount];
        private static int[,] alloc = new int[PCount, RCount];
        private static int[] available = new int[RCount];
        private static int[,] tempcopy = new int[PCount, RCount];


        private void initializeValues()
        {
            // Process sayısında satır, Resource sayısı olacak kadar da sütun ele alan 2D arraylerimiz.
            need = new int[PCount, RCount];
            temp = new int[PCount, RCount];
            tempcopy = new int[PCount, RCount];
            alloc = new int[PCount, RCount];
            max = new int[PCount, RCount];

            for (int i = 0; i < PCount; i++)
            {
                for (int j = 0; j < RCount; j++) //Arayüzdeki tablolardaki değerleri ilgili Arraylere atama.
                {
                    alloc[i, j] = int.Parse(dataGridView1.Rows[i].Cells[j].Value.ToString());
                    max[i, j] = int.Parse(dataGridView2.Rows[i].Cells[j].Value.ToString());
                }
            }

            for (int i = 0; i < RCount; i++)
            {
                int sum = 0;
                for (int j = 0; j < PCount; j++)//Available(Mevcut) hesabı, Alokasyonları sütun şeklinde toplayıp                          
                {                               // girilen mevcut kaynakların yeterliliğine bakar. (bunun kontrolü is_available fonksiyonunda)
                    sum += alloc[j, i];
                }
                available[i] = resources[i] - sum;
                for (int j = 0; j < PCount; j++)
                {
                    temp[j, i] = resources[i] - sum;
                    tempcopy[j, i] = resources[i] - sum;
                }
            }
        }

        private static bool is_available(int process_id,
                                int[,] allocated,
                                int[,] max, int[,] need,
                                int[] available)
        {
            bool flag = true;

            // Mevcut kaynakların işlemlerin gereksinimlerinden büyük olup olmadığını kontrol eder.
            for (int i = 0; i < RCount; i++)
            {
                if (need[process_id, i] > available[i])
                {
                    flag = false;
                }
            }
            return flag;
        }

        //Güvenlilik kontrol fonksiyonu
        private void safe_sequence(bool[] marked,
                                int[,] alloc,
                                int[,] max, int[,] need,
                                int[] available,
                                List<int> safe)
        {
            for (int i = 0; i < PCount; i++)
            {

                // işaretlilik (boolean marked, daha önce işlenmiş anlamına geliyor) ve atanabilirlik kontrolü
                if (!marked[i] &&
                    is_available(i, alloc, max,
                                need, available))
                {
                    // fonksiyona girildiyse işlemi işaretle
                    marked[i] = true;

                    // mevcut kaynak sayısına, alokasyon yapılmış işlemin kaynaklarının geri kazandırılması
                    for (int j = 0; j < RCount; j++)
                    {
                        available[j] += alloc[i, j];
                    }
                    // İşlemin indexi (Hangi P olduğu) safe listesinde saklanır.
                    safe.Add(i);
                    // Bir sonraki güvenli işlemi bulmak için fonksiyon tekrar çağrılır, böylece güvenli yol saptanır.
                    safe_sequence(marked, alloc, max,
                                need, available, safe);
                    safe.RemoveAt(safe.Count - 1);
                    // Güvenli yollar bulunduktan sonra işaret kaldırılır.
                    marked[i] = false;
                    // Bir sonraki güvenli yol arayışı başlaması için kaynaklar, girilen kaynağa eşitlenir.
                    for (int j = 0; j < RCount; j++)
                    {
                        available[j] -= alloc[i, j];
                    }
                }
            }
            //Güvenli yol bulunduysa, bunu saklar ve arayüze verir.
            if (safe.Count == PCount)
            {
                countavail++;
                string ListAdd = "";
                for (int i = 0; i < PCount; i++)
                {
                    ListAdd += ("P" + safe[i]);
                    if (i != (PCount - 1))
                    {
                        ListAdd += "->";
                    }
                }

                comperator.Insert(total, ListAdd);
                label9.Text = string.Join("\n", comperator);

                dataGridView4.Rows.Add(temp[0, 0], temp[0, 1], temp[0, 2]);
                for (int i = 0; i < PCount; i++)
                {
                    
                    for (int j = 0; j < PCount; j++)
                    {
                        temp[j, 0] += alloc[safe[i], 0];
                        temp[j, 1] += alloc[safe[i], 1];
                        temp[j, 2] += alloc[safe[i], 2];
                    }
                    dataGridView4.Rows.Add(temp[i, 0], temp[i, 1], temp[i, 2]);
                    dataGridView4.Rows[cntr+countavail].HeaderCell.Value = ("P" + safe[i] + " Sonrası");
                    cntr++;
                }
                total++;

                for (int i = 0; i < RCount; i++)
                {
                    for (int j = 0; j < PCount; j++)
                    {
                        temp[j, i] = tempcopy[j,i];
                    }
                }
            }
        }

        private void calculateNeed()
        {
            while (dataGridView3.Rows.Count < PCount)
            {
                dataGridView3.Rows.Add(1);
            }
            for (int i = 0; i < PCount; i++)
            {
                for (int j = 0; j < RCount; j++)
                {
                    need[i, j] = max[i, j] - alloc[i, j];
                    dataGridView3.Rows[i].HeaderCell.Value = ("P" + i);
                }
                dataGridView3.Rows[i].SetValues(need[i, 0], need[i, 1], need[i, 2]);

            }
        }


        public Form1()
        {
            InitializeComponent();
        }

        public void AlokasyonButton_Click(object sender, EventArgs e)
        {    //Alokasyon ekleme tuşuna basıldığında, girilen değerler arayüzdeki tabloya atılır ve işlem sayısı sayacı artırılır.
            int A = int.Parse(textBox1.Text);
            int B = int.Parse(textBox2.Text);
            int C = int.Parse(textBox3.Text);
            dataGridView1.Rows.Add(A, B, C);
            dataGridView1.Rows[PCount].HeaderCell.Value = ("P" + PCount);
            PCount++;
        }

        public void MaxButton_Click(object sender, EventArgs e)
        {   //Max ekleme tuşuna basıldığında, girilen değerler arayüzdeki tabloya atılır ve Aloke edilmemiş işlemlerin Max değerlerinin girilmesi engellenir.
            int A = int.Parse(textBox4.Text);
            int B = int.Parse(textBox5.Text);
            int C = int.Parse(textBox6.Text);

            if (PCount >= MCount)
            {
                dataGridView2.Rows.Add(A, B, C);
                dataGridView2.Rows[MCount].HeaderCell.Value = ("P" + MCount);
                MCount++;
            }
        }

        public void MevcutButton_Click(object sender, EventArgs e)
        {   //Hesapla tuşuna basıldığında, kullanıcının girdiği kaynak değerlerine ve tablolardaki değerlere göre hesaplamalar yapan fonksiyonlar çağrılır.
            //Sonuçlar arayüze verilir.
            int A = int.Parse(textBox7.Text);
            int B = int.Parse(textBox8.Text);
            int C = int.Parse(textBox9.Text);
            available = new int[RCount];
            resources[0] = A; resources[1] = B; resources[2] = C;

            initializeValues();
            bool[] marked = new bool[PCount];

            calculateNeed();

            safe_sequence(marked, alloc, max,
                    need, available, safe);
        }

        private void button1_Click(object sender, EventArgs e)
        {   //Kullanıcının girdiği Güvenli Yol, programın hesapladığı güvenli yollarla karşılaştırılır ve uyum varsa güvenli, yoksa güvneli değil yazar.
            string safe = textBox11.Text;

            if (comperator.Contains(safe))
            { label14.Text = "Güvenli!"; }
            else { label14.Text = "Güvenli değil"; }
        }
    }
}
