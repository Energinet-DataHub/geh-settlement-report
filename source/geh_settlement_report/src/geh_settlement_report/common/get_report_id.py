import sys


def get_report_id_from_args(args: list[str] = sys.argv) -> str:
    """Check if --report-id is part of sys.argv and returns its value.

    Returns:
        str: The value of --report-id

    Raises:
        ValueError: If --report-id is not found in sys.argv
    """
    for i, arg in enumerate(args):
        if arg.startswith("--report-id"):
            if "=" in arg:
                return arg.split("=")[1]
            else:
                if i + 1 <= len(args):
                    return args[i + 1]
    raise ValueError(f"'--report-id' was not found in arguments. Existing arguments: {','.join(sys.argv)}")
