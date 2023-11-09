using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PillCat.Models;
using PillCat.Models.DbContexts;
using PillCat.Models.Responses;
using PillCat.Services.Clients;
using PillCat.Services.Interfaces;

namespace PillCat.Services
{
    public class PillsService : BaseService, IPillsService
    {
        private readonly IOcrClient _ocrClient;
        private readonly BularioClient _bularioClient;
        private readonly PillContext _context;

        public PillsService(PillContext context, IOcrClient ocrClient)
        {
            _context = context;
            _ocrClient = ocrClient;
            _bularioClient = new BularioClient();
        }

        public async Task<OcrTextResponse> GetImageTextFromUrl(string url)
        {
            return await _ocrClient.GetImageTextFromUrl("K81989641788957", url);
        }

        public async Task<OcrTextResponse> GetImageTextFromFile(string mimeType, string fileContent)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(fileContent), "base64Image" }
            };

            return await _ocrClient.GetImageTextFromFile("K81989641788957", content);
        }

        public async Task<OcrTextResponse> GetImageTextFromLocalFileUrl(string url)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(url), "url" }
            };

            return await _ocrClient.GetImageTextFromLocalFileUrl("K81989641788957", content);
        }

        public async Task<dynamic> GetInformationFromPill(string name)
        {
            return await _bularioClient.SearchMed(name);
        }

        public async Task<dynamic> GetLeaflet(dynamic pillInfo)
        {
            return await _bularioClient.GetLeaflet(pillInfo);
        }

        public async Task<IEnumerable<Pill>> GetPills()
        {
            if (_context.Pills == null)
            {
                return null;
            }

            return await _context.Pills.ToListAsync();
        }

        public async Task<IEnumerable<TodayPillsResponse>> GetTodayPills()
        {
            if (_context.Pills == null)
            {
                return null;
            }

            var currentDate = DateTime.Today;

            var pillsListResult = await _context.Pills
                .Include(p => p.UsageRecord)
                .Select(p => new TodayPillsResponse
                {
                    pill = p,
                    UsageRecord = p.UsageRecord.Where(ur => ur.DateTime.Date == currentDate).ToList(),
                })
                .ToListAsync();

            return pillsListResult;
        }

        public async Task<Pill> GetPill(string name)
        {
            if (_context.Pills == null)
            {
                return null;
            }

            var pills = await _context.Pills.ToListAsync();
            Pill pill = null;

            Parallel.ForEach(pills, (u, state) =>
            {
                if (u.Name == name)
                {
                    pill = u;
                    state.Break();
                }
            });

            if (pill == null)
            {
                return null;
            }

            return pill;
        }

        public async Task<Pill> PutPill(int id, Pill pill, Pill completePillInfo)
        {
            if (id != pill.Id)
            {
                return null;
            }

            _context.Entry(completePillInfo).CurrentValues.SetValues(pill);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PillExists(id))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return pill;
        }

        public async Task<Pill> PutPillSimple(Pill pill)
        {
            _context.Entry(pill).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return pill;
        }


        public async Task<List<UsageRecord>> PutUsageRecordOfPill(string name, bool usageState)
        {
            Pill pill = await GetPill(name);

            var currentDate = DateTime.Today;

            var putUsageRecordOfPillResult = await _context.Pills
                .Include(p => p.UsageRecord)
                .Select(p => new
                {
                    Pills = p,
                    UsageRecords = p.UsageRecord.Where(ur => ur.DateTime.Date == currentDate)
                })
                .Where(p => p.Pills.Name.Equals(name))
                .Select(p => PutUsageRecord(p.UsageRecords, usageState))
                .ToListAsync();

            if (usageState)
            {
                pill.QuantityInBox--;
            }

            _context.Entry(pill).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return putUsageRecordOfPillResult;
        }

        public async Task<Pill> PostPill(Pill pill)
        {
            if (_context.Pills == null)
            {
                throw new InvalidOperationException("Entity set 'PillContext.Pills' is null.");
            }

            var pillAux = new Pill(pill);
            
            _context.Pills.Add(pill);
            await _context.SaveChangesAsync();
            
            pillAux.Id = pill.Id;
           
            return pillAux;
        }

        public async Task<bool> DeletePill(int id)
        {
            if (_context.Pills == null)
            {
                throw new InvalidOperationException("Pills set is not initialized.");
            }

            var pill = await _context.Pills.FindAsync(id);
            if (pill == null)
            {
                return false;
            }

            _context.Pills.Remove(pill);
            await _context.SaveChangesAsync();

            return true;
        }

        private bool PillExists(int id)
        {
            return (_context.Pills?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private UsageRecord PutUsageRecord(IEnumerable<UsageRecord> usageRecord, bool usageState)
        {
            var currentDate = DateTime.Today;

            UsageRecord updatedUsageRecord =
                usageRecord.FirstOrDefault(u => u.PillUsed == false);

            if(updatedUsageRecord == null) {
                throw new Exception("There are no other pills to take today");
            }

            updatedUsageRecord.PillUsed = usageState;
            updatedUsageRecord.DateTime = DateTime.Now;

            return updatedUsageRecord;
        }
    }
}
