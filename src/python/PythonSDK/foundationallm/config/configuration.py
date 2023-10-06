import os
from azure.keyvault.secrets import SecretClient
from azure.identity import DefaultAzureCredential
from tenacity import (retry, wait_random_exponential,
                      stop_after_attempt, RetryError)
import logging


class Configuration():
    keyvault_name: str = None
    __secret_client: SecretClient = None

    def __init__(self, keyvault_name: str):
        self.keyvault_name = keyvault_name

    def get_value(self, name: str, default: str = None) -> str:
        """
        Retrieves the value from a variable, and if not found attempts to get the default
        config value.

        Parameters
        ----------
        - name : str
            The name of the env variable to retrieve.
        - default : str
            Default value if variable not found.
        Returns
        -------
        The value of the environment variable if it exists, or the Key Vault
        value for the variable.
        """

        try:
            value = self.__get_value(name)
            return value

        except Exception as e:
            print(e)
            pass

        value = os.environ.get(name)

        if value is not None:
            return value

        # If name not found as an env variable
        else:
            if default:
                return default
            else:
                return self.default_config_value(name)

    def __retry_before_sleep(retry_state):
        # Log the outcome of each retry attempt.
        message = f"""Retrying {retry_state.fn}:
                        attempt {retry_state.attempt_number}
                        ended with: {retry_state.outcome}"""
        if retry_state.outcome.failed:
            ex = retry_state.outcome.exception()
            message += f"; Exception: {ex.__class__.__name__}: {ex}"
        if retry_state.attempt_number < 1:
            logging.info(message)
        else:
            logging.warning(message)

    # Retry with jitter on transient errors. Initially up to 2^x * 1 seconds between each retry until
    # the range reaches 30 seconds, then randomly up to 60 seconds afterwards. Ultimately, stop after 5 attempts.
    @retry(
            wait=wait_random_exponential(multiplier=1, max=5),
            stop=stop_after_attempt(5),
            before_sleep=__retry_before_sleep
        )
    def __get_secret_with_retry(self, name):
        try:
            return self.__secret_client.get_secret(name)
        except RetryError:
            pass

    def __get_value(self, name: str) -> str:
        vault_url = f"https://{self.keyvault_name}.vault.azure.net"

        if self.__secret_client is None:
            credential = DefaultAzureCredential()
            self.__secret_client = SecretClient(
                                        vault_url=vault_url,
                                        credential=credential
                                    )

        val = self.__get_secret_with_retry(name=name)

        return val.value
