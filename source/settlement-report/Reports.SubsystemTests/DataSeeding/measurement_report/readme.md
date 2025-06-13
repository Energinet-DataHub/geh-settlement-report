# Populating databricks tables

This folder contains two `.sql` scripts intended for populating Databricks development environments with test data.

The procedure is the following:

1. Navigate to the Databricks web UI and select the appropriate development environment
1. Open the **SQL editor** from the left-hand navigation menu.
1. Copy the content of one of the provided SQL scripts and replace the placeholder `INSERT_CATALOG_NAME_HERE` with the actual catalog name for the selected environment.
1. Select the `Settlement Report Deployment Warehouse` as the compute resource.
1. Run the script.
1. Repeat the process for the second SQL script.
