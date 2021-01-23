using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;


namespace UploadFilesToYD
{
    // нужно перенести все в отдельный класс и добавить выбор репозитория и папки загрузки
    class Program
    {
        
        const string token = "AgAAAAAcFGEcAADLW2q-SLRT0U_mnlZDN_OQK0U";
        const string ftpBaseUri = "https://cloud-api.yandex.net/v1/disk/resources/upload/";
        
        static void Main(string[] args)
        {
            Console.ResetColor();
            UploadToDisc();
            //Console.WriteLine("Прогоню ка цикл");
            //var i = Int32.Parse(Console.ReadLine());
            //for (int j = 0; j < i; j++)
            //{
            //    System.Threading.Thread.Sleep(2000);
            //    Console.Write($" {j} ,");
            //}
            Console.ReadLine();
        }
        public static void UploadToDisc(string parhToSaveFile = @"L:\test\")
        {
            //Dictionary<string, string> urlAdreses = new Dictionary<string, string>();
            //List<string>  namesOffiles = new List<string>();
            string[] files = Directory.GetFiles(parhToSaveFile, "*.*", SearchOption.AllDirectories);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Получение списка файлов  в каталоге");
            Console.ResetColor();
            foreach (var s in files)
            {
                var fileName = s.Remove(0, parhToSaveFile.Length);
                UploadAsync(s, fileName);
            }
            //foreach (var s in files)
            //{
            //    var fileName = s.Remove(0, parhToSaveFile.Length);
            //    Console.WriteLine(fileName);
            //    urlAdreses[s] = (GetUploadUrl(fileName, true));
            ////}
            //Console.WriteLine("После нажатия любой клавиши начнется загрузка файлов");
            //Console.ReadLine();
            //Console.WriteLine("");
            ////foreach (var s in urlAdreses)
            ////{
            ////    Console.WriteLine($"File {s.Key} upload started.");
            ////    UploadAsync(s.Value, s.Key);
            ////}
            //Console.WriteLine();
        }
        //private static void UploadFileCallback2(Object sender, UploadFileCompletedEventArgs e)
        //{
        //    System.Threading.AutoResetEvent waiter = (System.Threading.AutoResetEvent)e.UserState; 
        //    try
        //    {
        //        string reply = System.Text.Encoding.UTF8.GetString(e.Result);

        //        Console.WriteLine($"File Uploaded {reply}" );
        //    }
        //    finally
        //    {
        //        // If this thread throws an exception, make sure that
        //        // you let the main application thread resume.
        //        //waiter.Set();
        //    }
        //}
        //private static void UploadProgressCallback(object sender, UploadProgressChangedEventArgs e)
        //{
        //    // Displays the operation identifier, and the transfer progress.
        //    Console.WriteLine("{0}    uploaded {1} of {2} bytes. {3} % complete...",
        //        (string)e.UserState,
        //        e.BytesSent,
        //        e.TotalBytesToSend,
        //        e.ProgressPercentage);
        //}
        //public static void UploadFileInBackground(string address, string fileName)
        //{
        //    WebClient client = new WebClient();
        //    Uri uri = new Uri(address);

        //    client.UploadFileCompleted += new UploadFileCompletedEventHandler(UploadFileCallback2);

        //    // Specify a progress notification handler.
        //    //client.UploadProgressChanged += new UploadProgressChangedEventHandler(UploadProgressCallback);
        //    client.UploadFileAsync(uri, "POST", fileName);
        //    Console.WriteLine($"File {fileName} upload started.");
        //}


        public static  string  GetUploadUrl(string fileName = "", bool overwrite = true)
        {
            string URl = "";
            var req = WebRequest.Create(new Uri("https://cloud-api.yandex.net/v1/disk/resources/upload?path=" 
                + "disk:/" + fileName + "&overwrite="+ overwrite.ToString()));
            
            ((HttpWebRequest)req).Accept = "*/*";
            req.Method = "GET";
            req.Headers["Authorization"] = string.Format("OAuth {0}", token);
            ((HttpWebRequest)req).Proxy = null;
            HttpWebResponse myHttpWebResponse = (HttpWebResponse)req.GetResponse();
            if (myHttpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                Stream dataStream = myHttpWebResponse.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string strResponse = reader.ReadToEnd();
                dataStream.Close();
                JObject response = JObject.Parse(strResponse);
                URl = (string)response.SelectToken("href");// поиск ссылки на УРЛ(загрузки) по ключу
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Plase reserved: {fileName}");
            Console.ResetColor();
            myHttpWebResponse.Close();
            return URl;
        }
        public static void UploadToUrl(string url,  string localFileNameWithPath,bool overwrite = true)
        {
            var discPath = GetUploadUrl(localFileNameWithPath, overwrite);
            try
            {
                WebClient client = new WebClient();
                client.Credentials = CredentialCache.DefaultCredentials;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"File {localFileNameWithPath} upload started.");
                Console.ResetColor();
                client.UploadFile(discPath, "POST", url);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"File {localFileNameWithPath} Uploaded!");
                Console.ResetColor();
                client.Dispose();
            }
            catch (Exception err)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(err.Message);
                Console.ResetColor();
            }

        }
        public static async void UploadAsync(string url, string localFileName)
        {
            await Task.Run(() => UploadToUrl(url, localFileName));
        }


    }
}
