using Newtonsoft.Json;

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
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);

                    return jsonResponse;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");

                    throw new Exception();
                }
            }
        }

        public async Task<dynamic> GetLeaflet(dynamic medInfo)
        {
            if (medInfo != null && medInfo.content != null && medInfo.content.Count > 0 && medInfo.content[0] != null && medInfo.content[0].idBulaProfissionalProtegido != null)
            {
                // Retrieve the value
                string idBulaProfissionalProtegido = medInfo.content[0].idBulaProfissionalProtegido;

                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = "https://bula.vercel.app/bula?id=" + idBulaProfissionalProtegido;

                    // Sending a GET request
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);

                        if (jsonResponse != null)
                        {                            
                            return jsonResponse;
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
