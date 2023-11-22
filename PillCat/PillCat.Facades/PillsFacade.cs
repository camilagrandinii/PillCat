using AutoMapper;
using Microsoft.AspNetCore.Http;
using PillCat.Facades.Interfaces;
using PillCat.Models;
using PillCat.Models.Requests;
using PillCat.Models.Responses;
using PillCat.Services.Interfaces;
using System.Text.RegularExpressions;

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
            pillRequest.Name = pillRequest.Name.ToLower();

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

        public async Task<OcrInfo> GetImageTextFromFile(IFormFile file)
        {
            OcrInfo finalInfo = new OcrInfo();
            var leafletInfo = "";
            var leafletInfoValid = "";
            var quantComprimidos = "";
            var pillName = "";

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

                var getImageTextResult = await _pillsService.GetImageTextFromFile(mimeType, fileContent);

                if (!getImageTextResult.Error)
                {
                    string[] textFromImage = (getImageTextResult.ParsedResults[0].ParsedText).Split(new[] { "\r\n" }, StringSplitOptions.None);

                    foreach (String textPart in textFromImage)
                    {
                        var resultPill = await GetInformationFromPill(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""));

                        if (!resultPill.Contains("totalElements\":0"))
                        {
                            leafletInfo = await GetLeafletFromPill(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""));

                            if (leafletInfo.Length > 0)
                            {
                                leafletInfoValid = leafletInfo;
                                pillName = Regex.Replace(textPart, "[^a-zA-Z0-9]", "");
                            }
                        }
                        if (Regex.Replace(textPart, "[^a-zA-Z0-9]", "").Contains("comprimidos"))
                        {
                            quantComprimidos = FilterNumbers(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""), @"\d");
                        }
                    }
                }
                else
                {
                    finalInfo.Error = true;
                    finalInfo.Message = "A imagem não está em um formato suportado.";
                    finalInfo.Name = pillName;
                    finalInfo.leafletLink = leafletInfoValid;
                    finalInfo.QuantityInBox = quantComprimidos;
                    return finalInfo;
                }

                if (pillName == "" || leafletInfoValid == "" || quantComprimidos == "")
                {
                    finalInfo.Error = true;
                    finalInfo.Message = "Não foi possível identificar o remedio!";
                }
                else
                {
                    finalInfo.Error = false;
                    finalInfo.Message = "Remédio encontrado!";
                }

                finalInfo.Name = pillName;
                finalInfo.leafletLink = leafletInfoValid;
                finalInfo.QuantityInBox = quantComprimidos;

                int aumountOfPillsInt = int.Parse(quantComprimidos);

                // TODO:
                // Extrair o nome do remédio para procura-lo no banco de dados por meio do nome
                // Substituir o placeholder "rivotril" por uma variável que contenha o nome do remédio
                // Antes usar o método GetPill("rivotril"), faça um ToLowerCase na string em questão

                // No teste que eu fiz ele não conseguiu extrair o nome do remédio, MAS isso não pode acontecer
                // Tenta resolver isso please <3

                Pill pill = await _pillsService.GetPill("rivotril");

                pill.QuantityInBox = aumountOfPillsInt;

                await _pillsService.PutPillSimple(pill);

                return finalInfo;
            }
        }

        public async Task<OcrInfo> GetImageTextFromBase64String(GetImageTextRequest base64File)
        {
            OcrInfo finalInfo = new OcrInfo();
            var leafletInfo = "";
            var leafletInfoValid = "";
            var quantComprimidos = "";
            var pillName = "";

            string mimeType = ObtainMimeType(base64File.Image);

            var fileContent = base64File.Image.StartsWith("data:") ? base64File.Image : $"data:{mimeType};base64,{base64File}";

            var getImageTextResult = await _pillsService.GetImageTextFromFile(mimeType, fileContent);

            if (!getImageTextResult.Error)
            {
                string[] textFromImage = (getImageTextResult.ParsedResults[0].ParsedText).Split(new[] { "\r\n" }, StringSplitOptions.None);

                foreach (String textPart in textFromImage)
                {
                    var resultPill = await GetInformationFromPill(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""));

                    if (!resultPill.Contains("totalElements\":0"))
                    {
                        leafletInfo = await GetLeafletFromPill(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""));

                        if (leafletInfo.Length > 0)
                        {
                            leafletInfoValid = leafletInfo;

                            if (string.IsNullOrEmpty(pillName)){ 
                                pillName = Regex.Replace(textPart, "[^a-zA-Z0-9]", "");
                            }
                        }
                    }
                    if (Regex.Replace(textPart, "[^a-zA-Z0-9]", "").Contains("comprimidos"))
                    {
                        quantComprimidos = FilterNumbers(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""), @"\d");
                    }
                }
            }
            else
            {
                finalInfo.Error = true;
                finalInfo.Message = "A imagem não está em um formato suportado.";
                finalInfo.Name = pillName;
                finalInfo.leafletLink = leafletInfoValid;
                finalInfo.QuantityInBox = quantComprimidos;
                return finalInfo;
            }

            if (pillName == "" || leafletInfoValid == "" || quantComprimidos == "")
            {
                finalInfo.Error = true;
                finalInfo.Message = "Não foi possível identificar o remedio!";
            }
            else
            {
                finalInfo.Error = false;
                finalInfo.Message = "Remédio encontrado!";
            }

            int aumountOfPillsInt = int.Parse(quantComprimidos);

            finalInfo.Name = pillName.ToLower();
            finalInfo.leafletLink = leafletInfoValid;
            finalInfo.QuantityInBox = aumountOfPillsInt.ToString();

            Pill pill = await _pillsService.GetPill(finalInfo.Name);

            pill.QuantityInBox = aumountOfPillsInt;

            await _pillsService.PutPillSimple(pill);

            return finalInfo;
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

        private string FilterNumbers(string input, string pattern)
        {
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(input);

            string resultado = "";
            foreach (Match match in matches)
            {
                resultado += match.Value;
            }

            return resultado;
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

        private string ObtainMimeType(string base64File)
        {
            int index = base64File.IndexOf(';');

            if (index != -1)
            {
                string fileTypeSubstring = base64File.Substring(5, index - 5);

                if (!string.IsNullOrEmpty(fileTypeSubstring))
                {
                    return fileTypeSubstring;
                }
            }

            return string.Empty;
        }

    }
}
