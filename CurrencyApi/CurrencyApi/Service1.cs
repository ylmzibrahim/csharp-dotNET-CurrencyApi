using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Xml.Linq;

namespace CurrencyApi
{
    public partial class Service1 : ServiceBase
    {
        string connection_string;
        public byte totalCurrency = 20;
        static System.Timers.Timer timer;
        SqlConnection sqlConnection = new SqlConnection();
        List<string> baseUrl = new List<string>();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Out("Hizmet Başlangıç ----------------------------------------------------------------------------------------------- " + DateTime.Now);

            //Getting DATABASE.INI file path in the database_path.txt file.
            string database_path = @"database_path.txt";
            string database_read = @"C:\ProgramData\Mikro\v16xx\DATABASE.INI";
            //string database_read = @"D:\v16xx\DATABASE.INI";

            if (!File.Exists(database_path))
            {
                File.Create(database_path).Close();
                var tw = new StreamWriter(database_path);
                tw.Write(database_read);
                tw.Close();
            }
            else if (File.Exists(database_path))
            {
                database_read = System.IO.File.ReadAllText(database_path.ToString());
            }

            try
            {
                //DATABASE.INI file read.
                //var MyIni = new IniFile(@"D:\v16xx\DATABASE.INI");
                var MyIni = new IniFile(database_read);

                //connection_string and SQL_Server read.
                string SQL_Server = MyIni.Read("SQL_Server", "Roots");
                connection_string = MyIni.Read("connection_string", "Roots");

                //Provider=*; removed from connection_string.
                int start = connection_string.IndexOf("Provider=");
                int end = connection_string.IndexOf(";", start);
                connection_string = connection_string.Remove(start, end - start + 1);

                //Updating Inıtıal Catalog="string".
                start = connection_string.IndexOf("Initial Catalog=");
                connection_string = connection_string.Remove(start + 16);

                //Data Source=*; and Initial Catalog=* added to connection_string.
                connection_string = "Data Source=" + SQL_Server + ";" + connection_string + "MikroDB_V16";

                sqlConnection.ConnectionString = connection_string;

                Logger.Out("BAŞARILI: Veri Tabanına Bağlandı. " + DateTime.Now);
            }
            catch (Exception exeption)
            {
                Logger.Out("HATA: Veri tabanına bağlanırken hata oluştu. Veri tabanı adresi: " + database_read + " / Bağlantı cümlesi: " + connection_string + " " + DateTime.Now);
                Logger.Out(exeption.ToString());
            }

                timer = new System.Timers.Timer();
                timer.AutoReset = false;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(GetCurrency);
                timer.Interval = GetInterval();
                timer.Start();
            
        }

        protected override void OnStop()
        {
            timer.Stop();
            Logger.Out("SERVİS: Servis Durduruldu! ----------------------------------------------------------------------------------------------- " + DateTime.Now);
        }

        public static class Logger
        {
            public static StringBuilder LogString = new StringBuilder();
            public static void Out(string str)
            {
                Console.WriteLine(str);
                LogString.Append(str).Append(Environment.NewLine);

                string log_path = @"log.txt";
                if (!File.Exists(log_path))
                {
                    File.Create(log_path).Close();
                    var tw = new StreamWriter(log_path);
                    tw.WriteLine("LOG DOSYASI OLUŞTURULDU " + DateTime.Now);
                    tw.Close();
                }
                else if (File.Exists(log_path))
                {
                    var tw = new StreamWriter(log_path, true);
                    tw.WriteLine(LogString.ToString());
                    tw.Close();
                }
            }
        }

        public static double GetInterval()
        {
            DateTime now = DateTime.Now;
            //double perSec = 1000;
            //double perMin = (1000 * (60 - now.Second));
            double perHour = ((60 * 1000 * (60 - now.Minute)) - now.Second * 1000);
            return (perHour); //Once per hour
        }

        public async void GetCurrency(object sender, System.Timers.ElapsedEventArgs e)
        {
                timer.Start();
                timer.Interval = GetInterval();
            try
            {
                Logger.Out("Döviz Çekiliyor ----------------------------------------------------------------------------------------------- " + DateTime.Now);

                List<Currency> currencies = new List<Currency>();
                string[,] cons = new string[20, 2] {
                { "TP.DK.USD.A.YTL", "TP.DK.USD.S.YTL" ,} ,
                { "TP.DK.EUR.A.YTL", "TP.DK.EUR.S.YTL" } ,
                { "TP.DK.CAD.A.YTL", "TP.DK.CAD.S.YTL" } ,
                { "TP.DK.DKK.A.YTL", "TP.DK.DKK.S.YTL" } ,
                { "TP.DK.SEK.A.YTL", "TP.DK.SEK.S.YTL" } ,
                { "TP.DK.CHF.A.YTL", "TP.DK.CHF.S.YTL" } ,
                { "TP.DK.NOK.A.YTL", "TP.DK.NOK.S.YTL" } ,
                { "TP.DK.JPY.A.YTL", "TP.DK.JPY.S.YTL" } ,
                { "TP.DK.SAR.A.YTL", "TP.DK.SAR.S.YTL" } ,
                { "TP.DK.KWD.A.YTL", "TP.DK.KWD.S.YTL" } ,
                { "TP.DK.AUD.A.YTL", "TP.DK.AUD.S.YTL" } ,
                { "TP.DK.GBP.A.YTL", "TP.DK.GBP.S.YTL" } ,
                { "TP.DK.IRR.A.YTL", "TP.DK.IRR.S.YTL" } ,
                { "TP.DK.BGN.A.YTL", "TP.DK.BGN.S.YTL" } ,
                { "TP.DK.RON.A.YTL", "TP.DK.RON.S.YTL" } ,
                { "TP.DK.CNY.A.YTL", "TP.DK.CNY.S.YTL" } ,
                { "TP.DK.PKR.A.YTL", "TP.DK.PKR.S.YTL" } ,
                { "TP.DK.QAR.A.YTL", "TP.DK.QAR.S.YTL" } ,
                { "TP.DK.RUB.A.YTL", "TP.DK.RUB.S.YTL" } ,
                { "TP.DK.XDR.A.YTL", "TP.DK.XDR.S.YTL" }
            };
                int i = 0;

                if (sqlConnection.State == ConnectionState.Closed)
                    sqlConnection.Open();
                SqlCommand command = new SqlCommand("SELECT DISTINCT dov_fiyat1, dov_fiyat2, dov_no FROM DOVIZ_KURLARI WHERE dov_no=1 OR dov_no=2 OR dov_no=3 OR dov_no=4 OR dov_no=5 OR dov_no=6 OR dov_no=7 OR dov_no=8 OR dov_no=9 OR dov_no=10 OR dov_no=11 OR dov_no=12 OR dov_no=13 OR dov_no=16 OR dov_no=17 OR dov_no=44 OR dov_no=118 OR dov_no=121 OR dov_no=123 OR dov_no=159 ORDER BY dov_no ASC", sqlConnection);
                SqlDataReader sqlDataReader = command.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    currencies.Add(new Currency()
                    {
                        dov_no = byte.Parse(sqlDataReader["dov_no"].ToString()),
                        dov_fiyat1 = float.Parse(sqlDataReader["dov_fiyat1"].ToString()),
                        dov_fiyat2 = float.Parse(sqlDataReader["dov_fiyat2"].ToString()),
                        dov_kod_alis = cons[i, 0],
                        dov_kod_satis = cons[i, 1],
                        dov_url_alis = "https://evds2.tcmb.gov.tr/service/evds/series=" + cons[i, 0] + "&startDate=" + DateTime.Today.ToString("dd-MM-yyyy") + "&endDate=" + DateTime.Today.ToString("dd-MM-yyyy") + "&type=xml&key=qGLP2xIpBm&aggregationTypes=last&formulas=0&frequency=1",
                        dov_url_satis = "https://evds2.tcmb.gov.tr/service/evds/series=" + cons[i, 1] + "&startDate=" + DateTime.Today.ToString("dd-MM-yyyy") + "&endDate=" + DateTime.Today.ToString("dd-MM-yyyy") + "&type=xml&key=qGLP2xIpBm&aggregationTypes=last&formulas=0&frequency=1"
                    });
                    i++;
                }
                sqlConnection.Close();

                //We will now define the HttpClient with the first using statement which will use a IDisposable.
                using (HttpClient client = new HttpClient())
                {
                    if (sqlConnection.State == ConnectionState.Closed)
                        sqlConnection.Open();
                    foreach (var item in currencies)
                    {
                        //In the next using statement you will initiate the Get Request, use the await keyword so it will execute the using statement in order.
                        HttpResponseMessage resUSDalis = await client.GetAsync(item.dov_url_alis);
                        HttpResponseMessage resUSDsatis = await client.GetAsync(item.dov_url_satis);

                        //Now assign your content to your data variable, by converting into a string using the await keyword.
                        var data1 = await resUSDalis.Content.ReadAsStringAsync();
                        var data2 = await resUSDsatis.Content.ReadAsStringAsync();

                        //To get specific element in the document, we use foreach loop. 
                        foreach (XElement element in XDocument.Parse(data1).Descendants(item.dov_kod_alis.Replace('.', '_')))
                        {
                            if (element.Value != "")
                                item.dov_fiyat1 = float.Parse(element.Value.ToString().Replace('.', ','));
                        }
                        foreach (XElement element in XDocument.Parse(data2).Descendants(item.dov_kod_satis.Replace('.', '_')))
                            if (element.Value != "")
                                item.dov_fiyat2 = float.Parse(element.Value.ToString().Replace('.', ','));


                        SqlCommand sqlCommand = new SqlCommand("UPDATE DOVIZ_KURLARI SET dov_fiyat1=" + item.dov_fiyat1.ToString().Replace(',', '.') + ", dov_fiyat2=" + item.dov_fiyat2.ToString().Replace(',', '.') + " WHERE dov_no=" + item.dov_no, sqlConnection);
                        sqlCommand.ExecuteNonQuery();

                    }
                    sqlConnection.Close();

                    Logger.Out("BAŞARILI: Döviz Çekme Başarılı " + DateTime.Now);
                }
            }
            //If the connection lost, program will catch the error.
            catch (Exception exception)
            {
                Logger.Out("HATA: Döviz Çekme Hatası " + DateTime.Now);
                Logger.Out(exception.ToString());
            }
        }
    }
}