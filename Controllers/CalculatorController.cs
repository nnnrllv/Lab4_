using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SQLite;

namespace Lab4_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        // Метод для сохранения данных в формате XML
        [HttpPost]
        [Route("SaveToXml/{fileName}")]
        public ActionResult SaveToXml(string fileName, [FromBody] object data)
        {
            DataSet dataSet = new DataSet("myData");
            DataTable dataTable = new DataTable("myTable");
            dataTable.ReadXml(new StringReader(data.ToString()));

            dataSet.Tables.Add(dataTable);
            dataSet.WriteXml(fileName);
            return Ok("Данные сохранены в XML");
        }

        // Метод для загрузки данных из XML
        [HttpGet]
        [Route("LoadFromXml/{fileName}")]
        public ActionResult LoadFromXml(string fileName)
        {
            try
            {
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(fileName);
                return Ok(dataSet.Tables[0].Rows);
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка при загрузке данных из XML: " + ex.Message);
            }
        }

        // Метод для сохранения данных в базе данных SQLite
        [HttpPost]
        [Route("SaveToSQLite/{dbFileName}")]
        public ActionResult SaveToSQLite(string dbFileName, [FromBody] object data)
        {
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={dbFileName}.db"))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS MyTable (MyData TEXT)", connection))
                {
                    command.ExecuteNonQuery();

                    foreach (var item in (object[])data)
                    {
                        using (SQLiteCommand insertCommand = new SQLiteCommand("INSERT INTO MyTable (MyData) VALUES (@myData)", connection))
                        {
                            insertCommand.Parameters.AddWithValue("@myData", item.ToString());
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            return Ok("Данные сохранены в SQLite");
        }

        // Метод для загрузки данных из базы данных SQLite
        [HttpGet]
        [Route("LoadFromSQLite/{dbFileName}")]
        public ActionResult LoadFromSQLite(string dbFileName)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection($"Data Source={dbFileName}.db"))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("SELECT * FROM MyTable", connection))
                    {
                        using (SQLiteDataReader reader = selectCommand.ExecuteReader())
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);
                            return Ok(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка при загрузке данных из SQLite: " + ex.Message);
            }
        }
    }
    public class Program
    {
        static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
            bool exit = false; // Указывает, следует ли завершать работу программы

            while (!exit)
            {
                string input = Console.ReadLine(); // Вводимые данные
                double inputNumber;
                bool isDouble = double.TryParse(input, out inputNumber);

                if (isDouble)
                {
                    Calculator.ProcessInputNumber(inputNumber);
                }
                else
                {
                    Calculator.ProcessInputOperation(input);
                }
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Program>();
    }
}
