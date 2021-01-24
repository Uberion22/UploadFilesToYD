using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace UploadFilesToYD
{
    public static class YaDiskUploader
    {
        //Перед работой с программой нужно указать ключ авторизации для загрузки файлов на Яндекс Диск
        const string token = "";
        // API Яндекс Диска для загрузки файлов
        const string baseUri = "https://cloud-api.yandex.net/v1/disk/resources/upload/";
        /// <summary>
        /// Метод получает список  файлов хранящихся в указанном каталоге,
        /// после чего пытается выгрузить каждый найденный файл на Яндекс Диск 
        /// Метод принимает 2 аргумента : локальный путь к файлам в формате L:\test\folder\ ,
        /// а так же  путь к папке на Яндекс Диске без учета корневого каталога.
        /// Чтобы скопировать файлы в папку на Яндекс Диске,
        /// нужно ввести адрес  конечной папки диска. Пример: "Music/".
        /// По умолчанию файлы копируются в корневой каталог.
        /// </summary>
        public static void UploadToDisk(string localFileDirectory = @"L:\test\", string diskDirecrory = "")
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
                    UploadAsync(s, fileName, diskDirecrory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Метод принимает 3 параметра и создает асинхронную задачу загрузки файла на Яндекс Диск
        /// </summary>
        /// <param name="LocalAdress">ссылка на полный адрес файла хранящегося на ПК </param>
        /// <param name="localFileName"> Имя файла</param>
        /// <param name="diskDir">путь сохранения файла на Яндекс Диске</param>
        public static async void UploadAsync(string LocalAdress, string localFileName, string diskDir)
        {
            await Task.Run(() => UploadToUrl(LocalAdress, localFileName, diskDir));
        }

        /// <summary>
        /// Метод формирует запрос к Яндекс Диску с целью получения ссылки для выгрузки локального файла.
        /// При успешном выполнении метод возвращает ссылку.
        /// Требуется вторизационный ключ. Указан как константа выше.
        /// </summary>
        /// <param name="fileName"> Имя выгружаемого фалйа</param>
        /// <param name="overwrite">Пометка на перезапись. При нахождении файлов с одинаковыми названиями,
        /// файлы храняящиеся на диске заменяюся новыми</param>
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
        /// Метод служит для выгрузки файлов на Яндекс Диск с помощью специальной ссылки(полученной от ЯД) 
        /// </summary>
        /// <param name="fileName"> Имя файла</param>
        /// <param name="localFileNameWithPath">Адрес локального файла включающий его имя</param>
        /// <param name="diskDir">Путь к папке на Яндекс Диске</param>
        /// <param name="overwrite"> Пометка на перезапись файлов с одинаковми именами</param>
        public static void UploadToUrl(string fileName, string localFileNameWithPath, string diskDir = "", bool overwrite = true)
        {
            try
            {
                var diskPath = GetUploadUrl(localFileNameWithPath, overwrite, diskDir);
                WebClient client = new WebClient();
                client.Credentials = CredentialCache.DefaultCredentials;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"File {localFileNameWithPath}: Upload started!");
                Console.ResetColor();
                client.UploadFile(diskPath, "POST", fileName);
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
