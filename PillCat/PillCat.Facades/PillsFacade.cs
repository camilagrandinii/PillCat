using AutoMapper;
using Microsoft.AspNetCore.Http;
using PillCat.Facades.Interfaces;
using PillCat.Models;
using PillCat.Models.Responses;
using PillCat.Services.Interfaces;

namespace PillCat.Facades
{
    public class PillsFacade : BaseFacade, IPillsFacade
    {
        private readonly IPillsService _pillsService;

        public PillsFacade(IPillsService pillsService)
        {
            _pillsService = pillsService;
        }

        public async Task<Pill> PostPill(PostPillRequest pillRequest)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PostPillRequest, Pill>();
            });

            var mapper = configuration.CreateMapper();

            Pill enrichedPill = mapper.Map<Pill>(pillRequest);

            enrichedPill.setPillId();

            var leafletInfo = await GetLeafletFromPill(enrichedPill.Name);

            enrichedPill.Leaflet = leafletInfo;

            enrichedPill.UsageRecord = new List<UsageRecord>();

            for (int i = 0; i < enrichedPill.PeriodOfTreatment.Amount; i += enrichedPill.FrequencyOfPill.IntervalPeriod)
            {
                enrichedPill.UsageRecord.Add(new UsageRecord { DateTime = DateTime.Today.AddDays(i), Pill = enrichedPill, PillUsed = false, UsageRecordId = Guid.NewGuid().GetHashCode() });
            }

            await _pillsService.PostPill(enrichedPill);

            return enrichedPill;
        }

        public async Task<Pill> EnrichPill(PostPillRequest postPillRequest)
        {
            var enrichedPill = MapPostRequestToPill(postPillRequest);

            enrichedPill.setPillId();

            var leafletInfo = await GetLeafletFromPill(enrichedPill.Name);

            enrichedPill.Leaflet = leafletInfo;

            enrichedPill.UsageRecord = new List<UsageRecord>();

            for (int i = 0; i < enrichedPill.PeriodOfTreatment.Amount; i += enrichedPill.FrequencyOfPill.IntervalPeriod)
            {
                enrichedPill.UsageRecord.Add(new UsageRecord { DateTime = DateTime.Today.AddDays(i), Pill = enrichedPill, PillUsed = false, UsageRecordId = Guid.NewGuid().GetHashCode() });
            }

            return enrichedPill;
        }

        public async Task<OcrTextResponse> GetImageTextFromUrl(string url)
        {
            return await _pillsService.GetImageTextFromUrl(url);
        }

        public async Task<OcrTextResponse> GetImageTextFromFile(IFormFile file)
        {
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                var mimeType = MimeTypes.GetMimeType(file.FileName);

                if (mimeType != "image/jpeg" && mimeType != "image/png")
                {
                    throw new ArgumentException("A imagem não está em um formato suportado.");
                }

                Console.WriteLine("Imagem válida");

                byte[] bytes = ms.ToArray();

                string base64 = Convert.ToBase64String(bytes);
                string fileContent = $"data:{mimeType};base64,{base64}";

                var ocrTextResponse = await _pillsService.GetImageTextFromFile(mimeType, fileContent);

                Pill pill = await _pillsService.GetPill("rivotril");
                pill.QuantityInBox = 3;

                await _pillsService.PutPillSimple(pill);

                return ocrTextResponse;
            }
        }

        public async Task<OcrTextResponse> GetImageTextFromLocalFileUrl(string url)
        {
            return await _pillsService.GetImageTextFromLocalFileUrl(url);
        }

        public async Task<dynamic> GetInformationFromPill(string name)
        {
            return await _pillsService.GetInformationFromPill(name);
        }

        public async Task<dynamic> GetLeafletFromPill(string name)
        {
            var informationFromPillResult = await _pillsService.GetInformationFromPill(name);
            var url = await _pillsService.GetLeaflet(informationFromPillResult);
            return url;
        }

        public Task<IEnumerable<Pill>> GetPills()
        {
            return _pillsService.GetPills();
        }

        public Task<IEnumerable<TodayPillsResponse>> GetTodayPills()
        {
            return _pillsService.GetTodayPills();
        }

        public async Task<string> GetPillLeafLet(string name)
        {
            Pill pill = await _pillsService.GetPill(name);

            return pill.Leaflet;
        }

        public Task<Pill> GetPill(string name)
        {
            return _pillsService.GetPill(name);
        }

        public async Task<Pill> PutPill(PostPillRequest updatePillRequest)
        {
            Pill completePillInfo = await _pillsService.GetPill(updatePillRequest.Name);

            Pill pill = MapPostRequestToPill(updatePillRequest);
            pill.Id = completePillInfo.Id;

            return await _pillsService.PutPill(pill.Id, pill, completePillInfo);
        }

        public Task<bool> DeletePill(int id)
        {
            return _pillsService.DeletePill(id);
        }

        public Task<List<UsageRecord>> PutUsageRecordOfPill(string name, bool usageState)
        {
            return _pillsService.PutUsageRecordOfPill(name, usageState);
        }

        private Pill MapPostRequestToPill(PostPillRequest postPillRequest)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PostPillRequest, Pill>();
            });

            var mapper = configuration.CreateMapper();

            Pill enrichedPill = mapper.Map<Pill>(postPillRequest);

            return enrichedPill;
        }
    }
}
