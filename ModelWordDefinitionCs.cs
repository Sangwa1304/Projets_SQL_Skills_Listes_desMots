using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Projets_SQL_Skills_Listes_desMots
{
    /// <summary>
    /// Modèles POCO pour désérialiser la réponse JSON de l'API MediaWiki (fr.wiktionary).
    /// Utiliser System.Text.Json.JsonSerializer.Deserialize&lt;MediaWikiResponse&gt;(json).
    /// </summary>
    public class MediaWikiResponse
    {
        [JsonPropertyName("batchcomplete")]
        public string BatchComplete { get; set; }

        [JsonPropertyName("query")]
        public Query Query { get; set; }
    }

    public class Query
    {
        // "pages" contient des clés dynamiques (pageid ou "-1"), on désérialise donc en dictionnaire.
        [JsonPropertyName("pages")]
        public Dictionary<string, Page> Pages { get; set; }
    }

    public class Page
    {
        [JsonPropertyName("pageid")]
        public int? PageId { get; set; }

        [JsonPropertyName("ns")]
        public int Ns { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("missing")]
        public string Missing { get; set; } // présent si la page est manquante

        [JsonPropertyName("revisions")]
        public List<Revision> Revisions { get; set; }
    }

    public class Revision
    {
        [JsonPropertyName("revid")]
        public int? RevId { get; set; }

        [JsonPropertyName("parentid")]
        public int? ParentId { get; set; }

        [JsonPropertyName("contentformat")]
        public string ContentFormat { get; set; }

        [JsonPropertyName("contentmodel")]
        public string ContentModel { get; set; }

        // Certaines réponses ont le wikitext directement dans la propriété "*" :
        [JsonPropertyName("*")]
        public string Content { get; set; }

        // D'autres réponses utilisent la structure "slots": { "main": { "*" : "..." } }
        [JsonPropertyName("slots")]
        public Slots Slots { get; set; }

        /// <summary>
        /// Retourne le texte effectif de la révision (gère "*" ou slots.main.*).
        /// </summary>
        [JsonIgnore]
        public string EffectiveContent
        {
            get
            {
                if (!string.IsNullOrEmpty(Content))
                    return Content;
                return Slots?.Main?.Content;
            }
        }
    }

    public class Slots
    {
        [JsonPropertyName("main")]
        public Slot Main { get; set; }
    }

    public class Slot
    {
        [JsonPropertyName("contentmodel")]
        public string ContentModel { get; set; }

        [JsonPropertyName("contentformat")]
        public string ContentFormat { get; set; }

        [JsonPropertyName("*")]
        public string Content { get; set; }
    }
}
