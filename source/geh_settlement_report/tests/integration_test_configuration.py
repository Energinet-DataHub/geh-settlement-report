from azure.identity import DefaultAzureCredential
from azure.keyvault.secrets import SecretClient


class IntegrationTestConfiguration:
    def __init__(self, azure_keyvault_url: str):
        self._credential = DefaultAzureCredential()
        self._azure_keyvault_url = azure_keyvault_url

        # noinspection PyTypeChecker
        # From https://youtrack.jetbrains.com/issue/PY-59279/Type-checking-detects-an-error-when-passing-an-instance-implicitly-conforming-to-a-Protocol-to-a-function-expecting-that:
        #    DefaultAzureCredential does not conform to protocol TokenCredential, because its method get_token is missing
        #    the arguments claims and tenant_id. Surely, they might appear among the arguments passed as **kwargs, but it's
        #    not guaranteed. In other words, you can make a call to get_token which will typecheck fine for
        #    DefaultAzureCredential, but not for TokenCredential.
        self._secret_client = SecretClient(
            vault_url=self._azure_keyvault_url,
            credential=self._credential,
        )

    @property
    def credential(self) -> DefaultAzureCredential:
        return self._credential

    def get_analytics_workspace_id(self) -> str:
        return self._get_secret_value("AZURE-LOGANALYTICS-WORKSPACE-ID")

    def get_applicationinsights_connection_string(self) -> str:
        # This is the name of the secret in Azure Key Vault in the integration test environment
        return self._get_secret_value("AZURE-APPINSIGHTS-CONNECTIONSTRING")

    def _get_secret_value(self, secret_name: str) -> str:
        secret = self._secret_client.get_secret(secret_name)
        return secret.value
