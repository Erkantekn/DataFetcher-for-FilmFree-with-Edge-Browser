using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace edgeDeneme1
{
    /* Film eklendiği halde iframe linki eklenmemişleri bulan program

      string bulunanlar = "";
           List<IframeTablosu> iframes = db.IframeTablosu.ToList();
           List<int> films = db.Film.Select(x => x.id).ToList();
           foreach (var item in films)
           {
               bool bulduMu = false;
               foreach (var ifrm in iframes)
               {
                   if(ifrm.filmId==item)
                   {
                       bulduMu = true;
                       break;
                   }
               }


               if (!bulduMu)
               bulunanlar += item + ",";
           }

           MessageBox.Show(bulunanlar);
  */
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int sayfa = 1, maxSayfa = 5, suankiFilm = 1;
        string url = "https://www.fullhdfilmizlesene.com/yeni-filmler-izle/";



        FilmFreeEntities db = new FilmFreeEntities();
        string getTurIds(string aranacak)
        {




            List<FilmTur> turs = db.FilmTur.ToList();
            FilmTur TUR_REAL = new FilmTur();
            string hangisi = "";
            //string aranacak = "Aksiyon Filmleri,Aile Filmleri,Bilim Kurgu Filmleri";
            foreach (var arncak2 in aranacak.Split(','))
            {
                string arncak = arncak2;
                if (arncak.StartsWith(" "))
                {
                    arncak = arncak.Remove(0, 1);
                }
                if (arncak.EndsWith(" "))
                {
                    arncak = arncak.Remove(arncak.Length - 1, 1);
                }
                bool bulunduMu = false;
                foreach (var item in turs)
                {
                    if (arncak == item.TurAdi)
                    {
                        TUR_REAL = item;
                        bulunduMu = true;
                        break;
                    }

                }

                if (!bulunduMu)
                {
                    //tür bulunamadı. Yeni eklenmesi gerekiyor
                    TUR_REAL.TurAdi = arncak;
                    db.FilmTur.Add(TUR_REAL);
                    db.SaveChanges();
                }
                hangisi += TUR_REAL.id + ",";
            }

            if (!String.IsNullOrEmpty(hangisi))
            {
                hangisi = hangisi.Remove(hangisi.Length - 1, 1);
            }
            else
                return "31"; //Bulunamadı


            return hangisi;



        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //    List<Film> filmler = db.Film.ToList();
            //    List<FilmTur> turler = db.FilmTur.ToList();
            //    foreach (var film in filmler)
            //    {
            //        foreach (var tur in turler)
            //        {
            //            if()
            //        }
            //    }

        }

        List<string> filmPagesLink = null;
        async void GetFilmPages(object sender, EventArgs e)
        {
            filmPagesLink = new List<string>();
            webView.CoreWebView2.DOMContentLoaded -= new EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs>(GetFilmPages);

            string sonuc = "";
            while (String.IsNullOrEmpty(sonuc))
            {
                sonuc = await webView.ExecuteScriptAsync("var str = '';$('.index-orta.liste-3-box').children(1).children().each(function(j, li){str+=$(li).children().eq(2).attr('href')+',';}); str;");
                Application.DoEvents();
            }


            sonuc = sonuc.Replace("\"", "");
            foreach (var item in sonuc.Split(','))
            {
                if (!String.IsNullOrEmpty(item))
                    filmPagesLink.Add(item);
            }

            //film linkleri çekildi. Artık film sayfalarına girebiliriz.
            suankiFilm = 0;
            webView.CoreWebView2.Navigate(filmPagesLink[suankiFilm]);
            webView.CoreWebView2.DOMContentLoaded += new EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs>(GetDataFilm);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox2.Text))
            {
                sayfa = Convert.ToInt32(textBox2.Text);
            }
            if (!String.IsNullOrEmpty(textBox3.Text))
            {
                maxSayfa = Convert.ToInt32(textBox3.Text);
            }

            await webView.EnsureCoreWebView2Async(null);
            timer1.Enabled = true;
            //ilk 16 filmin linkini çekiyoruz
            webView.CoreWebView2.Navigate(url + sayfa);
            webView.CoreWebView2.DOMContentLoaded += new EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs>(GetFilmPages);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        async void WaitForScript(string script)
        {
            string sonuc = await webView.ExecuteScriptAsync(script);
            int denemeSayac = 0;
            while (true)
            {
                denemeSayac++;
                if (denemeSayac > 50)
                {
                    break;
                }
                System.Threading.Thread.Sleep(100);
                if (!String.IsNullOrEmpty(sonuc))
                    break;
            }
        }
        int suankiZaman = 0, maxZaman = 5000;
        private void button3_Click(object sender, EventArgs e)
        {
            maxZaman = Convert.ToInt32(textBox5.Text);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (suankiZaman > maxZaman)
            {
                suankiZaman = 0;
                webView.Reload();

            }
            else
                suankiZaman += timer1.Interval;
        }
        int sayac = 0;
        async void GetDataFilm(object sender, EventArgs e)
        {
        //timer zaman aşımı
        //zaman aşılırsa sayfayı yeniler
        //System.Threading.Thread.Sleep(500);
        etiket1: try
            {
                suankiZaman = 0;


                //film sayfasına girildi. bilgiler çekilsin.
                string isim = await webView.ExecuteScriptAsync("$('.header-sol').first().children(0).children(0).first().text();");
                isim = isim.Replace("\"", "");

                string icerik = await webView.ExecuteScriptAsync("$('.detay-sag').first().children(0).children(0).children(0).first().text();");
                icerik = icerik.Replace("\"", "");
                icerik = icerik.Replace('½', ' ');
                icerik = icerik.Replace(@"\n", " ");
                icerik = icerik.Replace("'", " ");

                string imdb = await webView.ExecuteScriptAsync("$('.imdb-ic').children(0).first().text();");
                imdb = imdb.Replace("\"", "");

                string yonetmen = await webView.ExecuteScriptAsync("$('.film-info').first().children(0).children().eq(1).children().first().children().first().text();");
                yonetmen = yonetmen.Replace("\"", "");

                string yapimTarihi = await webView.ExecuteScriptAsync("var yapim = $('.yapim').first().children().first().text();yapim.split(\" \")[0];");
                yapimTarihi = yapimTarihi.Replace("\"", "");
                DateTime yapimTarihiDate;
                try
                {
                    yapimTarihiDate = (new DateTime(Convert.ToInt32(yapimTarihi), 01, 01));
                }
                catch
                {
                    yapimTarihiDate = new DateTime(1900, 1, 1);
                }


                string oyuncular = await webView.ExecuteScriptAsync("$('.film-info').first().children(0).children().eq(3).text();");
                oyuncular = oyuncular.Replace("\"", "");

                string tur = await webView.ExecuteScriptAsync("$('.film-info').first().children(0).children().eq(7).text()");
                tur = tur.Replace("\"", "");

                string kategori = await webView.ExecuteScriptAsync("$('.film-info').first().children(0).children().eq(9).text();");
                kategori = kategori.Replace("\"", "");
                kategori = kategori.Replace("-", ",");
                kategori = kategori.Replace(" ", "");

                string dil = await webView.ExecuteScriptAsync("$('.film-info').first().children(0).children().eq(11).text()");
                dil = dil.Replace("\"", "");
                dil = dil.Replace(" ", "");
                dil = dil.Replace("/", ",");

                string sure = await webView.ExecuteScriptAsync("var sure = $('.mov-box.cm.sure').first().text();sure.split(\" \")[0];");
                sure = sure.Replace("\"", "");

                //sıra iframe bilgisi çekmeye geldi

                int toplamIframe = Convert.ToInt32(await webView.ExecuteScriptAsync("$('.part-sources').children().length;"));

                string iframe = "";
                int denemeSayac = 0;
                for (int i = 0; i < toplamIframe; i++)
                {
                    denemeSayac++;
                    if(denemeSayac + (i *50) > ((i + 1) * 50))
                    {
                        iframe = "Null,Null,Null";
                        break;
                    }
                    await webView.ExecuteScriptAsync("$('.part-sources').children().eq(" + i + ").click()");
                    WaitForScript("$('#plx').children().first().attr('src');");
                    //System.Threading.Thread.Sleep(3000);
                    string kaynak = await webView.ExecuteScriptAsync("$('.part-source-sec').first().text();");

                    //traltyazı/dublaj tuşlarına bakalım
                    int toplamDilButonu = Convert.ToInt32(await webView.ExecuteScriptAsync("$('.part-btns').children().length;"));
                    if (toplamDilButonu == 2)
                    {


                        string dublajAltyazi1 = await webView.ExecuteScriptAsync("$('.part-btns').children().eq(0).text();");
                        dublajAltyazi1 = dublajAltyazi1.Replace("\"", "");
                        string link1 = await webView.ExecuteScriptAsync("$('#plx').children().first().attr('src');");
                        link1 = link1.Replace("\"", "");

                        await webView.ExecuteScriptAsync("$('.part-btns').children().eq(1).children().first().click();");
                        WaitForScript("$('#plx').children().first().attr('src');");
                        //System.Threading.Thread.Sleep(3000);


                        string dublajAltyazi2 = await webView.ExecuteScriptAsync("$('.part-btns').children().eq(1).text();");
                        dublajAltyazi2 = dublajAltyazi2.Replace("\"", "");
                        string link2 = await webView.ExecuteScriptAsync("$('#plx').children().first().attr('src');");
                        link2 = link2.Replace("\"", "");


                        kaynak = kaynak.Replace("\"", "");
                        if (!string.IsNullOrEmpty(dublajAltyazi1) && !string.IsNullOrEmpty(dublajAltyazi2) && dublajAltyazi1 != "null" && dublajAltyazi1 != "NULL" && dublajAltyazi2 != "null" && dublajAltyazi2 != "NULL")
                            iframe += kaynak + "," + dublajAltyazi1 + "," + link1 + "," + dublajAltyazi2 + "," + link2 + "ß";
                        else
                        {
                            //Null veriler çekildi döngüyü baştan başlat
                            i--;
                            System.Threading.Thread.Sleep(100);
                            continue;
                        }
                    }
                    else
                    {


                        string link = await webView.ExecuteScriptAsync("$('#plx').children().first().attr('src');");
                        if (string.IsNullOrEmpty(link) || link == "null" || link == "NULL")
                        {
                            //null veriler çekildi baştan başlat
                            i--;
                            System.Threading.Thread.Sleep(100);
                            continue;
                        }
                        kaynak = kaynak.Replace("\"", "");
                        link = link.Replace("\"", "");
                        iframe += kaynak + "," + link + "ß";
                    }


                }

                //sonraki ß'yi silip % yapıyoruz
                iframe = iframe.Remove(iframe.Length - 1, 1);



                Film film = new Film();
                film.Isim = isim;

                foreach (var turr in getTurIds(tur).Split(','))
                {
                    int turId = 31;
                    Int32.TryParse(turr, out turId);
                    film.FilmTur.Add(db.FilmTur.FirstOrDefault(x => x.id == turId));
                }



                if (Int32.TryParse(sure, out int j))
                    film.SureDK = j;
                else
                    film.SureDK = 0;



                film.YayınTarihi = yapimTarihiDate;
                film.Icerik = icerik;
                film.Oyuncular = oyuncular;
                film.Tags = kategori;
                film.eklenmeTarihi = DateTime.Now;
                film.IMDB = imdb;
                film.Dil = dil;
                try
                {
                    db.Film.Add(film);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    listBox1.Items.Add("HATA: " + film.Isim);
                    listBox1.Items.Add(ex.ToString());
                }


                foreach (string item in iframe.Split('ß'))
                {
                    IframeTablosu ifrme = new IframeTablosu();
                    if (item.Split(',').Count() == 5)
                    {
                        //TR Dublaj, TR Altyazı halleri
                        ifrme.filmId = film.id;
                        ifrme.kaynakIsmi = item.Split(',')[0];
                        if (item.Split(',')[1] == "TR Dublaj")
                        {
                            ifrme.VarsaTrDublaj = item.Split(',')[2];
                            ifrme.VarsaTrAltyazi = item.Split(',')[4];
                        }
                        else if (item.Split(',')[1] == "TR Altyazı")
                        {
                            ifrme.VarsaTrAltyazi = item.Split(',')[2];
                            ifrme.VarsaTrDublaj = item.Split(',')[4];
                        }
                        else
                        {

                            //part butonlarının ismi Tek Part, Tek Part ise
                            ifrme.kaynakIsmi = item.Split(',')[0] + "-1";
                            ifrme.kaynakLinki = item.Split(',')[2];

                            IframeTablosu ifrme2 = new IframeTablosu();
                            ifrme2.filmId = film.id;
                            ifrme2.kaynakIsmi = item.Split(',')[0] + "-2";
                            ifrme2.kaynakLinki = item.Split(',')[4];
                            db.IframeTablosu.Add(ifrme2);

                        }
                        db.IframeTablosu.Add(ifrme);
                        db.SaveChanges();
                    }
                    else if (item.Split(',').Count() == 2)
                    {
                        //kaynaklink,kaynak link
                        ifrme.filmId = film.id;
                        ifrme.kaynakIsmi = item.Split(',')[0];
                        ifrme.kaynakLinki = item.Split(',')[1];
                        db.IframeTablosu.Add(ifrme);
                        db.SaveChanges();
                    }
                }





                // listBox1.Items.Add("Iframe: " + iframe + " ismi: " + isim + " İçerik: " + icerik + " imdb: " + imdb + " Yönetmen: " + yonetmen + " Oyuncular: " + oyuncular + " tur: " + tur + " Kategori: " + kategori + " dil: " + dil);
                label3.Text = "Sayaç :" + (++sayac).ToString();



                suankiFilm++;
                //Eğer tüm linklere girilmişse döngüden çık
                if (suankiFilm == filmPagesLink.Count)
                {
                    webView.CoreWebView2.DOMContentLoaded -= new EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs>(GetDataFilm);
                    //eğer son sayfaya gelinmemişse dön ve filmleri yeniden çek
                    if (sayfa < maxSayfa)
                    {
                        sayfa++;
                        webView.CoreWebView2.DOMContentLoaded += new EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs>(GetFilmPages);
                        webView.CoreWebView2.Navigate(url + sayfa);
                    }

                }
                else
                {
                    webView.CoreWebView2.Navigate(filmPagesLink[suankiFilm]);
                }
            }
            catch (Exception ex2)
            {
                listBox1.Items.Add("Hata: " + ex2.ToString());
                System.Threading.Thread.Sleep(300);
                listBox1.Refresh();
                goto etiket1;
            }


        }


        private async void button1_Click(object sender, EventArgs e)
        {

            string sonuc = await webView.ExecuteScriptAsync(textBox1.Text);
            //string sonuc = await webView.ExecuteScriptAsync(@"var str = "";
            //$('.index-orta.liste-3-box').children(1).children().each(function(j, li){
            //            str+=$(li).children().eq(2).attr('href')+',';
            //        });
            // str;");
            sonuc = sonuc.Replace("\"", "");
            string sonuc2 = "";
            foreach (var item in sonuc.Split(','))
            {
                sonuc2 += item + "\n";
            }
            label4.Text = sonuc2;

        }

    }
}
