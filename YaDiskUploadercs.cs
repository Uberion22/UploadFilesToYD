using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace UploadFilesToYD
{
    public static class YaDiskUploader
    {
        //Здесь нужно указать ключ авторизации для загрузки файлов на диск
        const string token = "";
        // API Яндекс Диска для загрузки файлов
        const string baseUri = "https://cloud-api.yandex.net/v1/disk/resources/upload/";
        /// <summary>
        /// Метод принимает 2 аргумента : локальный путь к файлам в формате L:\test\folder\ ,
        /// а так же  путь к папке на Яндекс диске без учета корневого каталога.
        /// Чтобы скопировать файлы в папку "Music" на Диске,
        /// нужно ввести в качестве конечной папки диска "Music/".
        /// По умолчанию файлы копируются в корневой каталог.
        /// </summary>
        public static void UploadToDisc(string localFileDirectory = @"L:\test\", string discDirecrory = "")
        {
            try
            {
                // ищем все фалы в директори(в том числе вложенные) 
                string[] files = Directory.GetFiles(@localFileDirectory, "*.*", SearchOption.AllDirectories);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Получение списка файлов  в каталоге");
                Console.ResetColor();
                foreach (var s in files)
                {
                    var fileName = s.Remove(0, localFileDirectory.Length);
                    UploadAsync(s, fileName, discDirecrory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Метод принимает 3 параметра и создает асинхронную задачу загрузки на Диск
        /// </summary>
        /// <param name="LocalAdress">ссылка на полный адрес файла на ПК </param>
        /// <param name="localFileName"> Имя файла</param>
        /// <param name="diskDir">путь сохранения файла на Яндекс Диске</param>
        public static async void UploadAsync(string LocalAdress, string localFileName, string diskDir)
        {
            await Task.Run(() => UploadToUrl(LocalAdress, localFileName, diskDir));
        }
        /// <summary>
        /// Метод формирует запрос к Яндекс Диску с целью получения ссылки для выгрузки локального файла,
        /// При успешном выполнении метод возвращает ссылку.
        /// </summary>
        /// <param name="fileName"> имя выгружаемого фалйа</param>
        /// <param name="overwrite">пометка на перезапись при нахождении файлов с одинаковыми названиями</param>
        /// <param name="diskFolder"> конечная директория на диске</param>
        /// <returns></returns>
        public static string GetUploadUrl(string fileName = "", bool overwrite = true, string diskFolder = "")
        {
            string URl = "";
            var req = WebRequest.Create(new Uri(baseUri + "?path="
                + "disk:/" + @diskFolder + fileName + "&overwrite=" + overwrite.ToString()));
            ((HttpWebRequest)req).Accept = "*/*";
            req.Method = "GET";
            // аваторизационный ключ для работы с диском
            req.Headers["Authorization"] = string.Format("OAuth {0}", token);
            ((HttpWebRequest)req).Proxy = null;
            try
            {
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)req.GetResponse();
                if (myHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    Stream dataStream = myHttpWebResponse.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string strResponse = reader.ReadToEnd();
                    dataStream.Close();
                    JObject response = JObject.Parse(strResponse);
                    URl = (string)response.SelectToken("href");// выбор ссылки на URL(загрузки) по ключу
                }
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Plase reserved: {fileName}");
                Console.ResetColor();
                myHttpWebResponse.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return URl;
        }
        /// <summary>
        /// Метод выгружает файлы на Яндекс диск и принимает 4 параметра
        /// </summary>
        /// <param name="fileName"> имя файла</param>
        /// <param name="localFileNameWithPath">путь к локальному </param>
        /// <param name="discDir">путь к папке на Яндекс Диске</param>
        /// <param name="overwrite"> пометка на перезапись файлов с одинаковми именами</param>
        public static void UploadToUrl(string fileName, string localFileNameWithPath, string discDir = "", bool overwrite = true)
        {
            try
            {
                var discPath = GetUploadUrl(localFileNameWithPath, overwrite, discDir);
                WebClient client = new WebClient();
                client.Credentials = CredentialCache.DefaultCredentials;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"File {localFileNameWithPath} upload started.");
                Console.ResetColor();
                client.UploadFile(discPath, "POST", fileName);
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
    }
}
