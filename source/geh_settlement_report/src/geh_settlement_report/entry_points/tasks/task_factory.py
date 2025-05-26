from geh_common.databricks.get_dbutils import get_dbutils
from pyspark.sql import SparkSession

from geh_settlement_report.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.entry_points.tasks.charge_link_periods_task import (
    ChargeLinkPeriodsTask,
)
from geh_settlement_report.entry_points.tasks.charge_price_points_task import (
    ChargePricePointsTask,
)
from geh_settlement_report.entry_points.tasks.energy_resuls_task import (
    EnergyResultsTask,
)
from geh_settlement_report.entry_points.tasks.metering_point_periods_task import (
    MeteringPointPeriodsTask,
)
from geh_settlement_report.entry_points.tasks.monthly_amounts_task import (
    MonthlyAmountsTask,
)
from geh_settlement_report.entry_points.tasks.task_base import (
    TaskBase,
)
from geh_settlement_report.entry_points.tasks.task_type import TaskType
from geh_settlement_report.entry_points.tasks.time_series_points_task import (
    TimeSeriesPointsTask,
)
from geh_settlement_report.entry_points.tasks.wholesale_results_task import (
    WholesaleResultsTask,
)
from geh_settlement_report.entry_points.tasks.zip_task import ZipTask


def create(
    task_type: TaskType,
    spark: SparkSession,
    args: SettlementReportArgs,
) -> TaskBase:
    dbutils = get_dbutils(spark)
    if task_type is TaskType.MeteringPointPeriods:
        return MeteringPointPeriodsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.TimeSeriesQuarterly:
        return TimeSeriesPointsTask(
            spark=spark,
            dbutils=dbutils,
            args=args,
            task_type=TaskType.TimeSeriesQuarterly,
        )
    elif task_type is TaskType.TimeSeriesHourly:
        return TimeSeriesPointsTask(spark=spark, dbutils=dbutils, args=args, task_type=TaskType.TimeSeriesHourly)
    elif task_type is TaskType.ChargeLinks:
        return ChargeLinkPeriodsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.ChargePricePoints:
        return ChargePricePointsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.EnergyResults:
        return EnergyResultsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.WholesaleResults:
        return WholesaleResultsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.MonthlyAmounts:
        return MonthlyAmountsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.Zip:
        return ZipTask(spark=spark, dbutils=dbutils, args=args)
    else:
        raise ValueError(f"Unknown task type: {task_type}")
