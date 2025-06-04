--TODO HENRIK: Fill with correct information. 

INSERT INTO {catalog_name}.{database}.{table} (
    metering_point_id,
    metering_point_type,
    orchestration_type,
    orchestration_instance_id,
    observation_time,
    quantity,
    quality,
    unit, 
    resolution,
    transaction_id,
    transaction_creation_datetime,
    is_cancelled,
    created,
    modified
)
VALUES
(
    '{row.metering_point_id}',
    '{row.metering_point_type.value}',
    '{row.orchestration_type.value}',
    '{str(row.orchestration_instance_id)}',
    '{row.observation_time.strftime("%Y-%m-%d %H:%M:%S")}',
    '{format(row.quantity, ".3f")}',
    '{row.quality.value}',
    '{QuantityUnit.KWH.value}',
    '{MeteringPointResolution.HOUR.value}',
    '{str(row.transaction_id)}',
    GETDATE(),
    false,
    GETDATE(),
    GETDATE()
)
