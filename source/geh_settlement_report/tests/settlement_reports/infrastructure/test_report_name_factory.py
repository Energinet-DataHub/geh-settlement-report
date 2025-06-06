import json
import sys
import uuid
from datetime import datetime, timezone

import pytest
from pyspark.sql import SparkSession

from geh_settlement_report.settlement_reports.application.job_args.calculation_type import CalculationType
from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.domain.utils.csv_column_names import EphemeralColumns
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from geh_settlement_report.settlement_reports.infrastructure.report_name_factory import (
    FileNameFactory,
    ReportDataType,
)


@pytest.fixture(scope="function")
def default_settlement_report_args(monkeypatch: pytest.MonkeyPatch) -> SettlementReportArgs:
    """
    Note: Some tests depend on the values of `period_start` and `period_end`
    """
    grid_area_uuid = {"016": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373").hex}
    args = [
        f"--report-id={str(uuid.uuid4())}",
        "--requesting-actor-id=4123456789012",
        f"--period-start={datetime(2024, 6, 30, 22, 0, 0, tzinfo=timezone.utc)}",
        f"--period-end={datetime(2024, 7, 31, 22, 0, 0, tzinfo=timezone.utc)}",
        f"--calculation-type={CalculationType.WHOLESALE_FIXING.value}",
        f"--calculation-id-by-grid-area={json.dumps(grid_area_uuid)}",
        "--split-report-by-grid-area",
        "--energy-supplier-ids=[1234567890123]",
        f"--requesting-actor-market-role={MarketRole.DATAHUB_ADMINISTRATOR.value}",
        "--settlement-reports-output-path=some_output_volume_path",
        "--include-basis-data",
    ]
    monkeypatch.setenv("TIME_ZONE", "Europe/Copenhagen")
    monkeypatch.setenv("CATALOG_NAME", "spark_catalog")

    monkeypatch.setattr(sys, "argv", ["program"] + args)

    return SettlementReportArgs()


@pytest.mark.parametrize(
    "report_data_type,expected_pre_fix",
    [
        (ReportDataType.TimeSeriesHourly, "TSSD60"),
        (ReportDataType.TimeSeriesQuarterly, "TSSD15"),
        (ReportDataType.ChargeLinks, "CHARGELINK"),
        (ReportDataType.EnergyResults, "RESULTENERGY"),
        (ReportDataType.WholesaleResults, "RESULTWHOLESALE"),
        (ReportDataType.MonthlyAmounts, "RESULTMONTHLY"),
    ],
)
def test_create__when_energy_supplier__returns_expected_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    report_data_type: ReportDataType,
    expected_pre_fix: str,
):
    # Arrange
    args = default_settlement_report_args
    energy_supplier_id = "1234567890123"
    grid_area_code = "123"
    args.requesting_actor_id = energy_supplier_id
    args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER
    args.energy_supplier_ids = [energy_supplier_id]
    sut = FileNameFactory(report_data_type, args)
    partitions = {EphemeralColumns.grid_area_code_partitioning: grid_area_code}

    # Act
    actual = sut.create("", partitions)

    # Assert
    assert actual == f"{expected_pre_fix}_{grid_area_code}_{energy_supplier_id}_DDQ_01-07-2024_31-07-2024.csv"


def test_create__when_grid_access_provider__returns_expected_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    args = default_settlement_report_args
    grid_area_code = "123"
    requesting_actor_id = "1111111111111"
    args.requesting_actor_market_role = MarketRole.GRID_ACCESS_PROVIDER
    args.requesting_actor_id = requesting_actor_id
    args.energy_supplier_ids = None
    sut = FileNameFactory(ReportDataType.TimeSeriesHourly, args)
    partitions = {EphemeralColumns.grid_area_code_partitioning: grid_area_code}

    # Act
    actual = sut.create("", partitions)

    # Assert
    assert actual == f"TSSD60_{grid_area_code}_{requesting_actor_id}_DDM_01-07-2024_31-07-2024.csv"


@pytest.mark.parametrize(
    "market_role, energy_supplier_id, expected_file_name",
    [
        (MarketRole.SYSTEM_OPERATOR, None, "TSSD60_123_01-07-2024_31-07-2024.csv"),
        (
            MarketRole.DATAHUB_ADMINISTRATOR,
            None,
            "TSSD60_123_01-07-2024_31-07-2024.csv",
        ),
        (
            MarketRole.SYSTEM_OPERATOR,
            "1987654321123",
            "TSSD60_123_1987654321123_01-07-2024_31-07-2024.csv",
        ),
        (
            MarketRole.DATAHUB_ADMINISTRATOR,
            "1987654321123",
            "TSSD60_123_1987654321123_01-07-2024_31-07-2024.csv",
        ),
    ],
)
def test_create__when_system_operator_or_datahub_admin__returns_expected_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    market_role: MarketRole,
    energy_supplier_id: str,
    expected_file_name: str,
):
    # Arrange
    args = default_settlement_report_args
    args.requesting_actor_market_role = market_role
    args.energy_supplier_ids = [energy_supplier_id] if energy_supplier_id else None
    grid_area_code = "123"
    sut = FileNameFactory(ReportDataType.TimeSeriesHourly, args)
    partitions = {EphemeralColumns.grid_area_code_partitioning: grid_area_code}

    # Act
    actual = sut.create("", partitions)

    # Assert
    assert actual == expected_file_name


def test_create__when_split_index_is_set__returns_file_name_that_include_split_index(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    args = default_settlement_report_args
    energy_supplier_id = "222222222222"
    args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER
    args.requesting_actor_id = energy_supplier_id
    args.energy_supplier_ids = [energy_supplier_id]
    sut = FileNameFactory(ReportDataType.TimeSeriesHourly, args)
    partitions = {EphemeralColumns.grid_area_code_partitioning: "123", EphemeralColumns.chunk_index: "17"}

    # Act
    actual = sut.create("", partitions)

    # Assert
    assert actual == f"TSSD60_123_{energy_supplier_id}_DDQ_01-07-2024_31-07-2024_17.csv"


@pytest.mark.parametrize(
    "period_start,period_end,expected_start_date,expected_end_date",
    [
        (
            datetime(2024, 2, 29, 23, 0, 0, tzinfo=timezone.utc),
            datetime(2024, 3, 31, 22, 0, 0, tzinfo=timezone.utc),
            "01-03-2024",
            "31-03-2024",
        ),
        (
            datetime(2024, 9, 30, 22, 0, 0, tzinfo=timezone.utc),
            datetime(2024, 10, 31, 23, 0, 0, tzinfo=timezone.utc),
            "01-10-2024",
            "31-10-2024",
        ),
    ],
)
def test_create__when_daylight_saving_time__returns_expected_dates_in_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    period_start: datetime,
    period_end: datetime,
    expected_start_date: str,
    expected_end_date: str,
):
    # Arrange
    args = default_settlement_report_args
    args.period_start = period_start
    args.period_end = period_end
    args.energy_supplier_ids = None
    sut = FileNameFactory(ReportDataType.TimeSeriesHourly, args)
    partitions = {EphemeralColumns.grid_area_code_partitioning: "123", EphemeralColumns.chunk_index: "17"}

    # Act
    actual = sut.create("", partitions)

    # Assert
    assert actual == f"TSSD60_123_{expected_start_date}_{expected_end_date}_17.csv"


@pytest.mark.parametrize(
    "report_data_type, pre_fix",
    [
        pytest.param(
            ReportDataType.EnergyResults,
            "RESULTENERGY",
            id="returns correct energy file name",
        ),
        pytest.param(
            ReportDataType.WholesaleResults,
            "RESULTWHOLESALE",
            id="returns correct wholesale file name",
        ),
        pytest.param(
            ReportDataType.MonthlyAmounts,
            "RESULTMONTHLY",
            id="returns correct monthly amounts file name",
        ),
    ],
)
def test_create__when_energy_supplier_requests_report_not_combined(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    report_data_type: ReportDataType,
    pre_fix: str,
):
    # Arrange
    args = default_settlement_report_args
    args.split_report_by_grid_area = True
    args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER
    args.energy_supplier_ids = [args.requesting_actor_id]

    factory = FileNameFactory(report_data_type, args)
    partitions = {
        EphemeralColumns.grid_area_code_partitioning: "123",
    }

    # Act
    actual = factory.create("", partitions)

    # Assert
    assert actual == f"{pre_fix}_123_{args.requesting_actor_id}_DDQ_01-07-2024_31-07-2024.csv"


@pytest.mark.parametrize(
    "report_data_type, pre_fix",
    [
        pytest.param(
            ReportDataType.EnergyResults,
            "RESULTENERGY",
            id="returns correct energy file name",
        ),
        pytest.param(
            ReportDataType.WholesaleResults,
            "RESULTWHOLESALE",
            id="returns correct wholesale file name",
        ),
        pytest.param(
            ReportDataType.MonthlyAmounts,
            "RESULTMONTHLY",
            id="returns correct monthly amounts file name",
        ),
    ],
)
def test_create__when_energy_supplier_requests_report_combined(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    report_data_type: ReportDataType,
    pre_fix: str,
):
    # Arrange
    args = default_settlement_report_args
    args.calculation_id_by_grid_area = {
        "123": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }

    args.split_report_by_grid_area = False
    args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER
    args.energy_supplier_ids = [args.requesting_actor_id]
    factory = FileNameFactory(report_data_type, args)
    partitions = {}

    # Act
    actual = factory.create("", partitions)

    # Assert
    assert actual == f"{pre_fix}_flere-net_{args.requesting_actor_id}_DDQ_01-07-2024_31-07-2024.csv"


@pytest.mark.parametrize(
    "report_data_type, pre_fix",
    [
        pytest.param(
            ReportDataType.EnergyResults,
            "RESULTENERGY",
            id="returns correct energy file name",
        ),
        pytest.param(
            ReportDataType.WholesaleResults,
            "RESULTWHOLESALE",
            id="returns correct wholesale file name",
        ),
        pytest.param(
            ReportDataType.MonthlyAmounts,
            "RESULTMONTHLY",
            id="returns correct monthly amounts file name",
        ),
    ],
)
def test_create__when_grid_access_provider_requests_report(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    report_data_type: ReportDataType,
    pre_fix: str,
):
    # Arrange
    args = default_settlement_report_args
    args.requesting_actor_market_role = MarketRole.GRID_ACCESS_PROVIDER
    args.calculation_id_by_grid_area = {
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }
    args.energy_supplier_ids = [args.requesting_actor_id]
    factory = FileNameFactory(report_data_type, args)
    partitions = {
        EphemeralColumns.grid_area_code_partitioning: "456",
    }

    # Act
    actual = factory.create("", partitions)

    # Assert
    assert actual == f"{pre_fix}_456_{args.requesting_actor_id}_DDM_01-07-2024_31-07-2024.csv"


@pytest.mark.parametrize(
    "report_data_type, pre_fix",
    [
        pytest.param(
            ReportDataType.EnergyResults,
            "RESULTENERGY",
            id="returns correct energy file name",
        ),
        pytest.param(
            ReportDataType.WholesaleResults,
            "RESULTWHOLESALE",
            id="returns correct wholesale file name",
        ),
        pytest.param(
            ReportDataType.MonthlyAmounts,
            "RESULTMONTHLY",
            id="returns correct monthly amounts file name",
        ),
    ],
)
def test_create__when_datahub_administrator_requests_report_single_grid(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    report_data_type: ReportDataType,
    pre_fix: str,
):
    # Arrange
    args = default_settlement_report_args
    args.requesting_actor_market_role = MarketRole.DATAHUB_ADMINISTRATOR
    args.energy_supplier_ids = None
    args.calculation_id_by_grid_area = {
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }

    factory = FileNameFactory(report_data_type, args)
    partitions = {
        EphemeralColumns.grid_area_code_partitioning: "456",
    }

    # Act
    actual = factory.create("", partitions)

    # Assert
    assert actual == f"{pre_fix}_456_01-07-2024_31-07-2024.csv"


@pytest.mark.parametrize(
    "report_data_type, pre_fix",
    [
        pytest.param(
            ReportDataType.EnergyResults,
            "RESULTENERGY",
            id="returns correct energy file name",
        ),
        pytest.param(
            ReportDataType.WholesaleResults,
            "RESULTWHOLESALE",
            id="returns correct wholesale file name",
        ),
        pytest.param(
            ReportDataType.MonthlyAmounts,
            "RESULTMONTHLY",
            id="returns correct monthly amounts file name",
        ),
    ],
)
def test_create__when_datahub_administrator_requests_report_multi_grid_not_combined(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    report_data_type: ReportDataType,
    pre_fix: str,
):
    # Arrange
    args = default_settlement_report_args
    args.calculation_id_by_grid_area = {
        "123": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }
    args.split_report_by_grid_area = True
    args.requesting_actor_market_role = MarketRole.DATAHUB_ADMINISTRATOR
    args.energy_supplier_ids = None
    factory = FileNameFactory(report_data_type, args)
    partitions = {
        EphemeralColumns.grid_area_code_partitioning: "456",
    }

    # Act
    actual = factory.create("", partitions)

    # Assert
    assert actual == f"{pre_fix}_456_01-07-2024_31-07-2024.csv"


@pytest.mark.parametrize(
    "report_data_type, pre_fix",
    [
        pytest.param(
            ReportDataType.EnergyResults,
            "RESULTENERGY",
            id="returns correct energy file name",
        ),
        pytest.param(
            ReportDataType.WholesaleResults,
            "RESULTWHOLESALE",
            id="returns correct wholesale file name",
        ),
        pytest.param(
            ReportDataType.MonthlyAmounts,
            "RESULTMONTHLY",
            id="returns correct monthly amounts file name",
        ),
    ],
)
def test_create__when_datahub_administrator_requests_report_multi_grid_single_provider_combined(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    report_data_type: ReportDataType,
    pre_fix: str,
):
    # Arrange
    args = default_settlement_report_args
    energy_supplier_id = "1234567890123"
    args.calculation_id_by_grid_area = {
        "123": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }
    args.split_report_by_grid_area = False
    args.requesting_actor_market_role = MarketRole.DATAHUB_ADMINISTRATOR
    args.energy_supplier_ids = [energy_supplier_id]

    factory = FileNameFactory(report_data_type, args)
    partitions = {}

    # Act
    actual = factory.create("", partitions)

    # Assert
    assert actual == f"{pre_fix}_flere-net_{energy_supplier_id}_01-07-2024_31-07-2024.csv"


@pytest.mark.parametrize(
    "report_data_type, pre_fix",
    [
        pytest.param(
            ReportDataType.EnergyResults,
            "RESULTENERGY",
            id="returns correct energy file name",
        ),
        pytest.param(
            ReportDataType.WholesaleResults,
            "RESULTWHOLESALE",
            id="returns correct wholesale file name",
        ),
        pytest.param(
            ReportDataType.MonthlyAmounts,
            "RESULTMONTHLY",
            id="returns correct monthly amounts file name",
        ),
    ],
)
def test_create__when_datahub_administrator_requests_result_report_multi_grid_all_providers_combined(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    report_data_type: ReportDataType,
    pre_fix: str,
):
    # Arrange
    args = default_settlement_report_args
    args.calculation_id_by_grid_area = {
        "123": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }
    args.split_report_by_grid_area = False
    args.requesting_actor_market_role = MarketRole.DATAHUB_ADMINISTRATOR
    args.energy_supplier_ids = None
    factory = FileNameFactory(report_data_type, args)
    partitions = {}

    # Act
    actual = factory.create("", partitions)

    # Assert
    assert actual == f"{pre_fix}_flere-net_01-07-2024_31-07-2024.csv"
