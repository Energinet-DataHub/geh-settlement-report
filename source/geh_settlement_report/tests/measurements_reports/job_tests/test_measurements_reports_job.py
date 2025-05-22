import pytest

from geh_settlement_report.entry_points.entry_point import start_zip
from tests.integration_test_configuration import IntegrationTestConfiguration


def test_zip_job(
    integration_test_configuration: IntegrationTestConfiguration,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    # Arrange
    applicationinsights_connection_string = integration_test_configuration.get_applicationinsights_connection_string()
    monkeypatch.setenv("APPLICATIONINSIGHTS_CONNECTION_STRING", applicationinsights_connection_string)

    # Act
    start_zip()

    # Aseert
    # check if a file appeared
