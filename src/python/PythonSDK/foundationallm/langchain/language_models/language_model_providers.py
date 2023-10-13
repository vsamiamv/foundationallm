from enum import Enum

class LanguageModelProviders(str, Enum):
    """Enumerator of the Language Model providers."""

    MICROSOFT = "microsoft"
    OPENAI = "openai"