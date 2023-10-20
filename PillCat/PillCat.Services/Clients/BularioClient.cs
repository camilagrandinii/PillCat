using System.Text.Json;

namespace PillCat.Services.Clients
{
    public class BularioClient
    {
        public async Task<dynamic> SearchMed(string name)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://bula.vercel.app/pesquisar?nome=" + name;

                // Sending a GET request
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");

                    throw new Exception();
                }
            }
        }

        public async Task<dynamic> GetLeaflet(dynamic stringMedInfo)
        {
            JsonDocument medInfo = JsonDocument.Parse(stringMedInfo);
            if (medInfo != null)
            {
                // Retrieve the value
                    JsonElement idBulaProfissionalProtegidoValue = medInfo.RootElement
                    .GetProperty("content")[0] // Assuming there's at least one element in the "content" array
                    .GetProperty("idBulaProfissionalProtegido");

                    string idBulaProfissionalProtegido = idBulaProfissionalProtegidoValue.GetString();

                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = "https://bula.vercel.app/bula?id=" + idBulaProfissionalProtegido;

                    // Sending a GET request
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JsonDocument jsonDocument = JsonDocument.Parse(responseBody);

                        if (jsonDocument != null)
                        {                        
                            JsonElement pdfElement = jsonDocument.RootElement.GetProperty("pdf");
                            var url = pdfElement.GetString();
                            return url;

                        }
                        else
                        {
                            Console.WriteLine("Error: não foi possivel realizar a requisicao!");

                            throw new Exception();
                        }

                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");

                        throw new Exception();
                    }
                }
            }

            return null;
        }
    }
}
