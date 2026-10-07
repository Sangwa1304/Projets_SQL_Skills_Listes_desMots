using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Projets_SQL_Skills_Listes_desMots
{
    public static class DictionaryApi
    {
        // Réutiliser un HttpClient singleton pour de meilleures performances
        private static readonly HttpClient httpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Projets_SQL_Skills_Listes_desMots/1.0 (+https://github.com/Sangwa1304)");
            return client;
        }

        /// <summary>
        /// Interroge l'API de fr.wiktionary.org pour le mot fourni et retourne le JSON brut.
        /// Exemple d'usage de l'API : action=query&prop=revisions&rvprop=content&format=json&titles={mot}
        /// </summary>
        /// <param name="mot">Le mot à rechercher (ex: "chat")</param>
        /// <param name="cancellationToken">Token d'annulation optionnel</param>
        /// <returns>JSON brut renvoyé par l'API (string)</returns>
        public static async Task<string> GetWiktionaryJsonAsync(string mot, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(mot))
                throw new ArgumentException("Le mot ne peut pas être vide.", nameof(mot));

            // Encoder le mot pour l'URL
            var encoded = HttpUtility.UrlEncode(mot);

            // Requête : on demande le wikitext de la page (révisions) et on demande le format JSON
            var url = $"https://fr.wiktionary.org/w/api.php?action=query&prop=revisions&rvprop=content&format=json&titles={encoded}";

            using (var resp = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false))
            {
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return json;
            }
        }
    }
}
