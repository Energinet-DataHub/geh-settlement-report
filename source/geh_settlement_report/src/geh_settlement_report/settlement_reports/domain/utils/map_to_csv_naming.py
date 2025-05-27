from geh_settlement_report.settlement_reports.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
    ChargeTypeDataProductValue,
    MeteringPointResolutionDataProductValue,
    MeteringPointTypeDataProductValue,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.data_values.settlement_method import (
    SettlementMethodDataProductValue,
)

METERING_POINT_TYPES = {
    MeteringPointTypeDataProductValue.VE_PRODUCTION.value: "D01",
    MeteringPointTypeDataProductValue.NET_PRODUCTION.value: "D05",
    MeteringPointTypeDataProductValue.SUPPLY_TO_GRID.value: "D06",
    MeteringPointTypeDataProductValue.CONSUMPTION_FROM_GRID.value: "D07",
    MeteringPointTypeDataProductValue.WHOLESALE_SERVICES_INFORMATION.value: "D08",
    MeteringPointTypeDataProductValue.OWN_PRODUCTION.value: "D09",
    MeteringPointTypeDataProductValue.NET_FROM_GRID.value: "D10",
    MeteringPointTypeDataProductValue.NET_TO_GRID.value: "D11",
    MeteringPointTypeDataProductValue.TOTAL_CONSUMPTION.value: "D12",
    MeteringPointTypeDataProductValue.ELECTRICAL_HEATING.value: "D14",
    MeteringPointTypeDataProductValue.NET_CONSUMPTION.value: "D15",
    MeteringPointTypeDataProductValue.CAPACITY_SETTLEMENT.value: "D19",
    MeteringPointTypeDataProductValue.CONSUMPTION.value: "E17",
    MeteringPointTypeDataProductValue.PRODUCTION.value: "E18",
    MeteringPointTypeDataProductValue.EXCHANGE.value: "E20",
}

SETTLEMENT_METHODS = {
    SettlementMethodDataProductValue.NON_PROFILED.value: "E02",
    SettlementMethodDataProductValue.FLEX.value: "D01",
}

CALCULATION_TYPES_TO_ENERGY_BUSINESS_PROCESS = {
    CalculationTypeDataProductValue.BALANCE_FIXING.value: "D04",
    CalculationTypeDataProductValue.WHOLESALE_FIXING.value: "D05",
    CalculationTypeDataProductValue.FIRST_CORRECTION_SETTLEMENT.value: "D32",
    CalculationTypeDataProductValue.SECOND_CORRECTION_SETTLEMENT.value: "D32",
    CalculationTypeDataProductValue.THIRD_CORRECTION_SETTLEMENT.value: "D32",
}

CALCULATION_TYPES_TO_PROCESS_VARIANT = {
    CalculationTypeDataProductValue.FIRST_CORRECTION_SETTLEMENT.value: "1ST",
    CalculationTypeDataProductValue.SECOND_CORRECTION_SETTLEMENT.value: "2ND",
    CalculationTypeDataProductValue.THIRD_CORRECTION_SETTLEMENT.value: "3RD",
}


RESOLUTION_NAMES = {
    MeteringPointResolutionDataProductValue.HOUR.value: "TSSD60",
    MeteringPointResolutionDataProductValue.QUARTER.value: "TSSD15",
}

CHARGE_TYPES = {
    ChargeTypeDataProductValue.SUBSCRIPTION.value: "D01",
    ChargeTypeDataProductValue.FEE.value: "D02",
    ChargeTypeDataProductValue.TARIFF.value: "D03",
}

TAX_INDICATORS = {
    True: 1,
    False: 0,
}
