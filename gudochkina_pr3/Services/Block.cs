using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.IO;
namespace gudochkina_pr3.Services
{
    public class Block
    {
        private static readonly string BlockFilePath = "block_time.json";

        public static DateTime? GetBlockEndTime()
        {
            try
            {
                if (File.Exists(BlockFilePath))
                {
                    var json = File.ReadAllText(BlockFilePath);
                    var blockTime = JsonSerializer.Deserialize<DateTime>(json);

                    if (blockTime > DateTime.Now)
                    {
                        return blockTime;
                    }
                    else
                    {
                        File.Delete(BlockFilePath);
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                File.Delete(BlockFilePath);
            }
            return null;
        }

        public static void SetBlockEndTime(DateTime blockEndTime)
        {
            try
            {
                var json = JsonSerializer.Serialize(blockEndTime);
                File.WriteAllText(BlockFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения времени блокировки: {ex.Message}");
            }
        }

        public static void ClearBlockTime()
        {
            try
            {
                if (File.Exists(BlockFilePath))
                    File.Delete(BlockFilePath);
            }
            catch { }
        }
    }
}
