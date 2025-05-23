from pyspark.sql import SparkSession


def assert_file_names_and_columns(
    path: str,
    actual_files: list[str],
    expected_columns: list[str],
    expected_file_names: list[str],
    spark: SparkSession,
):
    assert sorted(set(actual_files)) == sorted(set(expected_file_names)), (
        f"File names do not match:\nActual: {actual_files}\nExpected: {expected_file_names}"
    )
    for file_name in actual_files:
        df = spark.read.csv(f"{path}/{file_name}", header=True)
        nrows = df.count()
        assert nrows > 0, f"File {file_name} is empty"
        assert df.columns == expected_columns, (
            f"File {file_name} has unexpected columns:\nActual: {df.columns}\nExpected: {expected_columns}"
        )
