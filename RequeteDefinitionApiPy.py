import requests
from dataclasses import dataclass, field
from typing import Optional, List, Dict, Any


API_URL = "https://fr.wiktionary.org/w/api.php"


@dataclass
class MainSlot:
    content: str = ""

    @classmethod
    def from_dict(cls, data: Optional[Dict[str, Any]]) -> Optional["MainSlot"]:
        if not data:
            return None
        return cls(content=data.get("*", ""))


@dataclass
class Revision:
    raw_content: str = ""
    slots: Optional[MainSlot] = None

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> "Revision":
        slots_data = data.get("slots", {}).get("main", {})
        slots = MainSlot.from_dict(slots_data)
        raw_content = data.get("*", "")
        return cls(raw_content=raw_content, slots=slots)


@dataclass
class Page:
    pageid: Optional[int] = None
    title: Optional[str] = None
    missing: bool = False
    revisions: List[Revision] = field(default_factory=list)

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> "Page":
        revisions = data.get("revisions", [])
        return cls(
            pageid=data.get("pageid"),
            title=data.get("title"),
            missing=data.get("missing", False),
            revisions=[Revision.from_dict(r) for r in revisions],
        )


@dataclass
class Query:
    pages: List[Page] = field(default_factory=list)

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> "Query":
        pages_data = data.get("pages", [])
        return cls(pages=[Page.from_dict(p) for p in pages_data])


@dataclass
class MediaWikiResponse:
    query: Optional[Query] = None

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> "MediaWikiResponse":
        query_data = data.get("query", {})
        return cls(query=Query.from_dict(query_data))


def get_wiktionary_definition(word: str) -> str:
    params = {
        "action": "query",
        "prop": "revisions",
        "rvprop": "content",
        "rvslots": "main",
        "format": "json",
        "titles": word,
    }

    response = requests.get(API_URL, params=params, timeout=20)
    response.raise_for_status()

    payload = response.json()
    model = MediaWikiResponse.from_dict(payload)

    if model.query is None or not model.query.pages:
        raise ValueError(f"Aucune page trouvée pour le mot : {word}")

    page = model.query.pages[0]

    if page.missing:
        raise ValueError(f"Le mot n'existe pas dans le Wiktionary : {word}")

    if not page.revisions:
        raise ValueError(f"Aucune révision trouvée pour le mot : {word}")

    revision = page.revisions[0]

    # Prendre le contenu principal de la révision
    content = revision.slots.content if revision.slots else revision.raw_content

    if not content:
        raise ValueError(f"Le contenu est vide pour le mot : {word}")

    return content


# Exemple d'utilisation
if __name__ == "__main__":
    try:
        text = get_wiktionary_definition("chat")
        print(text[:2000])  # Affiche seulement le début
    except Exception as e:
        print("Erreur :", e)
