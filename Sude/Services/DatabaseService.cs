using Dapper;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Data.SqlClient;
using Sude.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sude.Services
{
    public class DatabaseService
    {

        // ==========================================
        //           DASHBOARD METOTLARI
        // ==========================================
        public async Task<int> GetTotalDeviceCountAsync()
        {
            using var conn = SqlConnectionHelper.GetConnection();
            return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Devices");
        }
        public async Task<int> GetActiveUserCountAsync()
        {
            using var conn = SqlConnectionHelper.GetConnection();
            return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE IsActive = 1");
        }
        public async Task<int> GetTotalLogCountAsync()
        {
            using var conn = SqlConnectionHelper.GetConnection();
            return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Logs");
        }
        public async Task<Dictionary<string, int>> GetWeeklyActivityAsync()
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = @"
                SELECT 
                    CAST(Tarihi AS DATE) as Tarih, 
                    COUNT(*) as IslemSayisi
                FROM Logs
                WHERE Tarihi >= CAST(GETDATE() - 6 AS DATE)
                GROUP BY CAST(Tarihi AS DATE)";

            var result = await conn.QueryAsync(query);

            var haftalikVeri = new Dictionary<string, int>();
            for (int i = 6; i >= 0; i--)
            {
                haftalikVeri.Add(DateTime.Today.AddDays(-i).ToString("dd MMM"), 0);
            }

            foreach (var row in result)
            {
                DateTime tarih = (DateTime)row.Tarih;
                string formatliTarih = tarih.ToString("dd MMM");
                if (haftalikVeri.ContainsKey(formatliTarih))
                {
                    haftalikVeri[formatliTarih] = (int)row.IslemSayisi;
                }
            }

            return haftalikVeri;
        }
        public async Task<Dictionary<string, int>> GetMonthlyActivityAsync()
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = @"
                SELECT CAST(Tarihi AS DATE) as Tarih, COUNT(*) as IslemSayisi
                FROM Logs
                WHERE Tarihi >= CAST(GETDATE() - 29 AS DATE)
                GROUP BY CAST(Tarihi AS DATE)";
            var result = await conn.QueryAsync(query);

            var aylikVeri = new Dictionary<string, int>();
            for (int i = 29; i >= 0; i--)
                aylikVeri.Add(DateTime.Today.AddDays(-i).ToString("dd MMM"), 0);

            foreach (var row in result)
            {
                string formatliTarih = ((DateTime)row.Tarih).ToString("dd MMM");
                if (aylikVeri.ContainsKey(formatliTarih))
                    aylikVeri[formatliTarih] = (int)row.IslemSayisi;
            }
            return aylikVeri;
        }
        public async Task<Dictionary<string, int>> GetYearlyActivityAsync()
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = @"
                SELECT YEAR(Tarihi) as Yil, MONTH(Tarihi) as Ay, COUNT(*) as IslemSayisi
                FROM Logs
                WHERE Tarihi >= DATEADD(month, -11, DATEADD(day, 1-DAY(GETDATE()), CAST(GETDATE() AS DATE)))
                GROUP BY YEAR(Tarihi), MONTH(Tarihi)";
            var result = await conn.QueryAsync(query);

            var yillikVeri = new Dictionary<string, int>();
            for (int i = 11; i >= 0; i--)
                yillikVeri.Add(DateTime.Today.AddMonths(-i).ToString("MMM yy"), 0);

            foreach (var row in result)
            {
                DateTime date = new DateTime((int)row.Yil, (int)row.Ay, 1);
                string formatliTarih = date.ToString("MMM yy");
                if (yillikVeri.ContainsKey(formatliTarih))
                    yillikVeri[formatliTarih] = (int)row.IslemSayisi;
            }
            return yillikVeri;
        }
        public async Task<List<Log>> GetRecentLogsAsync(int count = 50)
        {
            using var conn = SqlConnectionHelper.GetConnection();

            string query = $@"SELECT TOP {count} l.Id, l.Tarihi, u.Username AS KullaniciAdi, t.TType AS IslemAdi, 
                             l.HedefUserId, l.HedefDeviceId, l.HedefSeriNo
                      FROM Logs l
                      INNER JOIN Users u ON l.UserId = u.Id
                      INNER JOIN TransactionTypes t ON l.TTypeId = t.Id
                      ORDER BY l.Tarihi DESC";

            var result = await conn.QueryAsync<Log>(query);
            return result.ToList();
        }

        // ==========================================
        //             USER METOTLARI
        // ==========================================
        public async Task<List<User>> GetUsersAsync()
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = "SELECT Id, Username, PasswordHash AS Password, Role, CreatedAt FROM Users WHERE IsActive = 1 AND Role = 'User' ORDER BY CreatedAt DESC";

            var result = await conn.QueryAsync<User>(query);
            return result.ToList();
        }
        public async Task<int> AddUserAsync(User user)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = @"INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt) 
                     VALUES (@Username, @Password, @Role, 1, GETDATE());
                     SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.ExecuteScalarAsync<int>(query, user);
        }
        public async Task UpdateUserAsync(User user)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = "UPDATE Users SET Username = @Username, PasswordHash = @Password, Role = @Role WHERE Id = @Id";
            await conn.ExecuteAsync(query, user);
        }
        public async Task SoftDeleteUserAsync(int userId, int adminId)
        {
            using var conn = SqlConnectionHelper.GetConnection();

            string query = "UPDATE Users SET IsActive = 0 WHERE Id = @Id";
            await conn.ExecuteAsync(query, new { Id = userId });

            await LogActionAsync(adminId, IslemTuru.KullaniciSilindi, targetUserId: userId);
        }
        public async Task<int> GetActiveAdminCountAsync()
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = "SELECT COUNT(*) FROM Users WHERE Role = 'Admin' AND IsActive = 1";
            return await conn.ExecuteScalarAsync<int>(query);
        }

        //===========================================
        //             LOG METOTLARI                   
        //===========================================
        public async Task<List<Log>> GetAllLogsAsync(Log filter = null)
        {
            using var conn = SqlConnectionHelper.GetConnection();

            string query = @"
        SELECT 
            l.Id, 
            l.Tarihi, 
            l.HedefUserId, 
            l.HedefDeviceId, 
            l.HedefSeriNo, 
            u.Username AS KullaniciAdi, 
            tt.TType AS IslemAdi 
        FROM Logs l
        LEFT JOIN Users u ON l.UserId = u.Id
        LEFT JOIN TransactionTypes tt ON l.TTypeId = tt.Id 
        WHERE 1=1";

            if (filter != null)
            {
                if (filter.FiltreBaslangic.HasValue) query += " AND l.Tarihi >= @FiltreBaslangic";
                if (filter.FiltreBitis.HasValue) query += " AND l.Tarihi <= @FiltreBitis";
                if (filter.SecilenIslemId.HasValue) query += " AND l.TTypeId = @SecilenIslemId";
                if (!string.IsNullOrWhiteSpace(filter.SecilenKullanici)) query += " AND u.Username LIKE @SecilenKullanici";
            }

            query += " ORDER BY l.Tarihi DESC";

            var result = await conn.QueryAsync<Log>(query, filter);
            return result.ToList();
        }
        public async Task<List<dynamic>> GetIslemTurleriTableAsync()
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = "SELECT Id, TType FROM TransactionTypes ORDER BY TType ASC";
            var result = await conn.QueryAsync(query);
            return result.ToList();
        }
        public async Task LogActionAsync(int performerId, IslemTuru tur, int? targetUserId = null, int? targetDeviceId = null, string hedefSeriNo = null)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = @"INSERT INTO Logs (UserId, HedefUserId, HedefDeviceId, HedefSeriNo, Tarihi, TTypeId) 
                     VALUES (@UserId, @HedefUserId, @HedefDeviceId, @HedefSeriNo, GETDATE(), @TTypeId)";

            await conn.ExecuteAsync(query, new
            {
                UserId = performerId,
                HedefUserId = targetUserId,
                HedefDeviceId = targetDeviceId,
                HedefSeriNo = hedefSeriNo, 
                TTypeId = (int)tur
            });
        }

        // ==========================================
        //             DEVICE METOTLARI
        // ==========================================
        public async Task<List<DeviceAsset>> GetDeviceAssetsAsync(int deviceId)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = "SELECT * FROM DeviceAssets WHERE DeviceId = @DeviceId ORDER BY StepOrder ASC";

            var result = await conn.QueryAsync<DeviceAsset>(query, new { DeviceId = deviceId });
            return result.ToList();
        }
        public async Task<List<Device>> GetDevicesAsync()
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = "SELECT Id, DeviceType, ProjectFileName, VideoFileName, ProjectFileData, VideoFileData, MainImageFileData FROM Devices";
            var result = await conn.QueryAsync<Device>(query);
            return result.ToList();
        }
        public async Task UpdateDeviceAsync(Device device, List<DeviceAsset> newAssets = null)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                string query = @"UPDATE Devices SET DeviceType = @DeviceType";

                if (device.ProjectFileData != null) query += ", ProjectFileName = @ProjectFileName, ProjectFileData = @ProjectFileData";
                if (device.VideoFileData != null) query += ", VideoFileName = @VideoFileName, VideoFileData = @VideoFileData";
                if (device.MainImageFileData != null) query += ", MainImageFileData = @MainImageFileData";

                query += " WHERE Id = @Id";

                await conn.ExecuteAsync(query, device, transaction);

                if (newAssets != null)
                {
                    await conn.ExecuteAsync("DELETE FROM DeviceAssets WHERE DeviceId = @Id", new { Id = device.Id }, transaction);

                    if (newAssets.Count > 0)
                    {
                        string assetQuery = @"INSERT INTO DeviceAssets (DeviceId, StepOrder, ContentType, ContentText, ContentData) 
                                              VALUES (@DeviceId, @StepOrder, @ContentType, @ContentText, @ContentData)";

                        foreach (var asset in newAssets)
                        {
                            asset.DeviceId = device.Id;
                            await conn.ExecuteAsync(assetQuery, asset, transaction);
                        }
                    }
                }
                transaction.Commit();
            }
            catch { transaction.Rollback(); throw; }
        }
        public async Task<bool> IsDeviceIdExistsAsync(int deviceId)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = "SELECT COUNT(1) FROM Devices WHERE Id = @Id";
            int count = await conn.ExecuteScalarAsync<int>(query, new { Id = deviceId });
            return count > 0;
        }
        public async Task<int> AddDeviceWithAssetsAsync(Device device, List<DeviceAsset> assets)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                string query = @"
                    SET IDENTITY_INSERT Devices ON;
                    
                    INSERT INTO Devices 
                    (Id, DeviceType, ProjectFileName, VideoFileName, ProjectFileData, VideoFileData, MainImageFileData) 
                    VALUES 
                    (@Id, @DeviceType, @ProjectFileName, @VideoFileName, @ProjectFileData, @VideoFileData, @MainImageFileData);
                    
                    SET IDENTITY_INSERT Devices OFF;";

                await conn.ExecuteAsync(query, device, transaction);

                if (assets != null && assets.Count > 0)
                {
                    string assetQuery = @"INSERT INTO DeviceAssets (DeviceId, StepOrder, ContentType, ContentText, ContentData) 
                                          VALUES (@DeviceId, @StepOrder, @ContentType, @ContentText, @ContentData)";

                    foreach (var asset in assets)
                    {
                        asset.DeviceId = device.Id;
                        await conn.ExecuteAsync(assetQuery, asset, transaction);
                    }
                }
                transaction.Commit();
                return device.Id;
            }
            catch { transaction.Rollback(); throw; }
        }

        //===========================================
        //             SETTING METOTLARI
        //===========================================
        public async Task UpdateAdminCredentialsAsync(int userId, string newUsername, string newPassword)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = "UPDATE Users SET Username = @Username, PasswordHash = @Password WHERE Id = @Id AND Role = 'Admin'";
            await conn.ExecuteAsync(query, new { Username = newUsername, Password = newPassword, Id = userId });
        }
        public async Task<List<Log>> GetLogsOlderThanAsync(DateTime date)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = @"
                SELECT l.Id, l.Tarihi, u.Username AS KullaniciAdi, tt.TType AS IslemAdi, 
                       l.HedefUserId, l.HedefDeviceId, l.HedefSeriNo
                FROM Logs l
                LEFT JOIN Users u ON l.UserId = u.Id
                LEFT JOIN TransactionTypes tt ON l.TTypeId = tt.Id
                WHERE l.Tarihi < @Date
                ORDER BY l.Tarihi ASC";

            var result = await conn.QueryAsync<Log>(query, new { Date = date });
            return result.ToList();
        }

        public async Task DeleteLogsOlderThanAsync(DateTime date)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            string query = "DELETE FROM Logs WHERE Tarihi < @Date";
            await conn.ExecuteAsync(query, new { Date = date });
        }

        // ==========================================
        //             LOGİN METODU
        // ==========================================
        public async Task<User> LoginAsync(string username, string password)
        {
            using var conn = SqlConnectionHelper.GetConnection();

            string query = @"SELECT Id, Username, PasswordHash AS Password, Role, CreatedAt 
                     FROM Users 
                     WHERE Username = @Username AND PasswordHash = @Password AND IsActive = 1";

            var user = await conn.QueryFirstOrDefaultAsync<User>(query, new { Username = username, Password = password });
            return user;
        }

        // ============================================
        //          SERİ NUMARASI METOTLARI
        // ============================================
        public byte[] GetLastSerial(int deviceId)
        {
            string query = "SELECT TOP 1 SerialNumber FROM SerialNumbers WHERE DeviceId = @DeviceId ORDER BY Id DESC";

            using var conn = SqlConnectionHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@DeviceId", deviceId);
            conn.Open();
            var result = cmd.ExecuteScalar();

            if (result != null) return (byte[])result;

            byte[] serial = new byte[6];
            serial[0] = 0x02;
            serial[1] = 0xD6;
            serial[2] = (byte)deviceId;
            serial[3] = (byte)(DateTime.Now.Year % 100);
            serial[4] = 0x00;
            serial[5] = 0x00;
            return serial;
        }
        public (int Id, byte[] SerialBytes) NewSerial(int deviceId, int firmCode)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("GenerateCounter", conn);

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DeviceId", deviceId);
            cmd.Parameters.AddWithValue("@FirmCode", firmCode);

            conn.Open();
            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int id = Convert.ToInt32(reader["ID"]);
                byte[] serialBytes = (byte[])reader["SerialNumber"];
                return (id, serialBytes);
            }
            throw new Exception("Yeni seri numara oluşturulamadı!");
        }
        public void UpdateAssigned(int id, bool success)
        {
            using var conn = SqlConnectionHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("UPDATE SerialNumbers SET IsAssigned=@assigned WHERE Id=@ID", conn);

            cmd.Parameters.AddWithValue("@assigned", success ? 1 : 0);
            cmd.Parameters.AddWithValue("@ID", id);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}