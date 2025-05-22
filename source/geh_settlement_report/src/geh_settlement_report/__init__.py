import logging as logger

logger.getLogger("azure.core.pipeline.policies.http_logging_policy").setLevel(logger.WARNING)
logger.getLogger("azure.monitor.opentelemetry.exporter.export").setLevel(logger.WARNING)
