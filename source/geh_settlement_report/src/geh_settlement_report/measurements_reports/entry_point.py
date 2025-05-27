from geh_common.telemetry.logging_configuration import configure_logging

from geh_settlement_report.measurements_reports.application.tasks.measurements_report_task import (
    start_measurements_report_with_deps,
)


def start_measurements_report() -> None:
    configure_logging(cloud_role_name="dbr-measurements-report", subsystem="settlement-report")
    start_measurements_report_with_deps()
