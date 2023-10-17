import json
from langchain.base_language import BaseLanguageModel
from langchain.chat_models import AzureChatOpenAI

from foundationallm.config import Configuration
from foundationallm.langchain.language_models.openai import OpenAIModelBase, AzureOpenAIAPIType
import foundationallm.models.metadata.language_model as LanguageModel

class AzureChatModel(OpenAIModelBase):
    """Azure OpenAI chat model."""
    config_value_base_name: str = 'FoundationaLLM:AzureOpenAI:API'
    deployment_name: str
    model_version: str
    
    def __init__(self, language_model: LanguageModel, config: Configuration):
        """
        Initializes the Azure Open AI chat model.

        Parameters
        ----------
        language_model: LanguageModel
            The language model metadata class.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.config = config
        self.config_value = json.loads(self.config.get_value(self.config_value_base_name))

        self.openai_api_base = self.config_value["Endpoint"]
        self.openai_api_version = self.config_value["Version"]
        self.deployment_name = self.config_value["Completions"]["DeploymentName"]
        self.max_tokens = self.config_value["Completions"]["MaxTokens"]
        self.model_version = self.config_value["Completions"]["ModelVersion"]
        self.temperature = self.config_value["Completions"]["Temperature"]

        self.openai_api_type = AzureOpenAIAPIType.AZURE
        self.openai_api_key = self.config.get_value(f'{self.config_value_base_name}:Key')
         
    def get_language_model(self) -> BaseLanguageModel:
        """
        Returns the Azure OpenAI chat model.

        Returns
        -------
        BaseLanguageModel
            Returns an AzureChatOpenAI chat model.
        """
        return AzureChatOpenAI(
            temperature = self.temperature,
            openai_api_base = self.openai_api_base,
            openai_api_key = self.openai_api_key,
            openai_api_type = self.openai_api_type,
            openai_api_version = self.openai_api_version,
            deployment_name = self.deployment_name,
            #max_tokens = self.max_tokens,
            model_version = self.model_version
        )